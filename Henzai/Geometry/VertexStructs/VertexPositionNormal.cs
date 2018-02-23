using System.Numerics;
using Henzai.Runtime;

namespace Henzai.Geometry
{
    public struct VertexPositionNormal
    {
        public const byte SizeInBytes = 24;
        public const byte PositionOffset = 0;
        public const byte NormalOffset = 12;
        public const byte ElementCount = 2;
        public const HenzaiTypes HenzaiType = HenzaiTypes.VertexPositionNormal;

        public readonly Vector3 Position;
        public readonly Vector3 Normal;

        public VertexPositionNormal(Vector3 position, Vector3 normal)
        {
            Position = position;
            Normal = normal;
        }
    }
}
