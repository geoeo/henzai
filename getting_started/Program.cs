using System;
using Henzai;
using Veldrid;

namespace getting_started
{
    class Program
    {
        static void Main(string[] args)
        {
            Scene scene = new Scene();
            Resolution renderResolution = new Resolution(960,540);
            Resolution windowSize = new Resolution(960,540);
            GraphicsDeviceOptions gdOptions = new GraphicsDeviceOptions()
            {
                Debug = false,
                SwapchainDepthFormat = null,
                SyncToVerticalBlank = false

            };
            scene.Run("Getting Started",renderResolution,windowSize,gdOptions,GraphicsBackend.OpenGL,true);
        }
    }
}
