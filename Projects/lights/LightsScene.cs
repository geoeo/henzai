using System;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;
using Henzai.Core;
using Henzai.Core.VertexGeometry;
using Henzai.Core.Extensions;
using Henzai.Core.Materials;
using Henzai.Geometry;
using Henzai.Runtime;
using Henzai.Cameras;

namespace Henzai.Examples
{
    sealed class LightsScene : Renderable
    {

        Model<VertexPositionNormal, RealtimeMaterial> _sun;
        // Model<VertexPositionColor> _floor;

        public LightsScene(string title,Resolution windowSize, GraphicsDeviceOptions graphicsDeviceOptions, RenderOptions renderOptions)
            : base(title,windowSize,graphicsDeviceOptions,renderOptions){
        }

        //TODO: Abstract this
        public LightsScene(string title, Sdl2Window contextWindow, GraphicsDeviceOptions graphicsDeviceOptions, RenderOptions renderOptions)
            : base(title,contextWindow,graphicsDeviceOptions,renderOptions){
        }

        override protected void CreateResources(){

            // RgbaFloat lightColor = RgbaFloat.Orange;
            // RgbaFloat lightColor = RgbaFloat.Blue;
            RgbaFloat lightColor = RgbaFloat.LightGrey;
            var spotLightPos = new Vector4(0.0f,5.0f,7.0f,1.0f);
            var lightLookAt = spotLightPos-Light.DEFAULT_POSITION;
            var lightCam = new OrthographicCamera(35, 35, Light.DEFAULT_POSITION, lightLookAt);
            var spotLightCam = new OrthographicCamera(RenderResoultion.Horizontal,RenderResoultion.Vertical,spotLightPos , Light.DEFAULT_LOOKAT);
            // RgbaFloat lightColor = new RgbaFloat(1.0f,0.36f,0.0f,0.2f);
            _sceneRuntimeState.Light = new Light(lightCam,lightColor, 0.1f);
            _sceneRuntimeState.Camera = Camera;
            _sceneRuntimeState.SpotLight 
                = new Light(
                    spotLightCam,
                    RgbaFloat.DarkRed,
                    1.0f,
                    0.02f,
                    new Vector4(Math.Cos(17.5f.ToRadians()).ToFloat(),Math.Cos(12.5f.ToRadians()).ToFloat(),0.0f,1.0f));

            // Sun //TODO: Make this VertexPositionNormalColor
            _sun = new Model<VertexPositionNormal,RealtimeMaterial>(String.Empty, GeometryFactory.GenerateSphereNormal(100,100,1), new RealtimeMaterial());
            var sunMeshZero = _sun.GetMesh(0);
            var sunMaterialZero = _sun.GetMaterial(0);
            // _sun.meshes[0].TryGetMaterial().ambient = new Vector4(0.0f,0.0f,0.0f,0.0f);
            sunMaterialZero.ambient = lightColor.ToVector4();
            Vector4 lightPos = _sceneRuntimeState.Light.LightPos;
            Vector3 newTranslation = new Vector3(lightPos.X,lightPos.Y,lightPos.Z);
            _sun.SetNewWorldTranslation(ref newTranslation, true);

            //TODO: This geometry will cause problems for shadow mapping. Has to be ignored in the shadowmap generation process.
            var sunRuntimeState = new ModelRuntimeDescriptor<VertexPositionNormal>(_sun,"Phong","PhongNoShadow", VertexRuntimeTypes.VertexPositionNormal,PrimitiveTopology.TriangleList, RenderFlags.NORMAL);
            sunRuntimeState.CallVertexLayoutGeneration+=ResourceGenerator.GenerateVertexLayoutForPN;
            _modelPNDescriptorList.Add(sunRuntimeState);

            var spotlight = new Model<VertexPositionNormal, RealtimeMaterial>(String.Empty,GeometryFactory.GenerateSphereNormal(100,100,1), new RealtimeMaterial());
            var spotlightMeshZero = spotlight.GetMesh(0);
            var spotlightMaterialZero = spotlight.GetMaterial(0);
            spotlightMaterialZero.ambient = _sceneRuntimeState.SpotLight.Color_DontMutate;
            // spotlight.meshes[0].TryGetMaterial().ambient = new Vector4(1.0f,1.0f,1.0f,1.0f);
            // _sun.meshes[0].TryGetMaterial().ambient = lightColor.ToVector4();
            Vector4 lightPosSpot = _sceneRuntimeState.SpotLight.LightPos;
            Vector3 newTranslationSpot = new Vector3(lightPosSpot.X,lightPosSpot.Y,lightPosSpot.Z);
            spotlight.SetNewWorldTranslation(ref newTranslationSpot, true);

            var spotLightRuntimeState = new ModelRuntimeDescriptor<VertexPositionNormal>(spotlight,"Phong","PhongNoShadow", VertexRuntimeTypes.VertexPositionNormal,PrimitiveTopology.TriangleList, RenderFlags.NORMAL | RenderFlags.SHADOW_MAP);
            spotLightRuntimeState.CallVertexLayoutGeneration+=ResourceGenerator.GenerateVertexLayoutForPN;
            _modelPNDescriptorList.Add(spotLightRuntimeState);

            // Colored Quad
            // var offsets = new Vector3[] {new Vector3(-1.0f,0.0f,0f),new Vector3(1.0f,0.0f,0.0f)};
            // // var offsets = new Vector3[] {new Vector3(0.0f,0.0f,0.0f)};
            // var instancingData = new InstancingData {Positions = offsets};
            // var floor = new Model<VertexPositionColor>(String.Empty,GeometryFactory.GenerateColorQuad(RgbaFloat.Red,RgbaFloat.Yellow,RgbaFloat.Green,RgbaFloat.LightGrey));
            // var floorRuntimeState = new ModelRuntimeDescriptor<VertexPositionColor>(floor,"OffsetColor","Color",VertexTypes.VertexPositionColor,PrimitiveTopology.TriangleStrip);
            // floorRuntimeState.TotalInstanceCount = offsets.Length.ToUnsigned();
            // floorRuntimeState.CallVertexLayoutGeneration+=ResourceGenerator.GenerateVertexLayoutForPC;
            // floorRuntimeState.CallVertexInstanceLayoutGeneration+=ResourceGenerator.GenerateVertexInstanceLayoutForPC;
            // _modelPCDescriptorList.Add(floorRuntimeState);

            //Quad Textured
            // var offsets = new Vector3[] {new Vector3(-1.0f,0.0f,0f),new Vector3(1.0f,0.0f,0.0f)};
            // // var offsets = new Vector3[] {new Vector3(0.0f,0.0f,0.0f)};
            // var instancingData = new InstancingData {Positions = offsets};
            // var floor = new Model<VertexPositionTexture>("paving",GeometryFactory.GenerateTexturedQuad());
            // floor.meshes[0].TryGetMaterial().textureDiffuse="pavingColor.jpg";
            // var floorRuntimeState = new ModelRuntimeDescriptor<VertexPositionTexture>(floor,"PositionOffsetTexture","Texture",VertexTypes.VertexPositionTexture,PrimitiveTopology.TriangleStrip);
            // floorRuntimeState.TotalInstanceCount = offsets.Length.ToUnsigned();

            // floorRuntimeState.CallVertexLayoutGeneration+=ResourceGenerator.GenerateVertexLayoutForPT;
            // floorRuntimeState.CallVertexInstanceLayoutGeneration+=ResourceGenerator.GenerateVertexInstanceLayoutForPositionOffset;
            // floorRuntimeState.CallTextureResourceLayoutGeneration+=ResourceGenerator.GenerateTextureResourceLayoutForDiffuseMapping;
            // floorRuntimeState.CallTextureResourceSetGeneration+=ResourceGenerator.GenerateTextureResourceSetForDiffuseMapping;
            // floorRuntimeState.CallSamplerGeneration+=ResourceGenerator.GenerateLinearSampler;
            // _modelPTDescriptorList.Add(floorRuntimeState);

            // floor
            // var offsets = new Vector3[] {new Vector3(-1.0f,0.0f,0f),new Vector3(1.0f,0.0f,0.0f)};
            // var offsets = new Vector3[] {new Vector3(0.0f,0.0f,0.0f)};
            var offsets = GeometryUtils.CreateTilingList_XZ(-20,20,-10,10,0,GeometryFactory.QUAD_WIDTH,GeometryFactory.QUAD_HEIGHT);
            // var offsets = GeometryUtils.CreateTilingList_XZ(-1,1,0,0,0,GeometryFactory.QUAD_WIDTH,GeometryFactory.QUAD_HEIGHT);
            var instancingData = new InstancingData {Types = InstancingTypes.Positions, Positions = offsets};

            var floor = new Model<VertexPositionNormalTextureTangentBitangent, RealtimeMaterial>("paving/",GeometryFactory.GenerateQuadPNTTB_XZ(), new RealtimeMaterial());
            var floorMeshZero = floor.GetMesh(0);
            var flootMaterialZero = floor.GetMaterial(0);
            flootMaterialZero.textureDiffuse="pavingColor.jpg";
            flootMaterialZero.textureNormal="pavingNorm.jpg";
            flootMaterialZero.ambient=new Vector4(0.3f,0.3f,0.3f,1.0f);
            var floorTranslation = new Vector3(0.0f,-2.0f,0.0f);
            floor.SetNewWorldTranslation(ref floorTranslation,true);
            var floorRuntimeState = new ModelRuntimeDescriptor<VertexPositionNormalTextureTangentBitangent>(floor, "PositionOffsetPhongBitangentTexture", "PhongBitangentTexture", VertexRuntimeTypes.VertexPositionNormalTextureTangentBitangent, PrimitiveTopology.TriangleStrip, RenderFlags.NORMAL | RenderFlags.SHADOW_MAP);
            floorRuntimeState.TotalInstanceCount = offsets.Length.ToUnsigned();

            //TODO: Test ViewMatrix Instancing
            // Matrix4x4 test = Camera.ViewMatrix;
            // Matrix4x4 test2 = Camera.ViewMatrix;
            // test.Translation = new Vector3(0,1,-10);
            // test2.Translation = new Vector3(0,0,-10);
            // var viewMatrices = new Matrix4x4[] {test2, test};


            // var viewInstancingData = new InstancingData{
            //     Types = InstancingTypes.ViewMatricies, 
            //     ViewMatrices = viewMatrices
            //     };
            // var floorRuntimeState = new ModelRuntimeDescriptor<VertexPositionNormalTextureTangentBitangent>(floor, "ViewMatInstancePhongBitangentTexture", "PhongBitangentTexture", VertexRuntimeTypes.VertexPositionNormalTextureTangentBitangent, PrimitiveTopology.TriangleStrip, RenderFlags.NORMAL | RenderFlags.SHADOW_MAP);
            //floorRuntimeState.TotalInstanceCount = viewMatrices.Length.ToUnsigned();

            floorRuntimeState.CallVertexLayoutGeneration+=ResourceGenerator.GenerateVertexLayoutForPNTTB;
            floorRuntimeState.CallVertexShadowMapInstanceLayoutGeneration+=ResourceGenerator.GenerateVertexLayoutForPNTTB;
            floorRuntimeState.CallVertexInstanceLayoutGeneration+=ResourceGenerator.GenerateVertexInstanceLayoutForPositionOffset;
            //floorRuntimeState.CallVertexInstanceLayoutGeneration+=ResourceGenerator.GenerateVertexInstanceLayoutForViewMatrixOffset; //viewMatInstance
            floorRuntimeState.CallTextureResourceLayoutGeneration+=ResourceGenerator.GenerateTextureResourceLayoutForNormalMapping;
            floorRuntimeState.CallTextureResourceSetGeneration+=ResourceGenerator.GenerateTextureResourceSetForNormalMapping;
            floorRuntimeState.CallSamplerGeneration+=ResourceGenerator.GenerateTriLinearSampler;
            _modelPNTTBDescriptorList.Add(floorRuntimeState);

            foreach(var modelDescriptor in _modelPNDescriptorList){
                FillRuntimeDescriptor(modelDescriptor,_sceneRuntimeState,InstancingData.NO_DATA); 
                PNRuntimeGeometry.AddModel(modelDescriptor);
            }

            // foreach(var modelDescriptor in _modelPTDescriptorList){
            //     FillRuntimeDescriptor(modelDescriptor,_sceneRuntimeState,InstancingData.NO_DATA); 
            // }

            foreach(var modelDescriptor in _modelPNTTBDescriptorList){
                FillRuntimeDescriptor(modelDescriptor,_sceneRuntimeState,instancingData); 
                PNTTBRuntimeGeometry.AddModel(modelDescriptor);
            }

        }

