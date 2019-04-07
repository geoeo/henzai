module Raytracer.Material

open System.Numerics
open SixLabors.ImageSharp.PixelFormats
open Henzai.Core.Materials;

type Color = Vector4

[<Struct>]
type Material =
        val Albedo : Color
        val Emmitance : Color
        new (color : Color) = {Albedo = color; Emmitance = Vector4.Zero}
        new (color : Rgba32) = {Albedo = color.ToVector4(); Emmitance = Vector4.Zero}
        new (color : Rgba32, albedoFactor: float32, emmitance: Rgba32, emmitingFactor: float32) = {
                Albedo = albedoFactor*color.ToVector4(); 
                Emmitance = emmitingFactor*emmitance.ToVector4(); 
                }

        //TODO: Think about making Albedo mutable or maybe reworking the CoreMaterial concept.
        //interface CoreMaterial with 
            //override this.ApplyMaterialDataInto(colors: Vector4[], textureStrings: string[], cubeMapStrings: string[]) =
                //this.Albedo <- colors.[1]
                
