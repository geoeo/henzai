using System;
using System.Numerics;
using Henzai.Core.VertexGeometry;

namespace Henzai.Core.Acceleration
{   
    /// This struct serves as a BVH primitive
    public struct MeshBVH<T> : AxisAlignedBoundable where T : struct, VertexLocateable
    {
        private AABB _aabb;
        public bool AABBIsValid;
        /// <summary>
        /// The BVH building process will reorder the structs. Therefore we need a way to index the other data parts
        /// </summary>
        public int ModelRuntimeIndex;
        /// <summary>
        /// The BVH building process will reorder the structs. Therefore we need a way to index the other data parts
        /// </summary>
        public int MeshRuntimeIndex;

        public MeshBVH(Mesh<T> mesh){
            AABBIsValid = true;    

            ModelRuntimeIndex = -1;
            MeshRuntimeIndex = -1;

            var vertices = mesh.Vertices;
            var v = vertices[0].GetPosition();
            var xMin = v.X;
            var yMin = v.Y;
            var zMin = v.Z;

            var xMax = v.X;
            var yMax = v.Y;
            var zMax = v.Z;

            for(int i = 1; i < vertices.Length; i++){
                var vert = vertices[i].GetPosition();
                var x = vert.X;
                var y = vert.Y;
                var z = vert.Z;

                if(x < xMin)
                    xMin = x;
                else if(x > xMax)
                    xMax = x;

                if(y < yMin)
                    yMin = y;
                else if(y > yMax)
                    yMax = y;

                if(z < zMin)
                    zMin = z;
                else if(x > zMax)
                    zMax = z;
            }

            var pMin = new Vector4(xMin, yMin, zMin, 1.0f);
            var pMax = new Vector4(xMax, yMax, zMax, 1.0f);
            _aabb = new AABB(pMin, pMax);
        }

        public MeshBVH(Mesh<T> mesh, ref Matrix4x4 world){    
            AABBIsValid = true;    

            ModelRuntimeIndex = -1;
            MeshRuntimeIndex = -1;

            var vertices = mesh.Vertices;
            var v = vertices[0].GetPosition();
            v = Vector4.Transform(v, world);

            var xMin = v.X;
            var yMin = v.Y;
            var zMin = v.Z;

            var xMax = v.X;
            var yMax = v.Y;
            var zMax = v.Z;

            for(int i = 1; i < vertices.Length; i++){
                var vert = vertices[i].GetPosition();
                vert = Vector4.Transform(vert, world);

                var x = vert.X;
                var y = vert.Y;
                var z = vert.Z;

                if(x < xMin)
                    xMin = x;
                else if(x > xMax)
                    xMax = x;

                if(y < yMin)
                    yMin = y;
                else if(y > yMax)
                    yMax = y;

                if(z < zMin)
                    zMin = z;
                else if(x > zMax)
                    zMax = z;
            }

            var pMin = new Vector4(xMin, yMin, zMin, 1.0f);
            var pMax = new Vector4(xMax, yMax, zMax, 1.0f);
            _aabb = new AABB(pMin, pMax);
        }

        public bool IsBoundable() => true;

        public AABB GetBounds()
        {
            return _aabb;
        }
    }
}