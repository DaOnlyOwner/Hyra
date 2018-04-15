cmake_minimum_required(VERSION 2.8)
project(Hyra)

set(CMAKE_CXX_STANDARD 14)

file(GLOB_RECURSE HYRA_SOURCES src/*.cpp src/*.h)

find_package(LLVM REQUIRED)
find_package(Boost REQUIRED)

find_library(REFLEX_LIBRARY NAMES reflex)
find_path(REFLEX_INCLUDE_DIR NAMES "absmatcher.h" PATH_SUFFIXES "include/reflex")

add_executable(Hyra ${HYRA_SOURCES})
target_link_libraries(Hyra ${REFLEX_LIBRARY})
target_include_directories(Hyra PUBLIC ${REFLEX_INCLUDE_DIR})
target_include_directories(Hyra PUBLIC ${Boost_INCLUDE_DIRS})

