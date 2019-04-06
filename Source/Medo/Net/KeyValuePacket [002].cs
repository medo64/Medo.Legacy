/* Josip Medved <jmedved@jmedved.com> * www.medo64.com * MIT License */

//2008-11-04: Moved to Medo.Net namespace.
//2008-08-08: Initial version.


using System;
using System.Text;

namespace Medo.Net {

    /// <summary>
    /// Defines a key/value pair that can be set or retrieved.
    /// </summary>
    public struct KeyValuePacket {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="value">Value.</param>
        public KeyValuePacket(string key, byte[] value) {
            Key = key;
            _value = value;
        }

        /// <summary>
        /// Gets key.
        /// </summary>
        public string Key { get; private set; }

        private readonly byte[] _value;
        /// <summary>
        /// Returns value.
        /// </summary>
        public byte[] GetValue() {
            return _value;
        }


        /// <summary>
        /// Returns byte array.
        /// </summary>
        public byte[] ToArray() {
            byte[] baKey;
            byte[] baValue;
            int keyLength;
            int valueLength;

            if (Key == null) {
                baKey = new byte[] { };
                keyLength = -1;
            } else {
                baKey = UTF8Encoding.UTF8.GetBytes(Key);
                keyLength = baKey.Length;
            }
            if (_value == null) {
                baValue = new byte[] { };
                valueLength = -1;

            } else {
                baValue = _value;
                valueLength = baValue.Length;
            }

            //Packet format:
            //4x Header KVP1
            //4x Length of key
            //4x Length of value
            //4x XORed header check;
            //?x key as UTF8
            //?x value as UTF8

            var baHeader = new byte[] { 0x4B, 0x56, 0x50, 0x01 };
            var keyLengthLE = System.BitConverter.GetBytes(keyLength);
            var baKeyLengthBE = new byte[] { keyLengthLE[3], keyLengthLE[2], keyLengthLE[1], keyLengthLE[0] };
            var valueLengthLE = System.BitConverter.GetBytes(valueLength);
            var baValueLengthBE = new byte[] { valueLengthLE[3], valueLengthLE[2], valueLengthLE[1], valueLengthLE[0] };
            var baXor = new byte[] { (byte)(baHeader[0] ^ baKeyLengthBE[1] ^ baValueLengthBE[2]), (byte)(baHeader[1] ^ baKeyLengthBE[2] ^ baValueLengthBE[3]), (byte)(baHeader[2] ^ baKeyLengthBE[3] ^ baValueLengthBE[0]), (byte)(baHeader[3] ^ baKeyLengthBE[0] ^ baValueLengthBE[1]) };

            var ret = new byte[4 + 4 + 4 + 4 + baKey.Length + baValue.Length];
            System.Array.Copy(baHeader, 0, ret, 0, 4);
            System.Array.Copy(baKeyLengthBE, 0, ret, 4, 4);
            System.Array.Copy(baValueLengthBE, 0, ret, 8, 4);
            System.Array.Copy(baXor, 0, ret, 12, 4);
            System.Array.Copy(baKey, 0, ret, 16 + 0, baKey.Length);
            System.Array.Copy(baValue, 0, ret, 16 + baKey.Length, baValue.Length);
            return ret;
        }


        /// <summary>
        /// Returns parsed array.
        /// </summary>
        /// <param name="array">Array to parse.</param>
        /// <exception cref="System.FormatException">Not enough bytes to start. -or- Invalid header. -or- Not enough bytes. -or- Invalid checksum.</exception>
        public static KeyValuePacket Parse(byte[] array) {
            return Parse(array, 0);
        }

        /// <summary>
        /// Returns parsed array.
        /// </summary>
        /// <param name="array">Array to parse.</param>
        /// <param name="offset">Array offset at which to start.</param>
        /// <exception cref="System.FormatException">Not enough bytes to start. -or- Invalid header. -or- Not enough bytes. -or- Invalid checksum.</exception>
        public static KeyValuePacket Parse(byte[] array, int offset) {
            if (TryParse(array, offset, out var result, out _, out var exception)) {
                return result;
            } else {
                throw exception;
            }
        }

        /// <summary>
        /// Returns true if parsing is successful.
        /// </summary>
        /// <param name="array">Array to parse.</param>
        /// <param name="offset">Array offset at which to start.</param>
        /// <param name="result">Out. Result of parse.</param>
        public static bool TryParse(byte[] array, int offset, out KeyValuePacket result) {
            return TryParse(array, offset, out result, out _, out _);
        }

        /// <summary>
        /// Returns true if parsing is successful.
        /// </summary>
        /// <param name="array">Array to parse.</param>
        /// <param name="offset">Array offset at which to start.</param>
        /// <param name="result">Out. Result of parse.</param>
        /// <param name="newOffset">Out. Index at which last operation was performed.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#", Justification = "TryParse needs out parameter.")]
        public static bool TryParse(byte[] array, int offset, out KeyValuePacket result, out int newOffset) {
            return TryParse(array, offset, out result, out newOffset, out _);
        }

