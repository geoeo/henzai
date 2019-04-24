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
            createScene(graphicsBackend, contextWindow);
        }

        protected void BuildBVH(ModelRuntimeDescriptor<VertexPositionNormalTextureTangentBitangent>[] modelPNTTBDescriptorArray,ModelRuntimeDescriptor<VertexPositionNormal>[] modelPNDescriptorArray,ModelRuntimeDescriptor<VertexPositionTexture>[] modelPTDescriptorArray, ModelRuntimeDescriptor<VertexPositionColor>[] modelPCDescriptorArray, ModelRuntimeDescriptor<VertexPosition>[] modelPDescriptorArray){
            var allTrianglesCount = 0;
            foreach(var modelDescriptor in modelPNTTBDescriptorArray)
                allTrianglesCount += modelDescriptor.Model.TotalTriangleCount;

            var allBVHTriangles = new IndexedTriangleEngine<VertexPositionNormalTextureTangentBitangent>[allTrianglesCount];

            var index = 0;
            foreach(var modelDescriptor in modelPNTTBDescriptorArray){
                var model = modelDescriptor.Model;
                var meshCount = model.MeshCount;
                for(int i = 0; i < meshCount; i++){
                    
                    var mesh = model.GetMesh(i);
                    var triangles = mesh.Triangles;
                    triangles.CopyTo(allBVHTriangles,index);
                    index += mesh.TriangleCount;
                }
            }

            var bvhTriangles = modelPNTTBDescriptorArray[0].Model.GetMesh(0).Triangles;
            var t = BVHTreeBuilder<IndexedTriangleEngine<VertexPositionNormalTextureTangentBitangent>>.Build(bvhTriangles, SplitMethods.SAH);
        }

        // Very slow
        // Still buggy
        protected void EnableCulling(float deltaTime, GraphicsDevice graphicsDevice, CommandList commandList, Camera camera, ModelRuntimeDescriptor<VertexPositionNormalTextureTangentBitangent>[] modelPNTTBDescriptorArray,ModelRuntimeDescriptor<VertexPositionNormal>[] modelPNDescriptorArray,ModelRuntimeDescriptor<VertexPositionTexture>[] modelPTDescriptorArray, ModelRuntimeDescriptor<VertexPositionColor>[] modelPCDescriptorArray, ModelRuntimeDescriptor<VertexPosition>[] modelPDescriptorArray){

            commandList.Begin();

            foreach(var modelDescriptor in modelPNTTBDescriptorArray){
                var model = modelDescriptor.Model;
                byte vertexSizeInBytes = model.GetMesh(0).Vertices[0].GetSizeInBytes();

                var meshCount = model.MeshCount;
                for(int i = 0; i < meshCount; i++)
                {
                    var vertexBuffer = modelDescriptor.VertexBuffers[i];
                    var indexBuffer = modelDescriptor.IndexBuffers[i];
                    var mesh = model.GetMesh(i);
                    var worldMatrix = mesh.World;
                    var indexClean = new ushort[mesh.IndexCount];
                    var vertexClean = new VertexPositionNormalTextureTangentBitangent[mesh.VertexCount];
                    indexClean.CopyTo(mesh.ValidIndices,0);
                    vertexClean.CopyTo(mesh.ValidVertices,0);

                    var MVP = worldMatrix*camera.ViewProjectionMatirx;
                    Culler.FrustumCullMesh(ref MVP, mesh);
                    
                    uint vertexBytesToCopy = (vertexSizeInBytes* mesh.ValidVertexCount).ToUnsigned();
                    uint indexBytesToCopy = (sizeof(ushort)*mesh.ValidIndexCount).ToUnsigned();
                    uint allIndexBytesToCopy = (sizeof(ushort)*mesh.IndexCount).ToUnsigned();
                    uint allVertexBytesToCopy = (vertexSizeInBytes*mesh.VertexCount).ToUnsigned();

                    commandList.UpdateBuffer<VertexPositionNormalTextureTangentBitangent>(vertexBuffer,0,ref vertexClean[0], allVertexBytesToCopy);     
                    commandList.UpdateBuffer<ushort>(indexBuffer,0,ref indexClean[0], allIndexBytesToCopy);

                    commandList.UpdateBuffer<VertexPositionNormalTextureTangentBitangent>(vertexBuffer,0,ref mesh.ValidVertices[0], vertexBytesToCopy);     
                    commandList.UpdateBuffer<ushort>(indexBuffer,0,ref mesh.ValidIndices[0], indexBytesToCopy);
                }
            }

            foreach(var modelDescriptor in modelPNDescriptorArray){
                var model = modelDescriptor.Model;              
                byte vertexSizeInBytes = model.GetMesh(0).Vertices[0].GetSizeInBytes();

                var meshCount = model.MeshCount;
                for (int i = 0; i < meshCount; i++)
                {
                    var vertexBuffer = modelDescriptor.VertexBuffers[i];
                    var indexBuffer = modelDescriptor.IndexBuffers[i];
                    var mesh = model.GetMesh(i);
                    var worldMatrix = mesh.World;
                    var indexClean = new ushort[mesh.IndexCount];
                    var vertexClean = new VertexPositionNormal[mesh.VertexCount];
                    indexClean.CopyTo(mesh.ValidIndices,0);
                    vertexClean.CopyTo(mesh.ValidVertices,0);

                    var MVP = worldMatrix*camera.ViewProjectionMatirx;
                    Culler.FrustumCullMesh(ref MVP, mesh); 
                    
                    uint vertexBytesToCopy = (vertexSizeInBytes* mesh.ValidVertexCount).ToUnsigned();
                    uint indexBytesToCopy = (sizeof(ushort)*mesh.ValidIndexCount).ToUnsigned();
                    uint allIndexBytesToCopy = (sizeof(ushort)*mesh.IndexCount).ToUnsigned();
                    uint allVertexBytesToCopy = (vertexSizeInBytes*mesh.VertexCount).ToUnsigned();

                    commandList.UpdateBuffer<VertexPositionNormal>(vertexBuffer,0,ref vertexClean[0], allVertexBytesToCopy);     
                    commandList.UpdateBuffer<ushort>(indexBuffer,0,ref indexClean[0], allIndexBytesToCopy);

                    commandList.UpdateBuffer<VertexPositionNormal>(vertexBuffer,0,ref mesh.ValidVertices[0], vertexBytesToCopy);     
                    commandList.UpdateBuffer<ushort>(indexBuffer,0,ref mesh.ValidIndices[0], indexBytesToCopy);
                }             
            }

            foreach(var modelDescriptor in modelPTDescriptorArray){
                var model = modelDescriptor.Model;
                byte vertexSizeInBytes = model.GetMesh(0).Vertices[0].GetSizeInBytes();

                var meshCount = model.MeshCount;
                for (int i = 0; i < meshCount; i++)
                {
                    var vertexBuffer = modelDescriptor.VertexBuffers[i];
                    var indexBuffer = modelDescriptor.IndexBuffers[i];
                    var mesh = model.GetMesh(i);
                    var indexClean = new ushort[mesh.IndexCount];
                    var vertexClean = new VertexPositionTexture[mesh.VertexCount];
                    indexClean.CopyTo(mesh.ValidIndices,0);
                    vertexClean.CopyTo(mesh.ValidVertices,0);

                    var worldMatrix = mesh.World;
                    var MVP = worldMatrix*camera.ViewProjectionMatirx;               
                    Culler.FrustumCullMesh(ref MVP, mesh); 

                    uint vertexBytesToCopy = (vertexSizeInBytes* mesh.ValidVertexCount).ToUnsigned();
                    uint indexBytesToCopy = (sizeof(ushort)*mesh.ValidIndexCount).ToUnsigned();
                    uint allIndexBytesToCopy = (sizeof(ushort)*mesh.IndexCount).ToUnsigned();
                    uint allVertexBytesToCopy = (vertexSizeInBytes*mesh.VertexCount).ToUnsigned();

                    commandList.UpdateBuffer<VertexPositionTexture>(vertexBuffer,0,ref vertexClean[0], allVertexBytesToCopy);     
                    commandList.UpdateBuffer<ushort>(indexBuffer,0,ref indexClean[0], allIndexBytesToCopy);

                    commandList.UpdateBuffer<VertexPositionTexture>(vertexBuffer,0,ref mesh.ValidVertices[0], vertexBytesToCopy);     
                    commandList.UpdateBuffer<ushort>(indexBuffer,0,ref mesh.ValidIndices[0], indexBytesToCopy);
                }               
            }

            foreach(var modelDescriptor in modelPCDescriptorArray){
                var model = modelDescriptor.Model;
                byte vertexSizeInBytes = model.GetMesh(0).Vertices[0].GetSizeInBytes();

                var meshCount = model.MeshCount;
                for (int i = 0; i < meshCount; i++)
                {
                    var vertexBuffer = modelDescriptor.VertexBuffers[i];
                    var indexBuffer = modelDescriptor.IndexBuffers[i];
                    var mesh = model.GetMesh(i);
                    var indexClean = new ushort[mesh.IndexCount];
                    var vertexClean = new VertexPositionColor[mesh.VertexCount];
                    indexClean.CopyTo(mesh.ValidIndices,0);
                    vertexClean.CopyTo(mesh.ValidVertices,0);

                    var worldMatrix = mesh.World;
                    var MVP = worldMatrix*camera.ViewProjectionMatirx;
                    Culler.FrustumCullMesh(ref MVP, mesh); 

                    uint vertexBytesToCopy = (vertexSizeInBytes* mesh.ValidVertexCount).ToUnsigned();
                    uint indexBytesToCopy = (sizeof(ushort)*mesh.ValidIndexCount).ToUnsigned();
                    uint allIndexBytesToCopy = (sizeof(ushort)*mesh.IndexCount).ToUnsigned();
                    uint allVertexBytesToCopy = (vertexSizeInBytes*mesh.VertexCount).ToUnsigned();

                    commandList.UpdateBuffer<VertexPositionColor>(vertexBuffer,0,ref vertexClean[0], allVertexBytesToCopy);     
                    commandList.UpdateBuffer<ushort>(indexBuffer,0,ref indexClean[0], allIndexBytesToCopy);

                    commandList.UpdateBuffer<VertexPositionColor>(vertexBuffer,0,ref mesh.ValidVertices[0], vertexBytesToCopy);     
                    commandList.UpdateBuffer<ushort>(indexBuffer,0,ref mesh.ValidIndices[0], indexBytesToCopy);
                }              
            }

            /// Dont cull VertexPosition as Skyboxes are VP and geometry is set via tricks in fragment shader
            // foreach(var modelDescriptor in modelPDescriptorArray){
            //     var model = modelDescriptor.Model;
            //     byte vertexSizeInBytes = model.GetMesh(0).Vertices[0].GetSizeInBytes();

            //     var meshCount = model.MeshCount;
            //     for (int i = 0; i < meshCount; i++)
            //     {
            //         var vertexBuffer = modelDescriptor.VertexBuffers[i];
            //         var indexBuffer = modelDescriptor.IndexBuffers[i];
            //         var mesh = model.GetMesh(i);
            //         var indexClean = new ushort[mesh.IndexCount];
            //         var vertexClean = new VertexPosition[mesh.VertexCount];
            //         indexClean.CopyTo(mesh.ValidIndices,0);
            //         vertexClean.CopyTo(mesh.ValidVertices,0);

            //         var worldMatrix = mesh.World;
            //         var MVP = worldMatrix*camera.ViewProjectionMatirx;
            //         Culler.FrustumCullMesh(ref MVP, mesh); 

            //         uint vertexBytesToCopy = (vertexSizeInBytes* mesh.ValidVertexCount).ToUnsigned();
            //         uint indexBytesToCopy = (sizeof(ushort)*mesh.ValidIndexCount).ToUnsigned();
            //         uint allIndexBytesToCopy = (sizeof(ushort)*mesh.IndexCount).ToUnsigned();
            //         uint allVertexBytesToCopy = (vertexSizeInBytes*mesh.VertexCount).ToUnsigned();

            //         commandList.UpdateBuffer<VertexPosition>(vertexBuffer,0,ref vertexClean[0], allVertexBytesToCopy);     
            //         commandList.UpdateBuffer<ushort>(indexBuffer,0,ref indexClean[0], allIndexBytesToCopy);

            //         commandList.UpdateBuffer<VertexPosition>(vertexBuffer,0,ref mesh.ValidVertices[0], vertexBytesToCopy);     
            //         commandList.UpdateBuffer<ushort>(indexBuffer,0,ref mesh.ValidIndices[0], indexBytesToCopy);
            //     }               
            // }

            commandList.End();
            graphicsDevice.SubmitCommands(commandList);
    }

    }
}