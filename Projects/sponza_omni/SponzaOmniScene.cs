using System;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;
using Henzai.Core;
using Henzai.Core.VertexGeometry;
using Henzai.Core.Materials;
using Henzai.Geometry;
using Henzai.Runtime;
using Henzai.Cameras;
using Henzai.Effects;

namespace Henzai.Examples
{
    sealed class SponzaOmniScene : Renderable
    {

        private Model<VertexPositionNormal, RealtimeMaterial> _sun;
        private ModelRuntimeDescriptor<VertexPosition> _skyBoxRuntimeState;

        public SponzaOmniScene(string title,Resolution windowSize, GraphicsDeviceOptions graphicsDeviceOptions, RenderOptions renderOptions)
            : base(title,windowSize,graphicsDeviceOptions,renderOptions){
        }

        public SponzaOmniScene(string title, Sdl2Window contextWindow, GraphicsDeviceOptions graphicsDeviceOptions, RenderOptions renderOptions)
            : base(title,contextWindow,graphicsDeviceOptions,renderOptions){
        }

        override protected void CreateResources(){

            RgbaFloat lightColor = RgbaFloat.White;
            // RgbaFloat lightColor = RgbaFloat.LightGrey;
            var lightPos = new Vector4(0,30,0,1);
            var lookAt = new Vector4(0,0,0,1)- new Vector4(0,50,0,1);
            var lightCam = new OrthographicCamera(50.0f, 50.0f, lightPos, lookAt);
            //_sceneRuntimeState.Light = new Light(lightCam,lightColor,0.1f);
            _sceneRuntimeState.Camera = Camera;
            _sceneRuntimeState.SpotLight = Light.NO_POINTLIGHT;

            var omniCameras =  CubeMap.GenerateOmniCameras(lightPos,1024,1024);
            _sceneRuntimeState.OmniLights = Light.GenerateOmniLights(omniCameras,lightColor,0.1f);

            // string filePath = Path.Combine(AppContext.BaseDirectory, "armor/armor.dae"); 
            // string filePath = Path.Combine(AppContext.BaseDirectory, "nanosuit/nanosuit.obj"); 

            var scale = Matrix4x4.CreateScale(0.05f,0.05f,0.05f);
            //var scale = Matrix4x4.CreateScale(1.00f,1.00f,1.00f);


            string filePath = "models/chinesedragon.dae";
            // string filePath = "Models/Box.dae";
            var model = AssimpLoader.LoadFromFileWithRealtimeMaterial<VertexPositionNormal>(AppContext.BaseDirectory, filePath, VertexPositionNormal.HenzaiType);
            var newModelTranslation = Matrix4x4.CreateTranslation(new Vector3(0,20,0));
            var modelRuntimeState 
                = new ModelRuntimeDescriptor<VertexPositionNormal>(model,"PhongOmni","PhongOmni", VertexRuntimeTypes.VertexPositionNormal,PrimitiveTopology.TriangleList, new RenderDescription(RenderFlags.NORMAL | RenderFlags.OMNI_SHADOW_MAPS), new InstancingRenderDescription(RenderFlags.NONE, InstancingDataFlags.EMPTY));
            modelRuntimeState.CallVertexLayoutGeneration+=ResourceGenerator.GenerateVertexLayoutForPN;
            model.SetNewWorldTransformation(ref newModelTranslation, true);
            
            
            //TODO: Write method to remove ambient terms
            var sponzaModels = AssimpLoader.LoadRealtimeModelsFromFile(AppContext.BaseDirectory,"sponza/sponza.obj");
            var sponzaPNTTB = sponzaModels.modelPNTTB;
            var sponzaPC = sponzaModels.modelPC;

            for( int i = 0; i < sponzaPNTTB.MaterialCount; i++)
                sponzaPNTTB.GetMaterial(i).ambient =new Vector4(0.3f,0.3f,0.3f,1.0f);
            
            for( int i = 0; i < sponzaPC.MaterialCount; i++)
                sponzaPC.GetMaterial(i).ambient =new Vector4(0.3f,0.3f,0.3f,1.0f);
            sponzaPNTTB.SetNewWorldTransformation(ref scale, true);
            sponzaPC.SetNewWorldTransformation(ref scale, true);

            var sponzaRuntimeState  //TOOD: Omni Fragment shader produces a runtime error
                = new ModelRuntimeDescriptor<VertexPositionNormalTextureTangentBitangent>(sponzaPNTTB, "PhongBitangentTextureOmni", "PhongBitangentTextureOmni", VertexRuntimeTypes.VertexPositionNormalTextureTangentBitangent, PrimitiveTopology.TriangleList, new RenderDescription(RenderFlags.NORMAL | RenderFlags.OMNI_SHADOW_MAPS), new InstancingRenderDescription(RenderFlags.NONE, InstancingDataFlags.EMPTY));
            sponzaRuntimeState.CallVertexLayoutGeneration += ResourceGenerator.GenerateVertexLayoutForPNTTB;
            sponzaRuntimeState.CallSamplerGeneration += ResourceGenerator.GenerateTriLinearSampler;
            sponzaRuntimeState.CallTextureResourceLayoutGeneration += ResourceGenerator.GenerateTextureResourceLayoutForNormalMapping;
            sponzaRuntimeState.CallTextureResourceSetGeneration += ResourceGenerator.GenerateTextureResourceSetForNormalMapping;


            var sponzaRuntimeStateColorOnly = new ModelRuntimeDescriptor<VertexPositionColor>(sponzaPC, "Color", "Color", VertexRuntimeTypes.VertexPositionColor, PrimitiveTopology.TriangleList, new RenderDescription(RenderFlags.NORMAL | RenderFlags.OMNI_SHADOW_MAPS), new InstancingRenderDescription(RenderFlags.NONE, InstancingDataFlags.EMPTY));
            sponzaRuntimeStateColorOnly.CallVertexLayoutGeneration += ResourceGenerator.GenerateVertexLayoutForPC;

            ///

            var floor = new Model<VertexPositionNormalTextureTangentBitangent, RealtimeMaterial>("paving/", GeometryFactory.GenerateQuadPNTTB_XY(), new RealtimeMaterial());
            var floorMeshZero = floor.GetMesh(0);
            var flootMaterialZero = floor.GetMaterial(0);
            flootMaterialZero.textureDiffuse = "pavingColor.jpg";
            flootMaterialZero.textureNormal = "pavingNorm.jpg";
            flootMaterialZero.ambient = new Vector4(0.3f, 0.3f, 0.3f, 1.0f);
            var floorTranslation = Matrix4x4.CreateTranslation(0, 0, -50);
            var floorScale = Matrix4x4.CreateScale(100.0f, 100.0f, 100.0f);
            var newTrans = Matrix4x4.Multiply(floorScale, floorTranslation);
            floor.SetNewWorldTransformation(ref newTrans, true);
            var floorRuntimeState = new ModelRuntimeDescriptor<VertexPositionNormalTextureTangentBitangent>(floor, "PhongBitangentTextureOmni", "PhongBitangentTextureOmni", VertexRuntimeTypes.VertexPositionNormalTextureTangentBitangent, PrimitiveTopology.TriangleStrip, new RenderDescription(RenderFlags.NORMAL | RenderFlags.OMNI_SHADOW_MAPS), new InstancingRenderDescription(RenderFlags.NONE, InstancingDataFlags.EMPTY));

            floorRuntimeState.CallVertexLayoutGeneration += ResourceGenerator.GenerateVertexLayoutForPNTTB;
            floorRuntimeState.CallTextureResourceLayoutGeneration += ResourceGenerator.GenerateTextureResourceLayoutForNormalMapping;
            floorRuntimeState.CallTextureResourceSetGeneration += ResourceGenerator.GenerateTextureResourceSetForNormalMapping;
            floorRuntimeState.CallSamplerGeneration += ResourceGenerator.GenerateTriLinearSampler;


            /////
            var floor2 = new Model<VertexPositionNormalTextureTangentBitangent, RealtimeMaterial>("paving/", GeometryFactory.GenerateQuadPNTTB_XZ(), new RealtimeMaterial());
            var floorMeshZero2 = floor2.GetMesh(0);
            var flootMaterialZero2 = floor2.GetMaterial(0);
            flootMaterialZero2.textureDiffuse = "pavingColor.jpg";
            flootMaterialZero2.textureNormal = "pavingNorm.jpg";
            flootMaterialZero2.ambient = new Vector4(0.3f, 0.3f, 0.3f, 1.0f);
            var floorTranslation2 = Matrix4x4.CreateTranslation(0, -10, 0);
            var floorScale2 = Matrix4x4.CreateScale(100.0f, 100.0f, 100.0f);
            var newTrans2 = Matrix4x4.Multiply(floorScale2, floorTranslation2);
            floor2.SetNewWorldTransformation(ref newTrans2, true);
            var floorRuntimeState2 = new ModelRuntimeDescriptor<VertexPositionNormalTextureTangentBitangent>(floor2, "PhongBitangentTextureOmni", "PhongBitangentTextureOmni", VertexRuntimeTypes.VertexPositionNormalTextureTangentBitangent, PrimitiveTopology.TriangleStrip, new RenderDescription(RenderFlags.NORMAL | RenderFlags.OMNI_SHADOW_MAPS), new InstancingRenderDescription(RenderFlags.NONE, InstancingDataFlags.EMPTY));

            floorRuntimeState2.CallVertexLayoutGeneration += ResourceGenerator.GenerateVertexLayoutForPNTTB;
            floorRuntimeState2.CallTextureResourceLayoutGeneration += ResourceGenerator.GenerateTextureResourceLayoutForNormalMapping;
            floorRuntimeState2.CallTextureResourceSetGeneration += ResourceGenerator.GenerateTextureResourceSetForNormalMapping;
            floorRuntimeState2.CallSamplerGeneration += ResourceGenerator.GenerateTriLinearSampler;

            //



            var skyBox = new Model<VertexPosition, RealtimeMaterial>("cloudtop", GeometryFactory.GenerateCube(true), new RealtimeMaterial());
            var skyBoxMaterial = skyBox.GetMaterial(0);
            skyBoxMaterial.AssignCubemapPaths("cloudtop_ft.png", "cloudtop_bk.png", "cloudtop_lf.png", "cloudtop_rt.png", "cloudtop_up.png", "cloudtop_dn.png");

            _skyBoxRuntimeState = new ModelRuntimeDescriptor<VertexPosition>(skyBox, "Skybox", "Skybox", VertexRuntimeTypes.VertexPosition, PrimitiveTopology.TriangleList, new RenderDescription(RenderFlags.NORMAL), new InstancingRenderDescription(RenderFlags.NONE, InstancingDataFlags.EMPTY));
            _skyBoxRuntimeState.CallVertexLayoutGeneration += ResourceGenerator.GenerateVertexLayoutForP;
            _skyBoxRuntimeState.CallSamplerGeneration += ResourceGenerator.GenerateBiLinearSampler;
            _skyBoxRuntimeState.CallTextureResourceLayoutGeneration += ResourceGenerator.GenerateTextureResourceLayoutForCubeMapping;
            _skyBoxRuntimeState.CallTextureResourceSetGeneration += ResourceGenerator.GenerateTextureResourceSetForCubeMapping;

            _sun = new Model<VertexPositionNormal, RealtimeMaterial>(string.Empty, GeometryFactory.GenerateSphereNormal(100,100,0.5f), new RealtimeMaterial());
            _sun.GetMaterial(0).ambient = lightColor.ToVector4();
            Vector3 newTranslation = new Vector3(lightPos.X,lightPos.Y,lightPos.Z);
            _sun.SetNewWorldTranslation(ref newTranslation, true);

            var sunRuntimeState = new ModelRuntimeDescriptor<VertexPositionNormal>(_sun, "Phong", "PhongNoShadow", VertexRuntimeTypes.VertexPositionNormal, PrimitiveTopology.TriangleList, new RenderDescription(RenderFlags.NORMAL), new InstancingRenderDescription(RenderFlags.NONE, InstancingDataFlags.EMPTY));
            sunRuntimeState.CallVertexLayoutGeneration+=ResourceGenerator.GenerateVertexLayoutForPN;

            var _sun2 = new Model<VertexPositionNormal, RealtimeMaterial>(string.Empty, GeometryFactory.GenerateSphereNormal(100, 100, 1), new RealtimeMaterial());
            _sun2.GetMaterial(0).ambient = lightColor.ToVector4();
            Vector3 newTranslation2 = new Vector3(0, 10, 0);
            _sun2.SetNewWorldTranslation(ref newTranslation2, true);

            var sunRuntimeState2 = new ModelRuntimeDescriptor<VertexPositionNormal>(_sun2, "PhongOmni", "PhongOmni", VertexRuntimeTypes.VertexPositionNormal, PrimitiveTopology.TriangleList, new RenderDescription(RenderFlags.NORMAL | RenderFlags.OMNI_SHADOW_MAPS), new InstancingRenderDescription(RenderFlags.NONE, InstancingDataFlags.EMPTY));
            sunRuntimeState2.CallVertexLayoutGeneration += ResourceGenerator.GenerateVertexLayoutForPN;

            var _sun3 = new Model<VertexPositionNormal, RealtimeMaterial>(string.Empty, GeometryFactory.GenerateSphereNormal(100, 100, 1), new RealtimeMaterial());
            _sun3.GetMaterial(0).ambient = lightColor.ToVector4();
            Vector3 newTranslation3 = new Vector3(0, 30, -24);
            _sun3.SetNewWorldTranslation(ref newTranslation3, true);

            var sunRuntimeState3 = new ModelRuntimeDescriptor<VertexPositionNormal>(_sun3, "PhongOmni", "PhongOmni", VertexRuntimeTypes.VertexPositionNormal, PrimitiveTopology.TriangleList, new RenderDescription(RenderFlags.NORMAL | RenderFlags.OMNI_SHADOW_MAPS), new InstancingRenderDescription(RenderFlags.NONE, InstancingDataFlags.EMPTY));
            sunRuntimeState3.CallVertexLayoutGeneration += ResourceGenerator.GenerateVertexLayoutForPN;



            //TODO: Automate this
            //_modelPNTTBDescriptorList.Add(sponzaRuntimeState);
            _modelPNTTBDescriptorList.Add(floorRuntimeState);
            _modelPNTTBDescriptorList.Add(floorRuntimeState2);
            //_modelPCDescriptorList.Add(sponzaRuntimeStateColorOnly);
            _modelPDescriptorList.Add(_skyBoxRuntimeState);

            _modelPNDescriptorList.Add(sunRuntimeState);

            _modelPNDescriptorList.Add(sunRuntimeState2);
            _modelPNDescriptorList.Add(sunRuntimeState3);
            //_modelPNDescriptorList.Add(modelRuntimeState);


            InstanceData[] instancingData = {InstanceData.NO_DATA};

            //TODO: Abstrct this
            foreach(var modelDescriptor in _modelPNTTBDescriptorList){
                FillRuntimeDescriptor(modelDescriptor,_sceneRuntimeState,instancingData); 
                PNTTBRuntimeGeometry.AddModel(modelDescriptor);
            }

            foreach(var modelDescriptor in _modelPNDescriptorList){
                FillRuntimeDescriptor(modelDescriptor,_sceneRuntimeState,instancingData); 
                PNRuntimeGeometry.AddModel(modelDescriptor);
            }

            foreach(var modelDescriptor in _modelPTDescriptorList){
                FillRuntimeDescriptor(modelDescriptor,_sceneRuntimeState,instancingData); 
                PTRuntimeGeometry.AddModel(modelDescriptor);
            }

            foreach(var modelDescriptor in _modelPCDescriptorList){
                FillRuntimeDescriptor(modelDescriptor,_sceneRuntimeState,instancingData); 
                PCRuntimeGeometry.AddModel(modelDescriptor);
            }

            foreach(var modelDescriptor in _modelPDescriptorList){
                FillRuntimeDescriptor(modelDescriptor,_sceneRuntimeState,instancingData); 
                PRuntimeGeometry.AddModel(modelDescriptor);
            }



        }

