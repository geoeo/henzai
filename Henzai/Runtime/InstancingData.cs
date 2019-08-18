using System.Numerics;

namespace Henzai.Runtime
{
    // TODO: Make this more configurable. -> Add some kind of flag to identify the instancing type
    //@Investigate: Is can also be represented as a UNION type?
    public sealed class InstancingData
    {
        private static readonly InstancingData no_data = new InstancingData();
        public static InstancingData NO_DATA => no_data;
        public InstancingTypes Types {get; set;}
        public Vector3[] Positions {get;set;}
        public Matrix4x4[] ViewMatrices {get;set;}
    }
}