using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Numerics;
using ImGuiNET;
using Veldrid;
using Veldrid.Sdl2;
using Henzai.Runtime;


namespace Henzai.UI
{
    // TODO: Refactor into explicity UI Renderable or a explicit ChildRenderable
    /// See Veldrid.ImGuiRenderer
    public abstract class UserInterface : Renderable
    {
        private readonly Assembly _assembly;
        private DeviceBuffer _vertexBuffer;
        private DeviceBuffer _indexBuffer;
        private DeviceBuffer _projMatrixBuffer;
        private Texture _fontTexture;
        private Shader _vertexShader;
        private Shader _fragmentShader;
        private ResourceLayout _layout;
        private ResourceLayout _textureLayout;
        private Pipeline _pipeline;
        private ResourceSet _mainResourceSet;
        private ResourceSet _fontTextureResourceSet;
        private IntPtr _fontAtlasID = (IntPtr)1;

        private bool _controlDown;
        private bool _shiftDown;
        private bool _altDown;

        protected bool backendCallback = false;
        //TODO: check this
        protected GraphicsBackend newGraphicsBackend = GraphicsBackend.OpenGL;
        public event Action<GraphicsBackend> changeBackendAction;


        public UserInterface(GraphicsDevice graphicsDevice, Sdl2Window contextWindow) : base(graphicsDevice,contextWindow){
            _assembly = typeof(UserInterface).GetTypeInfo().Assembly;

            IntPtr context = ImGui.CreateContext();
            ImGui.SetCurrentContext(context);

            ImGui.GetIO().Fonts.AddFontDefault();
            _isChild = true;
        }

        public void UpdateImGui(float deltaSeconds, InputSnapshot inputSnapshot){

            var io = ImGui.GetIO();
            io.DeltaTime = deltaSeconds;
            io.DisplaySize = new System.Numerics.Vector2(ContextWindow.Width, ContextWindow.Height);
            io.DisplayFramebufferScale = new System.Numerics.Vector2(ContextWindow.Width / ContextWindow.Height);
            
            ImGui.NewFrame();
            UpdateImGuiInput(inputSnapshot);
            SubmitImGUILayout(deltaSeconds);
            ImGui.Render();

            if(backendCallback)
                changeBackendAction?.Invoke(newGraphicsBackend); 
        }

        abstract protected unsafe void SubmitImGUILayout(float secondsPerFrame);

