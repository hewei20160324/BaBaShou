using UnityEngine;
using System.Collections;
using CustomNetwork;

namespace CustomGame
{
    public class RoomInfoProtocol : BaseProtocol
    {
        public string mRoomName;
        // public 

        #region 读取、保存
        public override void Serial(BitMemStream bit)
        {
            if (bit == null) return;
            bit.Serial(ref mRoomName);
        }
        #endregion
    }
}