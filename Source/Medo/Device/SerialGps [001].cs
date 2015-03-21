//Copyright (c) 2015 Josip Medved <jmedved@jmedved.com>

//2015-03-16: Initial version.


using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;

namespace Medo.Device {

    /// <summary>
    /// Communication with NMEA capable serial GPS.
    /// </summary>
    public class SerialGps : IDisposable {

        /// <summary>
        /// Creates new instance.
        /// Default baud rate is 4800.
        /// </summary>
        /// <param name="portName">The port to use.</param>
        /// <exception cref="System.ArgumentNullException">Port name cannot be null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Invalid port parameters.</exception>
        public SerialGps(string portName)
            : this(portName, 4800) {
        }

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="portName">The port to use.</param>
        /// <param name="baudRate">Baud rate.</param>
        /// <exception cref="System.ArgumentNullException">Port name cannot be null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Invalid port parameters.</exception>
        public SerialGps(string portName, int baudRate) {
            if (portName == null) { throw new ArgumentNullException("portName", "Port name cannot be null."); }

            var foundPort = false;
            portName = portName.Trim();
            foreach (var systemPortName in SerialPort.GetPortNames()) { //match system casing
                if (string.Equals(portName, systemPortName, StringComparison.OrdinalIgnoreCase)) {
                    this.PortName = systemPortName;
                    foundPort = true;
                    break;
                }
            }
            if (!foundPort) { throw new ArgumentOutOfRangeException("portName", "Invalid port parameters."); }

            this.SerialPort = new SerialPort(this.PortName, baudRate, Parity.None, 8, StopBits.One);
            this.SerialPort.Encoding = ASCIIEncoding.ASCII;
            this.SerialPort.NewLine = "\n";
            this.SerialPort.ReadTimeout = 2500;
            this.SerialPort.WriteTimeout = 1000;
            this.SerialPort.DtrEnable = true;
            this.SerialPort.RtsEnable = true;
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
                this.SerialPort.Open();
            } catch (Exception ex) {
                throw new IOException(ex.Message, ex);
            }

            this.ThreadCancelEvent = new ManualResetEvent(false);
            this.Thread = new Thread(Run) { CurrentCulture = CultureInfo.InvariantCulture, IsBackground = true, Name = "Hermo @" + this.PortName };
            this.Thread.Start();
        }

        /// <summary>
        /// Closes a connection.
        /// </summary>
        public void Close() {
            if (this.SerialPort.IsOpen) {
                this.SerialPort.Close();
                this.ThreadCancelEvent.Set();
                while (this.Thread.IsAlive) { Thread.Sleep(1); }
                this.ThreadCancelEvent.Close();
                this.ThreadCancelEvent = null;
                this.Thread = null;
            }
        }


        /// <summary>
        /// Gets if connection toward Hermo is open.
        /// </summary>
        public bool IsOpen {
            get { return this.SerialPort.IsOpen; }
        }


        #region IDisposable

