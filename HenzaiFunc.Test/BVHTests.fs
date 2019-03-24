module BVHTests

open System.Numerics
open Xunit
open HenzaiFunc.Core.RaytraceGeometry
open HenzaiFunc.Core.Acceleration
open HenzaiFunc.Core.Types
open HenzaiFunc.Core


let simpleSphereGeomertryArray : RaytracingGeometry []  = 
    [|
        Sphere(Vector3(4.0f,-1.0f,-15.0f),3.5f);
    |]

let standardSphereGeomertryArray : RaytracingGeometry []  = 
    [|
        Sphere(Vector3(4.0f,-1.0f,-15.0f),3.5f);
        Sphere(Vector3(10.0f,-4.5f,-7.0f),1.5f);
        Sphere(Vector3(2.0f,-4.5f,-7.0f),1.5f)
    |]

let largeSphereGeomertryArray : RaytracingGeometry []  = 
    [|
       Sphere(Vector3(4.0f,-2.0f,-16.0f),3.5f);
       Sphere(Vector3(4.0f,5.0f,-16.5f),3.5f);
       Sphere(Vector3(4.0f,-1.0f,-15.0f),3.5f);
       Sphere(Vector3(5.0f,-4.5f,-7.0f),1.5f);
       Sphere(Vector3(2.0f,-4.5f,-7.0f),1.5f)
    |]

let overlappingSphereGeomertryArray : RaytracingGeometry []  = 
    [|
     Sphere(Vector3(4.0f,-1.0f,-15.0f),100.0f);
     Sphere(Vector3(2.0f,-4.5f,-7.0f),1.5f);
     Sphere(Vector3(2.5f,-4.5f,-7.0f),1.5f)
    |]

let identicalSphereGeomertryArray : RaytracingGeometry []  = 
    [|
     Sphere(Vector3(2.0f,-4.5f,-7.0f),1.5f);
     Sphere(Vector3(2.0f,-4.5f,-7.0f),1.5f);
     Sphere(Vector3(2.0f,-4.5f,-7.0f),1.5f)
    |]

let smallLargeSphereGeometryArrray : RaytracingGeometry [] = 
    [|
        Sphere(Vector3(1.5f,-1.5f,-3.5f),5.5f);
        Sphere(Vector3(0.0f,-70.0f,0.0f),50.0f)
    |]

let smallLargeInFrontSphereGeometryArray : RaytracingGeometry [] =
    [|
        Sphere(Vector3(4.0f,-1.0f,-15.0f),3.5f);
        Sphere(Vector3(1.5f,-1.5f,-3.5f),5.5f);
        Sphere(Vector3(0.0f,-70.0f,0.0f),50.0f)
    |]


let smallLargeInFront2SphereGeometryArray : RaytracingGeometry [] =
    [|
        Sphere(Vector3(4.0f,-1.0f,-15.0f),3.5f);
        Sphere(Vector3(-4.5f,-1.5f,-3.5f),5.5f);
        Sphere(Vector3(0.0f,-70.0f,0.0f),50.0f)
    |]
    


let aLotOfSpheresGeometryArray : RaytracingGeometry [] =
    [|
        Sphere(Vector3(4.0f,-1.0f,-15.0f),3.5f)
        Sphere(Vector3(4.0f,-1.0f,-22.5f),3.5f);
        Sphere(Vector3(2.0f,-4.5f,-7.0f),1.5f);
        Sphere(Vector3(6.0f,-4.5f,-7.0f),1.5f);
        //Sphere(Vector3(-1.5f,-4.5f,-7.0f),1.5f);
        //Sphere(Vector3(-5.1f,-2.0f,-11.0f),3.0f);
        //Sphere(Vector3(2.5f,-3.0f,-3.0f),1.5f);
    |]

   



