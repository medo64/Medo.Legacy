//Josip Medved <jmedved@jmedved.com> http://www.jmedved.com

//2010-05-19: Initial version.
//2010-05-27: Design changes.


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
    public sealed class LogOnWindow : IDisposable {

        /// <summary>
        /// Create new instance.
        /// </summary>
        public LogOnWindow() : this(null, null) { }

        /// <summary>
        /// Create new instance.
        /// </summary>
        /// <param name="users">Predefined users.</param>
        public LogOnWindow(IEnumerable<IIdentity> users) : this(users, null) { }

        /// <summary>
        /// Create new instance.
        /// </summary>
        /// <param name="users">Predefined users.</param>
        /// <param name="selectedUserName">Pre-selected user name.</param>
        public LogOnWindow(IEnumerable<IIdentity> users, string selectedUserName) {
            this.Users = users;
            this.SelectedUserName = selectedUserName;
        }


        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        public void Dispose() { }


        /// <summary>
        /// Gets/sets list of predefined users.
        /// </summary>
        public IEnumerable<IIdentity> Users { get; private set; }

        /// <summary>
        /// Gets/sets selected user.
        /// </summary>
        public object SelectedUser { get; private set; }

        /// <summary>
        /// Gets/sets user name.
        /// </summary>
        public string SelectedUserName { get; private set; }

        /// <summary>
        /// Gets/sets password.
        /// </summary>
        public string SelectedPassword { get; private set; }


        private ComboBox _comboUser;
        private PasswordBox _textPassword;
        private Button _buttonLogOn;
        private Window _window;


        /// <summary>
        /// Opens a window and returns only when the newly opened window is closed.
        /// </summary>
        /// <param name="owner">Window that owns this window.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "All those classes are needed to manually create WPF window.")]
        public bool? ShowDialog(Window owner) {
            var contentGrid = new Grid() { Margin = new Thickness(11) };
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition() { });
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(7) });
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(250) });
            contentGrid.RowDefinitions.Add(new RowDefinition() { });
            contentGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(7) });
            contentGrid.RowDefinitions.Add(new RowDefinition() { });

            var labelUser = new Label() { Content = Localizations.UserName, VerticalAlignment = VerticalAlignment.Center };
            Grid.SetRow(labelUser, 0);
            Grid.SetColumn(labelUser, 0);
            contentGrid.Children.Add(labelUser);

            this._comboUser = new ComboBox() { VerticalAlignment = VerticalAlignment.Center, IsEditable = true };
            foreach (var user in this.Users) {
                this._comboUser.Items.Add(user);
            }
            Grid.SetRow(this._comboUser, 0);
            Grid.SetColumn(this._comboUser, 2);
            contentGrid.Children.Add(this._comboUser);

            var labelPassword = new Label() { Content = Localizations.Password, VerticalAlignment = VerticalAlignment.Center };
            Grid.SetRow(labelPassword, 2);
            Grid.SetColumn(labelPassword, 0);
            contentGrid.Children.Add(labelPassword);
            this._textPassword = new PasswordBox() { VerticalAlignment = VerticalAlignment.Center };
            Grid.SetRow(this._textPassword, 2);
            Grid.SetColumn(this._textPassword, 2);
            contentGrid.Children.Add(this._textPassword);

            this._buttonLogOn = new Button() { Content = "Log on", Width = 75, Height = 23, Margin = new Thickness(0, 0, 7, 0) };
            var buttonCancel = new Button() { Content = "Cancel", Width = 75, Height = 23, IsCancel = true };

            var buttonsStack = new StackPanel() { Margin = new Thickness(11), Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            buttonsStack.Children.Add(this._buttonLogOn);
            buttonsStack.Children.Add(buttonCancel);

            var windowStack = new StackPanel();
            windowStack.Children.Add(new Border() { Child = contentGrid, Background = SystemColors.WindowBrush, BorderThickness = new Thickness(0) });
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
            this._window.Title = "Log on";
            this._window.Content = windowStack;

            this._comboUser.KeyDown += new KeyEventHandler(_comboUser_KeyDown);
            this._textPassword.KeyDown += new KeyEventHandler(_textPassword_KeyDown);
            this._buttonLogOn.Click += new RoutedEventHandler(_buttonLogOn_Click);
            this._window.Loaded += new RoutedEventHandler(window_Loaded);

            return this._window.ShowDialog();
        }

        private void window_Loaded(object sender, RoutedEventArgs e) {
            var isUserSelected = false;
            foreach (var item in this._comboUser.Items) {
                if (item.Equals(this.SelectedUserName)) {
                    this._comboUser.SelectedItem = item;
                    isUserSelected = true;
                    break;
                }
            }
            if (isUserSelected) {
                this._textPassword.Focus();
            } else {
                this._comboUser.Focus();
            }
        }

        private void _comboUser_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                this._textPassword.Focus();
                e.Handled = true;
            }
        }

        private void _textPassword_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                LogonClick();
                e.Handled = true;
            }
        }

        private void _buttonLogOn_Click(object sender, RoutedEventArgs e) {
            LogonClick();
        }

        private void LogonClick() {
            this.SelectedUserName = this._comboUser.Text;
            this.SelectedPassword = this._textPassword.Password;

            if (this._comboUser.SelectedItem != null) {
                foreach (IIdentity item in this._comboUser.Items) {
                    if (item.Equals(this._comboUser.SelectedItem)) {
                        this.SelectedUserName = item.Name;
                        this.SelectedUser = item;
                        break;
                    }
                }
            } else {
                this.SelectedUser = null;
            }

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

            internal static string UserName {
                get {
                    if (Thread.CurrentThread.CurrentUICulture.Name.StartsWith("hr-", StringComparison.OrdinalIgnoreCase)) {
                        return "Korisnièko ime:";
                    } else {
                        return "User name:";
                    }
                }
            }

            internal static string Password {
                get {
                    if (Thread.CurrentThread.CurrentUICulture.Name.StartsWith("hr-", StringComparison.OrdinalIgnoreCase)) {
                        return "Lozinka:";
                    } else {
                        return "Password:";
                    }
                }
            }

        }

    }

}
