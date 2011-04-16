//Copyright (c) 2008 Josip Medved <jmedved@jmedved.com>

//2008-11-05: New version.
//2010-04-17: Changed namespace from Medo.IO.SerialDevices to Medo.Device.


using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO.Ports;
using System.Text;

namespace Medo.Device {

    /// <summary>
    /// Management of Eltra EL4000 serial printer.
    /// </summary>
    public class EltraEL4000 : IDisposable {

        private SerialPort _serial;

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="portName">The port to use.</param>
        public EltraEL4000(string portName) {
            this._serial = new SerialPort(portName, 9600, Parity.None, 8, StopBits.One);
            this._serial.ReadTimeout = 15000;
        }

        /// <summary>
        /// Opens serial port.
        /// </summary>
        public void Open() {
            this._serial.Open();
        }

        /// <summary>
        /// Closes serial port.
        /// </summary>
        public void Close() {
            this._serial.Close();
        }

        private byte[] _lastData;
        /// <summary>
        /// Returns data from last command.
        /// </summary>
        public byte[] GetLastData() {
            return this._lastData;
        }

        private EltraCommandStatus _lastCommandStatus;
        /// <summary>
        /// Gets status of last command.
        /// </summary>
        public EltraCommandStatus LastCommandStatus {
            get { return this._lastCommandStatus; }
        }

        private EltraErrorCodes _lastErrorCode;
        /// <summary>
        /// Gets error code of last command.
        /// Notice that some error codes are in fact status reports.
        /// </summary>
        public EltraErrorCodes LastErrorCode {
            get { return this._lastErrorCode; }
        }


        /// <summary>
        /// The command allows the transport of the card present in mouth, inside the module.
        /// Returns true upon successful completion.
        /// </summary>
        public bool InsertTicket() {
            WritePortCommand(0x3C, null); //<
            return ReadPortAnswer();
        }

        /// <summary>
        /// The command provides the physical status of the module, by reading the status of the photodiodes at the moment that the module receives the command.
        /// Returns true upon successful completion.
        /// </summary>
        public bool SendSensorStatus() {
            WritePortCommand(0x3F, null); //?
            return ReadPortAnswer();
        }

        /// <summary>
        /// The command allow the ticket insertion in mouth by opening the shutter, if present, in a way to let that ticket goes under rollers.
        /// Returns true upon successful completion.
        /// </summary>
        public bool EnableTicketInsertion() {
            WritePortCommand(0x41, null); //A
            return ReadPortAnswer();
        }

        /// <summary>
        /// The command disable detection ticket from the module if enabled.
        /// Returns true upon successful completion.
        /// </summary>
        public bool DisableTicketInsertion() {
            WritePortCommand(0x42, null); //B
            return ReadPortAnswer();
        }

        /// <summary>
        /// The command reads the coding present on the ticket stopped under the scanner beam.
        /// Returns field text upon successful completion.
        /// </summary>
        public string ReadBarcodeField() {
            WritePortCommand(0x43, null); //C
            if (ReadPortAnswer()) {
                return System.Text.ASCIIEncoding.ASCII.GetString(this._lastData);
            } else {
                return null;
            }
        }

        /// <summary>
        /// The command work as software reset able to initialize the unit. Ticket present inside the module will be re-positioning in home position under the scanner beam.
        /// Returns true upon successful completion.
        /// </summary>
        public bool InitializeModule() {
            WritePortCommand(0x44, null); //D
            return ReadPortAnswer();
        }

        /// <summary>
        /// The command provides the Logical Status of the module referred to the last operation carried on. The Logical Status of the module is always updated by a new command.
        /// Returns true upon successful completion.
        /// </summary>
        public bool SendLogicalStatus() {
            WritePortCommand(0x45, null); //E
            return ReadPortAnswer();
        }

