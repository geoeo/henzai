module Raytracer.Scene.Builder

open Raytracer.Scene.Geometry
open Raytracer.Surface
open HenzaiFunc.Core.Types
open HenzaiFunc.Core.Acceleration

// let scene = List.concat [spheres_scene_2;planes_scene_2;lights]
// let scene = List.concat [triangle_scene;planes_scene_2;lights]
let sceneList = List.concat [spheres_scene_2;triangle_scene;planes_scene_2;lights]
let sceneArray : (Surface[]) = sceneList |> Array.ofList
//let scene = List.concat [plane_floor;light_sphere;spheres_glass]

let constructBVHTree surfaceArray = 
    let geometryArray = Array.map (fun (elem : Surface) -> elem.Geometry :> AxisAlignedBoundable) surfaceArray
    BVHTree.build geometryArray SplitMethods.Middle