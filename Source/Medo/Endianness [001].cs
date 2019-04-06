/* Josip Medved <jmedved@jmedved.com> * www.medo64.com * MIT License */

//2008-01-22: Initial version.


using System;

namespace Medo {

	/// <summary>
	/// Handling of little-endian to big-endian conversions.
	/// </summary>
    public static class Endianness {

		/// <summary>
		/// Converts number from current platform order to it's big-endian form.
		/// If current platform is little-endian, bytes will be swapped; in case of big-endian, nothing will happen.
		/// </summary>
		/// <param name="value">Value to convert.</param>
		public static System.Int16 ToBigEndian(System.Int16 value) {
			if (System.BitConverter.IsLittleEndian) {
				byte[] valueBA = System.BitConverter.GetBytes(value);
				Array.Reverse(valueBA);
				return System.BitConverter.ToInt16(valueBA, 0);
			} else {
				return value;
			}
		}

		/// <summary>
		/// Converts number from current platform order to it's big-endian form.
		/// If current platform is little-endian, bytes will be swapped; in case of big-endian, nothing will happen.
		/// </summary>
		/// <param name="value">Value to convert.</param>
		public static System.Int32 ToBigEndian(System.Int32 value) {
			if (System.BitConverter.IsLittleEndian) {
				byte[] valueBA = System.BitConverter.GetBytes(value);
				Array.Reverse(valueBA);
				return System.BitConverter.ToInt32(valueBA, 0);
			} else {
				return value;
			}
		}

		/// <summary>
		/// Converts number from current platform order to it's big-endian form.
		/// If current platform is little-endian, bytes will be swapped; in case of big-endian, nothing will happen.
		/// </summary>
		/// <param name="value">Value to convert.</param>
		public static System.Int64 ToBigEndian(System.Int64 value) {
			if (System.BitConverter.IsLittleEndian) {
				byte[] valueBA = System.BitConverter.GetBytes(value);
				Array.Reverse(valueBA);
				return System.BitConverter.ToInt64(valueBA, 0);
			} else {
				return value;
			}
		}

		/// <summary>
		/// Converts number from current platform order to it's big-endian form.
		/// If current platform is little-endian, bytes will be swapped; in case of big-endian, nothing will happen.
		/// </summary>
		/// <param name="value">Value to convert.</param>
		public static System.Single ToBigEndian(System.Single value) {
			if (System.BitConverter.IsLittleEndian) {
				byte[] valueBA = System.BitConverter.GetBytes(value);
				Array.Reverse(valueBA);
				return System.BitConverter.ToSingle(valueBA, 0);
			} else {
				return value;
			}
		}

		/// <summary>
		/// Converts number from current platform order to it's big-endian form.
		/// If current platform is little-endian, bytes will be swapped; in case of big-endian, nothing will happen.
		/// </summary>
		/// <param name="value">Value to convert.</param>
		public static System.Double ToBigEndian(System.Double value) {
			if (System.BitConverter.IsLittleEndian) {
				byte[] valueBA = System.BitConverter.GetBytes(value);
				Array.Reverse(valueBA);
				return System.BitConverter.ToDouble(valueBA, 0);
			} else {
				return value;
			}
		}


		/// <summary>
		/// Converts number from it's big-endian form to current platform order.
		/// If current platform is little-endian, bytes will be swapped; in case of big-endian, nothing will happen.
		/// </summary>
		/// <param name="value">Value to convert.</param>
		public static System.Int16 FromBigEndian(System.Int16 value) {
			return ToBigEndian(value);
		}

		/// <summary>
		/// Converts number from it's big-endian form to current platform order.
		/// If current platform is little-endian, bytes will be swapped; in case of big-endian, nothing will happen.
		/// </summary>
		/// <param name="value">Value to convert.</param>
		public static System.Int32 FromBigEndian(System.Int32 value) {
			return ToBigEndian(value);
		}

		/// <summary>
		/// Converts number from it's big-endian form to current platform order.
		/// If current platform is little-endian, bytes will be swapped; in case of big-endian, nothing will happen.
		/// </summary>
		/// <param name="value">Value to convert.</param>
		public static System.Int64 FromBigEndian(System.Int64 value) {
			return ToBigEndian(value);
		}

		/// <summary>
		/// Converts number from it's big-endian form to current platform order.
		/// If current platform is little-endian, bytes will be swapped; in case of big-endian, nothing will happen.
		/// </summary>
		/// <param name="value">Value to convert.</param>
		public static System.Single FromBigEndian(System.Single value) {
			return ToBigEndian(value);
		}

		/// <summary>
		/// Converts number from it's big-endian form to current platform order.
		/// If current platform is little-endian, bytes will be swapped; in case of big-endian, nothing will happen.
		/// </summary>
		/// <param name="value">Value to convert.</param>
		public static System.Double FromBigEndian(System.Double value) {
			return ToBigEndian(value);
		}

	}

}