        //TODO: Add color space handling 
        override protected void CreateResources(){

            ResourceFactory factory = GraphicsDevice.ResourceFactory;
            _vertexBuffer = factory.CreateBuffer(new BufferDescription(10000, BufferUsage.VertexBuffer | BufferUsage.Dynamic));
            _vertexBuffer.Name = "ImGui.NET Vertex Buffer";
            _indexBuffer = factory.CreateBuffer(new BufferDescription(2000, BufferUsage.IndexBuffer | BufferUsage.Dynamic));
            _indexBuffer.Name = "ImGui.NET Index Buffer";

            _projMatrixBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            _projMatrixBuffer.Name = "ImGui.NET Projection Buffer";

            byte[] vertexShaderBytes = LoadEmbeddedShaderCode(GraphicsDevice.ResourceFactory, "imgui-vertex");
            byte[] fragmentShaderBytes = LoadEmbeddedShaderCode(GraphicsDevice.ResourceFactory, "imgui-frag");
            _vertexShader = factory.CreateShader(new ShaderDescription(ShaderStages.Vertex, vertexShaderBytes, GraphicsDevice.BackendType == GraphicsBackend.Vulkan ? "main" : "VS"));
            _fragmentShader = factory.CreateShader(new ShaderDescription(ShaderStages.Fragment, fragmentShaderBytes, GraphicsDevice.BackendType == GraphicsBackend.Vulkan ? "main" : "FS"));

            VertexLayoutDescription[] vertexLayouts = new VertexLayoutDescription[]
            {
                new VertexLayoutDescription(
                    new VertexElementDescription("in_position", VertexElementSemantic.Position, VertexElementFormat.Float2),
                    new VertexElementDescription("in_texCoord", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                    new VertexElementDescription("in_color", VertexElementSemantic.Color, VertexElementFormat.Byte4_Norm))
            };

            _layout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("ProjectionMatrixBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                new ResourceLayoutElementDescription("MainSampler", ResourceKind.Sampler, ShaderStages.Fragment)));
            _textureLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("MainTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment)));

            GraphicsPipelineDescription pd = new GraphicsPipelineDescription(
                BlendStateDescription.SingleAlphaBlend,
                new DepthStencilStateDescription(false, false, ComparisonKind.Always),
                new RasterizerStateDescription(FaceCullMode.None, PolygonFillMode.Solid, FrontFace.Clockwise, true, true),
                PrimitiveTopology.TriangleList,
                new ShaderSetDescription(
                    vertexLayouts,
                    new[] { _vertexShader, _fragmentShader },
                    new[]
                    {
                        new SpecializationConstant(0, GraphicsDevice.IsClipSpaceYInverted),
                        new SpecializationConstant(1, true),
                    }),
                new ResourceLayout[] { _layout, _textureLayout },
                GraphicsDevice.SwapchainFramebuffer.OutputDescription,
                ResourceBindingModel.Improved);
            _pipeline = factory.CreateGraphicsPipeline(ref pd);

            _mainResourceSet = factory.CreateResourceSet(new ResourceSetDescription(_layout,
                _projMatrixBuffer,
                GraphicsDevice.PointSampler));

            RecreateFontDeviceTexture(GraphicsDevice);

        }

        override protected void BuildCommandList(){
            _commandList.Begin();
            _commandList.SetFramebuffer(GraphicsDevice.SwapchainFramebuffer);

            RenderImDrawData(ImGui.GetDrawData(), GraphicsDevice, _commandList);
            
            _commandList.End();
        }

        override protected void Draw(){
            GraphicsDevice.SubmitCommands(_commandList);
        }

        private unsafe void RenderImDrawData(ImDrawDataPtr draw_data, GraphicsDevice gd, CommandList cl){
            var io = ImGui.GetIO();
            uint vertexOffsetInVertices = 0;
            uint indexOffsetInElements = 0;

            if (draw_data.CmdListsCount == 0)
            {
                return;
            }

            uint totalVBSize = (uint)(draw_data.TotalVtxCount * sizeof(ImDrawVert));
            if (totalVBSize > _vertexBuffer.SizeInBytes)
            {
                _vertexBuffer.Dispose();
                _vertexBuffer = gd.ResourceFactory.CreateBuffer(new BufferDescription((uint)(totalVBSize * 1.5f), BufferUsage.VertexBuffer | BufferUsage.Dynamic));
            }

            uint totalIBSize = (uint)(draw_data.TotalIdxCount * sizeof(ushort));
            if (totalIBSize > _indexBuffer.SizeInBytes)
            {
                _indexBuffer.Dispose();
                _indexBuffer = gd.ResourceFactory.CreateBuffer(new BufferDescription((uint)(totalIBSize * 1.5f), BufferUsage.IndexBuffer | BufferUsage.Dynamic));
            }

            for (int i = 0; i < draw_data.CmdListsCount; i++)
            {
                ImDrawListPtr cmd_list = draw_data.CmdListsRange[i];

                // TODO: we seem to write the wrong color (if at all?)
                cl.UpdateBuffer(
                    _vertexBuffer,
                    vertexOffsetInVertices * (uint)sizeof(ImDrawVert),
                    cmd_list.VtxBuffer.Data,
                    (uint)(cmd_list.VtxBuffer.Size * sizeof(ImDrawVert)));

                cl.UpdateBuffer(
                    _indexBuffer,
                    indexOffsetInElements * sizeof(ushort),
                    cmd_list.IdxBuffer.Data,
                    (uint)(cmd_list.IdxBuffer.Size * sizeof(ushort)));

                vertexOffsetInVertices += (uint)cmd_list.VtxBuffer.Size;
                indexOffsetInElements += (uint)cmd_list.IdxBuffer.Size;
            }

            // Setup orthographic projection matrix into our constant buffer
            Matrix4x4 mvp = Matrix4x4.CreateOrthographicOffCenter(
                0f,
                io.DisplaySize.X,
                io.DisplaySize.Y,
                0.0f,
                -1.0f,
                1.0f);

            gd.UpdateBuffer(_projMatrixBuffer, 0, ref mvp);
            

            cl.SetVertexBuffer(0, _vertexBuffer);
            cl.SetIndexBuffer(_indexBuffer, IndexFormat.UInt16);
            cl.SetPipeline(_pipeline);
            cl.SetGraphicsResourceSet(0, _mainResourceSet);

            draw_data.ScaleClipRects(ImGui.GetIO().DisplayFramebufferScale);

            // Render command lists
            int vtx_offset = 0;
            int idx_offset = 0;
            for (int n = 0; n < draw_data.CmdListsCount; n++)
            {
                ImDrawListPtr cmd_list = draw_data.CmdListsRange[n];
                for (int cmd_i = 0; cmd_i < cmd_list.CmdBuffer.Size; cmd_i++)
                {
                    ImDrawCmdPtr pcmd = cmd_list.CmdBuffer[cmd_i];
                    if (pcmd.UserCallback != IntPtr.Zero)
                    {
                        throw new NotImplementedException();
                    }
                    else
                    {
                        if (pcmd.TextureId != IntPtr.Zero)
                        {
                            if (pcmd.TextureId == _fontAtlasID)
                            {
                                cl.SetGraphicsResourceSet(1, _fontTextureResourceSet);
                            }
                            else
                            {
                                throw new NotImplementedException("TODO: Implement Custom Resources");
                            }
                        }

                        cl.SetScissorRect(
                            0,
                            (uint)pcmd.ClipRect.X,
                            (uint)pcmd.ClipRect.Y,
                            (uint)(pcmd.ClipRect.Z - pcmd.ClipRect.X),
                            (uint)(pcmd.ClipRect.W - pcmd.ClipRect.Y));

                        cl.DrawIndexed(pcmd.ElemCount, 1, (uint)idx_offset, vtx_offset, 0);
                    }

                    idx_offset += (int)pcmd.ElemCount;
                }
                vtx_offset += cmd_list.VtxBuffer.Size;
            }
        }

        /// <summary>
        /// Recreates the device texture used to render text.
        /// </summary>
        public unsafe void RecreateFontDeviceTexture(GraphicsDevice gd)
        {
            ImGuiIOPtr io = ImGui.GetIO();
            // Build
            io.Fonts.GetTexDataAsRGBA32(out byte* pixels, out int width, out int height, out int bytesPerPixel);

            // Store our identifier
            io.Fonts.SetTexID(_fontAtlasID);

            _fontTexture?.Dispose();
            _fontTexture = gd.ResourceFactory.CreateTexture(TextureDescription.Texture2D(
                (uint)width,
                (uint)height,
                1,
                1,
                PixelFormat.R8_G8_B8_A8_UNorm,
                TextureUsage.Sampled));
            _fontTexture.Name = "ImGui.NET Font Texture";
            gd.UpdateTexture(
                _fontTexture,
                (IntPtr)pixels,
                (uint)(bytesPerPixel * width * height),
                0,
                0,
                0,
                (uint)width,
                (uint)height,
                1,
                0,
                0);

            _fontTextureResourceSet?.Dispose();
            _fontTextureResourceSet = gd.ResourceFactory.CreateResourceSet(new ResourceSetDescription(_textureLayout, _fontTexture));

            io.Fonts.ClearTexData();
        }

        private unsafe void UpdateImGuiInput(InputSnapshot snapshot)
        {
            ImGuiIOPtr io = ImGui.GetIO();

            // Determine if any of the mouse buttons were pressed during this snapshot period, even if they are no longer held.
            bool leftPressed = false;
            bool middlePressed = false;
            bool rightPressed = false;
            for (int i = 0; i < snapshot.MouseEvents.Count; i++)
            {
                MouseEvent me = snapshot.MouseEvents[i];
                if (me.Down)
                {
                    switch (me.MouseButton)
                    {
                        case MouseButton.Left:
                            leftPressed = true;
                            break;
                        case MouseButton.Middle:
                            middlePressed = true;
                            break;
                        case MouseButton.Right:
                            rightPressed = true;
                            break;
                    }
                }
            }

            io.MouseDown[0] = leftPressed || snapshot.IsMouseDown(MouseButton.Left);
            io.MouseDown[1] = rightPressed || snapshot.IsMouseDown(MouseButton.Right);
            io.MouseDown[2] = middlePressed || snapshot.IsMouseDown(MouseButton.Middle);
            io.MousePos = snapshot.MousePosition;
            io.MouseWheel = snapshot.WheelDelta;

            IReadOnlyList<char> keyCharPresses = snapshot.KeyCharPresses;
            for (int i = 0; i < keyCharPresses.Count; i++)
            {
                char c = keyCharPresses[i];
                ImGui.GetIO().AddInputCharacter(c);
            }

            IReadOnlyList<KeyEvent> keyEvents = snapshot.KeyEvents;
            for (int i = 0; i < keyEvents.Count; i++)
            {
                KeyEvent keyEvent = keyEvents[i];
                io.KeysDown[(int)keyEvent.Key] = keyEvent.Down;
                if (keyEvent.Key == Key.ControlLeft)
                {
                    _controlDown = keyEvent.Down;
                }
                if (keyEvent.Key == Key.ShiftLeft)
                {
                    _shiftDown = keyEvent.Down;
                }
                if (keyEvent.Key == Key.AltLeft)
                {
                    _altDown = keyEvent.Down;
                }
            }

            io.KeyCtrl = _controlDown;
            io.KeyAlt = _altDown;
            io.KeyShift = _shiftDown;
        }

        private byte[] LoadEmbeddedShaderCode(ResourceFactory factory, string name)
        {
            switch (factory.BackendType)
            {
                case GraphicsBackend.Direct3D11:
                    {
                        string resourceName = name + ".hlsl.bytes";
                        return IO.GetEmbeddedResourceBytes(_assembly, resourceName);
                    }
                case GraphicsBackend.OpenGL:
                    {
                        string resourceName = name + ".glsl";
                        return IO.GetEmbeddedResourceBytes(_assembly,resourceName);
                    }
                case GraphicsBackend.Vulkan:
                    {
                        string resourceName = name + ".spv";
                        return IO.GetEmbeddedResourceBytes(_assembly,resourceName);
                    }
                case GraphicsBackend.Metal:
                    {
                        string resourceName = name + ".metallib";
                        return IO.GetEmbeddedResourceBytes(_assembly,resourceName);
                    }
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Frees all graphics resources used by the renderer.
        /// </summary>
        public override void Dispose()
        {
            _vertexBuffer.Dispose();
            _indexBuffer.Dispose();
            _projMatrixBuffer.Dispose();
            _fontTexture.Dispose();
            _vertexShader.Dispose();
            _fragmentShader.Dispose();
            _layout.Dispose();
            _textureLayout.Dispose();
            _pipeline.Dispose();
            _mainResourceSet.Dispose();
            _fontTextureResourceSet.Dispose();

            base.Dispose();

        }

    }
}