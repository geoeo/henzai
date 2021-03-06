module Raytracer.Scene.Geometry

open System.Numerics
open SixLabors.ImageSharp.PixelFormats
open Raytracer.Surface.Surface
open Raytracer.Surface.Lambertian
open Raytracer.Surface.Metal
open Raytracer.Surface.Dielectric
open Raytracer.Surface.DebugSurfaces
open Henzai.Core.Materials
open HenzaiFunc.Core.RaytraceGeometry
open Raytracer.RuntimeParameters

let lightsNonAA : Surface list 
    =  [
     NoSurface(assignIDAndIncrement id, Plane(new System.Numerics.Plane((Henzai.Core.Numerics.Vector.CreateUnitVector3(-1.0f, -1.0f, 1.0f)),20.0f),((Henzai.Core.Numerics.Vector.CreateUnitVector3(20.0f, 20.0f, -20.0f))),20.0f,13.0f), RaytraceMaterial(Rgba32.White, 0.0f, Rgba32.White, 1.0f));
    ]

let lightsNonAA_2 : Surface list 
    =  [
     NoSurface(assignIDAndIncrement id, Plane(new System.Numerics.Plane((Henzai.Core.Numerics.Vector.CreateUnitVector3(0.0f, -1.0f, -1.0f)),20.0f),((Henzai.Core.Numerics.Vector.CreateUnitVector3(0.0f, 20.0f, -20.0f))), 30.0f, 20.0f), RaytraceMaterial(Rgba32.White, 0.0f, Rgba32.White, 1.0f));
    ]


let lightsAA : Surface list 
    =  [
        NoSurface(assignIDAndIncrement id, Plane(new System.Numerics.Plane((Henzai.Core.Numerics.Vector.CreateUnitVector3(1.0f, 0.0f, 0.0f)),15.0f),((Vector3(-15.0f,5.0f,-5.0f))), 30.0f, 10.0f), RaytraceMaterial(Rgba32.White,0.0f, Rgba32.White,1.0f));
        //NoSurface(assignIDAndIncrement id, Plane(new System.Numerics.Plane((Henzai.Core.Numerics.Vector.CreateUnitVector3(0.0f, 0.0f, 1.0f)),20.0f),Some ((Henzai.Core.Numerics.Vector.CreateUnitVector3(0.0f, 0.0f, -20.0f))),Some 40.0f,Some 13.0f), RaytraceMaterial(Rgba32.White, 0.0f, Rgba32.White, 1.0f));
        // NoSurface(assignIDAndIncrement id,Plane(new System.Numerics.Plane((Henzai.Core.Numerics.Vector.CreateUnitVector3(0.0f, 0.0f, 1.0f)),21.0f),Some ((Vector3(-12.0f,0.0f,21.0f))),Some 5.0f,Some 8.0f), RaytraceMaterial(Rgba32.White,0.0f, Rgba32.White,1.0f));
        // NoSurface(assignIDAndIncrement id,Plane(new System.Numerics.Plane((Henzai.Core.Numerics.Vector.CreateUnitVector3(0.0f, 0.0f, 1.0f)),-21.0f),Some ((Vector3(-12.0f,0.0f,-21.0f))),Some 5.0f,Some 16.0f), RaytraceMaterial(Rgba32.White,0.0f, Rgba32.White,1.0f));
    ]

let lights_simple : Surface list 
    =  [
        //Emitting(assignIDAndIncrement id, Sphere(Vector3(3.0f, 8.0f, -5.0f),3.0f), RaytraceMaterial(Vector3(1.0f,1.0f,1.0f)));
        NoSurface(assignIDAndIncrement id, Sphere(Vector3(-5.0f,8.0f,0.0f),5.0f), RaytraceMaterial(Rgba32.White,0.0f, Rgba32.White,1.0f));
        //NoSurface(assignIDAndIncrement id, Sphere(Vector3(0.0f,-70.0f,0.0f),50.0f), RaytraceMaterial(Rgba32.White,0.0f, Rgba32.White,1.0f));
        //NoSurface(assignIDAndIncrement id, Plane(new System.Numerics.Plane((SurfaceNormal -1.0f -1.0f 1.0f),20.0f),Some ((SurfaceNormal -1.0f -1.0f 1.0f)*(-20.0f)),Some 40.0f,Some 13.0f), RaytraceMaterial(Rgba32.White, 0.0f, Rgba32.White, 1.0f));
        //NoSurface(assignIDAndIncrement id, Plane(new System.Numerics.Plane((SurfaceNormal 1.0f 0.0f 0.0f),15.0f),Some ((Vector3(-15.0f,5.0f,-10.0f))),Some 20.0f,Some 10.0f), RaytraceMaterial(Rgba32.White,0.0f, Rgba32.White,1.0f));
    ]