        override protected void BuildCommandList() {
            _commandList.Begin();
            _commandList.SetFramebuffer(GraphicsDevice.SwapchainFramebuffer);
            _commandList.SetFullViewports();

            _commandList.ClearColorTarget(0, RgbaFloat.White);
            _commandList.ClearDepthStencil(1f);

            RenderCommandGenerator.GenerateCommandsForScene_Inline(
                _commandList,
                _sceneRuntimeState.CameraProjViewBuffer,
                _sceneRuntimeState.LightBuffer,
                _sceneRuntimeState.SpotLightBuffer,
                _sceneRuntimeState.LightProjViewBuffer,
                _sceneRuntimeState.Camera,
                _sceneRuntimeState.OmniLights[0],
                _sceneRuntimeState.SpotLight);


            RenderCommandGenerator.GenerateRenderCommandsForModelDescriptor<VertexPositionNormalTextureTangentBitangent>(_commandList,_modelPNTTBDescriptorArray,_sceneRuntimeState, new RenderDescription(RenderFlags.NORMAL),VertexRuntimeTypes.VertexPositionNormalTextureTangentBitangent);
            RenderCommandGenerator.GenerateRenderCommandsForModelDescriptor<VertexPositionNormal>(_commandList,_modelPNDescriptorArray,_sceneRuntimeState, new RenderDescription(RenderFlags.NORMAL), VertexRuntimeTypes.VertexPositionNormal);
            RenderCommandGenerator.GenerateRenderCommandsForModelDescriptor<VertexPositionTexture>(_commandList,_modelPTDescriptorArray,_sceneRuntimeState, new RenderDescription(RenderFlags.NORMAL), VertexRuntimeTypes.VertexPositionTexture);
            RenderCommandGenerator.GenerateRenderCommandsForModelDescriptor<VertexPositionColor>(_commandList,_modelPCDescriptorArray,_sceneRuntimeState, new RenderDescription(RenderFlags.NORMAL), VertexRuntimeTypes.VertexPositionColor);
            RenderCommandGenerator.GenerateRenderCommandsForCubeMapModelDescriptor(_commandList,_skyBoxRuntimeState,_sceneRuntimeState);
            
            _commandList.End();
        }
        
    }
    
}