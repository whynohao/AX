using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MES_Sys.UtilsBcf.Com
{
    public sealed class ComCommonalityData
    {
        /// <summary>
        /// 用户人员组主数据中的业务属性字段的值
        /// </summary>
        public static readonly string[] PERSONGROUPATTRIBUTE =new string[] { "计划", "生产", "采购", "品质", "仓储", "设备" };
        /// <summary>
        /// 异常报告单异常属性的值
        /// </summary>
        public static readonly string[] ABNORMALPROTOTYPE = new string[] { "收货环节", "检验环节", "入库环节", "备料环节", "派料环节", "接收环节", "生产过程环节", "无样品环节", "无成本环节", "信息配套环节", "关键物料环节", "设备作业执行环节" };

        /// <summary>
        /// 车辆主数据中的业务属性字段的值
        /// </summary>
        public static readonly string[] BIZATTR = new string[] { "厂内", "厂外" };

        /// <summary>
        /// 节拍时间单位（秒、分）
        /// </summary>
        public static readonly string[] BEATTIMEUNIT = new string[] { "秒", "分" };

        /// <summary>
        /// 时间单位（秒、分、时）
        /// </summary>
        public static readonly string[] TIMEUNIT = new string[] { "秒", "分", "时" };

        /// <summary>
        /// 终端设备类型
        /// </summary>
        public static readonly string[] TERMINALTYPE = new string[] { "电脑", "PDA", "云终端", "瘦客户机", "立式一体机", "读写器" };
    }
}
