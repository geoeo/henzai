using System.Numerics;   

    namespace Henzai.Runtime
    {
        public interface VertexRuntime {
            byte GetSizeInBytes();
        }

        public interface VertexLocateable {
            Vector3 GetPosition();
        }

        public interface VertexTangentspace : VertexLocateable {
            Vector3 GetNormal();
        }

    }
