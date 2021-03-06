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
    sealed class SponzaScene : Renderable
    {

        private Model<VertexPositionNormal, RealtimeMaterial> _sun;
        private ModelRuntimeDescriptor<VertexPosition> _skyBoxRuntimeState;

        public SponzaScene(string title,Resolution windowSize, GraphicsDeviceOptions graphicsDeviceOptions, RenderOptions renderOptions)
            : base(title,windowSize,graphicsDeviceOptions,renderOptions){
        }

        public SponzaScene(string title, Sdl2Window contextWindow, GraphicsDeviceOptions graphicsDeviceOptions, RenderOptions renderOptions)
            : base(title,contextWindow,graphicsDeviceOptions,renderOptions){
        }

        override protected void CreateResources(){

            RgbaFloat lightColor = RgbaFloat.White;
            // RgbaFloat lightColor = RgbaFloat.LightGrey;
            var lightPos = new Vector4(0,100,0,1);
            var lookAt = new Vector4(0,0,0,1)- new Vector4(0,50,0,1);
            var lightCam = new OrthographicCamera(50.0f, 50.0f, lightPos, lookAt);
            _sceneRuntimeState.Light = new Light(lightCam,lightColor,0.1f);
            _sceneRuntimeState.Camera = Camera;
            _sceneRuntimeState.SpotLight = Light.NO_POINTLIGHT;

            // string filePath = Path.Combine(AppContext.BaseDirectory, "armor/armor.dae"); 
            // string filePath = Path.Combine(AppContext.BaseDirectory, "nanosuit/nanosuit.obj"); 

            var scale = Matrix4x4.CreateScale(0.05f,0.05f,0.05f);
            //var scale = Matrix4x4.CreateScale(1.00f,1.00f,1.00f);


            string filePath = "models/chinesedragon.dae";
            // string filePath = "Models/Box.dae";
            var model = AssimpLoader.LoadFromFileWithRealtimeMaterial<VertexPositionNormal>(AppContext.BaseDirectory, filePath, VertexPositionNormal.HenzaiType);
            var newModelTranslation = Matrix4x4.CreateTranslation(new Vector3(0,20,0));
            var modelRuntimeState 
                = new ModelRuntimeDescriptor<VertexPositionNormal>(model,"Phong","Phong", VertexRuntimeTypes.VertexPositionNormal,PrimitiveTopology.TriangleList, new RenderDescription(RenderFlags.NORMAL | RenderFlags.SHADOW_MAP), new InstancingRenderDescription(RenderFlags.NONE, InstancingDataFlags.EMPTY));
            modelRuntimeState.CallVertexLayoutGeneration+=ResourceGenerator.GenerateVertexLayoutForPN;
            model.SetNewWorldTransformation(ref newModelTranslation, true);
            
            var plane = new Model<VertexPositionNormal, RealtimeMaterial>(String.Empty,GeometryFactory.GenerateQuadPN_XZ(),new RealtimeMaterial());
            var newPlaneTranslation = Matrix4x4.CreateTranslation(new Vector3(0,10,0));
            var newPlaneScale = Matrix4x4.CreateScale(new Vector3(30,1,30));
            var trafo = newPlaneScale*newPlaneTranslation;
            plane.SetNewWorldTransformation(ref trafo, true);
            var planeRuntimeState = new ModelRuntimeDescriptor<VertexPositionNormal>(plane,"Phong","Phong", VertexRuntimeTypes.VertexPositionNormal,PrimitiveTopology.TriangleStrip, new RenderDescription(RenderFlags.NORMAL | RenderFlags.SHADOW_MAP), new InstancingRenderDescription(RenderFlags.NONE, InstancingDataFlags.EMPTY));
            planeRuntimeState.CallVertexLayoutGeneration+=ResourceGenerator.GenerateVertexLayoutForPN;
            
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



            var sponzaRuntimeState 
                = new ModelRuntimeDescriptor<VertexPositionNormalTextureTangentBitangent>(sponzaPNTTB,"PhongBitangentTexture","PhongBitangentTexture", VertexRuntimeTypes.VertexPositionNormalTextureTangentBitangent,PrimitiveTopology.TriangleList, new RenderDescription(RenderFlags.NORMAL | RenderFlags.SHADOW_MAP), new InstancingRenderDescription(RenderFlags.NONE, InstancingDataFlags.EMPTY));
            sponzaRuntimeState.CallVertexLayoutGeneration+=ResourceGenerator.GenerateVertexLayoutForPNTTB;
            sponzaRuntimeState.CallSamplerGeneration+=ResourceGenerator.GenerateTriLinearSampler;
            sponzaRuntimeState.CallTextureResourceLayoutGeneration+=ResourceGenerator.GenerateTextureResourceLayoutForNormalMapping;
            sponzaRuntimeState.CallTextureResourceSetGeneration+=ResourceGenerator.GenerateTextureResourceSetForNormalMapping;

            // var sponzaRuntimeStateTexOnly = new ModelRuntimeDescriptor<VertexPositionTexture>(sponzaPT,"Texture","Texture", VertexTypes.VertexPositionTexture,PrimitiveTopology.TriangleList);
            // sponzaRuntimeStateTexOnly.CallVertexLayoutGeneration+=ResourceGenerator.GenerateVertexLayoutForPT;
            // sponzaRuntimeStateTexOnly.CallSamplerGeneration+=ResourceGenerator.GenerateTriLinearSampler;
            // sponzaRuntimeStateTexOnly.CallTextureResourceLayoutGeneration+=ResourceGenerator.GenerateTextureResourceLayoutForDiffuseMapping;
            // sponzaRuntimeStateTexOnly.CallTextureResourceSetGeneration+=ResourceGenerator.GenerateTextureResourceSetForDiffuseMapping;

            var sponzaRuntimeStateColorOnly = new ModelRuntimeDescriptor<VertexPositionColor>(sponzaPC,"Color","Color", VertexRuntimeTypes.VertexPositionColor,PrimitiveTopology.TriangleList, new RenderDescription(RenderFlags.NORMAL | RenderFlags.SHADOW_MAP), new InstancingRenderDescription(RenderFlags.NONE, InstancingDataFlags.EMPTY));
            sponzaRuntimeStateColorOnly.CallVertexLayoutGeneration+= ResourceGenerator.GenerateVertexLayoutForPC;

            // var sunRuntimeState = new ModelRuntimeState<VertexPositionNormalTextureTangentBitangent>(sun,"PhongBitangentTexture","PhongBitangentTexture");
            // sunRuntimeState.CallVertexLayoutGeneration+=ResourceGenerator.GenerateVertexLayoutForPNTTB;
            // sunRuntimeState.CallSamplerGeneration+=ResourceGenerator.GenerateLinearSampler;
            // sunRuntimeState.CallTextureResourceLayoutGeneration+=ResourceGenerator.GenerateTextureResourceLayoutForNormalMapping;
            // sunRuntimeState.CallTextureResourceSetGeneration+=ResourceGenerator.GenerateTextureResourceSetForNormalMapping;

            var skyBox = new Model<VertexPosition, RealtimeMaterial>("cloudtop", GeometryFactory.GenerateCube(true), new RealtimeMaterial());
            var skyBoxMaterial = skyBox.GetMaterial(0);
            skyBoxMaterial.AssignCubemapPaths("cloudtop_ft.png", "cloudtop_bk.png", "cloudtop_lf.png", "cloudtop_rt.png", "cloudtop_up.png", "cloudtop_dn.png");

            _skyBoxRuntimeState = new ModelRuntimeDescriptor<VertexPosition>(skyBox, "Skybox", "Skybox", VertexRuntimeTypes.VertexPosition, PrimitiveTopology.TriangleList, new RenderDescription(RenderFlags.NORMAL), new InstancingRenderDescription(RenderFlags.NONE, InstancingDataFlags.EMPTY));
            _skyBoxRuntimeState.CallVertexLayoutGeneration += ResourceGenerator.GenerateVertexLayoutForP;
            _skyBoxRuntimeState.CallSamplerGeneration += ResourceGenerator.GenerateBiLinearSampler;
            _skyBoxRuntimeState.CallTextureResourceLayoutGeneration += ResourceGenerator.GenerateTextureResourceLayoutForCubeMapping;
            _skyBoxRuntimeState.CallTextureResourceSetGeneration += ResourceGenerator.GenerateTextureResourceSetForCubeMapping;

            _sun = new Model<VertexPositionNormal, RealtimeMaterial>(string.Empty, GeometryFactory.GenerateSphereNormal(100,100,1), new RealtimeMaterial());
            _sun.GetMaterial(0).ambient = lightColor.ToVector4();
            // _sun.meshes[0].TryGetMaterial().ambient = lightColor.ToVector4();
            Vector3 newTranslation = new Vector3(lightPos.X,lightPos.Y,lightPos.Z);
            _sun.SetNewWorldTranslation(ref newTranslation, true);

            var sunRuntimeState = new ModelRuntimeDescriptor<VertexPositionNormal>(_sun, "Phong", "PhongNoShadow", VertexRuntimeTypes.VertexPositionNormal, PrimitiveTopology.TriangleList, new RenderDescription(RenderFlags.NORMAL), new InstancingRenderDescription(RenderFlags.NONE, InstancingDataFlags.EMPTY));
            sunRuntimeState.CallVertexLayoutGeneration+=ResourceGenerator.GenerateVertexLayoutForPN;

            //TODO: Automate this
            _modelPNTTBDescriptorList.Add(sponzaRuntimeState);
            _modelPCDescriptorList.Add(sponzaRuntimeStateColorOnly);
            _modelPDescriptorList.Add(_skyBoxRuntimeState);
            // _modelPTDescriptorList.Add(sponzaRuntimeStateTexOnly);
            // _modelStatesList.Add(sunRuntimeState);
            _modelPNDescriptorList.Add(sunRuntimeState);
            _modelPNDescriptorList.Add(modelRuntimeState);
            //_modelPNDescriptorList.Add(planeRuntimeState);

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
                _sceneRuntimeState.LightProjViewBuffer,
                _sceneRuntimeState.Camera,
                _sceneRuntimeState.Light,
                _sceneRuntimeState.SpotLight);

            //RenderCommandGenerator.GenerateRenderCommandsForModelDescriptor(_commandList,_modelPNTTBDescriptorArray,_sceneRuntimeState, PipelineTypes.Normal);
            RenderCommandGenerator.GenerateRenderCommandsForModelDescriptor(_commandList,_modelPNTTBDescriptorArray,_sceneRuntimeState, PNTTBRuntimeGeometry.MeshBVHArray, new RenderDescription(RenderFlags.NORMAL));
            RenderCommandGenerator.GenerateRenderCommandsForModelDescriptor<VertexPositionNormal>(_commandList,_modelPNDescriptorArray,_sceneRuntimeState, new RenderDescription(RenderFlags.NORMAL), VertexRuntimeTypes.VertexPositionNormal);
            RenderCommandGenerator.GenerateRenderCommandsForModelDescriptor<VertexPositionTexture>(_commandList,_modelPTDescriptorArray,_sceneRuntimeState, new RenderDescription(RenderFlags.NORMAL), VertexRuntimeTypes.VertexPositionTexture);
            RenderCommandGenerator.GenerateRenderCommandsForModelDescriptor<VertexPositionColor>(_commandList,_modelPCDescriptorArray,_sceneRuntimeState, new RenderDescription(RenderFlags.NORMAL), VertexRuntimeTypes.VertexPositionColor);
            RenderCommandGenerator.GenerateRenderCommandsForCubeMapModelDescriptor(_commandList,_skyBoxRuntimeState,_sceneRuntimeState);
            
            _commandList.End();
        }
        
    }
    
}