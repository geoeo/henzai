using System.Numerics;   

    namespace Henzai.Core.VertexGeometry
    {
        public interface VertexRuntime {
            byte GetSizeInBytes();
        }

        public interface VertexLocateable {
            Vector4 GetPosition();
        }

        public interface VertexTangentspace : VertexLocateable {
            Vector3 GetNormal();
        }

    }
