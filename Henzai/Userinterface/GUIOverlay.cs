using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Numerics;
using ImGuiNET;
using Veldrid;
using Veldrid.Sdl2;
using Henzai.Runtime;


namespace Henzai.UserInterface
{
    /// See Veldrid.ImGuiRenderer
    public class GUIOverlay : Renderable
    {

        private readonly Assembly _assembly;
        private DeviceBuffer _vertexBuffer;
        private DeviceBuffer _indexBuffer;
        private DeviceBuffer _projMatrixBuffer;
        private Texture _fontTexture;
        private TextureView _fontTextureView;
        private Shader _vertexShader;
        private Shader _fragmentShader;
        private ResourceLayout _layout;
        private ResourceLayout _textureLayout;
        private Pipeline _pipeline;
        private ResourceSet _mainResourceSet;
        private ResourceSet _fontTextureResourceSet;
        private IntPtr _fontAtlasID = (IntPtr)1;


        public GUIOverlay(GraphicsDevice graphicsDevice, Sdl2Window contextWindow) : base(graphicsDevice,contextWindow){
            _assembly = typeof(GUIOverlay).GetTypeInfo().Assembly;
            ImGui.GetIO().FontAtlas.AddDefaultFont();
        }


        public void SetOverlayFor(Renderable scene){
            scene.PreDraw += UpdateImGui;
            AddThisAsPostTo(scene);
        }

        public void UpdateImGui(float deltaSeconds){

            ImGuiNET.IO io = ImGui.GetIO();
            io.DeltaTime = deltaSeconds;
            io.DisplaySize = new System.Numerics.Vector2(ContextWindow.Width, ContextWindow.Height);
            io.DisplayFramebufferScale = new System.Numerics.Vector2(ContextWindow.Width / ContextWindow.Height);
            

            ImGui.NewFrame();
            SubmitImGUICommands(deltaSeconds);
            ImGui.Render();

        }

