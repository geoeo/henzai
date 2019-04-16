module Raytracer.Surface

open System
open System.Numerics
open Raytracer.RuntimeParameters
open HenzaiFunc.Core.Types
open HenzaiFunc.Core.RaytraceGeometry
open Henzai.Core.Materials;
open Henzai.Core.Numerics

let randomState = Random()
   
//TODO: Refactor namespace + Split this up    
[<AbstractClass>]
type Surface(id: ID, geometry : RaytracingGeometry, material : RaytraceMaterial) =
    //let mutable samplesArray  = Array.zeroCreate<Ray*Raytracer.Material.Color> this.SampleCount

    abstract member Scatter: Ray -> LineParameter -> int -> (bool*Ray*Cosine)
    abstract member Emitted : Ray -> LineParameter -> Color 
    abstract member SampleCount : int
    abstract member PDF : float32
    abstract member BRDF : Color
    abstract member GenerateSamples : Ray -> LineParameter -> int -> struct(Ray* Color)[]->(int*struct(Ray* Color)[])

    member this.ID = id
    member this.Geometry = geometry
    member this.Material = material
    member this.MCComputeBRDF cosOfIncidence = this.BRDF*(cosOfIncidence/this.PDF)
    member this.ComputeSample (b : bool , ray : Ray , cosOfIncidience : Cosine) = struct(ray, this.MCComputeBRDF cosOfIncidience)
    member this.SamplesArray  = Array.zeroCreate<struct(Ray*Color)> this.SampleCount 

    default this.Scatter _ _ _ = (true, Ray(Vector4.UnitX, Vector4.UnitX), 1.0f)
    default this.Emitted _ _ = this.Material.Emittance
    default this.SampleCount = noSampleCount
    default this.PDF = 1.0f
    default this.BRDF = this.Material.Albedo
    default this.GenerateSamples (incommingRay : Ray) (t : LineParameter) (depthLevel : int) samplesArray  = 
        for i in 0..this.SampleCount-1 do
            let shading = this.ComputeSample (this.Scatter incommingRay t depthLevel)
            samplesArray.SetValue(shading, i)
        (this.SampleCount, samplesArray)

    interface Hitable with
        member this.TMin = this.Geometry.AsHitable.TMin
        member this.TMax = this.Geometry.AsHitable.TMax
        member this.HasIntersection ray = this.Geometry.AsHitable.HasIntersection ray
        member this.Intersect ray = this.Geometry.AsHitable.Intersect ray
        member this.IntersectionAcceptable b t factor point = this.Geometry.AsHitable.IntersectionAcceptable b t factor point
        member this.NormalForSurfacePoint point = this.Geometry.AsHitable.NormalForSurfacePoint point
        member this.IsObstructedBySelf ray = this.Geometry.AsHitable.IsObstructedBySelf ray

    interface AxisAlignedBoundable with
        member this.GetBounds = this.Geometry.AsBoundable.GetBounds
        member this.IsBoundable = this.Geometry.AsBoundable.IsBoundable


type NoSurface(id: ID, geometry : RaytracingGeometry, material : RaytraceMaterial) =
    inherit Surface(id, geometry, material)

    override this.GenerateSamples _ _ _ _ = (noSampleCount, this.SamplesArray)


type Lambertian(id: ID, geometry : RaytracingGeometry, material : RaytraceMaterial) =
    inherit Surface(id, geometry, material)

    override this.SampleCount = lambertianSampleCount
    override this.PDF = 1.0f / (2.0f * MathF.PI)
    override this.BRDF = this.Material.Albedo / MathF.PI
    override this.Scatter (incommingRay : Ray) (t : LineParameter) (depthLevel : int) =

        let positionOnSurface = incommingRay.Origin + t*incommingRay.Direction
        let mutable normal = this.Geometry.AsHitable.NormalForSurfacePoint positionOnSurface

        //sampling hemisphere
        let rand_norm = RandomSampling.RandomInUnitHemisphere_Sync()
        let cosOfIncidence = rand_norm.Y
        let mutable nb = Vector4.Zero
        let mutable nt = Vector4.Zero
        Henzai.Core.Numerics.Geometry.CreateCoordinateSystemAroundNormal(&normal, &nt, &nb)
        let changeOfBaseMatrix = Henzai.Core.Numerics.Geometry.ChangeOfBase(&nt, &normal, &nb)
        let normalSample = Vector4.Transform(rand_norm, changeOfBaseMatrix)

        let outDir = Vector4.Normalize(normalSample)
        let outRay = Ray(positionOnSurface, outDir)
        (true, outRay, cosOfIncidence)

