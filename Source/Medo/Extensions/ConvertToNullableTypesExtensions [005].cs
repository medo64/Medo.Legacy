/* Josip Medved <jmedved@jmedved.com> * www.medo64.com * MIT License */

//2010-05-14: Changed namespace.
//2008-11-14: Added boolean conversion.
//2008-08-04: Added overloads for non-extension usage.
//2008-04-10: Now uses IFormatProvider.
//2008-04-05: Initial version.


using System;
using System.Globalization;

namespace Medo.Extensions.ConvertToNullableTypes {

    /// <summary>
    /// Conversions from objects to most common nullable types.
    /// </summary>
    public static class ConvertToNullableTypesExtensions {

        /// <summary>
        /// Returns Nullable&lt;Boolean&gt;.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        public static bool? ExtToNullableBoolean(object value)
        {
            if (value == null) { return null; }
            if (System.Convert.IsDBNull(value)) { return null; }
            return System.Convert.ToBoolean(value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Returns Nullable&lt;Byte&gt;.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        public static byte? ExtToNullableByte(object value) {
            if (value == null) { return null; }
            if (System.Convert.IsDBNull(value)) { return null; }
            return System.Convert.ToByte(value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Returns Nullable&lt;Int16&gt;.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        public static short? ExtToNullableInt16(object value) {
            if (value == null) { return null; }
            if (System.Convert.IsDBNull(value)) { return null; }
            return System.Convert.ToInt16(value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Returns Nullable&lt;Int32&gt;.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        public static int? ExtToNullableInt32(object value) {
            if (value == null) { return null; }
            if (System.Convert.IsDBNull(value)) { return null; }
            return System.Convert.ToInt32(value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Returns Nullable&lt;Int64&gt;.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        public static long? ExtToNullableInt64(object value) {
            if (value == null) { return null; }
            if (System.Convert.IsDBNull(value)) { return null; }
            return System.Convert.ToInt64(value, CultureInfo.InvariantCulture);
        }


        /// <summary>
        /// Returns Nullable&lt;Decimal&gt;.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        public static decimal? ExtToNullableDecimal(object value) {
            if (value == null) { return null; }
            if (System.Convert.IsDBNull(value)) { return null; }
            return System.Convert.ToDecimal(value, CultureInfo.InvariantCulture);
        }


        /// <summary>
        /// Returns Nullable&lt;Single&gt;.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        public static float? ExtToNullableSingle(object value) {
            if (value == null) { return null; }
            if (System.Convert.IsDBNull(value)) { return null; }
            return System.Convert.ToSingle(value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Returns Nullable&lt;Double&gt;.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        public static double? ExtToNullableDouble(object value) {
            if (value == null) { return null; }
            if (System.Convert.IsDBNull(value)) { return null; }
            return System.Convert.ToDouble(value, CultureInfo.InvariantCulture);
        }


        /// <summary>
        /// Returns Nullable&lt;Char&gt;.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        public static char? ExtToNullableChar(object value) {
            if (value == null) { return null; }
            if (System.Convert.IsDBNull(value)) { return null; }
            return System.Convert.ToChar(value, CultureInfo.InvariantCulture);
        }


        /// <summary>
        /// Returns Nullable&lt;DateTime&gt;.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        public static DateTime? ExtToNullableDateTime(object value) {
            if (value == null) { return null; }
            if (System.Convert.IsDBNull(value)) { return null; }
            return System.Convert.ToDateTime(value, CultureInfo.InvariantCulture);
        }


        /// <summary>
        /// Returns Nullable&lt;Boolean&gt;.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        public static bool? ToNullableBoolean(this object value)
        {
            return ExtToNullableBoolean(value);
        }

        /// <summary>
        /// Returns Nullable&lt;Byte&gt;.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        public static byte? ToNullableByte(this object value) {
            return ExtToNullableByte(value);
        }

        /// <summary>
        /// Returns Nullable&lt;Int16&gt;.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        public static short? ToNullableInt16(this object value) {
            return ExtToNullableInt16(value);
        }

        /// <summary>
        /// Returns Nullable&lt;Int32&gt;.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        public static int? ToNullableInt32(this object value) {
            return ExtToNullableInt32(value);
        }

        /// <summary>
        /// Returns Nullable&lt;Int64&gt;.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        public static long? ToNullableInt64(this object value) {
            return ExtToNullableInt64(value);
        }


        /// <summary>
        /// Returns Nullable&lt;Decimal&gt;.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        public static decimal? ToNullableDecimal(this object value) {
            return ExtToNullableDecimal(value);
        }


        /// <summary>
        /// Returns Nullable&lt;Single&gt;.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        public static float? ToNullableSingle(this object value) {
            return ExtToNullableSingle(value);
        }

        /// <summary>
        /// Returns Nullable&lt;Double&gt;.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        public static double? ToNullableDouble(this object value) {
            return ExtToNullableDouble(value);
        }


        /// <summary>
        /// Returns Nullable&lt;Char&gt;.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        public static char? ToNullableChar(this object value) {
            return ExtToNullableChar(value);
        }


        /// <summary>
        /// Returns Nullable&lt;DateTime&gt;.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        public static DateTime? ToNullableDateTime(this object value) {
            return ExtToNullableDateTime(value);
        }

    }

}
