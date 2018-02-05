using System;
using System.Numerics;
using Veldrid;

namespace Henzai.Geometry
{
    public struct TexturedCube
    {
        public VertexPositionTexture[] vertices;

        public TexturedCube(VertexPositionTexture[] texturedCubeDefinition)
        {
            vertices = texturedCubeDefinition;
        }

    }

}