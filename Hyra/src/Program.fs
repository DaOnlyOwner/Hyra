open Parser
open HyraParser

[<EntryPoint>]
let main _ = 
    let input = Input.inputFromStr "1+2*3+5" 
    printfn "Result: %A" (run (recoverParser pBinary) input)
    System.Console.ReadKey() |> ignore
    0

