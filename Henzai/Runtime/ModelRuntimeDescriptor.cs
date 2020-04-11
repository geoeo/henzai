using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using Veldrid;
using Henzai.Core;
using Henzai.Core.Materials;
using Henzai.Core.VertexGeometry;
using Henzai.Core.Reflection;
using Henzai.Runtime.EventHandlerKeys;
using Henzai.Effects;

namespace Henzai.Runtime
{
    //TODO: Move all the lists/arrays into a contiuous array and only store start and length
    //TODO: Refactor ShadowMap datastruts into more generic effects pipeline
    public sealed class ModelRuntimeDescriptor<T> where T : struct, VertexLocateable
    {
        public int Length { get; private set; }
        /// <summary>
        /// Used During Resource Creation
        /// </summary>
        public List<DeviceBuffer> VertexBufferList { get; private set; }
        /// <summary>
        /// Used During Resource Creation
        /// </summary>
        public List<DeviceBuffer> IndexBufferList { get; private set; }
        /// <summary>
        /// Used During Resource Creation
        /// </summary>
        public List<DeviceBuffer>[] InstanceBufferLists { get; private set; }
        /// <summary>
        /// Used During Resource Creation
        /// </summary>
        public List<ResourceSet> TextureResourceSetsList { get; private set; }
        /// <summary>
        /// Used During Rendering
        /// </summary>
        public DeviceBuffer[] VertexBuffers { get; private set; }
        /// <summary>
        /// Used During Rendering
        /// </summary>
        public DeviceBuffer[] IndexBuffers { get; private set; }
        /// <summary>
        /// Used During Rendering
        /// </summary>
        public DeviceBuffer[][] InstanceBuffers { get; private set; }
        /// <summary>
        /// Used During Rendering
        /// </summary>
        public ResourceSet[] TextureResourceSets { get; private set; }
        public ResourceLayout TextureResourceLayout { get; set; }
        public ResourceSet[][] EffectResourceSets { get; set; }
        public Sampler TextureSampler { get; set; }
        public Shader VertexShader { get; private set; }
        public VertexLayoutDescription[] VertexLayouts { get; private set; }
        public VertexLayoutDescription[] VertexPreEffectsLayouts { get; private set; }
        private string _vertexShaderName;
        public Shader FragmentShader { get; private set; }
        private string _fragmentShaderName;
        public Shader[] VertexPreEffectShaders { get; private set; }
        public Shader[] FragmentPreEffectShaders { get; private set; }
        /// <summary>
        /// Defines a Higher Level Render State.
        /// Buffers, Layouts, Shaders, Ratsterizer.
        /// See: <see cref="Veldrid.Pipeline"/>
        /// </summary>
        public Pipeline[] Pipelines { get; set; }
        /// <summary>
        //TODO: Remove this
        /// Contains Geometry and Material Properties
        /// See: <see cref="Henzai.Geometry.Model{T}"/>
        /// </summary>
        public Model<T, RealtimeMaterial> Model { get; set; }
        public RenderDescription RenderDescription { get; set; }
        public uint PreEffectsFlag { get; set; }
        public uint PreEffectsInstancingFlag { get; set; }
        public uint InstancingDataFlag { get; set; }
        public VertexRuntimeTypes VertexRuntimeType { get; private set; }
        public PrimitiveTopology PrimitiveTopology { get; private set; }
        public uint TotalInstanceCount { get; set; }
        public event Func<VertexLayoutDescription> CallVertexLayoutGeneration;
        private EventHandlerList VertexPreEffectsInstanceLayoutGenerationList;
        private EventHandlerList VertexInstanceLayoutGenerationList;
        public event Func<DisposeCollectorResourceFactory, Sampler> CallSamplerGeneration;
        public event Func<DisposeCollectorResourceFactory, ResourceLayout> CallTextureResourceLayoutGeneration;
        // TODO: @Performance: Investigate Texture Cache for already loaded textures
        public event Func<ModelRuntimeDescriptor<T>, int, DisposeCollectorResourceFactory, GraphicsDevice, ResourceSet> CallTextureResourceSetGeneration;

