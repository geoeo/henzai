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
    internal class Scene : Renderable
    {

        private CommandList _commandList;
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
        // TODO: Refactor this into a class with colour
        private Vector4 LIGHT_POS = new Vector4(0,30,30,0);

        Model<VertexPositionNormalTextureTangent> _model;

        public Scene(string title,Resolution windowSize, GraphicsDeviceOptions graphicsDeviceOptions, GraphicsBackend preferredBackend, bool usePreferredGraphicsBackend)
            : base(title,windowSize,graphicsDeviceOptions,preferredBackend,usePreferredGraphicsBackend){
                _vertexBuffers = new List<DeviceBuffer>();
                _indexBuffers = new List<DeviceBuffer>();

                PreDraw+=RotateSphereModel;
        }

        private void RotateSphereModel(){
            var newWorld = _model.World*Matrix4x4.CreateRotationY(Math.PI.ToFloat());
            _model.SetNewWorldTransformation(newWorld);
        }

        private void RotateSphereModel(float delta){
            var newWorld = _model.World*Matrix4x4.CreateRotationY(Math.PI.ToFloat()*delta/10.0f);
            _model.SetNewWorldTransformation(newWorld); 
        }

        // TODO: Abstract Resource Crreation for Uniforms, Vertex Layouts, Disposing
        override protected void CreateResources(){


            // string filePath = Path.Combine(AppContext.BaseDirectory, "armor/armor.dae"); 
            string filePath = Path.Combine(AppContext.BaseDirectory, "nanosuit/nanosuit.obj"); 
            _model = AssimpLoader.LoadFromFile<VertexPositionNormalTextureTangent>(filePath,VertexPositionNormalTextureTangent.HenzaiType);
            GeometryUtils.GenerateTangentSpaceFor(_model);


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

            for(int i = 0; i < _model.meshCount; i++){

                DeviceBuffer vertexBuffer 
                    =  _factory.CreateBuffer(new BufferDescription(_model.meshes[i].vertices.LengthUnsigned() * VertexPositionNormalTextureTangent.SizeInBytes, BufferUsage.VertexBuffer)); 

                DeviceBuffer indexBuffer
                    = _factory.CreateBuffer(new BufferDescription(_model.meshes[i].meshIndices.LengthUnsigned()*sizeof(uint),BufferUsage.IndexBuffer));
                    

                _vertexBuffers.Add(vertexBuffer);
                _indexBuffers.Add(indexBuffer);

                graphicsDevice.UpdateBuffer(vertexBuffer,0,_model.meshes[i].vertices);
                graphicsDevice.UpdateBuffer(indexBuffer,0,_model.meshes[i].meshIndices);
            }

            //Texture Samper
            ImageSharpTexture diffuseTexture = new ImageSharpTexture(Path.Combine(AppContext.BaseDirectory, "armor", "diffuse.png"));
            Texture sphereDiffuseTexture = diffuseTexture.CreateDeviceTexture(graphicsDevice, _factory);
            TextureView sphereDiffuseTextureView = _factory.CreateTextureView(sphereDiffuseTexture);

            ImageSharpTexture normalTexture = new ImageSharpTexture(Path.Combine(AppContext.BaseDirectory, "armor", "normal.png"));
            Texture sphereNormalTexture = normalTexture.CreateDeviceTexture(graphicsDevice, _factory);
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
                graphicsDevice.LinearSampler,
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

            _vertexShader = IO.LoadShader("PhongTexture",ShaderStages.Vertex,graphicsDevice);
            _fragmentShader = IO.LoadShader("PhongTexture",ShaderStages.Fragment,graphicsDevice);

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
                Outputs = graphicsDevice.SwapchainFramebuffer.OutputDescription
            };

            _pipeline = _factory.CreateGraphicsPipeline(pipelineDescription);

            _commandList = _factory.CreateCommandList();

        }

        override protected void BuildCommandList(){
            _commandList.Begin();
            _commandList.SetFramebuffer(graphicsDevice.SwapchainFramebuffer);
            _commandList.SetPipeline(_pipeline);
            _commandList.SetFullViewports();
            _commandList.ClearColorTarget(0,RgbaFloat.White);
            _commandList.ClearDepthStencil(1f);
            for(int i = 0; i < _model.meshCount; i++){
                Material material = _model.meshes[i].TryGetMaterial();

                _commandList.SetVertexBuffer(0,_vertexBuffers[i]);
                _commandList.SetIndexBuffer(_indexBuffers[i],IndexFormat.UInt32);
                _commandList.UpdateBuffer(_cameraProjViewBuffer,0,camera.ViewMatrix);
                _commandList.UpdateBuffer(_cameraProjViewBuffer,64,camera.ProjectionMatrix);
                _commandList.UpdateBuffer(_cameraProjViewBuffer,128,_model.World);
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
                    indexCount: _model.meshes[i].meshIndices.Length.ToUnsigned(),
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
            graphicsDevice.SubmitCommands(_commandList);
        }


    }
    
}