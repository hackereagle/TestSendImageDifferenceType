#include <iostream>
#include "TcpServer.h"
#include <memory>
#include <string>

typedef struct 
{
    int width = 0;
    int height = 0;
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
            std::string header = "aAOI1";

            char temp[100];
            sprintf(temp, "%s.GRAB:%d,%d,", header.c_str(), img->width, img->height);
            std::string _temp = std::string(temp);

            int len = _temp.size() + (img->width * img->height);
            char* package = new char[len];
            memcpy(package, _temp.c_str(), _temp.size());
            memcpy(package + _temp.size(), img->data, img->width * img->height);

            if(server->SendData(package, len)){
                std::cout << "Send sucess" << std::endl;
                std::cout << "\tTotal length = " << len
                          << "\n\tTotal Command length = " << _temp.size()
                          << "\n\tData length = " << img->width * img->height
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