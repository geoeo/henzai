module TypeTests

open Xunit
open HenzaiFunc.Core.Types
open System.Runtime.InteropServices


[<Fact>]
let SplitAxisSizeTest () =
    Assert.Equal(1, sizeof<SplitAxis>)

[<Fact>]
let InteriorLinearNodeInfoSizeTest () =
    let t = InteriorLinearNodeInfo()
    Assert.Equal(8, sizeof<InteriorLinearNodeInfo>)
    Assert.Equal(8, Marshal.SizeOf(t))

[<Fact>]
let LeafLinearNodeInfoSizeTest () =
    let t = LeafLinearNodeInfo()
    Assert.Equal(8, sizeof<LeafLinearNodeInfo>)
    Assert.Equal(8, Marshal.SizeOf(t))

[<Fact>]
let BVHLinearNodeInfoSizeTest () =
    Assert.Equal(32, sizeof<BVHLinearNode>)