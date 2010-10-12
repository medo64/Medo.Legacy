//Josip Medved <jmedved@jmedved.com> http://www.jmedved.com

//2010-10-12: Initial version.


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Medo.Blueprints.Net {

    /// <summary>
    /// Sending and receiving UDP messages.
    /// Requires .NET Framework 3.5 SP1 or above.
    /// </summary>
    public class TinyMessage {




    }

    /// <summary>
    /// Encoder/decoder for TinyMessage packets.
    /// Requires .NET Framework 3.5 SP1 or above.
    /// </summary>
    public class TinyMessagePacket<TData> {

        private static readonly UTF8Encoding TextEncoding = new UTF8Encoding(false);
        private static DataContractJsonSerializer JsonSerializer = new DataContractJsonSerializer(typeof(TData));

        /// <summary>
        /// Creates new instance
        /// </summary>
        /// <param name="product">Name of product. Preferred format would be application name, at (@) sign, IANA assigned Private Enterprise Number. E.g. Application@12345</param>
        /// <param name="operation">Message type.</param>
        /// <param name="data">Data to be encoded in JSON.</param>
        /// <exception cref="System.ArgumentNullException">Product cannot be null or empty. -or- Operation cannot be null or empty.</exception>
        /// <exception cref="System.ArgumentException">Product cannot contain space character. -or- Operation cannot contain space character.</exception>
        public TinyMessagePacket(string product, string operation, TData data) {
            if (string.IsNullOrEmpty(product)) { throw new ArgumentNullException("product", "Product cannot be null or empty."); }
            if (product.Contains(' ')) { throw new ArgumentException("Product cannot contain space character.", "product"); }
            if (string.IsNullOrEmpty(operation)) { throw new ArgumentNullException("operation", "Operation cannot be null or empty."); }
            if (operation.Contains(' ')) { throw new ArgumentException("Operation cannot contain space character.", "operation"); }

            this.Product = product;
            this.Operation = operation;
            this.Data = data;
        }

        /// <summary>
        /// Gets name of product.
        /// </summary>
        public string Product { get; private set; }

        /// <summary>
        /// Gets operation.
        /// </summary>
        public string Operation { get; private set; }

        /// <summary>
        /// Gets data object.
        /// </summary>
        public TData Data { get; private set; }

        /// <summary>
        /// Converts message to it's representation in bytes.
        /// </summary>
        public byte[] GetBytes() {
            using (var stream = new MemoryStream()) {
                byte[] productBytes = TextEncoding.GetBytes(this.Product);
                stream.Write(productBytes, 0, productBytes.Length);

                stream.Write(new byte[] { 0x20 }, 0, 1);

                byte[] operationBytes = TextEncoding.GetBytes(this.Operation);
                stream.Write(operationBytes, 0, operationBytes.Length);

                stream.Write(new byte[] { 0x20 }, 0, 1);

                JsonSerializer.WriteObject(stream, this.Data);

                return stream.ToArray();
            }
        }

        /// <summary>
        /// Returns parsed packet.
        /// </summary>
        /// <param name="bytes">Buffer.</param>
        public static TinyMessagePacket<TData> Parse(byte[] bytes) {
            using (var stream = new MemoryStream(bytes)) {
                string product = null;
                var productBytes = new List<byte>(); ;
                for (; ; ) {
                    var oneByte = (byte)stream.ReadByte();
                    if (oneByte == 0x20) {
                        product = TextEncoding.GetString(productBytes.ToArray());
                        break;
                    } else {
                        productBytes.Add(oneByte);
                    }
                    if (stream.Position == stream.Length) { break; }
                }

                string operation = null;
                var operationBytes = new List<byte>(); ;
                for (; ; ) {
                    var oneByte = (byte)stream.ReadByte();
                    if (oneByte == 0x20) {
                        operation = TextEncoding.GetString(operationBytes.ToArray());
                        break;
                    } else {
                        operationBytes.Add(oneByte);
                    }
                    if (stream.Position == stream.Length) { break; }
                }


                TData data = (TData)JsonSerializer.ReadObject(stream);

                return new TinyMessagePacket<TData>(product, operation, data);
            }
        }


        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        public override bool Equals(object obj) {
            return base.Equals(obj);
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        public override int GetHashCode() {
            return (this.Product).GetHashCode() ^ this.Operation.GetHashCode();
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        public override string ToString() {
            return this.Product + "-" + this.Operation;
        }

    }

}
