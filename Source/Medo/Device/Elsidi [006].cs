//Copyright (c) 2008 Josip Medved <jmedved@jmedved.com>

//2008-12-01: New version.
//2010-04-17: Changed namespace from Medo.IO.SerialDevices to Medo.Device.
//2010-07-19: Compatible with Elsidi revG; not compatible with rev 3.
//2012-11-24: Changing methods AddSwithTo*Display to AddSwitchTo*Display.
//2013-01-06: Updated for Elsidi [K].
//2013-01-14: Updated for Elsidi [K 2013-01-14].


using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Ports;
using System.Text;

namespace Medo.Device {

    /// <summary>
    /// Sending data to Elsidi board. Compatible with versions equal to or above K.
    /// </summary>
    public class Elsidi : IDisposable {

        private SerialPort _serial;
        private const byte BS = 0x08; //Return home
        private const byte HT = 0x09; //Command mode
        private const byte LF = 0x0A; //Next line
        private const byte VT = 0x0B; //Clear display
        private const byte FF = 0x0C; //LCD instruction mode
        private const byte CR = 0x0D; //Next line
        private const byte SO = 0x0E; //Select secondary display (E2).
        private const byte SI = 0x0F; //Select primary display (E1).

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="portName">The port to use.</param>
        public Elsidi(String portName) {
            this._serial = new SerialPort(portName, 9600, Parity.None, 8, StopBits.One);
            this._serial.NewLine = System.Convert.ToChar(0x0A).ToString(); //LF
            this._serial.ReadTimeout = 500;
            this._serial.WriteTimeout = 500;
        }


        /// <summary>
        /// Opens serial port.
        /// </summary>
        public void Open() {
            this._serial.DtrEnable = true;
            this._serial.RtsEnable = true;
            this._serial.Open();
            this._serial.DiscardInBuffer();
            this._serial.DiscardOutBuffer();
        }

        /// <summary>
        /// Closes serial port.
        /// </summary>
        public void Close() {
            this._serial.DtrEnable = false;
            this._serial.RtsEnable = false;
            this._serial.Close();
        }


        /// <summary>
        /// Gets or sets timeout for read operations.
        /// </summary>
        public Int32 ReadTimeout {
            get { return this._serial.ReadTimeout; }
            set { this._serial.ReadTimeout = value; }
        }

        /// <summary>
        /// Gets or sets timeout for write operations.
        /// </summary>
        public Int32 WriteTimeout {
            get { return this._serial.WriteTimeout; }
            set { this._serial.WriteTimeout = value; }
        }


        /// <summary>
        /// Writes text to LCD.
        /// Returns true if operation succeeded.
        /// </summary>
        /// <param name="text">ASCII text.</param>
        public Boolean SendText(String text) {
            this._serial.DiscardInBuffer();
            this._serial.DiscardOutBuffer();

            var buffer = ASCIIEncoding.ASCII.GetBytes(text);
            for (int i = 0; i < buffer.Length; i++) {
                if ((buffer[i] >= 0x08) && (buffer[i] <= 0x0F)) { buffer[i] -= 0x08; }
                this._serial.Write(buffer, i, 1);
                var ret = this._serial.ReadByte();
                if (ret != buffer[i]) { return false; }
            }

            return true;
        }

        /// <summary>
        /// Writes instruction to LCD.
        /// Returns true if operation succeeded.
        /// </summary>
        /// <param name="instruction">Instruction.</param>
        public Boolean SendInstruction(Byte instruction) {
            this._serial.DiscardInBuffer();
            this._serial.DiscardOutBuffer();

            var buffer = new byte[] { FF, instruction };
            this._serial.Write(buffer, 0, buffer.Length);
            var ret = this._serial.ReadByte();
            return (ret == LF);
        }


