using System;
using System.Collections.Generic;

using System.Text;
using System.Net;
using System.IO;
using CustomUtil;

namespace CustomNetwork
{
	public class BitMemStream : MemStream
	{
		static MemoryStream DataBox = new MemoryStream();
		/// <summary>
		/// 当前BYTE还剩多少BIT
		/// </summary>
		protected UInt32 _FreeBits;

		/// <summary>
		///
		/// </summary>
		/// <param name="inputStream">true 往外读数据的输入流，false 往里写数据的输出流</param>
		public BitMemStream(bool inputStream)
			: base(inputStream)
		{
			_FreeBits = 8; /// MemStream的Pos是从4开始的。。。
		}

		public BitMemStream(Int32 len, bool inputStream)
			: base(len, inputStream)
		{
			_FreeBits = 8; /// MemStream的Pos是从4开始的。。。
		}

		/// <summary>
		/// 创建一个输人流，用于往外读数据
		/// </summary>
		/// <param name="buf"></param>
		/// <param name="len"></param>
		public BitMemStream(Byte[] buf, Int32 len)
			: base(true)
		{
			_Pos = 4;
			_FreeBits = 8;
			_Buffer = new Byte[len + 4];
			for (int i = 0; i < len; i++)
			{
				_Buffer[i + 4] = buf[i];
			}
			_DataSize = _Buffer.Length;
		}

		/// <summary>
		/// TODO：如用法不当，可能造成读取过量数据
		/// </summary>
		/// <param name="buf"></param>
		/// <param name="len"></param>
		public void LoadBytes(Byte[] buf, Int32 start, Int32 len)
		{
			if (len > (_Buffer.Length - 4))
			{
				_Buffer = new byte[len + 4];
			}

			Array.ConstrainedCopy(buf, start, _Buffer, 4, len);
			_DataSize = (Int32)len + 4 + 1;
		}

		/// <summary>
		/// 创建一个输人流，用于往外读数据，只有发消息才可以使用
		/// </summary>
		/// <param name="bitMem"></param>
		public BitMemStream(BitMemStream bitMem)
			: base(bitMem._Pos + 1, bitMem.IsReading)
		{
			_Pos = bitMem._Pos;
			_FreeBits = bitMem._FreeBits;
			Array.Copy(bitMem._Buffer, _Buffer, bitMem._Pos + 1);
		}

		/// <summary>
		/// 创建一个输人流，用于往外读数据，暂时用于字符串相关的，其他地方慎用
		/// </summary>
		/// <param name="bitMem"></param>
		public BitMemStream(BitMemStream bitMem, bool isString)
			: base(bitMem._DataSize, bitMem.IsReading)
		{
			if (isString == false)
			{
				return;
			}
			_Pos = bitMem._Pos;
			_FreeBits = bitMem._FreeBits;
			
			Array.Copy(bitMem._Buffer, _Buffer, bitMem._DataSize);
		}

		public void Clone(BitMemStream bitMem)
		{
			_Buffer = new Byte[bitMem._DataSize];
			SetInOut(bitMem.IsReading);
			_Pos = bitMem._Pos;
			_FreeBits = bitMem._FreeBits;
			Array.Copy(bitMem._Buffer, _Buffer, bitMem._DataSize);

		}

		public UInt32 GetPosInBit()
		{
		    // 实际含义：return (UInt32)((_Pos + 1 - 4) * 8 - _FreeBits);
		    return (UInt32)((_Pos - 3) * 8 - _FreeBits);
		}

		public void PrepareNextByte()
		{
			_FreeBits = 8;
			++_Pos;
			_Buffer[_Pos] = 0;
			/// 这里还应该处理内存不够等问题;
		}


		/// <summary>
		/// Serialize only the nbits lower bits of value (nbits range: [1..32])
		/// When using this method, always leave resetvalue to true.
		/// </summary>
		/// <param name="?"></param>
		public void Serial(ref UInt32 value, UInt32 nbits, bool resetvalue)
		{
			InternalSerial(ref value, nbits, resetvalue);
		}


		public override void Serial(ref UInt64 value)
		{
//			InternalSerial(ref value, 64);
			InternalSerial(ref value, 32);
			value >>= 32;
			InternalSerial(ref value, 32);
		}
		/// <summary>
		/// 临时的，只负责32位内往外序列化，其他人不要用，否则后果自负哟~~；
		/// </summary>
		/// <param name="value"></param>
		public void Serial(ref Int64 value)
		{
			UInt64 ub = 0;
			InternalSerial(ref ub, 32);
			value = (Int64)ub;
			InternalSerial(ref ub, 32);
			ub >>= 32;
			value |= (Int64)ub;

			//UInt64 ub = 0;
			//if (IsReading)
			//{
			//    Serial(ref ub);
			//    value = (Int64)ub;
			//}
			//else
			//{
			//    ub = (UInt64)value;
			//    Serial(ref ub);
			//}
		}


