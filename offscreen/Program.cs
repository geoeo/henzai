using System;
using Henzai;
using Veldrid;

namespace Henzai.Examples
{
    class Program
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
                "Offscreen",
                windowSize,
                gdOptions,
                GraphicsBackend.OpenGL,
                usePreferredGraphicsBackend: false
            );

            scene.Run(renderResolution);
        }
    }
}
