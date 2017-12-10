using AxCRL.Comm.Runtime;
using AxCRL.Template.DataSource;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using AxCRL.Comm.Utils;

namespace AxCRL.Data
{
    public class LibDataAccess
    {
        private LibDBTransaction _Transaction;
        private static Database dataBase = null;
        private static LibDatabaseType _DatabaseType;

        public LibDatabaseType DatabaseType
        {
            get { return _DatabaseType; }
        }

        public LibDBTransaction Transaction
        {
            get
            {
                if (_Transaction == null)
                    _Transaction = new LibDBTransaction();
                return _Transaction;
            }
            private set { _Transaction = value; }
        }

        public LibDataAccess()
        {
            if (dataBase == null)
            {
                DatabaseProviderFactory factory = new DatabaseProviderFactory(ConfigurationSourceFactory.Create());
                dataBase = factory.Create("DefaultConnection");
                _DatabaseType = dataBase.GetType().Name == "OracleDatabase" ? LibDatabaseType.Oracle : LibDatabaseType.SqlServer;
            }
        }

        ~LibDataAccess()
        {
            CloseConnection();
        }

        private void CloseConnection()
        {
            try
            {
                if (_Transaction != null && _Transaction.SqlTransaction != null && _Transaction.SqlTransaction.Connection != null
                    && _Transaction.SqlTransaction.Connection.State != ConnectionState.Closed)
                {
                    _Transaction.SqlTransaction.Connection.Close();
                    _Transaction.SqlTransaction.Connection.Dispose();
                }
            }
            catch
            {

            }
        }

        public DbConnection CreateConnection()
        {
            return dataBase.CreateConnection();
        }


        public DataSet ExecuteStoredProcedureReturnDataSet(string storedProcedureName, ref Dictionary<string, object> outputData, params object[] parameterValues)
        {
            DataSet ds = null;
            if (outputData == null)
                outputData = new Dictionary<string, object>();
            using (DbCommand command = dataBase.GetStoredProcCommand(storedProcedureName, parameterValues))
            {
                if (this.Transaction.Running)
                    ds = dataBase.ExecuteDataSet(command, this.Transaction.SqlTransaction);
                else
                    ds = dataBase.ExecuteDataSet(command);
                foreach (DbParameter item in command.Parameters)
                {
                    if (item.Direction == ParameterDirection.InputOutput || item.Direction == ParameterDirection.Output)
                    {
                        if (outputData.ContainsKey(item.ParameterName))
                            outputData[item.ParameterName] = item.Value;
                        else
                            outputData.Add(item.ParameterName, item.Value);
                    }
                }
            }
            return ds;
        }

        public int ExecuteStoredProcedure(string storedProcedureName, out Dictionary<string, object> outputData, params object[] parameterValues)
        {
            int rowsAffected = 0;
            outputData = new Dictionary<string, object>();
            using (DbCommand command = dataBase.GetStoredProcCommand(storedProcedureName, parameterValues))
            {
                if (this.Transaction.Running)
                    rowsAffected = dataBase.ExecuteNonQuery(command, this.Transaction.SqlTransaction);
                else
                    rowsAffected = dataBase.ExecuteNonQuery(command);
                foreach (DbParameter item in command.Parameters)
                {
                    if (item.Direction == ParameterDirection.InputOutput || item.Direction == ParameterDirection.Output)
                    {
                        string parameterName = item.ParameterName;
                        if (this.DatabaseType == LibDatabaseType.SqlServer)
                            parameterName = parameterName.Remove(0, 1).ToUpper();
                        outputData.Add(parameterName, item.Value);
                    }
                }
            }
            return rowsAffected;
        }

        public int ExecuteStoredProcedure(string storedProcedureName, params object[] parameterValues)
        {
            if (this.Transaction.Running)
                return dataBase.ExecuteNonQuery(this.Transaction.SqlTransaction, storedProcedureName, parameterValues);
            else
                return dataBase.ExecuteNonQuery(storedProcedureName, parameterValues);
        }

