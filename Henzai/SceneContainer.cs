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
        protected static UserInterface  gui;
        private BVHTree _bvhTree;
        private VertexPositionNormalTextureTangentBitangent[] _PNTTBOrdered;


        public abstract void createScene(GraphicsBackend graphicsBackend, Sdl2Window contextWindow = null);

        public void ChangeBackend(GraphicsBackend graphicsBackend){
            Sdl2Window contextWindow = scene.ContextWindow;
            scene.Dispose();
            createScene(graphicsBackend,contextWindow);
        }

        protected void BuildBVH(ModelRuntimeDescriptor<VertexPositionNormalTextureTangentBitangent>[] modelPNTTBDescriptorArray,ModelRuntimeDescriptor<VertexPositionNormal>[] modelPNDescriptorArray,ModelRuntimeDescriptor<VertexPositionTexture>[] modelPTDescriptorArray, ModelRuntimeDescriptor<VertexPositionColor>[] modelPCDescriptorArray, ModelRuntimeDescriptor<VertexPosition>[] modelPDescriptorArray){
            var bvhTriangles = modelPNTTBDescriptorArray[0].Model.GetMesh(0).Triangles;
            var t = BVHTreeBuilder<IndexedTriangleEngine<VertexPositionNormalTextureTangentBitangent>>.Build(bvhTriangles, SplitMethods.SAH);
        }


        protected void EnableCulling(float deltaTime, GraphicsDevice graphicsDevice, CommandList commandList, Camera camera, ModelRuntimeDescriptor<VertexPositionNormalTextureTangentBitangent>[] modelPNTTBDescriptorArray,ModelRuntimeDescriptor<VertexPositionNormal>[] modelPNDescriptorArray,ModelRuntimeDescriptor<VertexPositionTexture>[] modelPTDescriptorArray, ModelRuntimeDescriptor<VertexPositionColor>[] modelPCDescriptorArray, ModelRuntimeDescriptor<VertexPosition>[] modelPDescriptorArray){

            foreach(var modelDescriptor in modelPNTTBDescriptorArray){
                var model = modelDescriptor.Model;
                var vertexBuffer = modelDescriptor.VertexBuffers[0];
                var indexBuffer = modelDescriptor.IndexBuffers[0];
                byte vertexSizeInBytes = model.GetMesh(0).Vertices[0].GetSizeInBytes();

                commandList.Begin();

                var meshCount = model.MeshCount;
                for(int i = 0; i < meshCount; i++)
                {
                    var mesh = model.GetMesh(i);
                    var worldMatrix = mesh.World;
                    var MVP = worldMatrix*camera.ViewProjectionMatirx;
                    Culler.FrustumCullMesh(ref MVP, mesh);
                    uint vertexBytesToCopy = (vertexSizeInBytes* mesh.ValidVertexCount).ToUnsigned();
                    uint indexBytesToCopy = (sizeof(ushort)*mesh.ValidIndexCount).ToUnsigned();

                    commandList.UpdateBuffer<VertexPositionNormalTextureTangentBitangent>(vertexBuffer,0,ref mesh.ValidVertices[0], vertexBytesToCopy);     
                    commandList.UpdateBuffer<ushort>(indexBuffer,0,ref mesh.ValidIndices[0], indexBytesToCopy);
                }
            }

            foreach(var modelDescriptor in modelPNDescriptorArray){
                var model = modelDescriptor.Model;
                var vertexBuffer = modelDescriptor.VertexBuffers[0];
                var indexBuffer = modelDescriptor.IndexBuffers[0];                
                byte vertexSizeInBytes = model.GetMesh(0).Vertices[0].GetSizeInBytes();

                var meshCount = model.MeshCount;
                for (int i = 0; i < meshCount; i++)
                {
                    var mesh = model.GetMesh(i);
                    var worldMatrix = mesh.World;
                    var MVP = worldMatrix*camera.ViewProjectionMatirx;
                    Culler.FrustumCullMesh(ref MVP, mesh); 
                    uint vertexBytesToCopy = (vertexSizeInBytes* mesh.ValidVertexCount).ToUnsigned();
                    uint indexBytesToCopy = (sizeof(ushort)*mesh.ValidIndexCount).ToUnsigned();

                    commandList.UpdateBuffer<VertexPositionNormal>(vertexBuffer,0,ref mesh.ValidVertices[0], vertexBytesToCopy);     
                    commandList.UpdateBuffer<ushort>(indexBuffer,0,ref mesh.ValidIndices[0], indexBytesToCopy);
                }             
            }

            foreach(var modelDescriptor in modelPTDescriptorArray){
                var model = modelDescriptor.Model;
                var vertexBuffer = modelDescriptor.VertexBuffers[0];
                var indexBuffer = modelDescriptor.IndexBuffers[0];
                byte vertexSizeInBytes = model.GetMesh(0).Vertices[0].GetSizeInBytes();

                var meshCount = model.MeshCount;
                for (int i = 0; i < meshCount; i++)
                {
                    var mesh = model.GetMesh(i);
                    var worldMatrix = mesh.World;
                    var MVP = worldMatrix*camera.ViewProjectionMatirx;               
                    Culler.FrustumCullMesh(ref MVP, mesh); 
                    uint vertexBytesToCopy = (vertexSizeInBytes* mesh.ValidVertexCount).ToUnsigned();
                    uint indexBytesToCopy = (sizeof(ushort)*mesh.ValidIndexCount).ToUnsigned();

                    commandList.UpdateBuffer<VertexPositionTexture>(vertexBuffer,0,ref mesh.ValidVertices[0], vertexBytesToCopy);     
                    commandList.UpdateBuffer<ushort>(indexBuffer,0,ref mesh.ValidIndices[0], indexBytesToCopy);
                }               
            }

            foreach(var modelDescriptor in modelPCDescriptorArray){
                var model = modelDescriptor.Model;
                var vertexBuffer = modelDescriptor.VertexBuffers[0];
                var indexBuffer = modelDescriptor.IndexBuffers[0];
                byte vertexSizeInBytes = model.GetMesh(0).Vertices[0].GetSizeInBytes();

                var meshCount = model.MeshCount;
                for (int i = 0; i < meshCount; i++)
                {
                    var mesh = model.GetMesh(i);
                    var worldMatrix = mesh.World;
                    var MVP = worldMatrix*camera.ViewProjectionMatirx;
                    Culler.FrustumCullMesh(ref MVP, mesh); 
                    uint vertexBytesToCopy = (vertexSizeInBytes* mesh.ValidVertexCount).ToUnsigned();
                    uint indexBytesToCopy = (sizeof(ushort)*mesh.ValidIndexCount).ToUnsigned();

                    commandList.UpdateBuffer<VertexPositionColor>(vertexBuffer,0,ref mesh.ValidVertices[0], vertexBytesToCopy);     
                    commandList.UpdateBuffer<ushort>(indexBuffer,0,ref mesh.ValidIndices[0], indexBytesToCopy);
                }              
            }

            foreach(var modelDescriptor in modelPDescriptorArray){
                var model = modelDescriptor.Model;
                var vertexBuffer = modelDescriptor.VertexBuffers[0];
                var indexBuffer = modelDescriptor.IndexBuffers[0];
                byte vertexSizeInBytes = model.GetMesh(0).Vertices[0].GetSizeInBytes();

                var meshCount = model.MeshCount;
                for (int i = 0; i < meshCount; i++)
                {
                    var mesh = model.GetMesh(i);
                    var worldMatrix = mesh.World;
                    var MVP = worldMatrix*camera.ViewProjectionMatirx;
                    Culler.FrustumCullMesh(ref MVP, mesh); 
                    uint vertexBytesToCopy = (vertexSizeInBytes* mesh.ValidVertexCount).ToUnsigned();
                    uint indexBytesToCopy = (sizeof(ushort)*mesh.ValidIndexCount).ToUnsigned();

                    commandList.UpdateBuffer<VertexPosition>(vertexBuffer,0,ref mesh.ValidVertices[0], vertexBytesToCopy);     
                    commandList.UpdateBuffer<ushort>(indexBuffer,0,ref mesh.ValidIndices[0], indexBytesToCopy);
                }               
            }

            commandList.End();
    }

    }
}