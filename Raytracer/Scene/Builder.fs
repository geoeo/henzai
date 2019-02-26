module Raytracer.Scene.Builder

open Raytracer.Scene.Geometry
open Raytracer.Surface
open HenzaiFunc.Core.Types
open HenzaiFunc.Core.RaytraceGeometry
open HenzaiFunc.Core.Acceleration

//let scene = List.concat [plane_floor;light_sphere;spheres_glass]
// let scene = List.concat [spheres_scene_2;planes_scene_2;lights]
//let sceneList = List.concat [triangle_scene;spheres_scene_2;light_sphere;lightsAA;lightsNonAA;planes_scene_2]
//let sceneList = List.concat [triangle_scene;light_sphere]
let sceneList = List.concat [lightsAA;triangle_scene;spheres_scene_2;light_sphere;planes_scene_2_AA]


let sceneArray : (Surface[]) = sceneList |> Array.ofList
let sceneNonBoundableArray : Surface [] = List.concat [plane_floor_Unbounded;lightsNonAA;planes_scene_2_NonAA] |> Array.ofList
//let sceneNonBoundableArray : Surface [] = List.concat [plane_floor] |> Array.ofList

let constructBVHTree surfaceArray = 
    let bvhTreeBuilder = BVHTreeBuilder<Surface>()
    bvhTreeBuilder.build surfaceArray SplitMethods.Middle
    