        /// <summary>
        /// The command provides the ticket ejection by moving it to the mouth position. The ticket will stay hooked under rollers.
        /// Returns true upon successful completion.
        /// </summary>
        public bool EjectTicket() {
            WritePortCommand(0x46, null); //F
            return ReadPortAnswer();
        }

        /// <summary>
        /// The command provides to transport the ticket present on mouth inside the module and under the scanner beam. The ticket coding will be read and then ticket will be ejected.
        /// Returns field text upon successful completion.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "This operation is expensive.")]
        public string GetInsideReadDataAndParkingUnderPrinter() {
            WritePortCommand(0x47, null); //G
            if (ReadPortAnswer()) {
                return System.Text.ASCIIEncoding.ASCII.GetString(this._lastData);
            } else {
                return null;
            }
        }

        /// <summary>
        /// The command works by doing a simulate hardware reset able to re-start the unit as per the Power-On procedure. Hardware reset command ends execution of all commands. Ticket present inside the module will be captured.
        /// Returns true upon successful completion.
        /// </summary>
        public bool HardwareReset() {
            WritePortCommand(0x48, null); //H
            return ReadPortAnswer();
        }

        /// <summary>
        /// The command move the ticket on the 2nd printed bar code field and read the coding present under the scanner beam.
        /// Returns field text upon successful completion.
        /// </summary>
        public string ReadSecondBarcodeField() {
            WritePortCommand(0x49, null); //I
            if (ReadPortAnswer()) {
                return System.Text.ASCIIEncoding.ASCII.GetString(this._lastData);
            } else {
                return null;
            }
        }

        /// <summary>
        /// The command shall be used to print some “text” on the same ticket where barcode is already present (i.e. to get receipt) having a maximum of both 240 printed characters and 24 characters per line.
        /// Returns true upon successful completion.
        /// </summary>
        /// <param name="text">Printed data on ticket</param>
        public bool PrintTextOnTicket(string text) {
            List<byte> buffer = new List<byte>();
            buffer.AddRange(ASCIIEncoding.ASCII.GetBytes(text));
            buffer.Add(0x0A); //LF
            WritePortCommand(0x50, buffer.ToArray()); //P
            return ReadPortAnswer();
        }

        /// <summary>
        /// The command combines together the features of two commands “Insert Ticket” and “Read Barcode” in only one single command. The purpose of this command is to speed up ticket issuing time.
        /// Returns field text upon successful completion.
        /// </summary>
        public string InsertTicketAndReadBarcodeField() {
            WritePortCommand(0x52, null); //R
            if (ReadPortAnswer()) {
                return System.Text.ASCIIEncoding.ASCII.GetString(this._lastData);
            } else {
                return null;
            }
        }

        /// <summary>
        /// The command shall be used to validate and to issue the ticket already present under printer. The printed validation string can be set as per any customer need.
        /// Returns true upon successful completion.
        /// </summary>
        /// <param name="barcode">Barcode field.</param>
        /// <param name="text">Text line.</param>
        public bool ValidateTicket(string barcode, params string[] text) {
            List<byte> buffer = new List<byte>();

            if (barcode != null) { buffer.AddRange(ASCIIEncoding.ASCII.GetBytes(barcode)); }
            buffer.Add(0x0A); //LF

            if (text != null) {
                for (int i = 0; i < text.Length; ++i) {
                    if (text[i] != null) { buffer.AddRange(ASCIIEncoding.ASCII.GetBytes(text[i])); }
                    buffer.Add(0x0A); //LF
                }
            }

            WritePortCommand(0x56, buffer.ToArray()); //V
            return ReadPortAnswer();
        }

        //public void ReadModuleConfiguration() {
        //}

        //public void PeripheralAddressSetting() {
        //}

        /// <summary>
        /// The command is accepted if there is a ticket present inside the module. This command provides to transport the ticket under the printer.
        /// Returns true upon successful completion.
        /// </summary>
        public bool LoadTicketUnderPrinter() {
            WritePortCommand(0x70, null); //p
            return ReadPortAnswer();
        }

