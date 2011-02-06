//Josip Medved <jmedved@jmedved.com>  http://www.jmedved.com  http://blog.jmedved.com

//2010-03-09: First version.


using System;
using System.Globalization;
using System.Text;

namespace Medo.Device.Protocol {

    /// <summary>
    /// A-bus communication version 2.10 (17.07.2009).
    /// </summary>
    public class ABusFrame {

        /// <summary>
        /// Creates new instance.
        /// </summary>
        private ABusFrame() {
        }

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="from">Source address.</param>
        /// <param name="to">Destination address.</param>
        /// <param name="messageDirection">Message direction.</param>
        /// <param name="messageType">Message type.</param>
        /// <param name="parameters">Parameters.</param>
        internal ABusFrame(int from, int to, ABusMessageStatus messageDirection, ABusMessageType messageType, byte[] parameters) {
            this.From = from;
            this.To = to;
            this.MessageStatus = messageDirection;
            this.MessageType = messageType;
            this.Parameters = parameters;
            this.PasswordByte1 = 0;
            this.PasswordByte2 = 0;
            this.IsCrcValid = true;
        }

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="frame">Request frame.</param>
        internal ABusFrame(ABusFrame frame) {
            this.From = frame.From;
            this.To = frame.To;
            this.MessageStatus = frame.MessageStatus;
            this.MessageType = frame.MessageType;
            this.Parameters = frame.Parameters;
            this.PasswordByte1 = frame.PasswordByte1;
            this.PasswordByte2 = frame.PasswordByte2;
            this.IsCrcValid = frame.IsCrcValid;
        }


        /// <summary>
        /// Gets source address.
        /// Number 0 is broadcast, 1 is CyPro, 2 to 999 is custom device, 1000 and above is CyBro.
        /// </summary>
        public int From { get; private set; }

        /// <summary>
        /// Gets destination address.
        /// Number 0 is broadcast, 1 is CyPro, 2 to 999 is custom device, 1000 and above is CyBro.
        /// </summary>
        public int To { get; private set; }

        /// <summary>
        /// Gets message direction.
        /// </summary>
        public ABusMessageStatus MessageStatus { get; private set; }

        /// <summary>
        /// Gets message type.
        /// </summary>
        public ABusMessageType MessageType { get; private set; }

        /// <summary>
        /// Gets whole data block.
        /// </summary>
        public byte[] Parameters { get; protected set; }

        /// <summary>
        /// Gets/sets password (first byte).
        /// </summary>
        public byte PasswordByte1 { get; private set; }

        /// <summary>
        /// Gets/sets password (second byte).
        /// </summary>
        public byte PasswordByte2 { get; private set; }

        /// <summary>
        /// Gets whether CRC is valid. It is always true for computer generated messages.
        /// </summary>
        public bool IsCrcValid { get; private set; }

        /// <summary>
        /// Length of complete frame.
        /// </summary>
        public int FrameLength {
            get {
                return 12 + 2 + this.Parameters.Length + 2 + 2;
            }
        }

        /// <summary>
        /// Sets password.
        /// </summary>
        /// <param name="passwordByte1">Password (first byte).</param>
        /// <param name="passwordByte2">Password (second byte).</param>
        public void SetPassword(byte passwordByte1, byte passwordByte2) {
            this.PasswordByte1 = passwordByte1;
            this.PasswordByte2 = passwordByte2;
        }

        /// <summary>
        /// Returns frame parsed from given bytes.
        /// </summary>
        /// <param name="bytes">Bytes from which frame is to be reconstructed.</param>
        public static ABusFrame Parse(byte[] bytes) {
            return Parse(bytes, null);
        }

        /// <summary>
        /// Returns frame parsed from given bytes.
        /// </summary>
        /// <param name="bytes">Bytes from which frame is to be reconstructed.</param>
        /// <param name="request">Request to be used in determining response type.</param>
        public static ABusFrame Parse(byte[] bytes, ABusRequest request) {
            if (bytes == null) { throw new ArgumentNullException("bytes", "Parameter cannot be null."); }
            if (bytes.Length < 16) { throw new FormatException("Frame is too short."); }
            if ((bytes[0] != 0xAA) || (bytes[1] != 0x55)) { throw new FormatException("Invalid frame start."); }

            int dataBlockAndPasswordLength = BitConverter.ToInt16(bytes, 2);
            if (12 + dataBlockAndPasswordLength + 2 > bytes.Length) { throw new FormatException("Frame is too short."); }

            var rawFrame = new ABusFrame();
            rawFrame.From = BitConverter.ToInt32(bytes, 4);
            rawFrame.To = BitConverter.ToInt32(bytes, 8);
            rawFrame.MessageStatus = (ABusMessageStatus)bytes[12];
            rawFrame.MessageType = (ABusMessageType)bytes[13];
            rawFrame.Parameters = new byte[dataBlockAndPasswordLength - 2 - 2];
            Array.Copy(bytes, 14, rawFrame.Parameters, 0, rawFrame.Parameters.Length);
            rawFrame.PasswordByte1 = bytes[rawFrame.FrameLength - 4];
            rawFrame.PasswordByte2 = bytes[rawFrame.FrameLength - 3];

            byte[] crc = CalculateCrc(bytes, 0, rawFrame.FrameLength - 2);
            rawFrame.IsCrcValid = ((crc[0] == bytes[rawFrame.FrameLength - 2]) && (crc[1] == bytes[rawFrame.FrameLength - 1]));

            if (rawFrame.IsCrcValid) {
                if (rawFrame.MessageStatus == ABusMessageStatus.Request) {
                    if (rawFrame.MessageType == ABusMessageType.Command) {
                        if (rawFrame.Parameters.Length >= 1) {
                            switch ((ABusCommand)rawFrame.Parameters[0]) {
                                case ABusCommand.Ping: return new ABusPingRequest(rawFrame);
                                case ABusCommand.ReadPlcStatus: return new ABusReadPlcStatusRequest(rawFrame);
                                case ABusCommand.ReadCodeBlock: return new ABusReadCodeBlockRequest(rawFrame);
                                case ABusCommand.ReadData: return new ABusReadDataRequest(rawFrame);
                                case ABusCommand.WriteData: return new ABusWriteDataRequest(rawFrame);
                                case ABusCommand.Start: return new ABusStartRequest(rawFrame);
                                case ABusCommand.Stop: return new ABusStopRequest(rawFrame);
                                case ABusCommand.Pause: return new ABusPauseRequest(rawFrame);
                                case ABusCommand.SetTime: return new ABusSetTimeRequest(rawFrame);
                                case ABusCommand.GetTime: return new ABusGetTimeRequest(rawFrame);
                            }
                            return new ABusRequest(rawFrame); //all other requests
                        }
                    }
                } else { //this is response
                    if (request != null) { //pairing request and response
                        if (rawFrame.MessageType == ABusMessageType.Command) {
                            switch (request.Command) {
                                case ABusCommand.Ping: return new ABusPingResponse(rawFrame);
                                case ABusCommand.ReadPlcStatus: return new ABusReadPlcStatusResponse(rawFrame);
                                case ABusCommand.ReadCodeBlock: return new ABusReadCodeBlockResponse(rawFrame);
                                case ABusCommand.ReadData: return new ABusReadDataResponse(rawFrame);
                                case ABusCommand.WriteData: return new ABusWriteDataResponse(rawFrame);
                                case ABusCommand.SetTime: return new ABusSetTimeResponse(rawFrame);
                                case ABusCommand.GetTime: return new ABusGetTimeResponse(rawFrame);
                            }
                        }
                    }
                    return new ABusResponse(rawFrame); //if everything else fails
                }
            }
            return rawFrame; //something is wrong with this one
        }

