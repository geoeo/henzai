module Raytracer.Camera

open System
open System.Numerics
open Henzai.Core.Numerics

// https://www.scratchapixel.com/lessons/3d-basic-rendering/ray-tracing-generating-camera-rays/generating-camera-rays

let pertrube px py width height = 
    let randVec = RandomSampling.RandomInUnitSphere_Sync()
    let xOff = randVec.X/((float32)(width))
    let yOff = randVec.Y/((float32)(height))
    //TODO: investigate artefacts: Maybe a convergence issue
    // let xOff = randVec.X/10.0f
    // let yOff = randVec.Y/10.0f
    // let xOff = randVec.X/2.0f
    // let yOff = randVec.Y/2.0f
    (px + xOff, py+yOff)

// ndc is [0,1] from top left
let PixelToNDC px py width height =
    let px_pert, py_pert = pertrube (px + 0.5f) (py + 0.5f) width height
    (px_pert/width , py_pert/height)

// screen space is [-1,1] from top left
let NDCToScreen (x_ndc , y_ndc) = (2.0f*x_ndc-1.0f, 1.0f-2.0f*y_ndc)

let AspectRatio (width:float32) (height:float32) = width/height

// pixel coordiantes / sample points (X,Y) in camera space (3D)
// focal length is implicit in the tan(alpha / 2) calc
let ScreenToCamera (x_screen, y_screen) aspectRatio fov =
    (x_screen*aspectRatio*MathF.Tan(fov/2.0f), y_screen*MathF.Tan(fov/2.0f))

let PixelToCamera x y width height fov =
    ScreenToCamera (NDCToScreen (PixelToNDC x y width height)) (AspectRatio width height) fov

// ray direction wrt to camera
let RayDirection (cameraPixel_x, cameraPixel_y) =
    Vector4.Normalize(Vector4(cameraPixel_x, cameraPixel_y, -1.0f, 0.0f))

let WorldToCamera  position target up = Matrix4x4.CreateLookAt(position, target, up);

let CameraToWorld worldToCamera = 
    let success , cameraToWorld = Matrix4x4.Invert(worldToCamera)
    if success then cameraToWorld else failwith "Matrix4x4 Invert Failed"

/// Seems to slightly less precise than Matrix4x4.Invert
/// Since this is an SE3 matrix we can simply transpose R and -R_t*t
/// http://ethaneade.com/lie.pdf
let cameraToWorldFast (worldToCamera : Matrix4x4) (roundToDigits: int) = 
    let rotation 
        = Matrix4x4(-worldToCamera.M11,-worldToCamera.M21,-worldToCamera.M31,0.0f,
                    -worldToCamera.M12,-worldToCamera.M22,-worldToCamera.M32,0.0f,
                    -worldToCamera.M13,-worldToCamera.M23,-worldToCamera.M33,0.0f,
                    0.0f,0.0f,0.0f,0.0f)
    let translation = Vector4(worldToCamera.M41, worldToCamera.M42, worldToCamera.M43, 0.0f)
    let trans_inv = Vector4.Transform(translation, rotation)
    let round x = MathF.Round(x, roundToDigits)
    Matrix4x4(worldToCamera.M11,worldToCamera.M21,worldToCamera.M31,0.0f,
              worldToCamera.M12,worldToCamera.M22,worldToCamera.M32,0.0f,
              worldToCamera.M13,worldToCamera.M23,worldToCamera.M33,0.0f,
              round trans_inv.X,round trans_inv.Y,round trans_inv.Z,1.0f)





