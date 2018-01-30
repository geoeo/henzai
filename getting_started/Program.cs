using System.IO;
using System.Numerics;
using Veldrid;
using Veldrid.StartupUtilities;
using Veldrid.Sdl2;

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

        static void Main(string[] args)
        {
            //Console.WriteLine("Hello World!");
            WindowCreateInfo windowCI = new WindowCreateInfo()
            {
                X = 100,
                Y = 100,
                WindowWidth = 960,
                WindowHeight = 540,
                WindowTitle = "Veldrid Getting Started"
            };
            Sdl2Window window = VeldridStartup.CreateWindow(ref windowCI);

            _graphicsDevice = VeldridStartup.CreateGraphicsDevice(window);

            CreateResources();

            while (window.Exists)
            {
                window.PumpEvents();
            }
        }

        private static void CreateResources()
        {
            ResourceFactory _factory = _graphicsDevice.ResourceFactory;

            VertexPositionColor[] quadVerticies = {
                new VertexPositionColor(new Vector2(-0.75f,0.75f),RgbaFloat.Red),
                new VertexPositionColor(new Vector2(0.75f,0.75f),RgbaFloat.Green),
                new VertexPositionColor(new Vector2(-0.75f,-0.75f),RgbaFloat.Blue),
                new VertexPositionColor(new Vector2(0.75f,-0.75f),RgbaFloat.Yellow)
            };

            ushort[] quadIndicies = { 0, 1, 2, 3 };

            // declare (VBO) buffers
            _vertexBuffer 
                = _factory.CreateBuffer(new BufferDescription(4 * VertexPositionColor.SizeInBytes, BufferUsage.VertexBuffer));
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

        }

        
        private static Shader LoadShader(ShaderStages stage){
            string extension = null;
            switch(_graphicsDevice.BackendType){
                case GraphicsBackend.OpenGL:
                    extension = "glsl";
                    break;
                case GraphicsBackend.Metal:
                    extension = "glsl";
                    // extension = "metal";
                    break;
                default: throw new System.InvalidOperationException();
            }

            string entryPoint = stage == ShaderStages.Vertex ? "VS" : "FS";
            string path = Path.Combine(System.AppContext.BaseDirectory,"Shaders","${stage.ToString()}.{extension}");
            byte[] shaderBytes = File.ReadAllBytes(path);
            return _graphicsDevice.ResourceFactory.CreateShader(new ShaderDescription(stage,shaderBytes,entryPoint));
        }
    }
    struct VertexPositionColor
    {
        public Vector2 Position; // Position in NDC
        public RgbaFloat Colour;
        public VertexPositionColor(Vector2 position, RgbaFloat colour)
        {
            Position = position;
            Colour = colour;
        }
        // 2*8 Bytes for Position + 8 Bytes for Colour
        public const uint SizeInBytes = 24;
    }
}
