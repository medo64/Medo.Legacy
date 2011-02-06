//Josip Medved <jmedved@jmedved.com> http://www.jmedved.com

//2008-01-22: New version.


namespace Medo {

	/// <summary>
	/// Provides functions for shifting and rotating bits. They are sign-bit insensitive.
	/// </summary>
	public static class Bitwise {

		private static uint[] _lookupOn = { 0x00000000, 0x00000001, 0x00000003, 0x00000007, 0x0000000F, 0x0000001F, 0x0000003F, 0x0000007F, 0x000000FF, 0x000001FF, 0x000003FF, 0x000007FF, 0x00000FFF, 0x00001FFF, 0x00003FFF, 0x00007FFF, 0x0000FFFF, 0x0001FFFF, 0x0003FFFF, 0x0007FFFF, 0x000FFFFF, 0x001FFFFF, 0x003FFFFF, 0x007FFFFF, 0x00FFFFFF, 0x01FFFFFF, 0x03FFFFFF, 0x07FFFFFF, 0x0FFFFFFF, 0x1FFFFFFF, 0x3FFFFFFF, 0xFFFFFFFF };
		private static uint[] _lookupPower = { 0x00000001, 0x00000002, 0x00000004, 0x00000008, 0x00000010, 0x00000020, 0x00000040, 0x00000080, 0x00000100, 0x00000200, 0x00000400, 0x00000800, 0x00001000, 0x00002000, 0x00004000, 0x00008000, 0x00010000, 0x00020000, 0x00040000, 0x00080000, 0x00100000, 0x00200000, 0x00400000, 0x00800000, 0x01000000, 0x02000000, 0x04000000, 0x08000000, 0x10000000, 0x20000000, 0x40000000, 0x80000000 };


		#region ShiftLeft

		/// <summary>
		/// Performs an unsigned arithmetic left shift on a bit pattern.
		/// </summary>
		/// <param name="value">The bit pattern to be shifted.</param>
		/// <param name="amount">The number of bits to shift the bit pattern.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "8-amount", Justification = "No overflow will happen since amount is limited.")]
		public static byte ShiftLeft(byte value, int amount) {
			amount = amount % 8;
			if (amount == 0) { return value; }
			return (byte)((value & _lookupOn[8 - amount]) * (_lookupPower[amount]));
		}

		/// <summary>
		/// Performs an unsigned arithmetic left shift on a bit pattern.
		/// </summary>
		/// <param name="value">The bit pattern to be shifted.</param>
		/// <param name="amount">The number of bits to shift the bit pattern.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "16-amount", Justification = "No overflow will happen since amount is limited.")]
		public static short ShiftLeft(short value, int amount) {
			amount = amount % 16;
			if (amount == 0) { return value; }
			return (short)(((ushort)value & _lookupOn[16 - amount]) * (_lookupPower[amount]));
		}

		/// <summary>
		/// Performs an unsigned arithmetic left shift on a bit pattern.
		/// </summary>
		/// <param name="value">The bit pattern to be shifted.</param>
		/// <param name="amount">The number of bits to shift the bit pattern.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "32-amount", Justification = "No overflow will happen since amount is limited.")]
		public static int ShiftLeft(int value, int amount) {
			amount = amount % 32;
			if (amount == 0) { return value; }
			return (int)(((uint)value & _lookupOn[32 - amount]) * (_lookupPower[amount]));
		}

		#endregion


		#region ShiftRight

		/// <summary>
		/// Performs an unsigned arithmetic right shift on a bit pattern.
		/// </summary>
		/// <param name="value">The bit pattern to be shifted.</param>
		/// <param name="amount">The number of bits to shift the bit pattern.</param>
		public static byte ShiftRight(byte value, int amount) {
			amount = amount % 8;
			if (amount == 0) { return value; }
			return (byte)(value / _lookupPower[amount]);
		}

		/// <summary>
		/// Performs an unsigned arithmetic right shift on a bit pattern.
		/// </summary>
		/// <param name="value">The bit pattern to be shifted.</param>
		/// <param name="amount">The number of bits to shift the bit pattern.</param>
		public static short ShiftRight(short value, int amount) {
			amount = amount % 16;
			if (amount == 0) { return value; }
			return (short)((ushort)value / _lookupPower[amount]);
		}

		/// <summary>
		/// Performs an unsigned arithmetic right shift on a bit pattern.
		/// </summary>
		/// <param name="value">The bit pattern to be shifted.</param>
		/// <param name="amount">The number of bits to shift the bit pattern.</param>
		public static int ShiftRight(int value, int amount) {
			amount = amount % 32;
			if (amount == 0) { return value; }
			return (int)((uint)value / _lookupPower[amount]);
		}

