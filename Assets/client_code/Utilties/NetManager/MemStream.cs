using System;
using System.IO;
using System.Collections.Generic;

using System.Text;
using System.Net;
using System.Net.Sockets;
using CustomUtil;

//using ProtoBuf;

namespace CustomNetwork
{

	/// <summary>
	///  创建的数据流会自动忽略掉前4个字节
	/// </summary>
	public class MemStream : IStream
	{

		protected Byte[] _Buffer;
		protected Int32 _Pos;

		/// <summary>
		///  只有 BufferToFill函数可以修改此数据，给ActionGeneric专门定义的
		/// </summary>
		protected Int32 _DataSize;


		public Byte[] Buffer()
		{
			return _Buffer;
		}

		public virtual Int32 GetPos()
		{
			return _Pos;
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="inputStream">true 往外读数据的输入流，false 往里写数据的输出流</param>
		public MemStream(bool inputStream)
			: base(inputStream)
		{
			_Pos = 4;
			_Buffer = new Byte[10240];
			_DataSize = _Buffer.Length;
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="inputStream">true 往外读数据的输入流，false 往里写数据的输出流</param>
		public MemStream(Int32 len, bool inputStream)
			: base(inputStream)
		{
			_Pos = 4;
			_Buffer = new Byte[len];
			_DataSize = _Buffer.Length;
		}

		/// <summary>
		/// 创建一个输人流，用于往外读数据
		/// </summary>
		/// <param name="buf"></param>
		/// <param name="len"></param>
		public MemStream(Byte[] buf, Int32 len, bool setPos0 = false)
			: base(true)
		{
			_Pos = setPos0 ? 0 : 4;
			_Buffer = new Byte[len];
			for (int i = 0; i < len; i++)
			{
				_Buffer[i] = buf[i];
			}
			_DataSize = _Buffer.Length;
		}

		/// <summary>
		/// 创建一个输人流，用于序列化客户端本地二进制文件
		/// </summary>
		/// <param name="buf">文件流</param>
		/// <param name="len">长度</param>
		/// <param name = "offsetPos">文件偏移位置</param>
		public MemStream(Byte[] buf, Int32 len, Int32 offsetPos)
			: base(true)
		{
			_Pos = 4;
			_Buffer = new Byte[len];
			for (int i = 0; i < len && (i + offsetPos) < buf.Length; i++)
			{
				_Buffer[i] = buf[i + offsetPos];
			}
			_DataSize = _Buffer.Length;
		}

		public MemStream(Byte[] buf)
			: base(true)
		{
			if (buf == null)
			{
				_Pos = 0;
				_Buffer = null;
				_DataSize = 0;
				return;
			}
			_Pos = 0;
			_Buffer = buf;
			_DataSize = _Buffer.Length;
		}

		public void SetData(Byte[] buf)
		{
			if (buf == null)
			{
				_Pos = 0;
				_Buffer = null;
				_DataSize = 0;
				return;
			}
			_Pos = 0;
			_Buffer = buf;
			_DataSize = _Buffer.Length;
		}

		public void Move(Int32 length)
		{
			_Pos += length;
			if (_Pos > Length())
			{
				_Pos = (Int32)Length();
			}
			if (_Pos < 0)
			{
				_Pos = 0;
			}
		}

		public bool End()
		{
			if (_Pos == (Int32)Length())
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public override void Serial(ref bool v)
		{
			if (IsReading)
			{
				v = (0 == _Buffer[_Pos]) ? true : false;
			}
			else
			{
				_Buffer[_Pos] = v ? (Byte)(1) : (Byte)(0);
			}
			_Pos += 1;
		}

		public override void Serial(ref Byte v)
		{
			if (IsReading)
			{
				v = _Buffer[_Pos];
			}
			else
			{
				_Buffer[_Pos] = v;
			}
			_Pos += 1;
		}

		public override void Serial(ref Int16 v)
		{
			if (IsReading)
			{
				v = BitConverter.ToInt16(_Buffer, _Pos);
			}
			else
			{
				//BitConverter.GetBytes(v).Reverse().CopyTo(_Buffer, _Pos);
				_Buffer[_Pos + 0] = (Byte)(v);
				_Buffer[_Pos + 1] = (Byte)(v >> 8);
			}
			_Pos += 2;
		}

		public override void Serial(ref UInt16 v)
		{
			if (IsReading)
			{
				v = BitConverter.ToUInt16(_Buffer, _Pos);
			}
			else
			{
				//BitConverter.GetBytes(v).Reverse().CopyTo(_Buffer, _Pos);

				_Buffer[_Pos + 0] = (Byte)(v);
				_Buffer[_Pos + 1] = (Byte)(v >> 8);
			}
			_Pos += 2;
		}

		public override void Serial(ref Int32 v)
		{
			if (IsReading)
			{
				v = BitConverter.ToInt32(_Buffer, _Pos);
			}
			else
			{
				//BitConverter.GetBytes(v).CopyTo(_Buffer, _Pos);

				_Buffer[_Pos + 0] = (Byte)(v);
				_Buffer[_Pos + 1] = (Byte)(v >> 8);
				_Buffer[_Pos + 2] = (Byte)(v >> 16);
				_Buffer[_Pos + 3] = (Byte)(v >> 24);
			}
			_Pos += 4;
		}

		public override void Serial(ref UInt32 v)
		{
			if (IsReading)
			{
				v = BitConverter.ToUInt32(_Buffer, _Pos);
			}
			else
			{
				//BitConverter.GetBytes(v).CopyTo(_Buffer, _Pos);

				_Buffer[_Pos + 0] = (Byte)(v);
				_Buffer[_Pos + 1] = (Byte)(v >> 8);
				_Buffer[_Pos + 2] = (Byte)(v >> 16);
				_Buffer[_Pos + 3] = (Byte)(v >> 24);
			}
			_Pos += 4;
		}


		//public override void Serial(ref Int64 v)
		//{
		//    if (IsReading())
		//    {
		//        v = BitConverter.ToInt64(_Buffer, _Pos);
		//    }
		//    else
		//    {
		//        BitConverter.GetBytes(v).CopyTo(_Buffer, _Pos);
		//    }
		//    _Pos += 8;
		//}

		public override void Serial(ref UInt64 v)
		{
			if (IsReading)
			{
				v = BitConverter.ToUInt64(_Buffer, _Pos);
			}
			else
			{
				BitConverter.GetBytes(v).CopyTo(_Buffer, _Pos);
			}
			_Pos += 8;
		}

		public override void Serial(ref String v)
		{
			Int32 strLen = 0;
			if (IsReading)
			{
				Serial(ref strLen);
				v = System.Text.Encoding.UTF8.GetString(_Buffer, _Pos, strLen);
				_Pos += strLen;
			}
			else
			{
				Byte[] by = System.Text.Encoding.UTF8.GetBytes(v);
				strLen = by.Length;
				Serial(ref strLen);
				by.CopyTo(_Buffer, _Pos);
				_Pos += strLen;
			}
		}

		public void SerialArray(ref Byte[] v)
		{
			Int32 strLen = 0;
			if (IsReading)
			{
				Serial(ref strLen);
				v = new Byte[strLen];
				System.Buffer.BlockCopy(_Buffer, _Pos, v, 0, strLen);
				_Pos += strLen;
			}
			else
			{
				strLen = v.Length;
				Serial(ref strLen);
				System.Buffer.BlockCopy(v, 0, _Buffer, _Pos, strLen);
				//v.CopyTo(_Buffer, _Pos);
				_Pos += strLen;
			}
		}

		public override void Serial(ref MemoryStream s)
		{
			Int32 length = 0;
			if (IsReading)
			{
				Serial(ref length);
				s.Write(_Buffer, _Pos, length);
				s.Seek(0, SeekOrigin.Begin);
			}
			else
			{
				length = (Int32)s.Length;
				Serial(ref length);
				s.Position = 0;
				s.Seek(0, SeekOrigin.Begin);
				s.Read(_Buffer, _Pos, length);
			}
			_Pos += length;
		}

		/// <summary>
		/// 对应C++的float序列化
		/// </summary>
		/// <param name="v"></param>
		public override void Serial(ref Single v)
		{
			if (IsReading)
			{
				v = BitConverter.ToSingle(_Buffer, _Pos);
			}
			else
			{
				BitConverter.GetBytes(v).CopyTo(_Buffer, _Pos);
			}
			_Pos += 4;
		}

		/// <summary>
		///  对应double
		/// </summary>
		/// <param name="v"></param>
		public override void Serial(ref Double d)
		{
			if (IsReading)
			{
				d = BitConverter.ToDouble(_Buffer, _Pos);
			}
			else
			{
				BitConverter.GetBytes(d).CopyTo(_Buffer, _Pos);
			}
			_Pos += 8;
		}

		/// <summary>
		/// TODO：RPC相关的东西暂时有用到吗？
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="pb"></param>
		//public void SerialPB<T>(ref T pb)
		//{
		//    var memStream = new MemoryStream();
		//    if (IsReading)
		//    {
		//        Serial(ref memStream);
		//        pb = Serializer.Deserialize<T>(memStream);
		//    }
		//    else
		//    {
		//        Serializer.Serialize(memStream, pb);
		//        Serial(ref memStream);
		//    }
		//}

		public override void SerialCont(ref Int32[] v)
		{
			Int32 arrayLen = 0;
			if (IsReading)
			{
				Serial(ref arrayLen);
			}
			else
			{
				arrayLen = v.Length;
				Serial(ref arrayLen);
			}
			//ClientCommon.Log("arraylen " + arrayLen);
			for (Int32 i = 0; i < arrayLen; i++)
			{
				//ClientCommon.Log("array " + i + ": " + v[i]);
				Serial(ref v[i]);
			}
		}

		public override void SerialCont(ref UInt32[] v)
		{
			Int32 arrayLen = 0;
			if (IsReading)
			{
				Serial(ref arrayLen);
			}
			else
			{
				arrayLen = v.Length;
				Serial(ref arrayLen);
			}
			//ClientCommon.Log("arraylen " + arrayLen);
			for (Int32 i = 0; i < arrayLen; i++)
			{
				//ClientCommon.Log("array " + i + ": " + v[i]);
				Serial(ref v[i]);
			}
		}

		public override void SerialCont(ref Boolean[] v)
		{
			Int32 arrayLen = 0;
			if (IsReading)
			{
				Serial(ref arrayLen);
			}
			else
			{
				arrayLen = v.Length;
				Serial(ref arrayLen);
			}
			for (Int32 i = 0; i < arrayLen; i++)
			{
				Serial(ref v[i]);
			}
		}

		/// <summary>
		/// SerialBuffer (inherited from IStream)
		/// </summary>
		/// <param name="buf"></param>
		/// <param name="start"></param>
		/// <param name="len"></param>
		public override void SerialBuffer(ref Byte[] buf, UInt32 start, UInt32 len)
		{
			// by zhongwei   only BitMemStream.SerialBuffer is used
			// incorrect code
/*			if (len == 0)
				return;

			if (IsReading)
			{
				// Check that we don't read more than there is to read
				//checkStreamSize(len);

				UInt32 pos = LengthS;
				UInt32 total = Length();
				if (pos + len > total) // calls virtual length (cf. sub messages)
				{
					//throw EStreamOverflow( "CMemStream serialBuffer overflow: Read past %u bytes", total );
				}

				// Serialize out
				MemoryStream ms = new MemoryStream(buf, (Int32)start, (Int32)len);
				ms.Read(_Buffer, _Pos, (Int32)len);
			}
			else
			{
				// Serialize in
				//increaseBufferIfNecessary (len);
				MemoryStream ms = new MemoryStream(buf, (Int32)start, (Int32)len);
				ms.Write(_Buffer, _Pos, (Int32)len);
			}
			_Pos += (Int32)len;*/
		}
		/// <summary>
		/// SerialBit (inherited from IStream)
		/// </summary>
		/// <param name="bit"></param>
		public override void SerialBit(ref bool bit)
		{
			Byte u = 1;
			if (IsReading)
			{
				Serial(ref u);
				bit = (u != 0);
			}
			else
			{
				u = bit ? (Byte)1 : (Byte)0;
				Serial(ref u);
			}
		}

		public void FixMsg()
		{
			Int32 size = (Int32)Length();
			_Buffer[3] = (Byte)(size);
			_Buffer[2] = (Byte)(size >> 8);
			_Buffer[1] = (Byte)(size >> 16);
			_Buffer[0] = (Byte)(size >> 24);
		}


// 		public void WriteTo(NetworkStream stream)
// 		{
// 			FixMsg();
// 
// 			stream.Write(_Buffer, 0, (Int32)Length() + 4);
// 		}
// 
// 		/// <summary>
// 		/// 只供测试用，接收由网络部分处理
// 		/// </summary>
// 		/// <param name="stream"></param>
// 		public void ReadFrom(NetworkStream stream)
// 		{
// 			//_InputStream = false;
// 			stream.Read(_Buffer, 0, 4);
// 			Int32 size = BitConverter.ToInt32(_Buffer, 0);
// 			size = IPAddress.NetworkToHostOrder(size);
// 
// 			stream.Read(_Buffer, 4, size);
// 
// 			_Pos = 4;
// 		}

		public virtual UInt32 Length()
		{
			if (IsReading)
			{
				return LengthR;
			}
			else
			{
				return LengthS;
			}
		}


		public UInt32 LengthR
		{
			get
			{
				Int32 size = _DataSize - 4;
				if (size < 0)
				{
					size = 0;
				}
				return (UInt32)size;
			}
		}

		public UInt32 LengthS
		{
			get
			{
				Int32 size = _Pos - 4;
				if (size < 0)
				{
					size = 0;
				}
				return (UInt32)size;
			}
		}
			
		public UInt32 BufferLengthR
		{
			get
			{
				Int32 size = _Buffer.Length - 4;
				if (size < 0)
				{
					size = 0;
				}
				return (UInt32)size;
			}
		}

		/// <summary>
		/// Increase the buffer size if 'len' can't enter, otherwise, do nothing
		/// </summary>
		/// <param name="len"></param>
		protected void IncreaseBufferIfNecessary(UInt32 len)
		{
			UInt32 oldBufferSize = (UInt32)_Buffer.Length;
			if (_Pos + len > oldBufferSize)
			{
				// need to increase the buffer size
				ResizeBuffer(oldBufferSize * 2 + len);
			}
		}

		protected void ResizeBuffer(UInt32 size)
		{
			MemoryStream mms = new MemoryStream();
			mms.Write(_Buffer, 0, _Pos);
			_Buffer = new Byte[size];
			mms.Read(_Buffer, 0, _Pos);
			mms.Close();
		}

		/// <summary>
		/// * Transforms the message from input to output or from output to input
		///
		/// * Precondition:
		/// * - If the stream is in input mode, it must not be empty (nothing filled), otherwise the position
		/// *   will be set to the end of the preallocated buffer (see DefaultCapacity).
		/// * Postcondition:
		/// * - Read->write, the position is set at the end of the stream, it is possible to add more data
		/// * - Write->Read, the position is set at the beginning of the stream
		/// </summary>
		public virtual void Invert()
		{
			if (IsReading)
			{
				// In->Out: We want to write (serialize out) what we have read (serialized in)
				UInt32 sizeOfReadStream = BufferLengthR;
				SetInOut(false);
				_Pos = (Int32)sizeOfReadStream;
			}
			else
			{
				// Out->In: We want to read (serialize in) what we have written (serialized out)
				SetInOut(true);
				// TODO : is it necessary ?
				//ResizeBuffer((UInt32)_Pos);
				_Pos = 4;
			}
		}

		public virtual void Clear()
		{
			_Pos = 4;
		}

		public virtual byte[] BufferToFill(UInt32 size)
		{
			_Pos = (Int32)size + 4;
			_DataSize = _Pos + 1;
			return _Buffer;
		}

		/// <summary>
		/// Serials the name.
		/// 服务器发来的是UNICODE 要专门写序列化接口
		/// 角色列表专用
		/// 
		/// </summary>
		/// <param name='stream'>
		/// Stream.
		/// </param>
		public override void SerialUnicodeString(ref String v)
		{
			UInt32 strLen = 0;
			if (IsReading)
			{
				v = "";
				Serial(ref strLen);

				if (strLen > (Length() - GetPos() + 4))
				{
					throw new Exception("unicode str outof stream, " + strLen + " > " + Length() + " - " + GetPos());
				}

				UInt16[] temp = new UInt16[strLen];
				for (int index = 0; index < strLen; index++)
				{
					Serial(ref temp[index]);
				}
                try
                {
                    for (int index = 0; index < strLen; index++)
                    {
                        v += ((char)Convert.ToInt32(temp[index])).ToString();
                    }
                }
                catch (System.Exception ex)
                {
                    UnityCustomUtil.CustomLogError(ex.Message.ToString());
                }

			}
			else
			{
				strLen = (UInt32)v.Length;
				Serial(ref strLen);
				UInt16 uchar = 0;
                try
                {
                    for (int index = 0; index < strLen; index++)
                    {
                        uchar = Convert.ToUInt16(v[index]);
                        Serial(ref uchar);
                    }
                }
                catch (System.Exception ex)
                {
                    UnityCustomUtil.CustomLogError(ex.Message.ToString());
                }

			}

		}
	}
}
