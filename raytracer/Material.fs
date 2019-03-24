module Raytracer.Material

open System.Numerics
open SixLabors.ImageSharp.PixelFormats

type Color = Vector4

[<Struct>]
type Material =
        val Albedo : Color
        val Emmitance : Color
        new (color : Color) = {Albedo = color; Emmitance = Vector4.Zero}
        new (color : Rgba32) = {
                Albedo = color.ToVector4(); Emmitance = Vector4.Zero}
        new (color : Rgba32, albedoFactor : float32, emmitance : Rgba32, emmitingFactor : float32) = {
                Albedo = albedoFactor*color.ToVector4(); 
                Emmitance = emmitingFactor*emmitance.ToVector4(); 
                }
