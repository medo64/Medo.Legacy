/* Josip Medved <jmedved@jmedved.com> * www.medo64.com * MIT License */

//2015-03-18: Fixed error measurement mixup.
//2015-03-15: Works under Mono (and Linux).
//2015-03-02: Initial version.


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;

namespace Medo.Device {

    /// <summary>
    /// Communication with Hermo device.
    /// </summary>
    public class Hermo : IDisposable {

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="portName">The port to use.</param>
        /// <exception cref="System.ArgumentNullException">Port name cannot be null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Unknown port name.</exception>
        public Hermo(string portName) {
            if (portName == null) { throw new ArgumentNullException("portName", "Port name cannot be null."); }

            var foundPort = false;
            portName = portName.Trim();
            foreach (var systemPortName in SerialPort.GetPortNames()) { //match system casing
                if (string.Equals(portName, systemPortName, StringComparison.OrdinalIgnoreCase)) {
                    PortName = systemPortName;
                    foundPort = true;
                    break;
                }
            }
            if (!foundPort) { throw new ArgumentOutOfRangeException("portName", "Unknown port name."); }

            SerialPort = new SerialPort(PortName, 9600, Parity.None, 8, StopBits.One) {
                Encoding = ASCIIEncoding.ASCII,
                NewLine = "\n",
                ReadTimeout = 2500,
                WriteTimeout = 1000,
                DtrEnable = true,
                RtsEnable = true
            };
        }


        private readonly SerialPort SerialPort;


        /// <summary>
        /// Gets the port for communication with Hermo device.
        /// </summary>
        public string PortName { get; private set; }

        /// <summary>
        /// Opens a connection.
        /// </summary>
        /// <exception cref="System.IO.IOException">Cannot open port.</exception>
        public void Open() {
            try {
                SerialPort.Open();
            } catch (Exception ex) {
                throw new IOException(ex.Message, ex);
            }

            ThreadCancelEvent = new ManualResetEvent(false);
            Thread = new Thread(Run) { CurrentCulture = CultureInfo.InvariantCulture, IsBackground = true, Name = "Hermo @" + PortName };
            Thread.Start();
        }

        /// <summary>
        /// Closes a connection.
        /// </summary>
        public void Close() {
            if (SerialPort.IsOpen) {
                SerialPort.Close();
                ThreadCancelEvent.Set();
                while (Thread.IsAlive) { Thread.Sleep(1); }
                ThreadCancelEvent.Close();
                ThreadCancelEvent = null;
                Thread = null;
            }
        }


        /// <summary>
        /// Gets if connection toward Hermo is open.
        /// </summary>
        public bool IsOpen {
            get { return SerialPort.IsOpen; }
        }


        #region IDisposable

