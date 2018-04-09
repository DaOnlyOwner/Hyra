#pragma once
#include "AST.h"
#include "HyraLexer.h"

class HyraParser
{
public:
    explicit HyraParser(HyraLexer&& lexer_) : lexer(std::move(lexer_)){ lexer.Init(); }
    AstNode Parse();

    // Helpers
private:
    inline TokenInformation eat(){ return lexer.Eat();}
    inline const TokenInformation& peek(){return lexer.Peek();}
private:
    //  Expressions
    AstNode pExprChain(AstNode lhs,int minPrec);
    AstNode pAtomExpr();
    // Statements
    AstNode pFuncPrototype();
    AstNode pFuncDef();

private:
    HyraLexer lexer;

    AstNode pFuncDec();

    AstNode pStatement();

    AstNode pControlFlowStatement();

    AstNode pDec_DefStatement();

    AstNode pStatementExpression();

    AstNode pControlFlowStatements();

    AstNode pIfBlock();
};
