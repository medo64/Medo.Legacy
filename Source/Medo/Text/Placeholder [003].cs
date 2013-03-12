//Copyright (c) 2013 Josip Medved <jmedved@jmedved.com>

//2013-03-04: Initial version.
//2013-03-08: Bug-fixing.
//2013-03-11: Nulls are supported.


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace Medo.Text {

    /// <summary>
    /// Composite formatting based on placeholder name.
    /// </summary>
    public static class Placeholder {

        /// <summary>
        /// Replaces one or more format items in a specified string with the string representation of a specified object.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="items">Replacement items.</param>
        /// <exception cref="System.ArgumentNullException">Format string cannot be null.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1719:ParameterNamesShouldNotMatchMemberNames", MessageId = "0#", Justification = "Naming kept to match string.Format.")]
        public static String Format(String format, IDictionary<String, Object> items) {
            return Format(CultureInfo.CurrentCulture, format, items);
        }

        /// <summary>
        /// Replaces one or more format items in a specified string with the string representation of a specified object.
        /// </summary>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="items">Replacement items.</param>
        /// <exception cref="System.ArgumentNullException">Provider cannot be null. -or- Format string cannot be null.</exception>
        /// <exception cref="System.ArgumentException">Invalid closing brace. -or- Cannot find placeholder item.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1719:ParameterNamesShouldNotMatchMemberNames", MessageId = "1#", Justification = "Naming kept to match string.Format.")]
        public static String Format(IFormatProvider provider, String format, IDictionary<String, Object> items) {
            if (provider == null) { throw new ArgumentNullException("provider", "Provider cannot be null."); }
            if (format == null) { throw new ArgumentNullException("format", "Format string cannot be null."); }

            var sbFormat = new StringBuilder();
            var sbArgName = new StringBuilder();
            var sbArgFormat = new StringBuilder();
            var argIndex = 0;
            var args = new List<Object>();

            var state = State.Default;
            foreach (var ch in format) {
                switch (state) {
                    case State.Default: {
                            if (ch == '{') {
                                state = State.LeftBrace;
                            } else if (ch == '}') {
                                state = State.RightBrace;
                            } else {
                                sbFormat.Append(ch);
                            }
                        } break;

                    case State.LeftBrace: {
                            if (ch == '{') {
                                sbFormat.Append("{{");
                                state = State.Default;
                            } else {
                                sbArgName.Append(ch);
                                state = State.ArgName;
                            }
                        } break;

                    case State.RightBrace: {
                            if (ch == '}') {
                                sbFormat.Append("}}");
                                state = State.Default;
                            } else {
                                throw new ArgumentException("Invalid closing brace.", "format");
                            }
                        } break;

                    case State.ArgName: {
                            if (ch == '}') {
                                var argName = sbArgName.ToString();
                                object arg;                                
                                if (GetArg(items, argName, out arg)) {
                                    args.Add(arg);
                                    sbFormat.AppendFormat(CultureInfo.InvariantCulture, "{{{0}}}", argIndex);
                                    argIndex += 1;
                                } else {
                                    throw new ArgumentException("Cannot find placeholder item '" + argName + "'.", "items");
                                }
                                sbArgName.Length = 0;
                                state = State.Default;
                            } else if (ch == ':') {
                                state = State.ArgFormat;
                            } else {
                                sbArgName.Append(ch);
                            }
                        } break;

                    case State.ArgFormat: {
                            if (ch == '}') {
                                var argName = sbArgName.ToString();
                                object arg;
                                if (GetArg(items, argName, out arg)) {
                                    args.Add(arg);
                                    var argFormat = sbArgFormat.ToString();
                                    sbFormat.AppendFormat(CultureInfo.InvariantCulture, "{{{0}:{1}}}", argIndex, argFormat);
                                    argIndex += 1;
                                } else {
                                    throw new ArgumentException("Cannot find placeholder item '" + argName + "'.", "items");
                                }
                                sbArgName.Length = 0;
                                sbArgFormat.Length = 0;
                                state = State.Default;
                            } else {
                                sbArgFormat.Append(ch);
                            }
                        } break;

                    default: Trace.Fail("Unknown state (" + state.ToString() + ")."); break;
                }
            }

            return String.Format(provider, sbFormat.ToString(), args.ToArray());
        }


        /// <summary>
        /// Replaces one or more format items in a specified string with the string representation of a specified object.
        /// </summary>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="namesAndValues">Names and values interleaved together.</param>
        /// <exception cref="System.ArgumentNullException">Provider cannot be null. -or- Format string cannot be null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">There must be even number of names and values. -or- Name must be a string.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1719:ParameterNamesShouldNotMatchMemberNames", MessageId = "1#", Justification = "Naming kept to match string.Format.")]
        public static String Format(IFormatProvider provider, String format, params Object[] namesAndValues) {
            var items = new Dictionary<String, Object>();
            if (namesAndValues != null) {
                if ((namesAndValues.Length % 2) != 0) { throw new ArgumentOutOfRangeException("namesAndValues", "There must be even number of names and values."); }
                for (int i = 0; i < namesAndValues.Length; i += 2) {
                    var name = namesAndValues[i] as string;
                    if (name == null) { throw new ArgumentOutOfRangeException("namesAndValues", "Name must be a string."); }
                    var value = namesAndValues[i + 1];
                    items.Add(name, value);
                }
            }
            return Format(provider, format, items);
        }

        /// <summary>
        /// Replaces one or more format items in a specified string with the string representation of a specified object.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="namesAndValues">Names and values interleaved together.</param>
        /// <exception cref="System.ArgumentNullException">Provider cannot be null. -or- Format string cannot be null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">There must be even number of names and values. -or- Name must be a string.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1719:ParameterNamesShouldNotMatchMemberNames", MessageId = "0#", Justification = "Naming kept to match string.Format.")]
        public static String Format(String format, params Object[] namesAndValues) {
            return Format(CultureInfo.CurrentCulture, format, namesAndValues);
        }


        private static bool GetArg(IDictionary<String, Object> items, string argumentName, out object value) {
            if (items.TryGetValue(argumentName, out value)) {
                return true;
            } else {
                return false;
            }
        }


        private enum State {
            Default,
            LeftBrace,
            RightBrace,
            ArgName,
            ArgFormat,
        }

    }
}
