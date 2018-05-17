module Parser

open Input

type WhereBegin = Input.InputState
type WhereEnd = Input.InputState
type Note = string option
type Expected = string
type Unexpected = string

type ErrorInformation = WhereBegin * WhereEnd * Note * Expected


type Result<'a> = 
    | Ok of  'a * Input.InputState 
    | Fail of ErrorInformation list    

type Parser<'a> = {
    fn:Input.InputState -> Result<'a>
    expected: string
    note : string option
    recoverMethod: InputState -> InputState
}

type ItemState<'a> =
    | Valid of 'a
    | Invalid

let inline run parser state =
    parser.fn state

// Error 

open System
open System.Linq

let genError info whereBegin note expected =
    let (_,whereEnd,_,_) = List.head info
    (whereBegin,whereEnd,note,expected)::info

let leafError info whereBegin note expected =
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



let emitError (info : ErrorInformation list) =
    let x::xs = List.rev info

    let errorColor = ConsoleColor.Red
    let lineNoColor = ConsoleColor.Cyan
    let headerTrailerError = ConsoleColor.Blue
    let rootErrorColor = ConsoleColor.Magenta
    let noteColor = ConsoleColor.Green

    cprintfn headerTrailerError "========== Syntax Error =========="
    let (whereBegin , whereEnd , noteOption , expected) = x
    let note = match noteOption with | Some(item) -> sprintf "Note: %s" item | None -> ""
    let offendingString = Input.getRange whereBegin.text whereBegin.pos whereEnd.pos

    let (prepend,intermediate,append) = Input.getPartitionLines whereBegin.text whereBegin whereEnd
    let splittedIntermediate = intermediate.Split([|'\n'|])

    printfn "Error at %i:%i-%i:%i%sOffending Text:%s" whereBegin.lineNo whereBegin.colNo whereEnd.lineNo whereEnd.colNo Environment.NewLine Environment.NewLine


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



    List.iter (fun (xNew : ErrorInformation) -> 
        let (whereBegin , whereEnd , noteOption , expected) = xNew
        let note = match noteOption with | Some(item) -> sprintf "Note: %s" item | None -> ""
        cprintfn rootErrorColor "when parsing: %s, starting at line %i:column %i, ending at line %i:column %i.\n%s\n" expected whereBegin.lineNo whereBegin.colNo whereEnd.lineNo whereEnd.colNo note) xs
    cprintfn headerTrailerError "========== ------------ =========="
    let (_,whereEnd,_,_) = List.head info
    whereEnd


let recover info method (input : Input.InputState) =
    let whereEnd = emitError info
    let newInput = {Input.InputState.text=input.text; Input.InputState.colNo = input.colNo; Input.InputState.lineNo = input.lineNo; Input.InputState.pos=whereEnd.pos}
    let inputAfterRecovery = method newInput
    Ok(Invalid,inputAfterRecovery)

let skipUntil pred fromState =
    let rec inner input =
        let item,newInput = Input.next input 
        match item with
        | Some(ch) -> if pred ch then newInput else inner newInput
        | None -> newInput
    let whereEnd = inner fromState
    whereEnd

let recoverUntilSpecific c = skipUntil c.Equals
let inline rus c = recoverUntilSpecific c // Shorthand

let recoverOne = skipUntil (fun _ -> true)


let toLeafError p1 (note,expected) =
    let inner input =
        let res = run p1 input
        match res with
        | Ok(item,newInput) -> Ok(item,newInput)
        | Fail(info) -> Fail( [leafError info input note expected] )
    {fn=inner; expected = expected; note = note; recoverMethod = recoverOne}
    
   
let renameRootError p1 (note,expected) = 
    let inner input = 
        let res = run p1 input
        match res with 
        | Ok(item,newInput) -> Ok(item,newInput)
        | Fail(info) -> 
            let (whereBegin,whereEnd,_,_) = List.head info
            Fail((whereBegin,whereEnd,note,expected)::info)
    {fn=inner; expected = expected; note = note; recoverMethod = recoverOne}



