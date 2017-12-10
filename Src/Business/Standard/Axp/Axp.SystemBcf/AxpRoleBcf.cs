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
using System.Text;
using System.Threading.Tasks;
namespace Axp.SystemBcf
{
    [ProgId(ProgId = "axp.Role", ProgIdType = ProgIdType.Bcf)]
    public class AxpRoleBcf : LibBcfData
    {
        protected override LibTemplate RegisterTemplate()
        {
            return new AxpRoleBcfTemplate("axp.Role");
        }

        protected override void AfterUpdate()
        {
            base.AfterUpdate();
            //清除缓存
            DataRow masterRow = this.DataSet.Tables[0].Rows[0];
            if (this.BillAction != AxCRL.Bcf.BillAction.AddNew && this.BillAction != AxCRL.Bcf.BillAction.SaveToDraft && this.BillAction != AxCRL.Bcf.BillAction.SubmitDraft)
            {
                LibRolePermissionCache.Default.RemoveCacheItem(LibSysUtils.ToString(masterRow["ROLEID", DataRowVersion.Original]));
            }
        }

        protected override void AfterDelete()
        {
            base.AfterDelete();
            DataRow masterRow = this.DataSet.Tables[0].Rows[0];
            LibRolePermissionCache.Default.RemoveCacheItem(LibSysUtils.ToString(masterRow["ROLEID"]));
        }
    }

    public class AxpRoleBcfTemplate : LibTemplate
    {
        private const string masterTableName = "AXPROLE";
        private const string bodyTableName = "AXPROLEDETAIL";

        public AxpRoleBcfTemplate(string progId)
            : base(progId, BillType.Master, "角色")
        {

        }

        protected override void BuildDataSet()
        {
            this.DataSet = new DataSet();
            string primaryName = "ROLEID";
            DataTable masterTable = new DataTable(masterTableName);
            DataSourceHelper.AddColumn(new DefineField(masterTable, primaryName, "角色代码", FieldSize.Size20) { AllowEmpty = false, AllowCopy = false });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "ROLENAME", "角色名称", FieldSize.Size50) { AllowEmpty = false });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "ISUNLIMITED", "无限制") { DataType = LibDataType.Boolean, ControlType = LibControlType.YesNo });
            DataSourceHelper.AddFixColumn(masterTable, this.BillType);
            masterTable.PrimaryKey = new DataColumn[] { masterTable.Columns[primaryName] };
            this.DataSet.Tables.Add(masterTable);

            DataTable bodyTable = new DataTable(bodyTableName);
            DataSourceHelper.AddColumn(new DefineField(bodyTable, primaryName, "角色代码", FieldSize.Size20) { AllowEmpty = false, AllowCopy = false });
            DataSourceHelper.AddRowId(bodyTable);
            DataSourceHelper.AddRowNo(bodyTable);
            DataSourceHelper.AddRemark(bodyTable);
            DataSourceHelper.AddColumn(new DefineField(bodyTable, "PERMISSIONGROUPID", "权限组", FieldSize.Size50)
            {
                AllowEmpty = false,
                ControlType = LibControlType.IdName,
                RelativeSource = new RelativeSourceCollection(){
                    new RelativeSource("axp.PermissionGroup"){
                           RelFields = new RelFieldCollection(){
                           new RelField("PERMISSIONGROUPNAME", LibDataType.NText,FieldSize.Size50,"权限组名称")
                      }  
                    }
                }
            });
            bodyTable.PrimaryKey = new DataColumn[] { bodyTable.Columns[primaryName], bodyTable.Columns["ROW_ID"] };
            this.DataSet.Tables.Add(bodyTable);
            this.DataSet.Relations.Add(string.Format("{0}_{1}", masterTableName, bodyTableName), masterTable.Columns[primaryName], bodyTable.Columns[primaryName]);
        }

        protected override void DefineViewTemplate(DataSet dataSet)
        {
            LibBillLayout layout = new LibBillLayout(this.DataSet);
            layout.HeaderRange = layout.BuildControlGroup(0, string.Empty, new List<string>() { "ROLEID", "ROLENAME", "ISUNLIMITED" });
            layout.GridRange = layout.BuildGrid(1, "权限组");
            this.ViewTemplate = new LibBillTpl(this.DataSet, layout);
        }
    }
}
