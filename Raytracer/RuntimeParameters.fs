module Raytracer.RuntimeParameters

open System
open HenzaiFunc.Core.Types

let lambertianSampleCount = 4
let noSampleCount = 0
let dialectricSampleCount = 2
let metalSampleCount = 1

let width = 800
let height = 640
let samplesPerPixel = 8
let batchSize = 1
let renderSquareSide = 2
let renderSquareSize = renderSquareSide*renderSquareSide
let maxTraceDepth = 4us

assert (width%renderSquareSide = 0)
assert (height%renderSquareSide = 0)
assert (renderSquareSide%4 = 0)

let outputDir = "Output/"
let sceneImageName = "scene.jpg"
let depthImageName = "depth.jpg"

let sceneImagePath = outputDir+sceneImageName
let depthImagePath = outputDir+depthImageName

//let monitor = Object()

let mutable id : ID = 1UL

let assignIDAndIncrement idIn : ID =
    let toBeAssigned = idIn
    id <- id + 1UL
    toBeAssigned

