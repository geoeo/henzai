using System.Numerics;

namespace Henzai.Core.VertexGeometry
{
    public struct VertexPositionNormalTextureTangent : VertexRuntime, VertexLocateable
    {
        public const byte SizeInBytes = 44;
        public const byte PositionOffset = 0;
        public const byte NormalOffset = 12;
        public const byte TextureCoordinatesOffset = 24;
        public const byte TangentOffset = 32;
        public const byte ElementCount = 4;
        public const VertexRuntimeTypes HenzaiType = VertexRuntimeTypes.VertexPositionNormalTextureTangent;

        public Vector3 Position;
        public Vector3 Normal;
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
        public byte GetSizeInBytes(){
            return SizeInBytes;
        }

        public Vector3 GetPosition(){
            return Position;
        }
    }
}