		public void SerialUInt64(ref UInt64 value)
		{
			UInt32 low = (UInt32)value;
			Serial(ref low);
			UInt32 high = (UInt32)(value >> 32);
			Serial(ref high);
			value = high;
			value <<= 32;
			value |= low;//太tm搓了吧
		}
		/// <summary>
		/// Serial the specified v and isBuffer.
		/// 此接口对应服务器的 serialcount
		/// </summary>
		/// <param name='v'>
		/// V.
		/// </param>
		/// <param name='isBuffer'>
		/// Is buffer.
		/// </param>
		public void Serial(ref UInt64 v, bool isCount)
		{
			UInt32 lsd = 0;
			InternalSerial(ref lsd, 32, true);

			// Reset and read LSD
			UInt32 msd = 0;
			InternalSerial(ref msd, 32, true);

			v = ((UInt64)msd << 32) | ((UInt64)lsd);
		}

		/// <summary>
		/// Serialize only the nbits lower bits of 64-bit value (nbits range: [1..64])
		/// </summary>
		/// <param name="value"></param>
		/// <param name="nbits"></param>
		public void Serial(ref UInt64 value, UInt32 nbits)
		{
			InternalSerial(ref value, nbits);
		}

		/// <summary>
		/// Helper for serial(uint32,UInt32)
		/// </summary>
		/// <param name="value"></param>
		/// <param name="nbits"></param>
		/// <param name="resetvalue"></param>
		public void InternalSerial(ref UInt32 value, UInt32 nbits, bool resetvalue)
		{
			if (nbits == 0 || nbits > 32)
				return;

			if (IsReading)
			{
				UInt32 pib = GetPosInBit();
				UInt32 len = Length();
				if (pib + nbits > len * 8)
				{
					return;
				}

				if (resetvalue)
				{
					value = 0;
				}

				if (_Pos >= _DataSize)
				{
					return;
				}

				Byte v = (Byte)(_Buffer[_Pos] & ((1 << (int)_FreeBits) - 1));

				if (nbits > _FreeBits)
				{
					value |= (UInt32)(v << (int)(nbits - _FreeBits));
					_Pos++;
					UInt32 readbits = _FreeBits;
					_FreeBits = 8;
					InternalSerial(ref value, nbits - readbits, false); // read without resetting value
				}
				else
				{
					value |= (UInt32)(v >> (int)(_FreeBits - nbits));
					_FreeBits -= nbits;
					if (_FreeBits == 0)
					{
						_FreeBits = 8;
						_Pos++;
					}
				}
			}
			else
			{
				UInt32 v;
				if (nbits != 32)
				{
					UInt32 mask = ((UInt32)1 << (int)nbits) - 1;
					v = value & mask;
				}
				else
				{
					v = value;
				}

				if (nbits > _FreeBits)
				{
					Byte bitValue = (Byte)(v >> (int)(nbits - _FreeBits));
					_Buffer[_Pos] |= bitValue;
					UInt32 filledbits = _FreeBits;
					PrepareNextByte();
					InternalSerial(ref v, nbits - filledbits, true);
				}
				else
				{
					Byte bitValue = (Byte)(v << (int)(_FreeBits - nbits));
					_Buffer[_Pos] |= bitValue;

					_FreeBits -= nbits;
					if (_FreeBits == 0)
					{
						PrepareNextByte();
					}
				}
			}
		}

		public override void Serial(ref Int32 b)
		{
			UInt32 ub = 0;
			if (IsReading)
			{
				InternalSerial(ref ub, sizeof(Int32) * 8, true);
				b = (Int32)ub;
			}
			else
			{
				ub = (UInt32)b;
				InternalSerial(ref ub, sizeof(Int32) * 8, true);
			}

		}

		public override void Serial(ref UInt32 b)
		{
			if (IsReading)
			{
				InternalSerial(ref b, sizeof(UInt32) * 8, true);
			}
			else
			{
				InternalSerial(ref b, sizeof(Int32) * 8, true);
			}
		}


		public void SerialAndLog(ref UInt32 value, UInt32 nbits)
		{
			//Int32 bitpos = (Int32)GetPosInBit();
			InternalSerial(ref value, nbits, true);
		}

