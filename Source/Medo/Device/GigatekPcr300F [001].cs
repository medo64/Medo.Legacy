//Copyright (c) 2010 Josip Medved <jmedved@jmedved.com>

//2010-05-26: Initial version.


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO.Ports;
using System.Text;
using System.Threading;

namespace Medo.Device {

    /// <summary>
    /// Management of Gigatek PCR300F RFID proximity reader.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Gigatek", Justification = "Naming is as intended.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Pcr", Justification = "Naming is as intended.")]
    public class GigatekPcr300F : IDisposable {

        private Thread _thread;
        private ManualResetEvent _cancelEvent;
        private SerialPort _serial;
        private readonly object _syncCodes = new object();
        private Queue<string> _codes = new Queue<string>();

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="portName">The port to use.</param>
        public GigatekPcr300F(string portName) {
            this._serial = new SerialPort(portName, 9600, Parity.None, 8, StopBits.One);
            this._serial.ReadTimeout = 15000;
        }

        /// <summary>
        /// Opens connection toward device.
        /// </summary>
        public void Open() {
            this._serial.Open();

            this._cancelEvent = new ManualResetEvent(false);
            this._thread = new Thread(Run) { IsBackground = true };
            this._thread.Name = this.ToString() + " @ " + this._serial.PortName;
            this._thread.Start();
        }

        /// <summary>
        /// Closes connection toward device.
        /// </summary>
        public void Close() {
                _cancelEvent.Set();
                while (_thread.IsAlive) { Thread.Sleep(10); }
                _thread = null;
            this._cancelEvent.Dispose();
            this._serial.Close();
        }


        /// <summary>
        /// Gets whether there is code waiting to be read.
        /// </summary>
        public bool HasCode {
            get {
                lock (this._syncCodes) {
                    return (this._codes.Count > 0);
                }
            }
        }

        /// <summary>
        /// Removes all codes read.
        /// </summary>
        public void Clear() {
            lock (this._syncCodes) {
                this._codes.Clear();
            }
        }

        /// <summary>
        /// Returns code. If one is not available, it waits for specified time before throwing exception.
        /// </summary>
        /// <param name="timeout">Timeout in milliseconds.</param>
        /// <exception cref="System.TimeoutException">Timeout expired.</exception>
        public string ReadCode(int timeout) {
            var timer = new Stopwatch();
            timer.Start();
            while (timer.ElapsedMilliseconds < timeout) {
                lock (this._syncCodes) {
                    if (this._codes.Count > 0) {
                        return this._codes.Dequeue();
                    }
                }
                Thread.Sleep(1);
            }
            throw new TimeoutException("Timeout expired.");
        }

        /// <summary>
        /// Gets available code. If there is no code available, returns null.
        /// </summary>
        public string Code {
            get {
                lock (this._syncCodes) {
                    if (this._codes.Count > 0) {
                        return this._codes.Dequeue();
                    } else {
                        return null;
                    }
                }
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
                this.Close();
            }
        }

        #endregion



        private enum ExpectedState {
            Default = 0,
            Stx = 1,
            Data = 2,
            Cr = 3,
            Lf = 4,
            Escape = 5,

        }

        private void Run() {
            try {
                var dataBuffer = new List<byte>();
                var expectedState = ExpectedState.Stx;

                while (!_cancelEvent.WaitOne(0, false)) {

                    if (_serial.BytesToRead > 0) {
                        try {
                            var input = (byte)_serial.ReadByte();
                            switch (expectedState) {

                                case ExpectedState.Stx: {
                                        if (input == 0x02) {
                                            expectedState = ExpectedState.Data;
                                            dataBuffer.Clear();
                                        }
                                    } break;

                                case ExpectedState.Data: {
                                        if (((input >= 0x30) && (input <= 0x39)) //0-9
                                        || ((input >= 0x41) && (input <= 0x46)) //A-F
                                        || ((input >= 0x61) && (input <= 0x66))) {//a-f
                                            dataBuffer.Add(input);
                                            if (dataBuffer.Count == 10) {
                                                expectedState = ExpectedState.Cr;
                                            }
                                        } else {
                                            expectedState = ExpectedState.Stx;
                                        }
                                    } break;

                                case ExpectedState.Cr: {
                                        if (input == 13) {
                                            expectedState = ExpectedState.Lf;
                                        } else {
                                            expectedState = ExpectedState.Stx;
                                        }
                                    } break;

                                case ExpectedState.Lf: {
                                        if (input == 10) {
                                            expectedState = ExpectedState.Escape;
                                        } else {
                                            expectedState = ExpectedState.Stx;
                                        }
                                    } break;

                                case ExpectedState.Escape: {
                                        if (input == 27) {
                                            var code = ASCIIEncoding.ASCII.GetString(dataBuffer.ToArray()).ToUpper(CultureInfo.InvariantCulture);
                                            this._codes.Enqueue(code);
                                        }
                                        expectedState = ExpectedState.Stx;
                                    } break;

                            }
                        } catch (InvalidOperationException) {
                            expectedState = ExpectedState.Stx;
                        } catch (TimeoutException) {
                            expectedState = ExpectedState.Stx;
                        } catch (InvalidCastException) {
                            expectedState = ExpectedState.Stx;
                        }

                    } else {
                        Thread.Yield();
                    }

                }
            } catch (ThreadAbortException) { }
        }

    }

}
