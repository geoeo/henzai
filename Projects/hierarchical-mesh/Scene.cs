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
        private List<DeviceBuffer> _vertexBuffersList;
        private List<DeviceBuffer> _indexBuffersList;
        private List<ResourceSet> _textureResourceSetsList;
        private List<Model<VertexPositionNormalTextureTangentBitangent>> _modelsList;
        private DeviceBuffer[] _vertexBuffers;
        private DeviceBuffer[] _indexBuffers;
        private ResourceSet[] _textureResourceSets;
        private Model<VertexPositionNormalTextureTangentBitangent>[] _models;
        private Shader _vertexShader;
        private Shader _fragmentShader;
        private Pipeline _pipeline;
        private DeviceBuffer _cameraProjViewBuffer;
        private DeviceBuffer _materialBuffer;
        private DeviceBuffer _lightBuffer;
        private ResourceSet _cameraResourceSet;
        private ResourceSet _materialResourceSet;
        private ResourceSet _lightResourceSet;
        private ResourceLayout _cameraResourceLayout;
        private ResourceLayout _materialResourceLayout;
        private ResourceLayout _lightResourceLayout;
        // TODO: Refactor this into a class with colour
        private Vector4 LIGHT_POS = new Vector4(0,10,15,1);

        Model<VertexPositionNormalTextureTangentBitangent> _model;

        public Scene(string title,Resolution windowSize, GraphicsDeviceOptions graphicsDeviceOptions, RenderOptions renderOptions)
            : base(title,windowSize,graphicsDeviceOptions,renderOptions){
                _vertexBuffersList = new List<DeviceBuffer>();
                _indexBuffersList = new List<DeviceBuffer>();
                _textureResourceSetsList = new List<ResourceSet>();
                _modelsList = new List<Model<VertexPositionNormalTextureTangentBitangent>>();

                PreDraw+=RotateModel;
                PreRenderLoop+=FormatResources;
        }

        private void RotateSphereModel(){
            // var newWorld = _model.World*Matrix4x4.CreateRotationY(Math.PI.ToFloat());
            // _model.SetNewWorldTransformation(newWorld);
        }

        private void RotateModel(float delta){
            var newWorld = _model.World*Matrix4x4.CreateRotationY(Math.PI.ToFloat()*delta/10.0f);
            _model.SetNewWorldTransformation(newWorld); 
        }

        private void FormatResources(){
        _vertexBuffers = _vertexBuffersList.ToArray();
        _indexBuffers = _indexBuffersList.ToArray();
        _textureResourceSets = _textureResourceSetsList.ToArray();
        _models = _modelsList.ToArray();;
        }

        // TODO: Abstract Resource Crreation for Uniforms, Vertex Layouts, Disposing
        override protected void CreateResources(){


            // string filePath = Path.Combine(AppContext.BaseDirectory, "armor/armor.dae"); 
            // string filePath = Path.Combine(AppContext.BaseDirectory, "nanosuit/nanosuit.obj"); 
            _model = AssimpLoader.LoadFromFile<VertexPositionNormalTextureTangentBitangent>(AppContext.BaseDirectory,"nanosuit/nanosuit.obj",VertexPositionNormalTextureTangentBitangent.HenzaiType);
            GeometryUtils.GenerateTangentAndBitagentSpaceFor(_model);
            var sun = new Model<VertexPositionNormalTextureTangentBitangent>("water",GeometryFactory.generateSphereTangentBitangent(10,10,1));
            sun.meshes[0].TryGetMaterial().textureDiffuse = "Water.jpg";
            sun.meshes[0].TryGetMaterial().textureNormal = "WaterNorm.jpg";
            sun.meshes[0].TryGetMaterial().ambient = new Vector4(1.0f,1.0f,1.0f,1.0f);
            sun.World.Translation = new Vector3(LIGHT_POS.X,LIGHT_POS.Y,LIGHT_POS.Z);

            _modelsList.Add(sun);
            _modelsList.Add(_model);

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

            foreach(var model in _modelsList){
                for(int i = 0; i < model.meshCount; i++){

                    DeviceBuffer vertexBuffer 
                        =  _factory.CreateBuffer(new BufferDescription(model.meshes[i].vertices.LengthUnsigned() * VertexPositionNormalTextureTangentBitangent.SizeInBytes, BufferUsage.VertexBuffer)); 

                    DeviceBuffer indexBuffer
                        = _factory.CreateBuffer(new BufferDescription(model.meshes[i].meshIndices.LengthUnsigned()*sizeof(uint),BufferUsage.IndexBuffer));
                        

                    _vertexBuffersList.Add(vertexBuffer);
                    _indexBuffersList.Add(indexBuffer);

                    graphicsDevice.UpdateBuffer(vertexBuffer,0,model.meshes[i].vertices);
                    graphicsDevice.UpdateBuffer(indexBuffer,0,model.meshes[i].meshIndices);

                    Material material = model.meshes[i].TryGetMaterial();

                    ImageSharpTexture diffuseTextureIS = new ImageSharpTexture(Path.Combine(AppContext.BaseDirectory, model.BaseDir, material.textureDiffuse));
                    Texture diffuseTexture = diffuseTextureIS.CreateDeviceTexture(graphicsDevice, _factory);
                    TextureView diffuseTextureView = _factory.CreateTextureView(diffuseTexture);

                    string normalTexPath = material.textureNormal.Length == 0 ? material.textureBump : material.textureNormal;
                    ImageSharpTexture normalTextureIS = new ImageSharpTexture(Path.Combine(AppContext.BaseDirectory, model.BaseDir, normalTexPath));
                    Texture normalTexture = normalTextureIS.CreateDeviceTexture(graphicsDevice, _factory);
                    TextureView normalTextureView = _factory.CreateTextureView(normalTexture);

                    ResourceSet textureResourceSet = _factory.CreateResourceSet(new ResourceSetDescription(
                    textureLayout,
                    diffuseTextureView,
                    sampler,
                    normalTextureView,
                    sampler
                    ));

                    _textureResourceSetsList.Add(textureResourceSet);
                }

            }
            
            VertexLayoutDescription vertexLayout 
                = new VertexLayoutDescription(
                    new VertexElementDescription("Position",VertexElementSemantic.Position,VertexElementFormat.Float3),
                    new VertexElementDescription("Normal",VertexElementSemantic.Normal,VertexElementFormat.Float3),
                    new VertexElementDescription("UV",VertexElementSemantic.TextureCoordinate,VertexElementFormat.Float2),
                    new VertexElementDescription("Tangent",VertexElementSemantic.Normal,VertexElementFormat.Float3),
                    new VertexElementDescription("Bitangent",VertexElementSemantic.Normal,VertexElementFormat.Float3)
                );

            _vertexShader = IO.LoadShader("PhongBitangentTexture",ShaderStages.Vertex,graphicsDevice);
            _fragmentShader = IO.LoadShader("PhongBitangentTexture",ShaderStages.Fragment,graphicsDevice);

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
            int runningMeshTotal = 0;

            _commandList.Begin();
            _commandList.SetFramebuffer(graphicsDevice.SwapchainFramebuffer);
            _commandList.SetFullViewports();

            _commandList.ClearColorTarget(0,RgbaFloat.White);
            _commandList.ClearDepthStencil(1f);
            for(int j = 0; j < _models.Length; j++){
                var model = _models[j];
                _commandList.SetPipeline(_pipeline);

                _commandList.UpdateBuffer(_cameraProjViewBuffer,0,camera.ViewMatrix);
                _commandList.UpdateBuffer(_cameraProjViewBuffer,64,camera.ProjectionMatrix);

                _commandList.UpdateBuffer(_lightBuffer,0,LIGHT_POS);
                for(int i = 0; i < model.meshCount; i++){
                    var mesh = model.meshes[i];
                    Material material = mesh.GetMaterialRuntime();

                    _commandList.SetVertexBuffer(0,_vertexBuffers[runningMeshTotal]);
                    _commandList.SetIndexBuffer(_indexBuffers[runningMeshTotal],IndexFormat.UInt32);
                    _commandList.UpdateBuffer(_cameraProjViewBuffer,128,model.World);
                    _commandList.SetGraphicsResourceSet(0,_cameraResourceSet); // Always after SetPipeline
                    _commandList.SetGraphicsResourceSet(1,_lightResourceSet);
                    _commandList.UpdateBuffer(_materialBuffer,0,material.diffuse);
                    _commandList.UpdateBuffer(_materialBuffer,16,material.specular);
                    _commandList.UpdateBuffer(_materialBuffer,32,material.ambient);
                    _commandList.UpdateBuffer(_materialBuffer,48,material.coefficients);
                    _commandList.SetGraphicsResourceSet(2,_materialResourceSet);
                    _commandList.SetGraphicsResourceSet(3,_textureResourceSets[runningMeshTotal]);
                    _commandList.DrawIndexed(
                        indexCount: mesh.meshIndices.Length.ToUnsigned(),
                        instanceCount: 1,
                        indexStart: 0,
                        vertexOffset: 0,
                        instanceStart: 0
                    );
                //_commandList.Draw(_sphereModel.meshes[i].vertices.Length.ToUnsigned());
                    runningMeshTotal++;
                }
            }
             
            _commandList.End();
        }

        override protected void Draw(){
            graphicsDevice.SubmitCommands(_commandList);
        }


    }
    
}