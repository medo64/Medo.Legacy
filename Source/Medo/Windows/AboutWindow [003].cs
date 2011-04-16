//Copyright (c) 2010 Josip Medved <jmedved@jmedved.com>

//2010-04-12: Initial version.
//2010-05-19: Refactoring (CA1303:Do not pass literals as localized parameters).
//            Executable icon is now displayed.
//2010-05-27: Design changes.


using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Interop;
using System.Runtime.InteropServices;

namespace Medo.Windows {

    /// <summary>
    /// Simple about form.
    /// </summary>
    public static class AboutWindow {

        /// <summary>
        /// Opens a window and returns only when the newly opened window is closed.
        /// </summary>
        /// <param name="owner">Window that owns this window.</param>
        public static bool? ShowDialog(Window owner) {
            return ShowDialog(owner, null, null);
        }

        /// <summary>
        /// Opens a window and returns only when the newly opened window is closed.
        /// </summary>
        /// <param name="owner">Window that owns this window.</param>
        /// <param name="webpage">URI of program's web page.</param>
        public static bool? ShowDialog(Window owner, Uri webpage) {
            return ShowDialog(owner, webpage, null);
        }

        /// <summary>
        /// Opens a window and returns only when the newly opened window is closed.
        /// </summary>
        /// <param name="owner">Window that owns this window.</param>
        /// <param name="webpage">URI of program's web page.</param>
        /// <param name="productAddendum">Addendum (such as beta) to be added to product name.</param>
        public static bool? ShowDialog(Window owner, Uri webpage, string productAddendum) {
            var assembly = Assembly.GetEntryAssembly();

            var iconImage = new Image() { Width = 32, Height = 32, Margin = new Thickness(0, 0, 7, 0), VerticalAlignment = VerticalAlignment.Center };
            if ((owner != null) && (owner.Icon != null)) { iconImage.Source = owner.Icon; } else { iconImage.Source = GetAppIcon(assembly); }

            string productText = GetAppProductText(assembly);
            if (!string.IsNullOrEmpty(productAddendum)) { productText += Localizations.LeftParenthesis + productAddendum + Localizations.RightParenthesis; }
            var productTextBlock = new TextBlock() { Text = productText, FontSize = SystemFonts.MessageFontSize * 2, FontWeight = FontWeights.Bold, VerticalAlignment = VerticalAlignment.Center };

            var titleStack = new StackPanel() { Orientation = Orientation.Horizontal };
            titleStack.Children.Add(iconImage);
            titleStack.Children.Add(productTextBlock);

            var titleAndVersionTextBlock = new TextBlock() { Text = GetAppTitleText(assembly) + Localizations.Space + assembly.GetName().Version.ToString(), FontSize = SystemFonts.MessageFontSize * 1.5, Margin = new Thickness(0, 7, 0, 0) };

            var dotNetFrameworkTextBlock = new TextBlock() { Text = Localizations.NetFramework + Environment.Version.ToString(), Margin = new Thickness(0, 7, 0, 0) };

            var osVersionTextBlock = new TextBlock() { Text = System.Environment.OSVersion.VersionString, Margin = new Thickness(0, 0, 0, 0) };
            
            var mainStack = new StackPanel() { Margin = new Thickness(11) };
            mainStack.Children.Add(titleStack);
            mainStack.Children.Add(titleAndVersionTextBlock);
            mainStack.Children.Add(dotNetFrameworkTextBlock);
            mainStack.Children.Add(osVersionTextBlock);

            string copyrightText = GetAppCopyright(assembly);
            if (!string.IsNullOrEmpty(copyrightText)) {
                var copyright = new TextBlock() { Text = copyrightText, Margin = new Thickness(0, 7, 0, 0) };
                mainStack.Children.Add(copyright);
            }


            var buttonPanelLeft = new StackPanel() { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 11, 0) };

