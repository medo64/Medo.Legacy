//Josip Medved <jmedved@jmedved.com>   www.medo64.com

//2012-11-24: Removing link demands.
//2008-04-12: New version.


using System;
using System.Collections.Generic;
using System.Drawing;
using System.Security.Permissions;
using System.Text;
using System.Windows.Forms;

namespace Medo.Windows.Forms {

    /// <summary>
    /// Represents a Windows text box control.
    /// </summary>
    public class TextBox : System.Windows.Forms.TextBox {

        /// <summary>
        /// Gets/sets whether next control will be selected on Enter.
        /// Only valid if control is not multiline.
        /// </summary>
        [System.ComponentModel.Category("Behavior")]
        [System.ComponentModel.DefaultValue(false)]
        public bool SelectNextControlOnReturn { get; set; }

        /// <summary>
        /// Gets/sets whether color will change when control is focused.
        /// </summary>
        [System.ComponentModel.Category("Appearance")]
        [System.ComponentModel.DefaultValue(false)]
        public bool UseFocusColor { get; set; }

        private Color _focusedBackColor = SystemColors.Info;
        /// <summary>
        /// The background color when control has focus.
        /// </summary>
        [System.ComponentModel.Category("Appearance")]
        [System.ComponentModel.DefaultValue("Info")]
        public Color FocusedBackColor {
            get { return this._focusedBackColor; }
            set { this._focusedBackColor = value; }
        }

        private Color _focusedForeColor = SystemColors.InfoText;
        /// <summary>
        /// The foreground color when control has focus.
        /// </summary>
        [System.ComponentModel.Category("Appearance")]
        [System.ComponentModel.DefaultValue("InfoText")]
        public Color FocusedForeColor {
            get { return this._focusedForeColor; }
            set { this._focusedForeColor = value; }
        }


        private Color _lastBackColor;
        private Color _lastForeColor;


        /// <summary>
        /// Raises the Enter event.
        /// </summary>
        /// <param name="e">An System.EventArgs that contains the event data.</param>
        protected override void OnEnter(EventArgs e) {
            this._lastBackColor = this.BackColor;
            this._lastForeColor = this.ForeColor;
            if (this.UseFocusColor) {
                this.BackColor = this.FocusedBackColor;
                this.ForeColor = this.FocusedForeColor;
            }
            this.SelectAll();
            base.OnEnter(e);
        }

        /// <summary>
        /// Raises the Leave event.
        /// </summary>
        /// <param name="e">An System.EventArgs that contains the event data.</param>
        protected override void OnLeave(EventArgs e) {
            this.BackColor = this._lastBackColor;
            this.ForeColor = this._lastForeColor;
            base.OnLeave(e);
        }

        /// <summary>
        /// Processes a command key.
        /// </summary>
        /// <param name="msg">A System.Windows.Forms.Message, passed by reference, that represents the Win32 message to process.</param>
        /// <param name="keyData">One of the System.Windows.Forms.Keys values that represents the key to process.</param>
        protected override bool ProcessCmdKey(ref System.Windows.Forms.Message msg, System.Windows.Forms.Keys keyData) {
            if ((this.SelectNextControlOnReturn) && (!this.Multiline) && (keyData == System.Windows.Forms.Keys.Enter)) {
                if (this.Parent != null) {
                    this.Parent.SelectNextControl(this, true, true, true, true);
                    return true;
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

    }
}
