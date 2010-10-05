//Josip Medved <jmedved@jmedved.com> http://www.jmedved.com

//2008-12-01: New version.
//2010-04-17: Changed namespace from Medo.IO.SerialDevices to Medo.Device.
//2010-07-19: Compatible with Elsidi revG; not compatible with rev 3.


using System;
using System.Collections.Generic;
using System.IO.Ports;

namespace Medo.Device {

    /// <summary>
    /// Sending data to LCD232 board. Compatible with versions equal to or above 1.90.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Elsidi")]
    public class Elsidi : IDisposable {

        private SerialPort _serial;
        private List<byte> _outputBuffer;
        private const int MaxOutputBufferCapacity = 208;
        private const byte HT = 0x09;
        private const byte LF = 0x0A;
        private const byte FF = 0x0C;
        private const byte CR = 0x0D;
        private const byte SO = 0x0E;
        private const byte SI = 0x0F;

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="portName">The port to use.</param>
        public Elsidi(string portName) {
            this._serial = new SerialPort(portName, 9600, Parity.None, 8, StopBits.One);
            this._serial.NewLine = System.Convert.ToChar(0x0A).ToString(); //LF
            this._outputBuffer = new List<byte>(MaxOutputBufferCapacity + 2); //+2 is because on AppendTextToOutputBuffer can append up to two characters more than buffer size before overflow is detected.
        }

