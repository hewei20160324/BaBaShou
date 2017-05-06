using UnityEngine;
using System.Collections;
using CustomUtil;
using CustomNetwork;

public class SocketTestUI : MonoBehaviour {

    UILabel mClientLabel = null;
    UILabel mServerLabel = null;
    UILabel mReceiveLabel = null;
    UILabel mInputLabel = null;

    void Awake()
    {
        GameObject menuObj = UnityCustomUtil.GetChild(gameObject, "menu");
        if (menuObj == null) return;

        UIButton btn = UnityCustomUtil.GetChildComponent<UIButton>(menuObj, "mode0");
        if (btn != null) EventDelegate.Set(btn.onClick, OnClientClick);
        mClientLabel = UnityCustomUtil.GetChildComponent<UILabel>(menuObj, "mode0/label");

        btn = UnityCustomUtil.GetChildComponent<UIButton>(menuObj, "mode1");
        if (btn != null) EventDelegate.Set(btn.onClick, OnServerClick);
        mServerLabel = UnityCustomUtil.GetChildComponent<UILabel>(menuObj, "mode1/label");

        btn = UnityCustomUtil.GetChildComponent<UIButton>(menuObj, "send");
        if (btn != null) EventDelegate.Set(btn.onClick, OnSendClick);

        mInputLabel = UnityCustomUtil.GetChildComponent<UILabel>(menuObj, "input/content");
        mReceiveLabel = UnityCustomUtil.GetChildComponent<UILabel>(menuObj, "receive_data/content");
        if (mReceiveLabel != null) mReceiveLabel.text = string.Empty;

        UIInput input = UnityCustomUtil.GetChildComponent<UIInput>(menuObj, "input");
        if (input != null) EventDelegate.Set(input.onSubmit, OnInputSubmit);
    }

    void Start()
    {
        FlushClientLabel();
        FlushServerLabel();
    }

    #region Click Event;
    void OnClientClick()
    {
        RoomManager.GetInstance().openClient = !RoomManager.GetInstance().openClient;
        FlushClientLabel();
    }

    void OnServerClick()
    {
        RoomManager.GetInstance().openServer = !RoomManager.GetInstance().openServer;
        FlushServerLabel();
    }

    void OnSendClick()
    {
        OnInputSubmit();
        CreatRoomServer.GetInstance().SendRoomInfo();
    }

    void OnInputSubmit()
    {
        if(mInputLabel == null)
        {
            return;
        }
        BitMemStream bitMemStream = CreatRoomServer.GetInstance().GetSendMsg();
        string str = mInputLabel.text;
        if (bitMemStream != null) bitMemStream.Serial(ref str);
    }
    #endregion

    void FlushClientLabel()
    {
        if (mClientLabel == null) return;
        mClientLabel.text = RoomManager.GetInstance().openClient ? "StopClient" : "StartClient";
    }

    void FlushServerLabel()
    {
        if (mServerLabel == null) return;
        mServerLabel.text = RoomManager.GetInstance().openServer ? "StopServer" : "StartServer";
    }
	
	// Update is called once per frame
	void Update () {
        BitMemStream memStream = CreatRoomClient.GetInstance().GetReceiveData();
        if(memStream == null)
        {
            return;
        }

        if(mReceiveLabel == null)
        {
            return;
        }

        string str = string.Empty;
        memStream.Serial(ref str);
        mReceiveLabel.text = str;
    }
}