        public bool ShadowMapEnabled => (PreEffectsFlag & RenderFlags.SHADOW_MAP) == RenderFlags.SHADOW_MAP;
        public bool OmniShadowMapEnabled => (PreEffectsFlag & RenderFlags.OMNI_SHADOW_MAPS) == RenderFlags.OMNI_SHADOW_MAPS;

        //TODO: Refactor renderFlag preEffectsInstancingFlag and instancingFlag into an class/struct
        public ModelRuntimeDescriptor(Model<T, RealtimeMaterial> modelIn, string vShaderName, string fShaderName, VertexRuntimeTypes vertexRuntimeType, PrimitiveTopology primitiveTopology, RenderDescription renderDescription, InstancingRenderDescription instancingRenderDescription)
        {

            if (!Verifier.VerifyVertexStruct<T>(vertexRuntimeType))
                throw new ArgumentException($"Type Mismatch ModelRuntimeDescriptor");

            Model = modelIn;
            Length = modelIn.MeshCount;

            TotalInstanceCount = 1;

            _vertexShaderName = vShaderName;
            _fragmentShaderName = fShaderName;

            VertexRuntimeType = vertexRuntimeType;
            PrimitiveTopology = primitiveTopology;

            RenderDescription = renderDescription;
            PreEffectsFlag = RenderFlags.PRE_EFFECTS_MASK & renderDescription.RenderModeFlag;
            PreEffectsInstancingFlag = instancingRenderDescription.PreEffectsFlag;
            InstancingDataFlag = instancingRenderDescription.RenderModeFlag;

            var preEffectsInstancing = RenderFlags.GetAllPreEffectFor(PreEffectsInstancingFlag);

            VertexBufferList = new List<DeviceBuffer>();
            IndexBufferList = new List<DeviceBuffer>();
            Pipelines = new Pipeline[RenderFlags.EFFCTS_TOTAL_COUNT];
            InstanceBufferLists = new List<DeviceBuffer>[RenderFlags.EFFCTS_TOTAL_COUNT];
            InstanceBufferLists[RenderFlags.NORMAL_ARRAY_INDEX] = new List<DeviceBuffer>();
            foreach (var preEffect in preEffectsInstancing)
                InstanceBufferLists[RenderFlags.GetArrayIndexForFlag(preEffect)] = new List<DeviceBuffer>();
            InstanceBuffers = new DeviceBuffer[RenderFlags.EFFCTS_TOTAL_COUNT][];
            TextureResourceSetsList = new List<ResourceSet>();
            VertexInstanceLayoutGenerationList = new EventHandlerList();
            VertexPreEffectsInstanceLayoutGenerationList = new EventHandlerList();

            VertexPreEffectShaders = new Shader[RenderFlags.PRE_EFFCTS_TOTAL_COUNT];
            FragmentPreEffectShaders = new Shader[RenderFlags.PRE_EFFCTS_TOTAL_COUNT];

            // Reserve first spot for base vertex geometry
            //TODO: Make on list
            VertexLayouts = new VertexLayoutDescription[InstancingDataFlags.GetSizeOfPreEffectFlag(InstancingDataFlag) + 1];
            VertexPreEffectsLayouts = new VertexLayoutDescription[RenderFlags.GetSizeOfPreEffectFlag(PreEffectsInstancingFlag) + 1];

            EffectResourceSets = new ResourceSet[RenderFlags.EFFCTS_TOTAL_COUNT][];
            for (int i = 0; i < RenderFlags.EFFCTS_TOTAL_COUNT; i++)
                EffectResourceSets[i] = new ResourceSet[0];

        }

