using System.Numerics;

namespace Henzai.Runtime
{
    // TODO: Make this more configurable. -> Add some kind of flag to identify the instancing type
    //@Investigate: Is can also be represented as a UNION type?
    public sealed class InstanceData
    {
        private static readonly InstanceData no_data = new InstanceData();
        public static InstanceData NO_DATA => no_data;
        public uint Flag {get; set;}
        public Vector3[] Positions {get;set;}
        public Matrix4x4[] ViewMatrices {get;set;}
    }
}