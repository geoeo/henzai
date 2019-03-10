namespace HenzaiFunc.Core

module Utils =
    let conditionalSwap f a b = if f a b then struct(b, a) else struct(a, b)

    let boolToInt b = 
        match b with
        | true -> 1
        | false -> 0

    let forceExtract option =
        match option with
        | Some x -> x
        | None -> failwithf "Make sure what you are extracting is not None!"