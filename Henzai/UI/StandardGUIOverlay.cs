using System.Numerics;
using ImGuiNET;
using Veldrid;
using Veldrid.Sdl2;
using System;

namespace Henzai.UI
{
    public class StandardGUIOverlay : UserInterface
   {

       private const int MAX_SAMPLES = 100;
       private float[] _secondsPerFrameSamples;
       private int _sampleCounter;

        public StandardGUIOverlay(GraphicsDevice graphicsDevice, Sdl2Window contextWindow) 
        : base(graphicsDevice,contextWindow){
            _secondsPerFrameSamples = new float[MAX_SAMPLES];
            _sampleCounter = 0;
        }

        override protected unsafe void SubmitImGUILayout(float secondsPerFrame){

            _secondsPerFrameSamples[_sampleCounter] = secondsPerFrame;
            if (_sampleCounter < MAX_SAMPLES-1)
                _sampleCounter++;
            else
                _sampleCounter = 0;

            //float fps = 1.0f/secondsPerFrame;
            string performance = $"Seconds per Frame: {secondsPerFrame.ToString()}";
            //string performance_2 = $"Frames per Second: {fps.ToString()}";

            var acc = 0.0f;
            for( int i = 0; i < MAX_SAMPLES; i++){
                acc += _secondsPerFrameSamples[i];
            }
            acc /= MAX_SAMPLES;
            string avg_performance = $"Average Seconds per Frame: {acc.ToString()}";

            if (ImGui.BeginMainMenuBar())
            {
                ImGui.Text(performance);
                ImGui.Text("||");
                ImGui.Text(avg_performance);
                ImGui.Text("||");
                if (ImGui.BeginMenu("Backend"))
                {
                    if (ImGui.MenuItem("OpenGL",GraphicsDevice.IsBackendSupported(GraphicsBackend.OpenGL))) {
                        backendCallback = true;
                        newGraphicsBackend = GraphicsBackend.OpenGL;
                    }
                    if (ImGui.MenuItem("Metal",GraphicsDevice.IsBackendSupported(GraphicsBackend.Metal))){ 
                        backendCallback = true;
                        newGraphicsBackend = GraphicsBackend.Metal;
                    }
                    ImGui.EndMenu();
                }
                ImGui.EndMainMenuBar(); 

            }

        }

   } 

}
