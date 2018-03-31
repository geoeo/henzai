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
        public const VertexTypes HenzaiType = VertexTypes.VertexPositionNormal;

        public Vector3 Position;
        public Vector3 Normal;

        public VertexPositionNormal(Vector3 position, Vector3 normal)
        {
            Position = position;
            Normal = Vector3.Normalize(normal);
        }
    }
}
