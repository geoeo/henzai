open Raytracer.Scene
open BenchmarkDotNet.Running


let benchmarkScene = lazy BenchmarkRunner.Run<Raytracer.Scene.Scene>()

[<EntryPoint>]
let main argv =
    // Console.Write("Press Enter")
    //let input = Console.ReadLine()
    printfn "Starting.."
    let mainScene = Scene ()
    mainScene.RenderScene()
    printfn "Finished Rendering"
    mainScene.SaveFrameBuffer()
    mainScene.SaveDepthBuffer()
    //let summary = benchmarkScene.Force()
    0 // return an integer exit code