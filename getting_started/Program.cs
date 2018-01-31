using System.IO;
using System.Numerics;
using Veldrid;
using Veldrid.StartupUtilities;
using Veldrid.Sdl2;
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

            VertexPositionColour[] quadVerticies = {
                new VertexPositionColour(new Vector2(-0.75f,0.75f),RgbaFloat.Red),
                new VertexPositionColour(new Vector2(0.75f,0.75f),RgbaFloat.Green),
                new VertexPositionColour(new Vector2(-0.75f,-0.75f),RgbaFloat.Blue),
                new VertexPositionColour(new Vector2(0.75f,-0.75f),RgbaFloat.Yellow)
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

            VertexLayoutDescription vertexLayout 
                = new VertexLayoutDescription(
                    new VertexElementDescription("Position",VertexElementSemantic.Position,VertexElementFormat.Float2),
                    new VertexElementDescription("Colour",VertexElementSemantic.Color,VertexElementFormat.Float4)
                );

            _vertexShader = LoadShader(ShaderStages.Vertex);
            _fragmentShader = LoadShader(ShaderStages.Fragment);

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
                ResourceLayouts = System.Array.Empty<ResourceLayout>(),
                ShaderSet = new ShaderSetDescription(
                    vertexLayouts: new VertexLayoutDescription[] {vertexLayout},
                    shaders: new Shader[] {_vertexShader,_fragmentShader}
                ),
                Outputs = _graphicsDevice.SwapchainFramebuffer.OutputDescription
            };

            _pipeline = _factory.CreateGraphicsPipeline(pipelineDescription);

            _commandList = _factory.CreateCommandList();

        }

        
        private static Shader LoadShader(ShaderStages stage){
            string extension = null;
            switch(_graphicsDevice.BackendType){
                case GraphicsBackend.OpenGL:
                    extension = "glsl";
                    break;
                case GraphicsBackend.Metal:
                    extension = "metallib";
                    break;
                default: throw new System.InvalidOperationException();
            }

            string entryPoint = stage == ShaderStages.Vertex ? "VS" : "FS";
            string path = Path.Combine(System.AppContext.BaseDirectory,"Shaders",$"{stage.ToString()}.{extension}");
            byte[] shaderBytes = File.ReadAllBytes(path);
            return _graphicsDevice.ResourceFactory.CreateShader(new ShaderDescription(stage,shaderBytes,entryPoint));
        }

        private static void Draw(){
            _commandList.Begin();
            _commandList.SetFramebuffer(_graphicsDevice.SwapchainFramebuffer);
            _commandList.SetFullViewports();
            _commandList.ClearColorTarget(0,RgbaFloat.Black);
            _commandList.SetVertexBuffer(0,_vertexBuffer);
            _commandList.SetIndexBuffer(_indexBuffer,IndexFormat.UInt16);
            _commandList.SetPipeline(_pipeline);
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
            _graphicsDevice.Dispose();
        }
    }
    // struct VertexPositionColour
    // {
    //     public Vector2 Position; // Position in NDC
    //     public RgbaFloat Colour;
    //     public VertexPositionColour(Vector2 position, RgbaFloat colour)
    //     {
    //         Position = position;
    //         Colour = colour;
    //     }
    //     // 2*4 Bytes for Position + 16 Bytes for Colour
    //     public const uint SizeInBytes = 24;
    // }
}