type NormalVis(id: ID, geometry : RaytracingGeometry, material : RaytraceMaterial) =
    inherit Surface(id, geometry, material)

    override this.Emitted (incommingRay : Ray) (t : LineParameter) =
        let positionOnSurface = incommingRay.Origin + t*incommingRay.Direction
        this.Geometry.AsHitable.NormalForSurfacePoint positionOnSurface

    override this.GenerateSamples _ _ _ _ = (noSampleCount, this.SamplesArray)

type IntersectVis(id: ID, geometry : RaytracingGeometry, material : RaytraceMaterial) =
    inherit Surface(id, geometry, material)

    override this.Emitted (incommingRay : Ray) (t : LineParameter) =
        let positionOnSurface = incommingRay.Origin + t*incommingRay.Direction
        this.Geometry.AsHitable.NormalForSurfacePoint positionOnSurface
        
    override this.GenerateSamples _ _ _ _ = (noSampleCount, this.SamplesArray)


type Metal(id: ID, geometry : RaytracingGeometry, material : RaytraceMaterial, fuzz : float32) =
    inherit Surface(id, geometry, material)

    member this.Fuzz = MathF.Max(MathF.Min(1.0f, fuzz), 0.0f)
    member this.Reflect (incommingRay : Ray) (normalToSurface : Normal) 
        = incommingRay.Direction - 2.0f*Vector4.Dot(incommingRay.Direction, normalToSurface)*normalToSurface 

    override this.SampleCount = metalSampleCount
    override this.Scatter (incommingRay : Ray) (t : LineParameter) (depthLevel : int) =

        let positionOnSurface = incommingRay.Origin + t*incommingRay.Direction
        let mutable normal = Vector4.Normalize(this.Geometry.AsHitable.NormalForSurfacePoint positionOnSurface)

        //sampling hemisphere
        let rand_norm = RandomSampling.RandomInUnitHemisphere_Sync()
        let mutable nb = Vector4.Zero
        let mutable nt = Vector4.Zero
        Henzai.Core.Numerics.Geometry.CreateCoordinateSystemAroundNormal(&normal, &nt, &nb)
        let changeOfBaseMatrix = Henzai.Core.Numerics.Geometry.ChangeOfBase(&nt, &normal, &nb)
        let rand_norm_transformed = Vector4.Transform(rand_norm, changeOfBaseMatrix)
        let normalSample = rand_norm_transformed
        let modifiedNormal = Vector4.Normalize((1.0f - this.Fuzz)*normal + this.Fuzz*normalSample)

        let outDir = Vector4.Normalize(this.Reflect incommingRay modifiedNormal)
        let outRay =  Ray(positionOnSurface, outDir)    
        (true,outRay,1.0f)

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
    member this.CalcFresnel (incommingRay : Ray) (t : LineParameter) (depthLevel : int) = 
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

    override this.Scatter (incommingRay : Ray) (t : LineParameter) (depthLevel : int) =
        let (reflectProb, positionOnSurface, reflectDir, refractionDir) = this.CalcFresnel incommingRay t depthLevel
        let randomFloat = RandomSampling.RandomFloat_Sync()
        if randomFloat <= reflectProb 
        then 
            let reflectRay = Ray(positionOnSurface, reflectDir)
            (true, reflectRay, 1.0f)
        else // refraction has to have been successful
            let refractRay = Ray(positionOnSurface, refractionDir)
            (true, refractRay, 1.0f)

    override this.GenerateSamples (incommingRay : Ray) (t : LineParameter) (depthLevel : int) samplesArray =
        let (reflectProb, positionOnSurface, reflectDir, refractionDir) = this.CalcFresnel incommingRay t depthLevel
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


let findClosestIntersection (ray : Ray) (surfaces : Surface[]) =
    let mutable (bMin,tMin, vMin : Surface) = (false, Single.MaxValue, upcast (NoSurface(0UL, NotHitable(), RaytraceMaterial(Vector4.Zero))))
    for surface in surfaces do
        let struct(b,t) = surface.Geometry.AsHitable.Intersect ray
        if surface.Geometry.AsHitable.IntersectionAcceptable b t 1.0f (RaytraceGeometryUtils.PointForRay ray t) &&  t < tMin then
            bMin <- b
            tMin <- t
            vMin <- surface

    struct(bMin, tMin, vMin)      

//https://learnopengl.com/Lighting/Light-casters
//TODO refactor constants
let attenuate distance = 1.0f/(1.0f + 0.5f*distance + 0.02f*(distance*distance))
let AllSurfacesWithoutId (surfaces : Surface list) (id : ID) =
    List.filter (fun (surface : Surface) -> surface.ID <> id) surfaces

let SurfaceWithId (surfaces : Surface list) (id : ID) =
    List.head (List.filter (fun (surface : Surface) -> surface.ID = id) surfaces)

let SurfacesToGeometry (surfaces : Surface list) =
    List.map (fun (x : Surface) -> x.Geometry) surfaces