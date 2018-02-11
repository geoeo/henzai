using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        private Camera _camera {get; set;} 
        public Camera camera => _camera;
        private FrameTimer _frameTimer;
        /// <summary>
        /// Renderable objects which should be drawn before this.Draw()
        /// </summary>
        public List<Renderable> childrenPre;
        /// <summary>
        /// Renderable objects which should be drawn after this.Draw()
        /// </summary>
        public List<Renderable> childrenPost;
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
        /// <summary>
        /// Includes this.BuildCommandList()
        /// </summary>
        private Task[] buildCommandListTasks;
        private Task[] drawTasksPre;
        private Task[] drawTasksPost;

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

            childrenPre = new List<Renderable>();
            childrenPost = new List<Renderable>();

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
            foreach(var child in childrenPre)
                _sceneResources.AddRange(child.CreateResources());
            foreach(var child in childrenPost)
                _sceneResources.AddRange(child.CreateResources());

            List<Renderable> allChildren = new List<Renderable>();
            allChildren.AddRange(childrenPre);
            allChildren.AddRange(childrenPost);

            buildCommandListTasks = new Task[allChildren.Count+1];
            drawTasksPre = new Task[childrenPre.Count];
            drawTasksPost = new Task[childrenPost.Count];

            PreRenderLoop?.Invoke();
            while (_contextWindow.Exists)
            {
                _frameTimer.Start();
                InputSnapshot inputSnapshot = _contextWindow.PumpEvents();
                InputTracker.UpdateFrameInput(inputSnapshot);

                if(_contextWindow.Exists){

                    PreDraw?.Invoke(_frameTimer.prevFrameTicksInSeconds);

                    buildCommandListTasks[0] = Task.Run(() => this.BuildCommandList());
                    for(int i = 0; i < allChildren.Count; i++){
                        var child = allChildren[i];
                        buildCommandListTasks[i+1] = Task.Run(() => child.BuildCommandList());
                    } 

                    // Wait untill every command list has been built
                    Task.WaitAll(buildCommandListTasks);

                    for(int i = 0; i < childrenPre.Count; i++){
                        var child = childrenPre[i];
                        drawTasksPre[i] = Task.Run(() => child.Draw());
                    } 

                    Task.WaitAll(drawTasksPre);

                    Draw();

                    for(int i = 0; i < childrenPost.Count; i++){
                        var child = childrenPost[i];
                        drawTasksPost[i] = Task.Run(() => child.Draw());
                    } 

                    Task.WaitAll(drawTasksPost);

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

        abstract protected void BuildCommandList();

        /// <summary>
        /// Disposes of all elements in _sceneResources
        /// </summary>
        virtual public void Dispose(){
            foreach(IDisposable resource in _sceneResources)
                resource.Dispose();              
        }
    }
}