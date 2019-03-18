using System.Numerics;

namespace Henzai.Core.VertexGeometry
{
    /// <summary>
    /// Mesh holds Vertex and Texture/Color information
    /// </summary>
    public sealed class Mesh<T> where T : struct, VertexLocateable
    {
        private readonly GeometryDefinition<T> _geometryDefinition;
        public GeometryDefinition<T> GeometryDefinition => _geometryDefinition;
        public T[] Vertices => _geometryDefinition.GetValidVertices;
        public ushort[] MeshIndices {
            get
            {
                return _geometryDefinition.GetValidIndices;
            }

            set
            {
                _geometryDefinition.SetMeshIndices(value);
            }
        }

        public int NumberOfValidVertices => _geometryDefinition.GetNumberOfValidVertices;
        public int NumberOfValidIndices => _geometryDefinition.GetNumberOfValidIndices;
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

    }

}