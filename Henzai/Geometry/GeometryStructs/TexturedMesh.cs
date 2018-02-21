using System;
using System.Numerics;
using Veldrid;

namespace Henzai.Geometry
{
    /// <summary>
    /// Mesh hold Position, Texture Information. No Normal Information
    /// </summary>
    public class TexturedMesh
    {
        public readonly VertexPositionTexture[] vertices;

        public TexturedMesh(VertexPositionTexture[] texturedMeshDefinition)
        {
            vertices = texturedMeshDefinition;
        }

    }

}