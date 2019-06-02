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
        private MeshBVH<VertexPositionNormalTextureTangentBitangent>[] _orderedPNTTB;
        private BVHRuntimeNode[] _bvhRuntimeNodesPN;
        private MeshBVH<VertexPositionNormal>[] _orderedPN;
        private BVHRuntimeNode[] _bvhRuntimeNodesPT;
        private MeshBVH<VertexPositionTexture>[] _orderedPT;
        private BVHRuntimeNode[] _bvhRuntimeNodesPC;
        private MeshBVH<VertexPositionColor>[] _orderedPC;
        private int[] _bvhTraversalStack;
        private const int CULLING_THRESH = 10;

        public abstract void createScene(GraphicsBackend graphicsBackend, Sdl2Window contextWindow = null);

        public void ChangeBackend(GraphicsBackend graphicsBackend)
        {
            Sdl2Window contextWindow = scene.ContextWindow;
            scene.Dispose();
            createScene(graphicsBackend, contextWindow);
        }

        protected void BuildBVH(ModelRuntimeDescriptor<VertexPositionNormalTextureTangentBitangent>[] modelPNTTBDescriptorArray, ModelRuntimeDescriptor<VertexPositionNormal>[] modelPNDescriptorArray, ModelRuntimeDescriptor<VertexPositionTexture>[] modelPTDescriptorArray, ModelRuntimeDescriptor<VertexPositionColor>[] modelPCDescriptorArray, ModelRuntimeDescriptor<VertexPosition>[] modelPDescriptorArray)
        {
            var allMeshCountPNTTB = 0;
            var allMeshCountPN = 0;
            var allMeshCountPT = 0;
            var allMeshCountPC = 0;
            var splitMethod = SplitMethods.SAH;

            foreach (var modelDescriptor in modelPNTTBDescriptorArray)
                allMeshCountPNTTB += modelDescriptor.Model.MeshCount;

            foreach (var modelDescriptor in modelPNDescriptorArray)
                allMeshCountPN += modelDescriptor.Model.MeshCount;

            foreach (var modelDescriptor in modelPTDescriptorArray)
                allMeshCountPT += modelDescriptor.Model.MeshCount;

            foreach (var modelDescriptor in modelPCDescriptorArray)
                allMeshCountPC += modelDescriptor.Model.MeshCount;

            var allMeshBVHPNTTB = new MeshBVH<VertexPositionNormalTextureTangentBitangent>[allMeshCountPNTTB];
            var allMeshBVHPN = new MeshBVH<VertexPositionNormal>[allMeshCountPN];
            var allMeshBVHPT = new MeshBVH<VertexPositionTexture>[allMeshCountPT];
            var allMeshBVHPC = new MeshBVH<VertexPositionColor>[allMeshCountPC];

            var indexPNTTB = 0;
            foreach (var modelDescriptor in modelPNTTBDescriptorArray)
            {
                var model = modelDescriptor.Model;
                var meshCount = model.MeshCount;
                for (int i = 0; i < meshCount; i++)
                {
                    var meshBVH = model.GetMeshBVH(i); 
                    allMeshBVHPNTTB[indexPNTTB] = meshBVH;
                    indexPNTTB++;
                }
            }

            // PN
            var indexPN = 0;
            foreach (var modelDescriptor in modelPNDescriptorArray)
            {
                var model = modelDescriptor.Model;
                var meshCount = model.MeshCount;
                for (int i = 0; i < meshCount; i++)
                {
                    var meshBVH = model.GetMeshBVH(i);
                    allMeshBVHPN[indexPN] = meshBVH;
                    indexPN++;
                }
            }

            // PT
            var indexPT = 0;
            foreach (var modelDescriptor in modelPTDescriptorArray)
            {
                var model = modelDescriptor.Model;
                var meshCount = model.MeshCount;
                for (int i = 0; i < meshCount; i++)
                {
                    var meshBVH = model.GetMeshBVH(i);
                    allMeshBVHPT[indexPT] = meshBVH;
                    indexPT++;
                }
            }

            // PC
            var indexPC = 0;
            foreach (var modelDescriptor in modelPCDescriptorArray)
            {
                var model = modelDescriptor.Model;
                var meshCount = model.MeshCount;
                for (int i = 0; i < meshCount; i++)
                {
                    var meshBVH = model.GetMeshBVH(i);
                    allMeshBVHPC[indexPC] = meshBVH;
                    indexPC++;
                }
            }

            var tuplePNTTB = BVHTreeBuilder<MeshBVH<VertexPositionNormalTextureTangentBitangent>>.Build(allMeshBVHPNTTB, splitMethod);
            BVHTree PNTTBTree = tuplePNTTB.Item1;
            _orderedPNTTB = tuplePNTTB.Item2;
            int PNTTBTotalNodes = tuplePNTTB.Item3;

            var tuplePN = BVHTreeBuilder<MeshBVH<VertexPositionNormal>>.Build(allMeshBVHPN, splitMethod);
            BVHTree PNTree= tuplePN.Item1;
            _orderedPN = tuplePN.Item2;
            int PNTotalNodes = tuplePN.Item3;

            var tuplePT = BVHTreeBuilder<MeshBVH<VertexPositionTexture>>.Build(allMeshBVHPT, splitMethod);
            BVHTree PTTree = tuplePT.Item1;
            _orderedPT = tuplePT.Item2;
            int PTTotalNodes = tuplePT.Item3;

            var tuplePC = BVHTreeBuilder<MeshBVH<VertexPositionColor>>.Build(allMeshBVHPC, splitMethod);
            BVHTree PCTree = tuplePC.Item1;
            _orderedPC = tuplePC.Item2;
            int PCTotalNodes = tuplePC.Item3;

            _bvhRuntimeNodesPNTTB = BVHRuntime.ConstructBVHRuntime(PNTTBTree, PNTTBTotalNodes);
            _bvhRuntimeNodesPN = BVHRuntime.ConstructBVHRuntime(PNTree, PNTotalNodes);
            _bvhRuntimeNodesPT = BVHRuntime.ConstructBVHRuntime(PTTree, PTTotalNodes);
            _bvhRuntimeNodesPC = BVHRuntime.ConstructBVHRuntime(PCTree, PCTotalNodes);   

            var maxPrimitives = Math.Max(_orderedPNTTB.Length,Math.Max(_orderedPN.Length,Math.Max(_orderedPT.Length,_orderedPC.Length)));
            _bvhTraversalStack = new int[maxPrimitives];                                                                                                        
        }

         protected void EnableBVHCulling(float deltaTime, GraphicsDevice graphicsDevice, CommandList commandList, Camera camera, ModelRuntimeDescriptor<VertexPositionNormalTextureTangentBitangent>[] modelPNTTBDescriptorArray, ModelRuntimeDescriptor<VertexPositionNormal>[] modelPNDescriptorArray, ModelRuntimeDescriptor<VertexPositionTexture>[] modelPTDescriptorArray, ModelRuntimeDescriptor<VertexPositionColor>[] modelPCDescriptorArray, ModelRuntimeDescriptor<VertexPosition>[] modelPDescriptorArray){
      
            if (_orderedPNTTB.Length > CULLING_THRESH){
                invalidateAABB(modelPNTTBDescriptorArray);
                BVHRuntime.TraverseWithFrustumForMesh(_bvhRuntimeNodesPNTTB, _orderedPNTTB, _bvhTraversalStack, ref camera.ViewProjectionMatirx);
                // since meshBVH is a struct we have to copy it back
                // @Investigate -> use the same array for culling and generating render commands
                var indexPNTTB = 0;
                foreach (var modelDescriptor in modelPNTTBDescriptorArray)
                {
                    var model = modelDescriptor.Model;
                    var meshCount = model.MeshCount;
                    for (int i = 0; i < meshCount; i++)
                    {
                        var meshBVH = _orderedPNTTB[indexPNTTB]; 
                        model.SetMeshBVH(i, meshBVH);
                        indexPNTTB++;
                    }
                }
            }
            if (_orderedPN.Length > CULLING_THRESH){
                invalidateAABB(modelPNDescriptorArray);
                BVHRuntime.TraverseWithFrustumForMesh(_bvhRuntimeNodesPN, _orderedPN, _bvhTraversalStack, ref camera.ViewProjectionMatirx);
                var indexPN = 0;
                foreach (var modelDescriptor in modelPNDescriptorArray)
                {
                    var model = modelDescriptor.Model;
                    var meshCount = model.MeshCount;
                    for (int i = 0; i < meshCount; i++)
                    {
                        var meshBVH = _orderedPN[indexPN];
                        model.SetMeshBVH(i, meshBVH);
                        indexPN++;
                    }
                }
            }
            if (_orderedPT.Length > CULLING_THRESH){
                invalidateAABB(modelPTDescriptorArray);
                BVHRuntime.TraverseWithFrustumForMesh(_bvhRuntimeNodesPT, _orderedPT, _bvhTraversalStack, ref camera.ViewProjectionMatirx);
                var indexPT = 0;
                foreach (var modelDescriptor in modelPTDescriptorArray)
                {
                    var model = modelDescriptor.Model;
                    var meshCount = model.MeshCount;
                    for (int i = 0; i < meshCount; i++)
                    {
                        var meshBVH = _orderedPT[indexPT];
                        model.SetMeshBVH(i, meshBVH);
                        indexPT++;
                    }
                }
            }
            if (_orderedPC.Length > CULLING_THRESH){
                invalidateAABB(modelPCDescriptorArray);
                BVHRuntime.TraverseWithFrustumForMesh(_bvhRuntimeNodesPC, _orderedPC, _bvhTraversalStack, ref camera.ViewProjectionMatirx);
                var indexPC = 0;
                foreach (var modelDescriptor in modelPCDescriptorArray)
                {
                    var model = modelDescriptor.Model;
                    var meshCount = model.MeshCount;
                    for (int i = 0; i < meshCount; i++)
                    {
                        var meshBVH = _orderedPC[indexPC];
                        model.SetMeshBVH(i, meshBVH);
                        indexPC++;
                    }
                }
            }
         }

        private static void invalidateAABB<T>(ModelRuntimeDescriptor<T>[] modelDescriptorArray) where T : struct, VertexLocateable {
            foreach (var modelDescriptor in modelDescriptorArray)
            {
                var model = modelDescriptor.Model;
                var meshCount = model.MeshCount;
                for (int i = 0; i < meshCount; i++){
                    var meshBVH = model.GetMeshBVH(i);
                    meshBVH.AABBIsValid = false;
                    model.SetMeshBVH(i, meshBVH);
                }
                    
            }
        }


    }
}