        /// <summary>
        /// Returns frame bytes.
        /// </summary>
        public byte[] GetBytes() {
            var bytesDataBlockAndPasswordLength = BitConverter.GetBytes((ushort)(2 + this.Parameters.Length + 2));
            var bytesFrom = BitConverter.GetBytes(this.From);
            var bytesTo = BitConverter.GetBytes(this.To);

            var bytes = new byte[this.FrameLength];
            bytes[0] = 0xAA;
            bytes[1] = 0x55;
            Array.Copy(bytesDataBlockAndPasswordLength, 0, bytes, 2, 2);
            Array.Copy(bytesFrom, 0, bytes, 4, 4);
            Array.Copy(bytesTo, 0, bytes, 8, 4);
            bytes[12] = (byte)this.MessageStatus;
            bytes[13] = (byte)this.MessageType;
            Array.Copy(this.Parameters, 0, bytes, 14, this.Parameters.Length);
            bytes[this.FrameLength - 4] = this.PasswordByte1;
            bytes[this.FrameLength - 3] = this.PasswordByte2;

            var bytesCrc = CalculateCrc(bytes, 0, this.FrameLength - 2);
            Array.Copy(bytesCrc, 0, bytes, this.FrameLength - 2, 2);

            return bytes;
        }

        /// <summary>
        /// Returns string representation.
        /// </summary>
        public override string ToString() {
            var text = new StringBuilder();
            text.Append(base.ToString());
            text.AppendFormat(CultureInfo.InvariantCulture, ": From='{0}'", this.From);
            text.AppendFormat(CultureInfo.InvariantCulture, "; To='{0}'", this.To);
            text.AppendFormat(CultureInfo.InvariantCulture, "; Direction='{0}'", this.MessageStatus);
            text.AppendFormat(CultureInfo.InvariantCulture, "; Type='{0}'", this.MessageType);
            if ((this.PasswordByte1 != 0) || (this.PasswordByte2 != 0)) {
                text.AppendFormat(CultureInfo.InvariantCulture, "; Password='0x{0:X2}{1:X2}'; ", this.PasswordByte1, this.PasswordByte2);
            }
            return text.ToString();
        }

        private readonly static int[] CrcPrimTable = new int[] { 0x049D, 0x0C07, 0x1591, 0x1ACF, 0x1D4B, 0x202D, 0x2507, 0x2B4B, 0x34A5, 0x38C5, 0x3D3F, 0x4445, 0x4D0F, 0x538F, 0x5FB3, 0x6BBF };

        private static byte[] CalculateCrc(byte[] input, int offset, int length) {
            int crc = 0;
            int index = 0;
            while (index < length) {
                crc = crc + (input[offset + index] ^ 0x5A) * CrcPrimTable[index & 0x0F];
                index += 1;
            }
            ushort crc2 = (ushort)(crc & 0xFFFF);
            return BitConverter.GetBytes(crc2);
        }

    }


    #region Enums

    /// <summary>
    /// A-bus message code.
    /// </summary>
    public enum ABusMessageStatus : byte {
        /// <summary>
        /// Request.
        /// </summary>
        Request = 0,
        /// <summary>
        /// ACK: acknowledge, response data valid.
        /// </summary>
        ResponseAcknowledge = 1,
        /// <summary>
        /// INTERNAL: unspecified error.
        /// </summary>
        ResponseErrorUnspecified = 2,
        /// <summary>
        /// BADCMD: unrecognized command.
        /// </summary>
        ResponseErrorUnrecognizedCommand = 3,
        /// <summary>
        /// BADPARA: parameter out of range.
        /// </summary>
        ResponseErrorParameterOutOfRange = 4,
        /// <summary>
        /// NOTREADY: device not ready.
        /// </summary>
        ResponseErrorDeviceNotReady = 5,
        /// <summary>
        /// NOPGM: no valid program.
        /// </summary>
        ResponseErrorNoValidProgram = 6,
        /// <summary>
        /// NOKERN: no valid kernel.
        /// </summary>
        ResponseErrorNoValidKernel = 7,
        /// <summary>
        /// NOSOCKET: no valid socket.
        /// </summary>
        ResponseErrorNoValidSocket = 8,
        /// <summary>
        /// NOMASTER: no valid master.
        /// </summary>
        ResponseErrorNoValidMaster = 9,
        /// <summary>
        /// BADRAM: RAM fail.
        /// </summary>
        ResponseErrorRamFail = 10,
        /// <summary>
        /// BADFLASH: FLASH fail.
        /// </summary>
        ResponseErrorFlashFail = 11,
        /// <summary>
        /// BADPASS: incorrect authentification.
        /// </summary>
        ResponseErrorIncorrectAuthentification = 12
    }


    /// <summary>
    /// A-bus message type.
    /// Values other than Command (0) are Socket IDs.
    /// </summary>
    public enum ABusMessageType : byte {
        /// <summary>
        /// COMMAND: message contains command (request or response).
        /// </summary>
        Command = 0
    }


