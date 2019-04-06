module Raytracer.RuntimeParameters

let lambertianSampleCount = 4
let noSampleCount = 0
let dialectricSampleCount = 2
let metalSampleCount = 1

let width = 800
let height = 640
let samplesPerPixel = 1
let batchSize = 1
let maxTraceDepth = 5us

let outputDir = "Output/"
let sceneImageName = "scene.jpg"
let depthImageName = "depth.jpg"

let sceneImagePath = outputDir+sceneImageName
let depthImagePath = outputDir+depthImageName