using Veldrid;
using Veldrid.Sdl2;
using HenzaiFunc.Core.Acceleration;
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

        public abstract void createScene(GraphicsBackend graphicsBackend, Sdl2Window contextWindow = null);

        public void ChangeBackend(GraphicsBackend graphicsBackend){
            Sdl2Window contextWindow = scene.ContextWindow;
            scene.Dispose();
            createScene(graphicsBackend,contextWindow);
        }

        //TODO: Way too slow, needs bounding boxes
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
                    uint indexBytesToCopy = (sizeof(ushort)*mesh.ValidIndicesCount).ToUnsigned();

                    commandList.UpdateBuffer<VertexPositionNormalTextureTangentBitangent>(vertexBuffer,0,ref mesh.Vertices[0], vertexBytesToCopy);     
                    commandList.UpdateBuffer<ushort>(indexBuffer,0,ref mesh.Indices[0], indexBytesToCopy);
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
                    uint indexBytesToCopy = (sizeof(ushort)*mesh.ValidIndicesCount).ToUnsigned();

                    commandList.UpdateBuffer<VertexPositionNormal>(vertexBuffer,0,ref mesh.Vertices[0], vertexBytesToCopy);     
                    commandList.UpdateBuffer<ushort>(indexBuffer,0,ref mesh.Indices[0], indexBytesToCopy);
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
                    uint indexBytesToCopy = (sizeof(ushort)*mesh.ValidIndicesCount).ToUnsigned();

                    commandList.UpdateBuffer<VertexPositionTexture>(vertexBuffer,0,ref mesh.Vertices[0], vertexBytesToCopy);     
                    commandList.UpdateBuffer<ushort>(indexBuffer,0,ref mesh.Indices[0], indexBytesToCopy);
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
                    uint indexBytesToCopy = (sizeof(ushort)*mesh.ValidIndicesCount).ToUnsigned();

                    commandList.UpdateBuffer<VertexPositionColor>(vertexBuffer,0,ref mesh.Vertices[0], vertexBytesToCopy);     
                    commandList.UpdateBuffer<ushort>(indexBuffer,0,ref mesh.Indices[0], indexBytesToCopy);
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
                    uint indexBytesToCopy = (sizeof(ushort)*mesh.ValidIndicesCount).ToUnsigned();

                    commandList.UpdateBuffer<VertexPosition>(vertexBuffer,0,ref mesh.Vertices[0], vertexBytesToCopy);     
                    commandList.UpdateBuffer<ushort>(indexBuffer,0,ref mesh.Indices[0], indexBytesToCopy);
                }               
            }

            commandList.End();
        }

    }
}