[<Fact>]
let buildBVHSimpleSphereTest () =
    let bvhTreeBuilder = BVHTreeBuilder<RaytracingGeometry>() 
    let (bvhTree, orderedPrimitiveList, nodeCount) = bvhTreeBuilder.build simpleSphereGeomertryArray SplitMethods.Middle
    let (value, left, right) = BVHTree.decompose bvhTree
    Assert.Equal(1, nodeCount)
    Assert.Equal(1, orderedPrimitiveList.Length)
    Assert.Equal(1, value.nPrimitives)
    
    

[<Fact>]
let buildBVHStandardSphereTest () =
    let bvhTreeBuilder = BVHTreeBuilder<RaytracingGeometry>() 
    let (bvhTree, orderedPrimitiveList, nodeCount) = bvhTreeBuilder.build standardSphereGeomertryArray SplitMethods.Middle
    Assert.Equal(5, nodeCount)
    Assert.Equal(3, orderedPrimitiveList.Length)

[<Fact>]
let buildBVHOverlappingSphereTest () =
    let bvhTreeBuilder = BVHTreeBuilder<RaytracingGeometry>() 
    let (bvhTree, orderedPrimitiveList, nodeCount) = bvhTreeBuilder.build overlappingSphereGeomertryArray SplitMethods.Middle
    Assert.Equal(5, nodeCount)
    Assert.Equal(3, orderedPrimitiveList.Length)

[<Fact>]
let buildBVHIdenticalSphereTest () =
    let bvhTreeBuilder = BVHTreeBuilder<RaytracingGeometry>() 
    let (bvhTree, orderedPrimitiveList, nodeCount) = bvhTreeBuilder.build identicalSphereGeomertryArray SplitMethods.Middle
    Assert.Equal(1, nodeCount)
    Assert.Equal(3, orderedPrimitiveList.Length)

[<Fact>]
let buildBVHLargeSphereTest () =
    let bvhTreeBuilder = BVHTreeBuilder<RaytracingGeometry>() 
    let (bvhTree, orderedPrimitiveList, nodeCount) = bvhTreeBuilder.build largeSphereGeomertryArray SplitMethods.Middle
    Assert.Equal(9, nodeCount)
    Assert.Equal(5, orderedPrimitiveList.Length)

[<Fact>]
let buildBVHSmallLargeSphereTest () =
    let bvhTreeBuilder = BVHTreeBuilder<RaytracingGeometry>() 
    let (bvhTree, orderedPrimitiveList, nodeCount) = bvhTreeBuilder.build smallLargeSphereGeometryArrray SplitMethods.Middle
    let (v, l ,r) = BVHTree.decompose bvhTree
    Assert.Equal(3, nodeCount)
    Assert.Equal(2, orderedPrimitiveList.Length)
    Assert.Equal(SplitAxis.Y, v.splitAxis)

    
[<Fact>]
let buildBVHALotTest () =
    let bvhTreeBuilder = BVHTreeBuilder<RaytracingGeometry>() 
    let (bvhTree, orderedPrimitiveList, nodeCount) = bvhTreeBuilder.build aLotOfSpheresGeometryArray SplitMethods.Middle
    let (v, l ,r) = BVHTree.decompose bvhTree
    Assert.True(true)


[<Fact>]
let flattenBVHSimpleSphereTest () =
    let bvhTreeBuilder = BVHTreeBuilder<RaytracingGeometry>() 
    let bvhRuntime = BVHRuntime<RaytracingGeometry>()
    let (bvhTree, orderedPrimitiveList, nodeCount) = bvhTreeBuilder.build simpleSphereGeomertryArray SplitMethods.Middle
    let bvhArray = bvhRuntime.allocateMemoryForBVHRuntime nodeCount
    let out = bvhRuntime.flattenBVHTree bvhTree bvhArray 0 CoordinateSystem.XYZ
    let bvhRuntimeNode = bvhArray.[0]
    let leafNode = bvhRuntimeNode.leafNode 
    let primitive = simpleSphereGeomertryArray.[leafNode.primitivesOffset]
    let sphere = primitive :?> Sphere

    Assert.Equal(Vector4(4.0f,-1.0f,-15.0f,1.0f), sphere.Center)
    Assert.Equal(1, nodeCount)
    Assert.Equal(1, bvhRuntimeNode.nPrimitives)


