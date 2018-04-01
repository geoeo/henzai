using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Veldrid;
using Veldrid.StartupUtilities;
using Veldrid.Sdl2;
using Henzai.UserInterface;
using Henzai.Extensions;
using Henzai.Runtime;

namespace Henzai
{
    //TODO: Investigate Making a Completely Separate Thread for UI
    /// <summary>
    /// A boilerplate for renderable scenes.
    /// Every disposable resource should be returned by its respective method
    /// </summary>
    public abstract class Renderable : IDisposable
    {
        private Camera _camera {get; set;} 
        public Camera Camera => _camera;
        private FrameTimer _frameTimer;
        //TODO: Investigate Switching to Array
        /// <summary>
        /// Renderable objects which should be drawn before this.Draw()
        /// </summary>
        public List<Renderable> childrenPre = new List<Renderable>();
        /// <summary>
        /// Renderable objects which should be drawn after this.Draw()
        /// </summary>
        public List<Renderable> childrenPost = new List<Renderable>();
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
            foreach(var child in childrenPre)
                child.CreateResources();
            foreach(var child in childrenPost)
                child.CreateResources();           

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

                    // Perform draw tasks which should be done before "main" draw e.g. shadow maps
                    for(int i = 0; i < childrenPre.Count; i++){
                        var child = childrenPre[i];
                        drawTasksPre[i] = Task.Run(() => child.Draw());
                    } 

                    Task.WaitAll(drawTasksPre);

                    Draw();

                    // Perform draw tasks which should be after after "main" draw e.g. UI updates
                    for(int i = 0; i < childrenPost.Count; i++){
                        var child = childrenPost[i];
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

        protected void FillRuntimeDescriptor<T>(ModelRuntimeDescriptor<T> modelDescriptor, SceneRuntimeDescriptor sceneRuntimeDescriptor) where T : struct, VertexRuntime{
                var model = modelDescriptor.Model;

                modelDescriptor.TextureResourceLayout = modelDescriptor.InvokeTextureResourceLayoutGeneration(_factory);
                modelDescriptor.TextureSampler = modelDescriptor.InvokeSamplerGeneration(_factory);
                modelDescriptor.LoadShaders(GraphicsDevice);
                var vertexSizeInBytes = model.meshes[0].vertices[0].GetSizeInBytes();

                for(int i = 0; i < model.meshCount; i++){

                    DeviceBuffer vertexBuffer 
                        =  _factory.CreateBuffer(new BufferDescription(model.meshes[i].vertices.LengthUnsigned() * vertexSizeInBytes, BufferUsage.VertexBuffer)); 

                    DeviceBuffer indexBuffer
                        = _factory.CreateBuffer(new BufferDescription(model.meshes[i].meshIndices.LengthUnsigned()*sizeof(uint),BufferUsage.IndexBuffer));
                        

                    modelDescriptor.VertexBuffersList.Add(vertexBuffer);
                    modelDescriptor.IndexBuffersList.Add(indexBuffer);

                    GraphicsDevice.UpdateBuffer(vertexBuffer,0,model.meshes[i].vertices);
                    GraphicsDevice.UpdateBuffer(indexBuffer,0,model.meshes[i].meshIndices);

                    modelDescriptor.TextureResourceSetsList.Add(
                        modelDescriptor.InvokeTextureResourceSetGeneration(i,_factory,GraphicsDevice)
                        );
                }
                modelDescriptor.VertexLayout = modelDescriptor.InvokeVertexLayoutGeneration();

                switch(modelDescriptor.VertexType){
                    case VertexTypes.VertexPositionNormal:
                        modelDescriptor.Pipeline = _factory.CreateGraphicsPipeline(ResourceGenerator.GeneratePipelinePN(modelDescriptor,sceneRuntimeDescriptor,GraphicsDevice));
                        break;
                    case VertexTypes.VertexPositionNormalTextureTangentBitangent:
                        modelDescriptor.Pipeline = _factory.CreateGraphicsPipeline(ResourceGenerator.GeneratePipelinePNTTB(modelDescriptor,sceneRuntimeDescriptor,GraphicsDevice));
                        break;
                    default:
                        throw new NotImplementedException($"{modelDescriptor.VertexType.ToString("g")} not implemented");
                }
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
        virtual public void Dispose(){

            _factory.DisposeCollector.DisposeAll();
            foreach(var child in childrenPre)
                child.Dispose();
            foreach(var child in childrenPost)
                child.Dispose();  

            _graphicsDevice.Dispose();
        }
    }
}