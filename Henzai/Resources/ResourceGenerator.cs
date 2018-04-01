using System;
using System.IO;
using Veldrid;
using Veldrid.ImageSharp;
using Henzai;
using Henzai.Geometry;
using Henzai.Runtime;

namespace Henzai
{
    // TODO: Investigate splitting this up / VertexStruct abstraction
    public static class ResourceGenerator
    {

        public static ResourceLayout GenerateResourceLayout(DisposeCollectorResourceFactory factory, string name,ResourceKind resourceKind, ShaderStages shaderStages){
            var resourceLayoutElementDescription = new ResourceLayoutElementDescription(name,resourceKind,shaderStages);
            ResourceLayoutElementDescription[] resourceLayoutElementDescriptions = {resourceLayoutElementDescription};
            var resourceLayoutDescription = new ResourceLayoutDescription(resourceLayoutElementDescriptions);
            return factory.CreateResourceLayout(resourceLayoutDescription);
        }

        public static ResourceSet GenrateResourceSet(DisposeCollectorResourceFactory factory, ResourceLayout resourceLayout, BindableResource[] bindableResources){
            ResourceSetDescription resourceSetDescription = new ResourceSetDescription(resourceLayout,bindableResources);
            return factory.CreateResourceSet(resourceSetDescription);
        }

