using AxCRL.Bcf;
using AxCRL.Comm.Define;
using AxCRL.Comm.Utils;
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
using AxCRL.Core.Server;

namespace MES_Com.AbnormalBcf
{
    [ProgId(ProgId = "com.Liaison", ProgIdType = ProgIdType.Bcf)]
    public class ComLiaisonBcf : LibBcfData
    {
        protected override LibTemplate RegisterTemplate()
        {
            return new ComLiaisonBcfTemplate("com.Liaison");
        }
    }

    public class ComLiaisonBcfTemplate : LibTemplate
    {
        private const string tableName = "COMLIAISON";

        public ComLiaisonBcfTemplate(string progId)
            : base(progId, BillType.Bill, "联络单")
        {

        }

        protected override void BuildDataSet()
        {
            this.DataSet = new DataSet();
            string primaryName = "BILLNO";
            DataTable headTable = new DataTable(tableName);
            DataSourceHelper.AddColumn(new DefineField(headTable, primaryName, "单据编号", FieldSize.Size20) { AllowEmpty = false, AllowCopy = false, DataType = LibDataType.Text });
            DataSourceHelper.AddColumn(new DefineField(headTable, "TYPEID", "单据类型", FieldSize.Size20)
            {
                AllowEmpty = false,
                DataType = LibDataType.Text,
                ControlType = LibControlType.IdName,
                RelativeSource = new RelativeSourceCollection()
                {
                    new RelativeSource("com.LiaisonType")
                    {
                        RelFields = new RelFieldCollection()
                        {
                          new RelField("TYPENAME", LibDataType.NText,FieldSize.Size50,"单据类型名称")
                        }
                    }
                }
            });
            DataSourceHelper.AddBillDate(headTable);
            DataSourceHelper.AddColumn(new DefineField(headTable, "PRIORITYSTATE", "急缓程度") { DataType = LibDataType.Int32, ControlType = LibControlType.TextOption, DefaultValue = 1, TextOption = new string[] { "轻缓", "普通", "紧急" } });
            DataSourceHelper.AddColumn(new DefineField(headTable, "PERSONID", "接收人", FieldSize.Size20)
            {
                ControlType = LibControlType.IdName,
                RelativeSource = new RelativeSourceCollection() 
                { 
                    new RelativeSource("com.Person")
                    {
                        RelFields = new RelFieldCollection()
                        {
                          new RelField("PERSONNAME", LibDataType.NText,FieldSize.Size50,"接收人名称")
                        },
                        SetValueFields = new SetValueFieldCollection()
                        {
                            new SetValueField("DEPTID"),
                            new SetValueField("DEPTNAME")
                        }
                    }
                }
            });
            DataSourceHelper.AddColumn(new DefineField(headTable, "DEPTID", "接收部门", FieldSize.Size20)
            {
                ControlType = LibControlType.IdName,
                RelativeSource = new RelativeSourceCollection() 
                { 
                    new RelativeSource("com.Dept")
                    {
                         RelFields = new RelFieldCollection()
                        {
                          new RelField("DEPTNAME", LibDataType.NText,FieldSize.Size50,"接收部门名称")
                       }  
                    }
                }
            });
            DataSourceHelper.AddColumn(new DefineField(headTable, "TITLE", "主题", FieldSize.Size200) { ColumnSpan = 3, RowSpan = 1 });
            DataSourceHelper.AddColumn(new DefineField(headTable, "MAINCONTENT", "主要内容", FieldSize.Size1000) { ColumnSpan = 2, RowSpan = 4 });
            DataSourceHelper.AddColumn(new DefineField(headTable, "REPLYCONTENT", "回执情况", FieldSize.Size1000) { ColumnSpan = 2, RowSpan = 4 });
            DataSourceHelper.AddColumn(new DefineField(headTable, "SENDPERSONID", "签发人", FieldSize.Size20)
            {
                AllowEmpty = false,
                DataType = LibDataType.Text,
                ControlType = LibControlType.NText,
                RelativeSource = new RelativeSourceCollection() 
                { 
                    new RelativeSource("com.Person")
                    {
                        RelFields = new RelFieldCollection()
                        {
                          new RelField("PERSONNAME", LibDataType.NText,FieldSize.Size50,"签发人名称","SENDPERSONNAME"),
                          new RelField("DEPTID", LibDataType.Text,FieldSize.Size50,"签发部门","SENDDEPTID"){ ControlType = LibControlType.IdName},
                          new RelField("DEPTNAME", LibDataType.NText,FieldSize.Size50,"签发部门名称","SENDDEPTNAME")
                        }  
                    }
                }
            });
            DataSourceHelper.AddColumn(new DefineField(headTable, "INFOID", "附带信息", FieldSize.Size100) { ColumnSpan = 1, ReadOnly = true });
            DataSourceHelper.AddFixColumn(headTable, this.BillType);
            headTable.PrimaryKey = new DataColumn[] { headTable.Columns[primaryName] };
            this.DataSet.Tables.Add(headTable);
        }

        protected override void DefineViewTemplate(DataSet dataSet)
        {
            LibBillLayout layout = new LibBillLayout(this.DataSet);
            layout.HeaderRange = layout.BuildControlGroup(0, string.Empty, new List<string>() { "BILLNO", "TYPEID", "BILLDATE", "PRIORITYSTATE", "PERSONID", "DEPTID", "SENDPERSONID", "SENDDEPTID", "TITLE", "INFOID", "MAINCONTENT", "REPLYCONTENT" });
            layout.ButtonRange = layout.BuildButton(new List<FunButton>() { new FunButton("btnOpen", "打开附带信息") });
            this.ViewTemplate = new LibBillTpl(this.DataSet, layout);
        }
        protected override void DefineFuncPermission()
        {
            base.DefineFuncPermission();
            this.FuncPermission = new LibFuncPermission("", this.BillType);
            this.FuncPermission.BillTypeName = string.Format("{0}Type", this.ProgId);
            this.FuncPermission.EntryParam.Add("TYPEID");
        }

    }

}
