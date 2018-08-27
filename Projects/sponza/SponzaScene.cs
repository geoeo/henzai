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
    sealed class SponzaScene : Renderable
    {

        private Model<VertexPositionNormal> _sun;
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
            _sceneRuntimeState.Light = new Light(new Vector4(0,50,0,1),lightColor,0.1f);
            _sceneRuntimeState.Camera = Camera;
            _sceneRuntimeState.SpotLight = Light.NO_POINTLIGHT;

            // string filePath = Path.Combine(AppContext.BaseDirectory, "armor/armor.dae"); 
            // string filePath = Path.Combine(AppContext.BaseDirectory, "nanosuit/nanosuit.obj"); 
            
            //TODO: Write method to remove ambient terms
            var sponzaModels = AssimpLoader.LoadModelsFromFile(AppContext.BaseDirectory,"sponza/sponza.obj");
            var sponzaPNTTB = sponzaModels.modelPNTTB;
            var sponzaPC = sponzaModels.modelPC;

            sponzaPNTTB.SetAmbientForAllMeshes(Vector4.Zero);

            var sponzaRuntimeState = new ModelRuntimeDescriptor<VertexPositionNormalTextureTangentBitangent>(sponzaPNTTB,"PhongBitangentTexture","PhongBitangentTexture", VertexTypes.VertexPositionNormalTextureTangentBitangent,PrimitiveTopology.TriangleList);
            sponzaRuntimeState.CallVertexLayoutGeneration+=ResourceGenerator.GenerateVertexLayoutForPNTTB;
            sponzaRuntimeState.CallSamplerGeneration+=ResourceGenerator.GenerateTriLinearSampler;
            sponzaRuntimeState.CallTextureResourceLayoutGeneration+=ResourceGenerator.GenerateTextureResourceLayoutForNormalMapping;
            sponzaRuntimeState.CallTextureResourceSetGeneration+=ResourceGenerator.GenerateTextureResourceSetForNormalMapping;

            // var sponzaRuntimeStateTexOnly = new ModelRuntimeDescriptor<VertexPositionTexture>(sponzaPT,"Texture","Texture", VertexTypes.VertexPositionTexture,PrimitiveTopology.TriangleList);
            // sponzaRuntimeStateTexOnly.CallVertexLayoutGeneration+=ResourceGenerator.GenerateVertexLayoutForPT;
            // sponzaRuntimeStateTexOnly.CallSamplerGeneration+=ResourceGenerator.GenerateTriLinearSampler;
            // sponzaRuntimeStateTexOnly.CallTextureResourceLayoutGeneration+=ResourceGenerator.GenerateTextureResourceLayoutForDiffuseMapping;
            // sponzaRuntimeStateTexOnly.CallTextureResourceSetGeneration+=ResourceGenerator.GenerateTextureResourceSetForDiffuseMapping;

            var sponzaRuntimeStateColorOnly = new ModelRuntimeDescriptor<VertexPositionColor>(sponzaPC,"Color","Color", VertexTypes.VertexPositionColor,PrimitiveTopology.TriangleList);
            sponzaRuntimeStateColorOnly.CallVertexLayoutGeneration+= ResourceGenerator.GenerateVertexLayoutForPC;

            // var sunRuntimeState = new ModelRuntimeState<VertexPositionNormalTextureTangentBitangent>(sun,"PhongBitangentTexture","PhongBitangentTexture");
            // sunRuntimeState.CallVertexLayoutGeneration+=ResourceGenerator.GenerateVertexLayoutForPNTTB;
            // sunRuntimeState.CallSamplerGeneration+=ResourceGenerator.GenerateLinearSampler;
            // sunRuntimeState.CallTextureResourceLayoutGeneration+=ResourceGenerator.GenerateTextureResourceLayoutForNormalMapping;
            // sunRuntimeState.CallTextureResourceSetGeneration+=ResourceGenerator.GenerateTextureResourceSetForNormalMapping;

            var skyBox = new Model<VertexPosition>("cloudtop", GeometryFactory.GenerateCube(true));
            var skyBoxMaterial = skyBox.meshes[0].TryGetMaterial();
            skyBoxMaterial.cubeMapFront = "cloudtop_ft.png";
            skyBoxMaterial.cubeMapBack = "cloudtop_bk.png";
            skyBoxMaterial.cubeMapLeft = "cloudtop_lf.png";
            skyBoxMaterial.cubeMapRight = "cloudtop_rt.png";
            skyBoxMaterial.cubeMapTop = "cloudtop_up.png";
            skyBoxMaterial.cubeMapBottom = "cloudtop_dn.png";
            
            _skyBoxRuntimeState = new ModelRuntimeDescriptor<VertexPosition>(skyBox,"Skybox","Skybox",VertexTypes.VertexPosition,PrimitiveTopology.TriangleList);
            _skyBoxRuntimeState.CallVertexLayoutGeneration += ResourceGenerator.GenerateVertexLayoutForP;
            _skyBoxRuntimeState.CallSamplerGeneration += ResourceGenerator.GenerateBiLinearSampler;
            _skyBoxRuntimeState.CallTextureResourceLayoutGeneration += ResourceGenerator.GenerateTextureResourceLayoutForCubeMapping;
            _skyBoxRuntimeState.CallTextureResourceSetGeneration += ResourceGenerator.GenerateTextureResourceSetForCubeMapping;

            _sun = new Model<VertexPositionNormal>(String.Empty,GeometryFactory.GenerateSphereNormal(100,100,1));
            _sun.meshes[0].TryGetMaterial().ambient = lightColor.ToVector4();
            // _sun.meshes[0].TryGetMaterial().ambient = lightColor.ToVector4();
            ref Vector4 lightPos = ref _sceneRuntimeState.Light.LightPos_DontMutate;
            Vector3 newTranslation = new Vector3(lightPos.X,lightPos.Y,lightPos.Z);
            _sun.SetNewWorldTranslation(ref newTranslation, true);

            var sunRuntimeState = new ModelRuntimeDescriptor<VertexPositionNormal>(_sun,"Phong","Phong",VertexTypes.VertexPositionNormal,PrimitiveTopology.TriangleList);
            sunRuntimeState.CallVertexLayoutGeneration+=ResourceGenerator.GenerateVertexLayoutForPN;

            //TODO: Automate this
            _modelPNTTBDescriptorList.Add(sponzaRuntimeState);
            _modelPCDescriptorList.Add(sponzaRuntimeStateColorOnly);
            _modelPDescriptorList.Add(_skyBoxRuntimeState);
            // _modelPTDescriptorList.Add(sponzaRuntimeStateTexOnly);
            // _modelStatesList.Add(sunRuntimeState);
            _modelPNDescriptorList.Add(sunRuntimeState);

            //TODO: Abstrct this
            foreach(var modelDescriptor in _modelPNTTBDescriptorList){
                FillRuntimeDescriptor(modelDescriptor,_sceneRuntimeState,InstancingData.NO_DATA); 
            }

            foreach(var modelDescriptor in _modelPNDescriptorList){
                FillRuntimeDescriptor(modelDescriptor,_sceneRuntimeState,InstancingData.NO_DATA); 
            }

            foreach(var modelDescriptor in _modelPTDescriptorList){
                FillRuntimeDescriptor(modelDescriptor,_sceneRuntimeState,InstancingData.NO_DATA); 
            }

            foreach(var modelDescriptor in _modelPCDescriptorList){
                FillRuntimeDescriptor(modelDescriptor,_sceneRuntimeState,InstancingData.NO_DATA); 
            }

            foreach(var modelDescriptor in _modelPDescriptorList){
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
            RenderCommandGenerator.GenerateRenderCommandsForModelDescriptor(_commandList,_modelPTDescriptorArray,_sceneRuntimeState);
            RenderCommandGenerator.GenerateRenderCommandsForModelDescriptor(_commandList,_modelPCDescriptorArray,_sceneRuntimeState);
            RenderCommandGenerator.GenerateRenderCommandsForCubeMapModelDescriptor(_commandList,_skyBoxRuntimeState,_sceneRuntimeState);
            
            _commandList.End();
        }

        override protected void Draw(){
            GraphicsDevice.SubmitCommands(_commandList);
        }


    }
    
}