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
                ResourceBindingModel = ResourceBindingModel.Improved //TODO

            };

            RenderOptions rdOptions = new RenderOptions()
            {
                PreferredGraphicsBackend = GraphicsBackend.OpenGL,
                UsePreferredGraphicsBackend = false,
                LimitFrames = true,
                FPSTarget = 60.0
            };


            OffscreenScene scene = new OffscreenScene(
                "Offscreen",
                windowSize,
                gdOptions,
                rdOptions
            );

            var gui = new StandardGUIOverlay(scene.GraphicsDevice, renderResolution);
            scene.SetUI(gui);

            scene.SetUp(renderResolution);
            scene.Run();
        }
    }
}
