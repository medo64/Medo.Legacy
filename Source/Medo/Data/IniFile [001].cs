/* Josip Medved <jmedved@jmedved.com> * www.medo64.com * MIT License */

//2012-11-06: Initial version.


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

namespace Medo.Data {

    /// <summary>
    /// Reading and writing .ini files.
    /// </summary>
    public class IniFile {

        /// <summary>
        /// Creates new instances.
        /// </summary>
        public IniFile() {
            Initialize();
        }

        /// <summary>
        /// Creates new instances.
        /// </summary>
        /// <param name="stream">Stream to load.</param>
        public IniFile(Stream stream) {
            Load(stream);
        }

        /// <summary>
        /// Creates new instances.
        /// </summary>
        /// <param name="fileName">File name to load.</param>
        public IniFile(string fileName) {
            Load(fileName);
        }


        private void Initialize() {
            FileName = null;
            Sections = new List<IniSection>();
        }


        /// <summary>
        /// Gets list of sections.
        /// </summary>
        public IList<IniSection> Sections { get; private set; }

        /// <summary>
        /// Gets last file name used for load/save (if any).
        /// </summary>
        public string FileName { get; private set; }


        /// <summary>
        /// Adds single section.
        /// </summary>
        /// <param name="sectionName">Section name.</param>
        public IniSection AddSection(string sectionName) {
            var section = new IniSection(sectionName);
            Sections.Add(section);
            return section;
        }

        /// <summary>
        /// Adds single section and removes all others with same name.
        /// </summary>
        /// <param name="sectionName">Section name.</param>
        public IniSection SetSection(string sectionName) {
            var section = new IniSection(sectionName);
            RemoveSections(sectionName);
            Sections.Add(section);
            return section;
        }

