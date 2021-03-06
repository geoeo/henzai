﻿using Veldrid;
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
            programm.createScene(GraphicsBackend.OpenGL);
        }

        public override void createScene(GraphicsBackend graphicsBackend, Sdl2Window contextWindow = null){

            Resolution renderResolution = new Resolution(1024, 1024);
            Resolution windowSize = new Resolution(1024, 1024);
            GraphicsDeviceOptions gdOptions = new GraphicsDeviceOptions()
            {
                Debug = true,
                SwapchainDepthFormat = PixelFormat.R32_Float,
                SyncToVerticalBlank = false,
                ResourceBindingModel = ResourceBindingModel.Improved,
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

            scene = new SponzaOmniScene(
                    "Sponza Omni",
                    windowSize,
                    gdOptions,
                    rdOptions
                );

            gui = new StandardGUIOverlay(scene.GraphicsDevice, renderResolution);
            scene.SetUI(gui);
            gui.changeBackendAction += ChangeBackend;

            //scene.PreRender_Descriptors += BuildBVH;
            //scene.PreDraw_Time_Camera_Descriptors += EnableBVHCulling;

            scene.CreateOmniShadowMap(renderResolution);
            scene.SetUp(renderResolution);
            scene.Run();
        }
    }
}
