open Raytracer.Scene.Builder
open Raytracer.Scene.Runtime
open BenchmarkDotNet.Running
open Raytracer.Surface
open HenzaiFunc.Core.Acceleration
open HenzaiFunc.Core.Types


let benchmarkScene = lazy BenchmarkRunner.Run<RuntimeScene>()

[<EntryPoint>]
let main argv =
    // Console.Write("Press Enter")
    //let input = Console.ReadLine()
    let stopWatch = System.Diagnostics.Stopwatch.StartNew()
    printfn "Starting.." 
    printfn "Loading Assets.."
    let (sceneArray, sceneNonBoundableArray) = loadAssets
    stopWatch.Stop()
    printfn "Loaded Assets in %f ms" stopWatch.Elapsed.TotalMilliseconds

    printfn "Constructing BVH Tree.."
    stopWatch.Restart()
    let (bvhTree, orderedSurfaceArray, totalNodeCount) = constructBVHTree sceneArray SplitMethods.SAH
    let bvhRuntime = BVHRuntime<Surface>()
    let bvhRuntimeArray = bvhRuntime.constructBVHRuntime bvhTree totalNodeCount
    stopWatch.Stop()
    printfn "Constructed BVH Tree in %f ms" stopWatch.Elapsed.TotalMilliseconds

    printfn "Rendering.."
    stopWatch.Restart()
    let mainScene = RuntimeScene (orderedSurfaceArray, sceneNonBoundableArray, bvhRuntime, bvhRuntimeArray)
    //mainScene.RenderScene()
    mainScene.RenderSceneParallel()
    //mainScene.RenderSceneSync()
    printfn "Finished Rendering. Time taken: %f ms" stopWatch.Elapsed.TotalMilliseconds
    mainScene.SaveFrameBuffer()
    mainScene.SaveDepthBuffer()
    //let summary = benchmarkScene.Force()
    0 // return an integer exit code