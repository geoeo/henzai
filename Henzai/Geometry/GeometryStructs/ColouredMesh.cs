using System;
using System.Numerics;
using Veldrid;

namespace Henzai.Geometry
{
    public class ColouredMesh
    {
        /// <summary>
        /// Mesh hold Position, Colour Information. Normal or Texture Information
        /// </summary>
        public readonly VertexPositionColour[] vertecies;

        public ColouredMesh(VertexPositionColour[] colouredQuadDefinition){
            vertecies = colouredQuadDefinition;
        }
    }
}