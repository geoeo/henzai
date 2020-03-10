using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using Veldrid;
using Veldrid.ImageSharp;
using Henzai.Runtime;
using Henzai.Core.VertexGeometry;
using Henzai.Core.Materials;
using Henzai.Core.Extensions;

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

        public static ResourceLayout GenerateTextureResourceLayoutForShadowMapping(DisposeCollectorResourceFactory factory){
            return factory.CreateResourceLayout(
                    new ResourceLayoutDescription(
                        new ResourceLayoutElementDescription("ShadowMapTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                        new ResourceLayoutElementDescription("ShadowMapSampler", ResourceKind.Sampler, ShaderStages.Fragment)
                        ));

        }

        public static ResourceLayout GenerateTextureResourceLayoutForDiffuseMapping(DisposeCollectorResourceFactory factory){
            return factory.CreateResourceLayout(
                    new ResourceLayoutDescription(
                        new ResourceLayoutElementDescription("DiffuseTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                        new ResourceLayoutElementDescription("DiffuseSampler", ResourceKind.Sampler, ShaderStages.Fragment)
                        ));

        }

        public static ResourceLayout GenerateTextureResourceLayoutForCubeMapping(DisposeCollectorResourceFactory factory){
            return factory.CreateResourceLayout(
                    new ResourceLayoutDescription(
                        new ResourceLayoutElementDescription("CubeTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                        new ResourceLayoutElementDescription("CubeSampler", ResourceKind.Sampler, ShaderStages.Fragment)
                        ));

        }

        public static Sampler GenerateTriLinearSampler(DisposeCollectorResourceFactory factory){
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

        public static Sampler GeneratePointSampler(DisposeCollectorResourceFactory factory){
            return factory.CreateSampler(new SamplerDescription
                {
                    AddressModeU = SamplerAddressMode.Wrap,
                    AddressModeV = SamplerAddressMode.Wrap,
                    AddressModeW = SamplerAddressMode.Wrap,
                    Filter = SamplerFilter.MinPoint_MagPoint_MipPoint,
                    LodBias = 0,
                    MinimumLod = 0,
                    MaximumLod = uint.MaxValue,
                    MaximumAnisotropy = 0,
                });
        }


        public static Sampler GenerateBiLinearSampler(DisposeCollectorResourceFactory factory){
            return factory.CreateSampler(new SamplerDescription
                {
                    AddressModeU = SamplerAddressMode.Wrap,
                    AddressModeV = SamplerAddressMode.Wrap,
                    AddressModeW = SamplerAddressMode.Wrap,
                    Filter = SamplerFilter.MinLinear_MagLinear_MipPoint,
                    LodBias = 0,
                    MinimumLod = 0,
                    MaximumLod = 0,
                    MaximumAnisotropy = 0,
                });
        }

        //TODO: UNIFY THIS!
        public static Sampler GenerateBiLinearSampler_Clamp(DisposeCollectorResourceFactory factory){
            return factory.CreateSampler(new SamplerDescription
                {
                    AddressModeU = SamplerAddressMode.Clamp,
                    AddressModeV = SamplerAddressMode.Clamp,
                    AddressModeW = SamplerAddressMode.Clamp,
                    Filter = SamplerFilter.MinLinear_MagLinear_MipPoint,
                    LodBias = 0,
                    MinimumLod = 0,
                    MaximumLod = 0,
                    MaximumAnisotropy = 0,
                });
        }

        public static ResourceSet GenerateTextureResourceSetForNormalMapping<T>(ModelRuntimeDescriptor<T> modelRuntimeState, int meshIndex, DisposeCollectorResourceFactory factory, GraphicsDevice graphicsDevice) where T : struct , VertexLocateable {
            RealtimeMaterial material = modelRuntimeState.Model.GetMaterial(meshIndex);

            ImageSharpTexture diffuseTextureIS = new ImageSharpTexture(Path.Combine(AppContext.BaseDirectory, modelRuntimeState.Model.BaseDir, material.textureDiffuse));
            Texture diffuseTexture = diffuseTextureIS.CreateDeviceTexture(graphicsDevice, factory);
            TextureView diffuseTextureView = factory.CreateTextureView(diffuseTexture);

            string normalTexPath = material.textureNormal.Length == 0 ? material.textureBump : material.textureNormal;
            ImageSharpTexture normalTextureIS = new ImageSharpTexture(Path.Combine(AppContext.BaseDirectory, modelRuntimeState.Model.BaseDir, normalTexPath));
            Texture normalTexture = normalTextureIS.CreateDeviceTexture(graphicsDevice, factory);
            TextureView normalTextureView = factory.CreateTextureView(normalTexture);


            return factory.CreateResourceSet(new ResourceSetDescription(
            modelRuntimeState.TextureResourceLayout ,
            diffuseTextureView,
            modelRuntimeState.TextureSampler,
            normalTextureView,
            modelRuntimeState.TextureSampler
            ));
        }

        public static ResourceSet GenerateResourceSetForShadowMapping(TextureView shadowMapTexView, DisposeCollectorResourceFactory factory) {
            var textureLayout = GenerateTextureResourceLayoutForShadowMapping(factory);
            var sampler = ResourceGenerator.GenerateBiLinearSampler_Clamp(factory);

            return factory.CreateResourceSet(new ResourceSetDescription(
            textureLayout,
            shadowMapTexView,
            sampler
            ));
        }

        public static ResourceSet GenerateTextureResourceSetForDiffuseMapping<T>(ModelRuntimeDescriptor<T> modelRuntimeState,int meshIndex, DisposeCollectorResourceFactory factory, GraphicsDevice graphicsDevice) where T : struct, VertexLocateable {
            RealtimeMaterial material = modelRuntimeState.Model.GetMaterial(meshIndex);

            ImageSharpTexture diffuseTextureIS = new ImageSharpTexture(Path.Combine(AppContext.BaseDirectory, modelRuntimeState.Model.BaseDir, material.textureDiffuse));
            Texture diffuseTexture = diffuseTextureIS.CreateDeviceTexture(graphicsDevice, factory);
            TextureView diffuseTextureView = factory.CreateTextureView(diffuseTexture);

            return factory.CreateResourceSet(new ResourceSetDescription(
            modelRuntimeState.TextureResourceLayout,
            diffuseTextureView,
            modelRuntimeState.TextureSampler
            ));

        }

        public static ResourceSet GenerateTextureResourceSetForCubeMapping<T>(ModelRuntimeDescriptor<T> modelRuntimeState,int meshIndex, DisposeCollectorResourceFactory factory, GraphicsDevice graphicsDevice) where T : struct, VertexLocateable {

            RealtimeMaterial material = modelRuntimeState.Model.GetMaterial(meshIndex);

            Image<Rgba32> front = Image.Load(Path.Combine(AppContext.BaseDirectory, modelRuntimeState.Model.BaseDir, material.cubeMapFront));
            Image<Rgba32> back = Image.Load(Path.Combine(AppContext.BaseDirectory, modelRuntimeState.Model.BaseDir, material.cubeMapBack));
            Image<Rgba32> left = Image.Load(Path.Combine(AppContext.BaseDirectory, modelRuntimeState.Model.BaseDir, material.cubeMapLeft));
            Image<Rgba32> right = Image.Load(Path.Combine(AppContext.BaseDirectory, modelRuntimeState.Model.BaseDir, material.cubeMapRight));
            Image<Rgba32> top = Image.Load(Path.Combine(AppContext.BaseDirectory, modelRuntimeState.Model.BaseDir, material.cubeMapTop));
            Image<Rgba32> bottom = Image.Load(Path.Combine(AppContext.BaseDirectory, modelRuntimeState.Model.BaseDir, material.cubeMapBottom));

            TextureView diffuseTextureView = ResourceGenerator.CreateCubeMapTextureViewFromImages(front,
                                                                                        back,
                                                                                        left,
                                                                                        right,
                                                                                        top,
                                                                                        bottom,
                                                                                        factory,
                                                                                        graphicsDevice);

            return factory.CreateResourceSet(new ResourceSetDescription(
            modelRuntimeState.TextureResourceLayout,
            diffuseTextureView,
            modelRuntimeState.TextureSampler
            ));

        }

        private static unsafe TextureView CreateCubeMapTextureViewFromImages(
                                                                Image<Rgba32> front,
                                                                Image<Rgba32> back,
                                                                Image<Rgba32> left,
                                                                Image<Rgba32> right,
                                                                Image<Rgba32> top,
                                                                Image<Rgba32> bottom,
                                                                DisposeCollectorResourceFactory factory,
                                                                GraphicsDevice graphicsDevice
                                                                ){
            Texture textureCube;
            TextureView textureView;
            fixed (Rgba32* frontPin = &MemoryMarshal.GetReference(front.GetPixelSpan()))
            fixed (Rgba32* backPin = &MemoryMarshal.GetReference(back.GetPixelSpan()))
            fixed (Rgba32* leftPin = &MemoryMarshal.GetReference(left.GetPixelSpan()))
            fixed (Rgba32* rightPin = &MemoryMarshal.GetReference(right.GetPixelSpan()))
            fixed (Rgba32* topPin = &MemoryMarshal.GetReference(top.GetPixelSpan()))
            fixed (Rgba32* bottomPin = &MemoryMarshal.GetReference(bottom.GetPixelSpan()))
            {
                uint width = (uint)front.Width;
                uint height = (uint)front.Height;
                textureCube = factory.CreateTexture(TextureDescription.Texture2D(
                    width,
                    height,
                    6,
                    1,
                    PixelFormat.R8_G8_B8_A8_UNorm,
                    TextureUsage.Sampled | TextureUsage.Cubemap));

                uint faceSize = (uint)(front.Width * front.Height * Unsafe.SizeOf<Rgba32>());
                graphicsDevice.UpdateTexture(textureCube, (IntPtr)rightPin, faceSize, 0, 0, 0, width, height, 1, 0, 0); // +X
                graphicsDevice.UpdateTexture(textureCube, (IntPtr)leftPin, faceSize, 0, 0, 0, width, height, 1, 0, 1); // -X
                graphicsDevice.UpdateTexture(textureCube, (IntPtr)topPin, faceSize, 0, 0, 0, width, height, 1, 0, 2); // +Y
                graphicsDevice.UpdateTexture(textureCube, (IntPtr)bottomPin, faceSize, 0, 0, 0, width, height, 1, 0, 3); // -Y
                graphicsDevice.UpdateTexture(textureCube, (IntPtr)backPin, faceSize, 0, 0, 0, width, height, 1, 0, 4); // +Z
                graphicsDevice.UpdateTexture(textureCube, (IntPtr)frontPin, faceSize, 0, 0, 0, width, height, 1, 0, 5); // -Z

                textureView = factory.CreateTextureView(new TextureViewDescription(textureCube));
            }

            return textureView;
        }

        public static TextureView CreateEmptyCubeMapTextureView(
                                                                int width,
                                                                int height,
                                                                DisposeCollectorResourceFactory factory,
                                                                GraphicsDevice graphicsDevice
                                                                ){
            Texture textureCube;
            TextureView textureView;

            textureCube = factory.CreateTexture(TextureDescription.Texture2D(
                (uint)width,
                (uint)height,
                6,
                1,
                PixelFormat.R8_G8_B8_A8_UNorm,
                TextureUsage.Sampled | TextureUsage.Cubemap));

            textureView = factory.CreateTextureView(new TextureViewDescription(textureCube));
            return textureView;
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
        /// <summary>
        /// <see cref="Henzai.Geometry.VertexPositionColor"/>
        /// </summary>
        public static VertexLayoutDescription GenerateVertexLayoutForPC(){
            return new VertexLayoutDescription(
                new VertexElementDescription("Position",VertexElementSemantic.Position,VertexElementFormat.Float3),
                new VertexElementDescription("Color",VertexElementSemantic.Color,VertexElementFormat.Float4)
            );
        }
        /// <summary>
        /// <see cref="Henzai.Geometry.VertexPositionTexture"/>
        /// </summary>
        public static VertexLayoutDescription GenerateVertexLayoutForPT(){
            return new VertexLayoutDescription(
                new VertexElementDescription("Position",VertexElementSemantic.Position,VertexElementFormat.Float3),
                new VertexElementDescription("UV",VertexElementSemantic.TextureCoordinate,VertexElementFormat.Float2)
            );
        }
        /// <summary>
        /// <see cref="Henzai.Geometry.VertexPosition"/>
        /// </summary>
        public static VertexLayoutDescription GenerateVertexLayoutForP(){
            return new VertexLayoutDescription(
                new VertexElementDescription("Position",VertexElementSemantic.Position,VertexElementFormat.Float3)
            );
        }

        /// <summary>
        /// Generates a <see cref="Veldrid.VertexLayoutDescription"/> for offsetting a 3D Position
        /// </summary>
        public static VertexLayoutDescription GenerateVertexInstanceLayoutForPositionOffset(){
            return new VertexLayoutDescription(
                stride:12, // Size of Vector 3
                instanceStepRate:1,
                elements: new VertexElementDescription[] {  new VertexElementDescription("Offset",VertexElementSemantic.Position,VertexElementFormat.Float3)}
            );
        }

        /// <summary>
        /// Generates a <see cref="Veldrid.VertexLayoutDescription"/> for offsetting a 3D Position
        /// </summary>
        public static VertexLayoutDescription GenerateVertexInstanceLayoutForViewMatrixOffset(){
            return new VertexLayoutDescription(
                stride:64, // Size of 4* Vector 4
                instanceStepRate:1,
                elements: new VertexElementDescription[] {  
                    new VertexElementDescription("ViewCol1",VertexElementSemantic.TextureCoordinate,VertexElementFormat.Float4),
                    new VertexElementDescription("ViewCol2",VertexElementSemantic.TextureCoordinate,VertexElementFormat.Float4),
                    new VertexElementDescription("ViewCol3",VertexElementSemantic.TextureCoordinate,VertexElementFormat.Float4),
                    new VertexElementDescription("ViewCol4",VertexElementSemantic.TextureCoordinate,VertexElementFormat.Float4)}
            );
        }

        //TODO: use read only sizes instad of hardcoded numbers
        public static DeviceBuffer AllocateInstanceDataBuffer(InstanceData instanceData, GraphicsDevice graphicsDevice, ResourceFactory resourceFactory) {
            DeviceBuffer deviceBuffer = null;
            switch(instanceData.Flag){
                case InstancingDataFlags.EMPTY:
                    break;
                case InstancingDataFlags.POSITION:
                    deviceBuffer =  resourceFactory.CreateBuffer(new BufferDescription(instanceData.Positions.Length.ToUnsigned() * 12, BufferUsage.VertexBuffer));
                    graphicsDevice.UpdateBuffer(deviceBuffer, 0, instanceData.Positions);
                    break;
                case InstancingDataFlags.VIEW_MATRICES:
                    deviceBuffer = resourceFactory.CreateBuffer(new BufferDescription(instanceData.ViewMatrices.Length.ToUnsigned() * 64, BufferUsage.VertexBuffer));
                    graphicsDevice.UpdateBuffer(deviceBuffer, 0, instanceData.ViewMatrices);
                    break;
                default:
                    throw new NotImplementedException($"{instanceData.Flag.ToString("g")} not implemented");
            }

            return deviceBuffer;
        }
    }
}