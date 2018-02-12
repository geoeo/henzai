using System.Numerics;
using ImGuiNET;
using Veldrid;
using Veldrid.Sdl2;

namespace Henzai.UserInterface
{
    public class SimpleGUIOverlay : UserInterface
   {
        public SimpleGUIOverlay(GraphicsDevice graphicsDevice, Sdl2Window contextWindow) 
        : base(graphicsDevice,contextWindow){}

        override protected unsafe void SubmitImGUILayout(float secondsPerFrame){

            float fps = 1.0f/secondsPerFrame;
            string performance = $"Seconds per Frame: {secondsPerFrame.ToString()}";
            //string performance_2 = $"Frames per Second: {fps.ToString()}";

            if (ImGui.BeginMainMenuBar())
            {
                ImGui.Text(performance);
                //ImGui.Text(performance_2);
                ImGui.EndMainMenuBar(); 
            }

        }

   } 

}
