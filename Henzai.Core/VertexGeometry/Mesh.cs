using System;
using System.Numerics;
using System.Diagnostics;
using System.Collections.Generic;
using Henzai.Core.Acceleration;

namespace Henzai.Core.VertexGeometry
{
    //TODO: @Investigate -> Make this a struct; Remove "valid" parts as culling triangles is not really feasable 
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
        public T[] Vertices => _vertices;
        public ushort[] Indices => _indices;
        public T[] ValidVertices => _validVertices;
        public T[] CleanVertices => _vertexClean;
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
        public bool IsCulled => ValidIndexCount == 0;
        public ref Matrix4x4 World => ref _world;
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

            _indexClean = null;
            _vertexClean = null;
        }

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

            _indexClean = new ushort[IndexCount];
            _vertexClean = new T[VertexCount];
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