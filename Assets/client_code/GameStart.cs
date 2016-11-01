using UnityEngine;
using System.Collections;
using CustomUtil;

public class GameStart : MonoBehaviour {

    void Awake()
    {
        GameObject.DontDestroyOnLoad(this.gameObject);
    }

	// Use this for initialization
	void Start () {
        UnityCustomUtil.CustomLog(NetworkUtil.GetLocalHostName());   
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    /// <summary>
	/// 强制暂停时，先 OnApplicationPause，后 OnApplicationFocus;
	/// 重新“启动”手机时，先OnApplicationFocus，后 OnApplicationPause;
	/// </summary>
	void OnApplicationFocus(bool isFocus)
    {
        UnityCustomUtil.CustomLog("OnApplicationFocus === " + isFocus);
#if !UNITY_EDITOR
		if (isFocus)
		{
			OnGameActive();
		}

#endif
    }

    void OnApplicationPause(bool isPause)
    {
        UnityCustomUtil.CustomLog("OnApplicationPause === " + isPause);
#if !UNITY_EDITOR
		if (isPause)
		{
			OnGamePause();
		}
#endif
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
        // 游戏失去焦距时玩家停止移动，避免在边界移动时失去焦点导致穿透空气墙，从而掉落到场景外;
//         UserEntity userEntity = EntityManager.GetInstance().GetUserEntity();
//         if (userEntity != null)
//         {
//             userEntity.StopMove();
//         }
//         GameClient.GetInstance().GetNetManager().SetAdditionalPing(xyjClient.Setting.PauseTimeOut);
    }
}
