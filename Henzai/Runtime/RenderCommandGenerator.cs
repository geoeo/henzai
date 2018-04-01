using System;
using System.Runtime.CompilerServices;
using System.Numerics;
using Veldrid;
using Henzai.Extensions;
using Henzai.Geometry;

namespace Henzai.Runtime
{
    public static class RenderCommandGenerator
    {


        /// <summary>
        /// Render Commands for Model of Type:
        /// <see cref="VertexStructs"/> 
        ///</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GenerateCommandsForModel_Inline<T>(
                                                    CommandList commandList, 
                                                    Pipeline pipeline,
                                                    DeviceBuffer cameraProjViewBuffer,
                                                    DeviceBuffer lightBuffer,
                                                    Camera camera,
                                                    ref Vector4 lightPos,
                                                    Model<T> model) where T : struct
                                                    {

                commandList.SetPipeline(pipeline);

                commandList.UpdateBuffer(cameraProjViewBuffer,0,camera.ViewMatrix);
                commandList.UpdateBuffer(cameraProjViewBuffer,64,camera.ProjectionMatrix);
                commandList.UpdateBuffer(lightBuffer,0,lightPos);


        }

        /// <summary>
        /// Render Commands for Mesh of Type:
        /// <see cref="Henzai.Geometry.VertexPositionNormalTextureTangentBitangent"/> 
        ///</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GenerateCommandsForMesh_Inline(
                                                    CommandList commandList, 
                                                    DeviceBuffer vertexBuffer, 
                                                    DeviceBuffer indexBuffer,
                                                    DeviceBuffer cameraProjViewBuffer,
                                                    DeviceBuffer materialBuffer,
                                                    ResourceSet cameraResourceSet,
                                                    ResourceSet lightResourceSet,
                                                    ResourceSet materialResourceSet,
                                                    ResourceSet textureResourceSet,
                                                    Mesh<VertexPositionNormalTextureTangentBitangent> mesh)
                                                    {


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

        /// <summary>
        /// Render Commands for Mesh of Type:
        /// <see cref="Henzai.Geometry.VertexPositionNormal"/> 
        ///</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GenerateCommandsForMesh_Inline(
                                                    CommandList commandList, 
                                                    DeviceBuffer vertexBuffer, 
                                                    DeviceBuffer indexBuffer,
                                                    DeviceBuffer cameraProjViewBuffer,
                                                    DeviceBuffer materialBuffer,
                                                    ResourceSet cameraResourceSet,
                                                    ResourceSet lightResourceSet,
                                                    ResourceSet materialResourceSet,
                                                    ResourceSet textureResourceSet,
                                                    Mesh<VertexPositionNormal> mesh)
                                                    {


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
            commandList.DrawIndexed(
                indexCount: mesh.meshIndices.Length.ToUnsigned(),
                instanceCount: 1,
                indexStart: 0,
                vertexOffset: 0,
                instanceStart: 0
            );

        }

        public static void GenerateRenderCommandsForModelDescriptor(CommandList commandList, 
                                                                    ModelRuntimeDescriptor<VertexPositionNormal>[] descriptorArray,
                                                                    SceneRuntimeDescriptor sceneRuntimeDescriptor){
            for(int j = 0; j < descriptorArray.Length; j++){
                var modelState = descriptorArray[j];
                var model = modelState.Model;
                RenderCommandGenerator.GenerateCommandsForModel_Inline(
                    commandList,
                    modelState.Pipeline,
                    sceneRuntimeDescriptor.CameraProjViewBuffer,
                    sceneRuntimeDescriptor.LightBuffer,
                    sceneRuntimeDescriptor.Camera,
                    ref sceneRuntimeDescriptor.Light.Light_DontMutate,
                    model);
                for(int i = 0; i < model.meshCount; i++){
                    var mesh = model.meshes[i];
                    RenderCommandGenerator.GenerateCommandsForMesh_Inline(
                        commandList,
                        modelState.VertexBuffers[i],
                        modelState.IndexBuffers[i],
                        sceneRuntimeDescriptor.CameraProjViewBuffer,
                        sceneRuntimeDescriptor.MaterialBuffer,
                        sceneRuntimeDescriptor.CameraResourceSet,
                        sceneRuntimeDescriptor.LightResourceSet,
                        sceneRuntimeDescriptor.MaterialResourceSet,
                        modelState.TextureResourceSets[i],
                        mesh
                    );
                }
            }
        }

        public static void GenerateRenderCommandsForModelDescriptor(CommandList commandList, 
                                                                    ModelRuntimeDescriptor<VertexPositionNormalTextureTangentBitangent>[] descriptorArray,
                                                                    SceneRuntimeDescriptor sceneRuntimeDescriptor){
            for(int j = 0; j < descriptorArray.Length; j++){
                var modelState = descriptorArray[j];
                var model = modelState.Model;
                RenderCommandGenerator.GenerateCommandsForModel_Inline(
                    commandList,
                    modelState.Pipeline,
                    sceneRuntimeDescriptor.CameraProjViewBuffer,
                    sceneRuntimeDescriptor.LightBuffer,
                    sceneRuntimeDescriptor.Camera,
                    ref sceneRuntimeDescriptor.Light.Light_DontMutate,
                    model);
                for(int i = 0; i < model.meshCount; i++){
                    var mesh = model.meshes[i];
                    RenderCommandGenerator.GenerateCommandsForMesh_Inline(
                        commandList,
                        modelState.VertexBuffers[i],
                        modelState.IndexBuffers[i],
                        sceneRuntimeDescriptor.CameraProjViewBuffer,
                        sceneRuntimeDescriptor.MaterialBuffer,
                        sceneRuntimeDescriptor.CameraResourceSet,
                        sceneRuntimeDescriptor.LightResourceSet,
                        sceneRuntimeDescriptor.MaterialResourceSet,
                        modelState.TextureResourceSets[i],
                        mesh
                    );
                }
            }
        }

    }
}