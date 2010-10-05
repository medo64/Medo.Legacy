using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Medo.Windows.Forms {

	public class QComboBox : System.Windows.Forms.ComboBox {

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
			this.SelectAll();
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

	}
}
