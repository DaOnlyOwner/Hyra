
open SpecificCombinators

[<EntryPoint>]
let main _ = 
    let x = satIdentifier
    let input = Input.inputFromFile "/home/daonlyowner/testprogram.hy"
    printfn "Result: %A" (Combinators.run x input)
    //System.Console.ReadKey() |> ignore
    0

