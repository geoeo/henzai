using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Veldrid;
using Veldrid.StartupUtilities;
using Veldrid.Sdl2;
using Henzai.UserInterface;
using Henzai.Extensions;

namespace Henzai.Runtime
{
    //TODO: Investigate Making a Completely Separate Thread for UI
    // TODO: Profile Render Loop
    /// <summary>
    /// A boilerplate for renderable scenes.
    /// Every disposable resource should be returned by its respective method
    /// </summary>
    public abstract class Renderable : IDisposable
    {
        private Camera _camera {get; set;} 
        public Camera Camera => _camera;
        private FrameTimer _frameTimer;
        /// <summary>
        /// Flag indicated if this is a child to another renderable
        /// </summary>
        private bool _isChild = false;
        //TODO: Investigate Switching to Array
        /// <summary>
        /// Renderable objects which should be drawn before this.Draw()
        /// </summary>
        private List<Renderable> _childrenPre = new List<Renderable>();
        public List<Renderable> ChildrenPre => _childrenPre;
        /// <summary>
        /// Renderable objects which should be drawn after this.Draw()
        /// </summary>
        private List<Renderable> _childrenPost = new List<Renderable>();
        public List<Renderable> ChildrenPost => _childrenPost;
        private Sdl2Window _contextWindow;
        public Sdl2Window contextWindow => _contextWindow;
        private GraphicsDevice _graphicsDevice;
        public GraphicsDevice GraphicsDevice => _graphicsDevice;
        private RenderOptions _renderOptions;
        protected DisposeCollectorResourceFactory _factory;
        protected CommandList _commandList;
        protected Resolution _renderResolution;
        /// <summary>
        /// Holds all created resources which implement IDisposable
        /// </summary>
        // private List<IDisposable> _sceneResources;
        // /// <summary>
        // /// Bind Actions that have to be executed prior to entering the render loop
        // /// </summary>
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

        public Renderable(string title,Resolution windowSize, GraphicsDeviceOptions graphicsDeviceOptions, RenderOptions renderOptions){
            WindowCreateInfo windowCI = new WindowCreateInfo()
            {
                X = 100,
                Y = 100,
                WindowWidth = windowSize.Horizontal,
                WindowHeight = windowSize.Vertical,
                WindowTitle = title
            };
            _contextWindow = VeldridStartup.CreateWindow(ref windowCI);

            if(renderOptions.UsePreferredGraphicsBackend)
                _graphicsDevice = VeldridStartup.CreateGraphicsDevice(_contextWindow,graphicsDeviceOptions,renderOptions.PreferredGraphicsBackend);
            else
                _graphicsDevice = VeldridStartup.CreateGraphicsDevice(_contextWindow,graphicsDeviceOptions);

            _renderOptions = renderOptions;
            _contextWindow.Title = $"{title} / {_graphicsDevice.BackendType.ToString()}";
            _factory = new DisposeCollectorResourceFactory(_graphicsDevice.ResourceFactory);
            _commandList = _factory.CreateCommandList();

        }

        public Renderable(GraphicsDevice graphicsDevice, Sdl2Window contextWindow){
            _graphicsDevice = graphicsDevice;
            _contextWindow = contextWindow;

            _factory = new DisposeCollectorResourceFactory(_graphicsDevice.ResourceFactory);
            _commandList = _factory.CreateCommandList();
        }

        //TODO: Investigate passing Render options
        /// <summary>
        /// Sets up windowing and keyboard input
        /// Calls Draw() method in rendering loop
        /// Calls Dispose() when done
        /// </summary>
        public void Run(Resolution renderResolution)
        {
            _renderResolution = renderResolution;

            _camera = new Camera(renderResolution.Horizontal,renderResolution.Vertical);
            // Tick every millisecond
            _frameTimer = new FrameTimer(1.0);

            CreateResources();
            foreach(var child in _childrenPre)
                child.CreateResources();
            foreach(var child in _childrenPost)
                child.CreateResources();           

            List<Renderable> allChildren = new List<Renderable>();
            allChildren.AddRange(_childrenPre);
            allChildren.AddRange(_childrenPost);

            buildCommandListTasks = new Task[allChildren.Count+1];
            drawTasksPre = new Task[_childrenPre.Count];
            drawTasksPost = new Task[_childrenPost.Count];

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

                    // Perform draw tasks which should be done before "main" draw e.g. shadow maps
                    for(int i = 0; i < _childrenPre.Count; i++){
                        var child = _childrenPre[i];
                        drawTasksPre[i] = Task.Run(() => child.Draw());
                    } 

                    Task.WaitAll(drawTasksPre);

                    Draw();

                    // Perform draw tasks which should be after after "main" draw e.g. UI updates
                    for(int i = 0; i < _childrenPost.Count; i++){
                        var child = _childrenPost[i];
                        drawTasksPost[i] = Task.Run(() => child.Draw());
                    } 

                    Task.WaitAll(drawTasksPost);
                    PostDraw?.Invoke();

                    if(_renderOptions.LimitFrames)
                        limitFrameRate_Blocking();

                    GraphicsDevice.SwapBuffers();
                }

