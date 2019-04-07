module Raytracer.Scene.Builder

open System
open Raytracer.Scene.Geometry
open Raytracer.Surface
open Raytracer.RuntimeParameters
open Henzai.Core;
open Henzai.Core.VertexGeometry;
open Henzai.Core.Materials;
open HenzaiFunc.Core.Types
open HenzaiFunc.Core.Acceleration
open HenzaiFunc.Core.RaytraceGeometry

//let scene = List.concat [plane_floor;light_sphere;spheres_glass]
// let scene = List.concat [spheres_scene_2;planes_scene_2;lights]
//let sceneList = List.concat [lights_simple]

let emptySurfaceArray :Surface[] = [||]

//TODO: Not working. Might have to check winding order explicitly
let convertModelToSurfaceList (model: Model<'T, RaytraceMaterial>) =
    let meshCount = model.MeshCount
    let mutable surfaceList: Surface list = [] 
    for i in 0..meshCount-1 do
        let mesh = model.GetMesh(i)
        let material = model.GetMaterial(i)

        let indicesCount = mesh.IndicesCount
        let indices = mesh.MeshIndices
        let vertices = mesh.Vertices

        let indices_2 = mesh.GeometryDefinition.GetIndices;
        let indicesCount_2 = indices_2.Length
        let vertices_2 = mesh.GeometryDefinition.GetVertices
        for j in 0..3..indicesCount_2-1 do
            let i1 = (int)indices_2.[j]
            let i2 = (int)indices_2.[j+1]
            let i3 = (int)indices_2.[j+2]

            let v1 = vertices_2.[i1]
            let v2 = vertices_2.[i2]
            let v3 = vertices_2.[i3]

            //let n1 = (v1 :> VertexTangentspace).GetNormal()
            

            let p1 = v1.GetPosition()
            let p2 = v2.GetPosition()
            let p3 = v3.GetPosition()


            //let triangle = new IndexedTriangle<'T>(i3, i2, i1, vertices_2)
            //let triangle = new TriangleWithNormal(p1, p2, p3, n1)
            let triangle = new Triangle(p1, p2, p3)
            let surface = new Lambertian(assignIDAndIncrement id, triangle, material)
            surfaceList <- surface :> Surface :: surfaceList
    surfaceList

        
let loadAssets = 
    let sceneList = List.concat [lightsAA;triangle_scene;spheres_scene_2;light_sphere;planes_scene_2_AA]
    let rtModel = AssimpLoader.LoadFromFileWithRealtimeMaterial<VertexPositionNormal>(AppContext.BaseDirectory, "Models/Box.dae", VertexPositionNormal.HenzaiType);
    //let rtModel = AssimpLoader.LoadFromFileWithRealtimeMaterial<VertexPositionNormal>(AppContext.BaseDirectory, "Models/sphere_centered.obj", VertexPositionNormal.HenzaiType);
    let raytracingModel = Model<VertexPositionNormal,RealtimeMaterial>.ConvertToRaytracingModel(rtModel);
    let modelSurfaceList = convertModelToSurfaceList raytracingModel
    let modelSurfaceArray : Surface[] = modelSurfaceList |> Array.ofList
    let sceneNonBoundableArray : Surface[] = List.concat [modelSurfaceList] |> Array.ofList
    //let sceneNonBoundableArray : Surface[] = List.concat [plane_floor_Unbounded;lightsNonAA;plane_mirror_NonAA] |> Array.ofList
    let sceneArray : Surface[] = sceneList |> Array.ofList
    //(sceneArray, sceneNonBoundableArray)
    (emptySurfaceArray, sceneNonBoundableArray)

let constructBVHTree surfaceArray = 
    let bvhTreeBuilder = BVHTreeBuilder<Surface>()
    bvhTreeBuilder.build surfaceArray SplitMethods.SAH

  
    