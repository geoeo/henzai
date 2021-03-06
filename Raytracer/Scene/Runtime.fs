module Raytracer.Scene.Runtime

open SixLabors.ImageSharp.PixelFormats
open SixLabors.ImageSharp
open System.IO
open System
open System.Numerics
//open Raytracer.RaytraceCamera
open HenzaiFunc.Core.RaytraceGeometry
open Henzai.Core.Numerics
open Henzai.Core.Raytracing
open HenzaiFunc.Core.RaytraceCamera
open HenzaiFunc.Core.Types
open HenzaiFunc.Core.Acceleration
open Raytracer.Surface.Surface
open Raytracer.RuntimeParameters
open BenchmarkDotNet.Attributes

type RuntimeScene (surfaces : Surface [], nonBoundableSurfaces : Surface [], bvhRuntimeArray : BVHRuntimeNode []) =

    //TODO decide on a async implementation
    // let bvhTraversalStack2D = Array2D.zeroCreate batchSize bvhRuntimeArray.Length
    let bvhTraversalStack2D = Array2D.zeroCreate batchSize bvhRuntimeArray.Length
    let randomStateForThread = Array.zeroCreate<Random> batchSize

    let batches = samplesPerPixel / batchSize
    let batchIndices = [|1..batchSize|] 

    let colorSamples = Array.create samplesPerPixel Vector4.Zero
    let colorSamplesClear = Array.create samplesPerPixel Vector4.Zero

    // let frameBuffer = Array2D.create width height defaultColor
    let frameBuffer = Array2D.create width height Vector4.Zero
    let depthBuffer = Array2D.create width height System.Single.MaxValue
    let mutable maxFrameBufferDepth = 0.0f
    let backgroundColor = Vector4.Zero
    //let backgroundColor = Vector4(1.0f,0.0f,0.0f,0.0f)

    //TODO: Refactor this into camera
    let cameraOriginWS = Vector3(-3.0f, 6.0f, 15.0f)
    //let cameraOriginWS = Vector3(-10.0f, 0.0f, 0.0f)
    let lookAt = Vector3(-1.0f, -1.0f, -10.0f)
    //let lookAt = Vector3(1.0f, 0.0f, 0.0f)
    // let lookAt = Vector3(0.0f,0.0f,-10.0f)

    let viewMatrix = WorldToCamera cameraOriginWS lookAt Vector3.UnitY

    let cameraWS = CameraToWorld viewMatrix 

    let fov = MathF.PI/4.0f

    let writeToDepthBuffer t px py = 
        depthBuffer.[px,py] <- t
        if t > maxFrameBufferDepth then 
            maxFrameBufferDepth <- t 

    let rec rayTrace previousTraceDepth (ray : Ray) batchID =
        let bvhTraversalStack = bvhTraversalStack2D.[batchID, *]
        let random = randomStateForThread.[batchID]

        if previousTraceDepth >= maxTraceDepth 
        then  
            backgroundColor
        else
            let currentTraceDepth = previousTraceDepth + 1us
            let mutable struct(hasIntersection, t, surfaceOption) = struct(false, 0.0f, None)
            if bvhRuntimeArray.Length > 0 then
                let struct(b_bvh, t_bvh, s_bvh) = BVHRuntime.Traverse<Surface> bvhRuntimeArray surfaces bvhTraversalStack ray
                hasIntersection <- b_bvh
                t <- t_bvh
                surfaceOption <- s_bvh
                
            // Some geometry is not present in the BVH i.e. infinite planes
            let (b_linear, t_linear, s_linear) = findClosestIntersection ray nonBoundableSurfaces
            if (not hasIntersection || t_linear < t ) && s_linear.Geometry.AsHitable.IntersectionAcceptable(b_linear, t_linear, 1.0f, (RaytraceGeometryUtils.PointForRay ray t_linear)) then
                hasIntersection <- b_linear
                t <- t_linear
                surfaceOption <- Some s_linear

            if hasIntersection then
                let surface = surfaceOption.Value
                let emittedRadiance = surface.Emitted ray t
                let (validSamples, raySamples) = surface.GenerateSamples ray t surface.SamplesArray random
                if validSamples = 0 then
                    emittedRadiance
                else 
                    let mutable totalReflectedLight = Vector4.Zero
                    for i in 0..validSamples-1 do
                        let struct(ray_new, shading) = raySamples.[i]
                        totalReflectedLight <- totalReflectedLight + shading*rayTrace currentTraceDepth ray_new batchID
                    emittedRadiance + totalReflectedLight/(float32)validSamples              
             else
                 backgroundColor


    let rayTraceBase (ray : Ray) px py batchID iteration = 
        let dotLookAtAndTracingRay = Vector3.Dot(Vector3.Normalize(lookAt), Vector.ToVec3(ray.Direction))
        let bvhTraversalStack = bvhTraversalStack2D.[batchID, *]
        let random = randomStateForThread.[batchID]

        let mutable struct(hasIntersection, t, surfaceOption) = struct(false, 0.0f, None)
        if bvhRuntimeArray.Length > 0 then
            let struct(b_bvh, t_bvh, s_bvh) = BVHRuntime.Traverse<Surface> bvhRuntimeArray surfaces bvhTraversalStack ray
            hasIntersection <- b_bvh
            t <- t_bvh
            surfaceOption <- s_bvh
            
        // Some geometry is not present in the BVH i.e. infinite planes
        let (b_linear, t_linear, s_linear) = findClosestIntersection ray nonBoundableSurfaces
        if (not hasIntersection || t_linear < t ) && s_linear.Geometry.AsHitable.IntersectionAcceptable(b_linear, t_linear, dotLookAtAndTracingRay, (RaytraceGeometryUtils.PointForRay ray t_linear)) then
            hasIntersection <- b_linear
            t <- t_linear
            surfaceOption <- Some s_linear

        if hasIntersection then
            let surface = surfaceOption.Value
            if batchID = 0 && iteration = 0 then writeToDepthBuffer t px py

            let currentTraceDepth = 0us
            let emittedRadiance = surface.Emitted ray t
            let (validSamples,raySamples) = surface.GenerateSamples ray t surface.SamplesArray random
            if validSamples = 0 then
                emittedRadiance
            else 
                let mutable totalReflectedLight = Vector4.Zero
                for i in 0..validSamples-1 do
                    let struct(ray_new, shading) = raySamples.[i]
                    totalReflectedLight <- totalReflectedLight + shading*rayTrace currentTraceDepth ray_new batchID
                emittedRadiance + totalReflectedLight/(float32)validSamples   
         else
             backgroundColor

    let renderPassParallel px py = 
        let pertrubePixels = true
        let dirCS = 
            RayDirection (PixelToCamera (float32 px) (float32 py) (float32 width) (float32 height) fov pertrubePixels)
        let rot : Matrix4x4 = Henzai.Core.Numerics.Geometry.Rotation(ref cameraWS)
        let dirWS = Vector4.Normalize(Vector4.Transform(dirCS, rot))
        let ray = Ray(Vector4(cameraWS.Translation, 1.0f), dirWS)
        //V2 - Fastest
        for batchIndex in 0..batches-1 do
            //TODO: move ray generation into the async block
            //TODO: Remove this map for / Preallocate array
            let colorSamplesBatch = Array.map (fun i -> async {return rayTraceBase ray px py batchIndex (i-1)}) batchIndices
            let colorsBatch =  colorSamplesBatch |> Async.Parallel |> Async.RunSynchronously
            // Clear colorSamples 
            Array.blit colorSamplesClear 0 colorSamples 0 samplesPerPixel 
            // fill colorSamples
            Array.blit colorsBatch 0 colorSamples (batchIndex*batchSize) batchSize 
        if Array.isEmpty colorSamples then Vector4.Zero else (Array.reduce (+) colorSamples)/(float32)samplesPerPixel

    let renderPass px py threadId it = 
        let pertrubePixels = true
        let dirCS = 
            RayDirection (PixelToCamera (float32 px) (float32 py) (float32 width) (float32 height) fov pertrubePixels)
        let rot : Matrix4x4 = Henzai.Core.Numerics.Geometry.Rotation(ref cameraWS)
        let dirWS = Vector4.Normalize(Vector4.Transform(dirCS, rot))
        let ray = Ray(Vector4(cameraWS.Translation, 1.0f), dirWS)
        
        rayTraceBase ray px py threadId it


    //TODO: Merge RenderSceneParallel and renderPassParallel.
    // At the moment they are exclusive because Async.Parallel can not be called within each other
    [<Benchmark>]
    member self.RenderScene() =
        for px in 0..width-1 do
            for py in 0..height-1 do
                let avgColor = renderPassParallel px py
                //Gamma correct TODO: refactor
                frameBuffer.[px,py] <- Vector4.SquareRoot(avgColor)

    member self.RenderSceneSync() =
        for px in 0..width-1 do
            for py in 0..height-1 do
                let mutable avgColor = Vector4(0.0f, 0.0f, 0.0f, 1.0f)
                for it in 0..samplesPerPixel-1 do
                    avgColor <- avgColor + renderPass px py 0 it
                //Gamma correct TODO: refactor
                frameBuffer.[px,py] <- Vector4.SquareRoot(avgColor/(float32)samplesPerPixel)

    member self.RenderSceneParallel() =
        let stopWatch = System.Diagnostics.Stopwatch.StartNew()
        for i in 0..batchSize-1 do
            let seed = stopWatch.Elapsed.TotalMilliseconds*100000.0
            randomStateForThread.[i]<-Random((int)seed)
        stopWatch.Stop()

        let pixelRenderCalls = Array.zeroCreate<Async<Color>> batchSize
        let addColorCalls = Array.zeroCreate<Async<unit>> batchSize
        let toneMapCalls = Array.zeroCreate<Async<unit>> batchSize

        for px in 0..width-1 do
            for py in 0..batchSize..height-batchSize do
                for it in 0..samplesPerPixel-1 do
                    for i in 0..batchSize-1 do
                        let x, y = px, py+i
                        pixelRenderCalls.[i]<-async {return renderPass x y i it}
                    let colors = pixelRenderCalls |> Async.Parallel |> Async.RunSynchronously
       
                    for i in 0..batchSize-1 do
                        addColorCalls.[i]<-
                            async {return frameBuffer.[px,py+i] <- frameBuffer.[px,py+i] + colors.[i]}
                    addColorCalls |> Async.Parallel |> Async.RunSynchronously |> ignore

                for i in 0..batchSize-1 do
                    toneMapCalls.[i] <-
                        async{return frameBuffer.[px,py+i] <- Vector4.SquareRoot(frameBuffer.[px,py+i]/(float32)samplesPerPixel)}
                toneMapCalls |> Async.Parallel |> Async.RunSynchronously |> ignore

    member self.SaveFrameBuffer() =
        using (File.OpenWrite(sceneImagePath)) (fun output ->
            using(new Image<Rgba32>(width, height))(fun image -> 
                for px in 0..width-1 do
                    for py in 0..height-1 do
                        image.Item(px, py) <- Rgba32(frameBuffer.[px,py])
                    
                image.SaveAsJpeg(output)
            )
        )

    member self.SaveDepthBuffer() =
        using (File.OpenWrite(depthImagePath)) (fun output ->
            using(new Image<Rgba32>(width, height))(fun image -> 
                for px in 0..width-1 do
                    for py in 0..height-1 do
                        let color = Vector4(Vector3(depthBuffer.[px,py]/maxFrameBufferDepth), 1.0f)
                        image.Item(px, py) <- Rgba32(color)
                    
                image.SaveAsJpeg(output)
            )
        )