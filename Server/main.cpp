#include <iostream>
#include <memory>
#include <string>
#include "jpeglib.h"
#include "TcpServer.h"
#include "TimeCounter.h"
#include "DateTime.h"

#ifdef SRC_PATH
    #define TESTING_IMG_PATH SRC_PATH"/../ProjectDocs/"
#else
    #define TESTING_IMG_PATH "../../../../ProjectDocs/"
#endif

typedef struct 
{
    int width = 0;
    int height = 0;
    int channels = 0;
    unsigned char *data = nullptr;
}RorzeImage;

// JPEG
#ifdef _WIN32
#define CLibMalloc(a) _aligned_malloc(a , 16)
#else
#define BYTE unsigned char
#define CLibMalloc(a) aligned_alloc(16 , a)
#endif

inline void* EncodeImageToJpegByteStream(const RorzeImage* img, const int q, unsigned long &output_length)
{
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
	cinfo.image_height = img->height;
	cinfo.image_width = img->width;
	cinfo.input_components = img->channels == 4 ? 3 : img->channels;

	if (img->channels == 1) {
		cinfo.in_color_space = J_COLOR_SPACE::JCS_GRAYSCALE;
	}
	else if (img->channels == 3 || img->channels == 4) {
		cinfo.in_color_space = J_COLOR_SPACE::JCS_RGB;
	}

	jpeg_set_defaults(&cinfo);
	jpeg_set_quality(&cinfo, quality, TRUE);
	BYTE *rowData = (BYTE *)malloc(sizeof(BYTE) * img->width * cinfo.input_components);

	jpeg_start_compress(&cinfo, TRUE);
	row_stride = img->width * img->channels;

	bool succ = true;


	while (cinfo.next_scanline < img->height) {
		BYTE* buffer = ((BYTE *)img->data) + row_stride * cinfo.next_scanline;
		if (img->channels == 4 || img->channels == 3) {
			//shrink to 3 channels
			for (int j = 0; j < img->width; ++j) {
				rowData[j * 3] = buffer[j * img->channels + 2];
				rowData[j * 3 + 1] = buffer[j * img->channels + 1];
				rowData[j * 3 + 2] = buffer[j * img->channels + 0];
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
    //FILE* f = fopen("/mnt/e/hacker_station_of_SC/project/TestingImage/lena_gray.bmp", "rb");
    std::string imgName = std::string(TESTING_IMG_PATH) + "lena_gray.bmp";
    FILE* f = fopen(imgName.c_str(), "rb");
    if(!f){
        std::cout << "\n\ncan not open file!" << std::endl;
        img->width = 512;
        img->height = 512;
        img->channels = 1;
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

enum class SendImgFormat
{
	BMP,
	JPEG,
};

inline bool SendImage(TcpServer* imgServer, RorzeImage* img, SendImgFormat format = SendImgFormat::BMP, int quality = 60)
{
	bool ret = false;

	#pragma pack(push)
	#pragma pack(1)
	struct img_hdr
	{
		int total;
		unsigned char type;
		int w, h, channels;
	};
	#pragma pack(pop)

	// variables which sending image needed
	int len = 0;
	char* imgData = nullptr;
	int imgDataLen = 0;
	int imgFmt = 0;

	// Begin orgnizing data
	if(SendImgFormat::JPEG == format){
		unsigned long jpegDataLen = 0;
		TimeCounter::GetInstance().SetStartCountPoint();
		imgData = (char*)EncodeImageToJpegByteStream(img, quality, jpegDataLen);
		std::cout << "compress to jpeg elapsed time = " << TimeCounter::GetInstance().GetElapsedTime() << "ms, data length = " << jpegDataLen << std::endl;

		imgDataLen = static_cast<int>(jpegDataLen);
		imgFmt = 1;
	}
	else{
		imgDataLen = img->width * img->height * img->channels;
		//imgData = new char[imgDataLen];
		imgData = (char*)malloc(imgDataLen);
		memcpy(imgData, img->data, imgDataLen);
		imgFmt = 0;
	}
	len = imgDataLen + sizeof(struct img_hdr);

	char* buff = new char[len];
	memset(buff, 0, len);
	struct img_hdr* hdr = (struct img_hdr*)buff;
	std::cout << "start address = " << hdr << std::endl;
	hdr->total = len;
	hdr->type = imgFmt;
	hdr->w = img->width;
	hdr->h = img->height;
	hdr->channels = img->channels;
	unsigned char* payload = (unsigned char*)(hdr + 1);
	memcpy(payload, imgData, imgDataLen);

	ret = imgServer->SendData(buff, len);

	delete [] buff;
	if(imgData)
		free(imgData);

	return ret;
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
            SendImage(server, img);

            if(img){
                delete [] img->data;
                delete img;
            }
        }
        else if(command == std::string("TestJpeg")){
            RorzeImage* img = nullptr;
            img = MockGrabImage();
            SendImage(server, img, SendImgFormat::JPEG, 60);

            if(img){
                delete [] img->data;
                delete img;
            }
        }
        else{
            std::cout << "type: " << command << std::endl;
        }
    }

    return EXIT_SUCCESS;
}