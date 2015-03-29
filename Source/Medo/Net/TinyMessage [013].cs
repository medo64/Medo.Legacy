//Copyright (c) 2011 Josip Medved <jmedved@jmedved.com>

//2011-08-26: Initial version (based on TinyMessage).
//2011-10-22: Adjusted to work on Mono.
//            Added IsListening property.
//2011-10-24: Added UseObjectEncoding.
//2011-11-07: Fixing encoding/decoding.
//            Changed all parsing errors to throw FormatException.
//2012-02-06: Renamed to TinyMessage.
//            Removed array encoding.
//            TinyPacket data items are accessible through indexed property.
//            Null strings can be encoded.
//2012-02-08: Changed GetHashCode.
//2012-02-19: Added IEnumerable interface to TinyPacket.
//2012-05-16: Fixing exception on null items.
//            Refactoring.
//2014-12-20: Adding encrypted mode.
//            Removing static Send methods.
//            Refactoring (not fully compatible with previous version).
//2014-12-26: Improving multicast code (sending to all interfaces).
//            Improving duplicate detection.
//2015-03-28: Allowing underscore (_) as key name.
//            Fixed filtering bug.
//            System properties start with dot (before it was underscore).
//2015-03-29: System property .Host is sent even in encrypted messages.


using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Medo.Net {

    /// <summary>
    /// Sending and receiving UDP messages.
    /// Supports TinyMessage with Dictionary&lt;String,String&gt; as data.
    /// </summary>
    public class TinyMessage : IDisposable {

        /// <summary>
        /// Creates new instance.
        /// </summary>
        public TinyMessage()
            : this(new IPEndPoint[] { 
                TinyMessage.DefaultIPv6MulticastEndpoint,
                TinyMessage.DefaultIPv4MulticastEndpoint,
                new IPEndPoint(IPAddress.IPv6Any, TinyMessage.DefaultPort),
                new IPEndPoint(IPAddress.Any, TinyMessage.DefaultPort),
            }) {
        }

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="localEndpoints">List of endpoints for receiving packet.</param>
        /// <exception cref="System.ArgumentNullException">Endpoints cannot be null. -or- Endpoints cannot contain null entries.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Endpoints must contain either IPv4 or IPv6 entries.</exception>
        public TinyMessage(params IPEndPoint[] localEndpoints) {
            if (localEndpoints == null) { throw new ArgumentNullException("localEndpoints", "Endpoints cannot be null."); }
            foreach (var endpoint in localEndpoints) {
                if (endpoint == null) { throw new ArgumentNullException("localEndpoints", "Endpoints cannot contain null entries."); }
                if ((endpoint.AddressFamily != AddressFamily.InterNetwork) && (endpoint.AddressFamily != AddressFamily.InterNetworkV6)) {
                    throw new ArgumentOutOfRangeException("localEndpoints", "Endpoints must contain either IPv4 or IPv6 entries.");
                }
            }

            this.PrivateLocalEndpoints = localEndpoints;
            this.FilterDuplicates = true;
        }


        private readonly IEnumerable<IPEndPoint> PrivateLocalEndpoints;
        /// <summary>
        /// Gets local IP end point.
        /// </summary>
        public IEnumerable<IPEndPoint> LocalEndpoints {
            get {
                foreach (var endpoint in this.PrivateLocalEndpoints) {
                    yield return new IPEndPoint(endpoint.Address, endpoint.Port); //to avoid somebody changing endpoint from outside
                }
            }
        }

        private byte[] _key = new byte[] { 42, 42, 42, 42, 42, 42, 42, 42, 42, 42, 42, 42, 42, 42, 42, 42 };
        /// <summary>
        /// Gets/sets key used for the AES encryption.
        /// </summary>
        /// <exception cref="System.ArgumentOutOfRangeException">Key must be 16 bytes (128 bits) in length.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Done intentionally to match common usage in other classes (e.g. RijndaelManaged).")]
        public Byte[] Key {
            get { return this._key; }
            set {
                if ((value != null) && (value.Length != 16)) { throw new ArgumentOutOfRangeException("value", "Key must be 16 bytes (128 bits) in length."); }
                this._key = value;
            }
        }

        /// <summary>
        /// Gets/sets filter for Product field in the packet.
        /// Packets not matching given filter will not be reported in TinyPacketReceived event.
        /// If filter is null, all packets will be reported.
        /// </summary>
        public String ProductFilter { get; set; }

        /// <summary>
        /// Gets/sets filter for Operation field in the packet.
        /// Packets not matching given filter will not be reported in TinyPacketReceived event.
        /// If filter is null, all packets will be reported.
        /// </summary>
        public String OperationFilter { get; set; }


        /// <summary>
        /// Gets/sets whether packets will by filtered for duplicates by receiving engine.
        /// Encrypted packets will be filtered based on their IV.
        /// Unencrypted packets will not be filtered.
        /// </summary>
        public Boolean FilterDuplicates { get; set; }


        /// <summary>
        /// Raises event when packet arrives.
        /// </summary>
        public event EventHandler<TinyPacketEventArgs> PacketReceived;


        #region Listen/Close

        /// <summary>
        /// Starts listeners on idependent threads.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Socket will be disposed in Close function.")]
        public void Listen() {
            if (this.HasBegunListening) { throw new InvalidOperationException("Listening in progress."); }
            this.HasBegunListening = true;

#if DEBUG
            var sw = Stopwatch.StartNew();
#endif

            var endpoints = this.LocalEndpoints;

            this.ListenCancelEvent = new ManualResetEvent(false);
            var listeningData = new List<Tuple<IPEndPoint, Socket, IList<Object>, Thread, String>>();

            try {
                foreach (var endpoint in endpoints) {
                    string tag;
                    if (endpoint.Address == IPAddress.IPv6Any) {
                        tag = "IPV6Any:" + endpoint.Port.ToString(CultureInfo.InvariantCulture);
                    } else if (endpoint.Address == IPAddress.Any) {
                        tag = "IPV4Any:" + endpoint.Port.ToString(CultureInfo.InvariantCulture);
                    } else {
                        tag = endpoint.ToString();
                    }

                    Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "TinyMessage: {0} Setup started.", tag));

                    IList<object> multicastOptions = null;

                    var socket = new Socket(endpoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
                    socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                    if (endpoint.Address.IsIPv6Multicast) {

                        socket.Bind(new IPEndPoint(IPAddress.IPv6Any, endpoint.Port));

                        multicastOptions = new List<object>();
                        foreach (var netInterface in GetApplicableNetworkInterfaces(endpoint.AddressFamily)) {
                            var netProperties = netInterface.GetIPProperties().GetIPv6Properties();
                            if (netProperties != null) {
                                try {
                                    var option = new IPv6MulticastOption(endpoint.Address, netProperties.Index);
                                    socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.AddMembership, option);
                                    Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "TinyMessage: {0} Added multicast membership to interface {1} ({2}).", tag, netProperties.Index, netInterface.Name));
                                    multicastOptions.Add(option);
                                } catch (SocketException ex) {
                                    Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "TinyMessage: {0} Exception adding multicast membership to interface {1} ({2}): {3}", tag, netProperties.Index, netInterface.Name, ex.Message));
                                }
                            }
                        }

                    } else if ((endpoint.Address.AddressFamily == AddressFamily.InterNetwork) && ((endpoint.Address.GetAddressBytes()[0] & 0xE0) == 0xE0)) { //IPv4 multicast

                        socket.Bind(new IPEndPoint(IPAddress.Any, endpoint.Port));

                        multicastOptions = new List<object>();
                        foreach (var netInterface in GetApplicableNetworkInterfaces(endpoint.AddressFamily)) {
                            var netProperties = netInterface.GetIPProperties().GetIPv4Properties();
                            if (netProperties != null) {
                                try {
                                    var option = new MulticastOption(endpoint.Address, netProperties.Index);
                                    socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, option);
                                    Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "TinyMessage: {0} Added multicast membership to interface {1} ({2}).", tag, netProperties.Index, netInterface.Name));
                                    multicastOptions.Add(option);
                                } catch (SocketException ex) {
                                    Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "TinyMessage: {0} Exception adding multicast membership to interface {1} ({2}): {3}", tag, netProperties.Index, netInterface.Name, ex.Message));
                                }
                            }
                        }

                    } else { //all other IP addresses

                        socket.Bind(endpoint);

                    }

                    var thread = new Thread(Run) { IsBackground = true, Name = "TinyMessage " + tag };

                    var datum = new Tuple<IPEndPoint, Socket, IList<Object>, Thread, String>(endpoint, socket, multicastOptions, thread, tag);
                    Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "TinyMessage: {0} Setup done.", tag));

                    thread.Start(datum);
                    listeningData.Add(datum);
                }
            } catch (Exception ex) {
                Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "TinyMessage: {0} exception: {1}", ex.GetType().Name, ex.Message));
                Cleanup(listeningData);
                throw;
            }

            this.ListeningData = listeningData.AsReadOnly();