        public int ExecuteNonQuery(string commandText, bool dealwithEmptyString = true)
        {

            if (this.DatabaseType == LibDatabaseType.Oracle && dealwithEmptyString)
                commandText = DealwithEmptyString(commandText);
            if (this.Transaction.Running)
                return dataBase.ExecuteNonQuery(this.Transaction.SqlTransaction, System.Data.CommandType.Text, commandText);
            else
                return dataBase.ExecuteNonQuery(System.Data.CommandType.Text, commandText);

        }

        public int ExecuteNonQuery(List<string> commandTextList, bool dealwithEmptyString = true, int batchNum = 1000)
        {
            if (commandTextList.Count == 0)
                return 0;
            int ret = 0;
            StringBuilder builder = new StringBuilder();
            if (this.DatabaseType == LibDatabaseType.Oracle)
                builder.AppendLine("begin");
            foreach (string commandText in commandTextList)
            {
                if (this.DatabaseType == LibDatabaseType.Oracle)
                {
                    if (dealwithEmptyString)
                        builder.AppendLine(string.Format("{0};", DealwithEmptyString(commandText)));
                    else
                        builder.AppendLine(string.Format("{0};", commandText));
                }
                else
                    builder.AppendLine(commandText);
                batchNum--;
                if (batchNum == 0)
                {
                    if (this.DatabaseType == LibDatabaseType.Oracle)
                        builder.Append("end;");
                    ret += ExecuteNonQuery(builder.ToString(), false);
                    batchNum = 1000;
                    builder.Length = 0;
                    if (this.DatabaseType == LibDatabaseType.Oracle)
                        builder.AppendLine("begin");
                }
            }
            if (builder.Length > 0 && batchNum != 1000)
            {
                if (this.DatabaseType == LibDatabaseType.Oracle)
                    builder.Append("end;");
                ret += ExecuteNonQuery(builder.ToString(), false);
            }
            return ret;
        }

        public int ExecuteNonQuery(string[] commandTexts, bool dealwithEmptyString = true, int batchNum = 1000)
        {
            if (commandTexts.Length == 0)
                return 0;
            int ret = 0;
            StringBuilder builder = new StringBuilder();
            if (this.DatabaseType == LibDatabaseType.Oracle)
                builder.AppendLine("begin");
            foreach (string commandText in commandTexts)
            {
                if (this.DatabaseType == LibDatabaseType.Oracle)
                {
                    if (dealwithEmptyString)
                        builder.AppendLine(string.Format("{0};", DealwithEmptyString(commandText)));
                    else
                        builder.AppendLine(string.Format("{0};", commandText));
                }
                else
                    builder.AppendLine(commandText);
                batchNum--;
                if (batchNum == 0)
                {
                    if (this.DatabaseType == LibDatabaseType.Oracle)
                        builder.Append("end;");
                    ret += ExecuteNonQuery(builder.ToString(), false);
                    batchNum = 1000;
                    builder.Length = 0;
                    if (this.DatabaseType == LibDatabaseType.Oracle)
                        builder.AppendLine("begin");
                }
            }
            if (builder.Length > 0 && batchNum != 1000)
            {
                if (this.DatabaseType == LibDatabaseType.Oracle)
                    builder.Append("end;");
                ret += ExecuteNonQuery(builder.ToString(), false);
            }
            return ret;
        }

        //public IDataReader ExecuteDataReader(IList<string> commandTextList)
        //{
        //    StringBuilder builder = new StringBuilder();
        //    if (this.DatabaseType == LibDatabaseType.Oracle)
        //    {
        //        builder.AppendLine("declare");
        //        for (int i = 0; i < commandTextList.Count; i++)
        //        {
        //            builder.AppendLine(string.Format("cur_out_{0} SYS_REFCURSOR;", i));
        //        }
        //        builder.AppendLine("begin");
        //    }
        //    for (int i = 0; i < commandTextList.Count; i++)
        //    {
        //        string commandText = commandTextList[i];
        //        if (this.DatabaseType == LibDatabaseType.Oracle)
        //            builder.AppendLine(string.Format("open cur_out_{0} for {1};", i, commandText));
        //        else
        //            builder.AppendLine(commandText);
        //    }
        //    if (this.DatabaseType == LibDatabaseType.Oracle)
        //        builder.Append("end;");
        //    return dataBase.ExecuteReader(System.Data.CommandType.Text, builder.ToString());
        //}

