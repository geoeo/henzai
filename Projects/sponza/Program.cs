using System;
using Henzai;
using Veldrid;
using Veldrid.Sdl2;
using Henzai.UI;

namespace Henzai.Examples
{
    // This has a long startup time in OpenGL. Dont click while it is loading, as the SDL context might get lost.
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

            scene = new SponzaScene(
                    "Sponza",
                    windowSize,
                    gdOptions,
                    rdOptions
                );

            gui = new StandardGUIOverlay(scene.GraphicsDevice, renderResolution);
            scene.SetUI(gui);
            gui.changeBackendAction += ChangeBackend;

            scene.PreRender_Descriptors += BuildBVH;
            scene.PreDraw_Time_Camera_Descriptors += EnableBVHCulling;

            //scene.CreateShadowMap(renderResolution);
            scene.SetUp(renderResolution);
            scene.Run();
        }
    }
}