#if DEBUG
            sw.Stop();
            Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "TinyMessage: Listen completed in {0} milliseconds.", sw.ElapsedMilliseconds));
#endif
        }

        /// <summary>
        /// Starts listeners on idependent threads.
        /// </summary>
        public async Task ListenAsync() {
            if (this.HasBegunListening) { throw new InvalidOperationException("Listening in progress."); }

            await Task.Run(() => {
                this.Listen();
            });
        }


        /// <summary>
        /// Stops listeners.
        /// </summary>
        public void Close() {
            if (this.IsListening == false) { return; } //only attempt close if all listening threads are setup.

#if DEBUG
            var sw = Stopwatch.StartNew();
#endif

            this.ListenCancelEvent.Set();

            Cleanup(this.ListeningData);

            this.ListenCancelEvent.Dispose();
            this.ListenCancelEvent = null;

            this.ListeningData = null;
            this.HasBegunListening = false;


#if DEBUG
            sw.Stop();
            Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "TinyMessage: Close completed in {0} milliseconds.", sw.ElapsedMilliseconds));
#endif
        }

        /// <summary>
        /// Stops listeners.
        /// </summary>
        public async Task CloseAsync() {
            if (this.IsListening == false) { return; } //only attempt close if all listening threads are setup.

            await Task.Run(() => {
                this.Close();
            });
        }


        private static void Cleanup(IEnumerable<Tuple<IPEndPoint, Socket, IList<Object>, Thread, String>> listeningData) {
#if DEBUG
            var sw = Stopwatch.StartNew();
#endif

            foreach (var item in listeningData) {
                var endpoint = (IPEndPoint)item.Item1;
                var socket = (Socket)item.Item2;
                var multicastOptions = (IList<object>)item.Item3;
                var tag = (String)item.Item5;
                Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "TinyMessage: {0} Socket cleanup.", tag));

                if (endpoint.Address.IsIPv6Multicast) {
                    foreach (var multicastOption in multicastOptions) {
                        var option = (IPv6MulticastOption)multicastOption;
                        try {
                            socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.DropMembership, option);
                            Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "TinyMessage: {0} Dropped multicast membership for interface {1}.", tag, option.InterfaceIndex));
                        } catch (SocketException ex) {
                            Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "TinyMessage: {0} Exception adding multicast membership to interface {1}: {2}", tag, option.InterfaceIndex, ex.Message));
                        }
                    }
                } else if ((endpoint.Address.AddressFamily == AddressFamily.InterNetwork) && ((endpoint.Address.GetAddressBytes()[0] & 0xE0) == 0xE0)) { //IPv4 multicast
                    foreach (var multicastOption in multicastOptions) {
                        var option = (MulticastOption)multicastOption;
                        try {
                            socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.DropMembership, option);
                            Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "TinyMessage: {0} Dropped multicast membership for interface {1}.", tag, option.InterfaceIndex));
                        } catch (SocketException ex) {
                            Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "TinyMessage: {0} Exception adding multicast membership to interface {1}: {2}", tag, option.InterfaceIndex, ex.Message));
                        }
                    }
                }
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "TinyMessage: {0} Socket cleanup completed.", tag));
            }
            foreach (var item in listeningData) {
                var thread = (Thread)item.Item4;
                var tag = (String)item.Item5;
                Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "TinyMessage: {0} Thread cleanup.", tag));

                while (thread.IsAlive) { Task.Delay(100); }
                Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "TinyMessage: {0} Thread cleanup completed.", tag));
            }

#if DEBUG
            sw.Stop();
            Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "TinyMessage: Cleanup completed."));
#endif
        }


        private bool HasBegunListening = false; //if listening setup is in progress

        /// <summary>
        /// Gets whether TinyMessage is in listening state.
        /// </summary>
        public Boolean IsListening { get { return (this.ListeningData != null); } }

        #endregion


        #region Receiving

        private IEnumerable<Tuple<IPEndPoint, Socket, IList<Object>, Thread, String>> ListeningData;
        private ManualResetEvent ListenCancelEvent = null;

        private void Run(object argument) {
            var cancelEvent = this.ListenCancelEvent;
            if (cancelEvent == null) { return; } //if cancel event has already been disposed, there must have been some exception

            var data = (Tuple<IPEndPoint, Socket, IList<Object>, Thread, String>)argument;
            var endpoint = (IPEndPoint)data.Item1;
            var socket = (Socket)data.Item2;
            var tag = (String)data.Item5;

            var buffer = new byte[16384];
            EndPoint remoteEP;
            int inCount;

            Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "TinyMessage: {0} Running.", tag));

            try {
                while (!cancelEvent.WaitOne(0, false)) {
                    try {
                        var remoteAddress = (socket.AddressFamily == AddressFamily.InterNetworkV6) ? IPAddress.IPv6None : IPAddress.None;
                        remoteEP = new IPEndPoint(remoteAddress, 0);
                        Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "TinyMessage: {0} Waiting for packet.", tag));
                        inCount = socket.ReceiveFrom(buffer, ref remoteEP);
                        Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "TinyMessage: {0} Received packet from {1}.", tag, remoteEP));
                    } catch (SocketException ex) {
                        if (ex.SocketErrorCode == SocketError.Interrupted) {
                            Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "TinyMessage: {0} Socket interrupted!", tag));
                            break;
                        } else {
                            Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "TinyMessage: {0} {1} exception: {2}", tag, ex.GetType().Name, ex.Message));
                            throw;
                        }
                    }

                    var receivedHandler = this.PacketReceived;
                    if (receivedHandler != null) {
                        var productFilter = this.ProductFilter;
                        var operationFilter = this.OperationFilter;
                        var remoteEndpoint = remoteEP as IPEndPoint;

                        var newBuffer = new byte[inCount];
                        Buffer.BlockCopy(buffer, 0, newBuffer, 0, inCount);
                        var key = this.Key;
                        Task.Run(() => {
                            var packet = GetUniquePacket(tag, remoteEndpoint, newBuffer, key);
                            if (packet == null) { return; }

                            if ((productFilter != null) && !packet.Product.Equals(productFilter, StringComparison.Ordinal)) {
                                Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "TinyMessage: {0} Ignoring packet {1}/{2} from {3} due to Product filter.", tag, packet.Product, packet.Operation, remoteEndpoint));
                                return;
                            }
                            if ((operationFilter != null) && !packet.Operation.Equals(operationFilter, StringComparison.Ordinal)) {
                                Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "TinyMessage: {0} Ignoring packet {1}/{2} from {3} due to Operation filter.", tag, packet.Product, packet.Operation, remoteEndpoint));
                                return;
                            }

                            var invokeArgs = new object[] { this, new TinyPacketEventArgs(packet, new IPEndPoint(endpoint.Address, endpoint.Port), remoteEndpoint) };
                            Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "TinyMessage: {0} Raising event for packet {1}/{2} from {3}.", tag, packet.Product, packet.Operation, remoteEndpoint));
                            foreach (Delegate iDelegate in receivedHandler.GetInvocationList()) {
                                ISynchronizeInvoke syncer = iDelegate.Target as ISynchronizeInvoke;
                                if (syncer == null) {
                                    try {
                                        iDelegate.DynamicInvoke(invokeArgs);
                                    } catch (MemberAccessException) { } catch (TargetInvocationException) { }
                                } else {
                                    try {
                                        syncer.BeginInvoke(iDelegate, invokeArgs);
                                    } catch (InvalidOperationException) { }
                                }
                            }
                        });
                    }
                }

            } catch (ThreadAbortException) {
                Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "TinyMessage: {0} Thread aborted!", tag));
            }

            Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "TinyMessage: {0} Thread finished.", tag));
        }


        #region DuplicateDetection

        private const Int32 DuplicateBufferCount = 1024;
        private readonly Guid[] DuplicateBuffer = new Guid[DuplicateBufferCount];
        private readonly Int64[] DuplicateBufferExpiry = new Int64[DuplicateBufferCount];
        private Int32 DuplicateBufferIndex = 0;
        private Object DuplicateBufferLock = new object();

        private TinyPacket GetUniquePacket(string tag, IPEndPoint remoteEndpoint, byte[] packetBuffer, byte[] key) { //returns null if there is an duplicate or error parsing.
            TinyPacket packet = null;
            var iv = TinyPacket.ParseIV(packetBuffer, 0, packetBuffer.Length);

            Guid uniqueId;
            if (iv != null) { //encrypted packet, use IV

                uniqueId = new Guid(iv); //use full IV as unique ID

            } else { //not encrypted, try using ID and host

                try {
                    packet = TinyPacket.Parse(packetBuffer, key);
                } catch (FormatException ex) {
                    Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "TinyMessage: {0} Couldn't parse the packet from {1}: {2}", tag, remoteEndpoint, ex.Message));
                    return null;
                }

                int id;
                if (int.TryParse(packet[".Id"], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out id)) {
                    var host = packet[".Host"];
                    if (host != null) {
                        var hostBytes = Encoding.UTF8.GetBytes(host);
                        short b = (hostBytes.Length >= 2) ? (short)(((int)hostBytes[1] << 8) | hostBytes[0]) : (hostBytes.Length >= 1) ? hostBytes[0] : (byte)0;
                        short c = (hostBytes.Length >= 4) ? (short)(((int)hostBytes[3] << 8) | hostBytes[2]) : (hostBytes.Length >= 3) ? hostBytes[2] : (byte)0;
                        byte d = (hostBytes.Length >= 5) ? hostBytes[4] : (byte)0;
                        byte e = (hostBytes.Length >= 6) ? hostBytes[5] : (byte)0;
                        byte f = (hostBytes.Length >= 7) ? hostBytes[6] : (byte)0;
                        byte g = (hostBytes.Length >= 8) ? hostBytes[7] : (byte)0;
                        byte h = (hostBytes.Length >= 9) ? hostBytes[8] : (byte)0;
                        byte i = (hostBytes.Length >= 10) ? hostBytes[9] : (byte)0;
                        byte j = (hostBytes.Length > 12) ? hostBytes[hostBytes.Length - 2] : (hostBytes.Length >= 11) ? hostBytes[10] : (byte)0; //if longer than 12 characters, take last two characters
                        byte k = (hostBytes.Length > 12) ? hostBytes[hostBytes.Length - 1] : (hostBytes.Length >= 12) ? hostBytes[11] : (byte)0;
                        uniqueId = new Guid(id, b, c, d, e, f, g, h, i, j, k);
                    } else {
                        uniqueId = new Guid(id, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0); //just use ID if there is no host
                    }
                } else {
                    return packet; //cannot parse ID - return packet but no duplicate check
                }
            }

            var nowTicks = DateTime.UtcNow.Ticks;
            lock (this.DuplicateBufferLock) {
                for (var i = 0; i < DuplicateBufferCount; i++) {
                    if ((DuplicateBufferExpiry[i] > nowTicks) && (DuplicateBuffer[i] == uniqueId)) { //must be non-expired and duplicate
                        Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "TinyMessage: {0} Ignoring duplicate packet from {1}.", tag, remoteEndpoint));
                        return null;
                    }
                }
                this.DuplicateBuffer[this.DuplicateBufferIndex] = uniqueId;
                this.DuplicateBufferExpiry[this.DuplicateBufferIndex] = nowTicks + TimeSpan.TicksPerSecond; //expires in a second
                this.DuplicateBufferIndex = (this.DuplicateBufferIndex + 1) % TinyMessage.DuplicateBufferCount;
            }

            try {
                if (packet == null) { packet = TinyPacket.Parse(packetBuffer, key); } //parse packet if it wasn't parsed before (i.e. only IV was parsed)
            } catch (FormatException ex) {
                Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "TinyMessage: {0} Couldn't parse the packet from {1}: {2}", tag, remoteEndpoint, ex.Message));
                return null;
            }

            return packet;
        }

        #endregion

        #endregion


        #region Send

        /// <summary>
        /// Sends packet to first IPv6 multicast address defined in the LocalEndpoints or to default multicast address if none is defined.
        /// AES encryption will be used if Key has been defined.
        /// </summary>
        /// <param name="packet">Packet.</param>
        /// <exception cref="System.ArgumentNullException">Packet cannot be null.</exception>
        public void Send(TinyPacket packet) {
            if (packet == null) { throw new ArgumentNullException("packet", "Packet cannot be null."); }

#if DEBUG
            var sw = Stopwatch.StartNew();
#endif

            this.Send(packet, this.Key);

#if DEBUG
            sw.Stop();
            Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "TinyMessage: Send completed in {0} milliseconds.", sw.ElapsedMilliseconds));
