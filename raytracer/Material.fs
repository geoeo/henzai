module Raytracer.Material

open System.Numerics
open Raytracer.Numerics
open SixLabors.ImageSharp.PixelFormats

type Color = Vector3

[<Struct>]
type Material =
        val Albedo : Color
        val Emmitance : Color
        new (color : Color) = {Albedo = color; Emmitance = Vector3.Zero}
        new (color : Rgba32) = {Albedo = (ToVec3 (color.ToVector4())); Emmitance = Vector3.Zero}
        new (color : Rgba32, albedoFactor : float32, emmitance : Rgba32, emmitingFactor : float32) = {
                Albedo = albedoFactor*(ToVec3 (color.ToVector4())); 
                Emmitance = emmitingFactor*(ToVec3 (emmitance.ToVector4())); 
                }