        /// <summary>
        /// Disposes used resources.
        /// </summary>
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes used resources.
        /// </summary>
        /// <param name="disposing">If true, managed resources are to be disposed.</param>
        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                Close();
                SerialPort.Dispose();
            }
        }

        #endregion


        #region Reading

        private readonly object ReadingsSyncRoot = new object();
        private readonly Dictionary<long, KeyValuePair<HermoReading, DateTime>> ReadingCache = new Dictionary<long, KeyValuePair<HermoReading, DateTime>>();
        private static readonly int ReadingValidityInSeconds = 15;

        /// <summary>
        /// Enumerates over all device readings.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Calling the method two times in succession creates different results.")]
        public IEnumerable<HermoReading> GetReadings() {
            var readings = new List<HermoReading>();
            lock (ReadingsSyncRoot) {
                foreach (var item in ReadingCache) {
                    var reading = item.Value.Key;
                    var expiry = item.Value.Value;
                    if (expiry > DateTime.UtcNow) {
                        readings.Add(reading);
                    }
                }
            }
            readings.Sort(delegate (HermoReading reading1, HermoReading reading2) { return reading1.Code.CompareTo(reading2.Code); }); //so that results are always in same order

            foreach (var reading in readings) {
                yield return reading;
            }
        }

        /// <summary>
        /// Enumerates over all temperature readings.
        /// Devices without temperature are not in list.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Calling the method two times in succession creates different results.")]
        public IEnumerable<HermoReading> GetTemperatureReadings() {
            foreach (var reading in GetReadings()) {
                if (reading.HasTemperature) {
                    yield return reading;
                }
            }
        }

        #endregion


        #region Events

        /// <summary>
        /// Occurs when device has been detected on a bus.
        /// </summary>
        public event EventHandler<HermoReadingEventArgs> DeviceRead;

        /// <summary>
        /// Occurs when a valid temperature is read.
        /// </summary>
        public event EventHandler<HermoReadingEventArgs> TemperatureRead;

        /// <summary>
        /// Raises DeviceRead event and, if there is a temperature reading, TemperatureRead event.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        /// <exception cref="System.ArgumentNullException">Event arguments cannot be null.</exception>
        private void OnDeviceRead(HermoReadingEventArgs e) {
            if (e == null) { throw new ArgumentNullException("e", "Event arguments cannot be null."); }

            var reading = e.Reading;
            var expiry = DateTime.UtcNow.AddSeconds(Hermo.ReadingValidityInSeconds);

            var key = reading.Code;
            var value = new KeyValuePair<HermoReading, DateTime>(reading, expiry);
            lock (ReadingsSyncRoot) {
                if (ReadingCache.ContainsKey(key)) {
                    if (reading.HasTemperature || (ReadingCache[key].Value.AddSeconds(-Hermo.ReadingValidityInSeconds / 2) > DateTime.UtcNow)) { //overwrite cache if new one has temperature or it is about to expire; to avoid overwrite without temperature
                        ReadingCache[key] = value;
                    }
                } else {
                    ReadingCache.Add(key, value);
                }
            }

            var eh = DeviceRead;
            if (eh != null) { eh.Invoke(this, e); }

            if ((e != null) && (reading.HasTemperature)) {
                var eh2 = TemperatureRead;
                if (eh2 != null) { eh2.Invoke(this, e); }
            }
        }

        #endregion


        #region Threading

        private Thread Thread;
        private ManualResetEvent ThreadCancelEvent;

        private void Run() {
            try {
                while (!ThreadCancelEvent.WaitOne(0, false)) {
                    try {
                        if (SerialPort.IsOpen) {
                            var line = SerialPort.ReadLine();
                            var reading = ParseLine(line);
                            if (reading != null) { OnDeviceRead(new HermoReadingEventArgs(reading)); }
                        } else {
                            ResetSerialPort();
                        }
                    } catch (InvalidOperationException) {
                        ResetSerialPort();
                    } catch (IOException) {
                        ResetSerialPort();
                    } catch (TimeoutException) {
                        ResetSerialPort();
                    } catch (UnauthorizedAccessException) {
                        ResetSerialPort();
                    }
                }
            } catch (ThreadAbortException) { }
        }

        private void ResetSerialPort() {
            try {
                if (SerialPort.IsOpen) { SerialPort.Close(); }
                for (var i = 0; i < SerialPort.ReadTimeout / 100; i++) {
                    if (ThreadCancelEvent.WaitOne(0, false)) { break; }
                    Thread.Sleep(100);
                }
                SerialPort.Open();
            } catch (IOException) {
                Thread.Sleep(100);
            } catch (UnauthorizedAccessException) {
                Thread.Sleep(100);
            }
        }

        private static HermoReading ParseLine(string value) {
            var bytes = GetBytesFromHex(value); //all bytes are LSB, in 1-wire orders
            if (bytes == null) { return null; }

            return ParseBytes(bytes);
        }

        private static HermoReading ParseBytes(byte[] bytes) {
            if (bytes.Length < 8) { return null; }

            var family = bytes[0];
            var codeCrc = bytes[7];
            if ((family == 0) && (codeCrc == 0)) { return null; } //probably reading of all 0s.

            var codeCrc2 = GetCrc8Digest(bytes, 0, 7);
            if (codeCrc != codeCrc2) { return null; } //CRC is not valid

            var code = BitConverter.ToInt64(BitConverter.IsLittleEndian ? bytes : new byte[] { bytes[7], bytes[6], bytes[5], bytes[4], bytes[3], bytes[2], bytes[1], bytes[0] }, 0);

            if (bytes.Length > 8) { //there is scratchpad
                var scratchpadCrc = bytes[bytes.Length - 1];
                var scratchpadCrc2 = GetCrc8Digest(bytes, 8, bytes.Length - 9);
                if (scratchpadCrc == scratchpadCrc2) { //CRC is valid
                    switch (family) { //depending on family, there are different calculations

                        case 0x10: { //DS18S20
                                var reading = ParseBytesForDS18S20(code, bytes);
                                if (reading != null) { return reading; }
                            }
                            break;

                        case 0x28: { //DS18B20
                                var reading = ParseBytesForDS18B20(code, bytes);
                                if (reading != null) { return reading; }
                            }
                            break;

                    }
                }
            }

            return new HermoReading(code, double.NaN, double.NaN);
        }

        private static HermoReading ParseBytesForDS18S20(long code, byte[] bytes) {
            if (bytes.Length <= 10) { return null; } //at least two temperature bytes (and CRC) are there
            if ((bytes[8] == 0xAA) && (bytes[9] == 0x00)) { return null; }  //Error measurement of 85°C

            var temperatureCounter = BitConverter.IsLittleEndian ? BitConverter.ToInt16(bytes, 8) : BitConverter.ToInt16(new byte[] { bytes[9], bytes[8] }, 0);
            var countRemain = (bytes.Length >= 15) ? bytes[14] : 0;
            var countPerC = (bytes.Length >= 15) ? bytes[15] : 0;

            if (countPerC != 0) {
                var temperature = (temperatureCounter * 0.5) - 0.25F + (double)((countPerC - countRemain) / countPerC);
                if ((temperature < -100) || (temperature > 170)) { return null; } //something is wrong with this reading
                return new HermoReading(code, temperature, 0.25);
            } else { //probably missing some data.
                var temperature = temperatureCounter * 0.5;
                if ((temperature < -100) || (temperature > 170)) { return null; } //something is wrong with this reading
                return new HermoReading(code, temperature, 0.5);
            }
        }

        private static HermoReading ParseBytesForDS18B20(long code, byte[] bytes) {
            if (bytes.Length <= 10) { return null; } //at least two temperature bytes (and CRC) are there
            if ((bytes[8] == 0x50) && (bytes[9] == 0x05)) { return null; }  //Error measurement of 85°C

            var temperatureCounter = BitConverter.IsLittleEndian ? BitConverter.ToInt16(bytes, 8) : BitConverter.ToInt16(new byte[] { bytes[9], bytes[8] }, 0);
            var temperature = temperatureCounter * 0.0625;
            if ((temperature < -100) || (temperature > 170)) { return null; } //something is wrong with this reading

            var configurationRegister = (bytes.Length >= 12) ? bytes[12] : 0;
            var resolutionBits = (configurationRegister & 0x7F) >> 5;
            switch (resolutionBits) {
                case 1: return new HermoReading(code, temperature, 0.25);
                case 2: return new HermoReading(code, temperature, 0.125);
                case 3: return new HermoReading(code, temperature, 0.0625);
                default: return new HermoReading(code, temperature, 0.5);
            }
        }

        private static byte[] GetBytesFromHex(string value) {
            byte? lowNibble = null;
            var bytes = new Stack<byte>();
            for (var i = value.Length - 1; i >= 0; i--) {
                var ch = value[i];
                if (char.IsWhiteSpace(ch) || (ch == ',') || (ch == '-') || (ch == ':')) { continue; } //ignore whitespaces and some other characters

                if (byte.TryParse(value.Substring(i, 1), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var nibble)) {
                    if (lowNibble.HasValue) {
                        bytes.Push((byte)((nibble << 4) | lowNibble));
                        lowNibble = null;
                    } else {
                        lowNibble = nibble;
                    }
                } else {
                    return null; //just return empty if you cannot parse
                }

            }
            if (lowNibble.HasValue) { bytes.Push(lowNibble.Value); }
            return bytes.ToArray();
        }

        #endregion


        #region CRC

        private static readonly byte[] Crc8Lookup = new byte[] { 0x00, 0x5E, 0xBC, 0xE2, 0x61, 0x3F, 0xDD, 0x83, 0xC2, 0x9C, 0x7E, 0x20, 0xA3, 0xFD, 0x1F, 0x41, 0x9D, 0xC3, 0x21, 0x7F, 0xFC, 0xA2, 0x40, 0x1E, 0x5F, 0x01, 0xE3, 0xBD, 0x3E, 0x60, 0x82, 0xDC, 0x23, 0x7D, 0x9F, 0xC1, 0x42, 0x1C, 0xFE, 0xA0, 0xE1, 0xBF, 0x5D, 0x03, 0x80, 0xDE, 0x3C, 0x62, 0xBE, 0xE0, 0x02, 0x5C, 0xDF, 0x81, 0x63, 0x3D, 0x7C, 0x22, 0xC0, 0x9E, 0x1D, 0x43, 0xA1, 0xFF, 0x46, 0x18, 0xFA, 0xA4, 0x27, 0x79, 0x9B, 0xC5, 0x84, 0xDA, 0x38, 0x66, 0xE5, 0xBB, 0x59, 0x07, 0xDB, 0x85, 0x67, 0x39, 0xBA, 0xE4, 0x06, 0x58, 0x19, 0x47, 0xA5, 0xFB, 0x78, 0x26, 0xC4, 0x9A, 0x65, 0x3B, 0xD9, 0x87, 0x04, 0x5A, 0xB8, 0xE6, 0xA7, 0xF9, 0x1B, 0x45, 0xC6, 0x98, 0x7A, 0x24, 0xF8, 0xA6, 0x44, 0x1A, 0x99, 0xC7, 0x25, 0x7B, 0x3A, 0x64, 0x86, 0xD8, 0x5B, 0x05, 0xE7, 0xB9, 0x8C, 0xD2, 0x30, 0x6E, 0xED, 0xB3, 0x51, 0x0F, 0x4E, 0x10, 0xF2, 0xAC, 0x2F, 0x71, 0x93, 0xCD, 0x11, 0x4F, 0xAD, 0xF3, 0x70, 0x2E, 0xCC, 0x92, 0xD3, 0x8D, 0x6F, 0x31, 0xB2, 0xEC, 0x0E, 0x50, 0xAF, 0xF1, 0x13, 0x4D, 0xCE, 0x90, 0x72, 0x2C, 0x6D, 0x33, 0xD1, 0x8F, 0x0C, 0x52, 0xB0, 0xEE, 0x32, 0x6C, 0x8E, 0xD0, 0x53, 0x0D, 0xEF, 0xB1, 0xF0, 0xAE, 0x4C, 0x12, 0x91, 0xCF, 0x2D, 0x73, 0xCA, 0x94, 0x76, 0x28, 0xAB, 0xF5, 0x17, 0x49, 0x08, 0x56, 0xB4, 0xEA, 0x69, 0x37, 0xD5, 0x8B, 0x57, 0x09, 0xEB, 0xB5, 0x36, 0x68, 0x8A, 0xD4, 0x95, 0xCB, 0x29, 0x77, 0xF4, 0xAA, 0x48, 0x16, 0xE9, 0xB7, 0x55, 0x0B, 0x88, 0xD6, 0x34, 0x6A, 0x2B, 0x75, 0x97, 0xC9, 0x4A, 0x14, 0xF6, 0xA8, 0x74, 0x2A, 0xC8, 0x96, 0x15, 0x4B, 0xA9, 0xF7, 0xB6, 0xE8, 0x0A, 0x54, 0xD7, 0x89, 0x6B, 0x35 };

        private static byte GetCrc8Digest(byte[] value, int index, int length) {
            byte digest = 0;
            for (var i = index; i < index + length; i++) {
                digest = Crc8Lookup[digest ^ value[i]];
            }
            return digest;
        }

        #endregion

    }


    /// <summary>
    /// Hermo temperature reading.
    /// </summary>
    [DebuggerDisplay("{Temperature}", Name = "{Code16.ToString(\"X4\")}")]
    public class HermoReading {

        /// <summary>
        /// Create new reading.
        /// </summary>
        /// <param name="code">64-bit device code.</param>
        /// <param name="temperature">Temperature reading. If NaN, temperature couldn't be read.</param>
        /// <param name="resolution">Resolution of a temperature reading.</param>
        internal HermoReading(long code, double temperature, double resolution) {
            Code = code;
            Temperature = temperature;
            Resolution = resolution;
        }


        /// <summary>
        /// Gets full 1-wire device code.
        /// </summary>
        public long Code { get; private set; }

        /// <summary>
        /// Gets 1-wire device code folded into 32-bits.
        /// Folding is done using XOR between high and low halves of 64-bit code.
        /// </summary>
        public int Code32 {
            get {
                var msb = (int)((Code >> 32) & 0xFFFFFFFF);
                var lsb = (int)(Code & 0xFFFFFFFF);
                return msb ^ lsb;
            }
        }

        /// <summary>
        /// Gets 1-wire device code folded into 16-bits.
        /// Folding is done using XOR between high and low halves of 32-bit code.
        /// </summary>
        public int Code16 {
            get {
                var msb = (Code32 >> 16) & 0xFFFF;
                var lsb = Code32 & 0xFFFF;
                return msb ^ lsb;
            }
        }


        /// <summary>
        /// Gets serial number portion of a code.
        /// </summary>
        public long Serial {
            get {
                return (Code & 0x00FFFFFFFFFFFF00) >> 8;
            }
        }

        /// <summary>
        /// Gets family ID.
        /// Common values are: 0x10 (DS18S20) and 0x28 (DS18B20).
        /// </summary>
        public int FamilyId {
            get {
                return (int)(Code & 0xFF);
            }
        }


        /// <summary>
        /// Gets temperature in °C.
        /// </summary>
        public double Temperature { get; private set; }

        /// <summary>
        /// Gets temperature resolution in °C.
        /// </summary>
        public double Resolution { get; private set; }

        /// <summary>
        /// Gets whether temperature is valid.
        /// </summary>
        public bool HasTemperature {
            get { return !double.IsNaN(Temperature); }
        }

    }


    /// <summary>
    /// Hermo reading event arguments.
    /// </summary>
    public class HermoReadingEventArgs : EventArgs {

        /// <summary>
        /// Create new instance.
        /// </summary>
        /// <param name="reading">Temperature reading.</param>
        /// <exception cref="System.ArgumentNullException">Reading cannot be null.</exception>
        public HermoReadingEventArgs(HermoReading reading) {
            Reading = reading ?? throw new ArgumentNullException("reading", "Reading cannot be null.");
        }

        /// <summary>
        /// Gets temperature reading.
        /// </summary>
        public HermoReading Reading { get; private set; }

    }

}