        /// <summary>
        /// Clear all the display data by writing “20H” (space code) to all DDRAM address, and set DDRAM address to “00H” into AC (address counter).
        /// Return cursor to the original status, namely, bring the cursor to the left edge on the fist line of the display.
        /// Make the entry mode increment (I/D=“High”).
        /// Returns true if operation succeeded.
        /// </summary>
        public Boolean ClearDisplay() {
            this._serial.DiscardInBuffer();
            this._serial.DiscardOutBuffer();

            var buffer = new byte[] { VT };
            this._serial.Write(buffer, 0, buffer.Length);
            var ret = this._serial.ReadByte();
            return (ret == LF);
        }

        /// <summary>
        /// Return home is cursor return home instruction.
        /// Set DDRAM address to “00H” into the address counter.
        /// Return cursor to its original site and return display to its original status, if shifted.
        /// Contents of DDRAM does not change.
        /// Returns true if operation succeeded.
        /// </summary>
        public Boolean ReturnHome() {
            this._serial.DiscardInBuffer();
            this._serial.DiscardOutBuffer();

            var buffer = new byte[] { BS };
            this._serial.Write(buffer, 0, buffer.Length);
            var ret = this._serial.ReadByte();
            return (ret == LF);
        }

        /// <summary>
        /// Set the moving direction of cursor and display.
        /// Returns true if operation succeeded.
        /// </summary>
        /// <param name="incrementAddress">If true, DDRAM/CGRAM address will be incremented on every write.</param>
        /// <param name="shiftWholeDisplay">If true, whole display will shift when data is written to.</param>
        public Boolean ChangeEntryMode(Boolean incrementAddress, Boolean shiftWholeDisplay) {
            byte instruction = 0x04;
            if (incrementAddress) { instruction = (byte)(instruction | 0x02); }
            if (shiftWholeDisplay) { instruction = (byte)(instruction | 0x01); }
            return SendInstruction(instruction);
        }

        /// <summary>
        /// Control display/cursor/blink ON/OFF 1 bit register.
        /// Returns true if operation succeeded.
        /// </summary>
        /// <param name="displayOn">If true, display will be on.</param>
        /// <param name="cursorOn">If true, cursor will be displayed.</param>
        /// <param name="cursorBlinkOn">If true, cursor will blink.</param>
        public Boolean ChangeDisplay(Boolean displayOn, Boolean cursorOn, Boolean cursorBlinkOn) {
            byte instruction = 0x08;
            if (displayOn) { instruction = (byte)(instruction | 0x04); }
            if (cursorOn) { instruction = (byte)(instruction | 0x02); }
            if (cursorBlinkOn) { instruction = (byte)(instruction | 0x01); }
            return SendInstruction(instruction);
        }

        /// <summary>
        /// Shifting of right/left cursor position or display without writing or reading of display data.
        /// This instruction is used to correct or search display data.
        /// During 2-line mode display, cursor moves to the 2nd line after the 40th digit of the 1st line.
        /// Note that display shift is performed simultaneously in all the lines.
        /// When display data is shifted repeatedly, each line is shifted individually.
        /// When display shift is performed, the contents of the address counter are not changed.
        /// Returns true if operation succeeded.
        /// </summary>
        /// <param name="shiftDisplay">If true, whole display will shift, if false only cursor will shift.</param>
        /// <param name="shiftRight">If true, display or cursor will move to right.</param>
        public bool ChangeCursorOrDisplayShift(bool shiftDisplay, bool shiftRight) {
            byte instruction = 0x10;
            if (shiftDisplay) { instruction = (byte)(instruction | 0x08); }
            if (shiftRight) { instruction = (byte)(instruction | 0x04); }
            return SendInstruction(instruction);
        }

        /// <summary>
        /// Sets basic functions of LCD.
        /// Returns true if operation succeeded.
        /// </summary>
        /// <param name="busWidth">Bus width towards MPU. It can be 4 or 8 bits.</param>
        /// <param name="lineCount">Number of lines to use. It can be 1 or 2.</param>
        /// <param name="highResolution">If true, 5x11 dot mode is used for characters, otherwise standard 5x8 is used.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Bus width can only be 4 or 8 bits. -or- Line count must be either 1 or 2.</exception>
        public Boolean ChangeFunction(Int32 busWidth, Int32 lineCount, Boolean highResolution) {
            if ((busWidth != 4) && (busWidth != 8)) { throw new ArgumentOutOfRangeException("busWidth", "Bus width can only be 4 or 8 bits."); }
            if ((lineCount != 1) && (lineCount != 2)) { throw new ArgumentOutOfRangeException("lineCount", "Line count must be either 1 or 2."); }
            byte instruction = 0x20;
            if (busWidth == 8) { instruction = (byte)(instruction | 0x10); }
            if (lineCount == 2) { instruction = (byte)(instruction | 0x08); }
            if (highResolution) { instruction = (byte)(instruction | 0x04); }
            return SendInstruction(instruction);
        }