        /// <summary>
        /// The command moves the ticket from the printer to the “Home position”. The ticket in “Home position” can be read from the barcode scanner module.
        /// Returns true upon successful completion.
        /// </summary>
        public bool UnloadTicketFromPrinter() {
            WritePortCommand(0x72, null); //r
            return ReadPortAnswer();
        }

        /// <summary>
        /// This command works only if the card is present inside the module. The card is moved towards the tail position to be captured.
        /// Returns true upon successful completion.
        /// </summary>
        public bool CaptureTicket() {
            WritePortCommand(0x76, null); //v
            return ReadPortAnswer();
        }


        private void WritePortCommand(byte code, byte[] data) {
            _lastCommandStatus = EltraCommandStatus.Unknown;
            _lastErrorCode = EltraErrorCodes.None;
            _lastData = null;

            List<byte> command = new List<byte>();
            command.Add(0x12); //WUP
            command.Add(0x02); //STX
            command.Add(0x42); //DEST
            command.Add(code); //CMD
            if (data != null) {
                command.AddRange(data); //DATA
            }
            command.Add(0x03); //ETX

            Lrc8 lrc = Lrc8.GetEltra();
            lrc.Append(command.ToArray());
            command.AddRange(lrc.DigestAsAscii30); //LRC

            command.Add(0x04); //EOT

            this._serial.Write(command.ToArray(), 0, command.Count);
        }