        override protected void CreateResources(){

            _vertexBuffer = _factory.CreateBuffer(new BufferDescription(10000, BufferUsage.VertexBuffer | BufferUsage.Dynamic));
            _vertexBuffer.Name = "ImGui.NET Vertex Buffer";
            _indexBuffer = _factory.CreateBuffer(new BufferDescription(2000, BufferUsage.IndexBuffer | BufferUsage.Dynamic));
            _indexBuffer.Name = "ImGui.NET Index Buffer";
    
            RecreateFontDeviceTexture(GraphicsDevice);

            _projMatrixBuffer = _factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            _projMatrixBuffer.Name = "ImGui.NET Projection Buffer";

            byte[] vertexShaderBytes = LoadEmbeddedShaderCode(GraphicsDevice.ResourceFactory, "imgui-vertex");
            byte[] fragmentShaderBytes = LoadEmbeddedShaderCode(GraphicsDevice.ResourceFactory, "imgui-frag");
            _vertexShader = _factory.CreateShader(new ShaderDescription(ShaderStages.Vertex, vertexShaderBytes, "VS"));
            _fragmentShader = _factory.CreateShader(new ShaderDescription(ShaderStages.Fragment, fragmentShaderBytes, "FS"));

            VertexLayoutDescription[] vertexLayouts = new VertexLayoutDescription[]
            {
                new VertexLayoutDescription(
                    new VertexElementDescription("in_position", VertexElementSemantic.Position, VertexElementFormat.Float2),
                    new VertexElementDescription("in_texCoord", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                    new VertexElementDescription("in_color", VertexElementSemantic.Color, VertexElementFormat.Byte4_Norm))
            };

            _layout = _factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("ProjectionMatrixBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                new ResourceLayoutElementDescription("MainSampler", ResourceKind.Sampler, ShaderStages.Fragment)));
            _textureLayout = _factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("MainTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment)));

            GraphicsPipelineDescription pd = new GraphicsPipelineDescription(
                BlendStateDescription.SingleAlphaBlend,
                new DepthStencilStateDescription(false, false, ComparisonKind.Always),
                new RasterizerStateDescription(FaceCullMode.None, PolygonFillMode.Solid, FrontFace.Clockwise, false, true),
                PrimitiveTopology.TriangleList,
                new ShaderSetDescription(vertexLayouts, new[] { _vertexShader, _fragmentShader }),
                new ResourceLayout[] { _layout, _textureLayout },
                GraphicsDevice.SwapchainFramebuffer.OutputDescription);
            _pipeline = _factory.CreateGraphicsPipeline(ref pd);

            _mainResourceSet = _factory.CreateResourceSet(new ResourceSetDescription(_layout,
                _projMatrixBuffer,
                GraphicsDevice.PointSampler));

            _fontTextureResourceSet = _factory.CreateResourceSet(new ResourceSetDescription(_textureLayout, _fontTextureView));

        }

        override protected void BuildCommandList(){
             _commandList.Begin();
            _commandList.SetFramebuffer(GraphicsDevice.SwapchainFramebuffer);
           // _commandList.SetFullViewports();
            // _commandList.ClearColorTarget(0,RgbaFloat.Red);
            //_commandList.ClearDepthStencil(1f);

            unsafe {
                RenderImGuiDrawData(ImGui.GetDrawData(), GraphicsDevice, _commandList);
            }
            _commandList.End();
        }

        override protected void Draw(){

            // _commandList.Begin();
            // _commandList.SetFramebuffer(graphicsDevice.SwapchainFramebuffer);

            // unsafe {
            //     RenderImGuiDrawData(ImGui.GetDrawData(), graphicsDevice, _commandList);
            // }
            // _commandList.End();

            GraphicsDevice.SubmitCommands(_commandList);

        }

        private unsafe void RenderImGuiDrawData(DrawData* drawData,GraphicsDevice graphicsDevice, CommandList commandList){
            uint vertexOffsetInVertices = 0;
            uint indexOffsetInElements = 0;

            if (drawData->CmdListsCount == 0)
            {
                return;
            }

            uint totalVBSize = (uint)(drawData->TotalVtxCount * sizeof(DrawVert));
            if (totalVBSize > _vertexBuffer.SizeInBytes)
            {
                graphicsDevice.DisposeWhenIdle(_vertexBuffer);
                _vertexBuffer = graphicsDevice.ResourceFactory.CreateBuffer(new BufferDescription((uint)(totalVBSize * 1.5f), BufferUsage.VertexBuffer | BufferUsage.Dynamic));
            }

            uint totalIBSize = (uint)(drawData->TotalIdxCount * sizeof(ushort));
            if (totalIBSize > _indexBuffer.SizeInBytes)
            {
                graphicsDevice.DisposeWhenIdle(_indexBuffer);
                _indexBuffer = graphicsDevice.ResourceFactory.CreateBuffer(new BufferDescription((uint)(totalIBSize * 1.5f), BufferUsage.IndexBuffer | BufferUsage.Dynamic));
            }

            MappedResource vbMap = graphicsDevice.Map(_vertexBuffer, MapMode.Write);
            MappedResource ibMap = graphicsDevice.Map(_indexBuffer, MapMode.Write);
            for (int i = 0; i < drawData->CmdListsCount; i++)
            {
                NativeDrawList* cmd_list = drawData->CmdLists[i];

                System.Runtime.CompilerServices.Unsafe.CopyBlock(
                    (byte*)vbMap.Data.ToPointer() + vertexOffsetInVertices * sizeof(DrawVert),
                    cmd_list->VtxBuffer.Data,
                    (uint)(cmd_list->VtxBuffer.Size * sizeof(DrawVert)));

                System.Runtime.CompilerServices.Unsafe.CopyBlock(
                    (byte*)ibMap.Data.ToPointer() + indexOffsetInElements * sizeof(ushort),
                    cmd_list->IdxBuffer.Data,
                    (uint)(cmd_list->IdxBuffer.Size * sizeof(ushort)));

                vertexOffsetInVertices += (uint)cmd_list->VtxBuffer.Size;
                indexOffsetInElements += (uint)cmd_list->IdxBuffer.Size;
            }
            graphicsDevice.Unmap(_vertexBuffer);
            graphicsDevice.Unmap(_indexBuffer);

            // Setup orthographic projection matrix into our constant buffer
            {
                var io = ImGui.GetIO();

                Matrix4x4 mvp = Matrix4x4.CreateOrthographicOffCenter(
                    0f,
                    io.DisplaySize.X,
                    io.DisplaySize.Y,
                    0.0f,
                    -1.0f,
                    1.0f);

                graphicsDevice.UpdateBuffer(_projMatrixBuffer, 0, ref mvp);
            }

            commandList.SetVertexBuffer(0, _vertexBuffer);
            commandList.SetIndexBuffer(_indexBuffer, IndexFormat.UInt16);
            commandList.SetPipeline(_pipeline);
            commandList.SetGraphicsResourceSet(0, _mainResourceSet);

            ImGui.ScaleClipRects(drawData, ImGui.GetIO().DisplayFramebufferScale);

            // Render command lists
            int vtx_offset = 0;
            int idx_offset = 0;
            for (int n = 0; n < drawData->CmdListsCount; n++)
            {
                NativeDrawList* cmd_list = drawData->CmdLists[n];
                for (int cmd_i = 0; cmd_i < cmd_list->CmdBuffer.Size; cmd_i++)
                {
                    DrawCmd* pcmd = &(((DrawCmd*)cmd_list->CmdBuffer.Data)[cmd_i]);
                    if (pcmd->UserCallback != IntPtr.Zero)
                    {
                        throw new NotImplementedException("TODO Call function");
                    }
                    else
                    {
                        if (pcmd->TextureId != IntPtr.Zero)
                        {
                            if (pcmd->TextureId == _fontAtlasID)
                            {
                                commandList.SetGraphicsResourceSet(1, _fontTextureResourceSet);
                            }
                            else
                            {
                                throw new NotImplementedException("TODO Implement Custom Resources");
                                // commandList.SetGraphicsResourceSet(1, GetImageResourceSet(pcmd->TextureId));
                            }
                        }

                        commandList.SetScissorRect(
                            0,
                            (uint)pcmd->ClipRect.X,
                            (uint)pcmd->ClipRect.Y,
                            (uint)(pcmd->ClipRect.Z - pcmd->ClipRect.X),
                            (uint)(pcmd->ClipRect.W - pcmd->ClipRect.Y));

                        commandList.DrawIndexed(pcmd->ElemCount, 1, (uint)idx_offset, vtx_offset, 0);
                    }

                    idx_offset += (int)pcmd->ElemCount;
                }
                vtx_offset += cmd_list->VtxBuffer.Size;
            }
        }

        /// <summary>
        /// Recreates the device texture used to render text.
        /// </summary>
        private unsafe void RecreateFontDeviceTexture(GraphicsDevice gd)
        {
            ImGuiNET.IO io = ImGui.GetIO();
            // Build
            FontTextureData textureData = io.FontAtlas.GetTexDataAsRGBA32();

            // Store our identifier
            io.FontAtlas.SetTexID(_fontAtlasID);

            _fontTexture = gd.ResourceFactory.CreateTexture(TextureDescription.Texture2D(
                (uint)textureData.Width,
                (uint)textureData.Height,
                1,
                1,
                PixelFormat.R8_G8_B8_A8_UNorm,
                TextureUsage.Sampled));
            _fontTexture.Name = "ImGui.NET Font Texture";
            gd.UpdateTexture(
                _fontTexture,
                (IntPtr)textureData.Pixels,
                (uint)(textureData.BytesPerPixel * textureData.Width * textureData.Height),
                0,
                0,
                0,
                (uint)textureData.Width,
                (uint)textureData.Height,
                1,
                0,
                0);
            _fontTextureView = gd.ResourceFactory.CreateTextureView(_fontTexture);

            io.FontAtlas.ClearTexData();
        }

        private unsafe void SubmitImGUICommands(float secondsPerFrame){

            // ImGui.GetStyle().WindowRounding = 0;

            // ImGui.SetNextWindowSize(new Vector2(contextWindow.Width/4, contextWindow.Height/4), Condition.Always);
            // ImGui.SetNextWindowPos(new Vector2(50,50), Condition.Always, Vector2.Zero);
            // if(ImGui.BeginWindow("ImGUI.NET Sample Program", WindowFlags.NoResize | WindowFlags.NoTitleBar | WindowFlags.NoMove))
            // {
            //     ImGui.Text("Hello World");
            // }
            // ImGui.EndWindow();
            float fps = 1.0f/secondsPerFrame;
            string performance = $"Seconds per Frame: {secondsPerFrame.ToString()}";
            //string performance_2 = $"Frames per Second: {fps.ToString()}";

            if (ImGui.BeginMainMenuBar())
            {
                ImGui.Text(performance);
                //ImGui.Text(performance_2);
                ImGui.EndMainMenuBar(); 
            }

        }


        private byte[] LoadEmbeddedShaderCode(ResourceFactory factory, string name)
        {
            switch (factory.BackendType)
            {
                case GraphicsBackend.Direct3D11:
                    {
                        string resourceName = name + ".hlsl.bytes";
                        return GetEmbeddedResourceBytes(resourceName);
                    }
                case GraphicsBackend.OpenGL:
                    {
                        string resourceName = name + ".glsl";
                        return GetEmbeddedResourceBytes(resourceName);
                    }
                case GraphicsBackend.Vulkan:
                    {
                        string resourceName = name + ".spv";
                        return GetEmbeddedResourceBytes(resourceName);
                    }
                case GraphicsBackend.Metal:
                    {
                        string resourceName = name + ".metallib";
                        return GetEmbeddedResourceBytes(resourceName);
                    }
                default:
                    throw new NotImplementedException();
            }
        }

        private byte[] GetEmbeddedResourceBytes(string resourceName)
        {
            using (Stream s = _assembly.GetManifestResourceStream(resourceName))
            {
                byte[] ret = new byte[s.Length];
                s.Read(ret, 0, (int)s.Length);
                return ret;
            }
        }
    }
}