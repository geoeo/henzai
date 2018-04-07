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
using Henzai.Runtime;

namespace Henzai.Examples
{
    internal class Scene : Renderable
    {
        private Camera _staticCamera;
        private Framebuffer _offScreenFBO;
        private Matrix4x4 _worldTransCube;
        private DeviceBuffer _vertexBufferCube;
        private DeviceBuffer _indexBufferCube;
        private Shader _vertexShaderCube;
        private Shader _fragmentShaderCube;
        private Pipeline _pipelineCube;
        private Pipeline _pipelineCubeOffscreen;

        private DeviceBuffer _indexBufferQuad;

        private Matrix4x4 _worldTranscoloredQuad;
        private DeviceBuffer _vertexBuffercoloredQuad;
        private Shader _vertexShadercoloredQuad;
        private Shader _fragmentShadercoloredQuad;
        private Pipeline _pipelinecoloredQuad;
        private Pipeline _pipelinecoloredQuadOffscreen;

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

        public Scene(string title,Resolution windowSize, GraphicsDeviceOptions graphicsDeviceOptions, RenderOptions renderOptions)
            : base(title,windowSize,graphicsDeviceOptions,renderOptions){
            PreRenderLoop += ScaleTextureQuadToMatchResolution;
            PreDraw += this.RotateCube;
        }

        override protected void FormatResourcesForRuntime(){}

        override protected void CreateResources(){

            _staticCamera = new Camera(_renderResolution.Horizontal,_renderResolution.Vertical);

            createOffscreenFBO();

            createTransformationPipelineUniform();

            createCubeResources();

            createcoloredQuadResources();

            createTexturedQuadResources();

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
                new ResourceLayoutElementDescription("colorTexture",ResourceKind.TextureReadOnly,ShaderStages.Fragment),
                new ResourceLayoutElementDescription("colorSampler",ResourceKind.Sampler,ShaderStages.Fragment)
            ));

            _textureOffscreenResourceSet = _factory.CreateResourceSet(new ResourceSetDescription(_offscreenLayout,_offscreenTextureView,GraphicsDevice.LinearSampler));

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
            GraphicsDevice.UpdateBuffer(_transformationPipelineBuffer,64,Camera.ProjectionMatrix);

