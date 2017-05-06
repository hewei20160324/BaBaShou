using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.Net;
using CustomUtil;
using CustomNetwork;
using System;
using System.Threading;

/// <summary>
/// BeginReceiveData:开始接受数据;
/// 不用开线程的方式了，开了线程运行过程中修改代码，再次启动游戏容易未响应;
/// udp仅仅用于创建房间逻辑，不用于游戏;
/// </summary>

public class CreatRoomClient : Singleton<CreatRoomClient>, IManagerProtocal
{
    Socket m_broadcastSocket = null;
    IPEndPoint m_broadcastIep = null;

    /// <summary>
    /// 接收缓冲区的大小;
    /// </summary>
    private const int BUF_SIZE = 10240;
    /// <summary>
    /// 接收和和发送队列长度;
    /// </summary>
    private const int QUEUE_SIZE = 400;
    private const int RECV_QUEUE_CACHE_SIZE = 400;//100;

    public byte[] _RecvBuffer = new byte[BUF_SIZE];

    private readonly SafeQueue _RecvQueue;
    private readonly SafeQueue _TmpRecvQueue;

    /// <summary>
    /// 接受数据的线程;
    /// </summary>
    private Thread mReceiveThread = null;
    /// <summary>
    /// 表示网络状态;
    /// </summary>
    private NetState _NetState = NetState.NS_Null;

    /// <summary>
    /// 异步通知;
    /// </summary>
    private bool m_InformDisconnection = false;
    private NetState m_InformState = NetState.NS_Null;

    public CreatRoomClient()
    {
        _RecvQueue = new SafeQueue(QUEUE_SIZE);
        _TmpRecvQueue = new SafeQueue(RECV_QUEUE_CACHE_SIZE);
    }

    ~CreatRoomClient()
    {
        if (mReceiveThread != null)
        {
            mReceiveThread.Abort();
            mReceiveThread = null;
        }
    }

    #region IManagerProtocal
    public void Init()
    {
    }

    public void Clear()
    {
        CloseSocket();
    }
    #endregion

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

    void ReceiveBroadcastData()
    {
        while (true)
        {
            try
            {
                while(_NetState == NetState.State_Connected && _RecvQueue.HasIdle && m_broadcastSocket != null)
                {
                    if(!m_broadcastSocket.Poll(0, SelectMode.SelectRead))
                    {
                        break;
                    }

                    EndPoint endPoint = m_broadcastIep;
                    int bytesRead = m_broadcastSocket.ReceiveFrom(_RecvBuffer, ref endPoint);
                    if (bytesRead > 0)
                    {
                        int msgLength = BitConverter.ToInt32(_RecvBuffer, 0);
                        msgLength = IPAddress.NetworkToHostOrder(msgLength);

                        // make sure receive buff size is ok!;
                        if (msgLength > _RecvBuffer.Length - 4)
                        {
                            UnityCustomUtil.CustomLogWarning(string.Format("socket receive buff size expand! new size is {0}", _RecvBuffer.Length.ToString()));
                            if (!!m_InformDisconnection)
                            {
                                InformDisconnected(NetState.State_DisconRecvErr1);
                            }
                            return;
                        }

                        BitMemStream memStream = GetRecvMsg();
                        if(memStream == null)
                        {
                            UnityCustomUtil.CustomLogWarning("GetRecvMsg is null !!!");
                            if (!!m_InformDisconnection)
                            {
                                InformDisconnected(NetState.State_DisconRecvErr1);
                            }
                            return;
                        }

                        memStream.LoadBytes(_RecvBuffer, 4, msgLength);
                        _RecvQueue.Push(memStream);
                    }
                    else
                    {
                        UnityCustomUtil.CustomLogWarning("recv 0 bytes from peer ");
                        if (!!m_InformDisconnection)
                        {
                            InformDisconnected(NetState.State_DisconRecvErr1);
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                if (!!m_InformDisconnection)
                {
                    InformDisconnected(NetState.State_DisconRecvErr2);
                }
                UnityCustomUtil.CustomLogWarning("RecvCallback ERROR " + ex.ToString());
            }

            Thread.Sleep(10);
        }
    }

    void SetNetState(NetState state)
    {
        lock (this)
        {
            _NetState = state;
        }
    }

    /// <summary>
    /// 在异步回调里通知连接失败/断开;
    /// </summary>
    /// <param name="state"></param>
    void InformDisconnected(NetState state)
    {
        lock (this)
        {
            m_InformDisconnection = true;
            m_InformState = state;
        }
    }

    void CreatSocket()
    {
        CloseSocket();
        m_broadcastSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        m_broadcastIep = new IPEndPoint(IPAddress.Any, 8000);
        m_broadcastSocket.Bind(m_broadcastIep);
        // 这个很关键，否则接受线程可能出现关不掉的问题，从而导致Unity引擎卡死;
        m_broadcastSocket.Blocking = false;

        SetNetState(NetState.State_Connected);
    }

    void CloseSocket(NetState netState = NetState.State_ClientClose)
    {
        SetNetState(netState);

        if (m_broadcastSocket == null) return;
        m_broadcastSocket.Close();
        m_broadcastSocket = null;
        
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

    void MakeSureSocketReady()
    {
        if (m_broadcastSocket == null || m_broadcastIep == null)
        {
            CreatSocket();
        }
    }

    /// <summary>
    /// 开启接受udp机制;
    /// </summary>
    public void BeginReceiveData()
    {
        MakeSureSocketReady();

        if (m_broadcastSocket == null || _RecvBuffer == null || m_broadcastIep == null)
        {
            UnityCustomUtil.CustomLogError("Create UDP Error !!!");
            CloseSocket();
            return;
        }
        
        if (mReceiveThread == null)
        {
            mReceiveThread = new Thread(new ThreadStart(ReceiveBroadcastData));
            mReceiveThread.Start();
        }
    }

    public void OnUpdate()
    {
        if(m_InformDisconnection == false)
        {
            return;
        }

        NetState tempNetState = m_InformState;
        lock (this)
        {
            m_InformDisconnection = false;
            m_InformState = NetState.NS_Null;
        }
        CloseSocket(tempNetState);
        // 直接重连;
        CreatSocket();
    }

    public bool IsConnected()
    {
        return (m_broadcastSocket != null && m_broadcastSocket.Connected);
    }

    /// <summary>
    /// 获取接受到的数据;
    /// </summary>
    /// <returns></returns>
    public BitMemStream GetReceiveData()
    {
        BitMemStream msg = null;
        _RecvQueue.Pop(ref msg);

        if (msg == null)
        {
            return null;
        }
        _TmpRecvQueue.Push(msg);
        return msg;
    }
}
