#pragma once

#include <string>
#include "TokenDefinitions.h


enum class ErrorFlag
{
    Minor, // E.g. Programmer forgot to add unecessary syntactical symbols like ':' or ';'. The full semantic meaning can be restored
    Major, // E.g. Programmer made a big syntactic mistake, the context is hard to recover. E.g. wrong intendation,
    Unrecoverable,

};

inline void minorError(const TokenInformation &info, std::string &&expected, std::string &&message)
{
    printf("#### SYNTAX ERROR ####\nIn line %lli, at Token '%s' (column %lli): Expected '%s' instead.\n%s\n#### ---- ####", info.LineNumber,info.Match.c_str(), info.ColumnNumber,
           expected.c_str(),message.c_str());
}

inline void indentError(const TokenInformation& info)
{
    printf("#### SYNTAX ERROR ####\nIn line %lli, at column %lli: Expected indentation that opens a new block.\n#### ---- ####",info.LineNumber,info.ColumnNumber);
}

inline void dedentError(const TokenInformation& info)
{
    printf("#### SYNTAX ERROR ####\nIn line %lli, at column %lli: Expected indentation that closes the current block.\n#### ---- ####",info.LineNumber,info.ColumnNumber);
}


inline void compilerBug(const TokenInformation& info, std::string&& message)
{
    printf("#### COMPILER BUG ####\nIn line %lli, at Token '%s' (column %lli): %s\n#### ---- ####", info.LineNumber,info.Match.c_str(),
           info.ColumnNumber,message.c_str());
}