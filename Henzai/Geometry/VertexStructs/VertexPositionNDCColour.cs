using System;
using System.Numerics;
using Henzai.Runtime;
using Veldrid;

namespace Henzai.Geometry
{

    public struct VertexPositionNDCColour
    {
        public const uint SizeInBytes = 24;
        public const VertexTypes HenzaiType = VertexTypes.VertexPositionNDCColour;

        public readonly Vector2 Position; // Position in NDC
        public RgbaFloat Colour;
        public VertexPositionNDCColour(Vector2 position, RgbaFloat colour)
        {
            Position = position;
            Colour = colour;
        }
        // 8 Bytes for Position + 16 Bytes for Colour
        
    }
}
