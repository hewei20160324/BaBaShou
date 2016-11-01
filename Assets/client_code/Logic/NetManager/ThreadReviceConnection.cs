using System;
using System.IO;
using System.Collections.Generic;

using System.Net;
using System.Net.Sockets;
using System.Threading;
using CustomUtil;

/// <summary>
/// BeginConnectServer:连接服务器;
/// DisconnectServer:客户端主动断开连接;
/// Disconnected:客户端被动断开连接;
/// SendSockMsg:主线程阻塞发送请求;
/// GetRecvMsg:获取发送数据填充BitMemStream，提高复用性;
/// </summary>

namespace CustomNetwork
{
	/// <summary>
	/// 支持用独立线程收发数据的TCP客户端;
	/// 提供异步阻塞的发送接口;
	/// 不关心具体协议;
	/// </summary>
	public class ThreadReviceConnection
	{
        /// <summary>
		/// 接收缓冲区的大小;
		/// </summary>
		private const int BUF_SIZE = 10240;
        /// <summary>
        /// 接收和和发送队列长度;
        /// </summary>
        private const int QUEUE_SIZE = 400;
        private const int RECV_QUEUE_CACHE_SIZE = 400;//100;
		/// <summary>
		/// 默认连接TCP服务端超时时间
		/// </summary>
		private const int DEFAULT_TIMEOUT = 3000;
        /// <summary>
        /// 长度数据大小;
        /// </summary>
        private const int LENGTH_DATA_SIZE = 4;

		private readonly SafeQueue _RecvQueue;
		private readonly SafeQueue _TmpRecvQueue;
		private BitMemStream _SendTmpStream = null;

		protected Socket _Socket = null;
		IAsyncResult _Connect = null;

		/// <summary>
		/// 表示网络状态;
		/// </summary>
		private NetState _NetState = NetState.NS_Null;

		/// <summary>
		/// 异步通知，用于异常主动断开连接;
		/// </summary>
		private bool m_InformDisconnection = false;
		private NetState m_InformState = NetState.NS_Null;

        /// <summary>
        /// 关闭标志位;
        /// </summary>
		public bool m_SocketCloseFlag = false;

        /// <summary>
        /// 接受数据的线程;
        /// </summary>
		Thread mReceiveThread = null;
		
        /// <summary>
        /// 接受缓存相关变量;
        /// </summary>

		public byte[] _RecvBuffer = new byte[BUF_SIZE];
        int _BufferPos = 0;
        bool _IsReadingSize = true;
        int _BytesRemaining = 0;

        public ThreadReviceConnection()
		{
			_RecvQueue = new SafeQueue(QUEUE_SIZE);
			_TmpRecvQueue = new SafeQueue(RECV_QUEUE_CACHE_SIZE);
		}

		~ThreadReviceConnection()
		{
			if (mReceiveThread != null)
			{
				mReceiveThread.Abort();
				mReceiveThread = null;
			}
		}

		public bool IsConnected()
		{
			return (_Socket != null) ? _Socket.Connected : false;
		}

		public void InitConnection()
		{
			lock (this)
			{
				if (_RecvBuffer != null)
				{
					_RecvBuffer.Initialize();
				}
				_BufferPos = 0;
				_BytesRemaining = LENGTH_DATA_SIZE;
				_NetState = NetState.State_Connected;
			}
		}

        // 连接服务器成功;
		protected virtual void OnConnectedServer() { }

        // 连接服务器失败;
		protected virtual void OnConnectedServerFailed() { }

        // 断网回调;
		protected virtual void OnDisconnected(NetState state) { }

        // 处理数据;
		protected virtual int OnNewMessage(ref BitMemStream msgin) { return 0; }