[<Fact>]
let flattenBVHIdenticalSphereTest () =
    let bvhTreeBuilder = BVHTreeBuilder<RaytracingGeometry>() 
    let bvhRuntime = BVHRuntime<RaytracingGeometry>()
    let (bvhTree, orderedPrimitiveArray, nodeCount) = bvhTreeBuilder.build identicalSphereGeomertryArray SplitMethods.Middle
    let bvhArray = bvhRuntime.allocateMemoryForBVHRuntime nodeCount
    let out = bvhRuntime.flattenBVHTree bvhTree bvhArray 0 CoordinateSystem.XYZ
    let bvhRuntimeNode_Zero = bvhArray.[0]
    let leafNode_Zero = bvhRuntimeNode_Zero.leafNode
    let primitive_Zero = orderedPrimitiveArray.[leafNode_Zero.primitivesOffset]
    let sphere_Zero = primitive_Zero :?> Sphere

    Assert.Equal(1, nodeCount)
    Assert.Equal(bvhRuntimeNode_Zero.nPrimitives, 3)
    Assert.Equal(Vector4(2.0f,-4.5f,-7.0f,1.0f), sphere_Zero.Center)


        
[<Fact>]
let flattenBVHOverlappingSphereTest () =
    let bvhTreeBuilder = BVHTreeBuilder<RaytracingGeometry>() 
    let bvhRuntime = BVHRuntime<RaytracingGeometry>()
    let (bvhTree, orderedPrimitiveArray, nodeCount) = bvhTreeBuilder.build overlappingSphereGeomertryArray SplitMethods.Middle
    let bvhArray = bvhRuntime.allocateMemoryForBVHRuntime nodeCount
    let out = bvhRuntime.flattenBVHTree bvhTree bvhArray 0 CoordinateSystem.XYZ
    let bvhRuntimeNode_Zero = bvhArray.[0]
    let bvhRuntimeNode_One = bvhArray.[1]
    let bvhRuntimeNode_Two = bvhArray.[2]
    let bvhRuntimeNode_Three = bvhArray.[3]
    let bvhRuntimeNode_Four = bvhArray.[4]
    let leafNode_Zero = bvhRuntimeNode_Two.leafNode
    let leafNode_One = bvhRuntimeNode_Three.leafNode
    let leafNode_Two = bvhRuntimeNode_Four.leafNode
    let primitive_Zero = orderedPrimitiveArray.[leafNode_Zero.primitivesOffset]
    let primitive_One = orderedPrimitiveArray.[leafNode_One.primitivesOffset]
    let primitive_Two = orderedPrimitiveArray.[leafNode_Two.primitivesOffset]
    let sphere_Zero = primitive_Zero :?> Sphere
    let sphere_One = primitive_One :?> Sphere
    let sphere_Two = primitive_Two :?> Sphere

    Assert.Equal(5, nodeCount)

    Assert.Equal(bvhRuntimeNode_Zero.nPrimitives, 0)
    Assert.Equal(bvhRuntimeNode_One.nPrimitives, 1)
    Assert.Equal(bvhRuntimeNode_Two.nPrimitives, 0)
    Assert.Equal(bvhRuntimeNode_Three.nPrimitives, 1)
    Assert.Equal(bvhRuntimeNode_Four.nPrimitives, 1)

    Assert.Equal(Vector4(4.0f,-1.0f,-15.0f,1.0f), sphere_Zero.Center)
    Assert.Equal(Vector4(2.0f,-4.5f,-7.0f,1.0f), sphere_One.Center)  
    Assert.Equal(Vector4(2.5f,-4.5f,-7.0f,1.0f), sphere_Two.Center)
    
    
