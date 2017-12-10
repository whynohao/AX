using AxCRL.Bcf;
using AxCRL.Comm.Define;
using AxCRL.Comm.Utils;
using AxCRL.Core.Cache;
using AxCRL.Data.SqlBuilder;
using AxCRL.Template;
using AxCRL.Template.DataSource;
using AxCRL.Template.Layout;
using AxCRL.Template.ViewTemplate;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
namespace Axp.SystemBcf
{
    [ProgId(ProgId = "axp.PermissionGroup", ProgIdType = ProgIdType.Bcf, VclPath = @"/Scripts/module/mes/axp/axpPermissionGroupVcl.js")]
    public class AxpPermissionGroupBcf : LibBcfData
    {
        protected override LibTemplate RegisterTemplate()
        {
            return new AxpPermissionGroupBcfTemplate("axp.PermissionGroup");
        }

        protected override void AfterChangeData(DataSet tables)
        {
            base.AfterChangeData(tables);
            foreach (DataRow dataRow in tables.Tables[1].Rows)
            {
                if (dataRow.RowState == DataRowState.Deleted)
                    continue;
                if (!string.IsNullOrEmpty(LibSysUtils.ToString(dataRow["SHOWCONDITION"])))
                    dataRow["HASSHOWCONDITION"] = true;
            }
        }

