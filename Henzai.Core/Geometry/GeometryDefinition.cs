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
        public int NumberOfValidVertices {get; set;}
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
        public int NumberOfValidIndicies {get; set;}

        public GeometryDefinition(T[] vertices, ushort[] meshIndices){
            Debug.Assert(vertices != null);
            Debug.Assert(meshIndices != null);

            _vertices = vertices;
            _culledVertices = new T[vertices.Length];
            CopyArrayOfStructs(_vertices, _culledVertices);
            NumberOfValidVertices = vertices.Length;

            _meshIndices = meshIndices;
            _culledMeshIndices = new ushort[meshIndices.Length];
            Buffer.BlockCopy(meshIndices,0,_culledMeshIndices,0,meshIndices.Length * sizeof(ushort));
            NumberOfValidIndicies = meshIndices.Length;
        }

        public GeometryDefinition(T[] vertices){
            Debug.Assert(vertices != null);

            _vertices = vertices;
            _culledVertices = new T[vertices.Length];
            CopyArrayOfStructs(_vertices, _culledVertices);
            NumberOfValidVertices = vertices.Length;

            _meshIndices = null;
            NumberOfValidIndicies = 0;
            _culledMeshIndices = null;
        }
        public T[] GetVertices => _vertices;
        public ushort[] GetIndices => _meshIndices;
        public T[] GetValidVertices => _culledVertices;
        public ushort[] GetValidIndices => _culledMeshIndices;

        public int GetNumberOfValidVertices => NumberOfValidVertices;
        public int GetNumberOfValidIndices => NumberOfValidIndicies;

        public bool IsCulled => NumberOfValidIndicies == 0;

        public void SetMeshIndices(ushort[] meshIndices){
            Debug.Assert(meshIndices != null);

            if(_culledMeshIndices == null)
                _culledMeshIndices = new ushort[meshIndices.Length];

            _meshIndices = meshIndices;
            NumberOfValidIndicies = meshIndices.Length;
            Buffer.BlockCopy(meshIndices,0,_culledMeshIndices,0,meshIndices.Length * sizeof(ushort));
        }

        //TODO: Look into optimizing this
        private void CopyArrayOfStructs(T[] source, T[] target) {
            Debug.Assert(source.Length == target.Length);
            
            for(int i = 0; i < source.Length; i++)
                target[i] = source[i];        
        }    
    }
}