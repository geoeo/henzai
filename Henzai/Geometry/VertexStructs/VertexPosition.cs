using System.Numerics;
using Henzai.Core.VertexGeometry;

namespace Henzai.Geometry
{
    public struct VertexPosition : VertexRuntime, VertexLocateable
    {
        //TODO: Investigate better aligment of bytes for all Vertex structs
        public const byte SizeInBytes = 12;
        public const byte ElementCount = 1;
        public const VertexRuntimeTypes HenzaiType = VertexRuntimeTypes.VertexPosition;

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