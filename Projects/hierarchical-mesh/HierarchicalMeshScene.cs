using System;
using System.Numerics;
using Veldrid;
using Henzai.Core.Extensions;
using Henzai.Core;
using Henzai.Core.VertexGeometry;
using Henzai.Core.Materials;
using Henzai.Geometry;
using Henzai.Runtime;

namespace Henzai.Examples
{
    sealed class HierarchicalMeshScene : Renderable
    {

        Model<VertexPositionNormalTextureTangentBitangent, RealtimeMaterial> _nanosuit;
        Model<VertexPositionNormal, RealtimeMaterial> _sun;

        public HierarchicalMeshScene(string title,Resolution windowSize, GraphicsDeviceOptions graphicsDeviceOptions, RenderOptions renderOptions)
            : base(title,windowSize,graphicsDeviceOptions,renderOptions){

                //PreDraw+=RotateModel;
        }

        private void RotateModel(float delta){
            var radian_slow = (Math.PI.ToFloat()/180.0f)*delta*10.0f;
            var radian_fast = (Math.PI.ToFloat()/180.0f)*delta*100.0f;

            // Rotate  nanosuit around itself
            var newWorld = _nanosuit.GetWorld_DontMutate*Matrix4x4.CreateRotationY(radian_fast);

            // Rotate around Sun without rotation around oneself
            Vector3 pos  = _nanosuit.GetWorld_DontMutate.Translation;
            Vector3 sunPos = _sun.GetWorld_DontMutate.Translation;
            Quaternion rotationAroundY = Quaternion.CreateFromAxisAngle(Vector3.UnitY,radian_slow);
            pos -= sunPos;
            pos = Vector3.Transform(pos, rotationAroundY);
            pos += sunPos;

            // add the translating rotation to the orientation defining rotation
            newWorld.Translation = pos;
            _nanosuit.SetNewWorldTransformation(ref newWorld,true);


            // Sun around nanosuit
            // Vector3 suitToSun = _sun.GetWorld_DontMutate.Translation - _nanosuit.GetWorld_DontMutate.Translation;
            // Quaternion rotationAroundY_2 = Quaternion.CreateFromAxisAngle(Vector3.UnitY,radian);
            // Vector3 newPos_2 = Vector3.Transform(suitToSun,rotationAroundY_2);
            // _sun.SetNewWorldTranslation(ref newPos_2,true);
        }

