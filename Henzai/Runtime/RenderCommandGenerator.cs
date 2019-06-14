using System.Runtime.CompilerServices;
using Veldrid;
using Henzai.Core.Extensions;
using Henzai.Geometry;
using Henzai.Cameras;
using Henzai.Core;
using Henzai.Core.Materials;
using Henzai.Core.VertexGeometry;
using Henzai.Core.Acceleration;

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
        public static void GenerateCommandsForScene_Inline(
                                                    CommandList commandList,
                                                    DeviceBuffer cameraProjViewBuffer,
                                                    DeviceBuffer lightBuffer,
                                                    DeviceBuffer pointLightBuffer,
                                                    Camera camera,
                                                    Light light,
                                                    Light pointLight)
        {
            var lightPos = light.LightPos;
            var pointLightPos = pointLight.LightPos;
            commandList.UpdateBuffer(cameraProjViewBuffer, 0, camera.ViewMatrix);
            commandList.UpdateBuffer(cameraProjViewBuffer, 64, camera.ProjectionMatrix);
            commandList.UpdateBuffer(lightBuffer, 0, ref lightPos);
            commandList.UpdateBuffer(lightBuffer, 16, ref light.Color_DontMutate);
            commandList.UpdateBuffer(lightBuffer, 32, ref light.Attentuation_DontMutate);
            commandList.UpdateBuffer(pointLightBuffer, 0, ref pointLightPos);
            commandList.UpdateBuffer(pointLightBuffer, 16, ref pointLight.Color_DontMutate);
            commandList.UpdateBuffer(pointLightBuffer, 32, ref pointLight.Direction_DontMutate);
            commandList.UpdateBuffer(pointLightBuffer, 48, ref pointLight.Parameters_DontMutate);
        }

        /// <summary>
        /// Render Commands for Model of Type:
        /// <see cref="VertexStructs"/> which only need to be displayed in 3D
        ///</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GenerateCommandsForScene_Inline(
                                                    CommandList commandList,
                                                    DeviceBuffer cameraProjViewBuffer,
                                                    Camera camera)
        {
            commandList.UpdateBuffer(cameraProjViewBuffer, 0, camera.ViewMatrix);
            commandList.UpdateBuffer(cameraProjViewBuffer, 64, camera.ProjectionMatrix);
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
                                                    RealtimeMaterial material,
                                                    Mesh<VertexPositionNormalTextureTangentBitangent> mesh,
                                                    uint modelInstanceCount)
        {
            commandList.SetVertexBuffer(0, vertexBuffer);
            commandList.SetIndexBuffer(indexBuffer, IndexFormat.UInt16);
            commandList.UpdateBuffer(cameraProjViewBuffer, 128, mesh.World);
            commandList.SetGraphicsResourceSet(0, cameraResourceSet); // Always after SetPipeline
            commandList.SetGraphicsResourceSet(1, lightResourceSet);
            commandList.SetGraphicsResourceSet(2, pointlightResourceSet);
            commandList.UpdateBuffer(materialBuffer, 0, material.diffuse);
            commandList.UpdateBuffer(materialBuffer, 16, material.specular);
            commandList.UpdateBuffer(materialBuffer, 32, material.ambient);
            commandList.UpdateBuffer(materialBuffer, 48, material.coefficients);
            commandList.SetGraphicsResourceSet(3, materialResourceSet);
            commandList.SetGraphicsResourceSet(4, textureResourceSet);
            commandList.DrawIndexed(
                indexCount: mesh.Indices.Length.ToUnsigned(),
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
                                                    RealtimeMaterial material,
                                                    Mesh<VertexPositionNormal> mesh,
                                                    uint modelInstanceCount)
        {
            commandList.SetVertexBuffer(0, vertexBuffer);
            commandList.SetIndexBuffer(indexBuffer, IndexFormat.UInt16);
            commandList.UpdateBuffer(cameraProjViewBuffer, 128, mesh.World);
            commandList.SetGraphicsResourceSet(0, cameraResourceSet); // Always after SetPipeline
            commandList.SetGraphicsResourceSet(1, lightResourceSet);
            commandList.UpdateBuffer(materialBuffer, 0, material.diffuse);
            commandList.UpdateBuffer(materialBuffer, 16, material.specular);
            commandList.UpdateBuffer(materialBuffer, 32, material.ambient);
            commandList.UpdateBuffer(materialBuffer, 48, material.coefficients);
            commandList.SetGraphicsResourceSet(2, materialResourceSet);
            commandList.DrawIndexed(
                indexCount: mesh.Indices.Length.ToUnsigned(),
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
        private static void GenerateCommandsForMesh_Inline<T>(
                                                    CommandList commandList,
                                                    DeviceBuffer vertexBuffer,
                                                    DeviceBuffer indexBuffer,
                                                    DeviceBuffer cameraProjViewBuffer,
                                                    ResourceSet cameraResourceSet,
                                                    ResourceSet textureResourceSet,
                                                    Mesh<VertexPositionTexture> mesh,
                                                    uint modelInstanceCount) where T : struct, VertexLocateable
        {
            commandList.SetVertexBuffer(0, vertexBuffer);
            commandList.SetIndexBuffer(indexBuffer, IndexFormat.UInt16);
            commandList.UpdateBuffer(cameraProjViewBuffer, 128, mesh.World);
            commandList.SetGraphicsResourceSet(0, cameraResourceSet); // Always after SetPipeline
            commandList.SetGraphicsResourceSet(1, textureResourceSet);
            commandList.DrawIndexed(
                indexCount: mesh.Indices.Length.ToUnsigned(),
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
        private static void GenerateCommandsForMesh_Inline<T>(
                                                    CommandList commandList,
                                                    DeviceBuffer vertexBuffer,
                                                    DeviceBuffer indexBuffer,
                                                    DeviceBuffer cameraProjViewBuffer,
                                                    ResourceSet cameraResourceSet,
                                                    Mesh<T> mesh,
                                                    uint modelInstanceCount) where T : struct, VertexLocateable
        {
            commandList.SetVertexBuffer(0, vertexBuffer);
            commandList.SetIndexBuffer(indexBuffer, IndexFormat.UInt16);
            commandList.UpdateBuffer(cameraProjViewBuffer, 128, mesh.World);
            commandList.SetGraphicsResourceSet(0, cameraResourceSet); // Always after SetPipeline
            commandList.DrawIndexed(
                indexCount: mesh.Indices.Length.ToUnsigned(),
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
            commandList.SetVertexBuffer(0, vertexBuffer);
            commandList.SetIndexBuffer(indexBuffer, IndexFormat.UInt16);
            commandList.UpdateBuffer(cameraProjViewBuffer, 128, mesh.World);
            commandList.SetGraphicsResourceSet(0, cameraResourceSet); // Always after SetPipeline
            commandList.SetGraphicsResourceSet(1, textureResourceSet);
            commandList.DrawIndexed(
                indexCount: mesh.Indices.Length.ToUnsigned(),
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
            commandList.SetVertexBuffer(0, vertexBuffer);
            commandList.SetIndexBuffer(indexBuffer, IndexFormat.UInt16);
            commandList.UpdateBuffer(cameraProjViewBuffer, 128, mesh.World);
            commandList.SetGraphicsResourceSet(0, cameraResourceSet); // Always after SetPipeline
            commandList.DrawIndexed(
                indexCount: mesh.Indices.Length.ToUnsigned(),
                instanceCount: modelInstanceCount,
                indexStart: 0,
                vertexOffset: 0,
                instanceStart: 0
            );

        }

        public static void GenerateRenderCommandsForCubeMapModelDescriptor(CommandList commandList,
                                                                    ModelRuntimeDescriptor<VertexPosition> cubeMapRuntimeDescriptor,
                                                                    SceneRuntimeDescriptor sceneRuntimeDescriptor,
                                                                    PipelineTypes piplelineType)
        {
            var model = cubeMapRuntimeDescriptor.Model;
            switch(piplelineType){
                case PipelineTypes.Normal:
                    commandList.SetPipeline(cubeMapRuntimeDescriptor.Pipeline);
                    break;
                case PipelineTypes.ShadowMap:
                    commandList.SetPipeline(cubeMapRuntimeDescriptor.ShadowMapPipeline);
                    break;
            }
            for (int i = 0; i < model.MeshCount; i++)
            {
                var mesh = model.GetMesh(i);
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
                                                                    SceneRuntimeDescriptor sceneRuntimeDescriptor,
                                                                    PipelineTypes piplelineType)
        {
            for (int j = 0; j < descriptorArray.Length; j++)
            {
                var modelState = descriptorArray[j];
                var model = modelState.Model;
                switch(piplelineType){
                    case PipelineTypes.Normal:
                        commandList.SetPipeline(modelState.Pipeline);
                        break;
                    case PipelineTypes.ShadowMap:
                        commandList.SetPipeline(modelState.ShadowMapPipeline);
                        break;
                }
                for (int i = 0; i < model.MeshCount; i++)
                {
                    if (!model.GetMeshBVH(i).AABBIsValid)
                        continue;
                    var mesh = model.GetMesh(i);
                    var material = model.GetMaterial(i);
                    RenderCommandGenerator.GenerateCommandsForMesh_Inline(
                        commandList,
                        modelState.VertexBuffers[i],
                        modelState.IndexBuffers[i],
                        sceneRuntimeDescriptor.CameraProjViewBuffer,
                        sceneRuntimeDescriptor.MaterialBuffer,
                        sceneRuntimeDescriptor.CameraResourceSet,
                        sceneRuntimeDescriptor.LightResourceSet,
                        sceneRuntimeDescriptor.MaterialResourceSet,
                        material,
                        mesh,
                        modelState.TotalInstanceCount
                    );
                }
            }
        }
        public static void GenerateRenderCommandsForModelDescriptor<T>(CommandList commandList,
                                                                       ModelRuntimeDescriptor<T>[] descriptorArray,
                                                                       SceneRuntimeDescriptor sceneRuntimeDescriptor,
                                                                       PipelineTypes piplelineType) where T : struct, VertexLocateable
        {
            for (int j = 0; j < descriptorArray.Length; j++)
            {
                var modelState = descriptorArray[j];
                switch(piplelineType){
                    case PipelineTypes.Normal:
                        commandList.SetPipeline(modelState.Pipeline);
                        break;
                    case PipelineTypes.ShadowMap:
                        commandList.SetPipeline(modelState.ShadowMapPipeline);
                        break;
                }
                var model = modelState.Model;
                for (int i = 0; i < model.MeshCount; i++)
                {
                    if (!model.GetMeshBVH(i).AABBIsValid)
                        continue;
                    var mesh = model.GetMesh(i);
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
                                                                       SceneRuntimeDescriptor sceneRuntimeDescriptor,
                                                                       PipelineTypes piplelineType)
        {
            for (int j = 0; j < descriptorArray.Length; j++)
            {
                var modelState = descriptorArray[j];
                switch(piplelineType){
                    case PipelineTypes.Normal:
                        commandList.SetPipeline(modelState.Pipeline);
                        break;
                    case PipelineTypes.ShadowMap:
                        commandList.SetPipeline(modelState.ShadowMapPipeline);
                        break;
                }
                var model = modelState.Model;
                //TODO:Inline this if more instance buffers are ever used
                for (int i = 0; i < modelState.InstanceBuffers.Length; i++)
                    commandList.SetVertexBuffer(i.ToUnsigned() + 1, modelState.InstanceBuffers[i]);
                for (int i = 0; i < model.MeshCount; i++)
                {
                    if (!model.GetMeshBVH(i).AABBIsValid)
                        continue;
                    var mesh = model.GetMesh(i);
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
                                                                       SceneRuntimeDescriptor sceneRuntimeDescriptor,
                                                                       PipelineTypes piplelineType)
        {
            for (int j = 0; j < descriptorArray.Length; j++)
            {
                var modelState = descriptorArray[j];
                switch(piplelineType){
                    case PipelineTypes.Normal:
                        commandList.SetPipeline(modelState.Pipeline);
                        break;
                    case PipelineTypes.ShadowMap:
                        commandList.SetPipeline(modelState.ShadowMapPipeline);
                        break;
                }
                var model = modelState.Model;

                for (int i = 0; i < model.MeshCount; i++)
                {
                    if (!model.GetMeshBVH(i).AABBIsValid)
                        continue;
                    var mesh = model.GetMesh(i);
                    RenderCommandGenerator.GenerateCommandsForMesh_Inline<VertexPositionTexture>(
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
                                                                       SceneRuntimeDescriptor sceneRuntimeDescriptor,
                                                                       PipelineTypes piplelineType)
        {
            for (int j = 0; j < descriptorArray.Length; j++)
            {
                var modelState = descriptorArray[j];
                switch(piplelineType){
                    case PipelineTypes.Normal:
                        commandList.SetPipeline(modelState.Pipeline);
                        break;
                    case PipelineTypes.ShadowMap:
                        commandList.SetPipeline(modelState.ShadowMapPipeline);
                        break;
                }
                var model = modelState.Model;
                //TODO:Inline this if more instance buffers are ever used
                for (int i = 0; i < modelState.InstanceBuffers.Length; i++)
                    commandList.SetVertexBuffer(i.ToUnsigned() + 1, modelState.InstanceBuffers[i]);
                for (int i = 0; i < model.MeshCount; i++)
                {
                    if (!model.GetMeshBVH(i).AABBIsValid)
                        continue;
                    var mesh = model.GetMesh(i);
                    RenderCommandGenerator.GenerateCommandsForMesh_Inline<VertexPositionTexture>(
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
                                                                    SceneRuntimeDescriptor sceneRuntimeDescriptor,
                                                                    PipelineTypes piplelineType)
        {
            for (int j = 0; j < descriptorArray.Length; j++)
            {
                var modelState = descriptorArray[j];
                switch(piplelineType){
                    case PipelineTypes.Normal:
                        commandList.SetPipeline(modelState.Pipeline);
                        break;
                    case PipelineTypes.ShadowMap:
                        commandList.SetPipeline(modelState.ShadowMapPipeline);
                        break;
                }
                var model = modelState.Model;
                for (int i = 0; i < modelState.InstanceBuffers.Length; i++)
                    commandList.SetVertexBuffer(i.ToUnsigned() + 1, modelState.InstanceBuffers[i]);
                for (int i = 0; i < model.MeshCount; i++)
                {
                    if (!model.GetMeshBVH(i).AABBIsValid)
                        continue;
                    var mesh = model.GetMesh(i);
                    var material = model.GetMaterial(i);
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
                        material,
                        mesh,
                        modelState.TotalInstanceCount
                    );
                }
            }
        }


        public static void GenerateRenderCommandsForModelDescriptor(CommandList commandList,
                                                                    ModelRuntimeDescriptor<VertexPositionNormalTextureTangentBitangent>[] descriptorArray,
                                                                    SceneRuntimeDescriptor sceneRuntimeDescriptor,
                                                                    PipelineTypes piplelineType)
        {
            for (int j = 0; j < descriptorArray.Length; j++)
            {
                var modelState = descriptorArray[j];
                var model = modelState.Model;
                switch(piplelineType){
                    case PipelineTypes.Normal:
                        commandList.SetPipeline(modelState.Pipeline);
                        break;
                    case PipelineTypes.ShadowMap:
                        commandList.SetPipeline(modelState.ShadowMapPipeline);
                        break;
                }
                for (int i = 0; i < model.MeshCount; i++)
                {
                    if (!model.GetMeshBVH(i).AABBIsValid)
                        continue;
                    var mesh = model.GetMesh(i);
                    var material = model.GetMaterial(i);
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
                        material,
                        mesh,
                        modelState.TotalInstanceCount
                    );
                }
            }
        }

        public static void GenerateRenderCommandsForModelDescriptor(CommandList commandList,
                                                                    ModelRuntimeDescriptor<VertexPositionNormalTextureTangentBitangent>[] descriptorArray,
                                                                    SceneRuntimeDescriptor sceneRuntimeDescriptor,
                                                                    MeshBVH<VertexPositionNormalTextureTangentBitangent>[] meshBVHArray,
                                                                    PipelineTypes piplelineType)
        {
            var meshCount = meshBVHArray.Length;
            for(int j = 0; j < meshCount; j++){
                var meshBVH = meshBVHArray[j];
                if(!meshBVH.AABBIsValid)
                    continue;
                var modelStateIndex = meshBVH.ModelRuntimeIndex;
                var meshIndex = meshBVH.MeshRuntimeIndex;
                var modelState = descriptorArray[modelStateIndex];
                var model = modelState.Model;
                switch(piplelineType){
                    case PipelineTypes.Normal:
                        commandList.SetPipeline(modelState.Pipeline);
                        break;
                    case PipelineTypes.ShadowMap:
                        commandList.SetPipeline(modelState.ShadowMapPipeline);
                        break;
                }

                var mesh = model.GetMesh(j);
                var material = model.GetMaterial(meshIndex);
                RenderCommandGenerator.GenerateCommandsForMesh_Inline(
                    commandList,
                    modelState.VertexBuffers[meshIndex],
                    modelState.IndexBuffers[meshIndex],
                    sceneRuntimeDescriptor.CameraProjViewBuffer,
                    sceneRuntimeDescriptor.MaterialBuffer,
                    sceneRuntimeDescriptor.CameraResourceSet,
                    sceneRuntimeDescriptor.LightResourceSet,
                    sceneRuntimeDescriptor.SpotLightResourceSet,
                    sceneRuntimeDescriptor.MaterialResourceSet,
                    modelState.TextureResourceSets[meshIndex],
                    material,
                    mesh,
                    modelState.TotalInstanceCount
                );
                

            }

        }

    }
}