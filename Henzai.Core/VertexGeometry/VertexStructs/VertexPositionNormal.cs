using System.Numerics;

namespace Henzai.Core.VertexGeometry
{
    public struct VertexPositionNormal : VertexRuntime, VertexTangentspace
    {
        public const byte SizeInBytes = 24;
        public const byte PositionOffset = 0;
        public const byte NormalOffset = 12;
        public const byte ElementCount = 2;
        public const VertexRuntimeTypes HenzaiType = VertexRuntimeTypes.VertexPositionNormal;

        public Vector3 Position;
        public Vector3 Normal;

        public VertexPositionNormal(Vector3 position, Vector3 normal)
        {
            Position = position;
            Normal = Vector3.Normalize(normal);
        }

        public byte GetSizeInBytes(){
            return SizeInBytes;
        }

        public Vector3 GetPosition(){
            return Position;
        }

        public Vector3 GetNormal()
        {
            return Normal;
        }
    }
}
