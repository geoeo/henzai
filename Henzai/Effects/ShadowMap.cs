using System;
using System.Collections.Generic;
using System.Numerics;
using Veldrid;
using Henzai.Runtime;

namespace Henzai.Effects 
{
    public sealed class ShadowMap : SubRenderable {

        public Camera lightCam {get; private set;}

        public DeviceBuffer lightCamBuffer {get; private set;}
        public ResourceLayout lightCamLayout {get; private set;}
        public ResourceSet lightCamSet {get; private set;}
        public TextureView ShadowMapTexView {get; private set;}

        public ShadowMap(GraphicsDevice graphicsDevice, Resolution resolution) : base(graphicsDevice, resolution){      
            lightCam = new OrthographicCamera(resolution.Horizontal,resolution.Vertical);
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
            //TODO:
            lightCamBuffer = _factory.CreateBuffer(new BufferDescription(Camera.SizeInBytes, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            lightCamLayout = ResourceGenerator.GenerateResourceLayout(
                    _factory,
                    "lightProjViewWorld",
                    ResourceKind.UniformBuffer,
                    ShaderStages.Vertex);
            lightCamSet = ResourceGenerator.GenrateResourceSet(
                    _factory,
                    lightCamLayout,
                    new BindableResource[] { lightCamBuffer });
        }

        public override void BuildCommandList(){
            //TODO:
        }

        public override void Draw(){
            //TODO:
        }



    }
}
