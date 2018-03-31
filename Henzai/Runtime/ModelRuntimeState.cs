using System;
using System.Collections.Generic;
using Veldrid;
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
        public Model<T> model;
        public Shader vertexShader;
        public Shader fragmentShader;
        /// <summary>
        /// Defines a Higher Level Render State.
        /// Buffers, Layouts, Shaders, Ratsterizer.
        /// See: <see cref="Veldrid.Pipeline"/>
        /// </summary>
        public Pipeline pipeline;

        public ModelRuntimeState(Model<T> modelIn){
            model = modelIn;

            VertexBuffersList = new List<DeviceBuffer>();
            IndexBuffersList = new List<DeviceBuffer>();
            TextureResourceSetsList = new List<ResourceSet>();
        }

        public void FormatResourcesForRuntime(){
            VertexBuffers = VertexBuffersList.ToArray();
            IndexBuffers = IndexBuffersList.ToArray();
            TextureResourceSets = TextureResourceSetsList.ToArray();
        }

    }
}