using System;
using System.Numerics;
using Henzai.Runtime;
using Veldrid;

namespace Henzai.Geometry
{

    public struct VertexPositionColor
    {
        // 12 Bytes for Position + 16 Bytes for color
        public const uint SizeInBytes = 28;
        public const VertexTypes HenzaiType = VertexTypes.VertexPositionColor;

        public readonly Vector3 Position;
        public Vector4 color;
        public VertexPositionColor(Vector3 position, Vector4 colorIn)
        {
            Position = position;
            color = colorIn;
        }

        public VertexPositionColor(Vector3 position, RgbaFloat colorIn)
        {
            Position = position;
            color = colorIn.ToVector4();
        }

        
    }
}
