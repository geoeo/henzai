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
        public readonly T[] vertices;
        /// <summary>
        /// Holds a continuous list of verticies which pass the frustum test.
        /// The array may be larger than actual vertex count.
        /// </summary>
        public T[] culledVerticies;
        /// <summary>
        /// The number of verticies that passed the frustum test.
        /// Gives the blit range of <see cref="culledVerticies"/>
        /// </summary>
        public int numberOfValidVerticies {get; set;}
        public ushort[] meshIndices {get; set;}
        /// <summary>
        /// Holds a continuous list of indices which pass the frustum test.
        /// The array may be larger than actual index count.
        /// </summary>
        public ushort[] culledMeshIndices {get; set;}
        /// <summary>
        /// The number of indices that passed the frustum test.
        /// Gives the blit range of <see cref="culledMeshIndices"/>
        /// </summary>
        public int numberOfValidIndicies {get; set;}

        private Matrix4x4 _world = Matrix4x4.Identity;
        public ref Matrix4x4 World => ref _world;

        public Material material {private get; set;}

        public Mesh(T[] meshDefinition)
        {
            vertices = meshDefinition;
            culledVerticies = new T[vertices.Length];
            meshIndices = null;
            culledMeshIndices = null;
            this.material = new Material();
        }

        public Mesh(T[] meshDefinition, ushort[] indices)
        {
            vertices = meshDefinition;
            culledVerticies = new T[vertices.Length];
            meshIndices = indices;
            culledMeshIndices = new ushort[meshIndices.Length];
            this.material = new Material();
        }

        public Mesh(T[] meshDefinition, Material material)
        {
            vertices = meshDefinition;
            culledVerticies = new T[vertices.Length];
            meshIndices = null;
            culledMeshIndices = null;
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