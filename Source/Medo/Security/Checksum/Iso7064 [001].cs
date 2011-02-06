//Josip Medved <jmedved@jmedved.com> http://www.jmedved.com

//2009-01-05: Initial version.


namespace Medo.Security.Checksum {

    /// <summary>
    /// Computes hash using standard 8-bit CRC algorithm.
    /// </summary>
    public class Iso7064 {

        private int _digestSum;


        /// <summary>
        /// Creates new instance.
        /// </summary>
        public Iso7064() {
            this._digestSum = 10;
        }

        /// <summary>
        /// Adds new data to digest.
        /// </summary>
        /// <param name="value">Data to add.</param>
        /// <exception cref="System.ArgumentNullException">Value cannot be null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Only numeric data is supported.</exception>
        public void Append(char[] value) {
            if (value == null) { throw new System.ArgumentNullException("value", Resources.ExceptionValueCannotBeNull); }
            this.Append(value, 0, value.Length);
        }

        /// <summary>
        /// Adds new data to digest.
        /// </summary>
        /// <param name="value">Data to add.</param>
        /// <param name="index">A 32-bit integer that represents the index at which data begins.</param>
        /// <param name="length">A 32-bit integer that represents the number of elements.</param>
        /// <exception cref="System.ArgumentNullException">Value cannot be null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Only numeric data is supported.</exception>
        public void Append(char[] value, int index, int length) {
            if (value == null) { throw new System.ArgumentNullException("value", Resources.ExceptionValueCannotBeNull); }
            int oldDigest = this._digestSum;
            for (int i = index; i < index + length; ++i) {
                if ((value[i] >= '0') && (value[i] <= '9')) {
                    this._digestSum += (value[i] - '0');
                    if (this._digestSum > 10) { this._digestSum -= 10; }
                    this._digestSum *= 2;
                    if (this._digestSum >= 11) { this._digestSum -= 11; }
                } else {
                    this._digestSum = oldDigest;
                    throw new System.ArgumentOutOfRangeException("value", Resources.ExceptionOnlyNumericDataIsSupported);
                }
            }
        }

        /// <summary>
        /// Adds new data to digest.
        /// </summary>
        /// <param name="value">Data to add.</param>
        /// <exception cref="System.ArgumentNullException">Value cannot be null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Only numeric data is supported.</exception>
        public void Append(string value) {
            Append(value.ToCharArray(), 0, value.Length);
        }

        /// <summary>
        /// Gets current digest.
        /// </summary>
        public char Digest {
            get {
                int tmp = 11 - this._digestSum;
                if (tmp == 10) {
                    return '0';
                } else {
                    return System.Convert.ToChar('0' + tmp);
                }
            }
        }


        private static class Resources {

            internal static string ExceptionValueCannotBeNull { get { return "Value cannot be null."; } }
            internal static string ExceptionOnlyNumericDataIsSupported { get { return "Only numeric data is supported."; } }

        }

    }

}
