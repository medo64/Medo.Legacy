using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Medo.Windows.Forms {

	public class QDateTimePicker : System.Windows.Forms.DateTimePicker {

		private bool _selectNextControlOnEnter = true;
		private Color _focusedBackColor = SystemColors.Info;
		private Color _focusedForeColor = SystemColors.InfoText;


		private Color _lastBackColor;
		private Color _lastForeColor;


		protected override void OnEnter(EventArgs e) {
			this._lastBackColor = this.BackColor;
			this._lastForeColor = this.ForeColor;
			this.BackColor = this._focusedBackColor;
			this.ForeColor = this._focusedForeColor;
			base.OnEnter(e);
		}

		protected override void OnLeave(EventArgs e) {
			this.BackColor = this._lastBackColor;
			this.ForeColor = this._lastForeColor;
			base.OnLeave(e);
		}

		protected override bool ProcessCmdKey(ref System.Windows.Forms.Message msg, System.Windows.Forms.Keys keyData) {
			if ((this._selectNextControlOnEnter) && (keyData == System.Windows.Forms.Keys.Enter)) {
				if (this.Parent != null) {
					this.Parent.SelectNextControl(this, true, true, true, true);
					return true;
				}
			}
			return base.ProcessCmdKey(ref msg, keyData);
		}

		protected override void WndProc(ref Message m) {
			System.Diagnostics.Debug.WriteLine(m.ToString());
			if (m.Msg == NativeMethods.WM_ERASEBKGND) {
				using (Graphics g = Graphics.FromHdc(m.WParam)) {
					g.FillRectangle(new SolidBrush(this.BackColor), this.ClientRectangle);
				}
				return;
			}
			base.WndProc(ref m);
		}


		public override Color BackColor {
			get { return base.BackColor; }
			set {
				base.BackColor = value;
				this.Invalidate();
			}
		}


		private class NativeMethods {

			internal const int WM_ERASEBKGND= 0x14;

		}

	}
}
