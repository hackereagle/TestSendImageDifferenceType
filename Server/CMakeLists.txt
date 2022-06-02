cmake_minimum_required(VERSION 3.4)
project(TestSendImage)
set(PROJ_NAME TestSendImage)
set(THREADS_PREFER_PTHREAD_FLAG ON)
find_package(Threads REQUIRED)

aux_source_directory(. DIR_SRCS)

add_executable(${PROJ_NAME} ${DIR_SRCS})
target_link_libraries(${PROJ_NAME} ${CMAKE_DL_LIBS} Threads::Threads)

set(CMAKE_VERBOSE_MAKEFILE ON)
set(CMAKE_BUILD_TYPE Debug)
set(CMAKE_CXX_STANDARD 17)