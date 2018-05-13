module Error

open Combinators
open System
open System.Linq

type ItemState<'a> =
    | Valid of 'a
    | Invalid

let genError info whereBegin note expected =
    let (_,whereEnd,_,_) = List.head info
    (whereBegin,whereEnd,note,expected)


// ----- From https://gist.github.com/devhawk/4719d1b369170b206cd88b9da16e1b8a ------
let consoleColor (fc : ConsoleColor) = 
    let current = Console.ForegroundColor
    Console.ForegroundColor <- fc
    { new IDisposable with
          member x.Dispose() = Console.ForegroundColor <- current }

// printf statements that allow user to specify output color
let cprintf color str = Printf.kprintf (fun s -> use c = consoleColor color in printf "%s" s) str
let cprintfn color str = Printf.kprintf (fun s -> use c = consoleColor color in printfn "%s" s) str
// ------------------------------------------------------------------------------------

let rootError p1 (note, expected) =
    let inner input =
        let res, invFn = run p1 input
        match res with
        | Ok(item,newInput) -> Ok(item,newInput), invFn
        | Fail(info) -> Fail( (genError info input note expected)::info ) , invFn
    {fn=inner}


// The line and the unexpected word can be computed using whereBegin and whereEnd... 
let rootErrorReplace p1 (note,expected) =
    let inner input =
        let res, invFn = run p1 input
        match res with
        | Ok(item,newInput) -> Ok(item,newInput), invFn
        | Fail(info) -> Fail( [genError info input note expected] ), invFn
    {fn=inner}

// Also include filename?
let emitError (info : ErrorInformation list) =
    let x::xs = List.rev info

    let errorColor = ConsoleColor.Red
    let lineNoColor = ConsoleColor.Cyan
    let header_trailerError = ConsoleColor.Blue
    let rootErrorColor = ConsoleColor.Magenta
    let noteColor = ConsoleColor.Green

    cprintfn header_trailerError "========== Syntax Error =========="
    let (whereBegin , whereEnd , noteOption , expected) = x
    let note = match noteOption with | Some(item) -> sprintf "Note: %s" item | None -> ""
    let offendingString = Input.getRange whereBegin.text whereBegin.pos whereEnd.pos

    let (prepend,intermediate,append) = Input.getPartitionLines whereBegin.text whereBegin whereEnd
    let splittedIntermediate = intermediate.Split([|'\n'|])

    printfn "Error: %i:%i-%i:%i%sOffending Text:%s" whereBegin.lineNo whereBegin.colNo whereEnd.lineNo whereEnd.colNo Environment.NewLine Environment.NewLine


    cprintf lineNoColor "L%i: " (whereBegin.lineNo + 1)
    printf "%s" prepend
    cprintf errorColor "%s" splittedIntermediate.[0]


    let mutable counter = whereBegin.lineNo + 2
    for line in splittedIntermediate.Skip(1).Take(splittedIntermediate.Length - 2).Skip(1) do
        cprintf lineNoColor "%sL%i: " Environment.NewLine counter
        cprintf errorColor "%s" line
    printfn "%s" append
    cprintf errorColor "Expected %s, but got %s" expected offendingString 
    cprintfn noteColor "%s%s" Environment.NewLine note



    List.iter (fun (x : ErrorInformation) -> 
        let (whereBegin , whereEnd , noteOption , expected) = x
        let note = match noteOption with | Some(item) -> sprintf "Note: %s" item | None -> ""
        cprintfn rootErrorColor "when parsing %s, starting at line %i:column %i, ending at line %i:column %i.\n%s\n" expected whereBegin.lineNo whereBegin.colNo whereEnd.lineNo whereEnd.colNo note) xs
    cprintfn header_trailerError "========== ------------ =========="
    let (_,whereEnd,_,_) = (List.head info)
    offendingString, whereEnd

let invRecover (invFn :InvertedParserFunc<'a>) item = match item with | Valid(unwItem) -> invFn unwItem | Invalid -> "" 
let recover skipMethod (p1 : Parser<'a>) =
    let inner input  =
        let res, invFn = run p1 input
        match res with
        | Ok(item,newInput) -> Ok(Valid(item),newInput), invRecover invFn 
        | Fail(info) -> 
            let unexpectedStr, newInput = emitError info
            let skipped, inputAfterSkipped = skipMethod newInput 
            Ok(Invalid, inputAfterSkipped), fun x -> invRecover invFn x + unexpectedStr + skipped
            
    {fn=inner}

let skipUntil pred fromState =
    let rec inner input =
        let item,newInput = Input.next input 
        match item with
        | Some(ch) -> if pred ch then newInput else inner newInput
        | None -> newInput
    let whereEnd = inner fromState
    Input.getRange fromState.text fromState.pos whereEnd.pos, whereEnd

let recoverSkip pred p1 = recover (skipUntil pred) p1
let recoverSkipUntilSpecific c p1 = recover (skipUntil c.Equals) p1