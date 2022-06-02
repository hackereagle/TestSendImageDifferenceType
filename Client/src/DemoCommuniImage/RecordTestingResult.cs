using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RORZE
{
    internal class TestingInform
    {
        private Double mMaxTime; // ms
        private Double mMinTime; // ms;
        private Double mCount;
        private Double mAvg;
        private Double mAvgSqrXi;
        private Double mCurrentTime;
        private Double mStanderdDeviation;

        public TestingInform()
        {
            mMaxTime = 0.001;
            mMinTime = 99999999.0;
            //mTotalTime = 0;
            mAvg = 0.0;
            mCount = 0.0;
            mCurrentTime = 0.0;
            mStanderdDeviation = 0.0;
        }

        public void RecordInformation(long elapseTime)
        {
            mCurrentTime = Convert.ToDouble(elapseTime) / 1000.0;
            //mTotalTime = mTotalTime + mCurrentTime;
            //UInt64 totalTime, sumSqrtXi;
            //totalTime = (mAvg * mCount) + mCurrentTime;
            //sumSqrtXi = (mAvgSqrXi * mCount) + (mCurrentTime * mCurrentTime);
            mCount++;

            //mAvg = totalTime / mCount;
            //mAvgSqrXi = sumSqrtXi / mCount;
            double lastCount = mCount - 1;
            mAvg = ((mAvg * lastCount) + mCurrentTime) / mCount;
            mAvgSqrXi = ((mAvgSqrXi * lastCount) + (mCurrentTime * mCurrentTime)) / mCount;
            mStanderdDeviation = Math.Sqrt(mAvgSqrXi - (mAvg * mAvg));
            if (mCurrentTime > mMaxTime)
                mMaxTime = mCurrentTime;
            else if (mCurrentTime < mMinTime)
                mMinTime = mCurrentTime;

        }
        public double MaxTime { get { return mMaxTime; } }
        public double MinTime { get { return mMinTime; } }
        public double AvgTime { get { return Math.Round(mAvg, 3); } }
        public double Count { get { return mCount; } }
        public double Current { get { return mCurrentTime; } }
        public double StandardDeviation { get { return Math.Round(mStanderdDeviation, 3); } }
    }

    internal class RecordTestingResult
    {
        private System.Diagnostics.Stopwatch mStopWatch;
        private TestingInform mInfo;
        public RecordTestingResult()
        {
            mInfo = new TestingInform();
            mStopWatch = new System.Diagnostics.Stopwatch();
        }

        public void SetStartRecordPoint()
        {
            mStopWatch.Restart();
        }

        public void Record()
        {
            mStopWatch.Stop();
            mInfo.RecordInformation(mStopWatch.ElapsedMilliseconds);
            BackgroundLogger.AsyncWrite(LogType.Socket, Status);
        }

        public double ElapseTime { get { return mInfo.Current; } }
        public double MaxTime { get { return mInfo.MaxTime; } }
        public double MinTime { get { return mInfo.MinTime; } }
        public double AvgTime { get { return mInfo.AvgTime; } }
        public double Count { get { return mInfo.Count; } }
        public double StandardDeviation { get { return mInfo.StandardDeviation; } }
        public string Status { get { return $"{mInfo.Count}-th: Elapse={mInfo.Current} s, Min={mInfo.MinTime} s, Max={mInfo.MaxTime} s, Avg={mInfo.AvgTime} s, Std = {mInfo.StandardDeviation} s"; } }
    }
}