        /// <summary>
        /// Formats Lists to Arrays for the Commandlist generation.
        /// These items are used only during the renderloop
        /// </summary>
        public void FormatResourcesForRuntime()
        {
            VertexBuffers = VertexBufferList.ToArray();
            IndexBuffers = IndexBufferList.ToArray();
            TextureResourceSets = TextureResourceSetsList.ToArray();
            foreach (var flag in RenderFlags.ALL_RENDER_FLAGS)
            {
                var index = RenderFlags.GetArrayIndexForFlag(flag);
                var instanceBufferList = InstanceBufferLists[index];
                InstanceBuffers[index] = instanceBufferList == null ? new DeviceBuffer[0] : InstanceBufferLists[index].ToArray();
            }
        }

        public void LoadShaders(GraphicsDevice graphicsDevice)
        {
            VertexShader = IO.LoadShader(_vertexShaderName, ShaderStages.Vertex, graphicsDevice);
            FragmentShader = IO.LoadShader(_fragmentShaderName, ShaderStages.Fragment, graphicsDevice);

            var preEffects = RenderFlags.GetAllPreEffectFor(PreEffectsFlag);
            foreach (var flag in preEffects)
            {
                var arrayIndex = RenderFlags.GetPreEffectArrayIndexForFlag(flag);
                var name = ShaderNames.PreEffectShaderNames[arrayIndex];
                VertexPreEffectShaders[arrayIndex] = IO.LoadShader(name, ShaderStages.Vertex, graphicsDevice);
                FragmentPreEffectShaders[arrayIndex] = IO.LoadShader(name, ShaderStages.Fragment, graphicsDevice);
            }
        }

        //TODO: @Investigate: What if multiple delegates are bound to the same event?         
        public void InvokeVertexLayoutGeneration()
        {

            // Base vertex geometry
            VertexLayouts[0] = CallVertexLayoutGeneration.Invoke();
            VertexPreEffectsLayouts[0] = CallVertexLayoutGeneration.Invoke();

            // Instancing data i.e. position, view matrices 
            var instancingEventKeys = InstancingEventHandlerKeys.GetKeys();
            var flagIndex = InstancingDataFlags.GetArrayIndexForFlag(InstancingDataFlag);
            if (flagIndex >= 0)
            {
                var instancingEventKey = instancingEventKeys[flagIndex];
                var instancingVertexInstanceDeletegate = (VertexInstanceLayoutGenerationList[instancingEventKey] as Func<VertexLayoutDescription>);
                VertexLayouts[flagIndex + 1] = instancingVertexInstanceDeletegate.Invoke();
            }

            // Instancing effects data 
            var preEffectsEventKeys = PreEffectEventHandlerKeys.GetKeys();
            var preEffectsflagIndex = RenderFlags.GetPreEffectArrayIndexForFlag(PreEffectsInstancingFlag);
            if (preEffectsflagIndex >= 0)
            {
                var preEffectsEventKey = preEffectsEventKeys[preEffectsflagIndex];
                var vertexInstanceDeletegate = (VertexPreEffectsInstanceLayoutGenerationList[preEffectsEventKey] as Func<VertexLayoutDescription>);
                VertexPreEffectsLayouts[preEffectsflagIndex + 1] = vertexInstanceDeletegate.Invoke();
            }

        }

        public Sampler InvokeSamplerGeneration(DisposeCollectorResourceFactory factory)
        {
            return CallSamplerGeneration != null ? CallSamplerGeneration(factory) : null;
        }

        public ResourceLayout InvokeTextureResourceLayoutGeneration(DisposeCollectorResourceFactory factory)
        {
            return CallTextureResourceLayoutGeneration != null ? CallTextureResourceLayoutGeneration(factory) : null;
        }

        public ResourceSet InvokeTextureResourceSetGeneration(int meshIndex, DisposeCollectorResourceFactory factory, GraphicsDevice graphicsDevice)
        {
            return CallTextureResourceSetGeneration != null ? CallTextureResourceSetGeneration(this, meshIndex, factory, graphicsDevice) : null;
        }

