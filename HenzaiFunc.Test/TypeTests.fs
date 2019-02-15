module TypeTests

open Xunit
open HenzaiFunc.Core.Types
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
let LeafRuntimeNodeInfoSizeTest () =
    let t = LeafRuntimeNode()
    Assert.Equal(4, sizeof<LeafRuntimeNode>)
    Assert.Equal(4, Marshal.SizeOf(t))

[<Fact>]
let BVHLinearNodeInfoSizeTest () =
    Assert.Equal(32, sizeof<BVHRuntimeNode>)