module Raytracer.Surface.SurfaceFactory

open System
open System.Numerics
open Raytracer.RuntimeParameters
open Raytracer.Surface.Surface
open Raytracer.Surface.Lambertian
open Raytracer.Surface.Metal
open Raytracer.Surface.Dielectric
open Raytracer.Surface.DebugSurfaces
open Raytracer.Surface.SurfaceTypes
open HenzaiFunc.Core.RaytraceGeometry
open Henzai.Core.VertexGeometry
open Henzai.Core.Materials
open Henzai.Core.Numerics
open Henzai.Core.Reflection
open Henzai.Core

let convertModelToSurfaceList (model: Model<'T, RaytraceMaterial> when 'T :> VertexTangentspace, vertexTypeTransform : 'T -> 'T, vertexRuntimeType : VertexRuntimeTypes, surfaceType : SurfaceTypes) =
    
    if not(Verifier.VerifyVertexStruct<'T>(vertexRuntimeType))
        then failwithf "Verifier Failed on vertexRuntimeType"
    
    let meshCount = model.MeshCount
    let mutable surfaceList: Surface list = []
    let mutable totalVertexCount = 0
    for i in 0..meshCount-1 do
        let mesh = model.GetMesh(i)
        let material = model.GetMaterial(i)

        let indexCount = mesh.IndexCount
        let indices = mesh.Indices
        let vertices : 'T[] = Array.map vertexTypeTransform mesh.Vertices

        totalVertexCount <- totalVertexCount + mesh.VertexCount
        
        for j in 0..3..indexCount-1 do
            let i1 = (int)indices.[j]
            let i2 = (int)indices.[j+1]
            let i3 = (int)indices.[j+2]  

            let v1 = vertices.[i1]
            let v2 = vertices.[i2]
            let v3 = vertices.[i3]

            let n1 = Vector4(v1.GetNormal(), 0.0f)
            let n2 = Vector4(v2.GetNormal(), 0.0f)
            let n3 = Vector4(v3.GetNormal(), 0.0f)

            let triangle = 
                match vertexRuntimeType with
                | VertexRuntimeTypes.VertexPosition -> new IndexedTriangle<'T>(i1, i2, i3, vertices) :> RaytracingGeometry
                | VertexRuntimeTypes.VertexPositionNormal -> new TriangleNormal<'T>(Vector.ToVec3(v1.GetPosition()), Vector.ToVec3(v2.GetPosition()), Vector.ToVec3(v3.GetPosition()), n1, n2, n3) :> RaytracingGeometry
                | x -> failwithf "vertexRuntimeType %u not implemented yet" (LanguagePrimitives.EnumToValue x)

            //TODO: allow to pass surface parameters
            let surface = 
                match surfaceType with
                | SurfaceTypes.NoSurface -> NoSurface(assignIDAndIncrement id, triangle, material)  :> Surface
                | SurfaceTypes.Lambertian -> Lambertian(assignIDAndIncrement id, triangle, material)  :> Surface
                | SurfaceTypes.Metal -> Metal(assignIDAndIncrement id, triangle, material, 0.3f) :> Surface
                | SurfaceTypes.Dielectric -> Dielectric(assignIDAndIncrement id, triangle, material, 1.5f)  :> Surface
                | SurfaceTypes.NormalVis -> NormalVis(assignIDAndIncrement id, triangle, material) :> Surface
                | x -> failwithf "surfaceType %u not implemented yet" (LanguagePrimitives.EnumToValue x)

            surfaceList <- surface :: surfaceList
    printfn "Total Vertex Count: %i" totalVertexCount
    surfaceList