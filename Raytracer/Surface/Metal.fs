module Raytracer.Surface.Metal

open System
open System.Numerics
open Raytracer.RuntimeParameters
open Raytracer.Surface.Surface
open HenzaiFunc.Core.Types
open HenzaiFunc.Core.RaytraceGeometry
open Henzai.Core.Materials
open Henzai.Core.Numerics
open Henzai.Core.Raytracing

type Metal(id: ID, geometry : RaytracingGeometry, material : RaytraceMaterial, fuzz : float32) =
    inherit Surface(id, geometry, material)

    member this.Fuzz = MathF.Max(MathF.Min(1.0f, fuzz), 0.0f)
    member this.Reflect (incommingRay : Ray) (normalToSurface : Normal) 
        = incommingRay.Direction - 2.0f*Vector4.Dot(incommingRay.Direction, normalToSurface)*normalToSurface 

    override this.SampleCount = metalSampleCount
    override this.Scatter (incommingRay : Ray) (t : LineParameter) (randomGen : Random) =

        let positionOnSurface = incommingRay.Origin + t*incommingRay.Direction
        let mutable normal = Vector4.Normalize(this.Geometry.AsHitable.NormalForSurfacePoint positionOnSurface)

        //sampling hemisphere
        let rand_norm = RandomSampling.RandomInUnitHemisphere(randomGen)
        let mutable nb = Vector4.Zero
        let mutable nt = Vector4.Zero
        Henzai.Core.Numerics.Geometry.CreateCoordinateSystemAroundNormal(&normal, &nt, &nb)
        let changeOfBaseMatrix = Henzai.Core.Numerics.Geometry.ChangeOfBase(&nt, &normal, &nb)
        let rand_norm_transformed = Vector4.Transform(rand_norm, changeOfBaseMatrix)
        let normalSample = rand_norm_transformed
        let modifiedNormal = Vector4.Normalize((1.0f - this.Fuzz)*normal + this.Fuzz*normalSample)

        let outDir = Vector4.Normalize(this.Reflect incommingRay modifiedNormal)
        let outRay =  Ray(positionOnSurface, outDir)    
        (outRay,1.0f)