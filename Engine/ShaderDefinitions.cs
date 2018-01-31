using System;
using System.Numerics;
using Veldrid;

namespace Engine
{
    struct VertexPositionColour
    {
        public Vector2 Position; // Position in NDC
        public RgbaFloat Colour;
        public VertexPositionColour(Vector2 position, RgbaFloat colour)
        {
            Position = position;
            Colour = colour;
        }
        // 8 Bytes for Position + 16 Bytes for Colour
        public const uint SizeInBytes = 24;
    }
}
