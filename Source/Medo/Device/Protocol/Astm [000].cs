//Copyright (c) 2008 Josip Medved <jmedved@jmedved.com>

//2008-04-26: First version.


using System;
using System.Collections.Generic;
using System.Globalization;

namespace Medo.IO.Astm {

    /// <summary>
    /// Base class for ASTM records.
    /// </summary>
    public abstract class AstmRecord {

        /// <summary>
        /// Creates new instance.
        /// </summary>
        protected AstmRecord() {
            this.Parent = null;
        }

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="parent">Record which will determine delimiters.</param>
        protected AstmRecord(AstmRecord parent) {
            this.Parent = parent;
            this.FieldDelimiter = parent.FieldDelimiter;
            this.ComponentDelimiter = parent.ComponentDelimiter;
        }

        /// <summary>
        /// Returns parsed record.
        /// </summary>
        /// <param name="previousRecords">Records that will be used to determine parent/child relations.</param>
        /// <param name="line">Data to parse.</param>
        public static AstmRecord Parse(AstmRecord[] previousRecords, string line) {
            if (line.StartsWith("H", StringComparison.Ordinal)) {
                return AstmHeaderRecord.Parse(line);
            }

            if (line.StartsWith("P", StringComparison.Ordinal)) {
                for (int i = previousRecords.Length - 1; i >= 0; --i) {
                    AstmHeaderRecord parent = previousRecords[i] as AstmHeaderRecord;
                    if (parent != null) {
                        return AstmPatientRecord.Parse(parent, line);
                    }
                }
                //exception no parent found
            }

            if (line.StartsWith("O", StringComparison.Ordinal)) {
                for (int i = previousRecords.Length - 1; i >= 0; --i) {
                    AstmPatientRecord parent = previousRecords[i] as AstmPatientRecord;
                    if (parent != null) {
                        return AstmOrderRecord.Parse(parent, line);
                    }
                }
                //exception no parent found
            }

            if (line.StartsWith("R", StringComparison.Ordinal)) {
                for (int i = previousRecords.Length - 1; i >= 0; --i) {
                    AstmOrderRecord parent = previousRecords[i] as AstmOrderRecord;
                    if (parent != null) {
                        return AstmResultRecord.Parse(parent, line);
                    }
                }
                //exception no parent found
            }

            if (line.StartsWith("M", StringComparison.Ordinal)) {
                for (int i = previousRecords.Length - 1; i >= 0; --i) {
                    AstmResultRecord parent = previousRecords[i] as AstmResultRecord;
                    if (parent != null) {
                        return AstmManufacturerRecord.Parse(parent, line);
                    }
                }
                //exception no parent found
            }

            if (line.StartsWith("L", StringComparison.Ordinal)) {
                for (int i = previousRecords.Length - 1; i >= 0; --i) {
                    AstmHeaderRecord parent = previousRecords[i] as AstmHeaderRecord;
                    if (parent != null) {
                        return AstmTerminatorRecord.Parse(parent, line);
                    }
                }
                //exception no parent found
            }

            throw new InvalidOperationException("");
        }

        /// <summary>
        /// Gets underlying values without any further processing.
        /// </summary>
        public string[][] RawValues { get; protected set; }

        public AstmRecord Parent { get; protected set; }

        /// <summary>
        /// Returns value found at given 1-based index.
        /// </summary>
        /// <param name="fieldIndex">1-based index of field to retrieve.</param>
        public string ValueAt(int fieldIndex) {
            if ((fieldIndex < 1) || (fieldIndex > this.RawValues.Length)) { return string.Empty; }
            int realFieldIndex = fieldIndex - 1;
            return string.Join(this.ComponentDelimiter.ToString(), this.RawValues[realFieldIndex]);
        }

        /// <summary>
        /// Returns value found at given 1-based index.
        /// </summary>
        /// <param name="fieldIndex">1-based index of field to retrieve.</param>
        /// <param name="componentIndex">1-based index of component within field.</param>
        public string ValueAt(int fieldIndex, int componentIndex) {
            if ((fieldIndex < 1) || (fieldIndex > this.RawValues.Length)) { return string.Empty; }
            int realFieldIndex = fieldIndex - 1;
            if ((componentIndex < 1) || (componentIndex > this.RawValues[realFieldIndex].Length)) { return string.Empty; }
            int realComponentIndex = componentIndex - 1;
            return this.RawValues[realFieldIndex][realComponentIndex];
        }

        /// <summary>
        /// Gets field delimiter.
        /// </summary>
        public char FieldDelimiter { get; protected set; }

        /// <summary>
        /// Gets component delimiter.
        /// </summary>
        public char ComponentDelimiter { get; protected set; }

    }





    /// <summary>
    /// Implementation of header record.
    /// </summary>
    public class AstmHeaderRecord : AstmRecord {

        private AstmHeaderRecord() { }

        public static AstmHeaderRecord Parse(string line) {
            AstmHeaderRecord newObject = new AstmHeaderRecord();

            newObject.FieldDelimiter = line[1];
            newObject.ComponentDelimiter = line[3];

            List<string[]> all = new List<string[]>();
            all.Add(new string[] { "H" });
            all.Add(new string[] { line.Substring(1, 1), line.Substring(2, 1), line.Substring(3, 1), line.Substring(4, 1) });

            string[] fields = line.Substring(6).Split(newObject.FieldDelimiter);
            for (int i = 0; i < fields.Length; ++i) {
                string[] components = fields[i].Split(newObject.ComponentDelimiter);
                all.Add(components);
            }

            newObject.RawValues = all.ToArray();

            return newObject;
        }

        public string SenderName {
            get { return this.ValueAt(4); }
        }

