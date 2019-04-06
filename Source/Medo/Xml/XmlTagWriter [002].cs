/* Josip Medved <jmedved@jmedved.com> * www.medo64.com * MIT License */

//2007-12-24: Added WriteStartDocument method.
//2007-09-19: New version.


using System.Xml;

namespace Medo.Xml {

    /// <summary>
    /// Some shortcut methods to use when creating XML file.
    /// </summary>
    public class XmlTagWriter : System.IDisposable {

        /// <summary>
        /// Creates an instance of the class using the specified file.
        /// </summary>
        /// <param name="fileName">The filename to write to. If the file exists, it truncates it and overwrites it with the new content.</param>
        /// <param name="encoding">The encoding to generate. If encoding is null it writes the file out as UTF-8, and omits the encoding attribute from the ProcessingInstruction.</param>
        public XmlTagWriter(string fileName, System.Text.Encoding encoding) {
            XmlTextWriter = new System.Xml.XmlTextWriter(fileName, encoding) {
                Formatting = System.Xml.Formatting.Indented
            };
        }

        /// <summary>
        /// Creates an instance of the class using the specified stream and encoding.
        /// </summary>
        /// <param name="stream">The stream to which you want to write.</param>
        /// <param name="encoding">The encoding to generate. If encoding is null it writes the file out as UTF-8, and omits the encoding attribute from the ProcessingInstruction.</param>
        public XmlTagWriter(System.IO.Stream stream, System.Text.Encoding encoding) {
            XmlTextWriter = new System.Xml.XmlTextWriter(stream, encoding) {
                Formatting = System.Xml.Formatting.Indented
            };
        }

        /// <summary>
        /// Creates an instance of the class using the specified System.IO.TextWriter.
        /// </summary>
        /// <param name="textWriter">The TextWriter to write to. It is assumed that the TextWriter is already set to the correct encoding.</param>
        public XmlTagWriter(System.IO.TextWriter textWriter) {
            XmlTextWriter = new System.Xml.XmlTextWriter(textWriter) {
                Formatting = System.Xml.Formatting.Indented
            };
        }


        /// <summary>
        /// Writes the XML declaration with the version "1.0".
        /// </summary>
        public void WriteStartDocument() {
            XmlTextWriter.WriteStartDocument();
        }

        /// <summary>
        /// Writes raw markup manually from a character buffer.
        /// </summary>
        /// <param name="buffer">Character array containing the text to write.</param>
        /// <param name="index">The position within the buffer indicating the start of the text to write.</param>
        /// <param name="count">The number of characters to write.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Index or count is less than zero.-or-The buffer length minus index is less than count.</exception>
        /// <exception cref="System.ArgumentNullException">Buffer is null.</exception>
        public void WriteRawLine(char[] buffer, int index, int count) {
            XmlTextWriter.WriteRaw(buffer, index, count);
            XmlTextWriter.WriteRaw("\n");
        }

        /// <summary>
        /// Writes raw markup manually from a string and appends it with NewLine characters.
        /// </summary>
        /// <param name="data">String containing the text to write.</param>
        public void WriteRawLine(string data) {
            XmlTextWriter.WriteRaw(data);
            XmlTextWriter.WriteRaw("\n");
        }


        /// <summary>
        /// Writes out a start tag with the specified local name.
        /// </summary>
        /// <param name="localName">The local name of the element.</param>
        public void StartTag(string localName) {
            XmlTextWriter.WriteStartElement(localName);
        }

        /// <summary>
        /// Writes out a start tag with the specified local name and appends it with attributes.
        /// </summary>
        /// <param name="localName">The local name of the element.</param>
        /// <param name="attributesAndValues">Attributes and their values. In case of uneven number of elements, string is appended.</param>
        public void StartTag(string localName, params string[] attributesAndValues) {
            if (localName == null) { return; }
            if (attributesAndValues == null) { return; }
            XmlTextWriter.WriteStartElement(localName);
            for (var i = 0; i < attributesAndValues.Length - 1; i += 2) {
                XmlTextWriter.WriteAttributeString(attributesAndValues[i], attributesAndValues[i + 1]);
            }
            if (attributesAndValues.Length % 2 != 0) {
                if (!string.IsNullOrEmpty(attributesAndValues[attributesAndValues.Length - 1])) {
                    XmlTextWriter.WriteString(attributesAndValues[attributesAndValues.Length - 1]);
                }
            }
        }

        /// <summary>
        /// Closes one element and pops the corresponding namespace scope.
        /// </summary>
        public void EndTag() {
            XmlTextWriter.WriteEndElement();
        }

        /// <summary>
        /// Writes out both start and end tag with the specified local name.
        /// </summary>
        /// <param name="localName">The local name of the element.</param>
        public void WriteTag(string localName) {
            StartTag(localName);
            EndTag();
        }

        /// <summary>
        /// Writes out both start and end tag with the specified local name and appends it with attributes.
        /// </summary>
        /// <param name="localName">The local name of the element.</param>
        /// <param name="attributesAndValues">Attributes and their values. In case of uneven number of elements, string is appended.</param>
        public void WriteTag(string localName, params string[] attributesAndValues) {
            StartTag(localName, attributesAndValues);
            EndTag();
        }

        /// <summary>
        /// Gets underlyings XmlTextWriter.
        /// </summary>
        public System.Xml.XmlTextWriter XmlTextWriter { get; private set; }


        /// <summary>
        /// Closes the stream.
        /// </summary>
        public void Close() {
            XmlTextWriter.Close();
        }



        #region IDisposable Members

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        public void Dispose() {
            Dispose(true);
            System.GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">True if managed resources should be disposed; otherwise, false.</param>
        protected virtual void Dispose(bool disposing) {
            Close();
        }

        #endregion
    }

}
