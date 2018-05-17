module HyraParser

open Parser

let pIdentifier = (pLetter <?> pCh '_') .>>. !* (pDigit <?> pLetter <?> pCh '_')
let pInt = (!+ pDigit) .>>. choice (List.map (!%) ["u32";"u64";"u16";"u8"; "s8"; "s16"; "s32"; "s64"; "u";"s"])
let pFloat = (!* pDigit) .>>. (pCh '.') .>>. (!+ pDigit) .>>. !? (choice (List.map (!%) ["f";"d"] ))

let pExpr = !% "expr"  
let pNestedExpr = (pCh '(') >/>. pExpr .>> (pCh ')')