let recoverParser p1 =
    let inner input =
        match run p1 input with 
        | Ok(item,newInput) -> Ok(Valid(item,newInput), newInput)
        | Fail(info) -> recover info p1.recoverMethod input
    {fn=inner; expected = "e.e"; note = None; recoverMethod = recoverOne}


// --- Combinators

open System

// BEGIN: TODO: REFACTOR    
let andThen p1 p2 = 
    let exp = sprintf "%s %s" p1.expected p2.expected
    let inner input =
        let res = run p1 input
        match res with
        | Ok(item,newInput) -> 
            let res2 = run p2 newInput
            match res2 with
            | Ok(item2,newInput2) -> Ok( (item,item2), newInput2)
            | Fail(info) -> Fail(genError info newInput None exp)
        | Fail(info) -> Fail(genError info input None exp)
    {fn=inner; expected = exp; note = None; recoverMethod = recoverOne}
 
let inline (.>>.) p1 p2 = andThen p1 p2 
 
let andThenLeft p1 p2 =
    let exp = sprintf "%s %s" p1.expected p2.expected
    let inner input =
        let res = run p1 input
        match res with
        | Ok(item,newInput) -> 
            let res2= run p2 newInput
            match res2 with
            | Ok(_,newInput2) -> Ok( item, newInput2)
            | Fail(info) -> Fail(genError info newInput None exp)
        | Fail(info) -> Fail(genError info input None exp)
    {fn=inner; expected = exp; note = None; recoverMethod = recoverOne}

let inline (>>.) p1 p2 = andThenLeft p1 p2

let andThenRight p1 p2 =
    let exp = sprintf "%s %s" p1.expected p2.expected
    let inner input =
        let res = run p1 input
        match res with
        | Ok(_,newInput) -> 
            let res2 = run p2 newInput
            match res2 with
            | Ok(item2,newInput2) -> Ok( item2, newInput2)
            | Fail(info) -> Fail(genError info newInput None exp)
        | Fail(info) -> Fail(genError info input None exp)
    {fn=inner; expected = exp; note = None; recoverMethod = recoverOne}

let inline (.>>) p1 p2 = andThenRight p1 p2
    
let andThenRecover p1 p2 = 
    let exp = sprintf "%s %s" p1.expected p2.expected
    let inner input =
        let res = run p1 input
        match res with
        | Ok(item,newInput) -> 
            let res2 = run p2 newInput
            match res2 with
            | Ok(item2,newInput2) -> Ok( Valid(item,item2), newInput2)
            | Fail(info) -> recover info p2.recoverMethod newInput
        | Fail(info) -> Fail(genError info input None exp)
    {fn=inner; expected = exp; note = None; recoverMethod = recoverOne}
    
let inline (.>/>.)  p1 p2 = andThenRecover p1 p2
 
let andThenLeftRecover p1 p2 =
    let exp = sprintf "%s %s" p1.expected p2.expected
    let inner input =
        let res = run p1 input
        match res with
        | Ok(item,newInput) -> 
            let res2= run p2 newInput
            match res2 with
            | Ok(_,newInput2) -> Ok( Valid(item), newInput2)
            | Fail(info) -> recover info p2.recoverMethod newInput
        | Fail(info) -> Fail(genError info input None exp)
    {fn=inner; expected = exp; note = None; recoverMethod = recoverOne}
    
let inline (>/>.) p1 p2 = andThenLeftRecover p1 p2

let andThenRightRecover p1 p2 =
    let exp = sprintf "%s %s" p1.expected p2.expected
    let inner input =
        let res = run p1 input
        match res with
        | Ok(_,newInput) -> 
            let res2 = run p2 newInput
            match res2 with
            | Ok(item2,newInput2) -> Ok( Valid(item2), newInput2)
            | Fail(info) -> recover info p2.recoverMethod newInput
        | Fail(info) -> Fail(genError info input None exp)
    {fn=inner; expected = exp; note = None; recoverMethod = recoverOne}
