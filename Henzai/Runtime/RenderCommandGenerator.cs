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
                                                    DeviceBuffer lightProjViewBuffer,
                                                    Camera camera,
                                                    Light light,
                                                    Light pointLight)
        {
            var lightPos = light.LightPos;
            var pointLightPos = pointLight.LightPos;
            commandList.UpdateBuffer(cameraProjViewBuffer, 0, camera.ViewMatrix);
            commandList.UpdateBuffer(cameraProjViewBuffer, 64, camera.ProjectionMatrix);
            //TODO: Maybe this is not necessary every frame
            commandList.UpdateBuffer(lightBuffer, 0, ref lightPos);
            commandList.UpdateBuffer(lightBuffer, 16, ref light.Color_DontMutate);
            commandList.UpdateBuffer(lightBuffer, 32, ref light.Attentuation_DontMutate);
            commandList.UpdateBuffer(pointLightBuffer, 0, ref pointLightPos);
            commandList.UpdateBuffer(pointLightBuffer, 16, ref pointLight.Color_DontMutate);
            commandList.UpdateBuffer(pointLightBuffer, 32, ref pointLight.Direction_DontMutate);
            commandList.UpdateBuffer(pointLightBuffer, 48, ref pointLight.Parameters_DontMutate);
            //TODO: Make this conditional on effects
            commandList.UpdateBuffer(lightProjViewBuffer, 0, light.LightCam.ViewProjectionMatirx);
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
                                                    ResourceSet[] effectResourceSets,
                                                    RealtimeMaterial material,
                                                    Mesh<VertexPositionNormalTextureTangentBitangent> mesh,
                                                    uint modelInstanceCount)
        {
            var effectIndex = 5;
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
            for(int i = 0; i < effectResourceSets.Length; i++){
                var resourceSet = effectResourceSets[i];
                var resourceSetIndex = effectIndex + i;
                commandList.SetGraphicsResourceSet((uint)resourceSetIndex, resourceSet);
            }
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
                                                    ResourceSet[] effectResourceSets,
                                                    RealtimeMaterial material,
                                                    Mesh<VertexPositionNormal> mesh,
                                                    uint modelInstanceCount)
        {
            var effectIndex = 3;
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
            for(int i = 0; i < effectResourceSets.Length; i++){
                var resourceSet = effectResourceSets[i];
                var resourceSetIndex = effectIndex + i;
                commandList.SetGraphicsResourceSet((uint)resourceSetIndex, resourceSet);
            }
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
                                                    ResourceSet[] effectResourceSets,
                                                    Mesh<VertexPositionTexture> mesh,
                                                    uint modelInstanceCount) where T : struct, VertexLocateable
        {
            var effectIndex = 2;
            commandList.SetVertexBuffer(0, vertexBuffer);
            commandList.SetIndexBuffer(indexBuffer, IndexFormat.UInt16);
            commandList.UpdateBuffer(cameraProjViewBuffer, 128, mesh.World);
            commandList.SetGraphicsResourceSet(0, cameraResourceSet); // Always after SetPipeline
            commandList.SetGraphicsResourceSet(1, textureResourceSet);
            for(int i = 0; i < effectResourceSets.Length; i++){
                var resourceSet = effectResourceSets[i];
                var resourceSetIndex = effectIndex + i;
                commandList.SetGraphicsResourceSet((uint)resourceSetIndex, resourceSet);
            }
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
                                                    ResourceSet[] effectResourceSets,
                                                    Mesh<T> mesh,
                                                    uint modelInstanceCount) where T : struct, VertexLocateable
        {
            var effectIndex = 1;
            commandList.SetVertexBuffer(0, vertexBuffer);
            commandList.SetIndexBuffer(indexBuffer, IndexFormat.UInt16);
            commandList.UpdateBuffer(cameraProjViewBuffer, 128, mesh.World);
            commandList.SetGraphicsResourceSet(0, cameraResourceSet); // Always after SetPipeline
            for(int i = 0; i < effectResourceSets.Length; i++){
                var resourceSet = effectResourceSets[i];
                var resourceSetIndex = effectIndex + i;
                commandList.SetGraphicsResourceSet((uint)resourceSetIndex, resourceSet);
            }
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
                                                    ResourceSet[] effectResourceSets,
                                                    Mesh<VertexPosition> mesh,
                                                    uint modelInstanceCount)
        {
            var effectIndex = 2;
            commandList.SetVertexBuffer(0, vertexBuffer);
            commandList.SetIndexBuffer(indexBuffer, IndexFormat.UInt16);
            commandList.UpdateBuffer(cameraProjViewBuffer, 128, mesh.World);
            commandList.SetGraphicsResourceSet(0, cameraResourceSet); // Always after SetPipeline
            commandList.SetGraphicsResourceSet(1, textureResourceSet);
            for(int i = 0; i < effectResourceSets.Length; i++){
                var resourceSet = effectResourceSets[i];
                var resourceSetIndex = effectIndex + i;
                commandList.SetGraphicsResourceSet((uint)resourceSetIndex, resourceSet);
            }
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
                                                                    SceneRuntimeDescriptor sceneRuntimeDescriptor)
        {
            var model = cubeMapRuntimeDescriptor.Model;
            commandList.SetPipeline(cubeMapRuntimeDescriptor.Pipeline);
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
                    cubeMapRuntimeDescriptor.EffectResourceSets,
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
                var renderFlags = modelState.RenderFlags;
                switch(piplelineType){
                    case PipelineTypes.Normal:
                    if((renderFlags & RenderFlags.NORMAL) == RenderFlags.NONE)
                            continue;
                        commandList.SetPipeline(modelState.Pipeline);
                        break;
                    case PipelineTypes.ShadowMap:
                        if((renderFlags & RenderFlags.SHADOW_MAP) == RenderFlags.NONE)
                            continue;
                        commandList.SetPipeline(modelState.ShadowMapPipeline);
                        break;
                }
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
                        sceneRuntimeDescriptor.MaterialResourceSet,
                        modelState.EffectResourceSets,
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
                var effectSets = sceneRuntimeDescriptor.NO_RESOURCE_SET;
                var renderFlags = modelState.RenderFlags;
                switch(piplelineType){
                    case PipelineTypes.Normal:
                        if((renderFlags & RenderFlags.NORMAL) == RenderFlags.NONE)
                            continue;
                        commandList.SetPipeline(modelState.Pipeline);
                        effectSets = modelState.EffectResourceSets;
                        break;
                    case PipelineTypes.ShadowMap:
                        if((renderFlags & RenderFlags.SHADOW_MAP) == RenderFlags.NONE)
                            continue;
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

                    RenderCommandGenerator.GenerateCommandsForMesh_Inline(
                        commandList,
                        modelState.VertexBuffers[i],
                        modelState.IndexBuffers[i],
                        sceneRuntimeDescriptor.CameraProjViewBuffer,
                        sceneRuntimeDescriptor.CameraResourceSet,
                        effectSets,
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
                var effectSets = sceneRuntimeDescriptor.NO_RESOURCE_SET;
                var renderFlags = modelState.RenderFlags;
                switch(piplelineType){
                    case PipelineTypes.Normal:
                        if((renderFlags & RenderFlags.NORMAL) == RenderFlags.NONE)
                            continue;
                        commandList.SetPipeline(modelState.Pipeline);
                        effectSets = modelState.EffectResourceSets;
                        break;
                    case PipelineTypes.ShadowMap:
                        if((renderFlags & RenderFlags.SHADOW_MAP) == RenderFlags.NONE)
                            continue;
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
                    RenderCommandGenerator.GenerateCommandsForMesh_Inline<VertexPositionTexture>(
                        commandList,
                        modelState.VertexBuffers[i],
                        modelState.IndexBuffers[i],
                        sceneRuntimeDescriptor.CameraProjViewBuffer,
                        sceneRuntimeDescriptor.CameraResourceSet,
                        modelState.TextureResourceSets[i],
                        effectSets,
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
                var renderFlags = modelState.RenderFlags;
                var effectSets = sceneRuntimeDescriptor.NO_RESOURCE_SET;
                var model = modelState.Model;
                switch(piplelineType){
                    case PipelineTypes.Normal:
                        if((renderFlags & RenderFlags.NORMAL) == RenderFlags.NONE)
                            continue;
                        commandList.SetPipeline(modelState.Pipeline);
                        effectSets = modelState.EffectResourceSets;
                        break;
                    case PipelineTypes.ShadowMap:
                        if((renderFlags & RenderFlags.SHADOW_MAP) == RenderFlags.NONE)
                            continue;
                        commandList.SetPipeline(modelState.ShadowMapPipeline);
                        break;
                }
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
                        effectSets,
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
                var effectSets = sceneRuntimeDescriptor.NO_RESOURCE_SET;
                var renderFlags = modelState.RenderFlags;
                switch(piplelineType){
                    case PipelineTypes.Normal:
                        if((renderFlags & RenderFlags.NORMAL) == RenderFlags.NONE)
                            continue;
                        commandList.SetPipeline(modelState.Pipeline);
                        effectSets = modelState.EffectResourceSets;
                        break;
                    case PipelineTypes.ShadowMap:
                        if((renderFlags & RenderFlags.SHADOW_MAP) == RenderFlags.NONE)
                            continue;
                        commandList.SetPipeline(modelState.ShadowMapPipeline);
                        break;
                }
                for (int i = 0; i < modelState.InstanceBuffers.Length; i++)
                    commandList.SetVertexBuffer(i.ToUnsigned() + 1, modelState.InstanceBuffers[i]);
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
                    effectSets,
                    material,
                    mesh,
                    modelState.TotalInstanceCount
                );
                

            }

        }

    }
}