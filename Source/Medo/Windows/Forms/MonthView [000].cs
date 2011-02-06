//Josip Medved <jmedved@jmedved.com> http://www.jmedved.com

//2008-05-31: New version.


namespace Medo.Windows.Forms {

	/// <summary>
	/// Represents a Windows control that enables the user to select a date using a visual monthly calendar display.
	/// </summary>
	[System.Drawing.ToolboxBitmap(typeof(MonthView))]
	public class MonthView : System.Windows.Forms.Control {

		private System.Collections.Generic.List<int> _annuallyBoldedDates = new System.Collections.Generic.List<int>();
		private System.Collections.Generic.List<System.DateTime> _boldedDates = new System.Collections.Generic.List<System.DateTime>();
		private System.Collections.Generic.List<int> _monthlyBoldedDates = new System.Collections.Generic.List<int>();


		/// <summary>
		/// Creates new instance.
		/// </summary>
		public MonthView() {
			this.DoubleBuffered = true;
			this.SetStyle(System.Windows.Forms.ControlStyles.Selectable, true);
			this.SetStyle(System.Windows.Forms.ControlStyles.StandardClick, true);
			this.SetStyle(System.Windows.Forms.ControlStyles.UserPaint, true);
			this.BackColor = System.Drawing.SystemColors.Window;
			this.SelectionRange = new System.Windows.Forms.SelectionRange(System.DateTime.Today, System.DateTime.Today);
		}


		private System.Drawing.Color _originalBackColor;


		#region Events

		/// <summary></summary>
		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		[System.ComponentModel.Browsable(false)]
		public new event System.EventHandler TextChanged {
			add { base.TextChanged += value; }
			remove { base.TextChanged -= value;}
		}

		/// <summary>
		/// Occurs when the selected date changes.
		/// </summary>
		public event System.Windows.Forms.DateRangeEventHandler DateChanged;

		#endregion


		#region Properties

		/// <summary>
		/// Gets or sets the array of System.DateTime objects that determines which annual days are displayed in bold.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is needed for compatibility with Microsoft's MonthCalendar control.")]
		public System.DateTime[] AnnuallyBoldedDates {
			get {
				System.Collections.Generic.List<System.DateTime> all = new System.Collections.Generic.List<System.DateTime>();
				for (int i = 0; i < this._annuallyBoldedDates.Count; i++) {
					int m = this._annuallyBoldedDates[i] / 100;
					int d = this._annuallyBoldedDates[i] % 100;
					all.Add(new System.DateTime(System.DateTime.Today.Year, m, d));
				}
				return all.ToArray();
			}
			set {
				this.RemoveAllAnnuallyBoldedDates();
				if (value != null) {
					for (int i = 0; i < value.Length; i++) {
						this.AddAnnuallyBoldedDate(value[i]);
					}
				}
				this.Invalidate();
			}
		}

		/// <summary>
		/// Gets or sets the array of System.DateTime objects that determines which nonrecurring dates are displayed in bold.
		/// </summary>
		/// <returns>The array of bold dates.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification="This is needed for compatibility with Microsoft's MonthCalendar control.")]
		public System.DateTime[] BoldedDates {
			get { return this._boldedDates.ToArray(); }
			set {
				this.RemoveAllBoldedDates();
				if (value != null) {
					for (int i = 0; i < value.Length; i++) {
						this.AddBoldedDate(value[i].Date);
					}
				}
				this.Invalidate();
			}
		}

		/// <summary>
		///	Gets or sets the array of System.DateTime objects that determine which monthly days to bold.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is needed for compatibility with Microsoft's MonthCalendar control.")]
		public System.DateTime[] MonthlyBoldedDates {
			get {
				System.Collections.Generic.List<System.DateTime> all = new System.Collections.Generic.List<System.DateTime>();
				for (int i = 0; i < this._monthlyBoldedDates.Count; i++) {
					all.Add(new System.DateTime(System.DateTime.Today.Year, System.DateTime.Today.Month, this._monthlyBoldedDates[i]));
				}
				return all.ToArray();
			}
			set {
				this.RemoveAllMonthlyBoldedDates();
				if (value != null) {
					for (int i = 0; i < value.Length; i++) {
						this.AddMonthlyBoldedDate(value[i]);
					}
				}
				this.Invalidate();
			}
		}

		/// <summary></summary>
		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		[System.ComponentModel.Browsable(false)]
		public override string Text {
			get { return base.Text; }
			set { base.Text = value; }
		}

