//Josip Medved <jmedved@jmedved.com>   www.medo64.com

//2010-05-28: Initial version.


using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Medo.Windows {

    /// <summary>
    /// Simple about form.
    /// </summary>
    public sealed class InputWindow : IDisposable {

        /// <summary>
        /// Create new instance.
        /// </summary>
        /// <param name="labelText">Label text.</param>
        public InputWindow(string labelText) : this(labelText, null, null) { }

        /// <summary>
        /// Create new instance.
        /// </summary>
        /// <param name="labelText">Label text.</param>
        /// <param name="defaultText">Default text.</param>
        public InputWindow(string labelText, string defaultText) : this(labelText, defaultText, null) { }

        /// <summary>
        /// Create new instance.
        /// </summary>
        /// <param name="labelText">Label text.</param>
        /// <param name="defaultText">Default text.</param>
        /// <param name="hintText">Hint text.</param>
        public InputWindow(string labelText, string defaultText, string hintText) {
            this.LabelText = labelText;
            this.DefaultText = defaultText;
            this.HintText = hintText;
        }


        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        public void Dispose() { }


        /// <summary>
        /// Gets label text.
        /// </summary>
        public string LabelText { get; private set; }

        /// <summary>
        /// Gets default text.
        /// </summary>
        public string DefaultText { get; private set; }

        /// <summary>
        /// Gets hint text.
        /// </summary>
        public string HintText { get; private set; }

        /// <summary>
        /// Gets selected text.
        /// </summary>
        public string SelectedText { get; private set; }

        private TextBox _textBox;
        private Button _buttonOk;
        private Window _window;


        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "All those classes are needed to manually create WPF window.")]
        /// <summary>
        /// Opens a window and returns only when the newly opened window is closed.
        /// </summary>
        /// <param name="owner">Window that owns this window.</param>
        public bool? ShowDialog(Window owner) {
            var inputStack = new StackPanel() { Margin = new Thickness(11), Orientation = Orientation.Horizontal };

            var labelUser = new Label() { Content = this.LabelText, VerticalAlignment = VerticalAlignment.Center };
            Grid.SetRow(labelUser, 0);
            Grid.SetColumn(labelUser, 0);
            inputStack.Children.Add(labelUser);

            this._textBox = new TextBox() { VerticalAlignment = VerticalAlignment.Center, ToolTip = this.HintText, Width = 250 };
            inputStack.Children.Add(this._textBox);

            this._buttonOk = new Button() { Content = Localizations.Ok, Width = 75, Height = 23, Margin = new Thickness(0, 0, 7, 0), IsDefault = true };
            var buttonCancel = new Button() { Content = Localizations.Cancel, Width = 75, Height = 23, IsCancel = true };

            var buttonsStack = new StackPanel() { Margin = new Thickness(11), Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            buttonsStack.Children.Add(this._buttonOk);
            buttonsStack.Children.Add(buttonCancel);

            var windowStack = new StackPanel();
            windowStack.Children.Add(new Border() { Child = inputStack, Background = SystemColors.WindowBrush, BorderThickness = new Thickness(0) });
            windowStack.Children.Add(buttonsStack);

            this._window = new Window() { FontFamily = SystemFonts.MessageFontFamily, FontSize = SystemFonts.MessageFontSize, FontWeight = SystemFonts.MessageFontWeight, MinWidth = 200, Background = SystemColors.ControlBrush };

            if (owner != null) {
                this._window.Icon = owner.Icon;
                this._window.Owner = owner;
                this._window.ShowInTaskbar = false;
                if (owner.Topmost) { this._window.Topmost = true; }
                this._window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            } else {
                this._window.Icon = GetAppIcon(Assembly.GetEntryAssembly());
                this._window.Owner = null;
                this._window.ShowInTaskbar = true;
                this._window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
            this._window.ResizeMode = ResizeMode.NoResize;
            this._window.ShowActivated = true;
            this._window.SizeToContent = SizeToContent.WidthAndHeight;
            this._window.Title = Localizations.Title;
            this._window.Content = windowStack;

            this._buttonOk.Click += new RoutedEventHandler(buttonOk_Click);
            this._window.Loaded += new RoutedEventHandler(window_Loaded);

            return this._window.ShowDialog();
        }

        private void window_Loaded(object sender, RoutedEventArgs e) {
            this._textBox.Text = this.DefaultText;
            this._textBox.SelectAll();
            this._textBox.Focus();
        }

        private void buttonOk_Click(object sender, RoutedEventArgs e) {
            ButtonOkClick();
        }

        private void ButtonOkClick() {
            this.SelectedText = this._textBox.Text;
            this._window.DialogResult = true;
        }


        private static ImageSource GetAppIcon(Assembly assembly) {
            var hLibrary = NativeMethods.LoadLibrary(assembly.Location);
            if (hLibrary != IntPtr.Zero) {
                var hIcon = NativeMethods.LoadIcon(hLibrary, "#32512");
                if (hIcon != IntPtr.Zero) {
                    return Imaging.CreateBitmapSourceFromHIcon(hIcon, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                }
            }
            return null;
        }

        private static class NativeMethods {

            [DllImport("user32.dll", CharSet = CharSet.Unicode)]
            static extern internal IntPtr LoadIcon(IntPtr hInstance, string lpIconName);

            [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
            static extern internal IntPtr LoadLibrary(string lpFileName);

        }

        private static class Localizations {

            internal static string Title {
                get {
                    if (Thread.CurrentThread.CurrentUICulture.Name.StartsWith("hr-", StringComparison.OrdinalIgnoreCase)) {
                        return "Unesite tekst";
                    } else {
                        return "Input text";
                    }
                }
            }

            internal static string Ok {
                get {
                    if (Thread.CurrentThread.CurrentUICulture.Name.StartsWith("hr-", StringComparison.OrdinalIgnoreCase)) {
                        return "U redu";
                    } else {
                        return "OK";
                    }
                }
            }

            internal static string Cancel {
                get {
                    if (Thread.CurrentThread.CurrentUICulture.Name.StartsWith("hr-", StringComparison.OrdinalIgnoreCase)) {
                        return "Odustani";
                    } else {
                        return "Cancel";
                    }
                }
            }

        }

    }

}
