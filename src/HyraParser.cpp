#include "HyraParser.h"
#include "Error.h"

/* Helper methods */

// Called by pExprChain
inline void makeLhsToNewRoot(AstExpr &lhs, AstExpr &rhs, TokenInformation &&newRootInfo)
{
    auto tmpRoot = std::make_unique<Ast::BinaryExpr>(std::move(newRootInfo));
    tmpRoot->Rhs =  std::move(rhs);
    tmpRoot->Lhs =  std::move(lhs);
    lhs = std::move(tmpRoot);
}

AstNode HyraParser::Parse()
{
    return pExprChain(pAtomExpr(),1);
}
// <literal> | '('<ExprChain>')'
AstExpr HyraParser::pAtomExpr() {
    const TokenInformation& la = peek();
    if (Tokens::IsLit(la.TokenType) || la.TokenType != Tokens::SYM_lparen)
    {
        return std::make_unique<Ast::LiteralExpr>(eat());
    }

    if (la.TokenType == Tokens::SYM_lparen)
    {
        eat();
        auto nestedExpr = pExprChain(pAtomExpr(),1);
        if (peek().TokenType != Tokens::SYM_rparen)
        {
            printf("Syntax Error in line %d, column %d: Expected a closing parenthesis");
            return std::make_unique<Ast::InvalidExpr>(eat());
        }
        else {eat(); return nestedExpr; }
    }
    printf("Syntax Error in line %d, column %d: Expected an atom (a literal or a parenthesized expression), but got %s",la.LineNumber,la.ColumnNumber,la.Match.c_str());
    return std::make_unique<Ast::InvalidExpr>(eat());
}

/*  <Atom> {<op> <ExprChain>} (* every precedence of op is the same, <Atom> is already parsed *)
 *  The grammar is not implemented as it is presented here, <Atom> is parsed in the caller and passed down
 *  This solution only recurses when it needs.
 *
 */
 AstExpr HyraParser::pExprChain(AstExpr lhs, int minPrec)
 {
     Tokens::Type currentOperatorType = peek().TokenType;
     AstExpr currentAtom;
     while (Tokens::IsBinOp(currentOperatorType))
     {
         TokenInformation currentOperator = eat();
         currentAtom = pAtomExpr();
         Tokens::Type peekOperatorType = peek().TokenType;
         if (Tokens::GetPrec(peekOperatorType) > minPrec)
         {
             /* Parse laType as an ExprChain with param lhs = a;
              * The result is the rhs. We construct a new binary tree with lhs and rhs as the corresponding children
              */
             auto rhs = pExprChain(std::move(currentAtom), Tokens::GetPrec(peekOperatorType));
             makeLhsToNewRoot(lhs, rhs, std::move(currentOperator));
         }
         else if (Tokens::GetPrec(peekOperatorType) < minPrec) { makeLhsToNewRoot(lhs,currentAtom,std::move(currentOperator)); return lhs; }
         else
         {
             if (Tokens::GetAssoc(peekOperatorType) == Tokens::Assoc::Right)
             {
                 // e.g. A ** B ** C == A ** (B ** C)
                 AstExpr rhs = pExprChain(std::move(currentAtom), minPrec);
                 makeLhsToNewRoot(lhs, rhs, std::move(currentOperator));
             }
             else
             {
                 // Okay, the last atom belongs to this chain. Add it to the tree as a rhs
                 makeLhsToNewRoot(lhs, currentAtom, std::move(currentOperator));
             }
         }
         currentOperatorType = peek().TokenType;
     }
     return lhs;
 }

/*
 * Stmts
 */

// <ControlFlowStmt> | <StmtExpression> | <Dec_DefStmt>
AstStmt HyraParser::pStmt()
{
    Tokens::Type peekType = peek().TokenType;
    if (Tokens::IsCF(peekType)) return pControlFlowStmt();
    else if (Tokens::IsDec_Def()) return pDec_DefStmt();
    else return pStmtExpression();
}

