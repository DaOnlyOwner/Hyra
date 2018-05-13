open Combinators
open SpecificCombinators
open Error
open System.IO

[<EntryPoint>]
let main _ = 
    let x =  recoverSkipUntilSpecific '3' (satAnyDigit .>>. satAnyDigit .>>. satAnyDigit .>>. satAnyDigit .>>. satAnyDigit)
    let input = Input.inputFromStr "12_34" 
    printfn "Result: %A" (Combinators.run x input)
    System.Console.ReadKey() |> ignore
    0

