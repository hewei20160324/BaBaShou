/**
 * breif: 单例模式;
 * data: 2016/10/31;
 */

using UnityEngine;
using System.Collections;

namespace CustomUtil
{
    public class Singleton<T> where T : class, new()
    {
        static T _instance = null;

        public static T GetInstance()
        {
            if (_instance == null)
            {
                _instance = new T();
            }
            return _instance;
        }

        public static void DestroyInstance()
        {
            _instance = null;
        }
    }
}

