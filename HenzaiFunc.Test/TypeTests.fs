module TypeTests

open Xunit
open HenzaiFunc.Core.Types
open HenzaiFunc.Core.RaytraceGeometry
open System.Runtime.InteropServices


[<Fact>]
let SplitAxisSizeTest () =
    Assert.Equal(1, sizeof<SplitAxis>)

[<Fact>]
let InteriorRuntimeNodeInfoSizeTest () =
    let t = InteriorRuntimeNode()
    Assert.Equal(8, sizeof<InteriorRuntimeNode>)
    Assert.Equal(8, Marshal.SizeOf(t))

[<Fact>]
let InteriorRuntimeNodeInfoAssignTest () =
    let t = InteriorRuntimeNode(2, SplitAxis.Y)
    Assert.Equal(8, sizeof<InteriorRuntimeNode>)
    Assert.Equal(8, Marshal.SizeOf(t))
    Assert.Equal(SplitAxis.Y, t.splitAxis)

[<Fact>]
let LeafRuntimeNodeInfoSizeTest () =
    let t = LeafRuntimeNode()
    Assert.Equal(4, sizeof<LeafRuntimeNode>)
    Assert.Equal(4, Marshal.SizeOf(t))

[<Fact>]
let BVHLinearNodeInfoSizeTest () =
    Assert.Equal(32, sizeof<BVHRuntimeNode>)

[<Fact>]
let BVHLinearNodeInfoAssignTest () =
    let i = InteriorRuntimeNode(2, SplitAxis.Y)
    let t = BVHRuntimeNode(AABB(), i, 0)
    Assert.Equal(32, sizeof<BVHRuntimeNode>)
    Assert.Equal(SplitAxis.Y, t.interiorNode.splitAxis)