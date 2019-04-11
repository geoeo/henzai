using System.Numerics;
using Henzai.Core.Numerics;

namespace Henzai.Core.VertexGeometry
{

    public struct VertexPositionNDCColor : VertexRuntime, VertexLocateable
    {
        // 8 Bytes for Position + 16 Bytes for color
        public const byte SizeInBytes = 24;
        public const VertexRuntimeTypes HenzaiType = VertexRuntimeTypes.VertexPositionNDCColor;

        public readonly Vector2 Position; // Position in NDC
        public Vector4 color;

        public VertexPositionNDCColor(Vector2 position, Vector4 colorIn)
        {
            Position = position;
            color = colorIn;
        }

        public VertexPositionNDCColor(ref Vector4 position, ref VertexPositionNDCColor v){
            Position = Numerics.Vector.ToVec2(ref position);
            color = v.color;
        }

        public byte GetSizeInBytes(){
            return SizeInBytes;
        }  

        public Vector4 GetPosition() {
            return new Vector4(Position, 0.0f, 1.0f);
        }    
    }
}
