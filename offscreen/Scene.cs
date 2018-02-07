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

namespace textured_cube
{
    public class Scene : Renderable
    {
        private CommandList _commandList;
        private Framebuffer _offScreenFBO;
        private ResourceFactory _factory;

        private DeviceBuffer _vertexBufferCube;
        private DeviceBuffer _indexBufferCube;
        private Shader _vertexShaderCube;
        private Shader _fragmentShaderCube;
        private Pipeline _pipelineCube;

        private DeviceBuffer _vertexBufferQuad;
        private DeviceBuffer _indexBufferQuad;
        private Shader _vertexShaderQuad;
        private Shader _fragmentShaderQuad;
        private Pipeline _pipelineQuad;

        private ResourceSet _textureResourceSet;

        private DeviceBuffer _cameraProjViewBuffer;
        private ResourceSet _uniformCameraResourceSet;
        private ResourceLayout _uniformCameraResourceLayout;

        private DeviceBuffer _worldTransformBuffer;
        private ResourceSet _worldTransformResourceSet;
        private ResourceLayout _worldTransformResourceLayout;

        override protected List<IDisposable> CreateResources()
        {
            _factory = _graphicsDevice.ResourceFactory;

            _commandList = _factory.CreateCommandList();

            List<IDisposable> resources = new List<IDisposable>(){_commandList};

            resources.AddRange(createCameraUniform());

            resources.AddRange(createCubeResources());

            resources.AddRange(createQuadResources());

            return resources;

        }

        private List<IDisposable> createCameraUniform(){

            _cameraProjViewBuffer = _factory.CreateBuffer(new BufferDescription(128,BufferUsage.UniformBuffer | BufferUsage.Dynamic));

            ResourceLayoutElementDescription resourceLayoutElementDescription = new ResourceLayoutElementDescription("projView",ResourceKind.UniformBuffer,ShaderStages.Vertex);
            ResourceLayoutElementDescription[] resourceLayoutElementDescriptions = {resourceLayoutElementDescription};
            ResourceLayoutDescription resourceLayoutDescription = new ResourceLayoutDescription(resourceLayoutElementDescriptions);
            BindableResource[] bindableResources = new BindableResource[]{_cameraProjViewBuffer};

            _uniformCameraResourceLayout = _factory.CreateResourceLayout(resourceLayoutDescription);
            ResourceSetDescription resourceSetDescription = new ResourceSetDescription(_uniformCameraResourceLayout,bindableResources);
            
            _uniformCameraResourceSet = _factory.CreateResourceSet(resourceSetDescription);

            _graphicsDevice.UpdateBuffer(_cameraProjViewBuffer,0,_camera.ViewMatrix);
            _graphicsDevice.UpdateBuffer(_cameraProjViewBuffer,64,_camera.ProjectionMatrix);

            return new List<IDisposable>()
            {
                _cameraProjViewBuffer,
                _uniformCameraResourceLayout,
                _uniformCameraResourceSet

            };
        }

        private List<IDisposable> createCubeResources(){

            ImageSharpTexture NameImage = new ImageSharpTexture(Path.Combine(AppContext.BaseDirectory, "Textures", "Name.png"));
            Texture cubeTexture = NameImage.CreateDeviceTexture(_graphicsDevice, _factory);
            TextureView cubeTextureView = _factory.CreateTextureView(cubeTexture);

            ResourceLayout textureLayout = _factory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("CubeTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("CubeSampler", ResourceKind.Sampler, ShaderStages.Fragment)));

            _textureResourceSet = _factory.CreateResourceSet(new ResourceSetDescription(
                textureLayout,
                cubeTextureView,
                _graphicsDevice.LinearSampler));

            TexturedCube texturedCube 
                = GeometryFactory.generateTexturedCube();

            ushort[] cubeIndicies = GeometryFactory.generateCubeIndicies_TriangleList_CW();

            // declare (VBO) buffers
            _vertexBufferCube 
                = _factory.CreateBuffer(new BufferDescription(texturedCube.vertices.LengthUnsigned() * VertexPositionTexture.SizeInBytes, BufferUsage.VertexBuffer));
            _indexBufferCube 
                = _factory.CreateBuffer(new BufferDescription(cubeIndicies.LengthUnsigned()*sizeof(ushort),BufferUsage.IndexBuffer));

            // fill buffers with data
            _graphicsDevice.UpdateBuffer(_vertexBufferCube,0,texturedCube.vertices);
            _graphicsDevice.UpdateBuffer(_indexBufferCube,0,cubeIndicies);

            VertexLayoutDescription vertexLayout 
                = new VertexLayoutDescription(
                    new VertexElementDescription("Position",VertexElementSemantic.Position,VertexElementFormat.Float3),
                    new VertexElementDescription("UV",VertexElementSemantic.TextureCoordinate,VertexElementFormat.Float2)
                );

            _vertexShaderCube = IO.LoadShader("Cube",ShaderStages.Vertex,_graphicsDevice);
            _fragmentShaderCube = IO.LoadShader("Cube",ShaderStages.Fragment,_graphicsDevice);

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
                ResourceLayouts = new ResourceLayout[] {_uniformCameraResourceLayout,textureLayout},
                ShaderSet = new ShaderSetDescription(
                    vertexLayouts: new VertexLayoutDescription[] {vertexLayout},
                    shaders: new Shader[] {_vertexShaderCube,_fragmentShaderCube}
                ),
                Outputs = _graphicsDevice.SwapchainFramebuffer.OutputDescription
            };

            _pipelineCube = _factory.CreateGraphicsPipeline(pipelineDescription);

            return new List<IDisposable>()
            {
                _vertexBufferCube,
                _indexBufferCube,
                _textureResourceSet,
                _vertexShaderCube,
                _fragmentShaderCube,
                _pipelineCube
            };
        }

