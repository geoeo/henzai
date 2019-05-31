using System;
using System.Numerics;
using Henzai.Core.VertexGeometry;

namespace Henzai.Core.Acceleration
{   
    //TODO: Try bounding meshes insteade of triangles
    /// This struct serves as a BVH primitive
    public struct MeshBVH<T> : AxisAlignedBoundable where T : struct, VertexLocateable
    {
        private AABB _aabb;

        public bool IsBoundable() => true;

        public AABB GetBounds()
        {
            return _aabb;
        }
    }
}