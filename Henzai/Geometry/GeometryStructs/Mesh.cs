using System;
using System.Numerics;
using Veldrid;

namespace Henzai.Geometry
{
    public struct Mesh
    {
        public VertexPositionNormalTexture[] vertices;

        public Mesh(VertexPositionNormalTexture[] meshDefinition)
        {
            vertices = meshDefinition;
        }

    }

}