using System.Numerics;
using Henzai.Core.Numerics;

namespace Henzai.Core.VertexGeometry
{

    public struct VertexPositionColor : VertexRuntime, VertexLocateable
    {
        // 12 Bytes for Position + 16 Bytes for color
        public const byte SizeInBytes = 28;
        public const byte PositionOffset = 0;
        public const byte ColorOffset = 12;
        public const VertexRuntimeTypes HenzaiType = VertexRuntimeTypes.VertexPositionColor;
        public readonly Vector3 Position;
        public Vector4 color;

        public VertexPositionColor(Vector3 position, Vector4 colorIn)
        {
            Position = position;
            color = colorIn;
        }

        public VertexPositionColor(ref Vector4 position, ref VertexPositionColor v){
            Position = Numerics.Vector.ToVec3(position);
            color = v.color;
        }

        public VertexPositionColor(Vector4 position, VertexPositionColor v){
            Position = Numerics.Vector.ToVec3(position);
            color = v.color;
        }

        public byte GetSizeInBytes(){
            return SizeInBytes;
        }

        public Vector4 GetPosition(){
            return new Vector4(Position, 1.0f);
        }

        
    }
}
