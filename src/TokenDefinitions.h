#pragma once

#include <string>
#include <map>
#define BIN_OP_LOWER_LIMIT 20
#define BIN_OP_UPPER_LIMIT 28
#define LIT_LOWER_LIMIT 32
#define LIT_UPPER_LIMIT 33


namespace Tokens {
    typedef int Type;
    enum {
        KW_if=0,
        KW_elif=1,
        KW_else=2,
        KW_for=3,
        KW_while=4,
        KW_and=5,
        KW_or=6,
        KW_xor=7,
        KW_not=8,
        KW_u8=9,
        KW_u16=10,
        KW_u32=11,
        KW_u64=12,
        KW_f8=13,
        KW_f16=14,
        KW_f32=15,
        KW_s8=16,
        KW_s16=17,
        KW_s32=18,
        KW_s64=19,
        OP_plus=20,
        OP_minus=21,
        OP_times=22,
        OP_div=23,
        OP_equals=24,
        OP_notEquals=25,
        OP_lesserThan=26,
        OP_greaterThan=27,
        OP_assign=28,
        SYM_colon=29,
        SYM_lparen=30,
        SYM_rparen=31,
        LIT_integer=32,
        LIT_float=33,
        Identifier=34,
        BlockStart=35,
        BlockEnd=36,
        EOF_=37};
    enum class Assoc
    {
        Left,Right
    };

    inline bool IsBinOp(Tokens::Type t)
    {
        // This is always subject to change.
        return t>= BIN_OP_LOWER_LIMIT && t<=BIN_OP_UPPER_LIMIT;
    }

    inline int GetPrec(Tokens::Type t)
    {
        if (t == Tokens::OP_plus) return 4;
        else if (t == Tokens::OP_minus) return 4;
        else if (t == Tokens::OP_times) return 5;
        else if (t == Tokens::OP_div)   return 5;
        else if (t == Tokens::OP_equals) return 2;
        else if (t == Tokens::OP_notEquals) return 2;
        else if (t == Tokens::OP_assign) return 1;
    }

    inline Assoc GetAssoc(Tokens::Type t)
    {
        return Assoc::Left;
    }

    inline bool IsLit(Tokens::Type t)
    {
        return t>=LIT_LOWER_LIMIT && t<=LIT_UPPER_LIMIT;
    }


    inline std::string GetName(int type){static std::map<int,std::string> namesMap{
                {KW_if, "KW_if"},
                {KW_elif, "KW_elif"},
                {KW_else, "KW_else"},
                {KW_for, "KW_for"},
                {KW_while, "KW_while"},
                {KW_and, "KW_and"},
                {KW_or, "KW_or"},
                {KW_xor, "KW_xor"},
                {KW_not, "KW_not"},
                {KW_u8, "KW_u8"},
                {KW_u16, "KW_u16"},
                {KW_u32, "KW_u32"},
                {KW_u64, "KW_u64"},
                {KW_f8, "KW_f8"},
                {KW_f16, "KW_f16"},
                {KW_f32, "KW_f32"},
                {KW_s8, "KW_s8"},
                {KW_s16, "KW_s16"},
                {KW_s32, "KW_s32"},
                {KW_s64, "KW_s64"},
                {OP_plus, "OP_plus"},
                {OP_minus, "OP_minus"},
                {OP_times, "OP_times"},
                {OP_div, "OP_div"},
                {OP_equals, "OP_equals"},
                {OP_notEquals, "OP_notEquals"},
                {OP_lesserThan, "OP_lesserThan"},
                {OP_greaterThan, "OP_greaterThan"},
                {OP_assign, "OP_assign"},
                {SYM_colon, "SYM_colon"},
                {SYM_lparen, "SYM_lparen"},
                {SYM_rparen, "SYM_rparen"},
                {LIT_integer, "LIT_integer"},
                {LIT_float, "LIT_float"},
                {Identifier, "Identifier"},
                {BlockStart, "BlockStart"},
                {BlockEnd, "BlockEnd"},
                {EOF_, "EOF_"}
        }; return namesMap[type]; }

    inline bool IsCF(Type type)
    {
        return false;
    }

    inline bool IsDec_Def()
    {
        return false;
    }
}

struct TokenInformation {
    Tokens::Type TokenType;
    std::string Match;
    size_t LineNumber;
    size_t ColumnNumber;
};