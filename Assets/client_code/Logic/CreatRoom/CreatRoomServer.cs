using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.Net;
using CustomUtil;

public class CreatRoomServer : Singleton<CreatRoomServer>, IManagerProtocal
{
    Socket m_broadcastSocket = null;
    IPEndPoint m_broadcastIep = null;

    #region IManagerProtocal
    public void Init()
    {

    }

    public void Clear()
    {
        CloseSocket();
    }
    #endregion

    public void SendRoomInfo(byte[] sendBytes)
    {
        if (sendBytes == null) return;

        MakeSureSocketReady();
        if (m_broadcastSocket == null || m_broadcastIep == null)
        {
            return;
        }
        m_broadcastSocket.SendTo(sendBytes, m_broadcastIep);
    }

    void CreatSocket()
    {
        CloseSocket();
        m_broadcastSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        // 允许广播;
        m_broadcastSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);

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
