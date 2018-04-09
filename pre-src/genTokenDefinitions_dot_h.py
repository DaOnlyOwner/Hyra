#import io

#def writeFile(namespaceName, tokens, tokenInformationVars):
#    outText = genAll(namespaceName,tokens,tokenInformationVars)
#    with open("../src/TokenDefinitions.h","w") as f:
#        f.write(outText)
#def genAll(namespaceName, tokens, tokenInformationVars):
#    return genTop() + genNamespace(namespaceName, tokens) + "\n" + genTokenInformation(tokenInformationVars)

#def genTop():
#    return "#pragma once\n#include <string>\n#include <map>\n"

#def genTokenInformation(vars):
#    out = "struct TokenInformation{\n"
#    for var in vars:
#        out += var + ";\n"
#    out += "};"
#    return out


#def genNamespace(name,tokens):
#    out = "namespace " + name +"{\ntypedef int Type;\n" + \
#          genEnum(tokens) + \
#          genMapBack(tokens) +"\n}\n"
#    return out

def genEnum(tokens):
    out = "enum {\n"
    for index,token in enumerate(tokens[0:-1]):
        out += "{0}={1},\n".format(token,index)
    out += tokens[-1] + "="+str(len(tokens)-1) +  "};\n"
    return out

def genMapBack(tokens):
    out = "inline std::string GetName(int type){static std::map<int,std::string> namesMap{\n"
    for token in tokens[0:-1]:
        out += "{"+token+', "'+token+'"'+"},\n"
    out += "{"+tokens[-1]+', "'+tokens[-1]+'"'+"}\n"
    out+="}; return namesMap[type]; }\n"
    return out

tokenList = [
"KW_if",
"KW_elif",
"KW_else",
"KW_while",
"KW_and",
"KW_or",
"KW_xor",
"KW_not",
"KW_u8",
"KW_u16",
"KW_u32",
"KW_u64",
"KW_f8",
"KW_f16",
"KW_f32",
"KW_s8",
"KW_s16",
"KW_s32",
"KW_s64",
# Arithmetic Operators
"OP_plus",
"OP_minus",
"OP_times",
"OP_div",
# Logical Operators
"OP_equals",
"OP_notEquals",
"OP_lesserThan",
"OP_greaterThan",
"OP_assign",


# Symbols
"SYM_colon",
"SYM_lparen",
"SYM_rparen",
# Literals
"LIT_integer",
"LIT_float",

# Other stuff
"Identifier",
"BlockStart",
"BlockEnd",
"EOF_"
]

name = "Tokens"
vars = ["Tokens::Type TokenType",
"std::string  Match",
"size_t LineNumber",
"size_t ColumnNumber"]

print(genEnum(tokenList))