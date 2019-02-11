module HenzaiFunc.Core.RaytraceGeometry.Ray

open HenzaiFunc.Core.Types
open Henzai.Core.Numerics
open System.Numerics

[<Struct>]
type Ray =
        val Origin : Point
        val Direction : Direction
        val SurfaceOrigin : ID

        new(origin, dir) = { Origin = origin; Direction = Henzai.Core.Numerics.Vector.Normalize(ref dir);SurfaceOrigin = 0UL }
        new(origin, dir, id) = { Origin = origin; Direction = Henzai.Core.Numerics.Vector.Normalize(ref dir);SurfaceOrigin = id }