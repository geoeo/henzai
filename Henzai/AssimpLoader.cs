using System;
using System.Runtime.InteropServices;
using System.Numerics;
using Assimp;
using Henzai.Geometry;
using Henzai.Extensions;
using Henzai.Runtime;

namespace Henzai
{   

    public static class AssimpExtensions
    {
        public static Vector3 ToVector3(this Vector3D vector){
            return new Vector3(vector.X,vector.Y,vector.Z);
        }

        public static Vector2 ToVector2(this Vector3D vector){
            return new Vector2(vector.X,vector.Y);
        }
    }


    //TODO investigate non-static for multithreading
    public static class AssimpLoader
    {
        private static Vector3D Zero3D = new Vector3D(0.0f, 0.0f, 0.0f);
        private static Color4D NoColour = new Color4D(0.0f, 0.0f, 0.0f,0.0f);

        // http://assimp.sourceforge.net/lib_html/postprocess_8h.html#a64795260b95f5a4b3f3dc1be4f52e410a9c3de834f0307f31fa2b1b6d05dd592b
        private const PostProcessSteps DefaultPostProcessSteps 
            = PostProcessSteps.FlipWindingOrder | PostProcessSteps.Triangulate | PostProcessSteps.PreTransformVertices
            | PostProcessSteps.CalculateTangentSpace | PostProcessSteps.GenerateSmoothNormals | PostProcessSteps.JoinIdenticalVertices;


         public static Model<T> LoadFromFile<T>(string filename, HenzaiTypes vertexType, PostProcessSteps flags = DefaultPostProcessSteps) where T : struct
        {
            AssimpContext assimpContext = new AssimpContext();
            Scene pScene = assimpContext.ImportFile(filename, flags);

            int meshCount = pScene.MeshCount;
            Geometry.Mesh<T>[] meshes = new Geometry.Mesh<T>[meshCount];
            uint[][] meshIndicies = new uint[meshCount][];

            for(int i = 0; i < meshCount; i++){

                var aiMesh = pScene.Meshes[i];
                Material aiMaterial = null;
                
                int materialIndex = aiMesh.MaterialIndex;
                if(materialIndex < pScene.MaterialCount)
                    aiMaterial = pScene.Materials[i];
                var vertexCount = aiMesh.VertexCount;
                if(vertexCount == 0)
                    continue;

                T[] meshDefinition = new T[vertexCount];

                for(int j = 0; j < vertexCount; j++){

                    Vector3D pPos = aiMesh.Vertices[j];
                    Vector3D pNormal = aiMesh.Normals[j];
                    Vector3D pTexCoord = aiMesh.HasTextureCoords(0) ? aiMesh.TextureCoordinateChannels[0][j] : Zero3D;
                    Color4D pColour = aiMesh.HasVertexColors(0) ? aiMesh.VertexColorChannels[0][j] : NoColour;
                    Vector3D pTangent = aiMesh.HasTangentBasis ? aiMesh.Tangents[j] : Zero3D;
                    Vector3D pBiTangent = aiMesh.HasTangentBasis ? aiMesh.BiTangents[j] : Zero3D;
                    
                    byte[] bytes;

                    switch(vertexType){
                        case HenzaiTypes.VertexPositionNormalTexture:
                            bytes = new byte[VertexPositionNormalTexture.SizeInBytes];
                            byte[] posAsBytes = ByteMarshal.ToBytes(pPos);
                            byte[] normalAsBytes = ByteMarshal.ToBytes(pNormal);
                            byte[] texCoordAsBytes = ByteMarshal.ToBytes(pTexCoord.ToVector2());

                            Array.Copy(posAsBytes,0,bytes,VertexPositionNormalTexture.PositionOffset,posAsBytes.Length);
                            Array.Copy(normalAsBytes,0,bytes,VertexPositionNormalTexture.NormalOffset,normalAsBytes.Length);
                            Array.Copy(texCoordAsBytes,0,bytes,VertexPositionNormalTexture.TextureCoordinatesOffset,texCoordAsBytes.Length);

                            meshDefinition[j] = ByteMarshal.ByteArrayToStructure<T>(bytes);
                            break;
                        default:
                            throw new NotImplementedException($"{vertexType.ToString("g")} not implemented");

                    }

                }

                meshes[i] = new Geometry.Mesh<T>(meshDefinition);

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

            return new Model<T>(meshes, meshIndicies);

        }
        
    }
}