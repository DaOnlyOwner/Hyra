#include "AST.h"
#include "Visitor.h"

#define DEF_NODE(name, def) void name :: Accept(Visitor & v) { def } void name :: AcceptCustom(Visitor& v) { v.Visit(*this); }


namespace Ast
{
    DEF_NODE(UnaryExpr,
    v.Visit(*this);
    Mid->Accept(v);
    )

    DEF_NODE(BinaryExpr,
    v.Visit(*this);
    Lhs->Accept(v);
    Rhs->Accept(v);
    )
    DEF_NODE(LiteralExpr, v.Visit(*this);)
    DEF_NODE(InvalidExpr, v.Visit(*this);)
};