using System;
using System.IO;
using System.Collections.Generic;
using Veldrid;
using Veldrid.ImageSharp;
using Henzai;
using Henzai.Runtime;
using Henzai.Geometry;

namespace Henzai.Runtime
{
    public sealed class ModelRuntimeDescriptor<T> where T : struct
    {

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
        /// <summary>
        /// Used During Rendering
        /// </summary>
        public ResourceSet[] TextureResourceSets {get; private set;}
        public ResourceLayout TextureResourceLayout {get; set;}
        public Sampler TextureSampler {get;set;}
        public Shader VertexShader {get; private set;}
        public List<VertexLayoutDescription> VertexLayoutList {get;private set;}
        public VertexLayoutDescription[] VertexLayouts {get;private set;}
        private string _vertexShaderName;
        public Shader FragmentShader {get; private set;}
        private string _fragmentShaderName;
        /// <summary>
        /// Defines a Higher Level Render State.
        /// Buffers, Layouts, Shaders, Ratsterizer.
        /// See: <see cref="Veldrid.Pipeline"/>
        /// </summary>
        public Pipeline Pipeline {get; set;}
        /// <summary>
        /// Contains Geometry and Material Properties
        /// See: <see cref="Henzai.Geometry.Model{T}"/>
        /// </summary>
        public Model<T> Model {get;set;}
        public VertexTypes VertexType {get; private set;}
        public PrimitiveTopology PrimitiveTopology {get; private set;}
        public uint TotalInstanceCount{get;set;}

        public event Func<VertexLayoutDescription> CallVertexLayoutGeneration;
        public event Func<VertexLayoutDescription> CallVertexInstanceLayoutGeneration;
        public event Func<DisposeCollectorResourceFactory,Sampler> CallSamplerGeneration;
        public event Func<DisposeCollectorResourceFactory,ResourceLayout> CallTextureResourceLayoutGeneration;
        public event Func<ModelRuntimeDescriptor<T>,int,DisposeCollectorResourceFactory,GraphicsDevice,ResourceSet> CallTextureResourceSetGeneration;

        public ModelRuntimeDescriptor(Model<T> modelIn, string vShaderName, string fShaderName, VertexTypes vertexType, PrimitiveTopology primitiveTopology){

            if(!Verifier.verifyVertexStruct<T>(vertexType))
                throw new ArgumentException($"Type Mismatch ModelRuntimeDescriptor");

            Model = modelIn;

            TotalInstanceCount = 1;

            _vertexShaderName = vShaderName;
            _fragmentShaderName = fShaderName;

            VertexType = vertexType;
            PrimitiveTopology = primitiveTopology;

            VertexBufferList = new List<DeviceBuffer>();
            IndexBufferList = new List<DeviceBuffer>();
            InstanceBufferList = new List<DeviceBuffer>();
            TextureResourceSetsList = new List<ResourceSet>();
            VertexLayoutList = new List<VertexLayoutDescription>();
        }

        /// <summary>
        /// Formats Lists to Arrays for the Commandlist generation.
        /// These items are used only during the renderloop
        /// </summary>
        public void FormatResourcesForRuntime(){
            VertexBuffers = VertexBufferList.ToArray();
            IndexBuffers = IndexBufferList.ToArray();
            InstanceBuffers = InstanceBufferList.ToArray();
            TextureResourceSets = TextureResourceSetsList.ToArray();
        }
        /// <summary>
        /// Formats Lists to Arrays for the Pipeline Generation
        /// These items are used during the CreateResources() stage
        /// </summary>
        public void FormatResourcesForPipelineGeneration(){
            VertexLayouts = VertexLayoutList.ToArray();
        }

        public void LoadShaders(GraphicsDevice graphicsDevice){
            VertexShader = IO.LoadShader(_vertexShaderName,ShaderStages.Vertex,graphicsDevice);
            FragmentShader = IO.LoadShader(_fragmentShaderName,ShaderStages.Fragment,graphicsDevice);
        }

        public void InvokeVertexLayoutGeneration(){
            VertexLayoutList.Add(CallVertexLayoutGeneration.Invoke());
        }

        public void InvokeVertexInstanceLayoutGeneration(){
            if(CallVertexInstanceLayoutGeneration != null)
                VertexLayoutList.Add(CallVertexInstanceLayoutGeneration.Invoke());
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

    }
}