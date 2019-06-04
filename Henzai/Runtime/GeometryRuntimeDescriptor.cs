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
    //TODO: Add Mesh, Vertex, Index Buffers. Also add the corresponding arrays aswell.
    public sealed class GeometryDescriptor<T> where T: struct, VertexLocateable
    {
        public List<MeshBVH<T>> MeshBVHList {get; private set;}
        public MeshBVH<T>[] MeshBVHArray;

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

        public void FormatForRuntime(ModelRuntimeDescriptor<T>[] modelDescriptorArray){
            MeshBVHArray = MeshBVHList.ToArray();

            var index = 0;
            for (int i = 0; i < modelDescriptorArray.Length; i++){
                var modelState = modelDescriptorArray[i];
                var geometryCount = modelState.Length;
                modelState.FormatResourcesForRuntime();
                for(int j = 0; j < geometryCount; j++){
                    var meshBVH = MeshBVHArray[index + j];
                    meshBVH.ModelRuntimeIndex = i;
                    meshBVH.MeshRuntimeIndex = j;
                    MeshBVHArray[j] = meshBVH;
                }
                index += geometryCount;
            }
        }

    }

}

