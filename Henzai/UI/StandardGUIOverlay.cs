using System.Numerics;
using ImGuiNET;
using Veldrid;
using System;

namespace Henzai.UI
{
    public class StandardGUIOverlay : UserInterface
   {

       private const int MAX_SAMPLES = 100;
       private float[] _secondsPerFrameSamples;
       private int _sampleCounter;

        public StandardGUIOverlay(GraphicsDevice graphicsDevice, Resolution resolution) 
        : base(graphicsDevice, resolution){
            _secondsPerFrameSamples = new float[MAX_SAMPLES];
            _sampleCounter = 0;
        }

        override protected unsafe void SubmitImGUILayout(float secondsPerFrame){

            _secondsPerFrameSamples[_sampleCounter] = secondsPerFrame;
            if (_sampleCounter < MAX_SAMPLES-1)
                _sampleCounter++;
            else
                _sampleCounter = 0;

            string performance = $"Seconds per Frame: {secondsPerFrame.ToString("N5")}";

            var acc = 0.0f;
            for( int i = 0; i < MAX_SAMPLES; i++){
                acc += _secondsPerFrameSamples[i];
            }
            acc /= MAX_SAMPLES;

            var avg_fps = 1.0f/acc;
            string avg_performance = $"Average Seconds per Frame: {acc.ToString("N5")}";
            string performance_fps = $"Average Frames per Second: {avg_fps.ToString("N2")}";

            if (ImGui.BeginMainMenuBar())
            {
                ImGui.Text(performance);
                ImGui.Text("||");
                ImGui.Text(performance_fps);
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
