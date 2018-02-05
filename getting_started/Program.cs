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
            scene.Run("Getting Started",renderResolution,windowSize,GraphicsBackend.OpenGL,false);
        }
    }
}