// <IfBlock> | <ForBlock> | <WhileBlock>
AstStmt HyraParser::pControlFlowStmt()
{
    TokenInformation cf = peek();
    switch (cf.TokenType)
    {
        case Tokens::KW_if:
            return pIfStmt();
        case Tokens::KW_while:
            //return pWhileBlock();
        case Tokens::KW_for:
            //return pForBlock();
        default:
            printf("This shouldnt have happended-.-");
    }
}
// "if" <ExprChain>":" <BlockStart> <Stmt>+ <BlockEnd> <ElifStmt>* ("else" ":" <BlockStart> <Stmt>* <BlockEnd>?
// TODO: Don't require a new block, it would be nice if I could do something like this: if A: doB() [no newline here]
AstStmt HyraParser::pIfStmt()
{
    eat(); // Eat the 'if'
    Ast::IfStmt ifStmt;
    auto condition = pExprChain(pAtomExpr(), 1);
    ifStmt.Condition = std::move(condition);
    const TokenInformation &colonMaybe = peek();
    if (colonMaybe.TokenType != Tokens::SYM_colon)
    {
        printf("Syntax Error in line %d, column %d: Expected a colon after the 'if' keyword.", colonMaybe.LineNumber,
               colonMaybe.ColumnNumber);
        assert(false);
    }
    eat(); // Eat the colon
    const TokenInformation &blockStartMaybe = peek();
    if (blockStartMaybe.TokenType != Tokens::BlockStart)
    {
        printf("#### SYNTAX ERROR #### in line %lli, column %lli: Wrong intendation or no other symbol to start a new block.\n#### ----- ####");
    }
    eat(); // Eat the BlockStart
    ifStmt.Body.push_back(pStmt());
    while (peek().TokenType != Tokens::BlockEnd)
    {
        ifStmt.Body.push_back(pStmt());
    }
    eat(); // Eat the BlockEnd Token
    const TokenInformation& elif_elseMaybe = peek();
    while (elif_elseMaybe.TokenType == Tokens::KW_elif) ifStmt.ElifStatements.push_back(pElifStmt());
    if (elif_elseMaybe.TokenType == Tokens::KW_else) ifStmt.OptElseStatement = pElseStmt();
    return ifStmt;
}

// "elif"|"else" <ExprChain>":" <BlockStart> <Stmt>* <BlockEnd>
AstStmt HyraParser::pBasicBranchStmt(Tokens::Type type)
{
    const TokenInformation& elifMaybe = peek();
    UPtr<Ast::BasicBranchStmt> stmt = std::make_unique();
    if (peek().TokenType != type)
    {
        compilerBug(elifMaybe, "Should not appear on your screen. 'Elif' is optional");
        exit(-1);
    }
    eat(); // Eat the elif kw
    stmt->Condition = pExprChain(pAtomExpr(),1);
    if(peek().TokenType != Tokens::SYM_colon)
    {
        minorError(peek(), ":", "The colon marks the start of a new block (a new scope)");
    }
    else eat(); // Eat the colon

    if(peek().TokenType != Tokens::BlockStart)
    {
        indentError(peek());
    }
    else eat(); // Eat the block start token
    while(peek().TokenType == Tokens::BlockEnd) stmt.Body.push_back(pStmt());
    eat(); // Eat the block end token.
}

// "def" <FuncPrototype> <BlockStart> <Stmt>* <BlockEnd>
AstStmt HyraParser::pFuncDef()
{
    return AstStmt();
}

// "dec" <FuncPrototype> ";"
AstStmt HyraParser::pFuncDec()
{

}

// <Identifier> <ArgumentList>
AstStmt HyraParser::pFuncPrototype()
{

}

AstStmt HyraParser::pDec_DefStmt()
{
    return AstStmt();
}

AstStmt HyraParser::pStmtExpression()
{
    return AstStmt();
}

