namespace HenzaiFunc.Core.RaytraceGeometry

open System
open System.Numerics
open HenzaiFunc.Core.Types
open HenzaiFunc.Core.Acceleration
open Henzai.Core.Numerics
   

type Sphere(sphereCenter : Point, radius : Radius) =
    inherit HitableGeometry() with

        // override this.TMin = 0.000001f// 0.0001// 0.000001f
        let center = sphereCenter

        let radius = radius
        
        // values for calculating a bounding box 
        let minThetaBounding = 14.0f*MathF.PI / 8.0f

        let minPhiBounding = 3.0f*MathF.PI/ 4.0f

        let maxThetaBounding = 6.0f * MathF.PI / 8.0f

        let maxPhiBounding = MathF.PI / 4.0f

        // http://mathworld.wolfram.com/Sphere.html
        static member ParametricEquationOfASpehre (r : Radius) (phi : Radians) (theta : Radians) = 
            assert (phi >= 0.0f && phi <= MathF.PI)

            r*Vector3(MathF.Sin(phi)*MathF.Cos(theta), MathF.Sin(phi)*MathF.Sin(theta), MathF.Cos(phi))

        static member BoundingSphere (aabb : AABB) =
            let center = AABB.center aabb
            let radius = if AABB.inside aabb center then Vector.Distance(ref center, ref aabb.PMax) else 0.0f    
            Sphere(center, radius)

        member this.Radius = radius

        member this.Center = center

        member this.IntersectWith (t : LineParameter) (ray : Ray) =
            ((ray.Origin + t*ray.Direction) - center).Length() <= radius

        member this.Intersections (ray : Ray) = 
            //A is always one
            let centerToRay = ray.Origin - center
            let B = 2.0f*Vector3.Dot(centerToRay, ray.Direction)
            let C = Vector3.Dot(centerToRay, centerToRay)-(Henzai.Core.Numerics.Utils.Square(radius))
            let discriminant = B*B - 4.0f*C
            if discriminant < 0.0f then (false, 0.0f, 0.0f)
            // TODO: may cause alsiasing investigate around sphere edges
            else if MathF.Round(discriminant, 3) = 0.0f then (false, -B/(2.0f), System.Single.MinValue)
            else (true, ((-B + MathF.Sqrt(discriminant))/(2.0f)), ((-B - MathF.Sqrt(discriminant))/(2.0f)))

        interface Hitable with 

            override this.Intersect (ray : Ray) = 
                let (hasIntersection,i1,i2) = this.Intersections (ray : Ray)
                if i1 >= this.AsHitable.TMin && i2 >= this.AsHitable.TMin then
                    (hasIntersection, MathF.Min(i1, i2))
                else if i1 < 0.0f then
                    (hasIntersection, i2)
                else
                    (hasIntersection, i1)

            override this.NormalForSurfacePoint (positionOnSphere:Point) =
                Vector3.Normalize((positionOnSphere - center))

            override this.HasIntersection ray =
                let (hasIntersection,_,_) = this.Intersections ray 
                hasIntersection

            override this.IntersectionAcceptable hasIntersection t _ _ =
                hasIntersection && t > this.AsHitable.TMin

            override this.IsObstructedBySelf ray =
                let (b,i1,i2) = this.Intersections ray
                this.AsHitable.IntersectionAcceptable b (MathF.Max(i1, i2)) 1.0f Vector3.Zero

        interface AxisAlignedBoundable with
            override this.GetBounds =

                let pMin = center + Vector3(-radius, -radius, -radius)
                let pMax = center + Vector3(radius, radius, radius)

                AABB(pMin, pMax)

            override this.IsBoundable = true

        

