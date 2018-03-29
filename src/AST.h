#pragma once

#include <memory>
#include <string>
#include "CommonSyntaxNotations.h"
#include "stdio.h"

#define MAKE_NODE(type, args...) std::make_unique< type >(args)
#define NODE_TYPE(type) std::unique_ptr< type >
#define REDIR_NODE(n1, n2) n1 = std::move( n2 )

namespace Ast {


    struct Node {
        virtual ~Node() = default;
    };

    struct Expr : Node {

    };

    struct BinaryExpr : Expr
    {
        NODE_TYPE(Expr) Lhs, Rhs;
        OperatorType Operator;
    };

    struct LiteralExpr : Expr {
        LiteralExpr(const std::string &Literal, LiteralType Type) : Literal(Literal), Type(Type) {}
        std::string Literal;
        LiteralType Type;
    };
}
