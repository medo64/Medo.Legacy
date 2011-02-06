//Josip Medved <jmedved@jmedved.com> http://www.jmedved.com

//2008-04-05: Initial version.
//2008-04-10: Now uses IFormatProvider.
//2008-08-04: Added overloads for non-extension usage.
//            Added ToString conversion.
//2008-11-14: Added boolean conversion.
//2010-05-14: Changed namespace.


using System;
using System.Globalization;

namespace Medo.Extensions.ConvertToTypes {

    /// <summary>
    /// Conversions from objects to most common nullable types.
    /// </summary>
    public static class ConvertToTypesExtensions {

        /// <summary>
        /// Returns converted value.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        public static Boolean ExtToBoolean(object value)
        {
            return System.Convert.ToBoolean(value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Returns converted value.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        public static Byte ExtToByte(object value) {
            return System.Convert.ToByte(value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Returns converted value.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        public static Int16 ExtToInt16(object value) {
            return System.Convert.ToInt16(value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Returns converted value.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        public static Int32 ExtToInt32(object value) {
            return System.Convert.ToInt32(value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Returns converted value.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        public static Int64 ExtToInt64(object value) {
            return System.Convert.ToInt64(value, CultureInfo.InvariantCulture);
        }


        /// <summary>
        /// Returns converted value.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        public static Decimal ExtToDecimal(object value) {
            return System.Convert.ToDecimal(value, CultureInfo.InvariantCulture);
        }


        /// <summary>
        /// Returns converted value.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        public static Single ExtToSingle(object value) {
            return System.Convert.ToSingle(value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Returns converted value.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        public static Double ExtToDouble(object value) {
            return System.Convert.ToDouble(value, CultureInfo.InvariantCulture);
        }


        /// <summary>
        /// Returns converted value.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        public static Char ExtToChar(object value) {
            return System.Convert.ToChar(value, CultureInfo.InvariantCulture);
        }


        /// <summary>
        /// Returns converted value.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        public static DateTime ExtToDateTime(object value) {
            return System.Convert.ToDateTime(value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Returns converted value.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        public static String ExtToString(object value) {
            if (value == null) { return null; }
            if (System.Convert.IsDBNull(value)) { return null; }
            return System.Convert.ToString(value, CultureInfo.InvariantCulture);
        }


        /// <summary>
        /// Returns converted value.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        public static Boolean ToBoolean(this object value)
        {
            return System.Convert.ToBoolean(value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Returns converted value.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        public static Byte ToByte(this object value) {
            return System.Convert.ToByte(value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Returns converted value.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        public static Int16 ToInt16(this object value) {
            return System.Convert.ToInt16(value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Returns converted value.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        public static Int32 ToInt32(this object value) {
            return System.Convert.ToInt32(value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Returns converted value.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        public static Int64 ToInt64(this object value) {
            return System.Convert.ToInt64(value, CultureInfo.InvariantCulture);
        }


        /// <summary>
        /// Returns converted value.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        public static Decimal ToDecimal(this object value) {
            return System.Convert.ToDecimal(value, CultureInfo.InvariantCulture);
        }


        /// <summary>
        /// Returns converted value.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        public static Single ToSingle(this object value) {
            return System.Convert.ToSingle(value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Returns converted value.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        public static Double ToDouble(this object value) {
            return System.Convert.ToDouble(value, CultureInfo.InvariantCulture);
        }


        /// <summary>
        /// Returns converted value.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        public static Char ToChar(this object value) {
            return System.Convert.ToChar(value, CultureInfo.InvariantCulture);
        }


        /// <summary>
        /// Returns converted value.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        public static DateTime ToDateTime(this object value) {
            return System.Convert.ToDateTime(value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Returns converted value.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        public static String ToString(this object value) {
            return ExtToString(value);
        }

    }

}
