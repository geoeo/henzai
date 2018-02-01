using System.Numerics;
using Veldrid;
using Veldrid.StartupUtilities;
using Veldrid.Sdl2;
using Veldrid.OpenGL;
using Henzai;

namespace getting_started
{
    // https://mellinoe.github.io/veldrid-docs/articles/getting-started/getting-started-part1.html
    class Program
    {
        private static GraphicsDevice _graphicsDevice;
        private static CommandList _commandList;
        private static DeviceBuffer _vertexBuffer;
        private static DeviceBuffer _indexBuffer;
        private static Shader _vertexShader;
        private static Shader _fragmentShader;
        private static Pipeline _pipeline;
        private static Camera _camera;
        private static DeviceBuffer _cameraProjViewBuffer;
        private static ResourceSet _resourceSet;
        private static ResourceLayout _resourceLayout;
        static void Main(string[] args)
        {
            WindowCreateInfo windowCI = new WindowCreateInfo()
            {
                X = 100,
                Y = 100,
                WindowWidth = 960,
                WindowHeight = 540,
                WindowTitle = "Veldrid Getting Started"
            };
            Sdl2Window window = VeldridStartup.CreateWindow(ref windowCI);

            _camera = new Camera(960,540);

            _graphicsDevice = VeldridStartup.CreateGraphicsDevice(window,GraphicsBackend.OpenGL);
            //_graphicsDevice = VeldridStartup.CreateGraphicsDevice(window); // Defaults to metal on mac

            CreateResources();

            while (window.Exists)
            {
                window.PumpEvents();
                Draw();
            }

            DisposeResources();
        }

        private static void CreateResources()
        {
            ResourceFactory _factory = _graphicsDevice.ResourceFactory;

            _cameraProjViewBuffer = _factory.CreateBuffer(new BufferDescription(128,BufferUsage.UniformBuffer));

            ResourceLayoutElementDescription resourceLayoutElementDescription = new ResourceLayoutElementDescription("projView",ResourceKind.UniformBuffer,ShaderStages.Vertex);
            ResourceLayoutElementDescription[] resourceLayoutElementDescriptions = {resourceLayoutElementDescription};
            ResourceLayoutDescription resourceLayoutDescription = new ResourceLayoutDescription(resourceLayoutElementDescriptions);

            _resourceLayout = _factory.CreateResourceLayout(resourceLayoutDescription);
            BindableResource[] bindableResources = new BindableResource[]{_cameraProjViewBuffer};
            ResourceSetDescription resourceSetDescription = new ResourceSetDescription(_resourceLayout,bindableResources);
            
            _resourceSet = _factory.CreateResourceSet(resourceSetDescription);

            VertexPositionColour[] quadVerticies = {
                new VertexPositionColour(new Vector2(-1.0f,1.0f),RgbaFloat.Red),
                new VertexPositionColour(new Vector2(1.0f,1.0f),RgbaFloat.Green),
                new VertexPositionColour(new Vector2(-1.0f,-1.0f),RgbaFloat.Blue),
                new VertexPositionColour(new Vector2(1.0f,-1.0f),RgbaFloat.Yellow)
            };

            ushort[] quadIndicies = { 0, 1, 2, 3 };


            // declare (VBO) buffers
            _vertexBuffer 
                = _factory.CreateBuffer(new BufferDescription(4 * VertexPositionColour.SizeInBytes, BufferUsage.VertexBuffer));
            _indexBuffer 
                = _factory.CreateBuffer(new BufferDescription(4*sizeof(ushort),BufferUsage.IndexBuffer));

            // fill buffers with data
            _graphicsDevice.UpdateBuffer(_vertexBuffer,0,quadVerticies);
            _graphicsDevice.UpdateBuffer(_indexBuffer,0,quadIndicies);
            _graphicsDevice.UpdateBuffer(_cameraProjViewBuffer,0,_camera.ViewMatrix);
            _graphicsDevice.UpdateBuffer(_cameraProjViewBuffer,64,_camera.ProjectionMatrix);

            VertexLayoutDescription vertexLayout 
                = new VertexLayoutDescription(
                    new VertexElementDescription("Position",VertexElementSemantic.Position,VertexElementFormat.Float2),
                    new VertexElementDescription("Colour",VertexElementSemantic.Color,VertexElementFormat.Float4)
                );

            _vertexShader = IO.LoadShader(ShaderStages.Vertex,_graphicsDevice);
            _fragmentShader = IO.LoadShader(ShaderStages.Fragment,_graphicsDevice);

            GraphicsPipelineDescription pipelineDescription = new GraphicsPipelineDescription(){
                BlendState = BlendStateDescription.SingleOverrideBlend,
                DepthStencilState = new DepthStencilStateDescription(
                    depthTestEnabled: true,
                    depthWriteEnabled: true,
                    comparisonKind: ComparisonKind.LessEqual),
                RasterizerState = new RasterizerStateDescription(
                    cullMode: FaceCullMode.Back,
                    fillMode: PolygonFillMode.Solid,
                    frontFace: FrontFace.Clockwise,
                    depthClipEnabled: true,
                    scissorTestEnabled: false
                ),
                PrimitiveTopology = PrimitiveTopology.TriangleStrip,
                ResourceLayouts = new ResourceLayout[] {_resourceLayout},
                ShaderSet = new ShaderSetDescription(
                    vertexLayouts: new VertexLayoutDescription[] {vertexLayout},
                    shaders: new Shader[] {_vertexShader,_fragmentShader}
                ),
                Outputs = _graphicsDevice.SwapchainFramebuffer.OutputDescription
            };

            _pipeline = _factory.CreateGraphicsPipeline(pipelineDescription);

            _commandList = _factory.CreateCommandList();

        }

        private static void Draw(){
            _commandList.Begin();
            _commandList.SetFramebuffer(_graphicsDevice.SwapchainFramebuffer);
            _commandList.SetFullViewports();
            _commandList.ClearColorTarget(0,RgbaFloat.Black);
            _commandList.SetVertexBuffer(0,_vertexBuffer);
            _commandList.SetIndexBuffer(_indexBuffer,IndexFormat.UInt16);
            _commandList.SetPipeline(_pipeline);
            _commandList.SetGraphicsResourceSet(0,_resourceSet); // Always after SetPipeline
            _commandList.DrawIndexed(
                indexCount: 4,
                instanceCount: 1,
                indexStart: 0,
                vertexOffset: 0,
                instanceStart: 0
            );
            _commandList.End();
            _graphicsDevice.SubmitCommands(_commandList);
            _graphicsDevice.SwapBuffers();
        }

        private static void DisposeResources(){
            _pipeline.Dispose();
            _vertexShader.Dispose();
            _fragmentShader.Dispose();
            _commandList.Dispose();
            _vertexBuffer.Dispose();
            _indexBuffer.Dispose();
            _cameraProjViewBuffer.Dispose();
            _graphicsDevice.Dispose();
            _resourceLayout.Dispose();
            _resourceSet.Dispose();
        }
    }
}