    /// <summary>
    /// Command codes for request.
    /// </summary>
    public enum ABusCommand : byte {
        /// <summary>
        /// START_LOADER: Start loader.
        /// </summary>
        StartLoader = 0x00,
        /// <summary>
        /// START_KERNEL: Start kernel.
        /// </summary>
        StartKernel = 0x01,
        /// <summary>
        /// PING: Connection check.
        /// </summary>
        Ping = 0x10,
        /// <summary>
        /// RD_STATUS: Read PLC status.
        /// </summary>
        ReadPlcStatus = 0x11,
        /// <summary>
        /// RD_LOADHEAD: Read loader header.
        /// </summary>
        ReadLoaderHeader = 0x12,
        /// <summary>
        /// RD_KERNHEAD: Read kernel header.
        /// </summary>
        ReadKernelHeader = 0x13,
        /// <summary>
        /// FLASH_INIT: Flash erase.
        /// </summary>
        FlashErase = 0x20,
        /// <summary>
        /// RD_CODE: Read code memory block.
        /// </summary>
        ReadCodeBlock = 0x21,
        /// <summary>
        /// WR_CODE: Write code memory block.
        /// </summary>
        WriteCodeBlock = 0x22,
        /// <summary>
        /// DATA_INIT: Init PLC variables.
        /// </summary>
        InitPlcVariables = 0x30,
        /// <summary>
        /// RD_DATA: Read data memory block.
        /// </summary>
        ReadDataBlock = 0x31,
        /// <summary>
        /// WR_DATA: Write data memory block.
        /// </summary>
        WriteDataBlock = 0x32,
        /// <summary>
        /// RD_RANDOM: Random read data memory.
        /// </summary>
        ReadData = 0x33,
        /// <summary>
        /// WR_RANDOM: Random write data memory.
        /// </summary>
        WriteData = 0x34,
        /// <summary>
        /// PLC_START: CyBro start (stop -> run).
        /// </summary>
        Start = 0x40,
        /// <summary>
        /// PLC_STOP: CyBro stop (run -> stop).
        /// </summary>
        Stop = 0x41,
        /// <summary>
        /// MASTER_START: Start network master.
        /// </summary>
        StartNetworkMaster = 0x42,
        /// <summary>
        /// MASTER_STOP: Stop network master.
        /// </summary>
        StopNetworkMaster = 0x43,
        /// <summary>
        /// SEND_PERM: Permission to send socket or command.
        /// </summary>
        PermissionToSendSocketOrCommand = 0x44,
        /// <summary>
        /// PLC_PAUSE CyBro pause (run/stop -> pause).
        /// </summary>
        Pause = 0x45,
        /// <summary>
        /// SET_MONITOR: Define list of monitor variables.
        /// </summary>
        DefineMonitorVariables = 0x50,
        /// <summary>
        /// RD_MONITOR: Read monitor variables.
        /// </summary>
        ReadMonitorVariables = 0x51,
        /// <summary>
        /// RD_CARDID: Read card id.
        /// </summary>
        ReadCardId = 0x60,
        /// <summary>
        /// SET_RTC: Set CyBro date and time.
        /// </summary>
        SetTime = 0x70,
        /// <summary>
        /// GET_RTC: Get CyBro date and time.
        /// </summary>
        GetTime = 0x71,
        /// <summary>
        /// IEX_TUNNEL Tunnel can messages over A-bus.
        /// </summary>
        TunnelCanMessages = 0x80,
    }

    #endregion


    #region Generic request/response

    /// <summary>
    /// Generic A-bus request.
    /// </summary>
    public class ABusRequest : ABusFrame {

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="from">Source address.</param>
        /// <param name="to">Destination address.</param>
        /// <param name="command">Command.</param>
        /// <param name="commandParameters">Parameters.</param>
        public ABusRequest(int from, int to, ABusCommand command, byte[] commandParameters) :
            base(from, to, ABusMessageStatus.Request, ABusMessageType.Command, null) {
            this.Command = command;
            this.CommandParameters = commandParameters;
            if ((commandParameters != null) && (commandParameters.Length > 0)) {
                var parameters = new byte[1 + commandParameters.Length];
                parameters[0] = (byte)command;
                Array.Copy(commandParameters, 0, parameters, 1, commandParameters.Length);
                this.Parameters = parameters;
            } else {
                this.Parameters = new byte[] { (byte)command };
            }
        }

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="frame">Request frame.</param>
        internal ABusRequest(ABusFrame frame) :
            base(frame) {

            this.Command = (ABusCommand)frame.Parameters[0];
            if (frame.Parameters.Length > 1) {
                var cmdparams = new byte[frame.Parameters.Length - 1];
                Array.Copy(frame.Parameters, 1, cmdparams, 0, cmdparams.Length);
                this.CommandParameters = cmdparams;
            } else {
                this.CommandParameters = new byte[] { };
            }
        }

        /// <summary>
        /// Gets command.
        /// </summary>
        public ABusCommand Command { get; private set; }

        /// <summary>
        /// Gets command parameters.
        /// </summary>
        public byte[] CommandParameters { get; private set; }

    }


    /// <summary>
    /// Generic A-bus request.
    /// </summary>
    public class ABusResponse : ABusFrame {

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="request">Source address.</param>
        /// <param name="status">Response status.</param>
        /// <param name="parameters">Parameters.</param>
        public ABusResponse(ABusRequest request, ABusMessageStatus status, byte[] parameters) :
            base(request.To, request.From, status, ABusMessageType.Command, parameters) {
            this.IsError = (base.MessageStatus != ABusMessageStatus.ResponseAcknowledge);
        }

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="frame">Request frame.</param>
        internal ABusResponse(ABusFrame frame) :
            base(frame) {
            this.IsError = (base.MessageStatus != ABusMessageStatus.ResponseAcknowledge);
        }


        /// <summary>
        /// Gets whether response has error status.
        /// </summary>
        public bool IsError { get; private set; }

    }

    #endregion


    #region PING: Connection check.

    /// <summary>
    /// PING: Connection check.
    /// </summary>
    public class ABusPingRequest : ABusRequest {

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="from">Source address.</param>
        /// <param name="to">Destination address.</param>
        public ABusPingRequest(int from, int to) :
            base(from, to, ABusCommand.Ping, null) {
        }

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="frame">Request frame.</param>
        internal ABusPingRequest(ABusFrame frame) :
            base(frame) {
        }


        private new byte[] Parameters { get { return base.Parameters; } }

    }

