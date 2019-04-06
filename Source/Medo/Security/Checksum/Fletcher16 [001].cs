/* Josip Medved <jmedved@jmedved.com> * www.medo64.com * MIT License */

//2017-04-24: Initial version.


using System;
using System.Security.Cryptography;

namespace Medo.Security.Checksum {

    /// <summary>
    /// Computes checksum using Fletcher-16 algorithm.
    /// </summary>
    public class Fletcher16 : HashAlgorithm {

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        public Fletcher16() { }


        /// <summary>
        /// Gets hash as 16-bit integer.
        /// </summary>
        public short HashAsInt16 => (short)((Sum2 << 8) | Sum1);


        #region HashAlgorithm

        /// <summary>
        /// Gets the size, in bits, of the computed hash code.
        /// </summary>
        public override int HashSize => 16;


        private int Sum1 = 0;
        private int Sum2 = 0;
        private const int MaximumRunningSum = int.MaxValue / 2; //to avoid modulus every run
        private bool InitializationPending;


        /// <summary>
        /// Initializes an instance.
        /// </summary>
        public override void Initialize() {
            InitializationPending = true; //to avoid base class' HashFinal call after ComputeHash clear HashAsInt16.
        }

        /// <summary>
        /// Computes the hash over the data.
        /// </summary>
        /// <param name="array">The input data.</param>
        /// <param name="ibStart">The offset into the byte array from which to begin using data.</param>
        /// <param name="cbSize">The number of bytes in the array to use as data.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Methods in base class are already validating these values.")]
        protected override void HashCore(byte[] array, int ibStart, int cbSize) {
            if (InitializationPending) {
                Sum1 = 0;
                Sum2 = 0;
                InitializationPending = false;
            }

            for (var i = ibStart; i < (ibStart + cbSize); i++) {
                Sum1 += array[i];
                Sum2 += Sum1;
                if (Sum2 > MaximumRunningSum) {
                    Sum1 %= 255;
                    Sum2 %= 255;
                }
            }

            Sum1 %= 255;
            Sum2 %= 255;
        }

        /// <summary>
        /// Finalizes the hash computation.
        /// </summary>
        /// <returns></returns>
        protected override byte[] HashFinal() {
            return new byte[] { (byte)Sum2, (byte)Sum1 };
        }

        #endregion

    }
}
