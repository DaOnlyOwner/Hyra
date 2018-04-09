#pragma once

#include <vector>
#include "Visitor.h"

class GraphVizPrinter : Visitor
{
public:
    void Start(AstNode& node);
    void Visit(Ast::LiteralExpr& expr) override;
    void Visit(Ast::BinaryExpr& expr) override;
    void Visit(Ast::UnaryExpr& expr) override;
    void Visit(Ast::InvalidExpr& expr) override;

private:
    void PreVisit() override;
    void PostVisit() override;

private:
    std::string outCode;
    uint64_t uniqueCounter = 0;
    std::string nodeDefs;
    void addToNodeDefs(const TokenInformation &info, uint64_t counter);
};