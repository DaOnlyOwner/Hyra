//
// Created by root on 06.04.18.
//

#include "TreeVisualizer.h"
#include <fstream>


void GraphVizPrinter::Visit(Ast::LiteralExpr& expr)
{
    uniqueCounter++;
    addToNodeDefs(expr.TokenInfo,uniqueCounter);
}


void GraphVizPrinter::Visit(Ast::BinaryExpr& expr)
{
    uniqueCounter++;
    uint64_t myName = uniqueCounter;
    addToNodeDefs(expr.TokenInfo,myName);
    uint64_t nameLhs = uniqueCounter+1;
    expr.Lhs->AcceptCustom(*this);
    uint64_t nameRhs = uniqueCounter+1;
    expr.Rhs->AcceptCustom(*this);
    outCode += std::to_string(myName) + "->" + std::to_string(nameLhs) + "\n";
    outCode += std::to_string(myName) + "->" + std::to_string(nameRhs) + "\n";
}

void GraphVizPrinter::Visit(Ast::UnaryExpr& expr)
{
    uniqueCounter++;
    uint64_t myName = uniqueCounter;
    addToNodeDefs(expr.TokenInfo,myName);
    uint64_t nameMid = uniqueCounter+1;
    expr.Mid->AcceptCustom(*this);
    outCode += std::to_string(myName) + "->" + std::to_string(nameMid) + "\n";
}

void GraphVizPrinter::PreVisit()
{

}

void GraphVizPrinter::PostVisit()
{
    std::ofstream outFile("visualized_ast.dot");
    outFile << "strict digraph {\n" + outCode + nodeDefs + "}";
    outFile.close();
}

void GraphVizPrinter::Visit(Ast::InvalidExpr &expr)
{
    uniqueCounter++;
    addToNodeDefs(expr.TokenInfo,uniqueCounter);
}

void GraphVizPrinter::Start(AstNode &node)
{
    PreVisit();
    node->AcceptCustom(*this);
    PostVisit();
}

void GraphVizPrinter::addToNodeDefs(const TokenInformation& info, uint64_t counter)
{
    nodeDefs += std::to_string(counter) + "[label=\"" + Tokens::GetName(info.TokenType) + "/" + info.Match + "\"];\n";
}
