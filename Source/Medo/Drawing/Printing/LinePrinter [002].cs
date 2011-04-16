//Copyright (c) 2008 Josip Medved <jmedved@jmedved.com>

//2008-01-22: Initial version.
//2008-01-31: All empty places are 0 instead of space.


using System;
using System.Collections.Generic;

namespace Medo.Drawing.Printing {

	/// <summary>
	/// Class for printing fixed width text.
	/// </summary>
	public class LinePrinter : System.IDisposable {

		private List<char[]> _lines = new List<char[]>();


		/// <summary>
		/// Creates new instance.
		/// Default width is 80.
		/// </summary>
		public LinePrinter() : this(80) { }

		/// <summary>
		/// Creates new instance.
		/// </summary>
		/// <param name="width">Total width of paper in characters.</param>
		public LinePrinter(int width) {
			this._width = width;
		}


		/// <summary>
		/// Gets all lines currently in buffer.
		/// </summary>
		public string[] GetLines() {
			List<string> lines = new List<string>();
			for (int i = 0; i < this._lines.Count; ++i) {
				lines.Add(new string(this._lines[i]).Replace('\0', ' '));
			}
			return lines.ToArray();
		}

		/// <summary>
		/// Gets number of lines.
		/// </summary>
		public int LineCount {
			get { return this._lines.Count; }
		}


		/// <summary>
		/// Writes text to line at given index.
		/// </summary>
		/// <param name="lineIndex">Index of line to modify. If line number is higher than last line (e.g. int.MaxValue), new line is created. If negative value is used (e.g -1), last line is used as base.</param>
		/// <param name="text">Text to be added.</param>
		/// <param name="left">Index at which text will be added.</param>
		/// <returns>Index of modified line. It may not be last line since word wrap can occurr.</returns>
		public int Write(int lineIndex, string text, int left) {
			return Write(lineIndex, text, left, this.Width - left, System.Windows.Forms.HorizontalAlignment.Left, true);
		}

		/// <summary>
		/// Writes text to line at given index.
		/// </summary>
		/// <param name="lineIndex">Index of line to modify. If line number is higher than last line (e.g. int.MaxValue), new line is created. If negative value is used (e.g -1), last line is used as base.</param>
		/// <param name="text">Text to be added.</param>
		/// <param name="left">Index at which text will be added.</param>
		/// <param name="width">Total width of given text.</param>
		/// <returns>Index of modified line. It may not be last line since word wrap can occurr.</returns>
		public int Write(int lineIndex, string text, int left, int width) {
			return Write(lineIndex, text, left, width, System.Windows.Forms.HorizontalAlignment.Left, true);
		}

		private int _lastLine = -1;

