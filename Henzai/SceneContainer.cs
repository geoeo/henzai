using System;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;
using HenzaiFunc.Core;
using HenzaiFunc.Core.Types;
using HenzaiFunc.Core.Acceleration;
using Henzai.Core.Acceleration;
using Henzai.Core.Extensions;
using Henzai.Core.Raytracing;
using Henzai.Core;
using Henzai.UI;

using Henzai.Runtime;
using Henzai.Core.VertexGeometry;

namespace Henzai
{
    public abstract class SceneContainer
    {
        protected static Renderable scene;
        protected static UserInterface gui;
        private BVHRuntimeNode[] _bvhRuntimeNodesPNTTB;
        private BVHRuntimeNode[] _bvhRuntimeNodesPN;
        private BVHRuntimeNode[] _bvhRuntimeNodesPT;
        private BVHRuntimeNode[] _bvhRuntimeNodesPC;
        private int[] _bvhTraversalStack;
        private const int CULLING_THRESH = 10;

        public abstract void createScene(GraphicsBackend graphicsBackend, Sdl2Window contextWindow = null);

        public void ChangeBackend(GraphicsBackend graphicsBackend)
        {
            Sdl2Window contextWindow = scene.ContextWindow;
            scene.Dispose();
            createScene(graphicsBackend, contextWindow);
        }

        protected void BuildBVH(GeometryDescriptor<VertexPositionNormalTextureTangentBitangent> PNTTBGeometryDescriptor, GeometryDescriptor<VertexPositionNormal> PNGeometryDescriptor, GeometryDescriptor<VertexPositionTexture> PTGeometryDescriptor, GeometryDescriptor<VertexPositionColor> PCGeometryDescriptor, GeometryDescriptor<VertexPosition> PGeometryDescriptor)
        {
            var splitMethod = SplitMethods.SAH;
            var PNTTBMeshBVHArray = PNTTBGeometryDescriptor.MeshBVHArray;
            var PNMeshBVHArray = PNGeometryDescriptor.MeshBVHArray;
            var PTMeshBVHArray = PTGeometryDescriptor.MeshBVHArray;
            var PCMeshBVHArray = PCGeometryDescriptor.MeshBVHArray;
            var PMeshBVHArray = PGeometryDescriptor.MeshBVHArray;

            var tuplePNTTB = BVHTreeBuilder<MeshBVH<VertexPositionNormalTextureTangentBitangent>>.Build(PNTTBMeshBVHArray, splitMethod);
            BVHTree PNTTBTree = tuplePNTTB.Item1;
            PNTTBMeshBVHArray = tuplePNTTB.Item2;
            int PNTTBTotalNodes = tuplePNTTB.Item3;

            var tuplePN = BVHTreeBuilder<MeshBVH<VertexPositionNormal>>.Build(PNMeshBVHArray, splitMethod);
            BVHTree PNTree= tuplePN.Item1;
            PNMeshBVHArray = tuplePN.Item2;
            int PNTotalNodes = tuplePN.Item3;

            var tuplePT = BVHTreeBuilder<MeshBVH<VertexPositionTexture>>.Build(PTMeshBVHArray, splitMethod);
            BVHTree PTTree = tuplePT.Item1;
            PTMeshBVHArray = tuplePT.Item2;
            int PTTotalNodes = tuplePT.Item3;

            var tuplePC = BVHTreeBuilder<MeshBVH<VertexPositionColor>>.Build(PCMeshBVHArray, splitMethod);
            BVHTree PCTree = tuplePC.Item1;
            PCMeshBVHArray = tuplePC.Item2;
            int PCTotalNodes = tuplePC.Item3;

            _bvhRuntimeNodesPNTTB = BVHRuntime.ConstructBVHRuntime(PNTTBTree, PNTTBTotalNodes);
            _bvhRuntimeNodesPN = BVHRuntime.ConstructBVHRuntime(PNTree, PNTotalNodes);
            _bvhRuntimeNodesPT = BVHRuntime.ConstructBVHRuntime(PTTree, PTTotalNodes);
            _bvhRuntimeNodesPC = BVHRuntime.ConstructBVHRuntime(PCTree, PCTotalNodes);   

            var maxPrimitives = Math.Max(PNTTBMeshBVHArray.Length,Math.Max(PNMeshBVHArray.Length,Math.Max(PTMeshBVHArray.Length,PCMeshBVHArray.Length)));
            _bvhTraversalStack = new int[maxPrimitives];                                                                                                        
        }

        protected void EnableBVHCulling(float deltaTime, Camera camera, GeometryDescriptor<VertexPositionNormalTextureTangentBitangent> PNTTBGeometryDescriptor, GeometryDescriptor<VertexPositionNormal> PNGeometryDescriptor, GeometryDescriptor<VertexPositionTexture> PTGeometryDescriptor, GeometryDescriptor<VertexPositionColor> PCGeometryDescriptor, GeometryDescriptor<VertexPosition> PGeometryDescriptor){

            var PNTTBMeshBVHArray = PNTTBGeometryDescriptor.MeshBVHArray;
            var PNMeshBVHArray = PNGeometryDescriptor.MeshBVHArray;
            var PTMeshBVHArray = PTGeometryDescriptor.MeshBVHArray;
            var PCMeshBVHArray = PCGeometryDescriptor.MeshBVHArray;
            var PMeshBVHArray = PGeometryDescriptor.MeshBVHArray;
              
            if (PNTTBMeshBVHArray.Length > CULLING_THRESH){
                invalidateAABB(PNTTBMeshBVHArray);
                BVHRuntime.TraverseWithFrustumForMesh(_bvhRuntimeNodesPNTTB, PNTTBMeshBVHArray, _bvhTraversalStack, ref camera.ViewProjectionMatirx);
            }
            if (PNMeshBVHArray.Length > CULLING_THRESH){
                invalidateAABB(PNMeshBVHArray);
                BVHRuntime.TraverseWithFrustumForMesh(_bvhRuntimeNodesPN, PNMeshBVHArray, _bvhTraversalStack, ref camera.ViewProjectionMatirx);
            }
            if (PTMeshBVHArray.Length > CULLING_THRESH){
                invalidateAABB(PTMeshBVHArray);
                BVHRuntime.TraverseWithFrustumForMesh(_bvhRuntimeNodesPT, PTMeshBVHArray, _bvhTraversalStack, ref camera.ViewProjectionMatirx);
            }
            if (PCMeshBVHArray.Length > CULLING_THRESH){
                invalidateAABB(PCMeshBVHArray);
                BVHRuntime.TraverseWithFrustumForMesh(_bvhRuntimeNodesPC, PCMeshBVHArray, _bvhTraversalStack, ref camera.ViewProjectionMatirx);
            }
        }

        private static void invalidateAABB<T>(MeshBVH<T>[] meshBVHArray) where T : struct, VertexLocateable {

            for (int i = 0; i < meshBVHArray.Length; i++){
                var meshBVH = meshBVHArray[i];
                meshBVH.AABBIsValid = false;
                meshBVHArray[i] = meshBVH;
            }
                    
        }


    }
}