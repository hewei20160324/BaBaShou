using System;
using System.Collections.Generic;

using System.Text;
using System.Collections;

namespace CustomNetwork
{
	public class BitSet
	{
		public const int BitLen = 32;
		public const int BitLenShift = 5;
		public BitArray Bits = null;
		//private Int32	NumBits;
		//private uint	MaskLast;	// Mask for the last uint32.

		public BitSet() { }
		public BitSet(uint numBits)
		{
			Resize(numBits);
		}
		public BitSet(BitSet bs)
		{
			Bits = bs.Bits;
		}

		public void Resize(uint numBits)
		{
			//if (numBits == 0)
			//    clear();

			Bits = new BitArray((int)numBits);
			// reset to 0.
			ClearAll();
		}

		// bool value=false
		public void ResizeNoReset(uint numBits, bool value)
		{
			if (numBits == 0)
				Clear();

			int oldNum = Bits.Length;

			BitArray newBit = new BitArray((int)numBits, value);
			for (int i = oldNum; i < (int)numBits; i++)
			{
				newBit[i] = Bits[i];
			}
			Bits = newBit;
		}

		public void Clear()
		{
			Bits.SetAll(false);
		}

		public uint Size()
		{
			return (uint)Bits.Length;
		}

		private bool GetFromUint(UInt32 uintNumber, int biteIndex)
		{
			// Assert
			if (biteIndex > 31 || biteIndex < 0)
			{
				return false;
			}


			UInt32 mask = (UInt32)biteIndex & (UInt32)(32 - 1);
			mask = (UInt32)(1 << (int)mask);
			return (uintNumber & mask) != 0;
		}
		/*
		 * Serial Uint32
		 * startIndex: Bits[] Index
		 * BitCount: Serial Bit Count
		 */
		public void SetUint(UInt32 uintNumber, int startIndex, int BitCount)
		{
			if ((startIndex + BitCount) > Bits.Length || (startIndex + BitCount) < 0)
			{
				return;
			}
			if (startIndex < 0 || BitCount < 1)
			{
				return;
			}
			for (int index = 0; index < BitCount; index++)
			{
				bool value = GetFromUint(uintNumber, index);
				Bits[startIndex + index] = value;
			}
		}
		
		public void set(int biteIndex, bool value)
		{
			if (!(biteIndex >= 0 && biteIndex < Bits.Length))
			{
				return;
			}

			Bits[biteIndex] = value;
		}
		/// Get the value of a bit.
		public bool Get(int biteIndex)
		{
			//nlassert(bitNumber>=0 && bitNumber<NumBits);
			return Bits[biteIndex];
		}
		///// Get the value of a bit.
		//public bool this[int bitNumber]
		//{
		//    get;
		//}
		/// Set a bit to 1.
		public void Set(int biteIndex)
		{
			set(biteIndex, true);
		}
		/// Set a bit to 0.
		public void Clear(int biteIndex)
		{
			set(biteIndex, false);
		}
		/// Set all bits to 1.
		public void SetAll()
		{
			Bits.SetAll(true);
		}
		/// Set all bits to 0.
		public void ClearAll()
		{
			Bits.SetAll(false);
		}
		//@}


		/**
		* Return this ANDed with bs.
		* The result BitSet is of size of \c *this. Any missing bits into bs will be considered as 0.
		*/
		public static BitSet operator &(BitSet bs1, BitSet bs2)
		{
			bs1.Bits.And(bs2.Bits);
			return bs1;
		}
		/**
		 * Return this ORed with bs.
		 * The result BitSet is of size of \c *this. Any missing bits into bs will be considered as 0.
		 */
		public static BitSet operator |(BitSet bs1, BitSet bs2)
		{
			bs1.Bits.Or(bs2.Bits);
			return bs1;
		}
		/**
		 * Return this XORed with bs.
		 * The result BitSet is of size of \c *this. Any missing bits into bs will be considered as 0.
		 */
		public static BitSet operator ^(BitSet bs1, BitSet bs2)
		{
			bs1.Bits.Xor(bs2.Bits);
			return bs1;
		}

		/// NOT the BitArray.
		public void Flip()
		{
			this.Bits.Not();
		}

		/// \name Bit comparisons.
		//@{
		/**
		 * Compare two BitSet not necessarely of same size. The comparison is done on N bits, where N=min(this->size(), bs.size())
		 * \return true if the N common bits of this and bs are the same. false otherwise.
		 */
		public bool CompareRestrict(BitSet bs)
		{
			int n = Bits.Length - bs.Bits.Length;

			int minCount = 0;
			if (n < 0)
			{
				minCount = Bits.Length;
			}
			else
			{
				minCount = bs.Bits.Length;
			}

			for (int i = 0; i < minCount; i++)
			{
				if (Bits[i] != bs.Bits[i])
				{
					return false;
				}
			}
			return true;
		}
		/// Compare two BitSet. If not of same size, return false.
		public static bool operator ==(BitSet bs1, BitSet bs2)
		{
			int n = bs1.Bits.Length - bs2.Bits.Length;

			int minCount = 0;
			if (n < 0)
			{
				minCount = bs1.Bits.Length;
			}
			else
			{
				minCount = bs2.Bits.Length;
			}

			for (int i = 0; i < minCount; i++)
			{
				if (bs1.Bits[i] != bs2.Bits[i])
				{
					return false;
				}
			}
			return true;
		}
		/// operator!=.
		public static bool operator !=(BitSet bs1, BitSet bs2)
		{
			int n = bs1.Bits.Length - bs2.Bits.Length;

			int minCount = 0;
			if (n < 0)
			{
				minCount = bs1.Bits.Length;
			}
			else
			{
				minCount = bs2.Bits.Length;
			}

			for (int i = 0; i < minCount; i++)
			{
				if (bs1.Bits[i] != bs2.Bits[i])
				{
					return true;
				}
			}
			return false;
		}
		/// Return true if all bits are set. false if size()==0.
		public bool AllSet()
		{
			if (Bits.Length == 0)
			{
				return false;
			}
			for (int index = 0; index < Bits.Length; index++)
			{
				if (Bits[index] != true)
				{
					return false;
				}
			}
			return true;
		}
		/// Return true if all bits are cleared. false if size()==0.
		public bool AllCleared()
		{
			if (Bits.Length == 0)
			{
				return false;
			}
			for (int index = 0; index < Bits.Length; index++)
			{
				if (Bits[index] != false)
				{
					return false;
				}
			}
			return true;
		}
		//@}


		///// Serialize
		//public void	serial(IStream f);

		/// Return the raw vector
		public BitArray GetVector()
		{
			return Bits;
		}

		/// Write an uint32 into the bit set (use with caution, no check)
		//public void setUint(UInt32 srcValue, uint i) { Bits[(int)i] = srcValue; }

		/// Return a string representing the bitfield with 1 and 0 (from left to right)
		public string BitsToString()
		{
			return Bits.ToString();
		}

		public override bool Equals(object obj)
		{
			return base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

	}
}
