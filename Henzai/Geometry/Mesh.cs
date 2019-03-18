using System;
using System.Numerics;
using Henzai.Core.VertexGeometry;

namespace Henzai.Geometry
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
        //TODO move this to Model class as a list of Materials with 1:1 correspondence
        //TODO make mesh to hold a list of Geometry Definiions (?)
        //public Material material {private get; set;}
        public bool IsCulled => _geometryDefinition.IsCulled;

        public Mesh(T[] meshDefinition)
        {
            _geometryDefinition = new GeometryDefinition<T>(meshDefinition);
            //material = new Material();
        }

        public Mesh(T[] meshDefinition, ushort[] indices)
        {
            _geometryDefinition = new GeometryDefinition<T>(meshDefinition, indices);
            //material = new Material();
        }

        //public Mesh(T[] meshDefinition)
        //{

        //    _geometryDefinition = new GeometryDefinition<T>(meshDefinition);
        //    //material = material;
        //}

        public void SetNewWorldTransformation(ref Matrix4x4 world){
            _world = world;
        }

        public void SetNewWorldTranslation(ref Vector3 translation){
            _world.Translation = translation;
        }

    }

}