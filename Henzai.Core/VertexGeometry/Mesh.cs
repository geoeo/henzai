using System;
using System.Numerics;
using System.Diagnostics;
using System.Collections.Generic;
using Henzai.Core.Acceleration;

namespace Henzai.Core.VertexGeometry
{
    //TODO Could remove this class entierly. World has to move into GeometryDefinition
    /// <summary>
    /// Mesh holds Vertex and Texture/Color information
    /// </summary>
    public sealed class Mesh<T> where T : struct, VertexLocateable
    {
        private readonly T[] _vertices;
        /// <summary>
        /// Holds a continuous list of verticies which pass the frustum test.
        /// The array may be larger than actual vertex count.
        /// </summary>
        private readonly T[] _validVertices;
        private readonly ushort[] _indices;
        /// <summary>
        /// Holds a continuous list of indices which pass the frustum test.
        /// The array may be larger than actual index count.
        /// </summary>
        private readonly ushort[] _validIndices;
        private readonly IndexedTriangleEngine<T>[] _triangles;
        // private readonly Dictionary<ushort, bool> _processedIndicesMap;
        private ushort[] _indexClean; 
        private T[] _vertexClean;
        private Matrix4x4 _world = Matrix4x4.Identity;

        /// <summary>
        /// The number of indices that passed the frustum test.
        /// Gives the blit range of <see cref="culledMeshIndices"/>
        /// </summary>
        public int ValidIndexCount {get; set;}
        /// <summary>
        /// The number of verticies that passed the frustum test.
        /// Gives the blit range of <see cref="culledVerticies"/>
        /// </summary>
        public int ValidVertexCount {get; set;}
        public int TriangleCount => _triangles.Length;
        public T[] Vertices => _vertices;
        public ushort[] Indices => _indices;
        public T[] ValidVertices => _validVertices;
        public T[] CleanVertices => _vertexClean;
        public IndexedTriangleEngine<T>[] Triangles => _triangles;
        /// <summary>
        /// Disclaimer: In general you want ValidVertexCount
        /// </summary>
        public ushort[] ValidIndices => _validIndices;
        public ushort[] CleanIndices => _indexClean;
        public int VertexCount => _vertices.Length;
        /// <summary>
        /// Disclaimer: In general you want ValidIndexCount
        /// </summary>
        public int IndexCount => _indices.Length;
        public bool AABBIsValid {get; set;}
        public bool IsCulled => ValidIndexCount == 0 || !AABBIsValid;
        public ref Matrix4x4 World => ref _world;
        // public Dictionary<ushort, bool> ProcessedIndicesMap => _processedIndicesMap;

        public Mesh(T[] vertices)
        {
            Debug.Assert(vertices != null);

            _vertices = vertices;
            _validVertices = new T[vertices.Length];
            Array.Copy(_vertices, _validVertices, _vertices.Length);
            ValidIndexCount = vertices.Length;

            _indices = null;
            ValidVertexCount = VertexCount;
            ValidIndexCount = 0;
            _validIndices = null;

            // _triangles = new IndexedTriangleEngine<T>[VertexCount/3];
            // for(int i = 0; i < VertexCount; i+=3)
            //     _triangles[i/3] = new IndexedTriangleEngine<T>(i, i+1, i+2, this);
            _triangles = null;
            _indexClean = null;
            _vertexClean = null;
            AABBIsValid = true;

            // _processedIndicesMap = new Dictionary<ushort, bool>();
        
        }

        //TODO: Support trianlge strips
        //TODO: Support instancing
        public Mesh(T[] vertices, ushort[] indices)
        {
            Debug.Assert(vertices != null);
            Debug.Assert(indices != null);

            _vertices = vertices;
            _validVertices = new T[vertices.Length];
            Array.Copy(_vertices, _validVertices, _vertices.Length);
            ValidVertexCount = vertices.Length;

            _indices = indices;
            _validIndices = new ushort[indices.Length];
            Buffer.BlockCopy(indices, 0, _validIndices, 0, indices.Length * sizeof(ushort));
            ValidIndexCount = indices.Length;

            _triangles = new IndexedTriangleEngine<T>[IndexCount/3];
            for(int i = 0; i < IndexCount; i+=3){
                var i0 = _indices[i];
                var i1 = _indices[i+1];
                var i2 = _indices[i+2];  
                _triangles[i/3] = new IndexedTriangleEngine<T>(i0, i1, i2, this);
            }

            _indexClean = new ushort[IndexCount];
            _vertexClean = new T[VertexCount];

            AABBIsValid = true;

            //  _processedIndicesMap = new Dictionary<ushort, bool>();
        }

        public void SetNewWorldTransformation(ref Matrix4x4 world){
            _world = world;
            for (int i = 0; i < _triangles.Length; i++){
                var tri = _triangles[i];
                _triangles[i] = new IndexedTriangleEngine<T>(ref tri, ref _world);
            }
        }

        public void SetNewWorldTranslation(ref Vector3 translation){
            _world.Translation = translation;
            for (int i = 0; i < _triangles.Length; i++){
                var tri = _triangles[i];
                _triangles[i] = new IndexedTriangleEngine<T>(ref tri, ref _world);
            } 
        }

        public void PrintAllVertexPositions(){
            foreach(T vertex in Vertices){
                var pos = vertex.GetPosition();
                Console.WriteLine($"X:{pos.X} Y:{pos.Y} Z:{pos.Z}");
            }
        }

        public void PrintAllVertexIndices(){
            foreach (ushort index in _indices)
                Console.WriteLine($"{index}");
        }

    }

}