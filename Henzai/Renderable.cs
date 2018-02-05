using System;
using Veldrid;
using Veldrid.StartupUtilities;
using Veldrid.Sdl2;

namespace Henzai
{
    /// <summary>
    /// A boilerplate for renderable scenes.
    /// </summary>
    public abstract class Renderable
    {

        public Camera _camera {get; private set;} 
        protected GraphicsDevice _graphicsDevice {get; private set;}
        protected Resolution _renderResolution;

        /// <summary>
        /// Sets up windowing and keyboard input
        /// Calls Draw() method in rendering loop
        /// Calls Dispose() when done
        /// </summary>
        public void Run(string title,Resolution renderResolution,Resolution windowSize, GraphicsBackend preferredBackend, bool useDefaultGraphicsBackend)
        {

            _renderResolution = renderResolution;

            WindowCreateInfo windowCI = new WindowCreateInfo()
            {
                X = 100,
                Y = 100,
                WindowWidth = windowSize.Horizontal,
                WindowHeight = windowSize.Vertical,
                WindowTitle = title
            };
            Sdl2Window window = VeldridStartup.CreateWindow(ref windowCI);

            _camera = new Camera(renderResolution.Horizontal,renderResolution.Vertical);

            if(useDefaultGraphicsBackend)
                _graphicsDevice = VeldridStartup.CreateGraphicsDevice(window);
            else
                _graphicsDevice = VeldridStartup.CreateGraphicsDevice(window,preferredBackend);

            CreateResources();

            while (window.Exists)
            {
                InputSnapshot inputSnapshot = window.PumpEvents();
                InputTracker.UpdateFrameInput(inputSnapshot);

                float deltaSeconds = 1/60f;
                _camera.Update(deltaSeconds);

                if(window.Exists)
                    Draw();
            }

            DisposeResources();

        }
      
        /// <summary>
        /// Executes the defined command list(s)
        /// </summary>
        abstract protected void Draw();

        /// <summary>
        /// Creates resources used to render e.g. Buffers, Textures etc.
        /// </summary>
        abstract protected void CreateResources();

        /// <summary>
        /// Disposes any created resources
        /// </summary>
        abstract protected void DisposeResources();
    }
}