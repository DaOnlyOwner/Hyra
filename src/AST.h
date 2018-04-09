#pragma once

#include <memory>
#include <string>
#include "TokenDefinitions.h"
#define DEC_NODE(name) struct name : Node { name (TokenInformation&& info) : Node(std::move(info)){} void Accept(Visitor&) override; void AcceptCustom(Visitor&) override; }
#define DEC_NODE_CUSTOM(name, def) struct name : Node { name (TokenInformation&& info) : Node(std::move(info)){} void Accept(Visitor&) override; void AcceptCustom(Visitor&) override; def };


template<typename T>
using UPtr = std::unique_ptr<T>;
class Visitor;
namespace Ast {
    struct Node {
        TokenInformation TokenInfo;
        Node(TokenInformation&& info) : TokenInfo(std::move(info)){}
        virtual ~Node() = default;
        virtual void Accept(Visitor& visitor) = 0;
        virtual void AcceptCustom(Visitor& visitor) = 0;
    };

    DEC_NODE(LiteralExpr);
    DEC_NODE(InvalidExpr);
    DEC_NODE_CUSTOM(BinaryExpr, UPtr<Node> Lhs; UPtr<Node> Rhs;)
    DEC_NODE_CUSTOM(UnaryExpr, UPtr<Node> Mid;)
}
using AstNode = UPtr<Ast::Node>;
