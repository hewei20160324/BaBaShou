using UnityEngine;
using System.Collections;
using CustomUtil;
using Common;
//using ProtoBuf;

namespace CustomNetwork
{
    public class NetManager : ThreadReviceConnection
    {
        public const byte SYSTEM_PING_CODE = 0;
        public const byte SYSTEM_PROTO_CODE = 1;
        public const byte SYSTEM_QUITGAME_CODE = 2;

        /// <summary>
        /// 需要抽成配置;
        /// </summary>
        const float PING_CYCLE_TIME = 5.0f;
        const float NET_TIME_OUT = 10.0f;

        /// <summary>
        /// 用于发送心跳;
        /// </summary>
        float mLastSendPingTime = 0;

        /// <summary>
        /// 用于判断网络超时;
        /// </summary>
        float mLastRecvPkgTime = 0;

        /// <summary>
        /// 是否需要处理网络超时;
        /// </summary>
        bool mCheckNetConnection = false;
        public bool CheckNetConnection
        {
            get { return mCheckNetConnection; }
            set { mCheckNetConnection = value; }
        }

        sealed protected override int OnNewMessage(ref BitMemStream msgin)
        {
            byte code = 0;
            msgin.Serial(ref code);//解析出消息类型;
            switch (code)
            {
                case SYSTEM_PING_CODE://ping消息;
                    {
                        //                     int sendTick = 0;
                        //                     msgin.Serial(ref sendTick);
                        //                     _LastPong = GetTickTime();
                        //                     int pingTime = (Int32)_LastPong - (Int32)sendTick;
                        //                     if (pingTime > 0)
                        //                     {
                        //                         _PingTime = (UInt32)(pingTime);
                        //                     }
                        //                     int serverTime = 0;
                        //                     msgin.Serial(ref serverTime);
                        //                     ClientCommon.SetServerCurTime(serverTime);
                        CheckNetConnection = false;
                    }
                    break;
                case SYSTEM_PROTO_CODE://新版pb协议消息;
                    {
                        try
                        {
                            //GleoMessageManager.GetInstance().ExecuteGleoMessage(msgin);
                        }
                        catch (System.Exception ex)
                        {
                            UnityCustomUtil.CustomLogError(ex.ToString());
                        }
                    }
                    break;
                case SYSTEM_QUITGAME_CODE://提示消息;
                    try
                    {
                        //DecodeQuitNotice(ref msgin);
                    }
                    catch (System.Exception ex)
                    {
                        UnityCustomUtil.CustomLogError(ex.Message);
                    }
                    break;
                default://暂且没有其他类型的消息;
                    UnityCustomUtil.CustomLogError("unknown type " + code.ToString());
                    return -1;

            }
            return 0;
        }

        /// <summary>
        /// 周期发送心跳请求;
        /// </summary>
        void SendPingMessageCycleTime()
        {
            float delteTick = GetTickTime() - mLastSendPingTime;
            if (delteTick > PING_CYCLE_TIME)
            {
                if (IsConnected())
                {
                    SendPingMessage();
                    mLastSendPingTime = GetTickTime();
                }
            }
        }

        /// <summary>
        /// 检测网络;
        /// </summary>
        void CheckNetConnectionUpdate()
        {
            if(mCheckNetConnection == false)
            {
                return;
            }

            // 根据上一次收包的时间，判断超时，断开连接;
            if ((GetTickTime() - mLastRecvPkgTime) > NET_TIME_OUT)
            {
                Disconnected(NetState.State_DisconTimeout);
            }
        }

        /// <summary>
		/// 处理当前接收到的所有消息，并根据条件发送消息，每帧调用一次,;
		/// 当帧数低的时候会导致逻辑错误 ;
		/// </summary>
		/// <returns></returns>
		public bool UpdateNetwork()
        {
            if (!UpdateConnection())
            {
                return false;
            }

            SendPingMessageCycleTime();
            CheckNetConnectionUpdate();
            return true;
        }

        float GetTickTime()
        {
            return ClientTime.GetInstance().CurTime;
        }

        /// <summary>5
		/// 发送一个Ping消息
		/// </summary>
		void SendPingMessage()
        {
            BitMemStream pingMessage = GetSendMsg();

            byte code = SYSTEM_PING_CODE;
            pingMessage.Serial(ref code);

            // uint curTick = ClientTime.GetInstance().ServerTime;
            // pingMessage.Serial(ref curTick);
            // pingMessage.Serial(ref curTick);

            CheckNetConnection = true;
            mLastRecvPkgTime = GetTickTime();

            SendSockMsg(pingMessage);
        }

        // 被动网络断连(网络异常、超时);
        protected override void OnDisconnected(CustomNetwork.NetState state)
        {
            try
            {
                CheckNetConnection = false;

                // 非客户端强制断连才需要提示;
                if (state != CustomNetwork.NetState.State_ClientClose)
                {
                    UnityCustomUtil.CustomLogWarning("FS Close Connection === " + state);
                    // T3GameClient.ClientEventManager.GetInstance().AddEvent(GameEventID.GEFSDisconnect);
                }
            }
            catch (System.Exception ex)
            {
                UnityCustomUtil.CustomLogWarning("OnDisconnected: " + ex.ToString());
            }
        }

        /// <summary>
		/// 客户端主动断开服务器其连接;
		/// </summary>
		public override void DisconnectServer()
        {
            try
            {
                base.DisconnectServer();
                CheckNetConnection = false;
            }
            catch (System.Exception ex)
            {
                UnityCustomUtil.CustomLogWarning("DisconnectServer: " + ex.ToString());
            }
        }

        protected override void OnConnectedServer()
        {
            UnityCustomUtil.CustomLog("OnConnectedServer ========");
            // SendLoginMessage(T3GameClient.LoginManager.GetInstance().LoginCookie);
        }

        /// <summary>
        /// socket连接阶段的异常断开;
        /// </summary>
        protected override void OnConnectedServerFailed()
        {
            // 断线重连时走断线重连机制;
            //             GameStateType staeType = GameStateManager.GetInstance().GetCurrentStateType();
            //             if ((staeType == GameStateType.ELoadScene || staeType == GameStateType.EMainGame) && ReturnLoginManagerEx.GetInstance().IsReLoginInScence)
            //             {
            //                 T3GameClient.ClientEventManager.GetInstance().AddEvent(Common.GameEventID.GELoadingTimeOut);
            //                 return;
            //             }
            // 
            //             // 非断线重连时连接失败就直接弹框提示，然后返回登录界面;
            //             T3GameClient.ClientEventManager.GetInstance().AddEvent(GameEventID.emEvent_Login_ConnectFsFailed);
            CheckNetConnection = false;
        }
    }
}

