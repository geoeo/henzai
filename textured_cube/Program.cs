using System;
using Henzai;
using Veldrid;

namespace textured_cube
{
    class Program
    {
        static void Main(string[] args)
        {
            Scene scene = new Scene();
            Resolution renderResolution = new Resolution(960,540);
            Resolution windowSize = new Resolution(960,540);
            scene.Run("Textured Cube",renderResolution,windowSize,GraphicsBackend.OpenGL,false);
        }
    }
}
