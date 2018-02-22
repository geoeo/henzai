using System;
using System.Numerics;
using System.Collections.Generic;
using System.IO;
using Veldrid;
using Veldrid.StartupUtilities;
using Veldrid.Sdl2;
using Veldrid.OpenGL;
using Henzai;
using Henzai.Extensions;
using Henzai.Geometry;
using Henzai.Runtime;

namespace Henzai.Examples
{
    internal class Scene : Renderable
    {

        private CommandList _commandList;
        private List<DeviceBuffer> _vertexBuffers;
        private List<DeviceBuffer> _indexBuffers;
        private Shader _vertexShader;
        private Shader _fragmentShader;
        private Pipeline _pipeline;
        private DeviceBuffer _cameraProjViewBuffer;
        private ResourceSet _cameraResourceSet;
        private ResourceLayout _resourceLayout;

        Model<VertexPositionNormalTexture> _sphereModel;

        public Scene(string title,Resolution windowSize, GraphicsDeviceOptions graphicsDeviceOptions, GraphicsBackend preferredBackend, bool usePreferredGraphicsBackend)
            : base(title,windowSize,graphicsDeviceOptions,preferredBackend,usePreferredGraphicsBackend){
                _vertexBuffers = new List<DeviceBuffer>();
                _indexBuffers = new List<DeviceBuffer>();


        }


        override protected List<IDisposable> CreateResources(){

            //string filePath = Path.Combine(AppContext.BaseDirectory, "Models/sphere.obj");
            //string filePath = Path.Combine(AppContext.BaseDirectory, "Models/sphere_centered.obj");
            string filePath = Path.Combine(AppContext.BaseDirectory, "Models/chinesedragon.dae");
            _sphereModel = AssimpLoader.LoadFromFile<VertexPositionNormalTexture>(filePath,VertexPositionNormalTexture.HenzaiType);

            ResourceFactory _factory = graphicsDevice.ResourceFactory;

            _cameraProjViewBuffer = _factory.CreateBuffer(new BufferDescription(128,BufferUsage.UniformBuffer | BufferUsage.Dynamic));

            ResourceLayoutElementDescription resourceLayoutElementDescription = new ResourceLayoutElementDescription("projView",ResourceKind.UniformBuffer,ShaderStages.Vertex);
            ResourceLayoutElementDescription[] resourceLayoutElementDescriptions = {resourceLayoutElementDescription};
            ResourceLayoutDescription resourceLayoutDescription = new ResourceLayoutDescription(resourceLayoutElementDescriptions);
            BindableResource[] bindableResources = new BindableResource[]{_cameraProjViewBuffer};

            _resourceLayout = _factory.CreateResourceLayout(resourceLayoutDescription);
            ResourceSetDescription resourceSetDescription = new ResourceSetDescription(_resourceLayout,bindableResources);
            
            _cameraResourceSet = _factory.CreateResourceSet(resourceSetDescription);

            for(int i = 0; i < _sphereModel.meshCount; i++){

                DeviceBuffer vertexBuffer 
                    =  _factory.CreateBuffer(new BufferDescription(_sphereModel.meshes[i].vertices.LengthUnsigned() * VertexPositionNormalTexture.SizeInBytes, BufferUsage.VertexBuffer)); 

                DeviceBuffer indexBuffer
                    = _factory.CreateBuffer(new BufferDescription(_sphereModel.meshIndicies[i].LengthUnsigned()*sizeof(uint),BufferUsage.IndexBuffer));
                    

                _vertexBuffers.Add(vertexBuffer);
                _indexBuffers.Add(indexBuffer);

                graphicsDevice.UpdateBuffer(vertexBuffer,0,_sphereModel.meshes[i].vertices);
                graphicsDevice.UpdateBuffer(indexBuffer,0,_sphereModel.meshIndicies[i]);
            }

            VertexLayoutDescription vertexLayout 
                = new VertexLayoutDescription(
                    new VertexElementDescription("Position",VertexElementSemantic.Position,VertexElementFormat.Float3),
                    new VertexElementDescription("Normal",VertexElementSemantic.Normal,VertexElementFormat.Float3),
                    new VertexElementDescription("UV",VertexElementSemantic.TextureCoordinate,VertexElementFormat.Float2)
                );

            _vertexShader = IO.LoadShader(string.Empty,ShaderStages.Vertex,graphicsDevice);
            _fragmentShader = IO.LoadShader(string.Empty,ShaderStages.Fragment,graphicsDevice);

            GraphicsPipelineDescription pipelineDescription = new GraphicsPipelineDescription(){
                BlendState = BlendStateDescription.SingleOverrideBlend,
                DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual,
                RasterizerState = new RasterizerStateDescription(
                    cullMode: FaceCullMode.Back,
                    fillMode: PolygonFillMode.Solid,
                    frontFace: FrontFace.Clockwise,
                    depthClipEnabled: true,
                    scissorTestEnabled: false
                ),
                PrimitiveTopology = PrimitiveTopology.TriangleList,
                ResourceLayouts = new ResourceLayout[] {_resourceLayout},
                ShaderSet = new ShaderSetDescription(
                    vertexLayouts: new VertexLayoutDescription[] {vertexLayout},
                    shaders: new Shader[] {_vertexShader,_fragmentShader}
                ),
                Outputs = graphicsDevice.SwapchainFramebuffer.OutputDescription
            };

            _pipeline = _factory.CreateGraphicsPipeline(pipelineDescription);

            _commandList = _factory.CreateCommandList();

            List<IDisposable> disposeList = new List<IDisposable>()
            {
                _commandList,
                _pipeline,
                _vertexShader,
                _fragmentShader,
                _cameraProjViewBuffer,
                _cameraResourceSet,
                _resourceLayout
            };

            disposeList.AddRange(_vertexBuffers);
            disposeList.AddRange(_indexBuffers);

            return disposeList;

        }

        override protected void BuildCommandList(){
            _commandList.Begin();
            _commandList.SetFramebuffer(graphicsDevice.SwapchainFramebuffer);
            _commandList.SetPipeline(_pipeline);
            _commandList.SetFullViewports();
            _commandList.ClearColorTarget(0,RgbaFloat.White);
            _commandList.ClearDepthStencil(1f);
            for(int i = 0; i < _sphereModel.meshCount; i++){

                _commandList.SetVertexBuffer(0,_vertexBuffers[i]);
                _commandList.SetIndexBuffer(_indexBuffers[i],IndexFormat.UInt32);
                _commandList.UpdateBuffer(_cameraProjViewBuffer,0,camera.ViewMatrix);
                _commandList.UpdateBuffer(_cameraProjViewBuffer,64,camera.ProjectionMatrix);
                _commandList.SetGraphicsResourceSet(0,_cameraResourceSet); // Always after SetPipeline
                _commandList.DrawIndexed(
                    indexCount: _sphereModel.meshIndicies[i].Length.ToUnsigned(),
                    instanceCount: 1,
                    indexStart: 0,
                    vertexOffset: 0,
                    instanceStart: 0
                );
                //_commandList.Draw(_sphereModel.meshes[i].vertices.Length.ToUnsigned());

            } 

            _commandList.End();
        }

        override protected void Draw(){
            graphicsDevice.SubmitCommands(_commandList);
        }


    }
    
}