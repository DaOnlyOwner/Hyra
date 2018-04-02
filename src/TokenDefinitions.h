#pragma once

#include <string>
#include <map>

namespace Tokens {
    enum {
        KW_if,
        KW_elif,
        KW_else,
        KW_while,
        KW_and,
        KW_or,
        KW_xor,
        KW_not,
        KW_u8,
        KW_u16,
        KW_u32,
        KW_u64,
        KW_f8,
        KW_f16,
        KW_f32,
        KW_s8,
        KW_s16,
        KW_s32,
        KW_s64,
        OP_plus,
        OP_minus,
        OP_times,
        OP_div,
        OP_equals,
        OP_notEquals,
        OP_lesserThan,
        OP_greaterThan,
        OP_assign,
        SYM_colon,
        SYM_lparen,
        SYM_rparen,
        LIT_integer,
        LIT_float,
        Identifier,
        BlockStart,
        BlockEnd,
        EOF_
    };

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

    typedef int Type;
}

struct TokenInformation {
    Tokens::Type TokenType;
    std::string Match;
    size_t LineNumber;
    size_t ColumnNumber;
};