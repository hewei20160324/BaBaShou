

namespace CustomNetwork
{
	/// <summary>
	/// TCP 连接状态，详细的状态可以使用TCP协议状态
	/// </summary>
	public enum NetState
	{
		NS_Null					= 0,
		State_Initialized		= 1,		// 初始状态（未连接）
		State_Connected			= 2,		// 正常连接
		State_ConnectFailed		= 4,	// 连接失败
		State_DisconTimeout		= 8,	// 被动断连，超时
		State_DisconRecvErr1	= 16,	// 被动断连，接收1(收到数据为0);
		State_DisconRecvErr2	= 32,	// 被动断连，接收2(接受数据异常);
		State_DisconSendErr1	= 64,	// 被动断连，发送1
		State_DisconSendErr2	= 128,	// 被动断连，发送2
		State_ClientClose		= 256,	// 主动断开（客户端发起）
		State_Disonnected		= 1024
	}
    
    /// <summary>
    /// 安全队列，在不同线程Push、Pop时，只要size足够，都可以正确读取内容，很好的设计 by hewei 2016/11/02;
    /// </summary>
	public class SafeQueue
	{
		public SafeQueue(int size)
		{
			_Size = size;
			_ObjectArray = new BitMemStream[size];
		}

		public bool Push(BitMemStream obj)
		{
			if (_Head - _Tail == 1 || _Tail - _Head >= _Size - 1)
			{
				return false;
			}
			_ObjectArray[_Tail] = obj;

			if (_Tail + 1 == _Size)
			{
				_Tail = 0;
			}
			else
			{
				_Tail++;
			}
			return true;
		}

		public bool Pop(ref BitMemStream obj)
		{
			if (_Head == _Tail)
			{
				return false;
			}
			obj = _ObjectArray[_Head];
			if (_Head + 1 == _Size)
			{
				_Head = 0;
			}
			else
			{
				_Head++;
			}
			return true;
		}

		public int Count()
		{
			int count = _Tail - _Head;
			return count < 0 ? count + _Size : count;
		}

		private int _Size = 0;
		private int _Head = 0;
		private int _Tail = 0;
		private BitMemStream[] _ObjectArray = null;

	}
}