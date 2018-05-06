
open SpecificCombinators

[<EntryPoint>]
let main argv = 
    let x = satIdentifier
    let input = Input.inputFromFile @"E:\F#Projects\Hyra\Tests\testprogram.hy"
    printfn "Result: %A" (Combinators.run x input)
    System.Console.ReadKey() |> ignore
    0


