using System;
using Veldrid;
using Veldrid.Sdl2;
using Henzai;
using Henzai.Extensions;
using Henzai.UI;
using Henzai.Runtime;
using Henzai.Geometry;
using Henzai.Core.Acceleration;

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

        //TODO: Way too slow and buggy
        protected void EnableCulling(float deltaTime, CommandList commandList, Camera camera, ModelRuntimeDescriptor<VertexPositionNormalTextureTangentBitangent>[] modelPNTTBDescriptorArray,ModelRuntimeDescriptor<VertexPositionNormal>[] modelPNDescriptorArray,ModelRuntimeDescriptor<VertexPositionTexture>[] modelPTDescriptorArray, ModelRuntimeDescriptor<VertexPositionColor>[] modelPCDescriptorArray, ModelRuntimeDescriptor<VertexPosition>[] modelPDescriptorArray){

            foreach(var modelDescriptor in modelPNTTBDescriptorArray){
                var model = modelDescriptor.Model;
                var vertexBuffer = modelDescriptor.VertexBuffers[0];
                var indexBuffer = modelDescriptor.IndexBuffers[0];
                byte vertexSizeInBytes = model.meshes[0].Vertices[0].GetSizeInBytes();

                foreach(var mesh in model.meshes){
                    var worldMatrix = mesh.World;
                    var MVP = worldMatrix*camera.ViewProjectionMatirx;
                    MeshCuller.FrustumCullGeometryDefinition(ref MVP, mesh.GeometryDefinition); 
                    commandList.UpdateBuffer<VertexPositionNormalTextureTangentBitangent>(vertexBuffer,0,ref mesh.Vertices[0], (vertexSizeInBytes* mesh.NumberOfValidVertices).ToUnsigned());     
                    commandList.UpdateBuffer<ushort>(indexBuffer,0,ref mesh.MeshIndices[0], (sizeof(ushort)*mesh.NumberOfValidIndices).ToUnsigned());
                }
            }

            foreach(var modelDescriptor in modelPNDescriptorArray){
                var model = modelDescriptor.Model;
                var vertexBuffer = modelDescriptor.VertexBuffers[0];
                var indexBuffer = modelDescriptor.IndexBuffers[0];                
                byte vertexSizeInBytes = model.meshes[0].Vertices[0].GetSizeInBytes();

                foreach(var mesh in model.meshes){
                    var worldMatrix = mesh.World;
                    var MVP = worldMatrix*camera.ViewProjectionMatirx;
                    MeshCuller.FrustumCullGeometryDefinition(ref MVP, mesh.GeometryDefinition); 
                    commandList.UpdateBuffer<VertexPositionNormal>(vertexBuffer,0,ref mesh.Vertices[0], (vertexSizeInBytes* mesh.NumberOfValidVertices).ToUnsigned());     
                    commandList.UpdateBuffer<ushort>(indexBuffer,0,ref mesh.MeshIndices[0], (sizeof(ushort)*mesh.NumberOfValidIndices).ToUnsigned());
                }             
            }

            foreach(var modelDescriptor in modelPTDescriptorArray){
                var model = modelDescriptor.Model;
                var vertexBuffer = modelDescriptor.VertexBuffers[0];
                var indexBuffer = modelDescriptor.IndexBuffers[0];
                byte vertexSizeInBytes = model.meshes[0].Vertices[0].GetSizeInBytes();

                foreach(var mesh in model.meshes){
                    var worldMatrix = mesh.World;
                    var MVP = worldMatrix*camera.ViewProjectionMatirx;                   
                    MeshCuller.FrustumCullGeometryDefinition(ref MVP, mesh.GeometryDefinition); 
                    commandList.UpdateBuffer<VertexPositionTexture>(vertexBuffer,0,ref mesh.Vertices[0], (vertexSizeInBytes* mesh.NumberOfValidVertices).ToUnsigned());     
                    commandList.UpdateBuffer<ushort>(indexBuffer,0,ref mesh.MeshIndices[0], (sizeof(ushort)*mesh.NumberOfValidIndices).ToUnsigned());
                }               
            }

            foreach(var modelDescriptor in modelPCDescriptorArray){
                var model = modelDescriptor.Model;
                var vertexBuffer = modelDescriptor.VertexBuffers[0];
                var indexBuffer = modelDescriptor.IndexBuffers[0];
                byte vertexSizeInBytes = model.meshes[0].Vertices[0].GetSizeInBytes();

                foreach(var mesh in model.meshes){
                    var worldMatrix = mesh.World;
                    var MVP = worldMatrix*camera.ViewProjectionMatirx;
                    MeshCuller.FrustumCullGeometryDefinition(ref MVP, mesh.GeometryDefinition); 
                    commandList.UpdateBuffer<VertexPositionColor>(vertexBuffer,0,ref mesh.Vertices[0], (vertexSizeInBytes* mesh.NumberOfValidVertices).ToUnsigned());     
                    commandList.UpdateBuffer<ushort>(indexBuffer,0,ref mesh.MeshIndices[0], (sizeof(ushort)*mesh.NumberOfValidIndices).ToUnsigned());
                }              
            }

            foreach(var modelDescriptor in modelPDescriptorArray){
                var model = modelDescriptor.Model;
                var vertexBuffer = modelDescriptor.VertexBuffers[0];
                var indexBuffer = modelDescriptor.IndexBuffers[0];
                byte vertexSizeInBytes = model.meshes[0].Vertices[0].GetSizeInBytes();

                foreach(var mesh in model.meshes){
                    var worldMatrix = mesh.World;
                    var MVP = worldMatrix*camera.ViewProjectionMatirx;
                    MeshCuller.FrustumCullGeometryDefinition(ref MVP, mesh.GeometryDefinition); 
                    commandList.UpdateBuffer<VertexPosition>(vertexBuffer,0,ref mesh.Vertices[0], (vertexSizeInBytes* mesh.NumberOfValidVertices).ToUnsigned());     
                    commandList.UpdateBuffer<ushort>(indexBuffer,0,ref mesh.MeshIndices[0], (sizeof(ushort)*mesh.NumberOfValidIndices).ToUnsigned());
                }               
            }
        }

    }
}