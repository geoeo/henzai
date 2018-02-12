using System;
using System.Numerics;
using System.Collections.Generic;
using System.IO;
using Veldrid;
using Veldrid.StartupUtilities;
using Veldrid.Sdl2;
using Veldrid.OpenGL;
using Henzai;
using Henzai.Extensions;
using Henzai.Geometry;

namespace Henzai.Examples
{
    internal class Scene : Renderable
    {

        public Scene(string title,Resolution windowSize, GraphicsDeviceOptions graphicsDeviceOptions, GraphicsBackend preferredBackend, bool usePreferredGraphicsBackend)
            : base(title,windowSize,graphicsDeviceOptions,preferredBackend,usePreferredGraphicsBackend){

                string filePath = Path.Combine(AppContext.BaseDirectory, "Models/sphere.obj");
                AssimpLoader.LoadFromFile(filePath);

        }

        override protected List<IDisposable> CreateResources(){

            return new List<IDisposable>();

        }

        override protected void BuildCommandList(){}

        override protected void Draw(){}


    }
    
}