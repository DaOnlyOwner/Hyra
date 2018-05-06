module Combinators
open Input

type Line = uint64
type Column = uint32

type Result<'a> = 
    | Good of 'a * Input.InputState
    | Bad 

type InvertedParserFunc<'a> = 'a -> string 

type Parser<'a> = {
    fn : Input.InputState -> Result<'a>
    invFn : 'a -> string
    } 

let inline defMFun x = ""

let inline run parser state =
    parser.fn state
  

   
let bind (parser : Parser<'a>)  (func : 'a -> Parser<'b>) : Parser<'b> =
    let inner input =
        let res = run parser input
        match res with 
        | Good (item, newInput) -> run (func item) newInput
        | Bad -> Bad
    {fn = inner}

let inline (>>=) p f = bind p f

let inline ret (a :'a) = {fn=(fun input -> Good(a,input) );}    

type ParserBuilder() =
    member this.Bind (parser,func) =
        bind parser func 
    member this.Return a = ret a
         
        
let parser = new ParserBuilder()    

let andThen p1 p2 = parser {
    let! item1 = p1
    let! item2 = p2
    return (item1,item2)
    }
let inline (.>>.) p1 p2 = andThen p1 p2

   
let orElse p1 p2 =
    let inner input =
        let res1 = run p1 input
        match res1 with 
        | Good(item,newInput) -> Good(item,newInput) 
        | Bad -> 
            let res2 = run p2 input
            match res2 with 
            | Good(item,newInput) -> Good(item,newInput) 
            | Bad -> Bad            
    
    {fn = inner} 

let zeroOrMore p1 =
    let s = List<'a>.Empty
    let rec inner xs input = 
        let res = run p1 input
        match res with 
        | Bad -> Good(List.rev xs,input)
        | Good (item, newInput)-> inner (item::xs) newInput                
    {fn = inner s} 

let inline oneOrMore p1 = p1 .>>. zeroOrMore p1 
    
   
    
let inline (!*) p1 = zeroOrMore p1    
    
let optional p1 = 
    let inner input =
        let res = run p1 input
        match res with 
        | Good (item, newInput) -> Good(Some(item), newInput)   
        | Bad -> Good(None,input)
            
    {fn = inner}
    
let inline (!?) p1 = optional p1    

let inline (!+) p1 = oneOrMore p1

let inline (<?>) p1 p2 = orElse p1 p2

let inline invSat item = sprintf "%c" item
let sat pred =
     let inner state =
        let item,newState = Input.next state
        match item with 
        | Some unwItem -> 
            if pred unwItem then Good (unwItem, newState)
            else Bad
        | None -> Bad
     {fn=inner; invFn = invSat}




let satAnyLetter = sat System.Char.IsLetter
let satAnyDigit = sat System.Char.IsDigit
let satSpecific c = sat (fun cIn -> c.Equals(cIn))  


let inline listToStr xs invSub = List.reduce (+) (List.map (fun x -> invSub x) xs)
let inline invOneOrMore xs invSub = 
    let x,xsNested = xs
    x + (listToStr xsNested invSub)
    
let inline invZeroOrMore xs invSub = match xs with | _ when List.isEmpty xs -> "" | _ -> (listToStr xs invSub)
let inline invOptional x invSub = Option.get (Option.map invSub x)
let inline invOrElse x invSub = invSub x
let inline invAndThen (x1,x2) (invSub1,invSub2) = invSub1 x1 + invSub2 x2 