        /// <summary>
        /// Disposes used resources.
        /// </summary>
        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes used resources.
        /// </summary>
        /// <param name="disposing">If true, managed resources are to be disposed.</param>
        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                this.Close();
                this.SerialPort.Dispose();
            }
        }

        #endregion


        #region Properties

        private readonly ExpirableReadings LastReadings = new ExpirableReadings();
        private readonly object SyncRoot = new object();

        /// <summary>
        /// Gets last reported position.
        /// Values expire after 5 seconds.
        /// </summary>
        public GpsPosition Position {
            get {
                lock (SyncRoot) {
                    return new GpsPosition(this.LastReadings.Latitude, this.LastReadings.Longitude, this.LastReadings.Altitude);
                }
            }
        }

        /// <summary>
        /// Gets last reported velocity.
        /// Values expire after 5 seconds.
        /// </summary>
        public GpsVelocity Velocity {
            get {
                lock (SyncRoot) {
                    return new GpsVelocity(this.LastReadings.Speed, this.LastReadings.Heading, this.LastReadings.MagneticVariation);
                }
            }
        }

        /// <summary>
        /// Gets last reported geometry.
        /// Values expire after 5 seconds.
        /// </summary>
        public GpsGeometry Geometry {
            get {
                lock (SyncRoot) {
                    return new GpsGeometry(this.LastReadings.HorizontalDilution, this.LastReadings.VerticalDilution, this.LastReadings.PositionDilution, this.LastReadings.SatellitesInUse, this.LastReadings.SatellitesInView);
                }
            }
        }

        #endregion


        #region Events

        /// <summary>
        /// Occurs when new data is received.
        /// </summary>
        public event EventHandler<EventArgs> DataUpdate;

        /// <summary>
        /// Raises DataUpdate event.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        /// <exception cref="System.ArgumentNullException">Event arguments cannot be null.</exception>
        private void OnDataUpdate(EventArgs e) {
            if (e == null) { throw new ArgumentNullException("e", "Event arguments cannot be null."); }

            var eh = this.DataUpdate;
            if (eh != null) { eh.Invoke(this, e); }
        }

        
        /// <summary>
        /// Occurs when new sentence is received.
        /// Only sentences that pass checksum check are reported
        /// </summary>
        public event EventHandler<GpsSentenceEventArgs> RawSentenceReceived;

        /// <summary>
        /// Raises RawSentenceReceived event.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        /// <exception cref="System.ArgumentNullException">Event arguments cannot be null.</exception>
        private void OnRawSentenceReceived(GpsSentenceEventArgs e) {
            if (e == null) { throw new ArgumentNullException("e", "Event arguments cannot be null."); }

            var eh = this.RawSentenceReceived;
            if (eh != null) { eh.Invoke(this, e); }
        }


        /// <summary>
        /// Occurs when time is received.
        /// </summary>
        public event EventHandler<GpsTimeEventArgs> TimeUpdate;

        /// <summary>
        /// Raises TimeUpdate event.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        /// <exception cref="System.ArgumentNullException">Event arguments cannot be null.</exception>
        private void OnTimeUpdate(GpsTimeEventArgs e) {
            if (e == null) { throw new ArgumentNullException("e", "Event arguments cannot be null."); }

            var eh = this.TimeUpdate;
            if (eh != null) { eh.Invoke(this, e); }
        }


        /// <summary>
        /// Occurs when position data is received.
        /// </summary>
        public event EventHandler<GpsPositionEventArgs> PositionUpdate;

        /// <summary>
        /// Raises PositionUpdate event.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        /// <exception cref="System.ArgumentNullException">Event arguments cannot be null.</exception>
        private void OnPositionUpdate(GpsPositionEventArgs e) {
            if (e == null) { throw new ArgumentNullException("e", "Event arguments cannot be null."); }

            var eh = this.PositionUpdate;
            if (eh != null) { eh.Invoke(this, e); }
        }


        /// <summary>
        /// Occurs when velocity data is received.
        /// </summary>
        public event EventHandler<GpsVelocityEventArgs> VelocityUpdate;

        /// <summary>
        /// Raises VelocityUpdate event.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        /// <exception cref="System.ArgumentNullException">Event arguments cannot be null.</exception>
        private void OnVelocityUpdate(GpsVelocityEventArgs e) {
            if (e == null) { throw new ArgumentNullException("e", "Event arguments cannot be null."); }

            var eh = this.VelocityUpdate;
            if (eh != null) { eh.Invoke(this, e); }
        }


        /// <summary>
        /// Occurs when geometry data is received.
        /// </summary>
        public event EventHandler<GpsGeometryEventArgs> GeometryUpdate;

        /// <summary>
        /// Raises GeometryUpdate event.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        /// <exception cref="System.ArgumentNullException">Event arguments cannot be null.</exception>
        private void OnGeometryUpdate(GpsGeometryEventArgs e) {
            if (e == null) { throw new ArgumentNullException("e", "Event arguments cannot be null."); }

            var eh = this.GeometryUpdate;
            if (eh != null) { eh.Invoke(this, e); }
        }

        #endregion


        #region Threading

        private Thread Thread;
        private ManualResetEvent ThreadCancelEvent;

        private void Run() {
            try {
                while (!this.ThreadCancelEvent.WaitOne(0, false)) {
                    try {
                        if (this.SerialPort.IsOpen) {
                            var line = this.SerialPort.ReadLine().Trim();
                            var readings = ParseLine(line);
                            if (readings != null) {
                                var anyUpdate = false;

                                if (readings.Time.HasValue) {
                                    this.OnTimeUpdate(new GpsTimeEventArgs(readings.Time.Value));
                                    anyUpdate = true;
                                }

                                anyUpdate |= ProcessPosition(readings);
                                anyUpdate |= ProcessVelocity(readings);
                                anyUpdate |= ProcessGeometry(readings);

                                if (anyUpdate) { this.OnDataUpdate(new EventArgs()); }
                                this.OnRawSentenceReceived(new GpsSentenceEventArgs(line));
                            }
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


        private bool ProcessPosition(Readings readings) {
            if (readings.Latitude.HasValue || readings.Longitude.HasValue || readings.Altitude.HasValue) {
                lock (this.SyncRoot) {
                    if (readings.Latitude.HasValue) { this.LastReadings.Latitude.Value = readings.Latitude.Value; }
                    if (readings.Longitude.HasValue) { this.LastReadings.Longitude.Value = readings.Longitude.Value; }
                    if (readings.Altitude.HasValue) { this.LastReadings.Altitude.Value = readings.Altitude.Value; }
                }
                this.OnPositionUpdate(new GpsPositionEventArgs(new GpsPosition(
                    readings.Latitude.HasValue ? readings.Latitude.Value : double.NaN,
                    readings.Longitude.HasValue ? readings.Longitude.Value : double.NaN,
                    readings.Altitude.HasValue ? readings.Altitude.Value : double.NaN
                )));
                return true;
            }
            return false;
        }

        private bool ProcessVelocity(Readings readings) {
            if (readings.Speed.HasValue || readings.Heading.HasValue || readings.MagneticVariation.HasValue) {
                lock (this.SyncRoot) {
                    if (readings.Speed.HasValue) { this.LastReadings.Speed.Value = readings.Speed.Value; }
                    if (readings.Heading.HasValue) { this.LastReadings.Heading.Value = readings.Heading.Value; }
                    if (readings.MagneticVariation.HasValue) { this.LastReadings.MagneticVariation.Value = readings.MagneticVariation.Value; }
                }
                this.OnVelocityUpdate(new GpsVelocityEventArgs(new GpsVelocity(
                    readings.Speed.HasValue ? readings.Speed.Value : double.NaN,
                    readings.Heading.HasValue ? readings.Heading.Value : double.NaN,
                    readings.MagneticVariation.HasValue ? readings.MagneticVariation.Value : double.NaN
                )));
                return true;
            }
            return false;
        }

        private bool ProcessGeometry(Readings readings) {
            if (readings.HorizontalDilution.HasValue || readings.VerticalDilution.HasValue || readings.PositionDilution.HasValue || readings.SatellitesInUse.HasValue || readings.SatellitesInView.HasValue) {
                lock (this.SyncRoot) {
                    if (readings.HorizontalDilution.HasValue) { this.LastReadings.HorizontalDilution.Value = readings.HorizontalDilution.Value; }
                    if (readings.VerticalDilution.HasValue) { this.LastReadings.VerticalDilution.Value = readings.VerticalDilution.Value; }
                    if (readings.PositionDilution.HasValue) { this.LastReadings.PositionDilution.Value = readings.PositionDilution.Value; }
                    if (readings.SatellitesInUse.HasValue) { this.LastReadings.SatellitesInUse.Value = readings.SatellitesInUse.Value; }
                    if (readings.SatellitesInView.HasValue) { this.LastReadings.SatellitesInView.Value = readings.SatellitesInView.Value; }
                }
                this.OnGeometryUpdate(new GpsGeometryEventArgs(new GpsGeometry(
                    readings.HorizontalDilution.HasValue ? readings.HorizontalDilution.Value : double.NaN,
                    readings.VerticalDilution.HasValue ? readings.VerticalDilution.Value : double.NaN,
                    readings.PositionDilution.HasValue ? readings.PositionDilution.Value : double.NaN,
                    readings.SatellitesInUse.HasValue ? readings.SatellitesInUse.Value : 0,
                    readings.SatellitesInView.HasValue ? readings.SatellitesInView.Value : 0
                )));
                return true;
            }
            return false;
        }


        private void ResetSerialPort() {
            try {
                if (this.SerialPort.IsOpen) { this.SerialPort.Close(); }
                for (var i = 0; i < this.SerialPort.ReadTimeout / 100; i++) {
                    if (this.ThreadCancelEvent.WaitOne(0, false)) { break; }
                    Thread.Sleep(100);
                }
                this.SerialPort.Open();
            } catch (IOException) {
                Thread.Sleep(100);
            } catch (UnauthorizedAccessException) {
                Thread.Sleep(100);
            }
        }

        #endregion


        #region Parsing

        private static Readings ParseLine(string line) {
            if (line == null) { return null; } //no line

            if (line.Length < 9) { return null; } //sentence is too short
            if (!line.StartsWith("$", StringComparison.Ordinal)) { return null; } //line must start with $
            if (line[6] != ',') { return null; } //after sentence there must be comma
            if (line[line.Length - 3] != '*') { return null; } //cannot find checksum

            byte newChecksum = 0;
            string data = line.Substring(1, line.Length - 3 - 1);
            foreach (var b in ASCIIEncoding.ASCII.GetBytes(data)) {
                newChecksum ^= b;
            }
            if (!line.EndsWith(newChecksum.ToString("X2", CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase)) { return null; } //checksum mismatch

            var parts = data.Split(',');
            switch (parts[0].ToUpperInvariant()) {
                case "GPGGA": return ParseSentenceGga(parts);
                case "GPGLL": return ParseSentenceGll(parts);
                case "GPGSA": return ParseSentenceGsa(parts);
                case "GPGSV": return ParseSentenceGsv(parts);
                case "GPRMC": return ParseSentenceRmc(parts);
                case "GPVTG": return ParseSentenceVtg(parts);
                default: return new Readings(); //unrecognized sentence will return empty reading
            }
        }


        private static Readings ParseSentenceGga(string[] parts) {
            var latitudePart = (parts.Length > 2) ? parts[2] : null;
            var latitudeSignPart = (parts.Length > 3) ? parts[3] : null;
            var longitudePart = (parts.Length > 4) ? parts[4] : null;
            var longitudeSignPart = (parts.Length > 5) ? parts[5] : null;
            var fixTypePart = (parts.Length > 6) ? parts[6] : null;
            var satellitesUsedPart = (parts.Length > 7) ? parts[7] : null;
            var hdopPart = (parts.Length > 8) ? parts[8] : null;
            var altitudePart = (parts.Length > 9) ? parts[9] : null;
            var altitudeUnitsPart = (parts.Length > 10) ? parts[10] : null;

            if (string.Equals(fixTypePart, "0", StringComparison.OrdinalIgnoreCase)) { return null; } //no lock

            var readings = new Readings();

            readings.Latitude = ParseLatitude(latitudePart, latitudeSignPart);
            readings.Longitude = ParseLongitude(longitudePart, longitudeSignPart);
            if (string.Equals(altitudeUnitsPart, "M", StringComparison.OrdinalIgnoreCase)) {
                readings.Altitude = ParseDouble(altitudePart);
            }
            readings.SatellitesInUse = ParseInt32(satellitesUsedPart);
            readings.HorizontalDilution = ParseDouble(hdopPart);

            return readings;
        }

        private static Readings ParseSentenceGll(string[] parts) {
            var latitudePart = (parts.Length > 1) ? parts[1] : null;
            var latitudeSignPart = (parts.Length > 2) ? parts[2] : null;
            var longitudePart = (parts.Length > 3) ? parts[3] : null;
            var longitudeSignPart = (parts.Length > 4) ? parts[4] : null;
            var statusPart = (parts.Length > 6) ? parts[6] : null;

            if ((statusPart != null) && !string.Equals(statusPart, "A", StringComparison.OrdinalIgnoreCase)) { return null; } //not active

            var readings = new Readings();

            readings.Latitude = ParseLatitude(latitudePart, latitudeSignPart);
            readings.Longitude = ParseLongitude(longitudePart, longitudeSignPart);

            return readings;
        }

        private static Readings ParseSentenceGsa(string[] parts) {
            var modePart = (parts.Length > 1) ? parts[1] : null;
            var hdopPart = (parts.Length > 15) ? parts[15] : null;
            var vdopPart = (parts.Length > 16) ? parts[16] : null;
            var pdopPart = (parts.Length > 17) ? parts[17] : null;

            if ((modePart != null) && string.Equals(modePart, "1", StringComparison.OrdinalIgnoreCase)) { return null; } //no fix

            var readings = new Readings();

            readings.HorizontalDilution = ParseDouble(hdopPart);
            readings.VerticalDilution = ParseDouble(vdopPart);
            readings.PositionDilution = ParseDouble(pdopPart);

            return readings;
        }

        private static Readings ParseSentenceGsv(string[] parts) {
            var satellitesInViewPart = (parts.Length > 3) ? parts[3] : null;

            var readings = new Readings();

            readings.SatellitesInView = ParseInt32(satellitesInViewPart);

            return readings;
        }

        private static Readings ParseSentenceRmc(string[] parts) {
            var utcTimePart = (parts.Length > 1) ? parts[1] : null;
            var statusPart = (parts.Length > 2) ? parts[2] : null;
            var latitudePart = (parts.Length > 3) ? parts[3] : null;
            var latitudeSignPart = (parts.Length > 4) ? parts[4] : null;
            var longitudePart = (parts.Length > 5) ? parts[5] : null;
            var longitudeSignPart = (parts.Length > 6) ? parts[6] : null;
            var speedOverGroundPart = (parts.Length > 7) ? parts[7] : null;
            var headingPart = (parts.Length > 8) ? parts[8] : null;
            var utcDatePart = (parts.Length > 9) ? parts[9] : null;
            var magneticVariationPart = (parts.Length > 10) ? parts[10] : null;
            var magneticVariationSignPart = (parts.Length > 11) ? parts[11] : null;

            if ((statusPart != null) && !string.Equals(statusPart, "A", StringComparison.OrdinalIgnoreCase)) { return null; } //not active

            var readings = new Readings();

            readings.Time = ParseTime(utcDatePart, utcTimePart);
            readings.Latitude = ParseLatitude(latitudePart, latitudeSignPart);
            readings.Longitude = ParseLongitude(longitudePart, longitudeSignPart);
            readings.Speed = ParseDouble(speedOverGroundPart, 1852.0 / 3600);
            readings.Heading = ParseDouble(headingPart);
            readings.MagneticVariation = ParseDegrees(magneticVariationPart, magneticVariationSignPart);

            return readings;
        }

        private static Readings ParseSentenceVtg(string[] parts) {
            var trackMadeGoodPart = (parts.Length > 1) ? parts[1] : null;
            var trackMadeGoodUnitPart = (parts.Length > 2) ? parts[2] : null;

            var speedOverGroundPart = (parts.Length > 5) ? parts[5] : null;
            var speedOverGroundUnitPart = (parts.Length > 6) ? parts[6] : null;

            var readings = new Readings();

            if (string.Equals(speedOverGroundUnitPart, "N", StringComparison.OrdinalIgnoreCase)) { //must be in knots
                readings.Speed = ParseDouble(speedOverGroundPart, 1852.0 / 3600);
            }
            if (string.Equals(trackMadeGoodUnitPart, "T", StringComparison.OrdinalIgnoreCase)) { //must be relative to true north
                readings.Heading = ParseDouble(trackMadeGoodPart);
            }

            return readings;
        }


        private static readonly string[] DateTimeFormats = { "ddMMyy HHmmss", "ddMMyy HHmmss.f", "ddMMyy HHmmss.ff", "ddMMyy HHmmss.fff" };

        private static DateTime? ParseTime(string utcDatePart, string utcTimePart) {
            if ((utcDatePart == null) || (utcTimePart == null)) { return null; }

            DateTime time;
            if (DateTime.TryParseExact(utcDatePart + " " + utcTimePart, DateTimeFormats, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out time)) {
                return time;
            }
            return null;
        }

        private static double? ParseLatitude(string latitudePart, string latitudeSignPart) {
            if ((latitudePart == null) || (latitudeSignPart == null)) { return null; }

            var latitudeDegreesPart = (latitudePart.Length > 2) ? latitudePart.Substring(0, 2) : null;
            var latitudeMinutesPart = (latitudePart.Length > 2) ? latitudePart.Substring(2) : null;
            int latitudeDegrees;
            double latitudeMinutes;
            if (int.TryParse(latitudeDegreesPart, NumberStyles.Integer, CultureInfo.InvariantCulture, out latitudeDegrees) && double.TryParse(latitudeMinutesPart, NumberStyles.Float, CultureInfo.InvariantCulture, out latitudeMinutes)) {
                var latitude = latitudeDegrees + latitudeMinutes / 60.0;
                if (string.Equals(latitudeSignPart, "N", StringComparison.OrdinalIgnoreCase)) {
                    return latitude;
                } else if (string.Equals(latitudeSignPart, "S", StringComparison.OrdinalIgnoreCase)) {
                    return -latitude;
                }
            }
            return null;
        }

        private static double? ParseLongitude(string longitudePart, string longitudeSignPart) {
            if ((longitudePart == null) || (longitudeSignPart == null)) { return null; }

            var longitudeDegreesPart = (longitudePart.Length > 2) ? longitudePart.Substring(0, 3) : null;
            var longitudeMinutesPart = (longitudePart.Length > 2) ? longitudePart.Substring(3) : null;
            int longitudeDegrees;
            double longitudeMinutes;
            if (int.TryParse(longitudeDegreesPart, NumberStyles.Integer, CultureInfo.InvariantCulture, out longitudeDegrees) && double.TryParse(longitudeMinutesPart, NumberStyles.Float, CultureInfo.InvariantCulture, out longitudeMinutes)) {
                var longitude = longitudeDegrees + longitudeMinutes / 60.0;
                if (string.Equals(longitudeSignPart, "E", StringComparison.OrdinalIgnoreCase)) {
                    return longitude;
                } else if (string.Equals(longitudeSignPart, "W", StringComparison.OrdinalIgnoreCase)) {
                    return -longitude;
                }
            }
            return null;
        }

        private static double? ParseDegrees(string degreesPart, string degreesSignPart) {
            double degrees;
            if (double.TryParse(degreesPart, NumberStyles.Float, CultureInfo.InvariantCulture, out degrees)) {
                if (string.Equals(degreesSignPart, "E", StringComparison.OrdinalIgnoreCase)) {
                    return degrees;
                } else if (string.Equals(degreesSignPart, "W", StringComparison.OrdinalIgnoreCase)) {
                    return -degrees;
                }
            }
            return null;
        }

        private static int? ParseInt32(string valuePart) {
            int value;
            if (int.TryParse(valuePart, NumberStyles.Integer, CultureInfo.InvariantCulture, out value)) {
                return value;
            }
            return null;
        }

        private static double? ParseDouble(string valuePart, double multiplier = 1) {
            double value;
            if (double.TryParse(valuePart, NumberStyles.Float, CultureInfo.InvariantCulture, out value)) {
                return value * multiplier;
            }
            return null;
        }


        [DebuggerDisplay("Time={Time}; Latitude={Latitude}; Longitude={Longitude}; Altitude={Altitude}; Speed={Speed}; Heading={Heading}")]
        private class Readings {
            internal DateTime? Time;
            internal double? Latitude;
            internal double? Longitude;
            internal double? Altitude;
            internal double? Speed;
            internal double? Heading;
            internal double? MagneticVariation;
            internal double? HorizontalDilution;
            internal double? VerticalDilution;
            internal double? PositionDilution;
            internal int? SatellitesInUse;
            internal int? SatellitesInView;
        }

        [DebuggerDisplay("Time={Time}; Latitude={Latitude}; Longitude={Longitude}; Altitude={Altitude}; Speed={Speed}; Heading={Heading}")]
        private class ExpirableReadings {
            internal ExpirableValue<double> Latitude = new ExpirableValue<double>(double.NaN);
            internal ExpirableValue<double> Longitude = new ExpirableValue<double>(double.NaN);
            internal ExpirableValue<double> Altitude = new ExpirableValue<double>(double.NaN);
            internal ExpirableValue<double> Speed = new ExpirableValue<double>(double.NaN);
            internal ExpirableValue<double> Heading = new ExpirableValue<double>(double.NaN);
            internal ExpirableValue<double> MagneticVariation = new ExpirableValue<double>(double.NaN);
            internal ExpirableValue<double> HorizontalDilution = new ExpirableValue<double>(double.NaN);
            internal ExpirableValue<double> VerticalDilution = new ExpirableValue<double>(double.NaN);
            internal ExpirableValue<double> PositionDilution = new ExpirableValue<double>(double.NaN);
            internal ExpirableValue<int> SatellitesInUse = new ExpirableValue<int>(0);
            internal ExpirableValue<int> SatellitesInView = new ExpirableValue<int>(0);

            internal class ExpirableValue<T> where T : struct {
                public ExpirableValue(T defaultValue) {
                    this.DefaultValue = defaultValue;
                }

                private readonly T DefaultValue;
                private DateTime ExpireTime;

                private T _value;
                internal T Value {
                    get { return (DateTime.UtcNow < this.ExpireTime) ? this._value : this.DefaultValue; }
                    set {
                        this._value = value;
                        this.ExpireTime = DateTime.UtcNow.AddSeconds(5);
                    }
                }

                public static implicit operator T(ExpirableValue<T> expirable) {
                    return (expirable != null) ? expirable.Value : default(T);
                }
            }
        }

        #endregion

    }


    #region EventArgs

    /// <summary>
    /// GPS time event arguments.
    /// </summary>
    public class GpsTimeEventArgs : EventArgs {

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="time">Time.</param>
        public GpsTimeEventArgs(DateTime time) {
            this.Time = time;
        }


        /// <summary>
        /// Gets time.
        /// </summary>
        public DateTime Time { get; private set; }

    }


    /// <summary>
    /// GPS position event arguments.
    /// </summary>
    public class GpsPositionEventArgs : EventArgs {

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="position">Positon.</param>
        /// <exception cref="System.ArgumentNullException">Position cannot be null.</exception>
        public GpsPositionEventArgs(GpsPosition position) {
            if (position == null) { throw new ArgumentNullException("position", "Position cannot be null."); }
            this.Position = position;
        }


        /// <summary>
        /// Gets position.
        /// </summary>
        public GpsPosition Position { get; private set; }

    }


    /// <summary>
    /// GPS velocity event arguments.
    /// </summary>
    public class GpsVelocityEventArgs : EventArgs {

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="velocity">Velocity.</param>
        /// <exception cref="System.ArgumentNullException">Velocity cannot be null.</exception>
        public GpsVelocityEventArgs(GpsVelocity velocity) {
            if (velocity == null) { throw new ArgumentNullException("velocity", "Velocity cannot be null."); }
            this.Velocity = velocity;
        }


        /// <summary>
        /// Gets velocity.
        /// </summary>
        public GpsVelocity Velocity { get; private set; }

    }


    /// <summary>
    /// GPS geometry event arguments.
    /// </summary>
    public class GpsGeometryEventArgs : EventArgs {

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="geometry">Geometry.</param>
        /// <exception cref="System.ArgumentNullException">Geometry cannot be null.</exception>
        public GpsGeometryEventArgs(GpsGeometry geometry) {
            if (geometry == null) { throw new ArgumentNullException("geometry", "Geometry cannot be null."); }
            this.Geometry = geometry;
        }


        /// <summary>
        /// Gets geometry information.
        /// </summary>
        public GpsGeometry Geometry { get; private set; }

    }


    /// <summary>
    /// GPS sentence event arguments.
    /// </summary>
    public class GpsSentenceEventArgs : EventArgs {

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="sentence">Sentence.</param>
        /// <exception cref="System.ArgumentNullException">Sentence cannot be null.</exception>
        public GpsSentenceEventArgs(string sentence) {
            if (sentence == null) { throw new ArgumentNullException("sentence", "Sentence cannot be null."); }
            this.Sentence = sentence;
        }


        /// <summary>
        /// Gets geometry information.
        /// </summary>
        public String Sentence { get; private set; }

    }

    #endregion


    /// <summary>
    /// Position as reported by GPS.
    /// </summary>
    [DebuggerDisplay("Latitude={Latitude}; Longitude={Longitude}")]
    public class GpsPosition {

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="latitude">Latitude in degrees.</param>
        /// <param name="longitude">Longitude in degrees.</param>
        public GpsPosition(double latitude, double longitude)
            : this(latitude, longitude, double.NaN) {
        }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="latitude">Latitude in degrees.</param>
        /// <param name="longitude">Longitude in degrees.</param>
        /// <param name="altitude">Altitude in meters.</param>
        public GpsPosition(double latitude, double longitude, double altitude) {
            this.Latitude = latitude;
            this.Longitude = longitude;
            this.Altitude = altitude;
        }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        public GpsPosition()
            : this(double.NaN, double.NaN, double.NaN) {
        }


        /// <summary>
        /// Gets latitude in degrees.
        /// If value is not defined Double.NaN will be returned.
        /// </summary>
        public double Latitude { get; private set; }

        /// <summary>
        /// Gets if latitude is defined.
        /// </summary>
        public bool HasLatitude {
            get { return !double.IsNaN(this.Latitude); }
        }


        /// <summary>
        /// Gets longitude in degrees.
        /// If value is not defined Double.NaN will be returned.
        /// </summary>
        public double Longitude { get; private set; }

        /// <summary>
        /// Gets if longitude is defined.
        /// </summary>
        public bool HasLongitude {
            get { return !double.IsNaN(this.Longitude); }
        }

        /// <summary>
        /// Gets altitude in meters.
        /// If value is not defined Double.NaN will be returned.
        /// </summary>
        public double Altitude { get; private set; }

        /// <summary>
        /// Gets if altitude is defined.
        /// </summary>
        public bool HasAltitude {
            get { return !double.IsNaN(this.Altitude); }
        }


        #region Calculation

        /// <summary>
        /// Distance between this and other position in meters.
        /// Altitude is not taken into consideration.
        /// If either position has no latitude or longitude, Double.NaN will be returned.
        /// </summary>
        /// <param name="position">Other position.</param>
        /// <exception cref="System.ArgumentNullException">Position cannot be null.</exception>
        public double DistanceTo(GpsPosition position) {
            return DistanceBetween(this, position);
        }

        private const double EarthRadius = 6371000;

        /// <summary>
        /// Returns distance between two positions in meters.
        /// Altitude is not taken into consideration.
        /// If either position has no latitude or longitude, Double.NaN will be returned.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">Position cannot be null.</exception>
        public static double DistanceBetween(GpsPosition position1, GpsPosition position2) {
            if (position1 == null) { throw new ArgumentNullException("position1", "Position cannot be null."); }
            if (position2 == null) { throw new ArgumentNullException("position2", "Position cannot be null."); }
            if (!position1.HasLatitude || !position1.HasLongitude || !position2.HasLatitude || !position2.HasLongitude) { return double.NaN; }

            //Haversine formula
            var lat1 = position1.Latitude * System.Math.PI / 180;
            var lat2 = position2.Latitude * System.Math.PI / 180;
            var dLat = lat2 - lat1;
            var dLon = (position2.Longitude - position1.Longitude) * System.Math.PI / 180;

            var a = System.Math.Pow(System.Math.Sin(dLat / 2), 2) + System.Math.Cos(lat1) * System.Math.Cos(lat2) * System.Math.Pow(System.Math.Sin(dLon / 2), 2);
            var c = 2 * System.Math.Atan2(System.Math.Sqrt(a), System.Math.Sqrt(1 - a));

            return EarthRadius * c;
        }

        #endregion


        #region Overrides

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        public override bool Equals(object obj) {
            var other = obj as GpsPosition;
            return ((other != null) && this.Latitude.Equals(other.Latitude) && this.Longitude.Equals(other.Longitude) && this.Altitude.Equals(other.Altitude));
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        public override int GetHashCode() {
            return unchecked(this.Latitude.GetHashCode() * 41 + this.Longitude.GetHashCode());
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            if (this.HasLatitude && this.HasLongitude) {
                return string.Format(CultureInfo.InvariantCulture, "{0:0.0000}° {1}, {2:0.0000}° {3}",
                    System.Math.Abs(this.Latitude), (this.Latitude >= 0) ? "N" : "S",
                    System.Math.Abs(this.Longitude), (this.Latitude >= 0) ? "E" : "W"
                );
            } else {
                return "";
            }
        }

        #endregion

    }


    /// <summary>
    /// Velocity as reported by GPS.
    /// </summary>
    [DebuggerDisplay("Speed={Speed}; Heading={Heading}")]
    public class GpsVelocity {

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="speed">Speed in meters per second.</param>
        public GpsVelocity(double speed)
            : this(speed, double.NaN, double.NaN) {
        }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="speed">Speed in meters per second.</param>
        /// <param name="heading">Heading in degrees.</param>
        public GpsVelocity(double speed, double heading)
            : this(speed, heading, double.NaN) {
        }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="speed">Speed in meters per second.</param>
        /// <param name="heading">Heading in degrees.</param>
        /// <param name="magneticVariation">Magnetic variation in degrees.</param>
        public GpsVelocity(double speed, double heading, double magneticVariation) {
            this.Speed = speed;
            this.Heading = heading;
            this.MagneticVariation = magneticVariation;
        }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        public GpsVelocity()
            : this(double.NaN, double.NaN, double.NaN) {
        }


        /// <summary>
        /// Gets speed in meters per second.
        /// If value is not defined Double.NaN will be returned.
        /// </summary>
        public double Speed { get; private set; }

        /// <summary>
        /// Gets if speed is defined.
        /// </summary>
        public bool HasSpeed {
            get { return !double.IsNaN(this.Speed); }
        }

        /// <summary>
        /// Gets speed in kilometers per hour.
        /// If value is not defined Double.NaN will be returned.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Kph", Justification = "Shorthand for km/h.")]
        public double SpeedInKph {
            get {
                return this.HasSpeed ? this.Speed * (3600 / 1000.0) : double.NaN;
            }
        }

        /// <summary>
        /// Gets speed in miles per hour.
        /// If value is not defined Double.NaN will be returned.
        /// </summary>
        public double SpeedInMph {
            get {
                return this.HasSpeed ? this.Speed * (3600 / 1609.344) : double.NaN;
            }
        }

        /// <summary>
        /// Gets speed in knots.
        /// If value is not defined Double.NaN will be returned.
        /// </summary>
        public double SpeedInKnots {
            get {
                return this.HasSpeed ? this.Speed * (3600 / 1852.0) : double.NaN;
            }
        }

        /// <summary>
        /// Gets speed in feets per second.
        /// If value is not defined Double.NaN will be returned.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Ftps", Justification = "Shorthand for ft/s.")]
        public double SpeedInFtps {
            get {
                return this.HasSpeed ? this.Speed * (1 / 0.3048) : double.NaN;
            }
        }


        /// <summary>
        /// Gets heading in degrees.
        /// If value is not defined Double.NaN will be returned.
        /// </summary>
        public double Heading { get; private set; }

        /// <summary>
        /// Gets if heading is defined.
        /// </summary>
        public bool HasHeading {
            get { return !double.IsNaN(this.Heading); }
        }


        /// <summary>
        /// Gets magnetic variation in degrees.
        /// Positive values are toward east, negative values are toward west.
        /// If value is not defined Double.NaN will be returned.
        /// </summary>
        public double MagneticVariation { get; private set; }

        /// <summary>
        /// Gets if magnetic variation is defined.
        /// </summary>
        public bool HasMagneticVariation {
            get { return !double.IsNaN(this.MagneticVariation); }
        }


        #region Overrides

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        public override bool Equals(object obj) {
            var other = obj as GpsVelocity;
            return ((other != null) && this.Speed.Equals(other.Speed) && this.Heading.Equals(other.Heading) && this.MagneticVariation.Equals(other.MagneticVariation));
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        public override int GetHashCode() {
            return unchecked(this.Speed.GetHashCode() * 41 + this.Heading.GetHashCode());
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            return this.HasSpeed ? this.Speed.ToString("0.0 m/s", CultureInfo.InvariantCulture) : "";
        }

        #endregion

    }


    /// <summary>
    /// Satellite geometry information, as reported by GPS.
    /// </summary>
    [DebuggerDisplay("HDOP={HorizontalDilution}; VDOP={VerticalDilution}; PDOP={PositionDilution}; Satellites={SatellitesInUse}/{SatellitesInView}")]
    public class GpsGeometry {

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="horizontalDilution">Horizontal dilution value.</param>
        /// <param name="verticalDilution">Vertical dilution value.</param>
        /// <param name="positionDilution">Position (3D) dilution value.</param>
        /// <param name="satellitesInUse">Number of satellites in use.</param>
        /// <param name="satellitesInView">Number of satellites in view.</param>
        public GpsGeometry(double horizontalDilution, double verticalDilution, double positionDilution, int satellitesInUse, int satellitesInView) {
            this.HorizontalDilution = horizontalDilution;
            this.VerticalDilution = verticalDilution;
            this.PositionDilution = positionDilution;
            this.SatellitesInUse = satellitesInUse;
            this.SatellitesInView = satellitesInView;
        }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="horizontalDilution">Horizontal dilution value.</param>
        /// <param name="verticalDilution">Vertical dilution value.</param>
        /// <param name="positionDilution">Position (3D) dilution value.</param>
        /// <param name="satellitesInUse">Number of satellites in use.</param>
        /// <param name="satellitesInView">Number of satellites in view.</param>
        public GpsGeometry(double horizontalDilution, double verticalDilution, double positionDilution)
            : this(horizontalDilution, verticalDilution, positionDilution, 0, 0) {
        }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        public GpsGeometry()
            : this(double.NaN, double.NaN, double.NaN) {
        }


        /// <summary>
        /// Gets horizontal dilution value.
        /// If value is not defined Double.NaN will be returned.
        /// </summary>
        public double HorizontalDilution { get; private set; }

        /// <summary>
        /// Gets if horizontal dilution is defined.
        /// </summary>
        public bool HasHorizontalDilution {
            get { return !double.IsNaN(this.HorizontalDilution); }
        }


        /// <summary>
        /// Gets vertical dilution value.
        /// If value is not defined Double.NaN will be returned.
        /// </summary>
        public double VerticalDilution { get; private set; }

        /// <summary>
        /// Gets if vertical dilution is defined.
        /// </summary>
        public bool HasVerticalDilution {
            get { return !double.IsNaN(this.VerticalDilution); }
        }


        /// <summary>
        /// Gets position (3D) dilution value.
        /// Positive values are toward east, negative values are toward west.
        /// If value is not defined Double.NaN will be returned.
        /// </summary>
        public double PositionDilution { get; private set; }

        /// <summary>
        /// Gets if position (3D) dilution is defined.
        /// </summary>
        public bool HasPositionDilution {
            get { return !double.IsNaN(this.PositionDilution); }
        }


        /// <summary>
        /// Gets number of satellites in use.
        /// If value is not defined 0 will be returned.
        /// </summary>
        public int SatellitesInUse { get; private set; }

        /// <summary>
        /// True if there is positive number of satellites in use.
        /// </summary>
        public bool HasAnySatellitesInUse {
            get { return (this.SatellitesInUse > 0); }
        }


        /// <summary>
        /// Gets number of satellites in view.
        /// If value is not defined 0 will be returned.
        /// </summary>
        public int SatellitesInView { get; private set; }

        /// <summary>
        /// True if there is positive number of satellites in view.
        /// </summary>
        public bool HasAnySatellitesInView {
            get { return (this.SatellitesInView > 0); }
        }


        #region Overrides

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        public override bool Equals(object obj) {
            var other = obj as GpsGeometry;
            return ((other != null) && this.HorizontalDilution.Equals(other.HorizontalDilution) && this.VerticalDilution.Equals(other.VerticalDilution) && this.PositionDilution.Equals(other.PositionDilution) && this.SatellitesInUse.Equals(other.SatellitesInUse) && this.SatellitesInView.Equals(other.SatellitesInView));
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        public override int GetHashCode() {
            return unchecked(this.HorizontalDilution.GetHashCode() * 41 + this.VerticalDilution.GetHashCode());
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            return this.HasPositionDilution ? this.PositionDilution.ToString("0.0", CultureInfo.InvariantCulture) : "";
        }

        #endregion

    }

}
