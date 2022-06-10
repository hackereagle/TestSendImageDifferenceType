#ifndef _TIME_COUNTER_H_
#define _TIME_COUNTER_H_

#include <chrono>
#include <atomic>
// #include "WindowsDllDefinition.hpp"

enum TimeCategry
{
    us,
    ms,
    s,
};


class TimeCounter
{
public:
    static TimeCounter GetInstance();
    ~TimeCounter();
    void SetStartCountPoint();
    double GetElapsedTime(TimeCategry category = TimeCategry::ms);

private:
    static TimeCounter *mInstance;
    std::chrono::steady_clock::time_point mStart;
    int64_t *mTimeLastStart = nullptr;

    TimeCounter();
};
#endif // _TIME_COUNTER_H_