        private List<IDisposable> createQuadResources(){

            ColouredQuad quad = GeometryFactory.generateColouredQuad(RgbaFloat.Red, RgbaFloat.Blue,RgbaFloat.Green,RgbaFloat.Orange);
            ushort[] quadIndicies = GeometryFactory.generateQuadIndicies_TriangleStrip_CW();

            _vertexBufferQuad = _factory.CreateBuffer(new BufferDescription(quad.vertecies.LengthUnsigned()* VertexPositionColour.SizeInBytes, BufferUsage.VertexBuffer));
            _indexBufferQuad = _factory.CreateBuffer(new BufferDescription(quadIndicies.LengthUnsigned()* sizeof(ushort), BufferUsage.IndexBuffer));

            _graphicsDevice.UpdateBuffer(_vertexBufferQuad,0,quad.vertecies);
            _graphicsDevice.UpdateBuffer(_indexBufferQuad,0,quadIndicies);

            VertexLayoutDescription vertexLayout 
                = new VertexLayoutDescription(
                    new VertexElementDescription("Position",VertexElementSemantic.Position,VertexElementFormat.Float2),
                    new VertexElementDescription("Colour",VertexElementSemantic.Color,VertexElementFormat.Float4)
                );

            _vertexShaderQuad = IO.LoadShader("QuadColour",ShaderStages.Vertex,_graphicsDevice);
            _fragmentShaderQuad = IO.LoadShader("QuadColour",ShaderStages.Fragment,_graphicsDevice);

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
                ResourceLayouts = new ResourceLayout[] {_uniformCameraResourceLayout},
                ShaderSet = new ShaderSetDescription(
                    vertexLayouts: new VertexLayoutDescription[] {vertexLayout},
                    shaders: new Shader[] {_vertexShaderQuad,_fragmentShaderQuad}
                ),
                Outputs = _graphicsDevice.SwapchainFramebuffer.OutputDescription
            };

            _pipelineQuad = _factory.CreateGraphicsPipeline(pipelineDescription);

            return new List<IDisposable>()
            {
                _vertexBufferQuad,
                _indexBufferQuad,
                _vertexShaderQuad,
                _fragmentShaderQuad,
                _pipelineQuad
            };



        }

        override protected void Draw(){
            _commandList.Begin();
            _commandList.SetFramebuffer(_graphicsDevice.SwapchainFramebuffer);
            _commandList.SetFullViewports();
            _commandList.ClearColorTarget(0,RgbaFloat.White);
            _commandList.ClearDepthStencil(1f);

            _commandList.SetPipeline(_pipelineCube);
            _commandList.SetVertexBuffer(0,_vertexBufferCube);
            _commandList.SetIndexBuffer(_indexBufferCube,IndexFormat.UInt16);
            _commandList.UpdateBuffer(_cameraProjViewBuffer,0,_camera.ViewMatrix);
            _commandList.UpdateBuffer(_cameraProjViewBuffer,64,_camera.ProjectionMatrix);
            _commandList.SetGraphicsResourceSet(0,_uniformCameraResourceSet); // Always after SetPipeline
            _commandList.SetGraphicsResourceSet(1,_textureResourceSet); // Always after SetPipeline
            _commandList.DrawIndexed(
                indexCount: 36,
                instanceCount: 1,
                indexStart: 0,
                vertexOffset: 0,
                instanceStart: 0
            );

            _commandList.SetPipeline(_pipelineQuad);
            _commandList.SetVertexBuffer(0,_vertexBufferQuad);
            _commandList.SetIndexBuffer(_indexBufferQuad,IndexFormat.UInt16);
            _commandList.UpdateBuffer(_cameraProjViewBuffer,0,_camera.ViewMatrix);
            _commandList.UpdateBuffer(_cameraProjViewBuffer,64,_camera.ProjectionMatrix);
            _commandList.SetGraphicsResourceSet(0,_uniformCameraResourceSet);
            _commandList.DrawIndexed(
                indexCount: 4,
                instanceCount: 1,
                indexStart: 0,
                vertexOffset: 0,
                instanceStart: 0
            );
            
            _commandList.End();
            _graphicsDevice.SubmitCommands(_commandList);
            _graphicsDevice.SwapBuffers();
        }
    }
}
