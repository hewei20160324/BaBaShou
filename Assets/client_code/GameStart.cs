using UnityEngine;
using System.Collections;
using CustomUtil;
using CustomGame;

public class GameStart : MonoBehaviour {

    void Awake()
    {
        GameObject.DontDestroyOnLoad(this.gameObject);
    }

	// Use this for initialization
	void Start () {
        //UnityCustomUtil.CustomLog(NetworkUtil.GetLocalHostName());   
        GameCore.GetInstance().Init();
    }
	
	// Update is called once per frame
	void Update () {
        GameCore.GetInstance().OnUpdate();
    }

    /// <summary>
	/// 强制暂停时，先 OnApplicationPause，后 OnApplicationFocus;
	/// 重新“启动”手机时，先OnApplicationFocus，后 OnApplicationPause;
	/// </summary>
	void OnApplicationFocus(bool isFocus)
    {
        //UnityCustomUtil.CustomLog("OnApplicationFocus === " + isFocus);
#if !UNITY_EDITOR
		if (isFocus)
		{
			OnGameActive();
		}

#endif
    }

    void OnApplicationPause(bool isPause)
    {
        //UnityCustomUtil.CustomLog("OnApplicationPause === " + isPause);
#if !UNITY_EDITOR
		if (isPause)
		{
			OnGamePause();
		}
#endif
    }

    void OnApplicationQuit()
    {
        //RoomManager.GetInstance().Clear();
    }

    /// <summary>
    /// 游戏暂停后重新激活;
    /// </summary>
    void OnGameActive()
    {
        //HardWareQuality.SetResolution(false);
    }

    /// <summary>
	/// 游戏暂停;
	/// </summary>
	void OnGamePause()
    {
//         GameClient.GetInstance().GetNetManager().SetAdditionalPing(xyjClient.Setting.PauseTimeOut);
    }
}