        public IDataReader ExecuteDataReader(string commandText, bool dealwithEmptyString = true)
        {
            if (this.DatabaseType == LibDatabaseType.Oracle && dealwithEmptyString)
                commandText = DealwithEmptyString(commandText);
            if (this.Transaction.Running)
                return dataBase.ExecuteReader(this.Transaction.SqlTransaction, CommandType.Text, commandText);
            else
                return dataBase.ExecuteReader(System.Data.CommandType.Text, commandText);
        }

        public object ExecuteScalar(string commandText, bool dealwithEmptyString = true)
        {
            if (this.DatabaseType == LibDatabaseType.Oracle && dealwithEmptyString)
                commandText = DealwithEmptyString(commandText);
            if (this.Transaction.Running)
                return dataBase.ExecuteScalar(this.Transaction.SqlTransaction, CommandType.Text, commandText);
            else
                return dataBase.ExecuteScalar(System.Data.CommandType.Text, commandText);
        }


        public LibDBTransaction BeginTransaction()
        {
            DbConnection conn = dataBase.CreateConnection();
            conn.Open();
            Transaction.BeginTransaction(conn);
            return this.Transaction;
        }


        private void ReadTable(IDataReader reader, DataTable table, int readRowCount = int.MaxValue)
        {
            table.BeginLoadData();
            try
            {
                int thisCount = 0;
                while (reader.Read() && thisCount < readRowCount)
                {
                    DataRow newRow = table.NewRow();
                    newRow.BeginEdit();
                    try
                    {
                        int count = reader.FieldCount;
                        for (int i = 0; i < count; i++)
                        {
                            string name = reader.GetName(i);
                            if (table.Columns.Contains(name))
                            {
                                if (!Convert.IsDBNull(reader[i]))
                                    newRow[name] = reader[i];
                            }
                        }
                    }
                    finally
                    {
                        newRow.EndEdit();
                    }
                    table.Rows.Add(newRow);
                    thisCount++;
                }
            }
            finally
            {
                table.EndLoadData();
            }
        }

        public DataSet ExecuteDataSet(string commandText, bool dealwithEmptyString = true)
        {
            if (this.DatabaseType == LibDatabaseType.Oracle && dealwithEmptyString)
                commandText = DealwithEmptyString(commandText);
            if (this.Transaction.Running)
                return dataBase.ExecuteDataSet(this.Transaction.SqlTransaction, CommandType.Text, commandText);
            else
                return dataBase.ExecuteDataSet(CommandType.Text, commandText);
        }

        public void ExecuteDataTable(string commandText, DataTable table, bool dealwithEmptyString = true, int readRowCount = int.MaxValue)
        {
            if (this.DatabaseType == LibDatabaseType.Oracle && dealwithEmptyString)
                commandText = DealwithEmptyString(commandText);
            using (IDataReader reader = dataBase.ExecuteReader(CommandType.Text, commandText))
            {
                ReadTable(reader, table,readRowCount);
            }
        }

        public void ExecuteDataTables(string commandText, DataSet dataSet, bool dealwithEmptyString = true)
        {
            if (this.DatabaseType == LibDatabaseType.Oracle && dealwithEmptyString)
                commandText = DealwithEmptyString(commandText);
            if (this.Transaction.Running)
            {
                using (IDataReader reader = dataBase.ExecuteReader(this.Transaction.SqlTransaction, CommandType.Text, commandText))
                {
                    int index = 0;
                    do
                    {
                        ReadTable(reader, dataSet.Tables[index]);
                        index++;
                    } while (reader.NextResult());
                }
            }
            else
            {
                using (IDataReader reader = dataBase.ExecuteReader(CommandType.Text, commandText))
                {
                    int index = 0;
                    do
                    {
                        ReadTable(reader, dataSet.Tables[index]);
                        index++;
                    } while (reader.NextResult());
                }
            }
        }
        public void ExecuteDataTables(DataSet dataSet, string storedProcedureName, params object[] parameterValues)
        {
            if (this.Transaction.Running)
            {
                using (IDataReader reader = dataBase.ExecuteReader(this.Transaction.SqlTransaction, storedProcedureName, parameterValues))
                {
                    bool isVirtual = false;
                    int index = 0;
                    do
                    {
                        do
                        {
                            isVirtual = LibSysUtils.ToBoolean(dataSet.Tables[index].ExtendedProperties[TableProperty.IsVirtual]);
                            if (!isVirtual)
                                ReadTable(reader, dataSet.Tables[index]);
                            index++;
                        }
                        while (isVirtual);

                    } while (reader.NextResult());
                }
            }
            else
            {
                using (IDataReader reader = dataBase.ExecuteReader(storedProcedureName, parameterValues))
                {
                    bool isVirtual = false;
                    int index = 0;
                    do
                    {
                        do
                        {
                            isVirtual = LibSysUtils.ToBoolean(dataSet.Tables[index].ExtendedProperties[TableProperty.IsVirtual]);
                            if (!isVirtual)
                                ReadTable(reader, dataSet.Tables[index]);
                            index++;
                        }
                        while (isVirtual);
                    } while (reader.NextResult());
                }
            }
        }

