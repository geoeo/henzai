using System;
using System.Numerics;
using System.Diagnostics;
using System.Collections.Generic;
using Henzai.Core.Acceleration;

namespace Henzai.Core.VertexGeometry
{
    //TODO: @Investigate -> Make this a struct
    /// <summary>
    /// Mesh holds Vertex and Texture/Color information
    /// </summary>
    public sealed class Mesh<T> where T : struct, VertexLocateable
    {
        private readonly T[] _vertices;
        private readonly ushort[] _indices;
        private Matrix4x4 _world = Matrix4x4.Identity;
        public T[] Vertices => _vertices;
        public ushort[] Indices => _indices;
        public int VertexCount => _vertices.Length;
        public int IndexCount => _indices.Length;
        public ref Matrix4x4 World => ref _world;
        public Mesh(T[] vertices)
        {
            Debug.Assert(vertices != null);

            _vertices = vertices;
            _indices = null;
        }

        //TODO: Support instancing
        public Mesh(T[] vertices, ushort[] indices)
        {
            Debug.Assert(vertices != null);
            Debug.Assert(indices != null);

            _vertices = vertices;
            _indices = indices;
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