    /// <summary>
    /// PING: Connection check.
    /// </summary>
    public class ABusPingResponse : ABusResponse {

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="request">Source address.</param>
        /// <param name="status">Response status.</param>
        public ABusPingResponse(ABusRequest request, ABusMessageStatus status) :
            base(request, status, null) {
        }

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="frame">Request frame.</param>
        internal ABusPingResponse(ABusFrame frame) :
            base(frame) {
        }


        private new byte[] Parameters { get { return base.Parameters; } }

    }

    #endregion


    #region RD_STATUS: Read PLC status.

    /// <summary>
    /// RD_STATUS: Read PLC status.
    /// </summary>
    public class ABusReadPlcStatusRequest : ABusRequest {

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="from">Source address.</param>
        /// <param name="to">Destination address.</param>
        public ABusReadPlcStatusRequest(int from, int to) :
            base(from, to, ABusCommand.ReadPlcStatus, null) {
        }

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="frame">Request frame.</param>
        internal ABusReadPlcStatusRequest(ABusFrame frame) :
            base(frame) {
        }


        private new byte[] Parameters { get { return base.Parameters; } }

    }

    /// <summary>
    /// RD_STATUS: Read PLC status.
    /// </summary>
    public class ABusReadPlcStatusResponse : ABusResponse {

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="request">Source address.</param>
        /// <param name="status">Response status.</param>
        /// <param name="systemStatus">System status.</param>
        /// <param name="plcStatus">PLC status.</param>
        public ABusReadPlcStatusResponse(ABusRequest request, ABusMessageStatus status, ABusSystemStatus systemStatus, ABusPlcStatus plcStatus) :
            base(request, status, new byte[] { (byte)systemStatus, (byte)plcStatus }) {

            this.SystemStatus = systemStatus;
            this.PlcStatus = plcStatus;

            base.Parameters = new byte[2];
            base.Parameters[0] = (byte)this.SystemStatus;
            base.Parameters[1] = (byte)this.PlcStatus;
        }

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="frame">Request frame.</param>
        internal ABusReadPlcStatusResponse(ABusFrame frame) :
            base(frame) {

            this.SystemStatus = (ABusSystemStatus)base.Parameters[0];
            this.PlcStatus = (ABusPlcStatus)base.Parameters[1];
        }


        /// <summary>
        /// Gets system status.
        /// </summary>
        public ABusSystemStatus SystemStatus { get; private set; }

        /// <summary>
        /// Gets PLC status.
        /// </summary>
        public ABusPlcStatus PlcStatus { get; private set; }


        private new byte[] Parameters { get { return base.Parameters; } }

