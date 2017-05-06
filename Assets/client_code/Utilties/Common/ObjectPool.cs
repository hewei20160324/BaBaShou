using System;
using System.Collections;
using System.Reflection;
using CustomUtil;

/// <summary>
/// 支持线程安全，适用范围：逻辑数据结构，减少GC神器;
/// Initialize:初始化;
/// RentObject:租赁;
/// GiveBackObject:归还;
/// 如果被管理对象继承IPooledObjectSupporter，在GiveBackObject时会调用Reset函数进行复位;
/// </summary>

namespace Common
{
    /**/
    /// <summary>
    /// IObjectPool
    /// Hash线程安全，对于非基础类的key值效率比Dictionary快;
    /// </summary>
    #region ObjectPool
    public class ObjectPool : IObjectPool
	{
		#region members
		private Type destType = null;
		private object[] ctorArgs = null;
		private int minObjCount = 0;
		private int maxObjCount = 0;
		private int shrinkPoint = 0;
		private Hashtable hashTableObjs = new Hashtable();
		private Hashtable hashTableStatus = new Hashtable(); //key - isIdle 其中key就是hashcode;
		private QuickList<Int32> keyList = new QuickList<Int32>();
		private bool supportReset = false;
		private Int32 mIdleObjCount = 0;
		#endregion


		public event CallBackObjPool PoolShrinked;
		public event CallBackObjPool MemoryUseOut;

		public bool Initialize(Type objType, object[] cArgs, int minNum, int maxNum)
		{
			if (minNum < 1)
			{
				minNum = 1;
			}
			if (maxNum < 2)
			{
				maxNum = 2;
			}
            if (maxNum < minNum)
            {
                maxNum = minNum;
            }

            this.destType = objType;
			this.ctorArgs = cArgs;
			this.minObjCount = minNum;
			this.maxObjCount = maxNum;
			double cof = 1 - ((double)minNum / (double)maxNum);
			this.shrinkPoint = (int)(cof * minNum);

			mIdleObjCount = 0;

			//缓存的类型是否支持IPooledObjSupporter接口
			Type supType = typeof(IPooledObjSupporter);
			if (supType.IsAssignableFrom(objType))
			{
				this.supportReset = true;
			}

			return true;
		}

		#region CreateOneObject ,DistroyOneObject
		private int CreateOneObject()
		{
			object obj = null;

			try
			{
				obj = Activator.CreateInstance(this.destType, this.ctorArgs);
			}
            //分配内存失败！;
            catch (Exception ee) 
			{
				PrintPoolStatus();
				UnityCustomUtil.CustomLogError("ObjectPool " + destType.ToString() + " used out!!!!!" + ee.ToString());
				//ee = ee;
				this.maxObjCount = this.CurObjCount;
				if (this.minObjCount > this.CurObjCount)
				{
					this.minObjCount = this.CurObjCount;
				}

				if (this.MemoryUseOut != null)
				{
					this.MemoryUseOut();
				}
				return -1;
			}

			int key = obj.GetHashCode();
			this.hashTableObjs.Add(key, obj);
			this.hashTableStatus.Add(key, true);
			this.keyList.Add(key);
			this.mIdleObjCount++;
			return key;
		}

		private void DistroyOneObject(int key)
		{
			object target = this.hashTableObjs[key];
			IDisposable tar = target as IDisposable;
			if (tar != null)
			{
				tar.Dispose();
			}

			if ((bool)this.hashTableStatus[key])
			{
				this.hashTableStatus[key] = true;
				this.mIdleObjCount--;
			}
			
			this.hashTableObjs.Remove(key);
			this.hashTableStatus.Remove(key);
			this.keyList.Remove(key);
		}
		#endregion

		public object RentObject()
		{
			lock (this)
			{
				Int32 key = -1;

				object target = null;

				for (Int32 index = 0; index < this.keyList.size; index++)
				{
					key = this.keyList[index];
					if ((bool)this.hashTableStatus[key]) //isIdle
					{
						this.hashTableStatus[key] = false;
						this.mIdleObjCount--;
						target = this.hashTableObjs[key];
						break;
					}
				}

				if (target == null)
				{
					if (this.keyList.size < this.maxObjCount)
					{
						key = this.CreateOneObject();
						if (key != -1)
						{
							this.hashTableStatus[key] = false;
							this.mIdleObjCount--;
							target = this.hashTableObjs[key];
						}
					}
				}

				if (target == null)
				{
					UnityCustomUtil.CustomLogError("Pool Rent Instance Failed! " + this.keyList.size);
				}
				return target;
			}

		}

