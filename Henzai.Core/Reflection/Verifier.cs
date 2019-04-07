using Henzai.Core.VertexGeometry;
using Henzai.Core.Materials;

namespace Henzai.Core.Reflection
{
    public static class Verifier
    {

        public static bool VerifyVertexStruct<T>(VertexRuntimeTypes vertexType) where T : struct
        {
            string typeName = typeof(T).Name;
            string enumName = vertexType.ToString();

            return typeName.Equals(enumName);
        }

        public static bool VerifyMaterialType<T>(MaterialTypes materialType)
        {
            string typeName = typeof(T).Name;
            string enumName = materialType.ToString();

            return typeName.Equals(enumName);
        }
    }
}
