using System;
using Henzai;
using Veldrid;
using Veldrid.Sdl2;
using Henzai.UI;
using Henzai.Runtime;

namespace Henzai
{
    public abstract class SceneContainer
    {
        protected static Renderable _scene;
        protected static UserInterface  _gui;

        public abstract void createScene(GraphicsBackend graphicsBackend, Sdl2Window contextWindow = null);

        public void ChangeBackend(GraphicsBackend graphicsBackend){
            Sdl2Window contextWindow = _scene.ContextWindow;
            _scene.Dispose(false);
            createScene(graphicsBackend,contextWindow);
        }

    }
}