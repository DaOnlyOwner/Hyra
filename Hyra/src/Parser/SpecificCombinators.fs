module SpecificCombinators

open Combinators


// like C: [a-zA-Z][a-zA-Z1-9_]*
let satIdentifier = satAnyLetter .>>. !* ( satAnyLetter <?> satAnyDigit <?> satSpecific '_' ) |> merge
