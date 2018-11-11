/* Josip Medved <jmedved@jmedved.com> * www.medo64.com * MIT License */

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
    /// Represents a Windows control that allows the user to select a date and a time and to display the date and time with a specified format.
    /// </summary>
    public class DateTimePicker : System.Windows.Forms.DateTimePicker {

        /// <summary>
        /// Gets/sets whether next control will be selected on Enter.
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
            get { return _focusedBackColor; }
            set { _focusedBackColor = value; }
        }

        private Color _focusedForeColor = SystemColors.InfoText;
        /// <summary>
        /// The foreground color when control has focus.
        /// </summary>
        [System.ComponentModel.Category("Appearance")]
        [System.ComponentModel.DefaultValue("InfoText")]
        public Color FocusedForeColor {
            get { return _focusedForeColor; }
            set { _focusedForeColor = value; }
        }


        private Color _lastBackColor;
        private Color _lastForeColor;


        /// <summary>
        /// Raises the Enter event.
        /// </summary>
        /// <param name="e">An System.EventArgs that contains the event data.</param>
        protected override void OnEnter(EventArgs e) {
            _lastBackColor = BackColor;
            _lastForeColor = ForeColor;
            if (UseFocusColor) {
                BackColor = FocusedBackColor;
                ForeColor = FocusedForeColor;
            }
            base.OnEnter(e);
        }

        /// <summary>
        /// Raises the Leave event.
        /// </summary>
        /// <param name="e">An System.EventArgs that contains the event data.</param>
        protected override void OnLeave(EventArgs e) {
            BackColor = _lastBackColor;
            ForeColor = _lastForeColor;
            base.OnLeave(e);
        }

        /// <summary>
        /// Processes a command key.
        /// </summary>
        /// <param name="msg">A System.Windows.Forms.Message, passed by reference, that represents the Win32 message to process.</param>
        /// <param name="keyData">One of the System.Windows.Forms.Keys values that represents the key to process.</param>
        protected override bool ProcessCmdKey(ref System.Windows.Forms.Message msg, System.Windows.Forms.Keys keyData) {
            if ((SelectNextControlOnReturn) && (keyData == System.Windows.Forms.Keys.Enter)) {
                if (Parent != null) {
                    Parent.SelectNextControl(this, true, true, true, true);
                    return true;
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }


        /// <summary>
        /// Processes Windows messages.
        /// </summary>
        /// <param name="m">The Windows System.Windows.Forms.Message to process.</param>
        protected override void WndProc(ref Message m) {
            if (m.Msg == NativeMethods.WM_ERASEBKGND) {
                using (Graphics g = Graphics.FromHdc(m.WParam)) {
                    g.FillRectangle(new SolidBrush(BackColor), ClientRectangle);
                }
                return;
            }
            base.WndProc(ref m);
        }

        /// <summary>
        /// Gets or sets the background color for the control.
        /// </summary>
        [System.ComponentModel.Category("Appearance")]
        [System.ComponentModel.DefaultValue("Window")]
        public override Color BackColor {
            get { return base.BackColor; }
            set {
                base.BackColor = value;
                Invalidate();
            }
        }


        private static class NativeMethods {
#pragma warning disable IDE0049 // Simplify Names

            internal const Int32 WM_ERASEBKGND = 0x14;

#pragma warning restore IDE0049 // Simplify Names
        }

    }
}
