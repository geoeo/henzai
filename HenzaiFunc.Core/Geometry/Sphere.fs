module HenzaiFunc.Core.Geometry.Sphere

open System
open System.Numerics
open HenzaiFunc.Core.Types
open HenzaiFunc.Core.Geometry.Ray
open HenzaiFunc.Core.Geometry.Hitable
open HenzaiFunc.Core.Acceleration.Boundable
open Henzai.Core.Numerics

// http://mathworld.wolfram.com/Sphere.html
let parametricEquationOfASpehre (r : Radius) (phi : Radians) (theta : Radians) =
    assert (phi >= 0.0f && phi <= MathF.PI)

    r*Vector3(MathF.Sin(phi)*MathF.Cos(theta), MathF.Sin(phi)*MathF.Sin(theta), MathF.Cos(phi))
   

type Sphere(sphereCenter : Point, radius : Radius) =
    inherit Hitable () with

        // override this.TMin = 0.000001f// 0.0001// 0.000001f
        let center = sphereCenter

        let radius = radius
        
        // values for calculating a bounding box 
        let minThetaBounding = 14.0f*MathF.PI / 8.0f

        let minPhiBounding = 3.0f*MathF.PI/ 4.0f

        let maxThetaBounding = 6.0f * MathF.PI / 8.0f

        let maxPhiBounding = MathF.PI / 4.0f

        member this.Radius = radius

        member this.IntersectWith (t : LineParameter) (ray : Ray) =
            ((ray.Origin + t*ray.Direction) - center).Length() <= radius

        member this.Intersections (ray : Ray) = 
            //A is always one
            let centerToRay = ray.Origin - center
            let B = 2.0f*Vector3.Dot(centerToRay,ray.Direction)
            let C = Vector3.Dot(centerToRay,centerToRay)-(Henzai.Core.Numerics.Utils.Square(radius))
            let discriminant = B*B - 4.0f*C
            if discriminant < 0.0f then (false, 0.0f,0.0f)
            // TODO: may cause alsiasing investigate around sphere edges
            else if MathF.Round(discriminant, 3) = 0.0f then (false,-B/(2.0f),System.Single.MinValue)
            else (true,((-B + MathF.Sqrt(discriminant))/(2.0f)),((-B - MathF.Sqrt(discriminant))/(2.0f)))

        override this.Intersect (ray : Ray) = 
            let (hasIntersection,i1,i2) = this.Intersections (ray : Ray)
            if i1 >= this.TMin && i2 >= this.TMin then
                (hasIntersection,MathF.Min(i1,i2))
            else if i1 < 0.0f then
                (hasIntersection,i2)
            else
                (hasIntersection,i1)

        override this.NormalForSurfacePoint (positionOnSphere:Point) =
            Vector3.Normalize((positionOnSphere - center))

        override this.HasIntersection ray =
            let (hasIntersection,_,_) = this.Intersections ray 
            hasIntersection

        override this.IntersectionAcceptable hasIntersection t _ _ =
            hasIntersection && t > this.TMin

        override this.IsObstructedBySelf ray =
            let (b,i1,i2) = this.Intersections ray
            this.IntersectionAcceptable b (MathF.Max(i1, i2)) 1.0f Vector3.Zero

        interface Boundable with
            override this.GetBounds =

                let pMin = parametricEquationOfASpehre radius minPhiBounding minThetaBounding
                let pMax = parametricEquationOfASpehre radius maxPhiBounding maxThetaBounding

                struct(pMin, pMax)

            override this.IsBoundable = true

