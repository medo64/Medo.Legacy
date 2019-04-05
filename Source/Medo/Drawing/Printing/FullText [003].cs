/* Josip Medved <jmedved@jmedved.com> * www.medo64.com * MIT License */

//2008-04-11: Cleaned code to match FxCop 1.36 beta 2 (SpecifyStringComparison).
//2008-01-21: Added Helper.Title.
//2008-01-03: Moved to blueprints.
//            Removed PrintPreview.


namespace Medo.Drawing.Printing {

	/// <summary>
	/// Class for printing text file.
	/// </summary>
	public class FullText : System.IDisposable {
        private string _text;

		/// <summary>
		/// Creates new instance.
		/// </summary>
		public FullText() : this(Helper.Title, "A4", 210F, 297F, 10F, 10F, 10F, 10F) { }

		/// <summary>
		/// Creates new instance.
		/// </summary>
		/// <param name="title">Title to be displayed while printing document.</param>
		public FullText(string title) : this(title, "A4", 210F, 297F, 10F, 10F, 10F, 10F) { }

		/// <summary>
		/// Creates new instance.
		/// </summary>
		/// <param name="title">Title to be displayed while printing document.</param>
		/// <param name="marginLeft">The left margin width, in millimeters.</param>
		/// <param name="marginRight">The right margin width, in millimeters.</param>
		/// <param name="marginTop">The top margin width, in millimeters.</param>
		/// <param name="marginBottom">The bottom margin width, in millimeters.</param>
		public FullText(string title, float marginLeft, float marginRight, float marginTop, float marginBottom) : this(title, "A4", 210F, 297F, marginLeft, marginRight, marginTop, marginBottom) { }

		/// <summary>
		/// Creates new instance.
		/// </summary>
		/// <param name="title">Title to be displayed while printing document.</param>
		/// <param name="paperName">The name of the type of paper.</param>
		/// <param name="paperWidth">The width of the paper, in millimeters.</param>
		/// <param name="paperHeight">The height of the paper, in millimeters.</param>
		public FullText(string title, string paperName, float paperWidth, float paperHeight) : this(title, paperName, paperWidth, paperHeight, 10F, 10F, 10F, 10F) { }

		/// <summary>
		/// Creates new instance using given PrintDocument as template.
		/// </summary>
		/// <param name="title">Title to be displayed while printing document.</param>
		/// <param name="paperName">The name of the type of paper.</param>
		/// <param name="paperWidth">The width of the paper, in millimeters.</param>
		/// <param name="paperHeight">The height of the paper, in millimeters.</param>
		/// <param name="marginLeft">The left margin width, in millimeters.</param>
		/// <param name="marginRight">The right margin width, in millimeters.</param>
		/// <param name="marginTop">The top margin width, in millimeters.</param>
		/// <param name="marginBottom">The bottom margin width, in millimeters.</param>
		public FullText(string title, string paperName, float paperWidth, float paperHeight, float marginLeft, float marginRight, float marginTop, float marginBottom) {
            Document = new System.Drawing.Printing.PrintDocument {
                OriginAtMargins = false,
                DocumentName = title
            };
            Document.DefaultPageSettings.PaperSize = new System.Drawing.Printing.PaperSize(paperName, System.Convert.ToInt32(paperWidth / 25.4 * 100), System.Convert.ToInt32(paperHeight / 25.4 * 100));
			Document.DefaultPageSettings.Margins = new System.Drawing.Printing.Margins(System.Convert.ToInt32(marginLeft / 25.4 * 100), System.Convert.ToInt32(marginRight / 25.4 * 100), System.Convert.ToInt32(marginTop / 25.4 * 100), System.Convert.ToInt32(marginBottom / 25.4 * 100));
			Document.PrinterSettings.DefaultPageSettings.PaperSize = Document.DefaultPageSettings.PaperSize;
			Document.PrinterSettings.DefaultPageSettings.Margins = Document.DefaultPageSettings.Margins;
			Document.OriginAtMargins = true;

			Document.BeginPrint += Document_BeginPrint;
			Document.PrintPage += Document_PrintPage;
		}

