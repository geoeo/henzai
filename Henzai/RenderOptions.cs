using System;
using Veldrid;

namespace Henzai
{

    public class RenderOptions
    {
        public Resolution Resolution {get; set;}
        public bool LimitFrames {get; set;}
        public double FPSTarget {get; set;}
        public double MillisecondsPerFrame => 1000.0/FPSTarget;
        public GraphicsBackend PreferredGraphicsBackend { get ;set; }
        public bool UsePreferredGraphicsBackend {get; set;}
        public float FarPlane {get; set;}
    }
}