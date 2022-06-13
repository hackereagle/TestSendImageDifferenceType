#ifndef _DATE_TIME_H_
#define _DATE_TIME_H_

#include <iostream>
#include <chrono>
#include <time.h>
#include <iomanip>


class DateTime
{
public:
    DateTime(/* args */)
    {
        auto now = std::chrono::system_clock::now();
        std::time_t currentTime = std::chrono::system_clock::to_time_t(now);
        std::chrono::milliseconds ms = std::chrono::duration_cast<std::chrono::milliseconds>(now.time_since_epoch());
        struct tm currentLocalTime;
        //localtime_r(&currentTime, &currentLocalTime);
        currentLocalTime = *localtime(&currentTime);
        
        mYear = currentLocalTime.tm_year + 1900;
        mMonth = currentLocalTime.tm_mon + 1;
        mDay = currentLocalTime.tm_mday;
        mHour = currentLocalTime.tm_hour;
        mMinute = currentLocalTime.tm_min;
        mSecond = currentLocalTime.tm_sec;
        mMillisecond = ms.count() % 1000;
        // std::cout << mHour << ":" << mMinute << ":" << mSecond << "." << mMillisecond << std::endl;
    }
    ~DateTime() {
    }
    int GetYear(){ return mYear;}
    int GetMonth(){ return mMonth;}
    int GetDay() { return mDay;}
    int GetHour() { return mHour;}
    int GetMinute() { return mMinute;}
    int GetSecond() { return mSecond;}
    int GetMillisecond() { return mMillisecond;}

private:
    int mYear;
    int mMonth;
    int mDay;
    int mHour;
    int mMinute;
    int mSecond;
    int mMillisecond;
};

#endif // _DATE_TIME_H_