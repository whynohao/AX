/**********************************************************************
 * CopyRight 2017 杭州集控科技有限公司 版权所有
 * 功能描述：同步数据到其他站点的配置
 * 创建标识：Zhangkj 2017/06/30
 * 
 *
************************************************************************/
using AxCRL.Bcf;
using AxCRL.Comm.Define;
using AxCRL.Core.Comm;
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
    [ProgId(ProgId = "axp.SyncDataHistory", ProgIdType = ProgIdType.Bcf)]
    public class AxpSyncDataHistoryBcf : LibBcfGrid
    {
        protected override LibTemplate RegisterTemplate()
        {
            return new AxpSyncDataHistoryBcfTemplate("axp.SyncDataHistory");
        }
    }
    public class AxpSyncDataHistoryBcfTemplate : LibTemplate
    {
        private const string masterTableName = "AXPSYNCDATAHISTORY";

        public AxpSyncDataHistoryBcfTemplate(string progId)
            : base(progId, BillType.Grid, "同步历史")
        {

        }

        protected override void BuildDataSet()
        {
            this.DataSet = new DataSet();
            DataSourceHelper.AddSyncDataHistoryTable(this.DataSet, masterTableName);
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
