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

        //TODO: Way too slow
            protected void EnableCulling(float deltaTime, GraphicsDevice graphicsDevice, CommandList commandList, Camera camera, ModelRuntimeDescriptor<VertexPositionNormalTextureTangentBitangent>[] modelPNTTBDescriptorArray,ModelRuntimeDescriptor<VertexPositionNormal>[] modelPNDescriptorArray,ModelRuntimeDescriptor<VertexPositionTexture>[] modelPTDescriptorArray, ModelRuntimeDescriptor<VertexPositionColor>[] modelPCDescriptorArray, ModelRuntimeDescriptor<VertexPosition>[] modelPDescriptorArray){

            foreach(var modelDescriptor in modelPNTTBDescriptorArray){
                var model = modelDescriptor.Model;
                var vertexBuffer = modelDescriptor.VertexBuffers[0];
                var indexBuffer = modelDescriptor.IndexBuffers[0];
                byte vertexSizeInBytes = model.meshes[0].Vertices[0].GetSizeInBytes();

                commandList.Begin();

                foreach(var mesh in model.meshes){
                    var worldMatrix = mesh.World;
                    var MVP = worldMatrix*camera.ViewProjectionMatirx;
                    MeshCuller.FrustumCullGeometryDefinition(ref MVP, mesh.GeometryDefinition);
                    uint bytesToCopy = (vertexSizeInBytes* mesh.NumberOfValidVertices).ToUnsigned();

                    commandList.UpdateBuffer<VertexPositionNormalTextureTangentBitangent>(vertexBuffer,0,ref mesh.Vertices[0], bytesToCopy);     
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
                    uint bytesToCopy = (vertexSizeInBytes* mesh.NumberOfValidVertices).ToUnsigned();

                    commandList.UpdateBuffer<VertexPositionNormal>(vertexBuffer,0,ref mesh.Vertices[0], bytesToCopy);     
                    commandList.UpdateBuffer<ushort>(indexBuffer,0,ref mesh.MeshIndices[0], bytesToCopy);
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
                    uint bytesToCopy = (vertexSizeInBytes* mesh.NumberOfValidVertices).ToUnsigned();

                    commandList.UpdateBuffer<VertexPositionTexture>(vertexBuffer,0,ref mesh.Vertices[0], bytesToCopy);     
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
                    uint bytesToCopy = (vertexSizeInBytes* mesh.NumberOfValidVertices).ToUnsigned();

                    commandList.UpdateBuffer<VertexPositionColor>(vertexBuffer,0,ref mesh.Vertices[0], bytesToCopy);     
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
                    uint bytesToCopy = (vertexSizeInBytes* mesh.NumberOfValidVertices).ToUnsigned();

                    commandList.UpdateBuffer<VertexPosition>(vertexBuffer,0,ref mesh.Vertices[0], bytesToCopy);     
                    commandList.UpdateBuffer<ushort>(indexBuffer,0,ref mesh.MeshIndices[0], (sizeof(ushort)*mesh.NumberOfValidIndices).ToUnsigned());
                }               
            }

            commandList.End();
        }

        protected void EnableCulling(float deltaTime, Camera camera, ModelRuntimeDescriptor<VertexPositionNormalTextureTangentBitangent>[] modelPNTTBDescriptorArray,ModelRuntimeDescriptor<VertexPositionNormal>[] modelPNDescriptorArray,ModelRuntimeDescriptor<VertexPositionTexture>[] modelPTDescriptorArray, ModelRuntimeDescriptor<VertexPositionColor>[] modelPCDescriptorArray, ModelRuntimeDescriptor<VertexPosition>[] modelPDescriptorArray){

            foreach(var modelDescriptor in modelPNTTBDescriptorArray){
                var model = modelDescriptor.Model;
                var vertexBuffer = modelDescriptor.VertexBuffers[0];
                var indexBuffer = modelDescriptor.IndexBuffers[0];
                byte vertexSizeInBytes = model.meshes[0].Vertices[0].GetSizeInBytes();

                foreach(var mesh in model.meshes){
                    var worldMatrix = mesh.World;
                    var MVP = worldMatrix*camera.ViewProjectionMatirx;

                    mesh.IsCulled = MeshCuller.IsGeometryDefinitionCulled(ref MVP, mesh.GeometryDefinition);
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

                    mesh.IsCulled = MeshCuller.IsGeometryDefinitionCulled(ref MVP, mesh.GeometryDefinition);
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

                    mesh.IsCulled = MeshCuller.IsGeometryDefinitionCulled(ref MVP, mesh.GeometryDefinition);
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

                    mesh.IsCulled = MeshCuller.IsGeometryDefinitionCulled(ref MVP, mesh.GeometryDefinition);
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

                    mesh.IsCulled = MeshCuller.IsGeometryDefinitionCulled(ref MVP, mesh.GeometryDefinition);
                }               
            }
        }

    }
}