using System;
using System.Numerics;
using Veldrid;

namespace Henzai.Geometry
{
    /// <summary>
    /// Mesh holds Vertex and Texture/Color information
    /// </summary>
    public sealed class Mesh<T> where T : struct
    {
        public readonly T[] vertices;
        public uint[] meshIndices {get; set;}

        public Material material {private get; set;}

        public Mesh(T[] meshDefinition)
        {
            vertices = meshDefinition;
            meshIndices = null;
            this.material = new Material();
        }

        public Mesh(T[] meshDefinition, uint[] indices)
        {
            vertices = meshDefinition;
            meshIndices = indices;
            this.material = new Material();
        }

        public Mesh(T[] meshDefinition, Material material)
        {
            vertices = meshDefinition;
            meshIndices = null;
            this.material = material;
        }

        public Material TryGetMaterial(){
            if(material == null)
                throw new NullReferenceException("The material you are trying to access is null");
            return material;
        }

    }

}