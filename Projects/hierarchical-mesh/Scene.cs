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

        private SceneRuntimeDescriptor _sceneRuntimeState;

        private List<ModelRuntimeDescriptor<VertexPositionNormalTextureTangentBitangent>> _modelPNTTBDescriptorList;
        private ModelRuntimeDescriptor<VertexPositionNormalTextureTangentBitangent> [] _modelPNTTBDescriptorArray;

        private List<ModelRuntimeDescriptor<VertexPositionNormal>> _modelPNDescriptorList;
        private ModelRuntimeDescriptor<VertexPositionNormal> [] _modelPNDescriptorArray;

        Model<VertexPositionNormalTextureTangentBitangent> _model;

        public Scene(string title,Resolution windowSize, GraphicsDeviceOptions graphicsDeviceOptions, RenderOptions renderOptions)
            : base(title,windowSize,graphicsDeviceOptions,renderOptions){
                _sceneRuntimeState = new SceneRuntimeDescriptor();

                _modelPNTTBDescriptorList = new List<ModelRuntimeDescriptor<VertexPositionNormalTextureTangentBitangent>>();
                _modelPNDescriptorList = new List<ModelRuntimeDescriptor<VertexPositionNormal>>();


                PreDraw+=RotateModel;
                PreRenderLoop+=FormatResourcesForRuntime;
        }

        private void RotateModel(float delta){
            var newWorld = _model.GetWorld_DontMutate*Matrix4x4.CreateRotationY(Math.PI.ToFloat()*delta/10.0f);
            _model.SetNewWorldTransformation(ref newWorld,true); 
        }

        // TODO: Investigate putting this in renderable
        override protected void FormatResourcesForRuntime(){

            foreach(var modelState in _modelPNTTBDescriptorList)
                modelState.FormatResourcesForRuntime();
            foreach(var modelState in _modelPNDescriptorList)
                modelState.FormatResourcesForRuntime();


            _modelPNTTBDescriptorArray = _modelPNTTBDescriptorList.ToArray();
            _modelPNDescriptorArray = _modelPNDescriptorList.ToArray();
        }

        override protected void CreateResources(){

            _sceneRuntimeState.Light = new Light();
            _sceneRuntimeState.Camera = Camera;

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
            // sun.meshes[0].TryGetMaterial().ambient = new Vector4(1.0f,0.0f,0.0f,1.0f);
            sun.meshes[0].TryGetMaterial().ambient = RgbaFloat.Orange.ToVector4();
            ref Vector4 lightPos = ref _sceneRuntimeState.Light.Light_DontMutate;
            Vector3 newTranslation = new Vector3(lightPos.X,lightPos.Y,lightPos.Z);
            sun.SetNewWorldTranslation(ref newTranslation, true);

            var nanoSuitRuntimeState = new ModelRuntimeDescriptor<VertexPositionNormalTextureTangentBitangent>(_model,"PhongBitangentTexture","PhongBitangentTexture", VertexTypes.VertexPositionNormalTextureTangentBitangent);
            nanoSuitRuntimeState.CallVertexLayoutGeneration+=ResourceGenerator.GenerateVertexLayoutForPNTTB;
            nanoSuitRuntimeState.CallSamplerGeneration+=ResourceGenerator.GenerateLinearSampler;
            nanoSuitRuntimeState.CallTextureResourceLayoutGeneration+=ResourceGenerator.GenerateTextureResourceLayoutForNormalMapping;
            nanoSuitRuntimeState.CallTextureResourceSetGeneration+=ResourceGenerator.GenerateTextureResourceSetForNormalMapping;

            // var sunRuntimeState = new ModelRuntimeState<VertexPositionNormalTextureTangentBitangent>(sun,"PhongBitangentTexture","PhongBitangentTexture");
            // sunRuntimeState.CallVertexLayoutGeneration+=ResourceGenerator.GenerateVertexLayoutForPNTTB;
            // sunRuntimeState.CallSamplerGeneration+=ResourceGenerator.GenerateLinearSampler;
            // sunRuntimeState.CallTextureResourceLayoutGeneration+=ResourceGenerator.GenerateTextureResourceLayoutForNormalMapping;
            // sunRuntimeState.CallTextureResourceSetGeneration+=ResourceGenerator.GenerateTextureResourceSetForNormalMapping;

            _modelPNTTBDescriptorList.Add(nanoSuitRuntimeState);
            // _modelStatesList.Add(sunRuntimeState);

            var sunRuntimeState = new ModelRuntimeDescriptor<VertexPositionNormal>(sun,"Phong","Phong",VertexTypes.VertexPositionNormal);
            sunRuntimeState.CallVertexLayoutGeneration+=ResourceGenerator.GenerateVertexLayoutForPN;
            _modelPNDescriptorList.Add(sunRuntimeState);

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
                    ShaderStages.Vertex);
            _sceneRuntimeState.LightResourceSet 
                = ResourceGenerator.GenrateResourceSet(
                    _factory,
                    _sceneRuntimeState.LightResourceLayout,
                    new BindableResource[]{_sceneRuntimeState.LightBuffer});

            foreach(var modelDescriptor in _modelPNTTBDescriptorList){
                FillRuntimeDescriptor(modelDescriptor,_sceneRuntimeState); 
            }

            foreach(var modelDescriptor in _modelPNDescriptorList){
                FillRuntimeDescriptor(modelDescriptor,_sceneRuntimeState); 
            }

        }

        override protected void BuildCommandList(){
            _commandList.Begin();
            _commandList.SetFramebuffer(GraphicsDevice.SwapchainFramebuffer);
            _commandList.SetFullViewports();

            _commandList.ClearColorTarget(0,RgbaFloat.White);
            _commandList.ClearDepthStencil(1f);

            RenderCommandGenerator.GenerateRenderCommandsForModelDescriptor(_commandList,_modelPNTTBDescriptorArray,_sceneRuntimeState);
            RenderCommandGenerator.GenerateRenderCommandsForModelDescriptor(_commandList,_modelPNDescriptorArray,_sceneRuntimeState);
            
            _commandList.End();
        }

        override protected void Draw(){
            GraphicsDevice.SubmitCommands(_commandList);
        }


    }
    
}