                _camera.Update(_frameTimer.prevFrameTicksInSeconds);
                _frameTimer.Stop();

            }

            _graphicsDevice.WaitForIdle();
            Dispose();

        }

        private void limitFrameRate_Blocking(){

            double millisecondsPerFrameDiff = _renderOptions.MillisecondsPerFrame - _frameTimer.Query();
            int msDiffRounded = millisecondsPerFrameDiff.ToInt32AwayFromZero();
            if(millisecondsPerFrameDiff > 0)
                Thread.Sleep(msDiffRounded);
        }

        protected void FillRuntimeDescriptor<T>(ModelRuntimeDescriptor<T> modelDescriptor, SceneRuntimeDescriptor sceneRuntimeDescriptor, InstancingData instancingData) where T : struct, VertexRuntime {
                var model = modelDescriptor.Model;

                modelDescriptor.TextureResourceLayout = modelDescriptor.InvokeTextureResourceLayoutGeneration(_factory);
                modelDescriptor.TextureSampler = modelDescriptor.InvokeSamplerGeneration(_factory);
                modelDescriptor.LoadShaders(GraphicsDevice);
                var vertexSizeInBytes = model.meshes[0].vertices[0].GetSizeInBytes();

                for(int i = 0; i < model.meshCount; i++){

                    //TODO: access this through DisposableResourceCollector properly
                    DeviceBuffer vertexBuffer 
                        =  _factory.CreateBuffer(new BufferDescription(model.meshes[i].vertices.LengthUnsigned() * vertexSizeInBytes, BufferUsage.VertexBuffer)); 

                    DeviceBuffer indexBuffer
                        = _factory.CreateBuffer(new BufferDescription(model.meshes[i].meshIndices.LengthUnsigned()*sizeof(ushort),BufferUsage.IndexBuffer));
                        

                    modelDescriptor.VertexBufferList.Add(vertexBuffer);
                    modelDescriptor.IndexBufferList.Add(indexBuffer);

                    //TODO: Make this more generic for more complex instancing behaviour
                    if(instancingData != null){
                        var instancingBuffer = _factory.CreateBuffer(new BufferDescription(instancingData.Positions.Length.ToUnsigned()*12,BufferUsage.VertexBuffer));
                        modelDescriptor.InstanceBufferList.Add(instancingBuffer);
                        _graphicsDevice.UpdateBuffer(instancingBuffer,0,instancingData.Positions);
                    }

                    _graphicsDevice.UpdateBuffer(vertexBuffer,0,model.meshes[i].vertices);
                    _graphicsDevice.UpdateBuffer(indexBuffer,0,model.meshes[i].meshIndices);

                    var resourceSet = modelDescriptor.InvokeTextureResourceSetGeneration(i,_factory,_graphicsDevice);
                    if(resourceSet != null)
                        modelDescriptor.TextureResourceSetsList.Add(resourceSet);
                }

                modelDescriptor.InvokeVertexLayoutGeneration();
                modelDescriptor.InvokeVertexInstanceLayoutGeneration();

                modelDescriptor.FormatResourcesForPipelineGeneration();

                switch(modelDescriptor.VertexType){
                    case VertexTypes.VertexPositionNormal:
                        modelDescriptor.Pipeline = _factory.CreateGraphicsPipeline(ResourceGenerator.GeneratePipelinePN(modelDescriptor,sceneRuntimeDescriptor,_graphicsDevice));
                        break;
                    case VertexTypes.VertexPositionNormalTextureTangentBitangent:
                        modelDescriptor.Pipeline = _factory.CreateGraphicsPipeline(ResourceGenerator.GeneratePipelinePNTTB(modelDescriptor,sceneRuntimeDescriptor,_graphicsDevice));
                        break;
                    case VertexTypes.VertexPositionColor:
                        modelDescriptor.Pipeline = _factory.CreateGraphicsPipeline(ResourceGenerator.GeneratePipelinePC(modelDescriptor,sceneRuntimeDescriptor,_graphicsDevice));
                        break;
                    default:
                        throw new NotImplementedException($"{modelDescriptor.VertexType.ToString("g")} not implemented");
                }
        }

        public void AddThisAsPreTo(Renderable parent){
            parent._childrenPre.Add(this);
            _isChild = true;
        }

        public void AddThisAsPostTo(Renderable parent){
            parent._childrenPost.Add(this);
            _isChild = true;
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
        /// Creates the command list and its containing render commands
        /// </summary>
        abstract protected void BuildCommandList();

        /// <summary>
        /// Convertes vertex list aggregation to arrays for runtime
        /// </summary>
        abstract protected void FormatResourcesForRuntime();

        /// <summary>
        /// Disposes of all elements in _sceneResources
        /// </summary>
         public void Dispose(){

            _factory.DisposeCollector.DisposeAll();
            foreach(var child in _childrenPre)
                child.Dispose();
            foreach(var child in _childrenPost)
                child.Dispose();  
            if(!_isChild)
                _graphicsDevice.Dispose();
        }
    }
}