        private static bool TryParse(byte[] array, int offset, out KeyValuePacket result, out int newOffset, out System.Exception exception) {
            if (offset + 16 > array.Length) {
                result = new KeyValuePacket();
                newOffset = offset;
                exception = new System.FormatException("Not enough bytes to start.");
                return false;
            }

            var baHeader = new byte[4];
            System.Array.Copy(array, offset + 0, baHeader, 0, 4);
            if ((baHeader[0] != 0x4B) || (baHeader[1] != 0x56) || (baHeader[2] != 0x50) || (baHeader[3] != 0x01)) {
                result = new KeyValuePacket();
                newOffset = offset;
                exception = new System.FormatException("Invalid header.");
                return false;
            }

            var baKeyLengthBE = new byte[4];
            System.Array.Copy(array, offset + 4, baKeyLengthBE, 0, 4);
            var keyLengthLE = new byte[] { baKeyLengthBE[3], baKeyLengthBE[2], baKeyLengthBE[1], baKeyLengthBE[0] };
            var keyLength = System.BitConverter.ToInt32(keyLengthLE, 0);
            var realKeyLength = keyLength;
            if (realKeyLength == -1) { realKeyLength = 0; }

            var baValueLengthBE = new byte[4];
            System.Array.Copy(array, offset + 8, baValueLengthBE, 0, 4);
            var valueLengthLE = new byte[] { baValueLengthBE[3], baValueLengthBE[2], baValueLengthBE[1], baValueLengthBE[0] };
            var valueLength = System.BitConverter.ToInt32(valueLengthLE, 0);
            var realValueLength = valueLength;
            if (realValueLength == -1) { realValueLength = 0; }

            if (offset + 16 + realKeyLength + realValueLength > array.Length) {
                result = new KeyValuePacket();
                newOffset = offset;
                exception = new System.FormatException("Not enough bytes.");
                return false;
            }

            var baXor = new byte[4];
            System.Array.Copy(array, offset + 12, baXor, 0, 4);
            if ((baXor[0] != (byte)(baHeader[0] ^ baKeyLengthBE[1] ^ baValueLengthBE[2])) || (baXor[1] != (byte)(baHeader[1] ^ baKeyLengthBE[2] ^ baValueLengthBE[3])) || (baXor[2] != (byte)(baHeader[2] ^ baKeyLengthBE[3] ^ baValueLengthBE[0])) || (baXor[3] != (byte)(baHeader[3] ^ baKeyLengthBE[0] ^ baValueLengthBE[1]))) {
                result = new KeyValuePacket();
                newOffset = offset;
                exception = new System.FormatException("Invalid checksum.");
                return false;
            }

            string key;
            if (keyLength == -1) {
                key = null;
            } else {
                key = UTF8Encoding.UTF8.GetString(array, offset + 16 + 0, realKeyLength);
            }

            byte[] value;
            if (valueLength == -1) {
                value = null;
            } else {
                value = new byte[realValueLength];
                System.Array.Copy(array, offset + 16 + realKeyLength, value, 0, realValueLength);
            }


            result = new KeyValuePacket(key, value);
            newOffset = offset + 16 + realKeyLength + realValueLength;
            exception = null;
            return true;
        }



        /// <summary>
        /// Returns a value indicating whether this instance's Key is equal to the specified TagItem object's Key.
        /// </summary>
        /// <param name="obj">A TagItem object to compare to this instance.</param>
        public override bool Equals(object obj) {
            if (obj is KeyValuePacket otherKeyValuePacket) {
                return (Key.Equals(otherKeyValuePacket.Key));
            }

            var otherString = obj as string;
            if (obj != null) {
                return (Key.Equals(otherString));
            }

            return false;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode() {
            return Key.GetHashCode();
        }

        /// <summary>
        /// Converts the value of this instance to its equivalent string representation.
        /// </summary>
        /// <returns>String that represents this instance.</returns>
        public override string ToString() {
            if (Key == null) {
                return "";
            } else {
                return Key;
            }
        }


        /// <summary>
        /// Returns true if both objects have same key.
        /// </summary>
        /// <param name="objA">First object.</param>
        /// <param name="objB">Second object.</param>
        public static bool operator ==(KeyValuePacket objA, KeyValuePacket objB) {
            return objA.Key.Equals(objB.Key);
        }

        /// <summary>
        /// Returns true if both objects have same key.
        /// </summary>
        /// <param name="objA">First object.</param>
        /// <param name="objB">Second object.</param>
        public static bool operator ==(KeyValuePacket objA, string objB) {
            return objA.Key.Equals(objB);
        }

        /// <summary>
        /// Returns true if both objects have same key.
        /// </summary>
        /// <param name="objA">First object.</param>
        /// <param name="objB">Second object.</param>
        public static bool operator ==(string objA, KeyValuePacket objB) {
            return objB.Key.Equals(objA);
        }

        /// <summary>
        /// Returns true if both objects have different key.
        /// </summary>
        /// <param name="objA">First object.</param>
        /// <param name="objB">Second object.</param>
        public static bool operator !=(KeyValuePacket objA, KeyValuePacket objB) {
            return !objA.Key.Equals(objB.Key);
        }

        /// <summary>
        /// Returns true if both objects have different key.
        /// </summary>
        /// <param name="objA">First object.</param>
        /// <param name="objB">Second object.</param>
        public static bool operator !=(KeyValuePacket objA, string objB) {
            return !objA.Key.Equals(objB);
        }

        /// <summary>
        /// Returns true if both objects have different key.
        /// </summary>
        /// <param name="objA">First object.</param>
        /// <param name="objB">Second object.</param>
        public static bool operator !=(string objA, KeyValuePacket objB) {
            return !objB.Key.Equals(objA);
        }

    }

}
