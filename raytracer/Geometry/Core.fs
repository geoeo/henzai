module Raytracer.Geometry.Core

open System.Numerics
open Raytracer.Numerics

type Origin = Vector3 // Position of a point in 3D space
type Direction = Vector3  
type Normal = Vector3
type Offset = float32  
type Radius = float32
type LineParameter = float32
type Point = Vector3
type Cosine = float32

type Ray =
    struct
        val Origin : Vector3
        val Direction : Vector3
        val SurfaceOrigin : uint64

        new(origin, dir) = { Origin = origin; Direction = NormalizedOrFail(&dir);SurfaceOrigin = 0UL }
        new(origin, dir, id) = { Origin = origin; Direction = NormalizedOrFail(&dir);SurfaceOrigin = id }

    end

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


type NotHitable() = inherit Hitable ()
 


/// Does the ray penetrating the surface have a t < tCompare
let IsIntersectionInfrontOf (geometry : Hitable) (ray : Ray) (tCompare : LineParameter) = 
        let (hasIntersections,t) = geometry.Intersect ray
        if hasIntersections && t > geometry.TMin then t < tCompare else false

let ParameterToPointForRay (ray : Ray) (point : Point) =
    if ray.Direction.X = 0.0f then (point.Y - ray.Origin.Y)/ray.Direction.Y
    else (point.X - ray.Origin.X)/ray.Direction.X

let PointForRay (ray : Ray) (t : LineParameter) = ray.Origin + t*ray.Direction

let smallestIntersection (b,t,x) (b_new,t_new,x_new) =
    if t <= t_new then (b,t,x)
    else (b_new,t_new,x_new)

let flattenIntersection ((b,t),x) = (b,t,x)
       

