#include "TcpServer.h"
#include <iostream>

TcpServer::TcpServer()
{
    strcpy(this->mServerIP, "127.0.0.1"); // it would be read from configure file, future.
    this->mPort = 12000; // it would be read from configure file, future.

    if(!Initialize())
    {
        std::cout << __func__ << " initialize failded!" << std::endl;
        return;
    }
}

TcpServer::~TcpServer()
{
    StopListen();
}

void TcpServer::StopListen()
{
    this->mIsListening = false;
    //int ret1 = close(this->mMasterFd);
    // int ret2 = close(this->mClientFd);
    // std::cout << "close mMasterFd return = " << ret1
    //             << ", close mClient return = " << ret2 << std::endl;
}

bool TcpServer::Initialize()
{
    this->mMasterFd = socket(AF_INET, SOCK_STREAM, 0);
    if(this->mMasterFd == -1)
    {
        perror("Create socket failed"); // log
        return false;
    }

    int opt = 1;
    if(setsockopt(this->mMasterFd, SOL_SOCKET, SO_REUSEADDR | SO_REUSEPORT, &opt, sizeof(opt)))
    {
        perror("set socket error!"); // log
        return false; 
    }

    this->mMasterAddr.sin_family = AF_INET;
    //this->mMasterAddr.sin_addr.s_addr = inet_addr(this->mServerIP);
    this->mMasterAddr.sin_addr.s_addr = INADDR_ANY;
    this->mMasterAddr.sin_port = htons(this->mPort);
    return true;
}

void TcpServer::StartListen()
{
    if(!this->mIsListening) {
        this->mIsListening = true;

        mListenThread = std::thread(&TcpServer::ListenFunction, this); 
    }
    else{
        perror("Have a thread listening! Please STOP it!"); // log
        return;
    }
}

void TcpServer::ListenFunction()
{
    int sockLen = sizeof(this->mMasterAddr);
    if(bind(this->mMasterFd, (sockaddr *)&this->mMasterAddr, sockLen) < 0){
        perror("Occuring ERROR in bind!"); // log
        this->mIsListening = false;
        return;
    }

    if(listen(this->mMasterFd, 5) < 0){
        perror("Occuring ERROR in listen!"); // log
        this->mIsListening = false;
        return;

    }

    while(this->mIsListening){
        std::cout << "waitting connection.........." << std::endl;
        this->mClientFd = accept(this->mMasterFd, (sockaddr *)&this->mClient, (socklen_t *)&sockLen);
        if(this->mClientFd < 0){
            perror("accept connection failed!"); // log
            this->mIsListening = false;
            return;
        }
        std::cout << "accept from " << inet_ntoa(this->mClient.sin_addr) << " connection" << std::endl;

        while (this->mIsListening) {
            char buffer[1024];
            memset((void *)buffer, 0, 1024);
            int lenRecv = recv(this->mClientFd, buffer, 1024, 0);
            if(lenRecv > 0){
                std::cout << "recieve package len = " << lenRecv << ", message = " << buffer << std::endl;
                // if connection success, here do some decoding or reply command.
                if(ReplyClientCmd != nullptr){
                    ReplyClientCmd(lenRecv, buffer);
                }
                // const char *retStr = "server receive!";
                // send(this->mClientFd, retStr, strlen(retStr), 0);
            }
            else{
                std::cout << "connection break!\n" << std::endl;
                close(this->mClientFd);
                break;
            }
        }
    }
    std::cout << "connection closed" << std::endl;
    
}

bool TcpServer::SendData(char* data, int len)
{
    int ret = send(this->mClientFd, data, len, 0);
    if(ret > 0)
        return true;
    else
        return false;   
}