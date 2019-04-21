using System.Numerics;

namespace Henzai.Core.Raytracing
{
    public interface Hitable
    {
        bool HasIntersection(Ray ray);
        //TODO investigate ValueTuple
        (bool, float) Intersect(Ray ray);
        bool IntersectionAcceptable(bool hasIntersection, float t, float dotView, Vector4 point);
        Vector4 NormalForSurfacePoint(Vector4 point);
        bool IsObstructedBySelf(Ray ray);
        float TMin();
        float TMax();
    }
}