[<Fact>]
let flattenBVHSphereTest () =
    let bvhTreeBuilder = BVHTreeBuilder<RaytracingGeometry>() 
    let bvhRuntime = BVHRuntime<RaytracingGeometry>()
    let (bvhTree, orderedPrimitiveArray, nodeCount) = bvhTreeBuilder.build standardSphereGeomertryArray SplitMethods.Middle
    let bvhArray = bvhRuntime.allocateMemoryForBVHRuntime nodeCount
    let out = bvhRuntime.flattenBVHTree bvhTree bvhArray 0 CoordinateSystem.XYZ
    let bvhRuntimeNode_Zero = bvhArray.[0]
    let bvhRuntimeNode_One = bvhArray.[1]
    let bvhRuntimeNode_Two = bvhArray.[2]
    let bvhRuntimeNode_Three = bvhArray.[3]
    let bvhRuntimeNode_Four = bvhArray.[4]
    let leafNode_Zero = bvhRuntimeNode_Two.leafNode
    let leafNode_One = bvhRuntimeNode_Three.leafNode
    let leafNode_Two = bvhRuntimeNode_Four.leafNode
    let primitive_Zero = orderedPrimitiveArray.[leafNode_Zero.primitivesOffset]
    let primitive_One = orderedPrimitiveArray.[leafNode_One.primitivesOffset]
    let primitive_Two = orderedPrimitiveArray.[leafNode_Two.primitivesOffset]
    let sphere_Zero = primitive_Zero :?> Sphere
    let sphere_One = primitive_One :?> Sphere
    let sphere_Two = primitive_Two :?> Sphere

    Assert.Equal(5, nodeCount)

    Assert.Equal(bvhRuntimeNode_Zero.nPrimitives, 0)
    Assert.Equal(bvhRuntimeNode_One.nPrimitives, 1)
    Assert.Equal(bvhRuntimeNode_Two.nPrimitives, 0)
    Assert.Equal(bvhRuntimeNode_Three.nPrimitives, 1)
    Assert.Equal(bvhRuntimeNode_Four.nPrimitives, 1)


    Assert.Equal(Vector4(4.0f,-1.0f,-15.0f,1.0f), sphere_Zero.Center)
    Assert.Equal(Vector4(2.0f,-4.5f,-7.0f,1.0f), sphere_One.Center)  
    Assert.Equal(Vector4(10.0f,-4.5f,-7.0f,1.0f), sphere_Two.Center)


