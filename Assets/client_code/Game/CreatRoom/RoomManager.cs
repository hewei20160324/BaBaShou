using UnityEngine;
using System.Collections;
using CustomUtil;

public class RoomManager : Singleton<RoomManager>, IManagerProtocal
{
    bool mIsOpenClient = false;
    bool mIsOpenServer = false;

    const float SEND_DATA_INTERVAL = 5.0f;
    float mSendTime = 0;

    #region IManagerProtocal
    public void Init()
    {
        CreatRoomClient.GetInstance().Init();
        CreatRoomServer.GetInstance().Init();
    }

    public void Clear()
    {
        CreatRoomClient.GetInstance().Clear();
        CreatRoomServer.GetInstance().Clear();
    }
    #endregion

    #region Property
    public bool openClient
    {
        get { return mIsOpenClient; }
        set
        {
            if(value == mIsOpenClient)
            {
                return;
            }
            mIsOpenClient = value;
            if(mIsOpenClient == false)
            {
                CreatRoomClient.GetInstance().Clear();
            }
            else
            {
                CreatRoomClient.GetInstance().BeginReceiveData();
            }
        }
    }

    public bool openServer
    {
        get { return mIsOpenServer; }
        set
        {
            if (value == mIsOpenServer)
            {
                return;
            }
            mIsOpenServer = value;
            if (mIsOpenServer == false)
            {
                CreatRoomServer.GetInstance().Clear();
            }
            else
            {
//                 CreatRoomServer.GetInstance().SendRoomInfo();
//                 mSendTime = Time.time + SEND_DATA_INTERVAL;
            }
        }
    }

    #endregion

	public void OnUpdate()
    {
        CreatRoomClient.GetInstance().OnUpdate();
//         if (openServer == false || Time.time < mSendTime)
//         {
//             return;
//         }
//         CreatRoomServer.GetInstance().SendRoomInfo();
//         mSendTime = Time.time + SEND_DATA_INTERVAL;
    }
}
