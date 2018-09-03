using System;
using System.Numerics;
using Henzai.Core.Geometry;

namespace Henzai.Geometry
{
    /// <summary>
    /// Mesh holds Vertex and Texture/Color information
    /// </summary>
    public sealed class Mesh<T> where T : struct, VertexLocateable
    {
        private GeometryDefinition<T> _geometryDefinition;

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

        public int GetNumberOfValidVertices => _geometryDefinition.GetNumberOfValidVertices;
        public int GetNumberOfValidIndices => _geometryDefinition.GetNumberOfValidIndices;
        private Matrix4x4 _world = Matrix4x4.Identity;
        public ref Matrix4x4 World => ref _world;

        public Material material {private get; set;}

        public Mesh(T[] meshDefinition)
        {
            _geometryDefinition = new GeometryDefinition<T>(meshDefinition);
            this.material = new Material();
        }

        public Mesh(T[] meshDefinition, ushort[] indices)
        {
            _geometryDefinition = new GeometryDefinition<T>(meshDefinition,indices);
            this.material = new Material();
        }

        public Mesh(T[] meshDefinition, Material material)
        {

            _geometryDefinition = new GeometryDefinition<T>(meshDefinition);
            this.material = material;
        }

        public Material TryGetMaterial(){
            if(material == null)
                throw new NullReferenceException("The material you are trying to access is null");
            return material;
        }

        public Material GetMaterialRuntime(){
            return material;
        }

        public void SetNewWorldTransformation(ref Matrix4x4 world){
            _world = world;
        }

        public void SetNewWorldTranslation(ref Vector3 translation){
            _world.Translation = translation;
        }

    }

}