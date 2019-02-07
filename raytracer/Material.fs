module Raytracer.Material

open System.Numerics
open Raytracer.Numerics
open SixLabors.ImageSharp.PixelFormats
open Henzai.Core.Numerics

type Color = Vector3

[<Struct>]
type Material =
        val Albedo : Color
        val Emmitance : Color
        new (color : Color) = {Albedo = color; Emmitance = Vector3.Zero}
        new (color : Rgba32) = {
                Albedo = (Henzai.Core.Numerics.Vector.ToVec3 (color.ToVector4())); Emmitance = Vector3.Zero}
        new (color : Rgba32, albedoFactor : float32, emmitance : Rgba32, emmitingFactor : float32) = {
                Albedo = albedoFactor*(Henzai.Core.Numerics.Vector.ToVec3 (color.ToVector4())); 
                Emmitance = emmitingFactor*(Henzai.Core.Numerics.Vector.ToVec3 (emmitance.ToVector4())); 
                }
