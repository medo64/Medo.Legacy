//Copyright (c) 2008 Josip Medved <jmedved@jmedved.com>

//2008-03-29: Initial version.
//2010-05-14: Changed namespace.


using System;
using System.Globalization;

namespace Medo.Extensions.ScientificNotation {

    /// <summary>
    /// Conversions to scientific notation strings.
    /// </summary>
	public static class ScientificNotationExtensions {

		/// <summary>
		/// Converts the value of this instance to its equivalent string representation with measurement unit prefixed with binary SI symbol.
		/// </summary>
		/// <param name="value">Value to convert.</param>
		public static string ToNormalizedExponentString(this double value) {
			return ConvertToString(value, -1, CultureInfo.CurrentCulture, 1);
		}

		/// <summary>
		/// Converts the value of this instance to its equivalent string representation with measurement unit prefixed with binary SI symbol.
		/// </summary>
		/// <param name="value">Value to convert.</param>
		/// <param name="decimalPlaces">Number of decimal places. If value is -1, all significant digits up to 6 places will be shown.</param>
		public static string ToNormalizedExponentString(this double value, int decimalPlaces) {
			return ConvertToString(value, decimalPlaces, CultureInfo.CurrentCulture, 1);
		}

		/// <summary>
		/// Converts the value of this instance to its equivalent string representation with measurement unit prefixed with binary SI symbol.
		/// </summary>
		/// <param name="value">Value to convert.</param>
		/// <param name="decimalPlaces">Number of decimal places. If value is -1, all significant digits up to 6 places will be shown.</param>
		/// <param name="formatProvider">An System.IFormatProvider that supplies culture-specific formatting information for value part.</param>
		public static string ToNormalizedExponentString(this double value, int decimalPlaces, IFormatProvider formatProvider) {
			return ConvertToString(value, decimalPlaces, formatProvider, 1);
		}


		/// <summary>
		/// Converts the value of this instance to its equivalent string representation with measurement unit prefixed with binary SI symbol.
		/// </summary>
		/// <param name="value">Value to convert.</param>
		public static string ToEngineeringExponentString(this double value) {
			return ConvertToString(value, -1, CultureInfo.CurrentCulture, 3);
		}

		/// <summary>
		/// Converts the value of this instance to its equivalent string representation with measurement unit prefixed with binary SI symbol.
		/// </summary>
		/// <param name="value">Value to convert.</param>
		/// <param name="decimalPlaces">Number of decimal places. If value is -1, all significant digits up to 6 places will be shown.</param>
		public static string ToEngineeringExponentString(this double value, int decimalPlaces) {
			return ConvertToString(value, decimalPlaces, CultureInfo.CurrentCulture, 3);
		}

		/// <summary>
		/// Converts the value of this instance to its equivalent string representation with measurement unit prefixed with binary SI symbol.
		/// </summary>
		/// <param name="value">Value to convert.</param>
		/// <param name="decimalPlaces">Number of decimal places. If value is -1, all significant digits up to 6 places will be shown.</param>
		/// <param name="formatProvider">An System.IFormatProvider that supplies culture-specific formatting information for value part.</param>
		public static string ToEngineeringExponentString(this double value, int decimalPlaces, IFormatProvider formatProvider) {
			return ConvertToString(value, decimalPlaces, formatProvider, 3);
		}


		/// <summary>
		/// Converts the value of this instance to its equivalent string representation with measurement unit prefixed with binary SI symbol.
		/// </summary>
		/// <param name="value">Value to convert.</param>
		/// <param name="exponent">Exponent that value should be converted into.</param>
		public static string ToForcedExponentString(this double value, int exponent) {
			return ToForcedExponentString(value, exponent, -1, CultureInfo.CurrentCulture);
		}

		/// <summary>
		/// Converts the value of this instance to its equivalent string representation with measurement unit prefixed with binary SI symbol.
		/// </summary>
		/// <param name="value">Value to convert.</param>
		/// <param name="exponent">Exponent that value should be converted into.</param>
		/// <param name="decimalPlaces">Number of decimal places. If value is -1, all significant digits up to 6 places will be shown.</param>
		public static string ToForcedExponentString(this double value, int exponent, int decimalPlaces) {
			return ToForcedExponentString(value, exponent, decimalPlaces, CultureInfo.CurrentCulture);
		}

		/// <summary>
		/// Converts the value of this instance to its equivalent string representation with measurement unit prefixed with binary SI symbol.
		/// </summary>
		/// <param name="value">Value to convert.</param>
		/// <param name="exponent">Exponent that value should be converted into.</param>
		/// <param name="decimalPlaces">Number of decimal places. If value is -1, all significant digits up to 6 places will be shown.</param>
		/// <param name="formatProvider">An System.IFormatProvider that supplies culture-specific formatting information for value part.</param>
		public static string ToForcedExponentString(this double value, int exponent, int decimalPlaces, IFormatProvider formatProvider) {
			string format;
			if (decimalPlaces <= -1) {
				format = "0.######";
			} else if (decimalPlaces == 0) {
				format = "0";
			} else {
				format = "0." + new string('0', decimalPlaces);
			}

            return (value / System.Math.Pow(10, exponent)).ToString(format, formatProvider) + "e" + exponent.ToString("0", CultureInfo.InvariantCulture);
		}




		private static string ConvertToString(double value, int decimalPlaces, IFormatProvider formatProvider, int stepPower) {
			string format;
			if (decimalPlaces <= -1) {
				format = "0.######";
			} else if (decimalPlaces == 0) {
				format = "0";
			} else {
				format = "0." + new string('0', decimalPlaces);
			}

			int e = 0;
            double step = System.Math.Pow(10, stepPower);
			if (value >= 1) {
				while (value >= step) {
					value /= step;
					e += stepPower;
				}
			} else {
				while (value < 1) {
					value *= step;
					e -= stepPower;
				}
			}

			return value.ToString(format, formatProvider) + "e" + e.ToString("0", CultureInfo.InvariantCulture);
		}

	}

}
