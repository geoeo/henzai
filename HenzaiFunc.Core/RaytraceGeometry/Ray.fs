namespace HenzaiFunc.Core.RaytraceGeometry

open HenzaiFunc.Core.Types
open System.Runtime.CompilerServices
open Henzai.Core.Numerics

[<IsReadOnly;Struct>]
type Ray =
        val Origin : Point
        val Direction : Direction
        val SurfaceOrigin : ID

        new(origin, dir : Direction) = { Origin = origin; Direction = Vector.Normalize(ref dir);SurfaceOrigin = 0UL }
        new(origin, dir : Direction, id) = { Origin = origin; Direction = Vector.Normalize(ref dir);SurfaceOrigin = id }