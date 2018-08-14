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
                SwapchainDepthFormat = PixelFormat.R32_Float,
                SyncToVerticalBlank = false,
                ResourceBindingModel = ResourceBindingModel.Improved

            };

            RenderOptions rdOptions = new RenderOptions()
            {
                Resolution = renderResolution,
                PreferredGraphicsBackend = GraphicsBackend.OpenGL,
                UsePreferredGraphicsBackend = true,
                LimitFrames = true,
                FPSTarget = 60.0,
                FarPlane = 2000f
            };

            Scene scene = new Scene(
                "Sponza",
                windowSize,
                gdOptions,
                rdOptions
            );

            SimpleGUIOverlay gui = new SimpleGUIOverlay(scene.GraphicsDevice,scene.contextWindow);
            gui.SetOverlayFor(scene);
            gui.changeBackend += Program.ChangeBackend;

            scene.Run(renderResolution);
        }

        public static void ChangeBackend(){
            Console.WriteLine("Change backend");
        }
    }
}
