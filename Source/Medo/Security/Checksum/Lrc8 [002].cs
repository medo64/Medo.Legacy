//Copyright (c) 2008 Josip Medved <jmedved@jmedved.com>

//2008-03-18: Initial version.
//2008-06-07: Append is not longer returning intermediate digest (performance reasons).


namespace Medo.Security.Checksum {

    /// <summary>
    /// Computes hash using 8-bit LRC algorithm.
    /// This is basicaly simple XOR on all bytes.
    /// </summary>
    public class Lrc8 {

        private byte _currDigest;


        /// <summary>
        /// Returns Eltra implementation.
        /// You would need to use DigestAsAscii30 on this also.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Calling the method results in different instances.")]
        public static Lrc8 GetEltra() {
            return new Lrc8(0x00);
        }

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="initialValue">Starting digest.</param>
        public Lrc8(byte initialValue) {
            this._currDigest = initialValue;
        }


        /// <summary>
        /// Adds new data to digest.
        /// </summary>
        /// <param name="value">Data to add.</param>
        /// <returns>Current digest.</returns>
        /// <exception cref="System.ArgumentNullException">Value cannot be null.</exception>
        public void Append(byte[] value) {
            if (value == null) { throw new System.ArgumentNullException("value", Resources.ExceptionValueCannotBeNull); }
            this.Append(value, 0, value.Length);
        }

        /// <summary>
        /// Adds new data to digest.
        /// </summary>
        /// <param name="value">Data to add.</param>
        /// <param name="index">A 32-bit integer that represents the index at which data begins.</param>
        /// <param name="length">A 32-bit integer that represents the number of elements.</param>
        /// <returns>Current digest.</returns>
        /// <exception cref="System.ArgumentNullException">Value cannot be null.</exception>
        public void Append(byte[] value, int index, int length) {
            if (value == null) { throw new System.ArgumentNullException("value", Resources.ExceptionValueCannotBeNull); }
            for (int i = index; i < index + length; i++) {
                this._currDigest = (byte)(this._currDigest ^ value[i]);
            }
        }

        /// <summary>
        /// Adds new data to digest.
        /// </summary>
        /// <param name="value">Data to add.</param>
        /// <param name="useAsciiEncoding">If True, ASCII encoding is used instead of Unicode.</param>
        /// <returns>Current digest.</returns>
        /// <exception cref="System.ArgumentNullException">Value cannot be null.</exception>
        public void Append(string value, bool useAsciiEncoding) {
            if (useAsciiEncoding) {
                this.Append(System.Text.ASCIIEncoding.ASCII.GetBytes(value));
            } else {
                this.Append(System.Text.UnicodeEncoding.Unicode.GetBytes(value));
            }//if
        }

        /// <summary>
        /// Gets current digest.
        /// </summary>
        public byte Digest {
            get { return this._currDigest; }
        }

        /// <summary>
        /// Gets current digest in splitted in two halfs with 0x30 added to each one.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is in order to have similar properties for all Medo.Security.Checksum namespace classes.")]
        public byte[] DigestAsAscii30 {
            get {
                byte part1 = (byte)(0x30 + (this.Digest >> 4));
                byte part2 = (byte)(0x30 + (this.Digest & 0x0f));
                return new byte[] { part1, part2 };
            }
        }


        private static class Resources {

            internal static string ExceptionValueCannotBeNull { get { return "Value cannot be null."; } }

        }

    }

}