let light_sphere : Surface list = [
        NoSurface(assignIDAndIncrement id, Sphere(Vector3(-15.0f,-6.0f,-4.0f),3.0f), RaytraceMaterial(Rgba32.White,0.0f, Rgba32.White,1.0f));  
        //NoSurface(assignIDAndIncrement id, Sphere(Vector3(0.0f,12.0f,20.0f),5.0f), RaytraceMaterial(Rgba32.White,0.0f, Rgba32.White,1.0f));  
]

let spheres : Surface list
    = [
        Dielectric(assignIDAndIncrement id,Sphere(Vector3(4.0f,-2.0f,-12.0f),2.0f), RaytraceMaterial(Rgba32.White),1.3f);
        Metal(assignIDAndIncrement id,Sphere(Vector3(3.0f,-1.0f,-19.0f),3.5f), RaytraceMaterial(Rgba32.RoyalBlue),0.7f);
        // Lambertian(assignIDAndIncrement id,Sphere(Vector3(2.0f,-2.0f,-14.0f),2.0f), RaytraceMaterial(Rgba32.Green));
        Metal(assignIDAndIncrement id,Sphere(Vector3(-1.5f,-4.5f,-7.0f),1.5f), RaytraceMaterial(Rgba32.White),0.0f);
        Lambertian(assignIDAndIncrement id,Sphere(Vector3(2.0f,-4.5f,-7.0f),1.5f), RaytraceMaterial(Rgba32.Red));
        Lambertian(assignIDAndIncrement id,Sphere(Vector3(6.0f,-4.5f,-7.0f),1.5f), RaytraceMaterial(Rgba32.DarkGreen));
        Dielectric(assignIDAndIncrement id,Sphere(Vector3(2.5f,-3.0f,-3.0f),1.5f), RaytraceMaterial(Rgba32.White),1.5f);
        // Lambertian(assignIDAndIncrement id,Sphere(Vector3(-1.5f,2.5f,-9.0f),0.8f), RaytraceMaterial(Rgba32.DarkGreen));
        // Lambertian(assignIDAndIncrement id,Sphere(Vector3(-1.5f,0.0f,-14.0f),2.0f),RaytraceMaterial(Vector3(0.0f,0.0f,1.0f)))
        // Metal(assignIDAndIncrement id,Sphere(Vector3(-6.0f,-0.5f,-6.0f),1.0f), RaytraceMaterial(Rgba32.White),0.0f);
        // Metal(assignIDAndIncrement id,Sphere(Vector3(-5.0f,0.0f,-21.0f),5.0f),RaytraceMaterial(Rgba32.RoyalBlue),0.3f)
        Dielectric(assignIDAndIncrement id,Sphere(Vector3(-5.1f,2.0f,-11.0f),5.0f),RaytraceMaterial(Rgba32.White),1.5f)
      ]
