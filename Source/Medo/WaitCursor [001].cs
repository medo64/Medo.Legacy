//Josip Medved <jmedved@jmedved.com>   www.medo64.com

//2007-10-30: New version.


namespace Medo {

	/// <summary>
	/// Class that shows Wait cursor until disposed. If dispose is not called, behaviour is not defined.
	/// This class is thread-safe.
	/// </summary>
	/// <example>
	/// using(WaitCursor w = new WaitCursor()) {
	///   //some action
	/// }
	/// </example>
	public class WaitCursor : System.IDisposable {

		private static System.Windows.Forms.Cursor _oldCursor;
		private readonly static object _syncRoot = new object();


		/// <summary>
		/// Creates new instance and changes current cursor to WaitCursor.
		/// </summary>
		public WaitCursor() {
			lock (_syncRoot) {
				_oldCursor = System.Windows.Forms.Cursor.Current;
				System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;
			}
		}

		/// <summary>
		/// Destroys instance and changes current cursor to previous value.
		/// </summary>
		~WaitCursor() {
			this.Dispose(false);
		}


		#region IDisposable Members

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		public void Dispose() {
			this.Dispose(true);
			System.GC.SuppressFinalize(this);
		}

		private static bool _isDisposed;
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">True if managed resources should be disposed; otherwise, false.</param>
		protected virtual void Dispose(bool disposing) {
			lock (_syncRoot) {
				if (!_isDisposed) {
					System.Windows.Forms.Cursor.Current = _oldCursor;
					_isDisposed = true;
				}
			}
		}

		#endregion

	}

}
