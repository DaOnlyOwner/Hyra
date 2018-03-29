cmake_minimum_required(VERSION 2.8)
project(Hyra)

set(CMAKE_CXX_STANDARD 14)

file(GLOB_RECURSE HYRA_SOURCES src/*.cpp src/*.h)

find_package(LLVM REQUIRED)

add_executable(Hyra ${HYRA_SOURCES} src/SourceReader.cpp src/SourceReader.h)