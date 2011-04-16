//Copyright (c) 2008 Josip Medved <jmedved@jmedved.com>

//2008-03-29: Initial version.
//2008-11-15: Added method overloads with Int64 data type.
//2010-05-14: Changed namespace.


using System;
using System.Globalization;

namespace Medo.Extensions.SIPrefix {

	/// <summary>
	/// Conversions to closest SI prefix.
	/// This extension methods are intended for double.
	/// </summary>
	public static class SIPrefixExtensions {

        private static readonly double[] prefixBigValues = new double[] { System.Math.Pow(10, 24), System.Math.Pow(10, 21), System.Math.Pow(10, 18), System.Math.Pow(10, 15), System.Math.Pow(10, 12), System.Math.Pow(10, 9), System.Math.Pow(10, 6), System.Math.Pow(10, 3) };
		private static readonly string[] prefixBigTexts = new string[] { "yotta-", "zetta-", "exa-", "peta-", "tera-", "giga-", "mega-", "kilo-" };
		private static readonly string[] prefixBigSymbols = new string[] { "Y", "Z", "E", "P", "T", "G", "M", "k" };

        private static readonly double[] prefixSmallValues = new double[] { System.Math.Pow(10, -24), System.Math.Pow(10, -21), System.Math.Pow(10, -18), System.Math.Pow(10, -15), System.Math.Pow(10, -12), System.Math.Pow(10, -9), System.Math.Pow(10, -6), System.Math.Pow(10, -3) };
		private static readonly string[] prefixSmallTexts = new string[] { "yocto-", "zepto-", "atto-", "femto-", "pico-", "nano-", "micro-", "milli-" };
		private static readonly string[] prefixSmallSymbols = new string[] { "y", "z", "a", "f", "p", "n", "µ", "m" };


		/// <summary>
		/// Converts the value of this instance to its equivalent string representation with measurement unit prefixed with SI prefix symbol.
		/// </summary>
		/// <param name="value">Value to convert.</param>
		/// <param name="measurementUnit">Measurement unit to which prefix will be attached.</param>
		public static string ToSIPrefixString(this double value, string measurementUnit) {
			return ConvertToString(value, measurementUnit, null, CultureInfo.CurrentCulture, prefixBigValues, prefixBigSymbols, prefixSmallValues, prefixSmallSymbols);
		}

        /// <summary>
        /// Converts the value of this instance to its equivalent string representation with measurement unit prefixed with SI prefix symbol.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <param name="measurementUnit">Measurement unit to which prefix will be attached.</param>
        public static string ToSIPrefixString(this long value, string measurementUnit)
        {
            return ConvertToString((double)value, measurementUnit, null, CultureInfo.CurrentCulture, prefixBigValues, prefixBigSymbols, prefixSmallValues, prefixSmallSymbols);
        }

		/// <summary>
		/// Converts the value of this instance to its equivalent string representation with measurement unit prefixed with SI prefix symbol.
		/// </summary>
		/// <param name="value">Value to convert.</param>
		/// <param name="measurementUnit">Measurement unit to which prefix will be attached.</param>
		/// <param name="format">A numeric format string for value part.</param>
		public static string ToSIPrefixString(this double value, string measurementUnit, string format) {
			return ConvertToString(value, measurementUnit, format, CultureInfo.CurrentCulture, prefixBigValues, prefixBigSymbols, prefixSmallValues, prefixSmallSymbols);
		}

        /// <summary>
        /// Converts the value of this instance to its equivalent string representation with measurement unit prefixed with SI prefix symbol.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <param name="measurementUnit">Measurement unit to which prefix will be attached.</param>
        /// <param name="format">A numeric format string for value part.</param>
        public static string ToSIPrefixString(this long value, string measurementUnit, string format)
        {
            return ConvertToString((double)value, measurementUnit, format, CultureInfo.CurrentCulture, prefixBigValues, prefixBigSymbols, prefixSmallValues, prefixSmallSymbols);
        }

		/// <summary>
		/// Converts the value of this instance to its equivalent string representation with measurement unit prefixed with SI prefix symbol.
		/// </summary>
		/// <param name="value">Value to convert.</param>
		/// <param name="measurementUnit">Measurement unit to which prefix will be attached.</param>
		/// <param name="format">A numeric format string for value part.</param>
		/// <param name="formatProvider">An System.IFormatProvider that supplies culture-specific formatting information for value part.</param>
		public static string ToSIPrefixString(this double value, string measurementUnit, string format, IFormatProvider formatProvider) {
			return ConvertToString(value, measurementUnit, format, formatProvider, prefixBigValues, prefixBigSymbols, prefixSmallValues, prefixSmallSymbols);
		}

