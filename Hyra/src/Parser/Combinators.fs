module Combinators

type Line = uint64
type Column = uint32

type Result<'a> = 
    | Good of 'a * Input.InputState
    | Bad

type Parser<'a> = {
    fn : Input.InputState -> Result<'a>
    } 

                
   
let bind (parser : Parser<'a>)  (func : 'a -> Parser<'b>) : Parser<'b> =
    let inner input =
        match parser.fn input with 
        | Good (item, newInput) -> newInput |> (item |> func).fn 
        | Bad -> Bad
    {fn = inner}

let (>>=) p f = bind p f

let ret (a :'a) =
    let inner input =
        Good (a,input)
    {fn=inner}    
    
let run parser state =
    parser.fn state

type ParserBuilder() =
    member this.Bind (parser,func) =
        bind parser func 
    member this.Return a = ret a
         
        
let parser = new ParserBuilder()    


let andThen p1 p2 = parser {
    let! res1 = p1
    let! res2 = p2
    return (res1,res2)
    }

let sat pred =
     let inner state =
        let item,newState = Input.next state
        match item with 
        | Some unwItem -> 
            if pred item then Good (unwItem, newState)
            else Bad
        | None -> Bad
     {fn=inner}

