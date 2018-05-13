module SpecificCombinators

open Combinators
open Error

let satAnyLetter = sat System.Char.IsLetter "a letter" None
let satAnyDigit = sat System.Char.IsDigit "a digit" None
let satSpecific c = sat c.Equals (c.ToString()) None
let satSpaces = sat System.Char.IsWhiteSpace "a whitespace" None

let dec<'a>() =
    let impl = ref Unchecked.defaultof<Parser<'a>>
    let actual = {fn = fun x-> run ! impl x}
    actual,impl

