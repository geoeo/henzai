using System.Numerics;
using Henzai.Runtime;

namespace Henzai.Geometry
{
    public struct VertexPosition
    {
        public const byte SizeInBytes = 12;
        public const byte ElementCount = 1;
        public const VertexTypes HenzaiType = VertexTypes.VertexPosition;

        public readonly Vector3 Position;

        public VertexPosition(Vector3 position)
        {
            Position = position;
        }
    }
}