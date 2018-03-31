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

        private List<ModelRuntimeState<VertexPositionNormalTextureTangentBitangent>> _modelStatesList;
        private ModelRuntimeState<VertexPositionNormalTextureTangentBitangent> [] _modelStates;

        private List<ModelRuntimeState<VertexPositionNormal>> _modelStatesPNList;
        private ModelRuntimeState<VertexPositionNormal> [] _modelStatesPN;

        Model<VertexPositionNormalTextureTangentBitangent> _model;

        public Scene(string title,Resolution windowSize, GraphicsDeviceOptions graphicsDeviceOptions, RenderOptions renderOptions)
            : base(title,windowSize,graphicsDeviceOptions,renderOptions){
                _sceneRuntimeState = new SceneRuntimeState();

                _modelStatesList = new List<ModelRuntimeState<VertexPositionNormalTextureTangentBitangent>>();
                _modelStatesPNList = new List<ModelRuntimeState<VertexPositionNormal>>();


                PreDraw+=RotateModel;
                PreRenderLoop+=FormatResourcesForRuntime;
        }

        private void RotateModel(float delta){
            var newWorld = _model.GetWorld_DontMutate*Matrix4x4.CreateRotationY(Math.PI.ToFloat()*delta/10.0f);
            _model.SetNewWorldTransformation(ref newWorld,true); 
        }

        private void FormatResourcesForRuntime(){

            foreach(var modelState in _modelStatesList)
                modelState.FormatResourcesForRuntime();
            foreach(var modelState in _modelStatesPNList)
                modelState.FormatResourcesForRuntime();


            _modelStates = _modelStatesList.ToArray();
            _modelStatesPN = _modelStatesPNList.ToArray();
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
            // var sun = new Model<VertexPositionNormalTextureTangentBitangent>("water",GeometryFactory.generateSphereTangentBitangent(100,100,1));
            var sun = new Model<VertexPositionNormal>(String.Empty,GeometryFactory.generateSphereNormal(100,100,1));
            sun.meshes[0].TryGetMaterial().textureDiffuse = "Water.jpg";
            sun.meshes[0].TryGetMaterial().textureNormal = "WaterNorm.jpg";
            sun.meshes[0].TryGetMaterial().ambient = new Vector4(1.0f,0.0f,0.0f,1.0f);
            ref Vector4 lightPos = ref _sceneRuntimeState.Light.Light_DontMutate;
            Vector3 newTranslation = new Vector3(lightPos.X,lightPos.Y,lightPos.Z);
            sun.SetNewWorldTranslation(ref newTranslation, true);

            var nanoSuitRuntimeState = new ModelRuntimeState<VertexPositionNormalTextureTangentBitangent>(_model,"PhongBitangentTexture","PhongBitangentTexture", VertexTypes.VertexPositionNormalTextureTangentBitangent);
            nanoSuitRuntimeState.CallVertexLayoutGeneration+=ResourceGenerator.GenerateVertexLayoutForPNTTB;
            nanoSuitRuntimeState.CallSamplerGeneration+=ResourceGenerator.GenerateLinearSampler;
            nanoSuitRuntimeState.CallTextureResourceLayoutGeneration+=ResourceGenerator.GenerateTextureResourceLayoutForNormalMapping;
            nanoSuitRuntimeState.CallTextureResourceSetGeneration+=ResourceGenerator.GenerateTextureResourceSetForNormalMapping;

            // var sunRuntimeState = new ModelRuntimeState<VertexPositionNormalTextureTangentBitangent>(sun,"PhongBitangentTexture","PhongBitangentTexture");
            // sunRuntimeState.CallVertexLayoutGeneration+=ResourceGenerator.GenerateVertexLayoutForPNTTB;
            // sunRuntimeState.CallSamplerGeneration+=ResourceGenerator.GenerateLinearSampler;
            // sunRuntimeState.CallTextureResourceLayoutGeneration+=ResourceGenerator.GenerateTextureResourceLayoutForNormalMapping;
            // sunRuntimeState.CallTextureResourceSetGeneration+=ResourceGenerator.GenerateTextureResourceSetForNormalMapping;

            _modelStatesList.Add(nanoSuitRuntimeState);
            // _modelStatesList.Add(sunRuntimeState);

            var sunRuntimeState = new ModelRuntimeState<VertexPositionNormal>(sun,"Phong","Phong",VertexTypes.VertexPositionNormal);
            sunRuntimeState.CallVertexLayoutGeneration+=ResourceGenerator.GenerateVertexLayoutForPN;
            _modelStatesPNList.Add(sunRuntimeState);

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

            foreach(var modelState in _modelStatesList){

                var model = modelState.Model;

                modelState.TextureResourceLayout = modelState.InvokeTextureResourceLayoutGeneration(_factory);

                modelState.TextureSampler = modelState.InvokeSamplerGeneration(_factory);

                modelState.LoadShaders(GraphicsDevice);

                for(int i = 0; i < model.meshCount; i++){

                    DeviceBuffer vertexBuffer 
                        =  _factory.CreateBuffer(new BufferDescription(model.meshes[i].vertices.LengthUnsigned() * VertexPositionNormalTextureTangentBitangent.SizeInBytes, BufferUsage.VertexBuffer)); 

                    DeviceBuffer indexBuffer
                        = _factory.CreateBuffer(new BufferDescription(model.meshes[i].meshIndices.LengthUnsigned()*sizeof(uint),BufferUsage.IndexBuffer));
                        

                    modelState.VertexBuffersList.Add(vertexBuffer);
                    modelState.IndexBuffersList.Add(indexBuffer);

                    GraphicsDevice.UpdateBuffer(vertexBuffer,0,model.meshes[i].vertices);
                    GraphicsDevice.UpdateBuffer(indexBuffer,0,model.meshes[i].meshIndices);

                    modelState.TextureResourceSetsList.Add(
                        modelState.InvokeTextureResourceSetGeneration(i,_factory,GraphicsDevice)
                        );
                }
                modelState.VertexLayout = modelState.InvokeVertexLayoutGeneration();

                switch(modelState.VertexType){
                    case VertexTypes.VertexPositionNormal:
                        modelState.Pipeline = _factory.CreateGraphicsPipeline(ResourceGenerator.GeneratePipelinePN(modelState,_sceneRuntimeState,GraphicsDevice));
                        break;
                    case VertexTypes.VertexPositionNormalTextureTangentBitangent:
                            modelState.Pipeline = _factory.CreateGraphicsPipeline(ResourceGenerator.GeneratePipelinePNTTB(modelState,_sceneRuntimeState,GraphicsDevice));
                        break;
                    default:
                        throw new NotImplementedException($"{modelState.VertexType.ToString("g")} not implemented");
                }
 
            }
            
            _commandList = _factory.CreateCommandList();

        }

        override protected void BuildCommandList(){
            _commandList.Begin();
            _commandList.SetFramebuffer(GraphicsDevice.SwapchainFramebuffer);
            _commandList.SetFullViewports();

            _commandList.ClearColorTarget(0,RgbaFloat.White);
            _commandList.ClearDepthStencil(1f);

            for(int j = 0; j < _modelStates.Length; j++){
                var modelState = _modelStates[j];
                var model = modelState.Model;
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
            GraphicsDevice.SubmitCommands(_commandList);
        }


    }
    
}