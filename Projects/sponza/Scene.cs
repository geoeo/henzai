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

        private List<ModelRuntimeDescriptor<VertexPositionTexture>> _modelPTDescriptorList;
        private ModelRuntimeDescriptor<VertexPositionTexture> [] _modelPTDescriptorArray;
        private List<ModelRuntimeDescriptor<VertexPositionColor>> _modelPCDescriptorList;
        private ModelRuntimeDescriptor<VertexPositionColor> [] _modelPCDescriptorArray;

        Model<VertexPositionNormalTextureTangentBitangent> _sponza;
        Model<VertexPositionNormal> _sun;

        public Scene(string title,Resolution windowSize, GraphicsDeviceOptions graphicsDeviceOptions, RenderOptions renderOptions)
            : base(title,windowSize,graphicsDeviceOptions,renderOptions){
                _sceneRuntimeState = new SceneRuntimeDescriptor();

                _modelPNTTBDescriptorList = new List<ModelRuntimeDescriptor<VertexPositionNormalTextureTangentBitangent>>();
                _modelPNDescriptorList = new List<ModelRuntimeDescriptor<VertexPositionNormal>>();
                _modelPTDescriptorList = new List<ModelRuntimeDescriptor<VertexPositionTexture>>();
                _modelPCDescriptorList = new List<ModelRuntimeDescriptor<VertexPositionColor>>();


                PreRenderLoop+=FormatResourcesForRuntime;
        }


        // TODO: Investigate putting this in renderable
        override protected void FormatResourcesForRuntime(){

            foreach(var modelState in _modelPNTTBDescriptorList)
                modelState.FormatResourcesForRuntime();
            foreach(var modelState in _modelPNDescriptorList)
                modelState.FormatResourcesForRuntime();
            foreach(var modelState in _modelPTDescriptorList)
                modelState.FormatResourcesForRuntime();
            foreach(var modelState in _modelPCDescriptorList)
                modelState.FormatResourcesForRuntime();


            _modelPNTTBDescriptorArray = _modelPNTTBDescriptorList.ToArray();
            _modelPNDescriptorArray = _modelPNDescriptorList.ToArray();
            _modelPTDescriptorArray = _modelPTDescriptorList.ToArray();
            _modelPCDescriptorArray = _modelPCDescriptorList.ToArray();
        }

        override protected void CreateResources(){

            // RgbaFloat lightColor = RgbaFloat.Orange;
            RgbaFloat lightColor = RgbaFloat.LightGrey;
            _sceneRuntimeState.Light = new Light(new Vector4(0,200,0,1),lightColor,0.1f);
            _sceneRuntimeState.Camera = Camera;
            _sceneRuntimeState.SpotLight = Light.NO_POINTLIGHT;

            // string filePath = Path.Combine(AppContext.BaseDirectory, "armor/armor.dae"); 
            // string filePath = Path.Combine(AppContext.BaseDirectory, "nanosuit/nanosuit.obj"); 
            
            //TODO: Write method to split Model into different vertex types if some textures are not available!
            var sponzaModels = AssimpLoader.LoadModelsFromFile(AppContext.BaseDirectory,"sponza/sponza.obj");
             _sponza = sponzaModels.modelPNTTB;
            var sponzaTexOnly = sponzaModels.modelPT;
            var sponzaColorOnly = sponzaModels.modelPC;
            // _nanosuit.SetAmbientForAllMeshes(new Vector4(0.1f,0.1f,0.1f,1.0f));
            // _model = AssimpLoader.LoadFromFile<VertexPositionNormalTextureTangentBitangent>(AppContext.BaseDirectory,"sponza/sponza.obj",VertexPositionNormalTextureTangentBitangent.HenzaiType);
        
            // GeometryUtils.CheckTBN(_model);
            // var sun = new Model<VertexPositionNormalTextureTangentBitangent>("water",GeometryFactory.generateSphereTangentBitangent(100,100,1));
            _sun = new Model<VertexPositionNormal>(String.Empty,GeometryFactory.GenerateSphereNormal(100,100,1));
            _sun.meshes[0].TryGetMaterial().textureDiffuse = "Water.jpg";
            _sun.meshes[0].TryGetMaterial().textureNormal = "WaterNorm.jpg";
            _sun.meshes[0].TryGetMaterial().ambient = lightColor.ToVector4();
            // _sun.meshes[0].TryGetMaterial().ambient = lightColor.ToVector4();
            ref Vector4 lightPos = ref _sceneRuntimeState.Light.LightPos_DontMutate;
            Vector3 newTranslation = new Vector3(lightPos.X,lightPos.Y,lightPos.Z);
            _sun.SetNewWorldTranslation(ref newTranslation, true);

            var sponzaRuntimeState = new ModelRuntimeDescriptor<VertexPositionNormalTextureTangentBitangent>(_sponza,"PhongBitangentTexture","PhongBitangentTexture", VertexTypes.VertexPositionNormalTextureTangentBitangent,PrimitiveTopology.TriangleList);
            sponzaRuntimeState.CallVertexLayoutGeneration+=ResourceGenerator.GenerateVertexLayoutForPNTTB;
            sponzaRuntimeState.CallSamplerGeneration+=ResourceGenerator.GenerateTriLinearSampler;
            sponzaRuntimeState.CallTextureResourceLayoutGeneration+=ResourceGenerator.GenerateTextureResourceLayoutForNormalMapping;
            sponzaRuntimeState.CallTextureResourceSetGeneration+=ResourceGenerator.GenerateTextureResourceSetForNormalMapping;

            var sponzaRuntimeStateTexOnly = new ModelRuntimeDescriptor<VertexPositionTexture>(sponzaTexOnly,"Texture","Texture", VertexTypes.VertexPositionTexture,PrimitiveTopology.TriangleList);
            sponzaRuntimeStateTexOnly.CallVertexLayoutGeneration+=ResourceGenerator.GenerateVertexLayoutForPT;
            sponzaRuntimeStateTexOnly.CallSamplerGeneration+=ResourceGenerator.GenerateTriLinearSampler;
            sponzaRuntimeStateTexOnly.CallTextureResourceLayoutGeneration+=ResourceGenerator.GenerateTextureResourceLayoutForDiffuseMapping;
            sponzaRuntimeStateTexOnly.CallTextureResourceSetGeneration+=ResourceGenerator.GenerateTextureResourceSetForDiffuseMapping;

            var sponzaRuntimeStateColorOnly = new ModelRuntimeDescriptor<VertexPositionColor>(sponzaColorOnly,"Color","Color", VertexTypes.VertexPositionColor,PrimitiveTopology.TriangleList);
            sponzaRuntimeStateColorOnly.CallVertexLayoutGeneration+=ResourceGenerator.GenerateVertexLayoutForPC;

            // var sunRuntimeState = new ModelRuntimeState<VertexPositionNormalTextureTangentBitangent>(sun,"PhongBitangentTexture","PhongBitangentTexture");
            // sunRuntimeState.CallVertexLayoutGeneration+=ResourceGenerator.GenerateVertexLayoutForPNTTB;
            // sunRuntimeState.CallSamplerGeneration+=ResourceGenerator.GenerateLinearSampler;
            // sunRuntimeState.CallTextureResourceLayoutGeneration+=ResourceGenerator.GenerateTextureResourceLayoutForNormalMapping;
            // sunRuntimeState.CallTextureResourceSetGeneration+=ResourceGenerator.GenerateTextureResourceSetForNormalMapping;

            _modelPNTTBDescriptorList.Add(sponzaRuntimeState);
            _modelPCDescriptorList.Add(sponzaRuntimeStateColorOnly);
            // _modelPTDescriptorList.Add(sponzaRuntimeStateTexOnly);
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
            
            _commandList.End();
        }

        override protected void Draw(){
            GraphicsDevice.SubmitCommands(_commandList);
        }


    }
    
}