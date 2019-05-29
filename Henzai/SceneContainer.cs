using Veldrid;
using Veldrid.Sdl2;
using HenzaiFunc.Core.Types;
using HenzaiFunc.Core.Acceleration;
using Henzai.Core.Acceleration;
using Henzai.Core.Extensions;
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
        private IndexedTriangleEngine<VertexPositionNormalTextureTangentBitangent>[] _orderedPNTTB;


        public abstract void createScene(GraphicsBackend graphicsBackend, Sdl2Window contextWindow = null);

        public void ChangeBackend(GraphicsBackend graphicsBackend)
        {
            Sdl2Window contextWindow = scene.ContextWindow;
            scene.Dispose();
            createScene(graphicsBackend, contextWindow);
        }

        protected void BuildBVH(ModelRuntimeDescriptor<VertexPositionNormalTextureTangentBitangent>[] modelPNTTBDescriptorArray, ModelRuntimeDescriptor<VertexPositionNormal>[] modelPNDescriptorArray, ModelRuntimeDescriptor<VertexPositionTexture>[] modelPTDescriptorArray, ModelRuntimeDescriptor<VertexPositionColor>[] modelPCDescriptorArray, ModelRuntimeDescriptor<VertexPosition>[] modelPDescriptorArray)
        {
            var allTrianglesCountPNTTB = 0;
            var allTrianglesCountPN = 0;
            var allTrianglesCountPT = 0;
            var allTrianglesCountPC = 0;

            foreach (var modelDescriptor in modelPNTTBDescriptorArray)
                allTrianglesCountPNTTB += modelDescriptor.Model.TotalTriangleCount;

            foreach (var modelDescriptor in modelPNDescriptorArray)
                allTrianglesCountPN += modelDescriptor.Model.TotalTriangleCount;

            foreach (var modelDescriptor in modelPTDescriptorArray)
                allTrianglesCountPT += modelDescriptor.Model.TotalTriangleCount;

            foreach (var modelDescriptor in modelPCDescriptorArray)
                allTrianglesCountPC += modelDescriptor.Model.TotalTriangleCount;

            var allBVHTrianglesPNTTB = new IndexedTriangleEngine<VertexPositionNormalTextureTangentBitangent>[allTrianglesCountPNTTB];
            var allBVHTrianglesPN = new IndexedTriangleEngine<VertexPositionNormal>[allTrianglesCountPN];
            var allBVHTrianglesPT = new IndexedTriangleEngine<VertexPositionTexture>[allTrianglesCountPT];
            var allBVHTrianglesPC = new IndexedTriangleEngine<VertexPositionColor>[allTrianglesCountPC];

            var indexPNTTB = 0;
            foreach (var modelDescriptor in modelPNTTBDescriptorArray)
            {
                var model = modelDescriptor.Model;
                var meshCount = model.MeshCount;
                for (int i = 0; i < meshCount; i++)
                {

                    var mesh = model.GetMesh(i);
                    var triangles = mesh.Triangles;
                    triangles.CopyTo(allBVHTrianglesPNTTB, indexPNTTB);
                    indexPNTTB += mesh.TriangleCount;
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

                    var mesh = model.GetMesh(i);
                    var triangles = mesh.Triangles;
                    triangles.CopyTo(allBVHTrianglesPN, indexPN);
                    indexPN += mesh.TriangleCount;
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

                    var mesh = model.GetMesh(i);
                    var triangles = mesh.Triangles;
                    triangles.CopyTo(allBVHTrianglesPT, indexPT);
                    indexPT += mesh.TriangleCount;
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

                    var mesh = model.GetMesh(i);
                    var triangles = mesh.Triangles;
                    triangles.CopyTo(allBVHTrianglesPC, indexPC);
                    indexPC += mesh.TriangleCount;
                }
            }

            var tuplePNTTB  = BVHTreeBuilder<IndexedTriangleEngine<VertexPositionNormalTextureTangentBitangent>>.Build(allBVHTrianglesPNTTB, SplitMethods.SAH);
            BVHTree PNTTBTree = tuplePNTTB.Item1;
            _orderedPNTTB = tuplePNTTB.Item2;
            int PNTTBTotalNodes = tuplePNTTB.Item3;

            var tuplePN  = BVHTreeBuilder<IndexedTriangleEngine<VertexPositionNormal>>.Build(allBVHTrianglesPN, SplitMethods.SAH);
            BVHTree PNTree= tuplePN.Item1;
            IndexedTriangleEngine<VertexPositionNormal>[] PNOrdered = tuplePN.Item2;
            int PNTotalNodes = tuplePN.Item3;

            var tuplePT  = BVHTreeBuilder<IndexedTriangleEngine<VertexPositionTexture>>.Build(allBVHTrianglesPT, SplitMethods.SAH);
            BVHTree PTTree= tuplePT.Item1;
            IndexedTriangleEngine<VertexPositionTexture>[] PTOrdered = tuplePT.Item2;
            int PTTotalNodes = tuplePT.Item3;

            var tuplePC  = BVHTreeBuilder<IndexedTriangleEngine<VertexPositionColor>>.Build(allBVHTrianglesPC, SplitMethods.SAH);
            BVHTree PCTree= tuplePC.Item1;
            IndexedTriangleEngine<VertexPositionColor>[] PCOrdered = tuplePC.Item2;
            int PCTotalNodes = tuplePC.Item3;

            _bvhRuntimeNodesPNTTB = BVHRuntime.ConstructBVHRuntime(PNTTBTree, PNTTBTotalNodes);
            var bvhRuntimePN = BVHRuntime.ConstructBVHRuntime(PNTree, PNTotalNodes);
            var bvhRuntimePT = BVHRuntime.ConstructBVHRuntime(PTTree, PTTotalNodes);
            var bvhRuntimePC = BVHRuntime.ConstructBVHRuntime(PCTree, PCTotalNodes);
                                                                                                                                                              
        
        }
        
         protected void EnableBVHCulling(float deltaTime, GraphicsDevice graphicsDevice, CommandList commandList, Camera camera, ModelRuntimeDescriptor<VertexPositionNormalTextureTangentBitangent>[] modelPNTTBDescriptorArray, ModelRuntimeDescriptor<VertexPositionNormal>[] modelPNDescriptorArray, ModelRuntimeDescriptor<VertexPositionTexture>[] modelPTDescriptorArray, ModelRuntimeDescriptor<VertexPositionColor>[] modelPCDescriptorArray, ModelRuntimeDescriptor<VertexPosition>[] modelPDescriptorArray){

            var updateBuffers = false;
            if (updateBuffers) {
                commandList.Begin();

                foreach (var modelDescriptor in modelPNTTBDescriptorArray)
                {
                    var model = modelDescriptor.Model;
                    byte vertexSizeInBytes = model.GetMesh(0).Vertices[0].GetSizeInBytes();

                    var meshCount = model.MeshCount;
                    for (int i = 0; i < meshCount; i++){
                        var mesh = model.GetMesh(i);
                        mesh.ValidIndexCount = 0;
                        mesh.ValidVertexCount = 0;
                    }
                }
            }

            Culler.FrustumCullBVH(_bvhRuntimeNodesPNTTB, _orderedPNTTB, ref camera.ViewProjectionMatirx);

            if (updateBuffers) {

                foreach (var modelDescriptor in modelPNTTBDescriptorArray)
                {
                    var model = modelDescriptor.Model;
                    byte vertexSizeInBytes = model.GetMesh(0).Vertices[0].GetSizeInBytes();

                    var meshCount = model.MeshCount;
                    for (int i = 0; i < meshCount; i++){
                        var vertexBuffer = modelDescriptor.VertexBuffers[i];
                        var indexBuffer = modelDescriptor.IndexBuffers[i];
                        var mesh = model.GetMesh(i);

                            mesh.CleanIndices.CopyTo(mesh.ValidIndices, 0);
                            mesh.CleanVertices.CopyTo(mesh.ValidVertices, 0);

                            uint vertexBytesToCopy = (vertexSizeInBytes * mesh.ValidVertexCount).ToUnsigned();
                            uint indexBytesToCopy = (sizeof(ushort) * mesh.ValidIndexCount).ToUnsigned();
                            uint allIndexBytesToCopy = (sizeof(ushort) * mesh.IndexCount).ToUnsigned();
                            uint allVertexBytesToCopy = (vertexSizeInBytes * mesh.VertexCount).ToUnsigned();

                            commandList.UpdateBuffer<VertexPositionNormalTextureTangentBitangent>(vertexBuffer, 0, ref mesh.CleanVertices[0], allVertexBytesToCopy);
                            commandList.UpdateBuffer<ushort>(indexBuffer, 0, ref mesh.CleanIndices[0], allIndexBytesToCopy);

                            commandList.UpdateBuffer<VertexPositionNormalTextureTangentBitangent>(vertexBuffer, 0, ref mesh.ValidVertices[0], vertexBytesToCopy);
                            commandList.UpdateBuffer<ushort>(indexBuffer, 0, ref mesh.ValidIndices[0], indexBytesToCopy);
                        
                    }      
                }

                commandList.End();
                graphicsDevice.SubmitCommands(commandList);
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