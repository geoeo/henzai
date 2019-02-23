﻿open Raytracer.Scene.Builder
open Raytracer.Scene.Runtime
open BenchmarkDotNet.Running
open HenzaiFunc.Core.RaytraceGeometry
open HenzaiFunc.Core.Acceleration


let benchmarkScene = lazy BenchmarkRunner.Run<RuntimeScene>()

[<EntryPoint>]
let main argv =
    // Console.Write("Press Enter")
    //let input = Console.ReadLine()
    printfn "Starting.." 
    printfn "Constructing BVH Tree.."
    let (bvhTree, orderedGeometryList, totalNodeCount) = constructBVHTree sceneArray
    let bvhRuntime = BVHRuntime<RaytracingGeometry>()
    let bvhRuntimeArray = bvhRuntime.constructBVHRuntime bvhTree totalNodeCount
    printfn "Constructed BVH Tree.."
    let mainScene = RuntimeScene (sceneArray)
    mainScene.RenderScene()
    printfn "Finished Rendering"
    mainScene.SaveFrameBuffer()
    mainScene.SaveDepthBuffer()
    //let summary = benchmarkScene.Force()
    0 // return an integer exit code