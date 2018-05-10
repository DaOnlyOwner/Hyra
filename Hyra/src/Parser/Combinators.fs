module Combinators


type WhereBegin = Input.InputState
type WhereEnd = Input.InputState
type Note = string option
type Expected = string
type Unexpected = string

type ErrorInformation = WhereBegin * WhereEnd * Note * Expected

type Result<'a> = 
    | Ok of 'a * Input.InputState 
    | Fail of ErrorInformation list    

type InvertedParserFunc<'a> = 'a -> string 

type Parser<'a> = Input.InputState -> (Result<'a> * InvertedParserFunc<'a>)

let inline defMFun _ = ""
let stdAndThen = ""
let stdOrElse = ""
let stdZeroOrMore = ""
let stdOneOrMore = ""
let stdOptional = ""


let inline run parser state =
    parser state
    
let inline invAndThen invSub1 invSub2 (x1,x2) = invSub1 x1 + invSub2 x2 
let andThen p1 p2 = 
    let inner input =
        let res,invFn = run p1 input
        match res with
        | Ok(item,newInput) -> 
            let res2,invFn2 = run p2 newInput
            match res2 with
            | Ok(item2,newInput2) -> Ok( (item,item2), newInput2), invAndThen invFn invFn2
            | Fail(info) -> Fail(info) , defMFun
        | Fail(info) -> Fail(info), defMFun
    inner
let inline (.>>.) p1 p2 = andThen p1 p2

   
let inline invOrElse invSub x = invSub x
let orElse p1 p2 =
    let inner input =
        let res1,invFn1 = run p1 input
        match res1 with 
        | Ok(item,newInput) -> Ok(item,newInput), invOrElse invFn1
        | _ -> 
            let res2,invFn2 = run p2 input
            match res2 with 
            | Ok(item,newInput) -> Ok(item,newInput) , invOrElse invFn2
            | Fail(info) -> Fail(info), defMFun     
    inner

let inline listToStr invFns xs = List.zip invFns xs  |> List.map (fun (invFn,x) -> invFn x) |> List.reduce (+)
let inline invZeroOrMore invFns xs = match xs with | _ when List.isEmpty xs -> "" | _ -> (listToStr invFns xs)

let zeroOrMore p1 =
    let s = List<'a>.Empty
    let t = List<InvertedParserFunc<'a>>.Empty
    let rec inner xs invFns input = 
        let res,invFn = run p1 input
        match res with 
        | Ok (item, newInput)-> inner (item::xs) (invFn::invFns) newInput
        | _ -> Ok(List.rev xs,input), invZeroOrMore (List.rev invFns)
    inner s t 


let inline invOneOrMore invSub xs = 
    let x,xsNested = xs
    x + (listToStr invSub xsNested)
let inline oneOrMore p1 = p1 .>>. zeroOrMore p1
   
    
let inline (!*) p1 = zeroOrMore p1    
    
let inline invOptional ( invFn : InvertedParserFunc<'a> ) item : string = invFn (Option.get item)
let optional p1 = 
    let inner input =
        let res,invFn = run p1 input
        match res with 
        | Ok (item, newInput) -> Ok(Some(item), newInput) , invOptional invFn
        | _ -> Ok(None,input), invOptional invFn    
    inner
    
let inline (!?) p1 = optional p1    

let inline (!+) p1 = oneOrMore p1

let inline (<?>) p1 p2 = orElse p1 p2

let inline invSat item = sprintf "%c" item
let sat pred expected note =
    let inner state =
       let item,newState = Input.next state
       match item with 
        |Some unwItem -> 
           if pred unwItem then Ok (unwItem, newState), invSat
           else Fail( [(state,newState,note,expected)] ),defMFun
        |None -> Fail(  [(state,newState, note, expected)] ), defMFun
    inner



let satAnyLetter = sat System.Char.IsLetter
let satAnyDigit = sat System.Char.IsDigit
let satSpecific c = sat c.Equals  

let merge p1 =
    let inner input =
        let res,invFn = run p1 input
        match res with
        | Ok(item, newInput) -> 
            let invItem = invFn item
            Ok(invItem, newInput), fun _ -> invItem
        | Fail(info) -> Fail(info), defMFun
    inner



let genError info whereBegin note expected =
    let (_,whereEnd,_,_) = List.head info
    (whereBegin,whereEnd,note,expected)

let rootError p1 (note, expected) =
    let inner input =
        let res, invFn = run p1 input
        match res with
        | Ok(item,newInput) -> Ok(item,newInput), invFn
        | Fail(info) -> Fail( (genError info input note expected)::info ) , invFn
    inner


// The line and the unexpected word can be computed using whereBegin and whereEnd... 
let rootErrorReplace p1 (note,expected) =
    let inner input =
        let res, invFn = run p1 input
        match res with
        | Ok(item,newInput) -> Ok(item,newInput), invFn
        | Fail(info) -> Fail( [genError info input note expected] ), invFn
    inner

// Also include filename?
let emitError (info : ErrorInformation list) =
    let x::xs = List.rev info
    printfn "========== Syntax Error =========="
    let (whereBegin , whereEnd , noteOption , expected) = x
    let note = match noteOption with | Some(item) -> sprintf "Note: %s" item | None -> ""
    let offendingString = Input.getRange whereBegin.text whereBegin.pos whereEnd.pos
    let offendingLine = Input.getLine whereEnd
    let caret = (String.init (whereEnd.pos-whereBegin.pos) (fun _ -> "~")) + "^" + "Expecting " + offendingString
    printfn "Error at line %i, column %i: Expected %s, but got %s\n---\n%s\n%s\n%s" whereBegin.lineNo whereEnd.colNo expected offendingString offendingLine caret note
    List.iter (fun (x : ErrorInformation) -> 
        let (whereBegin , whereEnd , noteOption , expected) = x
        let note = match noteOption with | Some(item) -> sprintf "Note: %s" item | None -> ""
        printfn "when parsing %s, starting at line %i:column %i, ending at line %i:column %i.\n%s" expected whereBegin.lineNo whereBegin.colNo whereEnd.lineNo whereEnd.colNo note) xs
    printfn "========== ------------ =========="
    let (_,whereEnd,_,_) = (List.head info)
    whereEnd

// At first no skipping, just plain "go one step ahead".
let recover p1 =
    let inner input =
        let res, invFn = run p1 input
        match res with
        | Ok(item,newInput) -> Ok(item,newInput),invFn
        | Fail(info) -> 
            let newInput = emitError info
            
    inner

    
