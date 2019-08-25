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
        private EventHandlerList VertexInstanceLayoutGenerationList;
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
            VertexInstanceLayoutGenerationList = new EventHandlerList();
            VertexPreEffectsInstanceLayoutGenerationList = new EventHandlerList();


            // Reserve first spot for base vertex geometry
            VertexPreEffectsLayouts = new VertexLayoutDescription[PreEffectFlags.GetSizeOfPreEffectFlag(PreEffectsInstancingFlag)+1];
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

        //TODO: @Investigate: What if multiple delegates are bound to the same event?         
        public void InvokeVertexLayoutGeneration(){

            // Base vertex geometry
            VertexLayoutList.Add(CallVertexLayoutGeneration.Invoke());
            VertexPreEffectsLayouts[0]= CallVertexLayoutGeneration.Invoke();

            // Instancing data
            foreach(var key in InstancingKeys.GetKeys()){
                var vertexInstanceDeletegate = VertexInstanceLayoutGenerationList[key] as Func<VertexLayoutDescription>;
                if(vertexInstanceDeletegate != null)
                    VertexLayoutList.Add(vertexInstanceDeletegate.Invoke());
            }

            foreach(var flagKeyTuple in PreEffectKeys.GetFlagKeyTuples()){
                var flag = flagKeyTuple.Item1;
                var key = flagKeyTuple.Item2;
                var vertexInstanceDeletegate = (VertexPreEffectsInstanceLayoutGenerationList[key] as Func<VertexLayoutDescription>);
                if(vertexInstanceDeletegate != null)
                    VertexPreEffectsLayouts[PreEffectFlags.GetArrayIndexForFlag(flag)+1] = vertexInstanceDeletegate.Invoke();
                    
            }
 
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

            if((id & PreEffectsInstancingFlag)== PreEffectFlags.NO_EFFECTS)
                throw new System.ArgumentException($"PreEffects id: {id} does not match the stored PreEffectsTypes");

            else if((id & PreEffectFlags.SHADOW_MAP) == PreEffectFlags.SHADOW_MAP)
                VertexPreEffectsInstanceLayoutGenerationList.AddHandler(PreEffectKeys.ShadowMapKey, vertexLayoutDelegate);
            
            else if((id & PreEffectFlags.OMNI_SHADOW_MAPS) == PreEffectFlags.OMNI_SHADOW_MAPS)
                VertexPreEffectsInstanceLayoutGenerationList.AddHandler(PreEffectKeys.OmniShadowMapKey, vertexLayoutDelegate);
        
        }

        public void AddVertexInstanceDelegate(InstancingTypes instancingTypes, Func<VertexLayoutDescription> vertexLayoutDelegate){

            switch(instancingTypes){
                case InstancingTypes.Positions:
                    VertexInstanceLayoutGenerationList.AddHandler(InstancingKeys.PositionKey, vertexLayoutDelegate);
                break;
                case InstancingTypes.ViewMatricies:
                    VertexInstanceLayoutGenerationList.AddHandler(InstancingKeys.ViewMatricesKey, vertexLayoutDelegate);
                break;
                case InstancingTypes.NoData:
                    Debug.WriteLine("No need to set delegate for NO_DATA. Maybe you are using the wrong flag?");
                break;
        
            }                
        }

    }
}