[<Fact>]
let flattenBVHLargeSphereTest () =
    let bvhTreeBuilder = BVHTreeBuilder<RaytracingGeometry>() 
    let bvhRuntime= BVHRuntime<RaytracingGeometry>()
    let (bvhTree, orderedPrimitiveArray, nodeCount) = bvhTreeBuilder.build largeSphereGeomertryArray SplitMethods.Middle
    let bvhArray = bvhRuntime.allocateMemoryForBVHRuntime nodeCount
    let out = bvhRuntime.flattenBVHTree bvhTree bvhArray 0 CoordinateSystem.XYZ

    let bvhRuntimeNode_Zero = bvhArray.[0]
    let bvhRuntimeNode_One = bvhArray.[1]
    let bvhRuntimeNode_Two = bvhArray.[2]
    let bvhRuntimeNode_Three = bvhArray.[3]
    let bvhRuntimeNode_Four = bvhArray.[4]
    let bvhRuntimeNode_Five = bvhArray.[5]
    let bvhRuntimeNode_Six = bvhArray.[6]
    let bvhRuntimeNode_Seven = bvhArray.[7]
    let bvhRuntimeNode_Eight = bvhArray.[8]

    let leafNode_Zero = bvhRuntimeNode_Two.leafNode
    let leafNode_One = bvhRuntimeNode_Three.leafNode
    let leafNode_Two = bvhRuntimeNode_Six.leafNode
    let leafNode_Three = bvhRuntimeNode_Seven.leafNode
    let leafNode_Four = bvhRuntimeNode_Eight.leafNode

    let primitive_Zero = orderedPrimitiveArray.[leafNode_Zero.primitivesOffset]
    let primitive_One = orderedPrimitiveArray.[leafNode_One.primitivesOffset]
    let primitive_Two = orderedPrimitiveArray.[leafNode_Two.primitivesOffset]
    let primitive_Three = orderedPrimitiveArray.[leafNode_Three.primitivesOffset]
    let primitive_Four = orderedPrimitiveArray.[leafNode_Four.primitivesOffset]

    let sphere_Zero = primitive_Zero :?> Sphere
    let sphere_One = primitive_One :?> Sphere
    let sphere_Two = primitive_Two :?> Sphere
    let sphere_Three = primitive_Three :?> Sphere
    let sphere_Four = primitive_Four :?> Sphere

    Assert.Equal(9, nodeCount)

    Assert.Equal(bvhRuntimeNode_Zero.nPrimitives, 0)
    Assert.Equal(bvhRuntimeNode_One.nPrimitives, 0)
    Assert.Equal(bvhRuntimeNode_Two.nPrimitives, 0)
    Assert.Equal(bvhRuntimeNode_Three.nPrimitives, 1)
    Assert.Equal(bvhRuntimeNode_Four.nPrimitives, 1)
    Assert.Equal(bvhRuntimeNode_Five.nPrimitives, 1)
    Assert.Equal(bvhRuntimeNode_Six.nPrimitives, 0)
    Assert.Equal(bvhRuntimeNode_Seven.nPrimitives, 1)
    Assert.Equal(bvhRuntimeNode_Eight.nPrimitives, 1)

    Assert.Equal(Vector4(4.0f,-2.0f,-16.0f,1.0f), sphere_Zero.Center)
    Assert.Equal(Vector4(4.0f,-2.0f,-16.0f,1.0f), sphere_One.Center)  
    Assert.Equal(Vector4(4.0f,-2.0f,-16.0f,1.0f), sphere_Two.Center)
    Assert.Equal(Vector4(2.0f,-4.5f,-7.0f,1.0f), sphere_Three.Center)
    Assert.Equal(Vector4(5.0f,-4.5f,-7.0f,1.0f), sphere_Four.Center)


[<Fact>]
let flattenBVHSmallLargeSphereTest () =
    let bvhTreeBuilder = BVHTreeBuilder<RaytracingGeometry>() 
    let bvhRuntime = BVHRuntime<RaytracingGeometry>()
    let (bvhTree, orderedPrimitiveList, nodeCount) = bvhTreeBuilder.build smallLargeSphereGeometryArrray SplitMethods.Middle
    let bvhArray = bvhRuntime.allocateMemoryForBVHRuntime nodeCount
    let out = bvhRuntime.flattenBVHTree bvhTree bvhArray 0 CoordinateSystem.XYZ
    let bvhRuntimeNode = bvhArray.[0]
    let leafNode = bvhRuntimeNode.leafNode 
    let primitive = simpleSphereGeomertryArray.[leafNode.primitivesOffset]
    let sphere = primitive :?> Sphere

    Assert.Equal(SplitAxis.Y, bvhRuntimeNode.interiorNode.splitAxis)


[<Fact>]
let flattenBVHALotSphereTest () =
    let bvhTreeBuilder = BVHTreeBuilder<RaytracingGeometry>() 
    let bvhRuntime = BVHRuntime<RaytracingGeometry>()
    let (bvhTree, orderedPrimitiveList, nodeCount) = bvhTreeBuilder.build aLotOfSpheresGeometryArray SplitMethods.Middle
    let bvhArray = bvhRuntime.allocateMemoryForBVHRuntime nodeCount
    let out = bvhRuntime.flattenBVHTree bvhTree bvhArray 0 CoordinateSystem.XYZ
    let bvhRuntimeNode = bvhArray.[0]
    let leafNode = bvhRuntimeNode.leafNode 
    let primitive = simpleSphereGeomertryArray.[leafNode.primitivesOffset]
    let sphere = primitive :?> Sphere

    Assert.True(true)


