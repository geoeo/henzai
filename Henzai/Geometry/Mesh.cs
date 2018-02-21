using System;
using System.Numerics;
using Veldrid;

namespace Henzai.Geometry
{
    /// <summary>
    /// Mesh hold Position, Texture and Normal Information
    /// </summary>
    public class Mesh<T> where T : struct
    {
        public readonly T[] vertices;

        public Mesh(T[] meshDefinition)
        {
            vertices = meshDefinition;
        }

    }

}