        public DateTime DateOfTheMessage {
            get {
                DateTime result;
                if (DateTime.TryParseExact(this.ValueAt(14, 1), "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out result)) {
                    return result;
                } else {
                    return DateTime.MinValue;
                }
            }
        }

    }

    /// <summary>
    /// Implementation of patient record.
    /// </summary>
    public class AstmPatientRecord : AstmRecord {

        private AstmPatientRecord(AstmHeaderRecord headerRecord)
            : base(headerRecord) {
        }

        public static AstmPatientRecord Parse(AstmHeaderRecord parent, string line) {
            AstmPatientRecord newObject = new AstmPatientRecord(parent);

            List<string[]> all = new List<string[]>();
            string[] fields = line.Split(newObject.FieldDelimiter);
            for (int i = 0; i < fields.Length; ++i) {
                string[] components = fields[i].Split(newObject.ComponentDelimiter);
                all.Add(components);
            }
            newObject.RawValues = all.ToArray();

            return newObject;
        }

        public string PatientID {
            get { return this.ValueAt(3); }
        }

        public string PatientNationalID {
            get { return this.ValueAt(5, 1); }
        }

        public string MedicalRecord {
            get { return this.ValueAt(5, 2); }
        }

        public string OtherId {
            get { return this.ValueAt(5, 3); }
        }

        public string PatientLastName {
            get { return this.ValueAt(6, 1); }
        }

        public string PatientFirstName {
            get { return this.ValueAt(6, 2); }
        }

        public string MothersMaiden {
            get { return this.ValueAt(7); }
        }

        public DateTime PatientBirthDate {
            get {
                DateTime result;
                if (DateTime.TryParseExact(this.ValueAt(8), "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out result)) {
                    return result;
                } else {
                    return DateTime.MinValue;
                }
            }
        }

    }





    public class AstmOrderRecord : AstmRecord {

        private AstmOrderRecord(AstmPatientRecord headerRecord)
            : base(headerRecord) {
        }

        public static AstmOrderRecord Parse(AstmPatientRecord parent, string line) {
            AstmOrderRecord newObject = new AstmOrderRecord(parent);

            List<string[]> all = new List<string[]>();
            string[] fields = line.Split(newObject.FieldDelimiter);
            for (int i = 0; i < fields.Length; ++i) {
                string[] components = fields[i].Split(newObject.ComponentDelimiter);
                all.Add(components);
            }
            newObject.RawValues = all.ToArray();

            return newObject;
        }

        public string SampleID {
            get { return this.ValueAt(3); }
        }

        public string TestID {
            get { return this.ValueAt(5); }
        }

        public DateTime DateOfRequest {
            get {
                DateTime result;
                if (DateTime.TryParseExact(this.ValueAt(7), "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out result)) {
                    return result;
                } else {
                    return DateTime.MinValue;
                }
            }
        }

        public bool IsFinal {
            get { return this.ValueAt(26) == "F"; }
        }

    }





    /// <summary>
    /// Implementation of result record.
    /// </summary>
    public class AstmResultRecord : AstmRecord {

        private AstmResultRecord(AstmOrderRecord headerRecord)
            : base(headerRecord) {
        }

        public static AstmResultRecord Parse(AstmOrderRecord parent, string line) {
            AstmResultRecord newObject = new AstmResultRecord(parent);

            List<string[]> all = new List<string[]>();
            string[] fields = line.Split(newObject.FieldDelimiter);
            for (int i = 0; i < fields.Length; ++i) {
                string[] components = fields[i].Split(newObject.ComponentDelimiter);
                all.Add(components);
            }
            newObject.RawValues = all.ToArray();

            return newObject;
        }

        public string NameOfAnalysis {
            get { return this.ValueAt(3); }
        }

        public string Result {
            get { return this.ValueAt(4); }
        }

        public bool IsFinal {
            get { return this.ValueAt(9) == "F"; }
        }

        public bool IsRepeated {
            get { return this.ValueAt(9) == "R"; }
        }

        public bool IsCanceled {
            get { return this.ValueAt(9) == "X"; }
        }

        public DateTime Time {
            get {
                DateTime result;
                if (DateTime.TryParseExact(this.ValueAt(13), "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out result)) {
                    return result;
                } else {
                    return DateTime.MinValue;
                }
            }
        }


    }



    /// <summary>
    /// Implementation of terminator record.
    /// </summary>
    public class AstmTerminatorRecord : AstmRecord {

        private AstmTerminatorRecord(AstmHeaderRecord headerRecord)
            : base(headerRecord) {
        }

        public static AstmTerminatorRecord Parse(AstmHeaderRecord parent, string line) {
            AstmTerminatorRecord newObject = new AstmTerminatorRecord(parent);

            List<string[]> all = new List<string[]>();
            string[] fields = line.Split(newObject.FieldDelimiter);
            for (int i = 0; i < fields.Length; ++i) {
                string[] components = fields[i].Split(newObject.ComponentDelimiter);
                all.Add(components);
            }
            newObject.RawValues = all.ToArray();

            return newObject;
        }

    }



    /// <summary>
    /// Implementation of manufacturer record.
    /// </summary>
    public class AstmManufacturerRecord : AstmRecord {

        private AstmManufacturerRecord(AstmResultRecord parent)
            : base(parent) {
        }

        public static AstmManufacturerRecord Parse(AstmResultRecord parent, string line) {
            AstmManufacturerRecord newObject = new AstmManufacturerRecord(parent);

            List<string[]> all = new List<string[]>();
            string[] fields = line.Split(newObject.FieldDelimiter);
            for (int i = 0; i < fields.Length; ++i) {
                string[] components = fields[i].Split(newObject.ComponentDelimiter);
                all.Add(components);
            }
            newObject.RawValues = all.ToArray();

            return newObject;
        }

    }

}
