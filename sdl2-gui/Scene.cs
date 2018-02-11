using System;
using System.Numerics;
using System.Collections.Generic;
using System.IO;
using Veldrid;
using Veldrid.StartupUtilities;
using Veldrid.Sdl2;
using Veldrid.ImageSharp;
using Henzai;
using Henzai.Extensions;
using Henzai.Geometry;

namespace Henzai.Examples
{
    public class Scene : Renderable
    {
        private Camera _staticCamera;

        private CommandList _commandList;
        private Framebuffer _offScreenFBO;
        private ResourceFactory _factory;

        private Matrix4x4 _worldTransCube;
        private DeviceBuffer _vertexBufferCube;
        private DeviceBuffer _indexBufferCube;
        private Shader _vertexShaderCube;
        private Shader _fragmentShaderCube;
        private Pipeline _pipelineCube;
        private Pipeline _pipelineCubeOffscreen;

        private DeviceBuffer _indexBufferQuad;

        private Matrix4x4 _worldTransColouredQuad;
        private DeviceBuffer _vertexBufferColouredQuad;
        private Shader _vertexShaderColouredQuad;
        private Shader _fragmentShaderColouredQuad;
        private Pipeline _pipelineColouredQuad;
        private Pipeline _pipelineColouredQuadOffscreen;

        private Matrix4x4 _worldTransTexturedQuad;
        private DeviceBuffer _vertexBufferTexturedQuad;
        private Shader _vertexShaderTexturedQuad;
        private Shader _fragmentShaderTexturedQuad;
        private Pipeline _pipelineTexturedQuad;

        private ResourceSet _textureNameResourceSet;

        private ResourceLayout _offscreenLayout;
        private ResourceSet _textureOffscreenResourceSet;
        private TextureView _offscreenTextureView;

        private DeviceBuffer _transformationPipelineBuffer;
        private ResourceSet _transformationPipelineResourceSet;
        private ResourceLayout _transformationPipelineResourceLayout;

        public Scene(string title,Resolution windowSize, GraphicsDeviceOptions graphicsDeviceOptions, GraphicsBackend preferredBackend, bool usePreferredGraphicsBackend)
            : base(title,windowSize,graphicsDeviceOptions,preferredBackend,usePreferredGraphicsBackend){
            PreRenderLoop += ScaleTextureQuadToMatchResolution;
            PreDraw += this.RotateCube;
        }

        override protected List<IDisposable> CreateResources(){

            _staticCamera = new Camera(_renderResolution.Horizontal,_renderResolution.Vertical);

            _factory = graphicsDevice.ResourceFactory;

            _commandList = _factory.CreateCommandList();

            List<IDisposable> resources = new List<IDisposable>(){_commandList};

            resources.AddRange(createOffscreenFBO());

            resources.AddRange(createTransformationPipelineUniform());

            resources.AddRange(createCubeResources());

            resources.AddRange(createColouredQuadResources());

            resources.AddRange(createTexturedQuadResources());

            return resources;

        }

        private List<IDisposable> createOffscreenFBO(){
            Texture offscreenTexture = _factory.CreateTexture(TextureDescription.Texture2D(
                    _renderResolution.Horizontal.ToUnsigned(),
                    _renderResolution.Vertical.ToUnsigned(),1,1,
                    PixelFormat.B8_G8_R8_A8_UNorm,
                    TextureUsage.RenderTarget | TextureUsage.Sampled));
            Texture offscreenDepth = _factory.CreateTexture(TextureDescription.Texture2D(
                _renderResolution.Horizontal.ToUnsigned(),
                _renderResolution.Vertical.ToUnsigned(),
                1,1,
                PixelFormat.R16_UNorm,TextureUsage.DepthStencil));

            _offscreenTextureView = _factory.CreateTextureView(offscreenTexture);
            _offScreenFBO = _factory.CreateFramebuffer(new FramebufferDescription(offscreenDepth,offscreenTexture));

            _offscreenLayout = _factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("ColourTexture",ResourceKind.TextureReadOnly,ShaderStages.Fragment),
                new ResourceLayoutElementDescription("ColourSampler",ResourceKind.Sampler,ShaderStages.Fragment)
            ));

            _textureOffscreenResourceSet = _factory.CreateResourceSet(new ResourceSetDescription(_offscreenLayout,_offscreenTextureView,graphicsDevice.LinearSampler));

