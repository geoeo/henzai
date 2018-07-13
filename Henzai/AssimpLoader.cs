using System;
using System.IO;
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

        public static Vector4 ToVector4(this Color4D color){
            return new Vector4(color.R,color.G,color.B,color.A);
        }

        public static Geometry.Material ToHenzaiMaterial(this Assimp.Material material){
            return new Geometry.Material(
                material.ColorAmbient.ToVector4(),
                material.ColorDiffuse.ToVector4(),
                material.ColorSpecular.ToVector4(),
                material.ColorEmissive.ToVector4(),
                material.ColorTransparent.ToVector4(),
                new Vector4(material.Shininess,material.ShininessStrength,material.Opacity,material.Reflectivity),
                material.TextureDiffuse.FilePath,
                material.TextureNormal.FilePath,
                material.TextureHeight.FilePath,
                material.TextureSpecular.FilePath);

        }

        //TODO: Refine this
        public static VertexTypes ToHenzaiVertexType(this Assimp.Mesh  aiMesh){
            var hasColor = aiMesh.HasVertexColors(0);
            var hasUVs = aiMesh.HasTextureCoords(0);
            var hasNormals = aiMesh.HasNormals;
            var hasTangent = aiMesh.HasTangentBasis;

            if(hasTangent && hasNormals && hasUVs)
                return VertexTypes.VertexPositionNormalTextureTangentBitangent;
            else if(hasNormals && hasUVs)
                return VertexTypes.VertexPositionNormalTexture;
            else if(hasNormals && !hasColor)
                return VertexTypes.VertexPositionNormal;
            else if(hasColor)
                return VertexTypes.VertexPositionColor;
            else 
                return VertexTypes.VertexPosition;
        }
    }

    //TODO: investigate non-static for multithreading, add option to split model on child model name
    public static class AssimpLoader
    {
        private static Vector3D Zero3D = new Vector3D(0.0f, 0.0f, 0.0f);
        private static Color4D Nocolor = new Color4D(0.0f, 0.0f, 0.0f,0.0f);

        // http://assimp.sourceforge.net/lib_html/postprocess_8h.html#a64795260b95f5a4b3f3dc1be4f52e410a9c3de834f0307f31fa2b1b6d05dd592b
        private const PostProcessSteps DefaultPostProcessSteps 
            = PostProcessSteps.FlipWindingOrder | PostProcessSteps.Triangulate | PostProcessSteps.PreTransformVertices | PostProcessSteps.FlipUVs
            | PostProcessSteps.CalculateTangentSpace | PostProcessSteps.GenerateSmoothNormals | PostProcessSteps.JoinIdenticalVertices ;


        public static Model<T> LoadFromFile<T>(string baseDirectory,string localPath, VertexTypes vertexType, PostProcessSteps flags = DefaultPostProcessSteps) where T : struct
        {

            if(!Verifier.verifyVertexStruct<T>(vertexType))
                throw new ArgumentException($"Type Mismatch AssimpLoader");

            string filePath = Path.Combine(AppContext.BaseDirectory, localPath); 
            
            string[] directoryStructure = localPath.Split('/');
            string modelDir = directoryStructure[0];

            AssimpContext assimpContext = new AssimpContext();
            Scene pScene = assimpContext.ImportFile(filePath, flags);

            int meshCount = pScene.MeshCount;

            Geometry.Mesh<T>[] meshes = new Geometry.Mesh<T>[meshCount];
            ushort[][] meshIndicies = new ushort[meshCount][];

            for(int i = 0; i < meshCount; i++){

                var aiMesh = pScene.Meshes[i];   
                var vertexCount = aiMesh.VertexCount;
                if(vertexCount == 0)
                    continue;

                Assimp.Material aiMaterial = pScene.Materials[aiMesh.MaterialIndex];
                Geometry.Material material = aiMaterial.ToHenzaiMaterial();

                T[] meshDefinition = new T[vertexCount];

                for(int j = 0; j < vertexCount; j++){

                    Vector3D pPos = aiMesh.Vertices[j];
                    Vector3D pNormal = aiMesh.Normals[j];
                    Vector3D pTexCoord = aiMesh.HasTextureCoords(0) ? aiMesh.TextureCoordinateChannels[0][j] : Zero3D;
                    Color4D pcolor = aiMesh.HasVertexColors(0) ? aiMesh.VertexColorChannels[0][j] : Nocolor;
                    Vector3D pTangent = Zero3D;
                    Vector3D pBiTangent = Zero3D;
                    
                    byte[] bytes;
                    byte[] posAsBytes = ByteMarshal.ToBytes(pPos);
                    byte[] normalAsBytes = ByteMarshal.ToBytes(pNormal);
                    byte[] texCoordAsBytes = ByteMarshal.ToBytes(pTexCoord.ToVector2());
                    byte[] tangentAsBytes = ByteMarshal.ToBytes(pTangent);
                    byte[] bitangentAsBytes = ByteMarshal.ToBytes(pBiTangent);

                    switch(vertexType){
                        case VertexTypes.VertexPositionTexture:
                            bytes = new byte[VertexPositionTexture.SizeInBytes];
                            Array.Copy(posAsBytes,0,bytes,VertexPositionTexture.PositionOffset,posAsBytes.Length);
                            Array.Copy(texCoordAsBytes,0,bytes,VertexPositionTexture.TextureCoordinatesOffset,texCoordAsBytes.Length);
                            break;
                        case VertexTypes.VertexPositionNormalTexture:
                            bytes = new byte[VertexPositionNormalTexture.SizeInBytes];
                            Array.Copy(posAsBytes,0,bytes,VertexPositionNormalTexture.PositionOffset,posAsBytes.Length);
                            Array.Copy(normalAsBytes,0,bytes,VertexPositionNormalTexture.NormalOffset,normalAsBytes.Length);
                            Array.Copy(texCoordAsBytes,0,bytes,VertexPositionNormalTexture.TextureCoordinatesOffset,texCoordAsBytes.Length);
                            break;
                        case VertexTypes.VertexPositionNormal:
                            bytes = new byte[VertexPositionNormal.SizeInBytes];
                            Array.Copy(posAsBytes,0,bytes,VertexPositionNormal.PositionOffset,posAsBytes.Length);
                            Array.Copy(normalAsBytes,0,bytes,VertexPositionNormal.NormalOffset,normalAsBytes.Length);
                            break;
                        case VertexTypes.VertexPositionNormalTextureTangent:
                            bytes = new byte[VertexPositionNormalTextureTangent.SizeInBytes];
                            Array.Copy(posAsBytes,0,bytes,VertexPositionNormalTextureTangent.PositionOffset,posAsBytes.Length);
                            Array.Copy(normalAsBytes,0,bytes,VertexPositionNormalTextureTangent.NormalOffset,normalAsBytes.Length);
                            Array.Copy(texCoordAsBytes,0,bytes,VertexPositionNormalTextureTangent.TextureCoordinatesOffset,texCoordAsBytes.Length);
                            Array.Copy(tangentAsBytes,0,bytes,VertexPositionNormalTextureTangent.TangentOffset,tangentAsBytes.Length);
                            break;
                        case VertexTypes.VertexPositionNormalTextureTangentBitangent:
                            bytes = new byte[VertexPositionNormalTextureTangentBitangent.SizeInBytes];
                            Array.Copy(posAsBytes,0,bytes,VertexPositionNormalTextureTangentBitangent.PositionOffset,posAsBytes.Length);
                            Array.Copy(normalAsBytes,0,bytes,VertexPositionNormalTextureTangentBitangent.NormalOffset,normalAsBytes.Length);
                            Array.Copy(texCoordAsBytes,0,bytes,VertexPositionNormalTextureTangentBitangent.TextureCoordinatesOffset,texCoordAsBytes.Length);
                            Array.Copy(tangentAsBytes,0,bytes,VertexPositionNormalTextureTangentBitangent.TangentOffset,tangentAsBytes.Length);
                            Array.Copy(bitangentAsBytes,0,bytes,VertexPositionNormalTextureTangentBitangent.BitangentOffset,bitangentAsBytes.Length);
                            break;
                        default:
                            throw new NotImplementedException($"{vertexType.ToString("g")} not implemented");
                    }

                    meshDefinition[j] = ByteMarshal.ByteArrayToStructure<T>(bytes);

                }

                meshes[i] = new Geometry.Mesh<T>(meshDefinition,material);

                var faceCount = aiMesh.FaceCount;
                meshIndicies[i] = new ushort[3*faceCount];

                for(int j = 0; j < faceCount; j++){
                    var face = aiMesh.Faces[j];

                    if (face.IndexCount != 3){
                        Console.Error.WriteLine("Loading Assimp: Face index count != 3!");
                        continue;

                    }

                    meshIndicies[i][3*j+0] = face.Indices[0].ToUnsignedShort();
                    meshIndicies[i][3*j+1] = face.Indices[1].ToUnsignedShort();
                    meshIndicies[i][3*j+2] = face.Indices[2].ToUnsignedShort();

                }
            }

            return new Model<T>(modelDir,meshes,meshIndicies);

        }

        public static LoadedModels LoadModelsFromFile(string baseDirectory,string localPath, PostProcessSteps flags = DefaultPostProcessSteps)
        {

            string filePath = Path.Combine(AppContext.BaseDirectory, localPath); 
            
            string[] directoryStructure = localPath.Split('/');
            string modelDir = directoryStructure[0];

            AssimpContext assimpContext = new AssimpContext();
            Scene pScene = assimpContext.ImportFile(filePath, flags);

            int meshCount = pScene.MeshCount;

            ushort[][] meshIndicies = new ushort[meshCount][];

            for(int i = 0; i < meshCount; i++){

                var aiMesh = pScene.Meshes[i];   
                var henzaiVertexType = aiMesh.ToHenzaiVertexType();
                var vertexCount = aiMesh.VertexCount;
                if(vertexCount == 0)
                    continue;

                Assimp.Material aiMaterial = pScene.Materials[aiMesh.MaterialIndex];
                Geometry.Material material = aiMaterial.ToHenzaiMaterial();

                T[] meshDefinition = new T[vertexCount];

                for(int j = 0; j < vertexCount; j++){

                    Vector3D pPos = aiMesh.Vertices[j];
                    Vector3D pNormal = aiMesh.Normals[j];
                    Vector3D pTexCoord = aiMesh.HasTextureCoords(0) ? aiMesh.TextureCoordinateChannels[0][j] : Zero3D;
                    Color4D pcolor = aiMesh.HasVertexColors(0) ? aiMesh.VertexColorChannels[0][j] : Nocolor;
                    Vector3D pTangent = Zero3D;
                    Vector3D pBiTangent = Zero3D;
                    
                    byte[] bytes;
                    byte[] posAsBytes = ByteMarshal.ToBytes(pPos);
                    byte[] normalAsBytes = ByteMarshal.ToBytes(pNormal);
                    byte[] texCoordAsBytes = ByteMarshal.ToBytes(pTexCoord.ToVector2());
                    byte[] tangentAsBytes = ByteMarshal.ToBytes(pTangent);
                    byte[] bitangentAsBytes = ByteMarshal.ToBytes(pBiTangent);

                    switch(vertexType){
                        case VertexTypes.VertexPositionTexture:
                            bytes = new byte[VertexPositionTexture.SizeInBytes];
                            Array.Copy(posAsBytes,0,bytes,VertexPositionTexture.PositionOffset,posAsBytes.Length);
                            Array.Copy(texCoordAsBytes,0,bytes,VertexPositionTexture.TextureCoordinatesOffset,texCoordAsBytes.Length);
                            break;
                        case VertexTypes.VertexPositionNormalTexture:
                            bytes = new byte[VertexPositionNormalTexture.SizeInBytes];
                            Array.Copy(posAsBytes,0,bytes,VertexPositionNormalTexture.PositionOffset,posAsBytes.Length);
                            Array.Copy(normalAsBytes,0,bytes,VertexPositionNormalTexture.NormalOffset,normalAsBytes.Length);
                            Array.Copy(texCoordAsBytes,0,bytes,VertexPositionNormalTexture.TextureCoordinatesOffset,texCoordAsBytes.Length);
                            break;
                        case VertexTypes.VertexPositionNormal:
                            bytes = new byte[VertexPositionNormal.SizeInBytes];
                            Array.Copy(posAsBytes,0,bytes,VertexPositionNormal.PositionOffset,posAsBytes.Length);
                            Array.Copy(normalAsBytes,0,bytes,VertexPositionNormal.NormalOffset,normalAsBytes.Length);
                            break;
                        case VertexTypes.VertexPositionNormalTextureTangent:
                            bytes = new byte[VertexPositionNormalTextureTangent.SizeInBytes];
                            Array.Copy(posAsBytes,0,bytes,VertexPositionNormalTextureTangent.PositionOffset,posAsBytes.Length);
                            Array.Copy(normalAsBytes,0,bytes,VertexPositionNormalTextureTangent.NormalOffset,normalAsBytes.Length);
                            Array.Copy(texCoordAsBytes,0,bytes,VertexPositionNormalTextureTangent.TextureCoordinatesOffset,texCoordAsBytes.Length);
                            Array.Copy(tangentAsBytes,0,bytes,VertexPositionNormalTextureTangent.TangentOffset,tangentAsBytes.Length);
                            break;
                        case VertexTypes.VertexPositionNormalTextureTangentBitangent:
                            bytes = new byte[VertexPositionNormalTextureTangentBitangent.SizeInBytes];
                            Array.Copy(posAsBytes,0,bytes,VertexPositionNormalTextureTangentBitangent.PositionOffset,posAsBytes.Length);
                            Array.Copy(normalAsBytes,0,bytes,VertexPositionNormalTextureTangentBitangent.NormalOffset,normalAsBytes.Length);
                            Array.Copy(texCoordAsBytes,0,bytes,VertexPositionNormalTextureTangentBitangent.TextureCoordinatesOffset,texCoordAsBytes.Length);
                            Array.Copy(tangentAsBytes,0,bytes,VertexPositionNormalTextureTangentBitangent.TangentOffset,tangentAsBytes.Length);
                            Array.Copy(bitangentAsBytes,0,bytes,VertexPositionNormalTextureTangentBitangent.BitangentOffset,bitangentAsBytes.Length);
                            break;
                        default:
                            throw new NotImplementedException($"{vertexType.ToString("g")} not implemented");
                    }

                    meshDefinition[j] = ByteMarshal.ByteArrayToStructure<T>(bytes);

                }
                
                Geometry.Mesh<T>[] meshes = new Geometry.Mesh<T>[meshCount];
                meshes[i] = new Geometry.Mesh<T>(meshDefinition,material);

                var faceCount = aiMesh.FaceCount;
                meshIndicies[i] = new ushort[3*faceCount];

                for(int j = 0; j < faceCount; j++){
                    var face = aiMesh.Faces[j];

                    if (face.IndexCount != 3){
                        Console.Error.WriteLine("Loading Assimp: Face index count != 3!");
                        continue;

                    }

                    meshIndicies[i][3*j+0] = face.Indices[0].ToUnsignedShort();
                    meshIndicies[i][3*j+1] = face.Indices[1].ToUnsignedShort();
                    meshIndicies[i][3*j+2] = face.Indices[2].ToUnsignedShort();

                }
            }

            return new Model<T>(modelDir,meshes,meshIndicies);

        }

        
    }
}