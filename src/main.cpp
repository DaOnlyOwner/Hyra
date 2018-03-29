#include "Parser.h"
#include "SourceReader.h"

int main()
{
    Parser<SourceReader> parser(SourceReader{});
    parser.Parse();
}