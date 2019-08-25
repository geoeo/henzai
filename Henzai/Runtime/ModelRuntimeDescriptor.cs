using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using Veldrid;
using Henzai.Effects;
using Henzai.Core;
using Henzai.Core.Materials;
using Henzai.Core.VertexGeometry;
using Henzai.Core.Reflection;

namespace Henzai.Runtime
{
    //TODO: Move all the lists/arrays into a contiuous array and only store start and length
    //TODO: Refactor ShadowMap datastruts into more generic effects pipeline
    public sealed class ModelRuntimeDescriptor<T> where T : struct, VertexLocateable
    {
        public int Length {get; private set;}
        /// <summary>
        /// Used During Resource Creation
        /// </summary>
        public List<DeviceBuffer> VertexBufferList {get; private set;}
        /// <summary>
        /// Used During Resource Creation
        /// </summary>
        public List<DeviceBuffer> IndexBufferList {get; private set;}
        /// <summary>
        /// Used During Resource Creation
        /// </summary>
        public List<DeviceBuffer> InstanceBufferList {get; private set;}
        public List<DeviceBuffer> InstanceShadowMapBufferList {get; private set;}
        /// <summary>
        /// Used During Resource Creation
        /// </summary>
        public List<ResourceSet> TextureResourceSetsList {get; private set;}
        /// <summary>
        /// Used During Rendering
        /// </summary>
        public DeviceBuffer[] VertexBuffers {get; private set;}
        /// <summary>
        /// Used During Rendering
        /// </summary>
        public DeviceBuffer[] IndexBuffers {get; private set;}
        /// <summary>
        /// Used During Rendering
        /// </summary>
        public DeviceBuffer[] InstanceBuffers {get; private set;}
        public DeviceBuffer[] InstanceShadowMapBuffers {get; private set;}
        /// <summary>
        /// Used During Rendering
        /// </summary>
        public ResourceSet[] TextureResourceSets {get; private set;}
        public ResourceLayout TextureResourceLayout {get; set;}
        public ResourceSet[] EffectResourceSets {get;set;}
        public Sampler TextureSampler {get;set;}
        public Shader VertexShader {get; private set;}
        public List<VertexLayoutDescription> VertexLayoutList {get;private set;}
        public List<VertexLayoutDescription> VertexPreEffectsLayoutList {get;private set;}
        public VertexLayoutDescription[] VertexLayouts {get;private set;}
        public VertexLayoutDescription[] VertexPreEffectsLayouts {get;private set;}
        private string _vertexShaderName;
        public Shader FragmentShader {get; private set;}
        private string _fragmentShaderName;
        public Shader VertexPreEffectsShader {get; private set;}
        public Shader FragmentShadowMapShader {get; private set;}
        /// <summary>
        /// Defines a Higher Level Render State.
        /// Buffers, Layouts, Shaders, Ratsterizer.
        /// See: <see cref="Veldrid.Pipeline"/>
        /// </summary>
        public Pipeline Pipeline {get; set;}
        public Pipeline PreEffectsPipeline {get; set;}
        /// <summary>
        //TODO: Remove this
        /// Contains Geometry and Material Properties
        /// See: <see cref="Henzai.Geometry.Model{T}"/>
        /// </summary>
        public Model<T,RealtimeMaterial> Model {get;set;}
        public uint RenderFlags {get;set;}
        public uint PreEffectsInstancingFlag {get;set;}
        public VertexRuntimeTypes VertexRuntimeType {get; private set;}
        public PrimitiveTopology PrimitiveTopology {get; private set;}
        public uint TotalInstanceCount{get;set;}
        public event Func<VertexLayoutDescription> CallVertexLayoutGeneration;
        //TODO: Use EventHandlerList to bind multiple events. Make events private and use public add methods
        private EventHandlerList VertexPreEffectsInstanceLayoutGenerationList;
        private EventHandlerList CallVertexInstanceLayoutGenerationList;
        //////////// Depreciated - to be removed
        public event Func<VertexLayoutDescription> CallVertexInstanceLayoutGeneration;
        ////////////// Depriciated END
        public event Func<DisposeCollectorResourceFactory,Sampler> CallSamplerGeneration;
        public event Func<DisposeCollectorResourceFactory,ResourceLayout> CallTextureResourceLayoutGeneration;
        // TODO: @Performance: Investigate Texture Cache for already loaded textures
        public event Func<ModelRuntimeDescriptor<T>,int,DisposeCollectorResourceFactory,GraphicsDevice,ResourceSet> CallTextureResourceSetGeneration;

        public ModelRuntimeDescriptor(Model<T, RealtimeMaterial> modelIn, string vShaderName, string fShaderName, VertexRuntimeTypes vertexRuntimeType, PrimitiveTopology primitiveTopology, uint renderFlags, uint preEffectsInstancingFlag){

            if(!Verifier.VerifyVertexStruct<T>(vertexRuntimeType))
                throw new ArgumentException($"Type Mismatch ModelRuntimeDescriptor");

            Model = modelIn;
            Length = modelIn.MeshCount;

            TotalInstanceCount = 1;

            _vertexShaderName = vShaderName;
            _fragmentShaderName = fShaderName;

            VertexRuntimeType = vertexRuntimeType;
            PrimitiveTopology = primitiveTopology;

            RenderFlags = renderFlags;
            PreEffectsInstancingFlag = preEffectsInstancingFlag;

            VertexBufferList = new List<DeviceBuffer>();
            IndexBufferList = new List<DeviceBuffer>();
            InstanceBufferList = new List<DeviceBuffer>();
            InstanceShadowMapBufferList = new List<DeviceBuffer>();
            TextureResourceSetsList = new List<ResourceSet>();
            VertexLayoutList = new List<VertexLayoutDescription>();
            VertexPreEffectsInstanceLayoutGenerationList = new EventHandlerList();


            // Reserve first spot for base vertex geometry
            VertexPreEffectsLayouts = new VertexLayoutDescription[PreEffects.GetSizeOfPreEffectFlag(PreEffectsInstancingFlag)+1];
        }

