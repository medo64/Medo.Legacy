/* Josip Medved <jmedved@jmedved.com> * www.medo64.com * MIT License */

//2010-06-02: Updated host name discovery to handle non-ascii characters gracefully.
//            Cleaned CA1822:MarkMembersAsStatic and CA1062:Validate arguments of public methods.
//2008-04-11: Integrated BsdSyslogMessage.
//            Moved inner classes outwards.
//2008-01-03: Added Resources.
//            Added Helper.
//2007-10-30: Moved to common.


using System.Globalization;
using System.Diagnostics;
using System.Text;
using System.Reflection;
using System.Net;

namespace Medo.Net {

    /// <summary>
    /// Syslog message as defined in RFC 5424 (The Syslog Protocol) and support for syslog message as defined in RFC 3164.
    /// </summary>
    public class SyslogMessage {

        /// <summary>
        /// Gets default syslog port.
        /// </summary>
        public static int DefaultPort { get { return 514; } }



        /// <summary>
        /// Creates new instance.
        /// </summary>
        private SyslogMessage() { } //used for Parse

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="message">Message.</param>
        public SyslogMessage(string message)
            : this(message, SyslogFacilityCode.UserLevel, SyslogSeverityCode.Notice, false) {
        }

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="facility">Facility.</param>
        /// <param name="severity">Severity.</param>
        /// <param name="includeOrigin">If true, origin Structured Data will be included.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Argument must contain only printable US ASCII characters.</exception>
        public SyslogMessage(string message, SyslogFacilityCode facility, SyslogSeverityCode severity, bool includeOrigin)
            : this(message, facility, severity, includeOrigin, null, null, null, null, System.DateTime.Now) {
        }

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="facility">Facility.</param>
        /// <param name="severity">Severity.</param>
        /// <param name="includeOrigin">If true, origin Structured Data will be included.</param>
        /// <param name="hostName">Host name.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Argument must contain only printable US ASCII characters.</exception>
        public SyslogMessage(string message, SyslogFacilityCode facility, SyslogSeverityCode severity, bool includeOrigin, string hostName)
            : this(message, facility, severity, includeOrigin, hostName, null, null, null, System.DateTime.Now) {
        }

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="facility">Facility.</param>
        /// <param name="severity">Severity.</param>
        /// <param name="includeOrigin">If true, origin Structured Data will be included.</param>
        /// <param name="hostName">Host name.</param>
        /// <param name="applicationName">Application parameterName. Must contain only printable US ASCII characters (%d33-126) and cannot be longer than 48 characters.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Argument must contain only printable US ASCII characters.</exception>
        public SyslogMessage(string message, SyslogFacilityCode facility, SyslogSeverityCode severity, bool includeOrigin, string hostName, string applicationName)
            : this(message, facility, severity, includeOrigin, hostName, applicationName, null, null, System.DateTime.Now) {
        }

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="facility">Facility.</param>
        /// <param name="severity">Severity.</param>
        /// <param name="includeOrigin">If true, origin Structured Data will be included.</param>
        /// <param name="hostName">Host name.</param>
        /// <param name="applicationName">Application parameterName. Must contain only printable US ASCII characters (%d33-126) and cannot be longer than 48 characters.</param>
        /// <param name="processId">Process Id. Must contain only printable US ASCII characters (%d33-126) and cannot be longer than 128 characters.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Argument must contain only printable US ASCII characters.</exception>
        public SyslogMessage(string message, SyslogFacilityCode facility, SyslogSeverityCode severity, bool includeOrigin, string hostName, string applicationName, string processId)
            : this(message, facility, severity, includeOrigin, hostName, applicationName, processId, null, System.DateTime.Now) {
        }

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="facility">Facility.</param>
        /// <param name="severity">Severity.</param>
        /// <param name="includeOrigin">If true, origin Structured Data will be included.</param>
        /// <param name="hostName">Host name.</param>
        /// <param name="applicationName">Application parameterName. Must contain only printable US ASCII characters (%d33-126) and cannot be longer than 48 characters.</param>
        /// <param name="processId">Process Id. Must contain only printable US ASCII characters (%d33-126) and cannot be longer than 128 characters.</param>
        /// <param name="messageId">Message Id. Must contain only printable US ASCII characters (%d33-126) and cannot be longer than 32 characters.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Argument must contain only printable US ASCII characters.</exception>
        public SyslogMessage(string message, SyslogFacilityCode facility, SyslogSeverityCode severity, bool includeOrigin, string hostName, string applicationName, string processId, string messageId)
            : this(message, facility, severity, includeOrigin, hostName, applicationName, processId, messageId, System.DateTime.Now) {
        }

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="facility">Facility.</param>
        /// <param name="severity">Severity.</param>
        /// <param name="includeOrigin">If true, origin Structured Data will be included.</param>
        /// <param name="hostName">Host name.</param>
        /// <param name="applicationName">Application parameterName. Must contain only printable US ASCII characters (%d33-126) and cannot be longer than 48 characters.</param>
        /// <param name="processId">Process Id. Must contain only printable US ASCII characters (%d33-126) and cannot be longer than 128 characters.</param>
        /// <param name="messageId">Message Id. Must contain only printable US ASCII characters (%d33-126) and cannot be longer than 32 characters.</param>
        /// <param name="timestamp">Timestamp.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Argument must contain only printable US ASCII characters.</exception>
        public SyslogMessage(string message, SyslogFacilityCode facility, SyslogSeverityCode severity, bool includeOrigin, string hostName, string applicationName, string processId, string messageId, System.DateTime timestamp) {
            if (hostName == null) {
                hostName = GetPrintableUSAscii(System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName()).HostName, 255);
            }
            if (!Helper.IsPrintUsAscii(hostName, 255)) { throw new System.ArgumentOutOfRangeException("hostName", Resources.ExceptionArgumentMustContainOnlyPrintableUsAsciiCharactersWithLengthOfNoMoreThan255Characters); }

