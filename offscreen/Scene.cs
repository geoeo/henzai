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
        private ResourceSet _uniformCameraresourceSet;
        private ResourceLayout _uniformCameraResourceLayout;

        override protected void CreateResources()
        {
            ResourceFactory _factory = _graphicsDevice.ResourceFactory;

            _cameraProjViewBuffer = _factory.CreateBuffer(new BufferDescription(128,BufferUsage.UniformBuffer | BufferUsage.Dynamic));

            ResourceLayoutElementDescription resourceLayoutElementDescription = new ResourceLayoutElementDescription("projView",ResourceKind.UniformBuffer,ShaderStages.Vertex);
            ResourceLayoutElementDescription[] resourceLayoutElementDescriptions = {resourceLayoutElementDescription};
            ResourceLayoutDescription resourceLayoutDescription = new ResourceLayoutDescription(resourceLayoutElementDescriptions);
            BindableResource[] bindableResources = new BindableResource[]{_cameraProjViewBuffer};

            _uniformCameraResourceLayout = _factory.CreateResourceLayout(resourceLayoutDescription);
            ResourceSetDescription resourceSetDescription = new ResourceSetDescription(_uniformCameraResourceLayout,bindableResources);
            
            _uniformCameraresourceSet = _factory.CreateResourceSet(resourceSetDescription);

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

            ushort[] quadIndicies = GeometryFactory.generateCubeIndicies_TriangleList_CW();

            // declare (VBO) buffers
            _vertexBufferCube 
                = _factory.CreateBuffer(new BufferDescription(texturedCube.vertices.LengthUnsigned() * VertexPositionTexture.SizeInBytes, BufferUsage.VertexBuffer));
            _indexBufferCube 
                = _factory.CreateBuffer(new BufferDescription(quadIndicies.LengthUnsigned()*sizeof(ushort),BufferUsage.IndexBuffer));

            // fill buffers with data
            _graphicsDevice.UpdateBuffer(_vertexBufferCube,0,texturedCube.vertices);
            _graphicsDevice.UpdateBuffer(_indexBufferCube,0,quadIndicies);
            _graphicsDevice.UpdateBuffer(_cameraProjViewBuffer,0,_camera.ViewMatrix);
            _graphicsDevice.UpdateBuffer(_cameraProjViewBuffer,64,_camera.ProjectionMatrix);

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

            _commandList = _factory.CreateCommandList();

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
            _commandList.SetGraphicsResourceSet(0,_uniformCameraresourceSet); // Always after SetPipeline
            _commandList.SetGraphicsResourceSet(1,_textureResourceSet); // Always after SetPipeline
            _commandList.DrawIndexed(
                indexCount: 36,
                instanceCount: 1,
                indexStart: 0,
                vertexOffset: 0,
                instanceStart: 0
            );
            
            _commandList.End();
            _graphicsDevice.SubmitCommands(_commandList);
            _graphicsDevice.SwapBuffers();
        }

        override protected void DisposeResources(){
            _pipelineCube.Dispose();
            _vertexShaderCube.Dispose();
            _fragmentShaderCube.Dispose();
            _commandList.Dispose();
            _vertexBufferCube.Dispose();
            _indexBufferCube.Dispose();
            _cameraProjViewBuffer.Dispose();
            _graphicsDevice.Dispose();
            _uniformCameraResourceLayout.Dispose();
            _uniformCameraresourceSet.Dispose();
        }
    }
}
