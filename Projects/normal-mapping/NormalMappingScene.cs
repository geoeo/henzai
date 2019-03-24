using System;
using System.Numerics;
using System.Collections.Generic;
using System.IO;
using Veldrid;
using Veldrid.StartupUtilities;
using Veldrid.Sdl2;
using Veldrid.OpenGL;
using Veldrid.ImageSharp;
using Henzai;
using Henzai.Extensions;
using Henzai.Geometry;
using Henzai.Runtime;

namespace Henzai.Examples
{
    internal class NormalMappingScene : Renderable
    {
        private List<DeviceBuffer> _vertexBuffers;
        private List<DeviceBuffer> _indexBuffers;
        private Shader _vertexShader;
        private Shader _fragmentShader;
        private Pipeline _pipeline;
        private DeviceBuffer _cameraProjViewBuffer;
        private DeviceBuffer _materialBuffer;
        private DeviceBuffer _lightBuffer;
        private ResourceSet _cameraResourceSet;
        private ResourceSet _materialResourceSet;
        private ResourceSet _lightResourceSet;
        private ResourceSet _textureResourceSet;
        private ResourceLayout _cameraResourceLayout;
        private ResourceLayout _materialResourceLayout;
        private ResourceLayout _lightResourceLayout;
        // TODO: Refactor this into a class with color
        private Vector4 LIGHT_POS = new Vector4(0,30,30,0);

        Model<VertexPositionNormalTextureTangent, Material> _model;

        public NormalMappingScene(string title,Resolution windowSize, GraphicsDeviceOptions graphicsDeviceOptions, RenderOptions renderOptions)
            : base(title,windowSize,graphicsDeviceOptions,renderOptions){
                _vertexBuffers = new List<DeviceBuffer>();
                _indexBuffers = new List<DeviceBuffer>();

                PreDraw+=RotateSphereModel;
        }

        private void RotateSphereModel(){
            var newWorld = _model.GetWorld_DontMutate*Matrix4x4.CreateRotationY(Math.PI.ToFloat());
            _model.SetNewWorldTransformation(ref newWorld,true);
        }

        private void RotateSphereModel(float delta){
            var newWorld = _model.GetWorld_DontMutate*Matrix4x4.CreateRotationY(Math.PI.ToFloat()*delta/10.0f);
            _model.SetNewWorldTransformation(ref newWorld,true); 
        }

