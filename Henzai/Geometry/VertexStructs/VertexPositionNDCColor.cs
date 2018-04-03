using System;
using System.Numerics;
using Henzai.Runtime;
using Veldrid;

namespace Henzai.Geometry
{

    public struct VertexPositionNDCColor
    {
        public const uint SizeInBytes = 24;
        public const VertexTypes HenzaiType = VertexTypes.VertexPositionNDCColor;

        public readonly Vector2 Position; // Position in NDC
        public RgbaFloat color;
        public VertexPositionNDCColor(Vector2 position, RgbaFloat colorIn)
        {
            Position = position;
            color = colorIn;
        }
        // 8 Bytes for Position + 16 Bytes for color
        
    }
}
