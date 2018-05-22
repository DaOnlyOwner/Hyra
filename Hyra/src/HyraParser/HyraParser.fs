module HyraParser

open Parser
open Ast

// Regular 

let pIdentifier = (pLetter <?> pCh '_') .>>. !* (pDigit <?> pLetter <?> pCh '_') |> merge <!> Expr.Ident
let pInt = (!+ pDigit) .>>. !?(choice (List.map (!%) ["u32";"u64";"u16";"u8"; "s8"; "s16"; "s32"; "s64"; "u";"s"]))  |> merge <!> Literal.Int
let pFloat = (!* pDigit) .>>. (pCh '.') .>>. (!* pDigit) .>>. choice (List.map (!%) ["f";"d"] ) |> merge <!> Literal.Float

let pLiteral =  choice [pFloat;pInt] <!> Expr.Literal


let pExpr,pExprImpl = dec()       
let pNested = (pCh '(') >/>. pExpr .>> (pCh ')') <!> Option.defaultValue Expr.Invalid

let pPrefix = pPlaceholderFail
let pPostfix = pPlaceholderFail

let pPrimary = pSpaces >>. choice[pNested;pPrefix; pPostfix; pIdentifier; pLiteral]


let pExprTemplL opP p = p .>>. !*(opP .>>. p) <!> fun (a1,b)->
    let rec comp xL root =
        match xL with
        | (op,a2)::xs -> comp xs (BinaryExpr(root,op,a2))
        | [] -> root
    comp b a1
       
let rec applyExprTempl opTempls  = 
    match opTempls with
    | x::xs when List.isEmpty xs -> (x pPrimary) 
    | x::xs -> x (applyExprTempl xs)
    | [] -> failwith "Shouldnt happen..."

let staticOps = [ pExprTemplL ( !% "+" <?> !% "-" ); pExprTemplL (!% "*" <?> !% "/")];

let pBinary = applyExprTempl staticOps

pExprImpl := pBinary

 