#endif
        }

        /// <summary>
        /// Sends packet to all multicast address defined in the LocalEndpoints or to default multicast address if none is defined.
        /// </summary>
        /// <param name="packet">Packet.</param>
        /// <param name="key">128-bit AES encryption key.</param>
        /// <exception cref="System.ArgumentNullException">Packet cannot be null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Key must be 16 bytes (128 bits) in length.</exception>
        public void Send(TinyPacket packet, byte[] key) {
            if (packet == null) { throw new ArgumentNullException("packet", "Packet cannot be null."); }
            if ((key != null) && (key.Length != 16)) { throw new ArgumentOutOfRangeException("key", "Key must be 16 bytes (128 bits) in length."); }

#if DEBUG
            var sw = Stopwatch.StartNew();
#endif

            var sentToAny = false;
            var packetBytes = packet.GetBytes(this.Key);
            foreach (var endpoint in this.PrivateLocalEndpoints) {
                if ((endpoint.AddressFamily == AddressFamily.InterNetworkV6) && endpoint.Address.IsIPv6Multicast) {
                    try {
                        this.SendPacket(packet, packetBytes, endpoint);
                        sentToAny = true;
                    } catch (SocketException) { }
                } else if ((endpoint.AddressFamily == AddressFamily.InterNetwork) && ((endpoint.Address.GetAddressBytes()[0] & 0xE0) == 0xE0)) {
                    try {
                        this.SendPacket(packet, packetBytes, endpoint);
                        sentToAny = true;
                    } catch (SocketException) { }
                }
            }
            if (!sentToAny) {
                try {
                    this.SendPacket(packet, packetBytes, TinyMessage.DefaultIPv6MulticastEndpoint);
                } catch (SocketException) {
                    Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "TinyMessage: Sending broadcast due to failure in multicast for {0}/{1}.", packet.Product, packet.Operation));
                    this.SendPacket(packet, packetBytes, TinyMessage.DefaultIPv4BroadcastEndpoint);
                }
            }

#if DEBUG
            sw.Stop();
            Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "TinyMessage: Send completed in {0} milliseconds.", sw.ElapsedMilliseconds));
#endif
        }

        /// <summary>
        /// Sends packet.
        /// </summary>
        /// <param name="packet">Packet.</param>
        /// <param name="remoteEndpoint">Remote endpoint.</param>
        /// <exception cref="System.ArgumentNullException">Packet cannot be null. -or- Remote endpoint cannot be null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Only IPv4 and IPv6 endpoints are supported.</exception>
        public void Send(TinyPacket packet, IPEndPoint remoteEndpoint) {
            if (packet == null) { throw new ArgumentNullException("packet", "Packet cannot be null."); }
            if (remoteEndpoint == null) { throw new ArgumentNullException("remoteEndpoint", "Remote endpoint cannot be null."); }
            if ((remoteEndpoint.AddressFamily != AddressFamily.InterNetwork) && (remoteEndpoint.AddressFamily != AddressFamily.InterNetworkV6)) { throw new ArgumentOutOfRangeException("remoteEndpoint", "Only IPv4 and IPv6 endpoints are supported."); }

#if DEBUG
            var sw = Stopwatch.StartNew();
#endif

            this.Send(packet, remoteEndpoint, this.Key);

#if DEBUG
            sw.Stop();
            Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "TinyMessage: Send completed in {0} milliseconds.", sw.ElapsedMilliseconds));
#endif
        }

        /// <summary>
        /// Sends packet.
        /// </summary>
        /// <param name="packet">Packet.</param>
        /// <param name="remoteEndpoint">Remote endpoint.</param>
        /// <param name="key">AES encryption key.</param>
        /// <exception cref="System.ArgumentNullException">Packet cannot be null. -or- Remote endpoint cannot be null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Only IPv4 and IPv6 endpoints are supported. -or- Key must be 16 bytes (128 bits) in length.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Not appropriate since other Send functions do require instance.")]
        public void Send(TinyPacket packet, IPEndPoint remoteEndpoint, byte[] key) {
            if (packet == null) { throw new ArgumentNullException("packet", "Packet cannot be null."); }
            if (remoteEndpoint == null) { throw new ArgumentNullException("remoteEndpoint", "Remote endpoint cannot be null."); }
            if ((remoteEndpoint.AddressFamily != AddressFamily.InterNetwork) && (remoteEndpoint.AddressFamily != AddressFamily.InterNetworkV6)) { throw new ArgumentOutOfRangeException("remoteEndpoint", "Only IPv4 and IPv6 endpoints are supported."); }
            if ((key != null) && (key.Length != 16)) { throw new ArgumentOutOfRangeException("key", "Key must be 16 bytes (128 bits) in length."); }

#if DEBUG
            var sw = Stopwatch.StartNew();
#endif

            this.SendPacket(packet, packet.GetBytes(key), remoteEndpoint);

#if DEBUG
            sw.Stop();
            Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "TinyMessage: Send completed in {0} milliseconds.", sw.ElapsedMilliseconds));