[<Fact>]
let intersectBVHSimpleSphereTest () =
    let bvhTreeBuilder = BVHTreeBuilder<RaytracingGeometry>()
    let bvhRuntime = BVHRuntime<RaytracingGeometry>() 
    let (bvhTree, orderedPrimitiveArray, nodeCount) = bvhTreeBuilder.build simpleSphereGeomertryArray SplitMethods.Middle
    let bvhArray = bvhRuntime.allocateMemoryForBVHRuntime nodeCount
    let bvhRuntimeStack = Array.zeroCreate bvhArray.Length
    let out = bvhRuntime.flattenBVHTree bvhTree bvhArray 0 CoordinateSystem.XYZ
    let ray = Ray(Vector4(4.0f, -1.0f, -10.0f,1.0f), Vector4(0.0f, 0.0f, -1.0f,0.0f))
    let struct(hasIntersection, tHit, geometryOption) = bvhRuntime.traverse bvhArray orderedPrimitiveArray bvhRuntimeStack ray

    let geometry = Utils.forceExtract geometryOption
    let sphere = geometry :?> Sphere


    Assert.True(hasIntersection)
    Assert.Equal(Vector4(4.0f,-1.0f,-15.0f,1.0f), sphere.Center)
    Assert.Equal(3.5f, sphere.Radius)
    Assert.Equal(tHit, 1.5f)


[<Fact>]
let intersectBVHStandardSphereTest () =
    let bvhTreeBuilder = BVHTreeBuilder<RaytracingGeometry>() 
    let bvhRuntime = BVHRuntime<RaytracingGeometry>()
    let (bvhTree, orderedPrimitiveArray, nodeCount) = bvhTreeBuilder.build standardSphereGeomertryArray SplitMethods.Middle
    let bvhArray = bvhRuntime.allocateMemoryForBVHRuntime nodeCount
    let bvhRuntimeStack = Array.zeroCreate bvhArray.Length
    let out = bvhRuntime.flattenBVHTree bvhTree bvhArray 0 CoordinateSystem.XYZ
    let ray = Ray(Vector4(12.0f, -4.5f, -7.0f,1.0f), Vector4(-1.0f, 0.0f, 0.0f,0.0f))
    let struct(hasIntersection, tHit, geometryOption) = bvhRuntime.traverse bvhArray orderedPrimitiveArray bvhRuntimeStack ray

    let geometry = Utils.forceExtract geometryOption
    let sphere = geometry :?> Sphere


    Assert.True(hasIntersection)
    Assert.Equal(Vector4(10.0f,-4.5f,-7.0f,1.0f), sphere.Center)
    Assert.Equal(1.5f, sphere.Radius)
    Assert.Equal(tHit, 0.5f)


[<Fact>]
let intersectBVHIdenticalSphereTest () =
    let bvhTreeBuilder = BVHTreeBuilder<RaytracingGeometry>() 
    let bvhRuntime = BVHRuntime<RaytracingGeometry>()
    let (bvhTree, orderedPrimitiveArray, nodeCount) = bvhTreeBuilder.build identicalSphereGeomertryArray SplitMethods.Middle
    let bvhArray = bvhRuntime.allocateMemoryForBVHRuntime nodeCount
    let bvhRuntimeStack = Array.zeroCreate bvhArray.Length
    let out = bvhRuntime.flattenBVHTree bvhTree bvhArray 0 CoordinateSystem.XYZ
    let ray = Ray(Vector4(2.0f, -10.5f, -7.0f,1.0f), Vector4(0.0f, 1.0f, 0.0f, 0.0f))
    let struct(hasIntersection, tHit, geometryOption) = bvhRuntime.traverse bvhArray orderedPrimitiveArray bvhRuntimeStack ray

    let geometry = Utils.forceExtract geometryOption
    let sphere = geometry :?> Sphere


    Assert.True(hasIntersection)
    Assert.Equal(Vector4(2.0f,-4.5f,-7.0f, 1.0f), sphere.Center)
    Assert.Equal(1.5f, sphere.Radius)
    Assert.Equal(tHit, 4.5f)



