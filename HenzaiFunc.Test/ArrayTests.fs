module ArrayTests

open Xunit
open HenzaiFunc.Core.Extensions.Array

let a1 : int[] = [|5; 2; 1; 0; 10; 11;|]
let a2 = [|1; 5; 10; 0; 2;|]
let a3 = [|1; 5; 10; 3; 2; 11;|]
let a4 = [|0; 1; 1; 0; 1; 0;|]
let a5 = [|0; 0; 0; 0; 0; 0;|]

[<Fact>]
let partitionTestA1() =
    let pivot1 = a1.Length / 2

    let k1 = a1.PartitionInPlace((fun x -> x), 0, a1.Length-1, pivot1)

    Assert.Equal(3, pivot1)
    Assert.Equal(0, k1)
    Assert.Equal(0, a1.[k1])

[<Fact>]
let partitionTestA2() =
    let pivot2 = a2.Length / 2

    let k2 = a2.PartitionInPlace((fun x -> x), 0, a2.Length-1, pivot2)

    Assert.Equal(2, pivot2)
    Assert.Equal(4, k2)
    Assert.Equal(10, a2.[k2])

[<Fact>]
let partitionTestA3() =
    let pivot3 = a3.Length / 2

    let k3 = a3.PartitionInPlace((fun x -> x), 0, a3.Length-1, pivot3)

    Assert.Equal(3, pivot3)
    Assert.Equal(2, k3)
    Assert.Equal(3, a3.[k3])

[<Fact>]
let partitionTestA4() =
    let pivot4 = a4.Length / 2

    let k4 = a4.PartitionInPlace((fun x -> x), 0, a4.Length-1, pivot4)

    Assert.Equal(3, pivot4)
    Assert.Equal(1, k4)
    Assert.Equal(0, a4.[k4])

[<Fact>]
let partitionTestA5() =
    let pivot5 = a5.Length / 2

    let k5 = a5.PartitionInPlace((fun x -> x), 0, a5.Length-1, pivot5)

    Assert.Equal(3, pivot5)
    Assert.Equal(3, k5)
    Assert.Equal(0, a5.[k5])