        /// <summary>
        /// Gets underlying document.
        /// </summary>
        public System.Drawing.Printing.PrintDocument Document { get; private set; }

        /// <summary>
        /// Default brush to be used for text if no other brush is specified.
        /// </summary>
        public System.Drawing.Brush Brush { get; set; } = System.Drawing.Brushes.Black;

        /// <summary>
        /// Default font to be used for text if no other font is specified.
        /// </summary>
        public System.Drawing.Font Font { get; set; } = new System.Drawing.Font("Tahoma", 10);

        /// <summary>
        /// Gets/sets text to be printed.
        /// </summary>
        public string Text {
			get {
				if (string.IsNullOrEmpty(_text)) {
					return "";
				} else {
					return _text;
				}
			}
			set {
				_text = value;
			}
		}

		/// <summary>
		/// Starts the document's printing process.
		/// </summary>
		public void Print() {
			Document.Print();
		}

		/// <summary>
		/// Occurs before printing starts.
		/// </summary>
		public event System.Drawing.Printing.PrintEventHandler BeginPrint;

		/// <summary>
		/// Occurs before printing of each page.
		/// </summary>
		public event System.Drawing.Printing.PrintPageEventHandler StartPrintPage;

		/// <summary>
		/// Occurs after printing of each page.
		/// </summary>
		public event System.Drawing.Printing.PrintPageEventHandler EndPrintPage;


		#region Printing

		private string _remainingText;

		private void Document_BeginPrint(object sender, System.Drawing.Printing.PrintEventArgs e) {
			_remainingText = _text;

            BeginPrint?.Invoke(this, e);
        }

		private void Document_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e) {
            StartPrintPage?.Invoke(this, e);
            while ((_remainingText.StartsWith(" ", System.StringComparison.Ordinal)) || (_remainingText.StartsWith(System.Convert.ToChar(13).ToString(), System.StringComparison.Ordinal)) || (_remainingText.StartsWith(System.Convert.ToChar(10).ToString(), System.StringComparison.Ordinal))) {
				_remainingText = _remainingText.Remove(0, 1);
			}
			var currText = _remainingText;

			do {
				var size = e.Graphics.MeasureString(currText, Font, e.MarginBounds.Width);
				if (e.MarginBounds.Height > size.Height) {
					e.Graphics.DrawString(currText, Font, Brush, new System.Drawing.RectangleF(0F, 0F, (float)(e.MarginBounds.Width), (float)(e.MarginBounds.Height)));
					_remainingText = _remainingText.Remove(0, currText.Length);
					break;
				} else { //remove one word
					var i = currText.LastIndexOfAny(new char[] { ' ', System.Convert.ToChar(13), System.Convert.ToChar(10) });
					if (i == 0) {
						currText = currText.Remove(currText.Length - 1);
					} else {
						currText = currText.Remove(i);
					}
				}
			} while (true);

			e.HasMorePages = (_remainingText.Length > 0);

            EndPrintPage?.Invoke(this, e);
        }

		#endregion


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
				Brush.Dispose();
				Font.Dispose();
			}
		}

		#endregion

		private static class Helper {

			internal static string Title {
				get {
					var titleAttributes = System.Reflection.Assembly.GetEntryAssembly().GetCustomAttributes(typeof(System.Reflection.AssemblyTitleAttribute), true);
					if ((titleAttributes != null) && (titleAttributes.Length >= 1)) {
						return ((System.Reflection.AssemblyTitleAttribute)titleAttributes[titleAttributes.Length - 1]).Title;
					} else {
						return System.Reflection.Assembly.GetEntryAssembly().GetName().Name;
					}
				}
			}

		}

	}

}
