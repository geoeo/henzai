using System;
using System.Collections.Generic;
using ImGuiNET;
using Veldrid;

namespace Henzai.GUI
{
    public class GUIRenderable : Renderable
    {

        public GUIRenderable(GraphicsDevice gd, Resolution screenResolution) : base(gd,screenResolution){
        }

        override protected List<IDisposable> CreateResources(){

            List<IDisposable> resources = new List<IDisposable>();

            return resources;

        }

        override protected void Draw(){

        }
    }
}