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
        /// ���ھ���Ψһ��Լ�������ݵĴ����Sql���.
        /// Key�����ݱ�����Value��Sql���
        /// </summary>
        Dictionary<string,string> DicUniqueDataSql { get; set; }
        void CreateTables(DataSet dataSet);
        void UpdateTables(DataSet dataSet, bool isDelete);

        bool ExistsObject(string name, DBObjectType type);
    }
}
