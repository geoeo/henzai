using System;
using System.Numerics;
using Veldrid;

namespace Henzai.Geometry
{
    public struct ColouredQuad
    {
        public VertexPositionColour[] vertecies;

        public ColouredQuad(VertexPositionColour[] colouredQuadDefinition){
            vertecies = colouredQuadDefinition;
        }
    }
}