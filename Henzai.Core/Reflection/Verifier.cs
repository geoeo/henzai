using Henzai.Core.VertexGeometry;

namespace Henzai.Core.Reflection
{
    public static class Verifier
    {

        public static bool verifyVertexStruct<T>(VertexRuntimeTypes vertexType) where T : struct
        {
            string typeName = typeof(T).Name;
            string enumName = vertexType.ToString();

            return typeName.Equals(enumName);
        }
    }
}
