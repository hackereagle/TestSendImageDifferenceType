#include <iostream>
#include "TcpServer.h"
#include <memory>
#include <string>
extern "C"{
#include "jpeglib.h"
}

typedef struct 
{
    int width = 0;
    int height = 0;
    int channels = 0;
    unsigned char *data = nullptr;
}RorzeImage;

// JPEG
typedef struct CLibImageStruct {
	int iWidth , iHeight , iChannel ;
	void *vPtrImage ;
} CLibImage ;

typedef CLibImage* HCLIBIMAGE;

#ifdef _WIN32
#define CLibMalloc(a) _aligned_malloc(a , 16)
#else
#define BYTE unsigned char
#define CLibMalloc(a) aligned_alloc(16 , a)
#endif

inline void* EncodeImageToJpegByteStream(const HCLIBIMAGE img, const int q, unsigned long &output_length)
{
	// if (IsEmptyImage(img))
	// {
	// 	return NULL;
	// }
	int quality = q;

	/* This struct contains the JPEG decompression parameters and pointers to
	* working space (which is allocated as needed by the JPEG library).
	*/
	struct jpeg_compress_struct cinfo;
	/* More stuff */
	int row_stride;		/* physical row width in output buffer */
						/* Now we can initialize the JPEG decompression object. */
	jpeg_create_compress(&cinfo);

	struct jpeg_error_mgr pub;	/* "public" fields */

								/* Step 2: specify data destination (eg, a file) */
	unsigned char *tmpBuffer = NULL;
	jpeg_mem_dest(&cinfo, &tmpBuffer, &output_length);
	//jpeg_stdio_dest(&cinfo, outfile);
	/* We set up the normal JPEG error routines, then override error_exit. */
	cinfo.err = jpeg_std_error(&pub);
	cinfo.image_height = img->iHeight;
	cinfo.image_width = img->iWidth;
	cinfo.input_components = img->iChannel == 4 ? 3 : img->iChannel;

	if (img->iChannel == 1) {
		cinfo.in_color_space = J_COLOR_SPACE::JCS_GRAYSCALE;
	}
	else if (img->iChannel == 3 || img->iChannel == 4) {
		cinfo.in_color_space = J_COLOR_SPACE::JCS_RGB;
	}

	jpeg_set_defaults(&cinfo);
	jpeg_set_quality(&cinfo, quality, TRUE);
	BYTE *rowData = (BYTE *)malloc(sizeof(BYTE) * img->iWidth * cinfo.input_components);

	jpeg_start_compress(&cinfo, TRUE);
	row_stride = img->iWidth * img->iChannel;

	bool succ = true;


	while (cinfo.next_scanline < img->iHeight) {
		BYTE* buffer = ((BYTE *)img->vPtrImage) + row_stride * cinfo.next_scanline;
		if (img->iChannel == 4 || img->iChannel == 3) {
			//shrink to 3 channels
			for (int j = 0; j < img->iWidth; ++j) {
				rowData[j * 3] = buffer[j * img->iChannel + 2];
				rowData[j * 3 + 1] = buffer[j * img->iChannel + 1];
				rowData[j * 3 + 2] = buffer[j * img->iChannel + 0];
			}
		}
		else {
			memcpy(rowData, buffer, sizeof(BYTE) * row_stride);
		}
		int res = jpeg_write_scanlines(&cinfo, &rowData, 1);
		if (res != 1) {
			succ = false;
			break;
		};
	};
	//clean-up all data
	free(rowData);
	jpeg_finish_compress(&cinfo);
	jpeg_destroy_compress(&cinfo);

	void *ret = CLibMalloc(sizeof(unsigned char) * output_length);
	memcpy(ret, tmpBuffer, sizeof(unsigned char) * output_length);

	free(tmpBuffer);

	return ret;
};
// JPEG

void ReplyCommand(int len, char* buffer)
{
    std::cout << "Receive reply" << std::endl;
}