#endif
        }


        /// <summary>
        /// Sends packet to first IPv6 multicast address defined in the LocalEndpoints or to default multicast address if none is defined.
        /// AES encryption will be used if Key has been defined.
        /// </summary>
        /// <param name="packet">Packet.</param>
        /// <exception cref="System.ArgumentNullException">Packet cannot be null.</exception>
        public async Task SendAsync(TinyPacket packet) {
            if (packet == null) { throw new ArgumentNullException("packet", "Packet cannot be null."); }

            await SendAsync(packet, this.Key);
        }

        /// <summary>
        /// Sends packet to all multicast address defined in the LocalEndpoints or to default multicast address if none is defined.
        /// </summary>
        /// <param name="packet">Packet.</param>
        /// <param name="key">128-bit AES encryption key.</param>
        /// <exception cref="System.ArgumentNullException">Packet cannot be null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Key must be 16 bytes (128 bits) in length.</exception>
        public async Task SendAsync(TinyPacket packet, byte[] key) {
            if (packet == null) { throw new ArgumentNullException("packet", "Packet cannot be null."); }
            if ((key != null) && (key.Length != 16)) { throw new ArgumentOutOfRangeException("key", "Key must be 16 bytes (128 bits) in length."); }

            await Task.Run(() => {
                this.Send(packet, key);
            });
        }

        /// <summary>
        /// Sends packet.
        /// </summary>
        /// <param name="packet">Packet.</param>
        /// <param name="remoteEndpoint">Remote endpoint.</param>
        /// <exception cref="System.ArgumentNullException">Packet cannot be null. -or- Remote endpoint cannot be null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Only IPv4 and IPv6 endpoints are supported.</exception>
        public async Task SendAsync(TinyPacket packet, IPEndPoint remoteEndpoint) {
            if (packet == null) { throw new ArgumentNullException("packet", "Packet cannot be null."); }
            if (remoteEndpoint == null) { throw new ArgumentNullException("remoteEndpoint", "Remote endpoint cannot be null."); }
            if ((remoteEndpoint.AddressFamily != AddressFamily.InterNetwork) && (remoteEndpoint.AddressFamily != AddressFamily.InterNetworkV6)) { throw new ArgumentOutOfRangeException("remoteEndpoint", "Only IPv4 and IPv6 endpoints are supported."); }

            await Task.Run(() => {
                this.Send(packet, remoteEndpoint);
            });
        }

        /// <summary>
        /// Sends packet.
        /// </summary>
        /// <param name="packet">Packet.</param>
        /// <param name="remoteEndpoint">Remote endpoint.</param>
        /// <param name="key">AES encryption key.</param>
        /// <exception cref="System.ArgumentNullException">Packet cannot be null. -or- Remote endpoint cannot be null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Only IPv4 and IPv6 endpoints are supported. -or- Key must be 16 bytes (128 bits) in length.</exception>
        public async Task SendAsync(TinyPacket packet, IPEndPoint remoteEndpoint, byte[] key) {
            if (packet == null) { throw new ArgumentNullException("packet", "Packet cannot be null."); }
            if (remoteEndpoint == null) { throw new ArgumentNullException("remoteEndpoint", "Remote endpoint cannot be null."); }
            if ((remoteEndpoint.AddressFamily != AddressFamily.InterNetwork) && (remoteEndpoint.AddressFamily != AddressFamily.InterNetworkV6)) { throw new ArgumentOutOfRangeException("remoteEndpoint", "Only IPv4 and IPv6 endpoints are supported."); }
            if ((key != null) && (key.Length != 16)) { throw new ArgumentOutOfRangeException("key", "Key must be 16 bytes (128 bits) in length."); }

            await Task.Run(() => {
                this.Send(packet, remoteEndpoint, key);
            });
        }


        private void SendPacket(TinyPacket packet, byte[] packetBytes, IPEndPoint remoteEndpoint) {
#if DEBUG
            var sw = Stopwatch.StartNew();
#endif

            if (remoteEndpoint.Address.Equals(IPAddress.Broadcast)) {

                var totalAvailable = 0;
                var totalFailed = 0;
                List<Exception> exceptions = null;
                foreach (var netInterface in this.GetApplicableNetworkInterfaces(remoteEndpoint.AddressFamily)) {
                    foreach (var unicast in netInterface.GetIPProperties().UnicastAddresses) {
                        if (unicast.Address.AddressFamily != AddressFamily.InterNetwork) { continue; }
                        totalAvailable += 1;
                        var addressInt = BitConverter.ToInt32(unicast.Address.GetAddressBytes(), 0);
                        var maskInt = BitConverter.ToInt32(unicast.IPv4Mask.GetAddressBytes(), 0);
                        var broadcastAddress = new IPAddress(BitConverter.GetBytes(addressInt | ~maskInt));
                        var broadcastEndpoint = new IPEndPoint(broadcastAddress, remoteEndpoint.Port);
                        var localEndpoint = new IPEndPoint(unicast.Address, remoteEndpoint.Port);
                        try {
                            using (var socket = new Socket(remoteEndpoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp)) {
                                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                                socket.Bind(localEndpoint);
                                SendPacketViaSocket(packet, packetBytes, socket, broadcastEndpoint, localEndpoint);
                            }
                        } catch (SocketException ex) { //ignore send errors in case of broadcast
                            totalFailed += 1;
                            if (exceptions == null) { exceptions = new List<Exception>(); } //allocate only if used
                            exceptions.Add(ex);
                            Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "TinyMessage: Exception while sending to {0}: {1}", broadcastEndpoint, ex.Message));
                        }
                    }
                    if ((totalAvailable == totalFailed) && (exceptions != null)) { //we tried to send but couldn't get it through a single interface
                        throw new AggregateException(exceptions[0].Message, exceptions);
                    }
                }

            } else if (remoteEndpoint.Address.IsIPv6Multicast) {

                foreach (var unicast in GetApplicableAddressInformation(remoteEndpoint.AddressFamily)) {
                    using (var socket = new Socket(remoteEndpoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp)) {
                        socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                        var localEndpoint = new IPEndPoint(unicast.Address, 0);
                        socket.Bind(localEndpoint);
                        SendPacketViaSocket(packet, packetBytes, socket, remoteEndpoint, localEndpoint);
                    }
                }

            } else if ((remoteEndpoint.Address.AddressFamily == AddressFamily.InterNetwork) && ((remoteEndpoint.Address.GetAddressBytes()[0] & 0xE0) == 0xE0)) { //IPv4 multicast

                foreach (var unicast in GetApplicableAddressInformation(remoteEndpoint.AddressFamily)) {
                    using (var socket = new Socket(remoteEndpoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp)) {
                        socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                        var localEndpoint = new IPEndPoint(unicast.Address, 0);
                        socket.Bind(localEndpoint);
                        SendPacketViaSocket(packet, packetBytes, socket, remoteEndpoint, localEndpoint);
                    }
                }

            } else { //not a broadcast

                using (var socket = new Socket(remoteEndpoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp)) {
                    SendPacketViaSocket(packet, packetBytes, socket, remoteEndpoint, null);
                }

            }

#if DEBUG
            sw.Stop();
            Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "TinyMessage: SendPacket completed in {0} milliseconds.", sw.ElapsedMilliseconds));
#endif
        }

        private static void SendPacketViaSocket(TinyPacket packet, byte[] packetBytes, Socket socket, IPEndPoint remoteEndpoint, IPEndPoint localEndpoint) {
#if DEBUG
            var sw = Stopwatch.StartNew();
#endif
            if (IsRunningOnMono == false) { socket.SetSocketOption(SocketOptionLevel.Udp, SocketOptionName.NoChecksum, false); }
            try {
                socket.SendTo(packetBytes, remoteEndpoint);
            } catch (SocketException ex) {
                Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "TinyMessage: Exception sending {0}/{1} to {2} (from {3}): {4}", packet.Product, packet.Operation, remoteEndpoint, localEndpoint, ex.Message));
                throw;
            }
#if DEBUG
            sw.Stop();
            Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "TinyMessage: SendPacketViaSocket completed for {0}/{1} to {2} (from {3}) in {4} milliseconds.", packet.Product, packet.Operation, remoteEndpoint, localEndpoint, sw.ElapsedMilliseconds));
