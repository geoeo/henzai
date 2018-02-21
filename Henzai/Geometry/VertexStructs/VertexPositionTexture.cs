using System.Numerics;
using Henzai.Runtime;

namespace Henzai.Geometry
{
    public struct VertexPositionTexture
    {
        public const byte SizeInBytes = 20;
        public const byte TextureCoordinatesOffset = 12;
        public const byte ElementCount = 2;
        public const HenzaiTypes HenzaiType = HenzaiTypes.VertexPositionTexture;

        public readonly Vector3 Position;
        public readonly Vector2 TextureCoordinates;

        public VertexPositionTexture(Vector3 position, Vector2 texCoords)
        {
            Position = position;
            TextureCoordinates = texCoords;
        }
    }
}
