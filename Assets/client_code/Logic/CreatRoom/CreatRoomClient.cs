using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.Net;
using CustomUtil;

public class CreatRoomClient : Singleton<CreatRoomClient>, IManagerProtocal
{
    Socket m_broadcastSocket = null;
    IPEndPoint m_broadcastIep = null;
    byte[] receiveBytes = new byte[10240];

    #region IManagerProtocal
    public void Init()
    {

    }

    public void Clear()
    {
        CloseSocket();
    }
    #endregion

    void ReceiveBroadcastData()
    {
        if(m_broadcastSocket == null || receiveBytes == null || m_broadcastIep == null)
        {
            return;
        }
        EndPoint endPoint = (EndPoint)m_broadcastIep;
        m_broadcastSocket.ReceiveFrom(receiveBytes, ref endPoint);
        string receiveStr = System.Text.Encoding.UTF8.GetString(receiveBytes);
        UnityCustomUtil.CustomLogWarning(receiveStr);
    }

    void CreatSocket()
    {
        CloseSocket();
        m_broadcastSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        m_broadcastIep = new IPEndPoint(IPAddress.Any, 8000);
        m_broadcastSocket.Bind(m_broadcastIep);
    }

    void CloseSocket()
    {
        if (m_broadcastSocket == null) return;
        m_broadcastSocket.Close();
        m_broadcastSocket = null;
    }
}
