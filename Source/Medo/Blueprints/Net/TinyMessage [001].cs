//Josip Medved <jmedved@jmedved.com> http://www.jmedved.com

//2010-10-21: Initial version.


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Globalization;

namespace Medo.Net {

    /// <summary>
    /// Sending and receiving UDP messages.
    /// Requires .NET Framework 3.5 SP1 or above.
    /// </summary>
    public class TinyMessage : IDisposable {

        private const int DefaultPort = 5104;


        /// <summary>
        /// Creates new instance.
        /// </summary>
        public TinyMessage()
            : this(new IPEndPoint(IPAddress.Any, TinyMessage.DefaultPort)) {
        }

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="localEndPoint">Local end point where messages should be received at.</param>
        /// <exception cref="System.ArgumentNullException">Local IP end point is null.</exception>
        public TinyMessage(IPEndPoint localEndPoint) {
            if (localEndPoint == null) { throw new ArgumentNullException("localEndPoint", "Local IP end point is null."); }
            this.LocalEndPoint = localEndPoint;
        }

        /// <summary>
        /// Gets local IP end point.
        /// </summary>
        public IPEndPoint LocalEndPoint { get; private set; }


        /// <summary>
        /// Starts listener on background thread.
        /// </summary>
        public void ListenAsync() {
            lock (this.ListenSyncRoot) {
                if (this.ListenThread != null) { throw new InvalidOperationException("Already listening."); }

                this.ListenCancelEvent = new ManualResetEvent(false);
                this.ListenThread = new Thread(Run) { IsBackground = true, Name = "TinyMessage " + this.LocalEndPoint.ToString() };
                this.ListenThread.Start();
            }
        }

        /// <summary>
        /// Stops listener on background thread.
        /// </summary>
        public void CloseAsync() {
            lock (this.ListenSyncRoot) {
                if (this.ListenThread == null) { return; }

                this.ListenCancelEvent.Set();
                this.ListenSocket.Shutdown(SocketShutdown.Both);
                this.ListenSocket.Close();

                while (this.ListenThread.IsAlive) { Thread.Sleep(100); }
                this.ListenCancelEvent.Dispose();
                this.ListenThread = null;
            }
        }


        /// <summary>
        /// Raises event when packet arrives.
        /// </summary>
        public event EventHandler<TinyMessagePacketEventArgs> TinyMessagePacketReceived;


        #region Threading

        private Thread ListenThread;
        private ManualResetEvent ListenCancelEvent = null;
        private readonly object ListenSyncRoot = new object();
        private Socket ListenSocket = null;

        private bool IsCanceled { get { return ListenCancelEvent.WaitOne(0, false); } }

