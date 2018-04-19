using System;
using Henzai;
using Veldrid;
using Henzai.UserInterface;

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
                SyncToVerticalBlank = true,
                ResourceBindingModel = ResourceBindingModel.Improved

            };

            RenderOptions rdOptions = new RenderOptions()
            {
                Resolution = renderResolution,
                PreferredGraphicsBackend = GraphicsBackend.OpenGL,
                UsePreferredGraphicsBackend = false,
                LimitFrames = true,
                FPSTarget = 60.0
            };

            Scene scene = new Scene(
                "Lights",
                windowSize,
                gdOptions,
                rdOptions
            );

            SimpleGUIOverlay gui = new SimpleGUIOverlay(scene.GraphicsDevice,scene.contextWindow);
            gui.SetOverlayFor(scene);

            scene.Run(renderResolution);
        }
    }
}
