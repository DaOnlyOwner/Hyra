module Combinators

type Expected = string
type FailureMessage = string
type Got = string

type PositionInfo = {
    line : string
    lineno : int
    colno : int
}


type ErrorInformation = 
    | Error of Got * Expected * PositionInfo
    | ExtendedError of Got * Expected * PositionInfo * FailureMessage 
  

type Result<'valOnGood> = 
    | Good of 'valOnGood 
    | Bad of ErrorInformation

type Parser<'valOnGood, 'input> = Parser of ('input -> Result<'valOnGood>) 

let run parser input =
    let fn = parser
    fn input
    
    
    
    
    