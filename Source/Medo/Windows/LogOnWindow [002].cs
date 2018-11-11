/* Josip Medved <jmedved@jmedved.com> * www.medo64.com * MIT License */

//2010-05-27: Design changes.
//2010-05-19: Initial version.


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
            Users = users;
            SelectedUserName = selectedUserName;
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

            _comboUser = new ComboBox() { VerticalAlignment = VerticalAlignment.Center, IsEditable = true };
            foreach (var user in Users) {
                _comboUser.Items.Add(user);
            }
            Grid.SetRow(_comboUser, 0);
            Grid.SetColumn(_comboUser, 2);
            contentGrid.Children.Add(_comboUser);

            var labelPassword = new Label() { Content = Localizations.Password, VerticalAlignment = VerticalAlignment.Center };
            Grid.SetRow(labelPassword, 2);
            Grid.SetColumn(labelPassword, 0);
            contentGrid.Children.Add(labelPassword);
            _textPassword = new PasswordBox() { VerticalAlignment = VerticalAlignment.Center };
            Grid.SetRow(_textPassword, 2);
            Grid.SetColumn(_textPassword, 2);
            contentGrid.Children.Add(_textPassword);

            _buttonLogOn = new Button() { Content = "Log on", Width = 75, Height = 23, Margin = new Thickness(0, 0, 7, 0) };
            var buttonCancel = new Button() { Content = "Cancel", Width = 75, Height = 23, IsCancel = true };

            var buttonsStack = new StackPanel() { Margin = new Thickness(11), Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            buttonsStack.Children.Add(_buttonLogOn);
            buttonsStack.Children.Add(buttonCancel);

            var windowStack = new StackPanel();
            windowStack.Children.Add(new Border() { Child = contentGrid, Background = SystemColors.WindowBrush, BorderThickness = new Thickness(0) });
            windowStack.Children.Add(buttonsStack);

            _window = new Window() { FontFamily = SystemFonts.MessageFontFamily, FontSize = SystemFonts.MessageFontSize, FontWeight = SystemFonts.MessageFontWeight, MinWidth = 200, Background = SystemColors.ControlBrush };

            if (owner != null) {
                _window.Icon = owner.Icon;
                _window.Owner = owner;
                _window.ShowInTaskbar = false;
                if (owner.Topmost) { _window.Topmost = true; }
                _window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            } else {
                _window.Icon = GetAppIcon(Assembly.GetEntryAssembly());
                _window.Owner = null;
                _window.ShowInTaskbar = true;
                _window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
            _window.ResizeMode = ResizeMode.NoResize;
            _window.ShowActivated = true;
            _window.SizeToContent = SizeToContent.WidthAndHeight;
            _window.Title = "Log on";
            _window.Content = windowStack;

            _comboUser.KeyDown += new KeyEventHandler(_comboUser_KeyDown);
            _textPassword.KeyDown += new KeyEventHandler(_textPassword_KeyDown);
            _buttonLogOn.Click += new RoutedEventHandler(_buttonLogOn_Click);
            _window.Loaded += new RoutedEventHandler(window_Loaded);

            return _window.ShowDialog();
        }

        private void window_Loaded(object sender, RoutedEventArgs e) {
            var isUserSelected = false;
            foreach (var item in _comboUser.Items) {
                if (item.Equals(SelectedUserName)) {
                    _comboUser.SelectedItem = item;
                    isUserSelected = true;
                    break;
                }
            }
            if (isUserSelected) {
                _textPassword.Focus();
            } else {
                _comboUser.Focus();
            }
        }

        private void _comboUser_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                _textPassword.Focus();
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
            SelectedUserName = _comboUser.Text;
            SelectedPassword = _textPassword.Password;

            if (_comboUser.SelectedItem != null) {
                foreach (IIdentity item in _comboUser.Items) {
                    if (item.Equals(_comboUser.SelectedItem)) {
                        SelectedUserName = item.Name;
                        SelectedUser = item;
                        break;
                    }
                }
            } else {
                SelectedUser = null;
            }

            _window.DialogResult = true;
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
#pragma warning disable IDE0049 // Simplify Names

            [DllImport("user32.dll", CharSet = CharSet.Unicode)]
            static extern internal IntPtr LoadIcon(IntPtr hInstance, String lpIconName);

            [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
            static extern internal IntPtr LoadLibrary(String lpFileName);

#pragma warning restore IDE0049 // Simplify Names
        }

        private static class Localizations {

            internal static string UserName {
                get {
                    if (Thread.CurrentThread.CurrentUICulture.Name.StartsWith("hr-", StringComparison.OrdinalIgnoreCase)) {
                        return "Korisničko ime:";
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
