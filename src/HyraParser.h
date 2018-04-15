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
    HyraLexer lexer;
    inline TokenInformation eat(){ return lexer.Eat();}
    inline const TokenInformation& peek(){return lexer.Peek();}
private:
    //  Expressions
    AstExpr pExprChain(AstExpr lhs,int minPrec);
    AstExpr pAtomExpr();

    // Stmts
private:

    AstStmt pFuncPrototype();

    AstStmt pFuncDef();

    AstStmt pFuncDec();

    AstStmt pStmt();

    AstStmt pControlFlowStmt();

    AstStmt pDec_DefStmt();

    AstStmt pStmtExpression();

    AstStmt pIfStmt();

    AstStmt pElifStmt();

    AstStmt pBasicBranchStmt(Tokens::Type type);
};
