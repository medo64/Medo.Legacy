/* Josip Medved <jmedved@jmedved.com> * www.medo64.com * MIT License */

//2012-01-11: Refactoring in order to support PostgreSQL.
//2011-08-07: Workaround mono bug #500987.
//2008-04-10: Uses IFormatProvider.
//2008-02-29: Fixed bugs in debug mode.
//2008-02-21: Initial version.


using System;
using System.Collections.Generic;
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
    public class UpdateCommand : IDbCommand {

        private readonly string _tableName;
        private readonly string _columnsAndValuesText;
        private bool _needsMonoFix; //Mono bug #500987 / Error converting data type varchar to datetime


        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="connection">A connection object.</param>
        /// <param name="tableName">Name of table.</param>
        /// <param name="columnsAndValues">Column names and values in alternating order (name1, value1, name2, value2).</param>
        /// <exception cref="System.ArgumentNullException">Connection cannot be null.</exception>
        /// <exception cref="System.ArgumentException">Table name cannot be empty or null.</exception>
        /// <exception cref="System.InvalidCastException">Column name should be string and non-null.</exception>
        public UpdateCommand(IDbConnection connection, string tableName, params object[] columnsAndValues) {
            if (connection == null) { throw new ArgumentNullException("connection", Resources.ExceptionConnectionCannotBeNull); }
            if (string.IsNullOrEmpty(tableName)) { throw new ArgumentException(Resources.ExceptionTableNameCannotBeEmptyOrNull, "tableName"); }
            if (columnsAndValues == null) { throw new ArgumentException(Resources.ExceptionNumberOfParametersMustBeMultipleOfTwo, "columnsAndValues"); }
            if (columnsAndValues.Length % 2 != 0) { throw new ArgumentException(Resources.ExceptionNumberOfParametersMustBeMultipleOfTwo, "columnsAndValues"); }

            BaseCommand = connection.CreateCommand();

            _tableName = tableName;

            _needsMonoFix = false;
            var sbColumnsAndValues = new StringBuilder();
            for (var i = 0; i < columnsAndValues.Length; i += 2) {
                if (!(columnsAndValues[i] is string name)) { throw new InvalidCastException(Resources.ExceptionColumnNameShouldBeStringAndNonNull); }
                var value = columnsAndValues[i + 1];

                if (sbColumnsAndValues.Length > 0) { sbColumnsAndValues.Append(", "); }
                if (Connection is SqlConnection) {
                    sbColumnsAndValues.Append("[" + name + "]=");
                } else {
                    sbColumnsAndValues.Append(name + "=");
                }

                if (value == null) {
                    sbColumnsAndValues.Append("NULL");
                } else {
                    var paramName = "@P" + (i / 2).ToString(CultureInfo.InvariantCulture);
                    sbColumnsAndValues.Append(paramName);
                    var param = BaseCommand.CreateParameter();
                    param.ParameterName = paramName;
                    if ((value is DateTime) && (IsRunningOnMono)) {
                        value = ((DateTime)value).ToString(CultureInfo.InvariantCulture);
                        _needsMonoFix = true;
                    }
                    param.Value = value;
                    if (param.DbType == DbType.DateTime) {
                        if (param is OleDbParameter odp) { odp.OleDbType = OleDbType.Date; }
                    }
                    BaseCommand.Parameters.Add(param);
                }
            }
            _columnsAndValuesText = sbColumnsAndValues.ToString();

            UpdateCommandText();
        }

        private string _whereText;
        private readonly List<IDbDataParameter> _whereParameters = new List<IDbDataParameter>();

        /// <summary>
        /// Sets where statement used.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An System.Object array containing zero or more objects to format. Those objects are inserted in Parameters as IDbDataParameter with name of @Px where x is order index.</param>
        public void SetWhere(string format, params object[] args) {
            if (_whereParameters != null) {
                for (var i = 0; i < _whereParameters.Count; ++i) {
                    BaseCommand.Parameters.Remove(_whereParameters[i]);
                }
            }

            _needsMonoFix = false;
            var argList = new List<string>();
            if (args != null) {
                for (var i = 0; i < args.Length; ++i) {
                    if (args[i] == null) {
                        argList.Add("NULL");
                    } else {
                        var paramName = string.Format(CultureInfo.InvariantCulture, "@W{0}", i);
                        argList.Add(paramName);
                        var param = BaseCommand.CreateParameter();
                        param.ParameterName = paramName;
                        if ((args[i] is DateTime) && (IsRunningOnMono)) {
                            args[i] = ((DateTime)args[i]).ToString(CultureInfo.InvariantCulture);
                            _needsMonoFix = true;
                        }
                        param.Value = args[i];
                        if (param.DbType == DbType.DateTime) {
                            if (param is OleDbParameter odp) { odp.OleDbType = OleDbType.Date; }
                        }
                        _whereParameters.Add(param);
                        BaseCommand.Parameters.Add(param);
                    }
                }
            }
            if (argList.Count > 0) {
                _whereText = string.Format(CultureInfo.InvariantCulture, format, argList.ToArray());
            } else {
                _whereText = format;
            }

            UpdateCommandText();
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Proper parameterization is done in code.")]
        private void UpdateCommandText() {
            if (_whereText == null) {
                if ((Connection is SqlConnection) && _needsMonoFix) {
                    BaseCommand.CommandText = string.Format(CultureInfo.InvariantCulture, "SET LANGUAGE us_english; UPDATE {0} SET {1};", _tableName, _columnsAndValuesText);
                } else {
                    BaseCommand.CommandText = string.Format(CultureInfo.InvariantCulture, "UPDATE {0} SET {1};", _tableName, _columnsAndValuesText);
                }
            } else {
                if ((Connection is SqlConnection) && _needsMonoFix) {
                    BaseCommand.CommandText = string.Format(CultureInfo.InvariantCulture, "SET LANGUAGE us_english; UPDATE {0} SET {1} WHERE {2};", _tableName, _columnsAndValuesText, _whereText.ToString());
                } else {
                    BaseCommand.CommandText = string.Format(CultureInfo.InvariantCulture, "UPDATE {0} SET {1} WHERE {2};", _tableName, _columnsAndValuesText, _whereText.ToString());
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
