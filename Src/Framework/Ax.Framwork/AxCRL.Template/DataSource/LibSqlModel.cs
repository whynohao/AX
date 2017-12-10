using AxCRL.Comm.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace AxCRL.Template.DataSource
{
    public class LibSqlModel : DataSet, ILibSerializable
    {
        public void CloneDataSet(DataSet dataSet)
        {
            foreach (DataTable item in dataSet.Tables)
            {
                LibSqlModelTable table = new LibSqlModelTable();
                table.CloneDataTable(item);
                this.Tables.Add(table);
            }
        }

        public void ReadObjectData(LibSerializationInfo info)
        {
            int count = info.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                this.Tables.Add((DataTable)info.ReadObject());
            }
        }

        public void WriteObjectData(LibSerializationInfo info)
        {
            int count = this.Tables.Count;
            info.WriteInt32(count);
            for (int i = 0; i < count; i++)
            {
                info.WriteObject((LibSqlModelTable)this.Tables[i]);
            }
        }
    }

    public class LibSqlModelTable : DataTable, ILibSerializable
    {
        public LibSqlModelTable()
        {

        }

        public void CloneDataTable(DataTable table)
        {
            this.TableName = table.TableName;
            if (table.ExtendedProperties.ContainsKey(TableProperty.FieldAddrDic))
            {
                this.ExtendedProperties.Add(TableProperty.FieldAddrDic, table.ExtendedProperties[TableProperty.FieldAddrDic]);
            }
            if (table.ExtendedProperties.ContainsKey(TableProperty.IsVirtual))
            {
                this.ExtendedProperties.Add(TableProperty.IsVirtual, table.ExtendedProperties[TableProperty.IsVirtual]);
            }
            foreach (DataColumn column in table.Columns)
            {
                LibSqlModelColumn newColumn = new LibSqlModelColumn();
                newColumn.CloneDataColumn(column);
                this.Columns.Add(newColumn);
            }
            DataColumn[] pks = new DataColumn[table.PrimaryKey.Length];
            for (int i = 0; i < table.PrimaryKey.Length; i++)
            {
                pks[i] = this.Columns[table.PrimaryKey[i].ColumnName];
            }
            this.PrimaryKey = pks;
        }

        public void ReadObjectData(LibSerializationInfo info)
        {
            this.TableName = info.ReadString();
            int count = info.ReadInt32();
            Dictionary<string, FieldAddr> fieldAddrDic = new Dictionary<string, FieldAddr>(count);
            for (int i = 0; i < count; i++)
            {
                fieldAddrDic.Add(info.ReadString(), (FieldAddr)info.ReadObject());
            }
            if (fieldAddrDic != null)
                this.ExtendedProperties.Add(TableProperty.FieldAddrDic, fieldAddrDic);
            if (info.ReadBoolean())
                this.ExtendedProperties.Add(TableProperty.IsVirtual, true);
            count = info.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                this.Columns.Add((DataColumn)info.ReadObject());
            }
            count = info.ReadInt32();
            DataColumn[] pks = new DataColumn[count];
            for (int i = 0; i < count; i++)
            {
                pks[i] = this.Columns[info.ReadString()];
            }
            this.PrimaryKey = pks;
        }

        public void WriteObjectData(LibSerializationInfo info)
        {
            info.WriteString(this.TableName);
            Dictionary<string, FieldAddr> fieldAddrDic = this.ExtendedProperties[TableProperty.FieldAddrDic] as Dictionary<string, FieldAddr>;
            if (fieldAddrDic == null)
                info.WriteInt32(0);
            else
            {
                info.WriteInt32(fieldAddrDic.Count);
                foreach (var item in fieldAddrDic)
                {
                    info.WriteString(item.Key);
                    info.WriteObject(item.Value);
                }
            }
            bool isVirtual = false;
            if (this.ExtendedProperties.ContainsKey(TableProperty.IsVirtual))
                isVirtual = (bool)this.ExtendedProperties[TableProperty.IsVirtual];
            info.WriteBoolean(isVirtual);
            int count = this.Columns.Count;
            info.WriteInt32(count);
            for (int i = 0; i < count; i++)
            {
                info.WriteObject((LibSqlModelColumn)this.Columns[i]);
            }
            count = this.PrimaryKey.Length;
            info.WriteInt32(count);
            for (int i = 0; i < count; i++)
            {
                info.WriteString(this.PrimaryKey[i].ColumnName);
            }
        }
    }

    public class LibSqlModelColumn : DataColumn, ILibSerializable
    {

        public void CloneDataColumn(DataColumn column)
        {
            this.ColumnName = column.ColumnName;
            this.MaxLength = column.MaxLength;
            this.DataType = column.DataType;
            this.ExtendedProperties.Add(FieldProperty.DataType, column.ExtendedProperties[FieldProperty.DataType]);
            if (column.ExtendedProperties.ContainsKey(FieldProperty.RelativeSource))
            {
                this.ExtendedProperties.Add(FieldProperty.RelativeSource, column.ExtendedProperties[FieldProperty.RelativeSource]);
            }
            if (column.ExtendedProperties.ContainsKey(FieldProperty.FieldType))
            {
                this.ExtendedProperties.Add(FieldProperty.FieldType, column.ExtendedProperties[FieldProperty.FieldType]);
            }
        }

        public void ReadObjectData(LibSerializationInfo info)
        {
            this.ColumnName = info.ReadString();
            this.MaxLength = info.ReadInt32();
            this.ExtendedProperties.Add(FieldProperty.FieldType, (FieldType)info.ReadInt32());
            LibDataType libDataType = (LibDataType)info.ReadInt32();
            this.ExtendedProperties.Add(FieldProperty.DataType, libDataType);
            int count = info.ReadInt32();
            RelativeSourceCollection relColl = null;
            for (int i = 0; i < count; i++)
            {
                if (relColl == null)
                    relColl = new RelativeSourceCollection();
                relColl.Add((RelativeSource)info.ReadObject());
            }
            this.ExtendedProperties.Add(FieldProperty.RelativeSource, relColl);
            this.DataType = LibDataTypeConverter.ConvertType(libDataType);
        }

        public void WriteObjectData(LibSerializationInfo info)
        {
            info.WriteString(this.ColumnName);
            info.WriteInt32(this.MaxLength);
            FieldType fieldType = FieldType.None;
            if (this.ExtendedProperties.ContainsKey(FieldProperty.FieldType))
                fieldType = (FieldType)this.ExtendedProperties[FieldProperty.FieldType];
            info.WriteInt32((int)fieldType);
            info.WriteInt32((int)this.ExtendedProperties[FieldProperty.DataType]);
            RelativeSourceCollection relColl = this.ExtendedProperties[FieldProperty.RelativeSource] as RelativeSourceCollection;
            if (relColl != null)
            {
                info.WriteInt32(relColl.Count);
                for (int i = 0; i < relColl.Count; i++)
                {
                    info.WriteObject(relColl[i]);
                }
            }
            else
            {
                info.WriteInt32(0);
            }
        }
    }

    /// <summary>
    /// Field 的摘要说明。
    /// </summary>
    [Serializable]
    public class LibField : ISerializable
    {
        private string name = "";
        private DataRow dr = null;
        private string title = "";
        private int index = -1;

        public int Index
        {
            get { return this.index; }
            set { this.index = value; }
        }

        public string Title
        {
            get { return this.title; }
            set { this.title = value; }
        }

        public string FieldName
        {
            get { return this.name; }
            set { this.name = value; }
        }

        public DataRow FieldInfo
        {
            get { return this.dr; }
            set { this.dr = value; }
        }

        public LibField()
        {
            //
            // TODO: 在此处添加构造函数逻辑
            //
        }

        protected LibField(SerializationInfo info, StreamingContext context)//特殊的构造函数，反序列化时自动调用
        {
            this.name = info.GetString("fieldname");
            this.title = info.GetString("fieldtitle");
            this.index = info.GetInt32("fieldindex");
            DataTable dt = info.GetValue("fieldinfo", new DataTable().GetType()) as DataTable;
            this.dr = dt.Rows[0];
        }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)//序列化时自动调用
        {
            info.AddValue("fieldname", this.name);
            info.AddValue("fieldtitle", this.title);
            info.AddValue("fieldindex", this.index);
            DataTable dt = this.dr.Table.Clone(); //datarow不能同时加入到两个DataTable中，必须先克隆一个
            DataRow row = dt.NewRow();
            row.ItemArray = dr.ItemArray;

            dt.Rows.Add(row);
            info.AddValue("fieldinfo", dt, dt.GetType());
        }



        public override string ToString()
        {
            return this.name;
        }

    }

}
