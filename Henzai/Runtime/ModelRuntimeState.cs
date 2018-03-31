using System;
using System.IO;
using System.Collections.Generic;
using Veldrid;
using Veldrid.ImageSharp;
using Henzai;
using Henzai.Geometry;

namespace Henzai.Runtime.Render
{
    public sealed class ModelRuntimeState<T> where T : struct
    {

        /// <summary>
        /// Used During Resource Creation
        /// </summary>
        public List<DeviceBuffer> VertexBuffersList {get; private set;}
        /// <summary>
        /// Used During Resource Creation
        /// </summary>
        public List<DeviceBuffer> IndexBuffersList {get; private set;}
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
        public ResourceSet[] TextureResourceSets {get; private set;}
        public ResourceLayout TextureResourceLayout {get; set;}
        public Sampler TextureSampler {get;set;}
        public Shader VertexShader {get; private set;}
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


        public event Func<VertexLayoutDescription> CallVertexLayoutGeneration;
        public event Func<DisposeCollectorResourceFactory,Sampler> CallSamplerGeneration;
        public event Func<DisposeCollectorResourceFactory,ResourceLayout> CallTextureResourceLayoutGeneration;
        public event Func<ModelRuntimeState<T>,int,DisposeCollectorResourceFactory,GraphicsDevice,ResourceSet> CallTextureResourceSetGeneration;

        public ModelRuntimeState(Model<T> modelIn, string vShaderName, string fShaderName){
            Model = modelIn;

            _vertexShaderName = vShaderName;
            _fragmentShaderName = fShaderName;

            VertexBuffersList = new List<DeviceBuffer>();
            IndexBuffersList = new List<DeviceBuffer>();
            TextureResourceSetsList = new List<ResourceSet>();
        }

        public void FormatResourcesForRuntime(){
            VertexBuffers = VertexBuffersList.ToArray();
            IndexBuffers = IndexBuffersList.ToArray();
            TextureResourceSets = TextureResourceSetsList.ToArray();
        }

        public void LoadShaders(GraphicsDevice graphicsDevice){
            VertexShader = IO.LoadShader(_vertexShaderName,ShaderStages.Vertex,graphicsDevice);
            FragmentShader = IO.LoadShader(_fragmentShaderName,ShaderStages.Fragment,graphicsDevice);
        }

        public VertexLayoutDescription InvokeVertexLayoutGeneration(){
            return CallVertexLayoutGeneration.Invoke();
        }

        public Sampler InvokeSamplerGeneration(DisposeCollectorResourceFactory factory){
            return CallSamplerGeneration(factory);
        }

        public ResourceLayout InvokeTextureResourceLayoutGeneration(DisposeCollectorResourceFactory factory){
            return CallTextureResourceLayoutGeneration(factory);
        }

        public ResourceSet InvokeTextureResourceSetGeneration(int meshIndex, DisposeCollectorResourceFactory factory, GraphicsDevice graphicsDevice){
            return CallTextureResourceSetGeneration(this,meshIndex,factory,graphicsDevice);
        }

    }
}