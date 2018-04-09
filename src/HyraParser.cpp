#include "HyraParser.h"

/* Helper methods */

// Called by pExprChain
inline void makeLhsToNewRoot(AstNode &lhs, AstNode &rhs, TokenInformation &&newRootInfo)
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
AstNode HyraParser::pAtomExpr() {
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
 AstNode HyraParser::pExprChain(AstNode lhs, int minPrec)
 {
     Tokens::Type currentOperatorType = peek().TokenType;
     AstNode currentAtom;
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
                 AstNode rhs = pExprChain(std::move(currentAtom), minPrec);
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
 * Statements
 */

// <ControlFlowStatement> | <StatementExpression> | <Dec_DefStatement>
AstNode HyraParser::pStatement()
{
    Tokens::Type peekType = peek().TokenType;
    if (Tokens::IsCF(peekType)) return pControlFlowStatement();
    else if (Tokens::IsDec_Def()) return pDec_DefStatement();
    else return pStatementExpression();
}

// <IfBlock> | <ForBlock> | <WhileBlock>
AstNode HyraParser::pControlFlowStatement()
{
    TokenInformation cf = eat();
    switch (cf.TokenType)
    {
        case Tokens::KW_if:
            return pIfBlock();
        case Tokens::KW_while:
            return pWhileBlock();
        case Tokens::KW_for:
            return pForBlock();
    }
}
// "if" (* already parsed in the caller *) <ExprChain>":" <BlockStart> <Statement>+ <BlockEnd> ("elif" <ExprChain>":" <BlockStart> <Statement>* <BlockEnd>)* ("else" ":" <BlockStart> <Statement>* <BlockEnd>?
// TODO: Don't require a new block, it would be nice if I could do something like this: if A: doB() [no newline here]
AstNode HyraParser::pIfBlock()
{
     auto condition = pExprChain(pAtomExpr(),1);
     TokenInformation colonMaybe = eat();
     if(colonMaybe.TokenType != Tokens::SYM_colon)
     {
         printf("Syntax Error in line %d, column %d: Expected a colon after the 'if' keyword.", colonMaybe.LineNumber,colonMaybe.ColumnNumber);
     }

}


// "def" <FuncPrototype> <BlockStart> <Statement>* <BlockEnd>
AstNode HyraParser::pFuncDef()
{
    return AstNode();
}

// "dec" <FuncPrototype> ";"
AstNode HyraParser::pFuncDec()
{

}

// <Identifier> <ArgumentList>
AstNode HyraParser::pFuncPrototype()
{

}

AstNode HyraParser::pDec_DefStatement()
{
    return AstNode();
}

AstNode HyraParser::pStatementExpression()
{
    return AstNode();
}

