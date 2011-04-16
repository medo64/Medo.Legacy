//Copyright (c) 2008 Josip Medved <jmedved@jmedved.com>

//2008-02-20: Initial version.
//2008-02-29: Fixed bugs in debug mode.
//            Added scope identity support.
//2008-04-10: Uses IFormatProvider.
//2008-05-20: Small fixes.
//2010-09-11: Added OutputColumn.


using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Globalization;
using System.Text;

namespace Medo.Data {

    /// <summary>
    /// Generating command objects based on SQL queries or stored procedures.
    /// </summary>
    public class InsertCommand : System.Data.IDbCommand {

        private readonly string TableName;
        private readonly string ColumnsText;
        private readonly string ValuesText;


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
            if (connection == null) { throw new System.ArgumentNullException("connection", Resources.ExceptionConnectionCannotBeNull); }
            if (string.IsNullOrEmpty(tableName)) { throw new System.ArgumentException(Resources.ExceptionTableNameCannotBeEmptyOrNull, "tableName"); }
            if (columnsAndValues == null) { throw new System.ArgumentException(Resources.ExceptionNumberOfParametersMustBeMultipleOfTwo, "columnsAndValues"); }
            if (columnsAndValues.Length % 2 != 0) { throw new System.ArgumentException(Resources.ExceptionNumberOfParametersMustBeMultipleOfTwo, "columnsAndValues"); }

            this._baseCommand = connection.CreateCommand();

            this.TableName = tableName;

            StringBuilder sbColumns = new StringBuilder();
            StringBuilder sbValues = new StringBuilder();
            for (int i = 0; i < columnsAndValues.Length; i += 2) {
                string name = columnsAndValues[i] as string;
                if (name == null) { throw new System.InvalidCastException(Resources.ExceptionColumnNameShouldBeStringAndNonNull); }
                object value = columnsAndValues[i + 1];

                if (sbColumns.Length > 0) { sbColumns.Append(", "); }
                sbColumns.Append("[" + name + "]");

                if (sbValues.Length > 0) { sbValues.Append(", "); }
                if (value == null) {
                    sbValues.Append("NULL");
                } else {
                    string paramName = "@P" + (i / 2).ToString(System.Globalization.CultureInfo.InvariantCulture);
                    sbValues.Append(paramName);
                    System.Data.IDbDataParameter param = this._baseCommand.CreateParameter();
                    param.ParameterName = paramName;
                    param.Value = value;
                    if (param.DbType == System.Data.DbType.DateTime) {
                        System.Data.OleDb.OleDbParameter odp = param as System.Data.OleDb.OleDbParameter;
                        if (odp != null) { odp.OleDbType = System.Data.OleDb.OleDbType.Date; }
                    }
                    this._baseCommand.Parameters.Add(param);
                }
            }
            this.ColumnsText = sbColumns.ToString();
            this.ValuesText = sbValues.ToString();

            UpdateCommandText();
        }

        private bool _useScopeIdentity;
        /// <summary>
        /// If true, command text will be extended with scope identity whose value can be retrieved via ExecuteScalar function.
        /// Cannot be used togeter with OutputColumn.
        /// </summary>
        public bool UseScopeIdentity {
            get { return this._useScopeIdentity; }
            set {
                this._useScopeIdentity = value;
                if (value == true) { this._outputColumn = null; }
                UpdateCommandText();
            }
        }

        private string _outputColumn;
        /// <summary>
        /// If true, command text will be extended with Output directive so first column's value can be retrieved via ExecuteScalar function.
        /// Cannot be used togeter with UseScopeIdentity.
        /// </summary>
        public string OutputColumn {
            get { return this._outputColumn; }
            set {
                if (value != null) {
                    this._outputColumn = "[" + value + "]";
                    this._useScopeIdentity = false;
                } else {
                    this._outputColumn = null;
                }
                UpdateCommandText();
            }
        }

        private void UpdateCommandText() {
            if (this.OutputColumn != null) {
                this._baseCommand.CommandText = string.Format(System.Globalization.CultureInfo.InvariantCulture, "INSERT INTO {0}({1}) OUTPUT INSERTED.{3} VALUES({2}); SELECT SCOPE_IDENTITY();", TableName, this.ColumnsText, this.ValuesText, this.OutputColumn);
            } else if (this.UseScopeIdentity) {
                this._baseCommand.CommandText = string.Format(System.Globalization.CultureInfo.InvariantCulture, "INSERT INTO {0}({1}) VALUES({2}); SELECT SCOPE_IDENTITY();", TableName, this.ColumnsText, this.ValuesText);
            } else {
                this._baseCommand.CommandText = string.Format(System.Globalization.CultureInfo.InvariantCulture, "INSERT INTO {0}({1}) VALUES({2});", TableName, this.ColumnsText, this.ValuesText);
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
            System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "I: {0}.    {{Medo.Data.InsertCommand}}", this._baseCommand.CommandText));
            for (int i = 0; i < this._baseCommand.Parameters.Count; ++i) {
                System.Data.Common.DbParameter curr = this._baseCommand.Parameters[i] as System.Data.Common.DbParameter;
                if (curr != null) {
                    System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "I:     {0}=\"{1}\".    {{Medo.Data.InsertCommand}}", curr.ParameterName, curr.Value));
                } else {
                    System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "I:     {0}.    {{Medo.Data.InsertCommand}}", this._baseCommand.Parameters[i].ToString()));
                }
            }
        }
#endif

        private static class Resources {

            internal static string ExceptionConnectionCannotBeNull { get { return "Connection cannot be null."; } }

            internal static string ExceptionTableNameCannotBeEmptyOrNull { get { return "Table name cannot be empty or null."; } }

            internal static string ExceptionColumnNameShouldBeStringAndNonNull { get { return "Column name should be string and non-null."; } }

            internal static string ExceptionNumberOfParametersMustBeMultipleOfTwo { get { return "Number of parameters must be multiple of two."; } }

        }

    }

}
