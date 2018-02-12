using System;
using Assimp;
using Henzai.Geometry;

namespace Henzai
{   
    //TODO investigate non-static for multithreading
    public static class AssimpLoader
    {
        private const PostProcessSteps DefaultPostProcessSteps 
            = PostProcessSteps.FlipWindingOrder | PostProcessSteps.Triangulate | PostProcessSteps.PreTransformVertices
            | PostProcessSteps.CalculateTangentSpace | PostProcessSteps.GenerateSmoothNormals;


         public static void LoadFromFile(string filename,PostProcessSteps flags = AssimpLoader.DefaultPostProcessSteps)
        {
            AssimpContext assimpContext = new AssimpContext();
            Scene pScene = assimpContext.ImportFile(filename, flags);

        }
        
    }
}