            if (applicationName == null) {
                var assembly = Assembly.GetEntryAssembly();
                var productAttributes = assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), true);
                if ((productAttributes != null) && (productAttributes.Length >= 1)) {
                    applicationName = ((AssemblyProductAttribute)productAttributes[productAttributes.Length - 1]).Product;
                } else {
                    var titleAttributes = assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), true);
                    if ((titleAttributes != null) && (titleAttributes.Length >= 1)) {
                        applicationName = ((AssemblyTitleAttribute)titleAttributes[titleAttributes.Length - 1]).Title;
                    } else {
                        applicationName = assembly.GetName().Name;
                    }
                }
                applicationName = GetPrintableUSAscii(applicationName, 48);
            }
            if (!Helper.IsPrintUsAscii(applicationName, 48)) { throw new System.ArgumentOutOfRangeException("applicationName", Resources.ExceptionArgumentMustContainOnlyPrintableUsAsciiCharactersWithLengthOfNoMoreThan48Characters); }

            if (processId == null) {
                processId = Process.GetCurrentProcess().Id.ToString(CultureInfo.InvariantCulture);
                if (processId.Length > 128) { processId = processId.Remove(128); }
            }
            if (!Helper.IsPrintUsAscii(processId, 128)) { throw new System.ArgumentOutOfRangeException("processId", Resources.ExceptionArgumentMustContainOnlyPrintableUsAsciiCharactersWithLengthOfNoMoreThan128Characters); }

            if (messageId == null) { messageId = string.Empty; }
            if (!Helper.IsPrintUsAscii(messageId, 32)) { throw new System.ArgumentOutOfRangeException("messageId", Resources.ExceptionArgumentMustContainOnlyPrintableUsAsciiCharactersWithLengthOfNoMoreThan32Characters); }

            if (message == null) { message = string.Empty; }


            _facility = facility;
            _severity = severity;
            _timestamp = timestamp;
            _hostName = hostName;
            _applicationName = applicationName;
            _processId = processId;
            _messageId = messageId;
            _message = message;

            if (includeOrigin) {
                var sd = new SyslogStructuredData("origin");
                var ips = Dns.GetHostAddresses(Dns.GetHostName());
                for (var i = 0; i < ips.Length; i++) {
                    sd.AddParameter("ip", ips[i].ToString());
                }
                sd.AddParameter("software", Assembly.GetEntryAssembly().GetName().Name);
                sd.AddParameter("swVersion", Assembly.GetEntryAssembly().GetName().Version.ToString());
                AddStructuredData(sd);
            }
        }

        private static string GetPrintableUSAscii(string text, int maximumLength) {
            var sb = new StringBuilder();
            var normText = text.Normalize(NormalizationForm.FormD).Replace("Đ", "D").Replace("đ", "d");
            foreach (var c in normText) {
                if (char.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark) {
                    var code = (int)c;
                    if (code < 32) { //ignore controlcharacters
                    } else if ((int)c == 32) { //replace space with underscore
                        sb.Append('_');
                    } else if ((int)c > 126) { //replace all characters above 126 with underscore
                        sb.Append('_');
                    } else { //these are ASCII characters
                        sb.Append(c);
                    }
                }
                if (sb.Length == maximumLength) { break; }
            }
            return sb.ToString();
        }


        private readonly System.Collections.Generic.List<SyslogStructuredData> _structuredDatas = new System.Collections.Generic.List<SyslogStructuredData>();
        /// <summary>
        /// Adds new Structured Data section or merges it with already existing one.
        /// </summary>
        /// <param name="structuredData">Structured Data section.</param>
        /// <exception cref="System.InvalidOperationException">At least one parameter must exist.</exception>
        public void AddStructuredData(SyslogStructuredData structuredData) {
            if (structuredData == null) { throw new System.ArgumentNullException("structuredData", "Structured data cannot be null."); }
            if (structuredData.Count == 0) { throw new System.InvalidOperationException(Resources.ExceptionAtLeastOneParameterMustExist); }
            for (var i = 0; i < _structuredDatas.Count; i++) {
                if (_structuredDatas[i].Id == structuredData.Id) {
                    for (var j = 0; j < structuredData.Count; j++) {
                        _structuredDatas[i].AddParameter(structuredData.GetName(j), structuredData.GetValue(j));
                    }
                    return;
                }
            }

            _structuredDatas.Add(structuredData);
        }

        /// <summary>
        /// Adds single parametar to existing StructuredData section or creates new one if one doesn't exist.
        /// </summary>
        /// <param name="ianaName">Id assigned by IANA.</param>
        /// <param name="parameterName">Parametar name.</param>
        /// <param name="parameterValue">Parametar value.</param>
        public void AddStructuredData(string ianaName, string parameterName, string parameterValue) {
            var sd = new SyslogStructuredData(ianaName);
            for (var i = 0; i < _structuredDatas.Count; i++) {
                if (_structuredDatas[i].Id == sd.Id) {
                    _structuredDatas[i].AddParameter(parameterName, parameterValue);
                    return;
                }
            }
            sd.AddParameter(parameterName, parameterValue);
            _structuredDatas.Add(sd);
        }

        /// <summary>
        /// Adds single parametar to existing StructuredData section or creates new one if one doesn't exist.
        /// </summary>
        /// <param name="name">Name of instance.</param>
        /// <param name="enterpriseId">Enterprise Id as specified in http://www.iana.org/assignments/enterprise-numbers.</param>
        /// <param name="parameterName">Parametar name.</param>
        /// <param name="parameterValue">Parametar value.</param>
        public void AddStructuredData(string name, int enterpriseId, string parameterName, string parameterValue) {
            var sd = new SyslogStructuredData(name, enterpriseId);
            for (var i = 0; i < _structuredDatas.Count; i++) {
                if (_structuredDatas[i].Id == sd.Id) {
                    _structuredDatas[i].AddParameter(parameterName, parameterValue);
                    return;
                }
            }
            sd.AddParameter(parameterName, parameterValue);
            _structuredDatas.Add(sd);
        }


        /// <summary>
        /// Returns single parametar from StructuredData section or null if one is not found.
        /// </summary>
        /// <param name="ianaName">Id assigned by IANA.</param>
        /// <param name="parameterName">Parametar name.</param>
        public string GetStructuredData(string ianaName, string parameterName) {
            var sd = new SyslogStructuredData(ianaName);
            for (var i = 0; i < _structuredDatas.Count; i++) {
                if (_structuredDatas[i].Id == sd.Id) {
                    for (var j = 0; j < _structuredDatas[i].Count; j++) {
                        if (string.Compare(parameterName, _structuredDatas[i].GetName(j), System.StringComparison.Ordinal) == 0) {
                            return _structuredDatas[i].GetValue(j);
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Returns single parametar from existing StructuredData section or null if one is not found.
        /// </summary>
        /// <param name="name">Name of instance.</param>
        /// <param name="enterpriseId">Enterprise ID as specified in http://www.iana.org/assignments/enterprise-numbers.</param>
        /// <param name="parameterName">Parametar name.</param>
        public string GetStructuredData(string name, int enterpriseId, string parameterName) {
            var sd = new SyslogStructuredData(name, enterpriseId);
            for (var i = 0; i < _structuredDatas.Count; i++) {
                if (_structuredDatas[i].Id == sd.Id) {
                    for (var j = 0; j < _structuredDatas[i].Count; j++) {
                        if (string.Compare(parameterName, _structuredDatas[i].GetName(j), System.StringComparison.Ordinal) == 0) {
                            return _structuredDatas[i].GetValue(j);
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Returns array with all structured data fields.
        /// </summary>
        public SyslogStructuredData[] GetStructuredDataArray() {
            return _structuredDatas.ToArray();
        }


        private SyslogFacilityCode _facility;
        /// <summary>
        /// Gets/sets facility designation.
        /// </summary>
        public SyslogFacilityCode Facility {
            get { return _facility; }
            set { _facility = value; }
        }

         private SyslogSeverityCode _severity;
        /// <summary>
        /// Gets/sets severity designation.
        /// </summary>
        public SyslogSeverityCode Severity {
            get { return _severity; }
            set { _severity = value; }
        }

        private System.DateTime _timestamp;
        /// <summary>
        /// Gets/sets timestamp.
        /// </summary>
        public System.DateTime Timestamp {
            get { return _timestamp; }
            set { _timestamp = value; }
        }

        private string _hostName;
        /// <summary>
        /// Gets/sets host name or null if it is unknown.
        /// </summary>
        public string HostName {
            get { return _hostName; }
            set {
                if (!Helper.IsPrintUsAscii(value, 255)) { throw new System.ArgumentOutOfRangeException("value", Resources.ExceptionArgumentMustContainOnlyPrintableUsAsciiCharactersWithLengthOfNoMoreThan255Characters); }
                _hostName = value;
            }
        }

        private string _applicationName;
        /// <summary>
        /// Gets application name or null if it is unknown.
        /// </summary>
        public string ApplicationName {
            get { return _applicationName; }
            set {
                if (!Helper.IsPrintUsAscii(value, 48)) { throw new System.ArgumentOutOfRangeException("value", Resources.ExceptionArgumentMustContainOnlyPrintableUsAsciiCharactersWithLengthOfNoMoreThan48Characters); }
                _applicationName = value;
            }
        }

        private string _processId;
        /// <summary>
        /// Gets/sets process Id or null if it is unknown.
        /// </summary>
        public string ProcessId {
            get { return _processId; }
            set {
                if (!Helper.IsPrintUsAscii(value, 128)) { throw new System.ArgumentOutOfRangeException("value", Resources.ExceptionArgumentMustContainOnlyPrintableUsAsciiCharactersWithLengthOfNoMoreThan128Characters); }
                _processId = value;
            }
        }

        private string _messageId;
        /// <summary>
        /// Gets/sets message Id or null if it is unknown.
        /// </summary>
        /// <exception cref="System.ArgumentOutOfRangeException">Argument must contain only printable US ASCII characters.</exception>
        public string MessageId {
            get { return _messageId; }
            set {
                if (!Helper.IsPrintUsAscii(value, 32)) { throw new System.ArgumentOutOfRangeException("value", Resources.ExceptionArgumentMustContainOnlyPrintableUsAsciiCharactersWithLengthOfNoMoreThan32Characters); }
                _messageId = value;
            }
        }

        private string _message;
        /// <summary>
        /// Gets/sets message.
        /// </summary>
        public string Message {
            get { return _message; }
            set { _message = value; }
        }


        #region Overrides

        /// <summary>
        /// Returns string representation of this class.
        /// </summary>
        public override string ToString() {
            return System.Text.UTF8Encoding.UTF8.GetString(ToByteArray());
        }

        /// <summary>
        /// Returns string representation of this class.
        /// </summary>
        public string ToBsdString() {
            return System.Text.UTF8Encoding.UTF8.GetString(ToBsdByteArray());
        }

        #endregion


        /// <summary>
        /// Returns byte array representation of this class.
        /// </summary>
        public byte[] ToByteArray() {
            var sb = new System.Collections.Generic.List<byte>();

            sb.AddRange(Helper.ToAscii("<"));
            sb.AddRange(Helper.ToAscii(((int)_facility * 8 + (int)_severity).ToString(System.Globalization.CultureInfo.InvariantCulture)));
            sb.AddRange(Helper.ToAscii(">"));
            sb.AddRange(Helper.ToAscii("1"));
            sb.AddRange(Helper.ToAscii(" "));
            sb.AddRange(Helper.ToAscii(_timestamp.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'ffffff'Z'", System.Globalization.CultureInfo.InvariantCulture)));
            sb.AddRange(Helper.ToAscii(" "));
            if (!string.IsNullOrEmpty(_hostName)) { sb.AddRange(Helper.ToAscii(_hostName)); } else { sb.AddRange(Helper.ToAscii("-")); }
            sb.AddRange(Helper.ToAscii(" "));
            if (!string.IsNullOrEmpty(_applicationName)) { sb.AddRange(Helper.ToAscii(_applicationName)); } else { sb.AddRange(Helper.ToAscii("-")); }
            sb.AddRange(Helper.ToAscii(" "));
            if (!string.IsNullOrEmpty(_processId)) { sb.AddRange(Helper.ToAscii(_processId)); } else { sb.AddRange(Helper.ToAscii("-")); }
            sb.AddRange(Helper.ToAscii(" "));
            if (!string.IsNullOrEmpty(_messageId)) { sb.AddRange(Helper.ToAscii(_messageId)); } else { sb.AddRange(Helper.ToAscii("-")); }
            sb.AddRange(Helper.ToAscii(" "));
            if (_structuredDatas.Count > 0) {
                for (var i = 0; i < _structuredDatas.Count; i++) {
                    sb.AddRange(_structuredDatas[i].ToByteArray());
                }
            } else {
                sb.AddRange(Helper.ToAscii("-"));
            }
            if (!string.IsNullOrEmpty(_message)) {
                sb.AddRange(Helper.ToAscii(" "));
                if (Helper.IsAllAscii(_message)) {
                    sb.AddRange(Helper.ToAscii(_message));
                } else {
                    sb.AddRange(new byte[] { 0xEF, 0xBB, 0xBF });
                    sb.AddRange(Helper.ToUtf8(_message));
                }
            }

            return sb.ToArray();
        }

        /// <summary>
        /// Returns byte array representation of this class as done in BSD syslog.
        /// Not all fields will be used.
        /// </summary>
        public byte[] ToBsdByteArray() {
            var sb = new System.Collections.Generic.List<byte>();
            sb.AddRange(Helper.ToAscii("<"));
            sb.AddRange(Helper.ToAscii(((int)_facility * 8 + (int)_severity).ToString(System.Globalization.CultureInfo.InvariantCulture)));
            sb.AddRange(Helper.ToAscii(">"));
            sb.AddRange(Helper.ToAscii(_timestamp.ToString("MMM", System.Globalization.CultureInfo.InvariantCulture)));
            sb.Add(32);
            sb.AddRange(Helper.ToAscii(_timestamp.Day.ToString(System.Globalization.CultureInfo.InvariantCulture).PadLeft(2)));
            sb.Add(32);
            sb.AddRange(Helper.ToAscii(_timestamp.ToString("HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture)));
            sb.Add(32);
            sb.AddRange(Helper.ToAscii(_hostName));
            sb.Add(32);
            if (!string.IsNullOrEmpty(_applicationName)) {
                sb.AddRange(Helper.ToAscii(_applicationName));
                if (!string.IsNullOrEmpty(_processId)) {
                    sb.AddRange(Helper.ToAscii("[" + _processId + "]"));
                }
                sb.AddRange(Helper.ToAscii(":"));
                sb.Add(32);
            }
            sb.AddRange(Helper.ToAscii(_message));
            return sb.ToArray();
        }



        /// <summary>
        /// Returns valid Syslog message from byte array. If message is invalid, it returns only Message property.
        /// </summary>
        /// <param name="messageBuffer">Message to parse.</param>
        public static SyslogMessage Parse(byte[] messageBuffer) {
            return Parse(messageBuffer, null);
        }

        /// <summary>
        /// Returns valid Syslog message from byte array. If message is invalid, it returns only Message property.
        /// </summary>
        /// <param name="messageBuffer">Message to parse.</param>
        /// <param name="endpoint">The remote System.Net.IPEndPoint that specifies the remote host.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public static SyslogMessage Parse(byte[] messageBuffer, System.Net.IPEndPoint endpoint) {
            if (messageBuffer == null) { throw new System.ArgumentNullException("messageBuffer", "Message buffer cannot be null."); }
            try {
                //Version check
                var priR = Helper.FindByteValue(messageBuffer, 0, 32);
                if ((messageBuffer[0] != 60) || (messageBuffer[priR - 2] != 62) || (messageBuffer[priR - 1] != 49)) { //this is not valid message - maybe older protocol?
                    throw new System.FormatException(Resources.ExceptionMessageFormatNotRecognized);
                }


                var ret = new SyslogMessage();


                //Facility/Severity
                var priVal = int.Parse(Helper.FromAscii(messageBuffer, 1, priR - 3), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture);
                if ((priVal < 0) || (priVal > 191)) { throw new System.FormatException(Resources.ExceptionPriMustBe0To191); }
                ret._facility = (SyslogFacilityCode)(priVal / 8);
                ret._severity = (SyslogSeverityCode)(priVal % 8);

                //Timestamp
                var timeL = priR + 1;
                var timeR = Helper.FindByteValue(messageBuffer, timeL, 32);
                var timeValue = Helper.FromAscii(messageBuffer, timeL, timeR - timeL);
                ret._timestamp = System.DateTime.Parse(timeValue, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);

                //Host name
                var hostL = timeR + 1;
                var hostR = Helper.FindByteValue(messageBuffer, hostL, 32);
                var hostValue = Helper.FromAscii(messageBuffer, hostL, hostR - hostL);
                if (hostValue == "-") {
                    ret._hostName = string.Empty;
                } else {
                    ret._hostName = hostValue;
                }

                //Application name
                var appL = hostR + 1;
                var appR = Helper.FindByteValue(messageBuffer, appL, 32);
                var appValue = Helper.FromAscii(messageBuffer, appL, appR - appL);
                if (appValue == "-") {
                    ret._applicationName = string.Empty;
                } else {
                    ret._applicationName = appValue;
                }

                //Process id
                var procL = appR + 1;
                var procR = Helper.FindByteValue(messageBuffer, procL, 32);
                var procValue = Helper.FromAscii(messageBuffer, procL, procR - procL);
                if (procValue == "-") {
                    ret._processId = string.Empty;
                } else {
                    ret._processId = procValue;
                }

                //Message ID
                var midL = procR + 1;
                var midR = Helper.FindByteValue(messageBuffer, midL, 32);
                var midValue = Helper.FromAscii(messageBuffer, midL, midR - midL);
                if (midValue == "-") {
                    ret._messageId = string.Empty;
                } else {
                    ret._messageId = midValue;
                }

                //SturucturedData
                var sdI = midR + 1;
                if (messageBuffer[sdI] != 45) { //noorigin ("-")

                    var state = Helper.SDParseStates.Default;

                    var content = new System.Collections.Generic.List<byte>();
                    string paramID = null;
                    string paramName = null;
                    string paramValue = null;
                    while (true) {
                        switch (messageBuffer[sdI]) {
                            case 32: { // " "
                                    switch (state) {
                                        case Helper.SDParseStates.ParamValueContent: {
                                                content.Add(messageBuffer[sdI]);
                                                break;
                                            }

                                        case Helper.SDParseStates.ParamID: {
                                                paramID = Helper.FromAscii(content.ToArray());
                                                content.Clear();
                                                state = Helper.SDParseStates.ParamName;
                                                break;
                                            }

                                        case Helper.SDParseStates.ParamValue: {
                                                state = Helper.SDParseStates.ParamName;
                                                break;
                                            }

                                        case Helper.SDParseStates.Default: {
                                                goto SDOver;
                                            }

                                        default: { throw new System.FormatException(Resources.ExceptionUnexpectedState); }

                                    }
                                }
                                break;

                            case 61: { // "="
                                    switch (state) {
                                        case Helper.SDParseStates.ParamName: {
                                                paramName = Helper.FromAscii(content.ToArray());
                                                content.Clear();
                                                state = Helper.SDParseStates.ParamValue;
                                                break;
                                            }

                                        default: { throw new System.FormatException(Resources.ExceptionUnexpectedState); }
                                    }
                                }
                                break;

                            case 34: { // "
                                    switch (state) {
                                        case Helper.SDParseStates.ParamValue: {
                                                state = Helper.SDParseStates.ParamValueContent;
                                                break;
                                            }

                                        case Helper.SDParseStates.ParamValueContent: {
                                                paramValue = Helper.FromUtf8(content.ToArray());

                                                ret.AddStructuredData(paramID, paramName, paramValue);
                                                paramName = null;
                                                paramValue = null;

                                                content.Clear();
                                                state = Helper.SDParseStates.ParamValue;
                                                break;
                                            }

                                        case Helper.SDParseStates.ParamValueContentEscaped: {
                                                content.Add(messageBuffer[sdI]);
                                                state = Helper.SDParseStates.ParamValueContent;
                                                break;
                                            }

                                        default: { throw new System.FormatException(Resources.ExceptionUnexpectedState); }
                                    }
                                }
                                break;

                            case 91: { // "["
                                    switch (state) {
                                        default: { throw new System.FormatException(Resources.ExceptionUnexpectedState); }

                                        case Helper.SDParseStates.ParamValueContent: {
                                                content.Add(messageBuffer[sdI]);
                                                break;
                                            }
                                        case Helper.SDParseStates.Default: {
                                                state = Helper.SDParseStates.ParamID;
                                                break;
                                            }
                                    }
                                }
                                break;

                            case 93: { // "]"
                                    switch (state) {

                                        case Helper.SDParseStates.ParamValue: {
                                                state = Helper.SDParseStates.Default;
                                                break;
                                            }

                                        case Helper.SDParseStates.ParamValueContentEscaped: {
                                                content.Add(messageBuffer[sdI]);
                                                state = Helper.SDParseStates.ParamValueContent;
                                                break;
                                            }

                                        default: { throw new System.FormatException(Resources.ExceptionUnexpectedState); }
                                    }
                                }
                                break;

                            case 92: { // "\"
                                    switch (state) {

                                        case Helper.SDParseStates.ParamValueContent: {
                                                state = Helper.SDParseStates.ParamValueContentEscaped;
                                                break;
                                            }

                                        case Helper.SDParseStates.ParamValueContentEscaped: {
                                                content.Add(messageBuffer[sdI]);
                                                state = Helper.SDParseStates.ParamValueContent;
                                                break;
                                            }

                                        default: { throw new System.FormatException(Resources.ExceptionUnexpectedState); }
                                    }
                                    break;
                                }

                            default: {
                                    if (state == Helper.SDParseStates.Default) { throw new System.FormatException(Resources.ExceptionUnexpectedState); }
                                    content.Add(messageBuffer[sdI]);
                                }
                                break;

                        }
                        sdI += 1;

                        if (sdI >= messageBuffer.Length) {
                            switch (state) {
                                case Helper.SDParseStates.Default: {
                                        goto SDOver;
                                    }
                                default: { throw new System.FormatException(Resources.ExceptionUnexpectedState); }
                            }
                        }//if
                    } //while
                    //do it as state machine



                } else {
                    sdI += 1;
                }

            SDOver:

                if (sdI < messageBuffer.Length) {

                    if (messageBuffer[sdI] != 32) { throw new System.FormatException(Resources.ExceptionSpaceExpected); }

                    if ((messageBuffer[sdI + 1] == 0xEF) && (messageBuffer[sdI + 2] == 0xBB) && (messageBuffer[sdI + 3] == 0xBF)) { //UTF8 BOF
                        var messL = sdI + 4;
                        var messR = messageBuffer.Length;
                        ret._message = Helper.FromUtf8(messageBuffer, messL, messR - messL);
                    } else { //ASCII
                        var messL = sdI + 1;
                        var messR = messageBuffer.Length;
                        ret._message = Helper.FromAscii(messageBuffer, messL, messR - messL);
                    }

                } else {

                    ret._message = string.Empty;

                }

                return ret;

            } catch (System.FormatException) {
                return ParseBsd(messageBuffer, endpoint);
            }
        }


        /// <summary>
        /// Returns syslog message from byte array using BSD conventions.
        /// </summary>
        /// <param name="messageBuffer">Message to parse.</param>
        public static SyslogMessage ParseBsd(byte[] messageBuffer) {
            return ParseBsd(messageBuffer, null);
        }

        /// <summary>
        /// Returns syslog message from byte array using BSD conventions.
        /// </summary>
        /// <param name="messageBuffer">Message to parse.</param>
        /// <param name="endpoint">The remote System.Net.IPEndPoint that specifies the remote host.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public static SyslogMessage ParseBsd(byte[] messageBuffer, System.Net.IPEndPoint endpoint) {
            if (messageBuffer == null) { return null; }

            var ret = new SyslogMessage {
                _processId = string.Empty,
                _applicationName = string.Empty
            };

            var rawMessage = System.Text.ASCIIEncoding.ASCII.GetString(messageBuffer, 0, messageBuffer.Length);
            var priL = rawMessage.IndexOf("<", System.StringComparison.Ordinal);
            var priR = rawMessage.IndexOf(">", System.StringComparison.Ordinal);
            if ((priL == 0) && (priR > priL)) { //Valid PRI?
                var priStr = rawMessage.Substring(priL + 1, priR - priL - 1);
                if (int.TryParse(priStr, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var priInt) == true) {
                    if ((priInt >= 0) && (priInt <= 191)) { //Valid PRI
                        var priFacility = priInt / 8;
                        var priSeverity = priInt % 8;

                        ret._facility = (SyslogFacilityCode)priFacility;
                        ret._severity = (SyslogSeverityCode)priSeverity;

                        if (messageBuffer.Length > (priR + 15)) { //Check Timestamp
                            var timestampStr = rawMessage.Substring(priR + 1, 15);
                            if (!System.DateTime.TryParseExact(timestampStr, "MMM dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeLocal, out var timestamp)) {
                                timestamp = System.DateTime.MinValue;
                            }

                            if (timestamp > System.DateTime.MinValue) { //Valid TIMESTAMP
                                ret._timestamp = timestamp;
                                var hostL = rawMessage.IndexOf(" ", priR + 15, System.StringComparison.Ordinal);
                                var hostR = rawMessage.IndexOf(" ", hostL + 1, System.StringComparison.Ordinal);

                                if ((hostL > 0) && (hostR > hostL)) { //Found host
                                    ret._hostName = rawMessage.Substring(hostL + 1, hostR - hostL - 1);

                                    ret._message = null;

                                    var tagL = hostR + 1;
                                    var tagR = rawMessage.IndexOf(": ", hostR, System.StringComparison.Ordinal);
                                    if ((tagR > -1) && (tagR - tagL < 32)) {
                                        var tagStr = rawMessage.Substring(tagL, tagR - tagL);
                                        var numLB = 0;
                                        var numRB = 0;
                                        var noAscii = false;
                                        for (var i = 0; i < tagStr.Length; i++) {
                                            switch (tagStr[i]) {
                                                case '[':
                                                    numLB += 1;
                                                    break;
                                                case ']':
                                                    numRB += 1;
                                                    break;
                                                default:
                                                    var charVal = System.Convert.ToInt32(tagStr[i]);
                                                    if ((charVal >= 48) && (charVal <= 57)) { // 0-9
                                                    } else if ((charVal >= 65) && (charVal <= 90)) { // A-Z
                                                    } else if ((charVal >= 97) && (charVal <= 122)) { // a-z
                                                    } else {
                                                        noAscii = true;
                                                    }
                                                    break;
                                            }
                                            if (noAscii) { break; }
                                        }

                                        if ((noAscii) || (numLB > 1) || (numRB > 1) || (numLB != numRB)) { //no ASCII or brackets are wrong.
                                        } else {
                                            var pidL = tagStr.IndexOf('[', 0);
                                            var pidR = tagStr.IndexOf(']', pidL + 1);
                                            if ((pidL > -1) && (pidR > -1) && (pidR == tagStr.Length - 1)) { //both brackets are there - pid.
                                                ret._applicationName = tagStr.Substring(0, pidL);
                                                ret._processId = tagStr.Substring(pidL + 1, pidR - pidL - 1);
                                                ret._message = rawMessage.Substring(tagR + 2);
                                            } else if ((pidL == -1) && (pidR == -1)) { //no brackets are there.
                                                ret._applicationName = tagStr;
                                                ret._processId = string.Empty;
                                                ret._message = rawMessage.Substring(tagR + 2);
                                            }
                                        }//if
                                    }
                                    if (ret._message == null) { ret._message = rawMessage.Substring(hostR + 1); }
                                    return ret; //valid message
                                }

                                //Invalid host - not specified in RFC 3164
                                if (endpoint != null) {
                                    ret._hostName = endpoint.Address.ToString();
                                } else {
                                    ret._hostName = string.Empty;
                                }
                                ret._message = rawMessage.Substring(priR + 1); //Just drop PRI part
                                //ret._isRawMessageOk = false;
                                return ret; //not quite valid message

                            } //if
                        } //if

                        //Valid PRI, Invalid Timestamp
                        ret._timestamp = System.DateTime.Now;
                        if (endpoint != null) {
                            ret._hostName = endpoint.Address.ToString();
                        } else {
                            ret._hostName = string.Empty;
                        }
                        ret._message = rawMessage.Substring(priR + 1);
                        return ret; //not quite valid message
                    } //if //Valid PRI
                }
            } //if ((priL == 0) && (priR > priL))

            //No PRI
            ret._facility = SyslogFacilityCode.UserLevel;
            ret._severity = SyslogSeverityCode.Notice;
            ret._timestamp = System.DateTime.Now;
            if (endpoint != null) {
                ret._hostName = endpoint.Address.ToString();
            } else {
                ret._hostName = string.Empty;
            }
            ret._message = rawMessage;

            return ret; //not quite valid message
        }


        #region UDP

        /// <summary>
        /// Sends message to the specified endpoint.
        /// </summary>
        /// <param name="remoteEP">The System.Net.EndPoint that represents the destination location for the data.</param>
        public void Send(System.Net.IPEndPoint remoteEP) {
            Send(this, remoteEP);
        }

        /// <summary>
        /// Sends message to the specified address on port 514.
        /// </summary>
        /// <param name="address">Address to which data is to be sent.</param>
        public void Send(System.Net.IPAddress address) {
            Send(this, address);
        }

        /// <summary>
        /// Sends message to the specified address.
        /// </summary>
        /// <param name="address">Address on which data is to be sent.</param>
        /// <param name="port">Port to which data is to be sent.</param>
        public void Send(System.Net.IPAddress address, int port) {
            Send(this, address, port);
        }

        /// <summary>
        /// Sends the specified message to the specified address on port 514.
        /// </summary>
        /// <param name="message">Syslog message to send.</param>
        /// <param name="address">Address to which data is to be sent.</param>
        public static void Send(SyslogMessage message, System.Net.IPAddress address) {
            Send(message, address, SyslogMessage.DefaultPort);
        }

        /// <summary>
        /// Sends the specified message to the specified address.
        /// </summary>
        /// <param name="message">Syslog message to send.</param>
        /// <param name="address">Address to which data is to be sent.</param>
        /// <param name="port">Port to which data is to be sent.</param>
        public static void Send(SyslogMessage message, System.Net.IPAddress address, int port) {
            Send(message, new System.Net.IPEndPoint(address, port));
        }

        /// <summary>
        /// Sends the specified message to the specified endpoint.
        /// </summary>
        /// <param name="message">Syslog message to send.</param>
        /// <param name="remoteEP">The System.Net.EndPoint that represents the destination location for the data.</param>
        public static void Send(SyslogMessage message, System.Net.IPEndPoint remoteEP) {
            if (message == null) { return; }
            if (remoteEP == null) { remoteEP = new System.Net.IPEndPoint(System.Net.IPAddress.Broadcast, SyslogMessage.DefaultPort); }
            using (var socket = new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Dgram, System.Net.Sockets.ProtocolType.Udp)) {
                if (remoteEP.Address == System.Net.IPAddress.Broadcast) {
                    socket.SetSocketOption(System.Net.Sockets.SocketOptionLevel.Socket, System.Net.Sockets.SocketOptionName.Broadcast, 1);
                }
                socket.SendTo(message.ToByteArray(), remoteEP);
            }
        }

        #endregion


        private static class Helper {

            internal static byte[] ToUtf8(string value) {
                return System.Text.UTF8Encoding.UTF8.GetBytes(value);
            }

            internal static bool IsAllAscii(string value) {
                if (value == null) { return true; }
                for (var i = 0; i < value.Length; i++) {
                    var charValue = System.Convert.ToInt32(value[i]);
                    if (!((charValue >= 32) && (charValue <= 126))) { return false; }
                }
                return true;
            }

            internal static bool IsPRINTUSASCII(string value) {
                if (value == null) { return true; }
                for (var i = 0; i < value.Length; i++) {
                    var charValue = System.Convert.ToInt32(value[i]);
                    if (!((charValue >= 33) && (charValue <= 126))) { return false; }
                }
                return true;
            }

            internal static bool IsPrintUsAscii(string value, int length) {
                if (value == null) { return true; }
                if (value.Length > length) { return false; }
                return IsPRINTUSASCII(value);
            }

            internal static byte[] ToAscii(string value) {
                return System.Text.ASCIIEncoding.ASCII.GetBytes(value);
            }

            internal static string FromAscii(byte[] value) {
                return System.Text.ASCIIEncoding.ASCII.GetString(value, 0, value.Length);
            }

            internal static string FromAscii(byte[] value, int index, int count) {
                return System.Text.ASCIIEncoding.ASCII.GetString(value, index, count);
            }

            internal static string FromUtf8(byte[] value) {
                return System.Text.UTF8Encoding.UTF8.GetString(value, 0, value.Length);
            }

            internal static string FromUtf8(byte[] value, int index, int count) {
                return System.Text.UTF8Encoding.UTF8.GetString(value, index, count);
            }

            internal static int FindByteValue(byte[] value, int startingIndex, byte valueToFind) {
                for (var i = startingIndex; i < value.Length; i++) {
                    if (value[i] == valueToFind) { return i; }
                }
                return -1;
            }

            internal enum SDParseStates {
                Default,   // starting state
                ParamID,
                ParamName,
                ParamValue,
                ParamValueContent,
                ParamValueContentEscaped,
            }


        }

        private static class Resources {

            internal static string ExceptionArgumentMustContainOnlyPrintableUsAsciiCharactersWithLengthOfNoMoreThan32Characters { get { return "Argument must contain only printable US ASCII characters with length of no more than 32 characters."; } }
            internal static string ExceptionArgumentMustContainOnlyPrintableUsAsciiCharactersWithLengthOfNoMoreThan48Characters { get { return "Argument must contain only printable US ASCII characters with length of no more than 48 characters."; } }
            internal static string ExceptionArgumentMustContainOnlyPrintableUsAsciiCharactersWithLengthOfNoMoreThan128Characters { get { return "Argument must contain only printable US ASCII characters with length of no more than 128 characters."; } }
            internal static string ExceptionArgumentMustContainOnlyPrintableUsAsciiCharactersWithLengthOfNoMoreThan255Characters { get { return "Argument must contain only printable US ASCII characters with length of no more than 255 characters."; } }
            internal static string ExceptionAtLeastOneParameterMustExist { get { return "At least one parameter must exist."; } }
            internal static string ExceptionMessageFormatNotRecognized { get { return "Message format not recognized."; } }
            internal static string ExceptionPriMustBe0To191 { get { return "PRI must be 0-191."; } }
            internal static string ExceptionSpaceExpected { get { return "Space expected."; } }
            internal static string ExceptionUnexpectedState { get { return "Unexpected state."; } }

        }

    }



    /// <summary>
    /// STRUCTURED-DATA member of syslog messageBuffer.
    /// </summary>
    public class SyslogStructuredData {

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="ianaName">Id assigned by IANA.</param>
        /// <exception cref="System.ArgumentNullException">Argument cannot be null or empty.</exception>
        public SyslogStructuredData(string ianaName) {
            if (string.IsNullOrEmpty(ianaName)) { throw new System.ArgumentNullException("ianaName", Resources.ExceptionArgumentCannotBeNullOrEmpty); }
            _id = ianaName;
        }

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="ianaName">Id assigned by IANA.</param>
        /// <param name="parameterName1">Parameter name.</param>
        /// <param name="parameterValue1">Parameter value.</param>
        /// <exception cref="System.ArgumentNullException">Argument cannot be null or empty.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Argument must contain only printable US ASCII characters.</exception>
        public SyslogStructuredData(string ianaName, string parameterName1, string parameterValue1)
            : this(ianaName) {
            AddParameter(parameterName1, parameterValue1);
        }

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="ianaName">Id assigned by IANA.</param>
        /// <param name="parameterName1">Parameter name.</param>
        /// <param name="parameterValue1">Parameter value.</param>
        /// <param name="parameterName2">Parameter name.</param>
        /// <param name="parameterValue2">Parameter value.</param>
        /// <exception cref="System.ArgumentNullException">Argument cannot be null or empty.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Argument must contain only printable US ASCII characters.</exception>
        public SyslogStructuredData(string ianaName, string parameterName1, string parameterValue1, string parameterName2, string parameterValue2)
            : this(ianaName) {
            AddParameter(parameterName1, parameterValue1);
            AddParameter(parameterName2, parameterValue2);
        }

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="ianaName">Id assigned by IANA.</param>
        /// <param name="parameterName1">Parameter name.</param>
        /// <param name="parameterValue1">Parameter value.</param>
        /// <param name="parameterName2">Parameter name.</param>
        /// <param name="parameterValue2">Parameter value.</param>
        /// <param name="parameterName3">Parameter name.</param>
        /// <param name="parameterValue3">Parameter value.</param>
        /// <exception cref="System.ArgumentNullException">Argument cannot be null or empty.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Argument must contain only printable US ASCII characters.</exception>
        public SyslogStructuredData(string ianaName, string parameterName1, string parameterValue1, string parameterName2, string parameterValue2, string parameterName3, string parameterValue3)
            : this(ianaName) {
            AddParameter(parameterName1, parameterValue1);
            AddParameter(parameterName2, parameterValue2);
            AddParameter(parameterName3, parameterValue3);
        }


        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="name">Name of instance.</param>
        /// <param name="enterpriseId">Enterprise Id as specified in http://www.iana.org/assignments/enterprise-numbers.</param>
        /// <exception cref="System.ArgumentNullException">Argument cannot be null or empty.</exception>
        public SyslogStructuredData(string name, int enterpriseId) {
            if (string.IsNullOrEmpty(name)) { throw new System.ArgumentNullException("name", Resources.ExceptionArgumentCannotBeNullOrEmpty); }
            _id = name + "@" + enterpriseId.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="name">Name of instance.</param>
        /// <param name="enterpriseId">Enterprise Id as specified in http://www.iana.org/assignments/enterprise-numbers.</param>
        /// <param name="parameterName1">Parameter name.</param>
        /// <param name="parameterValue1">Parameter value.</param>
        /// <exception cref="System.ArgumentNullException">Argument cannot be null or empty.</exception>
        public SyslogStructuredData(string name, int enterpriseId, string parameterName1, string parameterValue1)
            : this(name, enterpriseId) {
            AddParameter(parameterName1, parameterValue1);
        }

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="name">Name of instance.</param>
        /// <param name="enterpriseId">Enterprise Id as specified in http://www.iana.org/assignments/enterprise-numbers.</param>
        /// <param name="parameterName1">Parameter name.</param>
        /// <param name="parameterValue1">Parameter value.</param>
        /// <param name="parameterName2">Parameter name.</param>
        /// <param name="parameterValue2">Parameter value.</param>
        /// <exception cref="System.ArgumentNullException">Argument cannot be null or empty.</exception>
        public SyslogStructuredData(string name, int enterpriseId, string parameterName1, string parameterValue1, string parameterName2, string parameterValue2)
            : this(name, enterpriseId) {
            AddParameter(parameterName1, parameterValue1);
            AddParameter(parameterName2, parameterValue2);
        }

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="name">Name of instance.</param>
        /// <param name="enterpriseId">Enterprise Id as specified in http://www.iana.org/assignments/enterprise-numbers.</param>
        /// <param name="parameterName1">Parameter name.</param>
        /// <param name="parameterValue1">Parameter value.</param>
        /// <param name="parameterName2">Parameter name.</param>
        /// <param name="parameterValue2">Parameter value.</param>
        /// <param name="parameterName3">Parameter name.</param>
        /// <param name="parameterValue3">Parameter value.</param>
        /// <exception cref="System.ArgumentNullException">Argument cannot be null or empty.</exception>
        public SyslogStructuredData(string name, int enterpriseId, string parameterName1, string parameterValue1, string parameterName2, string parameterValue2, string parameterName3, string parameterValue3)
            : this(name, enterpriseId) {
            AddParameter(parameterName1, parameterValue1);
            AddParameter(parameterName2, parameterValue2);
            AddParameter(parameterName3, parameterValue3);
        }


        private readonly string _id;
        /// <summary>
        /// Gets Id of this class.
        /// </summary>
        public string Id {
            get { return _id; }
        }


        private readonly System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<string, string>> _params = new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<string, string>>();

        /// <summary>
        /// Returns name of parametar.
        /// </summary>
        /// <param name="index">Index.</param>
        public string GetName(int index) {
            return _params[index].Key;
        }

        /// <summary>
        /// Returns value of parametar.
        /// </summary>
        /// <param name="index">Index.</param>
        public string GetValue(int index) {
            return _params[index].Value;
        }

        /// <summary>
        /// Gets number of parametars.
        /// </summary>
        public int Count {
            get {
                return _params.Count;
            }
        }

        /// <summary>
        /// Adds parameter.
        /// </summary>
        /// <param name="parameterName">Name.</param>
        /// <param name="parameterValue">Value.</param>
        /// <exception cref="System.ArgumentNullException">Argument cannot be null or empty.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Argument must contain only printable US ASCII characters.</exception>
        public void AddParameter(string parameterName, string parameterValue) {
            if (string.IsNullOrEmpty(parameterName)) { throw new System.ArgumentNullException("parameterName", Resources.ExceptionArgumentCannotBeNullOrEmpty); }
            if (!Helper.IsPrintUsAscii(parameterName, 32, new char[] { '=', ' ', ']', '"' })) { throw new System.ArgumentOutOfRangeException("parameterName", Resources.ExceptionArgumentMustContainOnlyPrintableUsAsciiCharactersWithLengthOfNoMoreThan32CharactersAndNoXXXIsAllowed); }
            _params.Add(new System.Collections.Generic.KeyValuePair<string, string>(parameterName, parameterValue));
        }


        #region Overrides

        /// <summary>
        /// Returns string representation of this class.
        /// </summary>
        public override string ToString() {
            return System.Text.UTF8Encoding.UTF8.GetString(ToByteArray());
        }

        #endregion


        /// <summary>
        /// Returns byte array representation of this class.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">At least one parameter must exist.</exception>
        public byte[] ToByteArray() {
            if (_params.Count == 0) { throw new System.InvalidOperationException(Resources.ExceptionAtLeastOneParameterMustExist); }
            var sb = new System.Collections.Generic.List<byte>();

            sb.AddRange(Helper.ToAscii("["));
            sb.AddRange(Helper.ToAscii(_id));

            for (var i = 0; i < _params.Count; i++) {
                sb.AddRange(Helper.ToAscii(" "));
                sb.AddRange(Helper.ToAscii(_params[i].Key));
                sb.AddRange(Helper.ToAscii("="));
                sb.AddRange(Helper.ToAscii("\""));
                sb.AddRange(Helper.ToUtf8(_params[i].Value.Replace(@"\", @"\\").Replace("\"", "\\\"").Replace(@"]", @"\]")));
                sb.AddRange(Helper.ToAscii("\""));
            }

            sb.AddRange(Helper.ToAscii("]"));

            return sb.ToArray();
        }

        private static class Helper {

            internal static bool IsPrintUsAscii(string value, int length, char[] except) {
                if (value == null) { return true; }
                if (value.Length > length) { return false; }
                if ((except != null) && (except.Length > 0) && (value.IndexOfAny(except) > -1)) { return false; }

                for (var i = 0; i < value.Length; i++) {
                    var charValue = System.Convert.ToInt32(value[i]);
                    if (!((charValue >= 33) && (charValue <= 126))) { return false; }
                }

                return true;
            }

            internal static byte[] ToUtf8(string value) {
                return System.Text.UTF8Encoding.UTF8.GetBytes(value);
            }

            internal static byte[] ToAscii(string value) {
                return System.Text.ASCIIEncoding.ASCII.GetBytes(value);
            }


            internal enum SDParseStates {
                Default,   // starting state
                ParamID,
                ParamName,
                ParamValue,
                ParamValueContent,
                ParamValueContentEscaped,
            }


        }

        private static class Resources {

            internal static string ExceptionArgumentCannotBeNullOrEmpty { get { return "Argument cannot be null or empty."; } }
            internal static string ExceptionArgumentMustContainOnlyPrintableUsAsciiCharactersWithLengthOfNoMoreThan32CharactersAndNoXXXIsAllowed { get { return "Argument must contain only printable US ASCII characters with length of no more than 32 characters  and no '=', SP, ']', %d34 (\") is allowed."; } }
            internal static string ExceptionAtLeastOneParameterMustExist { get { return "At least one parameter must exist."; } }

        }

    }

    /// <summary>
    /// Represents severity of event.
    /// </summary>
    public enum SyslogSeverityCode {
        /// <summary>
        /// Emergency: system is unusable.
        /// </summary>
        [System.ComponentModel.Description("Emergency")]
        Emergency = 0,

        /// <summary>
        /// Alert: action must be taken immediately.
        /// </summary>
        [System.ComponentModel.Description("Alert")]
        Alert = 1,

        /// <summary>
        /// Critical: critical conditions.
        /// </summary>
        [System.ComponentModel.Description("Critical")]
        Critical = 2,

        /// <summary>
        /// Error: error conditions.
        /// </summary>
        [System.ComponentModel.Description("Error")]
        Error = 3,

        /// <summary>
        /// Warning: warning conditions.
        /// </summary>
        [System.ComponentModel.Description("Warning")]
        Warning = 4,

        /// <summary>
        /// Notice: normal but significant condition.
        /// </summary>
        [System.ComponentModel.Description("Notice")]
        Notice = 5,

        /// <summary>
        /// Informational: informational messages.
        /// </summary>
        [System.ComponentModel.Description("Informational")]
        Information = 6,

        /// <summary>
        /// Debug: debug-level messages.
        /// </summary>
        [System.ComponentModel.Description("Debug")]
        Debug = 7,
    }

    /// <summary>
    /// Facility that originated event.
    /// </summary>
    public enum SyslogFacilityCode {
        /// <summary>
        /// Kernel messages.
        /// </summary>
        [System.ComponentModel.Description("Kernel messages")]
        Kernel = 0,

        /// <summary>
        /// User-level messages.
        /// </summary>
        [System.ComponentModel.Description("User-level messages")]
        UserLevel = 1,

        /// <summary>
        /// Mail system.
        /// </summary>
        [System.ComponentModel.Description("Mail system")]
        MailSystem = 2,

        /// <summary>
        /// System deamons.
        /// </summary>
        [System.ComponentModel.Description("System deamons")]
        SystemDeamons = 3,

        /// <summary>
        /// Security/authorization messages.
        /// </summary>
        [System.ComponentModel.Description("Security/authorization messages")]
        Security = 4,

        /// <summary>
        /// Generated internally by syslogd. 
        /// </summary>
        [System.ComponentModel.Description("Generated internally by syslogd")]
        Syslogd = 5,

        /// <summary>
        /// Line printer subsystem.
        /// </summary>
        [System.ComponentModel.Description("Line printer subsystem")]
        LinePrinter = 6,

        /// <summary>
        /// Network news subsystem.
        /// </summary>
        [System.ComponentModel.Description("Network news subsystem")]
        NetworkNews = 7,

        /// <summary>
        /// UUCP subsystem.
        /// </summary>
        [System.ComponentModel.Description("UUCP subsystem")]
        UucpSubsystem = 8,

        /// <summary>
        /// Clock daemon.
        /// </summary>
        [System.ComponentModel.Description("Clock daemon")]
        ClockDaemon = 9,

        /// <summary>
        /// Security/authorization messages.
        /// </summary>
        [System.ComponentModel.Description("Security/authorization messages")]
        Authorization = 10,

        /// <summary>
        /// FTP daemon.
        /// </summary>
        [System.ComponentModel.Description("FTP daemon")]
        FtpDaemon = 11,

        /// <summary>
        /// NTP subsystem.
        /// </summary>
        [System.ComponentModel.Description("NTP subsystem")]
        NtpSubsystem = 12,

        /// <summary>
        /// Log audit.
        /// </summary>
        [System.ComponentModel.Description("Log audit")]
        LogAudit = 13,

        /// <summary>
        /// Log alert.
        /// </summary>
        [System.ComponentModel.Description("Log alert")]
        LogAlert = 14,

        /// <summary>
        /// Clock daemon.
        /// </summary>
        [System.ComponentModel.Description("Clock daemon")]
        ClockDaemon2 = 15,

        /// <summary>
        /// Local use 0.
        /// </summary>
        [System.ComponentModel.Description("Local use 0")]
        Local0 = 16,

        /// <summary>
        /// Local use 1.
        /// </summary>
        [System.ComponentModel.Description("Local use 1")]
        Local1 = 17,

        /// <summary>
        /// Local use 2.
        /// </summary>
        [System.ComponentModel.Description("Local use 2")]
        Local2 = 18,

        /// <summary>
        /// Local use 3.
        /// </summary>
        [System.ComponentModel.Description("Local use 3")]
        Local3 = 19,

        /// <summary>
        /// Local use 4.
        /// </summary>
        [System.ComponentModel.Description("Local use 4")]
        Local4 = 20,

        /// <summary>
        /// Local use 5.
        /// </summary>
        [System.ComponentModel.Description("Local use 5")]
        Local5 = 21,

        /// <summary>
        /// Local use 6.
        /// </summary>
        [System.ComponentModel.Description("Local use 6")]
        Local6 = 22,

        /// <summary>
        /// Local use 7.
        /// </summary>
        [System.ComponentModel.Description("Local use 7")]
        Local7 = 23,
    }

}
