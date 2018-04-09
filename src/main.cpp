#include "HyraParser.h"
#include "TreeVisualizer.h"
#include <iostream>
#include <fstream>
#include <cstdio>

int main()
{
    std::stringstream stream;
    stream << std::ifstream("testprogram.hy").rdbuf();
    std::string in = stream.str();
    reflex::Input input{in};
    HyraLexer lexer(input);
    HyraParser par{std::move(lexer)};
    auto ast = par.Parse();
    GraphVizPrinter printer;
    printer.Start(ast);
    return 0;
}