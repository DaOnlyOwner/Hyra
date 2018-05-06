module Input
open System

type LineNo = uint64
type ColumnNo = uint32
type Lines = string[]

type InputState =  {
    lines:string[]
    lineNo: int
    colNo: int
}

let inputFromFile file =
    let lines = System.IO.File.ReadAllLines file 
    {lines=lines; lineNo=0; colNo=0}

let extract (state : InputState) =
    if state.lineNo >= state.lines.Length then
        None
    else
        assert (state.colNo < state.lines.[state.lineNo].Length)
        Some state.lines.[state.lineNo].[state.colNo]
        

let incr (state : InputState) =
    if state.lineNo >= state.lines.Length then state
    elif state.colNo >= state.lines.[state.lineNo].Length then {lines=state.lines; lineNo=state.lineNo+1; colNo = 0}
    else {lines=state.lines; lineNo=state.lineNo; colNo = state.colNo+1}
   
let next (state : InputState) =
    (extract state, incr state)
    