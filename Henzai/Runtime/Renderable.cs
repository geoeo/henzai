using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using Veldrid;
using Veldrid.StartupUtilities;
using Veldrid.Sdl2;
using Henzai.UI;
using Henzai.Geometry;
using Henzai.Cameras;
using Henzai.Effects;
using Henzai.Core.Materials;
using Henzai.Core.VertexGeometry;
using Henzai.Core.Extensions;
using Henzai.Core.Acceleration;

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
        private Camera _camera { get; set; }
        public Camera Camera => _camera;
        private FrameTimer _frameTimer;
        //TODO: Investigate Switching to Array
        /// <summary>
        /// Renderable objects which should be drawn before this.Draw()
        /// </summary>
        private List<SubRenderable> _childrenPre = new List<SubRenderable>();
        public List<SubRenderable> ChildrenPre => _childrenPre;
        /// <summary>
        /// Renderable objects which should be drawn after this.Draw()
        /// </summary>
        private List<SubRenderable> _childrenPost = new List<SubRenderable>();
        public List<SubRenderable> ChildrenPost => _childrenPost;
        private List<SubRenderable> _allChildren = new List<SubRenderable>();
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
        public event Action<Camera> PreRender_Camera;
        public event Action<GeometryDescriptor<VertexPositionNormalTextureTangentBitangent>, GeometryDescriptor<VertexPositionNormal>, GeometryDescriptor<VertexPositionTexture>, GeometryDescriptor<VertexPositionColor>, GeometryDescriptor<VertexPosition>> PreRender_Descriptors;
        /// <summary>
        /// Bind Actions that have to be executed prior to every draw call
        /// </summary>
        public event Action<float, Camera> PreDraw_Time_Camera;
        public event Action<float, InputSnapshot> PreDraw_Time_Input;
        public event Action<float, Camera, GeometryDescriptor<VertexPositionNormalTextureTangentBitangent>, GeometryDescriptor<VertexPositionNormal>, GeometryDescriptor<VertexPositionTexture>, GeometryDescriptor<VertexPositionColor>, GeometryDescriptor<VertexPosition>> PreDraw_Time_Camera_Descriptors;
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

        protected List<ModelRuntimeDescriptor<VertexPositionNormalTextureTangentBitangent>> _modelPNTTBDescriptorList;
        protected ModelRuntimeDescriptor<VertexPositionNormalTextureTangentBitangent>[] _modelPNTTBDescriptorArray;

        protected List<ModelRuntimeDescriptor<VertexPositionNormal>> _modelPNDescriptorList;
        protected ModelRuntimeDescriptor<VertexPositionNormal>[] _modelPNDescriptorArray;

        protected List<ModelRuntimeDescriptor<VertexPositionTexture>> _modelPTDescriptorList;
        protected ModelRuntimeDescriptor<VertexPositionTexture>[] _modelPTDescriptorArray;

        protected List<ModelRuntimeDescriptor<VertexPositionColor>> _modelPCDescriptorList;
        protected ModelRuntimeDescriptor<VertexPositionColor>[] _modelPCDescriptorArray;

        protected List<ModelRuntimeDescriptor<VertexPosition>> _modelPDescriptorList;
        protected ModelRuntimeDescriptor<VertexPosition>[] _modelPDescriptorArray;

        protected GeometryDescriptor<VertexPositionNormalTextureTangentBitangent> PNTTBRuntimeGeometry;
        protected GeometryDescriptor<VertexPositionNormal> PNRuntimeGeometry;
        protected GeometryDescriptor<VertexPositionTexture> PTRuntimeGeometry;
        protected GeometryDescriptor<VertexPositionColor> PCRuntimeGeometry;
        protected GeometryDescriptor<VertexPosition> PRuntimeGeometry;

        public Renderable(string title, Sdl2Window contextWindow, GraphicsDeviceOptions graphicsDeviceOptions, RenderOptions renderOptions)
        {
            _contextWindow = contextWindow;

            if (renderOptions.UsePreferredGraphicsBackend)
                _graphicsDevice = VeldridStartup.CreateGraphicsDevice(_contextWindow, graphicsDeviceOptions, renderOptions.PreferredGraphicsBackend);
            else
                _graphicsDevice = VeldridStartup.CreateGraphicsDevice(_contextWindow, graphicsDeviceOptions);

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

            PNTTBRuntimeGeometry = new GeometryDescriptor<VertexPositionNormalTextureTangentBitangent>();
            PNRuntimeGeometry = new GeometryDescriptor<VertexPositionNormal>();
            PTRuntimeGeometry = new GeometryDescriptor<VertexPositionTexture>();
            PCRuntimeGeometry = new GeometryDescriptor<VertexPositionColor>();
            PRuntimeGeometry = new GeometryDescriptor<VertexPosition>();

            // Tick every millisecond
            _frameTimer = new FrameTimer(1.0);
        }

        public Renderable(string title, Resolution windowSize, GraphicsDeviceOptions graphicsDeviceOptions, RenderOptions renderOptions)
        {
            WindowCreateInfo windowCI = new WindowCreateInfo()
            {
                X = 100,
                Y = 100,
                WindowWidth = windowSize.Horizontal,
                WindowHeight = windowSize.Vertical,
                WindowTitle = title
            };
            _contextWindow = VeldridStartup.CreateWindow(ref windowCI);

            if (renderOptions.UsePreferredGraphicsBackend)
                _graphicsDevice = VeldridStartup.CreateGraphicsDevice(_contextWindow, graphicsDeviceOptions, renderOptions.PreferredGraphicsBackend);
            else
                _graphicsDevice = VeldridStartup.CreateGraphicsDevice(_contextWindow, graphicsDeviceOptions);

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

            PNTTBRuntimeGeometry = new GeometryDescriptor<VertexPositionNormalTextureTangentBitangent>();
            PNRuntimeGeometry = new GeometryDescriptor<VertexPositionNormal>();
            PTRuntimeGeometry = new GeometryDescriptor<VertexPositionTexture>();
            PCRuntimeGeometry = new GeometryDescriptor<VertexPositionColor>();
            PRuntimeGeometry = new GeometryDescriptor<VertexPosition>();

            // Tick every millisecond
            _frameTimer = new FrameTimer(1.0);
        }

        public Renderable(GraphicsDevice graphicsDevice, Sdl2Window contextWindow)
        {
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

            PNTTBRuntimeGeometry = new GeometryDescriptor<VertexPositionNormalTextureTangentBitangent>();
            PNRuntimeGeometry = new GeometryDescriptor<VertexPositionNormal>();
            PTRuntimeGeometry = new GeometryDescriptor<VertexPositionTexture>();
            PCRuntimeGeometry = new GeometryDescriptor<VertexPositionColor>();
            PRuntimeGeometry = new GeometryDescriptor<VertexPosition>();

            // Tick every millisecond
            _frameTimer = new FrameTimer(1.0);
        }

        //TODO: Investigate passing Render options
        /// <summary>
        /// Sets up windowing and keyboard input
        /// Calls Draw() method in rendering loop
        /// Calls Dispose() when done
        /// </summary>
        public void Run(Resolution renderResolution)
        {
            Debug.Assert(_commandList != null);
            Debug.Assert(_graphicsDevice != null);
            Debug.Assert(_factory != null);
            var inputTracker = new InputTracker();

            _renderResolution = renderResolution;

            if (_renderOptions.FarPlane > 0)
                _camera = new PerspectiveCamera(renderResolution.Horizontal, renderResolution.Vertical, _renderOptions.FarPlane);
            else
                _camera = new PerspectiveCamera(renderResolution.Horizontal, renderResolution.Vertical);

            _allChildren.AddRange(_childrenPre);
            _allChildren.AddRange(_childrenPost);

            CreateUniforms();

            CreateResources();
            foreach (var child in _allChildren)
                child.CreateResources();

            FormatResourcesForRuntime();
            // includes itself in the build command list process
            buildCommandListTasks = new Task[_allChildren.Count + 1];

            drawTasksPre = new Task[_childrenPre.Count];
            drawTasksPost = new Task[_childrenPost.Count];

            PreRender_Camera?.Invoke(_camera);
            PreRender_Descriptors?.Invoke(PNTTBRuntimeGeometry, PNRuntimeGeometry, PTRuntimeGeometry, PCRuntimeGeometry, PRuntimeGeometry);
            while (_contextWindow.Exists)
            {
                _frameTimer.Start();

                InputSnapshot inputSnapshot = _contextWindow.PumpEvents();
                inputTracker.UpdateFrameInput(inputSnapshot);

                var prevFrameTicksInSeconds = _frameTimer.prevFrameTicksInSeconds;
                _camera.Update(_frameTimer.prevFrameTicksInSeconds, inputTracker);

                PreDraw_Time_Camera?.Invoke(prevFrameTicksInSeconds, _camera);
                PreDraw_Time_Input?.Invoke(prevFrameTicksInSeconds, inputSnapshot);
                PreDraw_Time_Camera_Descriptors?.Invoke(
                    prevFrameTicksInSeconds,
                    _camera,
                    PNTTBRuntimeGeometry,
                    PNRuntimeGeometry,
                    PTRuntimeGeometry,
                    PCRuntimeGeometry,
                    PRuntimeGeometry);

                // blocking wait for delegates as they may submit to the command buffer
                _graphicsDevice.WaitForIdle();

                buildCommandListTasks[0] = Task.Run(() => this.BuildCommandList());
                for (int i = 1; i < buildCommandListTasks.Length; i++)
                {
                    var child = _allChildren[i-1];
                    buildCommandListTasks[i] = Task.Run(() => child.BuildCommandList());
                }

                // Wait untill every command list has been built
                Task.WaitAll(buildCommandListTasks);

                // Perform draw tasks which should be done before "main" draw e.g. shadow maps
                for (int i = 0; i < _childrenPre.Count; i++)
                {
                    var child = _childrenPre[i];
                    drawTasksPre[i] = Task.Run(() => child.Draw());
                }

                Task.WaitAll(drawTasksPre);

                Draw();

                PostDraw?.Invoke(prevFrameTicksInSeconds);

                // Perform draw tasks which should be after after "main" draw e.g. UI updates
                for (int i = 0; i < _childrenPost.Count; i++)
                {
                    var child = _childrenPost[i];
                    drawTasksPost[i] = Task.Run(() => child.Draw());
                }

                Task.WaitAll(drawTasksPost);

                if (_renderOptions.LimitFrames)
                    limitFrameRate_Blocking();

                // Wait for submitted UI Tasks
                _graphicsDevice.WaitForIdle();
                _graphicsDevice.SwapBuffers();

                _frameTimer.Stop();

            }
            
            Dispose();

        }

        private void limitFrameRate_Blocking()
        {

            double millisecondsPerFrameDiff = _renderOptions.MillisecondsPerFrame - _frameTimer.Query();
            int msDiffRounded = millisecondsPerFrameDiff.ToInt32AwayFromZero();
            if (millisecondsPerFrameDiff > 0)
                Thread.Sleep(msDiffRounded);
        }

        /// <summary>
        /// Sets delegates for runtime command generation.
        /// Also updates index and vertex buffers, sets pipeline
        /// </summary>
        /// <param name="modelDescriptor">Model descriptor.</param>
        /// <param name="sceneRuntimeDescriptor">Scene runtime descriptor.</param>
        /// <param name="instancingData">Instancing data.</param>
        /// <typeparam name="T">The type of Vertex sent to the GPU</typeparam>
        protected void FillRuntimeDescriptor<T>(ModelRuntimeDescriptor<T> modelDescriptor, SceneRuntimeDescriptor sceneRuntimeDescriptor, InstancingData instancingData) where T : struct, VertexRuntime, VertexLocateable
        {
            var model = modelDescriptor.Model;

            modelDescriptor.TextureResourceLayout = modelDescriptor.InvokeTextureResourceLayoutGeneration(_factory);
            modelDescriptor.TextureSampler = modelDescriptor.InvokeSamplerGeneration(_factory);
            modelDescriptor.LoadShaders(_graphicsDevice);
            byte vertexSizeInBytes = model.GetMesh(0).Vertices[0].GetSizeInBytes();

            var meshCount = model.MeshCount;
            for (int i = 0; i < meshCount; i++)
            {
                var mesh = model.GetMesh(i);
                var meshBVH = model.GetMeshBVH(i);
                DeviceBuffer vertexBuffer
                    = _factory.CreateBuffer(new BufferDescription(mesh.Vertices.LengthUnsigned() * vertexSizeInBytes, BufferUsage.VertexBuffer));

                DeviceBuffer indexBuffer
                    = _factory.CreateBuffer(new BufferDescription(mesh.Indices.LengthUnsigned() * sizeof(ushort), BufferUsage.IndexBuffer));


                modelDescriptor.VertexBufferList.Add(vertexBuffer);
                modelDescriptor.IndexBufferList.Add(indexBuffer);

                //TODO: Make this more generic for more complex instancing behaviour
                if (instancingData != null)
                {
                    var instancingBuffer = _factory.CreateBuffer(new BufferDescription(instancingData.Positions.Length.ToUnsigned() * 12, BufferUsage.VertexBuffer));
                    modelDescriptor.InstanceBufferList.Add(instancingBuffer);
                    _graphicsDevice.UpdateBuffer(instancingBuffer, 0, instancingData.Positions);
                }

                _graphicsDevice.UpdateBuffer<T>(vertexBuffer, 0, ref mesh.Vertices[0], (vertexSizeInBytes * mesh.VertexCount).ToUnsigned());
                _graphicsDevice.UpdateBuffer<ushort>(indexBuffer, 0, ref mesh.Indices[0], (sizeof(ushort) * mesh.IndexCount).ToUnsigned());

                var resourceSet = modelDescriptor.InvokeTextureResourceSetGeneration(i, _factory, _graphicsDevice);
                if (resourceSet != null)
                    modelDescriptor.TextureResourceSetsList.Add(resourceSet);                
            }

            modelDescriptor.InvokeVertexLayoutGeneration();
            modelDescriptor.InvokeVertexInstanceLayoutGeneration();

            modelDescriptor.FormatResourcesForPipelineGeneration();

            switch (modelDescriptor.VertexRuntimeType)
            {
                case VertexRuntimeTypes.VertexPosition:
                    modelDescriptor.Pipeline = _factory.CreateGraphicsPipeline(ResourceGenerator.GeneratePipelineP(modelDescriptor, sceneRuntimeDescriptor, _graphicsDevice));
                    break;
                case VertexRuntimeTypes.VertexPositionNormal:
                    modelDescriptor.Pipeline = _factory.CreateGraphicsPipeline(ResourceGenerator.GeneratePipelinePN(modelDescriptor, sceneRuntimeDescriptor, _graphicsDevice));
                    break;
                case VertexRuntimeTypes.VertexPositionNormalTextureTangentBitangent:
                    modelDescriptor.Pipeline = _factory.CreateGraphicsPipeline(ResourceGenerator.GeneratePipelinePNTTB(modelDescriptor, sceneRuntimeDescriptor, _graphicsDevice));
                    break;
                case VertexRuntimeTypes.VertexPositionColor:
                    modelDescriptor.Pipeline = _factory.CreateGraphicsPipeline(ResourceGenerator.GeneratePipelinePC(modelDescriptor, sceneRuntimeDescriptor, _graphicsDevice));
                    break;
                case VertexRuntimeTypes.VertexPositionTexture:
                    modelDescriptor.Pipeline = _factory.CreateGraphicsPipeline(ResourceGenerator.GeneratePipelinePT(modelDescriptor, sceneRuntimeDescriptor, _graphicsDevice));
                    break;
                default:
                    throw new NotImplementedException($"{modelDescriptor.VertexRuntimeType.ToString("g")} not implemented");
            }
        }

        private void CreateUniforms()
        {
            // Uniform 1 - Camera
            _sceneRuntimeState.CameraProjViewBuffer = _factory.CreateBuffer(new BufferDescription(Camera.SizeInBytes, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
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
                    new BindableResource[] { _sceneRuntimeState.CameraProjViewBuffer });

            // Uniform 2 - Material
            _sceneRuntimeState.MaterialBuffer = _factory.CreateBuffer(new BufferDescription(RealtimeMaterial.SizeInBytes, BufferUsage.UniformBuffer));
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
                    new BindableResource[] { _sceneRuntimeState.MaterialBuffer });

            // Uniform 3 - Light
            _sceneRuntimeState.LightBuffer = _factory.CreateBuffer(new BufferDescription(Light.SizeInBytes, BufferUsage.UniformBuffer));
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
                    new BindableResource[] { _sceneRuntimeState.LightBuffer });

            // Uniform 4 - PointLight
            _sceneRuntimeState.SpotLightBuffer = _factory.CreateBuffer(new BufferDescription(4 * 4 * 4, BufferUsage.UniformBuffer));
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
                    new BindableResource[] { _sceneRuntimeState.SpotLightBuffer });
        }

        //TODO: Maybe replace this by non virtual as it seems to always be the same
        /// <summary>
        /// Executes the defined command list(s)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void Draw(){
            _graphicsDevice.SubmitCommands(_commandList);
        }

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
        private void FormatResourcesForRuntime()
        {

            //TODO: Think of a way to incorporate instancing data better
            // foreach(var modelDescriptor in _modelPNTTBDescriptorList)
            //     FillRuntimeDescriptor(modelDescriptor,_sceneRuntimeState,InstancingData.NO_DATA); 

            // foreach(var modelDescriptor in _modelPNDescriptorList)
            //     FillRuntimeDescriptor(modelDescriptor,_sceneRuntimeState,InstancingData.NO_DATA); 

            // foreach(var modelDescriptor in _modelPTDescriptorList)
            //     FillRuntimeDescriptor(modelDescriptor,_sceneRuntimeState,InstancingData.NO_DATA); 

            // foreach(var modelDescriptor in _modelPCDescriptorList)
            //     FillRuntimeDescriptor(modelDescriptor,_sceneRuntimeState,InstancingData.NO_DATA); 



            _modelPNTTBDescriptorArray = _modelPNTTBDescriptorList.ToArray();
            _modelPNDescriptorArray = _modelPNDescriptorList.ToArray();
            _modelPTDescriptorArray = _modelPTDescriptorList.ToArray();
            _modelPCDescriptorArray = _modelPCDescriptorList.ToArray();
            _modelPDescriptorArray = _modelPDescriptorList.ToArray();

            PNTTBRuntimeGeometry.FormatForRuntime(_modelPNTTBDescriptorArray);
            PNRuntimeGeometry.FormatForRuntime(_modelPNDescriptorArray);
            PTRuntimeGeometry.FormatForRuntime(_modelPTDescriptorArray);
            PCRuntimeGeometry.FormatForRuntime(_modelPCDescriptorArray);
            PRuntimeGeometry.FormatForRuntime(_modelPDescriptorArray);

            foreach (var modelState in _modelPNDescriptorList)
                modelState.FormatResourcesForRuntime();
            foreach (var modelState in _modelPTDescriptorList)
                modelState.FormatResourcesForRuntime();
            foreach (var modelState in _modelPCDescriptorList)
                modelState.FormatResourcesForRuntime();
            foreach (var modelState in _modelPDescriptorList)
                modelState.FormatResourcesForRuntime();
        }

        //TODO: Refactor these methods into a class
        public void SetUI(UserInterface ui){
            PreDraw_Time_Input += ui.UpdateImGui;
            _childrenPost.Add(ui);
        }

        public void CreateShadowMap(Resolution resolution){
            //TODO: Needs different shaders!
            var shadowMap = new ShadowMap(_graphicsDevice, resolution, _modelPNTTBDescriptorArray, _modelPNDescriptorArray, _modelPTDescriptorArray, _modelPCDescriptorArray);
            _childrenPre.Add(shadowMap);
        }

        /// <summary>
        /// Disposes of all elements in _sceneResources
        /// </summary>
        public virtual void Dispose()
        {
            _frameTimer.Cancel();
            _graphicsDevice.WaitForIdle();
            foreach (var child in _allChildren)
                child.Dispose();
            _allChildren.Clear();
            _frameTimer.Dispose();
            _factory.DisposeCollector.DisposeAll();
            _graphicsDevice.WaitForIdle();
            _graphicsDevice.Dispose();
            _contextWindow.Close();
        }
    }
}