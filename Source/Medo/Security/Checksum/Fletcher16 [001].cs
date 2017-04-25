//Copyright 2017 by Josip Medved <jmedved@jmedved.com> (www.medo64.com) MIT License

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
        public Int16 HashAsInt16 => (Int16)((this.Sum2 << 8) | this.Sum1);


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
            this.InitializationPending = true; //to avoid base class' HashFinal call after ComputeHash clear HashAsInt16.
        }

        /// <summary>
        /// Computes the hash over the data.
        /// </summary>
        /// <param name="array">The input data.</param>
        /// <param name="ibStart">The offset into the byte array from which to begin using data.</param>
        /// <param name="cbSize">The number of bytes in the array to use as data.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Methods in base class are already validating these values.")]
        protected override void HashCore(byte[] array, int ibStart, int cbSize) {
            if (this.InitializationPending) {
                this.Sum1 = 0;
                this.Sum2 = 0;
                this.InitializationPending = false;
            }

            for (var i = ibStart; i < (ibStart + cbSize); i++) {
                this.Sum1 = this.Sum1 + array[i];
                this.Sum2 = this.Sum2 + this.Sum1;
                if (this.Sum2 > MaximumRunningSum) {
                    this.Sum1 %= 255;
                    this.Sum2 %= 255;
                }
            }

            this.Sum1 %= 255;
            this.Sum2 %= 255;
        }

        /// <summary>
        /// Finalizes the hash computation.
        /// </summary>
        /// <returns></returns>
        protected override byte[] HashFinal() {
            return new byte[] { (byte)this.Sum2, (byte)this.Sum1 };
        }

        #endregion

    }
}
