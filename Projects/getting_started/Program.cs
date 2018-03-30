using System;
using Henzai;
using Veldrid;

namespace Henzai.Examples
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Resolution renderResolution = new Resolution(960,540);
            Resolution windowSize = new Resolution(960,540);
            GraphicsDeviceOptions gdOptions = new GraphicsDeviceOptions();

            RenderOptions rdOptions = new RenderOptions()
            {
                PreferredGraphicsBackend = GraphicsBackend.OpenGL,
                UsePreferredGraphicsBackend = false,
                LimitFrames = true,
                FPSTarget = 60.0
            };

            Scene scene = new Scene(
                "Getting Started",
                windowSize,
                gdOptions,
                rdOptions
            );

            scene.Run(renderResolution);
        }
    }
}
