module Raytracer.Scene.Builder

open System
open System.Numerics
open Raytracer.Scene.Geometry
open Raytracer.Surface.Surface
open Raytracer.Surface.SurfaceFactory
open Henzai.Core
open Henzai.Core.VertexGeometry
open Henzai.Core.Materials
open Henzai.Core.Numerics
open HenzaiFunc.Core.VertexGeometry
open HenzaiFunc.Core.Acceleration
open SixLabors.ImageSharp.PixelFormats
open Raytracer.Surface.SurfaceTypes


//let scene = List.concat [plane_floor;light_sphere;spheres_glass]
// let scene = List.concat [spheres_scene_2;planes_scene_2;lights]
//let sceneList = List.concat [lights_simple]

let emptySurfaceArray :Surface[] = [||]

//TODO: profile larger models
let loadAssets = 
    //let sceneArray = List.concat [lightsAA;triangle_scene;spheres_scene_2;light_sphere;planes_scene_2_AA;plane_mirror_NonAA;lightsNonAA] |> Array.ofList //# old scene 
    //let sceneNonBoundableArray : Surface[] = List.concat [plane_floor_Unbounded] |> Array.ofList //# old scene

    //let rtModel = AssimpLoader.LoadFromFileWithRealtimeMaterial<VertexPositionNormal>(AppContext.BaseDirectory, "Models/Box.dae", VertexPositionNormal.HenzaiType, AssimpLoader.RaytracePostProcessSteps)
    //let rtModel = AssimpLoader.LoadFromFileWithRealtimeMaterial<VertexPositionNormal>(AppContext.BaseDirectory, "Models/duck.dae", VertexPositionNormal.HenzaiType, AssimpLoader.RaytracePostProcessSteps)
    //let rtModel = AssimpLoader.LoadFromFileWithRealtimeMaterial<VertexPositionNormal>(AppContext.BaseDirectory, "Models/teapot.dae", VertexPositionNormal.HenzaiType, AssimpLoader.RaytracePostProcessSteps)
    let rtModel = AssimpLoader.LoadFromFileWithRealtimeMaterial<VertexPositionNormal>(AppContext.BaseDirectory, "Models/chinesedragon.dae", VertexPositionNormal.HenzaiType, AssimpLoader.RaytracePostProcessSteps)
    //let rtModel = AssimpLoader.LoadFromFileWithRealtimeMaterial<VertexPositionNormal>(AppContext.BaseDirectory, "Models/sphere_centered.obj", VertexPositionNormal.HenzaiType, AssimpLoader.RaytracePostProcessSteps)
    //let rtModel = AssimpLoader.LoadFromFileWithRealtimeMaterial<VertexPositionNormal>(AppContext.BaseDirectory, "Models/sphere.obj", VertexPositionNormal.HenzaiType, AssimpLoader.RaytracePostProcessSteps)

    let raytracingModel = Model<VertexPositionNormal,RealtimeMaterial>.ConvertToRaytracingModel(rtModel, Rgba32.LightSeaGreen.ToVector4(), Vector4.Zero)
    //let transform = Matrix4x4.Identity
    //let transformScale = Matrix4x4.Identity
    //let mutable transformScale = Matrix4x4.CreateScale(0.02f) // duck
    //let mutable transformScale = Matrix4x4.CreateScale(0.1f) // sphere
    //let mutable transformScale = Matrix4x4.CreateScale(0.9f)
    let mutable transformScale = Matrix4x4.CreateScale(0.4f) // dragon
    //let mutable transformScale = Matrix4x4.CreateScale(2.0f)
    //let mutable transformRot = Matrix4x4.CreateFromYawPitchRoll(0.0f, -MathF.PI/2.0f, 0.0f) // pot
    let mutable transformRot = Matrix4x4.Identity
    let mutable transform = Matrix4x4.Multiply(transformRot, transformScale)
    transform.Translation <- Vector3(0.0f, -1.5f, -3.5f)
    let modelSurfaceList = convertModelToSurfaceList(raytracingModel, (vertexPositionNormalTransform transform), VertexRuntimeTypes.VertexPositionNormal, SurfaceTypes.Lambertian)

    //let sceneList  = List.concat[triangle_scene;spheres_scene_4;light_sphere;lightsAA; modelSurfaceList ]
    let sceneList  = List.concat[lightsAA;planes_scene_2_AA;spheres_scene_3;modelSurfaceList]
    //let sceneList  = List.concat[lightsAA;planes_scene_2_AA;spheres_scene_3]

    //let sceneNonBoundableArray : Surface[] = List.concat [modelSurfaceList] |> Array.ofList
    let sceneNonBoundableArray : Surface[] = List.concat [plane_floor_Unbounded;lightsNonAA] |> Array.ofList
    //let sceneNonBoundableArray : Surface[] = List.concat [light_box] |> Array.ofList
    let sceneArray : Surface[] = sceneList|> Array.ofList
    (sceneArray, sceneNonBoundableArray)

let constructBVHTree surfaceArray splitMethod = 
    BVHTreeBuilder<Surface>.Build surfaceArray splitMethod

  
    