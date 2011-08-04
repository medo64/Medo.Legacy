//Copyright (c) 2008 Josip Medved <jmedved@jmedved.com>

//2008-02-20: Initial version.
//2008-02-29: Fixed bugs in debug mode.
//2008-04-10: Uses IFormatProvider.
//2011-08-04: Workaround mono bug #500987.


using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Globalization;
using System.Text;

namespace Medo.Data {

    /// <summary>
    /// Generating command objects based on SQL queries or stored procedures.
    /// </summary>
    public class SelectCommand : System.Data.IDbCommand {

        private string _tableName;
        private string _columnsText;
        private bool _needsMonoFix; //Mono bug #500987 / Error converting data type varchar to datetime


        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="connection">A connection object.</param>
        /// <param name="tableName">Name of table.</param>
        /// <param name="columns">Column names and values in alternating order (name1, value1, name2, value2).</param>
        /// <exception cref="System.ArgumentNullException">Connection cannot be null.</exception>
        /// <exception cref="System.ArgumentException">Table name cannot be empty or null.</exception>
        /// <exception cref="System.InvalidCastException">Column name should be string and non-null.</exception>
        public SelectCommand(IDbConnection connection, string tableName, params string[] columns) {
            if (connection == null) { throw new System.ArgumentNullException("connection", Resources.ExceptionConnectionCannotBeNull); }
            if (string.IsNullOrEmpty(tableName)) { throw new System.ArgumentException(Resources.ExceptionTableNameCannotBeEmptyOrNull, "tableName"); }

            this._baseCommand = connection.CreateCommand();

            this._tableName = tableName;
            if ((columns != null) && (columns.Length > 0)) {
                StringBuilder sbColumns = new StringBuilder();
                for (int i = 0; i < columns.Length; i++) {
                    string name = columns[i];
                    if (name == null) { throw new System.InvalidCastException(Resources.ExceptionColumnNameShouldBeStringAndNonNull); }

                    if (sbColumns.Length > 0) { sbColumns.Append(", "); }
                    sbColumns.Append("[" + columns[i] + "]");
                }
                this._columnsText = sbColumns.ToString();
            } else {
                this._columnsText = "*";
            }

            UpdateCommandText();
        }



        private int _topCount;
        /// <summary>
        /// Sets top number of returned rows.
        /// </summary>
        /// <param name="count">Number of records to return.</param>
        public void SetTopCount(int count) {
            this._topCount = count;
            UpdateCommandText();
        }

        private string _orderByText;
        /// <summary>
        /// Sets order by statement used.
        /// </summary>
        /// <param name="text"></param>
        public void SetOrderBy(string text) {
            this._orderByText = text;
            UpdateCommandText();
        }

        private string _whereText;
        private List<IDbDataParameter> _whereParameters = new List<IDbDataParameter>();

