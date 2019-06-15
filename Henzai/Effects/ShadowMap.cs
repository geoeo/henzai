using System;
using System.Collections.Generic;
using System.Numerics;
using Veldrid;
using Henzai.Cameras;
using Henzai.Runtime;
using Henzai.Core.VertexGeometry;

namespace Henzai.Effects 
{
    public sealed class ShadowMap : SubRenderable {

        //TODO: Maybe put this into Light class
        public Camera lightCam {get; private set;}
        public DeviceBuffer lightCamBuffer {get; private set;}
        public ResourceLayout lightCamLayout {get; private set;}
        public ResourceSet lightCamSet {get; private set;}
        ///<summary>
        /// Holds the texture that the framebuffer renders into
        ///</summary>
        public TextureView ShadowMapTexView {get; private set;}

        // These hold the scene's geometry information
        private ModelRuntimeDescriptor<VertexPositionNormalTextureTangentBitangent>[] _modelPNTTBDescriptorArray;
        private ModelRuntimeDescriptor<VertexPositionNormal>[] _modelPNDescriptorArray;
        private ModelRuntimeDescriptor<VertexPositionTexture>[] _modelPTDescriptorArray;
        private ModelRuntimeDescriptor<VertexPositionColor>[] _modelPCDescriptorArray;

        private SceneRuntimeDescriptor _sceneRuntimeDescriptor;
        private Resolution _resolution;

        public ShadowMap(GraphicsDevice graphicsDevice, Resolution resolution) : base(graphicsDevice, resolution){      
            _resolution = resolution;
            _sceneRuntimeDescriptor = new SceneRuntimeDescriptor();
        }

        protected override void SetFramebuffer(){
            var desc = TextureDescription.Texture2D((uint)Resolution.Horizontal, (uint)Resolution.Vertical, 1, 1, PixelFormat.R32_Float, TextureUsage.DepthStencil | TextureUsage.Sampled);
            var depthTexture = _factory.CreateTexture(desc);
            depthTexture.Name = "Shadow Map";
            ShadowMapTexView = _factory.CreateTextureView(depthTexture);
            _frameBuffer = _factory.CreateFramebuffer(new FramebufferDescription(
                new FramebufferAttachmentDescription(depthTexture, 0), Array.Empty<FramebufferAttachmentDescription>()));
        }

        public override void CreateResources(SceneRuntimeDescriptor mainSceneRuntimeDescriptor,                        
                        ModelRuntimeDescriptor<VertexPositionNormalTextureTangentBitangent>[] modelPNTTBDescriptorArray, 
                        ModelRuntimeDescriptor<VertexPositionNormal>[] modelPNDescriptorArray, 
                        ModelRuntimeDescriptor<VertexPositionTexture>[] modelPTDescriptorArray, 
                        ModelRuntimeDescriptor<VertexPositionColor>[] modelPCDescriptorArray){

            _modelPNTTBDescriptorArray = modelPNTTBDescriptorArray;
            _modelPNDescriptorArray = modelPNDescriptorArray;
            _modelPTDescriptorArray = modelPTDescriptorArray;
            _modelPCDescriptorArray = modelPCDescriptorArray;
            var lightPos  = mainSceneRuntimeDescriptor.Light.LightPos;
            lightCam = new OrthographicCamera(_resolution.Horizontal,_resolution.Vertical, lightPos);

            _sceneRuntimeDescriptor.CameraProjViewBuffer = _factory.CreateBuffer(new BufferDescription(Camera.SizeInBytes, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            _sceneRuntimeDescriptor.CameraResourceLayout
                = ResourceGenerator.GenerateResourceLayout(
                    _factory,
                    "projViewWorld",
                    ResourceKind.UniformBuffer,
                    ShaderStages.Vertex);
            _sceneRuntimeDescriptor.CameraResourceSet
                = ResourceGenerator.GenrateResourceSet(
                    _factory,
                    _sceneRuntimeDescriptor.CameraResourceLayout,
                    new BindableResource[] { _sceneRuntimeDescriptor.CameraProjViewBuffer });


            lightCamBuffer = _factory.CreateBuffer(new BufferDescription(Camera.SizeInBytes, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            lightCamLayout = ResourceGenerator.GenerateResourceLayout(
                    _factory,
                    "light",
                    ResourceKind.UniformBuffer,
                    ShaderStages.Vertex);
            lightCamSet = ResourceGenerator.GenrateResourceSet(
                    _factory,
                    lightCamLayout,
                    new BindableResource[] { lightCamBuffer });

            _sceneRuntimeDescriptor.LightBuffer = lightCamBuffer;
            _sceneRuntimeDescriptor.LightResourceLayout = lightCamLayout;
            _sceneRuntimeDescriptor.LightResourceSet = lightCamSet;
        }

        public override void BuildCommandList(){
            _commandList.Begin();
            _commandList.SetFramebuffer(_frameBuffer);
            _commandList.SetFullViewports();
            _commandList.ClearDepthStencil(1f);

            RenderCommandGenerator.GenerateCommandsForScene_Inline(
                _commandList,
                _sceneRuntimeDescriptor.CameraProjViewBuffer,
                lightCam);

            //TODO: Problem with instancing
            //RenderCommandGenerator.GenerateRenderCommandsForModelDescriptor<VertexPositionNormalTextureTangentBitangent>(_commandList,_modelPNTTBDescriptorArray,_sceneRuntimeDescriptor, PipelineTypes.ShadowMap);
            RenderCommandGenerator.GenerateRenderCommandsForModelDescriptor<VertexPositionNormal>(_commandList,_modelPNDescriptorArray,_sceneRuntimeDescriptor, PipelineTypes.ShadowMap);
            RenderCommandGenerator.GenerateRenderCommandsForModelDescriptor<VertexPositionTexture>(_commandList,_modelPTDescriptorArray,_sceneRuntimeDescriptor, PipelineTypes.ShadowMap);
            RenderCommandGenerator.GenerateRenderCommandsForModelDescriptor<VertexPositionColor>(_commandList,_modelPCDescriptorArray,_sceneRuntimeDescriptor, PipelineTypes.ShadowMap);

            _commandList.End();
        }
    }
}
