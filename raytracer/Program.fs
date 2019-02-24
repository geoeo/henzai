open Raytracer.Scene.Builder
open Raytracer.Scene.Runtime
open BenchmarkDotNet.Running
open Raytracer.Surface
open HenzaiFunc.Core.Acceleration


let benchmarkScene = lazy BenchmarkRunner.Run<RuntimeScene>()

[<EntryPoint>]
let main argv =
    // Console.Write("Press Enter")
    //let input = Console.ReadLine()
    printfn "Starting.." 
    printfn "Constructing BVH Tree.."
    let (bvhTree, orderedSurfaceArray, totalNodeCount) = constructBVHTree sceneArray
    let bvhRuntime = BVHRuntime<Surface>()
    let bvhRuntimeArray = bvhRuntime.constructBVHRuntime bvhTree totalNodeCount
    printfn "Constructed BVH Tree.."
    printfn "Rendering.."
    let mainScene = RuntimeScene (orderedSurfaceArray, sceneNonBoundableArray, bvhRuntime, bvhRuntimeArray)
    mainScene.RenderScene()
    printfn "Finished Rendering"
    mainScene.SaveFrameBuffer()
    mainScene.SaveDepthBuffer()
    //let summary = benchmarkScene.Force()
    0 // return an integer exit code