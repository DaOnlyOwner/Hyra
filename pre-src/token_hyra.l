%top{
    #include "TokenDefinitions.h"
    #define RET_TOKEN(token) return Tokens:: token
%}

%class{
TokenInformation peekTokenInfo;
public:
TokenInformation Eat()
{
    auto tmp = peekTokenInfo;
    auto peekToken = lex();
    peekTokenInfo = {peekToken, str(), lineno(), columno()};
    return tmp;
}
const TokenInformation& Peek()
{
    return peekTokenInfo;
}
void Init()
{
    Eat();
}
%}

%o fast tabs=8 dotall noline header-file="../src/HyraLexer.h" outfile="../src/HyraLexer.cpp" lexer=HyraLexer
%%
^\h+
\j                               RET_TOKEN(BlockEnd);
if                               RET_TOKEN(KW_if);
elif                             RET_TOKEN(KW_elif);
else                             RET_TOKEN(KW_else);
while                            RET_TOKEN(KW_while);
and                              RET_TOKEN(KW_and);
or                               RET_TOKEN(KW_or);
xor                              RET_TOKEN(KW_xor);
not                              RET_TOKEN(KW_not);
u8                               RET_TOKEN(KW_u8);
u16                              RET_TOKEN(KW_u16);
u32                              RET_TOKEN(KW_u32);
u64                              RET_TOKEN(KW_u64);
f8                               RET_TOKEN(KW_f8);
f16                              RET_TOKEN(KW_f16);
f32                              RET_TOKEN(KW_f32);
s8                               RET_TOKEN(KW_s8);
s16                              RET_TOKEN(KW_s16);
s32                              RET_TOKEN(KW_s32);
s64                              RET_TOKEN(KW_s64);
"+"                              RET_TOKEN(OP_plus);
"-"                              RET_TOKEN(OP_minus);
"*"                              RET_TOKEN(OP_times);
"/"                              RET_TOKEN(OP_div);
==                               RET_TOKEN(OP_equals);
!=                               RET_TOKEN(OP_notEquals);
<=                               RET_TOKEN(OP_lesserThan);
>=                               RET_TOKEN(OP_greaterThan);
=                                RET_TOKEN(OP_assign);
:                                RET_TOKEN(SYM_colon);
[0-9]+\.[0-9]+(f8|f16|f32)?      RET_TOKEN(LIT_float);
[0-9]+(i64|i32|i16|i8)?          RET_TOKEN(LIT_integer);
[0-9]+(f8|f16|f32)?              RET_TOKEN(LIT_float);
[0-9a-zA-Z_]+                    RET_TOKEN(Identifier);
^\h*\i                           RET_TOKEN(BlockStart);
^\h*\j                           RET_TOKEN(BlockEnd);
"("                              RET_TOKEN(SYM_lparen);
")"                              RET_TOKEN(SYM_rparen);
<<EOF>>                          RET_TOKEN(EOF_);
%%
