using System;
using System.Runtime.CompilerServices;
using System.Numerics;
using Veldrid;
using Henzai.Extensions;
using Henzai.Geometry;
using Henzai.Core.Geometry;

namespace Henzai.Runtime
{
    //TODO: Refactor this class by vertex type
    public static class RenderCommandGenerator
    {


        /// <summary>
        /// Render Commands for Model of Type:
        /// <see cref="VertexStructs"/> which need light/material interactions
        ///</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void GenerateCommandsForModel_Inline<T>(
                                                    CommandList commandList, 
                                                    Pipeline pipeline,
                                                    DeviceBuffer cameraProjViewBuffer,
                                                    DeviceBuffer lightBuffer,
                                                    Camera camera,
                                                    Light light,
                                                    Model<T> model) where T : struct, VertexLocateable
                                                    {

                commandList.SetPipeline(pipeline);

                commandList.UpdateBuffer(cameraProjViewBuffer,0,camera.ViewMatrix);
                commandList.UpdateBuffer(cameraProjViewBuffer,64,camera.ProjectionMatrix);
                commandList.UpdateBuffer(lightBuffer,0,ref light.LightPos_DontMutate);
                commandList.UpdateBuffer(lightBuffer,16,ref light.Color_DontMutate);
                commandList.UpdateBuffer(lightBuffer,32,ref light.Attentuation_DontMutate);


        }

