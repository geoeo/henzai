using System;
using System.IO;
using System.Numerics;
using Henzai.Core.VertexGeometry;
using Henzai.Core.Extensions;
using Henzai.Core.Reflection;
using Henzai.Core.Materials;
using Assimp;

namespace Henzai.Core
{
    public static class AssimpExtensions
    {
        public static Vector3 ToVector3(this Vector3D vector)
        {
            return new Vector3(vector.X, vector.Y, vector.Z);
        }

        public static Vector2 ToVector2(this Vector3D vector)
        {
            return new Vector2(vector.X, vector.Y);
        }

        public static Vector4 ToVector4(this Color4D color)
        {
            return new Vector4(color.R, color.G, color.B, color.A);
        }

        public static RealtimeMaterial ToRealtimeMaterial(this Assimp.Material material)
        {
            return new RealtimeMaterial(
                material.ColorAmbient.ToVector4(),
                material.ColorDiffuse.ToVector4(),
                material.ColorSpecular.ToVector4(),
                material.ColorEmissive.ToVector4(),
                material.ColorTransparent.ToVector4(),
                new Vector4(material.Shininess, material.ShininessStrength, material.Opacity, material.Reflectivity),
                material.TextureDiffuse.FilePath,
                material.TextureNormal.FilePath,
                material.TextureHeight.FilePath,
                material.TextureSpecular.FilePath);

        }


        //TODO: Refine this
        public static VertexRuntimeTypes ToHenzaiVertexType(this Assimp.Material aiMaterial)
        {
            // var hasColor = aiMesh.HasVertexColors(0);
            // var hasUVs = aiMesh.HasTextureCoords(0);
            // var hasNormals = aiMesh.HasNormals;
            // var hasTangent = aiMesh.HasTangentBasis;

            var hasColor = aiMaterial.HasColorDiffuse;
            var hasUVs = aiMaterial.HasTextureDiffuse;
            var hasNormals = aiMaterial.HasTextureHeight || aiMaterial.HasTextureNormal;

            if (hasNormals && hasUVs)
                return VertexRuntimeTypes.VertexPositionNormalTextureTangentBitangent;
            // else if(hasNormals && hasUVs)
            //     return VertexRuntimeTypes.VertexPositionNormalTexture;
            else if (hasNormals && !hasColor)
                return VertexRuntimeTypes.VertexPositionNormal;
            else if (hasUVs)
                return VertexRuntimeTypes.VertexPositionTexture;
            else if (hasColor)
                return VertexRuntimeTypes.VertexPositionColor;
            else
                return VertexRuntimeTypes.VertexPosition;
        }

        public static LoadedMeshCounts GetHenzaiMeshCounts(this Assimp.Scene aiScene)
        {
            LoadedMeshCounts loadedMeshCounts = new LoadedMeshCounts();

            foreach (var mesh in aiScene.Meshes)
            {
                var material = aiScene.Materials[mesh.MaterialIndex];
                var henzaiVertexType = material.ToHenzaiVertexType();

                switch (henzaiVertexType)
                {
                    case VertexRuntimeTypes.VertexPositionColor:
                        loadedMeshCounts.meshCountPC++;
                        break;
                    case VertexRuntimeTypes.VertexPositionTexture:
                        loadedMeshCounts.meshCountPT++;
                        break;
                    case VertexRuntimeTypes.VertexPositionNormalTexture:
                        loadedMeshCounts.meshCountPNT++;
                        break;
                    case VertexRuntimeTypes.VertexPositionNormal:
                        loadedMeshCounts.meshCountPN++;
                        break;
                    case VertexRuntimeTypes.VertexPositionNormalTextureTangentBitangent:
                        loadedMeshCounts.meshCountPNTTB++;
                        break;
                    default:
                        throw new NotImplementedException($"{henzaiVertexType.ToString("g")} not implemented");
                }

            }

            return loadedMeshCounts;

        }
    }

    //TODO: investigate non-static for multithreading, add option to split model on child model name
    public static class AssimpLoader
    {
        private static Vector3D Zero3D = new Vector3D(0.0f, 0.0f, 0.0f);
        private static Color4D Nocolor = new Color4D(0.0f, 0.0f, 0.0f, 0.0f);

