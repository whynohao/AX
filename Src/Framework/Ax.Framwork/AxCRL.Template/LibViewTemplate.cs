using AxCRL.Comm.Utils;
using AxCRL.Template.DataSource;
using AxCRL.Template.Layout;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCRL.Template
{
    public abstract class LibViewTemplate
    {
        private string _ProgId;
        private string _DisplayText;
        private BillType _BillType;

        public string ProgId
        {
            get { return _ProgId; }
            set { _ProgId = value; }
        }

        public BillType BillType
        {
            get { return _BillType; }
            set { _BillType = value; }
        }

        public string DisplayText
        {
            get { return _DisplayText; }
            set { _DisplayText = value; }
        }
        private IViewLayout _Layout = null;

        public IViewLayout Layout
        {
            get { return _Layout; }
            set { _Layout = value; }
        }

        private Dictionary<string, TableDetail> _Tables;

        public Dictionary<string, TableDetail> Tables
        {
            get
            {
                if (_Tables == null)
                    _Tables = new Dictionary<string, TableDetail>();
                return _Tables;
            }
        }

        public LibViewTemplate(DataSet dataSet, IViewLayout layout)
        {
            foreach (DataTable table in dataSet.Tables)
            {
                this.Tables.Add(table.TableName, new TableDetail(table));
            }
            this.Layout = layout;
        }
    }

    public class TableDetail
    {

        private int _ParentIndex;

        public int ParentIndex
        {
            get { return _ParentIndex; }
            set { _ParentIndex = value; }
        }

        private string[] _Pk;

        public string[] Pk
        {
            get { return _Pk; }
            set { _Pk = value; }
        }

        private string _Fields;

        public string Fields
        {
            get { return _Fields; }
            set { _Fields = value; }
        }

        private bool _IsDynamic = false;

        public bool IsDynamic
        {
            get { return _IsDynamic; }
            set { _IsDynamic = value; }
        }

        private string _NewRowObj;

        public string NewRowObj
        {
            get { return _NewRowObj; }
            set { _NewRowObj = value; }
        }
        private bool _UsingRowId = false;

        public bool UsingRowId
        {
            get { return _UsingRowId; }
            set { _UsingRowId = value; }
        }

        private bool _UsingRowNo = false;

        public bool UsingRowNo
        {
            get { return _UsingRowNo; }
            set { _UsingRowNo = value; }
        }

        private bool _UsingApproveRow = false;

        public bool UsingApproveRow
        {
            get { return _UsingApproveRow; }
            set { _UsingApproveRow = value; }
        }

        private bool _UsingAttachment = false;

        public bool UsingAttachment
        {
            get { return _UsingAttachment; }
            set { _UsingAttachment = value; }
        }

        private Dictionary<string, int> _SubTableMap;

        public Dictionary<string, int> SubTableMap
        {
            get
            {
                if (_SubTableMap == null)
                    _SubTableMap = new Dictionary<string, int>();
                return _SubTableMap;
            }
        }

        public TableDetail(DataTable table)
        {
            SetTableInfo(table);
        }

        private void SetTableInfo(DataTable table)
        {
            if (table.ExtendedProperties.ContainsKey(TableProperty.UsingApproveRow))
                UsingApproveRow = (bool)table.ExtendedProperties[TableProperty.UsingApproveRow];
            if (table.ExtendedProperties.ContainsKey(TableProperty.UsingAttachment))
                UsingAttachment = (bool)table.ExtendedProperties[TableProperty.UsingAttachment];
            if (table.ParentRelations != null && table.ParentRelations.Count > 0)
            {
                string parentName = table.ParentRelations[0].ParentTable.TableName;
                for (int i = 0; i < table.DataSet.Tables.Count; i++)
                {
                    if (string.Compare(table.DataSet.Tables[i].TableName, parentName, true) == 0)
                    {
                        this.ParentIndex = i;
                        break;
                    }
                }
            }
            this.Pk = this.GetPk(table);
            StringBuilder builder = new StringBuilder();
            StringBuilder newRowObj = new StringBuilder();
            StringBuilder tempBuilder = new StringBuilder();
            int r = 0;
            foreach (DataColumn item in table.Columns)
            {
                if (!_UsingRowNo && item.ColumnName == "ROWNO")
                    _UsingRowNo = true;
                if (!_UsingRowId && item.ColumnName == "ROW_ID")
                    _UsingRowId = true;
                if (!this.IsDynamic)
                {
                    if (item.ExtendedProperties.ContainsKey(FieldProperty.IsDynamic))
                    {
                        this.IsDynamic = (bool)item.ExtendedProperties[FieldProperty.IsDynamic];
                    }
                }
                if (item.ExtendedProperties.ContainsKey(FieldProperty.SubTableIndex))
                {
                    SubTableMap.Add(item.ColumnName, (int)item.ExtendedProperties[FieldProperty.SubTableIndex]);
                }
                tempBuilder.AppendFormat("name:'{0}'", item.ColumnName);
                LibDataType dateType = (LibDataType)item.ExtendedProperties[FieldProperty.DataType];
                switch (dateType)
                {
                    case LibDataType.Text:
                    case LibDataType.NText:
                        newRowObj.AppendFormat("{0}:'{1}',", item.ColumnName, LibSysUtils.ToString(item.DefaultValue));
                        break;
                    case LibDataType.Int32:
                        tempBuilder.Append(",type:'number'");
                        newRowObj.AppendFormat("{0}:{1},", item.ColumnName, LibSysUtils.ToInt32(item.DefaultValue));
                        break;
                    case LibDataType.Int64:
                        tempBuilder.Append(",type:'number'");
                        newRowObj.AppendFormat("{0}:{1},", item.ColumnName, LibSysUtils.ToInt64(item.DefaultValue));
                        break;
                    case LibDataType.Numeric:
                        tempBuilder.Append(",type:'number'");
                        newRowObj.AppendFormat("{0}:{1},", item.ColumnName, LibSysUtils.ToDecimal(item.DefaultValue));
                        break;
                    case LibDataType.Float:
                        tempBuilder.Append(",type:'number'");
                        newRowObj.AppendFormat("{0}:{1},", item.ColumnName, LibSysUtils.ToSingle(item.DefaultValue));
                        break;
                    case LibDataType.Double:
                        tempBuilder.Append(",type:'number'");
                        newRowObj.AppendFormat("{0}:{1},", item.ColumnName, LibSysUtils.ToDouble(item.DefaultValue));
                        break;
                    case LibDataType.Byte:
                        tempBuilder.Append(",type:'number'");
                        newRowObj.AppendFormat("{0}:{1},", item.ColumnName, LibSysUtils.ToByte(item.DefaultValue));
                        break;
                    case LibDataType.Boolean:
                        tempBuilder.Append(",type:'boolean'");
                        newRowObj.AppendFormat("{0}:{1},", item.ColumnName, LibSysUtils.ToBoolean(item.DefaultValue) ? "true" : "false");
                        break;
                    case LibDataType.Binary:
                        newRowObj.AppendFormat("{0}:'{1}',", item.ColumnName, LibSysUtils.ToString(item.DefaultValue));
                        break;
                }
                if (r == 0)
                    builder.Append("{" + tempBuilder.ToString() + "}");
                else
                    builder.Append(",{" + tempBuilder.ToString() + "}");
                r++;
                tempBuilder.Length = 0;
            }
            newRowObj.Remove(newRowObj.Length - 1, 1);
            this.Fields = string.Format("[{0}]", builder.ToString());
            this.NewRowObj = "{" + newRowObj.ToString() + "}";
        }

        private string[] GetPk(DataTable table)
        {
            int length = table.PrimaryKey.Length;
            string[] ret = new string[length];
            for (int i = 0; i < length; i++)
            {
                ret[i] = table.PrimaryKey[i].ColumnName;
            }
            return ret;
        }
    }
}
