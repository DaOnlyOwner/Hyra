module Input

type LineNo = uint64
type ColumnNo = uint32
type Lines = string[]

type InputState =  {
    lines:string[]
    lineNo: int
    colNo: int
}

let inputFromStdin =
    let lines = new ResizeArray<string>()
    let mutable line = ""
    while line <> null do
        line <- System.Console.ReadLine()
        lines.Add(line)
    lines.ToArray()

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
    