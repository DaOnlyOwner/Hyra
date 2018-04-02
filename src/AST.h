#pragma once

#include <memory>
#include <string>
#include "stdio.h"
#include "TokenDefinitions.h"

#define MAKE_NODE(type, args...) std::make_unique< type >(args)
#define NODE_TYPE(type) std::unique_ptr< type >
#define REDIR_NODE(n1, n2) n1 = std::move( n2 )

namespace Ast {


    struct Node {
        Tokens::Type TokenType;
        virtual ~Node() = default;
    };

    struct Expr : Node
    {

    };

    struct InvalidExpr : Expr
    {

    };

    struct BinaryExpr : Expr
    {
        NODE_TYPE(Expr) Lhs, Rhs;
    };

    struct UnaryExpr : Expr
    {
        NODE_TYPE(Expr) Mid;
    };

    struct LiteralExpr : Expr {
        std::string Literal;
    };
}
