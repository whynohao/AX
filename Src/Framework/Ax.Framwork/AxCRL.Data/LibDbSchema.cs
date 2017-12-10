using System;
using System.Data;
using System.Globalization;
using System.Text;

using System.Xml;
using System.Data.Common;
using System.Collections;
using System.Collections.Generic;
using AxCRL.Template;
using AxCRL.Comm;
using AxCRL.Comm.Runtime;
using System.Data.SqlClient;
using AxCRL.Comm.Utils;
using AxCRL.Template.DataSource;



namespace AxCRL.Data
{
    public enum DBObjectType
    {
        Table = 1,
        Column = 2
    }
    public interface ILibDbSchema
    {
        /// <summary>
        /// 对于具有唯一性约束的数据的处理的Sql语句.
        /// Key是数据表名，Value是Sql语句
        /// </summary>
        Dictionary<string,string> DicUniqueDataSql { get; set; }
        void CreateTables(DataSet dataSet);
        void UpdateTables(DataSet dataSet, bool isDelete);

        bool ExistsObject(string name, DBObjectType type);
    }
}
