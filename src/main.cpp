#include "HyraLexer.h"
#include <cstdio>
int main()
{
    HyraLexer hyraLexer;
    int counter = 10;
    while(hyraLexer.Peek().TokenType != Tokens::EOF_ && counter-- != 0)
    {
        TokenInformation ti = hyraLexer.Eat();
        printf("%s: %s\n", ti.Match.c_str(), Tokens::GetName(ti.TokenType).c_str());
    }
    return 0;
}