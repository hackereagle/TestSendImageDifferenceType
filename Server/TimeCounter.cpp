#include <mutex>
#include "TimeCounter.h"

TimeCounter *TimeCounter::mInstance = nullptr;
std::mutex TimeCounterObj;
std::chrono::steady_clock::time_point mStart;

TimeCounter::TimeCounter()
{
    mStart = std::chrono::steady_clock::now();
    mTimeLastStart = new int64_t;
}

TimeCounter::~TimeCounter()
{}

TimeCounter TimeCounter::GetInstance()
{
    std::lock_guard<std::mutex> lock(TimeCounterObj);
    if(mInstance == nullptr){
        mInstance = new TimeCounter();
    }

    return *mInstance;
}

void TimeCounter::SetStartCountPoint()
{
    // mStart = std::chrono::steady_clock::now() can not update mStart
    // so use other variable to record time elapsed.
    std::chrono::steady_clock::time_point tmp = std::chrono::steady_clock::now();
    *mTimeLastStart = std::chrono::duration_cast<std::chrono::microseconds>(tmp - mStart).count();
}

double TimeCounter::GetElapsedTime(TimeCategry category)
{
    std::chrono::steady_clock::time_point end = std::chrono::steady_clock::now();
    double elapsed = 0.0; 
    int64_t timeLastCurrent  = std::chrono::duration_cast<std::chrono::microseconds>(end - mStart).count();
    int64_t _time = timeLastCurrent - *mTimeLastStart;

    if(TimeCategry::us == category){
        elapsed = static_cast<double>(_time);
    }
    else if(TimeCategry::ms == category){
        elapsed = static_cast<double>(_time) / 1000.0;
    }
    else if(TimeCategry::s == category){
        elapsed = static_cast<double>(_time) / 1000000.0;
    }

    return elapsed;
}
