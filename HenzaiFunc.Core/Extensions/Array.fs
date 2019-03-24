namespace HenzaiFunc.Core.Extensions

module Array =

    type 'T``[]`` with
        /// element x[m] holds the value as if it were sorted via map
        /// See https://en.cppreference.com/w/cpp/algorithm/nth_element
        member x.nthElement(map : ('T -> 'U)  when 'U : comparison, m : int) =
            let length = x.Length
            assert (m < length)

            for i in 0..m do
                if map(x.[i]) > map(x.[m]) then
                    let temp = x.[i]
                    x.[i] <- x.[m]
                    x.[m] <- temp

            for i in length-1..m do
                if map(x.[i]) < map(x.[m]) then
                    let temp = x.[i]
                    x.[i] <- x.[m]
                    x.[m] <- temp


        /// Partitions x such that x[i..k-1] <= x[k] <= x[k+1..j]
        /// Returns k, the new idx of x[pidx]
        member x.PartitionInPlace(map : ('T -> 'U)  when 'U : comparison, i: int, j: int, pidx: int) =
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

        // Obtain a sorted list of the m smallest elements of a given set of n elements.
        // Implementation : https://archive.siam.org/meetings/analco04/abstracts/CMartinez.pdf
        member x.PartialQuickSortInPlace(map : ('T -> 'U)  when 'U : comparison, m : int) = 
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
            

 
