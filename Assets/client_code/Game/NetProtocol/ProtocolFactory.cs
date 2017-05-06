using UnityEngine;
using System.Collections;
using CustomUtil;
using CustomNetwork;

namespace CustomGame
{
    public abstract class BaseProtocol
    {
        // 类型;
        EProtocolType mType = EProtocolType.None;
        public EProtocolType protocolType
        {
            get { return mType; }
            set { mType = value; }
        }

        // 读取、保存;
        public abstract void Serial(BitMemStream bit);
    }

    public class ProtocolFactory
    {
        public static BaseProtocol GetProtocolData(BitMemStream bit)
        {
            if (bit == null)
            {
                return null;
            }

            BaseProtocol baseProtocol = null;
            try
            {
                int type = (int)EProtocolType.None;
                bit.Serial(ref type);

                switch (type)
                {
                    case (int)EProtocolType.RoomInfo: break;
                    default: break;
                }

                if (baseProtocol == null)
                {
                    return null;
                }

                baseProtocol.Serial(bit);
                baseProtocol.protocolType = (EProtocolType)type;
            }
            catch (System.Exception ex)
            {
                UnityCustomUtil.CustomLogError(ex.ToString());
            }

            return baseProtocol;
        }
    }
}
