#ifndef _SERVER_H_
#define _SERVER_H_

#include <unistd.h>
#include <sys/socket.h>
#include <netinet/in.h> 
#include <arpa/inet.h>
#include <string.h>
#include <functional>
#include <atomic>
#include <thread>

class TcpServer
{
    public:
        TcpServer();
        ~TcpServer();
        void StopListen();
        void StartListen();
        bool SendData(char* data, int len);
        std::function<void(int, char*)> ReplyClientCmd = nullptr;

    private:
        char mServerIP[20];
        int mPort = 0;
        sockaddr_in mMasterAddr;
        int mMasterFd = 0;
        sockaddr_in mClient;
        int mClientFd = 0;
        // bool mIsListening = false;
        std::atomic<bool> mIsListening;
        bool Initialize();
        void ListenFunction();
        std::thread mListenThread;
};

#endif //_SERVER_H_