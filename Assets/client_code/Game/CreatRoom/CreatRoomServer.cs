using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.Net;
using CustomUtil;
using CustomNetwork;
using System;

public class CreatRoomServer : Singleton<CreatRoomServer>, IManagerProtocal
{
    private Socket m_broadcastSocket = null;
    private IPEndPoint m_broadcastIep = null;
    private BitMemStream _SendTmpStream = null;

    #region IManagerProtocal
    public void Init()
    {

    }

    public void Clear()
    {
        CloseSocket();
    }
    #endregion

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

    public void SendRoomInfo()
    {
        MakeSureSocketReady();
        if (m_broadcastSocket == null || m_broadcastIep == null || _SendTmpStream == null)
        {
            return;
        }

        try
        {
            _SendTmpStream.FixMsg();
            byte[] buffer = _SendTmpStream.Buffer();
            int msgLength = (int)_SendTmpStream.Length() + 4;
            int sendRet = m_broadcastSocket.SendTo(buffer, msgLength, SocketFlags.None, m_broadcastIep);
            if (sendRet < 0)
            {
                UnityCustomUtil.CustomLogError("UDP Send Data Error!");
                CreatSocket();
            }
        }
        catch (System.Exception ex)
        {
            UnityCustomUtil.CustomLogWarning("SendRoomInfo ERROR " + ex.ToString());
        }
    }

    void CreatSocket()
    {
        CloseSocket();
        m_broadcastSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        // 允许广播;
        m_broadcastSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
        // 广发端口;
        m_broadcastIep = new IPEndPoint(IPAddress.Broadcast, 8000);
    }

    void CloseSocket()
    {
        if (m_broadcastSocket == null) return;
        m_broadcastSocket.Close();
        m_broadcastSocket = null;
    }

    void MakeSureSocketReady()
    {
        if (m_broadcastSocket == null || m_broadcastIep == null)
        {
            CreatSocket();
        }
    }
}
