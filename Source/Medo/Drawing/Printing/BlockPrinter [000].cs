//Josip Medved <jmedved@jmedved.com> http://www.jmedved.com

//2008-11-07: Initial version.


using System;

namespace Medo.Drawing.Printing {

	/// <summary>
	/// Class for making print document with all it's output within blocks.
	/// </summary>
	public class BlockPrinter : System.IDisposable {

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
			}
		}

		#endregion

	}

}
