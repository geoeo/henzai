namespace HenzaiFunc.Core.RaytraceGeometry

open System
open System.Numerics
open HenzaiFunc.Core.Types
open Henzai.Core.Acceleration
open Henzai.Core.Numerics
open Henzai.Core.Raytracing
   

type Sphere(center : Point, radius : Radius) =
    inherit RaytracingGeometry() with

        new(sphereCenter : Vector3, radius : Radius) = 
            Sphere(Vector.ToHomogeneous(&sphereCenter, 1.0f), radius)

        //TODO: refactor candidate
        // http://mathworld.wolfram.com/Sphere.html
        static member ParametricEquationOfASpehre (r : Radius) (phi : Radians) (theta : Radians) = 
            assert (phi >= 0.0f && phi <= MathF.PI)

            r*Vector3(MathF.Sin(phi)*MathF.Cos(theta), MathF.Sin(phi)*MathF.Sin(theta), MathF.Cos(phi))

        //TODO: refactor candidate
        static member BoundingSphere (aabb : AABB) =
            let center = AABBProc.Center(aabb)
            let max = aabb.PMax
            let radius = if AABBProc.Inside(aabb, center) then Vector.Distance(&center, &max) else 0.0f    
            Sphere(center, radius)

        let aabb = 
            let pMin = center + Vector4(-radius, -radius, -radius, 0.0f)
            let pMax = center + Vector4(radius, radius, radius, 0.0f)

            AABB(pMin, pMax)

        member this.Radius = radius

        member this.Center = center

        member this.IntersectWith (t : LineParameter) (ray : Ray) =
            ((ray.Origin + t*ray.Direction) - center).Length() <= radius

        //TODO: use Henzai.Core Dot
        member this.Intersections (ray : Ray) = 
            //A is always one
            let centerToRay = ray.Origin - center
            let B = 2.0f*Vector4.Dot(centerToRay, ray.Direction)
            let C = Vector4.Dot(centerToRay, centerToRay)-(Henzai.Core.Numerics.Utils.Square(radius))
            let discriminant = B*B - 4.0f*C
            if discriminant < 0.0f then (false, 0.0f, 0.0f)
            // TODO: may cause alsiasing investigate around sphere edges
            else if MathF.Round(discriminant, 3) = 0.0f then (false, -B/(2.0f), System.Single.MinValue)
            else (true, ((-B + MathF.Sqrt(discriminant))/(2.0f)), ((-B - MathF.Sqrt(discriminant))/(2.0f)))

        interface Hitable with 

            override this.Intersect (ray : Ray) = 
                let (hasIntersection,i1,i2) = this.Intersections (ray : Ray)
                if i1 >= this.AsHitable.TMin() && i2 >= this.AsHitable.TMin() then
                    struct(hasIntersection, MathF.Min(i1, i2))
                else if i1 < 0.0f then
                    struct(hasIntersection, i2)
                else
                    struct(hasIntersection, i1)

            override this.NormalForSurfacePoint (positionOnSphere:Point) =
                Vector4.Normalize((positionOnSphere - center))

            override this.HasIntersection ray =
                let (hasIntersection,_,_) = this.Intersections ray 
                hasIntersection

            override this.IntersectionAcceptable(hasIntersection, t, _, _) =
                hasIntersection && t > this.AsHitable.TMin()

            override this.IsObstructedBySelf ray =
                let (b,i1,i2) = this.Intersections ray
                this.AsHitable.IntersectionAcceptable(b, (MathF.Max(i1, i2)), 1.0f, Vector4.Zero)

        interface AxisAlignedBoundable with
            override this.GetBounds() = aabb

            override this.IsBoundable() = true

        

