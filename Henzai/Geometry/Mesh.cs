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

        public Material material {private get; set;}

        public Mesh(T[] meshDefinition)
        {
            vertices = meshDefinition;
            this.material = null;
        }

        public Mesh(T[] meshDefinition, Material material)
        {
            vertices = meshDefinition;
            this.material = material;
        }

        public Material TryGetMaterial(){
            if(material == null)
                throw new NullReferenceException("The material you are trying to access is null");
            return material;
        }

    }

}