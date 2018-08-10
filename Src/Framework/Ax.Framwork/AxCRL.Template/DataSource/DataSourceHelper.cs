using AxCRL.Comm.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCRL.Template.DataSource
{
    public static class DataSourceHelper
    {
        public static Dictionary<string, FieldAddr> FillFieldAddr(DataTable table)
        {
            Dictionary<string, FieldAddr> dic = new Dictionary<string, FieldAddr>();

            for (int r = 0; r < table.Columns.Count; r++)
            {
                FieldType fieldType = table.Columns[r].ExtendedProperties.ContainsKey(FieldProperty.FieldType) ? (FieldType)table.Columns[r].ExtendedProperties[FieldProperty.FieldType] : FieldType.None;

                if (fieldType == FieldType.None || fieldType == FieldType.Virtual)
                { dic.Add(table.Columns[r].ColumnName, new FieldAddr(r, -1, 0)); }

                RelativeSourceCollection sources = null;

                if (table.Columns[r].ExtendedProperties.ContainsKey(FieldProperty.RelativeSource))
                { sources = (RelativeSourceCollection)table.Columns[r].ExtendedProperties[FieldProperty.RelativeSource]; }

                if (sources != null)
                {
                    for (int j = 0; j < sources.Count; j++)
                    {
                        RelFieldCollection relFields = sources[j].RelFields;

                        for (int k = 0; k < relFields.Count; k++)
                        {
                            string name = string.IsNullOrEmpty(relFields[k].AsName) ? relFields[k].Name : relFields[k].AsName;

                            if (dic.ContainsKey(name))
                            {
                                FieldAddr addr = dic[name];

                                if (addr.GroupRelIndexs == null)
                                { addr.GroupRelIndexs = new List<int[]>(); }

                                addr.GroupRelIndexs.Add(new int[] { j, k });

                            }
                            else
                            { dic.Add(name, new FieldAddr(r, j, k)); }
                        }
                    }
                }
            }

            return dic;
        }


        public static void AddDBIndex(DataTable table, DBIndexCollection dbIndexs)
        {
            table.ExtendedProperties.Add(TableProperty.DBIndex, dbIndexs);
        }

        private static object SetDefaultValue(LibDataType dataType, object defaultValue)
        {
            object ret = defaultValue;

            if (ret == null)
            {
                switch (dataType)
                {
                    case LibDataType.Text:
                    case LibDataType.NText:
                        ret = string.Empty;
                        break;
                    case LibDataType.Int32:
                    case LibDataType.Int64:
                    case LibDataType.Numeric:
                    case LibDataType.Float:
                    case LibDataType.Double:
                    case LibDataType.Byte:
                        ret = 0;
                        break;
                    case LibDataType.Boolean:
                        ret = false;
                        break;
                    case LibDataType.Binary:
                        ret = string.Empty;
                        break;
                    case LibDataType.DateTime:
                        ret = string.Empty;
                        break;
                    case LibDataType.Date:
                        ret = string.Empty;
                        break;
                    case LibDataType.Time:
                        ret = string.Empty;
                        break;
                    default:
                        break;
                }
            }

            return ret;
        }

        public static DataColumn AddRowId(DataTable table, string name = "ROW_ID", string displayName = "行标识")
        {
            DataColumn column = new DataColumn(name, typeof(int));
            column.Caption = displayName;
            column.AllowDBNull = false;
            column.DefaultValue = 1;
            column.ExtendedProperties.Add(FieldProperty.ReadOnly, true);
            column.ExtendedProperties.Add(FieldProperty.DataType, LibDataType.Int32);
            column.ExtendedProperties.Add(FieldProperty.ControlType, LibControlType.Number);
            table.Columns.Add(column);
            return column;
        }

        public static DataColumn AddRowNo(DataTable table, string name = "ROWNO", string displayName = "行号")
        {
            DataColumn column = new DataColumn(name, typeof(int));
            column.Caption = displayName;
            column.AllowDBNull = false;
            column.DefaultValue = 1;
            column.ExtendedProperties.Add(FieldProperty.ReadOnly, true);
            column.ExtendedProperties.Add(FieldProperty.DataType, LibDataType.Int32);
            column.ExtendedProperties.Add(FieldProperty.ControlType, LibControlType.Number);
            column.ExtendedProperties.Add(FieldProperty.QtyLimit, LibQtyLimit.GreaterThanZero);
            table.Columns.Add(column);
            return column;
        }

        public static DataColumn AddBillDate(DataTable table, string name = "BILLDATE", string displayName = "单据日期")
        {
            DataColumn column = new DataColumn(name, typeof(int));
            column.Caption = displayName;
            column.AllowDBNull = false;
            column.DefaultValue = LibDateUtils.DateTimeToLibDate(new DateTime());
            column.ExtendedProperties.Add(FieldProperty.DataType, LibDataType.Int32);
            column.ExtendedProperties.Add(FieldProperty.ControlType, LibControlType.Date);
            column.ExtendedProperties.Add(FieldProperty.AllowCopy, false);
            table.Columns.Add(column);
            return column;
        }

        public static DataColumn AddRemark(DataTable bodyTable, int size = FieldSize.Size200)
        {
            return DataSourceHelper.AddColumn(new DefineField(bodyTable, "REMARK", "备注", size) { DataType = LibDataType.NText });
        }

        public static DataColumn AddDefaultCreateState(DataTable headTable)
        {
            DataColumn column = new DataColumn("DEFAULTCREATESTATE", typeof(int));
            column.Caption = "缺省创建状态";
            column.AllowDBNull = false;
            column.DefaultValue = 0;
            column.ExtendedProperties.Add(FieldProperty.DataType, LibDataType.Int32);
            column.ExtendedProperties.Add(FieldProperty.ControlType, LibControlType.TextOption);
            column.ExtendedProperties.Add(FieldProperty.Option, new string[] { "未生效", "生效" });
            headTable.Columns.Add(column);
            return column;
        }

        public static DataColumn AddAttributeCode(DataTable table, int columnSpan = 1, string name = "ATTRIBUTECODE", string displayName = "特征标识", string attributeId = "ATTRIBUTEID")
        {
            DataColumn column = new DataColumn(name, typeof(string));
            column.Caption = displayName;
            column.AllowDBNull = false;
            column.DefaultValue = string.Empty;
            column.MaxLength = FieldSize.Size100;
            column.ExtendedProperties.Add(FieldProperty.DataType, LibDataType.Text);
            column.ExtendedProperties.Add(FieldProperty.ControlType, LibControlType.AttributeCodeField);
            column.ExtendedProperties.Add(FieldProperty.AttributeField, attributeId);

            if (columnSpan != 1)
            { column.ExtendedProperties.Add(FieldProperty.ColumnSpan, columnSpan); }

            table.Columns.Add(column);
            return column;
        }

        public static DataColumn AddAttributeDesc(DataTable table, int columnSpan = 1, int rowSpan = 1, string name = "ATTRIBUTEDESC", string displayName = "特征描述", string attributeId = "ATTRIBUTEID")
        {
            DataColumn column = new DataColumn(name, typeof(string));
            column.Caption = displayName;
            column.AllowDBNull = false;
            column.DefaultValue = string.Empty;
            column.MaxLength = FieldSize.Size1000;
            column.ExtendedProperties.Add(FieldProperty.DataType, LibDataType.NText);
            column.ExtendedProperties.Add(FieldProperty.ControlType, LibControlType.AttributeDescField);
            column.ExtendedProperties.Add(FieldProperty.AttributeField, attributeId);
            column.ExtendedProperties.Add(FieldProperty.ReadOnly, true);

            if (columnSpan != 1)
            { column.ExtendedProperties.Add(FieldProperty.ColumnSpan, columnSpan); }

            if (rowSpan != 1)
            { column.ExtendedProperties.Add(FieldProperty.RowSpan, rowSpan); }

            table.Columns.Add(column);
            return column;
        }
        /// <summary>
        /// 根据DataColumn的扩展属性将DataColumn转换成DefineField
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public static DefineField ConvertToDefineField(DataColumn column)
        {
            if (column == null)
            { return null; }

            PropertyCollection propertyList = column.ExtendedProperties;//test null
            DefineField defineField = new DefineField();
            defineField.AllowCondition = (propertyList.ContainsKey(FieldProperty.AllowCondition) ? (bool)propertyList[FieldProperty.AllowCondition] : true);
            defineField.AllowCopy = (propertyList.ContainsKey(FieldProperty.AllowCopy) ? (bool)propertyList[FieldProperty.AllowCopy] : true);
            defineField.AllowEmpty = (propertyList.ContainsKey(FieldProperty.AllowEmpty) ? (bool)propertyList[FieldProperty.AllowEmpty] : true);
            defineField.AttributeField = (propertyList.ContainsKey(FieldProperty.AttributeField) ? (string)propertyList[FieldProperty.AttributeField] : null);
            defineField.ColumnSpan = (propertyList.ContainsKey(FieldProperty.ColumnSpan) ? (int)propertyList[FieldProperty.ColumnSpan] : 1);
            defineField.DataType = propertyList.ContainsKey(FieldProperty.DataType) ? (LibDataType)propertyList[FieldProperty.DataType] : LibDataTypeConverter.ConvertToLibType(column.DataType); //无法区分NText、Text、Binary，Type为String都转换为LibDataType.NText
            defineField.DefaultValue = column.DefaultValue;
            defineField.DisplayName = column.Caption;
            defineField.FieldType = (propertyList.ContainsKey(FieldProperty.FieldType) ? (FieldType)propertyList[FieldProperty.FieldType] : FieldType.None);
            defineField.Format = (propertyList.ContainsKey(FieldProperty.Format) ? (string)propertyList[FieldProperty.Format] : null);
            defineField.InputType = (propertyList.ContainsKey(FieldProperty.InputType) ? (InputType)propertyList[FieldProperty.InputType] : InputType.Text);
            defineField.IsDynamic = (propertyList.ContainsKey(FieldProperty.IsDynamic) ? (bool)propertyList[FieldProperty.IsDynamic] : false);
            defineField.KeyValueOption = (propertyList.ContainsKey(FieldProperty.KeyValueOption) ? (LibTextOptionCollection)propertyList[FieldProperty.KeyValueOption] : null);
            defineField.Name = column.ColumnName;
            defineField.Precision = (propertyList.ContainsKey(FieldProperty.Precision) ? (int)propertyList[FieldProperty.Precision] : 2);
            defineField.QtyLimit = (propertyList.ContainsKey(FieldProperty.QtyLimit) ? (LibQtyLimit)propertyList[FieldProperty.QtyLimit] : LibQtyLimit.None);
            defineField.ReadOnly = (propertyList.ContainsKey(FieldProperty.ReadOnly) ? (bool)propertyList[FieldProperty.ReadOnly] : false);
            defineField.RelativeSource = (propertyList.ContainsKey(FieldProperty.RelativeSource) ? (RelativeSourceCollection)propertyList[FieldProperty.RelativeSource] : null);
            defineField.RelProgId = (propertyList.ContainsKey(FieldProperty.RelProgId) ? (string)propertyList[FieldProperty.RelProgId] : null);
            defineField.RelTableIndex = (propertyList.ContainsKey(FieldProperty.RelTableIndex) ? (string)propertyList[FieldProperty.RelTableIndex] : null);
            defineField.RowSpan = (propertyList.ContainsKey(FieldProperty.RowSpan) ? (int)propertyList[FieldProperty.RowSpan] : 1);

            if (column.MaxLength <= 0) //如果未设置，MaxLength属性默认为-1
            { defineField.Size = 0; }

            else
            { defineField.Size = column.MaxLength; }

            defineField.SubTableIndex = (propertyList.ContainsKey(FieldProperty.SubTableIndex) ? (int)propertyList[FieldProperty.SubTableIndex] : 0);
            defineField.Summary = (propertyList.ContainsKey(FieldProperty.Summary) ? (LibSummary)propertyList[FieldProperty.Summary] : LibSummary.None);
            defineField.SummaryRenderer = (propertyList.ContainsKey(FieldProperty.SummaryRenderer) ? (string)propertyList[FieldProperty.SummaryRenderer] : null);
            defineField.Table = null;//不设置DataTable引用
            defineField.TextOption = (propertyList.ContainsKey(FieldProperty.Option) ? (string[])propertyList[FieldProperty.Option] : null);

            //Zhangkj 20170227 增加步进值设置
            if (propertyList.ContainsKey(FieldProperty.StepValue))
            {
                double stepValue = 1.0;

                if (double.TryParse(propertyList[FieldProperty.StepValue].ToString(), out stepValue))
                {
                    defineField.StepValue = stepValue;
                }
            }

            if (propertyList.ContainsKey(FieldProperty.FontName))
            {
                string fontName = propertyList[FieldProperty.FontName].ToString();

                if (string.IsNullOrEmpty(fontName))
                {
                    defineField.FontName = fontName;
                }
            }

            if (propertyList.ContainsKey(FieldProperty.ControlType))
            { defineField.ControlType = (LibControlType)propertyList[FieldProperty.ControlType]; } //因设置其他值时会修改ControlType，所以ControlType放到最后

            return defineField;
        }
        public static DataColumn AddColumn(DefineField defineField)
        {
            DataColumn column = new DataColumn(defineField.Name, LibDataTypeConverter.ConvertType(defineField.DataType));
            column.Caption = defineField.DisplayName;

            if (defineField.Size != 0)
            { column.MaxLength = defineField.Size; }

            column.AllowDBNull = false;

            if (!defineField.AllowEmpty)
            { column.ExtendedProperties.Add(FieldProperty.AllowEmpty, false); }

            if (defineField.ReadOnly)
            { column.ExtendedProperties.Add(FieldProperty.ReadOnly, defineField.ReadOnly); }

            if (!defineField.AllowCopy)
            { column.ExtendedProperties.Add(FieldProperty.AllowCopy, false); }

            if (!defineField.AllowCondition)
            { column.ExtendedProperties.Add(FieldProperty.AllowCondition, false); }

            if (defineField.SubTableIndex > 0)
            { column.ExtendedProperties.Add(FieldProperty.SubTableIndex, defineField.SubTableIndex); }

            if (defineField.ColumnSpan > 1)
            { column.ExtendedProperties.Add(FieldProperty.ColumnSpan, defineField.ColumnSpan); }

            if (defineField.RowSpan > 1)
            { column.ExtendedProperties.Add(FieldProperty.RowSpan, defineField.RowSpan); }

            if (defineField.QtyLimit != LibQtyLimit.None)
            { column.ExtendedProperties.Add(FieldProperty.QtyLimit, defineField.QtyLimit); }

            if (!string.IsNullOrEmpty(defineField.RelProgId))
            {
                column.ExtendedProperties.Add(FieldProperty.RelProgId, defineField.RelProgId);
                column.ExtendedProperties.Add(FieldProperty.RelTableIndex, defineField.RelTableIndex);
            }

            if (defineField.IsDynamic)
            { column.ExtendedProperties.Add(FieldProperty.IsDynamic, true); }

            if (defineField.Summary != LibSummary.None)
            { column.ExtendedProperties.Add(FieldProperty.Summary, defineField.Summary); }

            if (!string.IsNullOrEmpty(defineField.SummaryRenderer))
            { column.ExtendedProperties.Add(FieldProperty.SummaryRenderer, defineField.SummaryRenderer); }

            if (!string.IsNullOrEmpty(defineField.AttributeField))
            { column.ExtendedProperties.Add(FieldProperty.AttributeField, defineField.AttributeField); }

            if (defineField.InputType != InputType.Text)
            { column.ExtendedProperties.Add(FieldProperty.InputType, defineField.InputType); }

            column.DefaultValue = SetDefaultValue(defineField.DataType, defineField.DefaultValue);
            column.ExtendedProperties.Add(FieldProperty.DataType, defineField.DataType);
            column.ExtendedProperties.Add(FieldProperty.ControlType, defineField.ControlType);
            column.ExtendedProperties.Add(FieldProperty.SelectSql, defineField.SelectSql);
            column.ExtendedProperties.Add(FieldProperty.SelectFields, defineField.SelectFields);

            if (defineField.TextOption != null && defineField.TextOption.Length > 0)
            { column.ExtendedProperties.Add(FieldProperty.Option, defineField.TextOption); }

            if (defineField.KeyValueOption != null && defineField.KeyValueOption.Count > 0)
            { column.ExtendedProperties.Add(FieldProperty.KeyValueOption, defineField.KeyValueOption); }

            if (defineField.FieldType != FieldType.None)
            { column.ExtendedProperties.Add(FieldProperty.FieldType, defineField.FieldType); }

            if (!string.IsNullOrEmpty(defineField.Format))
            { column.ExtendedProperties.Add(FieldProperty.Format, defineField.Format); }

            if (defineField.Precision != 2)
            { column.ExtendedProperties.Add(FieldProperty.Precision, defineField.Precision); }

            //Zhangkj 20170227
            if (defineField.StepValue != 1.0)
            { column.ExtendedProperties.Add(FieldProperty.StepValue, defineField.StepValue); }

            if (string.IsNullOrEmpty(defineField.FontName) == false)
            { column.ExtendedProperties.Add(FieldProperty.FontName, defineField.FontName); }

            bool hasRel = defineField.RelativeSource != null && defineField.RelativeSource.Count > 0;

            if (hasRel)
            { column.ExtendedProperties.Add(FieldProperty.RelativeSource, defineField.RelativeSource); }

            defineField.Table.Columns.Add(column);

            if (hasRel)
            {
                HashSet<string> hashSet = new HashSet<string>();

                foreach (var item in defineField.RelativeSource)
                {
                    foreach (var subItem in item.RelFields)
                    {
                        string name = string.IsNullOrEmpty(subItem.AsName) ? subItem.Name : subItem.AsName;

                        if (!hashSet.Contains(name))
                        {
                            hashSet.Add(name);
                            AddRelatveColumn(defineField.Table, name, subItem.DataType, subItem.Size, subItem.DisplayText, defineField.AllowCopy, subItem.AllowCondition, subItem.TextOption, subItem.KeyValueOption, subItem.ColumnSpan, subItem.RowSpan, subItem.Format, subItem.Precision, subItem.ControlType, subItem.Summary, subItem.SummaryRenderer, subItem.InputType, subItem.AttributeField);
                        }
                    }
                }
            }

            if (defineField.GridAttribute != null)
            { column.ExtendedProperties.Add(FieldProperty.GridAttribute, defineField.GridAttribute); }

            return column;
        }

        private static DataColumn AddRelatveColumn(DataTable table, string name, LibDataType dataType, int maxLength, string displayName, bool allowCopy, bool allowConditon, string[] textOption, LibTextOptionCollection keyValueOption,
                int columnSpan, int rowSpan, string format = null, int precision = 2, LibControlType controlType = LibControlType.NText, LibSummary summary = LibSummary.None, string summaryRenderer = "", InputType inputType = InputType.Text, string attributeField = "")
        {
            DataColumn column = new DataColumn(name, LibDataTypeConverter.ConvertType(dataType));
            column.Caption = displayName;

            if (dataType == LibDataType.Text || dataType == LibDataType.NText)
            { column.MaxLength = maxLength; }

            column.AllowDBNull = false;
            column.DefaultValue = SetDefaultValue(dataType, null);
            column.ExtendedProperties.Add(FieldProperty.ReadOnly, true);
            column.ExtendedProperties.Add(FieldProperty.DataType, dataType);
            column.ExtendedProperties.Add(FieldProperty.FieldType, FieldType.Relative);
            column.ExtendedProperties.Add(FieldProperty.ControlType, controlType);

            if (columnSpan > 1)
            { column.ExtendedProperties.Add(FieldProperty.ColumnSpan, columnSpan); }

            if (rowSpan > 1)
            { column.ExtendedProperties.Add(FieldProperty.RowSpan, rowSpan); }

            if (!string.IsNullOrEmpty(format))
            { column.ExtendedProperties.Add(FieldProperty.Format, format); }

            if (precision != 2)
            { column.ExtendedProperties.Add(FieldProperty.Precision, precision); }

            if (textOption != null && textOption.Length > 0)
            { column.ExtendedProperties.Add(FieldProperty.Option, textOption); }

            if (keyValueOption != null && keyValueOption.Count > 0)
            { column.ExtendedProperties.Add(FieldProperty.KeyValueOption, keyValueOption); }

            if (!allowCopy)
            { column.ExtendedProperties.Add(FieldProperty.AllowCopy, allowCopy); }

            if (!allowConditon)
            { column.ExtendedProperties.Add(FieldProperty.AllowCondition, allowConditon); }

            if (summary != LibSummary.None)
            { column.ExtendedProperties.Add(FieldProperty.Summary, summary); }

            if (!string.IsNullOrEmpty(summaryRenderer))
            { column.ExtendedProperties.Add(FieldProperty.SummaryRenderer, summaryRenderer); }

            if (!string.IsNullOrEmpty(attributeField))
            { column.ExtendedProperties.Add(FieldProperty.AttributeField, attributeField); }

            if (inputType != InputType.Text)
            { column.ExtendedProperties.Add(FieldProperty.InputType, inputType); }

            table.Columns.Add(column);
            return column;
        }

        public static void AddRelSource(DataColumn column, RelativeSourceCollection relSourceCollection)
        {
            column.ExtendedProperties.Add(FieldProperty.RelativeSource, relSourceCollection);
            bool allowCopy = true;

            if (column.ExtendedProperties.ContainsKey(FieldProperty.AllowCopy) && (bool)column.ExtendedProperties[FieldProperty.AllowCopy] == false)
            { allowCopy = false; }

            foreach (var item in relSourceCollection)
            {
                foreach (var subItem in item.RelFields)
                {
                    string name = string.IsNullOrEmpty(subItem.AsName) ? subItem.Name : subItem.AsName;
                    AddRelatveColumn(column.Table, name, subItem.DataType, subItem.Size, subItem.DisplayText, allowCopy, subItem.AllowCondition, subItem.TextOption, subItem.KeyValueOption, subItem.ColumnSpan, subItem.RowSpan, subItem.Format, subItem.Precision, subItem.ControlType, subItem.Summary, subItem.SummaryRenderer, subItem.InputType, subItem.AttributeField);
                }
            }
        }

        private static void AddPersonRelsource(DataColumn column, string asName, string relDisplayText)
        {
            RelativeSource relSource = new RelativeSource("com.Person");
            relSource.RelFields = new RelFieldCollection() { new RelField("PERSONNAME", LibDataType.Text, FieldSize.Size50, relDisplayText, asName) };
            DataSourceHelper.AddRelSource(column, new RelativeSourceCollection() { relSource });
        }

        public static void AddFixColumn(DataTable table, BillType billType)
        {
            //创建人,创建时间,审核人,审核时间,最后修改人,最后修改时间,被用,内码,备注
            DataColumn column = DataSourceHelper.AddColumn(new DefineField(table, "CreatorId", "创建人", FieldSize.Size20) { AllowCopy = false, ReadOnly = true });
            AddPersonRelsource(column, "CreatorName", "创建人名称");
            DataSourceHelper.AddColumn(new DefineField(table, "CreateTime", "创建时间") { DataType = LibDataType.DateTime, ControlType = LibControlType.DateTime, AllowCopy = false, ReadOnly = true });
            DataSourceHelper.AddColumn(new DefineField(table, "AuditState", "审核状态") { DataType = LibDataType.Int32, ControlType = LibControlType.Text, AllowCopy = false, ReadOnly = true, TextOption = new string[] { "未提交", "已提交", "已审核", "未通过" } });
            //Zhangkj 20170609增加提交审核时间
            DataSourceHelper.AddColumn(new DefineField(table, "SummitAuditTime", "提交审核时间") { DataType = LibDataType.Int64, AllowCopy = false, ControlType = LibControlType.DateTime, ReadOnly = true });
            DataSourceHelper.AddColumn(new DefineField(table, "FlowLevel", "审核层级") { DataType = LibDataType.Int32, ControlType = LibControlType.Number, AllowCopy = false, ReadOnly = true });
            column = DataSourceHelper.AddColumn(new DefineField(table, "ApprovrId", "审核人", FieldSize.Size20) { AllowCopy = false, ReadOnly = true });
            AddPersonRelsource(column, "ApprovrName", "审核人名称");
            DataSourceHelper.AddColumn(new DefineField(table, "ApprovalTime", "审核时间") { DataType = LibDataType.Int64, AllowCopy = false, ControlType = LibControlType.DateTime, ReadOnly = true });
            column = DataSourceHelper.AddColumn(new DefineField(table, "LastUpdateId", "最后修改人", FieldSize.Size20) { AllowCopy = false, ReadOnly = true });
            AddPersonRelsource(column, "LastUpdateName", "最后修改人名称");
            DataSourceHelper.AddColumn(new DefineField(table, "LASTUPDATETIME", "最后修改时间") { DataType = LibDataType.Int64, AllowCopy = false, ControlType = LibControlType.DateTime, ReadOnly = true });
            DataSourceHelper.AddColumn(new DefineField(table, "ISUSED", "被用标识") { DataType = LibDataType.Boolean, AllowCopy = false, ReadOnly = true });
            DataSourceHelper.AddColumn(new DefineField(table, "INTERNALID", "内码", FieldSize.Size50) { AllowCopy = false, ReadOnly = true, AllowEmpty = false });
            DataSourceHelper.AddColumn(new DefineField(table, "REMARK", "备注", FieldSize.Size500) { DataType = LibDataType.NText });

            if (billType == BillType.Bill)
            {
                //结案人,结案时间,单据状态(草稿、未生效、生效、作废、结案)
                column = DataSourceHelper.AddColumn(new DefineField(table, "ENDCASEID", "结案人", FieldSize.Size20) { AllowCopy = false, ReadOnly = true });
                AddPersonRelsource(column, "ENDCASENAME", "结案人名称");
                DataSourceHelper.AddColumn(new DefineField(table, "ENDCASETIME", "结案时间") { DataType = LibDataType.Int64, AllowCopy = false, ControlType = LibControlType.DateTime, ReadOnly = true });
                DataSourceHelper.AddColumn(new DefineField(table, "CURRENTSTATE", "单据状态") { DataType = LibDataType.Int32, AllowCopy = false, TextOption = new string[] { "草稿", "未生效", "生效", "作废", "结案" }, ReadOnly = true });

            }
            else
                if (billType == BillType.Master)
                {
                    //状态(草稿、未生效、生效)
                    DataSourceHelper.AddColumn(new DefineField(table, "CURRENTSTATE", "资料状态") { DataType = LibDataType.Int32, AllowCopy = false, TextOption = new string[] { "草稿", "未生效", "生效" }, ReadOnly = true });
                    //有效期
                    DataSourceHelper.AddColumn(new DefineField(table, "VALIDITYSTARTDATE", "有效期从") { DataType = LibDataType.Int32, ControlType = LibControlType.Date });
                    DataSourceHelper.AddColumn(new DefineField(table, "VALIDITYENDDATE", "有效期至") { DataType = LibDataType.Int32, ControlType = LibControlType.Date });
                    DataSourceHelper.AddColumn(new DefineField(table, "ISVALIDITY", "是否有效") { DataType = LibDataType.Boolean, ControlType = LibControlType.YesNo, ReadOnly = true, DefaultValue = true, AllowCopy = false });
                }
        }

        public static void AddApproveRowFixColumn(DataTable bodyTable)
        {
            if (bodyTable.Columns.Contains("AUDITSTATE") == false)
            {
                DataSourceHelper.AddColumn(new DefineField(bodyTable, "AUDITSTATE", "审核状态") { DataType = LibDataType.Int32, ControlType = LibControlType.Text, AllowCopy = false, ReadOnly = true, TextOption = new string[] { "未提交", "已提交", "已审核", "未通过" } });
                DataSourceHelper.AddColumn(new DefineField(bodyTable, "FLOWLEVEL", "审核层级") { DataType = LibDataType.Int32, ControlType = LibControlType.Number, AllowCopy = false, ReadOnly = true });
            }

            if (bodyTable.Columns.Contains("SUMMITAUDITTIME") == false)
            {
                //Zhangkj 20170609增加行项提交审核时间等
                DataSourceHelper.AddColumn(new DefineField(bodyTable, "SUMMITAUDITTIME", "提交审核时间") { DataType = LibDataType.Int64, AllowCopy = false, ControlType = LibControlType.DateTime, ReadOnly = true });
                DataColumn column = DataSourceHelper.AddColumn(new DefineField(bodyTable, "APPROVRID", "审核人", FieldSize.Size20) { AllowCopy = false, ReadOnly = true });
                AddPersonRelsource(column, "APPROVRNAME", "审核人名称");
                DataSourceHelper.AddColumn(new DefineField(bodyTable, "APPROVALTIME", "审核时间") { DataType = LibDataType.Int64, AllowCopy = false, ControlType = LibControlType.DateTime, ReadOnly = true });
            }
        }

        public static void AddImgSrcColumn(DataTable headTable)
        {
            DataSourceHelper.AddColumn(new DefineField(headTable, "IMGSRC", "图片信息", FieldSize.Size100) { DataType = LibDataType.Text, ReadOnly = true });
        }

        public static void AddAttachmentSrcColumn(DataTable table)
        {
            DataSourceHelper.AddColumn(new DefineField(table, "ATTACHMENTSRC", "附件", FieldSize.Size50) { DataType = LibDataType.Text, ReadOnly = true, AllowCondition = false });
        }
        /// <summary>
        /// 添加数据同步配置信息数据表
        /// </summary>
        /// <param name="dataSet"></param>
        /// <param name="tableName"></param>
        public static DataTable AddSyncDataSettingTable(DataSet dataSet, string tableName)
        {
            if (dataSet == null)
            { return null; }

            DataTable masterTable = new DataTable(tableName);
            DataSourceHelper.AddColumn(new DefineField(masterTable, "SETTINGID", "同步配置索引", FieldSize.Size50) { AllowCopy = false, ReadOnly = true, AllowEmpty = false }); //作为唯一索引使用
            DataSourceHelper.AddColumn(new DefineField(masterTable, "PROGID", "功能代码", FieldSize.Size50)
            {
                AllowEmpty = false,
                ReadOnly = true,
                ControlType = LibControlType.IdName,
                RelativeSource = new RelativeSourceCollection() {
                    new RelativeSource ("axp.FuncList") {
                        RelFields = new RelFieldCollection() {
                            new RelField ("PROGNAME", LibDataType.NText, FieldSize.Size50, "功能名称")
                        }
                    }
                }
            });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "USERID", "用户账户", FieldSize.Size20) { ReadOnly = true });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "ISSYNCTO", "同步至") { DataType = LibDataType.Boolean, ControlType = LibControlType.YesNo, DefaultValue = false });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "SITEID", "站点", FieldSize.Size20)
            {
                ReadOnly = true,
                RelativeSource = new RelativeSourceCollection() {
                    new RelativeSource ("axp.LinkSite") {
                        RelFields = new RelFieldCollection() {
                            new RelField ("SHORTNAME", LibDataType.NText, FieldSize.Size100, "站点名称")
                        }
                    }
                }
            });
            masterTable.PrimaryKey = new DataColumn[] { masterTable.Columns["SETTINGID"] };
            dataSet.Tables.Add(masterTable);
            return masterTable;
        }
        /// <summary>
        /// 添加数据同步历史信息数据表
        /// </summary>
        /// <param name="dataSet"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static DataTable AddSyncDataHistoryTable(DataSet dataSet, string tableName)
        {
            if (dataSet == null)
            { return null; }

            DataTable masterTable = new DataTable(tableName);
            DataSourceHelper.AddColumn(new DefineField(masterTable, "INFOID", "同步信息索引", FieldSize.Size50) { AllowCopy = false, ReadOnly = true, AllowEmpty = false }); //作为唯一索引使用
            DataSourceHelper.AddColumn(new DefineField(masterTable, "PROGID", "功能代码", FieldSize.Size50)
            {
                AllowEmpty = false,
                ReadOnly = true,
                ControlType = LibControlType.IdName,
                RelativeSource = new RelativeSourceCollection() {
                    new RelativeSource ("axp.FuncList") {
                        RelFields = new RelFieldCollection() {
                            new RelField ("PROGNAME", LibDataType.NText, FieldSize.Size50, "功能名称")
                        }
                    }
                }
            });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "INTERNALID", "单据内码", FieldSize.Size50) { ReadOnly = true });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "BILLNO", "单据编号", FieldSize.Size100) { ReadOnly = true });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "USERID", "用户账户", FieldSize.Size20) { ReadOnly = true });
            //人员名称虚字段
            DataSourceHelper.AddColumn(new DefineField(masterTable, "PERSONNAME", "人员", FieldSize.Size200) { ReadOnly = true, DataType = LibDataType.NText, ControlType = LibControlType.NText, FieldType = FieldType.Virtual });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "SITEID", "站点", FieldSize.Size20)
            {
                ReadOnly = true,
                RelativeSource = new RelativeSourceCollection() {
                    new RelativeSource ("axp.LinkSite") {
                        RelFields = new RelFieldCollection() {
                            new RelField ("SHORTNAME", LibDataType.NText, FieldSize.Size100, "站点名称")
                        }
                    }
                }
            });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "SYNCTIME", "同步时间") { DataType = LibDataType.Int64, AllowCopy = false, ControlType = LibControlType.DateTime, ReadOnly = true });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "SYNCOP", "同步操作") { DataType = LibDataType.Int32, ControlType = LibControlType.TextOption, TextOption = new string[] { "新增", "修改", "删除" }, ReadOnly = true });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "SYNCSTATE", "同步状态") { DataType = LibDataType.Int32, ControlType = LibControlType.TextOption, TextOption = new string[] { "未同步", "同步异常", "已同步" }, ReadOnly = true });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "SYNCINFO", "同步信息", FieldSize.Size1000) { DataType = LibDataType.NText, ControlType = LibControlType.NText, ReadOnly = true });
            masterTable.PrimaryKey = new DataColumn[] { masterTable.Columns["INFOID"] };
            dataSet.Tables.Add(masterTable);
            return masterTable;
        }
    }
    [Serializable]
    public class LibTextOption
    {
        private string _Key;
        private string _Value;

        public LibTextOption()
        {
        }

        public LibTextOption(string key, string value)
        {
            this._Key = key;
            this._Value = value;
        }

        public string Value
        {
            get { return _Value; }

            set { _Value = value; }
        }

        public string Key
        {
            get { return _Key; }

            set { _Key = value; }
        }
    }
    [Serializable]
    public class LibTextOptionCollection : Collection<LibTextOption>
    {

    }

    public class DefineField
    {
        private DataTable _Table;

        public DataTable Table
        {
            get { return _Table; }

            set { _Table = value; }
        }
        private string _Name;

        public string Name
        {
            get { return _Name; }

            set { _Name = value; }
        }
        private LibDataType _DataType = LibDataType.Text;

        public LibDataType DataType
        {
            get { return _DataType; }

            set
            {
                _DataType = value;

                switch (DataType)
                { //预设默认控件
                    case LibDataType.Text:
                        this.ControlType = LibControlType.Text;
                        break;

                    case LibDataType.NText:
                        this.ControlType = LibControlType.NText;
                        break;

                    case LibDataType.Boolean:
                        this.ControlType = LibControlType.YesNo;
                        break;
                }
            }
        }
        private int _Size;

        public int Size
        {
            get { return _Size; }

            set { _Size = value; }
        }
        private string _DisplayName;

        public string DisplayName
        {
            get { return _DisplayName; }

            set { _DisplayName = value; }
        }
        private LibControlType _ControlType = LibControlType.Text;

        public LibControlType ControlType
        {
            get { return _ControlType; }

            set { _ControlType = value; }
        }
        private object _DefaultValue = null;

        public object DefaultValue
        {
            get { return _DefaultValue; }

            set { _DefaultValue = value; }
        }
        private bool _ReadOnly = false;

        public bool ReadOnly
        {
            get { return _ReadOnly; }

            set { _ReadOnly = value; }
        }
        private FieldType _FieldType = FieldType.None;

        public FieldType FieldType
        {
            get { return _FieldType; }

            set { _FieldType = value; }
        }
        private string[] _TextOption = null;

        public string[] TextOption
        {
            get { return _TextOption; }

            set
            {
                _TextOption = value;

                if (_TextOption != null)
                { this._ControlType = LibControlType.TextOption; }
            }
        }

        private LibTextOptionCollection _KeyValueOption;

        public LibTextOptionCollection KeyValueOption
        {
            get { return _KeyValueOption; }

            set
            {
                _KeyValueOption = value;

                if (_KeyValueOption != null)
                { this._ControlType = LibControlType.KeyValueOption; }
            }
        }

        private bool _AllowEmpty = true;

        public bool AllowEmpty
        {
            get { return _AllowEmpty; }

            set { _AllowEmpty = value; }
        }
        private string _Format = null;

        public string Format
        {
            get { return _Format; }

            set { _Format = value; }
        }

        private int _Precision = 2;

        public int Precision
        {
            get { return _Precision; }

            set { _Precision = value; }
        }

        private bool _AllowCopy = true;

        public bool AllowCopy
        {
            get { return _AllowCopy; }

            set { _AllowCopy = value; }
        }

        private bool _AllowCondition = true;

        public bool AllowCondition
        {
            get { return _AllowCondition; }

            set { _AllowCondition = value; }
        }


        private RelativeSourceCollection _RelativeSource = null;

        public RelativeSourceCollection RelativeSource
        {
            get { return _RelativeSource; }

            set
            {
                _RelativeSource = value;

                if (_RelativeSource != null && _RelativeSource.Count > 0)
                {
                    if (this.ControlType != LibControlType.Id)
                    {
                        this.ControlType = LibControlType.IdName;
                    }
                }
            }
        }

        #region 搜索框的查询

        #region 查询语句
        private string _SelectSql = string.Empty;

        public string SelectSql
        {
            get { return _SelectSql; }
            set { _SelectSql = value; }
        }
        #endregion

        #region 查询字段，多个字段间用逗号(,)分隔
        private string _SelectFields = string.Empty;

        public string SelectFields
        {
            get { return _SelectFields; }
            set { _SelectFields = value; }
        }
        #endregion

        #endregion

        private int _SubTableIndex;

        public int SubTableIndex
        {
            get { return _SubTableIndex; }

            set { _SubTableIndex = value; }
        }

        private int _ColumnSpan = 1;

        public int ColumnSpan
        {
            get { return _ColumnSpan; }

            set { _ColumnSpan = value; }
        }

        private int _RowSpan = 1;

        public int RowSpan
        {
            get { return _RowSpan; }

            set { _RowSpan = value; }
        }

        private string _RelProgId;

        public string RelProgId
        {
            get { return _RelProgId; }

            set { _RelProgId = value; }
        }

        private string _RelTableIndex;

        public string RelTableIndex
        {
            get { return _RelTableIndex; }

            set { _RelTableIndex = value; }
        }

        private LibQtyLimit _QtyLimit = LibQtyLimit.None;

        public LibQtyLimit QtyLimit
        {
            get { return _QtyLimit; }

            set { _QtyLimit = value; }
        }

        private bool _IsDynamic = false;

        public bool IsDynamic
        {
            get { return _IsDynamic; }

            set { _IsDynamic = value; }
        }

        private LibSummary _Summary = LibSummary.None;

        public LibSummary Summary
        {
            get { return _Summary; }

            set { _Summary = value; }
        }

        private string _SummaryRenderer;

        public string SummaryRenderer
        {
            get { return _SummaryRenderer; }

            set { _SummaryRenderer = value; }
        }

        private string _AttributeField;

        public string AttributeField
        {
            get { return _AttributeField; }

            set { _AttributeField = value; }
        }

        private InputType _InputType = InputType.Text;

        public InputType InputType
        {
            get { return _InputType; }

            set { _InputType = value; }
        }

        private double _StepValue = 1.0;
        /// <summary>
        /// 步进值设置项，默认为1.0
        /// </summary>
        public double StepValue
        {
            get { return _StepValue; }

            set { _StepValue = value; }
        }
        /// <summary>
        /// 字段的特殊字体设置
        /// </summary>
        public string FontName { get; set; }

        public DefineField()
        {
        }

        public DefineField(DataTable table, string name, string displayName)
        {
            this._Table = table;
            this._Name = name;
            this._DisplayName = displayName;
        }

        public DefineField(DataTable table, string name, string displayName, int size)
        {
            this._Table = table;
            this._Name = name;
            this._DisplayName = displayName;
            this._Size = size;
        }

        public DefineField(DefineFieldDic fieldDic)
        {
            this._Name = fieldDic.Name;
            this._DisplayName = fieldDic.DisplayName;
            this._Size = fieldDic.Size;
            this._DataType = fieldDic.DataType;
            this._ControlType = fieldDic.ControlType;
            this._TextOption = fieldDic.TextOption;
            this._KeyValueOption = fieldDic.KeyValueOption;
            this._Precision = fieldDic.Precision;
        }


        private GridAttribute _GridAttribute;
        public GridAttribute GridAttribute
        {
            get { return _GridAttribute; }

            set { _GridAttribute = value; }
        }
    }

    public class DefineFieldDic
    {
        private string _Name;
        private LibDataType _DataType = LibDataType.Text;
        private int _Size;
        private string _DisplayName;
        private LibControlType _ControlType = LibControlType.Text;
        private string[] _TextOption = null;
        private LibTextOptionCollection _KeyValueOption;
        private int _Precision = 2;

        public string Name
        {
            get { return _Name; }

            set { _Name = value; }
        }

        public LibDataType DataType
        {
            get { return _DataType; }

            set
            {
                _DataType = value;

                switch (DataType)
                { //预设默认控件
                    case LibDataType.Text:
                        this.ControlType = LibControlType.Text;
                        break;

                    case LibDataType.NText:
                        this.ControlType = LibControlType.NText;
                        break;

                    case LibDataType.Boolean:
                        this.ControlType = LibControlType.YesNo;
                        break;
                }
            }
        }

        public int Size
        {
            get { return _Size; }

            set { _Size = value; }
        }

        public string DisplayName
        {
            get { return _DisplayName; }

            set { _DisplayName = value; }
        }


        public LibControlType ControlType
        {
            get { return _ControlType; }

            set { _ControlType = value; }
        }

        public string[] TextOption
        {
            get { return _TextOption; }

            set
            {
                _TextOption = value;

                if (_TextOption != null)
                { this._ControlType = LibControlType.TextOption; }
            }
        }

        public LibTextOptionCollection KeyValueOption
        {
            get { return _KeyValueOption; }

            set
            {
                _KeyValueOption = value;

                if (_TextOption != null)
                { this._ControlType = LibControlType.TextOption; }
            }
        }

        public int Precision
        {
            get { return _Precision; }

            set { _Precision = value; }
        }

        public DefineFieldDic()
        {
        }

        public DefineFieldDic(string name, string displayName)
        {
            this._Name = name;
            this._DisplayName = displayName;
        }

        public DefineFieldDic(string name, string displayName, int size)
        {
            this._Name = name;
            this._DisplayName = displayName;
            this._Size = size;
        }

        public DefineFieldDic(string name, string displayName, int size, int precision, LibDataType dataType,
                               LibControlType controlType, string[] textOption, LibTextOptionCollection keyValueOption)
        {
            this._Name = name;
            this._DisplayName = displayName;
            this._Size = size;
            this._DataType = dataType;
            this._ControlType = controlType;
            this._TextOption = textOption;
            this._KeyValueOption = keyValueOption;
            this._Precision = precision;
        }
    }

}
