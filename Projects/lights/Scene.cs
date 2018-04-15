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

        // TODO: Make this part of renderable?
        private List<ModelRuntimeDescriptor<VertexPositionColor>> _modelPCDescriptorList;
        private ModelRuntimeDescriptor<VertexPositionColor> [] _modelPCDescriptorArray;

        private List<ModelRuntimeDescriptor<VertexPositionNormal>> _modelPNDescriptorList;
        private ModelRuntimeDescriptor<VertexPositionNormal> [] _modelPNDescriptorArray;

        private List<ModelRuntimeDescriptor<VertexPositionNormalTextureTangentBitangent>> _modelPNTTBDescriptorList;
        private ModelRuntimeDescriptor<VertexPositionNormalTextureTangentBitangent> [] _modelPNTTBDescriptorArray;
        private List<ModelRuntimeDescriptor<VertexPositionTexture>> _modelPTDescriptorList;
        private ModelRuntimeDescriptor<VertexPositionTexture> [] _modelPTDescriptorArray;

        Model<VertexPositionNormal> _sun;
        // Model<VertexPositionColor> _floor;

        public Scene(string title,Resolution windowSize, GraphicsDeviceOptions graphicsDeviceOptions, RenderOptions renderOptions)
            : base(title,windowSize,graphicsDeviceOptions,renderOptions){
                _sceneRuntimeState = new SceneRuntimeDescriptor();

                _modelPCDescriptorList = new List<ModelRuntimeDescriptor<VertexPositionColor>>();
                _modelPNDescriptorList = new List<ModelRuntimeDescriptor<VertexPositionNormal>>();
                _modelPTDescriptorList = new List<ModelRuntimeDescriptor<VertexPositionTexture>>();
                _modelPNTTBDescriptorList = new List<ModelRuntimeDescriptor<VertexPositionNormalTextureTangentBitangent>>();

                PreRenderLoop+=FormatResourcesForRuntime;
        }



        // TODO: Investigate putting this in renderable
        override protected void FormatResourcesForRuntime(){

            foreach(var modelState in _modelPCDescriptorList)
                modelState.FormatResourcesForRuntime();
            foreach(var modelState in _modelPNDescriptorList)
                modelState.FormatResourcesForRuntime();
            foreach(var modelState in _modelPNTTBDescriptorList)
                modelState.FormatResourcesForRuntime();
            foreach(var modelState in _modelPTDescriptorList)
                modelState.FormatResourcesForRuntime();


            _modelPCDescriptorArray = _modelPCDescriptorList.ToArray();
            _modelPNDescriptorArray = _modelPNDescriptorList.ToArray();
            _modelPTDescriptorArray = _modelPTDescriptorList.ToArray();
            _modelPNTTBDescriptorArray = _modelPNTTBDescriptorList.ToArray();
        }

        override protected void CreateResources(){

            // RgbaFloat lightColor = RgbaFloat.Orange;
            RgbaFloat lightColor = RgbaFloat.LightGrey;
            // RgbaFloat lightColor = new RgbaFloat(1.0f,0.36f,0.0f,0.2f);
            _sceneRuntimeState.Light = new Light(lightColor);
            _sceneRuntimeState.Camera = Camera;
            _sceneRuntimeState.SpotLight 
                = new Light(
                    new Vector4(0.0f,5.0f,0.0f,1.0f),
                    RgbaFloat.DarkRed,
                    1.0f,
                    new Vector4(0,-1,0,0.02f),
                    new Vector4(Math.Cos(17.5f.ToRadians()).ToFloat(),Math.Cos(12.5f.ToRadians()).ToFloat(),0.0f,1.0f));

            // Sun
            _sun = new Model<VertexPositionNormal>(String.Empty,GeometryFactory.GenerateSphereNormal(100,100,1));
            _sun.meshes[0].TryGetMaterial().ambient = new Vector4(1.0f,1.0f,1.0f,1.0f);
            // _sun.meshes[0].TryGetMaterial().ambient = lightColor.ToVector4();
            ref Vector4 lightPos = ref _sceneRuntimeState.Light.LightPos_DontMutate;
            Vector3 newTranslation = new Vector3(lightPos.X,lightPos.Y,lightPos.Z);
            _sun.SetNewWorldTranslation(ref newTranslation, true);

            var sunRuntimeState = new ModelRuntimeDescriptor<VertexPositionNormal>(_sun,"Phong","Phong",VertexTypes.VertexPositionNormal,PrimitiveTopology.TriangleList);
            sunRuntimeState.CallVertexLayoutGeneration+=ResourceGenerator.GenerateVertexLayoutForPN;
            _modelPNDescriptorList.Add(sunRuntimeState);

            var spotlight = new Model<VertexPositionNormal>(String.Empty,GeometryFactory.GenerateSphereNormal(100,100,1));
            spotlight.meshes[0].TryGetMaterial().ambient = _sceneRuntimeState.SpotLight.Color_DontMutate;
            // spotlight.meshes[0].TryGetMaterial().ambient = new Vector4(1.0f,1.0f,1.0f,1.0f);
            // _sun.meshes[0].TryGetMaterial().ambient = lightColor.ToVector4();
            ref Vector4 lightPosSpot = ref _sceneRuntimeState.SpotLight.LightPos_DontMutate;
            Vector3 newTranslationSpot = new Vector3(lightPosSpot.X,lightPosSpot.Y,lightPosSpot.Z);
            spotlight.SetNewWorldTranslation(ref newTranslationSpot, true);

            var spotLightRuntimeState = new ModelRuntimeDescriptor<VertexPositionNormal>(spotlight,"Phong","Phong",VertexTypes.VertexPositionNormal,PrimitiveTopology.TriangleList);
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
            var instancingData = new InstancingData {Positions = offsets};
            var floor = new Model<VertexPositionNormalTextureTangentBitangent>("paving/",GeometryFactory.GenerateQuadPNTTB_XZ());
            floor.meshes[0].TryGetMaterial().textureDiffuse="pavingColor.jpg";
            floor.meshes[0].TryGetMaterial().textureNormal="pavingNorm.jpg";
            floor.meshes[0].TryGetMaterial().ambient=new Vector4(0.3f,0.3f,0.3f,1.0f);
            var floorTranslation = new Vector3(0.0f,-2.0f,0.0f);
            floor.SetNewWorldTranslation(ref floorTranslation,true);
            var floorRuntimeState = new ModelRuntimeDescriptor<VertexPositionNormalTextureTangentBitangent>(floor,"PositionOffsetPhongBitangentTexture","PhongBitangentTexture",VertexTypes.VertexPositionNormalTextureTangentBitangent,PrimitiveTopology.TriangleStrip);
            floorRuntimeState.TotalInstanceCount = offsets.Length.ToUnsigned();

            floorRuntimeState.CallVertexLayoutGeneration+=ResourceGenerator.GenerateVertexLayoutForPNTTB;
            floorRuntimeState.CallVertexInstanceLayoutGeneration+=ResourceGenerator.GenerateVertexInstanceLayoutForPositionOffset;
            floorRuntimeState.CallTextureResourceLayoutGeneration+=ResourceGenerator.GenerateTextureResourceLayoutForNormalMapping;
            floorRuntimeState.CallTextureResourceSetGeneration+=ResourceGenerator.GenerateTextureResourceSetForNormalMapping;
            floorRuntimeState.CallSamplerGeneration+=ResourceGenerator.GenerateTriLinearSampler;
            _modelPNTTBDescriptorList.Add(floorRuntimeState);

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

            // Uniform 4 - Material
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

            // Uniform 2 - Light
            _sceneRuntimeState.LightBuffer = _factory.CreateBuffer(new BufferDescription(Light.SizeInBytes,BufferUsage.UniformBuffer));
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

            // Uniform 3 - PointLight
            _sceneRuntimeState.SpotLightBuffer = _factory.CreateBuffer(new BufferDescription(4*4*4,BufferUsage.UniformBuffer));
            _sceneRuntimeState.SpotLightResourceLayout 
                = ResourceGenerator.GenerateResourceLayout(
                    _factory,
                    "spotlight",
                    ResourceKind.UniformBuffer,
                    ShaderStages.Fragment);
            _sceneRuntimeState.SpotLightResourceSet 
                = ResourceGenerator.GenrateResourceSet(
                    _factory,
                    _sceneRuntimeState.SpotLightResourceLayout,
                    new BindableResource[]{_sceneRuntimeState.SpotLightBuffer});

            // TODO: Make this part of renderable?
            foreach(var modelDescriptor in _modelPCDescriptorList){
                FillRuntimeDescriptor(modelDescriptor,_sceneRuntimeState,instancingData); 
            }

            foreach(var modelDescriptor in _modelPNDescriptorList){
                FillRuntimeDescriptor(modelDescriptor,_sceneRuntimeState,InstancingData.NO_DATA); 
            }

            foreach(var modelDescriptor in _modelPTDescriptorList){
                FillRuntimeDescriptor(modelDescriptor,_sceneRuntimeState,instancingData); 
            }

            foreach(var modelDescriptor in _modelPNTTBDescriptorList){
                FillRuntimeDescriptor(modelDescriptor,_sceneRuntimeState,instancingData); 
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
            RenderCommandGenerator.GenerateRenderCommandsForModelDescriptor_Instancing(_commandList,_modelPNTTBDescriptorArray,_sceneRuntimeState);
            RenderCommandGenerator.GenerateRenderCommandsForModelDescriptor(_commandList,_modelPNDescriptorArray,_sceneRuntimeState);
            
            _commandList.End();
        }

        override protected void Draw(){
            GraphicsDevice.SubmitCommands(_commandList);
        }


    }
    
}