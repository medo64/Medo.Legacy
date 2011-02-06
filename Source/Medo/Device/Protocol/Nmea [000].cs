using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Diagnostics;

namespace Medo.IO.Nmea {

    /// <summary>
    /// Enumerates longitude sign.
    /// </summary>
    public enum NmeaLongitudeSign : int {
        Unknown = 0,
        East = +1,
        West = -1
    }




    /// <summary>
    /// Enumerates latitude sign.
    /// </summary>
    public enum NmeaLatitudeSign : int {
        Unknown = 0,
        North = +1,
        South = -1
    }




    /// <summary>
    /// Enumerates GPS quality indicator.
    /// </summary>
    public enum NmeaQualityIndicator : int {
        Unknown = 0,
        FixNotAvailable = 48,
        GpsFix = 49,
        DifferentialGpsFix = 50,
        PpsFix = 51,
        RealTimeKinematic = 52,
        FloatRtk = 53,
        Estimated = 54,
        ManualInputMode = 55,
        SimulationMode = 56,
    }




    /// <summary>
    /// Enumerates FAA mode indicator (NMEA 2.3 and above).
    /// </summary>
    public enum NmeaFaaModeIndicator : int {
        Unknown = 0,
        AutonomousMode = 65,
        DifferentialMode = 68,
        EstimatedMode = 69,
        ManualInputMode = 77,
        SimulatedMode = 83,
        DataNotValid = 78,
    }




    /// <summary>
    /// Enumerates selection mode.
    /// </summary>
    public enum NmeaSatelliteSelectionMode : int {
        Unknown = 0,
        Automatic = 65,
        Manual = 77,
    }




    /// <summary>
    /// Enumerates selection mode.
    /// </summary>
    public enum NmeaSatelliteFixMode : int {
        Unknown = 0,
        NoFix = 49,
        TwoDimensional = 50,
        ThreeDimensional = 51,
    }




    /// <summary>
    /// Enumerates navigation status.
    /// </summary>
    public enum NmeaNavigationStatus : int {
        Unknown = 0,
        NavigationReceiverWarning = 86,
        Valid = 65,
    }




    /// <summary>
    /// Enumerates navigation status.
    /// </summary>
    public enum NmeaDataStatus : int {
        Unknown = 0,
        DataValid = 65,
        DataInvalid = 86,
    }





    /// <summary>
    /// General information about satellites.
    /// </summary>
    public class NmeaSatelliteInformation {

        /// <summary>
        /// Creates new instance.
        /// </summary>
        internal NmeaSatelliteInformation(int prn, int elevation, int azimuth, int snr) {
            this.Prn = prn;
            this.Elevation = elevation;
            this.Azimuth = azimuth;
            this.Snr = snr;
        }


        /// <summary>
        /// Satellite PRN number.
        /// </summary>
        public int Prn { get; private set; }

        /// <summary>
        /// Elevation in degrees (00-90).
        /// </summary>
        public int Elevation { get; private set; }

        /// <summary>
        /// Gets azimuth in degrees to true north (000-359).
        /// </summary>
        public int Azimuth { get; private set; }

        /// <summary>
        /// Gets SNR in dB (00-99).
        /// </summary>
        public int Snr { get; private set; }

    }




    /// <summary>
    /// Structure that stores latitude information.
    /// </summary>
    public struct NmeaLatitude {

        /// <summary>
        /// Returns latitude from given NMEA parameters.
        /// </summary>
        /// <param name="nmeaValue">Value as defined in NMEA.</param>
        /// <param name="nmeaSign">N, S or null.</param>
        /// <exception cref="System.FormatException">Invalid number format. -or- Invalid sign format.</exception>
        public static NmeaLatitude ParseNmea(string nmeaValue, string nmeaSign) {
            decimal value;
            if (!decimal.TryParse(nmeaValue, NumberStyles.Number, CultureInfo.InvariantCulture, out value)) {
                throw new System.FormatException("Invalid number format.");
            }
            NmeaLatitude ret = new NmeaLatitude();
            ret.Minutes = value % 100;
            ret.Degrees = (int)Math.Truncate(value) / 100;

            if (nmeaSign == null) {
                ret.Sign = NmeaLatitudeSign.Unknown;
            } else if (string.Compare(nmeaSign, "N", StringComparison.OrdinalIgnoreCase) == 0) {
                ret.Sign = NmeaLatitudeSign.North;
            } else if (string.Compare(nmeaSign, "S", StringComparison.OrdinalIgnoreCase) == 0) {
                ret.Sign = NmeaLatitudeSign.South;
            } else {
                throw new System.FormatException("Invalid sign format.");
            }

            return ret;
        }

        /// <summary>
        /// Gets latitude sign (north/south).
        /// </summary>
        public NmeaLatitudeSign Sign { get; private set; }

        /// <summary>
        /// Gets integral number of degrees.
        /// </summary>
        public int Degrees { get; private set; }

        /// <summary>
        /// Gets decimal number of minutes
        /// </summary>
        public decimal Minutes { get; private set; }