        // TODO: Abstract Resource Crreation for Uniforms, Vertex Layouts, Disposing
        override protected void CreateResources(){

            //string filePath = Path.Combine(AppContext.BaseDirectory, "Models/sphere.obj"); // huge 
            //string filePath = Path.Combine(AppContext.BaseDirectory, "Models/sphere_centered.obj"); // no texture coordiantes
            string filePath = "armor/armor.dae"; 
            _model = AssimpLoader.LoadFromFile<VertexPositionNormalTextureTangent>(AppContext.BaseDirectory,filePath,VertexPositionNormalTextureTangent.HenzaiType);
            //GeometryUtils.GenerateSphericalTextureCoordinatesFor(_model.meshes[0], UVMappingTypes.Spherical_Coordinates,true);
            GeometryUtils.GenerateTangentSpaceFor(_model);
            // _model = new Model<VertexPositionNormalTextureTangent>(GeometryFactory.generateSphere(100,100,1.0f));

            /// Uniform 1 - Camera
            _cameraProjViewBuffer = _factory.CreateBuffer(new BufferDescription(192,BufferUsage.UniformBuffer | BufferUsage.Dynamic));

            var resourceLayoutElementDescription = new ResourceLayoutElementDescription("projViewWorld",ResourceKind.UniformBuffer,ShaderStages.Vertex);
            ResourceLayoutElementDescription[] resourceLayoutElementDescriptions = {resourceLayoutElementDescription};
            var resourceLayoutDescription = new ResourceLayoutDescription(resourceLayoutElementDescriptions);
            BindableResource[] bindableResources = new BindableResource[]{_cameraProjViewBuffer};
            _cameraResourceLayout = _factory.CreateResourceLayout(resourceLayoutDescription);
            var resourceSetDescription = new ResourceSetDescription(_cameraResourceLayout,bindableResources);
            
            _cameraResourceSet = _factory.CreateResourceSet(resourceSetDescription);

            // Uniform 2 - Material
            _materialBuffer = _factory.CreateBuffer(new BufferDescription(64,BufferUsage.UniformBuffer));

            var resourceLayoutElementDescriptionMaterial = new ResourceLayoutElementDescription("material",ResourceKind.UniformBuffer,ShaderStages.Fragment);
            ResourceLayoutElementDescription[] resourceLayoutElementDescriptionsMaterial = {resourceLayoutElementDescriptionMaterial};
            var resourceLayoutDescriptionMaterial = new ResourceLayoutDescription(resourceLayoutElementDescriptionsMaterial);
            BindableResource[] bindableResourcesMaterial = new BindableResource[]{_materialBuffer};

            _materialResourceLayout = _factory.CreateResourceLayout(resourceLayoutDescriptionMaterial);
            var resourceSetDescriptionMaterial = new ResourceSetDescription(_materialResourceLayout,bindableResourcesMaterial);
            
            _materialResourceSet = _factory.CreateResourceSet(resourceSetDescriptionMaterial);

            // Uniform 3 - Light
            _lightBuffer = _factory.CreateBuffer(new BufferDescription(16,BufferUsage.UniformBuffer));

            var resourceLayoutElementDescriptionLight = new ResourceLayoutElementDescription("light",ResourceKind.UniformBuffer,ShaderStages.Vertex);
            ResourceLayoutElementDescription[] resourceLayoutElementDescriptionsLight = {resourceLayoutElementDescriptionLight};
            var resourceLayoutDescriptionLight = new ResourceLayoutDescription(resourceLayoutElementDescriptionsLight);
            BindableResource[] bindableResourcesLight = new BindableResource[]{_lightBuffer};

            _lightResourceLayout = _factory.CreateResourceLayout(resourceLayoutDescriptionLight);
            var resourceSetDescriptionLight = new ResourceSetDescription(_lightResourceLayout,bindableResourcesLight);
            
            _lightResourceSet = _factory.CreateResourceSet(resourceSetDescriptionLight);

            for(int i = 0; i < _model.MeshCount; i++){

                var mesh = _model.GetMesh(i);
                DeviceBuffer vertexBuffer 
                    =  _factory.CreateBuffer(new BufferDescription(mesh.Vertices.LengthUnsigned() * VertexPositionNormalTextureTangent.SizeInBytes, BufferUsage.VertexBuffer)); 

                DeviceBuffer indexBuffer
                    = _factory.CreateBuffer(new BufferDescription(mesh.MeshIndices.LengthUnsigned()*sizeof(ushort),BufferUsage.IndexBuffer));
                    

                _vertexBuffers.Add(vertexBuffer);
                _indexBuffers.Add(indexBuffer);

                GraphicsDevice.UpdateBuffer(vertexBuffer,0, mesh.Vertices);
                GraphicsDevice.UpdateBuffer(indexBuffer,0, mesh.MeshIndices);
            }

            //Texture Samper
            // ImageSharpTexture diffuseTexture = new ImageSharpTexture(Path.Combine(AppContext.BaseDirectory, "Textures", "earth.jpg"));
            // ImageSharpTexture diffuseTexture = new ImageSharpTexture(Path.Combine(AppContext.BaseDirectory, "Textures", "Water.jpg"));
            ImageSharpTexture diffuseTexture = new ImageSharpTexture(Path.Combine(AppContext.BaseDirectory, "armor", "diffuse.png"));
            Texture sphereDiffuseTexture = diffuseTexture.CreateDeviceTexture(GraphicsDevice, _factory);
            TextureView sphereDiffuseTextureView = _factory.CreateTextureView(sphereDiffuseTexture);

            // ImageSharpTexture normalTexture = new ImageSharpTexture(Path.Combine(AppContext.BaseDirectory, "Textures", "WaterNorm.jpg"));
            ImageSharpTexture normalTexture = new ImageSharpTexture(Path.Combine(AppContext.BaseDirectory, "armor", "normal.png"));
            Texture sphereNormalTexture = normalTexture.CreateDeviceTexture(GraphicsDevice, _factory);
            TextureView sphereNormalTextureView = _factory.CreateTextureView(sphereNormalTexture);

            ResourceLayout textureLayout = _factory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("DiffuseTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("DiffuseSampler", ResourceKind.Sampler, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("NormTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("NormSampler", ResourceKind.Sampler, ShaderStages.Fragment)
                    ));

