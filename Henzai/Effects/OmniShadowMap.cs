using System;
using Veldrid;
using Henzai.Cameras;
using Henzai.Runtime;
using Henzai.Core.VertexGeometry;

namespace Henzai.Effects 
{
    public sealed class OmniShadowMap : SubRenderable {

        ///<summary>
        /// Holds the texture that the framebuffer renders into
        ///</summary>
        public TextureView ShadowMapTexView {get; private set;}
        public ResourceSet ShadowMatrices {get; private set;}
        public ResourceSet CameraInfo {get; private set;}

        // These hold the scene's geometry information
        private ModelRuntimeDescriptor<VertexPositionNormalTextureTangentBitangent>[] _modelPNTTBDescriptorArray;
        private ModelRuntimeDescriptor<VertexPositionNormal>[] _modelPNDescriptorArray;
        private ModelRuntimeDescriptor<VertexPositionTexture>[] _modelPTDescriptorArray;
        private ModelRuntimeDescriptor<VertexPositionColor>[] _modelPCDescriptorArray;

        public OmniShadowMap(GraphicsDevice graphicsDevice, Resolution resolution) : base(graphicsDevice, resolution){      

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

             _sceneRuntimeDescriptor.OmniLightProjViewBuffer = _factory.CreateBuffer(new BufferDescription(6*Core.Numerics.Utils.SinglePrecision4x4InBytes, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            _sceneRuntimeDescriptor.OmniLightProvViewResourceLayout
                = ResourceGenerator.GenerateResourceLayout(
                    _factory,
                    "shadowMatrices",
                    ResourceKind.UniformBuffer,
                    ShaderStages.Geometry);
            _sceneRuntimeDescriptor.OmniLightProjViewResourceSet
                = ResourceGenerator.GenrateResourceSet(
                    _factory,
                    _sceneRuntimeDescriptor.OmniLightProvViewResourceLayout,
                    new BindableResource[] { _sceneRuntimeDescriptor.OmniLightProjViewBuffer });

            _sceneRuntimeDescriptor.CameraInfoBuffer = _factory.CreateBuffer(new BufferDescription(32, BufferUsage.UniformBuffer));
            _sceneRuntimeDescriptor.CameraInfoResourceLayout
                = ResourceGenerator.GenerateResourceLayout(
                    _factory,
                    "cameraInfo",
                    ResourceKind.UniformBuffer,
                    ShaderStages.Fragment);
            _sceneRuntimeDescriptor.CameraInfoResourceSet
                = ResourceGenerator.GenrateResourceSet(
                    _factory,
                    _sceneRuntimeDescriptor.CameraInfoResourceLayout,
                    new BindableResource[] { _sceneRuntimeDescriptor.CameraInfoBuffer });

            ShadowMatrices = _sceneRuntimeDescriptor.OmniLightProjViewResourceSet;
            CameraInfo = _sceneRuntimeDescriptor.CameraInfoResourceSet;

        }

        protected override void GenerateFramebuffer(){
            var textureDescription = ResourceGenerator.CreateEmptyCubeMapTextureDescription(1024);

            var depthTexture = _factory.CreateTexture(textureDescription);
            depthTexture.Name = "OmniShadowCubeTexture";
            ShadowMapTexView = _factory.CreateTextureView(depthTexture);

            _frameBuffer = _factory.CreateFramebuffer(new FramebufferDescription(depthTexture, Array.Empty<Texture>()));
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

            _sceneRuntimeDescriptor.Light = mainSceneRuntimeDescriptor.Light;
            _sceneRuntimeDescriptor.OmniLights = mainSceneRuntimeDescriptor.OmniLights;
        }

        public override void BuildCommandList(){
            _commandList.Begin();
            _commandList.SetFramebuffer(_frameBuffer);
            _commandList.SetFullViewports();
            _commandList.ClearDepthStencil(1.0f);


            RenderCommandGenerator.GenerateCommandsForScene_Inline(
                _commandList,
                _sceneRuntimeDescriptor.OmniLightProjViewBuffer,
                _sceneRuntimeDescriptor.CameraProjViewBuffer,
                _sceneRuntimeDescriptor.CameraInfoBuffer,
                _sceneRuntimeDescriptor.OmniLights);

            RenderCommandGenerator.GenerateRenderCommandsForModelDescriptor<VertexPositionNormalTextureTangentBitangent>(_commandList, _modelPNTTBDescriptorArray, _sceneRuntimeDescriptor, new RenderDescription(RenderFlags.OMNI_SHADOW_MAPS), VertexRuntimeTypes.VertexPositionNormalTextureTangentBitangent);
            RenderCommandGenerator.GenerateRenderCommandsForModelDescriptor<VertexPositionNormal>(_commandList, _modelPNDescriptorArray, _sceneRuntimeDescriptor, new RenderDescription(RenderFlags.OMNI_SHADOW_MAPS), VertexRuntimeTypes.VertexPositionNormal);
            RenderCommandGenerator.GenerateRenderCommandsForModelDescriptor<VertexPositionTexture>(_commandList, _modelPTDescriptorArray, _sceneRuntimeDescriptor, new RenderDescription(RenderFlags.OMNI_SHADOW_MAPS), VertexRuntimeTypes.VertexPositionTexture);
            RenderCommandGenerator.GenerateRenderCommandsForModelDescriptor<VertexPositionColor>(_commandList, _modelPCDescriptorArray, _sceneRuntimeDescriptor, new RenderDescription(RenderFlags.OMNI_SHADOW_MAPS), VertexRuntimeTypes.VertexPositionColor);

            _commandList.End();
            
        }
    }
}
