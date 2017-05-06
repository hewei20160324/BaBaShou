using System;
using System.Collections;
using System.Reflection;

namespace Common
{
    public interface IPooledObjSupporter : IDisposable
    {
        void Reset(); //恢复对象为初始状态，当IObjectPool.GiveBackObject时调用;
    }


    public interface IObjectPool
    {
        //objType为缓存的对象的类型，cArgs为缓存对象的构造参数;
        bool Initialize(Type objType, object[] cArgs, int minNum, int maxNum);
        object RentObject();
        void GiveBackObject(int objHashCode);
        void Dispose();

        int MinObjCount { get; }
        int MaxObjCount { get; }
        int CurObjCount { get; }
        int IdleObjCount { get; }

        event CallBackObjPool PoolShrinked;
        event CallBackObjPool MemoryUseOut; //内存分配失败;
    }

    public delegate void CallBackObjPool();
}