        override protected void CreateResources(){

            // RgbaFloat lightColor = RgbaFloat.Orange;
            RgbaFloat lightColor = RgbaFloat.LightGrey;
            _sceneRuntimeState.Light = new Light(lightColor,0.1f);
            _sceneRuntimeState.Camera = Camera;
            _sceneRuntimeState.SpotLight = Light.NO_POINTLIGHT;

            // string filePath = Path.Combine(AppContext.BaseDirectory, "armor/armor.dae"); 
            // string filePath = Path.Combine(AppContext.BaseDirectory, "nanosuit/nanosuit.obj"); 
            _nanosuit = AssimpLoader.LoadFromFileWithRealtimeMaterial<VertexPositionNormalTextureTangentBitangent>(AppContext.BaseDirectory,"nanosuit/nanosuit.obj", VertexPositionNormalTextureTangentBitangent.HenzaiType);
            // _nanosuit.SetAmbientForAllMeshes(new Vector4(0.1f,0.1f,0.1f,1.0f));
            // _model = AssimpLoader.LoadFromFile<VertexPositionNormalTextureTangentBitangent>(AppContext.BaseDirectory,"sponza/sponza.obj",VertexPositionNormalTextureTangentBitangent.HenzaiType);
            GeometryUtils.GenerateTangentAndBitagentSpaceFor(_nanosuit);
            // GeometryUtils.CheckTBN(_model);
            // var sun = new Model<VertexPositionNormalTextureTangentBitangent>("water",GeometryFactory.generateSphereTangentBitangent(100,100,1));
            _sun = new Model<VertexPositionNormal, RealtimeMaterial>(string.Empty, GeometryFactory.GenerateSphereNormal(100,100,1), new RealtimeMaterial());
            var meshZero = _sun.GetMesh(0);
            var materialZero = _sun.GetMaterial(0);
            materialZero.textureDiffuse = "Water.jpg";
            materialZero.textureNormal = "WaterNorm.jpg";
            materialZero.ambient = lightColor.ToVector4();
            // _sun.meshes[0].TryGetMaterial().ambient = lightColor.ToVector4();
            Vector4 lightPos = _sceneRuntimeState.Light.LightPos;
            Vector3 newTranslation = new Vector3(lightPos.X,lightPos.Y,lightPos.Z);
            _sun.SetNewWorldTranslation(ref newTranslation, true);

            var nanoSuitRuntimeState = new ModelRuntimeDescriptor<VertexPositionNormalTextureTangentBitangent>(_nanosuit,"PhongBitangentTexture","PhongBitangentTexture", VertexRuntimeTypes.VertexPositionNormalTextureTangentBitangent,PrimitiveTopology.TriangleList);
            nanoSuitRuntimeState.CallVertexLayoutGeneration+=ResourceGenerator.GenerateVertexLayoutForPNTTB;
            nanoSuitRuntimeState.CallSamplerGeneration+=ResourceGenerator.GenerateTriLinearSampler;
            nanoSuitRuntimeState.CallTextureResourceLayoutGeneration+=ResourceGenerator.GenerateTextureResourceLayoutForNormalMapping;
            nanoSuitRuntimeState.CallTextureResourceSetGeneration+=ResourceGenerator.GenerateTextureResourceSetForNormalMapping;

            // var sunRuntimeState = new ModelRuntimeState<VertexPositionNormalTextureTangentBitangent>(sun,"PhongBitangentTexture","PhongBitangentTexture");
            // sunRuntimeState.CallVertexLayoutGeneration+=ResourceGenerator.GenerateVertexLayoutForPNTTB;
            // sunRuntimeState.CallSamplerGeneration+=ResourceGenerator.GenerateLinearSampler;
            // sunRuntimeState.CallTextureResourceLayoutGeneration+=ResourceGenerator.GenerateTextureResourceLayoutForNormalMapping;
            // sunRuntimeState.CallTextureResourceSetGeneration+=ResourceGenerator.GenerateTextureResourceSetForNormalMapping;

            _modelPNTTBDescriptorList.Add(nanoSuitRuntimeState);
            // _modelStatesList.Add(sunRuntimeState);

            var sunRuntimeState = new ModelRuntimeDescriptor<VertexPositionNormal>(_sun,"Phong","Phong", VertexRuntimeTypes.VertexPositionNormal,PrimitiveTopology.TriangleList);
            sunRuntimeState.CallVertexLayoutGeneration+=ResourceGenerator.GenerateVertexLayoutForPN;
            _modelPNDescriptorList.Add(sunRuntimeState);


            foreach(var modelDescriptor in _modelPNTTBDescriptorList){
                FillRuntimeDescriptor(modelDescriptor,_sceneRuntimeState,InstancingData.NO_DATA); 
                PNTTBRuntimeGeometry.AddModel(modelDescriptor);
            }

            foreach(var modelDescriptor in _modelPNDescriptorList){
                FillRuntimeDescriptor(modelDescriptor,_sceneRuntimeState,InstancingData.NO_DATA); 
                PNRuntimeGeometry.AddModel(modelDescriptor);
            }

        }

        override protected void BuildCommandList(){
            _commandList.Begin();
            _commandList.SetFramebuffer(GraphicsDevice.SwapchainFramebuffer);
            _commandList.SetFullViewports();

            _commandList.ClearColorTarget(0,RgbaFloat.White);
            _commandList.ClearDepthStencil(1f);

            RenderCommandGenerator.GenerateCommandsForScene_Inline(
                _commandList,
                _sceneRuntimeState.CameraProjViewBuffer,
                _sceneRuntimeState.LightBuffer,
                _sceneRuntimeState.SpotLightBuffer,
                _sceneRuntimeState.Camera,
                _sceneRuntimeState.Light,
                _sceneRuntimeState.SpotLight);

            RenderCommandGenerator.GenerateRenderCommandsForModelDescriptor(_commandList,_modelPNTTBDescriptorArray,_sceneRuntimeState);
            RenderCommandGenerator.GenerateRenderCommandsForModelDescriptor(_commandList,_modelPNDescriptorArray,_sceneRuntimeState);
            
            _commandList.End();
        }
    }
    
}