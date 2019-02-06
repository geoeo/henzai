module Raytracer.Geometry.Plane

open System
open System.Numerics
open Raytracer.Numerics
open Raytracer.Geometry.Core

/// Space inwhich points are compared if they are inside a rectangle
/// Plane is XY
let CanonicalPlaneSpace = Vector3(0.0f,0.0f,-1.0f)

type Plane(plane : System.Numerics.Plane, center : Point option, width : float32 option, height : float32 option ) = 
    inherit Hitable () with
        member this.Plane = plane

        member this.Normal = Vector3.Normalize(this.Plane.Normal)

        member this.Center = center

        member this.Width = width

        member this.Height = height

        member this.PointLiesInRectangle (point : Point) =
            let widthOff = this.Width.Value / 2.0f
            let heightOff = this.Height.Value / 2.0f
            let mutable R = Matrix4x4.Identity
            RotationBetweenUnitVectors this.Normal CanonicalPlaneSpace (&R)
            let kern = if this.Plane.D > 0.0f then -1.0f*this.Plane.D*this.Normal else this.Plane.D*this.Normal
            let v = point - kern
            let b = this.Center.Value - kern
            let newDir = Vector4.Transform((ToHomogeneous v 0.0f), R)
            let newDir_b = Vector4.Transform((ToHomogeneous b 0.0f), R)
            let newP = kern + (ToVec3 newDir)
            let newB = kern + (ToVec3 newDir_b)
            newP.X <= newB.X + widthOff && 
            newP.X >= newB.X - widthOff && 
            newP.Y <= newB.Y + heightOff && 
            newP.Y >= newB.Y - heightOff

        override this.Intersect (ray:Ray) =
            let numerator = -this.Plane.D - Plane.DotNormal(this.Plane,ray.Origin) 
            let denominator = Plane.DotNormal(this.Plane,ray.Direction)
            if Math.Abs(denominator) < this.TMin then (false, 0.0f)
            else (true, numerator / denominator)

        override this.HasIntersection (ray:Ray) = 
            let (hasIntersection,_) = this.Intersect ray 
            hasIntersection
        // dotView factor ensures sampling "straight" at very large distances due to fov
        override this.IntersectionAcceptable hasIntersection t dotViewTrace pointOnSurface =
            let generalIntersection = hasIntersection && t > this.TMin && t <= (this.TMax/dotViewTrace)
            match this.Center with
                | Some _ -> generalIntersection && this.PointLiesInRectangle pointOnSurface
                | None -> generalIntersection

        override this.NormalForSurfacePoint _ =
            this.Normal