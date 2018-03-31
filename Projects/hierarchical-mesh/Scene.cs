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
using Henzai.Runtime.Render;

namespace Henzai.Examples
{
    internal class Scene : Renderable
    {
        private CommandList _commandList;
        private SceneRuntimeState _sceneRuntimeState;
        // private List<DeviceBuffer> _vertexBuffersList;
        // private List<DeviceBuffer> _indexBuffersList;
        // private List<ResourceSet> _textureResourceSetsList;
        // private List<Model<VertexPositionNormalTextureTangentBitangent>> _modelsList;
        // private DeviceBuffer[] _vertexBuffers;
        // private DeviceBuffer[] _indexBuffers;
        // private ResourceSet[] _textureResourceSets;
        // private Model<VertexPositionNormalTextureTangentBitangent>[] _models;
        // private Shader _vertexShader;
        // private Shader _fragmentShader;
        // private Pipeline _pipeline;

        private List<ModelRuntimeState<VertexPositionNormalTextureTangentBitangent>> _modelStatesList;
        private ModelRuntimeState<VertexPositionNormalTextureTangentBitangent> [] _modelStates;

        Model<VertexPositionNormalTextureTangentBitangent> _model;

        public Scene(string title,Resolution windowSize, GraphicsDeviceOptions graphicsDeviceOptions, RenderOptions renderOptions)
            : base(title,windowSize,graphicsDeviceOptions,renderOptions){
                _sceneRuntimeState = new SceneRuntimeState();

                // _vertexBuffersList = new List<DeviceBuffer>();
                // _indexBuffersList = new List<DeviceBuffer>();
                // _textureResourceSetsList = new List<ResourceSet>();
                // _modelsList = new List<Model<VertexPositionNormalTextureTangentBitangent>>();

                _modelStatesList = new List<ModelRuntimeState<VertexPositionNormalTextureTangentBitangent>>();


                PreDraw+=RotateModel;
                PreRenderLoop+=FormatResourcesForRuntime;
        }

        private void RotateModel(float delta){
            var newWorld = _model.GetWorld_DontMutate*Matrix4x4.CreateRotationY(Math.PI.ToFloat()*delta/10.0f);
            _model.SetNewWorldTransformation(ref newWorld,true); 
        }

        private void FormatResourcesForRuntime(){
        // _vertexBuffers = _vertexBuffersList.ToArray();
        // _indexBuffers = _indexBuffersList.ToArray();
        // _textureResourceSets = _textureResourceSetsList.ToArray();
        foreach(var modelState in _modelStatesList)
            modelState.FormatResourcesForRuntime();


        _modelStates = _modelStatesList.ToArray();
        }

