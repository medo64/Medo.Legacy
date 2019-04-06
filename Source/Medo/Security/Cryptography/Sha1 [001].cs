/* Josip Medved <jmedved@jmedved.com> * www.medo64.com * MIT License */

//2008-04-12: New version.


using System;
using System.Security.Cryptography;

namespace Medo.Security.Cryptography {

    /// <summary>
    /// Computes hash using standard SHA-1 algorithm.
    /// </summary>
    public class Sha1 : IDisposable {

        private SHA1Managed _sha;
        private bool _isFinalized;

        /// <summary>
        /// Creates new instance.
        /// </summary>
        public Sha1() {
            _sha = new SHA1Managed();
        }


        /// <summary>
        /// Adds new data and returns current digest.
        /// </summary>
        /// <param name="value">Data to add.</param>
        /// <exception cref="System.ArgumentNullException">Value cannot be null.</exception>
        public void Append(byte[] value) {
            if (value == null) { throw new System.ArgumentNullException("value", Resources.ExceptionValueCannotBeNull); }
            Append(value, 0, value.Length);
        }

        /// <summary>
        /// Adds new data and returns current digest.
        /// </summary>
        /// <param name="value">Data to add.</param>
        /// <param name="index">A 32-bit integer that represents the index at which data begins.</param>
        /// <param name="length">A 32-bit integer that represents the number of elements.</param>
        /// <exception cref="System.ArgumentNullException">Value cannot be null.</exception>
        public void Append(byte[] value, int index, int length) {
            if (value == null) { throw new System.ArgumentNullException("value", Resources.ExceptionValueCannotBeNull); }
            _sha.TransformBlock(value, index, length, value, 0);
        }

        /// <summary>
        /// Adds new data and returns current digest.
        /// </summary>
        /// <param name="value">Data to add.</param>
        /// <param name="useAsciiEncoding">If True, ASCII encoding is used instead of Unicode.</param>
        /// <exception cref="System.ArgumentNullException">Value cannot be null.</exception>
        public void Append(string value, bool useAsciiEncoding) {
            if (value == null) { throw new System.ArgumentNullException("value", Resources.ExceptionValueCannotBeNull); }
            if (useAsciiEncoding) {
                Append(System.Text.ASCIIEncoding.ASCII.GetBytes(value));
            } else {
                Append(System.Text.UnicodeEncoding.Unicode.GetBytes(value));
            }//if
        }

        /// <summary>
        /// Gets current hash.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Cannot retrieve hash more than once.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is to maintain likeness with other Medo.Security.* classes.")]
        public byte[] Hash {
            get {
                if (_isFinalized) { throw new System.InvalidOperationException(Resources.ExceptionCannotRetrieveHashMoreThanOnce); }
                _isFinalized = true;
                _sha.TransformFinalBlock(new byte[] { }, 0, 0);
                return _sha.Hash;
            }
        }



        /// <summary>
        /// Computes CRC-32 (IEEE 802.3) digest from given data.
        /// </summary>
        /// <param name="value">Value.</param>
        public static byte[] ComputeHash(byte[] value) {
            Sha1 sha = new Sha1();
            sha.Append(value);
            return sha.Hash;
        }

        /// <summary>
        /// Computes CRC-32 (IEEE 802.3) digest from given data.
        /// </summary>
        /// <param name="value">Value.</param>
        /// <param name="index">A 32-bit integer that represents the index at which data begins.</param>
        /// <param name="length">A 32-bit integer that represents the number of elements.</param>
        public static byte[] ComputeHash(byte[] value, int index, int length) {
            Sha1 sha = new Sha1();
            sha.Append(value, index, length);
            return sha.Hash;
        }

        /// <summary>
        /// Computes CRC-32 (IEEE 802.3) digest from given data.
        /// </summary>
        /// <param name="value">Unicode text for performing hash functions.</param>
        /// <param name="useAsciiEncoding">If True, ASCII encoding is used instead of Unicode.</param>
        public static byte[] ComputeHash(string value, bool useAsciiEncoding) {
            Sha1 sha = new Sha1();
            sha.Append(value, useAsciiEncoding);
            return sha.Hash;
        }


        private static class Resources {

            internal static string ExceptionValueCannotBeNull { get { return "Value cannot be null."; } }
            internal static string ExceptionCannotRetrieveHashMoreThanOnce { get { return "Cannot retrieve hash more than once."; } }

        }


        #region IDisposable Members

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">True if managed resources should be disposed; otherwise, false.</param>
        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                if (_sha != null) {
                    ((System.IDisposable)_sha).Dispose();
                    _sha = null;
                }
            }
        }

        /// <summary>
        /// Disposes object.
        /// </summary>
        public void Dispose() {
            Dispose(true);
            System.GC.SuppressFinalize(this);
        }

        #endregion
    }

}
