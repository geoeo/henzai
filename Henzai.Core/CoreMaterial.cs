using System;
using System.Numerics;
namespace Henzai.Core
{
    public interface CoreMaterial
    {
        void ApplyMaterialDataInto(Vector4[] colors, string[] textureStrings, string[] cubemapStrings);
    }
}
