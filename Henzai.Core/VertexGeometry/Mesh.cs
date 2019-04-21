using System;
using System.Numerics;
using System.Diagnostics;
using System.Collections.Generic;

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
        private readonly ushort[] _meshIndices;
        /// <summary>
        /// Holds a continuous list of indices which pass the frustum test.
        /// The array may be larger than actual index count.
        /// </summary>
        private readonly ushort[] _validMeshIndices;
        // private readonly AxisAlingedBoundable[] _triangles;

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
        public T[] Vertices => _vertices;
        public ushort[] Indices => _meshIndices;
        public T[] ValidVertices => _validVertices;
        public ushort[] ValidIndices => _validMeshIndices;
        public int VertexCount => _vertices.Length;
        public int IndicesCount => _meshIndices.Length;
        public int ValidVertexCount => NumberOfValidVertices;
        public int ValidIndicesCount => NumberOfValidIndicies;
        public bool IsCulled => NumberOfValidIndicies == 0;
        public int FaceCount => _vertices.Length / 3;
        public int ValidFaceCount => NumberOfValidVertices / 3;

        private Matrix4x4 _world = Matrix4x4.Identity;
        public ref Matrix4x4 World => ref _world;

        public Mesh(T[] vertices)
        {
            Debug.Assert(vertices != null);

            _vertices = vertices;
            _validVertices = new T[vertices.Length];
            Array.Copy(_vertices, _validVertices, _vertices.Length);
            NumberOfValidVertices = vertices.Length;

            _meshIndices = null;
            NumberOfValidIndicies = 0;
            _validMeshIndices = null;
        }

        public Mesh(T[] vertices, ushort[] indices)
        {
            Debug.Assert(vertices != null);
            Debug.Assert(indices != null);

            _vertices = vertices;
            _validVertices = new T[vertices.Length];
            Array.Copy(_vertices, _validVertices, _vertices.Length);
            NumberOfValidVertices = vertices.Length;

            _meshIndices = indices;
            _validMeshIndices = new ushort[indices.Length];
            Buffer.BlockCopy(indices,0,_validMeshIndices,0, indices.Length * sizeof(ushort));
            NumberOfValidIndicies = indices.Length;
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
            foreach (ushort index in _meshIndices)
                Console.WriteLine($"{index}");
        }

    }

}