using System;
using Henzai;
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
                SyncToVerticalBlank = false

            };

            RenderOptions rdOptions = new RenderOptions()
            {
                PreferredGraphicsBackend = GraphicsBackend.OpenGL,
                UsePreferredGraphicsBackend = true,
                LimitFrames = true,
                FPSTarget = 60.0
            };

            NormalMappingScene scene = new NormalMappingScene(
                "Normal Mapping",
                windowSize,
                gdOptions,
                rdOptions
            );
            var gui = new StandardGUIOverlay(scene.GraphicsDevice, scene.ContextWindow);
            scene.SetUI(gui);


            scene.Run(renderResolution);
        }
    }
}
