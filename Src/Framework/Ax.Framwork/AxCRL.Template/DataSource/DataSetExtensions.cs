using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCRL.Template.DataSource
{
    /// <summary>
    /// 数据源扩展方法
    /// </summary>
    public static class DataSetExtensions
    {
        public static string[] GetFieldBelongTo(this DataSet dataSource, int tableIndex, string fieldName)
        {
            DataTable table = dataSource.Tables[tableIndex];
            FieldAddr addr = ((Dictionary<string, FieldAddr>)table.ExtendedProperties[TableProperty.FieldAddrDic])[fieldName];
            if (addr.RelSourceIndex == -1)
            {
                return new string[] { table.TableName, table.Columns[addr.FieldIndex].ColumnName };
            }
            else
            {   //为关联字段
                RelativeSource relSource = null;
                if (table.Columns[addr.FieldIndex].ExtendedProperties.ContainsKey(FieldProperty.RelativeSource))
                    relSource = ((RelativeSourceCollection)table.Columns[addr.FieldIndex].ExtendedProperties[FieldProperty.RelativeSource])[addr.RelSourceIndex];
                DataSet relDataSource = LibSqlModelCache.Default.GetSqlModel(relSource.RelSource);
                return GetFieldBelongTo(relDataSource, relSource.TableIndex, relSource.RelFields[addr.RelFieldIndex].Name);
            }
        }

        private static string[] GetPkStr(DataTable table)
        {
            DataColumn[] primaryColumns = table.PrimaryKey;
            string[] ret = new string[primaryColumns.Length];
            for (int i = 0; i < primaryColumns.Length; i++)
            {
                ret[i] = primaryColumns[i].ColumnName;
            }
            return ret;
        }

        public static List<FieldRelation> GetFieldRelation(this DataSet dataSource, int tableIndex, string fieldName, string groupCondition = null, string join = null, string joinTableName = null, List<FieldOwn> joinFields = null)
        {
            List<FieldRelation> list = new List<FieldRelation>();
            DataTable table = dataSource.Tables[tableIndex];
            Dictionary<string, FieldAddr> fieldAddres = (Dictionary<string, FieldAddr>)table.ExtendedProperties[TableProperty.FieldAddrDic];
            if (fieldAddres.ContainsKey(fieldName.Trim()))  //Zhangkj 20170206 增加Trim，避免多余的空格导致的Key不匹配
            {
                FieldAddr addr = fieldAddres[fieldName.Trim()];
                FieldRelation fieldRelation = new FieldRelation(table.TableName, table.Columns[addr.FieldIndex].ColumnName, groupCondition);
                if (string.IsNullOrEmpty(join))
                {
                    fieldRelation.Join = string.Empty;
                    fieldRelation.On = null;
                }
                else
                {
                    fieldRelation.Join = join;
                    if (joinFields.Count == 1)
                    {
                        fieldRelation.On = new List<FieldOwn[]>() { new FieldOwn[] { joinFields[0],
                        new FieldOwn(table.TableName, table.PrimaryKey[0].ColumnName)} };
                    }
                    else
                    {
                        fieldRelation.On = new List<FieldOwn[]>();
                        string[] pks = GetPkStr(table);
                        for (int i = 0; i < pks.Length; i++)
                        {
                            fieldRelation.On.Add(new FieldOwn[] { joinFields[i], new FieldOwn(table.TableName, pks[i]) });
                        }
                    }
                }
                if (addr.RelSourceIndex == -1)
                {
                    list.Add(fieldRelation);
                }
                else
                {   //为关联字段
                    bool isLeftJoin = addr.GroupRelIndexs != null;
                    DoFieldRelation(dataSource, table, list, joinTableName, addr.FieldIndex, addr.RelSourceIndex, addr.RelFieldIndex, isLeftJoin);
                    if (isLeftJoin)
                    {
                        foreach (int[] item in addr.GroupRelIndexs)
                        {
                            DoFieldRelation(dataSource, table, list, joinTableName, addr.FieldIndex, item[0], item[1], isLeftJoin);
                        }
                    }
                }
            }
            return list;
        }

        private static void DoFieldRelation(DataSet dataSource, DataTable table, List<FieldRelation> list, string joinTableName, int fieldIndex, int relSourceIndex, int relFieldIndex, bool isLeftJoin)
        {
            RelativeSource relSource = null;
            if (table.Columns[fieldIndex].ExtendedProperties.ContainsKey(FieldProperty.RelativeSource))
                relSource = ((RelativeSourceCollection)table.Columns[fieldIndex].ExtendedProperties[FieldProperty.RelativeSource])[relSourceIndex];
            string tempJoin = relSource.IsCheckSource && !isLeftJoin ? "Inner Join" : "Left Join";
            List<FieldOwn> tempJoinFields = new List<FieldOwn>();
            if (!string.IsNullOrEmpty(relSource.RelPK))
            {
                string[] relPks = relSource.RelPK.Split(';');
                if (relPks == null)
                {
                    tempJoinFields.Add(ReturnFieldOwn(relSource.RelPK, dataSource));
                }
                else
                {
                    int length = relPks.Length;
                    for (int i = 0; i < length; i++)
                    {
                        tempJoinFields.Add(ReturnFieldOwn(relPks[i], dataSource));
                    }
                }
            }
            tempJoinFields.Add(new FieldOwn(table.TableName, table.Columns[fieldIndex].ColumnName));
            DataSet relDataSource = LibSqlModelCache.Default.GetSqlModel(relSource.RelSource);
            list.AddRange(GetFieldRelation(relDataSource, relSource.TableIndex, relSource.RelFields[relFieldIndex].Name, relSource.GroupCondation, tempJoin, joinTableName, tempJoinFields));
        }

        private static FieldOwn ReturnFieldOwn(string relPk, DataSet dataSource)
        {
            string[] temps = relPk.Split('.');
            int tableIndex = GetTablePrefix(temps[0][0]);
            string[] str = GetFieldBelongTo(dataSource, tableIndex, temps[1]);
            return new FieldOwn(str[0], str[1]);
        }

        private static int GetTablePrefix(char prefix)
        {
            return ((int)prefix - (int)'A');
        }
    }

    public class FieldOwn
    {
        public string Name { get; set; }
        public string TableName { get; set; }
        public FieldOwn(string tableName, string name)
        {
            this.TableName = tableName;
            this.Name = name;
        }
    }

    public class FieldRelation
    {
        /// <summary>
        /// 分组条件
        /// </summary>
        public string GroupCondition { get; set; }
        public string Name { get; set; }
        public string TableName { get; set; }
        public string Join { get; set; }
        public List<FieldOwn[]> On { get; set; }

        public FieldRelation(string tableName, string name, string groupCondition)
        {
            this.TableName = tableName;
            this.Name = name;
            if (!string.IsNullOrEmpty(groupCondition))
                this.GroupCondition = groupCondition.Trim();
        }

        public string GetOnStr()
        {
            if (this.On == null || this.On.Count == 0)
                return string.Empty;
            else
            {
                StringBuilder onBuilder = new StringBuilder();
                foreach (FieldOwn[] fieldOwn in this.On)
                {
                    onBuilder.AppendFormat("{0}.{1}={2}.{3} And ", fieldOwn[1].TableName, fieldOwn[1].Name, fieldOwn[0].TableName, fieldOwn[0].Name);
                }
                onBuilder.Remove(onBuilder.Length - 4, 4);
                return onBuilder.ToString();
            }
        }
    }
}
