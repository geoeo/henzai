module Raytracer.Surface.Dielectric

open System
open System.Numerics
open Raytracer.RuntimeParameters
open Raytracer.Surface.Surface
open HenzaiFunc.Core.Types
open HenzaiFunc.Core.RaytraceGeometry
open Henzai.Core.Materials
open Henzai.Core.Numerics
open Henzai.Core.Raytracing

// https://www.scratchapixel.com/lessons/3d-basic-rendering/introduction-to-shading/reflection-refraction-fresnel
type Dielectric(id: ID, geometry : RaytracingGeometry, material : RaytraceMaterial, refractiveIndex : float32) =
    inherit Surface(id, geometry, material)

    override this.SampleCount = dialectricSampleCount

    member this.RefractiveIndex = refractiveIndex
    //https://seblagarde.wordpress.com/2013/04/29/memo-on-fresnel-equations/
    member this.SchlickApprx (cos_incidence : float32) (refractiveIncidenceFactor : float32) (refractiveTransmissionFactor : float32) =
        let R0 = MathF.Pow((refractiveTransmissionFactor-refractiveIncidenceFactor)/(refractiveTransmissionFactor+refractiveIncidenceFactor), 2.0f)
        R0 + (1.0f - R0)*MathF.Pow((1.0f - cos_incidence), 5.0f)
    member this.Reflect (incommingRay : Ray) (normalToSurface : Normal) 
        = incommingRay.Direction - 2.0f*Vector4.Dot(incommingRay.Direction, normalToSurface)*normalToSurface 
    member this.Refract (incommingDirection : Direction) (normalToSurface : Normal) (refractiveIncidenceOverTransmission : float32) (cos_incidence : float32) =
        let discriminant = 1.0f - (Henzai.Core.Numerics.Utils.Square refractiveIncidenceOverTransmission)*(1.0f - Henzai.Core.Numerics.Utils.Square cos_incidence)
        if discriminant > 0.0f then 
            let refracted = refractiveIncidenceOverTransmission*(incommingDirection + cos_incidence*normalToSurface) - normalToSurface*MathF.Sqrt(discriminant)
            (true, Vector4.Normalize(refracted))
        // total internal refleciton
        else (false, Vector4.Zero) 
    ///<summary>
    /// Returns: (Reflect Probability,intersection Position,Reflection Dir, Refraction Dir)
    /// </summary>
    member this.CalcFresnel (incommingRay : Ray) (t : LineParameter) = 
        let positionOnSurface = incommingRay.Origin + t*incommingRay.Direction
        let normal = Vector4.Normalize(this.Geometry.AsHitable.NormalForSurfacePoint positionOnSurface)
        let reflectDir = Vector4.Normalize(this.Reflect incommingRay normal)
        let refrativeIndexAir = 1.0f

        //incidence over transmition
        let (incidenceIndex, transmissionIndex,fresnelNormal)
            // Vector is "comming out" of material into air
            = if Vector4.Dot(incommingRay.Direction, normal) > 0.0f 
              then 
                (this.RefractiveIndex, refrativeIndexAir, -normal)
              else 
                (refrativeIndexAir, this.RefractiveIndex, normal)
        let cos_incidence =  Vector4.Dot(incommingRay.Direction, -fresnelNormal)
        let (refracted, refrationDir) = this.Refract incommingRay.Direction fresnelNormal (incidenceIndex/transmissionIndex) cos_incidence
        
        //Use schlick if refraction was successful
        let reflectProb = if refracted then this.SchlickApprx cos_incidence incidenceIndex transmissionIndex  else 1.0f
        (reflectProb , positionOnSurface, reflectDir, refrationDir)

    override this.Scatter (incommingRay : Ray) (t : LineParameter) (randomGen : Random) =
        let (reflectProb, positionOnSurface, reflectDir, refractionDir) = this.CalcFresnel incommingRay t
        let randomFloat = RandomSampling.RandomFloat(randomGen)
        if randomFloat <= reflectProb 
        then 
            let reflectRay = Ray(positionOnSurface, reflectDir)
            (reflectRay, 1.0f)
        else // refraction has to have been successful
            let refractRay = Ray(positionOnSurface, refractionDir)
            (refractRay, 1.0f)

    override this.GenerateSamples (incommingRay : Ray) (t : LineParameter) samplesArray _ =
        let (reflectProb, positionOnSurface, reflectDir, refractionDir) = this.CalcFresnel incommingRay t
        let reflectRay = Ray(positionOnSurface, reflectDir)
        let reflectShading : Color = this.BRDF*reflectProb
        if MathF.Round(reflectProb, 3) = 1.0f then 
            samplesArray.SetValue(struct(reflectRay, reflectShading), 0)
            (1, samplesArray)
        else
            let refractRay = Ray(positionOnSurface, refractionDir)
            let refractShading : Color = this.BRDF*(1.0f - reflectProb)
            // Since this is a "fake brdf" we need to multiply by 2 since we are diving by the sample count
            samplesArray.SetValue(struct(reflectRay, 2.0f*reflectShading), 0)
            samplesArray.SetValue(struct(refractRay, 2.0f*refractShading), 1)
            (2, samplesArray)