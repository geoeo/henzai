using System;
using System.Numerics;
using System.IO;
using Veldrid;
using Veldrid.ImageSharp;
using Henzai.Core.Extensions;
using Henzai.Geometry;
using Henzai.Runtime;
using Henzai.Core.VertexGeometry;
using Henzai.Cameras;

namespace Henzai.Examples
{
    internal class TexturedCubeScene : Renderable
    {
        private DeviceBuffer _vertexBuffer;
        private DeviceBuffer _indexBuffer;
        private Shader _vertexShader;
        private Shader _fragmentShader;
        private Pipeline _pipeline;
        private DeviceBuffer _cameraProjViewBuffer;
        private ResourceSet _cameraResourceSet;
        private ResourceSet _textureResourceSet;
        private ResourceLayout _resourceLayout;
        private Matrix4x4 _worldTransCube;

        public TexturedCubeScene(string title,Resolution windowSize, GraphicsDeviceOptions graphicsDeviceOptions, RenderOptions renderOptions)
            : base(title,windowSize,graphicsDeviceOptions,renderOptions){
                PreDraw_Time_Camera += rotateCameraAround;
                _worldTransCube = Matrix4x4.CreateWorld(new Vector3(0,0,0),-Vector3.UnitZ,Vector3.UnitY);
            }

        private void rotateCameraAround(float deltaSeconds, Camera camera){
            float radian = (float)Math.PI/180.0f;
            radian *= 10.0f*deltaSeconds;
            // Matrix4x4 rotationAroundY = Matrix4x4.CreateRotationY(radian);
            Quaternion rotationAroundY = Quaternion.CreateFromAxisAngle(Vector3.UnitY,radian);
            var worldCubeTrans = new Vector4(_worldTransCube.Translation, 1.0f);
            var cubeToCamera = Camera.Position - worldCubeTrans;
            var cameraTranslation = Vector4.Transform(cubeToCamera,rotationAroundY);
            Camera.Position = cameraTranslation;
            Camera.LookDirection = -Vector4.Normalize(cubeToCamera); // not interpolating, might be choppy
        }

        override protected void CreateResources()
        {

            _cameraProjViewBuffer = _factory.CreateBuffer(new BufferDescription(Camera.SizeInBytes,BufferUsage.UniformBuffer | BufferUsage.Dynamic));

            ResourceLayoutElementDescription resourceLayoutElementDescription = new ResourceLayoutElementDescription("projView",ResourceKind.UniformBuffer,ShaderStages.Vertex);
            ResourceLayoutElementDescription[] resourceLayoutElementDescriptions = {resourceLayoutElementDescription};
            ResourceLayoutDescription resourceLayoutDescription = new ResourceLayoutDescription(resourceLayoutElementDescriptions);
            BindableResource[] bindableResources = new BindableResource[]{_cameraProjViewBuffer};

            _resourceLayout = _factory.CreateResourceLayout(resourceLayoutDescription);
            ResourceSetDescription resourceSetDescription = new ResourceSetDescription(_resourceLayout,bindableResources);
            
            _cameraResourceSet = _factory.CreateResourceSet(resourceSetDescription);

            ImageSharpTexture NameImage = new ImageSharpTexture(Path.Combine(AppContext.BaseDirectory, "Textures", "Name.png"));
            Texture cubeTexture = NameImage.CreateDeviceTexture(GraphicsDevice, _factory);
            TextureView cubeTextureView = _factory.CreateTextureView(cubeTexture);

            ResourceLayout textureLayout = _factory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("Texture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("Sampler", ResourceKind.Sampler, ShaderStages.Fragment)));

            _textureResourceSet = _factory.CreateResourceSet(new ResourceSetDescription(
                textureLayout,
                cubeTextureView,
                GraphicsDevice.LinearSampler));

            Mesh<VertexPositionTexture> texturedCube 
                = GeometryFactory.GenerateTexturedCube(false);

            ushort[] cubeIndicies = GeometryFactory.generateCubeIndicies_TriangleList_CW();

            // declare (VBO) buffers
            _vertexBuffer 
                = _factory.CreateBuffer(new BufferDescription(texturedCube.Vertices.LengthUnsigned() * VertexPositionTexture.SizeInBytes, BufferUsage.VertexBuffer));
            _indexBuffer 
                = _factory.CreateBuffer(new BufferDescription(cubeIndicies.LengthUnsigned()*sizeof(ushort),BufferUsage.IndexBuffer));

            // fill buffers with data
            GraphicsDevice.UpdateBuffer(_vertexBuffer,0,texturedCube.Vertices);
            GraphicsDevice.UpdateBuffer(_indexBuffer,0,cubeIndicies);
            //graphicsDevice.UpdateBuffer(_cameraProjViewBuffer,0,camera.ViewMatrix);
            //graphicsDevice.UpdateBuffer(_cameraProjViewBuffer,64,camera.ProjectionMatrix);

            VertexLayoutDescription vertexLayout 
                = new VertexLayoutDescription(
                    new VertexElementDescription("Position",VertexElementSemantic.Position,VertexElementFormat.Float3),
                    new VertexElementDescription("UV",VertexElementSemantic.TextureCoordinate,VertexElementFormat.Float2)
                );

            _vertexShader = IO.LoadShader("Texture",ShaderStages.Vertex,GraphicsDevice);
            _fragmentShader = IO.LoadShader("Texture",ShaderStages.Fragment,GraphicsDevice);

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
                ResourceLayouts = new ResourceLayout[] {_resourceLayout,textureLayout},
                ShaderSet = new ShaderSetDescription(
                    vertexLayouts: new VertexLayoutDescription[] {vertexLayout},
                    shaders: new Shader[] {_vertexShader,_fragmentShader}
                ),
                Outputs = GraphicsDevice.SwapchainFramebuffer.OutputDescription
            };

            _pipeline = _factory.CreateGraphicsPipeline(pipelineDescription);

        }

        override protected void BuildCommandList(){

            _commandList.Begin();
            _commandList.SetFramebuffer(GraphicsDevice.SwapchainFramebuffer);
            _commandList.SetPipeline(_pipeline);
            _commandList.SetFullViewports();
            _commandList.ClearColorTarget(0,RgbaFloat.White);
            _commandList.ClearDepthStencil(1f);
            _commandList.SetVertexBuffer(0,_vertexBuffer);
            _commandList.SetIndexBuffer(_indexBuffer,IndexFormat.UInt16);
            _commandList.UpdateBuffer(_cameraProjViewBuffer,0,Camera.ViewMatrix);
            _commandList.UpdateBuffer(_cameraProjViewBuffer,64,Camera.ProjectionMatrix);
            _commandList.UpdateBuffer(_cameraProjViewBuffer,128,_worldTransCube);
            _commandList.SetGraphicsResourceSet(0,_cameraResourceSet); // Always after SetPipeline
            _commandList.SetGraphicsResourceSet(1,_textureResourceSet); // Always after SetPipeline
            _commandList.DrawIndexed(
                indexCount: 36,
                instanceCount: 1,
                indexStart: 0,
                vertexOffset: 0,
                instanceStart: 0
            );
            _commandList.End();

        }        
    }
}
