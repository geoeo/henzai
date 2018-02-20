using System;
using System.Numerics;
using Veldrid;

namespace Henzai.Geometry
{
    public struct TexturedMesh
    {
        public VertexPositionTexture[] vertices;

        public TexturedMesh(VertexPositionTexture[] texturedMeshDefinition)
        {
            vertices = texturedMeshDefinition;
        }

    }

}