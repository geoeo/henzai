namespace HenzaiFunc.Core.Acceleration

open System
open System.Numerics
open HenzaiFunc.Core.Types
open HenzaiFunc.Core.RaytraceGeometry
open Henzai.Core.Numerics

// Phyisically Based Rendering Third Edition p. 78

module AABB =

    let center (aabb : AABB) = ((aabb.PMin + aabb.PMax) / 2.0f) : Point

    let unionWithPoint (aabb : AABB) (p : Point)  =
      let newMin = Vector4(MathF.Min(aabb.PMin.X, p.X), MathF.Min(aabb.PMin.Y, p.Y), MathF.Min(aabb.PMin.Z, p.Z), 0.0f)
      let newMax = Vector4(MathF.Max(aabb.PMax.X, p.X), MathF.Max(aabb.PMax.Y, p.Y), MathF.Max(aabb.PMax.Z, p.Z), 0.0f)
      AABB(newMin, newMax)

    let unionWithAABB (aabb1 : AABB) (aabb2 : AABB)  =
      let newMin = Vector4(MathF.Min(aabb1.PMin.X, aabb2.PMin.X), MathF.Min(aabb1.PMin.Y, aabb2.PMin.Y), MathF.Min(aabb1.PMin.Z, aabb2.PMin.Z), 0.0f)
      let newMax = Vector4(MathF.Max(aabb1.PMax.X, aabb2.PMax.X), MathF.Max(aabb1.PMax.Y, aabb2.PMax.Y), MathF.Max(aabb1.PMax.Z, aabb2.PMax.Z), 0.0f)
      AABB(newMin, newMax)

    let intersect (aabb1 : AABB) (aabb2 : AABB) = 
      let newMin = Vector4(MathF.Max(aabb1.PMin.X, aabb2.PMin.X), MathF.Max(aabb1.PMin.Y, aabb2.PMin.Y), MathF.Max(aabb1.PMin.Z, aabb2.PMin.Z), 0.0f)
      let newMax = Vector4(MathF.Min(aabb1.PMax.X, aabb2.PMax.X), MathF.Min(aabb1.PMax.Y, aabb2.PMax.Y), MathF.Min(aabb1.PMax.Z, aabb2.PMax.Z), 0.0f)
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
      let newMin = Vector4(aabb.PMin.X - delta, aabb.PMin.Y - delta, aabb.PMin.Z - delta, 0.0f)
      let newMax = Vector4(aabb.PMax.X + delta, aabb.PMax.Y + delta, aabb.PMax.Z + delta, 0.0f)
      AABB(newMin, newMax)

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
      if diagonal.X > diagonal.Y && diagonal.X > diagonal.Z then SplitAxis.X
      else if diagonal.Y > diagonal.Z then SplitAxis.Y
      else SplitAxis.Z

    let lerp (parameterVector : Vector4) (aabb : AABB) =
      let interpX = Utils.Lerp(parameterVector.X, aabb.PMin.X, aabb.PMax.X)
      let interpY = Utils.Lerp(parameterVector.Y, aabb.PMin.Y, aabb.PMax.Y)
      let interpZ = Utils.Lerp(parameterVector.Z, aabb.PMin.Z, aabb.PMax.Z)
      Vector4(interpX, interpY, interpZ, 1.0f) : Point


    /// Returns a poisition between pMin (0, 0, 0) and pMax (1, 1, 1)
    let offset (aabb : AABB) (p : Point) =
      let mutable offset = p - aabb.PMin
      // How can this happen??
      if (aabb.PMax.X > aabb.PMin.X) then offset.X <- offset.X / (aabb.PMax.X - aabb.PMin.X)
      if (aabb.PMax.Y > aabb.PMin.Y) then offset.Y <- offset.Y / (aabb.PMax.Y - aabb.PMin.Y)
      if (aabb.PMax.Z > aabb.PMin.Z) then offset.Z <- offset.Z / (aabb.PMax.Z - aabb.PMin.Z)
      offset : Point

    /// d is from the equation of a plane 0 = xa + yb + zc + d
    /// Assuming positive halfspace is "inside" of the plane i.e. normal pointing inwards
    /// Realtime Rendering Third Editin p. 756
    let PlaneIntersection (aabb: AABB) (planeNormal : byref<Vector4>) (d : float32) =
        let mutable c = center aabb
        let h = (diagonal aabb) / 2.0f
        let planeNormalAbs = Vector4.Abs(planeNormal)
        let e = h.X*planeNormalAbs.X + h.Y*planeNormalAbs.Y + h.Z*planeNormalAbs.Z
        let s = Vector.InMemoryDotProduct(&c, &planeNormal) + d

        if s - e < 0.0f then IntersectionResult.OUTSIDE
        elif s + e > 0.0f then IntersectionResult.INSIDE
        else IntersectionResult.INTERSECTING








