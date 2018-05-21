module Input
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

let inputFromStr str = {text = str; pos=0; lineNo =0; colNo=0}

let incr (state : InputState) =
    if state.pos = state.text.Length then state 
    elif state.text.[state.pos] = '\n' then {text=state.text; pos=state.pos+1; lineNo=state.lineNo+1; colNo=0}
    else {text=state.text;pos=state.pos+1; lineNo=state.lineNo; colNo=state.colNo+1}

let extract (state:InputState) =
    if state.pos = state.text.Length then None
    else Some state.text.[state.pos]
   
let next (state : InputState) = (extract state, incr state) 
    
let getRange (text : string) start end_  = text.Substring(start, end_ - start) 

let getPartitionLines (text : string) start end_ =
    let (pos1,pos2) = start.pos, end_.pos
    let eol = text.IndexOf('\n',pos2)
    let intermediate = getRange text pos1 pos2
    let append = text.Substring(pos2, if eol = -1 then text.Length-pos2 else eol-pos2)
    let prepend = text.Substring(pos1-start.colNo,start.colNo)
    (prepend,intermediate,append)

    
