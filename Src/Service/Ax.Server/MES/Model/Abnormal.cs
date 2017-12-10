using AxCRL.Comm.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace Ax.Server.MES.Model
{
   

    [Serializable]
    public class Region
    {
        private int _ZtwValue;
        private string _RegionValue;
        private string _RegionId;
        private string _RegionName;

        /// <summary>
        /// 区域名称
        /// </summary>
        public string RegionName
        {
            get { return _RegionName; }
            set { _RegionName = value; }
        }

        /// <summary>
        /// 区域代码
        /// </summary>
        public string RegionId
        {
            get { return _RegionId; }
            set { _RegionId = value; }
        }

        /// <summary>
        /// 区域值
        /// </summary>
        public string RegionValue
        {
            get { return _RegionValue; }
            set { _RegionValue = value; }
        }

        /// <summary>
        /// 区域号
        /// </summary>
        public int ZtwValue
        {
            get { return _ZtwValue; }
            set { _ZtwValue = value; }
        }
    }


    /// <summary>
    /// 异常主数据
    /// </summary>
    public class AbnormalMaster
    {
        public AbnormalMaster() { }
        public AbnormalMaster(DataSet ds)
        {
            DataRow dr = ds.Tables[0].Rows[0];
            _AbnormalId = LibSysUtils.ToString(dr["ABNORMALID"]);
            _AbnormalName = LibSysUtils.ToString(dr["ABNORMALNAME"]);
            _AbnormalTypeId = LibSysUtils.ToString(dr["ABNORMALTYPEID"]);
            _AbnormalTypeName = LibSysUtils.ToString(dr["ABNORMALTYPENAME"]);
            _DeptId = LibSysUtils.ToString(dr["DEPTID"]);
            _DeptName = LibSysUtils.ToString(dr["DEPTNAME"]);
        }

        private string _AbnormalId;
        private string _AbnormalName;
        private string _AbnormalTypeId;
        private string _AbnormalTypeName;
        private string _DeptId;
        private string _DeptName;

        public string DeptName
        {
            get { return _DeptName; }
            set { _DeptName = value; }
        }

        public string DeptId
        {
            get { return _DeptId; }
            set { _DeptId = value; }
        }

        public string AbnormalTypeName
        {
            get { return _AbnormalTypeName; }
            set { _AbnormalTypeName = value; }
        }

        public string AbnormalTypeId
        {
            get { return _AbnormalTypeId; }
            set { _AbnormalTypeId = value; }
        }

        public string AbnormalName
        {
            get { return _AbnormalName; }
            set { _AbnormalName = value; }
        }

        public string AbnormalId
        {
            get { return _AbnormalId; }
            set { _AbnormalId = value; }
        }
    }

    /// <summary>
    /// 子异常
    /// </summary>
    public class SubAbnormal
    {
        private string _SubAbnormalId;
        private string _SubAbnormalName;

        public string SubAbnormalName
        {
            get { return _SubAbnormalName; }
            set { _SubAbnormalName = value; }
        }

        public string SubAbnormalId
        {
            get { return _SubAbnormalId; }
            set { _SubAbnormalId = value; }
        }
    }

    /// <summary>
    /// 异常报告
    /// </summary>
    [Serializable]
    public class AbnormalReport
    {
        //private string _WorkStationId=string.Empty;
        //private string _WorkStationName = string.Empty;

        //public string WorkStationName
        //{
        //    get { return _WorkStationName; }
        //    set { _WorkStationName = value; }
        //}

        ///// <summary>
        ///// 工作站
        ///// </summary>
        //public string WorkStationId
        //{
        //    get { return _WorkStationId; }
        //    set { _WorkStationId = value; }
        //}


        public AbnormalProtoType abnormalPrptoType = AbnormalProtoType.Produce;
        //异常报告单号
        string _BillNo = string.Empty;
        public string BillNo
        {
            get { return _BillNo; }
            set { _BillNo = value; }
        }
        //类型id
        string _TypeId = string.Empty;
        public string TypeId
        {
            get { return _TypeId; }
            set { _TypeId = value; }
        }
        //异常类型id
        string _AbnormalTypeId = string.Empty;

        public string AbnormalTypeId
        {
            get { return _AbnormalTypeId; }
            set { _AbnormalTypeId = value; }
        }
        //异常id
        string _AbnormalId = string.Empty;

        public string AbnormalId
        {
            get { return _AbnormalId; }
            set { _AbnormalId = value; }
        }

        string _AbnormalName = string.Empty;

        public string AbnormalName
        {
            get { return _AbnormalName; }
            set { _AbnormalName = value; }
        }
        //责任部门
        string _DeptId = string.Empty;

        public string DeptId
        {
            get { return _DeptId; }
            set { _DeptId = value; }
        }
        //异常描述
        string _AbnormalDesc = string.Empty;

        public string AbnormalDesc
        {
            get { return _AbnormalDesc; }
            set { _AbnormalDesc = value; }
        }
        //责任人
        string _PersonId = string.Empty;

        public string PersonId
        {
            get { return _PersonId; }
            set { _PersonId = value; }
        }

        string _PersonName = string.Empty;
        public string PersonName
        {
            get { return _PersonName; }
            set { _PersonName = value; }
        }

        string _DestPhoneNo = string.Empty;
        public string DestPhoneNo
        {
            get { return _DestPhoneNo; }
            set { _DestPhoneNo = value; }
        }
        //报告人
        string _FromPersonId = string.Empty;

        public string FromPersonId
        {
            get { return _FromPersonId; }
            set { _FromPersonId = value; }
        }
        //报告人部门
        string _FromDeptId = string.Empty;

        public string FromDeptId
        {
            get { return _FromDeptId; }
            set { _FromDeptId = value; }
        }
        //影响生产
        int _AffectProduceState;

        public int AffectProduceState
        {
            get { return _AffectProduceState; }
            set { _AffectProduceState = value; }
        }
        //影响人数
        decimal _AffectPersonNum = 0;

        public decimal AffectPersonNum
        {
            get { return _AffectPersonNum; }
            set { _AffectPersonNum = value; }
        }
        //影响工时
        double _AffectTime;

        public double AffectTime
        {
            get { return _AffectTime; }
            set { _AffectTime = value; }
        }
        //开始时间
        long _StartTime;

        public long StartTime
        {
            get { return _StartTime; }
            set { _StartTime = value; }
        }
        //结束时间
        long _EndTime;

        public long EndTime
        {
            get { return _EndTime; }
            set { _EndTime = value; }
        }
        //处理状态
        int _DealWithState;

        public int DealWithState
        {
            get { return _DealWithState; }
            set { _DealWithState = value; }
        }
        //来源标识
        string _FromMark = string.Empty;

        public string FromMark
        {
            get { return _FromMark; }
            set { _FromMark = value; }
        }

        //系统创建ISSYSTEMBUILD
        int _IsSystemBuilD;

        public int IsSystemBuilD
        {
            get { return _IsSystemBuilD; }
            set { _IsSystemBuilD = value; }
        }

    }

    /// <summary>
    /// 异常追踪
    /// </summary>
    [Serializable]
    public class AbnormalTrace
    {
        private string _fromBillNo;
        private Int64 _PlanEndTime;
        private string _AbnormalReasonId;
        private string _PersonId;
        private string _DealwithPersonId;
        private string _Solution;
        private int _DealwithState;
        private string _TypeId;

        public string TypeId
        {
            get { return _TypeId; }
            set { _TypeId = value; }
        }

        public int DealwithState
        {
            get { return _DealwithState; }
            set { _DealwithState = value; }
        }

        public string Solution
        {
            get { return _Solution; }
            set { _Solution = value; }
        }

        public string DealwithPersonId
        {
            get { return _DealwithPersonId; }
            set { _DealwithPersonId = value; }
        }

        public string PersonId
        {
            get { return _PersonId; }
            set { _PersonId = value; }
        }

        public string AbnormalReasonId
        {
            get { return _AbnormalReasonId; }
            set { _AbnormalReasonId = value; }
        }

        public Int64 PlanEndTime
        {
            get { return _PlanEndTime; }
            set { _PlanEndTime = value; }
        }

        public string FromBillNo
        {
            get { return _fromBillNo; }
            set { _fromBillNo = value; }
        }
    }

    /// <summary>
    /// 异常属性类型
    /// </summary>
    [Serializable]
    public enum AbnormalProtoType
    {
        ReceiveTask = 0,
        Check = 1,
        StockIn = 2,
        TakeMat = 3,
        Send = 4,
        ReceiveMaterial = 5,
        Produce = 6

    }

    /// <summary>
    /// 异常过账
    /// </summary>
    [Serializable]
    public class AbnormalPost
    {
        private string _ReportBillNo = string.Empty;
        private string _Record = string.Empty;
        private string _TraceBillNo = string.Empty;
        private string _WorkStationId = string.Empty;
        private string _AbnormalTypeId = string.Empty;
        private string _AbnormalId = string.Empty;
        private int _State;
        private string _SubAbnormalId;
        private string _RegionId;

        public string RegionId
        {
            get { return _RegionId; }
            set { _RegionId = value; }
        }

        /// <summary>
        /// 子异常
        /// </summary>
        public string SubAbnormalId
        {
            get { return _SubAbnormalId; }
            set { _SubAbnormalId = value; }
        }

        public int State
        {
            get { return _State; }
            set { _State = value; }
        }

        public string AbnormalId
        {
            get { return _AbnormalId; }
            set { _AbnormalId = value; }
        }

        public string AbnormalTypeId
        {
            get { return _AbnormalTypeId; }
            set { _AbnormalTypeId = value; }
        }

        public string WorkStationId
        {
            get { return _WorkStationId; }
            set { _WorkStationId = value; }
        }

        public string TraceBillNo
        {
            get { return _TraceBillNo; }
            set { _TraceBillNo = value; }
        }

        public string Record
        {
            get { return _Record; }
            set { _Record = value; }
        }

        public string ReportBillNo
        {
            get { return _ReportBillNo; }
            set { _ReportBillNo = value; }
        }
    }

    public enum AbnormalState
    {
        Start,      //异常发起
        Handle,     //异常处理
        End         //异常结束
    }

    [Serializable]
    public class AnDengBillData
    {
        private string _PersonId = string.Empty;
        private string _PersonName = string.Empty;
        private string _AbnormalTypeId = string.Empty;
        private string _AbnormalId = string.Empty;
        private string _AbnormalDesc = string.Empty;
        private string _DestPhoneNo = string.Empty;
        private string _DeptId = string.Empty;
        private AbnormalProtoType _ProtoType;
        private string _SubAbnormalId = string.Empty;
        private AbnormalState _State;
        //private string _WorkStationId = string.Empty;
        private string _AbnormalName = string.Empty;
        private string _SubAbnormalName = string.Empty;

        public string SubAbnormalName
        {
            get { return _SubAbnormalName; }
            set { _SubAbnormalName = value; }
        }

        public string AbnormalName
        {
            get { return _AbnormalName; }
            set { _AbnormalName = value; }
        }

        //public string WorkStationId
        //{
        //    get { return _WorkStationId; }
        //    set { _WorkStationId = value; }
        //}

        public AbnormalState State
        {
            get { return _State; }
            set { _State = value; }
        }

        public string SubAbnormalId
        {
            get { return _SubAbnormalId; }
            set { _SubAbnormalId = value; }
        }

        public AbnormalProtoType ProtoType
        {
            get { return _ProtoType; }
            set { _ProtoType = value; }
        }

        public string DeptId
        {
            get { return _DeptId; }
            set { _DeptId = value; }
        }

        public string DestPhoneNo
        {
            get { return _DestPhoneNo; }
            set { _DestPhoneNo = value; }
        }

        public string AbnormalDesc
        {
            get { return _AbnormalDesc; }
            set { _AbnormalDesc = value; }
        }

        public string AbnormalId
        {
            get { return _AbnormalId; }
            set { _AbnormalId = value; }
        }

        public string AbnormalTypeId
        {
            get { return _AbnormalTypeId; }
            set { _AbnormalTypeId = value; }
        }

        public string PersonName
        {
            get { return _PersonName; }
            set { _PersonName = value; }
        }

        public string PersonId
        {
            get { return _PersonId; }
            set { _PersonId = value; }
        }
    }

    public class AnDonData
    {
        private string _anDonId;

        public string AnDonId
        {
            get { return _anDonId; }
            set { _anDonId = value; }
        }
        private string _anDonName;

        public string AnDonName
        {
            get { return _anDonName; }
            set { _anDonName = value; }
        }

        private string _abnormalId;

        public string AbnormalId
        {
            get { return _abnormalId; }
            set { _abnormalId = value; }
        }

    }
}