		public override UInt32 Length()
		{
			if (IsReading)
			{
				return LengthR;
			}
			else
			{
				if (_FreeBits == 8)
					return LengthS;
				else
					return LengthS + 1;
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
			UInt32 i;
			UInt32 v = 0;
			UInt32 end = start + len;
			if (IsReading)
			{
				for (i = start; i != end; ++i)
				{
					InternalSerial(ref v, 8, true);
					buf[i] = (Byte)v;
				}
			}
			else
			{
				for (i = start; i != end; ++i)
				{
					v = (UInt32)buf[i];
					InternalSerial(ref v, 8, true);
				}
			}
		}

		public override void Serial(ref bool v)
		{
			SerialBit(ref v);
		}



		/// <summary>
		/// 序列化字符串
		/// </summary>
		/// <param name="v"></param>
		public override void Serial(ref String v)
		{
			// TODO: 考虑限制字符串的长度
			UInt32 strLen = 0;
			if (IsReading)
			{
				Serial(ref strLen);

				if (strLen > ( Length() - (uint)GetPos() + 4 ) )
				{
					UnityEngine.Debug.LogError( "Serial String: strLen " + strLen + " totalLen " + Length() + " pos " + GetPos());
					throw new Exception("str out of strm");
				}
				Byte[] temp = new Byte[strLen];
				SerialBuffer(ref temp, 0, strLen);
				v = System.Text.Encoding.UTF8.GetString(temp, 0, (Int32)strLen);
			}
			else
			{
				Byte[] by = System.Text.Encoding.UTF8.GetBytes(v);
				strLen = (UInt32)by.Length;

				Serial(ref strLen);
				SerialBuffer(ref by, 0, strLen);
			}
		}

		/// <summary>
		/// SerialBit (inherited from IStream)
		/// </summary>
		/// <param name="bit"></param>
		public override void SerialBit(ref bool bit)
		{
			UInt32 ubit = 0;
			if (IsReading)
			{
				InternalSerial(ref ubit, 1, true);
				bit = (ubit != 0);
			}
			else
			{
				ubit = (UInt32)((bit == true) ? 1 : 0);
				InternalSerial(ref ubit, 1, true);
			}
		}

		public override void Serial(ref Byte b)
		{
			UInt32 ub = 0;
			if (IsReading)
			{
				InternalSerial(ref ub, sizeof(Byte) * 8, true);
				b = (Byte)ub;
			}
			else
			{
				ub = (UInt32)b;
				InternalSerial(ref ub, sizeof(Byte) * 8, true);
			}
		}

		public void ReadBits(ref BitSet bitfield)
		{
			UInt32 len = bitfield.Size();
			if (len != 0)
			{
				UInt32 i = 0;
				UInt32 v = 0;
				while (len > 32)
				{
					InternalSerial(ref v, 32, true);
					bitfield.SetUint(v, (int)i, 32);
					len -= 32;
					i = i + 32;
				}
				InternalSerial(ref v, len, true);
				bitfield.SetUint(v, (int)i, (int)len);
			}

		}

		public override void Serial(ref float b)
		{
			UInt32 uf = 0;
			if (IsReading)
			{
				InternalSerial(ref uf, sizeof(float) * 8, true);
				Byte[] bytes = BitConverter.GetBytes(uf);
				b = BitConverter.ToSingle(bytes, 0);
			}
			else
			{
				Byte[] bytes = BitConverter.GetBytes(b);
				uf = BitConverter.ToUInt32(bytes, 0);
				InternalSerial(ref uf, sizeof(float) * 8, true);
			}
		}

		public override void Serial(ref Int16 b)
		{
			UInt32 ub = 0;
			if (IsReading)
			{
				InternalSerial(ref ub, sizeof(Int16) * 8, true);
				b = (Int16)ub;
			}
			else
			{
				ub = (UInt32)b;
				InternalSerial(ref ub, sizeof(Int16) * 8, true);
			}
		}

		public override void Serial(ref UInt16 b)
		{
			UInt32 ub = 0;
			if (IsReading)
			{
				InternalSerial(ref ub, sizeof(UInt16) * 8, true);
				b = (UInt16)ub;
			}
			else
			{
				ub = (UInt32)b;
				InternalSerial(ref ub, sizeof(UInt16) * 8, true);
			}
		}

		public override void Serial(ref MemoryStream ms)
		{
			UInt32 len = 0;
			bool mini = false;
			if (IsReading)
			{
				Serial(ref mini);
				len = (UInt32)ms.Length;
				if (mini)
				{
					Serial(ref len, 7, true);
				}
				else
				{
					Serial(ref len, 23, true);
				}

				ms.SetLength(len);

				Byte[] buf = ms.GetBuffer();

				SerialBuffer(ref buf, 0, len);
				ms.Seek(0, SeekOrigin.Begin);
			}
			else
			{
				len = (UInt32)ms.Length;
				if (len < 0x7F)
				{
					mini = true;
					Serial(ref mini);
					Serial(ref len, 7, true);
				}
				else
				{
					mini = false;
					Serial(ref mini);
					Serial(ref len, 23, true);
				}
				Byte[] buf = ms.GetBuffer();
				SerialBuffer(ref buf, 0, len);
			}
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
				for (int index = 0; index < strLen; index++)
				{
					uchar = Convert.ToUInt16(v[index]);
					Serial(ref uchar);
				}
			}

		}

