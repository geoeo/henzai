using System;
using System.Collections.Generic;
using Veldrid;
using Veldrid.StartupUtilities;
using Veldrid.Sdl2;
using Henzai.GUI;

namespace Henzai
{
    /// <summary>
    /// A boilerplate for renderable scenes.
    /// Every disposable resource should be returned by its respective method
    /// </summary>
    public abstract class Renderable : IDisposable
    {
        public Camera _camera {get; private set;} 
        private FrameTimer _frameTimer;
        /// <summary>
        /// Indicates that this scene performs rendering tasks
        /// to support the Renderable owner
        /// </summary>
        public List<Renderable> children;
        private Sdl2Window _contextWindow;
        public Sdl2Window contextWindow => _contextWindow;
        private GraphicsDevice _graphicsDevice;
        public GraphicsDevice graphicsDevice => _graphicsDevice;
        protected Resolution _renderResolution;
        /// <summary>
        /// Holds all created resources which implement IDisposable
        /// </summary>
        private List<IDisposable> _sceneResources;
        /// <summary>
        /// Bind Actions that have to be executed prior to entering the render loop
        /// </summary>
        public event Action PreRenderLoop;
        /// <summary>
        /// Bind Actions that have to be executed prior to every draw call
        /// </summary>
        public event Action<float> PreDraw;
        /// <summary>
        /// Bind Actions that have to be executed after every draw call
        /// </summary>
        public event Action PostDraw;

        public Renderable(string title,Resolution windowSize, GraphicsDeviceOptions graphicsDeviceOptions, GraphicsBackend preferredBackend, bool usePreferredGraphicsBackend){
            WindowCreateInfo windowCI = new WindowCreateInfo()
            {
                X = 100,
                Y = 100,
                WindowWidth = windowSize.Horizontal,
                WindowHeight = windowSize.Vertical,
                WindowTitle = title
            };
            _contextWindow = VeldridStartup.CreateWindow(ref windowCI);

            if(usePreferredGraphicsBackend)
                _graphicsDevice = VeldridStartup.CreateGraphicsDevice(_contextWindow,graphicsDeviceOptions,preferredBackend);
            else
                _graphicsDevice = VeldridStartup.CreateGraphicsDevice(_contextWindow,graphicsDeviceOptions);

            _contextWindow.Title = $"{title} / {_graphicsDevice.BackendType.ToString()}";

            children = new List<Renderable>();

        }

        public Renderable(GraphicsDevice graphicsDevice, Sdl2Window contextWindow){
            _graphicsDevice = graphicsDevice;
            _contextWindow = contextWindow;
        }

        /// <summary>
        /// Sets up windowing and keyboard input
        /// Calls Draw() method in rendering loop
        /// Calls Dispose() when done
        /// </summary>
        public void Run(Resolution renderResolution)
        {
            _renderResolution = renderResolution;
            _sceneResources = new List<IDisposable>();

            _camera = new Camera(renderResolution.Horizontal,renderResolution.Vertical);
            // Tick every millisecond
            _frameTimer = new FrameTimer(1.0);

            _sceneResources.Add(_graphicsDevice);

            _sceneResources.AddRange(CreateResources());
            foreach(var child in children)
                _sceneResources.AddRange(child.CreateResources());

            PreRenderLoop?.Invoke();
            while (_contextWindow.Exists)
            {
                _frameTimer.Start();
                InputSnapshot inputSnapshot = _contextWindow.PumpEvents();
                InputTracker.UpdateFrameInput(inputSnapshot);

                if(_contextWindow.Exists){

                    PreDraw?.Invoke(_frameTimer.prevFrameTicksInSeconds);

                    Draw();
                    foreach(var child in children)
                        child.Draw();

                    graphicsDevice.SwapBuffers();
                    PostDraw?.Invoke();
                }

                _camera.Update(_frameTimer.prevFrameTicksInSeconds);
                _frameTimer.Stop();
            }

            Dispose();

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
        virtual public void Dispose(){
            foreach(IDisposable resource in _sceneResources)
                resource.Dispose();              
        }
    }
}