        /// <summary>
        /// Sets where statement used.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An System.Object array containing zero or more objects to format. Those objects are inserted in Parameters as IDbDataParameter with name of @Px where x is order index.</param>
        public void SetWhere(string format, params object[] args) {
            if (this._whereParameters != null) {
                for (int i = 0; i < this._whereParameters.Count; ++i) {
                    this._baseCommand.Parameters.Remove(this._whereParameters[i]);
                }
            }

            this._needsMonoFix = false;
            List<string> argList = new List<string>();
            if (args != null) {
                for (int i = 0; i < args.Length; ++i) {
                    if (args[i] == null) {
                        argList.Add("NULL");
                    } else {
                        string paramName = string.Format(CultureInfo.InvariantCulture, "@W{0}", i);
                        argList.Add(paramName);
                        System.Data.IDbDataParameter param = this._baseCommand.CreateParameter();
                        param.ParameterName = paramName;
                        if ((args[i] is DateTime) && (IsRunningOnMono)) {
                            args[i] = ((DateTime)args[i]).ToString(CultureInfo.InvariantCulture);
                            this._needsMonoFix = true;
                        }
                        param.Value = args[i];
                        if (param.DbType == DbType.DateTime) {
                            OleDbParameter odp = param as OleDbParameter;
                            if (odp != null) { odp.OleDbType = OleDbType.Date; }
                        }
                        this._whereParameters.Add(param);
                        this._baseCommand.Parameters.Add(param);
                    }
                }
            }
            if (argList.Count > 0) {
                this._whereText = string.Format(CultureInfo.InvariantCulture, format, argList.ToArray());
            } else {
                this._whereText = format;
            }

            UpdateCommandText();
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Proper parameterization is done in code.")]
        private void UpdateCommandText() {
            string selectPrefix;
            if (this._topCount == 0) {
                selectPrefix = "SELECT";
            } else {
                selectPrefix = string.Format(System.Globalization.CultureInfo.InvariantCulture, "SELECT TOP {0}", this._topCount);
            }
            if (this._needsMonoFix) { selectPrefix = "SET LANGUAGE us_english; " + selectPrefix; }
            string orderBySuffix;
            if (string.IsNullOrEmpty(_orderByText)) {
                orderBySuffix = string.Empty;
            } else {
                orderBySuffix = string.Format(System.Globalization.CultureInfo.InvariantCulture, " ORDER BY {0}", this._orderByText);
            }

            if (string.IsNullOrEmpty(this._whereText)) {
                this._baseCommand.CommandText = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{2} {1} FROM {0}{3};", this._tableName, this._columnsText, selectPrefix, orderBySuffix);
            } else {
                this._baseCommand.CommandText = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{3} {1} FROM {0} WHERE {2}{4};", this._tableName, this._columnsText, _whereText, selectPrefix, orderBySuffix);
            }
        }


        #region Base properties

        private IDbCommand _baseCommand;
        /// <summary>
        /// Gets underlying connection.
        /// </summary>
        public System.Data.IDbCommand BaseCommand {
            get { return this._baseCommand; }
        }

        #endregion

        #region IDbCommand Members

        /// <summary>
        /// Attempts to cancels the execution of an System.Data.IDbCommand.
        /// </summary>
        public void Cancel() {
            this._baseCommand.Cancel();
        }

        /// <summary>
        /// Gets or sets the text command to run against the data source.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Proper parameterization is done in code.")]
        public string CommandText {
            get { return this._baseCommand.CommandText; }
            set { this._baseCommand.CommandText = value; }
        }

        /// <summary>
        /// Gets or sets the wait time before terminating the attempt to execute a command and generating an error.
        /// </summary>
        public int CommandTimeout {
            get { return this._baseCommand.CommandTimeout; }
            set { this._baseCommand.CommandTimeout = value; }
        }

        /// <summary>
        /// Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.
        /// </summary>
        public System.Data.CommandType CommandType {
            get { return this._baseCommand.CommandType; }
            set { this._baseCommand.CommandType = value; }
        }

        /// <summary>
        /// Gets or sets the System.Data.IDbConnection used by this instance of the System.Data.IDbCommand.
        /// </summary>
        public System.Data.IDbConnection Connection {
            get { return this._baseCommand.Connection; }
            set { this._baseCommand.Connection = value; }
        }

        /// <summary>
        /// Creates a new instance of an System.Data.IDbDataParameter object.
        /// </summary>
        public System.Data.IDbDataParameter CreateParameter() {
            return this._baseCommand.CreateParameter();
        }

        /// <summary>
        /// Executes an SQL statement against the Connection object of a .NET Framework data provider, and returns the number of rows affected.
        /// </summary>
        public int ExecuteNonQuery() {
#if DEBUG
            DebugCommand();
#endif
            return this._baseCommand.ExecuteNonQuery();
        }

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection, and builds an System.Data.IDataReader using one of the System.Data.CommandBehavior values.
        /// </summary>
        /// <param name="behavior">One of the System.Data.CommandBehavior values.</param>
        public System.Data.IDataReader ExecuteReader(System.Data.CommandBehavior behavior) {
#if DEBUG
            DebugCommand();
#endif
            return this._baseCommand.ExecuteReader(behavior);
        }

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection and builds an System.Data.IDataReader.
        /// </summary>
        public System.Data.IDataReader ExecuteReader() {
#if DEBUG
            DebugCommand();
#endif
            return this._baseCommand.ExecuteReader();
        }

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the resultset returned by the query. Extra columns or rows are ignored.
        /// </summary>
        public object ExecuteScalar() {
#if DEBUG
            DebugCommand();
#endif
            return this._baseCommand.ExecuteScalar();
        }

        /// <summary>
        /// Gets the System.Data.IDataParameterCollection.
        /// </summary>
        public System.Data.IDataParameterCollection Parameters {
            get { return this._baseCommand.Parameters; }
        }

        /// <summary>
        /// Creates a prepared (or compiled) version of the command on the data source.
        /// </summary>
        public void Prepare() {
            this._baseCommand.Prepare();
        }

        /// <summary>
        /// Gets or sets the transaction within which the Command object of a .NET Framework data provider executes.
        /// </summary>
        public System.Data.IDbTransaction Transaction {
            get { return this._baseCommand.Transaction; }
            set { this._baseCommand.Transaction = value; }
        }

        /// <summary>
        /// Gets or sets how command results are applied to the System.Data.DataRow when used by the System.Data.IDataAdapter.Update(System.Data.DataSet) method of a System.Data.Common.DbDataAdapter.
        /// </summary>
        public System.Data.UpdateRowSource UpdatedRowSource {
            get { return this._baseCommand.UpdatedRowSource; }
            set { this._baseCommand.UpdatedRowSource = value; }
        }

        #endregion

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
                this._baseCommand.Dispose();
            }
        }

        #endregion



#if DEBUG
        private void DebugCommand() {
            System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "I: {0}.    {{Medo.Data.SelectCommand}}", this._baseCommand.CommandText));
            for (int i = 0; i < this._baseCommand.Parameters.Count; ++i) {
                System.Data.Common.DbParameter curr = this._baseCommand.Parameters[i] as System.Data.Common.DbParameter;
                if (curr != null) {
                    System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "I:     {0}=\"{1}\" ({2}).    {{Medo.Data.SelectCommand}}", curr.ParameterName, curr.Value, curr.DbType));
                } else {
                    System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "I:     {0}.    {{Medo.Data.SelectCommand}}", this._baseCommand.Parameters[i].ToString()));
                }
            }
        }
#endif


        private static class Resources {

            internal static string ExceptionConnectionCannotBeNull { get { return "Connection cannot be null."; } }

            internal static string ExceptionTableNameCannotBeEmptyOrNull { get { return "Table name cannot be empty or null."; } }

            internal static string ExceptionColumnNameShouldBeStringAndNonNull { get { return "Column name should be string and non-null."; } }

        }

        private static bool IsRunningOnMono {
            get {
                return (Type.GetType("Mono.Runtime") != null);
            }
        }

    }

}
