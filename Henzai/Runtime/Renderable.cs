using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Veldrid;
using Veldrid.StartupUtilities;
using Veldrid.Sdl2;
using Henzai.UI;
using Henzai.Geometry;
using Henzai.Core.Geometry;
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
        protected bool _isChild = false;
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
        private List<Renderable> _allChildren = new List<Renderable>();
        private Sdl2Window _contextWindow;
        public Sdl2Window ContextWindow => _contextWindow;
        private GraphicsDevice _graphicsDevice;
        public GraphicsDevice GraphicsDevice => _graphicsDevice;
        private RenderOptions _renderOptions;
        protected DisposeCollectorResourceFactory _factory;
        protected SceneRuntimeDescriptor _sceneRuntimeState;
        protected CommandList _commandList;
        private Resolution _renderResolution;
        public Resolution RenderResoultion => _renderResolution;
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
        public event Action<float,InputSnapshot> PreDraw_Time_Input;
        public event Action<float, CommandList, Camera, ModelRuntimeDescriptor<VertexPositionNormalTextureTangentBitangent>[],ModelRuntimeDescriptor<VertexPositionNormal>[],ModelRuntimeDescriptor<VertexPositionTexture>[],ModelRuntimeDescriptor<VertexPositionColor>[],ModelRuntimeDescriptor<VertexPosition>[]> PreDraw_Time_GraphicsDevice_Camera_Models;
        /// <summary>
        /// Bind Actions that have to be executed after every draw call
        /// </summary>
        public event Action<float> PostDraw;
        /// <summary>
        /// Includes this.BuildCommandList()
        /// </summary>
        private Task[] buildCommandListTasks;
        private Task[] drawTasksPre;
        private Task[] drawTasksPost;
        public Renderable UI {set; private get;}
        private CancellationTokenSource _uiCancellationTokenSource;

        protected List<ModelRuntimeDescriptor<VertexPositionNormalTextureTangentBitangent>> _modelPNTTBDescriptorList;
        protected ModelRuntimeDescriptor<VertexPositionNormalTextureTangentBitangent> [] _modelPNTTBDescriptorArray;

        protected List<ModelRuntimeDescriptor<VertexPositionNormal>> _modelPNDescriptorList;
        protected ModelRuntimeDescriptor<VertexPositionNormal> [] _modelPNDescriptorArray;

        protected List<ModelRuntimeDescriptor<VertexPositionTexture>> _modelPTDescriptorList;
        protected ModelRuntimeDescriptor<VertexPositionTexture> [] _modelPTDescriptorArray;

        protected List<ModelRuntimeDescriptor<VertexPositionColor>> _modelPCDescriptorList;
        protected ModelRuntimeDescriptor<VertexPositionColor> [] _modelPCDescriptorArray;

        protected List<ModelRuntimeDescriptor<VertexPosition>> _modelPDescriptorList;
        protected ModelRuntimeDescriptor<VertexPosition> [] _modelPDescriptorArray;

        public Renderable(string title, Sdl2Window contextWindow, GraphicsDeviceOptions graphicsDeviceOptions, RenderOptions renderOptions){
            _contextWindow = contextWindow;

            if(renderOptions.UsePreferredGraphicsBackend)
                _graphicsDevice = VeldridStartup.CreateGraphicsDevice(_contextWindow,graphicsDeviceOptions,renderOptions.PreferredGraphicsBackend);
            else
                _graphicsDevice = VeldridStartup.CreateGraphicsDevice(_contextWindow,graphicsDeviceOptions);

            _contextWindow.Title = $"{title} / {_graphicsDevice.BackendType.ToString()}";
            _renderOptions = renderOptions;
            _factory = new DisposeCollectorResourceFactory(_graphicsDevice.ResourceFactory);
            _commandList = _factory.CreateCommandList();

            _sceneRuntimeState = new SceneRuntimeDescriptor();

            _modelPNTTBDescriptorList = new List<ModelRuntimeDescriptor<VertexPositionNormalTextureTangentBitangent>>();
            _modelPNDescriptorList = new List<ModelRuntimeDescriptor<VertexPositionNormal>>();
            _modelPTDescriptorList = new List<ModelRuntimeDescriptor<VertexPositionTexture>>();
            _modelPCDescriptorList = new List<ModelRuntimeDescriptor<VertexPositionColor>>();
            _modelPDescriptorList = new List<ModelRuntimeDescriptor<VertexPosition>>();

            // Tick every millisecond
            _frameTimer = new FrameTimer(1.0); 

            if(!_isChild)
                _uiCancellationTokenSource = new CancellationTokenSource();

        }

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

            _sceneRuntimeState = new SceneRuntimeDescriptor();

            _modelPNTTBDescriptorList = new List<ModelRuntimeDescriptor<VertexPositionNormalTextureTangentBitangent>>();
            _modelPNDescriptorList = new List<ModelRuntimeDescriptor<VertexPositionNormal>>();
            _modelPTDescriptorList = new List<ModelRuntimeDescriptor<VertexPositionTexture>>();
            _modelPCDescriptorList = new List<ModelRuntimeDescriptor<VertexPositionColor>>();
            _modelPDescriptorList = new List<ModelRuntimeDescriptor<VertexPosition>>();

            // Tick every millisecond
            _frameTimer = new FrameTimer(1.0); 

            if(!_isChild)
                _uiCancellationTokenSource = new CancellationTokenSource();

        }

        public Renderable(GraphicsDevice graphicsDevice, Sdl2Window contextWindow){
            _graphicsDevice = graphicsDevice;
            _contextWindow = contextWindow;

            _factory = new DisposeCollectorResourceFactory(_graphicsDevice.ResourceFactory);
            _commandList = _factory.CreateCommandList();

            _sceneRuntimeState = new SceneRuntimeDescriptor();

            _modelPNTTBDescriptorList = new List<ModelRuntimeDescriptor<VertexPositionNormalTextureTangentBitangent>>();
            _modelPNDescriptorList = new List<ModelRuntimeDescriptor<VertexPositionNormal>>();
            _modelPTDescriptorList = new List<ModelRuntimeDescriptor<VertexPositionTexture>>();
            _modelPCDescriptorList = new List<ModelRuntimeDescriptor<VertexPositionColor>>();
            _modelPDescriptorList = new List<ModelRuntimeDescriptor<VertexPosition>>();

            // Tick every millisecond
            _frameTimer = new FrameTimer(1.0); 

            if(!_isChild)
                _uiCancellationTokenSource = new CancellationTokenSource();



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

            if(_renderOptions.FarPlane > 0)
            _camera = new Camera(renderResolution.Horizontal,renderResolution.Vertical,_renderOptions.FarPlane);
            else
                _camera = new Camera(renderResolution.Horizontal,renderResolution.Vertical);     

            _allChildren.AddRange(_childrenPre);
            _allChildren.AddRange(_childrenPost);
            if(UI!=null)
                _allChildren.Add(UI);

            CreateUniforms();

            CreateResources();
            foreach(var child in _allChildren)
                child.CreateResources();

            FormatResourcesForRuntime();
            // includes itself in the build command list process
            buildCommandListTasks = new Task[_allChildren.Count+1];

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
                    PreDraw_Time_Input?.Invoke(_frameTimer.prevFrameTicksInSeconds,inputSnapshot);
                    PreDraw_Time_GraphicsDevice_Camera_Models?.Invoke(
                        _frameTimer.prevFrameTicksInSeconds,
                        _commandList,
                        _camera,
                        _modelPNTTBDescriptorArray,_modelPNDescriptorArray,
                        _modelPTDescriptorArray,
                        _modelPCDescriptorArray,
                        _modelPDescriptorArray);

                    buildCommandListTasks[0] = Task.Run(() => this.BuildCommandList());
                    for(int i = 0; i < _allChildren.Count; i++){
                        var child = _allChildren[i];
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

                    PostDraw?.Invoke(_frameTimer.prevFrameTicksInSeconds);

                    // Perform draw tasks which should be after after "main" draw e.g. UI updates
                    for(int i = 0; i < _childrenPost.Count; i++){
                        var child = _childrenPost[i];
                        drawTasksPost[i] = Task.Run(() => child.Draw());
                    } 

                    Task.WaitAll(drawTasksPost);

                    // TODO investigate a special class for "main" render object
                    if(UI != null && !_uiCancellationTokenSource.IsCancellationRequested)
                        DrawUI();

                    if(_renderOptions.LimitFrames)
                        limitFrameRate_Blocking();

                    // Wait for submitted UI Tasks
                    _graphicsDevice.WaitForIdle();
                    _graphicsDevice.SwapBuffers();
                }

                _camera.Update(_frameTimer.prevFrameTicksInSeconds);
                _frameTimer.Stop();

            }

            Dispose();

        }

        private void limitFrameRate_Blocking(){

            double millisecondsPerFrameDiff = _renderOptions.MillisecondsPerFrame - _frameTimer.Query();
            int msDiffRounded = millisecondsPerFrameDiff.ToInt32AwayFromZero();
            if(millisecondsPerFrameDiff > 0)
                Thread.Sleep(msDiffRounded);
        }

        protected void FillRuntimeDescriptor<T>(ModelRuntimeDescriptor<T> modelDescriptor, SceneRuntimeDescriptor sceneRuntimeDescriptor, InstancingData instancingData) where T : struct, VertexRuntime, VertexLocateable {
                var model = modelDescriptor.Model;

                modelDescriptor.TextureResourceLayout = modelDescriptor.InvokeTextureResourceLayoutGeneration(_factory);
                modelDescriptor.TextureSampler = modelDescriptor.InvokeSamplerGeneration(_factory);
                modelDescriptor.LoadShaders(_graphicsDevice);
                byte vertexSizeInBytes = model.meshes[0].Vertices[0].GetSizeInBytes();

                for(int i = 0; i < model.meshCount; i++){

                    //TODO: access this through DisposableResourceCollector properly
                    DeviceBuffer vertexBuffer 
                        =  _factory.CreateBuffer(new BufferDescription(model.meshes[i].Vertices.LengthUnsigned() * vertexSizeInBytes, BufferUsage.VertexBuffer)); 

                    DeviceBuffer indexBuffer
                        = _factory.CreateBuffer(new BufferDescription(model.meshes[i].MeshIndices.LengthUnsigned()*sizeof(ushort),BufferUsage.IndexBuffer));
                        

                    modelDescriptor.VertexBufferList.Add(vertexBuffer);
                    modelDescriptor.IndexBufferList.Add(indexBuffer);

                    //TODO: Make this more generic for more complex instancing behaviour
                    if(instancingData != null){
                        var instancingBuffer = _factory.CreateBuffer(new BufferDescription(instancingData.Positions.Length.ToUnsigned()*12,BufferUsage.VertexBuffer));
                        modelDescriptor.InstanceBufferList.Add(instancingBuffer);
                        _graphicsDevice.UpdateBuffer(instancingBuffer,0,instancingData.Positions);
                    }

                    Geometry.Mesh<T> mesh = model.meshes[i];
                    _graphicsDevice.UpdateBuffer<T>(vertexBuffer,0,ref mesh.Vertices[0], (vertexSizeInBytes* mesh.NumberOfValidVertices).ToUnsigned());
                    _graphicsDevice.UpdateBuffer<ushort>(indexBuffer,0,ref mesh.MeshIndices[0], (sizeof(ushort)*mesh.NumberOfValidIndices).ToUnsigned());

                    var resourceSet = modelDescriptor.InvokeTextureResourceSetGeneration(i,_factory,_graphicsDevice);
                    if(resourceSet != null)
                        modelDescriptor.TextureResourceSetsList.Add(resourceSet);
                }

                modelDescriptor.InvokeVertexLayoutGeneration();
                modelDescriptor.InvokeVertexInstanceLayoutGeneration();

                modelDescriptor.FormatResourcesForPipelineGeneration();

                switch(modelDescriptor.VertexType){
                    case VertexTypes.VertexPosition:
                        modelDescriptor.Pipeline = _factory.CreateGraphicsPipeline(ResourceGenerator.GeneratePipelineP(modelDescriptor,sceneRuntimeDescriptor,_graphicsDevice));
                        break;
                    case VertexTypes.VertexPositionNormal:
                        modelDescriptor.Pipeline = _factory.CreateGraphicsPipeline(ResourceGenerator.GeneratePipelinePN(modelDescriptor,sceneRuntimeDescriptor,_graphicsDevice));
                        break;
                    case VertexTypes.VertexPositionNormalTextureTangentBitangent:
                        modelDescriptor.Pipeline = _factory.CreateGraphicsPipeline(ResourceGenerator.GeneratePipelinePNTTB(modelDescriptor,sceneRuntimeDescriptor,_graphicsDevice));
                        break;
                    case VertexTypes.VertexPositionColor:
                        modelDescriptor.Pipeline = _factory.CreateGraphicsPipeline(ResourceGenerator.GeneratePipelinePC(modelDescriptor,sceneRuntimeDescriptor,_graphicsDevice));
                        break;
                    case VertexTypes.VertexPositionTexture:
                        modelDescriptor.Pipeline = _factory.CreateGraphicsPipeline(ResourceGenerator.GeneratePipelinePT(modelDescriptor,sceneRuntimeDescriptor,_graphicsDevice));
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

        // TODO: Profile this in VS against being a Post Draw Task (without frame cap)
        public async void DrawUI(){
            await Task.Run(() => UI.Draw(),_uiCancellationTokenSource.Token).ConfigureAwait(false);
        }

        private void CreateUniforms(){
            // Uniform 1 - Camera
            _sceneRuntimeState.CameraProjViewBuffer  = _factory.CreateBuffer(new BufferDescription(Camera.SizeInBytes,BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            _sceneRuntimeState.CameraResourceLayout 
                = ResourceGenerator.GenerateResourceLayout(
                    _factory,
                    "projViewWorld",
                    ResourceKind.UniformBuffer,
                    ShaderStages.Vertex);
            _sceneRuntimeState.CameraResourceSet 
                = ResourceGenerator.GenrateResourceSet(
                    _factory,
                    _sceneRuntimeState.CameraResourceLayout,
                    new BindableResource[]{_sceneRuntimeState.CameraProjViewBuffer});

            // Uniform 2 - Material
            _sceneRuntimeState.MaterialBuffer = _factory.CreateBuffer(new BufferDescription(Material.SizeInBytes,BufferUsage.UniformBuffer));
            _sceneRuntimeState.MaterialResourceLayout 
                = ResourceGenerator.GenerateResourceLayout(
                    _factory,
                    "material",
                    ResourceKind.UniformBuffer,
                    ShaderStages.Fragment);
            _sceneRuntimeState.MaterialResourceSet 
                = ResourceGenerator.GenrateResourceSet(
                    _factory,
                    _sceneRuntimeState.MaterialResourceLayout,
                    new BindableResource[]{_sceneRuntimeState.MaterialBuffer});

            // Uniform 3 - Light
            _sceneRuntimeState.LightBuffer = _factory.CreateBuffer(new BufferDescription(Light.SizeInBytes,BufferUsage.UniformBuffer));
            _sceneRuntimeState.LightResourceLayout 
                = ResourceGenerator.GenerateResourceLayout(
                    _factory,
                    "light",
                    ResourceKind.UniformBuffer,
                    ShaderStages.Vertex | ShaderStages.Fragment);
            _sceneRuntimeState.LightResourceSet 
                = ResourceGenerator.GenrateResourceSet(
                    _factory,
                    _sceneRuntimeState.LightResourceLayout,
                    new BindableResource[]{_sceneRuntimeState.LightBuffer});

                // Uniform 4 - PointLight
            _sceneRuntimeState.SpotLightBuffer = _factory.CreateBuffer(new BufferDescription(4*4*4,BufferUsage.UniformBuffer));
            _sceneRuntimeState.SpotLightResourceLayout 
                = ResourceGenerator.GenerateResourceLayout(
                    _factory,
                    "spotlight",
                    ResourceKind.UniformBuffer,
                    ShaderStages.Fragment);
            _sceneRuntimeState.SpotLightResourceSet 
                = ResourceGenerator.GenrateResourceSet(
                    _factory,
                    _sceneRuntimeState.SpotLightResourceLayout,
                    new BindableResource[]{_sceneRuntimeState.SpotLightBuffer});
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
        private void FormatResourcesForRuntime(){

            //TODO: Think of a way to incorporate instancing data better
            // foreach(var modelDescriptor in _modelPNTTBDescriptorList)
            //     FillRuntimeDescriptor(modelDescriptor,_sceneRuntimeState,InstancingData.NO_DATA); 
            
            // foreach(var modelDescriptor in _modelPNDescriptorList)
            //     FillRuntimeDescriptor(modelDescriptor,_sceneRuntimeState,InstancingData.NO_DATA); 
            
            // foreach(var modelDescriptor in _modelPTDescriptorList)
            //     FillRuntimeDescriptor(modelDescriptor,_sceneRuntimeState,InstancingData.NO_DATA); 
            
            // foreach(var modelDescriptor in _modelPCDescriptorList)
            //     FillRuntimeDescriptor(modelDescriptor,_sceneRuntimeState,InstancingData.NO_DATA); 
            

            foreach(var modelState in _modelPNTTBDescriptorList)
                modelState.FormatResourcesForRuntime();
            foreach(var modelState in _modelPNDescriptorList)
                modelState.FormatResourcesForRuntime();
            foreach(var modelState in _modelPTDescriptorList)
                modelState.FormatResourcesForRuntime();
            foreach(var modelState in _modelPCDescriptorList)
                modelState.FormatResourcesForRuntime();
            foreach(var modelState in _modelPDescriptorList)
                modelState.FormatResourcesForRuntime();


            _modelPNTTBDescriptorArray = _modelPNTTBDescriptorList.ToArray();
            _modelPNDescriptorArray = _modelPNDescriptorList.ToArray();
            _modelPTDescriptorArray = _modelPTDescriptorList.ToArray();
            _modelPCDescriptorArray = _modelPCDescriptorList.ToArray();
            _modelPDescriptorArray = _modelPDescriptorList.ToArray();
        }

        //TODO: Unused
        /// <summary>
        /// Disposes of all elements in _sceneResources except the contexst window
        /// </summary>
        public void DisposeKeepContextWindow(){

            _graphicsDevice.WaitForIdle();
            foreach(var child in _allChildren)
                child.DisposeKeepContextWindow();
            _allChildren.Clear();
            _factory.DisposeCollector.DisposeAll();
            if(!_isChild){
                _graphicsDevice.WaitForIdle();
                _graphicsDevice.Dispose();
            }

        }

        /// <summary>
        /// Disposes of all elements in _sceneResources
        /// </summary>
        public virtual void Dispose(){

            _frameTimer.Cancel();
            if(!_isChild)
                _uiCancellationTokenSource.Cancel();
            _graphicsDevice.WaitForIdle();
            foreach(var child in _allChildren)
                child.Dispose();
            _allChildren.Clear();
            _frameTimer.Dispose();
            _factory.DisposeCollector.DisposeAll();
            if(!_isChild){
                _uiCancellationTokenSource.Dispose();
                _graphicsDevice.WaitForIdle();
                _graphicsDevice.Dispose();
                _contextWindow.Close();
            }

        }
    }
}