let spheres_submission : Surface list
    = [
        //Dielectric(assignIDAndIncrement id,Sphere(Vector3(4.0f,-2.0f,-12.0f),2.0f), RaytraceMaterial(Rgba32.White),1.3f);
        Lambertian(assignIDAndIncrement id,Sphere(Vector3(4.0f,-1.0f,-15.0f),3.5f), RaytraceMaterial(Rgba32.RoyalBlue));
        // Lambertian(assignIDAndIncrement id,Sphere(Vector3(2.0f,-2.0f,-14.0f),2.0f), RaytraceMaterial(Rgba32.Green));
        Metal(assignIDAndIncrement id,Sphere(Vector3(-1.5f,-4.5f,-7.0f),1.5f), RaytraceMaterial(Rgba32.White),0.0f);
        Lambertian(assignIDAndIncrement id,Sphere(Vector3(2.0f,-4.5f,-7.0f),1.5f), RaytraceMaterial(Rgba32.Red));
        Lambertian(assignIDAndIncrement id,Sphere(Vector3(6.0f,-4.5f,-7.0f),1.5f), RaytraceMaterial(Rgba32.DarkGreen));
        Dielectric(assignIDAndIncrement id,Sphere(Vector3(2.5f,-3.0f,-3.0f),1.5f), RaytraceMaterial(Rgba32.White),1.5f);
        // Lambertian(assignIDAndIncrement id,Sphere(Vector3(-1.5f,2.5f,-9.0f),0.8f), RaytraceMaterial(Rgba32.DarkGreen));
        // Lambertian(assignIDAndIncrement id,Sphere(Vector3(-1.5f,0.0f,-14.0f),2.0f),RaytraceMaterial(Vector3(0.0f,0.0f,1.0f)))
        // Metal(assignIDAndIncrement id,Sphere(Vector3(-6.0f,-0.5f,-6.0f),1.0f), RaytraceMaterial(Rgba32.White),0.0f);
        // Metal(assignIDAndIncrement id,Sphere(Vector3(-5.0f,0.0f,-21.0f),5.0f),RaytraceMaterial(Rgba32.RoyalBlue),0.3f)
        Dielectric(assignIDAndIncrement id,Sphere(Vector3(-5.1f,2.0f,-11.0f),5.0f),RaytraceMaterial(Rgba32.White),1.5f)
      ]

let spheres_simple : Surface list
    = [
        Lambertian(assignIDAndIncrement id,Sphere(Vector3(4.0f,-1.0f,-15.0f),3.5f), RaytraceMaterial(Rgba32.RoyalBlue));
        //Lambertian(assignIDAndIncrement id,Sphere(Vector3(2.0f,-4.5f,-7.0f),1.5f), RaytraceMaterial(Rgba32.Red));
        //Lambertian(assignIDAndIncrement id,Sphere(Vector3(6.0f,-4.5f,-7.0f),1.5f), RaytraceMaterial(Rgba32.DarkGreen));
      ]

let spheres_caustics : Surface list
    = [
        Dielectric(assignIDAndIncrement id,Sphere(Vector3(-5.1f,-2.0f,-11.0f),3.0f),RaytraceMaterial(Rgba32.White),1.5f)
      ]

let spheres_scene_2 : Surface list
    = [
        //NoSurface(assignIDAndIncrement id,Sphere(Vector3(4.0f,-1.0f,-22.5f),3.5f), RaytraceMaterial(Rgba32.RoyalBlue, 0.0f, Rgba32.RoyalBlue, 1.0f));
        Lambertian(assignIDAndIncrement id,Sphere(Vector3(4.0f,-1.0f,-15.0f),3.5f), RaytraceMaterial(Rgba32.RoyalBlue));
        Lambertian(assignIDAndIncrement id,Sphere(Vector3(2.0f,-4.5f,-7.0f),1.5f), RaytraceMaterial(Rgba32.Red));
        Lambertian(assignIDAndIncrement id,Sphere(Vector3(6.0f,-4.5f,-7.0f),1.5f), RaytraceMaterial(Rgba32.DarkGreen));
        Metal(assignIDAndIncrement id,Sphere(Vector3(-1.5f,-4.5f,-7.0f),1.5f), RaytraceMaterial(Rgba32.White),0.0f);
        Dielectric(assignIDAndIncrement id,Sphere(Vector3(-5.1f,-2.0f,-11.0f),3.0f),RaytraceMaterial(Rgba32.White),1.5f);
        Dielectric(assignIDAndIncrement id,Sphere(Vector3(2.5f,-3.0f,-3.0f),1.5f), RaytraceMaterial(Rgba32.White),1.5f);
      ]

     

let spheres_scene_3 : Surface list
     = [
         Lambertian(assignIDAndIncrement id,Sphere(Vector3(6.5f,-1.0f,-15.0f),3.5f), RaytraceMaterial(Rgba32.RoyalBlue));
         Metal(assignIDAndIncrement id,Sphere(Vector3(-3.5f,-4.5f,-7.0f),1.5f), RaytraceMaterial(Rgba32.White),0.0f);
       ]

