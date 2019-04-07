module Raytracer.Scene.Builder

open System
open Raytracer.Scene.Geometry
open Raytracer.Surface
open Henzai.Core;
open Henzai.Core.VertexGeometry;
open HenzaiFunc.Core.Types
open HenzaiFunc.Core.Acceleration

//let scene = List.concat [plane_floor;light_sphere;spheres_glass]
// let scene = List.concat [spheres_scene_2;planes_scene_2;lights]
//let sceneList = List.concat [lights_simple]

let loadAssets = 
    let sceneList = List.concat [lightsAA;triangle_scene;spheres_scene_2;light_sphere;planes_scene_2_AA]
    let loadedModel = AssimpLoader.LoadFromFile<VertexPositionNormal>(AppContext.BaseDirectory, "Models/chinesedragon.dae", VertexPositionNormal.HenzaiType);
    //let sceneNonBoundableArray : Surface [] = List.concat [plane_floor_Unbounded] |> Array.ofList
    let sceneNonBoundableArray : Surface [] = List.concat [plane_floor_Unbounded;lightsNonAA;planes_scene_2_NonAA] |> Array.ofList
    let sceneArray : (Surface[]) = sceneList |> Array.ofList
    (sceneArray, sceneNonBoundableArray)

let constructBVHTree surfaceArray = 
    let bvhTreeBuilder = BVHTreeBuilder<Surface>()
    bvhTreeBuilder.build surfaceArray SplitMethods.SAH
    