module Raytracer.Surface.DebugSurfaces

open System
open System.Numerics
open Raytracer.RuntimeParameters
open Raytracer.Surface.Surface
open HenzaiFunc.Core.Types
open HenzaiFunc.Core.RaytraceGeometry
open Henzai.Core.Materials
open Henzai.Core.Numerics
open Henzai.Core.Raytracing

type NormalVis(id: ID, geometry : RaytracingGeometry, material : RaytraceMaterial) =
    inherit Surface(id, geometry, material)

    override this.Emitted (incommingRay : Ray) (t : LineParameter) =
        let positionOnSurface = incommingRay.Origin + t*incommingRay.Direction
        this.Geometry.AsHitable.NormalForSurfacePoint positionOnSurface

    override this.GenerateSamples _ _ _ _ = (noSampleCount, this.SamplesArray)

type IntersectVis(id: ID, geometry : RaytracingGeometry, material : RaytraceMaterial) =
    inherit Surface(id, geometry, material)

    override this.Emitted (incommingRay : Ray) (t : LineParameter) =
        let positionOnSurface = incommingRay.Origin + t*incommingRay.Direction
        this.Geometry.AsHitable.NormalForSurfacePoint positionOnSurface
        
    override this.GenerateSamples _ _ _ _ = (noSampleCount, this.SamplesArray)