let spheres_scene_4 : Surface list
     = [
         Lambertian(assignIDAndIncrement id,Sphere(Vector3(-6.0f,-1.5f,-3.5f),2.5f), RaytraceMaterial(Rgba32.Red));
         NormalVis(assignIDAndIncrement id,Sphere(Vector3(6.0f,-1.5f,-3.5f),2.5f), RaytraceMaterial(Rgba32.Red));
       ]

let lights_spheres_scene_4 : Surface list
     = [
         NoSurface(assignIDAndIncrement id,Sphere(Vector3(-6.0f,0.5f,-3.5f),2.5f), RaytraceMaterial(Rgba32.White,0.0f, Rgba32.White,1.0f));
         NoSurface(assignIDAndIncrement id,Sphere(Vector3(6.0f,0.5f,-3.5f),2.5f), RaytraceMaterial(Rgba32.White,0.0f, Rgba32.White,1.0f));
       ]

let light_box : Surface list
     = [
     NoSurface(assignIDAndIncrement id, Plane(System.Numerics.Plane((Henzai.Core.Numerics.Vector.CreateUnitVector3(-1.0f, 0.0f, 0.0f)),20.0f),((Henzai.Core.Numerics.Vector.CreateUnitVector3(20.0f, 0.0f, 0.0f))),40.0f,40.0f), RaytraceMaterial(Rgba32.White, 0.0f, Rgba32.White, 1.0f));
     NoSurface(assignIDAndIncrement id, Plane(System.Numerics.Plane((Henzai.Core.Numerics.Vector.CreateUnitVector3(1.0f, 0.0f, 0.0f)),20.0f),((Henzai.Core.Numerics.Vector.CreateUnitVector3(-20.0f, 0.0f, 0.0f))),40.0f,40.0f), RaytraceMaterial(Rgba32.White, 0.0f, Rgba32.White, 1.0f));
     NoSurface(assignIDAndIncrement id, Plane(System.Numerics.Plane((Henzai.Core.Numerics.Vector.CreateUnitVector3(0.0f, -1.0f, 0.0f)),20.0f),((Henzai.Core.Numerics.Vector.CreateUnitVector3(0.0f, 20.0f, 0.0f))),40.0f,40.0f), RaytraceMaterial(Rgba32.White, 0.0f, Rgba32.White, 1.0f));
     NoSurface(assignIDAndIncrement id, Plane(System.Numerics.Plane((Henzai.Core.Numerics.Vector.CreateUnitVector3(0.0f, 1.0f, 0.0f)),20.0f),((Henzai.Core.Numerics.Vector.CreateUnitVector3(0.0f, -20.0f, 0.0f))),40.0f,40.0f), RaytraceMaterial(Rgba32.White, 0.0f, Rgba32.White, 1.0f));
     NoSurface(assignIDAndIncrement id, Plane(System.Numerics.Plane((Henzai.Core.Numerics.Vector.CreateUnitVector3(0.0f, 0.0f, -1.0f)),20.0f),((Henzai.Core.Numerics.Vector.CreateUnitVector3(0.0f, 0.0f, 20.0f))),40.0f,40.0f), RaytraceMaterial(Rgba32.White, 0.0f, Rgba32.White, 1.0f));
     NoSurface(assignIDAndIncrement id, Plane(System.Numerics.Plane((Henzai.Core.Numerics.Vector.CreateUnitVector3(0.0f, 0.0f, 1.0f)),20.0f),((Henzai.Core.Numerics.Vector.CreateUnitVector3(0.0f, 0.0f, -20.0f))),40.0f,40.0f), RaytraceMaterial(Rgba32.White, 0.0f, Rgba32.White, 1.0f));
       ]

let light_plane_z : Surface list 
        = [
        NoSurface(assignIDAndIncrement id, Plane(System.Numerics.Plane((Henzai.Core.Numerics.Vector.CreateUnitVector3(0.0f, 0.0f, -1.0f)),20.0f),((Henzai.Core.Numerics.Vector.CreateUnitVector3(0.0f, 0.0f, 20.0f))),40.0f,40.0f), RaytraceMaterial(Rgba32.White, 0.0f, Rgba32.White, 1.0f));
        //not working//NoSurface(assignIDAndIncrement id, Plane(System.Numerics.Plane((Henzai.Core.Numerics.Vector.CreateUnitVector3(0.0f, 0.0f, -1.0f)),20.0f),((Henzai.Core.Numerics.Vector.CreateUnitVector3(-100.0f, 0.0f, 20.0f))),20.0f,20.0f), RaytraceMaterial(Rgba32.White, 0.0f, Rgba32.White, 1.0f));
        ]

