using UnityEngine;
using System.Collections;
using CustomUtil;
using System;

namespace Common
{
    public class ClientTime : Singleton<ClientTime>
    {
        float mCurTime = 0;
        float mLastTime = 0;
        float mDeltaTime = 0;
        uint mServerTime = 0;

        public float CurTime { get { return mCurTime; } }
        public float DeltaTime { get { return mDeltaTime; } }
        public uint ServerTime
        {
            get { return mServerTime; }
            set { mServerTime = value; }
        }

        public ClientTime()
        {
            mCurTime = Time.realtimeSinceStartup;
            mLastTime = mCurTime;
        }

        public void OnUpdate()
        {
            mLastTime = mCurTime;
            mCurTime = Time.realtimeSinceStartup;
            mDeltaTime = mCurTime - mLastTime;
        }

        public static DateTime GetCurDataTime()
        {
            DateTime time = DateTime.Now;
            return time;
        }

        public static DateTime GetDataTime(UInt32 seconds)
        {
            DateTime time = new DateTime(1970, 1, 1);
            time = time.AddSeconds(seconds).ToLocalTime();
            return time;
        }
    }
}

