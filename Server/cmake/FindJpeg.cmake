unset(JPEG_FOUND)
unset(JPEG_INCLUDE_DIRECTORY)
unset(JPEG_LIBRARY)

if(MSVC)
    if(DEFINED ENV{JPEGROOT})
        set(JPEG_FOUND 1)
        set(JPEG_LIBS "$ENV{JPEGROOT}/lib/")
        set(JPEG_INCLUDE_DIRS "$ENV{JPEGROOT}/include")

        message(STATUS "libjpeg-dev library: ${JPEG_LIBS}")
        message(STATUS "libjpeg-dev library: ${JPEG_INCLUDE_DIRS}")
    else()
        set(JPEG_FOUND 0)
        message(STATUS "In FindJpeg.cmake, NOT FOUND libjpeg-dev enviroment variable")
    endif()
else()
    # find_path(JPEG_INCLUDE_DIRECTORY
    #             NAMES jpeglib.h
    #             HINTS
    #             ${CMAKE_SOURCE_DIR}/3rd-party/jpeg-9e)
    find_path(JPEG_INCLUDE_DIRECTORY
                NAMES jpeglib.h
                HINTS
                /usr/include)
    if (JPEG_INCLUDE_DIRECTORY)
        message(STATUS "libjpeg-dev include: ${JPEG_INCLUDE_DIRECTORY}")
    endif()

    execute_process( COMMAND uname -m COMMAND tr -d '\n' OUTPUT_VARIABLE ARCHITECTURE )
    message( STATUS "Architecture: ${ARCHITECTURE}" )
    message( STATUS "in /usr/lib/${ARCHITECTURE}-linux-gnu to find libjpeg.so" )
	find_path(JPEG_LIBRARY
				NAMES libjpeg.so
				HINTS
				/usr/lib/${ARCHITECTURE}-linux-gnu)
    # if(ARCHITECTURE MATCHES aarch64)
    #     find_path(JPEG_LIBRARY
    #                 NAMES libjpeg.so
    #                 HINTS
    #                 /usr/lib/aarch64-linux-gnu)
    # elseif(ARCHITECTURE MATCHES armv7a)
    #     find_path(JPEG_LIBRARY
    #                 NAMES libjpeg.so
    #                 HINTS
    #                 /usr/lib/armv7a-linux-gnu)
    # else()
    #     # on general linux system
    #     find_path(JPEG_LIBRARY
    #                 NAMES libjpeg.so
    #                 HINTS
    #                 /usr/lib/X86_64-linux-gnu)
    # endif()

    if (JPEG_LIBRARY)
        message(STATUS "libjpeg library: ${JPEG_LIBRARY}")
    endif()

    if(JPEG_INCLUDE_DIRECTORY AND JPEG_LIBRARY)
        set(JPEG_FOUND 1)
    endif(JPEG_INCLUDE_DIRECTORY AND JPEG_LIBRARY)

    if (JPEG_FOUND)
        set(JPEG_INCLUDE_DIRS "${JPEG_INCLUDE_DIRECTORY}")
        set(JPEG_LIBS "${JPEG_LIBRARY}")
    else()
        set(JPEG_FOUND 0)
        message(STATUS "In FindJpeg.cmake, NOT FOUND libjpeg-dev enviroment variable")
    endif()
endif(MSVC)