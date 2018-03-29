//
// Created by daonlyowner on 29.03.18.
//

#include "SourceReader.h"

int SourceReader::Eat() {
    if (!good()) return -1;
    return text[where++];
}

int SourceReader::Peek() {
    if (where+1 < text.size()) return -1;
    return text[where+1];
}


