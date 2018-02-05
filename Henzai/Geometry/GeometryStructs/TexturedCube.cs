using System;
using System.Numerics;
using Veldrid;

namespace Henzai.Geometry
{
    public struct TexturedCube
    {
        VertexPositionTexture[] vertices;

        public TexturedCube(VertexPositionTexture[] texturedCubeDefinition)
        {
            vertices = texturedCubeDefinition;
        }

    }

}