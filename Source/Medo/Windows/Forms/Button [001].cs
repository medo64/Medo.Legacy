/* Josip Medved <jmedved@jmedved.com> * www.medo64.com * MIT License */

//2008-04-12: New version.


using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Medo.Windows.Forms {

    /// <summary>
    /// Represents a Windows button control.
    /// </summary>
    public class Button : System.Windows.Forms.Button {

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

    }
}