#endif
        }

        #endregion


        #region Static

        /// <summary>
        /// Default port for TinyMessage protocol.
        /// </summary>
        public static Int32 DefaultPort { get { return 5104; } }


        private static readonly IPEndPoint _defaultIPv4BroadcastEndpoint = new IPEndPoint(IPAddress.Broadcast, TinyMessage.DefaultPort);
        /// <summary>
        /// Default IPv4 broadcast endpoint for TinyMessage protocol.
        /// </summary>
        public static IPEndPoint DefaultIPv4BroadcastEndpoint { get { return _defaultIPv4BroadcastEndpoint; } }


        private static readonly IPEndPoint _defaultIPv6MulticastEndpoint = new IPEndPoint(IPAddress.Parse("ff08:0::152"), TinyMessage.DefaultPort);
        /// <summary>
        /// Default IPv6 multicast endpoint for TinyMessage protocol.
        /// </summary>
        public static IPEndPoint DefaultIPv6MulticastEndpoint { get { return _defaultIPv6MulticastEndpoint; } }


        private static readonly IPEndPoint _defaultIPv4MulticastEndpoint = new IPEndPoint(IPAddress.Parse("239.192.111.17"), TinyMessage.DefaultPort);
        /// <summary>
        /// Default IPv4 multicast endpoint for TinyMessage protocol.
        /// </summary>
        public static IPEndPoint DefaultIPv4MulticastEndpoint { get { return _defaultIPv4MulticastEndpoint; } }



        private readonly List<NetworkInterface> CacheNetworkInterfacesIPv4 = new List<NetworkInterface>();
        private readonly List<NetworkInterface> CacheNetworkInterfacesIPv6 = new List<NetworkInterface>();
        private DateTime CacheNetworkInterfacesExpiry = DateTime.UtcNow;

        private IEnumerable<NetworkInterface> GetApplicableNetworkInterfaces(AddressFamily family) {
            var networkInterfaceCache = (family == AddressFamily.InterNetworkV6) ? this.CacheNetworkInterfacesIPv6 : this.CacheNetworkInterfacesIPv4;
            if ((networkInterfaceCache.Count == 0) || (DateTime.UtcNow > this.CacheNetworkInterfacesExpiry)) {
                var familyComponent = (family == AddressFamily.InterNetworkV6) ? NetworkInterfaceComponent.IPv6 : NetworkInterfaceComponent.IPv4;
                foreach (var netInterface in NetworkInterface.GetAllNetworkInterfaces()) {
                    if (netInterface.OperationalStatus != OperationalStatus.Up) { continue; }
                    if (netInterface.NetworkInterfaceType == NetworkInterfaceType.Loopback) { continue; }
                    if (netInterface.NetworkInterfaceType == NetworkInterfaceType.Tunnel) { continue; }
                    if (netInterface.IsReceiveOnly) { continue; }
                    if (!netInterface.Supports(familyComponent)) { continue; }
                    networkInterfaceCache.Add(netInterface);
                }
                this.CacheNetworkInterfacesExpiry = DateTime.UtcNow.AddMinutes(5); //refresh every N minutes
            }
            foreach (var netInterface in networkInterfaceCache) {
                yield return netInterface;
            }
        }

        private IEnumerable<UnicastIPAddressInformation> GetApplicableAddressInformation(AddressFamily family) {
            foreach (var netInterface in this.GetApplicableNetworkInterfaces(family)) {
                foreach (var unicast in netInterface.GetIPProperties().UnicastAddresses) {
                    if (unicast.Address.AddressFamily == family) {
                        yield return unicast;
                    }
                }
            }
        }

        #endregion


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
                this.Close();
            }
        }

        #endregion

        private static Boolean IsRunningOnMono { get { return (Type.GetType("Mono.Runtime") != null); } }

    }



    /// <summary>
    /// Encoder/decoder for Tiny packets.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    [DebuggerDisplay(@"{Product + ""/"" + Operation + "" - "" + Items.Count + "" items""}")]
    public class TinyPacket : IDisposable, IEnumerable<KeyValuePair<String, String>> {

        /// <summary>
        /// Creates new instance
        /// </summary>  
        /// <param name="product">Name of product. Preferred format would be application name, at (@) sign, IANA assigned Private Enterprise Number. E.g. Application@12345</param>
        /// <param name="operation">Message type.</param>
        /// <exception cref="System.ArgumentNullException">Product is null or empty. -or- Operation is null or empty.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Product/Operation length cannot be more than 32 characters. -or- Product/Operation must contain only letters and digits.</exception>
        public TinyPacket(String product, String operation) {
            if (string.IsNullOrEmpty(product)) { throw new ArgumentNullException("product", "Product is null or empty."); }
            if (product.Length > 32) { throw new ArgumentOutOfRangeException("product", "Product length cannot be more than 32 characters."); }
            foreach (var ch in product) { if (!char.IsLetterOrDigit(ch)) { throw new ArgumentOutOfRangeException("product", "Product must contain only letters and digits"); } }
            if (string.IsNullOrEmpty(operation)) { throw new ArgumentNullException("operation", "Operation is null or empty."); }
            if (operation.Length > 32) { throw new ArgumentOutOfRangeException("operation", "Operation length cannot be more than 32 characters."); }
            foreach (var ch in operation) { if (!char.IsLetterOrDigit(ch)) { throw new ArgumentOutOfRangeException("operation", "Operation must contain only letters and digits"); } }

            this.Product = product;
            this.Operation = operation;
            this.Items = new Dictionary<string, string>();
        }


        private TinyPacket(String product, String operation, IDictionary<String, String> items, byte[] iv) {
            this.Product = product;
            this.Operation = operation;
            this.Items = items;
            this.WasParsed = true;
            this.AesIV = iv;
        }


        /// <summary>
        /// Gets name of product.
        /// </summary>
        public String Product { get; private set; }

        /// <summary>
        /// Gets operation.
        /// </summary>
        public String Operation { get; private set; }


        /// <summary>
        /// Gets whether packet was created based on encrypted data.
        /// Data is only valid for parsed packets.
        /// Non-parsed packets will always return False.
        /// </summary>
        public Boolean WasEncrypted {
            get { return (this.AesIV != null); }
        }

        /// <summary>
        /// Returns IV that was used in encrypted packet.
        /// Data is only valid for parsed packets.
        /// Non-parsed packets will always return null.
        /// </summary>
        public byte[] GetIV() {
            if (this.AesIV == null) { return null; }
            var bytes = new byte[this.AesIV.Length];
            Buffer.BlockCopy(this.AesIV, 0, bytes, 0, bytes.Length);
            return bytes;
        }


        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        private readonly IDictionary<string, string> Items;

        private readonly byte[] AesIV;
        private readonly bool WasParsed;

        private static readonly string HostName = Dns.GetHostName();


        /// <summary>
        /// Adds the specified key and value to the dictionary.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add.</param>
        /// <exception cref="System.ArgumentNullException">Key is null or empty.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Key length cannot be more than 32 characters. -or- Key must contain only letters, digits, and underscore.</exception>
        public void Add(string key, string value) {
            if (this.WasParsed) { throw new NotSupportedException("Data is read-only."); }

            if (string.IsNullOrEmpty(key)) { throw new ArgumentNullException("key", "Key is null or empty."); }
            if (key.Length > 32) { throw new ArgumentOutOfRangeException("key", "Key length cannot be more than 32 characters."); }
            foreach (var ch in key) { if (!char.IsLetterOrDigit(ch) && (ch != '_')) { throw new ArgumentOutOfRangeException("key", "Key must contain only letters, digits, and underscore."); } }

            this.Items.Add(key, value);
        }

        /// <summary>
        /// Gets/sets data item.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <exception cref="System.NotSupportedException">Data is read-only.</exception>
        /// <exception cref="System.ArgumentNullException">Key is null or empty.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Key length cannot be more than 32 characters. -or- Key must contain only letters, digits, and underscore.</exception>
        public String this[String key] {
            get {
                if (this.Items == null) { return null; }
                if (this.Items.ContainsKey(key)) {
                    return this.Items[key];
                } else {
                    return null;
                }
            }
            set {
                if (this.WasParsed) { throw new NotSupportedException("Data is read-only."); }

                if (string.IsNullOrEmpty(key)) { throw new ArgumentNullException("key", "Key is null or empty."); }
                if (key.Length > 32) { throw new ArgumentOutOfRangeException("key", "Key length cannot be more than 32 characters."); }
                foreach (var ch in key) { if (!char.IsLetterOrDigit(ch) && (ch != '_')) { throw new ArgumentOutOfRangeException("key", "Key must contain only letters, digits, and underscore."); } }

                if (this.Items.ContainsKey(key)) {
                    this.Items[key] = value;
                } else {
                    this.Items.Add(key, value);
                }
            }
        }


        #region GetBytes

        /// <summary>
        /// Converts message to its byte representation.
        /// Notice that .Id and .Host entries will be added in non-encrypted packets in order to facilitate duplicate discovery.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Packet length exceeds 65507 bytes.</exception>
        public Byte[] GetBytes() {
            return this.GetBytes(null, omitIdentifiers: false);
        }

        /// <summary>
        /// Converts message to its bytes byte representation. If key is not null, content will be encrypted using 128-bit AES in CBC mode with a SHA-256 hash.
        /// Notice that .Id and .Host entries will be added in non-encrypted packets in order to facilitate duplicate discovery.
        /// </summary>
        /// <param name="key">AES 128-bit key.</param>
        /// <exception cref="System.InvalidOperationException">Packet length exceeds 65507 bytes.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Key must be 16 bytes (128 bits) in length.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "Nested streams in using statement are safe to dispose multiple times.")]
        public Byte[] GetBytes(Byte[] key) {
            return GetBytes(key, omitIdentifiers: false);
        }

        /// <summary>
        /// Converts message to its bytes byte representation. If key is not null, content will be encrypted using 128-bit AES in CBC mode with a SHA-256 hash.
        /// Notice that .Id and .Host entries will be added in non-encrypted packets in order to facilitate duplicate discovery.
        /// </summary>
        /// <param name="key">AES 128-bit key.</param>
        /// <param name="omitIdentifiers">If true, .Id and .Host are going to be omitted.</param>
        /// <exception cref="System.InvalidOperationException">Packet length exceeds 65507 bytes.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Key must be 16 bytes (128 bits) in length.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "Nested streams in using statement are safe to dispose multiple times.")]
        public Byte[] GetBytes(Byte[] key, Boolean omitIdentifiers) {
#if DEBUG
            var sw = Stopwatch.StartNew();
#endif

            if (key == null) {
                using (var stream = new MemoryStream(DefaultStreamCapacity)) {
                    this.EncodeToStream(stream, omitIdentifiers, isEncrypted: false);
                    if (stream.Position > 65507) { throw new InvalidOperationException("Packet length exceeds 65507 bytes."); }

#if DEBUG
                    sw.Stop();
                    Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "TinyPacket: GetBytes completed in {0} milliseconds.", sw.ElapsedMilliseconds));
#endif
                    return stream.ToArray();
                }
            }


            if (key.Length != 16) { throw new ArgumentOutOfRangeException("key", "Key must be 16 bytes (128 bits) in length."); }

            var aes = Aes128Cbc.Value;
            var sha = Sha256.Value;
            var random = Random.Value;

            var iv = new byte[16];
            random.GetBytes(iv);

            using (var stream = new MemoryStream(DefaultStreamCapacity)) {
                using (var encryptor = aes.CreateEncryptor(key, iv))
                using (var cryptoStream = new CryptoStream(stream, encryptor, CryptoStreamMode.Write)) {
                    stream.Write(BytesHeaderTiny128, 0, BytesHeaderTiny128.Length);
                    stream.Write(iv, 0, iv.Length);

                    this.EncodeToStream(cryptoStream, omitIdentifiers, isEncrypted: true);
                    cryptoStream.FlushFinalBlock();

                    stream.Position = 0;
                    stream.Write(sha.ComputeHash(stream), 0, 32);

                    if (stream.Length > 65507) { throw new InvalidOperationException("Packet length exceeds 65507 bytes."); }

#if DEBUG
                    sw.Stop();
                    Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "TinyPacket: GetBytes completed encryption in {0} milliseconds.", sw.ElapsedMilliseconds));
