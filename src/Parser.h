#pragma once

#include "AST.h"
#include "CommonSyntaxNotations.h"

#define PARSER_METHOD(ret, name) template <typename TSrcIterator> NODE_TYPE(ret) Parser<TSrcIterator> :: name

template <typename TSrcIterator>
class Parser
{
public:
    Parser(TSrcIterator&& srcIterator) : provider(std::move(srcIterator)){}
    NODE_TYPE(Ast::Expr) Parse();
// Helpers
private:
    int currentChar;
    int eat()
    {
        return (currentChar = provider.Eat());
    }

    int peek()
    {
        return provider.Peek();
    }

    int current()
    {
        return currentChar;
    }


// Regular 
private:
    OperatorType mrOperators()
    {
        switch (current())
        {
            case '+':
                return Operators::Plus;

            case '*':
                return Operators::Times;
            case '/':
                return Operators::Divide;
            case '-':
                return Operators::Minus;
            default:
                printf("Tried to parse an operator, but got %c", current());
                return Operators::Invalid;
        }
    }
    NODE_TYPE(Ast::LiteralExpr) prNumberLiteralExpr();

// Context-Free
private:
    // Might also return a single leaf
    NODE_TYPE(Ast::Expr) pExpr(int minPrec);

private:
    TSrcIterator provider;
};

PARSER_METHOD(Ast::Expr, Parse)()
{
    auto a = pExpr(1);
    return a;
}


// <Expr>[+ - * / % < > << >> // &  | : and or not == != *= += -= /= %= &= |= ...]<Expr>

// (<Atom><operator>)*<Atom>
PARSER_METHOD(Ast::Expr, pExpr)(int minPrec) {
    NODE_TYPE(Ast::Expr) father = prNumberLiteralExpr();
    OperatorType op = mrOperators();
    // While it has the same precedence, we can merge the whole thing
    int prec, assoc, subexprMinPrec;
    while ((prec = Operators::GetPrec(op)) >= minPrec) {
        assoc = Operators::GetAssoc(op);
        if (assoc == Operators::Assoc::Left) subexprMinPrec = prec + 1;
        else subexprMinPrec = prec;
        auto newFather = MAKE_NODE(Ast::BinaryExpr);
        newFather->Operator = op;
        REDIR_NODE(newFather->Lhs, father);
        REDIR_NODE(newFather->Rhs, pExpr(subexprMinPrec));
        REDIR_NODE(father, newFather);
        op = mrOperators();
    }
    return father;
}
// [0-9]*.?[0-9]+(f|d|(u8)|(u16)|(u32)|(u64)|... (same for s8,s16,...))?
// For now everything is a float.
PARSER_METHOD(Ast::LiteralExpr, prNumberLiteralExpr)()
{
    std::string num;
    while(isdigit(eat()))
    {
        num += current();
    }

    if(current() == '.') num+='.';
    eat();
    if(isdigit(current()))
    {
        do {num += current();}
        while(isdigit(eat()));
    
    }

    // Error checking
    if (num.empty())
    {
        printf("Error: Expected a number!");
        return MAKE_NODE(Ast::LiteralExpr,"",Literals::Invalid);

    }

    return MAKE_NODE(Ast::LiteralExpr,num,Literals::Float);
}