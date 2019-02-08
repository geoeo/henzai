module Raytracer.Geometry.Hitable

open Raytracer.Geometry.Types
open Raytracer.Geometry.Ray
open System.Numerics

[<AbstractClass>]
type Hitable ()  =
    abstract member HasIntersection: Ray -> bool
    abstract member Intersect: Ray -> bool*LineParameter
    abstract member IntersectionAcceptable : bool -> LineParameter -> float32 -> Point -> bool
    abstract member NormalForSurfacePoint : Point -> Normal
    abstract member IsObstructedBySelf: Ray -> bool 
    abstract member TMin : float32
    abstract member TMax : float32

    // effects shadow acne
    default this.TMin = 0.001f
    default this.TMax = 500.0f
    default this.HasIntersection _ = false
    default this.Intersect _ = (false, 0.0f)
    default this.IntersectionAcceptable _ _ _ _ = false
    default this.NormalForSurfacePoint _ = Vector3.Zero
    default this.IsObstructedBySelf _ = false