using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using Veldrid;
using Veldrid.StartupUtilities;
using Henzai.UI;
using Henzai.Geometry;
using Henzai.Core.Materials;
using Henzai.Core.VertexGeometry;
using Henzai.Core.Extensions;
using Henzai.Core.Acceleration;

namespace Henzai.Runtime
{
    /// <summary>
    /// A boilerplate for pre or post processing renderable scenes.
    /// This should never be used as a main scene.
    /// </summary>
    public abstract class SubRenderable : IDisposable
    {
        private Resolution _resolution;
        public Resolution Resolution => _resolution;
        protected GraphicsDevice _graphicsDevice;
        public GraphicsDevice GraphicsDevice => _graphicsDevice;
        protected Framebuffer _frameBuffer;
        public Framebuffer FrameBuffer => _frameBuffer;
        protected DisposeCollectorResourceFactory _factory;
        protected CommandList _commandList;
      
        public SubRenderable(GraphicsDevice graphicsDevice, Resolution resolution)
        {
            _graphicsDevice = graphicsDevice;
            _resolution = resolution;

            _factory = new DisposeCollectorResourceFactory(_graphicsDevice.ResourceFactory);
            _commandList = _factory.CreateCommandList();
            SetFramebuffer();

        }

        /// <summary>
        /// Executes the defined command list(s)
        /// </summary>
        abstract public void Draw();

        /// <summary>
        /// Creates resources used to render e.g. Buffers, Textures etc.
        /// </summary>
        abstract public void CreateResources();

        /// <summary>
        /// Creates the command list and its containing render commands
        /// </summary>
        abstract public void BuildCommandList();

        /// <summary>
        /// Sets the framebuffer for this scene
        /// </summary>
        abstract protected void SetFramebuffer();

        /// <summary>
        /// Disposes of all elements in crated by the ResourceFactory
        /// </summary>
        public virtual void Dispose()
        {
            _factory.DisposeCollector.DisposeAll();
        }
    }
}