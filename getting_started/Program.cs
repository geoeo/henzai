using System;
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
            WindowCreateInfo windowCI = new WindowCreateInfo(){
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

        private static void CreateResources(){
            ResourceFactory _factory = _graphicsDevice.ResourceFactory;
        }
    }
}
