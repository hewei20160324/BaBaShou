using System;
using System.Collections;
using System.Collections.Generic;
using CustomUtil;

/// <summary>
/// 事件机制:设计目的，解功能耦合，主要是逻辑与显示的耦合等;
/// AddProcessFunction:注册;
/// RemoveProcessFunction:取消注册;
/// AddEvent:抛事件;
/// ImmediatelyActiveEvent:立即执行掉指定事件;
/// </summary>

namespace Common
{
    public class ClientEventManager : Singleton<ClientEventManager>
	{
		static Int32 MaxEventCount = 4096;
		static Int32 EventCount = 512;
		static Int32 snMaxEventInvokePF = 100;

        // 对象池;
        ObjectPool mEventPool = new ObjectPool();

        /// <summary>
        /// 事件的关注处理回调列表.;
        /// </summary>
        public delegate void VoidDelegate(ClientEvent eve);

		/// <summary>
		/// 所有的事件，和其处理函数;
		/// </summary>
		List<VoidDelegate>[] mAllEvents = null;
		/// <summary>
		/// 被触发的事件列表;
		/// </summary>
		List<ClientEvent> mProcessEventsList = null;

		List<ClientEvent> GetProcessEvent()
		{
			if (mProcessEventsList == null)
			{
				mProcessEventsList = new List<ClientEvent>();
			}
			return mProcessEventsList;
		}

		public void ClearProcessEvent()
		{
			List<ClientEvent> eventList = GetProcessEvent();
			if (null == eventList)
			{
				return;
			}

			for (int i = 0; i < eventList.Count; i++)
			{
				ClientEvent proEvent = eventList[i];
				GameEventID type = proEvent.GetID();

				if (type >= 0 && type < GameEventID.GEIdCount)
				{
					List<VoidDelegate> mFuns = mAllEvents[(int)type];
					if (mFuns == null)
					{
						continue;
					}
					for (int index = 0; index < mFuns.Count; index++)
					{
						VoidDelegate function = mFuns[index];
						if (function != null)
						{
							function(proEvent);
						}
					}
				}

				DestoryEvent(eventList[i]);
			}
			mProcessEventsList.Clear();
		}

		public ClientEventManager()
		{
			Init();
		}

		public int IdleEventCount()
		{
			return mEventPool.MaxObjCount - mEventPool.CurObjCount;
		}

		public void ResCache()
		{
			if (mEventPool != null)
			{
				mEventPool.RestPool();
			}
		}
		
		void Init()
		{
			mAllEvents = new List<VoidDelegate>[(int)GameEventID.GEIdCount];
			mEventPool.Initialize(typeof(ClientEvent), null, 50, MaxEventCount);
            //snMaxEventInvokePF = xx;
        }

		/// <summary>
		/// 从对象池中得到一个Event
		/// </summary>
		/// <returns></returns>
		public ClientEvent AddEvent(GameEventID type)
		{
			ClientEvent eve = mEventPool.RentObject() as ClientEvent;
			if( eve != null )
			{
				eve.SetID(type);
				List<ClientEvent> eventlist = GetProcessEvent();
				eventlist.Add(eve);
			}
			else
			{
				UnityCustomUtil.CustomLogError("CreatEvent Faild " + type);
			}


			return eve;
		}

        /// <summary>
        /// 从对象池中得到一个Event，可设置是否立即触发以及是否携带参数
        /// </summary>
        /// <returns></returns>
        public ClientEvent AddEvent(GameEventID type, bool isImmediately = true, params object[] parameters)
        {
            ClientEvent eve = mEventPool.RentObject() as ClientEvent;
            if (eve != null)
            {
                eve.SetID(type);
                if (parameters != null)
                {
                    for (Int32 i = 0; i < parameters.Length; i++)
                    {
                        eve.AddParameter(parameters[i]);
                    }
                }

                List<ClientEvent> eventlist = GetProcessEvent();
                eventlist.Add(eve);

                if (isImmediately)
                {
                    ImmediatelyActiveEvent(eve);
                }
            }
            else
            {
                UnityCustomUtil.CustomLogError("CreatEvent Failed " + type);
            }

            return eve;
        }

        public void RemoveEvent(ClientEvent eve)
        {
            List<ClientEvent> eventList = GetProcessEvent();
            if (eventList == null)
            {
                return;
            }
            eventList.Remove(eve);
        }

		/// <summary>
		/// 对象池回收ClientEvent，并不是真正的销毁;
		/// </summary>
		/// <param name="eve"></param>
		/// <returns></returns>
		private void DestoryEvent(ClientEvent eve)
		{
			mEventPool.GiveBackObject(eve.GetHashCode());
		}