        protected override void CheckFieldReturn(int tableIndex, string fieldName, object[] curPk, Dictionary<string, object> fieldKeyAndValue, Dictionary<string, object> returnValue)
        {
            base.CheckFieldReturn(tableIndex, fieldName, curPk, fieldKeyAndValue, returnValue);
            if (tableIndex == 1 && fieldName == "PROGID" && curPk != null && curPk.Length == 1)
            {
                LibBcfBase bcfBase = LibBcfSystem.Default.GetBcfInstance(LibSysUtils.ToString(curPk[0]));
                if (bcfBase != null)
                {
                    bcfBase.Template.GetViewTemplate(bcfBase.DataSet);
                    returnValue.Add("OperatePowerData", BuildPowerInfo(bcfBase.Template.FuncPermission.Permission));
                    string sql = string.Format("select distinct BUTTONID,BUTTONNAME from AXPFUNCBUTTON where PROGID={0}", LibStringBuilder.GetQuotObject(curPk[0]));
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql, false))
                    {
                        while (reader.Read())
                        {
                            dic.Add(LibSysUtils.ToString(reader["BUTTONID"]), LibSysUtils.ToString(reader["BUTTONNAME"]));
                        }
                    }
                    returnValue.Add("ButtonPowerData", dic);
                }
            }
        }

        private Dictionary<int, PowerInfo> BuildPowerInfo(int permission)
        {
            Dictionary<int, PowerInfo> ret = new Dictionary<int, PowerInfo>();
            ret.Add((int)FuncPermissionEnum.Use, new PowerInfo("使用", (permission & (int)FuncPermissionEnum.Use) == (int)FuncPermissionEnum.Use));
            ret.Add((int)FuncPermissionEnum.Browse, new PowerInfo("浏览", (permission & (int)FuncPermissionEnum.Browse) == (int)FuncPermissionEnum.Browse));
            ret.Add((int)FuncPermissionEnum.Add, new PowerInfo("新增", (permission & (int)FuncPermissionEnum.Add) == (int)FuncPermissionEnum.Add));
            ret.Add((int)FuncPermissionEnum.Edit, new PowerInfo("修改", (permission & (int)FuncPermissionEnum.Edit) == (int)FuncPermissionEnum.Edit));
            ret.Add((int)FuncPermissionEnum.Delete, new PowerInfo("删除", (permission & (int)FuncPermissionEnum.Delete) == (int)FuncPermissionEnum.Delete));
            ret.Add((int)FuncPermissionEnum.Release, new PowerInfo("生效", (permission & (int)FuncPermissionEnum.Release) == (int)FuncPermissionEnum.Release));
            ret.Add((int)FuncPermissionEnum.CancelRelease, new PowerInfo("取消生效", (permission & (int)FuncPermissionEnum.CancelRelease) == (int)FuncPermissionEnum.CancelRelease));
            ret.Add((int)FuncPermissionEnum.Audit, new PowerInfo("审核", (permission & (int)FuncPermissionEnum.Audit) == (int)FuncPermissionEnum.Audit));
            ret.Add((int)FuncPermissionEnum.CancelAudit, new PowerInfo("弃审", (permission & (int)FuncPermissionEnum.CancelAudit) == (int)FuncPermissionEnum.CancelAudit));
            ret.Add((int)FuncPermissionEnum.EndCase, new PowerInfo("结案", (permission & (int)FuncPermissionEnum.EndCase) == (int)FuncPermissionEnum.EndCase));
            ret.Add((int)FuncPermissionEnum.CancelEndCase, new PowerInfo("取消结案", (permission & (int)FuncPermissionEnum.CancelEndCase) == (int)FuncPermissionEnum.CancelEndCase));
            ret.Add((int)FuncPermissionEnum.Invalid, new PowerInfo("作废", (permission & (int)FuncPermissionEnum.Invalid) == (int)FuncPermissionEnum.Invalid));
            ret.Add((int)FuncPermissionEnum.CancelInvalid, new PowerInfo("取消作废", (permission & (int)FuncPermissionEnum.CancelInvalid) == (int)FuncPermissionEnum.CancelInvalid));
            ret.Add((int)FuncPermissionEnum.Import, new PowerInfo("导入", (permission & (int)FuncPermissionEnum.Import) == (int)FuncPermissionEnum.Import));
            ret.Add((int)FuncPermissionEnum.Export, new PowerInfo("导出", (permission & (int)FuncPermissionEnum.Export) == (int)FuncPermissionEnum.Export));
            ret.Add((int)FuncPermissionEnum.Print, new PowerInfo("打印", (permission & (int)FuncPermissionEnum.Print) == (int)FuncPermissionEnum.Print));
            return ret;
        }


        protected override void BeforeUpdate()
        {
            base.BeforeUpdate();
            HashSet<string> hasSet = new HashSet<string>();
            foreach (DataRow curRow in this.DataSet.Tables[1].Rows)
            {
                if (curRow.RowState == DataRowState.Deleted)
                    continue;
                DataRow[] subRows = curRow.GetChildRows("AXPPERMISSIONGROUPDETAIL_AXPOPERATEPOWER", DataRowVersion.Current);
                int mark = 0;
                foreach (DataRow subRow in subRows)
                {
                    if (LibSysUtils.ToBoolean(subRow["CANUSE"]))
                        mark += LibSysUtils.ToInt32(subRow["OPERATEPOWERID"]);
                }
                curRow["OPERATEMARK"] = mark;
                string progId = LibSysUtils.ToString(curRow["PROGID"]);
                if (hasSet.Contains(progId))
                    this.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("行{0}的功能标识重复", curRow["ROWNO"]));
                else
                    hasSet.Add(progId);
            }
        }

        protected override void AfterUpdate()
        {
            base.AfterUpdate();
            //清除缓存
            DataRow masterRow = this.DataSet.Tables[0].Rows[0];
            if (this.BillAction != AxCRL.Bcf.BillAction.AddNew && this.BillAction != AxCRL.Bcf.BillAction.SaveToDraft && this.BillAction != AxCRL.Bcf.BillAction.SubmitDraft)
            {
                LibPermissionGroupCache.Default.RemoveCacheItem(LibSysUtils.ToString(masterRow["PERMISSIONGROUPID", DataRowVersion.Original]));
            }
        }

        protected override void AfterDelete()
        {
            base.AfterDelete();
            DataRow masterRow = this.DataSet.Tables[0].Rows[0];
            LibPermissionGroupCache.Default.RemoveCacheItem(LibSysUtils.ToString(masterRow["PERMISSIONGROUPID"]));
        }

    }

    [DataContract]
    public class PowerInfo
    {
        private string _DisplayText;
        private bool _CanUse;

        public PowerInfo(string displayText, bool canUse)
        {
            this.DisplayText = displayText;
            this.CanUse = canUse;
        }

        [DataMember]
        public bool CanUse
        {
            get { return _CanUse; }
            set { _CanUse = value; }
        }

        [DataMember]
        public string DisplayText
        {
            get { return _DisplayText; }
            set { _DisplayText = value; }
        }
    }



    public class AxpPermissionGroupBcfTemplate : LibTemplate
    {
        private const string masterTableName = "AXPPERMISSIONGROUP";
        private const string bodyTableName = "AXPPERMISSIONGROUPDETAIL";
        private const string subTableName = "AXPOPERATEPOWER";
        private const string fieldTableName = "AXPFIELDPOWER";
        private const string buttonTableName = "AXPBUTTONPOWER";


        public AxpPermissionGroupBcfTemplate(string progId)
            : base(progId, BillType.Master, "权限组")
        {

        }

        protected override void BuildDataSet()
        {
            this.DataSet = new DataSet();
            string primaryName = "PERMISSIONGROUPID";
            DataTable masterTable = new DataTable(masterTableName);
            DataSourceHelper.AddColumn(new DefineField(masterTable, primaryName, "权限组代码", FieldSize.Size20) { AllowEmpty = false, AllowCopy = false });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "PERMISSIONGROUPNAME", "权限组名称", FieldSize.Size50) { AllowEmpty = false });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "PARENTGROUPID", "父权限组", FieldSize.Size50)
            {
                ControlType = LibControlType.IdName,
                RelativeSource = new RelativeSourceCollection(){
                    new RelativeSource("axp.PermissionGroup"){
                           RelFields = new RelFieldCollection(){
                           new RelField("PERMISSIONGROUPNAME", LibDataType.NText,FieldSize.Size50,"父权限组名称","PARENTGROUPNAME")
                      }  
                    }
                }
            });
            DataSourceHelper.AddFixColumn(masterTable, this.BillType);
            masterTable.PrimaryKey = new DataColumn[] { masterTable.Columns[primaryName] };
            this.DataSet.Tables.Add(masterTable);

            DataTable bodyTable = new DataTable(bodyTableName);
            DataSourceHelper.AddColumn(new DefineField(bodyTable, primaryName, "权限组代码", FieldSize.Size20) { AllowEmpty = false, AllowCopy = false });
            DataSourceHelper.AddRowId(bodyTable);
            DataSourceHelper.AddRowNo(bodyTable);
            DataSourceHelper.AddColumn(new DefineField(bodyTable, "PROGID", "功能代码", FieldSize.Size50)
            {
                AllowEmpty = false,
                ControlType = LibControlType.IdName,
                RelativeSource = new RelativeSourceCollection(){
                    new RelativeSource("axp.FuncList"){
                           RelFields = new RelFieldCollection(){
                           new RelField("PROGNAME", LibDataType.NText,FieldSize.Size50,"功能名称")
                      }  
                    }
                }
            });
            DataSourceHelper.AddColumn(new DefineField(bodyTable, "SHOWCONDITION", "浏览条件") { DataType = LibDataType.Binary, ControlType = LibControlType.Text, ReadOnly = true });
            DataSourceHelper.AddColumn(new DefineField(bodyTable, "HASSHOWCONDITION", "存在浏览条件") { DataType = LibDataType.Boolean, ControlType = LibControlType.YesNo, FieldType = FieldType.Virtual, ReadOnly = true });
            DataSourceHelper.AddColumn(new DefineField(bodyTable, "ISOPERATEPOWER", "操作权限") { ReadOnly = true, DataType = LibDataType.Boolean, ControlType = LibControlType.YesNo, SubTableIndex = 2 });
            DataSourceHelper.AddColumn(new DefineField(bodyTable, "ISFIELDPOWER", "字段权限") { ReadOnly = true, DataType = LibDataType.Boolean, ControlType = LibControlType.YesNo, SubTableIndex = 3 });
            DataSourceHelper.AddColumn(new DefineField(bodyTable, "ISBUTTONPOWER", "功能权限") { ReadOnly = true, DataType = LibDataType.Boolean, ControlType = LibControlType.YesNo, SubTableIndex = 4 });
            DataSourceHelper.AddColumn(new DefineField(bodyTable, "OPERATEMARK", "操作权限标识") { DataType = LibDataType.Int32, ControlType = LibControlType.Number, ReadOnly = true });
            DataSourceHelper.AddRemark(bodyTable);
            bodyTable.PrimaryKey = new DataColumn[] { bodyTable.Columns[primaryName], bodyTable.Columns["ROW_ID"] };
            this.DataSet.Tables.Add(bodyTable);
            this.DataSet.Relations.Add(string.Format("{0}_{1}", masterTableName, bodyTableName), masterTable.Columns[primaryName], bodyTable.Columns[primaryName]);

            DataTable subTable = new DataTable(subTableName);
            DataSourceHelper.AddColumn(new DefineField(subTable, primaryName, "权限组代码", FieldSize.Size20) { AllowEmpty = false, AllowCopy = false });
            DataSourceHelper.AddRowId(subTable, "PARENTROWID", "父行标识");
            DataSourceHelper.AddRowId(subTable);
            DataSourceHelper.AddRowNo(subTable);
            DataSourceHelper.AddColumn(new DefineField(subTable, "OPERATEPOWERID", "操作代码") { DataType = LibDataType.Int32, ControlType = LibControlType.Number, ReadOnly = true });
            DataSourceHelper.AddColumn(new DefineField(subTable, "OPERATEPOWERNAME", "操作", FieldSize.Size50) { DataType = LibDataType.NText, ControlType = LibControlType.NText, ReadOnly = true });
            DataSourceHelper.AddColumn(new DefineField(subTable, "CANUSE", "具备权限") { DataType = LibDataType.Boolean, ControlType = LibControlType.YesNo });
            DataSourceHelper.AddRemark(subTable);
            subTable.PrimaryKey = new DataColumn[] { subTable.Columns[primaryName], subTable.Columns["PARENTROWID"], subTable.Columns["ROW_ID"] };
            this.DataSet.Tables.Add(subTable);
            this.DataSet.Relations.Add(string.Format("{0}_{1}", bodyTableName, subTableName), new DataColumn[] { bodyTable.Columns[primaryName], bodyTable.Columns["ROW_ID"] }, new DataColumn[] { subTable.Columns[primaryName], subTable.Columns["PARENTROWID"] });

            DataTable fieldTable = new DataTable(fieldTableName);
            DataSourceHelper.AddColumn(new DefineField(fieldTable, primaryName, "权限组代码", FieldSize.Size20) { AllowEmpty = false, AllowCopy = false });
            DataSourceHelper.AddRowId(fieldTable, "PARENTROWID", "父行标识");
            DataSourceHelper.AddRowId(fieldTable);
            DataSourceHelper.AddRowNo(fieldTable);
            DataSourceHelper.AddColumn(new DefineField(fieldTable, "TABLEINDEX", "表索引") { DataType = LibDataType.Int32, ControlType = LibControlType.Number, QtyLimit = LibQtyLimit.GreaterOrEqualThanZero });
            DataSourceHelper.AddColumn(new DefineField(fieldTable, "FIELDNAME", "字段名", FieldSize.Size50) { ControlType = LibControlType.FieldControl, RelProgId = "B.PROGID", AllowEmpty = false, RelTableIndex = "D.TABLEINDEX" });
            DataSourceHelper.AddColumn(new DefineField(fieldTable, "FIELDPOWER", "权限选项") { DataType = LibDataType.Int32, ControlType = LibControlType.TextOption, TextOption = new string[] { "不能查看", "不能编辑" } });
            DataSourceHelper.AddColumn(new DefineField(fieldTable, "USECONDITION", "控制条件", FieldSize.Size500));
            DataSourceHelper.AddRemark(fieldTable);
            fieldTable.PrimaryKey = new DataColumn[] { fieldTable.Columns[primaryName], fieldTable.Columns["PARENTROWID"], fieldTable.Columns["ROW_ID"] };
            this.DataSet.Tables.Add(fieldTable);
            this.DataSet.Relations.Add(string.Format("{0}_{1}", bodyTableName, fieldTable), new DataColumn[] { bodyTable.Columns[primaryName], bodyTable.Columns["ROW_ID"] }, new DataColumn[] { fieldTable.Columns[primaryName], fieldTable.Columns["PARENTROWID"] });

            DataTable buttonTable = new DataTable(buttonTableName);
            DataSourceHelper.AddColumn(new DefineField(buttonTable, primaryName, "权限组代码", FieldSize.Size20) { AllowEmpty = false, AllowCopy = false });
            DataSourceHelper.AddRowId(buttonTable, "PARENTROWID", "父行标识");
            DataSourceHelper.AddRowId(buttonTable);
            DataSourceHelper.AddRowNo(buttonTable);
            DataSourceHelper.AddColumn(new DefineField(buttonTable, "BUTTONID", "功能按钮", FieldSize.Size50)
            {
                AllowEmpty = false,
                ControlType = LibControlType.IdName,
                RelativeSource = new RelativeSourceCollection(){
                    new RelativeSource("axp.FuncButton"){
                           RelPK = "B.PROGID",
                           RelFields = new RelFieldCollection(){
                           new RelField("BUTTONNAME", LibDataType.NText,FieldSize.Size50,"功能按钮名称")
                      }  
                    }
                }
            });
            DataSourceHelper.AddColumn(new DefineField(buttonTable, "CANUSE", "具备权限") { DataType = LibDataType.Boolean, ControlType = LibControlType.YesNo, DefaultValue = true });
            DataSourceHelper.AddRemark(buttonTable);
            buttonTable.PrimaryKey = new DataColumn[] { buttonTable.Columns[primaryName], buttonTable.Columns["PARENTROWID"], buttonTable.Columns["ROW_ID"] };
            this.DataSet.Tables.Add(buttonTable);
            this.DataSet.Relations.Add(string.Format("{0}_{1}", bodyTableName, buttonTable), new DataColumn[] { bodyTable.Columns[primaryName], bodyTable.Columns["ROW_ID"] }, new DataColumn[] { buttonTable.Columns[primaryName], buttonTable.Columns["PARENTROWID"] });
        }

        protected override void DefineViewTemplate(DataSet dataSet)
        {
            LibBillLayout layout = new LibBillLayout(this.DataSet);
            layout.HeaderRange = layout.BuildControlGroup(0, string.Empty, new List<string>() { "PERMISSIONGROUPID", "PERMISSIONGROUPNAME", "PARENTGROUPID" });
            layout.GridRange = layout.BuildGrid(1, "权限明细");
            layout.SubBill.Add(2, layout.BuildGrid(2, "操作权限明细"));
            layout.SubBill.Add(3, layout.BuildGrid(3, "字段权限明细"));
            layout.SubBill.Add(4, layout.BuildGrid(4, "功能权限明细"));
            layout.ButtonRange = layout.BuildButton(new List<FunButton>() { new FunButton("btnCondition", "设置浏览条件") });
            this.ViewTemplate = new LibBillTpl(this.DataSet, layout);
        }
    }
}
