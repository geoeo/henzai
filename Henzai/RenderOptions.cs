using System;
using Veldrid;

namespace Henzai
{

    public class RenderOptions
    {
        public bool LimitFrames {get; set;}
        public double FPSTarget {get; set;}
        public double MillisecondsPerFrame => 1000.0/FPSTarget;
        public GraphicsBackend PreferredGraphicsBackend { get ;set; }
        public bool UsePreferredGraphicsBackend {get; set;}
    }
}