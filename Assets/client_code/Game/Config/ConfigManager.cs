using UnityEngine;
using System.Collections;
using CustomUtil;
using System;

namespace CustomGame
{
    public class ConfigManager : Singleton<ConfigManager>, IManagerProtocal
    {
        public static ClientConfig gameConfig = null;

        #region interface
        public void Init()
        {
            TextAsset textAssets = Resources.Load("config/client_config") as TextAsset;
            if(textAssets == null || textAssets.bytes == null)
            {
                gameConfig = new ClientConfig();
            }
            else
            {
                gameConfig = CommonUtil.ReadFromXmlString<ClientConfig>(textAssets.bytes);
            }
        }

        public void Clear()
        {
            //throw new NotImplementedException();
        }
        #endregion

    }
}
