using System;
using System.Numerics;
using Henzai.Core;
using Henzai.Core.VertexGeometry;

namespace Henzai.Geometry
{
    /// <summary>
    /// Usually used with loaded Meshes/Models
    /// Such as with Assimp
    /// </summary>
    public sealed class Model<T,U> where T : struct, VertexLocateable where U : class, CoreMaterial
    {
        private readonly Mesh<T>[] _meshes;
        private readonly U[] _materials;
        private Matrix4x4 _world = Matrix4x4.Identity;

        public int MeshCount => _meshes.Length;
        public int MaterialCount => _materials.Length;

        /// <summary>
        /// Should be get only! But this is not possible to enforce with refs
        /// </summary>
        public ref Matrix4x4 GetWorld_DontMutate => ref _world;
        public string BaseDir {get;private set;}

        public Model(string directory, Mesh<T>[] meshes, U[] materials){
            BaseDir = directory;
            _meshes = meshes;
            _materials = materials;
        }

        public Model(Mesh<T>[] meshesIn, U[] materials){
            BaseDir = string.Empty;
            _meshes = meshesIn;
            _materials = materials;
        }

        public Model(string directoy, Mesh<T> meshIn, U material){
            BaseDir = directoy;
            _meshes = new Mesh<T>[1];
            _meshes[0] = meshIn;

            _materials = new U[1];
            _materials[0] = material;
        }

        public Mesh<T> GetMesh(int index)
        {
            return _meshes[index];
        }

        public void SetNewWorldTransformation(ref Matrix4x4 world, bool applyToAllMeshes){
            _world = world;
            if(applyToAllMeshes){
                foreach(var mesh in _meshes)
                    mesh.SetNewWorldTransformation(ref world);
            }
        }

        public void SetNewWorldTranslation(ref Vector3 translation, bool applyToAllMeshes){
            _world.Translation = translation;
            if(applyToAllMeshes){
                foreach(var mesh in _meshes)
                    mesh.SetNewWorldTranslation(ref translation);
            }
        }

        /// <summary>
        /// This returns the material associated with a mesh.
        /// It can be indexed via the mesh index
        /// </summary>
        public U TryGetMaterial(int index)
        {
            return _materials[index] ?? throw new NullReferenceException("The material you are trying to access is null"); 
        }

        public U GetMaterialRuntime(int index)
        {
            return _materials[index];
        }

        //TODO: Retrieve a subset of the meshes encapsulated by this model class
        public Model<T,U> SplitByString(string id){
            return null;
        }

        
    }
}