[<Fact>]
let intersectBVHLargeSphereTest () =
    let bvhTreeBuilder = BVHTreeBuilder<RaytracingGeometry>() 
    let bvhRuntime = BVHRuntime<RaytracingGeometry>()
    let (bvhTree, orderedPrimitiveArray, nodeCount) = bvhTreeBuilder.build largeSphereGeomertryArray SplitMethods.Middle
    let bvhArray = bvhRuntime.allocateMemoryForBVHRuntime nodeCount
    let bvhRuntimeStack = Array.zeroCreate bvhArray.Length
    let out = bvhRuntime.flattenBVHTree bvhTree bvhArray 0 CoordinateSystem.XYZ
    let ray = Ray(Vector4(4.0f, 5.0f, -22.0f,1.0f), Vector4(0.0f, 0.0f, 1.0f, 0.0f))
    let struct(hasIntersection, tHit, geometryOption) = bvhRuntime.traverse bvhArray orderedPrimitiveArray bvhRuntimeStack ray

    let geometry = Utils.forceExtract geometryOption
    let sphere = geometry :?> Sphere


    Assert.True(hasIntersection)
    Assert.Equal(Vector4(4.0f, 5.0f, -16.5f, 1.0f), sphere.Center)
    Assert.Equal(3.5f, sphere.Radius)
    Assert.Equal(tHit, 2.0f)


[<Fact>]
let intersectBVHSmallLargeSphereTest () =
    let bvhTreeBuilder = BVHTreeBuilder<RaytracingGeometry>() 
    let bvhRuntime = BVHRuntime<RaytracingGeometry>()
    let (bvhTree, orderedPrimitiveArray, nodeCount) = bvhTreeBuilder.build smallLargeSphereGeometryArrray SplitMethods.Middle
    let bvhArray = bvhRuntime.allocateMemoryForBVHRuntime nodeCount
    let bvhRuntimeStack = Array.zeroCreate bvhArray.Length
    let out = bvhRuntime.flattenBVHTree bvhTree bvhArray 0 CoordinateSystem.XYZ
    let ray = Ray(Vector4(-1.0f, 0.0f, 15.0f,1.0f), Vector4(-1.0f, 0.0f, -10.0f,0.0f))
    let ray2 = Ray(Vector4(1.0f, 0.0f, 15.0f,1.0f), Vector4(1.0f, 0.0f, -10.0f,0.0f))

    let struct(hasIntersection, tHit, geometryOption) = bvhRuntime.traverse bvhArray orderedPrimitiveArray bvhRuntimeStack ray
    let struct(hasIntersection2, tHit2, geometryOption2) = bvhRuntime.traverse bvhArray orderedPrimitiveArray bvhRuntimeStack ray2


    Assert.True(hasIntersection)
    Assert.True(hasIntersection2)


[<Fact>]
let intersectBVHSmallLargeSphereInfrontTest () =
    let bvhTreeBuilder = BVHTreeBuilder<RaytracingGeometry>() 
    let bvhRuntime = BVHRuntime<RaytracingGeometry>()
    let (bvhTree, orderedPrimitiveArray, nodeCount) = bvhTreeBuilder.build smallLargeInFrontSphereGeometryArray SplitMethods.Middle
    let bvhArray = bvhRuntime.allocateMemoryForBVHRuntime nodeCount
    let bvhRuntimeStack = Array.zeroCreate bvhArray.Length
    let out = bvhRuntime.flattenBVHTree bvhTree bvhArray 0 CoordinateSystem.XYZ
    let ray = Ray(Vector3(1.0f, 0.0f, 0.0f), Vector3(0.0f, 0.0f, -1.0f))
    let ray2 = Ray(Vector3(1.0f, 0.0f, 15.0f), Vector3(1.0f, 0.0f, -10.0f))

    let struct(hasIntersection, tHit, geometryOption) = bvhRuntime.traverse bvhArray orderedPrimitiveArray bvhRuntimeStack ray

    let geometry = Utils.forceExtract geometryOption
    let sphere = geometry :?> Sphere

    Assert.Equal(Vector4(1.5f,-1.5f,-3.5f,1.0f), sphere.Center)
    Assert.Equal(5.5f, sphere.Radius)


    Assert.True(hasIntersection)



