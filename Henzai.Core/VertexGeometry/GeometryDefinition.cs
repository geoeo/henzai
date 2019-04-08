using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Henzai.Core.VertexGeometry
{
    
    public sealed class GeometryDefinition<T> where T : struct, VertexLocateable
    {
        private readonly T[] _vertices;
        /// <summary>
        /// Holds a continuous list of verticies which pass the frustum test.
        /// The array may be larger than actual vertex count.
        /// </summary>
        private readonly T[] _validVertices;
        private readonly ushort[] _meshIndices;
        /// <summary>
        /// Holds a continuous list of indices which pass the frustum test.
        /// The array may be larger than actual index count.
        /// </summary>
        private readonly ushort[] _validMeshIndices;

        /// <summary>
        /// The number of indices that passed the frustum test.
        /// Gives the blit range of <see cref="culledMeshIndices"/>
        /// </summary>
        public int NumberOfValidIndicies {get; set;}
        /// <summary>
        /// The number of verticies that passed the frustum test.
        /// Gives the blit range of <see cref="culledVerticies"/>
        /// </summary>
        public int NumberOfValidVertices {get; set;}
        public T[] GetVertices => _vertices;
        public ushort[] GetIndices => _meshIndices;
        public T[] GetValidVertices => _validVertices;
        public ushort[] GetValidIndices => _validMeshIndices;
        public int VertexCount => _vertices.Length;
        public int IndicesCount => _meshIndices.Length;
        public int ValidVertexCount => NumberOfValidVertices;
        public int ValidIndicesCount => NumberOfValidIndicies;
        public bool IsCulled => NumberOfValidIndicies == 0;
        /// <summary>
        /// A datastrcture used for geometry pre/post processing
        /// Used for culling operations
        /// </summary>
        public Dictionary<ushort, bool> ProcessedIndicesMap { get; }

        public GeometryDefinition(T[] vertices, ushort[] meshIndices){
            Debug.Assert(vertices != null);
            Debug.Assert(meshIndices != null);

            _vertices = vertices;
            _validVertices = new T[vertices.Length];
            CopyArrayOfStructs(_vertices, _validVertices);
            NumberOfValidVertices = vertices.Length;

            _meshIndices = meshIndices;
            _validMeshIndices = new ushort[meshIndices.Length];
            Buffer.BlockCopy(meshIndices,0,_validMeshIndices,0,meshIndices.Length * sizeof(ushort));
            NumberOfValidIndicies = meshIndices.Length;

            ProcessedIndicesMap = new Dictionary<ushort, bool>();
        }

        public GeometryDefinition(T[] vertices){
            Debug.Assert(vertices != null);

            _vertices = vertices;
            _validVertices = new T[vertices.Length];
            CopyArrayOfStructs(_vertices, _validVertices);
            NumberOfValidVertices = vertices.Length;

            _meshIndices = null;
            NumberOfValidIndicies = 0;
            _validMeshIndices = null;

            ProcessedIndicesMap = new Dictionary<ushort, bool>();
        }

        //TODO: Look into optimizing this
        private void CopyArrayOfStructs(T[] source, T[] target) {
            Debug.Assert(source.Length == target.Length);
            
            for(int i = 0; i < source.Length; i++)
                target[i] = source[i];        
        }    
    }
}