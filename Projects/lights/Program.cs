﻿using System;
using Henzai;
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


            if(contextWindow == null){
                scene = new LightsScene(
                    "Lights",
                    windowSize,
                    gdOptions,
                    rdOptions
                );
            }
            else{
                scene = new LightsScene(
                    "Lights",
                    contextWindow,
                    gdOptions,
                    rdOptions
                );
            }

            gui = new SimpleGUIOverlay(scene.GraphicsDevice,scene.ContextWindow);
            gui.SetOverlayFor(scene);
            gui.changeBackendAction += ChangeBackend;

            scene.Run(renderResolution);
        }
    }
}