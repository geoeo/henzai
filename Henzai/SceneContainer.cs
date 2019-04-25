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
        private BVHTree _bvhTree;
        private VertexPositionNormalTextureTangentBitangent[] _PNTTBOrdered;


        public abstract void createScene(GraphicsBackend graphicsBackend, Sdl2Window contextWindow = null);

        public void ChangeBackend(GraphicsBackend graphicsBackend)
        {
            Sdl2Window contextWindow = scene.ContextWindow;
            scene.Dispose();
            createScene(graphicsBackend, contextWindow);
        }

        protected void BuildBVH(ModelRuntimeDescriptor<VertexPositionNormalTextureTangentBitangent>[] modelPNTTBDescriptorArray, ModelRuntimeDescriptor<VertexPositionNormal>[] modelPNDescriptorArray, ModelRuntimeDescriptor<VertexPositionTexture>[] modelPTDescriptorArray, ModelRuntimeDescriptor<VertexPositionColor>[] modelPCDescriptorArray, ModelRuntimeDescriptor<VertexPosition>[] modelPDescriptorArray)
        {
            var allTrianglesCount = 0;
            foreach (var modelDescriptor in modelPNTTBDescriptorArray)
                allTrianglesCount += modelDescriptor.Model.TotalTriangleCount;

            var allBVHTriangles = new IndexedTriangleEngine<VertexPositionNormalTextureTangentBitangent>[allTrianglesCount];

            var index = 0;
            foreach (var modelDescriptor in modelPNTTBDescriptorArray)
            {
                var model = modelDescriptor.Model;
                var meshCount = model.MeshCount;
                for (int i = 0; i < meshCount; i++)
                {

                    var mesh = model.GetMesh(i);
                    var triangles = mesh.Triangles;
                    triangles.CopyTo(allBVHTriangles, index);
                    index += mesh.TriangleCount;
                }
            }

            var bvhTriangles = modelPNTTBDescriptorArray[0].Model.GetMesh(0).Triangles;
            var t = BVHTreeBuilder<IndexedTriangleEngine<VertexPositionNormalTextureTangentBitangent>>.Build(bvhTriangles, SplitMethods.SAH);
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