		#endregion


		#region RotateLeft

		/// <summary>
		/// Performs an unsigned arithmetic left rotation on a bit pattern.
		/// </summary>
		/// <param name="value">The bit pattern to be rotated.</param>
		/// <param name="amount">The number of bits to rotate the bit pattern.</param>
		public static byte RotateLeft(byte value, int amount) {
			return (byte)(ShiftLeft(value, amount) | ShiftRight(value, 8 - (amount % 8)));
		}

		/// <summary>
		/// Performs an unsigned arithmetic left rotation on a bit pattern.
		/// </summary>
		/// <param name="value">The bit pattern to be rotated.</param>
		/// <param name="amount">The number of bits to rotate the bit pattern.</param>
		public static short RotateLeft(short value, int amount) {
			return (short)(ShiftLeft(value, amount) | ShiftRight(value, 16 - (amount % 16)));
		}

		/// <summary>
		/// Performs an unsigned arithmetic left rotation on a bit pattern.
		/// </summary>
		/// <param name="value">The bit pattern to be rotated.</param>
		/// <param name="amount">The number of bits to rotate the bit pattern.</param>
		public static int RotateLeft(int value, int amount) {
			return (int)(ShiftLeft(value, amount) | ShiftRight(value, 32 - (amount % 32)));
		}

		#endregion


		#region RotateRight

		/// <summary>
		/// Performs an unsigned arithmetic right rotation on a bit pattern.
		/// </summary>
		/// <param name="value">The bit pattern to be rotated.</param>
		/// <param name="amount">The number of bits to rotate the bit pattern.</param>
		public static byte RotateRight(byte value, int amount) {
			return (byte)(ShiftRight(value, amount) | ShiftLeft(value, 8 - (amount % 8)));
		}

		/// <summary>
		/// Performs an unsigned arithmetic right rotation on a bit pattern.
		/// </summary>
		/// <param name="value">The bit pattern to be rotated.</param>
		/// <param name="amount">The number of bits to rotate the bit pattern.</param>
		public static short RotateRight(short value, int amount) {
			return (short)(ShiftRight(value, amount) | ShiftLeft(value, 16 - (amount % 16)));
		}

		/// <summary>
		/// Performs an unsigned arithmetic right rotation on a bit pattern.
		/// </summary>
		/// <param name="value">The bit pattern to be rotated.</param>
		/// <param name="amount">The number of bits to rotate the bit pattern.</param>
		public static int RotateRight(int value, int amount) {
			return (int)(ShiftRight(value, amount) | ShiftLeft(value, 32 - (amount % 32)));
		}

		#endregion


		#region Reverse

		/// <summary>
		/// Performs an unsigned arithmetic reversion on a bit pattern.
		/// </summary>
		/// <param name="value">The bit pattern to be reversed.</param>
		public static byte Reverse(byte value) {
			byte result = 0;
			for (int i = 0; i < 4; i++) {
				if ((value & _lookupPower[i]) == _lookupPower[i]) { result = (byte)(result | _lookupPower[7 - i]); }
				if ((value & _lookupPower[7 - i]) == _lookupPower[7 - i]) { result = (byte)(result | _lookupPower[i]); }
			}
			return result;
		}

		/// <summary>
		/// Performs an unsigned arithmetic reversion on a bit pattern.
		/// </summary>
		/// <param name="value">The bit pattern to be reversed.</param>
		public static short Reverse(short value) {
			ushort result = 0;
			ushort valueU = (ushort)value;
			for (int i = 0; i < 8; i++) {
				if ((valueU & _lookupPower[i]) == _lookupPower[i]) { result = (ushort)(result | _lookupPower[15 - i]); }
				if ((valueU & _lookupPower[15 - i]) == _lookupPower[15 - i]) { result = (ushort)(result | _lookupPower[i]); }
			}
			return (short)result;
		}

		/// <summary>
		/// Performs an unsigned arithmetic reversion on a bit pattern.
		/// </summary>
		/// <param name="value">The bit pattern to be reversed.</param>
		public static int Reverse(int value) {
			uint result = 0;
			uint valueU = (uint)value;
			for (int i = 0; i < 16; i++) {
				if ((valueU & _lookupPower[i]) == _lookupPower[i]) { result = (uint)(result | _lookupPower[31 - i]); }
				if ((valueU & _lookupPower[31 - i]) == _lookupPower[31 - i]) { result = (uint)(result | _lookupPower[i]); }
			}
			return (int)result;
		}

		#endregion

	}

}