        override protected void BuildCommandList(){
            _commandList.Begin();
            _commandList.SetFramebuffer(GraphicsDevice.SwapchainFramebuffer);
            _commandList.SetFullViewports();

            _commandList.ClearColorTarget(0,RgbaFloat.White);
            _commandList.ClearDepthStencil(1f);

            // RenderCommandGenerator.GenerateRenderCommandsForModelDescriptor(_commandList,_modelPCDescriptorArray,_sceneRuntimeState);
            // RenderCommandGenerator.GenerateRenderCommandsForModelDescriptor_Instancing(_commandList,_modelPCDescriptorArray,_sceneRuntimeState);
            // RenderCommandGenerator.GenerateRenderCommandsForModelDescriptor_Instancing(_commandList,_modelPTDescriptorArray,_sceneRuntimeState);

            RenderCommandGenerator.GenerateCommandsForScene_Inline(
                _commandList,
                _sceneRuntimeState.CameraProjViewBuffer,
                _sceneRuntimeState.LightBuffer,
                _sceneRuntimeState.SpotLightBuffer,
                _sceneRuntimeState.LightProjViewBuffer,
                _sceneRuntimeState.Camera,
                _sceneRuntimeState.Light,
                _sceneRuntimeState.SpotLight);

            RenderCommandGenerator.GenerateRenderCommandsForModelDescriptor<VertexPositionNormalTextureTangentBitangent>(_commandList,_modelPNTTBDescriptorArray,_sceneRuntimeState, PipelineTypes.Normal, VertexRuntimeTypes.VertexPositionNormalTextureTangentBitangent);
            RenderCommandGenerator.GenerateRenderCommandsForModelDescriptor<VertexPositionNormal>(_commandList,_modelPNDescriptorArray,_sceneRuntimeState, PipelineTypes.Normal, VertexRuntimeTypes.VertexPositionNormal);
            
            _commandList.End();
        }
    }
    
}