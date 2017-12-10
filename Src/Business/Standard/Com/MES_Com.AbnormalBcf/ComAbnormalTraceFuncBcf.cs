using AxCRL.Bcf;
using AxCRL.Comm.Define;
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

namespace MES_Com.AbnormalBcf
{
    /// <summary>
    /// 异常追踪单数据临时表
    /// </summary>
    [ProgId(ProgId = "com.AbnormalTraceFunc", ProgIdType = ProgIdType.Bcf, VclPath = @"/Scripts/module/mes/com/comAbnormalTraceFuncVcl.js")]
    public class ComAbnormalTraceFuncBcf : LibBcfDataFunc
    {
        /// <summary>
        /// 异常追踪单数据临时表 注册模板
        /// </summary>
        /// <returns>返回 异常追踪单数据临时表 的数据模板</returns>
        protected override LibTemplate RegisterTemplate()
        {
            return new ComAbnormalTraceFuncBcfTemplate("com.AbnormalTraceFunc");
        }
    }

    /// <summary>
    /// 异常追踪单数据临时表 功能模板
    /// </summary>
    public class ComAbnormalTraceFuncBcfTemplate : LibTemplate
    {
        // 异常追踪单数据临时表 主表
        private const string tableName = "COMABNORMALTRACEFUNC";
        // 异常追踪单数据临时表 子表 特殊处理人员明细
        private const string bodyTableName = "COMABNORMALTRACEFUNCDETAIL";

        /// <summary>
        /// 异常追踪单数据临时表 模板功能定义
        /// </summary>
        /// <param name="progId">异常追踪单数据临时表 功能标识</param>
        public ComAbnormalTraceFuncBcfTemplate(string progId)
            : base(progId, BillType.DataFunc, "特殊处理人员界面")
        {
        }

        /// <summary>
        /// 异常追踪单数据临时表 数据模型
        /// </summary>
        protected override void BuildDataSet()
        {
            base.BuildDataSet();
            this.DataSet = new DataSet();

            #region 异常追踪单数据临时表 主表
            DataTable headTable = new DataTable(tableName);
            DataSourceHelper.AddColumn(new DefineField(headTable, "TYPEID", "单据类型", FieldSize.Size50)
            {
                #region 异常追踪单单据类型
                ReadOnly = true, 
                DataType = LibDataType.Text,
                ControlType = LibControlType.IdName,
                RelativeSource = new RelativeSourceCollection()
                {
                    new RelativeSource("com.AbnormalTraceType")
                    {
                        RelFields = new RelFieldCollection()
                        {
                            new RelField("TYPENAME", LibDataType.NText,FieldSize.Size20,"单据类型名称")
                        }
                    }
                }
                #endregion
            });
            DataSourceHelper.AddColumn(new DefineField(headTable, "PROCESSLEVEL", "当前处理层级") { ReadOnly = true, DataType = LibDataType.Int32, ControlType = LibControlType.Number });
            this.DataSet.Tables.Add(headTable); 
            #endregion

            #region 异常追踪单数据临时表 子表 特殊处理人员明细
            DataTable bodyTable = new DataTable(bodyTableName);
            DataSourceHelper.AddColumn(new DefineField(bodyTable, "PERSONID", "处理人代码", FieldSize.Size20)
            {
                #region 人员
                AllowEmpty = false,
                ControlType = LibControlType.IdName,
                RelativeSource = new RelativeSourceCollection() 
                { 
                    new RelativeSource("com.Person")
                    {  
                        RelFields = new RelFieldCollection()
                        { 
                            new RelField("PERSONNAME", LibDataType.NText,FieldSize.Size50,"处理人名称"),
                            new RelField("POSITION", LibDataType.NText,FieldSize.Size50,"职位"),
                            new RelField("PHONENO",LibDataType.Text,FieldSize.Size20,"电话"),
                            new RelField("WECHAT", LibDataType.NText,FieldSize.Size50,"微信")
                        }
                    }  
                }
                #endregion
            });
            DataSourceHelper.AddColumn(new DefineField(bodyTable, "NEEDSMS", "发短信") { DataType = LibDataType.Boolean, ControlType = LibControlType.YesNo });
            DataSourceHelper.AddColumn(new DefineField(bodyTable, "SENDWECHAT", "发微信") { DataType = LibDataType.Boolean, ControlType = LibControlType.YesNo });
            this.DataSet.Tables.Add(bodyTable); 
            #endregion
        }

        ///<summary>
        ///异常追踪单数据临时表 页面排版模型
        ///</summary>
        protected override void DefineViewTemplate(DataSet dataSet)
        {
            LibBillLayout layout = new LibBillLayout(this.DataSet);
            layout.HeaderRange = layout.BuildControlGroup(0, string.Empty, new List<string>() { "TYPEID", "PROCESSLEVEL" });
            layout.TabRange.Add(layout.BuildGrid(1, "特殊处理人员界面"));
            this.ViewTemplate = new LibBillTpl(this.DataSet, layout);
        }
    }
}