        public void AddPreEffectsVertexInstanceDelegate(uint id, Func<VertexLayoutDescription> vertexLayoutDelegate)
        {
            var flagIndex = RenderFlags.GetPreEffectArrayIndexForFlag(PreEffectsInstancingFlag);
            VertexPreEffectsInstanceLayoutGenerationList.AddHandler(PreEffectEventHandlerKeys.GetKeys()[flagIndex], vertexLayoutDelegate);
        }

        public void AddVertexInstanceDelegate(uint instancingFlag, Func<VertexLayoutDescription> vertexLayoutDelegate)
        {
            var flagIndex = InstancingDataFlags.GetArrayIndexForFlag(PreEffectsInstancingFlag);
            VertexInstanceLayoutGenerationList.AddHandler(InstancingEventHandlerKeys.GetKeys()[flagIndex], vertexLayoutDelegate);
        }


        public ResourceLayout[] FillEffectsResourceSet(DisposeCollectorResourceFactory factory, SceneRuntimeDescriptor sceneRuntimeDescriptor, List<SubRenderable> childrenPre)
        {
            var effectLayoutList = new List<ResourceLayout>();

            //TODO: just iterate over children and process them as Subrenderable ?

            if (ShadowMapEnabled)
            {
                var effectCount = RenderFlags.GetSizeOfPreEffectFlag(PreEffectsFlag) * 2; // 1 for Vertex Stage 1 for Fragment
                EffectResourceSets[RenderFlags.NORMAL_ARRAY_INDEX] = new ResourceSet[effectCount];
                var shadowMapRenderable = childrenPre[0] as ShadowMap;

                var shadowMapResourceLayout = ResourceGenerator.GenerateTextureResourceLayoutForShadowMapping(factory);
                effectLayoutList.Add(sceneRuntimeDescriptor.LightProvViewResourceLayout);
                effectLayoutList.Add(shadowMapResourceLayout);
                EffectResourceSets[RenderFlags.NORMAL_ARRAY_INDEX][RenderFlags.GetPreEffectArrayIndexForFlag(RenderFlags.SHADOW_MAP)] = sceneRuntimeDescriptor.LightProjViewResourceSet;
                EffectResourceSets[RenderFlags.NORMAL_ARRAY_INDEX][RenderFlags.GetPreEffectArrayIndexForFlag(RenderFlags.SHADOW_MAP) + 1] = ResourceGenerator.GenerateResourceSetForShadowMapping(shadowMapResourceLayout, shadowMapRenderable.ShadowMapTexView, factory);
            }

            if (OmniShadowMapEnabled)
            {
                var effectCount = 2;
                EffectResourceSets[RenderFlags.NORMAL_ARRAY_INDEX] = new ResourceSet[effectCount];
                //var omniShadowMapRenderable =  childrenPre[RenderFlags.GetPreEffectArrayIndexForFlag(RenderFlags.OMNI_SHADOW_MAPS)] as OmniShadowMap;
                var omniShadowMapRenderable = childrenPre[0] as OmniShadowMap;
                var shadowMapResourceLayout = ResourceGenerator.GenerateTextureResourceLayoutForOmniShadowMapping(factory);
                //TODO: this has to be an array
                effectLayoutList.Add(sceneRuntimeDescriptor.CameraInfoResourceLayout);
                effectLayoutList.Add(shadowMapResourceLayout);
                EffectResourceSets[RenderFlags.NORMAL_ARRAY_INDEX][0] = sceneRuntimeDescriptor.CameraInfoResourceSet;
                EffectResourceSets[RenderFlags.NORMAL_ARRAY_INDEX][1] = ResourceGenerator.GenerateResourceSetForShadowMapping(shadowMapResourceLayout, omniShadowMapRenderable.ShadowMapTexView, factory);
            }


            return effectLayoutList.ToArray();
        }





    }
}