let spheres_glass : Surface list = [
        Dielectric(assignIDAndIncrement id,Sphere(Vector3(-5.1f,-2.0f,-11.0f),3.0f),RaytraceMaterial(Rgba32.White),1.5f);
]

let planes : Surface list = [
    Lambertian(assignIDAndIncrement id,Plane(System.Numerics.Plane.CreateFromVertices(Vector3(-1.0f,-6.0f,0.0f),Vector3(1.0f,-6.0f,0.0f),Vector3(0.0f,-6.0f,-1.0f)),None,None,None),RaytraceMaterial(Vector4(1.0f,1.0f,1.0f,1.0f)));
    Lambertian(assignIDAndIncrement id,Plane(System.Numerics.Plane((Henzai.Core.Numerics.Vector.CreateUnitVector3(0.0f, 0.0f, -1.0f)),17.0f),((Vector3(0.0f,0.0f,17.0f))), 30.0f, 10.0f), RaytraceMaterial(Rgba32.White));
    Lambertian(assignIDAndIncrement id,Plane(System.Numerics.Plane((Henzai.Core.Numerics.Vector.CreateUnitVector3(0.0f, 0.0f, 1.0f)),30.0f),((Vector3(-5.0f,10.0f,-30.0f))), 25.0f, 20.0f), RaytraceMaterial(Rgba32.IndianRed));
    Metal(assignIDAndIncrement id, Plane(System.Numerics.Plane((Henzai.Core.Numerics.Vector.CreateUnitVector3(-1.0f, 0.0f, 1.0f)),15.0f),((Vector3(15.0f,3.0f,-15.0f))), 10.0f, 6.0f), RaytraceMaterial(Rgba32.AntiqueWhite),0.01f)
    ]

let planes_submission : Surface list = [
    Lambertian(assignIDAndIncrement id,Plane(System.Numerics.Plane.CreateFromVertices(Vector3(-1.0f,-6.0f,0.0f),Vector3(1.0f,-6.0f,0.0f),Vector3(0.0f,-6.0f,-1.0f)),None,None,None),RaytraceMaterial(Vector4(1.0f,1.0f,1.0f,1.0f)));
    Lambertian(assignIDAndIncrement id,Plane(new System.Numerics.Plane((Henzai.Core.Numerics.Vector.CreateUnitVector3(0.0f, 0.0f, -1.0f)),17.0f), ((Vector3(0.0f,0.0f,17.0f))), 30.0f, 10.0f), RaytraceMaterial(Rgba32.White));
    Lambertian(assignIDAndIncrement id,Plane(new System.Numerics.Plane((Henzai.Core.Numerics.Vector.CreateUnitVector3(0.0f, 0.0f, 1.0f)),30.0f), ((Vector3(-5.0f,10.0f,-30.0f))), 25.0f, 20.0f), RaytraceMaterial(Rgba32.IndianRed));
    Metal(assignIDAndIncrement id, Plane(new System.Numerics.Plane((Henzai.Core.Numerics.Vector.CreateUnitVector3(-1.0f, 0.0f, 1.0f)),15.0f), ((Vector3(15.0f,3.0f,-15.0f))), 10.0f, 6.0f), RaytraceMaterial(Rgba32.AntiqueWhite),0.0f)
    ]
   
let planes_simple : Surface list = [
    Lambertian(assignIDAndIncrement id,Plane(System.Numerics.Plane.CreateFromVertices(Vector3(-1.0f,-6.0f,0.0f),Vector3(1.0f,-6.0f,0.0f),Vector3(0.0f,-6.0f,-1.0f)),None,None,None),RaytraceMaterial(Vector4(1.0f,1.0f,1.0f,1.0f)));
    //Lambertian(assignIDAndIncrement id,Plane(new System.Numerics.Plane((SurfaceNormal 0.0f 0.0f -1.0f),17.0f),Some ((Vector3(0.0f,0.0f,17.0f))),Some 30.0f,Some 10.0f), RaytraceMaterial(Rgba32.White));
    ]