            var sampler = _factory.CreateSampler(new SamplerDescription
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

            _textureResourceSet = _factory.CreateResourceSet(new ResourceSetDescription(
                textureLayout,
                sphereDiffuseTextureView,
                GraphicsDevice.LinearSampler,
                sphereNormalTextureView,
                sampler
                ));

            VertexLayoutDescription vertexLayout 
                = new VertexLayoutDescription(
                    new VertexElementDescription("Position",VertexElementSemantic.Position,VertexElementFormat.Float3),
                    new VertexElementDescription("Normal",VertexElementSemantic.Normal,VertexElementFormat.Float3),
                    new VertexElementDescription("UV",VertexElementSemantic.TextureCoordinate,VertexElementFormat.Float2),
                    new VertexElementDescription("Tangent",VertexElementSemantic.Normal,VertexElementFormat.Float3)
                );

            _vertexShader = IO.LoadShader("PhongTexture",ShaderStages.Vertex,GraphicsDevice);
            _fragmentShader = IO.LoadShader("PhongTexture",ShaderStages.Fragment,GraphicsDevice);

            GraphicsPipelineDescription pipelineDescription = new GraphicsPipelineDescription(){
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
                ResourceLayouts = new ResourceLayout[] {_cameraResourceLayout,_lightResourceLayout,_materialResourceLayout,textureLayout},
                ShaderSet = new ShaderSetDescription(
                    vertexLayouts: new VertexLayoutDescription[] {vertexLayout},
                    shaders: new Shader[] {_vertexShader,_fragmentShader}
                ),
                Outputs = GraphicsDevice.SwapchainFramebuffer.OutputDescription
            };

            _pipeline = _factory.CreateGraphicsPipeline(pipelineDescription);

        }

        override protected void BuildCommandList(){
            _commandList.Begin();
            _commandList.SetFramebuffer(GraphicsDevice.SwapchainFramebuffer);
            _commandList.SetPipeline(_pipeline);
            _commandList.SetFullViewports();
            _commandList.ClearColorTarget(0,RgbaFloat.White);
            _commandList.ClearDepthStencil(1f);
            for(int i = 0; i < _model.MeshCount; i++){
                var mesh = _model.GetMesh(i);
                var material = _model.TryGetMaterial(i);

                _commandList.SetVertexBuffer(0,_vertexBuffers[i]);
                _commandList.SetIndexBuffer(_indexBuffers[i],IndexFormat.UInt16);
                _commandList.UpdateBuffer(_cameraProjViewBuffer,0,Camera.ViewMatrix);
                _commandList.UpdateBuffer(_cameraProjViewBuffer,64,Camera.ProjectionMatrix);
                _commandList.UpdateBuffer(_cameraProjViewBuffer,128,_model.GetWorld_DontMutate);
                _commandList.SetGraphicsResourceSet(0,_cameraResourceSet); // Always after SetPipeline
                _commandList.UpdateBuffer(_lightBuffer,0,LIGHT_POS);
                _commandList.SetGraphicsResourceSet(1,_lightResourceSet);
                _commandList.UpdateBuffer(_materialBuffer,0,material.diffuse);
                _commandList.UpdateBuffer(_materialBuffer,16,material.specular);
                _commandList.UpdateBuffer(_materialBuffer,32,material.ambient);
                _commandList.UpdateBuffer(_materialBuffer,48,material.coefficients);
                _commandList.SetGraphicsResourceSet(2,_materialResourceSet);
                _commandList.SetGraphicsResourceSet(3,_textureResourceSet);
                _commandList.DrawIndexed(
                    indexCount: mesh.MeshIndices.Length.ToUnsigned(),
                    instanceCount: 1,
                    indexStart: 0,
                    vertexOffset: 0,
                    instanceStart: 0
                );
                //_commandList.Draw(_sphereModel.meshes[i].vertices.Length.ToUnsigned());

            } 

            _commandList.End();
        }

        override protected void Draw(){
            GraphicsDevice.SubmitCommands(_commandList);
        }


    }
    
}