module Raytracer.RuntimeParameters

open HenzaiFunc.Core.Types

let lambertianSampleCount = 4
let noSampleCount = 0
let dialectricSampleCount = 2
let metalSampleCount = 1

let width = 800
let height = 640
let samplesPerPixel = 1
let batchSize = 1
let renderSquareSide = 8
let maxTraceDepth = 1us

assert (width%renderSquareSide = 0)
assert (height%renderSquareSide = 0)

let outputDir = "Output/"
let sceneImageName = "scene.jpg"
let depthImageName = "depth.jpg"

let sceneImagePath = outputDir+sceneImageName
let depthImagePath = outputDir+depthImageName

let mutable id : ID = 1UL

let assignIDAndIncrement idIn : ID =
    let toBeAssigned = idIn
    id <- id + 1UL
    toBeAssigned