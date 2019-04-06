/* Josip Medved <jmedved@jmedved.com> * www.medo64.com * MIT License */

//2008-01-31: All empty places are 0 instead of space.
//2008-01-22: Initial version.


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
            Width = width;
        }


        /// <summary>
        /// Gets all lines currently in buffer.
        /// </summary>
        public string[] GetLines() {
            var lines = new List<string>();
            for (var i = 0; i < _lines.Count; ++i) {
                lines.Add(new string(_lines[i]).Replace('\0', ' '));
            }
            return lines.ToArray();
        }

        /// <summary>
        /// Gets number of lines.
        /// </summary>
        public int LineCount {
            get { return _lines.Count; }
        }


        /// <summary>
        /// Writes text to line at given index.
        /// </summary>
        /// <param name="lineIndex">Index of line to modify. If line number is higher than last line (e.g. int.MaxValue), new line is created. If negative value is used (e.g -1), last line is used as base.</param>
        /// <param name="text">Text to be added.</param>
        /// <param name="left">Index at which text will be added.</param>
        /// <returns>Index of modified line. It may not be last line since word wrap can occurr.</returns>
        public int Write(int lineIndex, string text, int left) {
            return Write(lineIndex, text, left, Width - left, System.Windows.Forms.HorizontalAlignment.Left, true);
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
            if (left + width > Width) { throw new System.OverflowException(string.Format(System.Globalization.CultureInfo.InvariantCulture, Resources.ExceptionOperationWouldResultInWriteAtLocationWhichIsBeyondEndOfPaper, left + width - 1, Width - 1)); }


            var wrappedContent = new List<string>();
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
                            var i = text.LastIndexOfAny(new char[] { '\0' }, width);
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

            var newContent = new string[wrappedContent.Count];
            for (var i = 0; i < wrappedContent.Count; ++i) {
                var curr = wrappedContent[i];
                switch (alignment) {
                    case System.Windows.Forms.HorizontalAlignment.Left:
                        newContent[i] = curr.PadRight(width, '\0');
                        break;
                    case System.Windows.Forms.HorizontalAlignment.Center:
                        var addToLeft = (width - curr.Length) / 2;
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

            int addLineCount;
            if ((lineIndex >= _lines.Count) || (_lines.Count == 0)) { //add new line
                lineIndex = _lines.Count;
                addLineCount = newContent.Length;
            } else if (lineIndex < 0) { //check last line
                lineIndex = _lastLine;
                addLineCount = System.Math.Max(0, newContent.Length - (_lines.Count - lineIndex));
            } else { //use existing line.
                addLineCount = System.Math.Max(0, newContent.Length - (_lines.Count - lineIndex));
            }
            for (var i = 0; i < addLineCount; ++i) {
                _lines.Add((new string('\0', Width)).ToCharArray());
            }


            for (var i = 0; i < newContent.Length; ++i) {
                var x = lineIndex + i;
                for (var j = 0; j < width; ++j) {
                    var y = left + j;
                    if (newContent[i][j] != '\0') {
                        _lines[x][y] = newContent[i][j];
                    }
                }
            }


            _lastLine = lineIndex;
            return lineIndex;
        }


        /// <summary>
        /// Adds new line.
        /// </summary>
        public int WriteLine() {
            return Write(int.MaxValue, string.Empty, 0, Width, System.Windows.Forms.HorizontalAlignment.Left, true);
        }

        /// <summary>
        /// Adds new line with given text.
        /// </summary>
        /// <param name="text">Text to write</param>
        public int WriteLine(string text) {
            return Write(int.MaxValue, text, 0, Width, System.Windows.Forms.HorizontalAlignment.Left, true);
        }

        /// <summary>
        /// Adds new line with given text.
        /// </summary>
        /// <param name="text">Text to write</param>
        /// <param name="alignment">Alignment of text.</param>
        public int WriteLine(string text, System.Windows.Forms.HorizontalAlignment alignment) {
            return Write(int.MaxValue, text, 0, Width, alignment, true);
        }

        /// <summary>
        /// Adds new line with given text.
        /// </summary>
        /// <param name="text">Text to write</param>
        /// <param name="alignment">Alignment of text.</param>
        /// <param name="wordWrap">If false, text will not be wrapped to next line in case it is too long to fit on one line.</param>
        public int WriteLine(string text, System.Windows.Forms.HorizontalAlignment alignment, bool wordWrap) {
            return Write(int.MaxValue, text, 0, Width, alignment, wordWrap);
        }

        /// <summary>
        /// Gets with of paper in characters.
        /// </summary>
        public int Width { get; private set; }


        /// <summary>
        /// Returns whole paper content as string.
        /// </summary>
        public override string ToString() {
            var sb = new System.Text.StringBuilder();
            for (var i = 0; i < _lines.Count; ++i) {
                sb.AppendLine(new string(_lines[i]).Replace('\0', ' '));
            }
            return sb.ToString();
        }

        #region IDisposable Members

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        public void Dispose() {
            Dispose(true);
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
