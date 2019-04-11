using System.Numerics;
using Henzai.Core.Numerics;

namespace Henzai.Core.VertexGeometry
{
    public struct VertexPositionNormalTexture : VertexRuntime, VertexLocateable
    {
        public const byte SizeInBytes = 32;
        public const byte PositionOffset = 0;
        public const byte NormalOffset = 12;
        public const byte TextureCoordinatesOffset = 24;
        public const byte ElementCount = 3;
        public const VertexRuntimeTypes HenzaiType = VertexRuntimeTypes.VertexPositionNormalTexture;

        public readonly Vector3 Position;
        public readonly Vector3 Normal;
        public Vector2 TextureCoordinates;

        public VertexPositionNormalTexture(Vector3 position, Vector3 normal, Vector2 texCoords)
        {
            Position = position;
            Normal = Vector3.Normalize(normal);
            TextureCoordinates = texCoords;
        }

        public VertexPositionNormalTexture(ref Vector4 position, ref VertexPositionNormalTexture v){
            Position = Numerics.Vector.ToVec3(ref position);
            Normal = v.Normal;
            TextureCoordinates = v.TextureCoordinates;
        }

        public VertexPositionNormalTexture(Vector4 position, VertexPositionNormalTexture v){
            Position = Numerics.Vector.ToVec3(ref position);
            Normal = v.Normal;
            TextureCoordinates = v.TextureCoordinates;
        }

        public byte GetSizeInBytes(){
            return SizeInBytes;
        }

        public Vector4 GetPosition(){
            return new Vector4(Position, 1.0f);
        }
    }
}
