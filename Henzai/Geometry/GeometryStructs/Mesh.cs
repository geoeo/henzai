using System;
using System.Numerics;
using Veldrid;

namespace Henzai.Geometry
{
    /// <summary>
    /// Mesh hold Position, Texture and Normal Information
    /// </summary>
    public class Mesh
    {
        public readonly VertexPositionNormalTexture[] vertices;

        public Mesh(VertexPositionNormalTexture[] meshDefinition)
        {
            vertices = meshDefinition;
        }

    }

}