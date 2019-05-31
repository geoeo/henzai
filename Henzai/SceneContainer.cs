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
        private Ray[,] _zCullingRays;
        private Vector4[,] _zCullingDirections;
        private int[] _bvhTraversalStack;
        private const int CULLING_THRESH = 10;



        public abstract void createScene(GraphicsBackend graphicsBackend, Sdl2Window contextWindow = null);

        public void ChangeBackend(GraphicsBackend graphicsBackend)
        {
            Sdl2Window contextWindow = scene.ContextWindow;
            scene.Dispose();
            createScene(graphicsBackend, contextWindow);
        }

        protected void GenerateZCullingRays(Camera camera){
            //TODO: Refactor this once geometry can be changes at runtime
            var height = (int)camera.WindowHeight;
            var width = (int)camera.WindowWidth;
            _zCullingRays = new Ray[height, width];
            _zCullingDirections = new Vector4[height, width];

            var cameraPos = new Vector4(camera.Position, 1.0f);
            var cameraToWS = RaytraceCamera.CameraToWorld(camera.ViewMatrix);
            var cameraToWSRot = Core.Numerics.Geometry.Rotation(ref cameraToWS);

            for (int py = 0; py < height; py++){
                for(int px = 0; px < width; px ++){
                    var cameraSamples = RaytraceCamera.PixelToCamera(px, py, width, height, camera.FieldOfView, false);
                    var dirCS = RaytraceCamera.RayDirection(cameraSamples.Item1, cameraSamples.Item2);
                    _zCullingDirections[py,px] = dirCS;
                }
            }
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

            var tuplePNTTB  = BVHTreeBuilder<MeshBVH<VertexPositionNormalTextureTangentBitangent>>.Build(allMeshBVHPNTTB, splitMethod);
            BVHTree PNTTBTree = tuplePNTTB.Item1;
            _orderedPNTTB = tuplePNTTB.Item2;
            int PNTTBTotalNodes = tuplePNTTB.Item3;

            var tuplePN  = BVHTreeBuilder<MeshBVH<VertexPositionNormal>>.Build(allMeshBVHPN, splitMethod);
            BVHTree PNTree= tuplePN.Item1;
            _orderedPN= tuplePN.Item2;
            int PNTotalNodes = tuplePN.Item3;

            var tuplePT  = BVHTreeBuilder<MeshBVH<VertexPositionTexture>>.Build(allMeshBVHPT, splitMethod);
            BVHTree PTTree= tuplePT.Item1;
            _orderedPT = tuplePT.Item2;
            int PTTotalNodes = tuplePT.Item3;

            var tuplePC  = BVHTreeBuilder<MeshBVH<VertexPositionColor>>.Build(allMeshBVHPC, splitMethod);
            BVHTree PCTree= tuplePC.Item1;
            _orderedPC = tuplePC.Item2;
            int PCTotalNodes = tuplePC.Item3;

            _bvhRuntimeNodesPNTTB = BVHRuntime.ConstructBVHRuntime(PNTTBTree, PNTTBTotalNodes);
            _bvhRuntimeNodesPN = BVHRuntime.ConstructBVHRuntime(PNTree, PNTotalNodes);
            _bvhRuntimeNodesPT = BVHRuntime.ConstructBVHRuntime(PTTree, PTTotalNodes);
            _bvhRuntimeNodesPC = BVHRuntime.ConstructBVHRuntime(PCTree, PCTotalNodes);   

            var maxPrimitives = Math.Max(_orderedPNTTB.Length,Math.Max(_orderedPN.Length,Math.Max(_orderedPT.Length,_orderedPC.Length)));
            _bvhTraversalStack = new int[maxPrimitives];                                                                                                        
        }

        protected void UpdateZCullingRays(float delta, Camera camera){
            var height = (int)camera.WindowHeight;
            var width = (int)camera.WindowWidth;
            var cameraPos = new Vector4(camera.Position, 1.0f);
            var cameraToWS = RaytraceCamera.CameraToWorld(camera.ViewMatrix);
            var cameraToWSRot = Core.Numerics.Geometry.Rotation(ref cameraToWS);

            for (int py = 0; py < height; py++){
                for(int px = 0; px < width; px++){
                    var dirCS = _zCullingDirections[py,px];
                    var dirWS = Vector4.Normalize(Vector4.Transform(dirCS, cameraToWSRot));
                    _zCullingRays[py,px] = new Ray(cameraPos,dirWS);
                }
            }
        }

        // Abysmal performance for triangles!
        protected void EnableZCulling(float deltaTime, GraphicsDevice graphicsDevice, CommandList commandList, Camera camera, ModelRuntimeDescriptor<VertexPositionNormalTextureTangentBitangent>[] modelPNTTBDescriptorArray, ModelRuntimeDescriptor<VertexPositionNormal>[] modelPNDescriptorArray, ModelRuntimeDescriptor<VertexPositionTexture>[] modelPTDescriptorArray, ModelRuntimeDescriptor<VertexPositionColor>[] modelPCDescriptorArray, ModelRuntimeDescriptor<VertexPosition>[] modelPDescriptorArray){

        //     invalidateAABB(modelPNTTBDescriptorArray);
        //     invalidateAABB(modelPNDescriptorArray);
        //     invalidateAABB(modelPTDescriptorArray);
        //     invalidateAABB(modelPCDescriptorArray);

        //     var height = _zCullingRays.GetLength(0);
        //     var width = _zCullingRays.GetLength(1);

        //     if (_orderedPNTTB.Length > 0){
                
        //         for (int py = 0; py < height; py++){
        //             for(int px = 0; px < width; px++){
        //                 var ray = _zCullingRays[py,px];
        //                 Culler.ZCullBVH(_bvhRuntimeNodesPNTTB, _orderedPNTTB, _bvhTraversalStack, ray);
        //             }
        //         }
        //     }

        }

         protected void EnableBVHCulling(float deltaTime, GraphicsDevice graphicsDevice, CommandList commandList, Camera camera, ModelRuntimeDescriptor<VertexPositionNormalTextureTangentBitangent>[] modelPNTTBDescriptorArray, ModelRuntimeDescriptor<VertexPositionNormal>[] modelPNDescriptorArray, ModelRuntimeDescriptor<VertexPositionTexture>[] modelPTDescriptorArray, ModelRuntimeDescriptor<VertexPositionColor>[] modelPCDescriptorArray, ModelRuntimeDescriptor<VertexPosition>[] modelPDescriptorArray){
      
            if (_orderedPNTTB.Length > CULLING_THRESH){
                invalidateAABB(modelPNTTBDescriptorArray);
                BVHRuntime.TraverseWithFrustumForMesh(_bvhRuntimeNodesPNTTB, _orderedPNTTB, _bvhTraversalStack, ref camera.ViewProjectionMatirx);
            }
            if (_orderedPN.Length > CULLING_THRESH){
                invalidateAABB(modelPNDescriptorArray);
                BVHRuntime.TraverseWithFrustumForMesh(_bvhRuntimeNodesPN, _orderedPN, _bvhTraversalStack, ref camera.ViewProjectionMatirx);
            }
            if (_orderedPT.Length > CULLING_THRESH){
                invalidateAABB(modelPTDescriptorArray);
                BVHRuntime.TraverseWithFrustumForMesh(_bvhRuntimeNodesPT, _orderedPT, _bvhTraversalStack, ref camera.ViewProjectionMatirx);
            }
            if (_orderedPC.Length > CULLING_THRESH){
                invalidateAABB(modelPCDescriptorArray);
                BVHRuntime.TraverseWithFrustumForMesh(_bvhRuntimeNodesPC, _orderedPC, _bvhTraversalStack, ref camera.ViewProjectionMatirx);
            }
         }

        protected void EnableCulling(float deltaTime, GraphicsDevice graphicsDevice, CommandList commandList, Camera camera, ModelRuntimeDescriptor<VertexPositionNormalTextureTangentBitangent>[] modelPNTTBDescriptorArray, ModelRuntimeDescriptor<VertexPositionNormal>[] modelPNDescriptorArray, ModelRuntimeDescriptor<VertexPositionTexture>[] modelPTDescriptorArray, ModelRuntimeDescriptor<VertexPositionColor>[] modelPCDescriptorArray, ModelRuntimeDescriptor<VertexPosition>[] modelPDescriptorArray)
        {
            var updateBuffers = false;
            if (updateBuffers)
                commandList.Begin();

            foreach (var modelDescriptor in modelPNTTBDescriptorArray)
            {
                var model = modelDescriptor.Model;
                byte vertexSizeInBytes = model.GetMesh(0).Vertices[0].GetSizeInBytes();

                var meshCount = model.MeshCount;
                for (int i = 0; i < meshCount; i++)
                    FrustumCullMesh<VertexPositionNormalTextureTangentBitangent>(commandList, camera, updateBuffers, modelDescriptor, model, vertexSizeInBytes, i);
            }

            foreach (var modelDescriptor in modelPNDescriptorArray)
            {
                var model = modelDescriptor.Model;
                byte vertexSizeInBytes = model.GetMesh(0).Vertices[0].GetSizeInBytes();

                var meshCount = model.MeshCount;
                for (int i = 0; i < meshCount; i++)
                    FrustumCullMesh<VertexPositionNormal>(commandList, camera, updateBuffers, modelDescriptor, model, vertexSizeInBytes, i);
            }

            foreach (var modelDescriptor in modelPTDescriptorArray)
            {
                var model = modelDescriptor.Model;
                byte vertexSizeInBytes = model.GetMesh(0).Vertices[0].GetSizeInBytes();

                var meshCount = model.MeshCount;
                for (int i = 0; i < meshCount; i++)
                    FrustumCullMesh<VertexPositionTexture>(commandList, camera, updateBuffers, modelDescriptor, model, vertexSizeInBytes, i);
            }

            foreach (var modelDescriptor in modelPCDescriptorArray)
            {
                var model = modelDescriptor.Model;
                byte vertexSizeInBytes = model.GetMesh(0).Vertices[0].GetSizeInBytes();

                var meshCount = model.MeshCount;
                for (int i = 0; i < meshCount; i++)
                    FrustumCullMesh<VertexPositionColor>(commandList, camera, updateBuffers, modelDescriptor, model, vertexSizeInBytes, i);
            }

            // // Dont cull VertexPosition as Skyboxes are VP and geometry is set via tricks in fragment shader
            // foreach(var modelDescriptor in modelPDescriptorArray){
            //     var model = modelDescriptor.Model;
            //     byte vertexSizeInBytes = model.GetMesh(0).Vertices[0].GetSizeInBytes();

            //     var meshCount = model.MeshCount;
            //     for (int i = 0; i < meshCount; i++)
            //         FrustumCullMesh<VertexPosition>(commandList, camera, updateBuffers, modelDescriptor, model, vertexSizeInBytes, i);             
            // }

            if (updateBuffers)
            {
                commandList.End();
                graphicsDevice.SubmitCommands(commandList);
            }
        }

        private static void invalidateAABB<T>(ModelRuntimeDescriptor<T>[] modelDescriptorArray) where T : struct, VertexLocateable {
            foreach (var modelDescriptor in modelDescriptorArray)
            {
                var model = modelDescriptor.Model;
                model.SetIsCulled();
                var meshCount = model.MeshCount;
                for (int i = 0; i < meshCount; i++){
                    var mesh = model.GetMesh(i);
                    mesh.AABBIsValid = false;
                }
            }
        }

        private static void FrustumCullMesh<T>(CommandList commandList, Camera camera, bool updateBuffers, ModelRuntimeDescriptor<T> modelDescriptor, Core.Model<T, Core.Materials.RealtimeMaterial> model, byte vertexSizeInBytes, int i) where T : struct, VertexLocateable
        {
            var vertexBuffer = modelDescriptor.VertexBuffers[i];
            var indexBuffer = modelDescriptor.IndexBuffers[i];
            var mesh = model.GetMesh(i);

            var worldMatrix = mesh.World;
            var MVP = worldMatrix * camera.ViewProjectionMatirx;

            if (updateBuffers)
            {
                mesh.CleanIndices.CopyTo(mesh.ValidIndices, 0);
                mesh.CleanVertices.CopyTo(mesh.ValidVertices, 0);
            }

            Culler.FrustumCullMesh<T>(ref MVP, mesh);

            if (updateBuffers)
            {
                uint vertexBytesToCopy = (vertexSizeInBytes * mesh.ValidVertexCount).ToUnsigned();
                uint indexBytesToCopy = (sizeof(ushort) * mesh.ValidIndexCount).ToUnsigned();
                uint allIndexBytesToCopy = (sizeof(ushort) * mesh.IndexCount).ToUnsigned();
                uint allVertexBytesToCopy = (vertexSizeInBytes * mesh.VertexCount).ToUnsigned();

                commandList.UpdateBuffer<T>(vertexBuffer, 0, ref mesh.CleanVertices[0], allVertexBytesToCopy);
                commandList.UpdateBuffer<ushort>(indexBuffer, 0, ref mesh.CleanIndices[0], allIndexBytesToCopy);

                commandList.UpdateBuffer<T>(vertexBuffer, 0, ref mesh.ValidVertices[0], vertexBytesToCopy);
                commandList.UpdateBuffer<ushort>(indexBuffer, 0, ref mesh.ValidIndices[0], indexBytesToCopy);
            }
        }
    }
}