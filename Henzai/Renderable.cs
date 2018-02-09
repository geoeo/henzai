using System;
using System.Collections.Generic;
using Veldrid;
using Veldrid.StartupUtilities;
using Veldrid.Sdl2;

namespace Henzai
{
    /// <summary>
    /// A boilerplate for renderable scenes.
    /// Every disposable resource should be returned by its respective method
    /// </summary>
    public abstract class Renderable
    {

        public Camera _camera {get; private set;} 
        protected GraphicsDevice _graphicsDevice {get; private set;}
        protected Resolution _renderResolution;
        /// <summary>
        /// Holds all created resources which implement IDisposable
        /// </summary>
        private List<IDisposable> _sceneResources;
        /// <summary>
        /// Bind Actions that have to be executed prior to entering the render loop
        /// </summary>
        protected event Action PreRenderLoop;
        /// <summary>
        /// Bind Actions that have to be executed prior to every draw call
        /// </summary>
        protected event Action PreDraw;
        /// <summary>
        /// Bind Actions that have to be executed after every draw call
        /// </summary>
        protected event Action PostDraw;

        /// <summary>
        /// Sets up windowing and keyboard input
        /// Calls Draw() method in rendering loop
        /// Calls Dispose() when done
        /// </summary>
        public void Run(string title,Resolution renderResolution,Resolution windowSize, GraphicsDeviceOptions graphicsDeviceOptions, GraphicsBackend preferredBackend, bool usePreferredGraphicsBackend)
        {
            _renderResolution = renderResolution;
            _sceneResources = new List<IDisposable>();

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

            if(usePreferredGraphicsBackend)
                _graphicsDevice = VeldridStartup.CreateGraphicsDevice(window,graphicsDeviceOptions,preferredBackend);
            else
                _graphicsDevice = VeldridStartup.CreateGraphicsDevice(window,graphicsDeviceOptions);

            _sceneResources.Add(_graphicsDevice);

            window.Title = $"{title} / {_graphicsDevice.BackendType.ToString()}";

            _sceneResources.AddRange(CreateResources());

            PreRenderLoop?.Invoke();
            while (window.Exists)
            {
                InputSnapshot inputSnapshot = window.PumpEvents();
                InputTracker.UpdateFrameInput(inputSnapshot);

                float deltaSeconds = 1/60f;
                _camera.Update(deltaSeconds);

                if(window.Exists){
                    PreDraw?.Invoke();
                    Draw();
                    PostDraw?.Invoke();
                }
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
        abstract protected List<IDisposable> CreateResources();

        /// <summary>
        /// Disposes of all elements in _sceneResources
        /// </summary>
        virtual protected void DisposeResources(){
            foreach(IDisposable resource in _sceneResources)
                resource.Dispose();              
        }
    }
}