        /// <summary>
        /// Converts the value of this instance to its equivalent string representation with measurement unit prefixed with SI prefix symbol.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <param name="measurementUnit">Measurement unit to which prefix will be attached.</param>
        /// <param name="format">A numeric format string for value part.</param>
        /// <param name="formatProvider">An System.IFormatProvider that supplies culture-specific formatting information for value part.</param>
        public static string ToSIPrefixString(this long value, string measurementUnit, string format, IFormatProvider formatProvider)
        {
            return ConvertToString((double)value, measurementUnit, format, formatProvider, prefixBigValues, prefixBigSymbols, prefixSmallValues, prefixSmallSymbols);
        }


		/// <summary>
		/// Converts the value of this instance to its equivalent string representation with measurement unit prefixed with SI prefix text.
		/// </summary>
		/// <param name="value">Value to convert.</param>
		/// <param name="measurementUnit">Measurement unit to which prefix will be attached.</param>
		public static string ToLongSIPrefixString(this double value, string measurementUnit) {
			return ConvertToString(value, measurementUnit, null, CultureInfo.CurrentCulture, prefixBigValues, prefixBigTexts, prefixSmallValues, prefixSmallTexts);
		}

        /// <summary>
        /// Converts the value of this instance to its equivalent string representation with measurement unit prefixed with SI prefix text.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <param name="measurementUnit">Measurement unit to which prefix will be attached.</param>
        public static string ToLongSIPrefixString(this long value, string measurementUnit)
        {
            return ConvertToString((double)value, measurementUnit, null, CultureInfo.CurrentCulture, prefixBigValues, prefixBigTexts, prefixSmallValues, prefixSmallTexts);
        }

		/// <summary>
		/// Converts the value of this instance to its equivalent string representation with measurement unit prefixed with SI prefix text.
		/// </summary>
		/// <param name="value">Value to convert.</param>
		/// <param name="measurementUnit">Measurement unit to which prefix will be attached.</param>
		/// <param name="format">A numeric format string for value part.</param>
		public static string ToLongSIPrefixString(this double value, string measurementUnit, string format) {
			return ConvertToString(value, measurementUnit, format, CultureInfo.CurrentCulture, prefixBigValues, prefixBigTexts, prefixSmallValues, prefixSmallTexts);
		}

        /// <summary>
        /// Converts the value of this instance to its equivalent string representation with measurement unit prefixed with SI prefix text.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <param name="measurementUnit">Measurement unit to which prefix will be attached.</param>
        /// <param name="format">A numeric format string for value part.</param>
        public static string ToLongSIPrefixString(this long value, string measurementUnit, string format)
        {
            return ConvertToString((double)value, measurementUnit, format, CultureInfo.CurrentCulture, prefixBigValues, prefixBigTexts, prefixSmallValues, prefixSmallTexts);
        }

		/// <summary>
		/// Converts the value of this instance to its equivalent string representation with measurement unit prefixed with SI prefix text.
		/// </summary>
		/// <param name="value">Value to convert.</param>
		/// <param name="measurementUnit">Measurement unit to which prefix will be attached.</param>
		/// <param name="format">A numeric format string for value part.</param>
		/// <param name="formatProvider">An System.IFormatProvider that supplies culture-specific formatting information for value part.</param>
		public static string ToLongSIPrefixString(this double value, string measurementUnit, string format, IFormatProvider formatProvider) {
			return ConvertToString(value, measurementUnit, format, formatProvider, prefixBigValues, prefixBigTexts, prefixSmallValues, prefixSmallTexts);
		}

        /// <summary>
        /// Converts the value of this instance to its equivalent string representation with measurement unit prefixed with SI prefix text.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <param name="measurementUnit">Measurement unit to which prefix will be attached.</param>
        /// <param name="format">A numeric format string for value part.</param>
        /// <param name="formatProvider">An System.IFormatProvider that supplies culture-specific formatting information for value part.</param>
        public static string ToLongSIPrefixString(this long value, string measurementUnit, string format, IFormatProvider formatProvider)
        {
            return ConvertToString((double)value, measurementUnit, format, formatProvider, prefixBigValues, prefixBigTexts, prefixSmallValues, prefixSmallTexts);
        }




		private static string ConvertToString(double value, string measurementUnit, string format, IFormatProvider formatProvider, double[] bigValues, string[] bigStrings, double[] smallValues, string[] smallStrings) {
			for (int i = 0; i < bigValues.Length; ++i) {
				double prefixValue = bigValues[i];
				if (value >= prefixValue) {
					return (value / prefixValue).ToString(format, formatProvider) + " " + bigStrings[i] + measurementUnit;
				}
			}

			for (int i = 0; i < smallValues.Length; ++i) {
				double prefixValue = smallValues[i];
				if (value <= prefixValue) {
					return (value / prefixValue).ToString(format, formatProvider) + " " + smallStrings[i] + measurementUnit;
				}
			}

			return value.ToString(format, formatProvider) + " " + measurementUnit;
		}

	}

}
