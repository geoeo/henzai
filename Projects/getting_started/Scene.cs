using System;
using System.Numerics;
using System.Collections.Generic;
using Veldrid;
using Veldrid.StartupUtilities;
using Veldrid.Sdl2;
using Veldrid.OpenGL;
using Henzai;
using Henzai.Extensions;
using Henzai.Geometry;
using Henzai.Runtime;

namespace Henzai.Examples
{
    // https://mellinoe.github.io/veldrid-docs/articles/getting-started/getting-started-part1.html
    internal class Scene : Renderable
    {
        private DeviceBuffer _vertexBuffer;
        private DeviceBuffer _indexBuffer;
        private DeviceBuffer _xOffsetBuffer;
        private Shader _vertexShader;
        private Shader _fragmentShader;
        private Pipeline _pipeline;
        private DeviceBuffer _cameraProjViewBuffer;
        private ResourceSet _resourceSet;
        private ResourceLayout _resourceLayout;

        public Scene(string title,Resolution windowSize, GraphicsDeviceOptions graphicsDeviceOptions, RenderOptions renderOptions)
            : base(title,windowSize,graphicsDeviceOptions,renderOptions){
        }

        override protected void FormatResourcesForRuntime(){}

        override protected void CreateResources()
        {

            _cameraProjViewBuffer = _factory.CreateBuffer(new BufferDescription(128,BufferUsage.UniformBuffer | BufferUsage.Dynamic));

            ResourceLayoutElementDescription resourceLayoutElementDescription = new ResourceLayoutElementDescription("projView",ResourceKind.UniformBuffer,ShaderStages.Vertex);
            ResourceLayoutElementDescription[] resourceLayoutElementDescriptions = {resourceLayoutElementDescription};
            ResourceLayoutDescription resourceLayoutDescription = new ResourceLayoutDescription(resourceLayoutElementDescriptions);
            BindableResource[] bindableResources = new BindableResource[]{_cameraProjViewBuffer};

            _resourceLayout = _factory.CreateResourceLayout(resourceLayoutDescription);
            ResourceSetDescription resourceSetDescription = new ResourceSetDescription(_resourceLayout,bindableResources);
            
            _resourceSet = _factory.CreateResourceSet(resourceSetDescription);

           Mesh<VertexPositionNDCColor> coloredQuad 
                = GeometryFactory.generateColorQuadNDC(RgbaFloat.Red,RgbaFloat.Green,RgbaFloat.Blue,RgbaFloat.Yellow);

            ushort[] quadIndicies = GeometryFactory.generateQuadIndicies_TriangleStrip_CW();

            float[] _xOffset = {-2f,2f};

            // declare (VBO) buffers
            _vertexBuffer 
                = _factory.CreateBuffer(new BufferDescription(coloredQuad.vertices.LengthUnsigned() * VertexPositionNDCColor.SizeInBytes, BufferUsage.VertexBuffer));
            _indexBuffer 
                = _factory.CreateBuffer(new BufferDescription(quadIndicies.LengthUnsigned()*sizeof(ushort),BufferUsage.IndexBuffer));
            _xOffsetBuffer
                = _factory.CreateBuffer(new BufferDescription(_xOffset.LengthUnsigned()*sizeof(float),BufferUsage.VertexBuffer));

            // fill buffers with data
            GraphicsDevice.UpdateBuffer(_vertexBuffer,0,coloredQuad.vertices);
            GraphicsDevice.UpdateBuffer(_indexBuffer,0,quadIndicies);
            GraphicsDevice.UpdateBuffer(_xOffsetBuffer,0,_xOffset);
            GraphicsDevice.UpdateBuffer(_cameraProjViewBuffer,0,Camera.ViewMatrix);
            GraphicsDevice.UpdateBuffer(_cameraProjViewBuffer,64,Camera.ProjectionMatrix);

            VertexLayoutDescription vertexLayout 
                = new VertexLayoutDescription(
                    new VertexElementDescription("Position",VertexElementSemantic.Position,VertexElementFormat.Float2),
                    new VertexElementDescription("Color",VertexElementSemantic.Color,VertexElementFormat.Float4)
                );
            
            VertexElementDescription vertexElementPerInstance
                = new VertexElementDescription("xOff",VertexElementSemantic.Position,VertexElementFormat.Float1);

            VertexLayoutDescription vertexLayoutPerInstance 
                = new VertexLayoutDescription(
                    stride: 4,
                    instanceStepRate: 1,
                    elements: new VertexElementDescription[] {vertexElementPerInstance}
                );

            _vertexShader = IO.LoadShader("OffsetXNDCColor",ShaderStages.Vertex,GraphicsDevice);
            _fragmentShader = IO.LoadShader("Color",ShaderStages.Fragment,GraphicsDevice);

            GraphicsPipelineDescription pipelineDescription = new GraphicsPipelineDescription(){
                BlendState = BlendStateDescription.SingleOverrideBlend,
                DepthStencilState = new DepthStencilStateDescription(
                    depthTestEnabled: true,
                    depthWriteEnabled: true,
                    comparisonKind: ComparisonKind.LessEqual),
                RasterizerState = new RasterizerStateDescription(
                    cullMode: FaceCullMode.Back,
                    fillMode: PolygonFillMode.Solid,
                    frontFace: FrontFace.Clockwise,
                    depthClipEnabled: true,
                    scissorTestEnabled: false
                ),
                PrimitiveTopology = PrimitiveTopology.TriangleStrip,
                ResourceLayouts = new ResourceLayout[] {_resourceLayout},
                ShaderSet = new ShaderSetDescription(
                    vertexLayouts: new VertexLayoutDescription[] {vertexLayout,vertexLayoutPerInstance},
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
            _commandList.SetVertexBuffer(0,_vertexBuffer);
            _commandList.SetIndexBuffer(_indexBuffer,IndexFormat.UInt16);
            _commandList.SetVertexBuffer(1,_xOffsetBuffer);
            _commandList.UpdateBuffer(_cameraProjViewBuffer,0,Camera.ViewMatrix);
            _commandList.UpdateBuffer(_cameraProjViewBuffer,64,Camera.ProjectionMatrix);
            _commandList.SetGraphicsResourceSet(0,_resourceSet); // Always after SetPipeline
            _commandList.DrawIndexed(
                indexCount: 4,
                instanceCount: 2,
                indexStart: 0,
                vertexOffset: 0,
                instanceStart: 0
            );
            _commandList.End();

        }

        override protected void Draw(){
            GraphicsDevice.SubmitCommands(_commandList);
        }
        
    }
}