            return new List<IDisposable>()
            {
                _transformationPipelineBuffer,
                _transformationPipelineResourceLayout,
                _transformationPipelineResourceSet

            };
        }

        private List<IDisposable> createCubeResources(){

            ImageSharpTexture NameImage = new ImageSharpTexture(Path.Combine(AppContext.BaseDirectory, "Textures", "Name.png"));
            Texture cubeTexture = NameImage.CreateDeviceTexture(GraphicsDevice, _factory);
            TextureView cubeTextureView = _factory.CreateTextureView(cubeTexture);

            ResourceLayout textureLayout = _factory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("CubeTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("CubeSampler", ResourceKind.Sampler, ShaderStages.Fragment)));

            _textureNameResourceSet = _factory.CreateResourceSet(new ResourceSetDescription(
                textureLayout,
                cubeTextureView,
                GraphicsDevice.LinearSampler));

            Mesh<VertexPositionTexture> texturedCube 
                = GeometryFactory.GenerateTexturedCube();

            ushort[] cubeIndicies = GeometryFactory.generateCubeIndicies_TriangleList_CW();

            // declare (VBO) buffers
            _vertexBufferCube 
                = _factory.CreateBuffer(new BufferDescription(texturedCube.vertices.LengthUnsigned() * VertexPositionTexture.SizeInBytes, BufferUsage.VertexBuffer));
            _indexBufferCube 
                = _factory.CreateBuffer(new BufferDescription(cubeIndicies.LengthUnsigned()*sizeof(ushort),BufferUsage.IndexBuffer));

            // fill buffers with data
            GraphicsDevice.UpdateBuffer(_vertexBufferCube,0,texturedCube.vertices);
            GraphicsDevice.UpdateBuffer(_indexBufferCube,0,cubeIndicies);

            VertexLayoutDescription vertexLayout 
                = new VertexLayoutDescription(
                    new VertexElementDescription("Position",VertexElementSemantic.Position,VertexElementFormat.Float3),
                    new VertexElementDescription("UV",VertexElementSemantic.TextureCoordinate,VertexElementFormat.Float2)
                );

            _worldTransCube = Matrix4x4.CreateWorld(new Vector3(0,0,2),-Vector3.UnitZ,Vector3.UnitY);

            _vertexShaderCube = IO.LoadShader("Texture",ShaderStages.Vertex,GraphicsDevice);
            _fragmentShaderCube = IO.LoadShader("Texture",ShaderStages.Fragment,GraphicsDevice);

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
                Outputs = GraphicsDevice.SwapchainFramebuffer.OutputDescription
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

        private List<IDisposable> createcoloredQuadResources(){

            Mesh<VertexPositionNDCColor> quad = GeometryFactory.GenerateColorQuadNDC(RgbaFloat.Red, RgbaFloat.Blue,RgbaFloat.Green,RgbaFloat.Orange);
            ushort[] quadIndicies = GeometryFactory.GenerateQuadIndicies_TriangleStrip_CW();

            _vertexBuffercoloredQuad = _factory.CreateBuffer(new BufferDescription(quad.vertices.LengthUnsigned()* VertexPositionNDCColor.SizeInBytes, BufferUsage.VertexBuffer));
            _indexBufferQuad = _factory.CreateBuffer(new BufferDescription(quadIndicies.LengthUnsigned()* sizeof(ushort), BufferUsage.IndexBuffer));

            GraphicsDevice.UpdateBuffer(_vertexBuffercoloredQuad,0,quad.vertices);
            GraphicsDevice.UpdateBuffer(_indexBufferQuad,0,quadIndicies);

            VertexLayoutDescription vertexLayout 
                = new VertexLayoutDescription(
                    new VertexElementDescription("Position",VertexElementSemantic.Position,VertexElementFormat.Float2),
                    new VertexElementDescription("color",VertexElementSemantic.Color,VertexElementFormat.Float4)
                );

            _worldTranscoloredQuad = Matrix4x4.CreateWorld(new Vector3(-5,0,0),-Vector3.UnitZ,Vector3.UnitY);

            _vertexShadercoloredQuad = IO.LoadShader("Quadcolor",ShaderStages.Vertex,GraphicsDevice);
            _fragmentShadercoloredQuad = IO.LoadShader("Quadcolor",ShaderStages.Fragment,GraphicsDevice);

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
                    shaders: new Shader[] {_vertexShadercoloredQuad,_fragmentShadercoloredQuad}
                ),
                Outputs = GraphicsDevice.SwapchainFramebuffer.OutputDescription
            };

            _pipelinecoloredQuad = _factory.CreateGraphicsPipeline(pipelineDescription);

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
                    shaders: new Shader[] {_vertexShadercoloredQuad,_fragmentShadercoloredQuad}
                ),
                Outputs = _offScreenFBO.OutputDescription
            };

            _pipelinecoloredQuadOffscreen = _factory.CreateGraphicsPipeline(pipelineDescription);

            return new List<IDisposable>()
            {
                _vertexBuffercoloredQuad,
                _indexBufferQuad,
                _vertexShadercoloredQuad,
                _fragmentShadercoloredQuad,
                _pipelinecoloredQuad
            };

        }

        private IList<IDisposable> createTexturedQuadResources(){

            Mesh<VertexPositionTexture> quad = GeometryFactory.GenerateTexturedQuad();
            if(_indexBufferQuad == null)
                throw new ApplicationException("_indexBufferQuad should have been created");

            _vertexBufferTexturedQuad = _factory.CreateBuffer(new BufferDescription(quad.vertices.LengthUnsigned() * VertexPositionTexture.SizeInBytes,BufferUsage.VertexBuffer));
            GraphicsDevice.UpdateBuffer(_vertexBufferTexturedQuad,0,quad.vertices);

            VertexLayoutDescription vertexLayout 
                = new VertexLayoutDescription(
                    new VertexElementDescription("Position",VertexElementSemantic.Position,VertexElementFormat.Float3),
                    new VertexElementDescription("UV",VertexElementSemantic.TextureCoordinate,VertexElementFormat.Float2)
                );

            _worldTransTexturedQuad = Matrix4x4.CreateWorld(new Vector3(5,0,0),-Vector3.UnitZ,Vector3.UnitY);

            _vertexShaderTexturedQuad = IO.LoadShader("QuadTexture",ShaderStages.Vertex,GraphicsDevice);
            _fragmentShaderTexturedQuad = IO.LoadShader("QuadTexture",ShaderStages.Fragment,GraphicsDevice);

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
                Outputs = GraphicsDevice.SwapchainFramebuffer.OutputDescription
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

        override protected void BuildCommandList(){

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
            // _commandList.UpdateBuffer(_transformationPipelineBuffer,128,new Vector4(1f,0f,0f,0f));
            // _commandList.UpdateBuffer(_transformationPipelineBuffer,144,new Vector4(0f,1f,0f,0f));
            // _commandList.UpdateBuffer(_transformationPipelineBuffer,160,new Vector4(0f,0f,1f,0f));
            // _commandList.UpdateBuffer(_transformationPipelineBuffer,176,new Vector4(-1f,0f,0f,1f));
            _commandList.SetGraphicsResourceSet(0,_transformationPipelineResourceSet); // Always after SetPipeline
            _commandList.SetGraphicsResourceSet(1,_textureNameResourceSet); // Always after SetPipeline
            _commandList.DrawIndexed(
                indexCount: 36,
                instanceCount: 1,
                indexStart: 0,
                vertexOffset: 0,
                instanceStart: 0
            );

            _commandList.SetPipeline(_pipelinecoloredQuadOffscreen);
            _commandList.SetVertexBuffer(0,_vertexBuffercoloredQuad);
            _commandList.SetIndexBuffer(_indexBufferQuad,IndexFormat.UInt16);
            _commandList.UpdateBuffer(_transformationPipelineBuffer,0,_staticCamera.ViewMatrix);
            _commandList.UpdateBuffer(_transformationPipelineBuffer,128,_worldTranscoloredQuad);
            _commandList.SetGraphicsResourceSet(0,_transformationPipelineResourceSet);
            _commandList.DrawIndexed(
                indexCount: 4,
                instanceCount: 1,
                indexStart: 0,
                vertexOffset: 0,
                instanceStart: 0
            );

            _commandList.SetFramebuffer(GraphicsDevice.SwapchainFramebuffer);
            _commandList.SetFullViewports();
            _commandList.ClearColorTarget(0,RgbaFloat.Black);
            _commandList.ClearDepthStencil(1f);

            _commandList.SetPipeline(_pipelineCube);
            _commandList.SetVertexBuffer(0,_vertexBufferCube);
            _commandList.SetIndexBuffer(_indexBufferCube,IndexFormat.UInt16);
            _commandList.UpdateBuffer(_transformationPipelineBuffer,0,Camera.ViewMatrix);
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

            _commandList.SetPipeline(_pipelinecoloredQuad);
            _commandList.SetVertexBuffer(0,_vertexBuffercoloredQuad);
            _commandList.SetIndexBuffer(_indexBufferQuad,IndexFormat.UInt16);
            _commandList.UpdateBuffer(_transformationPipelineBuffer,0,Camera.ViewMatrix);
            _commandList.UpdateBuffer(_transformationPipelineBuffer,128,_worldTranscoloredQuad);
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
            _commandList.UpdateBuffer(_transformationPipelineBuffer,0,Camera.ViewMatrix);
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

        }

        override protected void Draw(){     
            GraphicsDevice.SubmitCommands(_commandList);
        }

        private void ScaleTextureQuadToMatchResolution(){
            float horizontal = _renderResolution.Horizontal.ToFloat()/_renderResolution.Vertical.ToFloat();
            float vertical = 1;
            Matrix4x4 scale = Matrix4x4.CreateScale(horizontal,vertical,1f,new Vector3(-1,0,0));
            _worldTransTexturedQuad= scale*_worldTransTexturedQuad;
            
        }

        private void RotateCube(float deltaSeconds){
            float radian = (float)Math.PI/180.0f;
            radian *= 10.0f*deltaSeconds;
            Matrix4x4 rotationAroundY = Matrix4x4.CreateRotationY(radian);
            // rotates cube around origin before translaiton i.e spinning in place
            //_worldTransCube = _worldTransCube*rotationAroundY; 
            // rotates cube around origin after translaiton i.e spinning around origin
            _worldTransCube = _worldTransCube*rotationAroundY; 

        }
    }
}