		#region GiveBackObject
		public void GiveBackObject(int objHashCode)
		{
			if (this.hashTableStatus[objHashCode] == null)
			{
				return;
			}

			lock (this)
			{
				this.hashTableStatus[objHashCode] = true;
				this.mIdleObjCount++;
				if (this.supportReset)
				{
					IPooledObjSupporter supporter = (IPooledObjSupporter)this.hashTableObjs[objHashCode];
					supporter.Reset();
				}

				if (this.CanShrink())
				{
					this.Shrink();
				}
			}
		}


		/// <summary>
		/// 能够收缩对象池，防止浪费;
		/// 当利用率比较低时(容器较大，实际用的比较少);
		/// </summary>
		/// <returns></returns>
		private bool CanShrink()
		{
			int idleCount = this.GetIdleObjCount();
			int busyCount = this.CurObjCount - idleCount;

            // busy < min - min*min/max;
			return (busyCount < this.shrinkPoint) && (this.CurObjCount > (this.minObjCount + (this.maxObjCount - this.minObjCount) / 2));
		}

		private void Shrink()
		{
			for (Int32 index = 0; index < this.keyList.size;)
			{
				Int32 key = this.keyList[index];
				if ((bool)this.hashTableStatus[key])
				{
					this.DistroyOneObject(key);
				}
				else
				{
					index++;
				}

				if (this.CurObjCount <= this.minObjCount)
				{
					break;
				}
			}

			if (this.PoolShrinked != null)
			{
				this.PoolShrinked();
			}
		}

		#endregion

        /// <summary>
        /// 极少用;
        /// </summary>
        /// <param name="hashCode"></param>
        /// <returns></returns>
		public bool CheckObjectStatus(Int32 hashCode)
		{
			if (hashTableStatus[hashCode] == null)
			{
				return false;
			}
			else
			{
				return (bool)(hashTableStatus[hashCode]);
			}
		}

        /// <summary>
        /// 销毁;
        /// </summary>
		public void Dispose()
		{
			Type supType = typeof(System.IDisposable);
			if (supType.IsAssignableFrom(this.destType))
			{
				Int32 count = this.keyList.size - 1;
				for (Int32 index = count; index >= 0; index--)
				{
					this.DistroyOneObject(this.keyList[index]);
				}
			}

			this.hashTableStatus.Clear();
			this.hashTableObjs.Clear();
			this.keyList.Clear();
		}

		#region property
		public int MinObjCount
		{
			get
			{
				return this.minObjCount;
			}
		}

		public int MaxObjCount
		{
			get
			{
				return this.maxObjCount;
			}
		}

		public int CurObjCount
		{
			get
			{
				return this.keyList.size;
			}
		}

		public int IdleObjCount
		{
			get
			{
				lock (this)
				{
					return this.GetIdleObjCount();
				}
			}
		}

		private int GetIdleObjCount()
		{
			return this.mIdleObjCount;
		}
		#endregion

        #region Debug
		public string PrintPoolStatus()
		{
			string log = string.Format("Object Type:{0} -> (Current:{1}/Idle:{2}/Min:{3}/Max:{4})", destType.ToString(), CurObjCount, IdleObjCount, minObjCount, maxObjCount);
			return log;
		}

		public void SeeEveryNode()
		{	
			for (Int32 i = 0; i < keyList.size; ++i)
			{
				if ((bool)hashTableStatus[keyList[i]] == true)
				{
					System.Object obj = hashTableObjs[keyList[i]];
					UnityCustomUtil.CustomLog(obj.ToString());
				}
				else
				{
					System.Object obj = hashTableObjs[keyList[i]];
                    UnityCustomUtil.CustomLog(obj.ToString());
				}
			}
		}

		public bool isInPool(Int32 hashCode)
		{
			bool inPool = false;
			if (keyList == null)
			{
				return false;
			}
			for (Int32 i = 0; i < keyList.Count; ++i)
			{
				if (keyList[i] == hashCode)
				{
					inPool = true;
					break;
				}
			}
			return inPool;
		}

		public bool isIdle(Int32 hashCode)
		{
			if (isInPool(hashCode) == false)
			{
                UnityCustomUtil.CustomLogError("Found Object not in pool when you want to check this object is idle in pool >_<");
				return false;
			}
			bool result = (bool)hashTableStatus[hashCode];
			return result;
		}

		public void RestPool()
		{
			Shrink();
		}

        #endregion
	}
    #endregion
}
