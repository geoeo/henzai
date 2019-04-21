module Raytracer.Surface.Surface

open System
open System.Numerics
open Raytracer.RuntimeParameters
open HenzaiFunc.Core.Types
open HenzaiFunc.Core.RaytraceGeometry
open Henzai.Core.Materials
open Henzai.Core.Numerics
open Henzai.Core.Raytracing
open Henzai.Core.Acceleration
   
//TODO: Refactor namespace + Split this up    
[<AbstractClass>]
type Surface(id: ID, geometry : RaytracingGeometry, material : RaytraceMaterial) =
    abstract member Scatter: Ray -> LineParameter -> Random -> (Ray*Cosine)
    abstract member Emitted : Ray -> LineParameter -> Color 
    abstract member SampleCount : int
    abstract member PDF : float32
    abstract member BRDF : Color
    abstract member GenerateSamples : Ray -> LineParameter -> struct(Ray* Color)[] ->Random ->(int*struct(Ray* Color)[])

    member this.ID = id
    member this.Geometry = geometry
    member this.Material = material
    member this.MCComputeBRDF cosOfIncidence = this.BRDF*(cosOfIncidence/this.PDF)
    member this.ComputeSample (ray : Ray , cosOfIncidience : Cosine) = struct(ray, this.MCComputeBRDF cosOfIncidience)
    member this.SamplesArray  = Array.zeroCreate<struct(Ray*Color)> this.SampleCount 

    default this.Scatter _ _ _ = (Ray(Vector4.UnitX, Vector4.UnitX), 1.0f)
    default this.Emitted _ _ = this.Material.Emittance
    default this.SampleCount = noSampleCount
    default this.PDF = 1.0f
    default this.BRDF = this.Material.Albedo
    default this.GenerateSamples (incommingRay : Ray) (t : LineParameter) samplesArray randomGen = 
        for i in 0..this.SampleCount-1 do
            let shading = this.ComputeSample (this.Scatter incommingRay t randomGen)
            samplesArray.SetValue(shading, i)
            
        (this.SampleCount, samplesArray)

    interface Hitable with
        member this.TMin() = this.Geometry.AsHitable.TMin()
        member this.TMax() = this.Geometry.AsHitable.TMax()
        member this.HasIntersection ray = this.Geometry.AsHitable.HasIntersection ray
        member this.Intersect ray = this.Geometry.AsHitable.Intersect ray
        member this.IntersectionAcceptable(b, t, factor, point) = this.Geometry.AsHitable.IntersectionAcceptable(b, t, factor, point)
        member this.NormalForSurfacePoint point = this.Geometry.AsHitable.NormalForSurfacePoint point
        member this.IsObstructedBySelf ray = this.Geometry.AsHitable.IsObstructedBySelf ray

    interface AxisAlignedBoundable with
        member this.GetBounds() = this.Geometry.AsBoundable.GetBounds()
        member this.IsBoundable() = this.Geometry.AsBoundable.IsBoundable()


type NoSurface(id: ID, geometry : RaytracingGeometry, material : RaytraceMaterial) =
    inherit Surface(id, geometry, material)

    override this.GenerateSamples _ _ _ _ = (noSampleCount, this.SamplesArray)

// let noSurface : Surface = upcast (NoSurface(0UL, NotHitable(), RaytraceMaterial(Vector4.Zero)))

let findClosestIntersection (ray : Ray) (surfaces : Surface[]) =
    let mutable (bMin,tMin, vMin : Surface) = (false, Single.MaxValue,  upcast (NoSurface(0UL, NotHitable(), RaytraceMaterial(Vector4.Zero))))
    for surface in surfaces do
        let struct(b,t) = surface.Geometry.AsHitable.Intersect(ray)
        if surface.Geometry.AsHitable.IntersectionAcceptable(b, t, 1.0f, (RaytraceGeometryUtils.PointForRay ray t)) &&  t < tMin then
            bMin <- b
            tMin <- t
            vMin <- surface

    (bMin, tMin, vMin)      

//https://learnopengl.com/Lighting/Light-casters
//TODO refactor constants
let attenuate distance = 1.0f/(1.0f + 0.5f*distance + 0.02f*(distance*distance))
let AllSurfacesWithoutId (surfaces : Surface list) (id : ID) =
    List.filter (fun (surface : Surface) -> surface.ID <> id) surfaces

let SurfaceWithId (surfaces : Surface list) (id : ID) =
    List.head (List.filter (fun (surface : Surface) -> surface.ID = id) surfaces)

let SurfacesToGeometry (surfaces : Surface list) =
    List.map (fun (x : Surface) -> x.Geometry) surfaces