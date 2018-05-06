module Combinators

type Line = uint64
type Column = uint32

type Result<'a> = 
    | Good of 'a * Input.InputState
    | Bad 

type InvertedParserFunc<'a> = 'a -> string 

type Parser<'a> = {
    fn : Input.InputState -> (Result<'a> * InvertedParserFunc<'a>)
    } 

let inline defMFun _ = ""

let inline run parser state =
    parser.fn state
    
let inline invAndThen invSub1 invSub2 (x1,x2) = invSub1 x1 + invSub2 x2 
let andThen p1 p2 = 
    let inner input =
        let res,invFn = run p1 input
        match res with
        | Good(item,newInput) -> 
            let res2,invFn2 = run p2 newInput
            match res2 with
            | Good(item2,newInput2) -> Good( (item,item2), newInput2), invAndThen invFn invFn2
            | Bad -> Bad, defMFun
        | Bad -> Bad, defMFun
    {fn=inner}
let inline (.>>.) p1 p2 = andThen p1 p2

   
let inline invOrElse invSub x = invSub x
let orElse p1 p2 =
    let inner input =
        let res1,invFn1 = run p1 input
        match res1 with 
        | Good(item,newInput) -> Good(item,newInput), invOrElse invFn1
        | Bad -> 
            let res2,invFn2 = run p2 input
            match res2 with 
            | Good(item,newInput) -> Good(item,newInput) , invOrElse invFn2
            | Bad -> Bad, defMFun     
    
    {fn = inner} 

let inline listToStr invSub xs = List.reduce (+) (List.map invSub xs)
let inline invZeroOrMore invSub xs = match xs with | _ when List.isEmpty xs -> "" | _ -> (listToStr invSub xs)
let zeroOrMore p1 =
    let s = List<'a>.Empty
    let rec inner xs input = 
        let res,invFn = run p1 input
        match res with 
        | Bad -> Good(List.rev xs,input), invZeroOrMore invFn 
        | Good (item, newInput)-> inner (item::xs) newInput                
    {fn = inner s} 


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
        | Good (item, newInput) -> Good(Some(item), newInput) , invOptional invFn
        | Bad -> Good(None,input), invOptional invFn    
    {fn = inner}
    
let inline (!?) p1 = optional p1    

let inline (!+) p1 = oneOrMore p1

let inline (<?>) p1 p2 = orElse p1 p2

let inline invSat item = sprintf "%c" item
let sat pred =
    let inner state =
       let item,newState = Input.next state
       match item with 
        |Some unwItem -> 
           if pred unwItem then Good (unwItem, newState), invSat
           else Bad,defMFun
        |None -> Bad, defMFun
    {fn=inner}




let satAnyLetter = sat System.Char.IsLetter
let satAnyDigit = sat System.Char.IsDigit
let satSpecific c = sat c.Equals  

let merge p1 =
    let inner input =
        let res,invFn = run p1 input
        match res with
        | Good(item, newInput) -> 
            let invItem = invFn item
            Good(invItem, newInput), fun _ -> invItem
        | Bad -> Bad, defMFun
    {fn=inner}




    