        public static ResourceLayout GenerateTextureResourceLayoutForNormalMapping(DisposeCollectorResourceFactory factory){
            return factory.CreateResourceLayout(
                    new ResourceLayoutDescription(
                        new ResourceLayoutElementDescription("DiffuseTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                        new ResourceLayoutElementDescription("DiffuseSampler", ResourceKind.Sampler, ShaderStages.Fragment),
                        new ResourceLayoutElementDescription("NormTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                        new ResourceLayoutElementDescription("NormSampler", ResourceKind.Sampler, ShaderStages.Fragment)
                        ));

        }

        public static ResourceLayout GenerateTextureResourceLayoutForDiffuseMapping(DisposeCollectorResourceFactory factory){
            return factory.CreateResourceLayout(
                    new ResourceLayoutDescription(
                        new ResourceLayoutElementDescription("DiffuseTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                        new ResourceLayoutElementDescription("DiffuseSampler", ResourceKind.Sampler, ShaderStages.Fragment)
                        ));

        }

        public static Sampler GenerateLinearSampler(DisposeCollectorResourceFactory factory){
            return factory.CreateSampler(new SamplerDescription
                {
                    AddressModeU = SamplerAddressMode.Wrap,
                    AddressModeV = SamplerAddressMode.Wrap,
                    AddressModeW = SamplerAddressMode.Wrap,
                    Filter = SamplerFilter.MinLinear_MagLinear_MipLinear,
                    LodBias = 0,
                    MinimumLod = 0,
                    MaximumLod = uint.MaxValue,
                    MaximumAnisotropy = 0,
                });
        }

        public static ResourceSet GenerateTextureResourceSetForNormalMapping(ModelRuntimeDescriptor<VertexPositionNormalTextureTangentBitangent> modelRuntimeState,int meshIndex, DisposeCollectorResourceFactory factory, GraphicsDevice graphicsDevice){
                Material material = modelRuntimeState.Model.meshes[meshIndex].TryGetMaterial();

                ImageSharpTexture diffuseTextureIS = new ImageSharpTexture(Path.Combine(AppContext.BaseDirectory, modelRuntimeState.Model.BaseDir, material.textureDiffuse));
                Texture diffuseTexture = diffuseTextureIS.CreateDeviceTexture(graphicsDevice, factory);
                TextureView diffuseTextureView = factory.CreateTextureView(diffuseTexture);

                string normalTexPath = material.textureNormal.Length == 0 ? material.textureBump : material.textureNormal;
                ImageSharpTexture normalTextureIS = new ImageSharpTexture(Path.Combine(AppContext.BaseDirectory, modelRuntimeState.Model.BaseDir, normalTexPath));
                Texture normalTexture = normalTextureIS.CreateDeviceTexture(graphicsDevice, factory);
                TextureView normalTextureView = factory.CreateTextureView(normalTexture);


                return  factory.CreateResourceSet(new ResourceSetDescription(
                modelRuntimeState.TextureResourceLayout ,
                diffuseTextureView,
                modelRuntimeState.TextureSampler,
                normalTextureView,
                modelRuntimeState.TextureSampler
                ));

        }

                public static ResourceSet GenerateTextureResourceSetForDiffuseMapping(ModelRuntimeDescriptor<VertexPositionNormalTexture> modelRuntimeState,int meshIndex, DisposeCollectorResourceFactory factory, GraphicsDevice graphicsDevice){
                Material material = modelRuntimeState.Model.meshes[meshIndex].TryGetMaterial();

                ImageSharpTexture diffuseTextureIS = new ImageSharpTexture(Path.Combine(AppContext.BaseDirectory, modelRuntimeState.Model.BaseDir, material.textureDiffuse));
                Texture diffuseTexture = diffuseTextureIS.CreateDeviceTexture(graphicsDevice, factory);
                TextureView diffuseTextureView = factory.CreateTextureView(diffuseTexture);

                return  factory.CreateResourceSet(new ResourceSetDescription(
                modelRuntimeState.TextureResourceLayout,
                diffuseTextureView,
                modelRuntimeState.TextureSampler
                ));

        }


        /// <summary>
        /// <see cref="Henzai.Geometry.VertexPositionNormalTextureTangentBitangent"/>
        /// </summary>
        public static VertexLayoutDescription GenerateVertexLayoutForPNTTB(){
                return new VertexLayoutDescription(
                    new VertexElementDescription("Position",VertexElementSemantic.Position,VertexElementFormat.Float3),
                    new VertexElementDescription("Normal",VertexElementSemantic.Normal,VertexElementFormat.Float3),
                    new VertexElementDescription("UV",VertexElementSemantic.TextureCoordinate,VertexElementFormat.Float2),
                    new VertexElementDescription("Tangent",VertexElementSemantic.Normal,VertexElementFormat.Float3),
                    new VertexElementDescription("Bitangent",VertexElementSemantic.Normal,VertexElementFormat.Float3)
                );
        }

        /// <summary>
        /// <see cref="Henzai.Geometry.VertexPositionNormalTextureTangent"/>
        /// </summary>
        public static VertexLayoutDescription GenerateVertexLayoutForPNTT(){
                return new VertexLayoutDescription(
                    new VertexElementDescription("Position",VertexElementSemantic.Position,VertexElementFormat.Float3),
                    new VertexElementDescription("Normal",VertexElementSemantic.Normal,VertexElementFormat.Float3),
                    new VertexElementDescription("UV",VertexElementSemantic.TextureCoordinate,VertexElementFormat.Float2),
                    new VertexElementDescription("Tangent",VertexElementSemantic.Normal,VertexElementFormat.Float3)
                );
        }

        /// <summary>
        /// <see cref="Henzai.Geometry.VertexPositionNormalTexture"/>
        /// </summary>
        public static VertexLayoutDescription GenerateVertexLayoutForPNT(){
                return new VertexLayoutDescription(
                    new VertexElementDescription("Position",VertexElementSemantic.Position,VertexElementFormat.Float3),
                    new VertexElementDescription("Normal",VertexElementSemantic.Normal,VertexElementFormat.Float3),
                    new VertexElementDescription("UV",VertexElementSemantic.TextureCoordinate,VertexElementFormat.Float2)
                );
        }
        /// <summary>
        /// <see cref="Henzai.Geometry.VertexPositionNormalTexture"/>
        /// </summary>
        public static VertexLayoutDescription GenerateVertexLayoutForPN(){
                return new VertexLayoutDescription(
                    new VertexElementDescription("Position",VertexElementSemantic.Position,VertexElementFormat.Float3),
                    new VertexElementDescription("Normal",VertexElementSemantic.Normal,VertexElementFormat.Float3)
                );
        }

        public static GraphicsPipelineDescription GeneratePipelinePN<T>(
            ModelRuntimeDescriptor<T> modelRuntimeState, 
            SceneRuntimeDescriptor sceneRuntimeState, 
            GraphicsDevice graphicsDevice) where T : struct{

            return new GraphicsPipelineDescription(){
                    BlendState = BlendStateDescription.SingleOverrideBlend,
                    DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual,
                    RasterizerState = new RasterizerStateDescription(
                        cullMode: FaceCullMode.Back,
                        fillMode: PolygonFillMode.Solid,
                        frontFace: FrontFace.Clockwise,
                        depthClipEnabled: true,
                        scissorTestEnabled: false
                    ),
                    PrimitiveTopology = PrimitiveTopology.TriangleList,
                    ResourceLayouts = new ResourceLayout[] {
                        sceneRuntimeState.CameraResourceLayout,
                        sceneRuntimeState.LightResourceLayout,
                        sceneRuntimeState.MaterialResourceLayout},
                    ShaderSet = new ShaderSetDescription(
                        vertexLayouts: new VertexLayoutDescription[] {modelRuntimeState.VertexLayout},
                        shaders: new Shader[] {modelRuntimeState.VertexShader,modelRuntimeState.FragmentShader}
                    ),
                    Outputs = graphicsDevice.SwapchainFramebuffer.OutputDescription
                };
        }

        public static GraphicsPipelineDescription GeneratePipelinePNTTB<T>(
            ModelRuntimeDescriptor<T> modelRuntimeState, 
            SceneRuntimeDescriptor sceneRuntimeState, 
            GraphicsDevice graphicsDevice) where T : struct{

            return new GraphicsPipelineDescription(){
                    BlendState = BlendStateDescription.SingleOverrideBlend,
                    DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual,
                    RasterizerState = new RasterizerStateDescription(
                        cullMode: FaceCullMode.Back,
                        fillMode: PolygonFillMode.Solid,
                        frontFace: FrontFace.Clockwise,
                        depthClipEnabled: true,
                        scissorTestEnabled: false
                    ),
                    PrimitiveTopology = PrimitiveTopology.TriangleList,
                    ResourceLayouts = new ResourceLayout[] {
                        sceneRuntimeState.CameraResourceLayout,
                        sceneRuntimeState.LightResourceLayout,
                        sceneRuntimeState.MaterialResourceLayout,
                        modelRuntimeState.TextureResourceLayout},
                    ShaderSet = new ShaderSetDescription(
                        vertexLayouts: new VertexLayoutDescription[] {modelRuntimeState.VertexLayout},
                        shaders: new Shader[] {modelRuntimeState.VertexShader,modelRuntimeState.FragmentShader}
                    ),
                    Outputs = graphicsDevice.SwapchainFramebuffer.OutputDescription
                };
        }

    }
}