        /// <summary>
        /// Render Commands for Model of Type:
        /// <see cref="VertexStructs"/> which need light/material interactions
        ///</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void GenerateCommandsForModel_Inline<T>(
                                                    CommandList commandList, 
                                                    Pipeline pipeline,
                                                    DeviceBuffer cameraProjViewBuffer,
                                                    DeviceBuffer lightBuffer,
                                                    DeviceBuffer pointLightBuffer,
                                                    Camera camera,
                                                    Light light,
                                                    Light pointLight,
                                                    Model<T> model) where T : struct, VertexLocateable
                                                    {

                commandList.SetPipeline(pipeline);

                commandList.UpdateBuffer(cameraProjViewBuffer,0,camera.ViewMatrix);
                commandList.UpdateBuffer(cameraProjViewBuffer,64,camera.ProjectionMatrix);
                commandList.UpdateBuffer(lightBuffer,0,ref light.LightPos_DontMutate);
                commandList.UpdateBuffer(lightBuffer,16,ref light.Color_DontMutate);
                commandList.UpdateBuffer(lightBuffer,32,ref light.Attentuation_DontMutate);
                commandList.UpdateBuffer(pointLightBuffer,0,ref pointLight.LightPos_DontMutate);
                commandList.UpdateBuffer(pointLightBuffer,16,ref pointLight.Color_DontMutate);
                commandList.UpdateBuffer(pointLightBuffer,32,ref pointLight.Direction_DontMutate);
                commandList.UpdateBuffer(pointLightBuffer,48,ref pointLight.Parameters_DontMutate);


        }

        /// <summary>
        /// Render Commands for Model of Type:
        /// <see cref="VertexStructs"/> which only need to be displayed in 3D
        ///</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void GenerateCommandsForModel_Inline<T>(
                                                    CommandList commandList, 
                                                    Pipeline pipeline,
                                                    DeviceBuffer cameraProjViewBuffer,
                                                    Camera camera,
                                                    Model<T> model) where T : struct, VertexLocateable
                                                    {

                commandList.SetPipeline(pipeline);

                commandList.UpdateBuffer(cameraProjViewBuffer,0,camera.ViewMatrix);
                commandList.UpdateBuffer(cameraProjViewBuffer,64,camera.ProjectionMatrix);


        }

        /// <summary>
        /// Render Commands for Mesh of Type:
        /// <see cref="Henzai.Geometry.VertexPositionNormalTextureTangentBitangent"/> 
        ///</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void GenerateCommandsForMesh_Inline(
                                                    CommandList commandList, 
                                                    DeviceBuffer vertexBuffer, 
                                                    DeviceBuffer indexBuffer,
                                                    DeviceBuffer cameraProjViewBuffer,
                                                    DeviceBuffer materialBuffer,
                                                    ResourceSet cameraResourceSet,
                                                    ResourceSet lightResourceSet,
                                                    ResourceSet pointlightResourceSet,
                                                    ResourceSet materialResourceSet,
                                                    ResourceSet textureResourceSet,
                                                    Mesh<VertexPositionNormalTextureTangentBitangent> mesh,
                                                    uint modelInstanceCount)
                                                    {


            Material material = mesh.GetMaterialRuntime();

            commandList.SetVertexBuffer(0,vertexBuffer);
            commandList.SetIndexBuffer(indexBuffer,IndexFormat.UInt16);
            commandList.UpdateBuffer(cameraProjViewBuffer,128,mesh.World);
            commandList.SetGraphicsResourceSet(0,cameraResourceSet); // Always after SetPipeline
            commandList.SetGraphicsResourceSet(1,lightResourceSet);
            commandList.SetGraphicsResourceSet(2,pointlightResourceSet);
            commandList.UpdateBuffer(materialBuffer,0,material.diffuse);
            commandList.UpdateBuffer(materialBuffer,16,material.specular);
            commandList.UpdateBuffer(materialBuffer,32,material.ambient);
            commandList.UpdateBuffer(materialBuffer,48,material.coefficients);
            commandList.SetGraphicsResourceSet(3,materialResourceSet);
            commandList.SetGraphicsResourceSet(4,textureResourceSet);
            commandList.DrawIndexed(
                indexCount: mesh.MeshIndices.Length.ToUnsigned(),
                instanceCount: modelInstanceCount,
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
        private static void GenerateCommandsForMesh_Inline(
                                                    CommandList commandList, 
                                                    DeviceBuffer vertexBuffer, 
                                                    DeviceBuffer indexBuffer,
                                                    DeviceBuffer cameraProjViewBuffer,
                                                    DeviceBuffer materialBuffer,
                                                    ResourceSet cameraResourceSet,
                                                    ResourceSet lightResourceSet,
                                                    ResourceSet materialResourceSet,
                                                    Mesh<VertexPositionNormal> mesh,
                                                    uint modelInstanceCount)
                                                    {


            Material material = mesh.GetMaterialRuntime();

            commandList.SetVertexBuffer(0,vertexBuffer);
            commandList.SetIndexBuffer(indexBuffer,IndexFormat.UInt16);
            commandList.UpdateBuffer(cameraProjViewBuffer,128,mesh.World);
            commandList.SetGraphicsResourceSet(0,cameraResourceSet); // Always after SetPipeline
            commandList.SetGraphicsResourceSet(1,lightResourceSet);
            commandList.UpdateBuffer(materialBuffer,0,material.diffuse);
            commandList.UpdateBuffer(materialBuffer,16,material.specular);
            commandList.UpdateBuffer(materialBuffer,32,material.ambient);
            commandList.UpdateBuffer(materialBuffer,48,material.coefficients);
            commandList.SetGraphicsResourceSet(2,materialResourceSet);
            commandList.DrawIndexed(
                indexCount: mesh.MeshIndices.Length.ToUnsigned(),
                instanceCount: modelInstanceCount,
                indexStart: 0,
                vertexOffset: 0,
                instanceStart: 0
            );

        }

        /// <summary>
        /// Render Commands for Mesh of Type:
        /// <see cref="Henzai.Geometry.VertexPositionTexture"/> 
        ///</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void GenerateCommandsForMesh_Inline(
                                                    CommandList commandList, 
                                                    DeviceBuffer vertexBuffer, 
                                                    DeviceBuffer indexBuffer,
                                                    DeviceBuffer cameraProjViewBuffer,
                                                    ResourceSet cameraResourceSet,
                                                    ResourceSet textureResourceSet,
                                                    Mesh<VertexPositionTexture> mesh,
                                                    uint modelInstanceCount)
                                                    {


            Material material = mesh.GetMaterialRuntime();

            commandList.SetVertexBuffer(0,vertexBuffer);
            commandList.SetIndexBuffer(indexBuffer,IndexFormat.UInt16);
            commandList.UpdateBuffer(cameraProjViewBuffer,128,mesh.World);
            commandList.SetGraphicsResourceSet(0,cameraResourceSet); // Always after SetPipeline
            commandList.SetGraphicsResourceSet(1,textureResourceSet);
            commandList.DrawIndexed(
                indexCount: mesh.MeshIndices.Length.ToUnsigned(),
                instanceCount: modelInstanceCount,
                indexStart: 0,
                vertexOffset: 0,
                instanceStart: 0
            );

        }

        /// <summary>
        /// Render Commands for Mesh of Type:
        /// <see cref="Henzai.Geometry.VertexPositionColor"/> 
        ///</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void GenerateCommandsForMesh_Inline(
                                                    CommandList commandList, 
                                                    DeviceBuffer vertexBuffer, 
                                                    DeviceBuffer indexBuffer,
                                                    DeviceBuffer cameraProjViewBuffer,
                                                    ResourceSet cameraResourceSet,
                                                    Mesh<VertexPositionColor> mesh,
                                                    uint modelInstanceCount)
                                                    {


            Material material = mesh.GetMaterialRuntime();

            commandList.SetVertexBuffer(0,vertexBuffer);
            commandList.SetIndexBuffer(indexBuffer,IndexFormat.UInt16);
            commandList.UpdateBuffer(cameraProjViewBuffer,128,mesh.World);
            commandList.SetGraphicsResourceSet(0,cameraResourceSet); // Always after SetPipeline
            commandList.DrawIndexed(
                indexCount: mesh.MeshIndices.Length.ToUnsigned(),
                instanceCount: modelInstanceCount,
                indexStart: 0,
                vertexOffset: 0,
                instanceStart: 0
            );

        }

        /// <summary>
        // Only used for cube maps for now!
        /// Render Commands for Mesh of Type:
        /// <see cref="Henzai.Geometry.VertexPosition"/> 
        ///</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void GenerateCommandsForMesh_Inline(
                                                    CommandList commandList, 
                                                    DeviceBuffer vertexBuffer, 
                                                    DeviceBuffer indexBuffer,
                                                    DeviceBuffer cameraProjViewBuffer,
                                                    ResourceSet cameraResourceSet,
                                                    ResourceSet textureResourceSet,
                                                    Mesh<VertexPosition> mesh,
                                                    uint modelInstanceCount)
                                                    {


            Material material = mesh.GetMaterialRuntime();

            commandList.SetVertexBuffer(0,vertexBuffer);
            commandList.SetIndexBuffer(indexBuffer,IndexFormat.UInt16);
            commandList.UpdateBuffer(cameraProjViewBuffer,128,mesh.World);
            commandList.SetGraphicsResourceSet(0,cameraResourceSet); // Always after SetPipeline
            commandList.SetGraphicsResourceSet(1,textureResourceSet); 
            commandList.DrawIndexed(
                indexCount: mesh.MeshIndices.Length.ToUnsigned(),
                instanceCount: modelInstanceCount,
                indexStart: 0,
                vertexOffset: 0,
                instanceStart: 0
            );

        }

        /// <summary>
        /// Render Commands for Mesh of Type:
        /// <see cref="Henzai.Geometry.VertexPositionColor"/> 
        ///</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void GenerateCommandsForMesh_InlineInstancing(
                                                    CommandList commandList, 
                                                    DeviceBuffer vertexBuffer, 
                                                    DeviceBuffer indexBuffer,
                                                    DeviceBuffer instanceBuffer,
                                                    DeviceBuffer cameraProjViewBuffer,
                                                    ResourceSet cameraResourceSet,
                                                    Mesh<VertexPositionColor> mesh,
                                                    uint modelInstanceCount)
                                                    {


            Material material = mesh.GetMaterialRuntime();

            commandList.SetVertexBuffer(0,vertexBuffer);
            commandList.SetIndexBuffer(indexBuffer,IndexFormat.UInt16);
            commandList.UpdateBuffer(cameraProjViewBuffer,128,mesh.World);
            commandList.SetGraphicsResourceSet(0,cameraResourceSet); // Always after SetPipeline
            commandList.DrawIndexed(
                indexCount: mesh.MeshIndices.Length.ToUnsigned(),
                instanceCount: modelInstanceCount,
                indexStart: 0,
                vertexOffset: 0,
                instanceStart: 0
            );

        }

        public static void GenerateRenderCommandsForCubeMapModelDescriptor(CommandList commandList, 
                                                                    ModelRuntimeDescriptor<VertexPosition> cubeMapRuntimeDescriptor,
                                                                    SceneRuntimeDescriptor sceneRuntimeDescriptor){
            var model = cubeMapRuntimeDescriptor.Model;
            RenderCommandGenerator.GenerateCommandsForModel_Inline(
                commandList,
                cubeMapRuntimeDescriptor.Pipeline,
                sceneRuntimeDescriptor.CameraProjViewBuffer,
                sceneRuntimeDescriptor.Camera,
                model);  
            for(int i = 0; i < model.meshCount; i++){
                var mesh = model.meshes[i];
                RenderCommandGenerator.GenerateCommandsForMesh_Inline(
                    commandList,
                    cubeMapRuntimeDescriptor.VertexBuffers[i],
                    cubeMapRuntimeDescriptor.IndexBuffers[i],
                    sceneRuntimeDescriptor.CameraProjViewBuffer,
                    sceneRuntimeDescriptor.CameraResourceSet,
                    cubeMapRuntimeDescriptor.TextureResourceSets[i],
                    mesh,
                    cubeMapRuntimeDescriptor.TotalInstanceCount
                );
            }
            
        }

        public static void GenerateRenderCommandsForModelDescriptor(CommandList commandList, 
                                                                    ModelRuntimeDescriptor<VertexPositionNormal>[] descriptorArray,
                                                                    SceneRuntimeDescriptor sceneRuntimeDescriptor){
            for(int j = 0; j < descriptorArray.Length; j++){
                var modelState = descriptorArray[j];
                if(modelState.IsCulled)
                    continue;
                var model = modelState.Model;
                RenderCommandGenerator.GenerateCommandsForModel_Inline(
                    commandList,
                    modelState.Pipeline,
                    sceneRuntimeDescriptor.CameraProjViewBuffer,
                    sceneRuntimeDescriptor.LightBuffer,
                    sceneRuntimeDescriptor.Camera,
                    sceneRuntimeDescriptor.Light,
                    model);
                for(int i = 0; i < model.meshCount; i++){
                    var mesh = model.meshes[i];
                    if(mesh.IsCulled)
                        continue;
                    RenderCommandGenerator.GenerateCommandsForMesh_Inline(
                        commandList,
                        modelState.VertexBuffers[i],
                        modelState.IndexBuffers[i],
                        sceneRuntimeDescriptor.CameraProjViewBuffer,
                        sceneRuntimeDescriptor.MaterialBuffer,
                        sceneRuntimeDescriptor.CameraResourceSet,
                        sceneRuntimeDescriptor.LightResourceSet,
                        sceneRuntimeDescriptor.MaterialResourceSet,
                        mesh,
                        modelState.TotalInstanceCount
                    );
                }
            }
        }
     public static void GenerateRenderCommandsForModelDescriptor(CommandList commandList, 
                                                                    ModelRuntimeDescriptor<VertexPositionColor>[] descriptorArray,
                                                                    SceneRuntimeDescriptor sceneRuntimeDescriptor){
            for(int j = 0; j < descriptorArray.Length; j++){
                var modelState = descriptorArray[j];
                if(modelState.IsCulled)
                    continue;
                var model = modelState.Model;
                RenderCommandGenerator.GenerateCommandsForModel_Inline(
                    commandList,
                    modelState.Pipeline,
                    sceneRuntimeDescriptor.CameraProjViewBuffer,
                    sceneRuntimeDescriptor.Camera,
                    model);  
                for(int i = 0; i < model.meshCount; i++){
                    var mesh = model.meshes[i];
                    if(mesh.IsCulled)
                        continue;
                    RenderCommandGenerator.GenerateCommandsForMesh_Inline(
                        commandList,
                        modelState.VertexBuffers[i],
                        modelState.IndexBuffers[i],
                        sceneRuntimeDescriptor.CameraProjViewBuffer,
                        sceneRuntimeDescriptor.CameraResourceSet,
                        mesh,
                        modelState.TotalInstanceCount
                    );
                }
            }
        }

     public static void GenerateRenderCommandsForModelDescriptor_Instancing(CommandList commandList, 
                                                                    ModelRuntimeDescriptor<VertexPositionColor>[] descriptorArray,
                                                                    SceneRuntimeDescriptor sceneRuntimeDescriptor){
            for(int j = 0; j < descriptorArray.Length; j++){
                var modelState = descriptorArray[j];
                if(modelState.IsCulled)
                    continue;
                var model = modelState.Model;
                RenderCommandGenerator.GenerateCommandsForModel_Inline(
                    commandList,
                    modelState.Pipeline,
                    sceneRuntimeDescriptor.CameraProjViewBuffer,
                    sceneRuntimeDescriptor.Camera,
                    model);
                //TODO:Inline this if more instance buffers are ever used
                for(int i = 0; i<modelState.InstanceBuffers.Length; i++)
                    commandList.SetVertexBuffer(i.ToUnsigned()+1,modelState.InstanceBuffers[i]);
                for(int i = 0; i < model.meshCount; i++){
                    var mesh = model.meshes[i];
                    if(mesh.IsCulled)
                        continue;
                    RenderCommandGenerator.GenerateCommandsForMesh_Inline(
                        commandList,
                        modelState.VertexBuffers[i],
                        modelState.IndexBuffers[i],
                        sceneRuntimeDescriptor.CameraProjViewBuffer,
                        sceneRuntimeDescriptor.CameraResourceSet,
                        mesh,
                        modelState.TotalInstanceCount
                    );
                }
            }
        }


     public static void GenerateRenderCommandsForModelDescriptor(CommandList commandList, 
                                                                    ModelRuntimeDescriptor<VertexPositionTexture>[] descriptorArray,
                                                                    SceneRuntimeDescriptor sceneRuntimeDescriptor){
            for(int j = 0; j < descriptorArray.Length; j++){
                var modelState = descriptorArray[j];
                if(modelState.IsCulled)
                    continue;
                var model = modelState.Model;
                RenderCommandGenerator.GenerateCommandsForModel_Inline(
                    commandList,
                    modelState.Pipeline,
                    sceneRuntimeDescriptor.CameraProjViewBuffer,
                    sceneRuntimeDescriptor.Camera,
                    model);  
                for(int i = 0; i < model.meshCount; i++){
                    var mesh = model.meshes[i];
                    if(mesh.IsCulled)
                        continue;
                    RenderCommandGenerator.GenerateCommandsForMesh_Inline(
                        commandList,
                        modelState.VertexBuffers[i],
                        modelState.IndexBuffers[i],
                        sceneRuntimeDescriptor.CameraProjViewBuffer,
                        sceneRuntimeDescriptor.CameraResourceSet,
                        modelState.TextureResourceSets[i],
                        mesh,
                        modelState.TotalInstanceCount
                    );
                }
            }
        }


     public static void GenerateRenderCommandsForModelDescriptor_Instancing(CommandList commandList, 
                                                                    ModelRuntimeDescriptor<VertexPositionTexture>[] descriptorArray,
                                                                    SceneRuntimeDescriptor sceneRuntimeDescriptor){
            for(int j = 0; j < descriptorArray.Length; j++){
                var modelState = descriptorArray[j];
                if(modelState.IsCulled)
                    continue;
                var model = modelState.Model;
                RenderCommandGenerator.GenerateCommandsForModel_Inline(
                    commandList,
                    modelState.Pipeline,
                    sceneRuntimeDescriptor.CameraProjViewBuffer,
                    sceneRuntimeDescriptor.Camera,
                    model);
                //TODO:Inline this if more instance buffers are ever used
                for(int i = 0; i<modelState.InstanceBuffers.Length; i++)
                    commandList.SetVertexBuffer(i.ToUnsigned()+1,modelState.InstanceBuffers[i]);
                for(int i = 0; i < model.meshCount; i++){
                    var mesh = model.meshes[i];
                    if(mesh.IsCulled)
                        continue;
                    RenderCommandGenerator.GenerateCommandsForMesh_Inline(
                        commandList,
                        modelState.VertexBuffers[i],
                        modelState.IndexBuffers[i],
                        sceneRuntimeDescriptor.CameraProjViewBuffer,
                        sceneRuntimeDescriptor.CameraResourceSet,
                        modelState.TextureResourceSets[i],
                        mesh,
                        modelState.TotalInstanceCount
                    );
                }
            }
        }

        public static void GenerateRenderCommandsForModelDescriptor_Instancing(CommandList commandList, 
                                                                    ModelRuntimeDescriptor<VertexPositionNormalTextureTangentBitangent>[] descriptorArray,
                                                                    SceneRuntimeDescriptor sceneRuntimeDescriptor){
            for(int j = 0; j < descriptorArray.Length; j++){
                var modelState = descriptorArray[j];
                if(modelState.IsCulled)
                    continue;
                var model = modelState.Model;
                RenderCommandGenerator.GenerateCommandsForModel_Inline(
                    commandList,
                    modelState.Pipeline,
                    sceneRuntimeDescriptor.CameraProjViewBuffer,
                    sceneRuntimeDescriptor.LightBuffer,
                    sceneRuntimeDescriptor.SpotLightBuffer,
                    sceneRuntimeDescriptor.Camera,
                    sceneRuntimeDescriptor.Light,
                    sceneRuntimeDescriptor.SpotLight,
                    model);
                for(int i = 0; i<modelState.InstanceBuffers.Length; i++)
                    commandList.SetVertexBuffer(i.ToUnsigned()+1,modelState.InstanceBuffers[i]);
                for(int i = 0; i < model.meshCount; i++){
                    var mesh = model.meshes[i];
                    if(mesh.IsCulled)
                        continue;
                    RenderCommandGenerator.GenerateCommandsForMesh_Inline(
                        commandList,
                        modelState.VertexBuffers[i],
                        modelState.IndexBuffers[i],
                        sceneRuntimeDescriptor.CameraProjViewBuffer,
                        sceneRuntimeDescriptor.MaterialBuffer,
                        sceneRuntimeDescriptor.CameraResourceSet,
                        sceneRuntimeDescriptor.LightResourceSet,
                        sceneRuntimeDescriptor.SpotLightResourceSet,
                        sceneRuntimeDescriptor.MaterialResourceSet,
                        modelState.TextureResourceSets[i],
                        mesh,
                        modelState.TotalInstanceCount
                    );
                }
            }
        }


        public static void GenerateRenderCommandsForModelDescriptor(CommandList commandList, 
                                                                    ModelRuntimeDescriptor<VertexPositionNormalTextureTangentBitangent>[] descriptorArray,
                                                                    SceneRuntimeDescriptor sceneRuntimeDescriptor){
            for(int j = 0; j < descriptorArray.Length; j++){
                var modelState = descriptorArray[j];
                if(modelState.IsCulled)
                    continue;
                var model = modelState.Model;
                RenderCommandGenerator.GenerateCommandsForModel_Inline(
                    commandList,
                    modelState.Pipeline,
                    sceneRuntimeDescriptor.CameraProjViewBuffer,
                    sceneRuntimeDescriptor.LightBuffer,
                    sceneRuntimeDescriptor.SpotLightBuffer,
                    sceneRuntimeDescriptor.Camera,
                    sceneRuntimeDescriptor.Light,
                    sceneRuntimeDescriptor.SpotLight,
                    model);
                for(int i = 0; i < model.meshCount; i++){
                    var mesh = model.meshes[i];
                    if(mesh.IsCulled)
                        continue;
                    RenderCommandGenerator.GenerateCommandsForMesh_Inline(
                        commandList,
                        modelState.VertexBuffers[i],
                        modelState.IndexBuffers[i],
                        sceneRuntimeDescriptor.CameraProjViewBuffer,
                        sceneRuntimeDescriptor.MaterialBuffer,
                        sceneRuntimeDescriptor.CameraResourceSet,
                        sceneRuntimeDescriptor.LightResourceSet,
                        sceneRuntimeDescriptor.SpotLightResourceSet,
                        sceneRuntimeDescriptor.MaterialResourceSet,
                        modelState.TextureResourceSets[i],
                        mesh,
                        modelState.TotalInstanceCount
                    );
                }
            }
        }

    }
}