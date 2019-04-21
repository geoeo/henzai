module Raytracer.Surface.SurfaceTypes

type SurfaceTypes = 
    | NoSurface = 0us
    | Lambertian = 1us
    | Metal = 2us
    | Dielectric = 3us
    | NormalVis = 4us
    | IntersectVis = 5us