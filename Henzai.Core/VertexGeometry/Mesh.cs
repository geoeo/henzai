using System;
using System.Numerics;

namespace Henzai.Core.VertexGeometry
{
    //TODO Could remove this class entierly. World has to move into GeometryDefinition
    /// <summary>
    /// Mesh holds Vertex and Texture/Color information
    /// </summary>
    public sealed class Mesh<T> where T : struct, VertexLocateable
    {
        private readonly GeometryDefinition<T> _geometryDefinition;
        public GeometryDefinition<T> GeometryDefinition => _geometryDefinition;
        public T[] Vertices => _geometryDefinition.GetValidVertices;
        public ushort[] MeshIndices => _geometryDefinition.GetValidIndices;

        public int VertexCount => _geometryDefinition.VertexCount;
        public int IndicesCount => _geometryDefinition.IndicesCount;
        public int FaceCount => _geometryDefinition.VertexCount / 3;
        public int ValidVertexCount => _geometryDefinition.ValidVertexCount;
        public int ValidIndicesCount => _geometryDefinition.ValidIndicesCount;
        public int ValidFaceCount => _geometryDefinition.ValidVertexCount / 3;
        private Matrix4x4 _world = Matrix4x4.Identity;
        public ref Matrix4x4 World => ref _world;
        public bool IsCulled => _geometryDefinition.IsCulled;

        public Mesh(T[] meshDefinition)
        {
            _geometryDefinition = new GeometryDefinition<T>(meshDefinition);
        }

        public Mesh(T[] meshDefinition, ushort[] indices)
        {
            _geometryDefinition = new GeometryDefinition<T>(meshDefinition, indices);
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
            foreach (ushort index in MeshIndices)
                Console.WriteLine($"{index}");
        }

    }

}