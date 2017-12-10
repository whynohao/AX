/**********************************************************************
 * CopyRight 2017 杭州集控科技有限公司 版权所有
 * 功能描述：部门。
 * 修改标识：Zhangkj 2017/03/14
 *           为部门增加上级部门、增加部门岗位、序号、级别等 * 
************************************************************************/
using AxCRL.Bcf;
using AxCRL.Comm.Define;
using AxCRL.Comm.Utils;
using AxCRL.Core.Cache;
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
using System.Threading.Tasks;
using AxCRL.Comm.Bill;
using AxCRL.Core.Comm;

namespace MES_Com.MasterDataBcf
{
    [ProgId(ProgId = "com.Dept", ProgIdType = ProgIdType.Bcf)]
    public class ComDeptBcf : LibBcfData
    {
        protected override LibTemplate RegisterTemplate()
        {
            return new ComDeptBcfTemplate("com.Dept");
        }       
        protected override void BeforeUpdate()
        {
            base.BeforeUpdate();
            DataRow masterRow = this.DataSet.Tables[0].Rows[0];
            #region 检查部门下的同一人员是否担任了重复的职务
            HashSet<string> hasSet = new HashSet<string>();
            foreach (DataRow curRow in this.DataSet.Tables[1].Rows)
            {
                if (curRow.RowState == DataRowState.Deleted)
                    continue;
                string duty_PersonID = string.Format("{0}_{1}", LibSysUtils.ToString(curRow["DUTYID"]), LibSysUtils.ToString(curRow["PERSONID"]));
                if (hasSet.Contains(duty_PersonID))
                    this.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("岗位任职行{0}的任职岗位与人员已存在。", curRow["ROWNO"]));
                else
                    hasSet.Add(duty_PersonID);
            }
            #endregion
        }
        protected override void AfterUpdate()
        {
            base.AfterUpdate();
            DataRow masterRow = this.DataSet.Tables[0].Rows[0];
            //清除缓存
            if (this.BillAction != AxCRL.Bcf.BillAction.AddNew && this.BillAction != AxCRL.Bcf.BillAction.SaveToDraft && this.BillAction != AxCRL.Bcf.BillAction.SubmitDraft)
            {
                LibDeptDutyPersonCache.Default.RemoveCacheItem(LibSysUtils.ToString(masterRow["DEPTID", DataRowVersion.Original]));
            }
        }
        protected override void AfterDelete()
        {
            base.AfterDelete();
            //清空部门任职表的缓存数据
            DataRow masterRow = this.DataSet.Tables[0].Rows[0];
            LibDeptDutyPersonCache.Default.RemoveCacheItem(LibSysUtils.ToString(masterRow["DEPTID"]));
        }
    }

    public class ComDeptBcfTemplate : LibTemplate
    {
        private const string comDeptName = "COMDEPT";
        private const string comDeptDutyPersonName = "COMDEPTDUTYPERSON";

        public ComDeptBcfTemplate(string progId)
            : base(progId, BillType.Master, "部门")
        {

        }

        protected override void BuildDataSet()
        {
            this.DataSet = new DataSet();
            DataTable comDept = new DataTable(comDeptName);
            DataSourceHelper.AddColumn(new DefineField(comDept, "DEPTID", "部门代码", FieldSize.Size20) { AllowEmpty = false, AllowCopy = false });
            DataSourceHelper.AddColumn(new DefineField(comDept, "DEPTNAME", "部门名称", FieldSize.Size50) { DataType = LibDataType.NText, AllowEmpty = false });

            Dictionary<string, bool> orderBys = new Dictionary<string, bool>();
            orderBys.Add("SORTORDER", true);
            DataSourceHelper.AddColumn(new DefineField(comDept, "SUPERDEPTID", "上级部门", FieldSize.Size20)
            {
                RelativeSource = new RelativeSourceCollection()
                {
                    new RelativeSource("com.Dept")
                    {
                         RelFields = new RelFieldCollection()
                         {
                             new RelField("DEPTNAME", LibDataType.NText,FieldSize.Size50,"上级部门名称","PARENTDEPTNAME")
                         },
                         ContainsSub = false,//不显示子数据
                         ExpandAll=true,//默认全部展开
                         SearchFilterCount = 200,//筛选200条
                         ParentColumnName = "SUPERDEPTID",
                         OrderbyColumns= orderBys
                    }
                },
                ControlType = LibControlType.IdNameTree //以树形结构展示,需要在RelativeSource属性后设置，否则会重置为IdName

            });          
            DataSourceHelper.AddColumn(new DefineField(comDept, "SORTORDER", "序号") { DataType = LibDataType.Int32, ControlType = LibControlType.Number, QtyLimit = LibQtyLimit.GreaterOrEqualThanZero, Precision = 0, DefaultValue = 0, AllowEmpty = false });
            DataSourceHelper.AddColumn(new DefineField(comDept, "DEPTLEVEL", "部门级别") { DataType = LibDataType.Int32, ControlType = LibControlType.Number, QtyLimit = LibQtyLimit.GreaterOrEqualThanZero, Precision = 0, DefaultValue = 0, AllowEmpty = false });

            DataSourceHelper.AddFixColumn(comDept, BillType);
            comDept.PrimaryKey = new DataColumn[] { comDept.Columns["DEPTID"] };
            this.DataSet.Tables.Add(comDept);

            #region 部门岗位人员表
            //部门下的各个岗位对应的人员
            DataTable comDeptDutyPerson = new DataTable(comDeptDutyPersonName);
            DataSourceHelper.AddColumn(new DefineField(comDeptDutyPerson, "DEPTID", "部门代码", FieldSize.Size20) { ControlType = LibControlType.Id, AllowEmpty = false, ReadOnly = true });
            DataSourceHelper.AddRowId(comDeptDutyPerson);
            DataSourceHelper.AddRowNo(comDeptDutyPerson);
            //岗位
            DataSourceHelper.AddColumn(new DefineField(comDeptDutyPerson, "DUTYID", "岗位", FieldSize.Size20)
            {
                AllowEmpty = false,
                ControlType = LibControlType.IdName,
                RelativeSource = new RelativeSourceCollection()
                {
                    new RelativeSource("com.Duty")
                    {
                         RelFields = new RelFieldCollection()
                         {
                             new RelField("DUTYNAME", LibDataType.NText,FieldSize.Size50,"岗位名称")
                         }
                    }
                }

            });           
            //人员
            DataSourceHelper.AddColumn(new DefineField(comDeptDutyPerson, "PERSONID", "人员", FieldSize.Size20)
            {
                AllowEmpty = false,
                ControlType = LibControlType.IdName,
                RelativeSource = new RelativeSourceCollection()
                {
                    new RelativeSource("com.Person")
                    {
                         RelFields = new RelFieldCollection()
                         {
                             new RelField("PERSONNAME", LibDataType.NText,FieldSize.Size50,"人员名称")
                         }
                    }
                }

            });
            //岗位任职表中相同岗位的高低序号，对于一个部门中同一岗位下有多个人时，可按照序号从小到大表示职位在部门内的从高到低顺序，例如第一副经理、第二副经理
            DataSourceHelper.AddColumn(new DefineField(comDeptDutyPerson, "PERSONORDER", "序号") { DataType = LibDataType.Int32, ControlType = LibControlType.Number, QtyLimit = LibQtyLimit.GreaterOrEqualThanZero, Precision = 0, DefaultValue = 0, AllowEmpty = false });
            DataSourceHelper.AddRemark(comDeptDutyPerson);//备注

            //comDeptDutyPerson.ExtendedProperties.Add(TableProperty.UsingApproveRow, true);//启用行审核的测试
            //DataSourceHelper.AddApproveRowFixColumn(comDeptDutyPerson);

            comDeptDutyPerson.PrimaryKey = new DataColumn[] { comDeptDutyPerson.Columns["DEPTID"], comDeptDutyPerson.Columns["ROW_ID"] };
            this.DataSet.Tables.Add(comDeptDutyPerson);
            this.DataSet.Relations.Add(string.Format("{0}_{1}", comDeptName, comDeptDutyPersonName), new DataColumn[] { comDept.Columns["DEPTID"] }, new DataColumn[] { comDeptDutyPerson.Columns["DEPTID"] });

            #endregion
        }


        protected override void DefineViewTemplate(DataSet dataSet)
        {
            LibBillLayout layout = new LibBillLayout(this.DataSet);
            layout.HeaderRange = layout.BuildControlGroup(0, string.Empty, new List<string>() { "DEPTID", "DEPTNAME", "SUPERDEPTID", "SORTORDER", "DEPTLEVEL" });
            layout.TabRange.Add(layout.BuildGrid(1, "岗位任职"));
            this.ViewTemplate = new LibBillTpl(this.DataSet, layout);
        }
        protected override void DefineFuncPermission()
        {
            base.DefineFuncPermission();
            this.FuncPermission = new LibFuncPermission("", this.BillType);
            this.FuncPermission.BillTypeName = string.Format("{0}Type", this.ProgId);
            //this.FuncPermission.UsingApproveRow = true;
            this.FuncPermission.UseSynchroData = false;//是否启用数据同步
        }       
    }
}