		public void InternalSerial(ref UInt64 value, UInt32 nbits)
		{
			if (nbits > 32)
			{
				if (IsReading)
				{
					// Reset and read MSD
					UInt32 msd = 0;
					InternalSerial(ref msd, nbits - 32, true);

					// Reset and read LSD
					UInt32 lsd = 0;
					InternalSerial(ref lsd, 32, true);

					value = ((UInt64)msd << 32) | ((UInt64)lsd);
				}
				else
				{
					// Write MSD
					UInt32 msd = (UInt32)(value >> 32);
					InternalSerial(ref msd, nbits - 32, true);
					// Write LSD
					UInt32 lsd = (UInt32)value;
					InternalSerial(ref lsd, 32, true);
				}
			}
			else
			{
				if (IsReading)
				{
					// Reset MSB (=0 is faster than value&=0xFFFFFFFF)
					value = 0;
				}
				// Read or write LSB
				UInt32 lsd = (UInt32)value;
				InternalSerial(ref lsd, nbits, true);
				value = (UInt64)lsd;
			}
		}

		/// <summary>
		/// Transforms the message from input to output or from output to input
		/// </summary>
		public override void Invert()
		{
			if (!IsReading)
			{
				++_Pos; // write->read: extend to keep the last byte inside the payload
				_FreeBits = 8;
			}
			base.Invert();

			if (!IsReading)
			{
				--_Pos; // read->write: set the position on the last byte, not at the end as in CMemStream::invert()
			}
			// Keep the same _FreeBits
		}

		/// <summary>
		/// Clears the message
		/// </summary>
		public override void Clear()
		{
			_Pos = 4;
			_FreeBits = 8;
		}

		public void Reset()
		{
			_Pos = 4;
			_FreeBits = 8;
			Array.Clear(_Buffer, 0, _Buffer.Length);
		}

		public override byte[] BufferToFill(UInt32 size)
		{
			_FreeBits = 8;
			return base.BufferToFill(size);
		}

		/// <summary>
		/// dump the contenet of memstream by hexadecimal 
		/// </summary>
		/// <returns></returns>
		public string ToDebugString()
		{
			// max 4096 length for dump
			StringBuilder s = new StringBuilder(4096);

			for (int i = 0; i < _Pos; i++)
			{
				if (i % 4 == 0)
				{
					s.Append("\n");
				}

				s.AppendFormat("0x{0:x2} ", _Buffer[i]);
			}

			return s.ToString();
		}

		public T ReadPB<T>() where T : class
		{
			if (!IsReading)
			{
				return null;
			}

			BitMemStream.DataBox.SetLength(0);
			T retPb = null;
			
			try
			{
				Serial(ref BitMemStream.DataBox);
				retPb = ProtoBuf.Serializer.Deserialize<T>(BitMemStream.DataBox);
			}
			catch
			{
				return null;
			}

			return retPb;
		}

		public bool WirtePB<T>(T pb) where T : class
		{
			if (IsReading)
			{
				return false;
			}
			BitMemStream.DataBox.SetLength(0);
			try
			{
				ProtoBuf.Serializer.Serialize<T>(BitMemStream.DataBox, pb);
				Serial(ref BitMemStream.DataBox);
			}
            catch
            {
				return false;
			}

			return true;
		}

		public String WirteToString()
		{
			int dataLen = (int)Length();
			return System.Text.Encoding.ASCII.GetString(_Buffer, 4, dataLen);
		}

		public void ReadFromString(string s)
		{
			Byte[] by = System.Text.Encoding.ASCII.GetBytes(s);
			//int strLen = by.Length;
			by.CopyTo(_Buffer, 4);
			_Pos = by.Length;
		}

		public Byte[] WirteToByteArray()
		{
			int dataLen = (int)Length();
			Byte[] des = new Byte[dataLen];
			Array.Copy(_Buffer, 4, des, 0, dataLen);
			return des;
		}

		public void ReadFromByteArray(Byte[] by)
		{
			by.CopyTo(_Buffer, 4);
			_Pos = by.Length + 4;
		}
	}
}