        public void ExecuteDataTables(List<string> commandTextList, DataSet dataSet, bool dealwithEmptyString = true)
        {
            StringBuilder builder = new StringBuilder();
            if (this.DatabaseType == LibDatabaseType.Oracle)
                builder.AppendLine("begin");
            foreach (string commandText in commandTextList)
            {
                if (this.DatabaseType == LibDatabaseType.Oracle)
                {
                    if (dealwithEmptyString)
                        builder.AppendLine(string.Format("{0};", DealwithEmptyString(commandText)));
                    else
                        builder.AppendLine(string.Format("{0};", commandText));
                }
                else
                    builder.AppendLine(commandText);
            }
            if (this.DatabaseType == LibDatabaseType.Oracle)
                builder.Append("end;");
            ExecuteDataTables(builder.ToString(), dataSet, false);
        }

        public void ExecuteDataTables(string[] commandTexts, DataSet dataSet, bool dealwithEmptyString = true)
        {
            StringBuilder builder = new StringBuilder();
            if (this.DatabaseType == LibDatabaseType.Oracle)
                builder.AppendLine("begin");
            foreach (string commandText in commandTexts)
            {
                if (this.DatabaseType == LibDatabaseType.Oracle)
                {
                    if (dealwithEmptyString)
                        builder.AppendLine(string.Format("{0};", DealwithEmptyString(commandText)));
                    else
                        builder.AppendLine(string.Format("{0};", commandText));
                }
                else
                    builder.AppendLine(commandText);
            }
            if (this.DatabaseType == LibDatabaseType.Oracle)
                builder.Append("end;");
            ExecuteDataTables(builder.ToString(), dataSet, false);
        }

        private string DealwithEmptyString(string sql)
        {
            //一次一条sql
            int index = sql.IndexOf("where", 0, sql.Length, StringComparison.CurrentCultureIgnoreCase);
            if (index == -1)
                return sql;
            index += 5; //加入where关键字长度
            StringBuilder builder = new StringBuilder();
            builder.Append(sql.Substring(0, index));
            int length = sql.Length;
            for (int i = index; i < length; i++)
            {
                bool canAdd = true;
                char cur = sql[i];
                if (cur == '=')
                {
                    if (length > i + 1)
                    {
                        for (int j = i + 1; j < length; j++)
                        {
                            char next = sql[j];
                            if (next != ' ' && next != '\'')
                            {
                                break;
                            }
                            else if (next == '\'' && (j + 1) < length && sql[j + 1] == '\'')
                            {
                                canAdd = false;
                                builder.Append(" is null ");
                                i = j + 1;
                                break;
                            }
                        }
                    }
                }
                else if (cur == '<' && (i + 1) < length && sql[i + 1] == '>')
                {
                    if (length > i + 2)
                    {
                        for (int j = i + 2; j < length; j++)
                        {
                            char next = sql[j];
                            if (next != ' ' && next != '\'')
                            {
                                break;
                            }
                            else if (next == '\'' && (j + 1) < length && sql[j + 1] == '\'')
                            {
                                canAdd = false;
                                builder.Append(" is not null ");
                                i = j + 1;
                                break;
                            }
                        }
                    }
                }
                if (canAdd)
                    builder.Append(cur);
            }
            return builder.ToString();
        }

    }

}