            string readMePath = System.IO.Path.Combine(new FileInfo(assembly.Location).DirectoryName, "readme.txt");
            if (File.Exists(readMePath)) {
                var readmeButton = new Button() { Content = "Read me", Width = 75, Height = 23, Margin = new Thickness(0, 0, 7, 0), Tag = readMePath };
                readmeButton.Click += new RoutedEventHandler(readmeButton_Click);
                buttonPanelLeft.Children.Add(readmeButton);
            }

            if (webpage != null) {
                var webpageButton = new Button() { Content = "Web page", Width = 75, Height = 23, Margin = new Thickness(0, 0, 7, 0), Tag = webpage };
                webpageButton.Click += new RoutedEventHandler(webpageButton_Click);
                buttonPanelLeft.Children.Add(webpageButton);
            }

            var closeButton = new Button() { Content = "Close", Width = 75, Height = 23, IsDefault = true, IsCancel = true, HorizontalAlignment = HorizontalAlignment.Right };

            var buttonDockPanel = new DockPanel() { Margin = new Thickness(11) };
            buttonDockPanel.Children.Add(buttonPanelLeft);
            buttonDockPanel.Children.Add(closeButton);


            var windowStack = new StackPanel();
            windowStack.Children.Add(new Border() { Child = mainStack, Background = SystemColors.WindowBrush });
            windowStack.Children.Add(buttonDockPanel);

            var window = new Window() { FontFamily = SystemFonts.MessageFontFamily, FontSize = SystemFonts.MessageFontSize, FontWeight = SystemFonts.MessageFontWeight, MinWidth = 200, MaxWidth = 540, Background = SystemColors.ControlBrush };
            if (owner != null) {
                window.Icon = owner.Icon;
                window.Owner = owner;
                window.ShowInTaskbar = false;
                if (owner.Topmost) { window.Topmost = true; }
                window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            } else {
                window.Icon = GetAppIcon(assembly);
                window.Owner = null;
                window.ShowInTaskbar = true;
                window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
            window.ResizeMode = ResizeMode.NoResize;
            window.ShowActivated = true;
            window.SizeToContent = SizeToContent.WidthAndHeight;
            window.Title = "About";
            window.Content = windowStack;

            return window.ShowDialog();
        }


        private static void webpageButton_Click(object sender, RoutedEventArgs e) {
            try {
                var webpage = (Uri)((Control)sender).Tag;
                System.Diagnostics.Process.Start(webpage.ToString());
            } catch (System.ComponentModel.Win32Exception) { }
        }

        private static void readmeButton_Click(object sender, RoutedEventArgs e) {
            try {
                string readmePath = (string)((Control)sender).Tag;
                System.Diagnostics.Process.Start(readmePath);
            } catch (System.ComponentModel.Win32Exception) { }
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

        private static string GetAppProductText(Assembly assembly) {
            object[] productAttributes = assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), true);
            if ((productAttributes != null) && (productAttributes.Length >= 1)) {
                return ((AssemblyProductAttribute)productAttributes[productAttributes.Length - 1]).Product;
            } else {
                return GetAppTitleText(assembly);
            }
        }

        private static string GetAppTitleText(Assembly assembly) {
            object[] titleAttributes = assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), true);
            if ((titleAttributes != null) && (titleAttributes.Length >= 1)) {
                return ((AssemblyTitleAttribute)titleAttributes[titleAttributes.Length - 1]).Title;
            } else {
                return assembly.GetName().Name;
            }
        }

        private static string GetAppCopyright(Assembly assembly) {
            object[] copyrightAttributes = assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), true);
            if ((copyrightAttributes != null) && (copyrightAttributes.Length >= 1)) {
                return ((AssemblyCopyrightAttribute)copyrightAttributes[copyrightAttributes.Length - 1]).Copyright;
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

            internal static string RightParenthesis {
                get { return ")"; }
            }

            internal static string LeftParenthesis {
                get { return "("; }
            }

            internal static string Space {
                get { return " "; }
            }

            internal static string NetFramework {
                get { return ".NET framework "; }
            }

        }

    }

}
