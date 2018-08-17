using System;
using Henzai.Runtime;
using Veldrid;
using Henzai.UI;

namespace Henzai.Examples
{
    internal class Program
    {
        static void Main(string[] args)
        {
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
                PreferredGraphicsBackend = GraphicsBackend.OpenGL,
                UsePreferredGraphicsBackend = true,
                LimitFrames = true,
                FPSTarget = 60.0
            };

            SDL2GUIScene scene = new SDL2GUIScene(
                "GUI",
                windowSize,
                gdOptions,
                rdOptions
            );

            SimpleGUIOverlay gui = new SimpleGUIOverlay(scene.GraphicsDevice,scene.ContextWindow);
            gui.SetOverlayFor(scene);

            scene.Run(renderResolution);
        }
    }
}
