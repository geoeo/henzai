using System;
using System.Numerics;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Henzai.Core.Geometry
{
    
    public sealed class GeometryDefinition<T> where T : struct, VertexLocateable
    {
        private T[] _vertices;
        /// <summary>
        /// Holds a continuous list of verticies which pass the frustum test.
        /// The array may be larger than actual vertex count.
        /// </summary>
        private T[] _culledVertices;
        /// <summary>
        /// The number of verticies that passed the frustum test.
        /// Gives the blit range of <see cref="culledVerticies"/>
        /// </summary>
        private int _numberOfValidVertices;
        private ushort[] _meshIndices;
        /// <summary>
        /// Holds a continuous list of indices which pass the frustum test.
        /// The array may be larger than actual index count.
        /// </summary>
        private ushort[] _culledMeshIndices;
        /// <summary>
        /// The number of indices that passed the frustum test.
        /// Gives the blit range of <see cref="culledMeshIndices"/>
        /// </summary>
        private int _numberOfValidIndicies;

        public GeometryDefinition(T[] vertices, ushort[] meshIndices){
            Debug.Assert(vertices != null);
            Debug.Assert(meshIndices != null);

            _vertices = vertices;
            _culledVertices = new T[vertices.Length];
            CopyArrayOfStructs(_vertices, _culledVertices);
            _numberOfValidVertices = vertices.Length;

            _meshIndices = meshIndices;
            _culledMeshIndices = new ushort[meshIndices.Length];
            Buffer.BlockCopy(meshIndices,0,_culledMeshIndices,0,meshIndices.Length * sizeof(ushort));
            _numberOfValidIndicies = meshIndices.Length;
        }

        public GeometryDefinition(T[] vertices){
            Debug.Assert(vertices != null);

            _vertices = vertices;
            _culledVertices = new T[vertices.Length];
            CopyArrayOfStructs(_vertices, _culledVertices);
            _numberOfValidVertices = vertices.Length;

            _meshIndices = null;
            _numberOfValidIndicies = 0;
            _culledMeshIndices = null;
        }

        public T[] GetValidVertices(){
            return _culledVertices; 
        }

        public ushort[] GetValidIndices(){
            return _culledMeshIndices; 
        }

        public void SetMeshIndices(ushort[] meshIndices){
            Debug.Assert(meshIndices != null);

            if(_culledMeshIndices == null){
                _culledMeshIndices = new ushort[meshIndices.Length];
            }

            _meshIndices = meshIndices;
            _numberOfValidIndicies = meshIndices.Length;
            Buffer.BlockCopy(meshIndices,0,_culledMeshIndices,0,meshIndices.Length * sizeof(ushort));
        }

        //TODO: Look into optimizing this
        private void CopyArrayOfStructs(T[] source, T[] target) {
            Debug.Assert(source.Length == target.Length);
            
            for(int i = 0; i < source.Length; i++){
                target[i] = source[i];
            }
        }
    
    }
}