		private System.Windows.Forms.SelectionRange _selectionRange = new System.Windows.Forms.SelectionRange(System.DateTime.Today, System.DateTime.Today);
		/// <summary>
		/// Gets or sets the selected range of dates.
		/// </summary>
		[System.ComponentModel.Category("Behaviour")]
		[System.ComponentModel.Description("Gets or sets the selected range of dates.")]
		[System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
		public System.Windows.Forms.SelectionRange SelectionRange {
			get { return this._selectionRange; }
			set {
				if (value == null) { return; }
				if (this._selectionRange.Equals(value)) { return; } //no point in painting two same dates.

				System.TimeSpan ts = value.End - value.Start;
				if (ts.Days >= this.MaxSelectionCount) {
					if (this.SelectedDate > value.Start) {
						this._selectedDate = value.Start.AddDays(this.MaxSelectionCount - 1);
						this._selectionRange = new System.Windows.Forms.SelectionRange(value.Start, this._selectedDate);
					} else if (this.SelectedDate < value.End) {
						this._selectedDate = value.End.AddDays(-(this.MaxSelectionCount - 1));
						this._selectionRange = new System.Windows.Forms.SelectionRange(this._selectedDate, value.End);
					}
				} else {
					if (this.SelectedDate < value.Start) {
						this._selectedDate = value.Start;
					} else if (this.SelectedDate > value.End) {
						this._selectedDate = value.End;
					}
					this._selectionRange = value;
				}

				this.Invalidate();
				if (this.DateChanged != null) { this.DateChanged(this, new System.Windows.Forms.DateRangeEventArgs(this.SelectionRange.Start, this.SelectionRange.End)); }
			}
		}

		private System.DateTime _selectedDate = System.DateTime.Today;
		/// <summary>
		/// Gets or sets the selected date.
		/// </summary>
		[System.ComponentModel.Category("Behaviour")]
		[System.ComponentModel.Description("Gets or sets the selected range of dates for a month calendar control.")]
		[System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
		public System.DateTime SelectedDate {
			get { return this._selectedDate; }
			set {
				this._selectedDate = value;
				this.SelectionRange = new System.Windows.Forms.SelectionRange(value, value);
			}
		}


		private bool _useFocusColor;
		/// <summary>
		/// Gets or sets whether background will be painted in different color when control has focus. If Control has ReadOnly property set, this property will be ignored.
		/// </summary>
		[System.ComponentModel.Category("Appearance")]
		[System.ComponentModel.Description("Gets or sets whether background will be painted in different color when control has focus. If Control has ReadOnly property set, this property will be ignored.")]
		[System.ComponentModel.MergableProperty(true)]
		[System.ComponentModel.DefaultValue(false)]
		public bool UseFocusColor {
			get { return this._useFocusColor; }
			set { this._useFocusColor = value; }
		}

		private System.Drawing.Color _selectionBackColor = System.Drawing.SystemColors.Highlight;
		/// <summary>
		/// Gets or sets color of selection background.
		/// </summary>
		[System.ComponentModel.Category("Appearance")]
		[System.ComponentModel.Description("Gets or sets color of selection background.")]
		[System.ComponentModel.DefaultValue(typeof(System.Drawing.Color), "Highlight")]
		public System.Drawing.Color SelectionBackColor {
			get { return this._selectionBackColor; }
			set {
				this._selectionBackColor = value;
				this.Invalidate();
			}
		}

		private System.Drawing.Color _focusColor = System.Drawing.SystemColors.Info;
		/// <summary>
		/// Gets or sets the background color when control has focus. If Control has ReadOnly property set, this property will be ignored.
		/// </summary>
		/// <value>A System.Drawing.Color that represents the background of the control.</value>
		[System.ComponentModel.Category("Appearance")]
		[System.ComponentModel.Description("Color that will be background once control has focus. If Control has ReadOnly property set, this property will be ignored.")]
		[System.ComponentModel.MergableProperty(true)]
		[System.ComponentModel.DefaultValue(typeof(System.Drawing.Color), "Info")]
		public System.Drawing.Color FocusColor {
			get { return this._focusColor; }
			set { this._focusColor = value; }
		}

		private System.Drawing.Color _selectionForeColor = System.Drawing.SystemColors.HighlightText;
		/// <summary>
		/// Gets or sets color of selection background.
		/// </summary>
		[System.ComponentModel.Category("Appearance")]
		[System.ComponentModel.Description("Gets or sets color of selection background.")]
		[System.ComponentModel.DefaultValue(typeof(System.Drawing.Color), "HighlightText")]
		public System.Drawing.Color SelectionForeColor {
			get { return this._selectionForeColor; }
			set {
				this._selectionForeColor = value;
				this.Invalidate();
			}
		}

		private System.Drawing.Color _titleBackColor = System.Drawing.SystemColors.ActiveCaption;
		/// <summary>
		/// Gets or sets color of title background.
		/// </summary>
		[System.ComponentModel.Category("Appearance")]
		[System.ComponentModel.Description("Gets or sets color of title background.")]
		[System.ComponentModel.DefaultValue(typeof(System.Drawing.Color), "ActiveCaption")]
		public System.Drawing.Color TitleBackColor {
			get { return this._titleBackColor; }
			set {
				this._titleBackColor = value;
				this.Invalidate();
			}
		}

		private System.Drawing.Color _trailingForeColor = System.Drawing.SystemColors.GrayText;
		/// <summary>
		/// Gets or sets color of title background.
		/// </summary>
		[System.ComponentModel.Category("Appearance")]
		[System.ComponentModel.Description("Gets or sets color of title background.")]
		[System.ComponentModel.DefaultValue(typeof(System.Drawing.Color), "GrayText")]
		public System.Drawing.Color TrailingForeColor {
			get { return this._trailingForeColor; }
			set {
				this._trailingForeColor = value;
				this.Invalidate();
			}
		}

		private System.Drawing.Color _titleForeColor = System.Drawing.SystemColors.ActiveCaptionText;
		/// <summary>
		/// Gets or sets color of title foreground.
		/// </summary>
		[System.ComponentModel.Category("Appearance")]
		[System.ComponentModel.Description("Gets or sets color of title foreground.")]
		[System.ComponentModel.DefaultValue(typeof(System.Drawing.Color), "ActiveCaptionText")]
		public System.Drawing.Color TitleForeColor {
			get { return this._titleForeColor; }
			set {
				this._titleForeColor = value;
				this.Invalidate();
			}
		}

		private bool _showTodayCircle = true;
		/// <summary>
		/// Gets or sets a value indicating whether today's date is circled.
		/// </summary>
		[System.ComponentModel.Category("Behaviour")]
		[System.ComponentModel.Description("Gets or sets a value indicating whether today's date is circled.")]
		[System.ComponentModel.DefaultValue(true)]
		public bool ShowTodayCircle {
			get { return this._showTodayCircle; }
			set {
				this._showTodayCircle = value;
				this.Invalidate();
			}
		}

		private bool _hoverSelection;
		/// <summary>
		/// Gets or sets a value indicating whether an item is automatically selected when the mouse pointer remains over the item for a few seconds.
		/// </summary>
		/// <returns>True if an item is automatically selected when the mouse pointer hovers over it; otherwise, false. The default is false.</returns>
		[System.ComponentModel.Category("Behavior")]
		[System.ComponentModel.Description("Gets or sets a value indicating whether an item is automatically selected when the mouse pointer remains over the item for a few seconds.")]
		[System.ComponentModel.MergableProperty(true)]
		[System.ComponentModel.DefaultValue(false)]
		public bool HoverSelection {
			get { return this._hoverSelection; }
			set { this._hoverSelection = value; }
		}

		private bool _selectNextControlOnReturn;
		/// <summary>
		/// Gets or sets a value indicating whether next control will be selected upon recieving Return key.
		/// </summary>
		/// <returns>True if next control will be selected upon recieving Return key.</returns>
		[System.ComponentModel.Category("Behavior")]
		[System.ComponentModel.Description("If true, next control will be selected upon recieving return key.")]
		[System.ComponentModel.MergableProperty(true)]
		[System.ComponentModel.DefaultValue(false)]
		public bool SelectNextControlOnReturn {
			get { return this._selectNextControlOnReturn; }
			set { this._selectNextControlOnReturn = value; }
		}

		private bool _showSelection;
		/// <summary>
		/// Gets or sets a value indicating whether focused date has focus rectangle around it.
		/// </summary>
		[System.ComponentModel.Category("Behaviour")]
		[System.ComponentModel.Description("Gets or sets a value indicating whether focused date has focus rectangle around it.")]
		[System.ComponentModel.DefaultValue(false)]
		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		[System.ComponentModel.Browsable(false)]
		public bool ShowSelection {
			get { return this._showSelection; }
			set {
				this._showSelection = value;
				this.Invalidate();
			}
		}

		private bool _showToday = true;
		/// <summary>
		/// Gets or sets a value indicating whether the current date is displayed at the bottom of the control.
		/// </summary>
		[System.ComponentModel.Category("Behaviour")]
		[System.ComponentModel.Description("Gets or sets a value indicating whether the current date is displayed at the bottom of the control.")]
		[System.ComponentModel.DefaultValue(true)]
		public bool ShowToday {
			get { return this._showToday; }
			set {
				this._showToday = value;
				this.OnResize(null);
				this.Invalidate();
			}
		}

		private int _maxSelectionCount = 1;
		/// <summary>
		/// Gets or sets the maximum number of days that can be selected in a month calendar control.
		/// </summary>
		[System.ComponentModel.Category("Behaviour")]
		[System.ComponentModel.Description("Gets or sets the maximum number of days that can be selected in a month calendar control.")]
		[System.ComponentModel.DefaultValue(1)]
		public int MaxSelectionCount {
			get { return this._maxSelectionCount; }
			set {
				if (value > 0) {
					this._maxSelectionCount = value;
				}
				this.Invalidate();
			}
		}

		#endregion


		#region Methods

		/// <summary>
		/// Adds a day that is displayed in bold on an annual basis in the month calendar.
		/// </summary>
		/// <param name="date">The date to be displayed in bold.</param>
		public void AddAnnuallyBoldedDate(System.DateTime date) {
			this._annuallyBoldedDates.Add(date.Month * 100 + date.Day);
		}

		/// <summary>
		/// Adds a day to be displayed in bold in the month calendar.
		/// </summary>
		/// <param name="date">The date to be displayed in bold.</param>
		public void AddBoldedDate(System.DateTime date) {
			this._boldedDates.Add(date.Date);
		}

		/// <summary>
		/// Adds a day that is displayed in bold on a monthly basis in the month calendar.
		/// </summary>
		/// <param name="date">The date to be displayed in bold.</param>
		public void AddMonthlyBoldedDate(System.DateTime date) {
			this._monthlyBoldedDates.Add(date.Day);
		}

		/// <summary>
		/// Removes all the annually bold dates.
		/// </summary>
		public void RemoveAllAnnuallyBoldedDates() {
			this._annuallyBoldedDates.Clear();
		}

		/// <summary>
		/// Removes all the nonrecurring bold dates.
		/// </summary>
		public void RemoveAllBoldedDates() {
			this._boldedDates.Clear();
		}

		/// <summary>
		/// Removes all the monthly bold dates.
		/// </summary>
		public void RemoveAllMonthlyBoldedDates() {
			this._monthlyBoldedDates.Clear();
		}

		/// <summary>
		/// Removes the specified date from the list of annually bold dates.
		/// </summary>
		/// <param name="date">The date to remove from the date list.</param>
		public void RemoveAnnuallyBoldedDate(System.DateTime date) {
			int value = date.Month * 100 + date.Day;
			while (this._annuallyBoldedDates.Contains(value)) {
				this._annuallyBoldedDates.Remove(value);
			}
		}

		/// <summary>
		/// Removes the specified date from the list of nonrecurring bold dates.
		/// </summary>
		/// <param name="date">The date to remove from the date list.</param>
		public void RemoveBoldedDate(System.DateTime date) {
			while (this._boldedDates.Contains(date)) {
				this._boldedDates.Remove(date.Date);
			}
		}

		/// <summary>
		/// Removes the specified date from the list of monthly bolded dates.
		/// </summary>
		/// <param name="date">The date to remove from the date list.</param>
		public void RemoveMonthlyBoldedDate(System.DateTime date) {
			int value = date.Day;
			while (this._monthlyBoldedDates.Contains(value)) {
				this._monthlyBoldedDates.Remove(value);
			}
		}

		/// <summary>
		/// Repaints the bold dates to reflect the dates set in the lists of bold dates.
		/// </summary>
		public void UpdateBoldedDates() {
			this.Invalidate();
		}

		#endregion



		#region Override

		private int _sizeHeaderHeight, _sizeDaysHeight;
		private int _size00Width, _size00Height;
		/// <summary>
		/// Raises the System.Windows.Forms.Control.Paint event.
		/// </summary>
		/// <param name="e">A System.Windows.Forms.PaintEventArgs that contains the event data.</param>
		protected override void OnPaint(System.Windows.Forms.PaintEventArgs e) {
			System.Drawing.SizeF sf00 = e.Graphics.MeasureString("00", this.Font);
			this._size00Width = (int)sf00.Width + 6;
			this._size00Height = (int)sf00.Height;

			this._sizeHeaderHeight = 4 + this._size00Height + 4;

			e.Graphics.Clear(this.BackColor);


			//Header
			using (System.Drawing.Brush brushTitleBack = new System.Drawing.SolidBrush(this.TitleBackColor))
			using (System.Drawing.Brush brushTitleFore = new System.Drawing.SolidBrush(this.TitleForeColor))
			using (System.Drawing.Font fontTitle = new System.Drawing.Font(this.Font, System.Drawing.FontStyle.Bold)) {
				e.Graphics.FillRectangle(brushTitleBack, 1, 1, this.Width - 2, this._sizeHeaderHeight);
				string text = this.SelectedDate.ToString("MMMM, yyyy", System.Globalization.CultureInfo.CurrentCulture);
				System.Drawing.SizeF sf = e.Graphics.MeasureString(text, fontTitle);
				e.Graphics.DrawString(text, fontTitle, brushTitleFore, (this.Width - sf.Width) / 2, 1 + (this._sizeHeaderHeight - sf.Height) / 2);

				sf = e.Graphics.MeasureString("<", fontTitle);
				e.Graphics.DrawString("<", fontTitle, brushTitleFore, 1 + (this._sizeHeaderHeight - sf.Height), 1 + (this._sizeHeaderHeight - sf.Height) / 2);
				sf = e.Graphics.MeasureString(">", fontTitle);
				e.Graphics.DrawString(">", fontTitle, brushTitleFore, this.Width - (this._sizeHeaderHeight - sf.Height) - sf.Width, 1 + (this._sizeHeaderHeight - sf.Height) / 2);
			}


			System.DateTime startDate = new System.DateTime(this.SelectedDate.Year, this.SelectedDate.Month, 1);
			while (startDate.DayOfWeek != System.Threading.Thread.CurrentThread.CurrentCulture.DateTimeFormat.FirstDayOfWeek) {
				startDate = startDate.AddDays(-1);
			}


			//Days
			using (System.Drawing.Brush brushDaysFore = new System.Drawing.SolidBrush(this.TitleBackColor))
			using (System.Drawing.Font fontDays = new System.Drawing.Font(this.Font.Name, this.Font.Size * 0.8F)) {
				System.DateTime date = startDate;
				for (int i = 0; i < 7; i++) {
					System.Drawing.Rectangle rect = this.GetRectangle(i, 0);
					string text = date.ToString("ddd", System.Globalization.CultureInfo.CurrentCulture);
					System.Drawing.SizeF sf = e.Graphics.MeasureString(text, fontDays);
					_sizeDaysHeight = (int)sf.Height;
					e.Graphics.DrawString(text, fontDays, brushDaysFore, rect.X + (this._size00Width - sf.Width) / 2, 1 + this._sizeHeaderHeight);
					date = date.AddDays(1);
				}
			}


			//Calendar
			using (System.Drawing.Font fontBold = new System.Drawing.Font(this.Font, System.Drawing.FontStyle.Bold))
			using (System.Drawing.Brush brushBackSel = new System.Drawing.SolidBrush(this.SelectionBackColor))
			using (System.Drawing.Brush brushForeSel = new System.Drawing.SolidBrush(this.SelectionForeColor))
			using (System.Drawing.Brush brushForeText = new System.Drawing.SolidBrush(this.ForeColor))
			using (System.Drawing.Brush brushForeTrail = new System.Drawing.SolidBrush(this.TrailingForeColor)) {
				System.DateTime currDate = startDate;
				for (int i = 0; i < 6 * 7; i++) {
					System.Drawing.Font currFont;
					if (this._boldedDates.Contains(currDate.Date)) {
						currFont = fontBold;
					} else if (this._annuallyBoldedDates.Contains(currDate.Month * 100 + currDate.Day)) {
						currFont = fontBold;
					} else if (this._monthlyBoldedDates.Contains(currDate.Day)) {
						currFont = fontBold;
					} else {
						currFont = this.Font;
					}

					System.Drawing.SizeF sf = e.Graphics.MeasureString(currDate.Day.ToString(System.Globalization.CultureInfo.CurrentCulture), currFont);

					int x = i % 7;
					int y = i / 7;
					System.Drawing.Rectangle rect = GetRectangle(x, y);
					if ((currDate >= this.SelectionRange.Start) && (currDate <= this.SelectionRange.End)) {
						e.Graphics.FillRectangle(brushBackSel, rect.X, rect.Y, rect.Width, rect.Height);
					}

					if (currDate.Month == this.SelectedDate.Month) {
						if ((currDate >= this.SelectionRange.Start) && (currDate <= this.SelectionRange.End)) {
							e.Graphics.DrawString(currDate.Day.ToString(System.Globalization.CultureInfo.CurrentCulture), currFont, brushForeSel, rect.X + (rect.Width - sf.Width) / 2, rect.Y);
						} else {
							e.Graphics.DrawString(currDate.Day.ToString(System.Globalization.CultureInfo.CurrentCulture), currFont, brushForeText, rect.X + (rect.Width - sf.Width) / 2, rect.Y);
						}
					} else {
						e.Graphics.DrawString(currDate.Day.ToString(System.Globalization.CultureInfo.CurrentCulture), currFont, brushForeTrail, rect.X + (rect.Width - sf.Width) / 2, rect.Y);
					}

					currDate = currDate.AddDays(1);
				}

				if (this.ShowTodayCircle) {
					if ((System.DateTime.Today >= startDate) && (System.DateTime.Today <= startDate.AddDays(6 * 7))) {
						int i = (System.DateTime.Today - startDate).Days;
						int x = i % 7;
						int y = i / 7;
						System.Drawing.Rectangle rect = GetRectangle(x, y);
						e.Graphics.DrawRectangle(System.Drawing.Pens.Red, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
					}
				}

				if (this.ShowSelection) {
					int i = (this.SelectedDate - startDate).Days;
					int x = i % 7;
					int y = i / 7;
					System.Windows.Forms.ControlPaint.DrawFocusRectangle(e.Graphics, GetRectangle(x, y));
				}

				if (this.ShowToday) {
					System.Drawing.Rectangle rect = GetRectangle(0, 6);
					e.Graphics.DrawRectangle(System.Drawing.Pens.Red, rect.X, rect.Y + 1, rect.Width - 1, rect.Height - 1);

					string strToday = AssemblyHelper.GetInCurrentLanguage("Today: ", "Danas: ") + System.DateTime.Today.ToShortDateString();
					System.Drawing.SizeF sf = e.Graphics.MeasureString(strToday, fontBold);
					e.Graphics.DrawString(strToday, fontBold, brushForeText, rect.X + rect.Width, rect.Y + (rect.Height - sf.Height) / 2);
				}
			}

			base.OnPaint(e);
			this.OnResize(null);
		}


		private System.DateTime _selectionBaseDate = System.DateTime.MinValue; //last date that it is set on keyboard or with mouse.
		/// <summary>
		/// Raises the System.Windows.Forms.Control.MouseDown event.
		/// </summary>
		/// <param name="e">A System.Windows.Forms.MouseEventArgs that contains the event data.</param>
		protected override void OnMouseDown(System.Windows.Forms.MouseEventArgs e) {
			this.Select();

			if (this._selectionBaseDate == System.DateTime.MinValue) { this._selectionBaseDate = this.SelectedDate; }


			if (System.Windows.Forms.Control.ModifierKeys == System.Windows.Forms.Keys.None) {

				if (e.Y <= (1 + this._sizeHeaderHeight)) {
					System.Diagnostics.Debug.WriteLine("Header.OnMouseDown");
					if (e.X < this._sizeHeaderHeight) {
						this.SelectionRange = new System.Windows.Forms.SelectionRange(this.SelectionRange.Start.AddMonths(-1), this.SelectionRange.End.AddMonths(-1));
					} else if (e.X > this.Width - this._sizeHeaderHeight) {
						this.SelectionRange = new System.Windows.Forms.SelectionRange(this.SelectionRange.Start.AddMonths(+1), this.SelectionRange.End.AddMonths(+1));
					} else { 
						//middle is clicked - month menu?
					}
				} else if (e.Y <= (1 + this._sizeHeaderHeight + this._sizeDaysHeight)) {
					System.Diagnostics.Debug.WriteLine("Days.OnMouseDown");
				} else if (e.Y > 1 + this._sizeHeaderHeight + this._sizeDaysHeight + 1 + this._size00Height * 6 + 1) {
					System.Diagnostics.Debug.WriteLine("Today.OnMouseDown");
					this.SelectionRange = new System.Windows.Forms.SelectionRange(System.DateTime.Today, System.DateTime.Today);
				} else {
					System.Diagnostics.Debug.WriteLine("Calendar.OnMouseDown");
					System.DateTime date = GetDateFromXY(e.X, e.Y);
					this._selectionBaseDate = date;
					this.SelectionRange = new System.Windows.Forms.SelectionRange(date, date);
				}

			} else if (System.Windows.Forms.Control.ModifierKeys == System.Windows.Forms.Keys.Shift) {

				System.DateTime selDate = GetDateFromXY(e.X, e.Y);
				if (selDate > System.DateTime.MinValue) { // something is clicked.
					System.Diagnostics.Debug.WriteLine("OnMouseDown.Selected: " + selDate.ToShortDateString());
					if (selDate < this._selectionBaseDate) {
						this._selectedDate = selDate;
						this.SelectionRange = new System.Windows.Forms.SelectionRange(selDate, this._selectionBaseDate);
					} else if (selDate > this._selectionBaseDate) {
						this._selectedDate = selDate;
						this.SelectionRange = new System.Windows.Forms.SelectionRange(this._selectionBaseDate, selDate);
					}
				}

			}

			base.OnMouseDown(e);
		}

		/// <summary>
		/// Raises the System.Windows.Forms.Control.MouseMove event.
		/// </summary>
		/// <param name="e">A System.Windows.Forms.MouseEventArgs that contains the event data.</param>
		protected override void OnMouseMove(System.Windows.Forms.MouseEventArgs e) {
			if (e.Button == System.Windows.Forms.MouseButtons.Left) {
				System.DateTime selDate = GetDateFromXY(e.X, e.Y);
				if (selDate > System.DateTime.MinValue) { // something is clicked.
					System.Diagnostics.Debug.WriteLine("OnMouseMove.Selected: " + selDate.ToShortDateString());
					if (selDate < this._selectionBaseDate) {
						this._selectedDate = selDate;
						this.SelectionRange = new System.Windows.Forms.SelectionRange(selDate, this._selectionBaseDate);
					} else if (selDate > this._selectionBaseDate) {
						this._selectedDate = selDate;
						this.SelectionRange = new System.Windows.Forms.SelectionRange(this._selectionBaseDate, selDate);
					}
				}
			}

			base.OnMouseMove(e);
		}

		/// <summary>
		/// Raises the System.Windows.Forms.Control.GotFocus event.
		/// </summary>
		/// <param name="e">An System.EventArgs that contains the event data.</param>
		protected override void OnEnter(System.EventArgs e) {
			this._originalBackColor = base.BackColor;
			if (this.UseFocusColor) { base.BackColor = this.FocusColor; }
			base.OnEnter(e);
		}

		/// <summary>
		/// Raises the System.Windows.Forms.Control.Leave event.
		/// </summary>
		/// <param name="e">An System.EventArgs that contains the event data.</param>
		protected override void OnLeave(System.EventArgs e) {
			base.BackColor = this._originalBackColor;
			base.OnLeave(e);
		}

		/// <summary>
		/// Processes a command key.
		/// </summary>
		/// <param name="msg">A System.Windows.Forms.Message, passed by reference, that represents the window message to process.</param>
		/// <param name="keyData">One of the System.Windows.Forms.Keys values that represents the key to process.</param>
		/// <returns>True if the character was processed by the control; otherwise, false.</returns>
		[System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.LinkDemand, Flags = System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)]
		protected override bool ProcessCmdKey(ref System.Windows.Forms.Message msg, System.Windows.Forms.Keys keyData) {
			if (this._selectionBaseDate == System.DateTime.MinValue) { this._selectionBaseDate = this.SelectedDate; }

			System.DateTime date = this.SelectedDate;
			System.DateTime dateStart = this.SelectionRange.Start;
			System.DateTime dateEnd = this.SelectionRange.End;
			switch (keyData) {
				case System.Windows.Forms.Keys.Up:
					this.SelectedDate = date.AddDays(-7);
					this._selectionBaseDate = this.SelectedDate;
					return true;

				case System.Windows.Forms.Keys.Down:
					this.SelectedDate = date.AddDays(+7);
					this._selectionBaseDate = this.SelectedDate;
					return true;

				case System.Windows.Forms.Keys.Left:
					this.SelectedDate = date.AddDays(-1);
					this._selectionBaseDate = this.SelectedDate;
					return true;

				case System.Windows.Forms.Keys.Right:
					this.SelectedDate = date.AddDays(+1);
					this._selectionBaseDate = this.SelectedDate;
					return true;

				case System.Windows.Forms.Keys.PageUp:
					this.SelectedDate = date.AddMonths(-1);
					this._selectionBaseDate = this.SelectedDate;
					return true;

				case System.Windows.Forms.Keys.PageDown:
					this.SelectedDate = date.AddMonths(+1);
					this._selectionBaseDate = this.SelectedDate;
					return true;

				case System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.PageUp:
					this.SelectedDate = date.AddYears(-1);
					this._selectionBaseDate = this.SelectedDate;
					return true;

				case System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.PageDown:
					this.SelectedDate = date.AddYears(+1);
					this._selectionBaseDate = this.SelectedDate;
					return true;

				case System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.Up:
					if (dateEnd > this._selectionBaseDate) {
						System.Windows.Forms.SelectionRange selRange = new System.Windows.Forms.SelectionRange(dateStart, dateEnd.AddDays(-7));
						if (selRange.Start < this._selectionBaseDate) {
							this._selectedDate = selRange.Start;
						} else {
							this._selectedDate = selRange.End;
						}
						this.SelectionRange = selRange;
					} else if (dateStart <= this._selectionBaseDate) {
						System.Windows.Forms.SelectionRange selRange = new System.Windows.Forms.SelectionRange(dateStart.AddDays(-7), dateEnd);
						this._selectedDate = selRange.Start;
						this.SelectionRange = selRange;
					}
					return true;

				case System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.Down:
					if (dateStart < this._selectionBaseDate) {
						System.Windows.Forms.SelectionRange selRange = new System.Windows.Forms.SelectionRange(dateStart.AddDays(+7), dateEnd);
						if (selRange.End > this._selectionBaseDate) {
							this._selectedDate = selRange.End;
						} else {
							this._selectedDate = selRange.Start;
						}
						this.SelectionRange = selRange;
					} else if (dateEnd >= this._selectionBaseDate) {
						System.Windows.Forms.SelectionRange selRange = new System.Windows.Forms.SelectionRange(dateStart, dateEnd.AddDays(+7));
						this._selectedDate = selRange.End;
						this.SelectionRange = selRange;
					}
					return true;

				case System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.Left:
					if (dateEnd > this._selectionBaseDate) {
						System.Windows.Forms.SelectionRange selRange = new System.Windows.Forms.SelectionRange(dateStart, dateEnd.AddDays(-1));
						this._selectedDate = selRange.End;
						this.SelectionRange = selRange;
					} else if (dateStart <= this._selectionBaseDate) {
						System.Windows.Forms.SelectionRange selRange = new System.Windows.Forms.SelectionRange(dateStart.AddDays(-1), dateEnd);
						this._selectedDate = selRange.Start;
						this.SelectionRange = selRange;
					}
					return true;

				case System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.Right:
					if (dateStart < this._selectionBaseDate) {
						System.Windows.Forms.SelectionRange selRange = new System.Windows.Forms.SelectionRange(dateStart.AddDays(+1), dateEnd);
						this._selectedDate = selRange.Start;
						this.SelectionRange = selRange;
					} else if (dateEnd >= this._selectionBaseDate) {
						System.Windows.Forms.SelectionRange selRange = new System.Windows.Forms.SelectionRange(dateStart, dateEnd.AddDays(+1));
						this._selectedDate = selRange.End;
						this.SelectionRange = selRange;
					}
					return true;

				case System.Windows.Forms.Keys.Return:
					if (this.SelectNextControlOnReturn) {
						if (this.Parent != null) {
							this.Parent.SelectNextControl(this, true, true, true, true);
						}
						return true;
					}
					break;

				default:
					break;
			}

			return base.ProcessCmdKey(ref msg, keyData);
		}

		/// <summary>
		/// Raises the System.Windows.Forms.Control.MouseHover event.
		/// </summary>
		/// <param name="e">An System.EventArgs that contains the event data.</param>
		protected override void OnMouseHover(System.EventArgs e) {
			if (this.HoverSelection) {
				this.Select();
			}
			base.OnMouseHover(e);
		}

		/// <summary>
		/// Raises the System.Windows.Forms.Control.Resize event.
		/// </summary>
		/// <param name="e">An System.EventArgs that contains the event data.</param>
		protected override void OnResize(System.EventArgs e) {
			if (this._size00Width > 0) {
				if (this.ShowToday) {
					this.Size = new System.Drawing.Size(1 + this._size00Width * 7 + 1, 1 + this._sizeHeaderHeight + this._sizeDaysHeight + 1 + this._size00Height * 6 + 1 + this._size00Height + 1);
				} else {
					this.Size = new System.Drawing.Size(1 + this._size00Width * 7 + 1, 1 + this._sizeHeaderHeight + this._sizeDaysHeight + 1 + this._size00Height * 6 + 1);
				}
			}
			base.OnResize(e);
		}

		#endregion


		#region Private

		private System.Drawing.Rectangle GetRectangle(int x, int y) {
			int pw = this._size00Width;
			int ph = this._size00Height;
			int px = 1 + x * pw;
			int py = 1 + this._sizeHeaderHeight + this._sizeDaysHeight + 1 + y * ph;
			return new System.Drawing.Rectangle(px, py, pw, ph);
		}

		private System.DateTime GetDateFromXY(int x, int y) {
			System.DateTime startDate = new System.DateTime(this.SelectedDate.Year, this.SelectedDate.Month, 1);
			while (startDate.DayOfWeek != System.Threading.Thread.CurrentThread.CurrentCulture.DateTimeFormat.FirstDayOfWeek) {
				startDate = startDate.AddDays(-1);
			}

			System.DateTime date = startDate;
			for (int i = 0; i < 6 * 7; i++) {
				int ix = i % 7;
				int iy = i / 7;
				System.Drawing.Rectangle rect = GetRectangle(ix, iy);
				if (rect.Contains(x, y)) {
					return date;
				}
				date = date.AddDays(1);
			}

			return System.DateTime.MinValue;

		}

		#endregion

	}

}