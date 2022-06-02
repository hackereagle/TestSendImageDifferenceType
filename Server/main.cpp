#include <iostream>
#include "TcpServer.h"
#include <memory>
#include <string>

typedef struct 
{
    int width = 0;
    int height = 0;
    int channels = 0;
    unsigned char *data = nullptr;
}RorzeImage;

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

            char* buff = new char[len + (1 << 5)];
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
        }
        else{
            std::cout << "type: " << command << std::endl;
        }
    }

    return EXIT_SUCCESS;
}