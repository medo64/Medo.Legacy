//Josip Medved <jmedved@jmedved.com>

//2011-03-11: Initial version.


using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Medo.Configuration {

    /// <summary>
    /// Class for reading and writing ini files.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Ini", Justification = "This is intended naming")]
    public class IniFile {

        private readonly Dictionary<string, Dictionary<string, string>> Items = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
        private readonly Encoding Encoding = new UTF8Encoding(false);
        private readonly string NewLine = Environment.NewLine;

        /// <summary>
        /// Creates new instance.
        /// </summary>
        public IniFile() {
        }

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="fileName">Name of INI file.</param>
        /// <exception cref="System.ArgumentNullException">File name cannot be null.</exception>
        /// <exception cref="System.IO.IOException">Can not open file.</exception>
        public IniFile(string fileName) {
            if (fileName == null) { throw new ArgumentNullException("fileName", "File name cannot be null."); }
            try {
                using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
                    Initialize(stream);
                }
            } catch (IOException ex) {
                throw new IOException("Can not open file.", ex);
            }
        }

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="stream">Stream with data from .ini file.</param>
        /// <exception cref="System.ArgumentNullException">Stream cannot be null.</exception>
        public IniFile(Stream stream) {
            if (stream == null) { throw new ArgumentNullException("stream", "Stream cannot be null."); }
            Initialize(stream);
        }


        /// <summary>
        /// Returns value found or null if value is not found.
        /// </summary>
        /// <param name="section">Section.</param>
        /// <param name="name">Name.</param>
        public string Read(string section, string name) {
            return Read(section, name, null);
        }

        /// <summary>
        /// Returns value found or defaultValue if value is not found.
        /// </summary>
        /// <param name="section">Section.</param>
        /// <param name="name">Name.</param>
        /// <param name="defaultValue">Default value.</param>
        public string Read(string section, string name, string defaultValue) {
            if (this.Items.ContainsKey(section)) {
                var inner = this.Items[section];
                if (inner.ContainsKey(name)) {
                    return inner[name];
                }
            }
            return defaultValue;
        }

        /// <summary>
        /// Returns value found and converted to boolean or defaultValue if value is not found or it cannot be converted.
        /// </summary>
        /// <param name="section">Section.</param>
        /// <param name="name">Name.</param>
        /// <param name="defaultValue">Default value.</param>
        public bool Read(string section, string name, bool defaultValue) {
            bool result;
            if (bool.TryParse(this.Read(section, name, null), out result)) {
                return result;
            } else {
                return defaultValue;
            }
        }

        /// <summary>
        /// Returns value found and converted to integer or defaultValue if value is not found or it cannot be converted.
        /// </summary>
        /// <param name="section">Section.</param>
        /// <param name="name">Name.</param>
        /// <param name="defaultValue">Default value.</param>
        public int Read(string section, string name, int defaultValue) {
            int result;
            if (int.TryParse(this.Read(section, name, null), NumberStyles.Integer, CultureInfo.InvariantCulture, out result)) {
                return result;
            } else {
                return defaultValue;
            }
        }

        /// <summary>
        /// Returns value found and converted to double or defaultValue if value is not found or it cannot be converted.
        /// </summary>
        /// <param name="section">Section.</param>
        /// <param name="name">Name.</param>
        /// <param name="defaultValue">Default value.</param>
        public double Read(string section, string name, double defaultValue) {
            double result;
            if (double.TryParse(this.Read(section, name, null), NumberStyles.Float, CultureInfo.InvariantCulture, out result)) {
                return result;
            } else {
                return defaultValue;
            }
        }


        /// <summary>
        /// Sets value.
        /// </summary>
        /// <param name="section">Section.</param>
        /// <param name="name">Name.</param>
        /// <param name="value">Value.</param>
        /// <exception cref="System.ArgumentException">Invalid characters in section. -or- Invalid characters in name. -or- Invalid characters in value.</exception>
        public void Write(string section, string name, string value) {
            if (IsSectionValid(section) == false) { throw new ArgumentException("Invalid characters in section.", "section"); }
            if (IsNameValid(name) == false) { throw new ArgumentException("Invalid characters in name.", "name"); }
            if (IsValueValid(name) == false) { throw new ArgumentException("Invalid characters in value.", "value"); }

            if (this.Items.ContainsKey(section) == false) {
                this.Items.Add(section, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
            }
            var inner = this.Items[section];
            if (inner.ContainsKey(name) == false) {
                if (value != null) {
                    inner.Add(name, value);
                }
            } else {
                if (value != null) {
                    inner[name] = value;
                } else {
                    inner.Remove(name);
                }
            }
        }

        /// <summary>
        /// Sets value.
        /// </summary>
        /// <param name="section">Section.</param>
        /// <param name="name">Name.</param>
        /// <param name="value">Value.</param>
        public void Write(string section, string name, bool value) {
            this.Write(section, name, value.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Sets value.
        /// </summary>
        /// <param name="section">Section.</param>
        /// <param name="name">Name.</param>
        /// <param name="value">Value.</param>
        public void Write(string section, string name, int value) {
            this.Write(section, name, value.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Sets value.
        /// </summary>
        /// <param name="section">Section.</param>
        /// <param name="name">Name.</param>
        /// <param name="value">Value.</param>
        public void Write(string section, string name, double value) {
            this.Write(section, name, value.ToString(CultureInfo.InvariantCulture));
        }


        /// <summary>
        /// Deletes value.
        /// Same effect is archieved by setting value to null.
        /// </summary>
        /// <param name="section">Section.</param>
        /// <param name="name">Name.</param>
        public void Delete(string section, string name) {
            this.Write(section, name, null);
        }


        /// <summary>
        /// Saves current state to file.
        /// </summary>
        /// <param name="fileName">File name.</param>
        /// <exception cref="System.ArgumentNullException">File name cannot be null.</exception>
        /// <exception cref="System.IO.IOException">Can not open file.</exception>
        public void Save(string fileName) {
            if (fileName == null) { throw new ArgumentNullException("fileName", "File name cannot be null."); }
            try {
                using (var stream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read)) {
                    Save(stream);
                }
            } catch (IOException ex) {
                throw new IOException("Can not open file.", ex);
            }
        }

        /// <summary>
        /// Saves current state to stream.
        /// </summary>
        /// <param name="stream">Stream.</param>
        /// <exception cref="System.ArgumentNullException">Stream cannot be null.</exception>
        public void Save(Stream stream) {
            if (stream == null) { throw new ArgumentNullException("stream", "Stream cannot be null."); }
            StringBuilder sb = new StringBuilder();
            foreach (var section in this.Items) {
                if (section.Value.Count > 0) {
                    if (sb.Length > 0) { sb.Append(this.NewLine); }
                    sb.Append("[" + section.Key + "]" + this.NewLine);
                    foreach (var item in section.Value) {
                        sb.Append(item.Key + " = " + item.Value + this.NewLine);
                    }
                }
            }
            byte[] bytes = this.Encoding.GetBytes(sb.ToString());
            stream.Write(bytes, 0, bytes.Length);
        }



        #region IsValid

        //private void Initialize(Stream stream) {
        //    using (var sr = new StreamReader(stream, this.Encoding)) {
        //        string section = null;
        //        while (sr.Peek() >= 0) {
        //            var line = sr.ReadLine().Trim();
        //            if (line.StartsWith(";", StringComparison.OrdinalIgnoreCase)) { //comment
        //            } else if (line.StartsWith("[", StringComparison.OrdinalIgnoreCase) && line.EndsWith("]", StringComparison.OrdinalIgnoreCase)) { //section
        //                section = line.Substring(1, line.Length - 2);
        //            } else if ((section != null) && line.Contains("=")) {
        //                int iEq = line.IndexOf("=", StringComparison.OrdinalIgnoreCase);
        //                string name = line.Substring(0, iEq).Trim();
        //                string value = line.Substring(iEq + 1).Trim();
        //                if (value.StartsWith("\"", StringComparison.OrdinalIgnoreCase) && value.EndsWith("\"", StringComparison.OrdinalIgnoreCase)) {
        //                    this.Write(section, name, value.Substring(1, value.Length - 2));
        //                } else {
        //                    this.Write(section, name, value);
        //                }
        //            }
        //        }
        //    }
        //}

        private static bool IsSectionValid(string text) {
            foreach (var iChar in text) {
                if (char.IsWhiteSpace(iChar)) {
                    return false;
                }
            }
            return true;
        }

        private static bool IsNameValid(string text) {
            foreach (var iChar in text) {
                if (char.IsWhiteSpace(iChar) || (iChar == '=')) {
                    return false;
                }
            }
            return true;
        }

        private static bool IsValueValid(string text) {
            foreach (var iChar in text) {
                if ((iChar < 32) || (iChar > 127)) {
                    return false;
                }
            }
            return true;
        }


        private void Initialize(Stream stream) {
            using (var sr = new StreamReader(stream, this.Encoding)) {
                StringBuilder sbSection = new StringBuilder();
                StringBuilder sbName = new StringBuilder();
                StringBuilder sbValue = new StringBuilder();
                StringBuilder sbLine = new StringBuilder();
                int lineIndex = 1;
                CharState state = CharState.FirstCharInLine;
                char lastChar = '\0';
                while (sr.Peek() >= 0) {
                    char ch = (char)sr.Read();
                    if ((ch == '\r') || (ch == '\n')) {
                        if (!((ch == '\n') && (lastChar == '\r'))) {
                            lineIndex += 1;
                        }
                        sbLine.Length = 0;
                    } else {
                        sbLine.Append(ch);
                    }
                    switch (state) {
                        case CharState.FirstCharInLine: { //just starting 
                                if (ch == '[') { //this might be start of section
                                    sbSection.Length = 0;
                                    state = CharState.SectionCharacter;
                                } else if ((ch == '\r') || (ch == '\n')) {
                                    state = CharState.FirstCharInLine;
                                } else if ((ch == ' ') || (ch == '\t')) {
                                    state = CharState.WhitespacePrefix;
                                } else if (ch == ';') {
                                    state = CharState.Comment;
                                } else {
                                    sbName.Length = 0;
                                    sbName.Append(ch);
                                    state = CharState.NameCharacter;
                                }
                            } break;

                        case CharState.SectionCharacter: {
                                if ((ch == '\r') || (ch == '\n') || (ch == ';') || (ch == ' ') || (ch == '\t')) {
                                    throw new InvalidDataException(string.Format(CultureInfo.InvariantCulture, "File cannot be parsed (line {0}: \"{1}\").", lineIndex, sbLine), new InvalidOperationException("Unexpected end of section."));
                                } else if (ch == ']') { //end of section
                                    state = CharState.WhitespaceSufix;
                                } else {
                                    sbSection.Append(ch);
                                    state = CharState.SectionCharacter;
                                }
                            } break;

                        case CharState.NameCharacter: {
                                if ((ch == '\r') || (ch == '\n') || (ch == ';')) {
                                    throw new InvalidDataException(string.Format(CultureInfo.InvariantCulture, "File cannot be parsed (line {0}: \"{1}\").", lineIndex, sbLine), new InvalidOperationException("Unexpected end of name."));
                                } else if ((ch == ' ') || (ch == '\t')) {
                                    state = CharState.NameCharacterWhitespaceSuffix;
                                } else if (ch == '=') {
                                    sbValue.Length = 0;
                                    state = CharState.ValueCharacterPossibleWhitespace;
                                } else {
                                    sbName.Append(ch);
                                    state = CharState.NameCharacter;
                                }
                            } break;

                        case CharState.NameCharacterWhitespaceSuffix: {
                                if ((ch == '\r') || (ch == '\n') || (ch == ';')) {
                                    throw new InvalidDataException(string.Format(CultureInfo.InvariantCulture, "File cannot be parsed (line {0}: \"{1}\").", lineIndex, sbLine), new InvalidOperationException("Unexpected end of name."));
                                } else if ((ch == ' ') || (ch == '\t')) {
                                    state = CharState.NameCharacterWhitespaceSuffix;
                                } else if (ch == '=') {
                                    sbValue.Length = 0;
                                    state = CharState.ValueCharacterPossibleWhitespace;
                                } else {
                                    throw new InvalidDataException(string.Format(CultureInfo.InvariantCulture, "File cannot be parsed (line {0}: \"{1}\").", lineIndex, sbLine), new InvalidOperationException("Unexpected end of name."));
                                }
                            } break;

                        case CharState.ValueCharacterPossibleWhitespace: {
                                if ((ch == '\r') || (ch == '\n')) {
                                    this.Write(sbSection.ToString(), sbName.ToString(), sbValue.ToString());
                                    state = CharState.FirstCharInLine;
                                } else if (ch == ';') {
                                    this.Write(sbSection.ToString(), sbName.ToString(), sbValue.ToString());
                                    state = CharState.Comment;
                                } else if ((ch == ' ') || (ch == '\t')) {
                                    state = CharState.ValueCharacterPossibleWhitespace;
                                } else if ((ch == '\"')) {
                                    sbValue.Length = 0;
                                    state = CharState.QuotedValueCharacter;
                                } else {
                                    sbValue.Append(ch);
                                    state = CharState.ValueCharacter;
                                }
                            } break;

                        case CharState.ValueCharacter: {
                                if ((ch == '\r') || (ch == '\n')) {
                                    this.Write(sbSection.ToString(), sbName.ToString(), sbValue.ToString().TrimEnd());
                                    state = CharState.FirstCharInLine;
                                } else if (ch == ';') {
                                    this.Write(sbSection.ToString(), sbName.ToString(), sbValue.ToString().TrimEnd());
                                    state = CharState.Comment;
                                } else {
                                    sbValue.Append(ch);
                                    state = CharState.ValueCharacter;
                                }
                            } break;

                        case CharState.WhitespacePrefix: {
                                if ((ch == '\r') || (ch == '\n')) {
                                    state = CharState.FirstCharInLine;
                                } else if (ch == ';') { //comment
                                    state = CharState.Comment;
                                } else if ((ch == ' ') || (ch == '\t')) {
                                    state = CharState.WhitespacePrefix;
                                } else {
                                    throw new InvalidDataException(string.Format(CultureInfo.InvariantCulture, "File cannot be parsed (line {0}: \"{1}\").", lineIndex, sbLine), new InvalidOperationException("Unexpected character in whitespace line."));
                                }
                            } break;

                        case CharState.QuotedValueCharacter: {
                                if ((ch == '\r') || (ch == '\n')) {
                                    throw new InvalidDataException(string.Format(CultureInfo.InvariantCulture, "File cannot be parsed (line {0}: \"{1}\").", lineIndex, sbLine), new InvalidOperationException("Premature end of row."));
                                } else if (ch == '\"') {
                                    this.Write(sbSection.ToString(), sbName.ToString(), sbValue.ToString());
                                    state = CharState.WhitespaceSufix;
                                } else if (ch == '\\') {
                                    state = CharState.QuotedValueEscape;
                                } else {
                                    sbValue.Append(ch);
                                    state = CharState.QuotedValueCharacter;
                                }
                            } break;

                        case CharState.QuotedValueEscape: {
                                if (ch == 't') {
                                    sbValue.Append('\t');
                                    state = CharState.QuotedValueCharacter;
                                } else if (ch == 'n') {
                                    sbValue.Append('\n');
                                    state = CharState.QuotedValueCharacter;
                                } else if (ch == 'r') {
                                    sbValue.Append('\r');
                                    state = CharState.QuotedValueCharacter;
                                } else if (ch == '\"') {
                                    sbValue.Append('\"');
                                    state = CharState.QuotedValueCharacter;
                                } else if (ch == '\\') {
                                    sbValue.Append('\\');
                                    state = CharState.QuotedValueCharacter;
                                } else {
                                    throw new InvalidDataException(string.Format(CultureInfo.InvariantCulture, "File cannot be parsed (line {0}: \"{1}\").", lineIndex, sbLine), new InvalidOperationException("Invalid escape sequence."));
                                }
                            } break;

                        case CharState.WhitespaceSufix: {
                                if ((ch == '\r') || (ch == '\n')) {
                                    state = CharState.FirstCharInLine;
                                } else if (ch == ';') { //comment
                                    state = CharState.Comment;
                                } else if ((ch == ' ') || (ch == '\t')) {
                                    state = CharState.WhitespaceSufix;
                                } else {
                                    throw new InvalidDataException(string.Format(CultureInfo.InvariantCulture, "File cannot be parsed (line {0}: \"{1}\").", lineIndex, sbLine), new InvalidOperationException("Unexpected suffix character."));
                                }
                            } break;

                        case CharState.Comment: {
                                if ((ch == '\r') || (ch == '\n')) {
                                    state = CharState.FirstCharInLine;
                                } else {
                                    state = CharState.Comment;
                                }
                            } break;

                        default:
                            throw new InvalidDataException(string.Format(CultureInfo.InvariantCulture, "File cannot be parsed (line {0}: \"{1}\").", lineIndex, sbLine));
                    }

                    lastChar = ch;
                }
            }
        }

        private enum CharState {
            FirstCharInLine,
            SectionCharacter,
            NameCharacter,
            NameCharacterWhitespaceSuffix,
            ValueCharacterPossibleWhitespace,
            ValueCharacter,
            QuotedValueCharacter,
            QuotedValueEscape,
            WhitespacePrefix,
            WhitespaceSufix,
            Comment,
        }

        #endregion

    }
}
