using System;
using Henzai;
using Veldrid;
using Veldrid.Sdl2;
using Henzai.UserInterface;

namespace Henzai.Examples
{
    internal class Program
    {
        private static Scene _scene;
        private static SimpleGUIOverlay  _gui;

        static void Main(string[] args)
        {
            createScene(GraphicsBackend.OpenGL);
        }

        public static void createScene(GraphicsBackend graphicsBackend, Sdl2Window contextWindow = null){

            Resolution renderResolution = new Resolution(960,540);
            Resolution windowSize = new Resolution(960,540);
            GraphicsDeviceOptions gdOptions = new GraphicsDeviceOptions()
            {
                Debug = false,
                SwapchainDepthFormat = PixelFormat.R32_Float,
                SyncToVerticalBlank = false,
                ResourceBindingModel = ResourceBindingModel.Improved

            };

            RenderOptions rdOptions = new RenderOptions()
            {
                Resolution = renderResolution,
                PreferredGraphicsBackend = graphicsBackend,
                UsePreferredGraphicsBackend = true,
                LimitFrames = true,
                FPSTarget = 60.0,
                FarPlane = 2000f
            };

            if(contextWindow == null){
                _scene = new Scene(
                    "Sponza",
                    windowSize,
                    gdOptions,
                    rdOptions
                );
            }
            else{
                _scene = new Scene(
                    "Sponza",
                    contextWindow,
                    gdOptions,
                    rdOptions
                );
            }


            _gui = new SimpleGUIOverlay(_scene.GraphicsDevice,_scene.ContextWindow);
            _gui.SetOverlayFor(_scene);
            _gui.changeBackendAction += Program.ChangeBackend;

            _scene.Run(renderResolution);
        }

        //TODO: Abstract this so that all examples have access to this
        public static void ChangeBackend(GraphicsBackend graphicsBackend){
            Sdl2Window contextWindow = _scene.ContextWindow;
            _scene.Dispose(false);
            createScene(graphicsBackend,contextWindow);
        }
    }
}
