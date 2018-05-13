module Combinators

type WhereBegin = Input.InputState
type WhereEnd = Input.InputState
type Note = string option
type Expected = string
type Unexpected = string

type ErrorInformation = WhereBegin * WhereEnd * Note * Expected


type Result<'a> = 
    | Ok of  'a * Input.InputState 
    | Fail of ErrorInformation list    



type InvertedParserFunc<'a> = 'a -> string 

type Parser<'a> = {
    fn:Input.InputState -> (Result<'a> * InvertedParserFunc<'a>)
}

let inline defMFun _ = ""

let inline run parser state =
    parser.fn state
    
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
    {fn=inner}
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
    {fn=inner}

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
    {fn=inner s t} 


let inline invOneOrMore invSub xs = 
    let x,xsNested = xs
    x + (listToStr invSub xsNested)
let inline oneOrMore p1 = p1 .>>. zeroOrMore p1
   
    
let inline (!*) p1 = zeroOrMore p1    

    
let inline invOptional ( invFn : InvertedParserFunc<'a> ) item : string = match item with | Some(unwItem) -> invFn unwItem | None -> ""
let optional p1 = 
    let inner input =
        let res,invFn = run p1 input
        match res with 
        | Ok (item, newInput) -> Ok(Some(item), newInput) , invOptional invFn
        | _ -> Ok(None,input), invOptional invFn    
    {fn=inner}
    
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
    {fn=inner}

let merge p1 =
    let inner input =
        let res,invFn = run p1 input
        match res with
        | Ok(item, newInput) -> 
            let invItem = invFn item
            Ok(invItem, newInput), fun _ -> invItem
        | Fail(info) -> Fail(info), defMFun
    {fn=inner}


