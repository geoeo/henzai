using Veldrid;
using Veldrid.Sdl2;
using Henzai.UI;

namespace Henzai.Examples
{
    internal class Program : SceneContainer
    {
        static void Main(string[] args)
        {
            Program programm = new Program();
            programm.createScene(GraphicsBackend.OpenGL);
        }

        public override void createScene(GraphicsBackend graphicsBackend, Sdl2Window contextWindow = null){

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
                PreferredGraphicsBackend = graphicsBackend,
                UsePreferredGraphicsBackend = true,
                LimitFrames = true,
                FPSTarget = 60.0
            };

            scene = contextWindow == null ? new LightsScene(
                    "Lights",
                    windowSize,
                    gdOptions,
                    rdOptions
                ) : new LightsScene(
                    "Lights",
                    windowSize,
                    gdOptions,
                    rdOptions
                );

            gui = new StandardGUIOverlay(scene.GraphicsDevice, renderResolution);
            scene.SetUI(gui);
            gui.changeBackendAction += ChangeBackend;

            scene.CreateShadowMap(renderResolution);
            scene.SetUp(renderResolution);
            scene.Run();
        }
    }
}
