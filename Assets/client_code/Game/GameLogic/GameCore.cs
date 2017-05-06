using UnityEngine;
using System.Collections;
using CustomUtil;
using CustomNetwork;
using Common;

namespace CustomGame
{
    public class GameCore : Singleton<GameCore>
    {
        NetManager mNetManager = null;
        GameStateManager mGameStateManager = null;

        #region GET
        public NetManager GetNetManager() { return mNetManager; }
        public GameStateManager GetGameStateManager() { return mGameStateManager; }
        #endregion

        public void Init()
        {
            mNetManager = new NetManager();
            RoomManager.GetInstance().Init();
            ConfigManager.GetInstance().Init();
        }

        public void OnUpdate()
        {
            // 事件机制;
            ClientEventManager.GetInstance().OnUpdate();
            // 网络;
            if (mNetManager != null) mNetManager.UpdateNetwork();
            // 游戏逻辑;
            if (mGameStateManager != null) mGameStateManager.OnUpdate();
            // UDP机制;
            RoomManager.GetInstance().OnUpdate();
        }
    }
}