let planes_scene_2_AA : Surface list = [
    Lambertian(assignIDAndIncrement id,Plane(new System.Numerics.Plane((Henzai.Core.Numerics.Vector.CreateUnitVector3(0.0f, 0.0f, -1.0f)),17.0f), ((Vector3(5.0f,0.0f,17.0f))), 10.0f, 10.0f), RaytraceMaterial(Rgba32.Purple));
    Lambertian(assignIDAndIncrement id,Plane(new System.Numerics.Plane((Henzai.Core.Numerics.Vector.CreateUnitVector3(0.0f, 0.0f, 1.0f)),30.0f), ((Vector3(0.0f,10.0f,-30.0f))), 25.0f, 20.0f), RaytraceMaterial(Rgba32.IndianRed))
    ]

let plane_mirror_NonAA : Surface list = [
    Metal(assignIDAndIncrement id, Plane(new System.Numerics.Plane((Henzai.Core.Numerics.Vector.CreateUnitVector3(-1.0f, 0.0f, 1.0f)),15.0f), ((Vector3(15.0f,3.0f,-15.0f))), 10.0f, 6.0f), RaytraceMaterial(Rgba32.AntiqueWhite),0.0f);
    ]

let plane_floor_Unbounded : Surface list = [
    Lambertian(assignIDAndIncrement id,Plane(System.Numerics.Plane.CreateFromVertices(Vector3(-1.0f,-6.0f,0.0f),Vector3(1.0f,-6.0f,0.0f),Vector3(0.0f,-6.0f,-1.0f)),None,None,None),RaytraceMaterial(Vector4(1.0f,1.0f,1.0f,1.0f)));    
    ]

let plane_floor_bounded : Surface list = [
    Lambertian(assignIDAndIncrement id,Plane(System.Numerics.Plane.CreateFromVertices(Vector3(-1.0f,-6.0f,0.0f), Vector3(1.0f,-6.0f,0.0f), Vector3(0.0f,-6.0f,-1.0f)),( (Vector3(0.0f,0.0f,0.0f))), 100.0f,  100.0f),RaytraceMaterial(Vector4(1.0f,1.0f,1.0f,1.0f)));    
    ]

let triangle_scene : Surface list = [
            //Lambertian(assignIDAndIncrement id,Triangle(Vector3(-13.0f,-6.0f,-16.0f),Vector3(-17.0f,-6.0f,-16.0f),Vector3(-15.0f,-3.0f,-16.0f)),RaytraceMaterial(Rgba32.White));
            Dielectric(assignIDAndIncrement id,Triangle(Vector3(-13.0f,-6.0f,-16.0f),Vector3(-11.0f,-6.0f,-16.0f),Vector3(-12.0f,-3.0f,-16.0f)),RaytraceMaterial(Rgba32.IndianRed), 1.5f);
            //Lambertian(assignIDAndIncrement id,Triangle(Vector3(-13.0f,-6.0f,-16.0f),Vector3(-11.0f,-6.0f,-16.0f),Vector3(-12.0f,-3.0f,-16.0f)),RaytraceMaterial(Rgba32.IndianRed));
            Lambertian(assignIDAndIncrement id,Triangle(Vector3(-14.0f,-6.0f,-18.0f),Vector3(-10.0f,-6.0f,-18.0f),Vector3(-12.0f,0.0f,-18.0f)),RaytraceMaterial(Rgba32.RoyalBlue));
]

let triangle_scene_y : Surface list = [
            //Lambertian(assignIDAndIncrement id,Triangle(Vector3(-13.0f,-6.0f,-16.0f),Vector3(-17.0f,-6.0f,-16.0f),Vector3(-15.0f,-3.0f,-16.0f)),RaytraceMaterial(Rgba32.White));
            Lambertian(assignIDAndIncrement id,Triangle(Vector3(-14.0f,-4.0f,-10.0f),Vector3(-10.0f,-4.0f,-10.0f),Vector3(-14.0f,-4.0f,-14.0f)),RaytraceMaterial(Rgba32.RoyalBlue));
]