        private bool ReadPortAnswer() {
            int value = this._serial.ReadByte(); //Check whether command was ok.
            if (value != 0x06) { //ACK
                this._lastCommandStatus = EltraCommandStatus.CommandError;
                return false;
            }

            while (true) {
                this._serial.Write(new byte[] { 0x12, 0x05 }, 0, 2); //WUP ENQ  Pool whether answer is ready.

                value = this._serial.ReadByte();
                if (value == 0x10) { //DLE  No message to send.
                    System.Threading.Thread.Sleep(100); //just wait a little
                } else if (value == 0x02) { //STX  Answer is ready.
                    List<byte> buffer = new List<byte>();
                    buffer.Add((byte)value);
                    do {
                        value = this._serial.ReadByte();
                        buffer.Add((byte)value);
                    } while (value != 0x04); //EOT
                    if (buffer.Count < 10) {
                        this._lastCommandStatus = EltraCommandStatus.Unknown;
                        return false;
                    }

                    switch (buffer[3]) {
                        case 0x30: {
                                this._lastCommandStatus = EltraCommandStatus.CommandAccepted;
                            } break;
                        case 0x52: {
                                this._lastCommandStatus = EltraCommandStatus.CommandRefused;
                            } break;
                        case 0x54: {
                                this._lastCommandStatus = EltraCommandStatus.CommandNotAvailable;
                            } break;
                        default: throw new System.FormatException("Unknown answer status.");
                    }
                    int errorCodesX = (buffer[4] & 0xF);
                    int errorCodesY = (buffer[5] & 0xF);
                    switch (buffer[2]) {
                        case 0x3C: //Insert Ticket. Ticket in Position
                            if ((errorCodesX & 0x01) == 0x01) { this._lastErrorCode |= EltraErrorCodes.TicketAlreadyInsideTheTransport; }
                            if ((errorCodesX & 0x02) == 0x02) { this._lastErrorCode |= EltraErrorCodes.TransportFailure; }
                            if ((errorCodesX & 0x04) == 0x04) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesX & 0x08) == 0x08) { this._lastErrorCode |= EltraErrorCodes.TicketNotPresentInMouth; }
                            if ((errorCodesY & 0x01) == 0x01) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesY & 0x02) == 0x02) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesY & 0x04) == 0x04) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesY & 0x08) == 0x08) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            break;
                        case 0x3F: //Send Sensor Status
                            if ((errorCodesX & 0x01) == 0x01) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesX & 0x02) == 0x02) { this._lastErrorCode |= EltraErrorCodes.TicketInHomePosition; }
                            if ((errorCodesX & 0x04) == 0x04) { this._lastErrorCode |= EltraErrorCodes.TicketInCentralPosition; }
                            if ((errorCodesX & 0x08) == 0x08) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesY & 0x01) == 0x01) { this._lastErrorCode |= EltraErrorCodes.TicketInTailPosition; }
                            if ((errorCodesY & 0x02) == 0x02) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesY & 0x04) == 0x04) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesY & 0x08) == 0x08) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            break;
                        case 0x41: //Enable Ticket Insertion
                            if ((errorCodesX & 0x01) == 0x01) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesX & 0x02) == 0x02) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesX & 0x04) == 0x04) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesX & 0x08) == 0x08) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesY & 0x01) == 0x01) { this._lastErrorCode |= EltraErrorCodes.TicketAlreadyInsideTheTransport; }
                            if ((errorCodesY & 0x02) == 0x02) { this._lastErrorCode |= EltraErrorCodes.TicketInMouthPosition; }
                            if ((errorCodesY & 0x04) == 0x04) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesY & 0x08) == 0x08) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            break;
                        case 0x42: //Disable Ticket Insertion
                            if ((errorCodesX & 0x01) == 0x01) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesX & 0x02) == 0x02) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesX & 0x04) == 0x04) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesX & 0x08) == 0x08) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesY & 0x01) == 0x01) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesY & 0x02) == 0x02) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesY & 0x04) == 0x04) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesY & 0x08) == 0x08) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            break;
                        case 0x43: //Read Barcode Field
                            if ((errorCodesX & 0x01) == 0x01) { this._lastErrorCode |= EltraErrorCodes.NoTicketInsideTheTransport; }
                            if ((errorCodesX & 0x02) == 0x02) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesX & 0x04) == 0x04) { this._lastErrorCode |= EltraErrorCodes.ReadError; }
                            if ((errorCodesX & 0x08) == 0x08) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesY & 0x01) == 0x01) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesY & 0x02) == 0x02) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesY & 0x04) == 0x04) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesY & 0x08) == 0x08) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            break;
                        case 0x44: //Initialize Module
                            if ((errorCodesX & 0x01) == 0x01) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesX & 0x02) == 0x02) { this._lastErrorCode |= EltraErrorCodes.TransportFailure; }
                            if ((errorCodesX & 0x04) == 0x04) { this._lastErrorCode |= EltraErrorCodes.TicketInHomePosition; }
                            if ((errorCodesX & 0x08) == 0x08) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesY & 0x01) == 0x01) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesY & 0x02) == 0x02) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesY & 0x04) == 0x04) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesY & 0x08) == 0x08) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            break;
                        case 0x45: //Send Logical Status
                            if ((errorCodesX & 0x01) == 0x01) { this._lastErrorCode |= EltraErrorCodes.TicketInTailPosition; }
                            if ((errorCodesX & 0x02) == 0x02) { this._lastErrorCode |= EltraErrorCodes.TicketInReadPosition; }
                            if ((errorCodesX & 0x04) == 0x04) { this._lastErrorCode |= EltraErrorCodes.NoTicketInsideTheTransport; }
                            if ((errorCodesX & 0x08) == 0x08) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesY & 0x01) == 0x01) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesY & 0x02) == 0x02) { this._lastErrorCode |= EltraErrorCodes.PhotocellFailure; }
                            if ((errorCodesY & 0x04) == 0x04) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesY & 0x08) == 0x08) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            break;
                        case 0x46: //Eject Ticket
                            if ((errorCodesX & 0x01) == 0x01) { this._lastErrorCode |= EltraErrorCodes.NoTicketInsideTheTransport; }
                            if ((errorCodesX & 0x02) == 0x02) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesX & 0x04) == 0x04) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesX & 0x08) == 0x08) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesY & 0x01) == 0x01) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesY & 0x02) == 0x02) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesY & 0x04) == 0x04) { this._lastErrorCode |= EltraErrorCodes.TransportFailure; }
                            if ((errorCodesY & 0x08) == 0x08) { this._lastErrorCode |= EltraErrorCodes.TicketInMouthPosition; }
                            break;
                        case 0x47: //Get Inside, Read Data and Parking under Printer
                            if ((errorCodesX & 0x01) == 0x01) { this._lastErrorCode |= EltraErrorCodes.TicketAlreadyInsideTheTransport; }
                            if ((errorCodesX & 0x02) == 0x02) { this._lastErrorCode |= EltraErrorCodes.TransportFailure; }
                            if ((errorCodesX & 0x04) == 0x04) { this._lastErrorCode |= EltraErrorCodes.ReadError; }
                            if ((errorCodesX & 0x08) == 0x08) { this._lastErrorCode |= EltraErrorCodes.TicketAlreadyUnderPrinter; }
                            if ((errorCodesY & 0x01) == 0x01) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesY & 0x02) == 0x02) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesY & 0x04) == 0x04) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesY & 0x08) == 0x08) { this._lastErrorCode |= EltraErrorCodes.TicketNotPresentInMouth; }
                            break;
                        case 0x48: //Hardware Reset
                            if ((errorCodesX & 0x01) == 0x01) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesX & 0x02) == 0x02) { this._lastErrorCode |= EltraErrorCodes.TransportFailure; }
                            if ((errorCodesX & 0x04) == 0x04) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesX & 0x08) == 0x08) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesY & 0x01) == 0x01) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesY & 0x02) == 0x02) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesY & 0x04) == 0x04) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesY & 0x08) == 0x08) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            break;
                        case 0x49: //Read 2nd Barcode Field
                            if ((errorCodesX & 0x01) == 0x01) { this._lastErrorCode |= EltraErrorCodes.NoTicketInsideTheTransport; }
                            if ((errorCodesX & 0x02) == 0x02) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesX & 0x04) == 0x04) { this._lastErrorCode |= EltraErrorCodes.ReadError; }
                            if ((errorCodesX & 0x08) == 0x08) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesY & 0x01) == 0x01) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesY & 0x02) == 0x02) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesY & 0x04) == 0x04) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesY & 0x08) == 0x08) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            break;
                        case 0x50: //Print Text on Ticket
                            if ((errorCodesX & 0x01) == 0x01) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesX & 0x02) == 0x02) { this._lastErrorCode |= EltraErrorCodes.TransportFailure; }
                            if ((errorCodesX & 0x04) == 0x04) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesX & 0x08) == 0x08) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesY & 0x01) == 0x01) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesY & 0x02) == 0x02) { this._lastErrorCode |= EltraErrorCodes.JammingError; }
                            if ((errorCodesY & 0x04) == 0x04) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesY & 0x08) == 0x08) { this._lastErrorCode |= EltraErrorCodes.TicketInMouthPosition; }
                            break;
                        case 0x52: //Insert Ticket and Read Barcode Field
                            if ((errorCodesX & 0x01) == 0x01) { this._lastErrorCode |= EltraErrorCodes.TicketAlreadyInsideTheTransport; }
                            if ((errorCodesX & 0x02) == 0x02) { this._lastErrorCode |= EltraErrorCodes.TransportFailure; }
                            if ((errorCodesX & 0x04) == 0x04) { this._lastErrorCode |= EltraErrorCodes.ReadError; }
                            if ((errorCodesX & 0x08) == 0x08) { this._lastErrorCode |= EltraErrorCodes.TicketAlreadyUnderPrinter; }
                            if ((errorCodesY & 0x01) == 0x01) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesY & 0x02) == 0x02) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesY & 0x04) == 0x04) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesY & 0x08) == 0x08) { this._lastErrorCode |= EltraErrorCodes.TicketNotPresentInMouth; }
                            break;
                        case 0x56: //Validate Ticket
                            if ((errorCodesX & 0x01) == 0x01) { this._lastErrorCode |= EltraErrorCodes.TicketUnderPrinter; }
                            if ((errorCodesX & 0x02) == 0x02) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesX & 0x04) == 0x04) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesX & 0x08) == 0x08) { this._lastErrorCode |= EltraErrorCodes.VerifyError; }
                            if ((errorCodesY & 0x01) == 0x01) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesY & 0x02) == 0x02) { this._lastErrorCode |= EltraErrorCodes.JammingError; }
                            if ((errorCodesY & 0x04) == 0x04) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesY & 0x08) == 0x08) { this._lastErrorCode |= EltraErrorCodes.TicketInMouthPosition; }
                            break;
                        case 0x58: //Read Module Configuration
                            break;
                        case 0x59: //Peripheral Address Setting
                            break;
                        case 0x70: //Load Ticket under Printer
                            if ((errorCodesX & 0x01) == 0x01) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesX & 0x02) == 0x02) { this._lastErrorCode |= EltraErrorCodes.TransportFailure; }
                            if ((errorCodesX & 0x04) == 0x04) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesX & 0x08) == 0x08) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesY & 0x01) == 0x01) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesY & 0x02) == 0x02) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesY & 0x04) == 0x04) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesY & 0x08) == 0x08) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            break;
                        case 0x72: //Unload Ticket from Printer
                            if ((errorCodesX & 0x01) == 0x01) { this._lastErrorCode |= EltraErrorCodes.TicketInReadPosition; }
                            if ((errorCodesX & 0x02) == 0x02) { this._lastErrorCode |= EltraErrorCodes.TransportFailure; }
                            if ((errorCodesX & 0x04) == 0x04) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesX & 0x08) == 0x08) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesY & 0x01) == 0x01) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesY & 0x02) == 0x02) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesY & 0x04) == 0x04) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesY & 0x08) == 0x08) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            break;
                        case 0x76: //Capture Ticket
                            if ((errorCodesX & 0x01) == 0x01) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesX & 0x02) == 0x02) { this._lastErrorCode |= EltraErrorCodes.TransportFailure; }
                            if ((errorCodesX & 0x04) == 0x04) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesX & 0x08) == 0x08) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesY & 0x01) == 0x01) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesY & 0x02) == 0x02) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesY & 0x04) == 0x04) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if ((errorCodesY & 0x08) == 0x08) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            break;
                        default:
                            if (errorCodesX != 0x00) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            if (errorCodesY != 0x00) { this._lastErrorCode |= EltraErrorCodes.Unknown; }
                            break;
                    }
                    this._lastData = new byte[buffer.Count - 10];
                    Array.Copy(buffer.ToArray(), 6, this._lastData, 0, this._lastData.Length);
                    return (this._lastCommandStatus == EltraCommandStatus.CommandAccepted);
                }
            }
        }

        private class Lrc8 {

            private byte _currDigest;


            /// <summary>
            /// Returns Eltra implementation.
            /// You would need to use DigestAsAscii30 on this also.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Calling the method results in different instances.")]
            public static Lrc8 GetEltra() {
                return new Lrc8(0x00);
            }

            /// <summary>
            /// Creates new instance.
            /// </summary>
            /// <param name="initialValue">Starting digest.</param>
            public Lrc8(byte initialValue) {
                this._currDigest = initialValue;
            }


            /// <summary>
            /// Adds new data to digest.
            /// </summary>
            /// <param name="value">Data to add.</param>
            /// <returns>Current digest.</returns>
            /// <exception cref="System.ArgumentNullException">Value cannot be null.</exception>
            public void Append(byte[] value) {
                if (value == null) { throw new System.ArgumentNullException("value", Resources.ExceptionValueCannotBeNull); }
                this.Append(value, 0, value.Length);
            }

            /// <summary>
            /// Adds new data to digest.
            /// </summary>
            /// <param name="value">Data to add.</param>
            /// <param name="index">A 32-bit integer that represents the index at which data begins.</param>
            /// <param name="length">A 32-bit integer that represents the number of elements.</param>
            /// <returns>Current digest.</returns>
            /// <exception cref="System.ArgumentNullException">Value cannot be null.</exception>
            public void Append(byte[] value, int index, int length) {
                if (value == null) { throw new System.ArgumentNullException("value", Resources.ExceptionValueCannotBeNull); }
                for (int i = index; i < index + length; i++) {
                    this._currDigest = (byte)(this._currDigest ^ value[i]);
                }
            }

            /// <summary>
            /// Gets current digest.
            /// </summary>
            public byte Digest {
                get { return this._currDigest; }
            }

            /// <summary>
            /// Gets current digest in splitted in two halfs with 0x30 added to each one.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is in order to have similar properties for all Medo.Security.Checksum namespace classes.")]
            public byte[] DigestAsAscii30 {
                get {
                    byte part1 = (byte)(0x30 + (this.Digest >> 4));
                    byte part2 = (byte)(0x30 + (this.Digest & 0x0f));
                    return new byte[] { part1, part2 };
                }
            }


            private static class Resources {

                internal static string ExceptionValueCannotBeNull { get { return "Value cannot be null."; } }

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
            if (true) {
                this._serial.Dispose();
            }
        }

        #endregion

    }



    /// <summary>
    /// Command status.
    /// </summary>
    public enum EltraCommandStatus {
        /// <summary>
        /// Unknown status.
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// Command was accepted.
        /// </summary>
        CommandAccepted = 0x30,
        /// <summary>
        /// Command was refused.
        /// </summary>
        CommandRefused = 0x52,
        /// <summary>
        /// Command is not available.
        /// </summary>
        CommandNotAvailable = 0x54,
        /// <summary>
        /// Error.
        /// </summary>
        CommandError = -1
    }



    /// <summary>
    /// Error codes for Eltra device.
    /// Some of these codes are in fact more used like status.
    /// </summary>
    [Flags()]
    public enum EltraErrorCodes {
        /// <summary>
        /// None.
        /// </summary>
        None = 0x00,
        /// <summary>
        /// Ticket already inside the transport.
        /// </summary>
        TicketAlreadyInsideTheTransport = 0x01,
        /// <summary>
        /// Transport failure.
        /// </summary>
        TransportFailure = 0x02,
        /// <summary>
        /// Ticket not present in mouth.
        /// </summary>
        TicketNotPresentInMouth = 0x04,
        /// <summary>
        /// Ticket in home position.
        /// </summary>
        TicketInHomePosition = 0x08,
        /// <summary>
        /// Ticket in central position.
        /// </summary>
        TicketInCentralPosition = 0x10,
        /// <summary>
        /// Ticket in tail position.
        /// </summary>
        TicketInTailPosition = 0x20,
        /// <summary>
        /// Ticket in mouth position.
        /// </summary>
        TicketInMouthPosition = 0x40,
        /// <summary>
        /// No ticket inside the transport.
        /// </summary>
        NoTicketInsideTheTransport = 0x80,
        /// <summary>
        /// Read error.
        /// </summary>
        ReadError = 0x0100,
        /// <summary>
        /// Ticket in read position.
        /// </summary>
        TicketInReadPosition = 0x0200,
        /// <summary>
        /// Photocell failure.
        /// </summary>
        PhotocellFailure = 0x0400,
        /// <summary>
        /// Ticket already under printer.
        /// </summary>
        TicketAlreadyUnderPrinter = 0x0800,
        /// <summary>
        /// Jamming error.
        /// </summary>
        JammingError = 0x1000,
        /// <summary>
        /// Ticket under printer.
        /// </summary>
        TicketUnderPrinter = 0x2000,
        /// <summary>
        /// Verify error.
        /// </summary>
        VerifyError = 0x4000,
        /// <summary>
        /// Unknown.
        /// </summary>
        Unknown = 0x40000000
    }

}
