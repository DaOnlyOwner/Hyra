#include "HyraParser.h"

NODE_TYPE(Ast::Expr) HyraParser::pAtomExpr() {
    return std::unique_ptr<Ast::Expr>();
}

NODE_TYPE(Ast::Expr) HyraParser::pParenExpr() {
    return std::unique_ptr<Ast::Expr>();
}

/* <ExprChain> = <Atom> ({<op> <ExprChain>})
*/
 NODE_TYPE(Ast::Expr) HyraParser::pExprChain(int minPrec) {
     return std::unique_ptr<Ast::Expr>();
}