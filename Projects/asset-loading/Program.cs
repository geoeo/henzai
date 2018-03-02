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
                SyncToVerticalBlank = false

            };

            Scene scene = new Scene(
                "Asset Loading",
                windowSize,
                gdOptions,
                GraphicsBackend.OpenGL,
                usePreferredGraphicsBackend: false
            );

            SimpleGUIOverlay gui = new SimpleGUIOverlay(scene.graphicsDevice,scene.contextWindow);
            gui.SetOverlayFor(scene);

            scene.Run(renderResolution);
        }
    }
}
