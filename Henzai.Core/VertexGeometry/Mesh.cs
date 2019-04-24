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
        public int ValidTriangleCount {get; set;}
        public T[] Vertices => _vertices;
        public ushort[] Indices => _indices;
        public T[] ValidVertices => _validVertices;
        public ushort[] ValidIndices => _validIndices;
        public IndexedTriangleEngine<T>[] Triangles => _triangles;
        public int VertexCount => _vertices.Length;
        public int IndexCount => _indices.Length;
        public bool IsCulled => ValidIndexCount == 0;
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

            _triangles = new IndexedTriangleEngine<T>[IndexCount/3];
            for(int i = 0; i < VertexCount-1; i+=3)
                _triangles[i/3] = new IndexedTriangleEngine<T>(i, i+1, i+2, this);
            ValidTriangleCount = _triangles.Length;

            // _processedIndicesMap = new Dictionary<ushort, bool>();
        
        }

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
            for(int i = 0; i < IndexCount-1; i+=3){
                var i0 = _indices[i];
                var i1 = _indices[i+1];
                var i2 = _indices[i+2];  
                _triangles[i/3] = new IndexedTriangleEngine<T>(i0, i1, i2, this);
            }
            ValidTriangleCount = _triangles.Length;

            //  _processedIndicesMap = new Dictionary<ushort, bool>();
        }

        public void SetNewWorldTransformation(ref Matrix4x4 world){
            _world = world;
        }

        public void SetNewWorldTranslation(ref Vector3 translation){
            _world.Translation = translation;
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