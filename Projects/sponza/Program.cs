using System;
using Henzai;
using Veldrid;
using Veldrid.Sdl2;
using Henzai.UI;

namespace Henzai.Examples
{
    internal class Program : SceneContainer
    {

        static void Main(string[] args)
        {
            Program programm = new Program();
            programm.createScene(GraphicsBackend.Metal);
        }

        public override void createScene(GraphicsBackend graphicsBackend, Sdl2Window contextWindow = null){

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
                LimitFrames = false,
                FPSTarget = 60.0,
                FarPlane = 500.0f
            };

            scene = contextWindow == null ? new SponzaScene(
                    "Sponza",
                    windowSize,
                    gdOptions,
                    rdOptions
                ) : new SponzaScene(
                    "Sponza",
                    windowSize,
                    gdOptions,
                    rdOptions
                );

            gui = new StandardGUIOverlay(scene.GraphicsDevice,scene.ContextWindow);
            gui.SetOverlayFor(scene);
            gui.changeBackendAction += ChangeBackend;

            scene.PreRender_Models_Test += BuildBVH;
            scene.PreDraw_Time_GraphicsDevice_CommandList_Camera_Models_Test += EnableBVHCulling;

            scene.Run(renderResolution);
        }
    }
}
