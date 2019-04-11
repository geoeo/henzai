module Raytracer.Scene.Builder

open System
open System.Numerics
open Raytracer.Scene.Geometry
open Raytracer.Surface
open Raytracer.RuntimeParameters
open Henzai.Core;
open Henzai.Core.VertexGeometry;
open Henzai.Core.Materials;
open HenzaiFunc.Core.Acceleration
open HenzaiFunc.Core.RaytraceGeometry

//let scene = List.concat [plane_floor;light_sphere;spheres_glass]
// let scene = List.concat [spheres_scene_2;planes_scene_2;lights]
//let sceneList = List.concat [lights_simple]

let emptySurfaceArray :Surface[] = [||]

//TODO move this somewhere else. Maybe HenzaiFunc.Core
let vertexPositionTransform(transform : Matrix4x4) (v : VertexPosition) = 
    let transformedVector4 = Vector4.Transform(v.GetPosition(), transform)
    VertexPosition(transformedVector4)

let vertexPositionNormalTransform(transform : Matrix4x4) (v : VertexPositionNormal) = 
    let transformedVector4 = Vector4.Transform(v.GetPosition(), transform)
    VertexPositionNormal(transformedVector4, v)

//TODO move this somewhere else. Maybe HenzaiFunc.Core
//TODO add transformation support for scaling,translation rotation
let convertModelToSurfaceList (model: Model<'T, RaytraceMaterial>, vertexTypeTransform : 'T -> 'T) =
    let meshCount = model.MeshCount
    let mutable surfaceList: Surface list = []
    let mutable totalVertexCount = 0
    for i in 0..meshCount-1 do
        let mesh = model.GetMesh(i)
        let material = model.GetMaterial(i)

        let indicesCount = mesh.IndicesCount
        let indices = mesh.MeshIndices

        let vertices : 'T[] = Array.map vertexTypeTransform mesh.Vertices

        totalVertexCount <- totalVertexCount + mesh.VertexCount

        for j in 0..3..indicesCount-1 do
            let i1 = (int)indices.[j]
            let i2 = (int)indices.[j+1]
            let i3 = (int)indices.[j+2]           

            let triangle = new IndexedTriangle<'T>(i1, i2, i3, vertices)
            //let triangle = Triangle(vertices.[i1].GetPosition(), vertices.[i2].GetPosition(), vertices.[i3].GetPosition())
            let surface = Lambertian(assignIDAndIncrement id, triangle, material)
            surfaceList <- surface :> Surface :: surfaceList

    printfn "Total Vertex Count: %i" totalVertexCount
    surfaceList

//TODO: profile larger models
let loadAssets = 
    let sceneList = List.concat [lightsAA;triangle_scene;spheres_scene_2;light_sphere;planes_scene_2_AA]
    //let rtModel = AssimpLoader.LoadFromFileWithRealtimeMaterial<VertexPositionNormal>(AppContext.BaseDirectory, "Models/Box.dae", VertexPositionNormal.HenzaiType)
    //let rtModel = AssimpLoader.LoadFromFileWithRealtimeMaterial<VertexPositionNormal>(AppContext.BaseDirectory, "Models/duck.dae", VertexPositionNormal.HenzaiType)
    let rtModel = AssimpLoader.LoadFromFileWithRealtimeMaterial<VertexPositionNormal>(AppContext.BaseDirectory, "Models/chinesedragon.dae", VertexPositionNormal.HenzaiType)
    //let rtModel = AssimpLoader.LoadFromFileWithRealtimeMaterial<VertexPositionNormal>(AppContext.BaseDirectory, "Models/sphere_centered.obj", VertexPositionNormal.HenzaiType)
    let raytracingModel = Model<VertexPositionNormal,RealtimeMaterial>.ConvertToRaytracingModel(rtModel);
    //let transform = Matrix4x4.Identity
    let transform = Matrix4x4.CreateScale(0.1f)
    let modelSurfaceList = convertModelToSurfaceList(raytracingModel, (vertexPositionNormalTransform transform))
    let sceneNonBoundableArray : Surface[] = List.concat [modelSurfaceList] |> Array.ofList
    //let sceneNonBoundableArray : Surface[] = List.concat [lightsAA;lightsNonAA;triangle_scene_y;plane_floor_Unbounded] |> Array.ofList
    //let sceneNonBoundableArray : Surface[] = List.concat [plane_floor_Unbounded;lightsNonAA;plane_mirror_NonAA] |> Array.ofList
    let sceneArray : Surface[] = modelSurfaceList |> Array.ofList
    //let sceneArray : Surface[] = sceneList |> Array.ofList
    //(sceneArray, sceneNonBoundableArray)
    //(sceneArray, emptySurfaceArray)
    (sceneArray, emptySurfaceArray)

let constructBVHTree surfaceArray splitMethod = 
    let bvhTreeBuilder = BVHTreeBuilder<Surface>()
    bvhTreeBuilder.build surfaceArray splitMethod

  
    