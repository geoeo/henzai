using System;
using System.Numerics;

namespace Henzai
{
    // TODO: Make this more configurable. -> Add some kind of flag to identify the instancing type
    //@Investigate: Is can also be represented as a UNION type?
    public sealed class InstancingData
    {
        public static readonly InstancingData NO_DATA = null;
        public Vector3[] Positions {get;set;}
        public Matrix4x4[] Matrices {get;set;}

    }
}