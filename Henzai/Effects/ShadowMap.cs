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
        public TextureView ShadowMapTexView {get; private set;}

        // These hold the scene's geometry information
        private ModelRuntimeDescriptor<VertexPositionNormalTextureTangentBitangent>[] _modelPNTTBDescriptorArray;
        private ModelRuntimeDescriptor<VertexPositionNormal>[] _modelPNDescriptorArray;
        private ModelRuntimeDescriptor<VertexPositionTexture>[] _modelPTDescriptorArray;
        private ModelRuntimeDescriptor<VertexPositionColor>[] _modelPCDescriptorArray;

        public ShadowMap(GraphicsDevice graphicsDevice, 
                        Resolution resolution, 
                        Vector4 lightPos,
                        ModelRuntimeDescriptor<VertexPositionNormalTextureTangentBitangent>[] modelPNTTBDescriptorArray, 
                        ModelRuntimeDescriptor<VertexPositionNormal>[] modelPNDescriptorArray, 
                        ModelRuntimeDescriptor<VertexPositionTexture>[] modelPTDescriptorArray, 
                        ModelRuntimeDescriptor<VertexPositionColor>[] modelPCDescriptorArray) : base(graphicsDevice, resolution){      
            lightCam = new OrthographicCamera(resolution.Horizontal,resolution.Vertical, lightPos);
            _modelPNTTBDescriptorArray = modelPNTTBDescriptorArray;
            _modelPNDescriptorArray = modelPNDescriptorArray;
            _modelPTDescriptorArray = modelPTDescriptorArray;
            _modelPCDescriptorArray = modelPCDescriptorArray;
        }

        protected override void SetFramebuffer(){
            var desc = TextureDescription.Texture2D((uint)Resolution.Horizontal, (uint)Resolution.Vertical, 1, 1, PixelFormat.R32_Float, TextureUsage.DepthStencil | TextureUsage.Sampled);
            var depthTexture = _factory.CreateTexture(desc);
            depthTexture.Name = "Shadow Map";
            ShadowMapTexView = _factory.CreateTextureView(depthTexture);
            _frameBuffer = _factory.CreateFramebuffer(new FramebufferDescription(
                new FramebufferAttachmentDescription(depthTexture, 0), Array.Empty<FramebufferAttachmentDescription>()));
        }

        public override void CreateResources(){
            lightCamBuffer = _factory.CreateBuffer(new BufferDescription(Camera.SizeInBytes, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            lightCamLayout = ResourceGenerator.GenerateResourceLayout(
                    _factory,
                    "projViewWorld",
                    ResourceKind.UniformBuffer,
                    ShaderStages.Vertex);
            lightCamSet = ResourceGenerator.GenrateResourceSet(
                    _factory,
                    lightCamLayout,
                    new BindableResource[] { lightCamBuffer });
        }

        public override void BuildCommandList(){
            //TODO:
            _commandList.Begin();
            _commandList.SetFramebuffer(_frameBuffer);
            _commandList.SetFullViewports();

            _commandList.ClearColorTarget(0,RgbaFloat.White);
            _commandList.ClearDepthStencil(1f);

            // RenderCommandGenerator.GenerateCommandsForShadowMapScene_Inline(
            //     _commandList,
            //     lightCamBuffer,
            //     lightCam);

        }
    }
}
