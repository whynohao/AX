/**********************************************************************
 * CopyRight 2017 杭州集控科技有限公司 版权所有
 * 功能描述：职务主数据 管理类似 主管、经理、总监、总经理等的职务数据。一个组织中的岗位引用到相应的职务
 * 创建标识：Zhangkj 2017/03/14
 * 
************************************************************************/
using AxCRL.Bcf;
using AxCRL.Comm.Define;
using AxCRL.Comm.Utils;
using AxCRL.Core.Cache;
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

namespace MES_Com.MasterDataBcf
{
    [ProgId(ProgId = "com.Duty", ProgIdType = ProgIdType.Bcf)]
    public class ComDutyBcf : LibBcfData
    {
        protected override LibTemplate RegisterTemplate()
        {
            return new ComPostBcfTemplate("com.Duty");
        }
        protected override void AfterUpdate()
        {
            base.AfterUpdate();
            //清除缓存。
            if (this.BillAction != AxCRL.Bcf.BillAction.AddNew && this.BillAction != AxCRL.Bcf.BillAction.SaveToDraft && this.BillAction != AxCRL.Bcf.BillAction.SubmitDraft)
            {
                //因职级的改动会影响到每个部门中的岗位任职人员的次序，需要全部清空
                LibDeptDutyPersonCache.Default.RemoveAll();
            }
        }
        protected override void AfterDelete()
        {
            base.AfterDelete();
            //因职级的改动会影响到每个部门中的岗位任职人员的次序，需要全部清空
            LibDeptDutyPersonCache.Default.RemoveAll();            
        }       
    }

    public class ComPostBcfTemplate : LibTemplate
    {
        private const string comPostName = "COMDUTY";

        public ComPostBcfTemplate(string progId)
            : base(progId, BillType.Master, "职务")
        {

        }

        protected override void BuildDataSet()
        {
            this.DataSet = new DataSet();
            DataTable comPost = new DataTable(comPostName);
            DataSourceHelper.AddColumn(new DefineField(comPost, "DUTYID", "职务代码", FieldSize.Size20) { AllowEmpty = false, AllowCopy = false });
            DataSourceHelper.AddColumn(new DefineField(comPost, "DUTYNAME", "职务名称", FieldSize.Size50) { DataType = LibDataType.NText, AllowEmpty = false });
            DataSourceHelper.AddColumn(new DefineField(comPost, "DUTYLEVEL", "职务级别") { DataType = LibDataType.Int32, ControlType = LibControlType.Number, QtyLimit = LibQtyLimit.GreaterOrEqualThanZero, Precision = 0, DefaultValue = 0, AllowEmpty = false });
            DataSourceHelper.AddFixColumn(comPost, BillType);
            comPost.PrimaryKey = new DataColumn[] { comPost.Columns["DUTYID"] };
            this.DataSet.Tables.Add(comPost);
        }


        protected override void DefineViewTemplate(DataSet dataSet)
        {
            LibBillLayout layout = new LibBillLayout(this.DataSet);
            layout.HeaderRange = layout.BuildControlGroup(0, string.Empty, new List<string>() { "DUTYID", "DUTYNAME", "DUTYLEVEL" });
            this.ViewTemplate = new LibBillTpl(this.DataSet, layout);
        }
        protected override void DefineFuncPermission()
        {
            base.DefineFuncPermission();
            this.FuncPermission.UseSynchroData = false;//是否启用数据同步
        }
    }
}
