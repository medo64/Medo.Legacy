//Josip Medved <jmedved@jmedved.com> http://www.jmedved.com

//2007-01-04: New version.


using System.Drawing;
using System.Windows.Forms;

namespace Medo.Windows.Forms {

	/// <summary>
	/// Handling of wait box.
	/// </summary>
	public class WaitBox : System.IDisposable {

		private System.Threading.Thread _thread;
		private System.Threading.ManualResetEvent _hideEvent;
		private readonly object _syncRoot = new object();
		private readonly object _syncForm = new object();
		private Form _form;
		private System.Windows.Forms.IWin32Window _owner;
		private string _text;
		private int _progress = -1;
		private Label _label;
		private ProgressBar _progressBar;


		/// <summary>
		/// Creates new instance.
		/// </summary>
		/// <param name="owner">Any object that implements System.Windows.Forms.IWin32Window that represents the top-level window that will own the modal dialog box.</param>
		/// <param name="text">Text to show.</param>
		public WaitBox(IWin32Window owner, string text)
			: this(owner, text, -1) {
		}

		/// <summary>
		/// Creates new instance.
		/// </summary>
		/// <param name="owner">Any object that implements System.Windows.Forms.IWin32Window that represents the top-level window that will own the modal dialog box.</param>
		/// <param name="text">Text to show.</param>
		/// <param name="progress">Initial progress.</param>
		public WaitBox(IWin32Window owner, string text, int progress) {
			lock (_syncRoot) {
				System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;

				_hideEvent = new System.Threading.ManualResetEvent(false);

				_form = new Form();
				_owner = owner;
				_text = text;
				_progress = progress;

				_thread = new System.Threading.Thread(Run);
				_thread.Name = "Medo.Windows.Forms.WaitBox.0";
				_thread.IsBackground = true;
				_thread.SetApartmentState(System.Threading.ApartmentState.STA);
				_thread.Start();
			}
		}


		/// <summary>
		/// Gets/sets text displayed on form.
		/// </summary>
		public string Text {
			get {
				lock (_syncRoot) {
					return _text;
				}
			}
			set {
				lock (_syncRoot) {
					_text = value;
					if (_form != null) {
						if (_form.IsHandleCreated) {
							UpdateTextDelegate methodUpdate = new UpdateTextDelegate(UpdateText);
							_form.Invoke(methodUpdate);
						}
					}
				}
			}
		}

		/// <summary>
		/// Gets/sets progress displayed on form.
		/// </summary>
		public int Progress {
			get {
				lock (_syncRoot) {
					return _progress;
				}
			}
			set {
				lock (_syncRoot) {
					_progress = value;
					if (_form != null) {
						if (_form.IsHandleCreated) {
							UpdateProgressDelegate methodUpdate = new UpdateProgressDelegate(UpdateProgress);
							_form.Invoke(methodUpdate);
						}
					}
				}
			}
		}

		private void Run() {
			lock (_syncRoot) {
				if (_form == null) { return; }
				_form.Cursor = Cursors.WaitCursor;
				_form.FormBorderStyle = FormBorderStyle.FixedDialog;
				_form.ControlBox = false;
				_form.ShowIcon = false;
				_form.ShowInTaskbar = false;
				_form.MinimizeBox = false;
				_form.MaximizeBox = false;
				_form.AutoSize = false;
				_form.AutoScaleMode = AutoScaleMode.None;

				int borderX = (_form.Width - _form.ClientRectangle.Width);
				int borderY = (_form.Height - _form.ClientRectangle.Height);
				_form.Size = new Size(borderX + 7 + 160 + 7, borderY + 7 + 90 + 7);

				_progressBar = new ProgressBar();
				_progressBar.Location = new Point(7, _form.ClientRectangle.Bottom - _progressBar.Height - 7);
				_progressBar.Size = new Size(_form.ClientRectangle.Width - 7 - 7, _progressBar.Size.Height);
				_form.Controls.Add(_progressBar);
				UpdateProgress();

				_label = new Label();
				_label.AutoSize = false;
				_label.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
				_label.Location = new Point(7, 7);
				_label.Size = new Size(_form.ClientRectangle.Width - 7 - 7, _form.ClientRectangle.Height - 7 - 7 - 11 - _progressBar.Size.Height);
				_form.Controls.Add(_label);
				UpdateText();

				if (_owner != null) {
					Form formOwner = _owner as Form;
					if ((formOwner != null) && (formOwner.TopMost == true)) {
						//HACK: Because of bug in .NET.
						_form.TopMost = false;
						_form.TopMost = true;
					}
					_form.StartPosition = FormStartPosition.CenterParent;
					_form.Location = new Point((System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width - _form.Width) / 2, (System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height - _form.Height) / 2);
					_form.Show(_owner);
				} else {
					_form.StartPosition = FormStartPosition.CenterScreen;
					_form.Show();
				}
			}
			System.Windows.Forms.Application.Run(_form);
		}


		private delegate void CloseFormDelegate(Form form);

		private void CloseForm(Form form) {
			lock (_syncForm) {
				form.Dispose();
				_progressBar.Dispose();
				_progressBar = null;
				_label.Dispose();
				_label = null;
			}
		}


		private delegate void UpdateTextDelegate();

		private void UpdateText() {
			lock (_syncForm) {
				if (_label != null) {
					_label.Text = _text;
				}
			}
		}

		private delegate void UpdateProgressDelegate();

		private void UpdateProgress() {
			lock (_syncForm) {
				if (_progressBar != null) {
					if ((_progress < 0) || (_progress > 100)) {
						_progressBar.Value = 0;
						_progressBar.Style = ProgressBarStyle.Marquee;
					} else {
						_progressBar.Value = _progress;
						_progressBar.Style = ProgressBarStyle.Continuous;
					}
				}
			}
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
				lock (_syncRoot) {
					System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;

					_hideEvent.Set();
					_hideEvent.Close();

					if (_form != null) {
						if (_form.IsHandleCreated) {
							CloseFormDelegate methodCloseForm = new CloseFormDelegate(CloseForm);
							_form.Invoke(methodCloseForm, _form);
						}
						if (!_form.IsDisposed) {
							_form.Dispose();
						}
						_form = null;
					}
					if (_label != null) {
						_label.Dispose();
						_label = null;
					}
					if (_progressBar != null) {
						_progressBar.Dispose();
						_progressBar = null;
					}
				}
			}
		}

		#endregion
	}

}