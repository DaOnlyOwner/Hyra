#pragma once
#include "AST.h"
#include "HyraLexer.h"

class HyraParser
{
public:
    explicit HyraParser(HyraLexer&& lexer_) : lexer(std::move(lexer_)){}
    NODE_TYPE(Ast::Expr) Parse();

    // Helpers
private:
    inline const TokenInformation& eat(){ return lexer.Eat();}
    inline const TokenInformation& peek(){return lexer.Peek();}
    inline const TokenInformation& current(){return lexer.Current();}
// Context-Free
private:
    // Might also return a single leaf
    NODE_TYPE(Ast::Expr) pExprChain(int minPrec);
    NODE_TYPE(Ast::Expr) pAtomExpr();
    NODE_TYPE(Ast::Expr) pParenExpr();
private:
    HyraLexer lexer;
};
