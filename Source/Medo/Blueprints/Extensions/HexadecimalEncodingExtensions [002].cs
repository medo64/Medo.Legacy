//Josip Medved <jmedved@jmedved.com> http://www.jmedved.com

//2008-03-30: Initial version.
//2010-05-14: Changed namespace.


using System;

namespace Medo.Extensions.HexadecimalEncoding {

	/// <summary>
	/// Conversions for hexadecimal byte arrays.
	/// </summary>
	public static class HexadecimalEncodingExtensions {

		/// <summary>
		/// Converts byte array to hexadecimal string.
		/// </summary>
		/// <param name="inArray">Array to convert.</param>
		public static string ToHexString(this byte[] inArray) {
			if (inArray == null) { return null; }
			return ToHexString(inArray, 0, inArray.Length);
		}

		/// <summary>
		/// Converts byte array to hexadecimal string.
		/// </summary>
		/// <param name="inArray">Array to convert.</param>
		/// <param name="offset">An offset in inArray.</param>
		/// <param name="length">The number of elements of inArray to convert.</param>
		public static string ToHexString(this byte[] inArray, int offset, int length) {
			if (inArray == null) { return null; }
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			for (int i = offset; i < offset + length; i++) {
				sb.Append(inArray[i].ToString("x2", System.Globalization.CultureInfo.InvariantCulture));
			}
			return sb.ToString();
		}

		/// <summary>
		/// Converts byte array to hexadecimal string.
		/// </summary>
		/// <param name="value">Data to convert.</param>
		public static byte[] FromHexString(this string value) {
			if (value == null) { return null; }
			if (value.Length == 0) { return new byte[] { }; }
			System.Collections.Generic.List<byte> result = new System.Collections.Generic.List<byte>();
			int start = 0;
			if (value.Length % 2 == 1) {
				result.Add(byte.Parse(value.Substring(0, 1), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture));
				start = 1;
			}
			for (int i = start; i < value.Length; i += 2) {
				result.Add(byte.Parse(value.Substring(i, 2), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture));
			}
			return result.ToArray();
		}

	}

}
