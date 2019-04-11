using System.Numerics;
using Henzai.Core.Numerics;

namespace Henzai.Core.VertexGeometry
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

        public VertexPositionTexture(ref Vector4 position, ref VertexPositionTexture v)
        {
            Position = Numerics.Vector.ToVec3(position);
            TextureCoordinates = v.TextureCoordinates;
        }

        public VertexPositionTexture(Vector4 position, VertexPositionTexture v)
        {
            Position = Numerics.Vector.ToVec3(position);
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
