using System;
using System.Numerics;
using Veldrid;

namespace Henzai.Geometry
{
    public struct TexturedQuad
    {
        public VertexPositionTexture[] vertecies;

        public TexturedQuad(VertexPositionTexture[] texturedQuadDefinition){
            vertecies = texturedQuadDefinition;
        }
    }
}