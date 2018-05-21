open Parser
open HyraParser

[<EntryPoint>]
let main _ = 
    let input = Input.inputFromStr "3.f+2*3" 
    printfn "Result: %A" (run pExpr input)
    System.Console.ReadKey() |> ignore
    0