		/// <summary>
		/// Writes text to line at given index.
		/// </summary>
		/// <param name="lineIndex">Index of line to modify. If line number is higher than last line (e.g. int.MaxValue), new line is created. If negative value is used (e.g -1), last line is used as base.</param>
		/// <param name="text">Text to be added.</param>
		/// <param name="left">Index at which text will be added.</param>
		/// <param name="width">Total width of given text.</param>
		/// <param name="alignment">Alignment of text.</param>
		/// <param name="wordWrap">If false, text will not be wrapped to next line in case it is too long to fit on one line.</param>
		/// <returns>Index of modified line. It may not be last line since word wrap can occurr.</returns>
		/// <exception cref="System.ArgumentOutOfRangeException">Left cannot be negative. -or- Width cannot be zero or negative.</exception>
		/// <exception cref="System.OverflowException">Operation would result in write at location which is beyond end of paper.</exception>
		public int Write(int lineIndex, string text, int left, int width, System.Windows.Forms.HorizontalAlignment alignment, bool wordWrap) {
			if (left < 0) { throw new System.ArgumentOutOfRangeException("left", left, Resources.ExceptionLeftCannotBeNegative); }
			if (width <= 0) { throw new System.ArgumentOutOfRangeException("width", width, Resources.ExceptionWidthCannotBeZeroOrNegative); }
			if (left + width > this.Width) { throw new System.OverflowException(string.Format(System.Globalization.CultureInfo.InvariantCulture, Resources.ExceptionOperationWouldResultInWriteAtLocationWhichIsBeyondEndOfPaper, left + width - 1, this.Width - 1)); }


			List<string> wrappedContent = new List<string>();
			if (text.Length <= width) {
				wrappedContent.Add(text);
			} else { //trim is needed
				if (wordWrap == false) {
					wrappedContent.Add(text.Remove(width));
				} else {
					while (text != null) {
						if (text.Length <= width) {
							wrappedContent.Add(text);
							text = null;
						} else {
							int i = text.LastIndexOfAny(new char[] { '\0' }, width);
							if (i < 0) { //no space.
								wrappedContent.Add(text.Substring(0, width));
								text = text.Substring(width);
							} else { //space found
								wrappedContent.Add(text.Substring(0, i));
								text = text.Substring(i + 1);
							}
						}
					}
				}
			}

			string[] newContent = new string[wrappedContent.Count];
			for (int i = 0; i < wrappedContent.Count; ++i) {
				string curr = wrappedContent[i];
				switch (alignment) {
					case System.Windows.Forms.HorizontalAlignment.Left:
						newContent[i] = curr.PadRight(width, '\0');
						break;
					case System.Windows.Forms.HorizontalAlignment.Center:
						int addToLeft = (width - curr.Length) / 2;
						newContent[i] = curr.PadLeft(addToLeft + curr.Length, '\0').PadRight(width, '\0');
						break;
					case System.Windows.Forms.HorizontalAlignment.Right:
						newContent[i] = curr.PadLeft(width, '\0');
						break;
					default:
						newContent[i] = curr.PadRight(width, '\0');
						break;
				}
			}


			int addLineCount = 0;
			if ((lineIndex >= this._lines.Count) || (this._lines.Count == 0)) { //add new line
				lineIndex = this._lines.Count;
				addLineCount = newContent.Length;
			} else if (lineIndex < 0) { //check last line
				lineIndex = this._lastLine;
				addLineCount = System.Math.Max(0, newContent.Length - (this._lines.Count - lineIndex));
			} else { //use existing line.
				addLineCount = System.Math.Max(0, newContent.Length - (this._lines.Count - lineIndex));
			}
			for (int i = 0; i < addLineCount; ++i) {
				this._lines.Add((new string('\0', this.Width)).ToCharArray());
			}


			for (int i = 0; i < newContent.Length; ++i) {
				int x = lineIndex + i;
				for (int j = 0; j < width; ++j) {
					int y = left + j;
					if (newContent[i][j] != '\0') {
						this._lines[x][y] = newContent[i][j];
					}
				}
			}


			this._lastLine = lineIndex;
			return lineIndex;
		}


		/// <summary>
		/// Adds new line.
		/// </summary>
		public int WriteLine() {
			return Write(int.MaxValue, string.Empty, 0, this.Width, System.Windows.Forms.HorizontalAlignment.Left, true);
		}

		/// <summary>
		/// Adds new line with given text.
		/// </summary>
		/// <param name="text">Text to write</param>
		public int WriteLine(string text) {
			return Write(int.MaxValue, text, 0, this.Width, System.Windows.Forms.HorizontalAlignment.Left, true);
		}

		/// <summary>
		/// Adds new line with given text.
		/// </summary>
		/// <param name="text">Text to write</param>
		/// <param name="alignment">Alignment of text.</param>
		public int WriteLine(string text, System.Windows.Forms.HorizontalAlignment alignment) {
			return Write(int.MaxValue, text, 0, this.Width, alignment, true);
		}

		/// <summary>
		/// Adds new line with given text.
		/// </summary>
		/// <param name="text">Text to write</param>
		/// <param name="alignment">Alignment of text.</param>
		/// <param name="wordWrap">If false, text will not be wrapped to next line in case it is too long to fit on one line.</param>
		public int WriteLine(string text, System.Windows.Forms.HorizontalAlignment alignment, bool wordWrap) {
			return Write(int.MaxValue, text, 0, this.Width, alignment, wordWrap);
		}


		private int _width;
		/// <summary>
		/// Gets with of paper in characters.
		/// </summary>
		public int Width {
			get { return this._width; }
		}


		/// <summary>
		/// Returns whole paper content as string.
		/// </summary>
		public override string ToString() {
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			for (int i = 0; i < this._lines.Count; ++i) {
				sb.AppendLine(new string(this._lines[i]).Replace('\0', ' '));
			}
			return sb.ToString();
		}

		#region IDisposable Members

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		public void Dispose() {
			this.Dispose(true);
			System.GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">True if managed resources should be disposed; otherwise, false.</param>
		protected virtual void Dispose(bool disposing) {
			if (disposing) {
				_lines.Clear();
				_lines = null;
			}
		}

		#endregion


		private static class Resources {

			internal static string ExceptionLeftCannotBeNegative { get { return "Left cannot be negative."; } }

			internal static string ExceptionWidthCannotBeZeroOrNegative { get { return "Width cannot be zero or negative."; } }

			internal static string ExceptionOperationWouldResultInWriteAtLocationWhichIsBeyondEndOfPaper { get { return "Operation would result in write at location {0} which is beyond end of paper (at {1})."; } }

		}

	}

}