        /// <summary>
        /// Removes all sections matching name.
        /// </summary>
        /// <param name="sectionName">Section name.</param>
        public void RemoveSections(string sectionName) {
            for (int i = Sections.Count - 1; i >= 0; i--) {
                if (Sections[i].Name.Equals(sectionName, StringComparison.OrdinalIgnoreCase)) {
                    Sections.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Returns first section matching a name.
        /// </summary>
        /// <param name="sectionName">Section name.</param>
        public IniSection GetSection(string sectionName) {
            foreach (var section in Sections) {
                if (section.Name.Equals(sectionName, StringComparison.OrdinalIgnoreCase)) {
                    return section;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns true if section exists.
        /// </summary>
        /// <param name="sectionName">Section name.</param>
        public bool HasSection(string sectionName) {
            foreach (var section in Sections) {
                if (section.Name.Equals(sectionName, StringComparison.OrdinalIgnoreCase)) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns all sections matching given name.
        /// </summary>
        /// <param name="sectionName">Section name.</param>
        public IEnumerable<IniSection> RetrieveSections(string sectionName) {
            foreach (var section in Sections) {
                if (section.Name.Equals(sectionName, StringComparison.OrdinalIgnoreCase)) {
                    yield return section;
                }
            }
        }


        /// <summary>
        /// Gets section that matches name.
        /// Name is not case sensitive.
        /// </summary>
        /// <param name="sectionName">Section name.</param>
        public IniSection this[string sectionName] {
            get {
                return GetSection(sectionName);
            }
        }


        #region Load and Save

        private static readonly Encoding Encoding = new UTF8Encoding(false);

        /// <summary>
        /// Saves all items.
        /// </summary>
        /// <param name="stream">Writtable stream.</param>
        public void Save(Stream stream) {
            var firstLine = true;
            using (var sw = new StreamWriter(stream, Encoding)) {
                var sb = new StringBuilder();
                foreach (var section in Sections) {
                    if (firstLine) { firstLine = false; } else { sw.WriteLine(); }
                    sb.Append('[');
                    if (Escape(sb, section.Name)) {
                        sb.Insert(1, '\"');
                        sb.Append('\"');
                    }
                    sb.Append(']');
                    sw.WriteLine(sb.ToString());
                    sb.Length = 0;

                    foreach (var property in section.Properties) {
                        if (Escape(sb, property.Name)) {
                            sb.Insert(0, '\"');
                            sb.Append('\"');
                        }
                        sb.Append(" = ");
                        var quoteIndex = sb.Length;
                        if (Escape(sb, property.Value)) {
                            sb.Insert(quoteIndex, '\"');
                            sb.Append('\"');
                        }
                        sw.WriteLine(sb.ToString());
                        sb.Length = 0;
                    }
                }
            }
            FileName = null;
        }

        /// <summary>
        /// Saves all items.
        /// </summary>
        /// <param name="fileName">File name.</param>
        public void Save(string fileName) {
            using (var stream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read)) {
                Save(stream);
            }
            FileName = FileName;
        }


        /// <summary>
        /// Loads file.
        /// </summary>
        /// <param name="stream">Stream.</param>
        public void Load(Stream stream) {
            Initialize();

            using (var sr = new StreamReader(stream, Encoding, true)) {
                var sbName = new StringBuilder();
                var sbValue = new StringBuilder();

                while (!(sr.EndOfStream)) {
                    var line = sr.ReadLine();
                    ParseLine(this, line, sbName, sbValue);
                }
            }
            FileName = null;
        }

        /// <summary>
        /// Loads file.
        /// </summary>
        /// <param name="fileName">File name.</param>
        public void Load(string fileName) {
            using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read, 64 * 1024, FileOptions.SequentialScan)) {
                Load(stream);
            }
            FileName = fileName;
        }

        #endregion


        #region Helper

        private static void ParseLine(IniFile file, string line, StringBuilder sbName, StringBuilder sbValue) {
            for (int i = 0; i < line.Length; i++) {
                var ch = line[i];
                if (char.IsWhiteSpace(ch)) {
                } else if (ch == ';') { //comment
                    break;
                } else if (ch == '[') { //section
                    ParseSection(file, line, i, sbName);
                    break;
                } else { //property
                    if (file.Sections.Count == 0) { throw new FormatException("Property before section."); }
                    ParseProperty(file.Sections[file.Sections.Count - 1], line, i, sbName, sbValue);
                    break;
                }
            }
        }

        private static void ParseSection(IniFile file, string line, int start, StringBuilder sbName) {
            sbName.Length = 0;

            var exitLoop = false;
            var state = SState.LookingForOpenBracket;
            for (int i = start; i < line.Length; i++) {
                if (exitLoop) { break; }
                var ch = line[i];

                switch (state) {
                    case SState.LookingForOpenBracket: {
                            if (ch == '[') {
                                state = SState.LookingForNameStart;
                            } else if (char.IsWhiteSpace(ch)) {
                            } else if (ch == ';') {
                                return; //just a comment
                            } else {
                                break;
                            }
                        } break;

                    case SState.LookingForNameStart: {
                            if (ch == '\"') {
                                state = SState.QuotedName;
                            } else if (char.IsWhiteSpace(ch)) {
                            } else if (ch == ']') {
                                state = SState.LookingForEnd;
                            } else if (ch == ';') {
                                exitLoop = true;
                            } else {
                                sbName.Append(ch);
                                state = SState.NonquotedName;
                            }
                        } break;

                    case SState.QuotedName: {
                            if (ch == '\\') {
                                if (Descape(line, ref i, sbName) == false) {
                                    throw new FormatException("Unrecognized escape sequence.");
                                }
                            } else if (AppendQuotedText(sbName, ch) == false) {
                                state = SState.LookingForCloseBracket;
                            }
                        } break;

                    case SState.NonquotedName: {
                            if (ch == ']') {
                                for (int j = sbName.Length - 1; j >= 0; j--) { //trim whitespace
                                    if (char.IsWhiteSpace(sbName[j])) { sbName.Remove(j, 1); } else { break; }
                                }
                                state = SState.LookingForEnd;
                            } else if (ch == ';') {
                                exitLoop = true;
                            } else {
                                sbName.Append(ch);
                            }
                        } break;

                    case SState.LookingForCloseBracket: {
                            if (ch == ']') {
                                state = SState.LookingForEnd;
                            } else if (ch == ';') {
                                exitLoop = true;
                            } else if (char.IsWhiteSpace(ch)) {
                            } else {
                                throw new FormatException("Invalid characters after name.");
                            }
                        } break;

                    case SState.LookingForEnd: {
                            if (ch == ';') {
                                exitLoop = true;
                            } else if (char.IsWhiteSpace(ch)) {
                            } else {
                                throw new FormatException("Invalid characters after value.");
                            }
                        } break;

                }
            }

            if (state == SState.LookingForOpenBracket) {
                throw new FormatException("Cannot find section start.");
            } else if (state == SState.LookingForNameStart) {
                throw new FormatException("Cannot find section name.");
            } else if (state == SState.QuotedName) {
                throw new FormatException("Non-terminated quote in name.");
            } else if (state == SState.LookingForCloseBracket) {
                throw new FormatException("Cannot find section end.");
            }

            file.Sections.Add(new IniSection(sbName.ToString()));
        }

        private static void ParseProperty(IniSection section, string line, int start, StringBuilder sbName, StringBuilder sbValue) {
            sbName.Length = 0;
            sbValue.Length = 0;

            var exitLoop = false;
            var state = PState.LookingForStart;
            for (int i = start; i < line.Length; i++) {
                if (exitLoop) { break; }
                var ch = line[i];

                switch (state) {
                    case PState.LookingForStart: {
                            if (ch == '\"') {
                                state = PState.QuotedName;
                            } else if (char.IsWhiteSpace(ch)) {
                            } else if (ch == '=') {
                                state = PState.LookingForValue;
                            } else if (ch == ';') {
                                return; //just a comment
                            } else {
                                sbName.Append(ch);
                                state = PState.NonquotedName;
                            }
                        } break;

                    case PState.QuotedName: {
                            if (ch == '\\') {
                                if (Descape(line, ref i, sbName) == false) {
                                    throw new FormatException("Unrecognized escape sequence.");
                                }
                            } else if (AppendQuotedText(sbName, ch) == false) {
                                state = PState.LookingForEquals;
                            }
                        } break;

                    case PState.NonquotedName: {
                            if (ch == '=') {
                                for (int j = sbName.Length - 1; j >= 0; j--) { //trim whitespace
                                    if (char.IsWhiteSpace(sbName[j])) { sbName.Remove(j, 1); }
                                }
                                state = PState.LookingForValue;
                            } else if (ch == ';') {
                                exitLoop = true;
                            } else {
                                sbName.Append(ch);
                            }
                        } break;

                    case PState.LookingForEquals: {
                            if (ch == '=') {
                                state = PState.LookingForValue;
                            } else if (ch == ';') {
                                exitLoop = true;
                            } else if (char.IsWhiteSpace(ch)) {
                            } else {
                                throw new FormatException("Invalid characters after name.");
                            }
                        } break;

                    case PState.LookingForValue: {
                            if (ch == '\"') {
                                state = PState.QuotedValue;
                            } else if (char.IsWhiteSpace(ch)) {
                            } else {
                                sbValue.Append(ch);
                                state = PState.NonquotedValue;
                            }
                        } break;

                    case PState.NonquotedValue: {
                            if (ch == ';') {
                                exitLoop = true;
                            } else {
                                sbValue.Append(ch);
                            }
                        } break;

                    case PState.QuotedValue: {
                            if (ch == '\\') {
                                if (Descape(line, ref i, sbValue) == false) {
                                    throw new FormatException("Unrecognized escape sequence.");
                                }
                            } else if (AppendQuotedText(sbValue, ch) == false) {
                                state = PState.LookingForEnd;
                            }
                        } break;

                    case PState.LookingForEnd: {
                            if (ch == ';') {
                                exitLoop = true;
                            } else if (char.IsWhiteSpace(ch)) {
                            } else {
                                throw new FormatException("Invalid characters after value.");
                            }
                        } break;

                }
            }

            if (state == PState.NonquotedValue) {
                for (int j = sbValue.Length - 1; j >= 0; j--) { //trim whitespace
                    if (char.IsWhiteSpace(sbValue[j])) { sbValue.Remove(j, 1); } else { break; }
                }
            } else if (state == PState.LookingForStart) {
                throw new FormatException("Cannot find property name.");
            } else if (state == PState.QuotedName) {
                throw new FormatException("Non-terminated quote in name.");
            } else if ((state == PState.LookingForEquals) || (state == PState.NonquotedName)) {
                throw new FormatException("Cannot find property value.");
            } else if (state == PState.QuotedValue) {
                throw new FormatException("Non-terminated quote in value.");
            }

            section.Properties.Add(new IniProperty(sbName.ToString(), sbValue.ToString()));
        }


        private enum SState {
            LookingForOpenBracket,
            LookingForNameStart,
            QuotedName,
            NonquotedName,
            LookingForCloseBracket,
            LookingForEnd
        }

        private enum PState {
            LookingForStart,
            QuotedName,
            NonquotedName,
            LookingForEquals,
            LookingForValue,
            QuotedValue,
            NonquotedValue,
            LookingForEnd
        }

        private static bool AppendQuotedText(StringBuilder sb, char ch) { //returns true if append happened.
            if (ch == '\"') { return false; }
            sb.Append(ch);
            return true;
        }

        private static bool Descape(string line, ref int i, StringBuilder sb) {
            i += 1;
            if (i >= line.Length) { return false; }
            switch (line[i]) {
                case '\"': sb.Append("\""); return true;
                case '\\': sb.Append("\\"); return true;
                case '/': sb.Append("/"); return true;
                case 'b': sb.Append("\b"); return true;
                case 'f': sb.Append("\f"); return true;
                case 'n': sb.Append("\n"); return true;
                case 'r': sb.Append("\r"); return true;
                case 't': sb.Append("\t"); return true;
                case '0': sb.Append("\0"); return true;
                case 'u':
                    if ((i + 4) >= line.Length) { return false; }
                    var hex = new string(new char[] { line[i + 1], line[i + 2], line[i + 3], line[i + 4] });
                    var codepoint = uint.Parse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                    sb.Append(System.Convert.ToChar(codepoint).ToString());
                    i += 4;
                    return true;
                default: return false;
            }
        }

        private static bool Escape(StringBuilder sb, string value) {
            bool shouldBeInQuotes = (value.Length > 0) && (char.IsWhiteSpace(value[0]) || char.IsWhiteSpace(value[value.Length - 1]));
            foreach (var ch in value) {
                switch (ch) {
                    case '\"': sb.Append(@"\"""); shouldBeInQuotes = true; break;
                    case '\\': sb.Append(@"\\"); shouldBeInQuotes = true; break;
                    case '\b': sb.Append(@"\b"); shouldBeInQuotes = true; break;
                    case '\f': sb.Append(@"\f"); shouldBeInQuotes = true; break;
                    case '\n': sb.Append(@"\n"); shouldBeInQuotes = true; break;
                    case '\r': sb.Append(@"\r"); shouldBeInQuotes = true; break;
                    case '\t': sb.Append(@"\t"); shouldBeInQuotes = true; break;
                    case ';':
                    case '=':
                        sb.Append(ch);
                        shouldBeInQuotes = true;
                        break;
                    default:
                        if (char.IsControl(ch)) {
                            sb.Append("\\u" + ((int)ch).ToString("X4", CultureInfo.InvariantCulture));
                            shouldBeInQuotes = true;
                        } else {
                            sb.Append(ch);
                        }
                        break;
                }
            }
            return shouldBeInQuotes;
        }

        #endregion

    }


    /// <summary>
    /// Section with properties.
    /// </summary>
    [DebuggerDisplay(@"{""["" + Name + ""]""}")]
    public class IniSection {

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="name">Section name.</param>
        public IniSection(string name)
            : this(name, null) {
        }

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="name">Section name.</param>
        /// <param name="properties">Section properties.</param>
        public IniSection(string name, IEnumerable<IniProperty> properties) {
            Name = name ?? throw new ArgumentNullException("name", "Name cannot be null.");
            Properties = (properties != null) ? new List<IniProperty>(properties) : new List<IniProperty>();
        }


        private string _name;
        /// <summary>
        /// Gets name.
        /// </summary>
        public string Name {
            get {
                return _name;
            }
            set {
                _name = value ?? throw new ArgumentNullException("value", "Name cannot be null.");
            }
        }

        /// <summary>
        /// Gets properties.
        /// </summary>
        public IList<IniProperty> Properties { get; private set; }


        /// <summary>
        /// Adds single property.
        /// </summary>
        /// <param name="propertyName">Property name.</param>
        /// <param name="propertyValue">Property value.</param>
        public IniProperty AddProperty(string propertyName, string propertyValue) {
            var property = new IniProperty(propertyName, propertyValue);
            Properties.Add(property);
            return property;
        }

        /// <summary>
        /// Adds single property and removes all other properties with same name.
        /// </summary>
        /// <param name="propertyName">Property name.</param>
        /// <param name="propertyValue">Property value.</param>
        public IniProperty SetProperty(string propertyName, string propertyValue) {
            var property = new IniProperty(propertyName, propertyValue);
            RemoveProperties(propertyName);
            Properties.Add(property);
            return property;
        }

        /// <summary>
        /// Removes all properties matching name.
        /// </summary>
        /// <param name="propertyName">Property name.</param>
        public void RemoveProperties(string propertyName) {
            for (int i = Properties.Count - 1; i >= 0; i--) {
                if (Properties[i].Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase)) {
                    Properties.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Returns first property matching a name.
        /// </summary>
        /// <param name="propertyName">Property name.</param>
        public IniProperty GetProperty(string propertyName) {
            foreach (var property in Properties) {
                if (property.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase)) {
                    return property;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns true if property exists.
        /// </summary>
        /// <param name="propertyName">Property name.</param>
        public bool HasProperty(string propertyName) {
            foreach (var property in Properties) {
                if (property.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase)) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns all properties matching given name.
        /// </summary>
        /// <param name="propertyName">Property name.</param>
        public IEnumerable<IniProperty> RetrieveProperties(string propertyName) {
            foreach (var property in Properties) {
                if (property.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase)) {
                    yield return property;
                }
            }
        }


        /// <summary>
        /// Gets property that matches name.
        /// Name is not case sensitive.
        /// </summary>
        /// <param name="propertyName">Property name.</param>
        /// <exception cref="System.ArgumentNullException">Value cannot be null.</exception>
        public IniProperty this[string propertyName] {
            get {
                return GetProperty(propertyName);
            }
            set {
                if (value == null) { throw new ArgumentNullException("value", "Value cannot be null."); }
                SetProperty(propertyName, value.Value);
            }
        }


        /// <summary>
        /// Returns whether two objects are equal.
        /// </summary>
        /// <param name="obj">Other object.</param>
        public override bool Equals(object obj) {
            return base.Equals(obj);
        }

        /// <summary>
        /// Returns hash code.
        /// </summary>
        public override int GetHashCode() {
            return Name.GetHashCode();
        }

    }


    /// <summary>
    /// Represents one property.
    /// </summary>
    [DebuggerDisplay(@"{Name + "" = "" + Value}")]
    public class IniProperty {

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="name">Property name.</param>
        /// <param name="value">Property value.</param>
        /// <exception cref="System.ArgumentNullException">Name or value cannot be null.</exception>
        public IniProperty(string name, string value) {
            Name = name ?? throw new ArgumentNullException("name", "Name cannot be null.");
            Value = value ?? throw new ArgumentNullException("value", "Value cannot be null.");
        }


        private string _name;
        /// <summary>
        /// Gets/sets name.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">Name cannot be null.</exception>
        public string Name {
            get {
                return _name;
            }
            set {
                _name = value ?? throw new ArgumentNullException("value", "Name cannot be null.");
            }
        }

        private string _value;
        /// <summary>
        /// Gets/sets value.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">Value cannot be null.</exception>
        public string Value {
            get {
                return _value;
            }
            set {
                _value = value ?? throw new ArgumentNullException("value", "Value cannot be null.");
            }
        }


        /// <summary>
        /// Returns whethet two objects are equal.
        /// Object are equals if they have equal (case insensitive) name and equal (case sensitive) value.
        /// </summary>
        /// <param name="obj">Other object.</param>
        public override bool Equals(object obj) {
            return ((obj is IniProperty other) && Name.Equals(other.Name, StringComparison.OrdinalIgnoreCase) && Value.Equals(other.Value, StringComparison.Ordinal));
        }

        /// <summary>
        /// Returns hash code.
        /// </summary>
        public override int GetHashCode() {
            return Name.GetHashCode();
        }


        /// <summary>
        /// Returns string value from property.
        /// </summary>
        /// <param name="property">Property.</param>
        public static string FromProperty(IniProperty property) {
            return property?.Value;
        }

        /// <summary>
        /// Returns property with empty name and value of propertyValue.
        /// </summary>
        /// <param name="propertyValue">Property value</param>
        public static IniProperty ToProperty(string propertyValue) {
            return new IniProperty("", propertyValue);
        }

        /// <summary>
        /// Returns string value from property.
        /// </summary>
        /// <param name="property">Property.</param>
        public static implicit operator string(IniProperty property) {
            return property?.Value;
        }

        /// <summary>
        /// Returns property with empty name and value of propertyValue.
        /// </summary>
        /// <param name="propertyValue">Property value</param>
        public static implicit operator IniProperty(string propertyValue) {
            return new IniProperty("", propertyValue);
        }

    }

}
