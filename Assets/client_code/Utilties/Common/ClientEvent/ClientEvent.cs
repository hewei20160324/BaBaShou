using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using CustomUtil;

namespace Common
{
    public class ClientEvent : IPooledObjSupporter
    {
        QuickList<object> mParameters = new QuickList<object>(2);
        GameEventID mID = GameEventID.GEIdCount;
        float mTimeLock = 0;

        // 辅助删除标记，用于立即执行掉事件使用，统一删除地方;
        bool mDeleteFlg = false;
        public bool DeleteFlg { get { return mDeleteFlg; } set { mDeleteFlg = value; } }


        /// <summary>
        /// 成员变量重置，为的是对象可以重用;
        /// 继承自IPooledObjSupporter
        /// </summary>
        public void Reset()
        {
            if (mParameters != null)
            {
                mParameters.Clear();
            }
            else
            {
                mParameters = new QuickList<object>(2);
            }

            mID = GameEventID.GEIdCount;
            mTimeLock = 0;
            mDeleteFlg = false;
        }


        /// <summary>
        /// 对象销毁时做的一些资源回收，相当于析构函数
        /// 继承自IPooledObjSupporter
        /// </summary>
        public void Dispose()
        {

        }

        public GameEventID GetID()
        {
            return mID;
        }

        public void SetID(GameEventID id)
        {
            mID = id;
        }

        public float GetTimelock()
        {
            return mTimeLock;
        }

        /// <summary>
        /// 设置时间锁
        /// </summary>
        /// <param name="timelock"></param>
        public void SetTimelock(float timelock)
        {
            mTimeLock = timelock;
        }

        /// <summary>
        /// 设置延时
        /// </summary>
        /// <param name="lapse"></param>
        public void SetTimeLapse(float lapse)
        {
            mTimeLock = Time.time + lapse;
        }

        public int GetParametersCout()
        {
            return mParameters.Count;
        }

        public void AddParameter(object parameter)
        {
            mParameters.Add(parameter);
        }

        public void GetParameterBool(ref bool parameter, int index)
        {
            if (mParameters == null || index >= mParameters.Count)
            {
                Debug.LogError("Error: The Event Parameter index > mParameters.Count!!!");
                parameter = false;
                return;
            }

            if (mParameters[index] != null && mParameters[index].GetType() != parameter.GetType())
            {
                Debug.LogError("Error: The Event Parameter Type Error!!!");

                parameter = false;
                return;
            }

            parameter = (bool)mParameters[index];
        }

        public void GetParameterByte(ref System.Byte parameter, int index)
        {
            if (mParameters == null || index >= mParameters.Count)
            {
                Debug.LogError("Error: The Event Parameter index > mParameters.Count!!!");
                parameter = 0;
                return;
            }

            if (mParameters[index] != null && mParameters[index].GetType() != parameter.GetType())
            {
                Debug.LogError("Error: The Event Parameter Type Error!!!");

                parameter = 0;
                return;
            }

            parameter = (System.Byte)mParameters[index];
        }

        public void GetParameterFloat(ref float parameter, int index)
        {
            if (mParameters == null || index >= mParameters.Count)
            {
                Debug.LogError("Error: The Event Parameter index > mParameters.Count!!!");
                parameter = 0;
                return;
            }

            if (mParameters[index] != null && mParameters[index].GetType() != parameter.GetType())
            {
                Debug.LogError("Error: The Event Parameter Type Error!!!");

                parameter = 0;
                return;
            }

            parameter = (float)mParameters[index];
        }

        public void GetParameterUInt64(ref System.UInt64 parameter, int index)
        {
            if (mParameters == null || index >= mParameters.Count)
            {
                Debug.LogError("Error: The Event Parameter index > mParameters.Count!!!");
                parameter = 0;
                return;
            }

            if (mParameters[index] != null && mParameters[index].GetType() != parameter.GetType())
            {
                Debug.LogError("Error: The Event Parameter Type Error!!!");

                parameter = 0;
                return;
            }

            parameter = (System.UInt64)mParameters[index];
        }

        public void GetParameterUInt32(ref System.UInt32 parameter, int index)
        {
            if (mParameters == null || index >= mParameters.Count)
            {
                Debug.LogError("Error: The Event Parameter index > mParameters.Count!!!");
                parameter = 0;
                return;
            }

            if (mParameters[index] != null && mParameters[index].GetType() != parameter.GetType())
            {
                Debug.LogError("Error: The Event Parameter Type Error!!!");

                parameter = 0;
                return;
            }

            parameter = (System.UInt32)mParameters[index];
        }

        public void GetParameterInt64(ref System.Int64 parameter, int index)
        {
            if (mParameters == null || index >= mParameters.Count)
            {
                Debug.LogError("Error: The Event Parameter index > mParameters.Count!!!");
                parameter = 0;
                return;
            }

            if (mParameters[index] != null && mParameters[index].GetType() != parameter.GetType())
            {
                Debug.LogError("Error: The Event Parameter Type Error!!!");

                parameter = 0;
                return;
            }

            parameter = (System.Int64)mParameters[index];
        }

        public void GetParameterInt32(ref System.Int32 parameter, int index)
        {
            if (mParameters == null || index >= mParameters.Count)
            {
                Debug.LogError("Error: The Event Parameter index > mParameters.Count!!!");
                parameter = 0;
                return;
            }

            if (mParameters[index] != null && mParameters[index].GetType() != parameter.GetType())
            {
                Debug.LogError("Error: The Event Parameter Type Error!!!");

                parameter = 0;
                return;
            }

            parameter = (System.Int32)mParameters[index];
        }

        public void GetParameterString(ref string parameter, int index)
        {
            if (mParameters == null || index >= mParameters.Count)
            {
                Debug.LogError("Error: The Event Parameter index > mParameters.Count!!!");
                parameter = "";
                return;
            }

            if (mParameters[index] != null && mParameters[index].GetType() != parameter.GetType())
            {
                Debug.LogError("Error: The Event Parameter Type Error!!!");

                parameter = "";
                return;
            }

            parameter = (string)mParameters[index];
        }

        public void GetParameter<T>(ref T parameter, int index)
        {
            if (mParameters == null || index >= mParameters.Count)
            {
                Debug.LogError("Error: The Event Parameter index > mParameters.Count!!!");
                parameter = default(T);
                return;
            }

            if (mParameters[index] != null && mParameters[index].GetType() != typeof(T))
            {
                Debug.LogError("Error: The Event Parameter Type Error!!!");

                parameter = default(T);
                return;
            }

            parameter = (T)mParameters[index];
        }

        public T GetParameter<T>(int index) where T : class
        {
            if (index >= mParameters.Count || index < 0)
            {
                Debug.LogError("Error: The Event Parameter index out of range!!!");
                return default(T);
            }
            T param = mParameters[index] as T;
            return param;
        }

    }
}

