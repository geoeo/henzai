namespace HenzaiFunc.Core.RaytraceGeometry

open System.Numerics
open HenzaiFunc.Core.Types
open System.Runtime.CompilerServices
open Henzai.Core.Numerics

[<IsReadOnly;Struct>]
type Ray =
        val Origin : Point
        val Direction : Direction

        new(origin : Point, dir : Direction) = { Origin = origin; Direction = Vector.Normalize(ref dir)}
        new(origin : Vector3, dir : Vector3) = { Origin = Vector4(origin,1.0f); Direction = Vector4(Vector.Normalize(ref dir),0.0f)}