        /// <summary>
        /// Opens serial port.
        /// </summary>
        public void Open() {
            this._serial.DtrEnable = true;
            this._serial.RtsEnable = true;
            this._serial.Open();
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
        /// Clears output buffer.
        /// </summary>
        public void ClearBuffer() {
            this._outputBuffer.Clear();
        }


        /// <summary>
        /// Writes text to LCD.
        /// </summary>
        /// <param name="text">ASCII text.</param>
        /// <exception cref="System.InsufficientMemoryException">Maximum output buffer size exceeded.</exception>
        public void AddText(string text) {
            AppendTextToOutputBuffer(System.Text.ASCIIEncoding.ASCII.GetBytes(text), true);
        }

        /// <summary>
        /// Writes text to LCD without checking for command characters.
        /// </summary>
        /// <param name="text">ASCII text.</param>
        /// <exception cref="System.InsufficientMemoryException">Maximum output buffer size exceeded.</exception>
        public void AddLiteralText(string text) {
            AppendTextToOutputBuffer(System.Text.ASCIIEncoding.ASCII.GetBytes(text), false);
        }

        /// <summary>
        /// Writes one character to LCD.
        /// </summary>
        /// <param name="character">ASCII character.</param>
        /// <exception cref="System.InsufficientMemoryException">Maximum output buffer size exceeded.</exception>
        public void AddText(char character) {
            AppendTextToOutputBuffer(System.Text.ASCIIEncoding.ASCII.GetBytes(new char[] { character }), true);
        }

        /// <summary>
        /// Writes one character to LCD.
        /// </summary>
        /// <param name="asciiIndex">ASCII character to send.</param>
        /// <exception cref="System.InsufficientMemoryException">Maximum output buffer size exceeded.</exception>
        public void AddText(byte asciiIndex) {
            AppendTextToOutputBuffer(new byte[] { asciiIndex }, true);
        }

        /// <summary>
        /// Returns true when LCD is ready for next write.
        /// </summary>
        public bool IsReadyForWrite {
            get {
                return this._serial.CtsHolding;
            }
        }


        /// <summary>
        /// Adds command to output buffer.
        /// Clear all the display data by writing “20H” (space code) to all DDRAM address, and set DDRAM address to “00H” into AC (address counter).
        /// Return cursor to the original status, namely, bring the cursor to the left edge on the fist line of the display.
        /// Make the entry mode increment (I/D=“High”)
        /// </summary>
        /// <exception cref="System.InsufficientMemoryException">Maximum output buffer size exceeded.</exception>
        public void AddClearDisplay() {
            AppendToOutputBuffer(new byte[] { FF, 0x01 });
        }

        /// <summary>
        /// Return home is cursor return home instruction.
        /// Set DDRAM address to “00H” into the address counter.
        /// Return cursor to its original site and return display to its original status, if shifted.
        /// Contents of DDRAM does not change.
        /// </summary>
        /// <exception cref="System.InsufficientMemoryException">Maximum output buffer size exceeded.</exception>
        public void AddReturnHome() {
            AppendToOutputBuffer(new byte[] { FF, 0x02 });
        }

        /// <summary>
        /// Set the moving direction of cursor and display.
        /// </summary>
        /// <param name="incrementAddress">If true, DDRAM/CGRAM address will be incremented on every write.</param>
        /// <param name="shiftWholeDisplay">If true, whole display will shift when data is written to.</param>
        /// <exception cref="System.InsufficientMemoryException">Maximum output buffer size exceeded.</exception>
        public void AddEntryModeChange(bool incrementAddress, bool shiftWholeDisplay) {
            byte cmd = 0x04;
            if (incrementAddress) { cmd = (byte)(cmd | 0x02); }
            if (shiftWholeDisplay) { cmd = (byte)(cmd | 0x01); }
            AppendToOutputBuffer(new byte[] { FF, cmd });
        }

        /// <summary>
        /// Control display/cursor/blink ON/OFF 1 bit register.
        /// </summary>
        /// <param name="displayOn">If true, display will be on.</param>
        /// <param name="cursorOn">If true, cursor will be displayed.</param>
        /// <param name="cursorBlinkOn">If true, cursor will blink.</param>
        /// <exception cref="System.InsufficientMemoryException">Maximum output buffer size exceeded.</exception>
        public void AddDisplayOnOffControl(bool displayOn, bool cursorOn, bool cursorBlinkOn) {
            byte cmd = 0x08;
            if (displayOn) { cmd = (byte)(cmd | 0x04); }
            if (cursorOn) { cmd = (byte)(cmd | 0x02); }
            if (cursorBlinkOn) { cmd = (byte)(cmd | 0x01); }
            AppendToOutputBuffer(new byte[] { FF, cmd });
        }

        /// <summary>
        /// Shifting of right/left cursor position or display without writing or reading of display data.
        /// This instruction is used to correct or search display data.
        /// During 2-line mode display, cursor moves to the 2nd line after the 40th digit of the 1st line.
        /// Note that display shift is performed simultaneously in all the lines.
        /// When display data is shifted repeatedly, each line is shifted individually.
        /// When display shift is performed, the contents of the address counter are not changed.
        /// </summary>
        /// <param name="shiftDisplay">If true, whole display will shift, if false only cursor will shift.</param>
        /// <param name="shiftRight">If true, display or cursor will move to right.</param>
        /// <exception cref="System.InsufficientMemoryException">Maximum output buffer size exceeded.</exception>
        public void AddCursorOrDisplayShift(bool shiftDisplay, bool shiftRight) {
            byte cmd = 0x10;
            if (shiftDisplay) { cmd = (byte)(cmd | 0x08); }
            if (shiftRight) { cmd = (byte)(cmd | 0x04); }
            AppendToOutputBuffer(new byte[] { FF, cmd });
        }

        /// <summary>
        /// Sets basic functions of LCD.
        /// </summary>
        /// <param name="busWidth">Bus width towards MPU. It can be 4-bit or 8-bit.</param>
        /// <param name="lineCount">Number of lines to use. It can be 1 or 2.</param>
        /// <param name="highResolution">If true, 5x11 dot mode is used for characters, otherwise standard 5x8 is used.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Bus width can only be 4-bit or 8-bit. -or- Line count must be either 1 or 2.</exception>
        /// <exception cref="System.InsufficientMemoryException">Maximum output buffer size exceeded.</exception>
        public void AddFunctionChange(int busWidth, int lineCount, bool highResolution) {
            if ((busWidth != 4) && (busWidth != 8)) { throw new System.ArgumentOutOfRangeException("busWidth", "Bus width can only be 4-bit or 8-bit."); }
            if ((lineCount != 1) && (lineCount != 2)) { throw new System.ArgumentOutOfRangeException("lineCount", "Line count must be either 1 or 2."); }
            byte cmd = 0x20;
            if (busWidth == 8) { cmd = (byte)(cmd | 0x10); }
            if (lineCount == 2) { cmd = (byte)(cmd | 0x08); }
            if (highResolution) { cmd = (byte)(cmd | 0x04); }
            AppendToOutputBuffer(new byte[] { FF, cmd });
        }

        /// <summary>
        /// Set CGRAM address to AC. The instruction makes CGRAM data available from MPU.
        /// </summary>
        /// <param name="address">CGRAM address (0-63)</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Address is out of range.</exception>
        /// <exception cref="System.InsufficientMemoryException">Maximum output buffer size exceeded.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Cgram", Justification = "CGRAM is correct spelling.")]
        public void AddCgramAddressChange(int address) {
            if ((address < 0) || (address > 63)) { throw new System.ArgumentOutOfRangeException("address", "Address is out of range."); }
            byte cmd = 0x40;
            cmd = (byte)(cmd | address);
            AppendToOutputBuffer(new byte[] { FF, cmd });
        }

        /// <summary>
        /// Set DDRAM address to AC.
        /// This instruction makes DDRAM data available form MPU.
        /// </summary>
        /// <param name="address">DDRAM address (0-127)</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Address is out of range.</exception>
        /// <exception cref="System.InsufficientMemoryException">Maximum output buffer size exceeded.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Ddram", Justification = "DDRAM is correct spelling.")]
        public void AddDdramAddressChange(int address) {
            if ((address < 0) || (address > 127)) { throw new System.ArgumentOutOfRangeException("address", "Address is out of range."); }
            byte cmd = 0x80;
            cmd = (byte)(cmd | address);
            AppendToOutputBuffer(new byte[] { FF, cmd });
        }

        /// <summary>
        /// Custom instruction is sent as such without any parameters check.
        /// </summary>
        /// <param name="value">Instruction byte.</param>
        /// <exception cref="System.InsufficientMemoryException">Maximum output buffer size exceeded.</exception>
        public void AddCustomInstruction(byte value) {
            AppendToOutputBuffer(new byte[] { FF, value});
        }

        /// <summary>
        /// All further content will be executed on primary display.
        /// </summary>
        /// <exception cref="System.InsufficientMemoryException">Maximum output buffer size exceeded.</exception>
        public void AddSwithToPrimaryDisplay() {
            AppendToOutputBuffer(new byte[] { SI });
        }

        /// <summary>
        /// All further content will be executed on secondary display.
        /// </summary>
        /// <exception cref="System.InsufficientMemoryException">Maximum output buffer size exceeded.</exception>
        public void AddSwithToSecondaryDisplay() {
            AppendToOutputBuffer(new byte[] { SO });
        }

        /// <summary>
        /// Gets/sets whether serial LCD device is enabled.
        /// Works only on devices v2 and higher.
        /// </summary>
        public bool Enabled {
            get {
                return _serial.DtrEnable;
            }
            set {
                this._serial.DtrEnable = value;
            }
        }

        /// <summary>
        /// Executes command in buffer and waits for execution to finish.
        /// It will timeout after 10 seconds.
        /// </summary>
        /// <exception cref="System.TimeoutException">The operation did not complete before the time-out period ended.</exception>
        public void Execute() {
            Execute(10000);
        }

        /// <summary>
        /// Executes command in buffer and waits for execution to finish.
        /// </summary>
        /// <param name="timeout">Number of milliseconds to wait until execution is done. When timeout occurs, exception will be thrown.</param>
        /// <exception cref="System.TimeoutException">The operation did not complete before the time-out period ended.</exception>
        public void Execute(int timeout) {
            this._serial.DiscardInBuffer();
            this._serial.DiscardOutBuffer();
            this._outputBuffer.Add(0x0A);
            this._serial.Write(this._outputBuffer.ToArray(), 0, this._outputBuffer.Count);
            this._outputBuffer.Clear();
            this._serial.ReadTimeout = timeout;
            this._serial.ReadLine();
        }


        private void AppendTextToOutputBuffer(byte[] content, bool replaceCommandCharacters) {
            if ((content == null) || (content.Length == 0)) { return; }

            int oldBufferCount = _outputBuffer.Count;
            for (int i = 0; i < content.Length; ++i) {
                if ((content[i] >= 0x08) && (content[i] <= 0x0F) && replaceCommandCharacters) { //redirect to standard custom CGRAM characters
                    this._outputBuffer.Add((byte)(content[i] - 0x08));
                } else {
                    this._outputBuffer.Add(content[i]);
                }
                if (this._outputBuffer.Count > MaxOutputBufferCapacity) {
                    this._outputBuffer.RemoveRange(oldBufferCount, this._outputBuffer.Count - oldBufferCount);
                    throw new InsufficientMemoryException("Maximum output buffer size exceeded.");
                }
            }
        }

        private void AppendToOutputBuffer(byte[] content) {
            if ((content == null) || (content.Length == 0)) { return; }

            if (this._outputBuffer.Count + content.Length > MaxOutputBufferCapacity) { throw new InsufficientMemoryException("Maximum output buffer size exceeded."); }

            this._outputBuffer.AddRange(content);
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
