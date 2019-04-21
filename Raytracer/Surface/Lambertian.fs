module Raytracer.Surface.Lambertian

open System
open System.Numerics
open Raytracer.RuntimeParameters
open Raytracer.Surface.Surface
open HenzaiFunc.Core.Types
open HenzaiFunc.Core.RaytraceGeometry
open Henzai.Core.Materials
open Henzai.Core.Numerics
open Henzai.Core.Raytracing

type Lambertian(id: ID, geometry : RaytracingGeometry, material : RaytraceMaterial) =
    inherit Surface(id, geometry, material)

    override this.SampleCount = lambertianSampleCount
    override this.PDF = 1.0f / (2.0f * MathF.PI)
    override this.BRDF = this.Material.Albedo / MathF.PI
    override this.Scatter (incommingRay : Ray) (t : LineParameter) (randomGen : Random) =
        //TODO:@Perf external dependecies seem to slow down this section
        let positionOnSurface = incommingRay.Origin + t*incommingRay.Direction
        let mutable normal = this.Geometry.AsHitable.NormalForSurfacePoint positionOnSurface

        //sampling hemisphere
        let rand_norm = RandomSampling.RandomInUnitHemisphere(randomGen)
        let cosOfIncidence = rand_norm.Y
        let mutable nb = Vector4.Zero
        let mutable nt = Vector4.Zero
        Henzai.Core.Numerics.Geometry.CreateCoordinateSystemAroundNormal(&normal, &nt, &nb)
        let changeOfBaseMatrix = Henzai.Core.Numerics.Geometry.ChangeOfBase(&nt, &normal, &nb)
        let normalSample = Vector4.Transform(rand_norm, changeOfBaseMatrix)

        let outDir = Vector4.Normalize(normalSample)
        let outRay = Ray(positionOnSurface, outDir)
        (outRay, cosOfIncidence)