#endif
                    return stream.ToArray();
                }
            }
        }


        private void EncodeToStream(Stream stream, bool omitIdentifiers, bool isEncrypted) {
            var encoding = Utf8Encoding.Value;

            stream.Write(BytesHeaderTiny, 0, BytesHeaderTiny.Length);

            var productBytes = encoding.GetBytes(this.Product);
            stream.Write(productBytes, 0, productBytes.Length);
            stream.WriteByte(0x20);

            var operationBytes = encoding.GetBytes(this.Operation);
            stream.Write(operationBytes, 0, operationBytes.Length);
            stream.WriteByte(0x20);

            var addComma = false;
            if (this.Items != null) {
                stream.WriteByte(0x7B); //{
                if (!omitIdentifiers) {
                    if (!isEncrypted && !this.Items.ContainsKey(".Id")) { //IV can be ID for encrypted packets
                        var idBytes = new byte[4];
                        TinyPacket.Random.Value.GetBytes(idBytes);
                        stream.WriteByte(0x22); //"
                        WriteText(stream, ".Id");
                        stream.WriteByte(0x22); //"
                        stream.WriteByte(0x3A); //:
                        stream.WriteByte(0x22); //"
                        WriteText(stream, BitConverter.ToInt32(idBytes, 0).ToString("X8", CultureInfo.InvariantCulture));
                        stream.WriteByte(0x22); //"
                        addComma = true;
                    }
                    if (!this.Items.ContainsKey(".Host")) {
                        if (addComma) { stream.WriteByte(0x2C); } //,
                        stream.WriteByte(0x22); //"
                        WriteText(stream, ".Host");
                        stream.WriteByte(0x22); //"
                        stream.WriteByte(0x3A); //:
                        stream.WriteByte(0x22); //"
                        WriteText(stream, TinyPacket.HostName);
                        stream.WriteByte(0x22); //"
                        addComma = true;
                    }
                }
                foreach (var item in this.Items) {
                    if (addComma) { stream.WriteByte(0x2C); } //,
                    stream.WriteByte(0x22); //"
                    WriteText(stream, item.Key);
                    stream.WriteByte(0x22); //"
                    stream.WriteByte(0x3A); //:
                    if (item.Value != null) {
                        stream.WriteByte(0x22); //"
                        WriteText(stream, item.Value);
                        stream.WriteByte(0x22); //"
                    } else {
                        stream.Write(BytesNull, 0, BytesNull.Length); //null
                    }
                    addComma = true;
                }
                stream.WriteByte(0x7D); //}
            } else {
                stream.Write(BytesNull, 0, BytesNull.Length); //null
            }
        }

        private static void WriteText(Stream stream, string value) {
            var encoding = Utf8Encoding.Value;

            foreach (char ch in value) {
                switch (ch) {
                    case '\"': stream.WriteByte(0x5C); stream.WriteByte(0x22); break; // \"
                    case '\\': stream.WriteByte(0x5C); stream.WriteByte(0x5C); break; // \\
                    case '/': stream.WriteByte(0x5C); stream.WriteByte(0x2F); break; // \/
                    case '\b': stream.WriteByte(0x5C); stream.WriteByte(0x62); break; // \b
                    case '\f': stream.WriteByte(0x5C); stream.WriteByte(0x66); break; // \f
                    case '\n': stream.WriteByte(0x5C); stream.WriteByte(0x6E); break; // \n
                    case '\r': stream.WriteByte(0x5C); stream.WriteByte(0x72); break; // \r
                    case '\t': stream.WriteByte(0x5C); stream.WriteByte(0x74); break; // \t
                    default:
                        var charValue = (int)ch;
                        if (char.IsControl(ch)) {
                            var bytes = encoding.GetBytes("\\u" + charValue.ToString("x4", CultureInfo.InvariantCulture));
                            stream.Write(bytes, 0, bytes.Length);
                        } else if ((charValue >= 32) && (charValue <= 127)) {
                            stream.WriteByte((byte)charValue);
                        } else {
                            var bytes = encoding.GetBytes(new char[] { ch }, 0, 1);
                            stream.Write(bytes, 0, bytes.Length);
                        }
                        break;
                }
            }
        }

        #endregion


        #region Parse

        /// <summary>
        /// Returns IV if it is present in packet.
        /// If IV is not present (i.e. packet is not encrypted), null will be returned.
        /// </summary>
        /// <param name="buffer">Byte array.</param>
        /// <param name="offset">Starting offset.</param>
        /// <param name="count">Total lenght.</param>
        /// <exception cref="System.ArgumentNullException">Buffer is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Offset is less than zero. -or- Count is less than zero. -or- The sum of offset and count is greater than the length of buffer.</exception>
        /// <exception cref="System.FormatException">Cannot parse packet.</exception>
        internal static byte[] ParseIV(Byte[] buffer, Int32 offset, Int32 count) {
            if (buffer == null) { throw new ArgumentNullException("buffer", "Buffer is null."); }
            if (offset < 0) { throw new ArgumentOutOfRangeException("offset", "Index is less than zero."); }
            if (count < 0) { throw new ArgumentOutOfRangeException("count", "Count is less than zero."); }
            if (offset + count > buffer.Length) { throw new ArgumentOutOfRangeException("count", "The sum of offset and count is greater than the length of buffer."); }

            if (IsMatchingPrefix(buffer, offset, BytesHeaderTiny128)) {
                if (count < 56) { return null; } //Invalid encrypted headers.
                var iv = new byte[16];
                Buffer.BlockCopy(buffer, offset + 8, iv, 0, 16);
                return iv;
            }
            return null;
        }

        /// <summary>
        /// Returns parsed packet.
        /// </summary>
        /// <param name="buffer">Byte array.</param>
        /// <exception cref="System.ArgumentNullException">Buffer is null.</exception>
        /// <exception cref="System.FormatException">Cannot parse packet.</exception>
        public static TinyPacket Parse(Byte[] buffer) {
            if (buffer == null) { throw new ArgumentNullException("buffer", "Buffer is null."); }
            return TinyPacket.Parse(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Returns parsed packet.
        /// </summary>
        /// <param name="buffer">Byte array.</param>
        /// <param name="key">Decryption key.</param>
        /// <exception cref="System.ArgumentNullException">Buffer is null.</exception>
        /// <exception cref="System.FormatException">Cannot parse packet.</exception>
        public static TinyPacket Parse(Byte[] buffer, byte[] key) {
            if (buffer == null) { throw new ArgumentNullException("buffer", "Buffer is null."); }
            return TinyPacket.Parse(buffer, 0, buffer.Length, key);
        }

        /// <summary>
        /// Returns parsed packet.
        /// </summary>
        /// <param name="buffer">Byte array.</param>
        /// <param name="offset">Starting offset.</param>
        /// <param name="count">Total lenght.</param>
        /// <exception cref="System.ArgumentNullException">Buffer is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Offset is less than zero. -or- Count is less than zero. -or- The sum of offset and count is greater than the length of buffer.</exception>
        /// <exception cref="System.FormatException">Cannot parse packet.</exception>
        public static TinyPacket Parse(Byte[] buffer, Int32 offset, Int32 count) {
            return TinyPacket.Parse(buffer, offset, count, null);
        }

        /// <summary>
        /// Returns parsed packet.
        /// </summary>
        /// <param name="buffer">Byte array.</param>
        /// <param name="offset">Starting offset.</param>
        /// <param name="count">Total lenght.</param>
        /// <param name="key">Decryption key.</param>
        /// <exception cref="System.ArgumentNullException">Buffer is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Offset is less than zero. -or- Count is less than zero. -or- The sum of offset and count is greater than the length of buffer.</exception>
        /// <exception cref="System.FormatException">Cannot parse packet.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "Nested streams in using statement are safe to dispose multiple times.")]
        public static TinyPacket Parse(Byte[] buffer, Int32 offset, Int32 count, byte[] key) {
            if (buffer == null) { throw new ArgumentNullException("buffer", "Buffer is null."); }
            if (offset < 0) { throw new ArgumentOutOfRangeException("offset", "Index is less than zero."); }
            if (count < 0) { throw new ArgumentOutOfRangeException("count", "Count is less than zero."); }
            if (offset + count > buffer.Length) { throw new ArgumentOutOfRangeException("count", "The sum of offset and count is greater than the length of buffer."); }

            if (IsMatchingPrefix(buffer, offset, BytesHeaderTiny)) {
                using (var stream = new MemoryStream(buffer, offset + 5, count - 5)) {
                    return ParseData(stream, null);
                }
            } else if (IsMatchingPrefix(buffer, offset, BytesHeaderTiny128)) {
                if (key == null) { throw new FormatException("Missing key."); }
                if (count < 56) { throw new FormatException("Invalid encrypted headers."); }

                var sha = Sha256.Value;
                var hash = sha.ComputeHash(buffer, offset, count - 32);
                if (!IsMatchingPrefix(buffer, offset + count - 32, hash)) { throw new FormatException("Invalid hash."); }

                var iv = new byte[16];
                Buffer.BlockCopy(buffer, offset + 8, iv, 0, 16);

                var aes = Aes128Cbc.Value;
                try {
                    using (var stream = new MemoryStream(buffer, offset + 8 + 16, count - 8 - 16 - 32))
                    using (var decryptor = aes.CreateDecryptor(key, iv))
                    using (var cryptoStream = new CryptoStream(stream, decryptor, CryptoStreamMode.Read))
                    using (var decryptedStream = new MemoryStream(DefaultStreamCapacity)) {
                        cryptoStream.CopyTo(decryptedStream);
                        decryptedStream.Position = 0;
                        if (decryptedStream.Length < BytesHeaderTiny.Length) { throw new FormatException("Invalid encrypted header."); }
                        for (int i = 0; i < BytesHeaderTiny.Length; i++) {
                            if (decryptedStream.ReadByte() != BytesHeaderTiny[i]) { throw new FormatException("Invalid encrypted header."); }
                        }
                        return ParseData(decryptedStream, iv);
                    }
                } catch (CryptographicException ex) {
                    throw new FormatException("Cannot decrypt packet.", ex);
                }
            } else {
                throw new System.FormatException("Unknown protocol header.");
            }
        }

        private static TinyPacket ParseData(MemoryStream stream, byte[] iv) {
            var encoding = Utf8Encoding.Value;

            string product = ReadToSpaceOrEnd(stream);
            if (string.IsNullOrEmpty(product)) { throw new System.FormatException("Cannot parse product."); }

            string operation = ReadToSpaceOrEnd(stream);
            if (string.IsNullOrEmpty(operation)) { throw new System.FormatException("Cannot parse operation."); }

            var data = new Dictionary<string, string>();

            var jsonBytes = new byte[stream.Length - stream.Position];
            if (jsonBytes.Length > 0) {
                stream.Read(jsonBytes, 0, jsonBytes.Length);
                var jsonText = new Queue<char>(encoding.GetString(jsonBytes));
                while (true) { //remove whitespace and determine content kind
                    var ch = jsonText.Peek();
                    if (ch == ' ') {
                        jsonText.Dequeue();
                    } else if (ch == '{') {
                        ParseJsonObject(jsonText, data);
                        break;
                    } else {
                        if ((jsonText.Count > 0) && (jsonText.Dequeue() == 'n')) {
                            if ((jsonText.Count > 0) && (jsonText.Dequeue() == 'u')) {
                                if ((jsonText.Count > 0) && (jsonText.Dequeue() == 'l')) {
                                    if ((jsonText.Count > 0) && (jsonText.Dequeue() == 'l')) {
                                        while ((jsonText.Count > 0) && (jsonText.Peek() == ' ')) {
                                            jsonText.Dequeue();
                                        }
                                        if (jsonText.Count == 0) { break; }
                                    }
                                }
                            }
                        }
                        throw new System.FormatException("Cannot determine data kind.");
                    }
                }
            }
            return new TinyPacket(product, operation, data, iv);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Cyclomatic complexity is actually lower than code analysis shows.")]
        private static JsonState ParseJsonObject(Queue<char> jsonText, Dictionary<string, string> data) {
            var state = JsonState.Default;
            var sbName = new StringBuilder();
            var sbValue = new StringBuilder();
            while (jsonText.Count > 0) {
                var ch = jsonText.Dequeue();
                switch (state) {
                    case JsonState.Default: {
                            switch (ch) {
                                case '{': state = JsonState.LookingForNameStart; break;
                                default: throw new System.FormatException("Cannot find item start.");
                            }
                        } break;

                    case JsonState.LookingForNameStart: {
                            switch (ch) {
                                case ' ': break;
                                case '}': state = JsonState.DeadEnd; break; //empty object
                                case '\"': state = JsonState.LookingForNameEnd; break;
                                default: throw new System.FormatException("Cannot find key name start.");
                            }
                        } break;

                    case JsonState.LookingForNameEnd: {
                            switch (ch) {
                                case '\\': sbName.Append(Descape(jsonText)); break;
                                case '\"': state = JsonState.LookingForPairSeparator; break;
                                default: sbName.Append(ch); break;
                            }
                        } break;

                    case JsonState.LookingForPairSeparator: {
                            switch (ch) {
                                case ' ': break;
                                case ':': state = JsonState.LookingForValueStart; break;
                                default: throw new System.FormatException("Cannot find name/value separator.");
                            }
                        } break;

                    case JsonState.LookingForValueStart: {
                            switch (ch) {
                                case ' ': break;
                                case '\"': state = JsonState.LookingForValueEnd; break;
                                case 'n': state = JsonState.LookingForNullChar2; break;
                                default: throw new System.FormatException("Cannot find key value start.");
                            }
                        } break;

                    case JsonState.LookingForValueEnd: {
                            switch (ch) {
                                case '\\': sbValue.Append(Descape(jsonText)); break;
                                case '\"':
                                    var name = sbName.ToString();
                                    var value = sbValue.ToString();
                                    if (data.ContainsKey(name)) {
                                        data[name] = value;
                                    } else {
                                        data.Add(name, value);
                                    }
                                    sbName.Length = 0;
                                    sbValue.Length = 0;
                                    state = JsonState.LookingForObjectEnd;
                                    break;
                                default: sbValue.Append(ch); break;
                            }
                        } break;

                    case JsonState.LookingForNullChar2: {
                            switch (ch) {
                                case 'u': state = JsonState.LookingForNullChar3; break;
                                default: throw new System.FormatException("Cannot find null.");
                            }
                        } break;

                    case JsonState.LookingForNullChar3: {
                            switch (ch) {
                                case 'l': state = JsonState.LookingForNullChar4; break;
                                default: throw new System.FormatException("Cannot find null.");
                            }
                        } break;

                    case JsonState.LookingForNullChar4: {
                            switch (ch) {
                                case 'l':
                                    var name = sbName.ToString();
                                    if (data.ContainsKey(name)) {
                                        data[name] = null;
                                    } else {
                                        data.Add(name, null);
                                    }
                                    sbName.Length = 0;
                                    sbValue.Length = 0;
                                    state = JsonState.LookingForObjectEnd;
                                    break;
                                default: throw new System.FormatException("Cannot find null.");
                            }
                        } break;

                    case JsonState.LookingForObjectEnd: {
                            switch (ch) {
                                case ' ': break;
                                case ',': state = JsonState.LookingForNameStart; break;
                                case '}': state = JsonState.DeadEnd; break;
                                default: throw new System.FormatException("Cannot find item start.");
                            }
                        } break;

                    case JsonState.DeadEnd: {
                            switch (ch) {
                                case ' ': break;
                                default: throw new System.FormatException("Unexpected data.");
                            }
                        } break;

                    default: throw new System.FormatException("Unexpected state.");

                }
            }
            return state;
        }

        private static string Descape(Queue<char> jsonText) {
            var ch = jsonText.Dequeue();
            switch (ch) {
                case '\"': return "\"";
                case '\\': return "\\";
                case '/': return "/";
                case 'b': return System.Convert.ToChar(0x08).ToString();
                case 'f': return System.Convert.ToChar(0x0C).ToString();
                case 'n': return System.Convert.ToChar(0x0A).ToString();
                case 'r': return System.Convert.ToChar(0x0D).ToString();
                case 't': return System.Convert.ToChar(0x09).ToString();
                case 'u':
                    var hex = new string(new char[] { jsonText.Dequeue(), jsonText.Dequeue(), jsonText.Dequeue(), jsonText.Dequeue() });
                    var codepoint = UInt32.Parse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                    return System.Convert.ToChar(codepoint).ToString();
                default: throw new System.FormatException("Cannot decode escape sequence.");
            }
        }

        private enum JsonState {
            Default,
            LookingForObjectStart,
            LookingForNameStart,
            LookingForNameEnd,
            LookingForPairSeparator,
            LookingForValueStart,
            LookingForValueEnd,
            LookingForObjectEnd,
            LookingForObjectSeparator,
            LookingForNullChar2,
            LookingForNullChar3,
            LookingForNullChar4,
            DeadEnd,
        }


        private static string ReadToSpaceOrEnd(MemoryStream stream) {
            var encoding = Utf8Encoding.Value;

            var bytes = new List<byte>(); ;
            while (stream.Position < stream.Length) {
                var oneByte = (byte)stream.ReadByte();
                if (oneByte == 0x20) {
                    break;
                } else {
                    bytes.Add(oneByte);
                }
            }
            return encoding.GetString(bytes.ToArray());
        }

        private static bool IsMatchingPrefix(byte[] buffer, Int32 offset, byte[] prefix) {
            if (offset + prefix.Length > buffer.Length) { return false; }
            for (int i = 0; i < prefix.Length; i++) {
                if (buffer[offset + i] != prefix[i]) { return false; }
            }
            return true;
        }

        #endregion


        #region Thread-safe constructs

        private static readonly ThreadLocal<UTF8Encoding> Utf8Encoding = new ThreadLocal<UTF8Encoding>(() => { return new UTF8Encoding(false); });
        private static readonly ThreadLocal<RandomNumberGenerator> Random = new ThreadLocal<RandomNumberGenerator>(() => { return RandomNumberGenerator.Create(); });
        private static readonly ThreadLocal<RijndaelManaged> Aes128Cbc = new ThreadLocal<RijndaelManaged>(
            () => {
                RijndaelManaged aes = new RijndaelManaged();
                try {
                    aes.KeySize = 128;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;
                } catch {
                    if (aes != null) { aes.Dispose(); } //will not happen - just there to satisfy CA2000
                    throw;
                }
                return aes;
            });
        private static readonly ThreadLocal<SHA256> Sha256 = new ThreadLocal<SHA256>(() => { return SHA256Managed.Create(); });

        private static readonly byte[] BytesHeaderTiny128 = new byte[] { 0x54, 0x69, 0x6E, 0x79, 0x31, 0x32, 0x38, 0x20 };
        private static readonly byte[] BytesHeaderTiny = new byte[] { 0x54, 0x69, 0x6E, 0x79, 0x20 };
        private static readonly byte[] BytesNull = new byte[] { 0x6E, 0x75, 0x6C, 0x6C };

        private const int DefaultStreamCapacity = 1024;

        #endregion


        /// <summary>
        /// Returns cloned packet.
        /// All system identifiers are removed in process.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Object is intentionally not disposed.")]
        public TinyPacket Clone() {
            var clone = new TinyPacket(this.Product, this.Operation);
            foreach (var item in this.Items) {
                if (!item.Key.StartsWith(".", StringComparison.Ordinal)) {
                    clone.Add(item.Key, item.Value);
                }
            }
            return clone;
        }


        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        public override Boolean Equals(Object obj) {
            return base.Equals(obj);
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        public override Int32 GetHashCode() {
            unchecked {
                int hash = 17;
                hash = hash * 31 + this.Product.GetHashCode();
                hash = hash * 31 + this.Operation.GetHashCode();
                return hash;
            }
        }


        #region IDisposable Members

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">True if managed resources should be disposed; otherwise, false.</param>
        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                this.Product = null;
                this.Operation = null;
                this.Items.Clear();
            }
        }

        #endregion

        #region IEnumerable

        /// <summary>
        /// Returns an enumerator.
        /// </summary>
        public IEnumerator<KeyValuePair<String, String>> GetEnumerator() {
            foreach (var item in this.Items) {
                if (!item.Key.StartsWith(".", StringComparison.Ordinal)) {
                    yield return item;
                }
            }
        }

        /// <summary>
        /// Returns an enumerator.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }

        #endregion

    }



    /// <summary>
    /// Event arguments for TinyPacketReceived message.
    /// </summary>
    public class TinyPacketEventArgs : EventArgs {

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="packet">Packet.</param>
        /// <param name="localEndpoint">Local end point.</param>
        /// <param name="remoteEndpoint">Remote end point.</param>
        /// <exception cref="System.ArgumentNullException">Packet cannot be null.</exception>
        public TinyPacketEventArgs(TinyPacket packet, IPEndPoint localEndpoint, IPEndPoint remoteEndpoint) {
            if (packet == null) { throw new ArgumentNullException("packet", "Packet cannot be null."); }

            this.Packet = packet;
            this.LocalEndpoint = localEndpoint;
            this.RemoteEndpoint = remoteEndpoint;
        }


        /// <summary>
        /// Gets packet that was received.
        /// </summary>
        public TinyPacket Packet { get; private set; }

        /// <summary>
        /// Gets endpoint that was origin of message.
        /// </summary>
        public IPEndPoint RemoteEndpoint { get; private set; }

        /// <summary>
        /// Gets endpoint that was destination of message.
        /// </summary>
        public IPEndPoint LocalEndpoint { get; private set; }

    }
}



