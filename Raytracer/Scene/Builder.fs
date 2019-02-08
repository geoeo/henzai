module Raytracer.Scene.Builder

open Raytracer.Scene.Geometry

// let scene = List.concat [spheres_scene_2;planes_scene_2;lights]
// let scene = List.concat [triangle_scene;planes_scene_2;lights]
let scene = List.concat [spheres_scene_2;triangle_scene;planes_scene_2;lights]
//let scene = List.concat [plane_floor;light_sphere;spheres_glass]