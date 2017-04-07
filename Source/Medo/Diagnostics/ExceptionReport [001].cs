//Josip Medved <jmedved@jmedved.com>   www.medo64.com

//2010-04-25: Initial version (based on ErrorReport).


using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Medo.Diagnostics {

    /// <summary>
    /// Creating of error reports.
    /// This class is thread-safe.
    /// </summary>
    public static class ExceptionReport {

        private const int LineLength = 72;
        private static readonly object _syncRoot = new object();
        private static readonly string _logSeparator = Environment.NewLine + Environment.NewLine + new string('-', LineLength) + Environment.NewLine + Environment.NewLine + Environment.NewLine;
        private static readonly string _logFileName;

        private static readonly string _infoProductTitle;
        private static readonly string _infoProductVersion;
        private static readonly string _infoAssemblyFullName;
        private static readonly string[] _infoReferencedAssemblies;


        /// <summary>
        /// Setting up of initial variable values in order to avoid setting them once problems (e.g. OutOfMemoryException) occur.
        /// </summary>
        static ExceptionReport() {
            var assembly = Assembly.GetEntryAssembly();

            object[] productAttributes = assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), true);
            if ((productAttributes != null) && (productAttributes.Length >= 1)) {
                ExceptionReport._infoProductTitle = ((AssemblyProductAttribute)productAttributes[productAttributes.Length - 1]).Product;
            } else {
                object[] titleAttributes = assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), true);
                if ((titleAttributes != null) && (titleAttributes.Length >= 1)) {
                    ExceptionReport._infoProductTitle = ((AssemblyTitleAttribute)titleAttributes[titleAttributes.Length - 1]).Title;
                } else {
                    ExceptionReport._infoProductTitle = assembly.GetName().Name;
                }
            }
            _infoProductVersion = assembly.GetName().Version.ToString();
            ExceptionReport._logFileName = System.IO.Path.Combine(System.IO.Path.GetTempPath(), string.Format(System.Globalization.CultureInfo.InvariantCulture, "ErrorReport [{0}].log", _infoProductTitle));

            _infoAssemblyFullName = assembly.FullName;

            var listReferencedAssemblies = new List<string>();
            foreach (var iRefAss in assembly.GetReferencedAssemblies()) {
                listReferencedAssemblies.Add(iRefAss.ToString());
            }
            _infoReferencedAssemblies = listReferencedAssemblies.ToArray();
        }


        /// <summary>
        /// Returns text report.
        /// </summary>
        /// <param name="exception">Exception which is processed.</param>
        public static string GetText(Exception exception) {
            return GetText(exception, null);
        }

        /// <summary>
        /// Returns text report.
        /// </summary>
        /// <param name="exception">Exception which is processed.</param>
        /// <param name="additionalInformation">Additional information to be added in log.</param>
        public static string GetText(Exception exception, params string[] additionalInformation) {
            lock (_syncRoot) {
                var buffer = new StringBuilder(8000);

                AppendLine(buffer, "Environment");
                AppendLine(buffer, "");
                AppendLine(buffer, _infoAssemblyFullName, 1, true);
                AppendLine(buffer, System.Environment.OSVersion.ToString(), 1, true);
                AppendLine(buffer, ".NET Framework " + System.Environment.Version.ToString(), 1, true);
                AppendLine(buffer, "Local time is " + DateTime.Now.ToString(@"yyyy\-MM\-dd\THH\:mm\:ssK", System.Globalization.CultureInfo.InvariantCulture), 1, true);

                if (exception != null) {
                    AppendLine(buffer, "");
                    Exception ex = exception;
                    int exLevel = 0;
                    while (ex != null) {
                        AppendLine(buffer, "");

                        if (exLevel == 0) {
                            AppendLine(buffer, "Exception");
                        } else if (exLevel == 1) {
                            AppendLine(buffer, "Inner exception (1)");
                        } else if (exLevel == 2) {
                            AppendLine(buffer, "Inner exception (2)");
                        } else {
                            AppendLine(buffer, "Inner exception (...)");
                        }
                        AppendLine(buffer, "");
                        if (!(exception is OutOfMemoryException)) {
                            AppendLine(buffer, ex.GetType().ToString(), 1, true);
                        }
                        AppendLine(buffer, ex.Message, 1, true);
                        if (!string.IsNullOrEmpty(ex.StackTrace)) {
                            AppendLine(buffer, ex.StackTrace, 2, false);
                        }

                        ex = ex.InnerException;
                        exLevel += 1;
                    }

                    AppendLine(buffer, "");
                    AppendLine(buffer, "");
                    AppendLine(buffer, "Referenced assemblies");
                    AppendLine(buffer, "");
                    for (int i = 0; i < _infoReferencedAssemblies.Length; ++i) {
                        AppendLine(buffer, _infoReferencedAssemblies[i], 1, true);
                    }
                }

                if ((additionalInformation != null) && (additionalInformation.Length > 0)) {
                    AppendLine(buffer, "");
                    AppendLine(buffer, "");
                    AppendLine(buffer, "Additional information");
                    AppendLine(buffer, "");
                    for (int i = 0; i < additionalInformation.Length; ++i) {
                        AppendLine(buffer, additionalInformation[i], 1, true);
                    }
                }

                return buffer.ToString();
            }
        }


        /// <summary>
        /// Writes file with exception details in temp directory.
        /// Returns true if write succeded.
        /// </summary>
        /// <param name="exception">Exception which is processed.</param>
        public static bool SaveToTemp(Exception exception) {
            return SaveToTemp(exception, null);
        }

        /// <summary>
        /// Writes file with exception details in temp directory.
        /// Returns true if write succeded.
        /// </summary>
        /// <param name="exception">Exception which is processed.</param>
        /// <param name="additionalInformation">Additional information to be added in log.</param>
        public static bool SaveToTemp(Exception exception, params string[] additionalInformation) {
            lock (_syncRoot) {
                if (System.IO.File.Exists(_logFileName)) {
                    System.IO.File.AppendAllText(_logFileName, _logSeparator);
                }

                System.IO.File.AppendAllText(_logFileName, GetText(exception, additionalInformation));

                return true;
            }
        }


        private static readonly AssemblyNameComparer _assemblyNameComparer = new AssemblyNameComparer();
        private class AssemblyNameComparer : IComparer<AssemblyName> {
            int IComparer<AssemblyName>.Compare(AssemblyName x, AssemblyName y) {
                return string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase);
            }
        }

        private static string UrlEncode(string text) {
            byte[] source = System.Text.UTF8Encoding.UTF8.GetBytes(text);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < source.Length; ++i) {
                if (((source[i] >= 48) && (source[i] <= 57)) || ((source[i] >= 65) && (source[i] <= 90)) || ((source[i] >= 97) && (source[i] <= 122)) || (source[i] == 45) || (source[i] == 46) || (source[i] == 95) || (source[i] == 126)) { //A-Z a-z - . _ ~
                    sb.Append(System.Convert.ToChar(source[i]));
                } else {
                    sb.Append("%" + source[i].ToString("X2", System.Globalization.CultureInfo.InvariantCulture));
                }
            }
            return sb.ToString();
        }

        private static void AppendLine(StringBuilder output, string input) {
            AppendLine(output, input, 0, false);
        }

        private static void AppendLine(StringBuilder output, string input, int indentLevel, bool tickO) {
            if (input == null) { return; }
            if (input.Length == 0) {
                output.AppendLine();
                return;
            }

            if (tickO) {
                indentLevel += 1;
            }


            int maxWidth = LineLength - indentLevel * 3;
            int end = input.Length - 1;

            int firstChar = 0;

            int lastChar;
            int nextChar;
            do {
                if ((end - firstChar) < maxWidth) {
                    lastChar = end;
                    nextChar = end + 1;
                } else {
                    int nextCrBreak = input.IndexOf('\r', firstChar, maxWidth);
                    int nextLfBreak = input.IndexOf('\n', firstChar, maxWidth);
                    int nextCrLfBreak;
                    if (nextCrBreak == -1) {
                        nextCrLfBreak = nextLfBreak;
                    } else if (nextLfBreak == -1) {
                        nextCrLfBreak = nextCrBreak;
                    } else {
                        nextCrLfBreak = System.Math.Min(nextCrBreak, nextLfBreak);
                    }
                    if ((nextCrLfBreak != -1) && ((nextCrLfBreak - firstChar) <= maxWidth)) {
                        lastChar = nextCrLfBreak - 1;
                        nextChar = lastChar + 2;
                        if (nextChar <= end) {
                            if ((input[nextChar] == '\n') || (input[nextChar] == '\r')) {
                                nextChar += 1;
                            }
                        }
                    } else {
                        int nextSpaceBreak = input.LastIndexOf(' ', firstChar + maxWidth, maxWidth);
                        if ((nextSpaceBreak != -1) && ((nextSpaceBreak - firstChar) <= maxWidth)) {
                            lastChar = nextSpaceBreak;
                            nextChar = lastChar + 1;
                        } else {
                            int nextOtherBreak1 = input.LastIndexOf('-', firstChar + maxWidth, maxWidth);
                            int nextOtherBreak2 = input.LastIndexOf(':', firstChar + maxWidth, maxWidth);
                            int nextOtherBreak3 = input.LastIndexOf('(', firstChar + maxWidth, maxWidth);
                            int nextOtherBreak4 = input.LastIndexOf(',', firstChar + maxWidth, maxWidth);
                            int nextOtherBreak = System.Math.Max(nextOtherBreak1, System.Math.Max(nextOtherBreak2, System.Math.Max(nextOtherBreak3, nextOtherBreak4)));
                            if ((nextOtherBreak != -1) && ((nextOtherBreak - firstChar) <= maxWidth)) {
                                lastChar = nextOtherBreak;
                                nextChar = lastChar + 1;
                            } else {
                                lastChar = firstChar + maxWidth;
                                if (lastChar > end) { lastChar = end; }
                                nextChar = lastChar;
                            }
                        }
                    }
                }

                if (tickO) {
                    for (int i = 0; i < indentLevel - 1; ++i) { output.Append("   "); }
                    output.Append("o  ");
                    tickO = false;
                } else {
                    for (int i = 0; i < indentLevel; ++i) { output.Append("   "); }
                }
                for (int i = firstChar; i <= lastChar; ++i) {
                    output.Append(input[i]);
                }
                output.AppendLine();

                firstChar = nextChar;
            } while (nextChar <= end);
        }

    }

}
