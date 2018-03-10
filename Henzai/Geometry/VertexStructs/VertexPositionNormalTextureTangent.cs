using System.Numerics;
using Henzai.Runtime;

namespace Henzai.Geometry
{
    public struct VertexPositionNormalTextureTangent
    {
        public const byte SizeInBytes = 44;
        public const byte PositionOffset = 0;
        public const byte NormalOffset = 12;
        public const byte TextureCoordinatesOffset = 24;
        public const byte TangentOffset = 32;
        public const byte ElementCount = 5;
        public const VertexTypes HenzaiType = VertexTypes.VertexPositionNormalTextureTangent;

        public readonly Vector3 Position;
        public readonly Vector3 Normal;
        public Vector2 TextureCoordinates;
        public Vector3 Tangent;

        public VertexPositionNormalTextureTangent(Vector3 position, Vector3 normal, Vector2 texCoords)
        {
            Position = position;
            Normal = Vector3.Normalize(normal);
            TextureCoordinates = texCoords;
            Tangent = Vector3.Zero;
        }

        public VertexPositionNormalTextureTangent(Vector3 position, Vector3 normal, Vector2 texCoords,Vector3 tangent)
        {
            Position = position;
            Normal = Vector3.Normalize(normal);
            TextureCoordinates = texCoords;
            Tangent = tangent;
        }
    }
}