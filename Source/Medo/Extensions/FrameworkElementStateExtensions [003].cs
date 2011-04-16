//Copyright (c) 2010 Josip Medved <jmedved@jmedved.com>

//2010-04-13: Initial version.
//2010-04-25: Changed code to be compatible with .NET 3.5.
//2010-05-14: Changed namespace.


using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using System.Text;
using System.Globalization;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.IO;

namespace Medo.Extensions.FrameworkElementState {

    /// <summary>
    /// </summary>
    public static class FrameworkElementStateExtensions {

        /// <summary>
        /// Restore window state.
        /// </summary>
        /// <param name="window">Window.</param>
        public static void RestoreState(this Window window) {
            OpenDictionary();

            try { window.Left = ReadSetting(window, "Left", window.Left); } catch (ArgumentException) { }
            try { window.Top = ReadSetting(window, "Top", window.Top); } catch (ArgumentException) { }
            try { window.Width = ReadSetting(window, "Width", window.Width); } catch (ArgumentException) { }
            try { window.Height = ReadSetting(window, "Height", window.Height); } catch (ArgumentException) { }
            try { window.WindowState = ReadSetting(window, "WindowState", window.WindowState); } catch (ArgumentException) { }
        }

        /// <summary>
        /// Save window state.
        /// </summary>
        /// <param name="window">Window.</param>
        public static void SaveState(this Window window) {
            OpenDictionary();

            WriteSetting(window, "WindowState", window.WindowState);
            WriteSetting(window, "Left", window.Left);
            WriteSetting(window, "Top", window.Top);
            WriteSetting(window, "Width", window.Width);
            WriteSetting(window, "Height", window.Height);

            SaveDictionary();
        }



        private const string _stateFileName = "Medo.Extensions.FrameworkElementStateExtensions.state";
        private static Dictionary<string, object> _stateDictionary;
        private static IsolatedStorageFile _isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
        private static readonly object _syncRoot = new object();

        private static void OpenDictionary() {
            lock (_syncRoot) {
                if (_stateDictionary == null) {
                    try {
                        using (var stream = new IsolatedStorageFileStream(_stateFileName, FileMode.Open)) {
                            var formatter = new BinaryFormatter();
                            _stateDictionary = formatter.Deserialize(stream) as Dictionary<string, object>;
                        }
                    } catch (FileNotFoundException) { }
                    if (_stateDictionary == null) { _stateDictionary = new Dictionary<string, object>(); }
                }
            }
        }

        private static void SaveDictionary() {
            lock (_syncRoot) {
                using (var stream = new IsolatedStorageFileStream(_stateFileName, FileMode.Create)) {
                    var formatter = new BinaryFormatter();
                    formatter.Serialize(stream, _stateDictionary);
                }
            }
        }


        private static void WriteSetting(FrameworkElement element, string property, double value) {
            lock (_syncRoot) {
                var key = GetPath(element) + "." + property;
                if (_stateDictionary.ContainsKey(key)) {
                    _stateDictionary[key] = value;
                } else {
                    _stateDictionary.Add(key, value);
                }
            }
        }

        private static double ReadSetting(FrameworkElement element, string property, double defaultValue) {
            lock (_syncRoot) {
                var key = GetPath(element) + "." + property;
                object value;
                if (_stateDictionary.TryGetValue(key, out value)) {
                    if (value is double) {
                        return (double)value;
                    }
                }
                return defaultValue;
            }
        }

        private static void WriteSetting(FrameworkElement element, string property, WindowState value) {
            lock (_syncRoot) {
                var key = GetPath(element) + "." + property;
                if (_stateDictionary.ContainsKey(key)) {
                    _stateDictionary[key] = value;
                } else {
                    _stateDictionary.Add(key, value);
                }
            }
        }

        private static WindowState ReadSetting(FrameworkElement element, string property, WindowState defaultValue) {
            lock (_syncRoot) {
                var key = GetPath(element) + "." + property;
                object value;
                if (_stateDictionary.TryGetValue(key, out value)) {
                    if (value is WindowState) {
                        return (WindowState)value;
                    }
                }
                return defaultValue;
            }
        }


        private static string GetPath(FrameworkElement element) {
            lock (_syncRoot) {
                var path = new StringBuilder();
                DependencyObject currentObject = element;
                while (currentObject != null) {
                    if (path.Length > 0) { path.Append("."); }
                    var currentElement = currentObject as FrameworkElement;
                    path.AppendFormat(currentObject.GetType().Name);
                    if ((currentElement != null) && (!string.IsNullOrEmpty(currentElement.Name))) {
                        path.AppendFormat("!");
                        path.AppendFormat(currentElement.Name);
                    }
                    currentObject = VisualTreeHelper.GetParent(currentObject);
                }
                return path.ToString();
            }
        }

    }

}
