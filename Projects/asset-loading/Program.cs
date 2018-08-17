using System;
using Henzai;
using Veldrid;
using Henzai.UI;
using Veldrid.Sdl2;

namespace Henzai.Examples
{
    internal class Program : SceneContainer
    {
        static void Main(string[] args)
        {
            Program programm = new Program();
            programm.createScene(GraphicsBackend.OpenGL);
        }


        public override void createScene(GraphicsBackend graphicsBackend, Sdl2Window contextWindow = null){    
            Resolution renderResolution = new Resolution(960,540);
            Resolution windowSize = new Resolution(960,540);
            GraphicsDeviceOptions gdOptions = new GraphicsDeviceOptions()
            {
                Debug = false,
                SwapchainDepthFormat = PixelFormat.R16_UNorm,
                SyncToVerticalBlank = false,
                ResourceBindingModel = ResourceBindingModel.Improved

            };

            RenderOptions rdOptions = new RenderOptions()
            {
                PreferredGraphicsBackend = graphicsBackend,
                UsePreferredGraphicsBackend = true,
                LimitFrames = true,
                FPSTarget = 60.0
            };

            if(contextWindow == null){
                _scene = new AssetLoadingScene(
                    "Asset Loading",
                    windowSize,
                    gdOptions,
                    rdOptions
                );
            }
            else{
                _scene = new AssetLoadingScene(
                    "Asset Loading",
                    contextWindow,
                    gdOptions,
                    rdOptions
                );
            }

            _gui = new SimpleGUIOverlay(_scene.GraphicsDevice,_scene.ContextWindow);
            _gui.SetOverlayFor(_scene);
            _gui.changeBackendAction += ChangeBackend;

            _scene.Run(renderResolution);
        }
    }
}
