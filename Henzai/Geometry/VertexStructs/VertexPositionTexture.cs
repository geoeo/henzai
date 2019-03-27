using System.Numerics;
using Henzai.Core.VertexGeometry;

namespace Henzai.Geometry
{
    public struct VertexPositionTexture : VertexRuntime, VertexLocateable
    {
        public const byte SizeInBytes = 20;
        public const byte PositionOffset = 0;
        public const byte TextureCoordinatesOffset = 12;
        public const byte ElementCount = 2;
        public const VertexRuntimeTypes HenzaiType = VertexRuntimeTypes.VertexPositionTexture;

        public readonly Vector3 Position;
        public readonly Vector2 TextureCoordinates;

        public VertexPositionTexture(Vector3 position, Vector2 texCoords)
        {
            Position = position;
            TextureCoordinates = texCoords;
        }

        public byte GetSizeInBytes(){
            return SizeInBytes;
        }

        public Vector3 GetPosition(){
            return Position;
        }
    }
}