RorzeImage* MockGrabImage()
{
    // This method only used in testing story.
    // It would read a bmp to mock CCD grab image.
    // Refer to stackoverflow.com/questions/9296059/read-pixel-value-in-bmp-file by Owen Morga
    RorzeImage* img = new RorzeImage();
    memset(img, 0, sizeof(RorzeImage));

    int i;
    FILE* f = fopen("/mnt/e/hacker_station_of_SC/project/TestingImage/lena_gray.bmp", "rb");
    if(!f){
        std::cout << "\n\ncan not open file!" << std::endl;
        img->width = 512;
        img->height = 512;
        img->data = new unsigned char[img->width * img->height];
        return img;
    }
    unsigned char info[54];
    fread(info, sizeof(unsigned char), 54, f);

    // extract image height and width from header
    memcpy(&img->width, info + 18, sizeof(int));
    memcpy(&img->height, info + 22, sizeof(int));
    short bits = 0;
    memcpy(&bits, info + 28, sizeof(short));
    img->channels = bits / 8;

    bool isReverse = true;
    if(img->height < 0){
        img->height = img->height * (-1);
        isReverse = false;
    }
    int size = img->width * img->height;

    img->data = new unsigned char[size];
    fread(img->data, sizeof(unsigned char), size, f);
    fclose(f);

    if(isReverse){
        unsigned char *temp = new unsigned char[size];
        memcpy(temp, img->data, size);
        for(int i = 0 ; i < size; i++){
            *(img->data + i) = *(temp + size - i);
        }
        delete [] temp;
    }

    return img;

}

int main(int argc, const char* argv[])
{
    TcpServer *server = new TcpServer();
    server->ReplyClientCmd = ReplyCommand;
    server->StartListen();

    std::string command; 
    for(;;){
        getline(std::cin, command);

        if(command == std::string("CloseConnection"))
            server->StopListen();
        else if(command == std::string("Test")){
            RorzeImage* img = nullptr;
            img = MockGrabImage();
            #pragma pack(push)
            #pragma pack(1)
            struct img_hdr
            {
                int total;
                unsigned char type;
                int w, h, channels;
            };
            #pragma pack(pop)
            std::cout << "img_hdr size = " << sizeof(img_hdr) << std::endl;
            int len = img->width * img->height * img->channels + sizeof(struct img_hdr);

            char* buff = new char[len];
            memset(buff, 0, len);
            struct img_hdr* hdr = (struct img_hdr*)buff;
            std::cout << "start address = " << hdr << std::endl;
            hdr->total = len;
            hdr->type = 0;
            hdr->w = img->width;
            hdr->h = img->height;
            hdr->channels = img->channels;
            unsigned char* payload = (unsigned char*)(hdr + 1);
            std::cout << "image data address = " << payload << std::endl;
            memcpy(payload, img->data, img->width * img->height * img->channels);

            if(server->SendData(buff, len)){
                std::cout << "Send sucess" << std::endl;
                std::cout << "\tTotal length = " << len
                          << "\n\tData length = " << img->width * img->height * img->channels
                          << std::endl;
            }
            else
                std::cout << "Send fail" << std::endl;
            delete [] buff;
        }
        else if(command == std::string("TestJpeg")){
            RorzeImage* img = nullptr;
            img = MockGrabImage();
            #pragma pack(push)
            #pragma pack(1)
            struct img_hdr
            {
                int total;
                unsigned char type;
                int w, h, channels;
            };
            #pragma pack(pop)
            std::cout << "img_hdr size = " << sizeof(img_hdr) << std::endl;
            int len = img->width * img->height * img->channels + sizeof(struct img_hdr);

            char* buff = new char[len];
            memset(buff, 0, len);
            struct img_hdr* hdr = (struct img_hdr*)buff;
            std::cout << "start address = " << hdr << std::endl;
            hdr->total = len;
            hdr->type = 0;
            hdr->w = img->width;
            hdr->h = img->height;
            hdr->channels = img->channels;
            unsigned char* payload = (unsigned char*)(hdr + 1);
            std::cout << "image data address = " << payload << std::endl;
            //memcpy(payload, img->data, img->width * img->height * img->channels);

            if(server->SendData(buff, len)){
                std::cout << "Send sucess" << std::endl;
                std::cout << "\tTotal length = " << len
                          << "\n\tData length = " << img->width * img->height * img->channels
                          << std::endl;
            }
            else
                std::cout << "Send fail" << std::endl;
            delete [] buff;
        }
        else{
            std::cout << "type: " << command << std::endl;
        }
    }

    return EXIT_SUCCESS;
}