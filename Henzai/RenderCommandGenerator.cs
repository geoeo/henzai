using System;
using System.Runtime.CompilerServices;
using System.Numerics;
using Veldrid;
using Henzai.Extensions;
using Henzai.Geometry;

namespace Henzai
{
    public static class RenderCommandGenerator_Inline
    {


        /// <summary>
        /// Render Commands for Model of Type:
        /// <see cref="VertexPositionNormalTextureTangentBitangent"/> 
        ///</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GenerateCommandsForModelPNTTB(
                                                    CommandList commandList, 
                                                    Pipeline pipeline,
                                                    DeviceBuffer cameraProjViewBuffer,
                                                    DeviceBuffer lightBuffer,
                                                    Camera camera,
                                                    ref Vector4 lightPos,
                                                    Model<VertexPositionNormalTextureTangentBitangent> model){

                commandList.SetPipeline(pipeline);

                commandList.UpdateBuffer(cameraProjViewBuffer,0,camera.ViewMatrix);
                commandList.UpdateBuffer(cameraProjViewBuffer,64,camera.ProjectionMatrix);
                commandList.UpdateBuffer(lightBuffer,0,lightPos);


        }


        /// <summary>
        /// Render Commands for Mesh of Type:
        /// <see cref="VertexPositionNormalTextureTangentBitangent"/> 
        ///</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GenerateCommandsForMeshPNTTB(
                                                    CommandList commandList, 
                                                    DeviceBuffer vertexBuffer, 
                                                    DeviceBuffer indexBuffer,
                                                    DeviceBuffer cameraProjViewBuffer,
                                                    DeviceBuffer materialBuffer,
                                                    ResourceSet cameraResourceSet,
                                                    ResourceSet lightResourceSet,
                                                    ResourceSet materialResourceSet,
                                                    ResourceSet textureResourceSet,
                                                    Mesh<VertexPositionNormalTextureTangentBitangent> mesh){


            Material material = mesh.GetMaterialRuntime();

            commandList.SetVertexBuffer(0,vertexBuffer);
            commandList.SetIndexBuffer(indexBuffer,IndexFormat.UInt32);
            commandList.UpdateBuffer(cameraProjViewBuffer,128,mesh.World);
            commandList.SetGraphicsResourceSet(0,cameraResourceSet); // Always after SetPipeline
            commandList.SetGraphicsResourceSet(1,lightResourceSet);
            commandList.UpdateBuffer(materialBuffer,0,material.diffuse);
            commandList.UpdateBuffer(materialBuffer,16,material.specular);
            commandList.UpdateBuffer(materialBuffer,32,material.ambient);
            commandList.UpdateBuffer(materialBuffer,48,material.coefficients);
            commandList.SetGraphicsResourceSet(2,materialResourceSet);
            commandList.SetGraphicsResourceSet(3,textureResourceSet);
            commandList.DrawIndexed(
                indexCount: mesh.meshIndices.Length.ToUnsigned(),
                instanceCount: 1,
                indexStart: 0,
                vertexOffset: 0,
                instanceStart: 0
            );

        }

    }
}