using System.Numerics;
using SixLabors.ImageSharp.PixelFormats;


namespace Henzai.Core.Materials
{
    public struct RaytraceMaterial
    {
        public readonly Vector4 Albedo;
        public readonly Vector4 Emittance;

        public RaytraceMaterial(Vector4 color)
        {
            Albedo = color; Emittance = Vector4.Zero;
        }

        public RaytraceMaterial(Vector4 color, Vector4 emittance)
        {
            Albedo = color; Emittance = emittance;
        }

        public RaytraceMaterial(Rgba32 color)
        {
            Albedo = color.ToVector4();
            Emittance = Vector4.Zero;
        }

        public RaytraceMaterial(Rgba32 color, float albedoFactor, Rgba32 emmitance, float emmitingFactor)
        {
            Albedo = color.ToVector4()*albedoFactor;
            Emittance = emmitance.ToVector4()*emmitingFactor;
        }
    }
}
