using AxCRL.Bcf;
using AxCRL.Comm.Define;
using AxCRL.Services;
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
    [ProgId(ProgId = "axp.UserNews", ProgIdType = ProgIdType.Bcf)]
    public class AxpUserNewsBcf : LibBcfGrid
    {
        /// <summary>
        /// 返回本站点的系统消息数据
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="onlyUnRead"></param>
        /// <returns></returns>
        public List<LibNews> GetMyNews(long startTime, bool onlyUnRead)
        {
            return (new BillService()).GetMyNewsThisSite(this.Handle.Handle, startTime, onlyUnRead);
        }
        /// <summary>
        /// 标记本站点的指定消息为已读
        /// </summary>
        /// <param name="newsList"></param>
        public void SetMyNewsReadState(string[] newsList)
        {
            (new BillService()).SetMyNewsReadStateThisSite(this.Handle.Handle, newsList);
        }
        protected override LibTemplate RegisterTemplate()
        {
            return new AxpUserNewsBcfTemplate("axp.UserNews");
        }
    }

    public class AxpUserNewsBcfTemplate : LibTemplate
    {
        private const string masterTableName = "AXPUSERNEWS";

        public AxpUserNewsBcfTemplate(string progId)
            : base(progId, BillType.Grid, "用户消息")
        {

        }

        protected override void BuildDataSet()
        {
            this.DataSet = new DataSet();
            DataTable masterTable = new DataTable(masterTableName);
            DataSourceHelper.AddColumn(new DefineField(masterTable, "NEWSID", "消息代码", FieldSize.Size50));
            DataSourceHelper.AddColumn(new DefineField(masterTable, "USERID", "用户账号", FieldSize.Size20));
            DataSourceHelper.AddColumn(new DefineField(masterTable, "TITLE", "主题", FieldSize.Size200) { DataType = LibDataType.NText, ControlType = LibControlType.NText });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "MAINCONTENT", "主要内容", FieldSize.Size1000) { DataType = LibDataType.Binary, ControlType = LibControlType.NText });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "INFOID", "附带信息", FieldSize.Size100) { ReadOnly = true });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "CREATETIME", "时间") { DataType = LibDataType.Int64, ControlType = LibControlType.DateTime });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "PERSONID", "发送人", FieldSize.Size20)
            {
                ControlType = LibControlType.IdName,
                RelativeSource = new RelativeSourceCollection() 
                { 
                    new RelativeSource("com.Person")
                    {
                        RelFields = new RelFieldCollection()
                        {
                          new RelField("PERSONNAME", LibDataType.NText,FieldSize.Size50,"发送人名称")
                        }
                    }
                }
            });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "ISREAD", "已读") { DataType = LibDataType.Boolean, ControlType = LibControlType.YesNo });
            DBIndexCollection dbList = new DBIndexCollection();
            dbList.Add(new DBIndex("NEWS_CREATETIME_IDX", new DBIndexFieldCollection() { new DBIndexField("CREATETIME") }));
            masterTable.ExtendedProperties.Add(TableProperty.DBIndex, dbList);
            masterTable.PrimaryKey = new DataColumn[] { masterTable.Columns["NEWSID"] };
            this.DataSet.Tables.Add(masterTable);
        }

        protected override void DefineViewTemplate(DataSet dataSet)
        {
            LibBillLayout layout = new LibBillLayout(this.DataSet);
            layout.GridRange = layout.BuildGrid(0, string.Empty, null, true);
            this.ViewTemplate = new LibGridTpl(this.DataSet, layout);
        }

        protected override void DefineFuncPermission()
        {
            base.DefineFuncPermission();
            this.FuncPermission.CanMenu = false;
        }
    }
}
