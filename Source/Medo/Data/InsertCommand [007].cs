//Josip Medved <jmedved@jmedved.com>   www.medo64.com

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
        private bool NeedsMonoFix; //Mono bug #500987 / Error converting data type varchar to datetime


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

            this._baseCommand = connection.CreateCommand();

            this.TableName = tableName;

            this.NeedsMonoFix = false;
            StringBuilder sbColumns = new StringBuilder();
            StringBuilder sbValues = new StringBuilder();
            for (int i = 0; i < columnsAndValues.Length; i += 2) {
                string name = columnsAndValues[i] as string;
                if (name == null) { throw new InvalidCastException(Resources.ExceptionColumnNameShouldBeStringAndNonNull); }
                object value = columnsAndValues[i + 1];

                if (sbColumns.Length > 0) { sbColumns.Append(", "); }
                if (this.Connection is SqlConnection) {
                    sbColumns.Append("[" + name + "]");
                } else {
                    sbColumns.Append(name);
                }

                if (sbValues.Length > 0) { sbValues.Append(", "); }
                if (value == null) {
                    sbValues.Append("NULL");
                } else {
                    string paramName = "@P" + (i / 2).ToString(CultureInfo.InvariantCulture);
                    sbValues.Append(paramName);
                    var param = this._baseCommand.CreateParameter();
                    param.ParameterName = paramName;
                    if ((value is DateTime) && (IsRunningOnMono)) {
                        value = ((DateTime)value).ToString(CultureInfo.InvariantCulture);
                        this.NeedsMonoFix = true;
                    }
                    param.Value = value;
                    if (param.DbType == DbType.DateTime) {
                        var odp = param as OleDbParameter;
                        if (odp != null) { odp.OleDbType = OleDbType.Date; }
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
        public Boolean UseScopeIdentity {
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
        [Obsolete("Obsoleted because it is only properly supported by SQL Server.")]
        public String OutputColumn {
            get { return this._outputColumn; }
            set {
                if (value != null) {
                    if (this.Connection is SqlConnection) {
                        this._outputColumn = "[" + value + "]";
                    } else {
                        this._outputColumn = value;
                    }
                    this._useScopeIdentity = false;
                } else {
                    this._outputColumn = null;
                }
                UpdateCommandText();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Proper parameterization is done in code.")]
        private void UpdateCommandText() {
            if (this._outputColumn != null) {
                if ((this.Connection is SqlConnection) && this.NeedsMonoFix) {
                    this._baseCommand.CommandText = string.Format(CultureInfo.InvariantCulture, "SET LANGUAGE us_english; INSERT INTO {0}({1}) OUTPUT INSERTED.{3} VALUES({2}); SELECT SCOPE_IDENTITY();", TableName, this.ColumnsText, this.ValuesText, this._outputColumn);
                } else {
                    this._baseCommand.CommandText = string.Format(CultureInfo.InvariantCulture, "INSERT INTO {0}({1}) OUTPUT INSERTED.{3} VALUES({2}); SELECT SCOPE_IDENTITY();", TableName, this.ColumnsText, this.ValuesText, this._outputColumn);
                }
            } else if (this.UseScopeIdentity) {
                if (this.Connection is SqlConnection) {
                    if (this.NeedsMonoFix) {
                        this._baseCommand.CommandText = string.Format(CultureInfo.InvariantCulture, "SET LANGUAGE us_english; INSERT INTO {0}({1}) VALUES({2}); SELECT SCOPE_IDENTITY();", TableName, this.ColumnsText, this.ValuesText);
                    } else {
                        this._baseCommand.CommandText = string.Format(CultureInfo.InvariantCulture, "INSERT INTO {0}({1}) VALUES({2}); SELECT SCOPE_IDENTITY();", TableName, this.ColumnsText, this.ValuesText);
                    }
                } else if (this.Connection.GetType().FullName.Equals("Npgsql.NpgsqlConnection", StringComparison.Ordinal)) {
                    this._baseCommand.CommandText = string.Format(CultureInfo.InvariantCulture, "INSERT INTO {0}({1}) VALUES({2}) RETURNING ID;", TableName, this.ColumnsText, this.ValuesText);
                } else {
                    this._baseCommand.CommandText = string.Format(CultureInfo.InvariantCulture, "INSERT INTO {0}({1}) VALUES({2}); SELECT SCOPE_IDENTITY();", TableName, this.ColumnsText, this.ValuesText);
                }
            } else {
                if ((this.Connection is SqlConnection) && this.NeedsMonoFix) {
                    this._baseCommand.CommandText = string.Format(CultureInfo.InvariantCulture, "SET LANGUAGE us_english; INSERT INTO {0}({1}) VALUES({2});", TableName, this.ColumnsText, this.ValuesText);
                } else {
                    this._baseCommand.CommandText = string.Format(CultureInfo.InvariantCulture, "INSERT INTO {0}({1}) VALUES({2});", TableName, this.ColumnsText, this.ValuesText);
                }
            }
        }


        #region Base properties

        private IDbCommand _baseCommand;
        /// <summary>
        /// Gets underlying connection.
        /// </summary>
        public IDbCommand BaseCommand {
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
        public String CommandText {
            get { return this._baseCommand.CommandText; }
            set { this._baseCommand.CommandText = value; }
        }

        /// <summary>
        /// Gets or sets the wait time before terminating the attempt to execute a command and generating an error.
        /// </summary>
        public Int32 CommandTimeout {
            get { return this._baseCommand.CommandTimeout; }
            set { this._baseCommand.CommandTimeout = value; }
        }

        /// <summary>
        /// Indicates or specifies how the System.Data.IDbCommand.CommandText property is interpreted.
        /// </summary>
        public CommandType CommandType {
            get { return this._baseCommand.CommandType; }
            set { this._baseCommand.CommandType = value; }
        }

        /// <summary>
        /// Gets or sets the System.Data.IDbConnection used by this instance of the System.Data.IDbCommand.
        /// </summary>
        public IDbConnection Connection {
            get { return this._baseCommand.Connection; }
            set { this._baseCommand.Connection = value; }
        }

        /// <summary>
        /// Creates a new instance of an System.Data.IDbDataParameter object.
        /// </summary>
        public IDbDataParameter CreateParameter() {
            return this._baseCommand.CreateParameter();
        }

        /// <summary>
        /// Executes an SQL statement against the Connection object of a .NET Framework data provider, and returns the number of rows affected.
        /// </summary>
        public Int32 ExecuteNonQuery() {
#if DEBUG
            DebugCommand();
#endif
            return this._baseCommand.ExecuteNonQuery();
        }

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection, and builds an System.Data.IDataReader using one of the System.Data.CommandBehavior values.
        /// </summary>
        /// <param name="behavior">One of the System.Data.CommandBehavior values.</param>
        public IDataReader ExecuteReader(CommandBehavior behavior) {
#if DEBUG
            DebugCommand();
#endif
            return this._baseCommand.ExecuteReader(behavior);
        }

        /// <summary>
        /// Executes the System.Data.IDbCommand.CommandText against the System.Data.IDbCommand.Connection and builds an System.Data.IDataReader.
        /// </summary>
        public IDataReader ExecuteReader() {
#if DEBUG
            DebugCommand();
#endif
            return this._baseCommand.ExecuteReader();
        }

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the resultset returned by the query. Extra columns or rows are ignored.
        /// </summary>
        public Object ExecuteScalar() {
#if DEBUG
            DebugCommand();
#endif
            return this._baseCommand.ExecuteScalar();
        }

        /// <summary>
        /// Gets the System.Data.IDataParameterCollection.
        /// </summary>
        public IDataParameterCollection Parameters {
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
        public IDbTransaction Transaction {
            get { return this._baseCommand.Transaction; }
            set { this._baseCommand.Transaction = value; }
        }

        /// <summary>
        /// Gets or sets how command results are applied to the System.Data.DataRow when used by the System.Data.IDataAdapter.Update(System.Data.DataSet) method of a System.Data.Common.DbDataAdapter.
        /// </summary>
        public UpdateRowSource UpdatedRowSource {
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
            GC.SuppressFinalize(this);
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
            var sb = new StringBuilder();
            sb.AppendFormat(CultureInfo.InvariantCulture, "-- {0}", this._baseCommand.CommandText);
            for (int i = 0; i < this._baseCommand.Parameters.Count; ++i) {
                sb.AppendLine();
                var curr = this._baseCommand.Parameters[i] as DbParameter;
                if (curr != null) {
                    sb.AppendFormat(CultureInfo.InvariantCulture, "--     {0}=\"{1}\" ({2})", curr.ParameterName, curr.Value, curr.DbType);
                } else {
                    sb.AppendFormat(CultureInfo.InvariantCulture, "--     {0}", this._baseCommand.Parameters[i].ToString());
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
