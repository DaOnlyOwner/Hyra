#pragma once

#include <string>

class SourceReader {
public:
    int Eat();
    int Peek();
    int Current();
    bool good()
    {
        return where+1 < text.size();
    }
private:
    std::string text = "1+2*3.45*(3+2)+7";
    int where = -1;
};


