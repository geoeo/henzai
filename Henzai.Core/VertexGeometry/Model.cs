using System;
using System.Numerics;
using Henzai.Core.VertexGeometry;
using Henzai.Core.Materials;

namespace Henzai.Core
{
    /// <summary>
    /// Usually used with loaded Meshes/Models
    /// Such as with Assimp
    /// </summary>
    public sealed class Model<T, U> where T : struct, VertexLocateable
    {
        private readonly Mesh<T>[] _meshes;
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
            _meshes = meshes;
            _materials = materials;

            TotalTriangleCount = CalculateTotalValidTriangles(meshes);
            //SetCullingUpdateEvent(_meshes);
        }

        public Model(Mesh<T>[] meshesIn, U[] materials)
        {
            IsCulled = false;
            BaseDir = string.Empty;
            _meshes = meshesIn;
            _materials = materials;

            TotalTriangleCount = CalculateTotalValidTriangles(meshesIn);
            //SetCullingUpdateEvent(_meshes);
        }

        public Model(string directoy, Mesh<T> meshIn, U material)
        {
            IsCulled = false;
            BaseDir = directoy;
            _meshes = new Mesh<T>[1];
            _meshes[0] = meshIn;

            _materials = new U[1];
            _materials[0] = material;

            TotalTriangleCount = meshIn.TriangleCount;
            //SetCullingUpdateEvent(_meshes);
        }

        public Mesh<T> GetMesh(int index)
        {
            return _meshes[index];
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
            foreach(var mesh in _meshes)
                if(!mesh.IsCulled){
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
                foreach (var mesh in _meshes)
                    mesh.SetNewWorldTransformation(ref world);
            }
        }

        public void SetNewWorldTranslation(ref Vector3 translation, bool applyToAllMeshes)
        {
            _world.Translation = translation;
            if (applyToAllMeshes){
                foreach (var mesh in _meshes)
                    mesh.SetNewWorldTranslation(ref translation);
            }
        }

        //TODO: Retrieve a subset of the meshes encapsulated by this model class
        public Model<T, U> SplitByString(string id)
        {
            throw new NotImplementedException();
        }

        // Performance for this seems to be worse than simple for loop through all meshes
        private void SetCullingUpdateEvent(Mesh<T>[] meshes){
            foreach(var mesh in meshes)
                mesh.CulledStateSubscruber += UpdateCulled;
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

        private static int CalculateTotalValidTriangles(Mesh<T>[] meshes){
            var total = 0;
            foreach(var mesh in meshes)
                total += mesh.TriangleCount;
            return total;
        }
    }
}