        /// <summary>
        /// Returns string representation.
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            return base.ToString() + string.Format(CultureInfo.InvariantCulture, "; SystemStatus='{0}'; PlcStatus='{1}'", this.SystemStatus, this.PlcStatus);
        }

    }


    /// <summary>
    /// System status.
    /// </summary>
    public enum ABusSystemStatus : byte {
        /// <summary>
        /// Loader active (0).
        /// </summary>
        LoaderActive = 0,
        /// <summary>
        /// Kernel active (1).
        /// </summary>
        KernelActive = 1
    }

    /// <summary>
    /// PLC status.
    /// </summary>
    public enum ABusPlcStatus : byte {
        /// <summary>
        /// Stop (0).
        /// </summary>
        Stop = 0,
        /// <summary>
        /// Pause (1).
        /// </summary>
        Pause = 1,
        /// <summary>
        /// Run (2).
        /// </summary>
        Run = 2,
        /// <summary>
        /// No valid program (3).
        /// </summary>
        NoValidProgram = 3,
        /// <summary>
        /// Congestion error (4).
        /// </summary>
        CongestionError = 4,
    }

    #endregion


    #region RD_CODE: Read code memory block.

    /// <summary>
    /// RD_CODE: Read code memory block.
    /// </summary>
    public class ABusReadCodeBlockRequest : ABusRequest {

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="from">Source address.</param>
        /// <param name="to">Destination address.</param>
        /// <param name="segmentNumber">Segment number.</param>
        /// <param name="blockSize">Block size.</param>
        public ABusReadCodeBlockRequest(int from, int to, short segmentNumber, short blockSize) :
            base(from, to, ABusCommand.ReadCodeBlock, null) {

            this.SegmentNumber = segmentNumber;
            this.BlockSize = blockSize;

            base.Parameters = new byte[5];
            base.Parameters[0] = (byte)base.Command;
            Array.Copy(BitConverter.GetBytes(this.SegmentNumber), 0, base.Parameters, 1, 2);
            Array.Copy(BitConverter.GetBytes(this.BlockSize), 0, base.Parameters, 3, 2);
        }

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="frame">Request frame.</param>
        internal ABusReadCodeBlockRequest(ABusFrame frame) :
            base(frame) {

            this.SegmentNumber = BitConverter.ToInt16(frame.Parameters, 1);
            this.BlockSize = BitConverter.ToInt16(frame.Parameters, 3);
        }


        /// <summary>
        /// Gets segment number.
        /// </summary>
        public short SegmentNumber { get; private set; }

        /// <summary>
        /// Gets block size.
        /// </summary>
        public short BlockSize { get; private set; }


        private new byte[] Parameters { get { return base.Parameters; } }

        /// <summary>
        /// Returns string representation.
        /// </summary>
        public override string ToString() {
            return base.ToString() + string.Format(CultureInfo.InvariantCulture, "; SegmentNumber='{0}'; BlockSize='{1}'", this.SegmentNumber, this.BlockSize);
        }

    }

    /// <summary>
    /// RD_CODE: Read code memory block.
    /// </summary>
    public class ABusReadCodeBlockResponse : ABusResponse {

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="request">Source address.</param>
        /// <param name="status">Response status.</param>
        /// <param name="codeBlock">Code block.</param>
        public ABusReadCodeBlockResponse(ABusRequest request, ABusMessageStatus status, byte[] codeBlock) :
            base(request, status, codeBlock) {
        }

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="frame">Request frame.</param>
        internal ABusReadCodeBlockResponse(ABusFrame frame) :
            base(frame) {
        }


        /// <summary>
        /// Gets content of variables as big array.
        /// </summary>
        public byte[] CodeBlock {
            get { return base.Parameters; }
        }

        private new byte[] Parameters { get { return base.Parameters; } }

        /// <summary>
        /// Returns string representation.
        /// </summary>
        public override string ToString() {
            var text = new StringBuilder();
            text.Append(base.ToString());
            if (this.CodeBlock != null) {
                if (this.CodeBlock.Length > 0) {
                    text.AppendFormat(CultureInfo.InvariantCulture, "; CodeBlock[{0}]='0x", this.CodeBlock.Length);
                    for (int i = 0; i < this.CodeBlock.Length; ++i) {
                        if (i > 8) {
                            text.Append("...");
                            break;
                        }
                        text.Append(this.CodeBlock[i].ToString("X2", CultureInfo.InvariantCulture));
                    }
                    text.Append("'");
                } else {
                    text.Append("; CodeBlock[0]=''");
                }
            }
            return text.ToString();
        }

    }

    #endregion


    #region RD_RANDOM: Random read data memory.

    /// <summary>
    /// RD_RANDOM: Random read data memory.
    /// </summary>
    public class ABusReadDataRequest : ABusRequest {

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="from">Source address.</param>
        /// <param name="to">Destination address.</param>
        /// <param name="byteVariablePointers">Array of 1-byte variable pointers.</param>
        /// <param name="wordVariablePointers">Array of 2-byte variable pointers.</param>
        /// <param name="longVariablePointers">Array of 4-byte variable pointers.</param>
        public ABusReadDataRequest(int from, int to, int[] byteVariablePointers, int[] wordVariablePointers, int[] longVariablePointers) :
            base(from, to, ABusCommand.ReadData, null) {

            this.ByteVariablePointers = (byteVariablePointers != null) ? byteVariablePointers : new int[] { };
            this.WordVariablePointers = (wordVariablePointers != null) ? wordVariablePointers : new int[] { };
            this.LongVariablePointers = (longVariablePointers != null) ? longVariablePointers : new int[] { };

            base.Parameters = new byte[1 + 2 + 2 + 2 + (this.ByteVariablePointers.Length + this.WordVariablePointers.Length + this.LongVariablePointers.Length) * 2];
            base.Parameters[0] = (byte)base.Command;
            Array.Copy(BitConverter.GetBytes(this.ByteVariablePointers.Length), 0, base.Parameters, 1, 2);
            Array.Copy(BitConverter.GetBytes(this.WordVariablePointers.Length), 0, base.Parameters, 3, 2);
            Array.Copy(BitConverter.GetBytes(this.LongVariablePointers.Length), 0, base.Parameters, 5, 2);
            for (int i = 0; i < this.ByteVariablePointers.Length; ++i) {
                Array.Copy(BitConverter.GetBytes((ushort)this.ByteVariablePointers[i]), 0, base.Parameters, 7 + i * 2, 2);
            }
            for (int i = 0; i < this.WordVariablePointers.Length; ++i) {
                Array.Copy(BitConverter.GetBytes((ushort)this.WordVariablePointers[i]), 0, base.Parameters, 7 + this.ByteVariablePointers.Length * 2 + i * 2, 2);
            }
            for (int i = 0; i < this.LongVariablePointers.Length; ++i) {
                Array.Copy(BitConverter.GetBytes((ushort)this.LongVariablePointers[i]), 0, base.Parameters, 7 + this.ByteVariablePointers.Length * 2 + this.WordVariablePointers.Length * 2 + i * 2, 2);
            }
        }

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="frame">Request frame.</param>
        internal ABusReadDataRequest(ABusFrame frame) :
            base(frame) {

            int byteVars = BitConverter.ToUInt16(frame.Parameters, 1);
            int wordVars = BitConverter.ToUInt16(frame.Parameters, 3);
            int longVars = BitConverter.ToUInt16(frame.Parameters, 5);
            this.ByteVariablePointers = new int[byteVars];
            for (int i = 0; i < byteVars; ++i) {
                this.ByteVariablePointers[i] = BitConverter.ToUInt16(frame.Parameters, 7 + i * 2);
            }
            this.WordVariablePointers = new int[wordVars];
            for (int i = 0; i < wordVars; ++i) {
                this.WordVariablePointers[i] = BitConverter.ToUInt16(frame.Parameters, 7 + byteVars * 2 + i * 2);
            }
            this.LongVariablePointers = new int[longVars];
            for (int i = 0; i < longVars; ++i) {
                this.LongVariablePointers[i] = BitConverter.ToUInt16(frame.Parameters, 7 + byteVars * 2 + wordVars * 2 + i * 2);
            }
        }


        /// <summary>
        /// Gets array of 1-byte variable pointers.
        /// </summary>
        public int[] ByteVariablePointers { get; private set; }

        /// <summary>
        /// Gets array of 2-byte variable pointers.
        /// </summary>
        public int[] WordVariablePointers { get; private set; }

        /// <summary>
        /// Gets array of 4-byte variable pointers.
        /// </summary>
        public int[] LongVariablePointers { get; private set; }


        private new byte[] Parameters { get { return base.Parameters; } }

        /// <summary>
        /// Returns string representation.
        /// </summary>
        public override string ToString() {
            var text = new StringBuilder();
            text.Append(base.ToString());
            if (this.ByteVariablePointers.Length > 0) {
                text.AppendFormat(CultureInfo.InvariantCulture, "; ByteVariablePointers[{0}]='", this.ByteVariablePointers.Length);
                text.AppendFormat(CultureInfo.InvariantCulture, "0x{0:X4}", this.ByteVariablePointers[0]);
                for (int i = 1; i < this.ByteVariablePointers.Length; ++i) {
                    if (i > 8) {
                        text.Append("...");
                        break;
                    }
                    text.AppendFormat(CultureInfo.InvariantCulture, ",0x{0:X4}", this.ByteVariablePointers[i]);
                }
                text.Append("'");
            } else {
                text.Append("; ByteVariablePointers[0]=''");
            }
            if (this.WordVariablePointers.Length > 0) {
                text.AppendFormat(CultureInfo.InvariantCulture, "; WordVariablePointers[{0}]='", this.WordVariablePointers.Length);
                text.AppendFormat(CultureInfo.InvariantCulture, "0x{0:X4}", this.WordVariablePointers[0]);
                for (int i = 1; i < this.ByteVariablePointers.Length; ++i) {
                    if (i > 8) {
                        text.Append("...");
                        break;
                    }
                    text.AppendFormat(CultureInfo.InvariantCulture, ",0x{0:X4}", this.WordVariablePointers[i]);
                }
                text.Append("'");
            } else {
                text.Append("; WordVariablePointers[0]=''");
            }
            if (this.LongVariablePointers.Length > 0) {
                text.AppendFormat(CultureInfo.InvariantCulture, "; LongVariablePointers[{0}]='", this.LongVariablePointers.Length);
                text.AppendFormat(CultureInfo.InvariantCulture, "0x{0:X4}", this.LongVariablePointers[0]);
                for (int i = 1; i < this.LongVariablePointers.Length; ++i) {
                    if (i > 8) {
                        text.Append("...");
                        break;
                    }
                    text.AppendFormat(CultureInfo.InvariantCulture, ",0x{0:X4}", this.LongVariablePointers[i]);
                }
                text.Append("'");
            } else {
                text.Append("; LongVariablePointers[0]=''");
            }
            return text.ToString();
        }

    }

    /// <summary>
    /// RD_RANDOM: Random read data memory.
    /// </summary>
    public class ABusReadDataResponse : ABusResponse {

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="request">Source address.</param>
        /// <param name="status">Response status.</param>
        /// <param name="dataBlock">Data block.</param>
        public ABusReadDataResponse(ABusRequest request, ABusMessageStatus status, byte[] dataBlock) :
            base(request, status, dataBlock) {
        }

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="frame">Request frame.</param>
        internal ABusReadDataResponse(ABusFrame frame) :
            base(frame) {
        }


        /// <summary>
        /// Gets data block with content of all variables.
        /// </summary>
        public byte[] DataBlock {
            get { return base.Parameters; }
        }


        private new byte[] Parameters { get { return base.Parameters; } }

        /// <summary>
        /// Returns string representation.
        /// </summary>
        public override string ToString() {
            var text = new StringBuilder();
            text.Append(base.ToString());
            if (this.DataBlock != null) {
                if (this.DataBlock.Length > 0) {
                    text.AppendFormat(CultureInfo.InvariantCulture, "; DataBlock[{0}]='0x", this.DataBlock.Length);
                    for (int i = 0; i < this.DataBlock.Length; ++i) {
                        if (i > 8) {
                            //text.Append("...");
                            //break;
                        }
                        text.Append(this.DataBlock[i].ToString("X2", CultureInfo.InvariantCulture));
                    }
                    text.Append("'");
                } else {
                    text.Append("; DataBlock[0]=''");
                }
            }
            return text.ToString();
        }

    }

    #endregion


    #region WR_RANDOM: Random write data memory.

    /// <summary>
    /// WR_RANDOM: Random write data memory.
    /// </summary>
    public class ABusWriteDataRequest : ABusRequest {

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="from">Source address.</param>
        /// <param name="to">Destination address.</param>
        /// <param name="byteVariablePointers">Array of 1-byte variable pointers.</param>
        /// <param name="wordVariablePointers">Array of 2-byte variable pointers.</param>
        /// <param name="longVariablePointers">Array of 4-byte variable pointers.</param>
        /// <param name="dataBlock">Data block.</param>
        public ABusWriteDataRequest(int from, int to, int[] byteVariablePointers, int[] wordVariablePointers, int[] longVariablePointers, byte[] dataBlock) :
            base(from, to, ABusCommand.ReadData, null) {

            this.ByteVariablePointers = (byteVariablePointers != null) ? byteVariablePointers : new int[] { };
            this.WordVariablePointers = (wordVariablePointers != null) ? wordVariablePointers : new int[] { };
            this.LongVariablePointers = (longVariablePointers != null) ? longVariablePointers : new int[] { };

            this.DataBlock = (byteVariablePointers != null) ? dataBlock : new byte[] { };

            if (this.DataBlock.Length != (this.ByteVariablePointers.Length + this.WordVariablePointers.Length * 2 + this.LongVariablePointers.Length * 4)) { throw new ArgumentException("Data block size does not match size as defined by pointers.", "dataBlock"); }
            int totalPointerCount = this.ByteVariablePointers.Length + this.WordVariablePointers.Length + this.LongVariablePointers.Length;

            base.Parameters = new byte[1 + 2 + 2 + 2 + totalPointerCount * 2 + this.DataBlock.Length];
            base.Parameters[0] = (byte)base.Command;
            Array.Copy(BitConverter.GetBytes((ushort)this.ByteVariablePointers.Length), 0, base.Parameters, 1, 2);
            Array.Copy(BitConverter.GetBytes((ushort)this.WordVariablePointers.Length), 0, base.Parameters, 3, 2);
            Array.Copy(BitConverter.GetBytes((ushort)this.LongVariablePointers.Length), 0, base.Parameters, 5, 2);
            for (int i = 0; i < this.ByteVariablePointers.Length; ++i) {
                Array.Copy(BitConverter.GetBytes((ushort)(this.ByteVariablePointers[i])), 0, base.Parameters, 7 + i * 2, 2);
            }
            for (int i = 0; i < this.WordVariablePointers.Length; ++i) {
                Array.Copy(BitConverter.GetBytes((ushort)(this.WordVariablePointers[i])), 0, base.Parameters, 7 + this.ByteVariablePointers.Length * 2 + i * 2, 2);
            }
            for (int i = 0; i < this.LongVariablePointers.Length; ++i) {
                Array.Copy(BitConverter.GetBytes((ushort)(this.LongVariablePointers[i])), 0, base.Parameters, 7 + this.ByteVariablePointers.Length * 2 + this.WordVariablePointers.Length * 2 + i * 2, 2);
            }
            if (this.DataBlock.Length > 0) {
                Array.Copy(this.DataBlock, 0, this.Parameters, 1 + 2 + 2 + 2 + totalPointerCount * 2, this.DataBlock.Length);
            }
        }

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="frame">Request frame.</param>
        internal ABusWriteDataRequest(ABusFrame frame) :
            base(frame) {

            int byteVariableCount = BitConverter.ToUInt16(frame.Parameters, 1);
            int wordVariableCount = BitConverter.ToUInt16(frame.Parameters, 3);
            int longVariableCount = BitConverter.ToUInt16(frame.Parameters, 5);
            int varCount = byteVariableCount + wordVariableCount + longVariableCount;

            this.ByteVariablePointers = new int[byteVariableCount];
            for (int i = 0; i < this.ByteVariablePointers.Length; ++i) {
                this.ByteVariablePointers[i] = BitConverter.ToUInt16(frame.Parameters, 7 + i * 2);
            }

            this.WordVariablePointers = new int[wordVariableCount];
            for (int i = 0; i < this.WordVariablePointers.Length; ++i) {
                this.WordVariablePointers[i] = BitConverter.ToUInt16(frame.Parameters, 7 + this.ByteVariablePointers.Length * 2 + i * 2);
            }

            this.LongVariablePointers = new int[longVariableCount];
            for (int i = 0; i < this.LongVariablePointers.Length; ++i) {
                this.LongVariablePointers[i] = BitConverter.ToUInt16(frame.Parameters, 7 + this.ByteVariablePointers.Length * 2 + this.WordVariablePointers.Length * 2 + i * 2);
            }

            int dataBlockCount = byteVariableCount * 1 + wordVariableCount * 2 + longVariableCount * 4;
            this.DataBlock = new byte[dataBlockCount];
            Array.Copy(this.Parameters, 1 + 2 + 2 + 2 + varCount * 2, this.DataBlock, 0, dataBlockCount);
        }


        /// <summary>
        /// Gets array of 1-byte variable pointers.
        /// </summary>
        public int[] ByteVariablePointers { get; private set; }

        /// <summary>
        /// Gets array of 2-byte variable pointers.
        /// </summary>
        public int[] WordVariablePointers { get; private set; }

        /// <summary>
        /// Gets array of 4-byte variable pointers.
        /// </summary>
        public int[] LongVariablePointers { get; private set; }


        /// <summary>
        /// Gets data block.
        /// </summary>
        public byte[] DataBlock { get; private set; }


        private new byte[] Parameters { get { return base.Parameters; } }

        /// <summary>
        /// Returns string representation.
        /// </summary>
        public override string ToString() {
            var text = new StringBuilder();
            text.Append(base.ToString());
            if (this.ByteVariablePointers.Length > 0) {
                text.AppendFormat(CultureInfo.InvariantCulture, "; ByteVariablePointers[{0}]='", this.ByteVariablePointers.Length);
                text.AppendFormat(CultureInfo.InvariantCulture, "0x{0:X4}", this.ByteVariablePointers[0]);
                for (int i = 1; i < this.ByteVariablePointers.Length; ++i) {
                    if (i > 8) {
                        text.Append("...");
                        break;
                    }
                    text.AppendFormat(CultureInfo.InvariantCulture, ",0x{0:X4}", this.ByteVariablePointers[i]);
                }
                text.Append("'");
            } else {
                text.Append("; ByteVariablePointers[0]=''");
            }
            if (this.WordVariablePointers.Length > 0) {
                text.AppendFormat(CultureInfo.InvariantCulture, "; WordVariablePointers[{0}]='", this.WordVariablePointers.Length);
                text.AppendFormat(CultureInfo.InvariantCulture, "0x{0:X4}", this.WordVariablePointers[0]);
                for (int i = 1; i < this.ByteVariablePointers.Length; ++i) {
                    if (i > 8) {
                        text.Append("...");
                        break;
                    }
                    text.AppendFormat(CultureInfo.InvariantCulture, ",0x{0:X4}", this.WordVariablePointers[i]);
                }
                text.Append("'");
            } else {
                text.Append("; WordVariablePointers[0]=''");
            }
            if (this.LongVariablePointers.Length > 0) {
                text.AppendFormat(CultureInfo.InvariantCulture, "; LongVariablePointers[{0}]='", this.LongVariablePointers.Length);
                text.AppendFormat(CultureInfo.InvariantCulture, "0x{0:X4}", this.LongVariablePointers[0]);
                for (int i = 1; i < this.LongVariablePointers.Length; ++i) {
                    if (i > 8) {
                        text.Append("...");
                        break;
                    }
                    text.AppendFormat(CultureInfo.InvariantCulture, ",0x{0:X4}", this.LongVariablePointers[i]);
                }
                text.Append("'");
            } else {
                text.Append("; LongVariablePointers[0]=''");
            }
            if (this.DataBlock != null) {
                if (this.DataBlock.Length > 0) {
                    text.AppendFormat(CultureInfo.InvariantCulture, "; DataBlock[{0}]='0x", this.DataBlock.Length);
                    for (int i = 0; i < this.DataBlock.Length; ++i) {
                        if (i > 8) {
                            text.Append("...");
                            break;
                        }
                        text.Append(this.DataBlock[i].ToString("X2", CultureInfo.InvariantCulture));
                    }
                    text.Append("'");
                } else {
                    text.Append("; DataBlock[0]=''");
                }
            }
            return text.ToString();
        }

    }

    /// <summary>
    /// WR_RANDOM: Random write data memory.
    /// </summary>
    public class ABusWriteDataResponse : ABusResponse {

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="request">Source address.</param>
        /// <param name="status">Response status.</param>
        public ABusWriteDataResponse(ABusRequest request, ABusMessageStatus status) :
            base(request, status, null) {
        }

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="frame">Request frame.</param>
        internal ABusWriteDataResponse(ABusFrame frame) :
            base(frame) {
        }


        private new byte[] Parameters { get { return base.Parameters; } }

    }

    #endregion


    #region PLC_START: CyBro start (stop => run).

    /// <summary>
    /// PLC_START: CyBro start.
    /// </summary>
    public class ABusStartRequest : ABusRequest {

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="from">Source address.</param>
        /// <param name="to">Destination address.</param>
        public ABusStartRequest(int from, int to) :
            base(from, to, ABusCommand.Start, new byte[] { }) {
        }

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="frame">Request frame.</param>
        internal ABusStartRequest(ABusFrame frame) :
            base(frame) {
        }


        private new byte[] Parameters { get { return base.Parameters; } }

    }

    /// <summary>
    /// PLC_START: CyBro start.
    /// </summary>
    public class ABusStartResponse : ABusResponse {

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="request">Source address.</param>
        /// <param name="status">Response status.</param>
        public ABusStartResponse(ABusRequest request, ABusMessageStatus status) :
            base(request, status, new byte[] { }) {
        }

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="frame">Request frame.</param>
        internal ABusStartResponse(ABusFrame frame) :
            base(frame) {
        }

    }

    #endregion


    #region PLC_STOP: CyBro stop (run => stop).

    /// <summary>
    /// PLC_STOP: CyBro stop.
    /// </summary>
    public class ABusStopRequest : ABusRequest {

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="from">Source address.</param>
        /// <param name="to">Destination address.</param>
        public ABusStopRequest(int from, int to) :
            base(from, to, ABusCommand.Stop, new byte[] { }) {
        }

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="frame">Request frame.</param>
        internal ABusStopRequest(ABusFrame frame) :
            base(frame) {
        }


        private new byte[] Parameters { get { return base.Parameters; } }

    }

    /// <summary>
    /// PLC_STOP: CyBro stop.
    /// </summary>
    public class ABusStopResponse : ABusResponse {

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="request">Source address.</param>
        /// <param name="status">Response status.</param>
        public ABusStopResponse(ABusRequest request, ABusMessageStatus status) :
            base(request, status, new byte[] { }) {
        }

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="frame">Request frame.</param>
        internal ABusStopResponse(ABusFrame frame) :
            base(frame) {
        }

    }

    #endregion


    #region PLC_PAUSE: CyBro pause (run/stop => pause).

    /// <summary>
    /// PLC_PAUSE: CyBro pause.
    /// </summary>
    public class ABusPauseRequest : ABusRequest {

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="from">Source address.</param>
        /// <param name="to">Destination address.</param>
        public ABusPauseRequest(int from, int to) :
            base(from, to, ABusCommand.Pause, new byte[] { }) {
        }

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="frame">Request frame.</param>
        internal ABusPauseRequest(ABusFrame frame) :
            base(frame) {
        }


        private new byte[] Parameters { get { return base.Parameters; } }

    }

    /// <summary>
    /// PLC_PAUSE: CyBro pause.
    /// </summary>
    public class ABusPauseResponse : ABusResponse {

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="request">Source address.</param>
        /// <param name="status">Response status.</param>
        public ABusPauseResponse(ABusRequest request, ABusMessageStatus status) :
            base(request, status, new byte[] { }) {
        }

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="frame">Request frame.</param>
        internal ABusPauseResponse(ABusFrame frame) :
            base(frame) {
        }

    }

    #endregion


    #region SET_RTC: Set CyBro date and time.

    /// <summary>
    /// SET_RTC: Set CyBro date and time.
    /// </summary>
    public class ABusSetTimeRequest : ABusRequest {

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="from">Source address.</param>
        /// <param name="to">Destination address.</param>
        /// <param name="time">Time.</param>
        public ABusSetTimeRequest(int from, int to, DateTime time) :
            base(from, to, ABusCommand.SetTime, null) {

            this.Time = time;

            base.Parameters = new byte[8];
            base.Parameters[0] = (byte)base.Command;
            base.Parameters[1] = (byte)(time.Year - 2000);
            base.Parameters[2] = (byte)time.Month;
            base.Parameters[3] = (byte)time.Day;
            base.Parameters[4] = (byte)time.DayOfWeek;
            base.Parameters[5] = (byte)time.Hour;
            base.Parameters[6] = (byte)time.Minute;
            base.Parameters[7] = (byte)time.Second;
        }

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="frame">Request frame.</param>
        internal ABusSetTimeRequest(ABusFrame frame) :
            base(frame) {

            this.Time = new DateTime(2000 + frame.Parameters[1], frame.Parameters[2], frame.Parameters[3], frame.Parameters[5], frame.Parameters[6], frame.Parameters[7], DateTimeKind.Local);
        }


        /// <summary>
        /// Gets time.
        /// </summary>
        public DateTime Time { get; private set; }
        private new byte[] Parameters { get { return base.Parameters; } }

        /// <summary>
        /// Returns string representation.
        /// </summary>
        public override string ToString() {
            return base.ToString() + string.Format(CultureInfo.InvariantCulture, @"; Time='{0:yyyyMMdd\THHmmss}'", this.Time);
        }

    }

    /// <summary>
    /// SET_RTC: Set CyBro date and time.
    /// </summary>
    public class ABusSetTimeResponse : ABusResponse {

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="request">Source address.</param>
        /// <param name="status">Response status.</param>
        public ABusSetTimeResponse(ABusRequest request, ABusMessageStatus status) :
            base(request, status, null) {
        }

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="frame">Request frame.</param>
        internal ABusSetTimeResponse(ABusFrame frame) :
            base(frame) {
        }


        private new byte[] Parameters { get { return base.Parameters; } }

    }

    #endregion


    #region GET_RTC: Get CyBro date and time.

    /// <summary>
    /// GET_RTC: Get CyBro date and time.
    /// </summary>
    public class ABusGetTimeRequest : ABusRequest {

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="from">Source address.</param>
        /// <param name="to">Destination address.</param>
        public ABusGetTimeRequest(int from, int to) :
            base(from, to, ABusCommand.GetTime, null) {
        }

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="frame">Request frame.</param>
        internal ABusGetTimeRequest(ABusFrame frame) :
            base(frame) {
        }


        private new byte[] Parameters { get { return base.Parameters; } }

    }

    /// <summary>
    /// GET_RTC: Get CyBro date and time.
    /// </summary>
    public class ABusGetTimeResponse : ABusResponse {

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="request">Source address.</param>
        /// <param name="status">Response status.</param>
        /// <param name="time">Time.</param>
        public ABusGetTimeResponse(ABusRequest request, ABusMessageStatus status, DateTime time) :
            base(request, status, new byte[] { }) {

            this.Time = time;

            base.Parameters = new byte[7];
            base.Parameters[0] = (byte)(this.Time.Year - 2000);
            base.Parameters[1] = (byte)this.Time.Month;
            base.Parameters[2] = (byte)this.Time.Day;
            base.Parameters[3] = (byte)this.Time.DayOfWeek;
            base.Parameters[4] = (byte)this.Time.Hour;
            base.Parameters[5] = (byte)this.Time.Minute;
            base.Parameters[6] = (byte)this.Time.Second;
        }

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="frame">Request frame.</param>
        internal ABusGetTimeResponse(ABusFrame frame) :
            base(frame) {

            this.Time = new DateTime(2000 + base.Parameters[0], base.Parameters[1], base.Parameters[2], base.Parameters[4], base.Parameters[5], base.Parameters[6], DateTimeKind.Local);
        }


        /// <summary>
        /// Gets time.
        /// </summary>
        public DateTime Time { get; private set; }


        private new byte[] Parameters { get { return base.Parameters; } }

        /// <summary>
        /// Returns string representation.
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            return base.ToString() + string.Format(CultureInfo.InvariantCulture, @"; Time='{0:yyyyMMdd\THHmmss}'", this.Time);
        }

    }

    #endregion

}
