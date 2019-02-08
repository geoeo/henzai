module HenzaiFunc.Core.Geometry.Ray

open HenzaiFunc.Core.Geometry.Types
open Henzai.Core.Numerics
open System.Numerics

[<Struct>]
type Ray =
        val Origin : Point
        val Direction : Direction
        val SurfaceOrigin : ID

        new(origin, dir) = { Origin = origin; Direction = Henzai.Core.Numerics.Vector.Normalize(&dir);SurfaceOrigin = 0UL }
        new(origin, dir, id) = { Origin = origin; Direction = Henzai.Core.Numerics.Vector.Normalize(&dir);SurfaceOrigin = id }