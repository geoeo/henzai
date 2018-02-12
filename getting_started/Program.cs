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

            Scene scene = new Scene(
                "Getting Started",
                windowSize,
                gdOptions,
                GraphicsBackend.OpenGL,
                usePreferredGraphicsBackend: false
            );

            scene.Run(renderResolution);
        }
    }
}
