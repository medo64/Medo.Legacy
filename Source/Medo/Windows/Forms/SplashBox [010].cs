//Copyright (c) 2007 Josip Medved <jmedved@jmedved.com>

//2007-12-22: New version. 
//2007-12-27: Fixed error when application is without icon
//2008-01-02: This thread now uses own message loop.
//2008-01-26: Fixed bugs with disposing non-existing icon.
//2008-02-04: Refactored in order to prevent race conditions.
//2008-02-09: Refactored in order to prevent race conditions.
//2008-02-15: Removed threading code.
//2008-04-11: Cleaned code to match FxCop 1.36 beta 2 (SpecifyMarshalingForPInvokeStringArguments).
//2008-05-11: Fixed exception when there is no project's icon.
//2008-08-10: Form is not topmost any more.


using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Medo.Windows.Forms {

	/// <summary>
	/// Startup splash form handling.
	/// </summary>
	public static class SplashBox {

		private static readonly object _syncRoot = new object();
		private static Form _form;
		private static Bitmap _paintBitmap;


		/// <summary> 
		/// Shows splash screen. 
		/// </summary> 
		public static void Show() {
			lock (_syncRoot) {
				if (_form != null) { return; }

				System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.AppStarting;

				_form = new Form();

				_form.Cursor = Cursors.AppStarting;
				_form.FormBorderStyle = FormBorderStyle.FixedDialog;
				_form.ControlBox = false;
				_form.ShowIcon = false;
				_form.ShowInTaskbar = false;
				_form.MinimizeBox = false;
				_form.MaximizeBox = false;
				_form.AutoSize = false;
				_form.AutoScaleMode = AutoScaleMode.None;

				using (Bitmap tmpBitmap = new Bitmap(1, 1))
				using (Graphics tmpGraphics = Graphics.FromImage(tmpBitmap))
				using (Font font = new System.Drawing.Font(System.Drawing.SystemFonts.DefaultFont.Name, 32, System.Drawing.SystemFonts.DefaultFont.Style, System.Drawing.GraphicsUnit.Pixel, System.Drawing.SystemFonts.DefaultFont.GdiCharSet)) {
					Icon icon = null;
					try {
						//icon
						System.IntPtr hLibrary = NativeMethods.LoadLibrary(System.Reflection.Assembly.GetEntryAssembly().Location);
						if (!hLibrary.Equals(System.IntPtr.Zero)) {
							System.IntPtr hIcon = NativeMethods.LoadIcon(hLibrary, "#32512");
							if (!hIcon.Equals(System.IntPtr.Zero)) {
								icon = System.Drawing.Icon.FromHandle(hIcon);
							}
						}

						//text
						System.Reflection.Assembly assembly = System.Reflection.Assembly.GetEntryAssembly();
						object[] titleAttributes = assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyTitleAttribute), true);
						string text;
						if ((titleAttributes != null) && (titleAttributes.Length >= 1)) {
							text = ((System.Reflection.AssemblyTitleAttribute)titleAttributes[titleAttributes.Length - 1]).Title;
						} else {
							text = assembly.GetName().Name;
						}
						Size szText = tmpGraphics.MeasureString(text, font).ToSize();

						if (icon != null) {
							_paintBitmap = new Bitmap(7 + 32 + 7 + szText.Width + 7, 7 + 32 + 7);
						} else {
							_paintBitmap = new Bitmap(7 + szText.Width + 7, 7 + 32 + 7);
						}
						using (Graphics graphics = Graphics.FromImage(_paintBitmap)) {
							//graphics.Clear(_form.BackColor);
							graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
							if (icon != null) {
                                graphics.DrawIcon(icon, new Rectangle(7, 7, 32, 32));
                                graphics.DrawString(text, font, SystemBrushes.ControlText, 7 + 32 + 7, 7 + (32 - szText.Height) / 2);
							} else {
								graphics.DrawString(text, font, SystemBrushes.ControlText, 7, 7 + (32 - szText.Height) / 2);
							}
							graphics.Flush();
						}

					} finally {
						if (icon != null) { icon.Dispose(); }
					}

				}

				_form.Handle.ToInt32(); //to create whole form

				int borderX = (_form.Width - _form.ClientRectangle.Width);
				int borderY = (_form.Height - _form.ClientRectangle.Height);
				_form.Size = new Size(borderX + _paintBitmap.Width, borderY + _paintBitmap.Height);
				_form.Location = new Point((System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width - _form.Width) / 2, (System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height - _form.Height) / 2);

				_form.Paint += Form_Paint;

				_form.TopMost = false;
                _form.TopMost = true;
                _form.TopMost = false;
                _form.Show();
				_form.Refresh();
			}
		}

		/// <summary> 
		/// Shows splash screen. 
		/// </summary> 
		/// <param name="autoClose">It true, form is closed when Idle is detected.</param> 
		[Obsolete("This method was obsoleted. Please use either Show() or ShowUntilIdle() instead.")]
		public static void Show(bool autoClose) {
			lock (_syncRoot) {
				if (autoClose) {
					ShowUntilIdle();
				} else {
					Show();
				}
			}
		}

		/// <summary> 
		/// Shows splash screen until Application.Idle event fires.
		/// </summary>
		public static void ShowUntilIdle() {
			lock (_syncRoot) {
				System.Windows.Forms.Application.Idle += Application_Idle;
				Show();
			}
		}

		/// <summary> 
		/// Hides splash screen. 
		/// </summary> 
		public static void Hide() {
			lock (_syncRoot) {
				if (_form == null) { return; }

				System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;

				if (_form != null) {
					_form.Close();
					_form = null;
				}
				if (_paintBitmap != null) {
					_paintBitmap.Dispose();
					_paintBitmap = null;
				}
			}
		}


		private static void Application_Idle(object sender, System.EventArgs e) {
			lock (_syncRoot) {
				System.Windows.Forms.Application.Idle -= Application_Idle;
				Hide();
			}
		}


		private static void Form_Paint(object sender, System.Windows.Forms.PaintEventArgs e) {
			lock (_syncRoot) {
				if (_paintBitmap != null) { e.Graphics.DrawImageUnscaled(_paintBitmap, 0, 0); }
			}
		}


		private static class NativeMethods {

			[DllImport("user32.dll", CharSet = CharSet.Unicode)]
			static extern internal System.IntPtr LoadIcon(System.IntPtr hInstance, string lpIconName);

			[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
			static extern internal System.IntPtr LoadLibrary(string lpFileName);

		}

	}

}
