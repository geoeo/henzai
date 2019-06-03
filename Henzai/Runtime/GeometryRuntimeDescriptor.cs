using System;
using System.Collections.Generic;
using Veldrid;
using Henzai.Core;
using Henzai.Core.Materials;
using Henzai.Core.VertexGeometry;
using Henzai.Core.Reflection;
using Henzai.Core.Acceleration;

namespace Henzai.Runtime
{
    //TODO: Add Mesh, Vertex, Index Buffers
    public sealed class GeometryDescriptor<T> where T: struct, VertexLocateable
    {
        public List<MeshBVH<T>> MeshBVHList {get; private set;}

        public GeometryDescriptor(){
            MeshBVHList = new List<MeshBVH<T>>();

        }

        public void AddModel(ModelRuntimeDescriptor<T> modelRuntimeDescriptor) {
            var meshCount = modelRuntimeDescriptor.Length;
            for(int i =0; i < meshCount; i++){
                var meshBVH = modelRuntimeDescriptor.Model.GetMeshBVH(i);
                MeshBVHList.Add(meshBVH);
            }
        }

    }

}

