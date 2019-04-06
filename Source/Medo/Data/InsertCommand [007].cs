/* Josip Medved <jmedved@jmedved.com> * www.medo64.com * MIT License */

//2012-01-11: Refactoring in order to support PostgreSQL.
//2011-08-04: Workaround mono bug #500987.
//2010-09-11: Added OutputColumn.
//2008-05-20: Small fixes.
//2008-04-10: Uses IFormatProvider.
//2008-02-29: Fixed bugs in debug mode.
//            Added scope identity support.
//2008-02-20: Initial version.


using System;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace Medo.Data {

    /// <summary>
    /// Generating command objects based on SQL queries or stored procedures.
    /// </summary>
    public class InsertCommand : IDbCommand {

        private readonly string TableName;
        private readonly string ColumnsText;
        private readonly string ValuesText;
        private readonly bool NeedsMonoFix; //Mono bug #500987 / Error converting data type varchar to datetime


        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="connection">A connection object.</param>
        /// <param name="tableName">Name of table.</param>
        /// <param name="columnsAndValues">Column names and values in alternating order (name1, value1, name2, value2).</param>
        /// <exception cref="System.ArgumentNullException">Connection cannot be null.</exception>
        /// <exception cref="System.ArgumentException">Table name cannot be empty or null.</exception>
        /// <exception cref="System.InvalidCastException">Column name should be string and non-null.</exception>
        public InsertCommand(IDbConnection connection, string tableName, params object[] columnsAndValues) {
            if (connection == null) { throw new ArgumentNullException("connection", Resources.ExceptionConnectionCannotBeNull); }
            if (string.IsNullOrEmpty(tableName)) { throw new ArgumentException(Resources.ExceptionTableNameCannotBeEmptyOrNull, "tableName"); }
            if (columnsAndValues == null) { throw new ArgumentException(Resources.ExceptionNumberOfParametersMustBeMultipleOfTwo, "columnsAndValues"); }
            if (columnsAndValues.Length % 2 != 0) { throw new ArgumentException(Resources.ExceptionNumberOfParametersMustBeMultipleOfTwo, "columnsAndValues"); }

            BaseCommand = connection.CreateCommand();

            TableName = tableName;

            NeedsMonoFix = false;
            var sbColumns = new StringBuilder();
            var sbValues = new StringBuilder();
            for (var i = 0; i < columnsAndValues.Length; i += 2) {
                if (!(columnsAndValues[i] is string name)) { throw new InvalidCastException(Resources.ExceptionColumnNameShouldBeStringAndNonNull); }
                var value = columnsAndValues[i + 1];

                if (sbColumns.Length > 0) { sbColumns.Append(", "); }
                if (Connection is SqlConnection) {
                    sbColumns.Append("[" + name + "]");
                } else {
                    sbColumns.Append(name);
                }

                if (sbValues.Length > 0) { sbValues.Append(", "); }
                if (value == null) {
                    sbValues.Append("NULL");
                } else {
                    var paramName = "@P" + (i / 2).ToString(CultureInfo.InvariantCulture);
                    sbValues.Append(paramName);
                    var param = BaseCommand.CreateParameter();
                    param.ParameterName = paramName;
                    if ((value is DateTime) && (IsRunningOnMono)) {
                        value = ((DateTime)value).ToString(CultureInfo.InvariantCulture);
                        NeedsMonoFix = true;
                    }
                    param.Value = value;
                    if (param.DbType == DbType.DateTime) {
                        if (param is OleDbParameter odp) { odp.OleDbType = OleDbType.Date; }
                    }
                    BaseCommand.Parameters.Add(param);
                }
            }
            ColumnsText = sbColumns.ToString();
            ValuesText = sbValues.ToString();

            UpdateCommandText();
        }

        private bool _useScopeIdentity;
        /// <summary>
        /// If true, command text will be extended with scope identity whose value can be retrieved via ExecuteScalar function.
        /// Cannot be used together with OutputColumn.
        /// </summary>
        public bool UseScopeIdentity {
            get { return _useScopeIdentity; }
            set {
                _useScopeIdentity = value;
                if (value == true) { _outputColumn = null; }
                UpdateCommandText();
            }
        }

        private string _outputColumn;
        /// <summary>
        /// If true, command text will be extended with Output directive so first column's value can be retrieved via ExecuteScalar function.
        /// Cannot be used together with UseScopeIdentity.
        /// </summary>
        [Obsolete("Obsoleted because it is only properly supported by SQL Server.")]
        public string OutputColumn {
            get { return _outputColumn; }
            set {
                if (value != null) {
                    if (Connection is SqlConnection) {
                        _outputColumn = "[" + value + "]";
                    } else {
                        _outputColumn = value;
                    }
                    _useScopeIdentity = false;
                } else {
                    _outputColumn = null;
                }
                UpdateCommandText();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Proper parameterization is done in code.")]
        private void UpdateCommandText() {
            if (_outputColumn != null) {
                if ((Connection is SqlConnection) && NeedsMonoFix) {
                    BaseCommand.CommandText = string.Format(CultureInfo.InvariantCulture, "SET LANGUAGE us_english; INSERT INTO {0}({1}) OUTPUT INSERTED.{3} VALUES({2}); SELECT SCOPE_IDENTITY();", TableName, ColumnsText, ValuesText, _outputColumn);
                } else {
                    BaseCommand.CommandText = string.Format(CultureInfo.InvariantCulture, "INSERT INTO {0}({1}) OUTPUT INSERTED.{3} VALUES({2}); SELECT SCOPE_IDENTITY();", TableName, ColumnsText, ValuesText, _outputColumn);
                }
            } else if (UseScopeIdentity) {
                if (Connection is SqlConnection) {
                    if (NeedsMonoFix) {
                        BaseCommand.CommandText = string.Format(CultureInfo.InvariantCulture, "SET LANGUAGE us_english; INSERT INTO {0}({1}) VALUES({2}); SELECT SCOPE_IDENTITY();", TableName, ColumnsText, ValuesText);
                    } else {
                        BaseCommand.CommandText = string.Format(CultureInfo.InvariantCulture, "INSERT INTO {0}({1}) VALUES({2}); SELECT SCOPE_IDENTITY();", TableName, ColumnsText, ValuesText);
                    }
                } else if (Connection.GetType().FullName.Equals("Npgsql.NpgsqlConnection", StringComparison.Ordinal)) {
                    BaseCommand.CommandText = string.Format(CultureInfo.InvariantCulture, "INSERT INTO {0}({1}) VALUES({2}) RETURNING ID;", TableName, ColumnsText, ValuesText);
                } else {
                    BaseCommand.CommandText = string.Format(CultureInfo.InvariantCulture, "INSERT INTO {0}({1}) VALUES({2}); SELECT SCOPE_IDENTITY();", TableName, ColumnsText, ValuesText);
                }
            } else {
                if ((Connection is SqlConnection) && NeedsMonoFix) {
                    BaseCommand.CommandText = string.Format(CultureInfo.InvariantCulture, "SET LANGUAGE us_english; INSERT INTO {0}({1}) VALUES({2});", TableName, ColumnsText, ValuesText);
                } else {
                    BaseCommand.CommandText = string.Format(CultureInfo.InvariantCulture, "INSERT INTO {0}({1}) VALUES({2});", TableName, ColumnsText, ValuesText);
                }
            }
        }


        #region Base properties

        /// <summary>
        /// Gets underlying connection.
        /// </summary>
        public IDbCommand BaseCommand { get; private set; }

        #endregion

        #region IDbCommand Members

        /// <summary>
        /// Attempts to cancels the execution of an System.Data.IDbCommand.
        /// </summary>
        public void Cancel() {
            BaseCommand.Cancel();
        }

        /// <summary>
        /// Gets or sets the text command to run against the data source.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Proper parameterization is done in code.")]
        public string CommandText {
            get { return BaseCommand.CommandText; }
            set { BaseCommand.CommandText = value; }
        }

        /// <summary>
        /// Gets or sets the wait time before terminating the attempt to execute a command and generating an error.
        /// </summary>
        public int CommandTimeout {
            get { return BaseCommand.CommandTimeout; }
            set { BaseCommand.CommandTimeout = value; }
        }

        /// <summary>
        /// Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.
        /// </summary>
        public CommandType CommandType {
            get { return BaseCommand.CommandType; }
            set { BaseCommand.CommandType = value; }
        }

        /// <summary>
        /// Gets or sets the System.Data.IDbConnection used by this instance of the System.Data.IDbCommand.
        /// </summary>
        public IDbConnection Connection {
            get { return BaseCommand.Connection; }
            set { BaseCommand.Connection = value; }
        }

        /// <summary>
        /// Creates a new instance of an System.Data.IDbDataParameter object.
        /// </summary>
        public IDbDataParameter CreateParameter() {
            return BaseCommand.CreateParameter();
        }

        /// <summary>
        /// Executes an SQL statement against the Connection object of a .NET Framework data provider, and returns the number of rows affected.
        /// </summary>
        public int ExecuteNonQuery() {
#if DEBUG
            DebugCommand();
#endif
            return BaseCommand.ExecuteNonQuery();
        }

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection, and builds an System.Data.IDataReader using one of the System.Data.CommandBehavior values.
        /// </summary>
        /// <param name="behavior">One of the System.Data.CommandBehavior values.</param>
        public IDataReader ExecuteReader(CommandBehavior behavior) {
#if DEBUG
            DebugCommand();
#endif
            return BaseCommand.ExecuteReader(behavior);
        }

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection and builds an System.Data.IDataReader.
        /// </summary>
        public IDataReader ExecuteReader() {
#if DEBUG
            DebugCommand();
#endif
            return BaseCommand.ExecuteReader();
        }

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the resultset returned by the query. Extra columns or rows are ignored.
        /// </summary>
        public object ExecuteScalar() {
#if DEBUG
            DebugCommand();
#endif
            return BaseCommand.ExecuteScalar();
        }

        /// <summary>
        /// Gets the System.Data.IDataParameterCollection.
        /// </summary>
        public IDataParameterCollection Parameters {
            get { return BaseCommand.Parameters; }
        }

        /// <summary>
        /// Creates a prepared (or compiled) version of the command on the data source.
        /// </summary>
        public void Prepare() {
            BaseCommand.Prepare();
        }

        /// <summary>
        /// Gets or sets the transaction within which the Command object of a .NET Framework data provider executes.
        /// </summary>
        public IDbTransaction Transaction {
            get { return BaseCommand.Transaction; }
            set { BaseCommand.Transaction = value; }
        }

        /// <summary>
        /// Gets or sets how command results are applied to the System.Data.DataRow when used by the System.Data.IDataAdapter.Update(System.Data.DataSet) method of a System.Data.Common.DbDataAdapter.
        /// </summary>
        public UpdateRowSource UpdatedRowSource {
            get { return BaseCommand.UpdatedRowSource; }
            set { BaseCommand.UpdatedRowSource = value; }
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">True if managed resources should be disposed; otherwise, false.</param>
        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                BaseCommand.Dispose();
            }
        }

        #endregion



#if DEBUG
        private void DebugCommand() {
            var sb = new StringBuilder();
            sb.AppendFormat(CultureInfo.InvariantCulture, "-- {0}", BaseCommand.CommandText);
            for (var i = 0; i < BaseCommand.Parameters.Count; ++i) {
                sb.AppendLine();
                if (BaseCommand.Parameters[i] is DbParameter curr) {
                    sb.AppendFormat(CultureInfo.InvariantCulture, "--     {0}=\"{1}\" ({2})", curr.ParameterName, curr.Value, curr.DbType);
                } else {
                    sb.AppendFormat(CultureInfo.InvariantCulture, "--     {0}", BaseCommand.Parameters[i].ToString());
                }
            }
            Debug.WriteLine(sb.ToString());
        }
#endif

        private static class Resources {

            internal static string ExceptionConnectionCannotBeNull { get { return "Connection cannot be null."; } }

            internal static string ExceptionTableNameCannotBeEmptyOrNull { get { return "Table name cannot be empty or null."; } }

            internal static string ExceptionColumnNameShouldBeStringAndNonNull { get { return "Column name should be string and non-null."; } }

            internal static string ExceptionNumberOfParametersMustBeMultipleOfTwo { get { return "Number of parameters must be multiple of two."; } }

        }

        private static bool IsRunningOnMono {
            get {
                return (Type.GetType("Mono.Runtime") != null);
            }
        }

    }

}