        /// <summary>
        /// 线程接受数据方法;
        /// </summary>
		void ReceiveMsg()
		{
			while (true)
			{
				try
				{
					while (_NetState == NetState.State_Connected && _Socket != null && _Socket.Poll(0, SelectMode.SelectRead))
					{
						// 有数据可读;
						int bytesRead = _Socket.Receive(_RecvBuffer, _BufferPos, _BytesRemaining, SocketFlags.None);
						if (bytesRead > 0)
						{
							_BufferPos += bytesRead;
							_BytesRemaining -= bytesRead;
							if (_BytesRemaining == 0)
							{
								if (_IsReadingSize)
								{
									_BytesRemaining = BitConverter.ToInt32(_RecvBuffer, 0);
									_BytesRemaining = IPAddress.NetworkToHostOrder(_BytesRemaining);
									_BufferPos = 0;
									_IsReadingSize = false;

                                    // make sure receive buff size is ok!;
									if (_BytesRemaining > _RecvBuffer.Length)
									{
										_RecvBuffer = new byte[_BytesRemaining + 4];
                                        UnityCustomUtil.CustomLogWarning(string.Format("socket receive buff size expand! new size is {0}", _RecvBuffer.Length.ToString()));
									}
								}
								else
								{
									BitMemStream memStream = GetRecvMsg();
									memStream.LoadBytes(_RecvBuffer, 0, _BufferPos);
									_RecvQueue.Push(memStream);
									
									_IsReadingSize = true;
									_BytesRemaining = LENGTH_DATA_SIZE;
									_BufferPos = 0;
								}
							}

						}
						else
						{
                            UnityCustomUtil.CustomLogWarning("recv 0 bytes from peer ");
							if (!m_SocketCloseFlag && !m_InformDisconnection)
							{
								InformDisconnected(NetState.State_DisconRecvErr1);
							}
						}
					}
				}
				catch (Exception e)
				{
                    UnityCustomUtil.CustomLogWarning("RecvCallback ERROR " + e.ToString());
					if (!m_SocketCloseFlag && !m_InformDisconnection)
					{
						InformDisconnected(NetState.State_DisconRecvErr2);
					}
				}
				Thread.Sleep(10);
			}
		}

        /// <summary>
        /// 每帧调用，处理接受到的数据;
        /// </summary>
		protected bool UpdateConnection()
        {
//             if (!mIsLoginServer)
//             {
//                 mIsLoginServer = true;
//                 OnConnectedServer();
//                 if (mReceiveThread == null)
//                 {
//                     mReceiveThread = new Thread(new ThreadStart(ReceiveMsg));
//                     mReceiveThread.Start();
//                 }
//                 return true;
//             }

            BitMemStream msgin;
			while (GetOneMessage(out msgin))
			{
				OnNewMessage(ref msgin);
				ReturnRecvMsg(msgin);
			}

			if (m_InformDisconnection)
			{
				NetState tempState = m_InformState;
				lock (this)
				{
					m_InformDisconnection = false;
					m_InformState = NetState.NS_Null;
				}
				Disconnected(tempState);
				return false;
			}
			return true;
		}

		void SetNetState(NetState state)
		{
			lock (this)
			{
				_NetState = state;
			}
		}

        /// <summary>
        /// 和游戏服务器建立链接 
        /// </summary>
        /// <param name="hostname"></param>
        /// <param name="port"></param>
        /// <param name="quiet">连接失败，是否需要调用回调</param>
        public bool BeginConnectServer(string hostname, int port, bool quiet = false)
		{
			try
			{
				if (_NetState == NetState.State_Initialized)
				{
					return false;
				}
				if (_NetState == NetState.State_Connected)
				{
					DisconnectServer();
				}
				SetNetState(NetState.State_Initialized);

				_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				if (_Socket != null)
				{
					UnityCustomUtil.CustomLog("connect hostname == " + hostname + "port == " + port);
					_Connect = _Socket.BeginConnect(hostname, port, new AsyncCallback(ConnectCallback), _Socket);

                    m_SocketCloseFlag = false;
					return true;
				}
				else
				{
                    UnityCustomUtil.CustomLogError("Create soket error");
					return false;
				}
			}
			catch (Exception e)
			{
                UnityCustomUtil.CustomLogWarning("connectBack=====================" + e.ToString());
				SetNetState(NetState.State_ConnectFailed);
                CloseSocket();
				if (quiet)
				{
					return false;
				}
				OnConnectedServerFailed();
				return false;
			}

		}

		public void ConnectCallback(IAsyncResult ar)
		{
			try
			{
				ar.AsyncWaitHandle.Close();
				_Connect = null;
				Socket client = (Socket)ar.AsyncState;
				client.EndConnect(ar);
				client.Blocking = false;
				if (_Socket != null)
				{
					_Socket.ReceiveTimeout = DEFAULT_TIMEOUT;
					_Socket.SendTimeout = DEFAULT_TIMEOUT;
				}
				InitConnection();
                ConnectSuccess();
            }
			catch (Exception e)
			{
                UnityCustomUtil.CustomLogWarning("connectBack=====================" + e.ToString());
				SetNetState(NetState.State_ConnectFailed);
                CloseSocket();
				OnConnectedServerFailed();
			}
		}

        private void ConnectSuccess()
        {
            OnConnectedServer();
            if (mReceiveThread == null)
            {
                mReceiveThread = new Thread(new ThreadStart(ReceiveMsg));
                mReceiveThread.Start();
            }
        }

