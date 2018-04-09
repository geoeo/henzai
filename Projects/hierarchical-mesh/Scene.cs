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
    sealed class Scene : Renderable
    {

        private SceneRuntimeDescriptor _sceneRuntimeState;

        private List<ModelRuntimeDescriptor<VertexPositionNormalTextureTangentBitangent>> _modelPNTTBDescriptorList;
        private ModelRuntimeDescriptor<VertexPositionNormalTextureTangentBitangent> [] _modelPNTTBDescriptorArray;

        private List<ModelRuntimeDescriptor<VertexPositionNormal>> _modelPNDescriptorList;
        private ModelRuntimeDescriptor<VertexPositionNormal> [] _modelPNDescriptorArray;

        Model<VertexPositionNormalTextureTangentBitangent> _nanosuit;
        Model<VertexPositionNormal> _sun;

        public Scene(string title,Resolution windowSize, GraphicsDeviceOptions graphicsDeviceOptions, RenderOptions renderOptions)
            : base(title,windowSize,graphicsDeviceOptions,renderOptions){
                _sceneRuntimeState = new SceneRuntimeDescriptor();

                _modelPNTTBDescriptorList = new List<ModelRuntimeDescriptor<VertexPositionNormalTextureTangentBitangent>>();
                _modelPNDescriptorList = new List<ModelRuntimeDescriptor<VertexPositionNormal>>();


                PreDraw+=RotateModel;
                PreRenderLoop+=FormatResourcesForRuntime;
        }

        private void RotateModel(float delta){
            var radian_slow = (Math.PI.ToFloat()/180.0f)*delta*10.0f;
            var radian_fast = (Math.PI.ToFloat()/180.0f)*delta*100.0f;

            // Rotate  nanosuit around itself
            var newWorld = _nanosuit.GetWorld_DontMutate*Matrix4x4.CreateRotationY(radian_fast);

            // Rotate around Sun without rotation around oneself
            Vector3 pos  = _nanosuit.GetWorld_DontMutate.Translation;
            Vector3 sunToSuit = _sun.GetWorld_DontMutate.Translation;
            Quaternion rotationAroundY = Quaternion.CreateFromAxisAngle(Vector3.UnitY,radian_slow);
            pos -= sunToSuit;
            pos = Vector3.Transform(pos,rotationAroundY);
            pos += sunToSuit;

            // add the translating rotation to the orientation defining rotation
            newWorld.Translation = pos;
            _nanosuit.SetNewWorldTransformation(ref newWorld,true);


            // Sun around nanosuit
            // Vector3 suitToSun = _sun.GetWorld_DontMutate.Translation - _nanosuit.GetWorld_DontMutate.Translation;
            // Quaternion rotationAroundY_2 = Quaternion.CreateFromAxisAngle(Vector3.UnitY,radian);
            // Vector3 newPos_2 = Vector3.Transform(suitToSun,rotationAroundY_2);
            // _sun.SetNewWorldTranslation(ref newPos_2,true);
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

            // RgbaFloat lightColor = RgbaFloat.Orange;
            RgbaFloat lightColor = RgbaFloat.LightGrey;
            _sceneRuntimeState.Light = new Light(lightColor);
            _sceneRuntimeState.Camera = Camera;
            _sceneRuntimeState.PointLight = Light.NO_POINTLIGHT;

            // string filePath = Path.Combine(AppContext.BaseDirectory, "armor/armor.dae"); 
            // string filePath = Path.Combine(AppContext.BaseDirectory, "nanosuit/nanosuit.obj"); 
            _nanosuit = AssimpLoader.LoadFromFile<VertexPositionNormalTextureTangentBitangent>(AppContext.BaseDirectory,"nanosuit/nanosuit.obj",VertexPositionNormalTextureTangentBitangent.HenzaiType);
            _nanosuit.SetAmbientForAllMeshes(new Vector4(0.1f,0.1f,0.1f,1.0f));
            // _model = AssimpLoader.LoadFromFile<VertexPositionNormalTextureTangentBitangent>(AppContext.BaseDirectory,"sponza/sponza.obj",VertexPositionNormalTextureTangentBitangent.HenzaiType);
            GeometryUtils.GenerateTangentAndBitagentSpaceFor(_nanosuit);
            // GeometryUtils.CheckTBN(_model);
            // var sun = new Model<VertexPositionNormalTextureTangentBitangent>("water",GeometryFactory.generateSphereTangentBitangent(100,100,1));
            _sun = new Model<VertexPositionNormal>(String.Empty,GeometryFactory.GenerateSphereNormal(100,100,1));
            _sun.meshes[0].TryGetMaterial().textureDiffuse = "Water.jpg";
            _sun.meshes[0].TryGetMaterial().textureNormal = "WaterNorm.jpg";
            _sun.meshes[0].TryGetMaterial().ambient = new Vector4(1.0f,1.0f,1.0f,1.0f);
            // _sun.meshes[0].TryGetMaterial().ambient = lightColor.ToVector4();
            ref Vector4 lightPos = ref _sceneRuntimeState.Light.Light_DontMutate;
            Vector3 newTranslation = new Vector3(lightPos.X,lightPos.Y,lightPos.Z);
            _sun.SetNewWorldTranslation(ref newTranslation, true);

            var nanoSuitRuntimeState = new ModelRuntimeDescriptor<VertexPositionNormalTextureTangentBitangent>(_nanosuit,"PhongBitangentTexture","PhongBitangentTexture", VertexTypes.VertexPositionNormalTextureTangentBitangent,PrimitiveTopology.TriangleList);
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

            var sunRuntimeState = new ModelRuntimeDescriptor<VertexPositionNormal>(_sun,"Phong","Phong",VertexTypes.VertexPositionNormal,PrimitiveTopology.TriangleList);
            sunRuntimeState.CallVertexLayoutGeneration+=ResourceGenerator.GenerateVertexLayoutForPN;
            _modelPNDescriptorList.Add(sunRuntimeState);

            /// Uniform 1 - Camera
            _sceneRuntimeState.CameraProjViewBuffer  = _factory.CreateBuffer(new BufferDescription(Camera.SizeInBytes,BufferUsage.UniformBuffer | BufferUsage.Dynamic));
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
            _sceneRuntimeState.MaterialBuffer = _factory.CreateBuffer(new BufferDescription(Material.SizeInBytes,BufferUsage.UniformBuffer));
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
            _sceneRuntimeState.LightBuffer = _factory.CreateBuffer(new BufferDescription(Light.SizeInBytes,BufferUsage.UniformBuffer));
            _sceneRuntimeState.LightResourceLayout 
                = ResourceGenerator.GenerateResourceLayout(
                    _factory,
                    "light",
                    ResourceKind.UniformBuffer,
                    ShaderStages.Vertex | ShaderStages.Fragment);
            _sceneRuntimeState.LightResourceSet 
                = ResourceGenerator.GenrateResourceSet(
                    _factory,
                    _sceneRuntimeState.LightResourceLayout,
                    new BindableResource[]{_sceneRuntimeState.LightBuffer});

                // Uniform 4 - PointLight
            _sceneRuntimeState.PointLightBuffer = _factory.CreateBuffer(new BufferDescription(4*4*4,BufferUsage.UniformBuffer));
            _sceneRuntimeState.PointLightResourceLayout 
                = ResourceGenerator.GenerateResourceLayout(
                    _factory,
                    "pointlight",
                    ResourceKind.UniformBuffer,
                    ShaderStages.Fragment);
            _sceneRuntimeState.PointLightResourceSet 
                = ResourceGenerator.GenrateResourceSet(
                    _factory,
                    _sceneRuntimeState.PointLightResourceLayout,
                    new BindableResource[]{_sceneRuntimeState.PointLightBuffer});

            foreach(var modelDescriptor in _modelPNTTBDescriptorList){
                FillRuntimeDescriptor(modelDescriptor,_sceneRuntimeState,InstancingData.NO_DATA); 
            }

            foreach(var modelDescriptor in _modelPNDescriptorList){
                FillRuntimeDescriptor(modelDescriptor,_sceneRuntimeState,InstancingData.NO_DATA); 
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