using System.Numerics;
using Henzai.Core.Numerics;

namespace Henzai.Core.VertexGeometry
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

        public VertexPosition(ref Vector4 position){
            Position = Numerics.Vector.ToVec3(ref position);
        }

        public byte GetSizeInBytes(){
            return SizeInBytes;
        }

        public Vector4 GetPosition(){
            return new Vector4(Position, 1.0f);
        }
    }
}