        /// <summary>
        /// Formats Lists to Arrays for the Commandlist generation.
        /// These items are used only during the renderloop
        /// </summary>
        public void FormatResourcesForRuntime(){
            VertexBuffers = VertexBufferList.ToArray();
            IndexBuffers = IndexBufferList.ToArray();
            InstanceBuffers = InstanceBufferList.ToArray();
            InstanceShadowMapBuffers = InstanceShadowMapBufferList.ToArray();
            TextureResourceSets = TextureResourceSetsList.ToArray();
        }
        
        //TODO: Remove this by using a flag same as PreEffectsInstancing..
        /// <summary>
        /// Formats Lists to Arrays for the Pipeline Generation
        /// These items are used during the CreateResources() stage
        /// </summary>
        public void FormatResourcesForPipelineGeneration(){
            VertexLayouts = VertexLayoutList.ToArray();
        }

        public void LoadShaders(GraphicsDevice graphicsDevice){
            VertexShader = IO.LoadShader(_vertexShaderName,ShaderStages.Vertex, graphicsDevice);
            FragmentShader = IO.LoadShader(_fragmentShaderName,ShaderStages.Fragment, graphicsDevice);

            //TODO: make shader settable at runtime
            var shadowMapShaderName = "ShadowMap";
            
            VertexPreEffectsShader = IO.LoadShader("ShadowMap", ShaderStages.Vertex, graphicsDevice);
            FragmentShadowMapShader = IO.LoadShader(shadowMapShaderName, ShaderStages.Fragment, graphicsDevice);
        }

        //TODO: @Investigat: What if multiple delegates are bound to the same event?
        public void InvokeVertexLayoutGeneration(){

            // Base vertex geometry
            VertexLayoutList.Add(CallVertexLayoutGeneration.Invoke());
            VertexPreEffectsLayouts[0]= CallVertexLayoutGeneration.Invoke();

            // Instancing data
            if(CallVertexInstanceLayoutGeneration != null)
                VertexLayoutList.Add(CallVertexInstanceLayoutGeneration.Invoke());

            var shadowMapInstanceVertexLayoutDescriptionOption = (VertexPreEffectsInstanceLayoutGenerationList[PreEffects.ShadowMapKey] as Func<VertexLayoutDescription>);     
            var omniShadowMapInstanceVertexLayoutDescriptionOption = (VertexPreEffectsInstanceLayoutGenerationList[PreEffects.OmniShadowMapKey] as Func<VertexLayoutDescription>);   

            if(shadowMapInstanceVertexLayoutDescriptionOption != null)
                VertexPreEffectsLayouts[PreEffects.GetArrayIndexForFlag(PreEffects.SHADOW_MAP)+1] = shadowMapInstanceVertexLayoutDescriptionOption.Invoke();
            if(omniShadowMapInstanceVertexLayoutDescriptionOption != null)
                VertexPreEffectsLayouts[PreEffects.GetArrayIndexForFlag(PreEffects.OMNI_SHADOW_MAPS)+1] = omniShadowMapInstanceVertexLayoutDescriptionOption.Invoke();
 
        }

        public Sampler InvokeSamplerGeneration(DisposeCollectorResourceFactory factory){
            return CallSamplerGeneration!=null?CallSamplerGeneration(factory):null;
        }

        public ResourceLayout InvokeTextureResourceLayoutGeneration(DisposeCollectorResourceFactory factory){
            return CallTextureResourceLayoutGeneration!=null?CallTextureResourceLayoutGeneration(factory):null;
        }

        public ResourceSet InvokeTextureResourceSetGeneration(int meshIndex, DisposeCollectorResourceFactory factory, GraphicsDevice graphicsDevice){
            return CallTextureResourceSetGeneration!=null?CallTextureResourceSetGeneration(this,meshIndex,factory,graphicsDevice):null;
        }

        public void AddPreEffectsVertexInstanceDelegate(uint id, Func<VertexLayoutDescription> vertexLayoutDelegate){

            if((id & PreEffectsInstancingFlag)== PreEffects.NO_EFFECTS)
                throw new System.ArgumentException($"PreEffects id: {id} does not match the stored PreEffectsFlag");

            if((id & PreEffects.SHADOW_MAP) == PreEffects.SHADOW_MAP)
                VertexPreEffectsInstanceLayoutGenerationList.AddHandler(PreEffects.ShadowMapKey, vertexLayoutDelegate);
            
            if((id & PreEffects.OMNI_SHADOW_MAPS) == PreEffects.OMNI_SHADOW_MAPS)
                VertexPreEffectsInstanceLayoutGenerationList.AddHandler(PreEffects.OmniShadowMapKey, vertexLayoutDelegate);
        
        }

        public void addVertexInstanceDelegate(int id, Func<VertexLayoutDescription> vertexLayoutDelegate){
            //TODO
            //switch on instancing data
        }

    }
}