        // http://assimp.sourceforge.net/lib_html/postprocess_8h.html#a64795260b95f5a4b3f3dc1be4f52e410a9c3de834f0307f31fa2b1b6d05dd592b
        private const PostProcessSteps DefaultPostProcessSteps
            = PostProcessSteps.FlipWindingOrder | PostProcessSteps.Triangulate | PostProcessSteps.PreTransformVertices | PostProcessSteps.FlipUVs
            | PostProcessSteps.CalculateTangentSpace | PostProcessSteps.GenerateSmoothNormals | PostProcessSteps.JoinIdenticalVertices;

        //public static byte[] GenerateMaterialBytesArrayFromAssimp(MaterialTypes henzaiMaterialType, Assimp.Material aiMaterial)
        //{

        //    Vector4 ambient = aiMaterial.ColorAmbient.ToVector4();
        //    Vector4 diffuse = aiMaterial.ColorDiffuse.ToVector4();
        //    Vector4 specular = aiMaterial.ColorSpecular.ToVector4();
        //    Vector4 emissive = aiMaterial.ColorEmissive.ToVector4();
        //    Vector4 transparent = aiMaterial.ColorTransparent.ToVector4();
        //    Vector4 coefficients = new Vector4(aiMaterial.Shininess, aiMaterial.ShininessStrength, aiMaterial.Opacity, aiMaterial.Reflectivity);
        //    string diffuseTexturePath = aiMaterial.TextureDiffuse.FilePath;
        //    string normalTexturePath = aiMaterial.TextureNormal.FilePath;
        //    string heightTexturePath = aiMaterial.TextureHeight.FilePath;
        //    string specularTexturePath = aiMaterial.TextureSpecular.FilePath;

        //    byte[] bytes;
        //    byte[] ambientAsBytes = ByteMarshal.StructToBytes(ambient);
        //    byte[] diffuseAsBytes = ByteMarshal.StructToBytes(diffuse);
        //    byte[] specularAsBytes = ByteMarshal.StructToBytes(specular);
        //    byte[] emissiveAsBytes = ByteMarshal.StructToBytes(emissive);
        //    byte[] transparentAsBytes = ByteMarshal.StructToBytes(transparent);
        //    byte[] coefficientAsBytes = ByteMarshal.StructToBytes(coefficients);

        //    byte[] diffusePathAsBytes = ByteMarshal.UTF8ToBytes(diffuseTexturePath);
        //    byte[] normalPathAsBytes = ByteMarshal.UTF8ToBytes(normalTexturePath);
        //    byte[] bumpPathAsBytes = ByteMarshal.UTF8ToBytes(heightTexturePath);
        //    byte[] specularPathAsBytes = ByteMarshal.UTF8ToBytes(specularTexturePath);

        //    byte[] cubeFrontPathAsBytes = ByteMarshal.UTF8ToBytes(specularTexturePath);
        //    byte[] cubeBackPathAsBytes = ByteMarshal.UTF8ToBytes(specularTexturePath);
        //    byte[] cubeLeftPathAsBytes = ByteMarshal.UTF8ToBytes(specularTexturePath);
        //    byte[] cubeRightPathAsBytes = ByteMarshal.UTF8ToBytes(specularTexturePath);
        //    byte[] cubeTopPathAsBytes = ByteMarshal.UTF8ToBytes(specularTexturePath);
        //    byte[] cubeBottomPathAsBytes = ByteMarshal.UTF8ToBytes(specularTexturePath);


