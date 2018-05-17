open Parser
open HyraParser

[<EntryPoint>]
let main _ = 
    let input = Input.inputFromStr "34..33" 
    printfn "Result: %A" (run (recoverParser pFloat) input)
    System.Console.ReadKey() |> ignore
    0

