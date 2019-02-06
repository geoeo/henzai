open Raytracer.Scene
open BenchmarkDotNet.Running


let benchmarkScene = lazy BenchmarkRunner.Run<Raytracer.Scene.Scene>()

[<EntryPoint>]
let main argv =
    // Console.Write("Press Enter")
    //let input = Console.ReadLine()
    printfn "Starting.."
    let mainScene = Scene ()
    mainScene.renderScene ()
    printfn "Finished Rendering"
    mainScene.saveFrameBuffer ()
    mainScene.saveDepthBuffer ()
    //let summary = benchmarkScene.Force()
    0 // return an integer exit code