        // TODO: Abstract Resource Crreation for Uniforms, Vertex Layouts, Disposing
        override protected void CreateResources(){

            _sceneRuntimeState.Light = new Light();

            // string filePath = Path.Combine(AppContext.BaseDirectory, "armor/armor.dae"); 
            // string filePath = Path.Combine(AppContext.BaseDirectory, "nanosuit/nanosuit.obj"); 
            _model = AssimpLoader.LoadFromFile<VertexPositionNormalTextureTangentBitangent>(AppContext.BaseDirectory,"nanosuit/nanosuit.obj",VertexPositionNormalTextureTangentBitangent.HenzaiType);
            // _model = AssimpLoader.LoadFromFile<VertexPositionNormalTextureTangentBitangent>(AppContext.BaseDirectory,"sponza/sponza.obj",VertexPositionNormalTextureTangentBitangent.HenzaiType);
            GeometryUtils.GenerateTangentAndBitagentSpaceFor(_model);
            // GeometryUtils.CheckTBN(_model);
            var sun = new Model<VertexPositionNormalTextureTangentBitangent>("water",GeometryFactory.generateSphereTangentBitangent(100,100,1));
            sun.meshes[0].TryGetMaterial().textureDiffuse = "Water.jpg";
            sun.meshes[0].TryGetMaterial().textureNormal = "WaterNorm.jpg";
            sun.meshes[0].TryGetMaterial().ambient = new Vector4(1.0f,1.0f,1.0f,1.0f);
            ref Vector4 lightPos = ref _sceneRuntimeState.Light.Light_DontMutate;
            Vector3 newTranslation = new Vector3(lightPos.X,lightPos.Y,lightPos.Z);
            sun.SetNewWorldTranslation(ref newTranslation, true);

            _modelStatesList.Add(new ModelRuntimeState<VertexPositionNormalTextureTangentBitangent>(sun));
            _modelStatesList.Add(new ModelRuntimeState<VertexPositionNormalTextureTangentBitangent>(_model));

            /// Uniform 1 - Camera
            _sceneRuntimeState.CameraProjViewBuffer  = _factory.CreateBuffer(new BufferDescription(192,BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            _sceneRuntimeState.CameraResourceLayout 
                = ResourceGenerator.GenerateResourceLayout(
                    _factory,
                    "projViewWorld",
                    ResourceKind.UniformBuffer,
                    ShaderStages.Vertex);
            _sceneRuntimeState.CameraResourceSet 
                = ResourceGenerator.GenrateResourceSet(
                    _factory,
                    _sceneRuntimeState.CameraResourceLayout,
                    new BindableResource[]{_sceneRuntimeState.CameraProjViewBuffer});

            // Uniform 2 - Material
            _sceneRuntimeState.MaterialBuffer = _factory.CreateBuffer(new BufferDescription(64,BufferUsage.UniformBuffer));
            _sceneRuntimeState.MaterialResourceLayout 
                = ResourceGenerator.GenerateResourceLayout(
                    _factory,
                    "material",
                    ResourceKind.UniformBuffer,
                    ShaderStages.Fragment);
            _sceneRuntimeState.MaterialResourceSet 
                = ResourceGenerator.GenrateResourceSet(
                    _factory,
                    _sceneRuntimeState.MaterialResourceLayout,
                    new BindableResource[]{_sceneRuntimeState.MaterialBuffer});

            // Uniform 3 - Light
            _sceneRuntimeState.LightBuffer = _factory.CreateBuffer(new BufferDescription(16,BufferUsage.UniformBuffer));
            _sceneRuntimeState.LightResourceLayout 
                = ResourceGenerator.GenerateResourceLayout(
                    _factory,
                    "light",
                    ResourceKind.UniformBuffer,
                    ShaderStages.Fragment);
            _sceneRuntimeState.LightResourceSet 
                = ResourceGenerator.GenrateResourceSet(
                    _factory,
                    _sceneRuntimeState.LightResourceLayout,
                    new BindableResource[]{_sceneRuntimeState.LightBuffer});

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

            foreach(var modelState in _modelStatesList){

                var model = modelState.model;

                // ResourceLayout textureLayout = _factory.CreateResourceLayout(
                //     new ResourceLayoutDescription(
                //         new ResourceLayoutElementDescription("DiffuseTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                //         new ResourceLayoutElementDescription("DiffuseSampler", ResourceKind.Sampler, ShaderStages.Fragment),
                //         new ResourceLayoutElementDescription("NormTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                //         new ResourceLayoutElementDescription("NormSampler", ResourceKind.Sampler, ShaderStages.Fragment)
                //         ));

                // var sampler = _factory.CreateSampler(new SamplerDescription
                // {
                //     AddressModeU = SamplerAddressMode.Wrap,
                //     AddressModeV = SamplerAddressMode.Wrap,
                //     AddressModeW = SamplerAddressMode.Wrap,
                //     Filter = SamplerFilter.MinLinear_MagLinear_MipLinear,
                //     LodBias = 0,
                //     MinimumLod = 0,
                //     MaximumLod = uint.MaxValue,
                //     MaximumAnisotropy = 0,
                // });

                for(int i = 0; i < model.meshCount; i++){

                    DeviceBuffer vertexBuffer 
                        =  _factory.CreateBuffer(new BufferDescription(model.meshes[i].vertices.LengthUnsigned() * VertexPositionNormalTextureTangentBitangent.SizeInBytes, BufferUsage.VertexBuffer)); 

                    DeviceBuffer indexBuffer
                        = _factory.CreateBuffer(new BufferDescription(model.meshes[i].meshIndices.LengthUnsigned()*sizeof(uint),BufferUsage.IndexBuffer));
                        

                    modelState.VertexBuffersList.Add(vertexBuffer);
                    modelState.IndexBuffersList.Add(indexBuffer);
                    // _vertexBuffersList.Add(vertexBuffer);
                    // _indexBuffersList.Add(indexBuffer);

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

                    // _textureResourceSetsList.Add(textureResourceSet);
                    modelState.TextureResourceSetsList.Add(textureResourceSet);
                }

                VertexLayoutDescription vertexLayout 
                = new VertexLayoutDescription(
                    new VertexElementDescription("Position",VertexElementSemantic.Position,VertexElementFormat.Float3),
                    new VertexElementDescription("Normal",VertexElementSemantic.Normal,VertexElementFormat.Float3),
                    new VertexElementDescription("UV",VertexElementSemantic.TextureCoordinate,VertexElementFormat.Float2),
                    new VertexElementDescription("Tangent",VertexElementSemantic.Normal,VertexElementFormat.Float3),
                    new VertexElementDescription("Bitangent",VertexElementSemantic.Normal,VertexElementFormat.Float3)
                );

                modelState.VertexShader = IO.LoadShader("PhongBitangentTexture",ShaderStages.Vertex,graphicsDevice);
                modelState.FragmentShader = IO.LoadShader("PhongBitangentTexture",ShaderStages.Fragment,graphicsDevice);

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
                    ResourceLayouts = new ResourceLayout[] {
                        _sceneRuntimeState.CameraResourceLayout,
                        _sceneRuntimeState.LightResourceLayout,
                        _sceneRuntimeState.MaterialResourceLayout,
                        textureLayout},
                    ShaderSet = new ShaderSetDescription(
                        vertexLayouts: new VertexLayoutDescription[] {vertexLayout},
                        shaders: new Shader[] {modelState.VertexShader,modelState.FragmentShader}
                    ),
                    Outputs = graphicsDevice.SwapchainFramebuffer.OutputDescription
                };

                modelState.Pipeline = _factory.CreateGraphicsPipeline(pipelineDescription);

            // _pipeline = _factory.CreateGraphicsPipeline(pipelineDescription);

            }
            
            // VertexLayoutDescription vertexLayout 
            //     = new VertexLayoutDescription(
            //         new VertexElementDescription("Position",VertexElementSemantic.Position,VertexElementFormat.Float3),
            //         new VertexElementDescription("Normal",VertexElementSemantic.Normal,VertexElementFormat.Float3),
            //         new VertexElementDescription("UV",VertexElementSemantic.TextureCoordinate,VertexElementFormat.Float2),
            //         new VertexElementDescription("Tangent",VertexElementSemantic.Normal,VertexElementFormat.Float3),
            //         new VertexElementDescription("Bitangent",VertexElementSemantic.Normal,VertexElementFormat.Float3)
            //     );

            // _vertexShader = IO.LoadShader("PhongBitangentTexture",ShaderStages.Vertex,graphicsDevice);
            // _fragmentShader = IO.LoadShader("PhongBitangentTexture",ShaderStages.Fragment,graphicsDevice);

            // GraphicsPipelineDescription pipelineDescription = new GraphicsPipelineDescription(){
            //     BlendState = BlendStateDescription.SingleOverrideBlend,
            //     DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual,
            //     RasterizerState = new RasterizerStateDescription(
            //         cullMode: FaceCullMode.Back,
            //         fillMode: PolygonFillMode.Solid,
            //         frontFace: FrontFace.Clockwise,
            //         depthClipEnabled: true,
            //         scissorTestEnabled: false
            //     ),
            //     PrimitiveTopology = PrimitiveTopology.TriangleList,
            //     ResourceLayouts = new ResourceLayout[] {
            //         _sceneRuntimeState.CameraResourceLayout,
            //         _sceneRuntimeState.LightResourceLayout,
            //         _sceneRuntimeState.MaterialResourceLayout,
            //         textureLayout},
            //     ShaderSet = new ShaderSetDescription(
            //         vertexLayouts: new VertexLayoutDescription[] {vertexLayout},
            //         shaders: new Shader[] {_vertexShader,_fragmentShader}
            //     ),
            //     Outputs = graphicsDevice.SwapchainFramebuffer.OutputDescription
            // };

            // _pipeline = _factory.CreateGraphicsPipeline(pipelineDescription);

            _commandList = _factory.CreateCommandList();

        }

        override protected void BuildCommandList(){
            _commandList.Begin();
            _commandList.SetFramebuffer(graphicsDevice.SwapchainFramebuffer);
            _commandList.SetFullViewports();

            _commandList.ClearColorTarget(0,RgbaFloat.White);
            _commandList.ClearDepthStencil(1f);

            for(int j = 0; j < _modelStates.Length; j++){
                var modelState = _modelStates[j];
                var model = modelState.model;
                RenderCommandGenerator_Inline.GenerateCommandsForModel(
                    _commandList,
                    modelState.Pipeline,
                    _sceneRuntimeState.CameraProjViewBuffer,
                    _sceneRuntimeState.LightBuffer,
                    Camera,
                    ref _sceneRuntimeState.Light.Light_DontMutate,
                    model);
                for(int i = 0; i < model.meshCount; i++){
                    var mesh = model.meshes[i];
                    RenderCommandGenerator_Inline.GenerateCommandsForMesh(
                        _commandList,
                        modelState.VertexBuffers[i],
                        modelState.IndexBuffers[i],
                        _sceneRuntimeState.CameraProjViewBuffer,
                        _sceneRuntimeState.MaterialBuffer,
                        _sceneRuntimeState.CameraResourceSet,
                        _sceneRuntimeState.LightResourceSet,
                        _sceneRuntimeState.MaterialResourceSet,
                        modelState.TextureResourceSets[i],
                        mesh
                    );
                }
            }
             
            _commandList.End();
        }

        override protected void Draw(){
            graphicsDevice.SubmitCommands(_commandList);
        }


    }
    
}