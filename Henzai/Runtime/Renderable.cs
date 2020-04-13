using System;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.CompilerServices;

using Veldrid;
using Veldrid.StartupUtilities;
using Veldrid.Sdl2;
using Henzai.UI;
using Henzai.Cameras;
using Henzai.Effects;
using Henzai.Core.Materials;
using Henzai.Core.VertexGeometry;
using Henzai.Core.Extensions;


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
        //TODO: Investigate Switching to Array / Having fixed places for pre and post effects within the array
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
        public SceneRuntimeDescriptor SceneRuntimeDescriptor => _sceneRuntimeState;
        protected CommandList _commandList;
        protected Resolution _renderResolution;
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

        private bool _running;

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
            _running = false;
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
            _running = false;
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
            _running = false;
        }

        public void SetUp(Resolution renderResolution) {

            Debug.Assert(_commandList != null);
            Debug.Assert(_graphicsDevice != null);
            Debug.Assert(_factory != null);

            _renderResolution = renderResolution;

            if (_renderOptions.FarPlane > 0)
                _camera = new PerspectiveCamera(renderResolution.Horizontal, renderResolution.Vertical, Camera.DEFAULT_POSITION, Camera.DEFAULT_LOOK_DIRECTION, _renderOptions.FarPlane);
            else
                _camera = new PerspectiveCamera(renderResolution.Horizontal, renderResolution.Vertical, Camera.DEFAULT_POSITION, Camera.DEFAULT_LOOK_DIRECTION);

            _allChildren.AddRange(_childrenPre);
            _allChildren.AddRange(_childrenPost);

            // var shadowMapEnabled = _childrenPre.Count > 0;
            // var effectCount = 2*Convert.ToInt32(shadowMapEnabled); // 1 for Vertex Stage 1 for Fragment

            CreateUniforms();
            CreateResources();
            FormatResourcesForRuntime();

            foreach (var child in _allChildren)
                child.CreateResources(_sceneRuntimeState,_modelPNTTBDescriptorArray,_modelPNDescriptorArray, _modelPTDescriptorArray, _modelPCDescriptorArray);

            // includes itself in the build command list process
            buildCommandListTasks = new Task[_allChildren.Count + 1];

            drawTasksPre = new Task[_childrenPre.Count];
            drawTasksPost = new Task[_childrenPost.Count];

            PreRender_Camera?.Invoke(_camera);
            PreRender_Descriptors?.Invoke(PNTTBRuntimeGeometry, PNRuntimeGeometry, PTRuntimeGeometry, PCRuntimeGeometry, PRuntimeGeometry);
        }

        // This does get called before OpenGL crash when I close the window
        //TODO: maybe improve window closing
        public void Stop(){
            _frameTimer.Stop();
            _running = false;
        }

        //TODO: Investigate passing Render options
        /// <summary>
        /// Sets up windowing and keyboard input
        /// Calls Draw() method in rendering loop
        /// Calls Dispose() when done
        /// </summary>
        public void Run()
        {
            var inputTracker = new InputTracker();
            _contextWindow.Closed += Stop;
            _running = true;
            while (_running)
            {
                _frameTimer.Start();

                InputSnapshot inputSnapshot = _contextWindow.PumpEvents();
                // Somehow we have to check again after PumpEvents()
                if(!_running)
                    break;
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

                //TODO: split this up into pre and post too!
                buildCommandListTasks[buildCommandListTasks.Length-1] = Task.Run(() => this.BuildCommandList());
                for (int i = 0; i < buildCommandListTasks.Length-1; i++)
                {
                    var child = _allChildren[i];
                    buildCommandListTasks[i] = Task.Run(() => child.BuildCommandList());
                }

                // Wait untill every command list has been built
                Task.WaitAll(buildCommandListTasks);


                // Perform draw tasks which should be done before "main" draw e.g. shadow maps
                for (int i = 0; i < _childrenPre.Count; i++)
                {
                    var child = _childrenPre[i];
                    drawTasksPre[i] = Task.Run(() => child.Draw());
                    //TODO: Maybe WaitForIdle
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
            
            _contextWindow.Closed -= Stop;
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
        /// <param name="instancingData">Instance data enumerable.</param>
        /// <typeparam name="T">The type of Vertex sent to the GPU</typeparam>
        protected void FillRuntimeDescriptor<T>(ModelRuntimeDescriptor<T> modelDescriptor, SceneRuntimeDescriptor sceneRuntimeDescriptor, IEnumerable<InstanceData> instancingData) where T : struct, VertexRuntime, VertexLocateable
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
                var allInstancePreEffects = RenderFlags.GetAllPreEffectFor(modelDescriptor.PreEffectsInstancingFlag);

                foreach(var instanceData in instancingData){
                    var instanceDataBuffer = ResourceGenerator.AllocateInstanceDataBuffer(instanceData, _graphicsDevice, _factory);
                    if(instanceDataBuffer != null){
                     modelDescriptor.InstanceBufferLists[RenderFlags.NORMAL_ARRAY_INDEX].Add(instanceDataBuffer);
                     foreach(var preEffect in allInstancePreEffects)
                        modelDescriptor.InstanceBufferLists[RenderFlags.GetArrayIndexForFlag(preEffect)].Add(instanceDataBuffer); 
                    }
                }

                _graphicsDevice.UpdateBuffer<T>(vertexBuffer, 0, ref mesh.Vertices[0], (vertexSizeInBytes * mesh.VertexCount).ToUnsigned());
                _graphicsDevice.UpdateBuffer<ushort>(indexBuffer, 0, ref mesh.Indices[0], (sizeof(ushort) * mesh.IndexCount).ToUnsigned());

                var resourceSet = modelDescriptor.InvokeTextureResourceSetGeneration(i, _factory, _graphicsDevice);
                if (resourceSet != null)
                    modelDescriptor.TextureResourceSetsList.Add(resourceSet);                
            }

            var rasterizerStateCullBack = new RasterizerStateDescription(
                    cullMode: FaceCullMode.Back,
                    fillMode: PolygonFillMode.Solid,
                    frontFace: FrontFace.Clockwise,
                    depthClipEnabled: true,
                    scissorTestEnabled: false
                );
            var rasterizerStateCullFront = new RasterizerStateDescription(
                    cullMode: FaceCullMode.Front,
                    fillMode: PolygonFillMode.Solid,
                    frontFace: FrontFace.Clockwise,
                    
                    depthClipEnabled: true,
                    scissorTestEnabled: false
                );

            modelDescriptor.InvokeVertexLayoutGeneration();

            var shadowMapIndex = RenderFlags.GetArrayIndexForFlag(RenderFlags.SHADOW_MAP);
            var omniShadowMapIndex = RenderFlags.GetArrayIndexForFlag(RenderFlags.OMNI_SHADOW_MAPS);
            modelDescriptor.Pipelines[shadowMapIndex] = modelDescriptor.ShadowMapEnabled ? _factory.CreateGraphicsPipeline(PipelineGenerator.GenerateShadowMappingPreEffectPipeline(modelDescriptor, _childrenPre[RenderFlags.GetPreEffectArrayIndexForFlag(RenderFlags.SHADOW_MAP)].SceneRuntimeDescriptor,rasterizerStateCullFront,RenderFlags.SHADOW_MAP, _childrenPre[RenderFlags.GetPreEffectArrayIndexForFlag(RenderFlags.SHADOW_MAP)].FrameBuffer)) : null;
            modelDescriptor.Pipelines[omniShadowMapIndex] = modelDescriptor.OmniShadowMapEnabled ? _factory.CreateGraphicsPipeline(PipelineGenerator.GenerateOmniShadowMappingPreEffectPipeline(modelDescriptor, _childrenPre[0].SceneRuntimeDescriptor,rasterizerStateCullFront,RenderFlags.OMNI_SHADOW_MAPS, _childrenPre[0].FrameBuffer)) : null;
            
            
            var effectLayoutArray = modelDescriptor.FillEffectsResourceSet(_factory, sceneRuntimeDescriptor, _childrenPre);
            var normalIndex = RenderFlags.GetArrayIndexForFlag(RenderFlags.NORMAL);
            switch (modelDescriptor.VertexRuntimeType)
            {
                // Only cube maps for now
                case VertexRuntimeTypes.VertexPosition:
                //TODO: Use proper constants from RenderFlags.cs
                    modelDescriptor.Pipelines[normalIndex] = _factory.CreateGraphicsPipeline(PipelineGenerator.GeneratePipelineP(modelDescriptor, sceneRuntimeDescriptor,rasterizerStateCullFront, _graphicsDevice.SwapchainFramebuffer,effectLayoutArray));
                    break;
                case VertexRuntimeTypes.VertexPositionNormal:
                    modelDescriptor.Pipelines[normalIndex] = _factory.CreateGraphicsPipeline(PipelineGenerator.GeneratePipelinePN(modelDescriptor, sceneRuntimeDescriptor,rasterizerStateCullBack, _graphicsDevice.SwapchainFramebuffer,effectLayoutArray));
                    break;
                case VertexRuntimeTypes.VertexPositionNormalTextureTangentBitangent:
                    modelDescriptor.Pipelines[normalIndex] = _factory.CreateGraphicsPipeline(PipelineGenerator.GeneratePipelinePNTTB(modelDescriptor, sceneRuntimeDescriptor,rasterizerStateCullBack, _graphicsDevice.SwapchainFramebuffer,effectLayoutArray));
                    break;
                case VertexRuntimeTypes.VertexPositionColor:
                    modelDescriptor.Pipelines[normalIndex] = _factory.CreateGraphicsPipeline(PipelineGenerator.GeneratePipelinePC(modelDescriptor, sceneRuntimeDescriptor,rasterizerStateCullBack, _graphicsDevice.SwapchainFramebuffer,effectLayoutArray));
                    break;
                case VertexRuntimeTypes.VertexPositionTexture:
                    modelDescriptor.Pipelines[normalIndex] = _factory.CreateGraphicsPipeline(PipelineGenerator.GeneratePipelinePT(modelDescriptor, sceneRuntimeDescriptor,rasterizerStateCullBack, _graphicsDevice.SwapchainFramebuffer,effectLayoutArray));
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

            // Uniform 2 - Light
            _sceneRuntimeState.LightBuffer = _factory.CreateBuffer(new BufferDescription(Light.SizeInBytes + 16, BufferUsage.UniformBuffer));
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

            // Uniform 3 - PointLight
            _sceneRuntimeState.SpotLightBuffer = _factory.CreateBuffer(new BufferDescription(Core.Numerics.Utils.SinglePrecision4x4InBytes, BufferUsage.UniformBuffer));
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

            // Uniform 4 - Material
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

            // TODO: Make this conditional on effects
            // Uniform 5 - LightProjView (ShadowMapping)
            _sceneRuntimeState.LightProjViewBuffer = _factory.CreateBuffer(new BufferDescription(Core.Numerics.Utils.SinglePrecision4x4InBytes, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            _sceneRuntimeState.LightProvViewResourceLayout
                = ResourceGenerator.GenerateResourceLayout(
                    _factory,
                    "lightProjView",
                    ResourceKind.UniformBuffer,
                    ShaderStages.Vertex);
            _sceneRuntimeState.LightProjViewResourceSet
                = ResourceGenerator.GenrateResourceSet(
                    _factory,
                    _sceneRuntimeState.LightProvViewResourceLayout,
                    new BindableResource[] { _sceneRuntimeState.LightProjViewBuffer });

            _sceneRuntimeState.CameraInfoBuffer = _factory.CreateBuffer(new BufferDescription(2 * 8, BufferUsage.UniformBuffer));
            _sceneRuntimeState.CameraInfoResourceLayout
                = ResourceGenerator.GenerateResourceLayout(
                    _factory,
                    "cameraInfo",
                    ResourceKind.UniformBuffer,
                    ShaderStages.Fragment);
            _sceneRuntimeState.CameraInfoResourceSet
                = ResourceGenerator.GenrateResourceSet(
                    _factory,
                    _sceneRuntimeState.CameraInfoResourceLayout,
                    new BindableResource[] { _sceneRuntimeState.CameraInfoBuffer });

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

        //TODO: Make this generical for whole pre effects pipeline
        public void CreateShadowMap(Resolution resolution){
            var shadowMap = new ShadowMap(_graphicsDevice, resolution);
            // TODO: add to correct index - this only correct by chance
            _childrenPre.Add(shadowMap);
        }

        public void CreateOmniShadowMap(Resolution resolution){
            var omniShadowMap = new OmniShadowMap(_graphicsDevice, resolution);
            // TODO: add to correct index
            _childrenPre.Add(omniShadowMap);
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

        //TODO: Move this to resource generator?
        // private DeviceBuffer AllocateInstanceDataBuffer(InstanceData instanceData) {
        //     DeviceBuffer deviceBuffer = null;
        //     switch(instanceData.Flag){
        //         case InstancingDataFlags.EMPTY:
        //             break;
        //         case InstancingDataFlags.POSITION:
        //             deviceBuffer =  _factory.CreateBuffer(new BufferDescription(instanceData.Positions.Length.ToUnsigned() * 12, BufferUsage.VertexBuffer));
        //             _graphicsDevice.UpdateBuffer(deviceBuffer, 0, instanceData.Positions);
        //             break;
        //         case InstancingDataFlags.VIEW_MATRICES:
        //             deviceBuffer = _factory.CreateBuffer(new BufferDescription(instanceData.ViewMatrices.Length.ToUnsigned() * 64, BufferUsage.VertexBuffer));
        //             _graphicsDevice.UpdateBuffer(deviceBuffer, 0, instanceData.Positions);
        //             break;
        //         default:
        //             throw new NotImplementedException($"{instanceData.Flag.ToString("g")} not implemented");
        //     }

        //     return deviceBuffer;
        // }
    }


}