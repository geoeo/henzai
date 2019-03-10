namespace HenzaiFunc.Core.Extensions


module Array =

    type 'T``[]`` with
        //TODO: test
        /// Partitions x such that x[i..k-1] <= x[k] <= x[k+1..j]
        /// Returns k, the new idx of x[pidx]
        member x.PartitionInPlace(map : ('T -> 'U)  when 'U : comparison, i: int, j: int, pidx: int) =
            assert (pidx < j)
            assert (j < x.Length)

            let p = x.[pidx]
            let mappedP = map(p)
            let mutable running = true
            let mutable smallIdx = i
            let mutable largeIdx = j
            let mutable mappedSmall = map(x.[smallIdx])
            let mutable mappedLarge = map(x.[largeIdx])
            let mutable k = -1

            while running do
                while mappedSmall < mappedP && smallIdx < j do
                    smallIdx <- smallIdx + 1
                    mappedSmall <- map(x.[smallIdx])

                while mappedLarge > mappedP && largeIdx > i do
                    largeIdx <- largeIdx - 1
                    mappedLarge <- map(x.[largeIdx])

                if smallIdx >= largeIdx then
                    k <- smallIdx
                    running <- false
                else
                    if mappedSmall = mappedP && not (smallIdx = pidx) then
                        smallIdx <- smallIdx + 1
                        mappedSmall <- map(x.[smallIdx])

                    if mappedLarge = mappedP && not (largeIdx = pidx) then
                        largeIdx <- largeIdx - 1
                        mappedLarge <- map(x.[largeIdx])

                    let temp = x.[smallIdx]
                    x.[smallIdx] <- x.[largeIdx]
                    x.[largeIdx] <- temp
                    mappedSmall <- map(x.[smallIdx])
                    mappedLarge <- map(x.[largeIdx])

            k
        /// The value pointed at pivot is changed such that its value would be as if the array is sorted.
        /// All elements i' with an index less than pivot satisfy map(i') <= map(m).
        /// All elements i'' with an index greater than pivot satisfy map(i'') >= map(m).
        /// See https://en.cppreference.com/w/cpp/algorithm/nth_element
        /// Implementation : https://archive.siam.org/meetings/analco04/abstracts/CMartinez.pdf
        member x.PartialSortInPlace(map : ('T -> 'U)  when 'U : comparison, m : int) = 
            assert (m < x.Length)
            let pivot i j = (i+j)/2
            let rec partialSort i j =
                if i < j then
                    let pidx = pivot i j
                    let k = x.PartitionInPlace(map, i, j, pidx)
                    partialSort i (k-1)
                    if k < (m - 1) then partialSort (k+1) j
                    ()
            partialSort 0 x.Length
            

 