        //    switch (henzaiMaterialType)
        //    {
        //        case MaterialTypes.Realtime:
        //            bytes = new byte[RealtimeMaterial.SizeInBytes];
        //            Array.Copy(ambientAsBytes, 0, bytes, RealtimeMaterial.AmbientOffset, ambientAsBytes.Length);
        //            Array.Copy(diffuseAsBytes, 0, bytes, RealtimeMaterial.DiffuseOffset, ambientAsBytes.Length);
        //            Array.Copy(specularAsBytes, 0, bytes, RealtimeMaterial.SpecularOffset, ambientAsBytes.Length);
        //            Array.Copy(emissiveAsBytes, 0, bytes, RealtimeMaterial.EmissiveOffset, ambientAsBytes.Length);
        //            Array.Copy(transparentAsBytes, 0, bytes, RealtimeMaterial.TransparentOffset, ambientAsBytes.Length);
        //            Array.Copy(coefficientAsBytes, 0, bytes, RealtimeMaterial.CoefficientOffset, ambientAsBytes.Length);

        //            Array.Copy(diffusePathAsBytes, 0, bytes, RealtimeMaterial.DiffuseTexOffset, ambientAsBytes.Length);
        //            Array.Copy(normalPathAsBytes, 0, bytes, RealtimeMaterial.NormalTexOffset, ambientAsBytes.Length);
        //            Array.Copy(bumpPathAsBytes, 0, bytes, RealtimeMaterial.BumpTexOffset, ambientAsBytes.Length);
        //            Array.Copy(specularPathAsBytes, 0, bytes, RealtimeMaterial.SpecularTexOffset, ambientAsBytes.Length);

        //            Array.Copy(cubeFrontPathAsBytes, 0, bytes, RealtimeMaterial.CubeFrontOffset, ambientAsBytes.Length);
        //            Array.Copy(cubeBackPathAsBytes, 0, bytes, RealtimeMaterial.CubeBackOffset, ambientAsBytes.Length);
        //            Array.Copy(cubeLeftPathAsBytes, 0, bytes, RealtimeMaterial.CubeLeftOffset, ambientAsBytes.Length);
        //            Array.Copy(cubeRightPathAsBytes, 0, bytes, RealtimeMaterial.CubeRightOffset, ambientAsBytes.Length);
        //            Array.Copy(cubeTopPathAsBytes, 0, bytes, RealtimeMaterial.CubeTopOffset, ambientAsBytes.Length);
        //            Array.Copy(cubeBottomPathAsBytes, 0, bytes, RealtimeMaterial.CubeBottomOffset, ambientAsBytes.Length);
        //            break;

        //        default:
        //            throw new NotImplementedException($"{henzaiMaterialType.ToString("g")} not implemented");
        //    }

        //    return bytes;

        //}

