using System;
using System.Numerics;
using Henzai.Core.VertexGeometry;

namespace Henzai.Core.Acceleration
{
    /// This struct serves as a BVH primitive
    public struct IndexedTriangleEngine<T> : AxisAlignedBoundable where T : struct, VertexLocateable
    {
        private readonly int _i0;
        private readonly int _i1;
        private readonly int _i2;
        private readonly Mesh<T> _mesh;
        private readonly AABB _aabb;

        public Mesh<T> Mesh => _mesh;

        public IndexedTriangleEngine(int i0, int i1, int i2, Mesh<T> mesh){
            _i0 = i0;
            _i1 = i1;
            _i2 = i2;
            _mesh = mesh;
            
            //TODO: Might have to apply the World transform aswell!!
            var vertices = mesh.Vertices;
            var v0 = vertices[_i0].GetPosition();
            var v1 = vertices[_i1].GetPosition();
            var v2 = vertices[_i2].GetPosition();

            var pMin = new Vector4(MathF.Min(v0.X, MathF.Min(v1.X, v2.X)), MathF.Min(v0.Y, MathF.Min(v1.Y, v2.Y)), MathF.Min(v0.Z, MathF.Min(v1.Z, v2.Z)), 1.0f);
            var pMax = new Vector4(MathF.Max(v0.X, MathF.Max(v1.X, v2.X)), MathF.Max(v0.Y, MathF.Max(v1.Y, v2.Y)), MathF.Max(v0.Z, MathF.Max(v1.Z, v2.Z)), 1.0f);
            _aabb = new AABB(pMin, pMax);
        }

        public bool IsBoundable() => true;

        public AABB GetBounds()
        {
            return _aabb;
        }
    }
}