using System;
using System.Numerics;
using Assimp;
using Henzai.Geometry;
using Henzai.Extensions;

namespace Henzai
{   
    //TODO investigate non-static for multithreading
    public static class AssimpLoader
    {
        private static Vector3D Zero3D = new Vector3D(0.0f, 0.0f, 0.0f);

        // http://assimp.sourceforge.net/lib_html/postprocess_8h.html#a64795260b95f5a4b3f3dc1be4f52e410a9c3de834f0307f31fa2b1b6d05dd592b
        private const PostProcessSteps DefaultPostProcessSteps 
            = PostProcessSteps.FlipWindingOrder | PostProcessSteps.Triangulate | PostProcessSteps.PreTransformVertices
            | PostProcessSteps.CalculateTangentSpace | PostProcessSteps.GenerateSmoothNormals | PostProcessSteps.JoinIdenticalVertices;

        private static Vector3 toVector3(this Vector3D vector){
            return new Vector3(vector.X,vector.Y,vector.Z);
        }

        private static Vector2 toVector2(this Vector3D vector){
            return new Vector2(vector.X,vector.Y);
        }

         public static Model LoadFromFile(string filename,PostProcessSteps flags = AssimpLoader.DefaultPostProcessSteps)
        {
            AssimpContext assimpContext = new AssimpContext();
            Scene pScene = assimpContext.ImportFile(filename, flags);

            int meshCount = pScene.MeshCount;
            Geometry.Mesh[] meshes = new Geometry.Mesh[meshCount];
            uint[][] meshIndicies = new uint[meshCount][];

            for(int i = 0; i < meshCount; i++){

                var aiMesh = pScene.Meshes[i];
                var vertexCount = aiMesh.VertexCount;
                if(vertexCount == 0)
                    continue;

                VertexPositionNormalTexture[] meshDefinition = new VertexPositionNormalTexture[vertexCount];

                for(int j = 0; j < vertexCount; j++){

                    Vector3D pPos = aiMesh.Vertices[j];
                    Vector3D pNormal = aiMesh.Normals[j];
                    Vector3D pTexCoord = aiMesh.HasTextureCoords(0) ? aiMesh.TextureCoordinateChannels[0][j] : Zero3D;
                    Vector3D pTangent = aiMesh.HasTangentBasis ? aiMesh.Tangents[j] : Zero3D;
                    Vector3D pBiTangent = aiMesh.HasTangentBasis ? aiMesh.BiTangents[j] : Zero3D;

                    meshDefinition[j] 
                        = new VertexPositionNormalTexture(pPos.toVector3(),pNormal.toVector3(),pTexCoord.toVector2());

                }

                meshes[i] = new Geometry.Mesh(meshDefinition);

                var faceCount = aiMesh.FaceCount;
                meshIndicies[i] = new uint[3*faceCount];

                for(int j = 0; j < faceCount; j++){
                    var face = aiMesh.Faces[j];

                    if (face.IndexCount != 3){
                        Console.Error.WriteLine("Loading Assimp: Face index count != 3!");
                        continue;

                    }

                    meshIndicies[i][3*j+0] = face.Indices[0].ToUnsigned();
                    meshIndicies[i][3*j+1] = face.Indices[1].ToUnsigned();
                    meshIndicies[i][3*j+2] = face.Indices[2].ToUnsigned();

                }
            }

            return new Model(meshes, meshIndicies);

        }
        
    }
}