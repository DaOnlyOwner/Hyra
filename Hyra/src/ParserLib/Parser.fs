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
        let note = match noteOption with | Some(item) -> sprintf "%sNote: %s" Environment.NewLine item | None -> ""
        printf "when parsing (%i:%i-%i:%i): " whereBegin.lineNo whereBegin.colNo whereEnd.lineNo whereEnd.colNo
        cprintfn rootErrorColor "%s%s\n" expected note) xs
    cprintfn headerTrailerError "========== ------------ =========="
    let (_,whereEnd,_,_) = List.head info
    whereEnd


let recover info method (input : Input.InputState) =
    let whereEnd = emitError info
    let newInput = {Input.InputState.text=input.text; Input.InputState.colNo = input.colNo; Input.InputState.lineNo = input.lineNo; Input.InputState.pos=whereEnd.pos}
    let inputAfterRecovery = method newInput
    Ok(None,inputAfterRecovery)

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

let inline (<.-.>) p1 (note,expected) = toLeafError p1 (note,expected)      
   
let renameRootError p1 (note,expected) = 
    let inner input = 
        let res = run p1 input
        match res with 
        | Ok(item,newInput) -> Ok(item,newInput)
        | Fail(info) -> 
            let (whereBegin,whereEnd,_,_) = List.head info
            Fail((whereBegin,whereEnd,note,expected)::info)
    {fn=inner; expected = expected; note = note; recoverMethod = recoverOne}

let inline (<.^.>) p1 (note,expected) = renameRootError p1 (note,expected) 

let recoverParser p1 =
    let inner input =
        match run p1 input with 
        | Ok(item,newInput) -> Ok(Some(item,newInput), newInput)
        | Fail(info) -> recover info p1.recoverMethod input
    {fn=inner; expected = p1.expected; note = p1.note; recoverMethod = p1.recoverMethod}


// --- Combinators

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

let inline (.>>) p1 p2 = andThenLeft p1 p2

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

let inline (>>.) p1 p2 = andThenRight p1 p2
    
let andThenRecover p1 p2 = 
    let exp = sprintf "%s %s" p1.expected p2.expected
    let inner input =
        let res = run p1 input
        match res with
        | Ok(item,newInput) -> 
            let res2 = run p2 newInput
            match res2 with
            | Ok(item2,newInput2) -> Ok( Some(item,item2), newInput2)
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
            | Ok(_,newInput2) -> Ok( Some(item), newInput2)
            | Fail(info) -> recover info p2.recoverMethod newInput
        | Fail(info) -> Fail(genError info input None exp)
    {fn=inner; expected = exp; note = None; recoverMethod = recoverOne}
    
let inline (.>/>) p1 p2 = andThenLeftRecover p1 p2

let andThenRightRecover p1 p2 =
    let exp = sprintf "%s %s" p1.expected p2.expected
    let inner input =
        let res = run p1 input
        match res with
        | Ok(_,newInput) -> 
            let res2 = run p2 newInput
            match res2 with
            | Ok(item2,newInput2) -> Ok( Some(item2), newInput2)
            | Fail(info) -> recover info p2.recoverMethod newInput
        | Fail(info) -> Fail(genError info input None exp)
    {fn=inner; expected = exp; note = None; recoverMethod = recoverOne}
let inline (>/>.) p1 p2 = andThenRightRecover p1 p2

// END: TODO: REFACTOR


let orElse p1 p2 =
    let exp = sprintf "(%s) | (%s)" p1.expected p2.expected
    let inner input =
        let res1 = run p1 input
        match res1 with 
        | Ok(item1,newInput1) -> Ok(item1,newInput1)
        | _ -> 
            let res2 = run p2 input
            match res2 with 
            | Ok(item2,newInput2) -> Ok(item2,newInput2)
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
       
let maxMunch p1 p2 = 
    let exp = sprintf "%s | %s" p1.expected p2.expected
    let inner input =
        let res1 = run p1 input
        let res2 = run p2 input
        match res1 with
        | Ok(_,newInput1) -> 
            match res2 with
            | Ok(_,newInput2) -> 
                if newInput1.pos >= newInput2.pos then res1 else res2
            | Fail(_) -> res1
        | Fail(_) ->
            match res2 with
            | Ok(_) -> res2
            | Fail(info) -> Fail(genError info input None exp) 
    {fn=inner; expected = exp; note = None; recoverMethod = recoverOne}

let inline (<-?->) p1 p2 = maxMunch p1 p2  

let choiceMax pList = List.reduce maxMunch pList

open System.Collections.Generic
let cache p1 =
    let internalCache = new Dictionary<int,Result<'a>>()
    let inner input = 
        if internalCache.ContainsKey(input.pos) then internalCache.[input.pos] else
            let res = run p1 input
            internalCache.Add(input.pos,res)
            res
    {fn=inner; expected = p1.expected; note = p1.note; recoverMethod = p1.recoverMethod}

let inline (!~) p1 = cache p1

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
        printfn "%s" word
        let rec comp nextInput i =
            let (item,nextNewInput) = Input.next nextInput
            match item with 
            | Some(inCh) -> if  i = word.Length then Ok(word,nextInput) 
                            else if inCh <> word.[i] then Fail ( [input,nextInput,None,exp]) 
                            else comp nextNewInput (i+1)
            | None -> Fail( [input,nextInput,Some("End of File"), exp])
        comp input 0    
    {fn=inner; expected = exp; note = None; recoverMethod = recoverOne}


let inline (!.-) xs = everythingExcept xs 
let inline (!%) word = pWord word 


let pSpaces = !* (sat System.Char.IsWhiteSpace "<whitespace>" None)

let dec<'a>() =
    let impl = ref Unchecked.defaultof<Parser<'a>>
    let actual = {fn = (fun x-> run !impl x); expected=""; note=None; recoverMethod = recoverOne}
    actual,impl

let pPlaceholder<'a> = {fn = (fun input -> Ok(Unchecked.defaultof<'a>,input)); expected=""; note=Some("This is a placeholder"); recoverMethod=recoverOne}
let pPlaceholderFail = {fn = (fun input -> Fail([input,input,None,""])); expected=""; note=Some("This is a placeholder"); recoverMethod=recoverOne}



let merge p1 =
    let inner input=
        match run p1 input with
        | Ok(item,newInput) -> Ok(Input.getRange input.text input.pos newInput.pos,newInput)
        | Fail(info) -> Fail(info)
    {fn=inner; expected=p1.expected;note=p1.note;recoverMethod=recoverOne}




// Monadic stuff

let bind func p1 = 
    let exp = p1.expected
    let inner input =
        match run p1 input with
        | Ok(item,newInput) -> run (func item) newInput
        | Fail(info) -> Fail(info)
    {fn=inner;expected=exp; note=p1.note; recoverMethod=recoverOne}

let (>>=) func p1 = bind p1 func

let ret x = {fn = (fun input -> Ok(x,input)); expected = ""; note=None; recoverMethod=recoverOne}
let map func = bind (func >> ret)   

let (<!>) func p1 = map p1 func