        public static byte[] GenerateVertexBytesArrayFromAssimp(VertexRuntimeTypes henzaiVertexType, Assimp.Mesh aiMesh, int index)
        {

            Vector3D pPos = aiMesh.Vertices[index];
            Vector3D pNormal = aiMesh.Normals[index];
            Vector3D pTexCoord = aiMesh.HasTextureCoords(0) ? aiMesh.TextureCoordinateChannels[0][index] : Zero3D;
            Color4D pColor = aiMesh.HasVertexColors(0) ? aiMesh.VertexColorChannels[0][index] : Nocolor;
            Vector3D pTangent = aiMesh.HasTangentBasis ? aiMesh.Tangents[index] : Zero3D;
            Vector3D pBiTangent = aiMesh.HasTangentBasis ? aiMesh.BiTangents[index] : Zero3D;

            byte[] bytes;
            byte[] posAsBytes = ByteMarshal.StructToBytes(pPos);
            byte[] colorAsBytes = ByteMarshal.StructToBytes(pColor);
            byte[] normalAsBytes = ByteMarshal.StructToBytes(pNormal);
            byte[] texCoordAsBytes = ByteMarshal.StructToBytes(pTexCoord.ToVector2());
            byte[] tangentAsBytes = ByteMarshal.StructToBytes(pTangent);
            byte[] bitangentAsBytes = ByteMarshal.StructToBytes(pBiTangent);

            switch (henzaiVertexType)
            {
                case VertexRuntimeTypes.VertexPositionColor:
                    bytes = new byte[VertexPositionColor.SizeInBytes];
                    Array.Copy(posAsBytes, 0, bytes, VertexPositionColor.PositionOffset, posAsBytes.Length);
                    Array.Copy(colorAsBytes, 0, bytes, VertexPositionColor.ColorOffset, colorAsBytes.Length);
                    break;
                case VertexRuntimeTypes.VertexPositionTexture:
                    bytes = new byte[VertexPositionTexture.SizeInBytes];
                    Array.Copy(posAsBytes, 0, bytes, VertexPositionTexture.PositionOffset, posAsBytes.Length);
                    Array.Copy(texCoordAsBytes, 0, bytes, VertexPositionTexture.TextureCoordinatesOffset, texCoordAsBytes.Length);
                    break;
                case VertexRuntimeTypes.VertexPositionNormalTexture:
                    bytes = new byte[VertexPositionNormalTexture.SizeInBytes];
                    Array.Copy(posAsBytes, 0, bytes, VertexPositionNormalTexture.PositionOffset, posAsBytes.Length);
                    Array.Copy(normalAsBytes, 0, bytes, VertexPositionNormalTexture.NormalOffset, normalAsBytes.Length);
                    Array.Copy(texCoordAsBytes, 0, bytes, VertexPositionNormalTexture.TextureCoordinatesOffset, texCoordAsBytes.Length);
                    break;
                case VertexRuntimeTypes.VertexPositionNormal:
                    bytes = new byte[VertexPositionNormal.SizeInBytes];
                    Array.Copy(posAsBytes, 0, bytes, VertexPositionNormal.PositionOffset, posAsBytes.Length);
                    Array.Copy(normalAsBytes, 0, bytes, VertexPositionNormal.NormalOffset, normalAsBytes.Length);
                    break;
                case VertexRuntimeTypes.VertexPositionNormalTextureTangent:
                    bytes = new byte[VertexPositionNormalTextureTangent.SizeInBytes];
                    Array.Copy(posAsBytes, 0, bytes, VertexPositionNormalTextureTangent.PositionOffset, posAsBytes.Length);
                    Array.Copy(normalAsBytes, 0, bytes, VertexPositionNormalTextureTangent.NormalOffset, normalAsBytes.Length);
                    Array.Copy(texCoordAsBytes, 0, bytes, VertexPositionNormalTextureTangent.TextureCoordinatesOffset, texCoordAsBytes.Length);
                    Array.Copy(tangentAsBytes, 0, bytes, VertexPositionNormalTextureTangent.TangentOffset, tangentAsBytes.Length);
                    break;
                case VertexRuntimeTypes.VertexPositionNormalTextureTangentBitangent:
                    bytes = new byte[VertexPositionNormalTextureTangentBitangent.SizeInBytes];
                    Array.Copy(posAsBytes, 0, bytes, VertexPositionNormalTextureTangentBitangent.PositionOffset, posAsBytes.Length);
                    Array.Copy(normalAsBytes, 0, bytes, VertexPositionNormalTextureTangentBitangent.NormalOffset, normalAsBytes.Length);
                    Array.Copy(texCoordAsBytes, 0, bytes, VertexPositionNormalTextureTangentBitangent.TextureCoordinatesOffset, texCoordAsBytes.Length);
                    Array.Copy(tangentAsBytes, 0, bytes, VertexPositionNormalTextureTangentBitangent.TangentOffset, tangentAsBytes.Length);
                    Array.Copy(bitangentAsBytes, 0, bytes, VertexPositionNormalTextureTangentBitangent.BitangentOffset, bitangentAsBytes.Length);
                    break;
                default:
                    throw new NotImplementedException($"{henzaiVertexType.ToString("g")} not implemented");
            }

            return bytes;

        }