[<Fact>]
let intersectBVHALotLargeSphereInfrontTest () =
    let bvhTreeBuilder = BVHTreeBuilder<RaytracingGeometry>() 
    let bvhRuntime = BVHRuntime<RaytracingGeometry>()
    let (bvhTree, orderedPrimitiveArray, nodeCount) = bvhTreeBuilder.build aLotOfSpheresGeometryArray SplitMethods.Middle
    let bvhArray = bvhRuntime.allocateMemoryForBVHRuntime nodeCount
    let bvhRuntimeStack = Array.zeroCreate bvhArray.Length
    let out = bvhRuntime.flattenBVHTree bvhTree bvhArray 0 CoordinateSystem.XYZ

    let ray = Ray(Vector3(4.0f, -1.0f, 0.0f), Vector3(0.0f, 0.0f, -1.0f))
    let ray2 = Ray(Vector3(0.0f, 0.0f, 0.0f), Vector3(4.0f, -1.0f, -15.0f))
    let ray3 = Ray(Vector3(4.0f, -1.0f, -15.0f), Vector3(0.0f, 0.0f, -1.0f))
    let ray4 = Ray(Vector3(4.0f, -1.0f, -19.0f), Vector3(0.0f, 0.0f, 1.0f))

    let struct(hasIntersection, tHit, geometryOption) = bvhRuntime.traverse bvhArray orderedPrimitiveArray bvhRuntimeStack ray
    let struct(hasIntersection2, tHit2, geometryOption2) = bvhRuntime.traverse bvhArray orderedPrimitiveArray bvhRuntimeStack ray2
    let struct(hasIntersection3, tHit3, geometryOption3) = bvhRuntime.traverse bvhArray orderedPrimitiveArray bvhRuntimeStack ray3
    let struct(hasIntersection4, tHit4, geometryOption4) = bvhRuntime.traverse bvhArray orderedPrimitiveArray bvhRuntimeStack ray4

    let geometry = Utils.forceExtract geometryOption
    let sphere = geometry :?> Sphere

    let geometry2 = Utils.forceExtract geometryOption2
    let sphere2 = geometry2 :?> Sphere

    let geometry3 = Utils.forceExtract geometryOption3
    let sphere3 = geometry3 :?> Sphere

    let geometry4 = Utils.forceExtract geometryOption4
    let sphere4 = geometry4 :?> Sphere

    Assert.True(hasIntersection)
    Assert.True(hasIntersection2)
    Assert.True(hasIntersection3)
    Assert.True(hasIntersection4)
    

    Assert.Equal(Vector4(4.0f,-1.0f,-15.0f,1.0f), sphere2.Center)
    Assert.Equal(3.5f, sphere2.Radius)

    Assert.Equal(Vector4(4.0f,-1.0f,-15.0f,1.0f), sphere.Center)
    Assert.Equal(3.5f, sphere.Radius)

    Assert.Equal(Vector4(4.0f,-1.0f,-15.0f,1.0f), sphere3.Center)
    Assert.Equal(3.5f, sphere3.Radius)

    Assert.Equal(Vector4(4.0f,-1.0f,-15.0f,1.0f), sphere3.Center)
    Assert.Equal(3.5f, sphere3.Radius)

    Assert.Equal(11.5f, tHit)
    Assert.True(tHit2 > tHit)







    


