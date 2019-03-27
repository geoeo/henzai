using System.Numerics;

namespace Henzai.Core.Materials
{
    public interface CoreMaterial
    {
        void ApplyMaterialDataInto(Vector4[] colors, string[] textureStrings, string[] cubemapStrings);
    }
}