        //TODO add material enum, remove unused code
        public static Model<T, RealtimeMaterial> LoadFromFileWithRealtimeMaterial<T>(string baseDirectory, string localPath, VertexRuntimeTypes vertexType, PostProcessSteps flags = DefaultPostProcessSteps) where T : struct, VertexLocateable
        {

            if(!Verifier.VerifyVertexStruct<T>(vertexType))
                throw new ArgumentException($"Vertex Type Mismatch AssimpLoader");

            string filePath = Path.Combine(baseDirectory, localPath);

            string[] directoryStructure = localPath.Split('/');
            string modelDir = directoryStructure[0];

            AssimpContext assimpContext = new AssimpContext();
            Assimp.Scene pScene = assimpContext.ImportFile(filePath, flags);

            int meshCount = pScene.MeshCount;

            Mesh<T>[] meshes = new Mesh<T>[meshCount];
            RealtimeMaterial[] materials = new RealtimeMaterial[meshCount];
            ushort[][] meshIndicies = new ushort[meshCount][];

            for (int i = 0; i < meshCount; i++)
            {

                var aiMesh = pScene.Meshes[i];
                var vertexCount = aiMesh.VertexCount;
                if (vertexCount == 0)
                    continue;

                Assimp.Material aiMaterial = pScene.Materials[aiMesh.MaterialIndex];
                var material = aiMaterial.ToRealtimeMaterial();

                T[] meshDefinition = new T[vertexCount];

                for (int j = 0; j < vertexCount; j++)
                {
                    byte[] bytes = GenerateVertexBytesArrayFromAssimp(vertexType, aiMesh, j);
                    meshDefinition[j] = ByteMarshal.ByteArrayToStructure<T>(bytes);
                }

                materials[i] = material;
                var faceCount = aiMesh.FaceCount;
                meshIndicies[i] = new ushort[3 * faceCount];

                for (int j = 0; j < faceCount; j++)
                {
                    var face = aiMesh.Faces[j];

                    if (face.IndexCount != 3)
                    {
                        Console.Error.WriteLine("Loading Assimp: Face index count != 3!");
                        continue;

                    }

                    meshIndicies[i][3 * j + 0] = face.Indices[0].ToUnsignedShort();
                    meshIndicies[i][3 * j + 1] = face.Indices[1].ToUnsignedShort();
                    meshIndicies[i][3 * j + 2] = face.Indices[2].ToUnsignedShort();

                }
                meshes[i] = new Mesh<T>(meshDefinition, meshIndicies[i]);
            }

            return new Model<T, RealtimeMaterial>(modelDir, meshes, materials);
        }

