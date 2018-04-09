#pragma once

#include "AST.h"

class Visitor
{
public:
    virtual void Visit(Ast::LiteralExpr&) = 0;
    virtual void Visit(Ast::BinaryExpr&) = 0;
    virtual void Visit(Ast::UnaryExpr&) = 0;
    virtual void Visit(Ast::InvalidExpr&) = 0;

    virtual void PreVisit() = 0;
    virtual void PostVisit() = 0;
};