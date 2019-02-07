module Raytracer.Scene

open SixLabors.ImageSharp.PixelFormats
open SixLabors.ImageSharp
open System.IO
open System
open System.Numerics
open Raytracer.Camera
open Raytracer.Geometry.Core
open Raytracer.Numerics
open Raytracer.Surface
open Raytracer.SceneDefinitions
open BenchmarkDotNet.Attributes

type Scene () =

    let width = 800
    let height = 640
    let samplesPerPixel = 1
    let batchSize = 1
    let batches = samplesPerPixel / batchSize
    let batchIndices = [|1..batchSize|]
    let colorSamples = Array.create samplesPerPixel Vector3.Zero
    let colorSamplesClear = Array.create samplesPerPixel Vector3.Zero

    // let frameBuffer = Array2D.create width height defaultColor
    let frameBuffer = Array2D.create width height Vector4.Zero
    let depthBuffer = Array2D.create width height System.Single.MaxValue
    let mutable maxFrameBufferDepth = 0.0f
    let maxTraceDepth = 5us
    let backgroundColor = Vector3.Zero
    let surfaces : (Surface[]) = scene |> Array.ofList

    //TODO: Refactor this into camera
    let cameraOriginWS = Vector3(-3.0f, 6.0f, 15.0f)
    let lookAt = Vector3(-1.0f, 1.0f, -10.0f)
    // let lookAt = Vector3(0.0f,0.0f,-10.0f)

    let viewMatrix = WorldToCamera cameraOriginWS lookAt Vector3.UnitY

    let cameraWS = CameraToWorld viewMatrix 

    let fov = MathF.PI/4.0f

    let writeToDepthBuffer t px py = 
        depthBuffer.[px,py] <- t
        if t > maxFrameBufferDepth then 
            maxFrameBufferDepth <- t 


    let rec rayTrace previousTraceDepth (ray : Ray) =
        if previousTraceDepth > maxTraceDepth 
        then  
            backgroundColor
        else
            let currentTraceDepth = previousTraceDepth + 1us
            let (realSolution,t,surface) = findClosestIntersection ray surfaces
            let surfaceGeometry : Hitable = surface.Geometry
            if surfaceGeometry.IntersectionAcceptable realSolution t 1.0f (PointForRay ray t)
            then
                let emittedRadiance = surface.Emitted
                let (validSamples,raySamples) = surface.GenerateSamples ray t ((int)currentTraceDepth) surface.SamplesArray
                if validSamples = 0 then
                    emittedRadiance
                else 
                    let mutable totalReflectedLight = Vector3.Zero
                    for i in 0..validSamples-1 do
                        let (ray,shading) = raySamples.[i]
                        totalReflectedLight <- totalReflectedLight + shading*rayTrace currentTraceDepth ray
                    emittedRadiance + totalReflectedLight/(float32)validSamples              
            else 
                backgroundColor


    let rayTraceBase (ray : Ray) px py iteration batchIndex = 
        let dotLookAtAndTracingRay = Vector3.Dot(Vector3.Normalize(lookAt), ray.Direction)
        let (realSolution,t,surface) = findClosestIntersection ray surfaces
        let surfaceGeometry = surface.Geometry
        if surfaceGeometry.IntersectionAcceptable realSolution t dotLookAtAndTracingRay (PointForRay ray t) then
            if iteration = 1 && batchIndex = 0 then writeToDepthBuffer t px py

            let currentTraceDepth = 0us
            let emittedRadiance = surface.Emitted
            let (validSamples,raySamples) = surface.GenerateSamples ray t ((int)currentTraceDepth) surface.SamplesArray
            if validSamples = 0 then
                emittedRadiance
            else 
                let mutable totalReflectedLight = Vector3.Zero
                for i in 0..validSamples-1 do
                    let (ray,shading) = raySamples.[i]
                    totalReflectedLight <- totalReflectedLight + shading*rayTrace currentTraceDepth ray
                emittedRadiance + totalReflectedLight/(float32)validSamples
            
        else
            backgroundColor 

    let renderPass px py = 
        let dirCS = 
            RayDirection (PixelToCamera (float32 px) (float32 py) (float32 width) (float32 height) fov)
        let rot = Rotation cameraWS
        let dirWS = Vector3.Normalize(Vector3.TransformNormal(dirCS, rot))
        let ray = Ray(cameraWS.Translation, dirWS)
        //V2 - Fastest
        for batchIndex in 0..batches-1 do
            //TODO: Remove this map for / Preallocate array
            let colorSamplesBatch = Array.map (fun i -> async {return rayTraceBase ray px py i batchIndex}) batchIndices
            let colorsBatch =  colorSamplesBatch |> Async.Parallel |> Async.RunSynchronously
            Array.blit colorsBatch 0 colorSamples (batchIndex*batchSize) batchSize 
        let avgColor = if Array.isEmpty colorSamples then Vector3.Zero else (Array.reduce (+) colorSamples)/(float32)samplesPerPixel
        Array.blit colorSamplesClear 0 colorSamples 0 samplesPerPixel 
        //printfn "Completed Ray for pixels (%i,%i)" px py
        //async {printfn "Completed Ray for pixels (%i,%i)" px py} |> Async.StartAsTask |> ignore
        //Gamma correct TODO: refactor
        frameBuffer.[px,py] <- Vector4(Vector3.SquareRoot(avgColor), 1.0f)
        // frameBuffer.[px,py] <- Vector4(avgColor,1.0f)

    [<Benchmark>]
    member self.RenderScene() =
        for px in 0..width-1 do
            for py in 0..height-1 do
                renderPass px py

    member self.SaveFrameBuffer() =
        using (File.OpenWrite("scene.jpg")) (fun output ->
            using(new Image<Rgba32>(width, height))(fun image -> 
                for px in 0..width-1 do
                    for py in 0..height-1 do
                        image.Item(px, py) <- Rgba32(frameBuffer.[px,py])
                    
                image.SaveAsJpeg(output)
            )
        )

    member self.SaveDepthBuffer() =
        using (File.OpenWrite("depth.jpg")) (fun output ->
            using(new Image<Rgba32>(width, height))(fun image -> 
                for px in 0..width-1 do
                    for py in 0..height-1 do
                        let color = Vector4(Vector3(depthBuffer.[px,py]/maxFrameBufferDepth), 1.0f)
                        image.Item(px, py) <- Rgba32(color)
                    
                image.SaveAsJpeg(output)
            )
        )