        /// <summary>
        /// Set CGRAM address to AC. The instruction makes CGRAM data available from MPU.
        /// Returns true if operation succeeded.
        /// </summary>
        /// <param name="address">CGRAM address (0-63).</param>
        /// <exception cref="System.ArgumentOutOfRangeException">CGRAM address must be between 0 and 63.</exception>
        public Boolean ChangeCgramAddress(Int32 address) {
            if ((address < 0) || (address > 63)) { throw new ArgumentOutOfRangeException("address", "CGRAM address must be between 0 and 63."); }
            return SendInstruction((byte)(0x40 | address));
        }

        /// <summary>
        /// Set DDRAM address to AC.
        /// This instruction makes DDRAM data available form MPU.
        /// Returns true if operation succeeded.
        /// </summary>
        /// <param name="address">DDRAM address (0-127)</param>
        /// <exception cref="System.ArgumentOutOfRangeException">DDRAM address must be between 0 and 127.</exception>
        public Boolean ChangeDdramAddress(Int32 address) {
            if ((address < 0) || (address > 127)) { throw new ArgumentOutOfRangeException("address", "DDRAM address must be between 0 and 127."); }
            return SendInstruction((byte)(0x80 | address));
        }


        /// <summary>
        /// Sends command to Elsidi.
        /// Returns true if operation succeeded.
        /// </summary>
        /// <param name="command">Command character.</param>
        /// <param name="commandData">Command data.</param>
        public Boolean SendTextCommand(Char command, String commandData) {
            byte[] bytes;
            return SendTextCommand(command, commandData, out bytes);
        }

        /// <summary>
        /// Sends command to Elsidi.
        /// Returns true if operation succeeded.
        /// </summary>
        /// <param name="command">Command character.</param>
        /// <param name="commandData">Command data.</param>
        /// <param name="responseBytes">Command response bytes.</param>
        private Boolean SendTextCommand(Char command, String commandData, out Byte[] responseBytes) {
            this._serial.DiscardInBuffer();
            this._serial.DiscardOutBuffer();

            var bufferList = new List<byte>(new byte[] { HT });
            bufferList.AddRange(ASCIIEncoding.ASCII.GetBytes(command.ToString()));
            if (commandData != null) { bufferList.AddRange(ASCIIEncoding.ASCII.GetBytes(commandData)); }
            for (int i = bufferList.Count - 1; i >= 1; i--) {
                if ((bufferList[i] == LF) || (bufferList[i] == CR)) {
                    bufferList.RemoveAt(i);
                }
            }
            bufferList.Add(LF);
            var buffer = bufferList.ToArray();
            this._serial.Write(buffer, 0, buffer.Length);

            var response = new List<Byte>();
            var isValid = false;
            while (true) {
                var singleInt = this._serial.ReadByte();
                if (singleInt != -1) {
                    var singleByte = (byte)singleInt;
                    response.Add(singleByte);
                    if (singleByte == LF) {
                        isValid = true;
                        break;
                    } else if (singleByte == HT) { //cancel
                        break;
                    }
                } else {
                    break;
                }
            }
            responseBytes = response.ToArray();
            return isValid;
        }

        /// <summary>
        /// Moves cursor to next line.
        /// Returns true if operation succeeded.
        /// </summary>
        public Boolean NextLine() {
            this._serial.DiscardInBuffer();
            this._serial.DiscardOutBuffer();

            var buffer = new byte[] { LF };
            this._serial.Write(buffer, 0, buffer.Length);
            var ret = this._serial.ReadByte();
            return (ret == LF);
        }

