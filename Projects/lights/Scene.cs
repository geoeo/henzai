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

            // Sun
            _sun = new Model<VertexPositionNormal>(String.Empty,GeometryFactory.GenerateSphereNormal(100,100,1));
            _sun.meshes[0].TryGetMaterial().ambient = new Vector4(1.0f,1.0f,1.0f,1.0f);
            // _sun.meshes[0].TryGetMaterial().ambient = lightColor.ToVector4();
            ref Vector4 lightPos = ref _sceneRuntimeState.Light.Light_DontMutate;
            Vector3 newTranslation = new Vector3(lightPos.X,lightPos.Y,lightPos.Z);
            _sun.SetNewWorldTranslation(ref newTranslation, true);

            var sunRuntimeState = new ModelRuntimeDescriptor<VertexPositionNormal>(_sun,"Phong","Phong",VertexTypes.VertexPositionNormal,PrimitiveTopology.TriangleList);
            sunRuntimeState.CallVertexLayoutGeneration+=ResourceGenerator.GenerateVertexLayoutForPN;
            _modelPNDescriptorList.Add(sunRuntimeState);

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
            var offsets = new Vector3[] {new Vector3(-1.0f,0.0f,0f),new Vector3(1.0f,0.0f,0.0f)};
            // var offsets = new Vector3[] {new Vector3(0.0f,0.0f,0.0f)};
            var instancingData = new InstancingData {Positions = offsets};
            var floor = new Model<VertexPositionNormalTextureTangentBitangent>("paving/",GeometryFactory.GenerateTexturedQuadPNTTB());
            floor.meshes[0].TryGetMaterial().textureDiffuse="pavingColor.jpg";
            floor.meshes[0].TryGetMaterial().textureNormal="pavingNorm.jpg";
            floor.meshes[0].TryGetMaterial().ambient=new Vector4(0.3f,0.3f,0.3f,1.0f);
            var floorRuntimeState = new ModelRuntimeDescriptor<VertexPositionNormalTextureTangentBitangent>(floor,"PositionOffsetPhongBitangentTexture","PhongBitangentTexture",VertexTypes.VertexPositionNormalTextureTangentBitangent,PrimitiveTopology.TriangleStrip);
            floorRuntimeState.TotalInstanceCount = offsets.Length.ToUnsigned();

            floorRuntimeState.CallVertexLayoutGeneration+=ResourceGenerator.GenerateVertexLayoutForPNTTB;
            floorRuntimeState.CallVertexInstanceLayoutGeneration+=ResourceGenerator.GenerateVertexInstanceLayoutForPositionOffset;
            floorRuntimeState.CallTextureResourceLayoutGeneration+=ResourceGenerator.GenerateTextureResourceLayoutForNormalMapping;
            floorRuntimeState.CallTextureResourceSetGeneration+=ResourceGenerator.GenerateTextureResourceSetForNormalMapping;
            floorRuntimeState.CallSamplerGeneration+=ResourceGenerator.GenerateLinearSampler;
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