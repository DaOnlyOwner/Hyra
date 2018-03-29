#pragma once

#include <map>

typedef int OperatorType;
namespace Operators
{
    enum
    {
        Plus,
        Minus,
        Times,
        Divide,
        Invalid
    };

    enum Assoc
    {
        Right, Left
    };

    int GetPrec(OperatorType type)
    {
        static std::map<OperatorType, int> precMap{
                {Operators::Plus,   1},
                {Operators::Minus,  1},
                {Operators::Times,  2},
                {Operators::Divide, 2}
        };
        return precMap[type];
    }

    int GetAssoc(OperatorType type)
    {
        static std::map<OperatorType, Operators::Assoc> assocMap{
                {Operators::Plus,   Operators::Assoc::Right},
                {Operators::Minus,  Operators::Assoc::Right},
                {Operators::Times,  Operators::Assoc::Right},
                {Operators::Divide, Operators::Assoc::Right}
        };
        return assocMap[type];
    }
}

typedef int LiteralType;
namespace Literals
{
    enum
    {
        Float,
        Invalid
    };
}

