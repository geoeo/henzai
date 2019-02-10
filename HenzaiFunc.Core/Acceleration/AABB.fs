module HenzaiFunc.Core.Acceleration.AABB

open System
open System.Numerics
open HenzaiFunc.Core.Types
open HenzaiFunc.Core.Geometry.Sphere
open Henzai.Core.Numerics

type AABB(pMin : MinPoint, pMax : MaxPoint) = 

   let boundingCorners : Point[] = [|pMin; pMax|]

   member this.Corner (index : int) =
    
      let cornerXIndex = index &&& 1
      let cornerYIndex = index &&& 2
      let cornerZIndex = index &&& 4

      let cornerX = boundingCorners.[cornerXIndex].X
      let cornerY = boundingCorners.[cornerYIndex].Y
      let cornerZ = boundingCorners.[cornerZIndex].Z

      Vector3(cornerX, cornerY, cornerZ) : Point

   member this.PMin = boundingCorners.[0]
   member this.PMax = boundingCorners.[1]

// Phyisically Based Rendering Third Edition p. 78

let union (aabb : AABB) (p : Point)  =
   let newMin = Vector3(MathF.Min(aabb.PMin.X, p.X), MathF.Min(aabb.PMin.Y, p.Y), MathF.Min(aabb.PMin.Z, p.Z))
   let newMax = Vector3(MathF.Max(aabb.PMax.X, p.X), MathF.Max(aabb.PMax.Y, p.Y), MathF.Max(aabb.PMax.Z, p.Z))
   AABB(newMin, newMax)

let intersect (aabb1 : AABB) (aabb2 : AABB) = 
   let newMin = Vector3(MathF.Max(aabb1.PMin.X, aabb2.PMin.X), MathF.Max(aabb1.PMin.Y, aabb2.PMin.Y), MathF.Max(aabb1.PMin.Z, aabb2.PMin.Z))
   let newMax = Vector3(MathF.Min(aabb1.PMax.X, aabb2.PMax.X), MathF.Min(aabb1.PMax.Y, aabb2.PMax.Y), MathF.Min(aabb1.PMax.Z, aabb2.PMax.Z))
   AABB(newMin, newMax)

let overlaps (aabb1 : AABB) (aabb2 : AABB) = 
   let xOverlap = aabb1.PMax.X >= aabb2.PMin.X && aabb1.PMin.X <= aabb2.PMax.X
   let yOverlap = aabb1.PMax.Y >= aabb2.PMin.Y && aabb1.PMin.Y <= aabb2.PMax.Y
   let zOverlap = aabb1.PMax.Z >= aabb2.PMin.Z && aabb1.PMin.Z <= aabb2.PMax.Z
   xOverlap && yOverlap && zOverlap

let inside (aabb : AABB) (p : Point) = 
   let xInside = p.X >= aabb.PMin.X && p.X <= aabb.PMax.X
   let yInside = p.Y >= aabb.PMin.Y && p.Y <= aabb.PMax.Y
   let zInside = p.Z >= aabb.PMin.Z && p.Z <= aabb.PMax.Z
   xInside && yInside && zInside

let insideExclusive (aabb : AABB) (p : Point) = 
   let xInside = p.X >= aabb.PMin.X && p.X < aabb.PMax.X
   let yInside = p.Y >= aabb.PMin.Y && p.Y < aabb.PMax.Y
   let zInside = p.Z >= aabb.PMin.Z && p.Z < aabb.PMax.Z
   xInside && yInside && zInside

let expand (aabb : AABB) (delta : float32) =
   let newMin = Vector3(aabb.PMin.X - delta, aabb.PMin.Y - delta, aabb.PMin.Z - delta)
   let newMin = Vector3(aabb.PMax.X + delta, aabb.PMax.Y + delta, aabb.PMax.Z + delta)
   AABB(newMin, newMin)

let diagonal (aabb : AABB) = aabb.PMax - aabb.PMin

let surfaceArea (aabb : AABB) = 
   let diagonal = diagonal aabb
   2.0f * (diagonal.X * diagonal.Y + diagonal.X * diagonal.Z + diagonal.Y * diagonal.Z)

let volume (aabb : AABB) = 
   let diagonal = diagonal aabb
   diagonal.X * diagonal.Y * diagonal.Z

/// Returns the index of longest axis. 
/// X : 0, Y : 1, Z : 2
let maximumExtent (aabb : AABB) = 
   let diagonal = diagonal aabb
   if diagonal.X > diagonal.Y && diagonal.X > diagonal.Z then 0
   else if diagonal.Y > diagonal.Z then 1
   else 2

let lerp (parameterVector : Vector3) (aabb : AABB) =
   let interpX = Henzai.Core.Numerics.Utils.Lerp(parameterVector.X, aabb.PMin.X, aabb.PMax.X)
   let interpY = Henzai.Core.Numerics.Utils.Lerp(parameterVector.Y, aabb.PMin.Y, aabb.PMax.Y)
   let interpZ = Henzai.Core.Numerics.Utils.Lerp(parameterVector.Z, aabb.PMin.Z, aabb.PMax.Z)
   Vector3(interpX, interpY, interpZ)


/// Returns a poisition between pMin (0, 0, 0) and pMax (1, 1, 1)
let offset (aabb : AABB) (p : Point) =
   let mutable offset = p - aabb.PMin
   // How can this happen??
   if (aabb.PMax.X > aabb.PMin.X) then offset.X <- offset.X / (aabb.PMax.X - aabb.PMin.X)
   if (aabb.PMax.Y > aabb.PMin.Y) then offset.Y <- offset.Y / (aabb.PMax.Y - aabb.PMin.Y)
   if (aabb.PMax.Z > aabb.PMin.Z) then offset.Z <- offset.Z / (aabb.PMax.Z - aabb.PMin.Z)


let boundingSphere (aabb : AABB) =
   let center = (aabb.PMin + aabb.PMax) / 2.0f
   let radius = if inside aabb center then Henzai.Core.Numerics.Vector.Distance(ref center, ref aabb.PMax) else 0.0f
   Sphere(center, radius)