		/// <summary>
		/// 添加事件的关注处理函数;
		/// </summary>
		/// <param name='type' 关注的事件ID >
		/// Type.
		/// </param>
		/// <param name='function' 该事件的处理函数 >
		/// Function.
		/// </param>
		public void AddProcessFunction(GameEventID type, VoidDelegate function)
		{
			if (type >= 0 && type < GameEventID.GEIdCount)
			{
				List<VoidDelegate> funs = mAllEvents[(int)type];
				if (funs == null)
				{
					funs = new List<VoidDelegate>();
					mAllEvents[(int)type] = funs;
				}

                if (funs.Contains(function) == true)
                {
                    UnityCustomUtil.CustomLogWarning("AddProcessFunction error ================= repeat!" + type);
                    return;
                }

				funs.Add(function);
			}
		}


		/// <summary>
		/// 删除事件的关注处理函数;
		/// </summary>
		/// <param name="type"></param>
		/// <param name="function"></param>
		public void RemoveProcessFunction(GameEventID type, VoidDelegate function)
		{
			if (type >= 0 && type < GameEventID.GEIdCount)
			{
				List<VoidDelegate> funs = mAllEvents[(int)type];
				if (funs == null)
				{
					return;
				}
				if (funs.Contains(function) == true )
				{
                    funs.Remove(function);
				}
			}
		}

		/// <summary>
		/// 指定一个事件被触发;
		/// </summary>
		/// <param name='type' 被触发的事件ID >
		/// Type.
		/// </param>
		/// <param name='parameters' 处理函数需要的参数 >
		/// Parameters.
		/// </param>
		public void ImmediatelyActiveEvent(ClientEvent eve)
		{
			if (eve == null)
			{
				return;
			}

			ActiveFunction(eve);
			// 事件已经触发，先设置删除标志 ;
			eve.DeleteFlg = true;
			
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="type"></param>
		private void ActiveFunction(ClientEvent proEvent)
		{
			if (proEvent == null)
			{
				return;
			}

			GameEventID type = proEvent.GetID();
			if (type >= 0 && type < GameEventID.GEIdCount)
			{
				List<VoidDelegate> funs = mAllEvents[(int)type];
				if (funs == null)
				{
					return;
				}

				for (int index = 0; index < funs.Count; index++)
				{
					VoidDelegate function = funs[index];
					if (function != null)
					{
						try
						{
							function(proEvent);
						}
						catch (System.Exception ex)
						{
							UnityCustomUtil.CustomLogError("ProcessEvent Exception: " + ex.ToString());
						}
					}
				}
			}
		}

		/// <summary>
		/// 把大于EventCount的事件处理完，返回处理的事件数;
		/// </summary>
		private Int32 CheckEventCount()
		{
			Int32 retCount = 0;
			List<ClientEvent> processEvents = GetProcessEvent();
			if (processEvents == null)
			{
				return retCount;
			}
			Int32 eventCount = processEvents.Count - EventCount;
			for (Int32 i = 0; i < eventCount; i++)
			{
				ClientEvent proEvent = processEvents[0];
				if (proEvent == null)
				{
                    processEvents.RemoveAt(0);
					continue;
				}

				if (proEvent.DeleteFlg)
				{
					DestoryEvent(proEvent);
                    processEvents.RemoveAt(0);
					continue;
				}
				
				ActiveFunction(proEvent);
				DestoryEvent(proEvent);
                processEvents.RemoveAt(0);
                retCount++;
			}

			return retCount;
		}

		public void OnUpdate()
		{
			try
			{
				List<ClientEvent> processEvents = GetProcessEvent();
				if (processEvents == null)
				{
					return;
				}

				Int32 funcInvokeCount = CheckEventCount();

				float fCurTime = UnityEngine.Time.time;
                //遍历时有可能增加数组元素;
				for (int i = 0; i < processEvents.Count; )
				{
					if (funcInvokeCount >= snMaxEventInvokePF)
					{
						return;
					}

					ClientEvent proEvent = processEvents[i];
					if (proEvent == null)
					{
                        processEvents.RemoveAt(i);
						continue;
					}

					if (proEvent.DeleteFlg)
					{
						DestoryEvent(proEvent);
                        processEvents.RemoveAt(i);
						continue;
					}

					float timelock = proEvent.GetTimelock();
					if ((timelock > fCurTime))
					{
						i++;
						continue;
					}

					ActiveFunction(proEvent);
					DestoryEvent(proEvent);
                    processEvents.RemoveAt(i);
                    funcInvokeCount++;
				}
			}
			catch (System.Exception ex)
			{
				UnityCustomUtil.CustomLogError("processEvent Exception: " + ex.ToString());	
			}
		}

	}

}