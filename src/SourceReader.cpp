//
// Created by daonlyowner on 29.03.18.
//

#include "SourceReader.h"

int SourceReader::Eat() {
    if (!good()) return -1;
    return text[++where];
}

int SourceReader::Peek() {
    if (!good()) return -1;
    return text[where+1];
}

int SourceReader::Current() {
    if(where < 0 || where >= text.size()-1) return -1;
    return text[where];
}


