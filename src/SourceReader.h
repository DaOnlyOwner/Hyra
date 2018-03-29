#pragma once

#include <string>

class SourceReader {
public:
    int Eat();
    int Peek();
    bool good()
    {
        return where < text.size();
    }
private:
    std::string text = "1+2*3.98";
    int where = 0;
};


