#pragma once

#include <memory>
#include <string>
#include <vector>
#include <boost/optional.hpp>
#include "TokenDefinitions.h"



#define DEC_NODE(name, parent) struct name : parent { name ()=default; name (TokenInformation&& info) : Node(std::move(info)){} void Accept(Visitor&) override; void AcceptCustom(Visitor&) override; }
#define DEC_NODE_CUSTOM(name, parent, def) struct name : parent { name ()=default; name (TokenInformation&& info) : Node(std::move(info)){} void Accept(Visitor&) override; void AcceptCustom(Visitor&) override; def };

template<typename T>
using UPtr = std::unique_ptr<T>;
class Visitor;
namespace Ast
{
    enum InvalidType
    {
        Expression,
        Statement
    };

    struct Node
    {
        TokenInformation TokenInfo;

        Node() = default;
        Node(TokenInformation &&info) : TokenInfo(std::move(info)) {}
        virtual ~Node() = default;
        virtual void Accept(Visitor &visitor) = 0;
        virtual void AcceptCustom(Visitor &visitor) = 0;
    };
    struct InvalidNode : Node
    {
        InvalidNode(TokenInformation &&info, InvalidType type) : Node(std::move(info)), Type(type) {}
        InvalidType Type;
    };
    DEC_NODE(Expr, Node);
    DEC_NODE(Stmt, Node);
    DEC_NODE_CUSTOM(BasicBranchStmt, Stmt, UPtr<Expr> Condition; std::vector<UPtr<Stmt>> Body;);
    DEC_NODE_CUSTOM(IfStmt, Stmt, UPtr<Expr> Condition; std::vector<UPtr<Stmt>> Body; std::vector<UPtr<BasicBranchStmt>> ElifStatements; boost::optional<BasicBranchStmt> OptElseStatement;)

    DEC_NODE(LiteralExpr, Expr);
    DEC_NODE_CUSTOM(BinaryExpr, Expr, UPtr<Node> Lhs;
            UPtr<Node> Rhs;)
    DEC_NODE_CUSTOM(UnaryExpr, Expr, UPtr<Node> Mid;)
}
using AstNode = UPtr<Ast::Node>;
using AstExpr = UPtr<Ast::Expr>;
using AstStmt = UPtr<Ast::Stmt>;