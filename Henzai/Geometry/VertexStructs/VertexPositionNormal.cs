using System.Numerics;

namespace Henzai.Geometry
{
    public struct VertexPositionNormal
    {
        public const byte SizeInBytes = 24;
        public const byte NormalOffset = 12;
        public const byte ElementCount = 2;

        public readonly Vector3 Position;
        public readonly Vector3 Normal;

        public VertexPositionNormal(Vector3 position, Vector3 normal)
        {
            Position = position;
            Normal = normal;
        }
    }
}