		/// <summary>
		//客户端被动断连：超时或服务器断开;
		/// </summary>
		public void Disconnected(NetState state)
		{
			// 已经处于非连接状态，不处理;
			if (_NetState > NetState.State_Connected)
			{
				return;
			}

			try
			{
				SetNetState(state);
                CloseSocket();
				ResetCacheQueue();
				OnDisconnected(state);
			}
			catch (System.Exception ex)
			{
                UnityCustomUtil.CustomLogWarning("Disconnect Exception: " + ex.ToString());
			}
		}

		/// <summary>
		/// 客户端主动断开服务器其连接;
		/// </summary>
		public virtual void DisconnectServer()
		{
			SetNetState(NetState.State_ClientClose);
            CloseSocket();
			ResetCacheQueue();
		}

		/// <summary>
		/// 清空缓存队列;
		/// </summary>
		protected void ResetCacheQueue()
		{
			BitMemStream bs = null;

			while (_RecvQueue.Pop(ref bs))
			{
				_TmpRecvQueue.Push(bs);
			}
		}

		/// <summary>
		/// 关闭TCP连接;
		/// </summary>
		protected void CloseSocket()
		{
            m_SocketCloseFlag = true;

			if (_Socket != null)
			{
				if (_Socket.Connected)
				{
					try
					{
						_Socket.Shutdown(SocketShutdown.Both);
					}
					catch (System.Exception ex)
					{
                        UnityCustomUtil.CustomLogWarning("Shutdown Exception: " + ex.ToString());
					}
				}
				try
				{
					_Socket.Close();
				}
				catch (System.Exception ex)
				{
                    UnityCustomUtil.CustomLogWarning("_Socket Close: " + ex.ToString());
				}
			}


			if (_Connect != null)
			{
				try
				{
					_Connect.AsyncWaitHandle.Close();
				}
				catch (System.Exception ex)
				{
                    UnityCustomUtil.CustomLogWarning("AsyncWaitHandle : " + ex.ToString());
				}
				_Connect = null;
			}

			_Socket = null;

		}

		// 同步socket方式发送数据 ;
		// 异步send会导致服务器收到len=4的包，强制断开客户端连接,所以现在用同步send，性能相差无几;
		public bool SendSockMsg(BitMemStream msg)
		{
			//尝试解决断线重连崩溃;
			if (m_SocketCloseFlag || m_InformDisconnection)
			{
				return true;
			}
			if (null == _Socket || _NetState != NetState.State_Connected || !_Socket.Connected)
			{
				return false;
			}

			try
			{
				msg.FixMsg();
				Byte[] buffer = msg.Buffer();
				int msgLength = (int)msg.Length() + 4;

				int sendStart = 0;
				int sendLength = 0;
				do 
				{
					sendLength = _Socket.Send(buffer, sendStart, msgLength - sendStart, 0);
					sendStart += sendLength;
				} while (sendStart < msgLength);
			}
			catch (Exception e)
			{
				UnityEngine.Debug.LogError("send ERROR " + e.ToString());
				Disconnected(NetState.State_DisconSendErr1);
				return false;
			}
			return true;
		}

		/// <summary>
		/// 取出当前接收到的一条消息
		/// </summary>
		/// <param name="msgs"></param>
		/// <returns></returns>
		private bool GetOneMessage(out BitMemStream msg)
		{
			msg = null;
			return _RecvQueue.Pop(ref msg);
		}

		public BitMemStream GetSendMsg()
		{
			if (_SendTmpStream == null)
			{
				_SendTmpStream = new BitMemStream(false);
			}
			else
			{
				_SendTmpStream.Reset();
				if (_SendTmpStream.IsReading)
				{
					_SendTmpStream.Invert();
				}
			}
			return _SendTmpStream;
		}

		private BitMemStream GetRecvMsg()
		{
			BitMemStream memStream = null;
			if (_TmpRecvQueue.Pop(ref memStream))
			{
				memStream.Reset();
				if (!memStream.IsReading)
				{
					memStream.Invert();
				}
			}
			else
			{
				memStream = new BitMemStream(true);
			}
			return memStream;
		}

        private void ReturnRecvMsg(BitMemStream msg)
		{
			_TmpRecvQueue.Push(msg);
			msg = null;
		}

        /// <summary>
        /// 在异步回调里通知连接失败/断开
        /// </summary>
        /// <param name="state"></param>
        private void InformDisconnected(NetState state)
		{
			lock (this)
			{
				m_InformDisconnection = true;
				m_InformState = state;
			}
		}
	} // End of class Connection
}