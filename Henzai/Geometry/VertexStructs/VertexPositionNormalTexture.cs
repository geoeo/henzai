using System.Numerics;
using Henzai.Runtime;
using Henzai.Core.Geometry;

namespace Henzai.Geometry
{
    public struct VertexPositionNormalTexture : VertexRuntime, VertexLocateable
    {
        public const byte SizeInBytes = 32;
        public const byte PositionOffset = 0;
        public const byte NormalOffset = 12;
        public const byte TextureCoordinatesOffset = 24;
        public const byte ElementCount = 3;
        public const VertexTypes HenzaiType = VertexTypes.VertexPositionNormalTexture;

        public readonly Vector3 Position;
        public readonly Vector3 Normal;
        public Vector2 TextureCoordinates;

        public VertexPositionNormalTexture(Vector3 position, Vector3 normal, Vector2 texCoords)
        {
            Position = position;
            Normal = Vector3.Normalize(normal);
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
