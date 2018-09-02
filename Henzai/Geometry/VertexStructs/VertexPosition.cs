using System.Numerics;
using Henzai.Runtime;
using Henzai.Core.Geometry;

namespace Henzai.Geometry
{
    public struct VertexPosition : VertexRuntime, VertexLocateable
    {
        public const byte SizeInBytes = 12;
        public const byte ElementCount = 1;
        public const VertexTypes HenzaiType = VertexTypes.VertexPosition;

        public readonly Vector3 Position;

        public VertexPosition(Vector3 position)
        {
            Position = position;
        }

        public byte GetSizeInBytes(){
            return SizeInBytes;
        }

        public Vector3 GetPosition(){
            return Position;
        }
    }
}