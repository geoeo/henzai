using System;
using Henzai.Runtime;
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
                UsePreferredGraphicsBackend = true,
                LimitFrames = true,
                FPSTarget = 60.0
            };

            GettingStartedScene scene = new GettingStartedScene(
                "Getting Started",
                windowSize,
                gdOptions,
                rdOptions
            );

            scene.Run(renderResolution);
        }
    }
}
