using System;
using System.Numerics;
using Henzai.Core.VertexGeometry;

namespace Henzai.Core.Acceleration
{
    /// This struct serves as a BVH primitive
    public struct IndexedTriangleEngine<T> : AxisAlignedBoundable where T : struct, VertexLocateable
    {
        public readonly int i0;
        public readonly int i1;
        public readonly int i2;
        public readonly Mesh<T> mesh;
        private AABB _aabb;

        //TODO: Once we start inserting/removing models at runtime this has to check for null
        public Mesh<T> Mesh => mesh;

        public IndexedTriangleEngine(int i0, int i1, int i2, Mesh<T> mesh){
            this.i0 = i0;
            this.i1 = i1;
            this.i2 = i2;
            this.mesh = mesh;
            
            var vertices = mesh.Vertices;
            var v0 = vertices[this.i0].GetPosition();
            var v1 = vertices[this.i1].GetPosition();
            var v2 = vertices[this.i2].GetPosition();

            var pMin = new Vector4(MathF.Min(v0.X, MathF.Min(v1.X, v2.X)), MathF.Min(v0.Y, MathF.Min(v1.Y, v2.Y)), MathF.Min(v0.Z, MathF.Min(v1.Z, v2.Z)), 1.0f);
            var pMax = new Vector4(MathF.Max(v0.X, MathF.Max(v1.X, v2.X)), MathF.Max(v0.Y, MathF.Max(v1.Y, v2.Y)), MathF.Max(v0.Z, MathF.Max(v1.Z, v2.Z)), 1.0f);
            _aabb = new AABB(pMin, pMax);
        }

        public IndexedTriangleEngine(ref IndexedTriangleEngine<T> triangle, ref Matrix4x4 world){
   
            i0 = triangle.i0;
            i1 = triangle.i1;
            i2 = triangle.i2;
            mesh = triangle.mesh;

            var vertices = mesh.Vertices;
            var v0 = vertices[i0].GetPosition();
            var v1 = vertices[i1].GetPosition();
            var v2 = vertices[i2].GetPosition();

            v0 = Vector4.Transform(v0, world);
            v1 = Vector4.Transform(v1, world);
            v2 = Vector4.Transform(v2, world);

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