            return new List<IDisposable>()
            {
                _offscreenTextureView,
                _offScreenFBO,
                _offscreenLayout,
                _textureOffscreenResourceSet
            };

        }

        private List<IDisposable> createTransformationPipelineUniform(){

            _transformationPipelineBuffer = _factory.CreateBuffer(new BufferDescription(192,BufferUsage.UniformBuffer | BufferUsage.Dynamic));

            ResourceLayoutElementDescription resourceLayoutElementDescription = new ResourceLayoutElementDescription("transformPipeline",ResourceKind.UniformBuffer,ShaderStages.Vertex);
            ResourceLayoutElementDescription[] resourceLayoutElementDescriptions = {resourceLayoutElementDescription};
            ResourceLayoutDescription resourceLayoutDescription = new ResourceLayoutDescription(resourceLayoutElementDescriptions);
            BindableResource[] bindableResources = new BindableResource[]{_transformationPipelineBuffer};

            _transformationPipelineResourceLayout = _factory.CreateResourceLayout(resourceLayoutDescription);
            ResourceSetDescription resourceSetDescription = new ResourceSetDescription(_transformationPipelineResourceLayout,bindableResources);
            
            _transformationPipelineResourceSet = _factory.CreateResourceSet(resourceSetDescription);

            //_graphicsDevice.UpdateBuffer(_transformationPipelineBuffer,0,_camera.ViewMatrix);
            graphicsDevice.UpdateBuffer(_transformationPipelineBuffer,64,camera.ProjectionMatrix);

            return new List<IDisposable>()
            {
                _transformationPipelineBuffer,
                _transformationPipelineResourceLayout,
                _transformationPipelineResourceSet

            };
        }

        private List<IDisposable> createCubeResources(){

            ImageSharpTexture NameImage = new ImageSharpTexture(Path.Combine(AppContext.BaseDirectory, "Textures", "Name.png"));
            Texture cubeTexture = NameImage.CreateDeviceTexture(graphicsDevice, _factory);
            TextureView cubeTextureView = _factory.CreateTextureView(cubeTexture);

            ResourceLayout textureLayout = _factory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("CubeTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("CubeSampler", ResourceKind.Sampler, ShaderStages.Fragment)));

            _textureNameResourceSet = _factory.CreateResourceSet(new ResourceSetDescription(
                textureLayout,
                cubeTextureView,
                graphicsDevice.LinearSampler));

            TexturedCube texturedCube 
                = GeometryFactory.generateTexturedCube();

            ushort[] cubeIndicies = GeometryFactory.generateCubeIndicies_TriangleList_CW();

            // declare (VBO) buffers
            _vertexBufferCube 
                = _factory.CreateBuffer(new BufferDescription(texturedCube.vertices.LengthUnsigned() * VertexPositionTexture.SizeInBytes, BufferUsage.VertexBuffer));
            _indexBufferCube 
                = _factory.CreateBuffer(new BufferDescription(cubeIndicies.LengthUnsigned()*sizeof(ushort),BufferUsage.IndexBuffer));

            // fill buffers with data
            graphicsDevice.UpdateBuffer(_vertexBufferCube,0,texturedCube.vertices);
            graphicsDevice.UpdateBuffer(_indexBufferCube,0,cubeIndicies);

            VertexLayoutDescription vertexLayout 
                = new VertexLayoutDescription(
                    new VertexElementDescription("Position",VertexElementSemantic.Position,VertexElementFormat.Float3),
                    new VertexElementDescription("UV",VertexElementSemantic.TextureCoordinate,VertexElementFormat.Float2)
                );

            _worldTransCube = Matrix4x4.CreateWorld(new Vector3(0,0,0),-Vector3.UnitZ,Vector3.UnitY);

            _vertexShaderCube = IO.LoadShader("Cube",ShaderStages.Vertex,graphicsDevice);
            _fragmentShaderCube = IO.LoadShader("Cube",ShaderStages.Fragment,graphicsDevice);

            GraphicsPipelineDescription pipelineDescription = new GraphicsPipelineDescription(){
                BlendState = BlendStateDescription.SingleOverrideBlend,
                DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual,
                RasterizerState = new RasterizerStateDescription(
                    cullMode: FaceCullMode.Back,
                    fillMode: PolygonFillMode.Solid,
                    frontFace: FrontFace.Clockwise,
                    depthClipEnabled: true,
                    scissorTestEnabled: false
                ),
                PrimitiveTopology = PrimitiveTopology.TriangleList,
                ResourceLayouts = new ResourceLayout[] {_transformationPipelineResourceLayout,textureLayout},
                ShaderSet = new ShaderSetDescription(
                    vertexLayouts: new VertexLayoutDescription[] {vertexLayout},
                    shaders: new Shader[] {_vertexShaderCube,_fragmentShaderCube}
                ),
                Outputs = graphicsDevice.SwapchainFramebuffer.OutputDescription
            };

            _pipelineCube = _factory.CreateGraphicsPipeline(pipelineDescription);

            pipelineDescription = new GraphicsPipelineDescription(){
                BlendState = BlendStateDescription.SingleOverrideBlend,
                DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual,
                RasterizerState = new RasterizerStateDescription(
                    cullMode: FaceCullMode.Back,
                    fillMode: PolygonFillMode.Solid,
                    frontFace: FrontFace.Clockwise,
                    depthClipEnabled: true,
                    scissorTestEnabled: false
                ),
                PrimitiveTopology = PrimitiveTopology.TriangleList,
                ResourceLayouts = new ResourceLayout[] {_transformationPipelineResourceLayout,textureLayout},
                ShaderSet = new ShaderSetDescription(
                    vertexLayouts: new VertexLayoutDescription[] {vertexLayout},
                    shaders: new Shader[] {_vertexShaderCube,_fragmentShaderCube}
                ),
                Outputs = _offScreenFBO.OutputDescription
            };

            _pipelineCubeOffscreen = _factory.CreateGraphicsPipeline(pipelineDescription);

            return new List<IDisposable>()
            {
                _vertexBufferCube,
                _indexBufferCube,
                _textureNameResourceSet,
                _vertexShaderCube,
                _fragmentShaderCube,
                _pipelineCube
            };
        }

        private List<IDisposable> createColouredQuadResources(){

            ColouredQuad quad = GeometryFactory.generateColouredQuad(RgbaFloat.Red, RgbaFloat.Blue,RgbaFloat.Green,RgbaFloat.Orange);
            ushort[] quadIndicies = GeometryFactory.generateQuadIndicies_TriangleStrip_CW();

            _vertexBufferColouredQuad = _factory.CreateBuffer(new BufferDescription(quad.vertecies.LengthUnsigned()* VertexPositionColour.SizeInBytes, BufferUsage.VertexBuffer));
            _indexBufferQuad = _factory.CreateBuffer(new BufferDescription(quadIndicies.LengthUnsigned()* sizeof(ushort), BufferUsage.IndexBuffer));

            graphicsDevice.UpdateBuffer(_vertexBufferColouredQuad,0,quad.vertecies);
            graphicsDevice.UpdateBuffer(_indexBufferQuad,0,quadIndicies);

            VertexLayoutDescription vertexLayout 
                = new VertexLayoutDescription(
                    new VertexElementDescription("Position",VertexElementSemantic.Position,VertexElementFormat.Float2),
                    new VertexElementDescription("Colour",VertexElementSemantic.Color,VertexElementFormat.Float4)
                );

            _worldTransColouredQuad = Matrix4x4.CreateWorld(new Vector3(-5,0,0),-Vector3.UnitZ,Vector3.UnitY);

            _vertexShaderColouredQuad = IO.LoadShader("QuadColour",ShaderStages.Vertex,graphicsDevice);
            _fragmentShaderColouredQuad = IO.LoadShader("QuadColour",ShaderStages.Fragment,graphicsDevice);

            GraphicsPipelineDescription pipelineDescription = new GraphicsPipelineDescription(){
                BlendState = BlendStateDescription.SingleOverrideBlend,
                DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual,
                RasterizerState = new RasterizerStateDescription(
                    cullMode: FaceCullMode.Back,
                    fillMode: PolygonFillMode.Solid,
                    frontFace: FrontFace.Clockwise,
                    depthClipEnabled: true,
                    scissorTestEnabled: false
                ),
                PrimitiveTopology = PrimitiveTopology.TriangleStrip,
                ResourceLayouts = new ResourceLayout[] {_transformationPipelineResourceLayout},
                ShaderSet = new ShaderSetDescription(
                    vertexLayouts: new VertexLayoutDescription[] {vertexLayout},
                    shaders: new Shader[] {_vertexShaderColouredQuad,_fragmentShaderColouredQuad}
                ),
                Outputs = graphicsDevice.SwapchainFramebuffer.OutputDescription
            };

            _pipelineColouredQuad = _factory.CreateGraphicsPipeline(pipelineDescription);

            pipelineDescription = new GraphicsPipelineDescription(){
                BlendState = BlendStateDescription.SingleOverrideBlend,
                DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual,
                RasterizerState = new RasterizerStateDescription(
                    cullMode: FaceCullMode.Back,
                    fillMode: PolygonFillMode.Solid,
                    frontFace: FrontFace.Clockwise,
                    depthClipEnabled: true,
                    scissorTestEnabled: false
                ),
                PrimitiveTopology = PrimitiveTopology.TriangleStrip,
                ResourceLayouts = new ResourceLayout[] {_transformationPipelineResourceLayout},
                ShaderSet = new ShaderSetDescription(
                    vertexLayouts: new VertexLayoutDescription[] {vertexLayout},
                    shaders: new Shader[] {_vertexShaderColouredQuad,_fragmentShaderColouredQuad}
                ),
                Outputs = _offScreenFBO.OutputDescription
            };

            _pipelineColouredQuadOffscreen = _factory.CreateGraphicsPipeline(pipelineDescription);

            return new List<IDisposable>()
            {
                _vertexBufferColouredQuad,
                _indexBufferQuad,
                _vertexShaderColouredQuad,
                _fragmentShaderColouredQuad,
                _pipelineColouredQuad
            };

        }

        private IList<IDisposable> createTexturedQuadResources(){

            TexturedQuad quad = GeometryFactory.generateTexturedQuad();
            if(_indexBufferQuad == null)
                throw new ApplicationException("_indexBufferQuad should have been created");

            _vertexBufferTexturedQuad = _factory.CreateBuffer(new BufferDescription(quad.vertecies.LengthUnsigned() * VertexPositionTexture.SizeInBytes,BufferUsage.VertexBuffer));
            graphicsDevice.UpdateBuffer(_vertexBufferTexturedQuad,0,quad.vertecies);

            VertexLayoutDescription vertexLayout 
                = new VertexLayoutDescription(
                    new VertexElementDescription("Position",VertexElementSemantic.Position,VertexElementFormat.Float3),
                    new VertexElementDescription("UV",VertexElementSemantic.TextureCoordinate,VertexElementFormat.Float2)
                );

            _worldTransTexturedQuad = Matrix4x4.CreateWorld(new Vector3(5,0,0),-Vector3.UnitZ,Vector3.UnitY);

            _vertexShaderTexturedQuad = IO.LoadShader("QuadTexture",ShaderStages.Vertex,graphicsDevice);
            _fragmentShaderTexturedQuad = IO.LoadShader("QuadTexture",ShaderStages.Fragment,graphicsDevice);

            GraphicsPipelineDescription pipelineDescription = new GraphicsPipelineDescription(){
                BlendState = BlendStateDescription.SingleOverrideBlend,
                DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual,
                RasterizerState = new RasterizerStateDescription(
                    cullMode: FaceCullMode.Back,
                    fillMode: PolygonFillMode.Solid,
                    frontFace: FrontFace.Clockwise,
                    depthClipEnabled: true,
                    scissorTestEnabled: false
                ),
                PrimitiveTopology = PrimitiveTopology.TriangleStrip,
                ResourceLayouts = new ResourceLayout[] {_transformationPipelineResourceLayout,_offscreenLayout},
                ShaderSet = new ShaderSetDescription(
                    vertexLayouts: new VertexLayoutDescription[] {vertexLayout},
                    shaders: new Shader[] {_vertexShaderTexturedQuad,_fragmentShaderTexturedQuad}
                ),
                Outputs = graphicsDevice.SwapchainFramebuffer.OutputDescription
            };

            _pipelineTexturedQuad = _factory.CreateGraphicsPipeline(pipelineDescription);

            return new List<IDisposable>()
            {
               _textureOffscreenResourceSet, 
               _vertexBufferTexturedQuad,
               _vertexShaderTexturedQuad,
               _fragmentShaderTexturedQuad,
               _pipelineTexturedQuad

            };
            

        }

        override protected void Draw(){
            _commandList.Begin();
            
            _commandList.SetFramebuffer(_offScreenFBO);
            _commandList.SetFullViewports();
            _commandList.ClearColorTarget(0,RgbaFloat.White);
            _commandList.ClearDepthStencil(1f);

            _commandList.SetPipeline(_pipelineCubeOffscreen);
            _commandList.SetVertexBuffer(0,_vertexBufferCube);
            _commandList.SetIndexBuffer(_indexBufferCube,IndexFormat.UInt16);
            _commandList.UpdateBuffer(_transformationPipelineBuffer,0,_staticCamera.ViewMatrix);
            _commandList.UpdateBuffer(_transformationPipelineBuffer,128,_worldTransCube);
            _commandList.SetGraphicsResourceSet(0,_transformationPipelineResourceSet); // Always after SetPipeline
            _commandList.SetGraphicsResourceSet(1,_textureNameResourceSet); // Always after SetPipeline
            _commandList.DrawIndexed(
                indexCount: 36,
                instanceCount: 1,
                indexStart: 0,
                vertexOffset: 0,
                instanceStart: 0
            );

            _commandList.SetPipeline(_pipelineColouredQuadOffscreen);
            _commandList.SetVertexBuffer(0,_vertexBufferColouredQuad);
            _commandList.SetIndexBuffer(_indexBufferQuad,IndexFormat.UInt16);
            _commandList.UpdateBuffer(_transformationPipelineBuffer,0,_staticCamera.ViewMatrix);
            _commandList.UpdateBuffer(_transformationPipelineBuffer,128,_worldTransColouredQuad);
            _commandList.SetGraphicsResourceSet(0,_transformationPipelineResourceSet);
            _commandList.DrawIndexed(
                indexCount: 4,
                instanceCount: 1,
                indexStart: 0,
                vertexOffset: 0,
                instanceStart: 0
            );

            _commandList.SetFramebuffer(graphicsDevice.SwapchainFramebuffer);
            _commandList.SetFullViewports();
            _commandList.ClearColorTarget(0,RgbaFloat.Black);
            _commandList.ClearDepthStencil(1f);

            _commandList.SetPipeline(_pipelineCube);
            _commandList.SetVertexBuffer(0,_vertexBufferCube);
            _commandList.SetIndexBuffer(_indexBufferCube,IndexFormat.UInt16);
            _commandList.UpdateBuffer(_transformationPipelineBuffer,0,camera.ViewMatrix);
            _commandList.UpdateBuffer(_transformationPipelineBuffer,128,_worldTransCube);
            _commandList.SetGraphicsResourceSet(0,_transformationPipelineResourceSet); // Always after SetPipeline
            _commandList.SetGraphicsResourceSet(1,_textureNameResourceSet); // Always after SetPipeline
            _commandList.DrawIndexed(
                indexCount: 36,
                instanceCount: 1,
                indexStart: 0,
                vertexOffset: 0,
                instanceStart: 0
            );

            _commandList.SetPipeline(_pipelineColouredQuad);
            _commandList.SetVertexBuffer(0,_vertexBufferColouredQuad);
            _commandList.SetIndexBuffer(_indexBufferQuad,IndexFormat.UInt16);
            _commandList.UpdateBuffer(_transformationPipelineBuffer,0,camera.ViewMatrix);
            _commandList.UpdateBuffer(_transformationPipelineBuffer,128,_worldTransColouredQuad);
            _commandList.SetGraphicsResourceSet(0,_transformationPipelineResourceSet);
            _commandList.DrawIndexed(
                indexCount: 4,
                instanceCount: 1,
                indexStart: 0,
                vertexOffset: 0,
                instanceStart: 0
            );

            _commandList.SetPipeline(_pipelineTexturedQuad);
            _commandList.SetVertexBuffer(0,_vertexBufferTexturedQuad);
            _commandList.SetIndexBuffer(_indexBufferQuad,IndexFormat.UInt16);
            _commandList.UpdateBuffer(_transformationPipelineBuffer,0,camera.ViewMatrix);
            _commandList.UpdateBuffer(_transformationPipelineBuffer,128,_worldTransTexturedQuad);
            _commandList.SetGraphicsResourceSet(0,_transformationPipelineResourceSet);
            _commandList.SetGraphicsResourceSet(1,_textureOffscreenResourceSet); 
            _commandList.DrawIndexed(
                indexCount: 4,
                instanceCount: 1,
                indexStart: 0,
                vertexOffset: 0,
                instanceStart: 0
            );
            
            _commandList.End();

            graphicsDevice.SubmitCommands(_commandList);
        }

        private void ScaleTextureQuadToMatchResolution(){
            float horizontal = _renderResolution.Horizontal.ToFloat()/_renderResolution.Vertical.ToFloat();
            float vertical = 1;
            Matrix4x4 scale = Matrix4x4.CreateScale(horizontal,vertical,1f,new Vector3(-1,0,0));
            _worldTransTexturedQuad= scale*_worldTransTexturedQuad;
            
        }

        private void RotateCube(float deltaSeconds){
            float radian = (float)Math.PI/180.0f;
            radian *= 100.0f*deltaSeconds;
            Matrix4x4 rotationAroundY = Matrix4x4.CreateRotationY(radian);
            _worldTransCube = rotationAroundY*_worldTransCube;

        }
    }
}
