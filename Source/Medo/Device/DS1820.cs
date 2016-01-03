//Josip Medved <jmedved@jmedved.com>   www.medo64.com

//2012-05-16: Initial version.


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace Medo.Device {

    /// <summary>
    /// Handling of DS1820, DS18S20 and DS18B20 temperature sensors.
    /// </summary>
    [DebuggerDisplay(@"{RomCode.ToString(""X8"") + "" at "" + Temperature.ToString(""0.0' °C'"")}")]
    public class DS1820 {

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="romCode">ROM code for device. It can be in both MSB and LSB order.</param>
        /// <param name="scratchpad">Memory state of device. It can be in both MSB and LSB order.</param>
        /// <exception cref="System.ArgumentNullException">ROM code cannot be null. -or- Scratchpad cannot be null.</exception>
        /// <remarks>
        /// ROM code must be 8 bytes and scratchpad must be 9 bytes.
        /// If arrays are of different length, data might be filled but there are no guarantees.
        /// Since data in array is used for endian detection if that data is not correct, default alignment will be kept. For ROM code that is LSB first and for scratchpad this is byte 0 first.
        /// </remarks>
        public DS1820(Byte[] romCode, Byte[] scratchpad) {
            if (romCode == null) { throw new ArgumentNullException("romCode", "ROM code cannot be null."); }
            if (scratchpad == null) { throw new ArgumentNullException("scratchpad", "Scratchpad cannot be null."); }

            ProcessRomCode(romCode);
            ProcessScratchpad(scratchpad);
        }


        private void ProcessRomCode(byte[] romCode) {
            var romCodeEx = ExpandArray(romCode, 8);
            if (((romCodeEx[7] == 0x10) || (romCodeEx[7] == 0x28)) && (romCodeEx[0] != 0x10) && (romCodeEx[0] != 0x28)) { Array.Reverse(romCodeEx, 0, 8); }

            this.IsRomCodeValid = (GetCrc8Digest(romCodeEx, 0, 7) == romCodeEx[7]) && (romCode.Length == 8) && IsNotEmpty(romCode, 0, 8);

            if ((this.IsRomCodeValid ) && (romCode.Length >= 8)) {
                this.RomCode = BitConverter.ToInt64(BitConverter.IsLittleEndian ? new byte[] { romCode[0], romCode[1], romCode[2], romCode[3], romCode[4], romCode[5], romCode[6], romCode[7] } : new byte[] { romCode[7], romCode[6], romCode[5], romCode[4], romCode[3], romCode[2], romCode[1], romCode[0] }, 0);
            }

            this.Family = romCodeEx[0];

            if (romCode.Length >= 7) {
                this.Serial = BitConverter.ToInt64(BitConverter.IsLittleEndian ? new byte[] { romCodeEx[1], romCodeEx[2], romCodeEx[3], romCodeEx[4], romCodeEx[5], romCodeEx[6], 0, 0 } : new byte[] { 0, 0, romCodeEx[6], romCodeEx[5], romCodeEx[4], romCodeEx[3], romCodeEx[2], romCodeEx[1] }, 0);
            } else {
                this.Serial = -1;
            }
        }

        private void ProcessScratchpad(byte[] scratchpad) {
            var scratchpadEx = ExpandArray(scratchpad, 9);
            if (scratchpadEx[3] == 0xFF) { Array.Reverse(scratchpadEx, 0, 9); }

            this.IsScratchpadValid = (GetCrc8Digest(scratchpadEx, 0, 8) == scratchpadEx[8]) && (scratchpad.Length == 9) && IsNotEmpty(scratchpadEx, 0, 9);

            if (scratchpad.Length >= 2) {
                short temperatureValue = (BitConverter.IsLittleEndian) ? BitConverter.ToInt16(scratchpadEx, 0) : BitConverter.ToInt16(new byte[] { scratchpadEx[1], scratchpadEx[0] }, 0);
                switch (this.Family) {
                    case 0x10: { //DS18S20
                            if ((scratchpadEx[0] == 0x50) && (scratchpadEx[1] == 0x05)) { //Error measurement of 85°C
                                this.Temperature = double.NaN;
                            } else {
                                var countRemain = scratchpadEx[6];
                                var countPerC = scratchpadEx[7];
                                if (countPerC != 0) {
                                    this.Temperature = (temperatureValue * 0.5) - 0.25F + (double)((countPerC - countRemain) / countPerC);
                                } else { //probably missing some data.
                                    this.Temperature = temperatureValue * 0.5;
                                }
                            }
                            this.Resolution = 9;
                        } break;
                    case 0x28: { //DS18B20
                            if ((scratchpadEx[0] == 0xAA) && (scratchpadEx[1] == 0x00)) { //Error measurement of 85°C
                                this.Temperature = double.NaN;
                            } else {
                                this.Temperature = temperatureValue * 0.0625F;
                            }
                            if (scratchpad.Length >= 5) {
                                this.Resolution = ((scratchpadEx[4] & 0x9F) == 0x1F) ? 9 + ((scratchpadEx[4] & 0x60) >> 5) : 0;
                            }
                        } break;
                    default: {
                            this.Temperature = double.NaN;
                        } break;
                }
            } else {
                this.Temperature = double.NaN;
            }


            if (scratchpad.Length >= 3) {
                this.UserByte1 = scratchpadEx[2];
                this.TemperatureHigh = (((scratchpadEx[2] & 0x80) == 0x80) ? -1 : 1) * (scratchpadEx[2] & 0x7F);
            } else {
                this.UserByte1 = 0;
                this.TemperatureHigh = double.NaN;
            }

            if (scratchpad.Length >= 4) {
                this.UserByte2 = scratchpadEx[3];
                this.TemperatureLow = (((scratchpadEx[3] & 0x80) == 0x80) ? -1 : 1) * (scratchpadEx[3] & 0x7F);
            } else {
                this.UserByte2 = 0;
                this.TemperatureLow = double.NaN;
            }
        }


        /// <summary>
        /// True if ROM code could be successfully parsed and CRC is valid.
        /// </summary>
        public Boolean IsRomCodeValid { get; private set; }

        /// <summary>
        /// 64-bit ROM code.
        /// If value cannot be determined, it will be 0.
        /// If CRC fails, value will be 0.
        /// </summary>
        public Int64 RomCode { get; private set; }

        /// <summary>
        /// Device family.
        /// </summary>
        public Int32 Family { get; private set; }

        /// <summary>
        /// 48-bit serial number of device.
        /// This field does not include CRC nor Family-code.
        /// If value cannot be determined, it will be -1.
        /// </summary>
        public Int64 Serial { get; private set; }

        /// <summary>
        /// True if scratchpad could be successfully parsed and CRC is valid.
        /// </summary>
        public Boolean IsScratchpadValid { get; private set; }

        /// <summary>
        /// Sensor temperature.
        /// For DS18S20 this is adjusted for greater precision.
        /// If value cannot be determined, it will be NaN.
        /// </summary>
        public Double Temperature { get; private set; }

        /// <summary>
        /// User byte 1 register.
        /// This is same register as TemperatureHigh. However, values are different because of different formatting rules.
        /// If value cannot be determined, it will be 0.
        /// </summary>
        public Byte UserByte1 { get; private set; }

        /// <summary>
        /// User byte 2 register.
        /// This is same register as TemperatureLow. However, values are different because of different formatting rules.
        /// If value cannot be determined, it will be 0.
        /// </summary>
        public Byte UserByte2 { get; private set; }


        /// <summary>
        /// T(H) register from device.
        /// This is same register as UserByte1. However, values are different because of different formatting rules.
        /// If value cannot be determined, it will be NaN.
        /// </summary>
        public Double TemperatureHigh { get; private set; }

        /// <summary>
        /// T(L) register from device.
        /// This is same register as UserByte2. However, values are different because of different formatting rules.
        /// If value cannot be determined, it will be NaN.
        /// </summary>
        public Double TemperatureLow { get; private set; }

        /// <summary>
        /// Temperature resolution in bits.
        /// If value cannot be determined, it will be 0.
        /// </summary>
        public Int32 Resolution { get; private set; }


        /// <summary>
        /// Returns parsed data.
        /// </summary>
        /// <param name="romCodeAndScratchpad">Hexadecimal representation of 8-bytes of ROM code followed by 9 bytes of scratchpad.</param>
        /// <returns>Parsed data.</returns>
        /// <exception cref="System.ArgumentNullException">Data cannot be null.</exception>
        public static DS1820 Parse(String romCodeAndScratchpad) {
            if (romCodeAndScratchpad == null) { throw new ArgumentNullException("romCodeAndScratchpad", "Data cannot be null."); }
            return Parse(FromHex(romCodeAndScratchpad));
        }

        /// <summary>
        /// Returns parsed data.
        /// </summary>
        /// <param name="romCodeAndScratchpad">8-bytes of ROM code followed by 9 bytes of scratchpad.</param>
        /// <returns>Parsed data.</returns>
        /// <exception cref="System.ArgumentNullException">Data cannot be null.</exception>
        public static DS1820 Parse(Byte[] romCodeAndScratchpad) {
            if (romCodeAndScratchpad == null) { throw new ArgumentNullException("romCodeAndScratchpad", "Data cannot be null."); }

            if (romCodeAndScratchpad.Length <= 8) {
                return new DS1820(romCodeAndScratchpad, new byte[] { });
            } else {
                var romCode = new byte[8];
                var scratchpad = new byte[romCodeAndScratchpad.Length - 8];
                Buffer.BlockCopy(romCodeAndScratchpad, 0, romCode, 0, romCode.Length);
                Buffer.BlockCopy(romCodeAndScratchpad, 8, scratchpad, 0, scratchpad.Length);
                return new DS1820(romCode, scratchpad);
            }
        }

        /// <summary>
        /// Returns parsed data.
        /// </summary>
        /// <param name="romCode">8 bytes of ROM code in their hexadecimal representation.</param>
        /// <param name="scratchpad">9 bytes of scratchpad in their hexadecimal representation.</param>
        /// <returns>Parsed data.</returns>
        /// <exception cref="System.ArgumentNullException">ROM code cannot be null. -or- Scratchpad cannot be null.</exception>
        public static DS1820 Parse(String romCode, String scratchpad) {
            if (romCode == null) { throw new ArgumentNullException("romCode", "ROM code cannot be null."); }
            if (scratchpad == null) { throw new ArgumentNullException("scratchpad", "Scratchpad cannot be null."); }
            return new DS1820(FromHex(romCode), FromHex(scratchpad));
        }

        /// <summary>
        /// Returns parsed data.
        /// </summary>
        /// <param name="romCode">8 bytes of ROM code.</param>
        /// <param name="scratchpad">9 bytes of scratchpad.</param>
        /// <returns>Parsed data.</returns>
        /// <exception cref="System.ArgumentNullException">ROM code cannot be null. -or- Scratchpad cannot be null.</exception>
        public static DS1820 Parse(Byte[] romCode, Byte[] scratchpad) {
            return new DS1820(romCode, scratchpad);
        }


        #region Helper

        private static byte[] ExpandArray(byte[] buffer, int length) {
            if (buffer.Length < length) {
                var buffer2 = new byte[length];
                Buffer.BlockCopy(buffer, 0, buffer2, 0, buffer.Length);
                return buffer2;
            } else {
                return buffer;
            }
        }

        private static byte[] FromHex(string hex) {
            byte? lowNibble = null;
            var bytes = new Stack<byte>();
            for (int i = hex.Length - 1; i >= 0; i--) {
                var c = hex[i];
                if (char.IsWhiteSpace(c) == false) {
                    byte nibble;
                    if (byte.TryParse(hex.Substring(i, 1), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out nibble)) {
                        if (lowNibble.HasValue) {
                            bytes.Push((byte)((nibble << 4) | lowNibble));
                            lowNibble = null;
                        } else {
                            lowNibble = nibble;
                        }
                    } else {
                        throw new FormatException("Non-hexadecimal data detected.");
                    }
                }
            }
            if (lowNibble.HasValue) { bytes.Push(lowNibble.Value); }
            return bytes.ToArray();
        }

        private static readonly byte[] Crc8Lookup = new byte[] { 0x00, 0x5E, 0xBC, 0xE2, 0x61, 0x3F, 0xDD, 0x83, 0xC2, 0x9C, 0x7E, 0x20, 0xA3, 0xFD, 0x1F, 0x41, 0x9D, 0xC3, 0x21, 0x7F, 0xFC, 0xA2, 0x40, 0x1E, 0x5F, 0x01, 0xE3, 0xBD, 0x3E, 0x60, 0x82, 0xDC, 0x23, 0x7D, 0x9F, 0xC1, 0x42, 0x1C, 0xFE, 0xA0, 0xE1, 0xBF, 0x5D, 0x03, 0x80, 0xDE, 0x3C, 0x62, 0xBE, 0xE0, 0x02, 0x5C, 0xDF, 0x81, 0x63, 0x3D, 0x7C, 0x22, 0xC0, 0x9E, 0x1D, 0x43, 0xA1, 0xFF, 0x46, 0x18, 0xFA, 0xA4, 0x27, 0x79, 0x9B, 0xC5, 0x84, 0xDA, 0x38, 0x66, 0xE5, 0xBB, 0x59, 0x07, 0xDB, 0x85, 0x67, 0x39, 0xBA, 0xE4, 0x06, 0x58, 0x19, 0x47, 0xA5, 0xFB, 0x78, 0x26, 0xC4, 0x9A, 0x65, 0x3B, 0xD9, 0x87, 0x04, 0x5A, 0xB8, 0xE6, 0xA7, 0xF9, 0x1B, 0x45, 0xC6, 0x98, 0x7A, 0x24, 0xF8, 0xA6, 0x44, 0x1A, 0x99, 0xC7, 0x25, 0x7B, 0x3A, 0x64, 0x86, 0xD8, 0x5B, 0x05, 0xE7, 0xB9, 0x8C, 0xD2, 0x30, 0x6E, 0xED, 0xB3, 0x51, 0x0F, 0x4E, 0x10, 0xF2, 0xAC, 0x2F, 0x71, 0x93, 0xCD, 0x11, 0x4F, 0xAD, 0xF3, 0x70, 0x2E, 0xCC, 0x92, 0xD3, 0x8D, 0x6F, 0x31, 0xB2, 0xEC, 0x0E, 0x50, 0xAF, 0xF1, 0x13, 0x4D, 0xCE, 0x90, 0x72, 0x2C, 0x6D, 0x33, 0xD1, 0x8F, 0x0C, 0x52, 0xB0, 0xEE, 0x32, 0x6C, 0x8E, 0xD0, 0x53, 0x0D, 0xEF, 0xB1, 0xF0, 0xAE, 0x4C, 0x12, 0x91, 0xCF, 0x2D, 0x73, 0xCA, 0x94, 0x76, 0x28, 0xAB, 0xF5, 0x17, 0x49, 0x08, 0x56, 0xB4, 0xEA, 0x69, 0x37, 0xD5, 0x8B, 0x57, 0x09, 0xEB, 0xB5, 0x36, 0x68, 0x8A, 0xD4, 0x95, 0xCB, 0x29, 0x77, 0xF4, 0xAA, 0x48, 0x16, 0xE9, 0xB7, 0x55, 0x0B, 0x88, 0xD6, 0x34, 0x6A, 0x2B, 0x75, 0x97, 0xC9, 0x4A, 0x14, 0xF6, 0xA8, 0x74, 0x2A, 0xC8, 0x96, 0x15, 0x4B, 0xA9, 0xF7, 0xB6, 0xE8, 0x0A, 0x54, 0xD7, 0x89, 0x6B, 0x35 };

        private static byte GetCrc8Digest(byte[] value, int index, int length) {
            byte digest = 0;
            for (int i = index; i < index + length; i++) {
                digest = Crc8Lookup[digest ^ value[i]];
            }
            return digest;
        }

        private static bool IsNotEmpty(byte[] value, int index, int length) {
            for (int i = index; i < index + length; i++) {
                if (value[i] != 0) { return true; }
            }
            return false;
        }

        #endregion

    }
}
