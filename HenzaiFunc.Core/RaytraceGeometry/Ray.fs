namespace HenzaiFunc.Core.RaytraceGeometry

open System.Numerics
open HenzaiFunc.Core.Types
open System.Runtime.CompilerServices
open Henzai.Core.Numerics

[<IsReadOnly;Struct>]
type Ray =
        val Origin : Point
        val Direction : Direction
        val SurfaceOrigin : ID

        new(origin : Point, dir : Direction) = { Origin = origin; Direction = Vector.Normalize(ref dir);SurfaceOrigin = 0UL }
        new(origin : Point, dir : Direction, id) = { Origin = origin; Direction = Vector.Normalize(ref dir);SurfaceOrigin = id }
        new(origin : Vector3, dir : Vector3) = { Origin = Vector4(origin,1.0f); Direction = Vector4(Vector.Normalize(ref dir),0.0f);SurfaceOrigin = 0UL }
        new(origin : Vector3, dir : Vector3, id) = { Origin = Vector4(origin,1.0f); Direction = Vector4(Vector.Normalize(ref dir), 0.0f);SurfaceOrigin = id }