/*

TinyMessage Packet Format
=========================

TinyMessage is text based protocol. Each packet is encapsulated in UDP datagram
and it is of following content (each part encoded as UTF8, `<SP>` denotes
the space character):

    Protocol <SP> Product <SP> Operation <SP> Data

*Protocol*:   This field denotes protocol version. It is fixed to "Tiny".

*Product*:    This field denotes product which performs action. It is used to
              segment space of available operations. Product must not contain
              spaces and it should contain only ASCII. Preferred format would
              be application name, at (@) sign followed by IANA assigned
              Private Enterprise Number. E.g. Application@12345.

*Operation*: Denotes which operation is to be performed by receiver of message.
             Operation must not contain spaces and it should contain only
             ASCII.

*Data*:      JSON encoded object in form of multiple name/value pairs. E.g.:
            `{"Name1":"Value1","Name2":"Value2",...,"NameN":"ValueN"}`
             User property names are restricted to letters, numbers, and
             underscore character (_). System properties will start with dot
             (.). Currently defined system properties are .Id (hexadecimal
             32-bit integer) and .Host (name of a machine).


Shared-key Encryption
---------------------

TinyMessage also supports shared-key encryption using 128-bit AES protocol in
CBC mode with a PKCS#7 padding. Additionally is protected with a SHA-256 hash.
Format of the encrypted message would thus be as follows (each part is binary,
`<SP>` denotes the space character):

    Protocol <SP> IV Encrypted MAC

*Protocol*:   This field denotes protocol version. It is fixed to "Tiny128".

*IV*:         Random initialization vector. In case of AES this field will
              have a total length of 16 bytes (128 bits).

*Encrypted*:  This is encrypted content same as defined for plain-text packet.

*MAC*:        SHA-256 output covering all previous bytes. Total length of this
              field is 32 bytes (256 bits). If all values are zeros, MAC is
              considered as omitted.


Addresses and ports
-------------------            

TinyMessage has been allocated UDP port 5104 and IPv6 multicast address
ff0X:0::152. Assuming organization scope, preferred IPv6 address would thus be
ff08:0::152 (48-bit MAC is 33:33:00:00:01:52).

For IPv4 multicast group address was self-assigned from RFC 2365, IPv4
Organization Local Scope. Preferred address would be 239.192.111.17 (with MAC
address of 01:00:5E:40:6F:11).

*/
