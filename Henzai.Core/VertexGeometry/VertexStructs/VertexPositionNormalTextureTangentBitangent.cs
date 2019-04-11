using System.Numerics;
using Henzai.Core.Numerics;

namespace Henzai.Core.VertexGeometry
{
    public struct VertexPositionNormalTextureTangentBitangent : VertexRuntime, VertexLocateable
    {
        public const byte SizeInBytes = 56;
        public const byte PositionOffset = 0;
        public const byte NormalOffset = 12;
        public const byte TextureCoordinatesOffset = 24;
        public const byte TangentOffset = 32;
        public const byte BitangentOffset = 44;
        public const byte ElementCount = 5;
        public const VertexRuntimeTypes HenzaiType = VertexRuntimeTypes.VertexPositionNormalTextureTangentBitangent;

        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 TextureCoordinates;
        public Vector3 Tangent;
        public Vector3 Bitangent;

        public VertexPositionNormalTextureTangentBitangent(Vector3 position, Vector3 normal, Vector2 texCoords)
        {
            Position = position;
            Normal = Vector3.Normalize(normal);
            TextureCoordinates = texCoords;
            Tangent = Vector3.Zero;
            Bitangent = Vector3.Zero;
        }

        public VertexPositionNormalTextureTangentBitangent(Vector3 position, Vector3 normal, Vector2 texCoords,Vector3 tangent,Vector3 bitangent)
        {
            Position = position;
            Normal = Vector3.Normalize(normal);
            TextureCoordinates = texCoords;
            Tangent = tangent;
            Bitangent = bitangent;
        }

        public VertexPositionNormalTextureTangentBitangent(ref Vector4 position, ref VertexPositionNormalTextureTangentBitangent v)
        {
            Position = Numerics.Vector.ToVec3(position);
            Normal = v.Normal;
            TextureCoordinates = v.TextureCoordinates;
            Tangent = v.Tangent;
            Bitangent = v.Bitangent;
        }

        public VertexPositionNormalTextureTangentBitangent(Vector4 position, VertexPositionNormalTextureTangentBitangent v)
        {
            Position = Numerics.Vector.ToVec3(position);
            Normal = v.Normal;
            TextureCoordinates = v.TextureCoordinates;
            Tangent = v.Tangent;
            Bitangent = v.Bitangent;
        }

        public byte GetSizeInBytes(){
            return SizeInBytes;
        }

        public Vector4 GetPosition(){
            return new Vector4(Position, 1.0f);
        }
    }
}
