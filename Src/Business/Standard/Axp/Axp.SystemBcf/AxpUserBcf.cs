using AxCRL.Bcf;
using AxCRL.Comm.Define;
using AxCRL.Comm.Entity;
using AxCRL.Comm.Enums;
using AxCRL.Comm.Utils;
using AxCRL.Core.Comm;
using AxCRL.Core.Mail;
using AxCRL.Data.SqlBuilder;
using AxCRL.Services;
using AxCRL.Template;
using AxCRL.Template.DataSource;
using AxCRL.Template.Layout;
using AxCRL.Template.ViewTemplate;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Axp.SystemBcf
{
    [ProgId(ProgId = "axp.User", ProgIdType = ProgIdType.Bcf)]
    public class AxpUserBcf : LibBcfData
    {
        protected override LibTemplate RegisterTemplate()
        {
            return new AxpUserBcfTemplate("axp.User");
        }
        protected override void BeforeUpdate()
        {
            base.BeforeUpdate();
            List<int> appTypeList = new List<int>();
            int clientType = 0;
            //同一个账户下的UserApp子表中的App类型不能重复
            foreach(DataRow curRow in this.DataSet.Tables[1].Rows)
            {
                switch (curRow.RowState)
                {
                    case DataRowState.Added:
                    case DataRowState.Modified:
                    case DataRowState.Unchanged:
                        clientType = LibSysUtils.ToInt32(curRow["CLIENTTYPE", DataRowVersion.Current]);
                        if (appTypeList.Contains(clientType) == false)
                        {
                            appTypeList.Add(clientType);
                        }
                        else
                        {
                            this.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("APP明细，行号:{0},同一个账户下的UserApp子表中的App类型不能重复。",curRow["ROW_ID"]));
                        }
                        break;
                }
            }
            // 站点不能重复配置
            List<string> siteIdList = new List<string>();
            foreach (DataRow curRow in this.DataSet.Tables[2].Rows)
            {
                switch (curRow.RowState)
                {
                    case DataRowState.Added:
                    case DataRowState.Modified:
                    case DataRowState.Unchanged:
                        string siteId = curRow["SITEID", DataRowVersion.Current].ToString();
                        if (siteIdList.Contains(siteId) == false)
                        {
                            siteIdList.Add(siteId);
                        }
                        else
                        {
                            this.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("可访问站点配置，行号:{0},同一个账户下不能重复可访问站点。", curRow["ROW_ID"]));
                        }
                        break;
                }
            }
        }
        protected override void AfterCommintData()
        {
            base.AfterCommintData();
            if (!this.ManagerMessage.IsThrow)
            {
                List<AxCRL.Core.Mail.LibMailParam> list = new List<AxCRL.Core.Mail.LibMailParam>();
                foreach (DataRow curRow in this.DataSet.Tables[0].Rows)
                {
                    if (curRow.RowState == DataRowState.Modified)
                    {
                        if (LibSysUtils.ToBoolean(curRow["ISUSE"]) && !LibSysUtils.ToBoolean(curRow["ISUSE", DataRowVersion.Original]))
                        {
                            AxCRL.Core.Mail.LibMailParam param = new AxCRL.Core.Mail.LibMailParam();
                            param.Content = string.Format("您的账号 {0} 已开通。", curRow["USERID"]);
                            param.MailKind = AxCRL.Core.Mail.LibMailKind.Info;
                            param.Subject = "智慧工厂账号开通";
                            param.To = new List<string>() { LibSysUtils.ToString(curRow["PERSONID"]) };
                            list.Add(param);
                        }
                    }
                }
                if (list.Count > 0)
                {
                    ThreadPool.QueueUserWorkItem(LibMailHelper.SendMail, list);
                }
            }
        }
    }

    public class AxpUserBcfTemplate : LibTemplate
    {
        private const string masterTableName = "AXPUSER";
        private const string tableAppName = "AXPUSERAPP";
        private const string siteTableName = "AXPUSERSITE";
        public AxpUserBcfTemplate(string progId)
            : base(progId, BillType.Master, "系统账户")
        {

        }

        protected override void BuildDataSet()
        {
            this.DataSet = new DataSet();
            string primaryName = "USERID";
            DataTable masterTable = new DataTable(masterTableName);
            DataSourceHelper.AddColumn(new DefineField(masterTable, primaryName, "用户账号", FieldSize.Size20) { AllowEmpty = false, AllowCopy = false });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "USERPASSWORD", "用户密码", FieldSize.Size50) { AllowCondition = false, InputType = InputType.Password });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "PERSONID", "人员代码", FieldSize.Size20)
            {
                ControlType = LibControlType.IdName,
                RelativeSource = new RelativeSourceCollection() {
                 new RelativeSource("com.Person"){  RelFields = new RelFieldCollection()
                     { new RelField("PERSONNAME", LibDataType.NText,FieldSize.Size50,"人员名称"),
                       new RelField("PHONENO",LibDataType.Text,FieldSize.Size20,"手机","PERSONPHONE"),
                       new RelField("CORNET",LibDataType.Text,FieldSize.Size20,"短号","PERSONPHONENO"),
                       new RelField("HEADPORTRAIT",LibDataType.NText,FieldSize.Size500,"头像","PERSONHEADPORTRAIT"),
                       new RelField("MAIL",LibDataType.Text,FieldSize.Size50,"邮箱","PERSONMAIL")
                     }}
                }
            });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "ROLEID", "角色", FieldSize.Size20)
            {
                ControlType = LibControlType.IdName,
                RelativeSource = new RelativeSourceCollection() {
                 new RelativeSource("axp.Role"){  RelFields = new RelFieldCollection()
                     { new RelField("ROLENAME", LibDataType.NText,FieldSize.Size50,"角色名称") }}
                }
            });           
            DataSourceHelper.AddColumn(new DefineField(masterTable, "ISUSE", "启用") { DataType = LibDataType.Boolean, ControlType = LibControlType.YesNo, DefaultValue = true });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "WALLPAPER", "壁纸", FieldSize.Size100) { DataType = LibDataType.NText, ControlType = LibControlType.NText, ReadOnly = true });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "WALLPAPERSTRETCH", "充满桌面") { DataType = LibDataType.Boolean, ControlType = LibControlType.YesNo, ReadOnly = true, DefaultValue = true });
            DataSourceHelper.AddFixColumn(masterTable, this.BillType);
            masterTable.PrimaryKey = new DataColumn[] { masterTable.Columns[primaryName] };
            this.DataSet.Tables.Add(masterTable);
            ///*
            // 系统账户对应的各个APP信息
            DataTable bodyTable = new DataTable(tableAppName);
            DataSourceHelper.AddColumn(new DefineField(bodyTable, primaryName, "用户账号", FieldSize.Size20));
            DataSourceHelper.AddRowId(bodyTable);
            DataSourceHelper.AddRowNo(bodyTable);
            LibTextOptionCollection appTypeOptionList = new LibTextOptionCollection();
            appTypeOptionList.Add(new LibTextOption() { Key = ((int)AppType.LeaderMobile).ToString(), Value = "App" });
            appTypeOptionList.Add(new LibTextOption() { Key = ((int)AppType.PDA).ToString(), Value = "PDA" });
            DataSourceHelper.AddColumn(new DefineField(bodyTable, "CLIENTTYPE", "APP类型")
            {
                DataType = LibDataType.Int32,
                ControlType = LibControlType.KeyValueOption,// 需要以键值对的形式给出选项，以便Int32的值可以与枚举项的值对应
                KeyValueOption = appTypeOptionList
            });
            DataSourceHelper.AddColumn(new DefineField(bodyTable, "CLIENTID", "客户端ID", FieldSize.Size100));           
            bodyTable.PrimaryKey = new DataColumn[] { bodyTable.Columns[primaryName], bodyTable.Columns["ROW_ID"] };

            this.DataSet.Tables.Add(bodyTable);
            this.DataSet.Relations.Add(string.Format("{0}_{1}", masterTableName, tableAppName), masterTable.Columns[primaryName], bodyTable.Columns[primaryName]);

            // 账户可以登录的网站
            DataTable siteTable = new DataTable(siteTableName);
            DataSourceHelper.AddColumn(new DefineField(siteTable, primaryName, "用户账号", FieldSize.Size20));
            DataSourceHelper.AddRowId(siteTable);
            DataSourceHelper.AddRowNo(siteTable);
            DataSourceHelper.AddColumn(new DefineField(siteTable, "SITEID", "站点", FieldSize.Size20)
            {
                RelativeSource = new RelativeSourceCollection()
                {
                    new RelativeSource("axp.LinkSite")
                    {
                        RelFields = new RelFieldCollection()
                        {
                            new RelField("SHORTNAME", LibDataType.NText, FieldSize.Size100, "站点名称")
                        }
                    }
                }
            });           
            siteTable.PrimaryKey = new DataColumn[] { siteTable.Columns[primaryName], siteTable.Columns["ROW_ID"] };
            this.DataSet.Tables.Add(siteTable);
            this.DataSet.Relations.Add(string.Format("{0}_{1}", masterTableName, siteTableName), masterTable.Columns[primaryName], siteTable.Columns[primaryName]);
            //*/    
        }       
        protected override void DefineViewTemplate(DataSet dataSet)
        {
            LibBillLayout layout = new LibBillLayout(this.DataSet);
            layout.HeaderRange = layout.BuildControlGroup(0, string.Empty, new List<string>() { "USERID", "USERPASSWORD", "PERSONID", "PERSONNAME", "ROLEID", "ROLENAME", "ISUSE", "WALLPAPER", "WALLPAPERSTRETCH" });
            layout.TabRange.Add(layout.BuildGrid(1, "APP明细"));
            layout.TabRange.Add(layout.BuildGrid(2, "可访问站点配置"));
            this.ViewTemplate = new LibBillTpl(this.DataSet, layout);
        }
        protected override void DefineFuncPermission()
        {
            base.DefineFuncPermission();
            this.FuncPermission.UseSynchroData = false;//是否启用数据同步
        }
    }
}
