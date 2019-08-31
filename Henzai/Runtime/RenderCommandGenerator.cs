using System;
using System.Runtime.CompilerServices;
using Veldrid;
using Henzai.Core.Extensions;
using Henzai.Cameras;
using Henzai.Core.Materials;
using Henzai.Core.VertexGeometry;
using Henzai.Core.Acceleration;

namespace Henzai.Runtime
{
    //TODO: Refactor this class by vertex type
    public static class RenderCommandGenerator
    {

        //TODO: Make each object reponsible for updating its buffer
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
        private static void GenerateCommandsForPNTTB_Inline(
                                                    CommandList commandList,
                                                    ModelRuntimeDescriptor<VertexPositionNormalTextureTangentBitangent> modelState,
                                                    int meshIndex,
                                                    SceneRuntimeDescriptor sceneRuntimeDescriptor,
                                                    ResourceSet[] effectResourceSets,
                                                    RealtimeMaterial material,
                                                    Mesh<VertexPositionNormalTextureTangentBitangent> mesh,
                                                    uint modelInstanceCount)
        {
            var effectIndex = 5;
 
            var vertexBuffer = modelState.VertexBuffers[meshIndex];
            var indexBuffer = modelState.IndexBuffers[meshIndex];
            var textureResourceSet = modelState.TextureResourceSets[meshIndex];

            var cameraProjViewBuffer = sceneRuntimeDescriptor.CameraProjViewBuffer;
            var materialBuffer = sceneRuntimeDescriptor.MaterialBuffer;
            var cameraResourceSet = sceneRuntimeDescriptor.CameraResourceSet;
            var lightResourceSet = sceneRuntimeDescriptor.LightResourceSet;
            var pointlightResourceSet = sceneRuntimeDescriptor.SpotLightResourceSet;
            var materialResourceSet = sceneRuntimeDescriptor.MaterialResourceSet;

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
        private static void GenerateCommandsForPN_Inline(
                                                    CommandList commandList,
                                                    ModelRuntimeDescriptor<VertexPositionNormal> modelState,
                                                    int meshIndex,
                                                    SceneRuntimeDescriptor sceneRuntimeDescriptor,
                                                    ResourceSet[] effectResourceSets,
                                                    RealtimeMaterial material,
                                                    Mesh<VertexPositionNormal> mesh,
                                                    uint modelInstanceCount)
        {
            var effectIndex = 3;

            var vertexBuffer = modelState.VertexBuffers[meshIndex];
            var indexBuffer = modelState.IndexBuffers[meshIndex];

            var cameraProjViewBuffer = sceneRuntimeDescriptor.CameraProjViewBuffer;
            var materialBuffer = sceneRuntimeDescriptor.MaterialBuffer;
            var cameraResourceSet = sceneRuntimeDescriptor.CameraResourceSet;
            var lightResourceSet = sceneRuntimeDescriptor.LightResourceSet;
            var materialResourceSet = sceneRuntimeDescriptor.MaterialResourceSet;

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
        private static void GenerateCommandsForPT_Inline(
                                                    CommandList commandList,
                                                    ModelRuntimeDescriptor<VertexPositionTexture> modelState,
                                                    int meshIndex,
                                                    SceneRuntimeDescriptor sceneRuntimeDescriptor,
                                                    ResourceSet[] effectResourceSets,
                                                    Mesh<VertexPositionTexture> mesh,
                                                    uint modelInstanceCount)
        {
            var effectIndex = 2;

            var vertexBuffer = modelState.VertexBuffers[meshIndex];
            var indexBuffer = modelState.IndexBuffers[meshIndex];
            var textureResourceSet = modelState.TextureResourceSets[meshIndex];

            var cameraProjViewBuffer = sceneRuntimeDescriptor.CameraProjViewBuffer;
            var cameraResourceSet = sceneRuntimeDescriptor.CameraResourceSet;

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
        /// Also used in ShadowMap PrePass
        ///</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void GenerateCommandsForMesh_Inline<T>(
                                                    CommandList commandList,
                                                    ModelRuntimeDescriptor<T> modelState,
                                                    int meshIndex,
                                                    SceneRuntimeDescriptor sceneRuntimeDescriptor,
                                                    ResourceSet[] effectResourceSets,
                                                    Mesh<T> mesh,
                                                    uint modelInstanceCount) where T : struct, VertexLocateable
        {
            var effectIndex = 1;

            var vertexBuffer = modelState.VertexBuffers[meshIndex];
            var indexBuffer = modelState.IndexBuffers[meshIndex];

            var cameraProjViewBuffer = sceneRuntimeDescriptor.CameraProjViewBuffer;
            var cameraResourceSet = sceneRuntimeDescriptor.CameraResourceSet;

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
        private static void GenerateCommandsForP_Inline(
                                                    CommandList commandList,
                                                    ModelRuntimeDescriptor<VertexPosition> modelState,
                                                    int meshIndex,
                                                    SceneRuntimeDescriptor sceneRuntimeDescriptor,
                                                    ResourceSet[] effectResourceSets,
                                                    Mesh<VertexPosition> mesh,
                                                    uint modelInstanceCount)
        {
            var effectIndex = 2;

            var vertexBuffer = modelState.VertexBuffers[meshIndex];
            var indexBuffer = modelState.IndexBuffers[meshIndex];
            var textureResourceSet = modelState.TextureResourceSets[meshIndex];

            var cameraProjViewBuffer = sceneRuntimeDescriptor.CameraProjViewBuffer;
            var cameraResourceSet = sceneRuntimeDescriptor.CameraResourceSet;

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
            commandList.SetPipeline(cubeMapRuntimeDescriptor.Pipelines[RenderFlags.NORMAL_ARRAY_INDEX]);
            for (int i = 0; i < model.MeshCount; i++)
            {
                var mesh = model.GetMesh(i);
                RenderCommandGenerator.GenerateCommandsForP_Inline(
                    commandList,
                    cubeMapRuntimeDescriptor,
                    i,
                    sceneRuntimeDescriptor,
                    cubeMapRuntimeDescriptor.EffectResourceSets,
                    mesh,
                    cubeMapRuntimeDescriptor.TotalInstanceCount
                );
            }

        }

        public static void GenerateRenderCommandsForModelDescriptor<T>(CommandList commandList,
                                                                       ModelRuntimeDescriptor<T>[] descriptorArray,
                                                                       SceneRuntimeDescriptor sceneRuntimeDescriptor,
                                                                       uint renderFlag,
                                                                       VertexRuntimeTypes vertexRuntimeType) where T : struct, VertexLocateable
        {
            for (int j = 0; j < descriptorArray.Length; j++)
            {
                var modelState = descriptorArray[j];
                var effectSets = sceneRuntimeDescriptor.NO_RESOURCE_SET;
                var modelRenderFlag = modelState.RenderFlag;
                var currentRenderState = modelRenderFlag & renderFlag;
                if(currentRenderState == RenderFlags.NONE)
                    continue;
                var renderStateArrayIndex = RenderFlags.GetArrayIndexForFlag(currentRenderState);
                commandList.SetPipeline(modelState.Pipelines[renderStateArrayIndex]);
                var effectsInstanceBuffer = modelState.InstanceBuffers[renderStateArrayIndex];
                // We assume one other vertex buffer has been bound or will be bound.
                for (int i = 0; i < effectsInstanceBuffer.Length; i++)
                    commandList.SetVertexBuffer(i.ToUnsigned() + 1, effectsInstanceBuffer[i]);

                if((currentRenderState & RenderFlags.NORMAL) == RenderFlags.NORMAL)
                    effectSets = modelState.EffectResourceSets;

                var model = modelState.Model;

                for (int i = 0; i < model.MeshCount; i++)
                {
                    if (!model.GetMeshBVH(i).AABBIsValid)
                        continue;
                    var mesh = model.GetMesh(i);
                    var material = model.GetMaterial(i);

                    //TODO: @Investiagte this
                    if((renderFlag & RenderFlags.SHADOW_MAP)  == RenderFlags.SHADOW_MAP || vertexRuntimeType == VertexRuntimeTypes.VertexPositionColor)
                        RenderCommandGenerator.GenerateCommandsForMesh_Inline(
                            commandList,
                            modelState,
                            i,
                            sceneRuntimeDescriptor,
                            effectSets,
                            mesh,
                            modelState.TotalInstanceCount
                        );

                    
                    else {
                        switch (vertexRuntimeType){
                            case VertexRuntimeTypes.VertexPosition:
                                RenderCommandGenerator.GenerateCommandsForP_Inline(
                                commandList,
                                modelState as ModelRuntimeDescriptor<VertexPosition>,
                                i,
                                sceneRuntimeDescriptor,
                                effectSets,
                                mesh as Mesh<VertexPosition>,
                                modelState.TotalInstanceCount
                            );
                            break;
                            case VertexRuntimeTypes.VertexPositionTexture:
                                RenderCommandGenerator.GenerateCommandsForPT_Inline(
                                    commandList,
                                    modelState as ModelRuntimeDescriptor<VertexPositionTexture>,
                                    i,
                                    sceneRuntimeDescriptor,
                                    effectSets,
                                    mesh as Mesh<VertexPositionTexture>,
                                    modelState.TotalInstanceCount
                                );
                            break;
                            case VertexRuntimeTypes.VertexPositionNormal:
                                RenderCommandGenerator.GenerateCommandsForPN_Inline(
                                    commandList,
                                    modelState as ModelRuntimeDescriptor<VertexPositionNormal>,
                                    i,
                                    sceneRuntimeDescriptor,
                                    effectSets,
                                    material,
                                    mesh as Mesh<VertexPositionNormal>,
                                    modelState.TotalInstanceCount
                                );
                            break;
                            case VertexRuntimeTypes.VertexPositionNormalTextureTangentBitangent:
                                RenderCommandGenerator.GenerateCommandsForPNTTB_Inline(
                                    commandList,
                                    modelState as ModelRuntimeDescriptor<VertexPositionNormalTextureTangentBitangent>,
                                    i,
                                    sceneRuntimeDescriptor,
                                    effectSets,
                                    material,
                                    mesh as Mesh<VertexPositionNormalTextureTangentBitangent>,
                                    modelState.TotalInstanceCount
                                );
                            break;
                            default:
                                var errorStr = "Type: " + typeof(T).Name + " not implemented";
                                throw new System.NotImplementedException(errorStr);

                        }
 
                        
                    }

                }

            }
        }

        //TODO: Data driven -> should refactor so that all GenerateRenderCommandsForModelDescriptor use this structure
        public static void GenerateRenderCommandsForModelDescriptor(CommandList commandList,
                                                                    ModelRuntimeDescriptor<VertexPositionNormalTextureTangentBitangent>[] descriptorArray,
                                                                    SceneRuntimeDescriptor sceneRuntimeDescriptor,
                                                                    MeshBVH<VertexPositionNormalTextureTangentBitangent>[] meshBVHArray,
                                                                    uint renderFlag)
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
                var modelRenderFlag = modelState.RenderFlag;
                var currentRenderState = modelRenderFlag & renderFlag;
                if(currentRenderState == RenderFlags.NONE)
                    continue;
                var renderStateArrayIndex = RenderFlags.GetArrayIndexForFlag(currentRenderState);
                commandList.SetPipeline(modelState.Pipelines[renderStateArrayIndex]);
                var effectsInstanceBuffer = modelState.InstanceBuffers[renderStateArrayIndex];
                // We assume one other vertex buffer has been bound or will be bound.
                for (int i = 0; i < effectsInstanceBuffer.Length; i++)
                    commandList.SetVertexBuffer(i.ToUnsigned() + 1, effectsInstanceBuffer[i]);

                if((currentRenderState & RenderFlags.NORMAL) == RenderFlags.NORMAL)
                    effectSets = modelState.EffectResourceSets;

                var mesh = model.GetMesh(j);
                var material = model.GetMaterial(meshIndex);

                RenderCommandGenerator.GenerateCommandsForPNTTB_Inline(
                    commandList,
                    modelState,
                    meshIndex,
                    sceneRuntimeDescriptor,
                    effectSets,
                    material,
                    mesh,
                    modelState.TotalInstanceCount
                );
                

            }

        }

    }
}