let inline (.>/>) p1 p2 = andThenRightRecover p1 p2

// END: TODO: REFACTOR


let orElse p1 p2 =
    let exp = sprintf "(%s) | (%s)" p1.expected p2.expected
    let inner input =
        let res1 = run p1 input
        match res1 with 
        | Ok(item,newInput) -> Ok(item,newInput)
        | _ -> 
            let res2 = run p2 input
            match res2 with 
            | Ok(item,newInput) -> Ok(item,newInput)
            | Fail(info) -> Fail(genError info input None exp)
    {fn=inner; expected = exp; note = None; recoverMethod = recoverOne}

let zeroOrMore p1 =
    let exp = sprintf "(%s)*"  p1.expected
    let s = List<'a>.Empty
    let rec inner xs input = 
        let res = run p1 input
        match res with 
        | Ok (item, newInput)-> inner (item::xs) newInput
        | _ -> Ok(List.rev xs,input)
    {fn=inner s; expected = exp; note = None; recoverMethod = recoverOne} 


let inline oneOrMore p1 = p1 .>>. zeroOrMore p1
   
    
let inline (!*) p1 = zeroOrMore p1    

    
let optional p1 = 
    let exp = sprintf "(%s)?" p1.expected
    let inner input =
        let res = run p1 input
        match res with 
        | Ok (item, newInput) -> Ok(Some(item), newInput)
        | _ -> Ok(None,input)
    {fn=inner; expected = exp; note = None; recoverMethod = recoverOne}


let choice pList = List.reduce orElse pList   

let inline (!?) p1 = optional p1    

let inline (!+) p1 = oneOrMore p1

let inline (<?>) p1 p2 = orElse p1 p2

let sat pred expected note =
    let inner state =
       let item,newState = Input.next state
       match item with 
        |Some unwItem -> 
           if pred unwItem then Ok (unwItem, newState)
           else Fail( [(state,newState,note,expected)] )
        |None -> Fail(  [(state,newState, Some("End of File") , expected)] )
    {fn=inner; expected = expected; note = note; recoverMethod = recoverOne}
       
// Specific Combinators

let pLetter = sat System.Char.IsLetter "<letter>" None
let pDigit = sat System.Char.IsDigit "<digit>" None
let pCh c = sat c.Equals (sprintf "'%c'" c) None

let everythingExcept except =
    let exp = sprintf "<every character except %A>" except
    let pred = fun x -> Operators.not (List.contains x except)
    sat pred exp None
    

let pWord (word:string) = 
    let exp = sprintf "'%s'" word
    let inner (input : Input.InputState) =
        let rec comp nextInput i =
            let (item,nextNewInput) = Input.next nextInput
            match item with 
            | Some(inCh) -> if inCh <> word.[i] then Fail ( [input,nextNewInput,None,exp]), nextNewInput else comp nextNewInput (i+1)
            | None -> Fail( [input,nextNewInput,Some("End of File"), exp]),nextNewInput
        let (_,newInput) = comp input 0
        Ok(word,newInput)      
    {fn=inner; expected = exp; note = None; recoverMethod = recoverOne}


let inline (!.-) xs = everythingExcept xs 
let inline (!%) word = pWord word 


let pSpaces = !* (sat System.Char.IsWhiteSpace "<whitespace>" None)

let dec<'a>() =
    let impl = ref Unchecked.defaultof<Parser<'a>>
    let actual = {fn = (fun x-> run !impl x); expected=""; note=None; recoverMethod = recoverOne}
    actual,impl

(*
let merge p1 =
    let inner input =
        let (res,newInput) = run p input 
        let rec mergeInner p = 
            | Ok(item, newInput) -> 
                match item with
                | :? List as L -> List.reduce (mergeInner L)
                | :? string as S -> S
                | :? char as C -> sprintf "%c" C
                | :? Some(innerItem) -> mergeInner innerItem     
                | :? _ -> _
            | Fail(info) -> Fail(info)    *)
            














