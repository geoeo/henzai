using System;
using System.Numerics;
using Henzai.Core.VertexGeometry;
using Henzai.Core.Materials;
using Henzai.Core.Acceleration;

namespace Henzai.Core
{
    /// <summary>
    /// Usually used with loaded Meshes/Models
    /// Such as with Assimp
    /// </summary>
    public sealed class Model<T, U> where T : struct, VertexLocateable
    {
        private readonly MeshBVH<T>[] _meshes;
        private readonly U[] _materials;
        private Matrix4x4 _world = Matrix4x4.Identity;

        public bool IsCulled{get; private set;}

        public int MeshCount => _meshes.Length;
        public int MaterialCount => _materials.Length;
        public int TotalTriangleCount {get; private set;}
        /// <summary>
        /// This returns the material associated with a mesh.
        /// It can be indexed via the mesh index
        /// </summary>
        public U GetMaterial(int index) => _materials[index];

        /// <summary>
        /// Should be get only! But this is not possible to enforce with refs
        /// </summary>
        public ref Matrix4x4 GetWorld_DontMutate => ref _world;
        public string BaseDir { get; private set; }

        public Model(string directory, Mesh<T>[] meshes, U[] materials)
        {
            IsCulled = false;
            BaseDir = directory;
            _meshes = new MeshBVH<T>[meshes.Length];
            for(int i = 0; i < meshes.Length; i++)
                _meshes[i] = new MeshBVH<T>(meshes[i]);
            _materials = materials;
        }

        public Model(MeshBVH<T>[] meshes, U[] materials)
        {
            IsCulled = false;
            BaseDir = string.Empty;
            _meshes = new MeshBVH<T>[meshes.Length];
            for(int i = 0; i < meshes.Length; i++)
                _meshes[i] = meshes[i];
            _materials = materials;
        }

        public Model(Mesh<T>[] meshesIn, U[] materials)
        {
            IsCulled = false;
            BaseDir = string.Empty;
            _meshes = new MeshBVH<T>[meshesIn.Length];
            for(int i = 0; i < meshesIn.Length; i++)
                _meshes[i] = new MeshBVH<T>(meshesIn[i]);
            _materials = materials;
        }

        public Model(string directoy, Mesh<T> meshIn, U material)
        {
            IsCulled = false;
            BaseDir = directoy;
            _meshes = new MeshBVH<T>[1];
            _meshes[0] = new MeshBVH<T>(meshIn);

            _materials = new U[1];
            _materials[0] = material;
        }

        public Mesh<T> GetMesh(int index)
        {
            return _meshes[index].mesh;
        }

        public MeshBVH<T> GetMeshBVH(int index)
        {
            return _meshes[index];
        }

        public void SetMeshBVH(int index, MeshBVH<T> meshBVH)
        {
            _meshes[index] = meshBVH;
        }

        public void SetIsCulled(){
            IsCulled = true;
        }

        public void UpdateCulled(bool aabbIsValid){
            if(aabbIsValid && IsCulled)
                IsCulled = !aabbIsValid;
        }

        public bool AreMeshesCulled(){
            bool isCulled = true;
            foreach(var meshBVH in _meshes)
                if(!meshBVH.AABBIsValid){
                    isCulled = false;
                    break;
                }
            return isCulled;
        }

        public void SetNewWorldTransformation(ref Matrix4x4 world, bool applyToAllMeshes)
        {
            _world = world;
            if (applyToAllMeshes)
            { 
                for(int i = 0; i < _meshes.Length; i++){
                    var meshBVH = _meshes[i];
                    _meshes[i] = new MeshBVH<T>(ref meshBVH, ref world);
                }
            }
        }

        public void SetNewWorldTranslation(ref Vector3 translation, bool applyToAllMeshes)
        {
            _world.Translation = translation;
            if (applyToAllMeshes){
                for(int i = 0; i < _meshes.Length; i++){
                    var meshBVH = _meshes[i];
                    _meshes[i] = new MeshBVH<T>(ref meshBVH, ref _world);
                }
            }
        }

        //TODO: Retrieve a subset of the meshes encapsulated by this model class
        public Model<T, U> SplitByString(string id)
        {
            throw new NotImplementedException();
        }

        public static Model<T, RaytraceMaterial> ConvertToRaytracingModel(Model<T, RealtimeMaterial> rtModel)
        {
            var materialCount = rtModel.MaterialCount;
            var raytraceMaterials = new RaytraceMaterial[materialCount];
            for(int i = 0; i < rtModel.MaterialCount; i++){
                var rtMat = rtModel.GetMaterial(i);
                raytraceMaterials[i] = new RaytraceMaterial(rtMat.diffuse, rtMat.emissive);
            }
            return new Model<T, RaytraceMaterial>(rtModel._meshes, raytraceMaterials);
        }

        public static Model<T, RaytraceMaterial> ConvertToRaytracingModel(Model<T, RealtimeMaterial> rtModel, Vector4 diffuse, Vector4 emissive)
        {
            var materialCount = rtModel.MaterialCount;
            var raytraceMaterials = new RaytraceMaterial[materialCount];
            for (int i = 0; i < rtModel.MaterialCount; i++){
                raytraceMaterials[i] = new RaytraceMaterial(diffuse, emissive);
            }
            return new Model<T, RaytraceMaterial>(rtModel._meshes, raytraceMaterials);
        }
    }
}