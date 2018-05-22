module Ast

type Literal =
    | Int of string
    | Float of string

type Expr =
    | Literal of Literal
    | Ident of string
    | NestedExpr of Expr 
    | BinaryExpr of Expr * string * Expr
    | PrefixExpr of string * Expr
    | PostfixExpr of Expr * string
    | Invalid 
