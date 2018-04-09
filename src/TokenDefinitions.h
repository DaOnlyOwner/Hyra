#pragma once

#include <string>
#include <map>
#define BIN_OP_LOWER_LIMIT 19
#define BIN_OP_UPPER_LIMIT 27
#define LIT_LOWER_LIMIT 31
#define LIT_UPPER_LIMIT 32


namespace Tokens {
    typedef int Type;
    enum {
        KW_if=0,
        KW_elif=1,
        KW_else=2,
        KW_while=3,
        KW_and=4,
        KW_or=5,
        KW_xor=6,
        KW_not=7,
        KW_u8=8,
        KW_u16=9,
        KW_u32=10,
        KW_u64=11,
        KW_f8=12,
        KW_f16=13,
        KW_f32=14,
        KW_s8=15,
        KW_s16=16,
        KW_s32=17,
        KW_s64=18,

        //-- Prec 1
        OP_assign=27,
        //-- Prec 2
        OP_equals=23,
        OP_notEquals=24,

        //-- Prec 3
        OP_lesserThan=25,
        OP_greaterThan=26,
        //-- Prec 4
        OP_plus=19,
        OP_minus=20,

        //-- Prec 5
        OP_times=21,
        OP_div=22,


        SYM_colon=28,
        SYM_lparen=29,
        SYM_rparen=30,

        LIT_integer=31,
        LIT_float=32,

        Identifier=33,
        BlockStart=34,
        BlockEnd=35,
        EOF_=36};

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

    inline std::string GetName(int type) {
        static std::map<int, std::string> namesMap{
                {KW_if,          "KW_if"},
                {KW_elif,        "KW_elif"},
                {KW_else,        "KW_else"},
                {KW_while,       "KW_while"},
                {KW_and,         "KW_and"},
                {KW_or,          "KW_or"},
                {KW_xor,         "KW_xor"},
                {KW_not,         "KW_not"},
                {KW_u8,          "KW_u8"},
                {KW_u16,         "KW_u16"},
                {KW_u32,         "KW_u32"},
                {KW_u64,         "KW_u64"},
                {KW_f8,          "KW_f8"},
                {KW_f16,         "KW_f16"},
                {KW_f32,         "KW_f32"},
                {KW_s8,          "KW_s8"},
                {KW_s16,         "KW_s16"},
                {KW_s32,         "KW_s32"},
                {KW_s64,         "KW_s64"},
                {OP_plus,        "OP_plus"},
                {OP_minus,       "OP_minus"},
                {OP_times,       "OP_times"},
                {OP_div,         "OP_div"},
                {OP_equals,      "OP_equals"},
                {OP_notEquals,   "OP_notEquals"},
                {OP_lesserThan,  "OP_lesserThan"},
                {OP_greaterThan, "OP_greaterThan"},
                {OP_assign,      "OP_assign"},
                {SYM_colon,      "SYM_colon"},
                {SYM_lparen,     "SYM_lparen"},
                {SYM_rparen,     "SYM_rparen"},
                {LIT_integer,    "LIT_integer"},
                {LIT_float,      "LIT_float"},
                {Identifier,     "Identifier"},
                {BlockStart,     "BlockStart"},
                {BlockEnd,       "BlockEnd"},
                {EOF_,           "EOF_"}
        };
        return namesMap[type];
    }

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