        /// <summary>
        /// All further content will be executed on secondary display.
        /// Returns true if operation succeeded.
        /// </summary>
        public Boolean SwitchToSecondaryDisplay() {
            this._serial.DiscardInBuffer();
            this._serial.DiscardOutBuffer();

            var buffer = new byte[] { SO };
            this._serial.Write(buffer, 0, buffer.Length);
            var ret = this._serial.ReadByte();
            return (ret == LF);
        }

        /// <summary>
        /// All further content will be executed on primary display.
        /// Returns true if operation succeeded.
        /// </summary>
        public Boolean SwitchToPrimaryDisplay() {
            this._serial.DiscardInBuffer();
            this._serial.DiscardOutBuffer();

            var buffer = new byte[] { SI };
            this._serial.Write(buffer, 0, buffer.Length);
            var ret = this._serial.ReadByte();
            return (ret == LF);
        }


        /// <summary>
        /// Returns contrast percent currently set.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "This method might be perceivably slower than the time that is required to get the value of a field.")]
        public Int32 GetContrast() {
            byte[] bytes;
            if (SendTextCommand('c', "", out bytes)) {
                var percentText = System.Text.ASCIIEncoding.ASCII.GetString(bytes);
                int percent;
                if (int.TryParse(percentText, NumberStyles.Integer, CultureInfo.InvariantCulture, out percent)) {
                    if ((percent >= 0) && (percent <= 100)) {
                        return percent;
                    }
                }
            }
            return -1;
        }

        /// <summary>
        /// Temporarily sets contrast.
        /// Returns true if operation succeeded.
        /// </summary>
        /// <param name="percent">Percent value.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Percent value must be between 0 and 100.</exception>
        public Boolean SetContrast(Int32 percent) {
            return SetContrast(percent, false);
        }

        /// <summary>
        /// Sets contrast.
        /// Returns true if operation succeeded.
        /// </summary>
        /// <param name="percent">Percent value.</param>
        /// <param name="save">If true, value should be saved as a default.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Percent value must be between 0 and 100.</exception>
        public Boolean SetContrast(Int32 percent, Boolean save) {
            if ((percent < 0) || (percent > 100)) { throw new ArgumentOutOfRangeException("percent", "Percent value must be between 0 and 100."); }
            return SendTextCommand((save ? 'C' : 'c'), percent.ToString(CultureInfo.InvariantCulture));
        }


        /// <summary>
        /// Returns backlight percent currently set.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "This method might be perceivably slower than the time that is required to get the value of a field.")]
        public Int32 GetBacklight() {
            byte[] bytes;
            if (SendTextCommand('b', "", out bytes)) {
                var percentText = System.Text.ASCIIEncoding.ASCII.GetString(bytes);
                int percent;
                if (int.TryParse(percentText, NumberStyles.Integer, CultureInfo.InvariantCulture, out percent)) {
                    if ((percent >= 0) && (percent <= 100)) {
                        return percent;
                    }
                }
            }
            return -1;
        }

        /// <summary>
        /// Temporarily Sets backlight.
        /// Returns true if operation succeeded.
        /// </summary>
        /// <param name="percent">Percent value.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Percent value must be between 0 and 100.</exception>
        public Boolean SetBacklight(Int32 percent) {
            return SetBacklight(percent, false);
        }

        /// <summary>
        /// Sets backlight.
        /// Returns true if operation succeeded.
        /// </summary>
        /// <param name="percent">Percent value.</param>
        /// <param name="save">If true, value should be saved as a default.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Percent value must be between 0 and 100.</exception>
        public Boolean SetBacklight(Int32 percent, Boolean save) {
            if ((percent < 0) || (percent > 100)) { throw new ArgumentOutOfRangeException("percent", "Percent value must be between 0 and 100."); }
            return SendTextCommand((save ? 'B' : 'b'), percent.ToString(CultureInfo.InvariantCulture));
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
            if (true) {
                this._serial.Dispose();
            }
        }

        #endregion

    }

}
