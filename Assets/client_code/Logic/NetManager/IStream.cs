using System;
using System.Collections.Generic;

using System.Text;
using System.IO;

namespace CustomNetwork
{
    public abstract class IStream
    {
        private bool _InputStream;
        public static int _AllocateCount = 0;

        public IStream(bool inputStream)
        {
            _InputStream = inputStream;
            _AllocateCount++;
        }

        public bool IsReading
        {
            get { return _InputStream; }
        }

        public void SetInOut(bool inputStream)
        {
            _InputStream = inputStream;
        }

        public abstract void Serial(ref Int32 v);
        public abstract void Serial(ref UInt32 v);

        public abstract void Serial(ref Int16 v);

        public abstract void Serial(ref UInt16 v);

        public abstract void Serial(ref Byte v);

        //public abstract void Serial(ref SByte v);

        public abstract void Serial(ref bool v);

        public abstract void Serial(ref Single f);

        public abstract void Serial(ref Double d);

        public abstract void Serial(ref UInt64 v);
        //public abstract void Serial(ref Int64 v);

        public abstract void Serial(ref String b);
        public abstract void Serial(ref MemoryStream s);

        public virtual void SerialCont(ref Int32[] v) { }
        public virtual void SerialCont(ref UInt32[] v) { }
        public virtual void SerialCont(ref Boolean[] v) { }

        public abstract void SerialBuffer(ref Byte[] buf, UInt32 start, UInt32 len);
        public virtual void SerialBufferWithSize(ref Byte[] buf, UInt32 start, ref UInt32 len)
        {
            Serial(ref len);
            SerialBuffer(ref buf, start, len);
        }
        public abstract void SerialBit(ref bool bit);

        public virtual void SerialMemStream(ref MemStream v)
        {
        }


        //public void Serial<T>(ref T s) where T : class
        //{
        //    //s.Serial(this);
        //}

        /// <summary>
        /// Serials the name.
        /// 服务器发来的是UNICODE 要专门写序列化接口
        /// 角色列表专用
        /// 
        /// </summary>
        /// <param name='stream'>
        /// Stream.
        /// </param>
        public virtual void SerialUnicodeString(ref String v)
        {
            //UInt32 strLen = 0;
            //if (IsReading)
            //{
            //    v = "";
            //    Serial(ref strLen);
            //    UInt16[] temp = new UInt16[strLen];
            //    for (int index = 0; index < strLen; index++)
            //    {
            //        Serial(ref temp[index]);
            //    }
            //    for (int index = 0; index < strLen; index++)
            //    {
            //        v += ((char)Convert.ToInt32(temp[index])).ToString();
            //    }
            //}
            //else
            //{
            //    strLen = (UInt32)v.Length;
            //    Serial(ref strLen);
            //    UInt16 uchar = 0;
            //    for (int index = 0; index < strLen; index++)
            //    {
            //        uchar = Convert.ToUInt16(v[index]);
            //        Serial(ref uchar);
            //    }
            //}

        }
    }
}
