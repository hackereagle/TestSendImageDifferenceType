cmake_minimum_required(VERSION 3.4)
project(TestSendImage)
set(PROJ_NAME TestSendImage)
set(THREADS_PREFER_PTHREAD_FLAG ON)
find_package(Threads REQUIRED)

list(APPEND CMAKE_MODULE_PATH "${CMAKE_SOURCE_DIR}/cmake")
find_package(jpeg REQUIRED)
if(JPEG_FOUND)
    include_directories(${JPEG_INCLUDE_DIRS}    include_directories(${HALCON_INCLUDE_DIRS} ${HALCON_INCLUDE_DIRS}/halconcpp)
    link_directories(${HALCON_LIBS}))
    link_directories(${JPEG_LIBS})
endif(JPEG_FOUND)

aux_source_directory(. DIR_SRCS)

add_executable(${PROJ_NAME} ${DIR_SRCS})
target_link_libraries(${PROJ_NAME} ${CMAKE_DL_LIBS} Threads::Threads jpeg)

set(CMAKE_VERBOSE_MAKEFILE ON)
set(CMAKE_BUILD_TYPE Debug)
set(CMAKE_CXX_STANDARD 17)