        public static LoadedModels<RealtimeMaterial> LoadRealtimeModelsFromFile(string baseDirectory, string localPath, PostProcessSteps flags = DefaultPostProcessSteps)
        {
            string filePath = Path.Combine(baseDirectory, localPath);

            string[] directoryStructure = localPath.Split('/');
            string modelDir = directoryStructure[0];

            AssimpContext assimpContext = new AssimpContext();
            Assimp.Scene pScene = assimpContext.ImportFile(filePath, flags);

            //TODO: Identify meshcount for each vertex type. Have to preprocess
            int meshCount = pScene.MeshCount;

            var loadedMeshCounts = pScene.GetHenzaiMeshCounts();
            // int meshCountP = loadedMeshCounts.meshCountP;
            int meshCountPC = loadedMeshCounts.meshCountPC;
            int meshCountPN = loadedMeshCounts.meshCountPN;
            int meshCountPT = loadedMeshCounts.meshCountPT;
            int meshCountPNT = loadedMeshCounts.meshCountPNT;
            int meshCountPNTTB = loadedMeshCounts.meshCountPNTTB;

            // Geometry.Mesh<VertexPosition>[] meshesP = new Geometry.Mesh<VertexPosition>[meshCountP];
            Mesh<VertexPositionColor>[] meshesPC = new Mesh<VertexPositionColor>[meshCountPC];
            Mesh<VertexPositionNormal>[] meshesPN = new Mesh<VertexPositionNormal>[meshCountPN];
            Mesh<VertexPositionTexture>[] meshesPT = new Mesh<VertexPositionTexture>[meshCountPT];
            Mesh<VertexPositionNormalTexture>[] meshesPNT = new Mesh<VertexPositionNormalTexture>[meshCountPNT];
            Mesh<VertexPositionNormalTextureTangentBitangent>[] meshesPNTTB = new Mesh<VertexPositionNormalTextureTangentBitangent>[meshCountPNTTB];

            RealtimeMaterial[] materialsPC = new RealtimeMaterial[meshCountPC];
            RealtimeMaterial[] materialsPN = new RealtimeMaterial[meshCountPN];
            RealtimeMaterial[] materialsPT = new RealtimeMaterial[meshCountPT];
            RealtimeMaterial[] materialsPNT = new RealtimeMaterial[meshCountPNT];
            RealtimeMaterial[] materialsPNTTB = new RealtimeMaterial[meshCountPNTTB];

            // ushort[][] meshIndiciesP = new ushort[meshCountP][];
            ushort[][] meshIndiciesPC = new ushort[meshCountPC][];
            ushort[][] meshIndiciesPN = new ushort[meshCountPN][];
            ushort[][] meshIndiciesPT = new ushort[meshCountPT][];
            ushort[][] meshIndiciesPNT = new ushort[meshCountPNT][];
            ushort[][] meshIndiciesPNTTB = new ushort[meshCountPNTTB][];

            // int meshIndiciesP_Counter = 0;
            int meshIndiciesPC_Counter = 0;
            int meshIndiciesPN_Counter = 0;
            int meshIndiciesPT_Counter = 0;
            int meshIndiciesPNT_Counter = 0;
            int meshIndiciesPNTTB_Counter = 0;

            var loadedModels = new LoadedModels<RealtimeMaterial>();

            VertexPositionColor[] meshDefinitionPC = new VertexPositionColor[0];
            VertexPositionNormal[] meshDefinitionPN = new VertexPositionNormal[0];
            VertexPositionTexture[] meshDefinitionPT = new VertexPositionTexture[0];
            VertexPositionNormalTexture[] meshDefinitionPNT = new VertexPositionNormalTexture[0];
            VertexPositionNormalTextureTangentBitangent[] meshDefinitionPNTTB = new VertexPositionNormalTextureTangentBitangent[0];

            for (int i = 0; i < meshCount; i++)
            {

                var aiMesh = pScene.Meshes[i];
                var vertexCount = aiMesh.VertexCount;
                if (vertexCount == 0)
                {
                    Console.Error.WriteLine("Mesh has no verticies");
                    continue;
                }

                Assimp.Material aiMaterial = pScene.Materials[aiMesh.MaterialIndex];
                Core.Materials.RealtimeMaterial material = aiMaterial.ToRealtimeMaterial();
                VertexRuntimeTypes henzaiVertexType = aiMaterial.ToHenzaiVertexType();
                switch (henzaiVertexType)
                {
                    case VertexRuntimeTypes.VertexPositionColor:
                        meshDefinitionPC = new VertexPositionColor[vertexCount];
                        break;
                    case VertexRuntimeTypes.VertexPositionTexture:
                        meshDefinitionPT = new VertexPositionTexture[vertexCount];
                        break;
                    case VertexRuntimeTypes.VertexPositionNormalTexture:
                        meshDefinitionPNT = new VertexPositionNormalTexture[vertexCount];
                        break;
                    case VertexRuntimeTypes.VertexPositionNormal:
                        meshDefinitionPN = new VertexPositionNormal[vertexCount];
                        break;
                    case VertexRuntimeTypes.VertexPositionNormalTextureTangentBitangent:
                        meshDefinitionPNTTB = new VertexPositionNormalTextureTangentBitangent[vertexCount];
                        break;
                    default:
                        throw new NotImplementedException($"{henzaiVertexType.ToString("g")} not implemented");
                }

                for (int j = 0; j < vertexCount; j++)
                {

                    byte[] bytes = GenerateVertexBytesArrayFromAssimp(henzaiVertexType, aiMesh, j);

                    switch (henzaiVertexType)
                    {
                        case VertexRuntimeTypes.VertexPositionColor:
                            meshDefinitionPC[j] = ByteMarshal.ByteArrayToStructure<VertexPositionColor>(bytes);
                            break;
                        case VertexRuntimeTypes.VertexPositionTexture:
                            meshDefinitionPT[j] = ByteMarshal.ByteArrayToStructure<VertexPositionTexture>(bytes);
                            break;
                        case VertexRuntimeTypes.VertexPositionNormalTexture:
                            meshDefinitionPNT[j] = ByteMarshal.ByteArrayToStructure<VertexPositionNormalTexture>(bytes);
                            break;
                        case VertexRuntimeTypes.VertexPositionNormal:
                            meshDefinitionPN[j] = ByteMarshal.ByteArrayToStructure<VertexPositionNormal>(bytes);
                            break;
                        case VertexRuntimeTypes.VertexPositionNormalTextureTangentBitangent:
                            meshDefinitionPNTTB[j] = ByteMarshal.ByteArrayToStructure<VertexPositionNormalTextureTangentBitangent>(bytes);
                            break;
                        default:
                            throw new NotImplementedException($"{henzaiVertexType.ToString("g")} not implemented");
                    }

                }

                var faceCount = aiMesh.FaceCount;
                switch (henzaiVertexType)
                {
                    case VertexRuntimeTypes.VertexPositionColor:
                        materialsPC[meshIndiciesPC_Counter] = material;
                        meshIndiciesPC[meshIndiciesPC_Counter] = new ushort[3 * faceCount];

                        for (int j = 0; j < faceCount; j++)
                        {
                            var face = aiMesh.Faces[j];

                            if (face.IndexCount != 3)
                            {
                                Console.Error.WriteLine("Loading Assimp: Face index count != 3!");
                                continue;
                            }

                            meshIndiciesPC[meshIndiciesPC_Counter][3 * j + 0] = face.Indices[0].ToUnsignedShort();
                            meshIndiciesPC[meshIndiciesPC_Counter][3 * j + 1] = face.Indices[1].ToUnsignedShort();
                            meshIndiciesPC[meshIndiciesPC_Counter][3 * j + 2] = face.Indices[2].ToUnsignedShort();
                        }
                        meshesPC[meshIndiciesPC_Counter] = new Mesh<VertexPositionColor>(meshDefinitionPC, meshIndiciesPC[meshIndiciesPC_Counter]);
                        meshIndiciesPC_Counter++;
                        break;
                    case VertexRuntimeTypes.VertexPositionTexture:
                        materialsPT[meshIndiciesPT_Counter] = material;
                        meshIndiciesPT[meshIndiciesPT_Counter] = new ushort[3 * faceCount];

                        for (int j = 0; j < faceCount; j++)
                        {
                            var face = aiMesh.Faces[j];

                            if (face.IndexCount != 3)
                            {
                                Console.Error.WriteLine("Loading Assimp: Face index count != 3!");
                                continue;
                            }

                            meshIndiciesPT[meshIndiciesPT_Counter][3 * j + 0] = face.Indices[0].ToUnsignedShort();
                            meshIndiciesPT[meshIndiciesPT_Counter][3 * j + 1] = face.Indices[1].ToUnsignedShort();
                            meshIndiciesPT[meshIndiciesPT_Counter][3 * j + 2] = face.Indices[2].ToUnsignedShort();
                        }
                        meshesPT[meshIndiciesPT_Counter] = new Mesh<VertexPositionTexture>(meshDefinitionPT, meshIndiciesPT[meshIndiciesPT_Counter]);
                        meshIndiciesPT_Counter++;
                        break;
                    case VertexRuntimeTypes.VertexPositionNormalTexture:
                        materialsPNT[meshIndiciesPNT_Counter] = material;
                        meshIndiciesPNT[meshIndiciesPNT_Counter] = new ushort[3 * faceCount];

                        for (int j = 0; j < faceCount; j++)
                        {
                            var face = aiMesh.Faces[j];

                            if (face.IndexCount != 3)
                            {
                                Console.Error.WriteLine("Loading Assimp: Face index count != 3!");
                                continue;
                            }

                            meshIndiciesPNT[meshIndiciesPNT_Counter][3 * j + 0] = face.Indices[0].ToUnsignedShort();
                            meshIndiciesPNT[meshIndiciesPNT_Counter][3 * j + 1] = face.Indices[1].ToUnsignedShort();
                            meshIndiciesPNT[meshIndiciesPNT_Counter][3 * j + 2] = face.Indices[2].ToUnsignedShort();
                        }
                        meshesPNT[meshIndiciesPNT_Counter] = new Mesh<VertexPositionNormalTexture>(meshDefinitionPNT, meshIndiciesPNT[meshIndiciesPNT_Counter]);
                        meshIndiciesPNT_Counter++;
                        break;
                    case VertexRuntimeTypes.VertexPositionNormal:
                        materialsPN[meshIndiciesPN_Counter] = material;
                        meshIndiciesPN[meshIndiciesPN_Counter] = new ushort[3 * faceCount];

                        for (int j = 0; j < faceCount; j++)
                        {
                            var face = aiMesh.Faces[j];

                            if (face.IndexCount != 3)
                            {
                                Console.Error.WriteLine("Loading Assimp: Face index count != 3!");
                                continue;
                            }

                            meshIndiciesPN[meshIndiciesPN_Counter][3 * j + 0] = face.Indices[0].ToUnsignedShort();
                            meshIndiciesPN[meshIndiciesPN_Counter][3 * j + 1] = face.Indices[1].ToUnsignedShort();
                            meshIndiciesPN[meshIndiciesPN_Counter][3 * j + 2] = face.Indices[2].ToUnsignedShort();
                        }
                        meshesPN[meshIndiciesPN_Counter] = new Mesh<VertexPositionNormal>(meshDefinitionPN, meshIndiciesPN[meshIndiciesPN_Counter]);
                        meshIndiciesPN_Counter++;
                        break;
                    case VertexRuntimeTypes.VertexPositionNormalTextureTangentBitangent:
                        materialsPNTTB[meshIndiciesPNTTB_Counter] = material;
                        meshIndiciesPNTTB[meshIndiciesPNTTB_Counter] = new ushort[3 * faceCount];

                        for (int j = 0; j < faceCount; j++)
                        {
                            var face = aiMesh.Faces[j];

                            if (face.IndexCount != 3)
                            {
                                Console.Error.WriteLine("Loading Assimp: Face index count != 3!");
                                continue;
                            }

                            meshIndiciesPNTTB[meshIndiciesPNTTB_Counter][3 * j + 0] = face.Indices[0].ToUnsignedShort();
                            meshIndiciesPNTTB[meshIndiciesPNTTB_Counter][3 * j + 1] = face.Indices[1].ToUnsignedShort();
                            meshIndiciesPNTTB[meshIndiciesPNTTB_Counter][3 * j + 2] = face.Indices[2].ToUnsignedShort();
                        }
                        meshesPNTTB[meshIndiciesPNTTB_Counter] = new Mesh<VertexPositionNormalTextureTangentBitangent>(meshDefinitionPNTTB, meshIndiciesPNTTB[meshIndiciesPNTTB_Counter]);
                        meshIndiciesPNTTB_Counter++;
                        break;
                    default:
                        throw new NotImplementedException($"{henzaiVertexType.ToString("g")} not implemented");
                }

            }

            // if(meshCountP > 0)
            // loadedModels.modelP = new Model<VertexPosition>(modelDir,meshesP,meshIndiciesP);
            if (meshCountPC > 0)
                loadedModels.modelPC = new Model<VertexPositionColor, RealtimeMaterial>(modelDir, meshesPC, materialsPC);
            if (meshCountPN > 0)
                loadedModels.modelPN = new Model<VertexPositionNormal, RealtimeMaterial>(modelDir, meshesPN, materialsPN);
            if (meshCountPT > 0)
                loadedModels.modelPT = new Model<VertexPositionTexture, RealtimeMaterial>(modelDir, meshesPT, materialsPT);
            if (meshCountPNT > 0)
                loadedModels.modelPNT = new Model<VertexPositionNormalTexture, RealtimeMaterial>(modelDir, meshesPNT, materialsPNT);
            if (meshCountPNTTB > 0)
                loadedModels.modelPNTTB = new Model<VertexPositionNormalTextureTangentBitangent, RealtimeMaterial>(modelDir, meshesPNTTB, materialsPNTTB);

            return loadedModels;
        }
    }
}