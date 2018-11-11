/* Josip Medved <jmedved@jmedved.com> * www.medo64.com * MIT License */

//2012-01-11: Refactoring.
//2011-08-04: Workaround mono bug #500987.
//2008-04-10: Uses IFormatProvider.
//2008-02-29: Fixed bugs in debug mode.
//2008-02-21: Initial version.


using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace Medo.Data {

    /// <summary>
    /// Generating command objects based on SQL queries or stored procedures.
    /// </summary>
    public class DeleteCommand : IDbCommand {

        private readonly string _tableName;
        private bool _needsMonoFix; //Mono bug #500987 / Error converting data type varchar to datetime


        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="connection">A connection object.</param>
        /// <param name="tableName">Name of table.</param>
        /// <exception cref="System.ArgumentNullException">Connection cannot be null.</exception>
        /// <exception cref="System.ArgumentException">Table name cannot be empty or null.</exception>
        /// <exception cref="System.InvalidCastException">Column name should be string and non-null.</exception>
        public DeleteCommand(IDbConnection connection, string tableName) {
            if (connection == null) { throw new ArgumentNullException("connection", Resources.ExceptionConnectionCannotBeNull); }
            if (string.IsNullOrEmpty(tableName)) { throw new ArgumentException(Resources.ExceptionTableNameCannotBeEmptyOrNull, "tableName"); }

            _baseCommand = connection.CreateCommand();

            _tableName = tableName;

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
                for (int i = 0; i < _whereParameters.Count; ++i) {
                    _baseCommand.Parameters.Remove(_whereParameters[i]);
                }
            }

            _needsMonoFix = false;
            List<string> argList = new List<string>();
            if (args != null) {
                for (int i = 0; i < args.Length; ++i) {
                    if (args[i] == null) {
                        argList.Add("NULL");
                    } else {
                        string paramName = string.Format(CultureInfo.InvariantCulture, "@W{0}", i);
                        argList.Add(paramName);
                        var param = _baseCommand.CreateParameter();
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
                        _baseCommand.Parameters.Add(param);
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
            if (string.IsNullOrEmpty(_whereText)) {
                if ((Connection is SqlConnection) && _needsMonoFix) {
                    _baseCommand.CommandText = string.Format(CultureInfo.InvariantCulture, "SET LANGUAGE us_english; DELETE FROM {0};", _tableName);
                } else {
                    _baseCommand.CommandText = string.Format(CultureInfo.InvariantCulture, "DELETE FROM {0};", _tableName);
                }
            } else {
                if ((Connection is SqlConnection) && _needsMonoFix) {
                    _baseCommand.CommandText = string.Format(CultureInfo.InvariantCulture, "SET LANGUAGE us_english; DELETE FROM {0} WHERE {1};", _tableName, _whereText.ToString());
                } else {
                    _baseCommand.CommandText = string.Format(CultureInfo.InvariantCulture, "DELETE FROM {0} WHERE {1};", _tableName, _whereText.ToString());
                }
            }
        }


        #region Base properties

        private IDbCommand _baseCommand;
        /// <summary>
        /// Gets underlying connection.
        /// </summary>
        public IDbCommand BaseCommand {
            get { return _baseCommand; }
        }

        #endregion

        #region IDbCommand Members

        /// <summary>
        /// Attempts to cancels the execution of an System.Data.IDbCommand.
        /// </summary>
        public void Cancel() {
            _baseCommand.Cancel();
        }

        /// <summary>
        /// Gets or sets the text command to run against the data source.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Proper parameterization is done in code.")]
        public string CommandText {
            get { return _baseCommand.CommandText; }
            set { _baseCommand.CommandText = value; }
        }

        /// <summary>
        /// Gets or sets the wait time before terminating the attempt to execute a command and generating an error.
        /// </summary>
        public int CommandTimeout {
            get { return _baseCommand.CommandTimeout; }
            set { _baseCommand.CommandTimeout = value; }
        }

        /// <summary>
        /// Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.
        /// </summary>
        public CommandType CommandType {
            get { return _baseCommand.CommandType; }
            set { _baseCommand.CommandType = value; }
        }

        /// <summary>
        /// Gets or sets the System.Data.IDbConnection used by this instance of the System.Data.IDbCommand.
        /// </summary>
        public IDbConnection Connection {
            get { return _baseCommand.Connection; }
            set { _baseCommand.Connection = value; }
        }

        /// <summary>
        /// Creates a new instance of an System.Data.IDbDataParameter object.
        /// </summary>
        public IDbDataParameter CreateParameter() {
            return _baseCommand.CreateParameter();
        }

        /// <summary>
        /// Executes an SQL statement against the Connection object of a .NET Framework data provider, and returns the number of rows affected.
        /// </summary>
        public int ExecuteNonQuery() {
#if DEBUG
            DebugCommand();
#endif
            return _baseCommand.ExecuteNonQuery();
        }

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection, and builds an System.Data.IDataReader using one of the System.Data.CommandBehavior values.
        /// </summary>
        /// <param name="behavior">One of the System.Data.CommandBehavior values.</param>
        public IDataReader ExecuteReader(CommandBehavior behavior) {
#if DEBUG
            DebugCommand();
#endif
            return _baseCommand.ExecuteReader(behavior);
        }

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection and builds an System.Data.IDataReader.
        /// </summary>
        public IDataReader ExecuteReader() {
#if DEBUG
            DebugCommand();
#endif
            return _baseCommand.ExecuteReader();
        }

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the resultset returned by the query. Extra columns or rows are ignored.
        /// </summary>
        public object ExecuteScalar() {
#if DEBUG
            DebugCommand();
#endif
            return _baseCommand.ExecuteScalar();
        }

        /// <summary>
        /// Gets the System.Data.IDataParameterCollection.
        /// </summary>
        public IDataParameterCollection Parameters {
            get { return _baseCommand.Parameters; }
        }

        /// <summary>
        /// Creates a prepared (or compiled) version of the command on the data source.
        /// </summary>
        public void Prepare() {
            _baseCommand.Prepare();
        }

        /// <summary>
        /// Gets or sets the transaction within which the Command object of a .NET Framework data provider executes.
        /// </summary>
        public IDbTransaction Transaction {
            get { return _baseCommand.Transaction; }
            set { _baseCommand.Transaction = value; }
        }

        /// <summary>
        /// Gets or sets how command results are applied to the System.Data.DataRow when used by the System.Data.IDataAdapter.Update(System.Data.DataSet) method of a System.Data.Common.DbDataAdapter.
        /// </summary>
        public UpdateRowSource UpdatedRowSource {
            get { return _baseCommand.UpdatedRowSource; }
            set { _baseCommand.UpdatedRowSource = value; }
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
                _baseCommand.Dispose();
            }
        }

        #endregion


#if DEBUG
        private void DebugCommand() {
            var sb = new StringBuilder();
            sb.AppendFormat(CultureInfo.InvariantCulture, "-- {0}", _baseCommand.CommandText);
            for (int i = 0; i < _baseCommand.Parameters.Count; ++i) {
                sb.AppendLine();
                if (_baseCommand.Parameters[i] is System.Data.Common.DbParameter curr) {
                    sb.AppendFormat(CultureInfo.InvariantCulture, "--     {0}=\"{1}\" ({2})", curr.ParameterName, curr.Value, curr.DbType);
                } else {
                    sb.AppendFormat(CultureInfo.InvariantCulture, "--     {0}", _baseCommand.Parameters[i].ToString());
                }
            }
            Debug.WriteLine(sb.ToString());
        }
#endif

        private static class Resources {

            internal static string ExceptionConnectionCannotBeNull { get { return "Connection cannot be null."; } }

            internal static string ExceptionTableNameCannotBeEmptyOrNull { get { return "Table name cannot be empty or null."; } }

        }


        private static bool IsRunningOnMono {
            get {
                return (Type.GetType("Mono.Runtime") != null);
            }
        }

    }

}
