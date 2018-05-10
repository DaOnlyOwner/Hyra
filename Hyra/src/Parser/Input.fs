module Input
open System.Collections.Generic
open System
open System.Text

type InputState =  {
    text:string
    pos:int
    lineNo : int
    colNo : int
}

let inputFromFile file =
    let wholeText = System.IO.File.ReadAllText file 
    {text=wholeText; pos=0; lineNo=0; colNo=0}


let incr (state : InputState) =
    if state.pos+1 = state.text.Length then state 
    elif state.text.[state.pos] = '\n' then {text=state.text; pos=state.pos+1; lineNo=state.lineNo+1; colNo=0}
    else {text=state.text;pos=state.pos+1; lineNo=state.lineNo; colNo=state.colNo+1}

let extract (state:InputState) =
    if state.pos+1 = state.text.Length then None
    else Some state.text.[state.pos]
   
let next (state : InputState) = (extract state, incr state) 
    
let getRange (text : string) start end_  = text.Substring(start, end_ - start) 
let getLine input =
    let mutable counter = input.pos
    let strBuilder = new StringBuilder()
    while input.text.[counter] <> '\n' do 
        counter <- counter - 1
        strBuilder.Insert(0, input.text.[counter] ) |> ignore
    
    counter <- input.pos
    while input.text.[counter] <> '\n' do
        counter <- counter + 1
        strBuilder.Append(input.text.[counter] ) |> ignore
    
    strBuilder.ToString()



        
    