        /// <summary>
        /// Gets decimal number of degrees .
        /// </summary>
        public decimal TotalDegrees {
            get {
                decimal total = this.Degrees + this.Minutes / 60;
                switch (this.Sign) {
                    case NmeaLatitudeSign.North: return total;
                    case NmeaLatitudeSign.South: return -total;
                    default: return total;
                }
            }
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        public override bool Equals(object obj) {
            if (obj is NmeaLatitude) {
                NmeaLatitude other = (NmeaLatitude)obj;
                return (this.Degrees == other.Degrees) && (this.Minutes == other.Minutes) && (this.Sign == other.Sign);
            }
            return false;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="value1">First object to compare.</param>
        /// <param name="value2">Second object to compare.</param>
        public static bool operator ==(NmeaLatitude value1, NmeaLatitude value2) {
            return value1.Equals(value2);
        }

        /// <summary>
        /// Determines whether the specified object is inequal to the current object.
        /// </summary>
        /// <param name="value1">First object to compare.</param>
        /// <param name="value2">Second object to compare.</param>
        public static bool operator !=(NmeaLatitude value1, NmeaLatitude value2) {
            return !(value1 == value2);
        }

        /// <summary>
        /// Serves as a hash function.
        /// </summary>
        public override int GetHashCode() {
            return this.Degrees.GetHashCode();
        }

        /// <summary>
        /// Returns a System.String that represents the current object.
        /// </summary>
        public override string ToString() {
            switch (this.Sign) {
                case NmeaLatitudeSign.North: return "N" + this.Degrees.ToString("00", CultureInfo.InvariantCulture) + this.Minutes.ToString("00.0000", CultureInfo.InvariantCulture);
                case NmeaLatitudeSign.South: return "S" + this.Degrees.ToString("00", CultureInfo.InvariantCulture) + this.Minutes.ToString("00.0000", CultureInfo.InvariantCulture);
                default: return this.Degrees.ToString("00", CultureInfo.InvariantCulture) + this.Minutes.ToString("00.0000", CultureInfo.InvariantCulture);
            }
        }

    }




    /// <summary>
    /// Structure that stores longitude information.
    /// </summary>
    public struct NmeaLongitude {

        /// <summary>
        /// Returns longitude from given NMEA parameters.
        /// </summary>
        /// <param name="nmeaValue">Value as defined in NMEA.</param>
        /// <param name="nmeaSign">E, W or null.</param>
        /// <exception cref="System.FormatException">Invalid number format. -or- Invalid sign format.</exception>
        public static NmeaLongitude ParseNmea(string nmeaValue, string nmeaSign) {
            decimal value;
            if (!decimal.TryParse(nmeaValue, NumberStyles.Number, CultureInfo.InvariantCulture, out value)) {
                throw new System.FormatException("Invalid number format.");
            }
            NmeaLongitude ret = new NmeaLongitude();
            ret.Minutes = value % 100;
            ret.Degrees = (int)Math.Truncate(value) / 100;

            if (nmeaSign == null) {
                ret.Sign = NmeaLongitudeSign.Unknown;
            } else if (string.Compare(nmeaSign, "E", StringComparison.OrdinalIgnoreCase) == 0) {
                ret.Sign = NmeaLongitudeSign.East;
            } else if (string.Compare(nmeaSign, "W", StringComparison.OrdinalIgnoreCase) == 0) {
                ret.Sign = NmeaLongitudeSign.West;
            } else {
                throw new System.FormatException("Invalid sign format.");
            }

            return ret;
        }

        /// <summary>
        /// Gets latitude sign (north/south).
        /// </summary>
        public NmeaLongitudeSign Sign { get; private set; }

        /// <summary>
        /// Gets integral number of degrees.
        /// </summary>
        public int Degrees { get; private set; }

        /// <summary>
        /// Gets decimal number of minutes
        /// </summary>
        public decimal Minutes { get; private set; }

        /// <summary>
        /// Gets decimal number of degrees .
        /// </summary>
        public decimal TotalDegrees {
            get {
                decimal total = this.Degrees + this.Minutes / 60;
                switch (this.Sign) {
                    case NmeaLongitudeSign.East: return total;
                    case NmeaLongitudeSign.West: return -total;
                    default: return total;
                }
            }
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        public override bool Equals(object obj) {
            if (obj is NmeaLongitude) {
                NmeaLongitude other = (NmeaLongitude)obj;
                return (this.Degrees == other.Degrees) && (this.Minutes == other.Minutes) && (this.Sign == other.Sign);
            }
            return false;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="value1">First object to compare.</param>
        /// <param name="value2">Second object to compare.</param>
        public static bool operator ==(NmeaLongitude value1, NmeaLongitude value2) {
            return value1.Equals(value2);
        }

        /// <summary>
        /// Determines whether the specified object is inequal to the current object.
        /// </summary>
        /// <param name="value1">First object to compare.</param>
        /// <param name="value2">Second object to compare.</param>
        public static bool operator !=(NmeaLongitude value1, NmeaLongitude value2) {
            return !(value1 == value2);
        }

        /// <summary>
        /// Serves as a hash function.
        /// </summary>
        public override int GetHashCode() {
            return this.Degrees.GetHashCode();
        }

        /// <summary>
        /// Returns a System.String that represents the current object.
        /// </summary>
        public override string ToString() {
            switch (this.Sign) {
                case NmeaLongitudeSign.East: return "E" + this.Degrees.ToString("000", CultureInfo.InvariantCulture) + this.Minutes.ToString("00.0000", CultureInfo.InvariantCulture);
                case NmeaLongitudeSign.West: return "W" + this.Degrees.ToString("000", CultureInfo.InvariantCulture) + this.Minutes.ToString("00.0000", CultureInfo.InvariantCulture);
                default: return this.Degrees.ToString("000", CultureInfo.InvariantCulture) + this.Minutes.ToString("00.0000", CultureInfo.InvariantCulture);
            }
        }

    }




    /// <summary>
    /// Structure that combines both latitude and longitude.
    /// </summary>
    public struct NmeaPosition {

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="latitude">Latitude.</param>
        /// <param name="longitude">Longitude.</param>
        public NmeaPosition(NmeaLatitude latitude, NmeaLongitude longitude)
            : this() {
            this.Latitude = latitude;
            this.Longitude = longitude;
        }

        /// <summary>
        /// Gets latitude.
        /// </summary>
        public NmeaLatitude Latitude { get; private set; }

        /// <summary>
        /// Gets longitude.
        /// </summary>
        public NmeaLongitude Longitude { get; private set; }


        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        public override bool Equals(object obj) {
            if (obj is NmeaPosition) {
                NmeaPosition other = (NmeaPosition)obj;
                return (this.Latitude == other.Latitude) && (this.Longitude == other.Longitude);
            }
            return false;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="value1">First object to compare.</param>
        /// <param name="value2">Second object to compare.</param>
        public static bool operator ==(NmeaPosition value1, NmeaPosition value2) {
            return value1.Equals(value2);
        }

        /// <summary>
        /// Determines whether the specified object is inequal to the current object.
        /// </summary>
        /// <param name="value1">First object to compare.</param>
        /// <param name="value2">Second object to compare.</param>
        public static bool operator !=(NmeaPosition value1, NmeaPosition value2) {
            return !(value1 == value2);
        }

        /// <summary>
        /// Serves as a hash function.
        /// </summary>
        public override int GetHashCode() {
            return this.Latitude.GetHashCode() | this.Longitude.GetHashCode();
        }

        /// <summary>
        /// Returns a System.String that represents the current object.
        /// </summary>
        public override string ToString() {
            return this.Latitude.ToString() + " " + this.Longitude.ToString();
        }

    }




    /// <summary>
    /// Handling of streamed NMEA data.
    /// </summary>
    public class NmeaClient {

        private List<byte> _buffer = new List<byte>();


        /// <summary>
        /// Creates new instance.
        /// </summary>
        public NmeaClient() {
        }


        /// <summary>
        /// Adds new data to internal buffer.
        /// </summary>
        /// <param name="buffer">Data.</param>
        public void Add(byte[] buffer) {
            this._buffer.AddRange(buffer);
            while (true) {
                int i = _buffer.FindIndex(x => (x == 13) || (x == 10));
                if (i >= 0) {
                    if (i > 0) {
                        string line = System.Text.ASCIIEncoding.ASCII.GetString(_buffer.ToArray(), 0, i);
                        Debug.WriteLine(line);
                        NmeaSentence sentence;
                        if (NmeaSentence.TryParse(_buffer.ToArray(), 0, i, out sentence)) {
                            this.OnNmeaSentenceArrived(new NmeaSentenceEventArgs(sentence));
                        }
                        _buffer.RemoveRange(0, i + 1);
                    } else { //line delimiter is on very begining.
                        _buffer.RemoveAt(0);
                    }
                } else {
                    break;
                }
            }
        }


        /// <summary>
        /// Occurs when NMEA sentence arrives.
        /// </summary>
        public event EventHandler<NmeaSentenceEventArgs> NmeaSentenceArrived;

        /// <summary>
        /// Invoked when a NmeaSentenceArrived routed event occurs.
        /// </summary>
        /// <param name="e">Event data</param>
        protected void OnNmeaSentenceArrived(NmeaSentenceEventArgs e) {
            if (this.NmeaSentenceArrived != null) {
                this.NmeaSentenceArrived(this, e);
            }
        }

    }




    /// <summary>
    /// Event argument.
    /// </summary>
    public class NmeaSentenceEventArgs : System.EventArgs {

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="nmeaSentence">NMEA sentence to use.</param>
        public NmeaSentenceEventArgs(NmeaSentence nmeaSentence)
            : base() {
            this.NmeaSentence = nmeaSentence;
        }

        /// <summary>
        /// Gets NMEA sentence.
        /// </summary>
        public NmeaSentence NmeaSentence { get; private set; }

    }




    /// <summary>
    /// Base class for NMEA sentences.
    /// </summary>
    public class NmeaSentence {

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="talkerIdentifier">Talker identifier, usually GP.</param>
        /// <param name="sentenceIdentifier">Sentence identifier.</param>
        /// <param name="components">All inner message components.</param>
        protected NmeaSentence(string talkerIdentifier, string sentenceIdentifier, string[] components) {
            this.TalkerIdentifier = talkerIdentifier;
            this.SentenceIdentifier = sentenceIdentifier;
            this._components = components;
        }

        /// <summary>
        /// Gets talker identifier. This value is most often GP.
        /// </summary>
        public string TalkerIdentifier { get; private set; }

        /// <summary>
        /// Gets sentence identifier.
        /// </summary>
        public string SentenceIdentifier { get; private set; }

        private string[] _components;

        /// <summary>
        /// Returns all inner message components.
        /// </summary>
        public string[] GetComponents() { return _components; }

        /// <summary>
        /// Returns NMEA sentence if parsing is sucessfull or exception otherwise.
        /// </summary>
        /// <param name="buffer">Buffer.</param>
        /// <param name="offset">Starting index.</param>
        /// <param name="length">Length of buffer.</param>
        /// <exception cref="System.FormatException">Sentence too short. -or- Invalid start character. -or- Sentence start cannot be found. -or- Checksum cannot be found. -or- Checksum failed.</exception>
        public static NmeaSentence Parse(byte[] buffer, int offset, int length) {
            string errorMessage;
            NmeaSentence trySentence = null;
            if (TryParse(buffer, offset, length, ref trySentence, out errorMessage)) {
                return trySentence;
            } else {
                throw new System.FormatException(errorMessage);
            }
        }

        /// <summary>
        /// Returns true if NMEA sentence is sucessfully parsed.
        /// </summary>
        /// <param name="buffer">Buffer.</param>
        /// <param name="offset">Starting index.</param>
        /// <param name="length">Length of buffer.</param>
        /// <param name="sentence">Parsed NMEA sentence.</param>
        public static bool TryParse(byte[] buffer, int offset, int length, out NmeaSentence sentence) {
            string errorMessage;
            NmeaSentence trySentence = null;
            if (TryParse(buffer, offset, length, ref trySentence, out errorMessage)) {
                sentence = trySentence;
                return true;
            } else {
                sentence = null;
                return false;
            }
        }

        private static bool TryParse(byte[] buffer, int offset, int length, ref NmeaSentence sentence, out string errorMessage) {
            if ((buffer == null) || (length < 9)) { errorMessage = "Sentence too short."; return false; }
            if (buffer[offset] != 36) { errorMessage = "Invalid start character."; return false; } //$
            if (buffer[offset + 6] != 44) { errorMessage = "Sentence start cannot be found."; return false; } //,
            if (buffer[offset + length - 3] != 42) { errorMessage = "Checksum cannot be found."; return false; } //*
            byte newChecksum = 0;
            for (int i = offset + 1; i < offset + length - 3; ++i) {
                newChecksum ^= buffer[i];
            }
            if (string.Compare(ASCIIEncoding.ASCII.GetString(buffer, offset + length - 2, 2), newChecksum.ToString("X2", CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase) != 0) { errorMessage = "Checksum failed."; return false; }

            string talkerID = ASCIIEncoding.ASCII.GetString(buffer, offset + 1, 2);
            string sentenceID = ASCIIEncoding.ASCII.GetString(buffer, offset + 3, 3);
            string[] components = ASCIIEncoding.ASCII.GetString(buffer, offset + 7, length - 10).Split(',');

            if (string.Compare(sentenceID, "GGA", StringComparison.OrdinalIgnoreCase) == 0) {
                sentence = new NmeaGgaSentence(talkerID, sentenceID, components);
            } else if (string.Compare(sentenceID, "GLL", StringComparison.OrdinalIgnoreCase) == 0) {
                sentence = new NmeaGllSentence(talkerID, sentenceID, components);
                //} else if (string.Compare(sentenceID, "GRS", StringComparison.OrdinalIgnoreCase) == 0) {
                //    sentence = new NmeaGrsSentence(talkerID, sentenceID, components);
            } else if (string.Compare(sentenceID, "GSA", StringComparison.OrdinalIgnoreCase) == 0) {
                sentence = new NmeaGsaSentence(talkerID, sentenceID, components);
                //} else if (string.Compare(sentenceID, "GST", StringComparison.OrdinalIgnoreCase) == 0) {
                //    sentence = new NmeaGstSentence(talkerID, sentenceID, components);
            } else if (string.Compare(sentenceID, "GSV", StringComparison.OrdinalIgnoreCase) == 0) {
                sentence = new NmeaGsvSentence(talkerID, sentenceID, components);
            } else if (string.Compare(sentenceID, "RMC", StringComparison.OrdinalIgnoreCase) == 0) {
                sentence = new NmeaRmcSentence(talkerID, sentenceID, components);
            } else if (string.Compare(sentenceID, "VTG", StringComparison.OrdinalIgnoreCase) == 0) {
                sentence = new NmeaVtgSentence(talkerID, sentenceID, components);
            } else {
                sentence = new NmeaSentence(talkerID, sentenceID, components);
            }

            errorMessage = null;
            return true;
        }

        /// <summary>
        /// Returns a System.String that represents the current object.
        /// </summary>
        public override string ToString() {
            return this.SentenceIdentifier.ToString();
        }

    }





    /// <summary>
    /// NMEA GGA (Global Positioning System Fix Data) sentence.
    /// </summary>
    public class NmeaGgaSentence : NmeaSentence {

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="talkerIdentifier">Talker identifier, usually GP.</param>
        /// <param name="sentenceIdentifier">Sentence identifier.</param>
        /// <param name="components">All inner message components.</param>
        internal NmeaGgaSentence(string talkerIdentifier, string sentenceIdentifier, string[] components)
            : base(talkerIdentifier, sentenceIdentifier, components) {
            if (components.Length > 0) {
                string dtComponent = components[0];
                if ((dtComponent.Length >= 6)) {
                    int h, m;
                    decimal s;
                    if ((int.TryParse(dtComponent.Substring(0, 2), NumberStyles.Integer, CultureInfo.InvariantCulture, out h)) && (int.TryParse(dtComponent.Substring(2, 2), NumberStyles.Integer, CultureInfo.InvariantCulture, out m)) && (decimal.TryParse(dtComponent.Substring(4), NumberStyles.Number, CultureInfo.InvariantCulture, out s))) {
                        int sec = (int)Math.Truncate(s);
                        int msec = (int)((s - sec) * 1000);
                        this.UtcTime = new DateTime(1, 1, 1, h, m, sec, msec, DateTimeKind.Utc);
                    }
                }
            }
            if (components.Length > 4) {
                this.Latitude = NmeaLatitude.ParseNmea(components[1], components[2]);
                this.Longitude = NmeaLongitude.ParseNmea(components[3], components[4]);
            }
            if (components.Length > 5) { //GPS Quality Indicator
                switch (components[5].ToUpperInvariant()) {
                    case "0":
                        this.QualityIndicator = NmeaQualityIndicator.FixNotAvailable;
                        break;
                    case "1":
                        this.QualityIndicator = NmeaQualityIndicator.GpsFix;
                        break;
                    case "2":
                        this.QualityIndicator = NmeaQualityIndicator.DifferentialGpsFix;
                        break;
                    case "3":
                        this.QualityIndicator = NmeaQualityIndicator.PpsFix;
                        break;
                    case "4":
                        this.QualityIndicator = NmeaQualityIndicator.RealTimeKinematic;
                        break;
                    case "5":
                        this.QualityIndicator = NmeaQualityIndicator.FloatRtk;
                        break;
                    case "6":
                        this.QualityIndicator = NmeaQualityIndicator.Estimated;
                        break;
                    case "7":
                        this.QualityIndicator = NmeaQualityIndicator.ManualInputMode;
                        break;
                    case "8":
                        this.QualityIndicator = NmeaQualityIndicator.SimulationMode;
                        break;
                    default:
                        this.QualityIndicator = NmeaQualityIndicator.Unknown;
                        break;
                }
            }
            if (components.Length > 6) {
                int value;
                if (int.TryParse(components[6], NumberStyles.Integer, CultureInfo.InvariantCulture, out value)) {
                    this.SatellitesInView = value;
                }
            }
            if (components.Length > 7) {
                double value;
                if (double.TryParse(components[7], NumberStyles.Number, CultureInfo.InvariantCulture, out value)) {
                    this.HorizontalDilution = value;
                }
            }
            if (components.Length > 9) {
                double value;
                if (double.TryParse(components[8], NumberStyles.Number, CultureInfo.InvariantCulture, out value)) {
                    switch (components[9].ToUpperInvariant()) {
                        case "M":
                            this.Altitude = value;
                            break;
                    }
                }
            }
            if (components.Length > 11) {
                double value;
                if (double.TryParse(components[10], NumberStyles.Number, CultureInfo.InvariantCulture, out value)) {
                    switch (components[11].ToUpperInvariant()) {
                        case "M":
                            this.GeoidalSeparation = value;
                            break;
                    }
                }
            }
            if (components.Length > 12) {
                int value;
                if (int.TryParse(components[12], NumberStyles.Integer, CultureInfo.InvariantCulture, out value)) {
                    this.DifferentialDataAge = value;
                }
            }
            if (components.Length > 13) {
                int value;
                if (int.TryParse(components[13], NumberStyles.Integer, CultureInfo.InvariantCulture, out value)) {
                    this.DifferentialStationIdentifier = value;
                }
            }
        }

        /// <summary>
        /// Gets utc time if one exists or null otherwise.
        /// Only time component is given.
        /// </summary>
        public DateTime? UtcTime { get; private set; }

        /// <summary>
        /// Gets latitude if one exists or null otherwise.
        /// </summary>
        public NmeaLatitude? Latitude { get; private set; }

        /// <summary>
        /// Gets longitude if one exists or null otherwise.
        /// </summary>
        public NmeaLongitude? Longitude { get; private set; }

        /// <summary>
        /// Gets quality indicator.
        /// </summary>
        public NmeaQualityIndicator? QualityIndicator { get; private set; }

        /// <summary>
        /// Gets number of satellites in view.
        /// </summary>
        public int? SatellitesInView { get; private set; }

        /// <summary>
        /// Gets horizontal Dilution of precision (meters).
        /// </summary>
        public double? HorizontalDilution { get; private set; }

        /// <summary>
        /// Gets Antenna Altitude above/below mean-sea-level (geoid) (in meters)
        /// </summary>
        public double? Altitude { get; private set; }

        /// <summary>
        /// Gets Geoidal separation, the difference between the WGS-84 earth ellipsoid and mean-sea-level (geoid), "-" means mean-sea-level below ellipsoid.
        /// </summary>
        public double? GeoidalSeparation { get; private set; }

        /// <summary>
        /// Gets age of differential GPS data, time in seconds since last SC104 type 1 or 9 update, null field when DGPS is not used.
        /// </summary>
        public int? DifferentialDataAge { get; private set; }

        /// <summary>
        /// Gets differential reference station ID.
        /// </summary>
        public int? DifferentialStationIdentifier { get; private set; }


        /// <summary>
        /// Returns a System.String that represents the current object.
        /// </summary>
        public override string ToString() {
            StringBuilder sb = new StringBuilder(base.ToString() + ":");
            if (this.UtcTime.HasValue) { sb.AppendFormat(CultureInfo.InvariantCulture, " {0:HH:mm:ss\\Z}", this.UtcTime.Value); }
            if (this.Latitude.HasValue) { sb.AppendFormat(CultureInfo.InvariantCulture, " {0}", this.Latitude.Value); }
            if (this.Longitude.HasValue) { sb.AppendFormat(CultureInfo.InvariantCulture, " {0}", this.Longitude.Value); }
            if (this.QualityIndicator.HasValue) { sb.AppendFormat(CultureInfo.InvariantCulture, " {0}", this.QualityIndicator.Value); }
            if (this.SatellitesInView.HasValue) { sb.AppendFormat(CultureInfo.InvariantCulture, " {0}", this.SatellitesInView.Value); }
            if (this.HorizontalDilution.HasValue) { sb.AppendFormat(CultureInfo.InvariantCulture, " {0}m", this.HorizontalDilution.Value); }
            if (this.Altitude.HasValue) { sb.AppendFormat(CultureInfo.InvariantCulture, " {0}m", this.Altitude.Value); }
            if (this.GeoidalSeparation.HasValue) { sb.AppendFormat(CultureInfo.InvariantCulture, " {0}m", this.GeoidalSeparation.Value); }
            if (this.DifferentialDataAge.HasValue) { sb.AppendFormat(CultureInfo.InvariantCulture, " {0}s", this.DifferentialDataAge.Value); }
            if (this.DifferentialStationIdentifier.HasValue) { sb.AppendFormat(CultureInfo.InvariantCulture, " {0}", this.DifferentialStationIdentifier.Value); }
            return sb.ToString();
        }

    }





    /// <summary>
    /// NMEA GLL (Geographic Position - Latitude/Longitude) sentence.
    /// </summary>
    public class NmeaGllSentence : NmeaSentence {

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="talkerIdentifier">Talker identifier, usually GP.</param>
        /// <param name="sentenceIdentifier">Sentence identifier.</param>
        /// <param name="components">All inner message components.</param>
        internal NmeaGllSentence(string talkerIdentifier, string sentenceIdentifier, string[] components)
            : base(talkerIdentifier, sentenceIdentifier, components) {
            if (components.Length > 3) {
                this.Latitude = NmeaLatitude.ParseNmea(components[0], components[1]);
                this.Longitude = NmeaLongitude.ParseNmea(components[2], components[3]);
            }
            if (components.Length > 4) {
                string dtComponent = components[4];
                if (dtComponent.Length == 6) {
                    int h, m, s;
                    if ((int.TryParse(dtComponent.Substring(0, 2), NumberStyles.Integer, CultureInfo.InvariantCulture, out h)) && (int.TryParse(dtComponent.Substring(2, 2), NumberStyles.Integer, CultureInfo.InvariantCulture, out m)) && (int.TryParse(dtComponent.Substring(4, 2), NumberStyles.Integer, CultureInfo.InvariantCulture, out s))) {
                        this.UtcTime = new DateTime(1, 1, 1, h, m, s, DateTimeKind.Utc);
                    }
                }
            }
            if (components.Length > 5) {
                switch (components[5].ToUpperInvariant()) {
                    case "A":
                        this.Status = NmeaDataStatus.DataValid;
                        break;
                    case "V":
                        this.Status = NmeaDataStatus.DataInvalid;
                        break;
                    default:
                        this.Status = NmeaDataStatus.Unknown;
                        break;
                }
            } else {
                this.Status = NmeaDataStatus.Unknown;
            }
            if (components.Length > 6) {
                switch (components[6].ToUpperInvariant()) {
                    case "A":
                        this.FaaMode = NmeaFaaModeIndicator.AutonomousMode;
                        break;
                    case "D":
                        this.FaaMode = NmeaFaaModeIndicator.DifferentialMode;
                        break;
                    case "E":
                        this.FaaMode = NmeaFaaModeIndicator.EstimatedMode;
                        break;
                    case "M":
                        this.FaaMode = NmeaFaaModeIndicator.ManualInputMode;
                        break;
                    case "S":
                        this.FaaMode = NmeaFaaModeIndicator.SimulatedMode;
                        break;
                    case "N":
                        this.FaaMode = NmeaFaaModeIndicator.DataNotValid;
                        break;
                    default:
                        this.FaaMode = NmeaFaaModeIndicator.Unknown;
                        break;
                }
            } else {
                this.FaaMode = NmeaFaaModeIndicator.Unknown;
            }
        }

        /// <summary>
        /// Gets latitude if one exists or null otherwise.
        /// </summary>
        public NmeaLatitude? Latitude { get; private set; }

        /// <summary>
        /// Gets longitude if one exists or null otherwise.
        /// </summary>
        public NmeaLongitude? Longitude { get; private set; }

        /// <summary>
        /// Gets utc time if one exists or null otherwise.
        /// Only time component is given.
        /// </summary>
        public DateTime? UtcTime { get; private set; }

        /// <summary>
        /// Gets whether data is valid.
        /// </summary>
        public NmeaDataStatus Status { get; private set; }

        /// <summary>
        /// Gets FAA mode indicator.
        /// </summary>
        public NmeaFaaModeIndicator FaaMode { get; private set; }


        /// <summary>
        /// Returns a System.String that represents the current object.
        /// </summary>
        public override string ToString() {
            StringBuilder sb = new StringBuilder(base.ToString() + ":");
            if (this.Latitude.HasValue) { sb.AppendFormat(CultureInfo.InvariantCulture, " {0}", this.Latitude.Value); }
            if (this.Longitude.HasValue) { sb.AppendFormat(CultureInfo.InvariantCulture, " {0}", this.Longitude.Value); }
            if (this.UtcTime.HasValue) { sb.AppendFormat(CultureInfo.InvariantCulture, " {0:HH:mm:ss\\Z}", this.UtcTime.Value); }
            sb.AppendFormat(CultureInfo.InvariantCulture, " {0}", this.Status);
            sb.AppendFormat(CultureInfo.InvariantCulture, " {0}", this.FaaMode);
            return sb.ToString();
        }

    }





    /// <summary>
    /// NMEA GSA (GPS DOP and active satellites) sentence.
    /// </summary>
    public class NmeaGsaSentence : NmeaSentence {

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="talkerIdentifier">Talker identifier, usually GP.</param>
        /// <param name="sentenceIdentifier">Sentence identifier.</param>
        /// <param name="components">All inner message components.</param>
        internal NmeaGsaSentence(string talkerIdentifier, string sentenceIdentifier, string[] components)
            : base(talkerIdentifier, sentenceIdentifier, components) {
            if (components.Length > 0) {
                switch (components[0].ToUpperInvariant()) {
                    case "M":
                        this.SelectionMode = NmeaSatelliteSelectionMode.Manual;
                        break;
                    case "A":
                        this.SelectionMode = NmeaSatelliteSelectionMode.Automatic;
                        break;
                    default:
                        this.SelectionMode = NmeaSatelliteSelectionMode.Unknown;
                        break;
                }
            } else {
                this.SelectionMode = NmeaSatelliteSelectionMode.Unknown;
            }

            if (components.Length > 1) {
                switch (components[1].ToUpperInvariant()) {
                    case "1":
                        this.Mode = NmeaSatelliteFixMode.NoFix;
                        break;
                    case "2":
                        this.Mode = NmeaSatelliteFixMode.TwoDimensional;
                        break;
                    case "3":
                        this.Mode = NmeaSatelliteFixMode.ThreeDimensional;
                        break;
                    default:
                        this.Mode = NmeaSatelliteFixMode.Unknown;
                        break;
                }
            } else {
                this.Mode = NmeaSatelliteFixMode.Unknown;
            }

            for (int i = 2; i < Math.Min(12 + 2, components.Length); ++i) {
                int id;
                if (int.TryParse(components[i], NumberStyles.Integer, CultureInfo.InvariantCulture, out id)) {
                    this._satellitePrns.Add(id);
                }
            }

            if (components.Length > 14) {
                double value;
                if (double.TryParse(components[14], NumberStyles.Number, CultureInfo.InvariantCulture, out value)) {
                    this.PrecisionDilution = value;
                }
            }

            if (components.Length > 15) {
                double value;
                if (double.TryParse(components[15], NumberStyles.Number, CultureInfo.InvariantCulture, out value)) {
                    this.HorizontalPrecisionDilution = value;
                }
            }

            if (components.Length > 16) {
                double value;
                if (double.TryParse(components[16], NumberStyles.Number, CultureInfo.InvariantCulture, out value)) {
                    this.VerticalPrecisionDilution = value;
                }
            }
        }

        /// <summary>
        /// Gets selection mode.
        /// </summary>
        public NmeaSatelliteSelectionMode SelectionMode { get; private set; }

        /// <summary>
        /// Gets fix mode.
        /// </summary>
        public NmeaSatelliteFixMode Mode { get; private set; }

        private List<int> _satellitePrns = new List<int>();

        /// <summary>
        /// Returns count of satellite .
        /// </summary>
        public int SatellitePrnCount { get { return this._satellitePrns.Count; } }

        /// <summary>
        /// Returns satellite identificators.
        /// </summary>
        public int[] GetSatellitePrns() {
            return this._satellitePrns.ToArray();
        }

        /// <summary>
        /// Gets dilution of precision. DOP is unitless and can only be compared meaningfully to other DOP figures.
        /// </summary>
        public double? PrecisionDilution { get; private set; }

        /// <summary>
        /// Gets horizontal dilution of precision. DOP is unitless and can only be compared meaningfully to other DOP figures.
        /// </summary>
        public double? HorizontalPrecisionDilution { get; private set; }

        /// <summary>
        /// Gets vertical dilution of precision. DOP is unitless and can only be compared meaningfully to other DOP figures.
        /// </summary>
        public double? VerticalPrecisionDilution { get; private set; }



        /// <summary>
        /// Returns a System.String that represents the current object.
        /// </summary>
        public override string ToString() {
            StringBuilder sb = new StringBuilder(base.ToString() + ":");
            sb.AppendFormat(CultureInfo.InvariantCulture, " {0}", this.SelectionMode);
            sb.AppendFormat(CultureInfo.InvariantCulture, " {0}", this.Mode);
            for (int i = 0; i < this._satellitePrns.Count; ++i) {
                sb.AppendFormat(CultureInfo.InvariantCulture, " {0}", this._satellitePrns[i]);
            }
            if (this.PrecisionDilution.HasValue) { sb.AppendFormat(CultureInfo.InvariantCulture, " {0}", this.PrecisionDilution.Value); }
            if (this.HorizontalPrecisionDilution.HasValue) { sb.AppendFormat(CultureInfo.InvariantCulture, " {0}", this.HorizontalPrecisionDilution.Value); }
            if (this.VerticalPrecisionDilution.HasValue) { sb.AppendFormat(CultureInfo.InvariantCulture, " {0}", this.VerticalPrecisionDilution.Value); }
            return sb.ToString();
        }

    }





    /// <summary>
    /// NMEA GSV (Satellites in view) sentence.
    /// </summary>
    public class NmeaGsvSentence : NmeaSentence {

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="talkerIdentifier">Talker identifier, usually GP.</param>
        /// <param name="sentenceIdentifier">Sentence identifier.</param>
        /// <param name="components">All inner message components.</param>
        internal NmeaGsvSentence(string talkerIdentifier, string sentenceIdentifier, string[] components)
            : base(talkerIdentifier, sentenceIdentifier, components) {
            if (components.Length > 0) {
                int value;
                if (int.TryParse(components[0], NumberStyles.Number, CultureInfo.InvariantCulture, out value)) {
                    this.TotalMessageCount = value;
                }
            }
            if (components.Length > 1) {
                int value;
                if (int.TryParse(components[1], NumberStyles.Number, CultureInfo.InvariantCulture, out value)) {
                    this.MessageNumber = value;
                }
            }
            if (components.Length > 2) {
                int value;
                if (int.TryParse(components[2], NumberStyles.Number, CultureInfo.InvariantCulture, out value)) {
                    this.SatellitesInView = value;
                }
            }

            int offset = 3;
            while (components.Length > offset + 3) {
                int prn;
                int elevation;
                int azimuth;
                if (int.TryParse(components[offset + 0], NumberStyles.Number, CultureInfo.InvariantCulture, out prn) && int.TryParse(components[offset + 1], NumberStyles.Number, CultureInfo.InvariantCulture, out elevation) && int.TryParse(components[offset + 2], NumberStyles.Number, CultureInfo.InvariantCulture, out azimuth)) {
                    int snr;
                    if (int.TryParse(components[offset + 3], NumberStyles.Number, CultureInfo.InvariantCulture, out snr)) {
                        this._satelliteInformation.Add(new NmeaSatelliteInformation(prn, elevation, azimuth, snr));
                    } else {
                        this._satelliteInformation.Add(new NmeaSatelliteInformation(prn, elevation, azimuth, 0));
                    }
                }
                offset += 4;
            }
        }

        /// <summary>
        /// Gets total number of messages.
        /// </summary>
        public int TotalMessageCount { get; private set; }

        /// <summary>
        /// Gets message number.
        /// </summary>
        public int MessageNumber { get; private set; }

        /// <summary>
        /// Gets message number.
        /// </summary>
        public int SatellitesInView { get; private set; }

        private List<NmeaSatelliteInformation> _satelliteInformation = new List<NmeaSatelliteInformation>();

        /// <summary>
        /// Returns count of satellite information.
        /// </summary>
        public int SatelliteInformationCount { get { return this._satelliteInformation.Count; } }

        /// <summary>
        /// Returns information gathered from satellite.
        /// </summary>
        public NmeaSatelliteInformation[] GetSatelliteInformation() {
            return this._satelliteInformation.ToArray();
        }



        /// <summary>
        /// Returns a System.String that represents the current object.
        /// </summary>
        public override string ToString() {
            StringBuilder sb = new StringBuilder(base.ToString() + ":");
            sb.AppendFormat(CultureInfo.InvariantCulture, " {0}/{1}", this.MessageNumber, this.TotalMessageCount);
            sb.AppendFormat(CultureInfo.InvariantCulture, " {0}", this.SatellitesInView);
            for (int i = 0; i < this._satelliteInformation.Count; ++i) {
                sb.AppendFormat(CultureInfo.InvariantCulture, " {0} {1} {2}° {3}dB", this._satelliteInformation[i].Prn, this._satelliteInformation[i].Elevation, this._satelliteInformation[i].Azimuth, this._satelliteInformation[i].Snr);
            }
            return sb.ToString();
        }

    }





    /// <summary>
    /// NMEA RMC (Recommended Minimum Navigation Information) sentence.
    /// </summary>
    public class NmeaRmcSentence : NmeaSentence {

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="talkerIdentifier">Talker identifier, usually GP.</param>
        /// <param name="sentenceIdentifier">Sentence identifier.</param>
        /// <param name="components">All inner message components.</param>
        internal NmeaRmcSentence(string talkerIdentifier, string sentenceIdentifier, string[] components)
            : base(talkerIdentifier, sentenceIdentifier, components) {
            if (components.Length > 8) {
                int yyyy = 1;
                int mm = 1;
                int dd = 1;
                int hh = 0;
                int nn = 0;
                int ss = 0;
                int ms = 0;

                string dateComponent = components[8];
                if ((dateComponent.Length == 6)) {
                    int y;
                    int m;
                    int d;
                    if ((int.TryParse(dateComponent.Substring(0, 2), NumberStyles.Integer, CultureInfo.InvariantCulture, out d)) && (int.TryParse(dateComponent.Substring(2, 2), NumberStyles.Integer, CultureInfo.InvariantCulture, out m)) && (int.TryParse(dateComponent.Substring(4), NumberStyles.Number, CultureInfo.InvariantCulture, out y))) {
                        yyyy = 2000 + y;
                        mm = m;
                        dd = d;

                        string timeComponent = components[0];
                        if ((timeComponent.Length >= 6)) {
                            int h, n;
                            decimal s;
                            if ((int.TryParse(timeComponent.Substring(0, 2), NumberStyles.Integer, CultureInfo.InvariantCulture, out h)) && (int.TryParse(timeComponent.Substring(2, 2), NumberStyles.Integer, CultureInfo.InvariantCulture, out n)) && (decimal.TryParse(timeComponent.Substring(4), NumberStyles.Number, CultureInfo.InvariantCulture, out s))) {
                                hh = h;
                                nn = n;
                                ss = (int)Math.Truncate(s);
                                ms = (int)((s - ss) * 1000);

                                this.UtcTime = new DateTime(yyyy, mm, dd, hh, nn, ss, ms, DateTimeKind.Utc);
                            }
                        }
                    }
                }
            }
            if (components.Length > 1) {
                switch (components[1].ToUpperInvariant()) {
                    case "A":
                        this.Status = NmeaNavigationStatus.Valid;
                        break;
                    case "V":
                        this.Status = NmeaNavigationStatus.NavigationReceiverWarning;
                        break;
                    default:
                        this.Status = NmeaNavigationStatus.Unknown;
                        break;
                }
            } else {
                this.Status = NmeaNavigationStatus.Unknown;
            }
            if (components.Length > 5) {
                this.Latitude = NmeaLatitude.ParseNmea(components[2], components[3]);
                this.Longitude = NmeaLongitude.ParseNmea(components[4], components[5]);
            }
            if (components.Length > 6) {
                double value;
                if (double.TryParse(components[6], NumberStyles.Number, CultureInfo.InvariantCulture, out value)) {
                    this.Speed = value * 0.514444444; //as given by Google
                }
            }
            if (components.Length > 7) {
                double value;
                if (double.TryParse(components[7], NumberStyles.Number, CultureInfo.InvariantCulture, out value)) {
                    this.TrueCourse = value;
                }
            }
            if (components.Length > 10) {
                double value;
                if (double.TryParse(components[9], NumberStyles.Number, CultureInfo.InvariantCulture, out value)) {
                    switch (components[10].ToUpperInvariant()) {
                        case "E":
                            this.MagneticVariation = +value;
                            break;
                        case "W":
                            this.MagneticVariation = -value;
                            break;
                    }
                }
            }

            if (components.Length > 11) {
                switch (components[11].ToUpperInvariant()) {
                    case "A":
                        this.FaaMode = NmeaFaaModeIndicator.AutonomousMode;
                        break;
                    case "D":
                        this.FaaMode = NmeaFaaModeIndicator.DifferentialMode;
                        break;
                    case "E":
                        this.FaaMode = NmeaFaaModeIndicator.EstimatedMode;
                        break;
                    case "M":
                        this.FaaMode = NmeaFaaModeIndicator.ManualInputMode;
                        break;
                    case "S":
                        this.FaaMode = NmeaFaaModeIndicator.SimulatedMode;
                        break;
                    case "N":
                        this.FaaMode = NmeaFaaModeIndicator.DataNotValid;
                        break;
                    default:
                        this.FaaMode = NmeaFaaModeIndicator.Unknown;
                        break;
                }
            } else {
                this.FaaMode = NmeaFaaModeIndicator.Unknown;
            }
        }

        /// <summary>
        /// Gets utc time if one exists or null otherwise.
        /// Only time component is given.
        /// </summary>
        public DateTime? UtcTime { get; private set; }

        /// <summary>
        /// Gets whether status is valid.
        /// </summary>
        public NmeaNavigationStatus Status { get; private set; }

        /// <summary>
        /// Gets latitude if one exists or null otherwise.
        /// </summary>
        public NmeaLatitude? Latitude { get; private set; }

        /// <summary>
        /// Gets longitude if one exists or null otherwise.
        /// </summary>
        public NmeaLongitude? Longitude { get; private set; }

        /// <summary>
        /// Gets speed over ground (meters per second).
        /// </summary>
        public double? Speed { get; private set; }

        /// <summary>
        /// Gets track made good (degrees).
        /// </summary>
        public double? TrueCourse { get; private set; }

        /// <summary>
        /// Gets Magnetic variation (degrees).
        /// Negative values represent west, positive values represent east.
        /// </summary>
        public double? MagneticVariation { get; private set; }

        /// <summary>
        /// Gets FAA mode indicator.
        /// </summary>
        public NmeaFaaModeIndicator FaaMode { get; private set; }


        /// <summary>
        /// Returns a System.String that represents the current object.
        /// </summary>
        public override string ToString() {
            StringBuilder sb = new StringBuilder(base.ToString() + ":");
            if (this.UtcTime.HasValue) { sb.AppendFormat(CultureInfo.InvariantCulture, " {0:yyyy-MM-dd\\THH:mm:ss\\Z}", this.UtcTime.Value); }
            sb.AppendFormat(CultureInfo.InvariantCulture, " {0}", this.Status);
            if (this.Latitude.HasValue) { sb.AppendFormat(CultureInfo.InvariantCulture, " {0}", this.Latitude.Value); }
            if (this.Longitude.HasValue) { sb.AppendFormat(CultureInfo.InvariantCulture, " {0}", this.Longitude.Value); }
            if (this.Speed.HasValue) { sb.AppendFormat(CultureInfo.InvariantCulture, " {0}m/s", this.Speed.Value); }
            if (this.TrueCourse.HasValue) { sb.AppendFormat(CultureInfo.InvariantCulture, " {0}°", this.TrueCourse.Value); }
            if (this.MagneticVariation.HasValue) { sb.AppendFormat(CultureInfo.InvariantCulture, " {0}°", this.MagneticVariation.Value); }
            sb.AppendFormat(CultureInfo.InvariantCulture, " {0}", this.FaaMode);
            return sb.ToString();
        }

    }





    /// <summary>
    /// NMEA VTG (Track made good and Ground speed) sentence.
    /// </summary>
    public class NmeaVtgSentence : NmeaSentence {

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="talkerIdentifier">Talker identifier, usually GP.</param>
        /// <param name="sentenceIdentifier">Sentence identifier.</param>
        /// <param name="components">All inner message components.</param>
        internal NmeaVtgSentence(string talkerIdentifier, string sentenceIdentifier, string[] components)
            : base(talkerIdentifier, sentenceIdentifier, components) {
            if ((components.Length > 1) && (string.Compare(components[1], "T", StringComparison.OrdinalIgnoreCase) == 0)) {//new format;

                if (components.Length > 1) {
                    if (string.Compare(components[1], "T", StringComparison.OrdinalIgnoreCase) == 0) {
                        double value;
                        if (double.TryParse(components[0], NumberStyles.Number, CultureInfo.InvariantCulture, out value)) {
                            this.Course = value;
                        }
                    }
                }

                if (components.Length > 3) {
                    if (string.Compare(components[3], "M", StringComparison.OrdinalIgnoreCase) == 0) {
                        double value;
                        if (double.TryParse(components[2], NumberStyles.Number, CultureInfo.InvariantCulture, out value)) {
                            this.MagneticCourse = value;
                        }
                    }
                }

                if (components.Length > 5) {
                    if (string.Compare(components[5], "N", StringComparison.OrdinalIgnoreCase) == 0) {
                        double value;
                        if (double.TryParse(components[4], NumberStyles.Number, CultureInfo.InvariantCulture, out value)) {
                            this.Speed = value * 0.514444444; //as given by Google (knots -> m/s)
                        }
                    }
                    if ((!this.Speed.HasValue) && (components.Length > 7)) {
                        if (string.Compare(components[7], "K", StringComparison.OrdinalIgnoreCase) == 0) {
                            double valueKmh;
                            if (double.TryParse(components[6], NumberStyles.Number, CultureInfo.InvariantCulture, out valueKmh)) {
                                this.Speed = valueKmh * 0.277777778; //as given by Google (km/h -> m/s)
                            }
                        }
                    }
                }

                if (components.Length > 8) {
                    switch (components[8].ToUpperInvariant()) {
                        case "A":
                            this.FaaMode = NmeaFaaModeIndicator.AutonomousMode;
                            break;
                        case "D":
                            this.FaaMode = NmeaFaaModeIndicator.DifferentialMode;
                            break;
                        case "E":
                            this.FaaMode = NmeaFaaModeIndicator.EstimatedMode;
                            break;
                        case "M":
                            this.FaaMode = NmeaFaaModeIndicator.ManualInputMode;
                            break;
                        case "S":
                            this.FaaMode = NmeaFaaModeIndicator.SimulatedMode;
                            break;
                        case "N":
                            this.FaaMode = NmeaFaaModeIndicator.DataNotValid;
                            break;
                        default:
                            this.FaaMode = NmeaFaaModeIndicator.Unknown;
                            break;
                    }
                } else {
                    this.FaaMode = NmeaFaaModeIndicator.Unknown;
                }

            } else { //old format

                if (components.Length > 0) {
                    double value;
                    if (double.TryParse(components[0], NumberStyles.Number, CultureInfo.InvariantCulture, out value)) {
                        this.Course = value;
                    }
                }

                if (components.Length > 1) {
                    double value;
                    if (double.TryParse(components[1], NumberStyles.Number, CultureInfo.InvariantCulture, out value)) {
                        this.MagneticCourse = value;
                    }
                }

                if (components.Length > 2) {
                    double value;
                    if (double.TryParse(components[2], NumberStyles.Number, CultureInfo.InvariantCulture, out value)) {
                        this.Speed = value * 0.514444444; //as given by Google (knots -> m/s)
                    }

                    if ((!this.Speed.HasValue) && (components.Length > 3)) {
                        double valueKmh;
                        if (double.TryParse(components[3], NumberStyles.Number, CultureInfo.InvariantCulture, out valueKmh)) {
                            this.Speed = valueKmh * 0.277777778; //as given by Google (km/h -> m/s)
                        }
                    }
                }

                this.FaaMode = NmeaFaaModeIndicator.Unknown;
            }
        }

        /// <summary>
        /// Gets true course over ground (degrees) 000 to 359.
        /// </summary>
        public double? Course { get; private set; }

        /// <summary>
        /// Gets magnetic course over ground (degrees) 000 to 359.
        /// </summary>
        public double? MagneticCourse { get; private set; }

        /// <summary>
        /// Gets speed over ground (meters per second).
        /// </summary>
        public double? Speed { get; private set; }

        /// <summary>
        /// Gets FAA mode indicator.
        /// </summary>
        public NmeaFaaModeIndicator FaaMode { get; private set; }


        /// <summary>
        /// Returns a System.String that represents the current object.
        /// </summary>
        public override string ToString() {
            StringBuilder sb = new StringBuilder(base.ToString() + ":");
            if (this.Course.HasValue) { sb.AppendFormat(CultureInfo.InvariantCulture, " {0}°", this.Course.Value); }
            if (this.MagneticCourse.HasValue) { sb.AppendFormat(CultureInfo.InvariantCulture, " {0}°", this.MagneticCourse.Value); }
            if (this.Speed.HasValue) { sb.AppendFormat(CultureInfo.InvariantCulture, " {0}m/s", this.Speed.Value); }
            sb.AppendFormat(CultureInfo.InvariantCulture, " {0}", this.FaaMode);
            return sb.ToString();
        }

    }

}
