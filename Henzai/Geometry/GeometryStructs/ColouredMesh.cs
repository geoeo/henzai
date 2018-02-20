using System;
using System.Numerics;
using Veldrid;

namespace Henzai.Geometry
{
    public struct ColouredMesh
    {
        public VertexPositionColour[] vertecies;

        public ColouredMesh(VertexPositionColour[] colouredQuadDefinition){
            vertecies = colouredQuadDefinition;
        }
    }
}