        private void Run() {
            try {
                this.ListenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                this.ListenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                this.ListenSocket.Bind(this.LocalEndPoint);

                var buffer = new byte[16384];
                EndPoint remoteEP;
                int inCount;
                while (!this.IsCanceled) {
                    try {
                        remoteEP = new IPEndPoint(IPAddress.Any, 0);
                        inCount = this.ListenSocket.ReceiveFrom(buffer, ref remoteEP);
                    } catch (SocketException ex) {
                        if (ex.SocketErrorCode == SocketError.Interrupted) {
                            return;
                        } else {
                            throw;
                        }
                    }

                    if (TinyMessagePacketReceived != null) {
                        var newBuffer = new byte[inCount];
                        Buffer.BlockCopy(buffer, 0, newBuffer, 0, inCount);
#if DEBUG
                        Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "TinyMessage [{0} <- {1}]", TinyMessagePacket<object>.ParseHeaderOnly(newBuffer, 0, inCount), remoteEP));
#endif
                        var invokeArgs = new object[] { this, new TinyMessagePacketEventArgs(newBuffer, 0, inCount, remoteEP as IPEndPoint) };
                        foreach (Delegate iDelegate in TinyMessagePacketReceived.GetInvocationList()) {
                            ISynchronizeInvoke syncer = iDelegate.Target as ISynchronizeInvoke;
                            if (syncer == null) {
                                iDelegate.DynamicInvoke(invokeArgs);
                            } else {
                                syncer.BeginInvoke(iDelegate, invokeArgs);
                            }
                        }
                    }
                }

            } catch (ThreadAbortException) {
            } finally {
                if (this.ListenSocket != null) {
                    this.ListenSocket.Dispose();
                    this.ListenSocket = null;
                }
            }

        }

        #endregion


        /// <summary>
        /// Sends UDP packet.
        /// </summary>
        /// <param name="packet">Packet to send.</param>
        /// <param name="address">IP address of destination for packet. It can be broadcast address.</param>
        /// <exception cref="System.ArgumentNullException">Packet is null. -or- Remote IP end point is null.</exception>
        public static void Send(ITinyMessagePacket packet, IPAddress address) {
            Send(packet, new IPEndPoint(address, TinyMessage.DefaultPort));
        }

        /// <summary>
        /// Sends UDP packet.
        /// </summary>
        /// <param name="packet">Packet to send.</param>
        /// <param name="address">IP address of destination for packet. It can be broadcast address.</param>
        /// <param name="port">Port of destination for packet.</param>
        /// <exception cref="System.ArgumentNullException">Packet is null. -or- Remote IP end point is null.</exception>
        public static void Send(ITinyMessagePacket packet, IPAddress address, int port) {
            Send(packet, new IPEndPoint(address, port));
        }

        /// <summary>
        /// Sends UDP packet.
        /// </summary>
        /// <param name="packet">Packet to send.</param>
        /// <param name="remoteEndPoint">Address of destination for packet. It can be broadcast address.</param>
        /// <exception cref="System.ArgumentNullException">Packet is null. -or- Remote IP end point is null.</exception>
        public static void Send(ITinyMessagePacket packet, IPEndPoint remoteEndPoint) {
            if (packet == null) { throw new ArgumentNullException("packet", "Packet is null."); }
            if (remoteEndPoint == null) { throw new ArgumentNullException("remoteEndPoint", "Remote IP end point is null."); }
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)) {
                if (remoteEndPoint.Address == IPAddress.Broadcast) {
                    socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
                }
                socket.SendTo(packet.GetBytes(), remoteEndPoint);
                Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "TinyMessage [{0} -> {1}]", packet, remoteEndPoint));
            }
        }


        #region IDisposable Members

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        public void Dispose() {
            this.Dispose(true);
            System.GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">True if managed resources should be disposed; otherwise, false.</param>
        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                this.CloseAsync();
            }
        }

        #endregion

    }



    /// <summary>
    /// Interface for TinyMessage packets.
    /// </summary>
    public interface ITinyMessagePacket {

        /// <summary>
        /// Gets name of product.
        /// </summary>
        string Product { get; }

        /// <summary>
        /// Gets operation.
        /// </summary>
        string Operation { get; }

        /// <summary>
        /// Converts message to it's representation in bytes.
        /// </summary>
        /// <returns></returns>
        byte[] GetBytes();

    }


    /// <summary>
    /// Encoder/decoder for TinyMessage packets.
    /// Requires .NET Framework 3.5 SP1 or above.
    /// </summary>
    public class TinyMessagePacket<TData> : ITinyMessagePacket {

        private static readonly UTF8Encoding TextEncoding = new UTF8Encoding(false);
        private static DataContractJsonSerializer JsonSerializer = new DataContractJsonSerializer(typeof(TData));

        /// <summary>
        /// Creates new instance
        /// </summary>
        /// <param name="product">Name of product. Preferred format would be application name, at (@) sign, IANA assigned Private Enterprise Number. E.g. Application@12345</param>
        /// <param name="operation">Message type.</param>
        /// <param name="data">Data to be encoded in JSON.</param>
        /// <exception cref="System.ArgumentNullException">Product is null or empty. -or- Operation is null or empty.</exception>
        /// <exception cref="System.ArgumentException">Product contains space character. -or- Operation contains space character.</exception>
        public TinyMessagePacket(string product, string operation, TData data) {
            if (string.IsNullOrEmpty(product)) { throw new ArgumentNullException("product", "Product is null or empty."); }
            if (product.Contains(" ")) { throw new ArgumentException("Product contains space character.", "product"); }
            if (string.IsNullOrEmpty(operation)) { throw new ArgumentNullException("operation", "Operation is null or empty."); }
            if (operation.Contains(" ")) { throw new ArgumentException("Operation contains space character.", "operation"); }

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
        /// <exception cref="System.InvalidOperationException">Packet length exceeds 65507 bytes.</exception>
        public byte[] GetBytes() {
            using (var stream = new MemoryStream()) {
                byte[] productBytes = TextEncoding.GetBytes(this.Product);
                stream.Write(productBytes, 0, productBytes.Length);

                stream.Write(new byte[] { 0x20 }, 0, 1);

                byte[] operationBytes = TextEncoding.GetBytes(this.Operation);
                stream.Write(operationBytes, 0, operationBytes.Length);

                stream.Write(new byte[] { 0x20 }, 0, 1);

                JsonSerializer.WriteObject(stream, this.Data);

                if (stream.Position > 65507) { throw new InvalidOperationException("Packet length exceeds 65507 bytes."); }

                return stream.ToArray();
            }
        }

        /// <summary>
        /// Returns parsed packet.
        /// </summary>
        /// <param name="buffer">Byte array.</param>
        /// <exception cref="System.ArgumentNullException">Buffer is null.</exception>
        /// <exception cref="System.IO.InvalidDataException">Cannot parse packet.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "I disagree with this warning.")]
        public static TinyMessagePacket<TData> Parse(byte[] buffer) {
            if (buffer == null) { throw new ArgumentNullException("buffer", "Buffer is null."); }

            return Parse(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Returns partially parsed packet. Data argument is NOT parsed.
        /// </summary>
        /// <param name="buffer">Byte array.</param>
        /// <param name="offset">Starting offset.</param>
        /// <param name="count">Total lenght.</param>
        /// <exception cref="System.ArgumentNullException">Buffer is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Offset is less than zero. -or- Count is less than zero. -or- The sum of offset and count is greater than the length of buffer.</exception>
        /// <exception cref="System.IO.InvalidDataException">Cannot parse packet.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "I disagree with this warning.")]
        public static TinyMessagePacket<TData> ParseHeaderOnly(byte[] buffer, int offset, int count) {
            if (buffer == null) { throw new ArgumentNullException("buffer", "Buffer is null."); }
            if (offset < 0) { throw new ArgumentOutOfRangeException("offset", "Index is less than zero."); }
            if (count < 0) { throw new ArgumentOutOfRangeException("count", "Count is less than zero."); }
            if (offset + count > buffer.Length) { throw new ArgumentOutOfRangeException("count", "The sum of offset and count is greater than the length of buffer."); }

            using (var stream = new MemoryStream(buffer, offset, count)) {
                string product = ReadToSpace(stream);
                string operation = ReadToSpace(stream);
                return new TinyMessagePacket<TData>(product, operation, default(TData));
            }
        }

        /// <summary>
        /// Returns parsed packet.
        /// </summary>
        /// <param name="buffer">Byte array.</param>
        /// <param name="offset">Starting offset.</param>
        /// <param name="count">Total lenght.</param>
        /// <exception cref="System.ArgumentNullException">Buffer is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Offset is less than zero. -or- Count is less than zero. -or- The sum of offset and count is greater than the length of buffer.</exception>
        /// <exception cref="System.IO.InvalidDataException">Cannot parse packet.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "I disagree with this warning.")]
        public static TinyMessagePacket<TData> Parse(byte[] buffer, int offset, int count) {
            if (buffer == null) { throw new ArgumentNullException("buffer", "Buffer is null."); }
            if (offset < 0) { throw new ArgumentOutOfRangeException("offset", "Index is less than zero."); }
            if (count < 0) { throw new ArgumentOutOfRangeException("count", "Count is less than zero."); }
            if (offset + count > buffer.Length) { throw new ArgumentOutOfRangeException("count", "The sum of offset and count is greater than the length of buffer."); }

            using (var stream = new MemoryStream(buffer, offset, count)) {
                string product = ReadToSpace(stream);
                string operation = ReadToSpace(stream);

                try {
                    TData data = (TData)JsonSerializer.ReadObject(stream);
                    return new TinyMessagePacket<TData>(product, operation, data);
                } catch (SerializationException ex) {
                    throw new InvalidDataException("Cannot parse packet.", ex);
                }
            }
        }

        private static string ReadToSpace(MemoryStream stream) {
            var bytes = new List<byte>(); ;
            while (true) {
                if (stream.Position == stream.Length) { throw new InvalidDataException("Cannot parse packet."); }
                var oneByte = (byte)stream.ReadByte();
                if (oneByte == 0x20) {
                    return TextEncoding.GetString(bytes.ToArray());
                } else {
                    bytes.Add(oneByte);
                }
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
            return this.Product + ":" + this.Operation;
        }

    }



    /// <summary>
    /// Event arguments for TinyMessagePacketReceived message.
    /// </summary>
    public class TinyMessagePacketEventArgs : EventArgs {

        private readonly byte[] Buffer;
        private readonly int Offset;
        private readonly int Count;

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="buffer">Buffer.</param>
        /// <param name="offset">Offset</param>
        /// <param name="count">Count.</param>
        /// <param name="remoteEndPoint">Remote end point.</param>
        /// <exception cref="System.ArgumentNullException">Buffer is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Offset is less than zero. -or- Count is less than zero. -or- The sum of offset and count is greater than the length of buffer.</exception>
        public TinyMessagePacketEventArgs(byte[] buffer, int offset, int count, IPEndPoint remoteEndPoint) {
            if (buffer == null) { throw new ArgumentNullException("buffer", "Buffer is null."); }
            if (offset < 0) { throw new ArgumentOutOfRangeException("offset", "Index is less than zero."); }
            if (count < 0) { throw new ArgumentOutOfRangeException("count", "Count is less than zero."); }
            if (offset + count > buffer.Length) { throw new ArgumentOutOfRangeException("count", "The sum of offset and count is greater than the length of buffer."); }

            this.Buffer = buffer;
            this.Offset = offset;
            this.Count = count;
            this.RemoteEndPoint = remoteEndPoint;
        }

        /// <summary>
        /// Gets end point that was origin of message.
        /// </summary>
        public IPEndPoint RemoteEndPoint { get; private set; }

        /// <summary>
        /// Returns parsed packet.
        /// </summary>
        /// <typeparam name="TData">Type of packet's data element.</typeparam>
        /// <exception cref="System.IO.InvalidDataException">Cannot parse packet.</exception>
        public TinyMessagePacket<TData> GetPacket<TData>() {
            return TinyMessagePacket<TData>.Parse(this.Buffer, this.Offset, this.Count);
        }

        /// <summary>
        /// Returns parsed packet.
        /// </summary>
        /// <exception cref="System.IO.InvalidDataException">Cannot parse packet.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification="Method is appropriate here.")]
        public ITinyMessagePacket GetPacketWithoutData() {
            return TinyMessagePacket<object>.ParseHeaderOnly(this.Buffer, this.Offset, this.Count);
        }

    }

}
