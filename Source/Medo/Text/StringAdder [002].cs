/* Josip Medved <jmedved@jmedved.com> * www.medo64.com * MIT License */

//2008-04-11: Cleaned code to match FxCop 1.36 beta 2 (SpecifyIFormatProvider, SpecifyStringComparison).
//2008-01-26: New version.


using System.Globalization;
using System;
namespace Medo.Text {

	/// <summary>
	/// StringBuilder with user defined separator.
	/// </summary>
	public class StringAdder {

		/// <summary>
		/// Creates new instance with system defined separator.
		/// </summary>
		public StringAdder() {
			StringBuilder = new System.Text.StringBuilder();
			Separator = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ListSeparator;
		}

		/// <summary>
		/// Creates new instance with user defined separator.
		/// </summary>
		/// <param name="separator">String to use for separating different values.</param>
		public StringAdder(string separator) {
			StringBuilder = new System.Text.StringBuilder();
			Separator = separator;
		}



		/// <summary>
		/// Appends a copy of specified string to instance. Separator is added in front of given value.
		/// </summary>
		/// <param name="value">The System.String to append.</param>
		public void Append(string value) {
			Append(value, Separator);
		}

		/// <summary>
		/// Appends a copy of specified string to instance. Separator is added in front of given value.
		/// </summary>
		/// <param name="value">The System.String to append.</param>
		/// <param name="checkForExistingSeparator">If true, additional check is made to see if separator already exists.</param>
		public void Append(string value, bool checkForExistingSeparator) {
			Append(value, Separator, checkForExistingSeparator);
		}

		/// <summary>
		/// Appends a copy of specified string to instance. Separator is added in front of given value.
		/// </summary>
		/// <param name="value">The System.String to append.</param>
		/// <param name="separator">Separator to be added before text.</param>
		public void Append(string value, string separator) {
			Append(value, separator, false);
		}

		/// <summary>
		/// Appends a copy of specified string to instance. Separator is added in front of given value.
		/// </summary>
		/// <param name="value">The System.String to append.</param>
		/// <param name="separator">Separator to be added before text.</param>
		/// <param name="checkForExistingSeparator">If true, additional check is made to see if separator already exists.</param>
		public void Append(string value, string separator, bool checkForExistingSeparator) {
			if (value == null) { return; }
			if (separator == null) { separator = string.Empty; }
			if (StringBuilder.Length == 0) {
				StringBuilder.Append(value);
			} else {
				if ((checkForExistingSeparator == false) || ((!StringBuilder.ToString().EndsWith(Separator, StringComparison.CurrentCulture)) && (!value.StartsWith(Separator, StringComparison.CurrentCulture)) && (!string.IsNullOrEmpty(value)))) {
					StringBuilder.Append(separator);
				}
				StringBuilder.Append(value);
			}
		}

		/// <summary>
		/// Appends a formatted string, which contains zero or more format specifications, to this instance. Each format specification is replaced by the string representation of a corresponding object argument.
		/// </summary>
		/// <param name="format">A string containing zero or more format specifications.</param>
		/// <param name="args">An array of objects to format.</param>
		/// <returns>A reference to this instance with format appended. Any format specification in format is replaced by the string representation of the corresponding object argument.</returns>
		public StringAdder AppendFormat(string format, params object[] args) {
			return AppendFormat(CultureInfo.CurrentCulture, format, args);
		}

		/// <summary>
		/// Appends a formatted string, which contains zero or more format specifications, to this instance. Each format specification is replaced by the string representation of a corresponding object argument.
		/// </summary>
		/// <param name="provider">An System.IFormatProvider that supplies culture-specific formatting information.</param>
		/// <param name="format">A string containing zero or more format specifications.</param>
		/// <param name="args">An array of objects to format.</param>
		/// <returns>A reference to this instance with format appended. Any format specification in format is replaced by the string representation of the corresponding object argument.</returns>
		public StringAdder AppendFormat(IFormatProvider provider, string format, params object[] args) {
			if (StringBuilder.Length == 0) {
				_stringBuilder.AppendFormat(provider, format, args);
			} else {
				StringBuilder.Append(Separator);
				_stringBuilder.AppendFormat(provider, format, args);
			}
			return this;
		}


		/// <summary>
		/// Converts this instance to string.
		/// </summary>
		/// <returns>String.</returns>
		public new string ToString() {
			return StringBuilder.ToString();
		}



		private string _separator;
		/// <summary>
		/// Gets or sets string to use for separating different values.
		/// </summary>
		public string Separator {
			get { return _separator; }
			set { _separator = value; }
		}

		private System.Text.StringBuilder _stringBuilder;
		/// <summary>
		/// Gets underlying string builder.
		/// </summary>
		public System.Text.StringBuilder StringBuilder {
			get { return _stringBuilder; }
			private set { _stringBuilder = value; }
		}

	}

}
