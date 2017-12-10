/**********************************************************************
 * CopyRight 2016 杭州集控科技有限公司 版权所有
 * 功能描述：功能模块的表单附件与对应的文档库目录编号相关的处理类
 * 创建标识：Zhangkj 2017/01/05
 * 
************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jikon.MES_Dm.DMCommon
{
    /// <summary>
    /// 功能模块的表单附件与对应的文档库目录编号相关的处理类
    /// </summary>
    public class ProgDirInfo
    {
        private static string _BillAttachmentTopDirId = string.Empty;

        /// <summary>
        /// 表单附件一级公共目录的名称
        /// </summary>
        public const string BillAttachmentTopDirName = "表单附件";
        /// <summary>
        /// 获取表单附件一级公共目录的编码
        /// 初始为空
        /// </summary>
        public static string BillAttachmentTopDirId
        {
            get { return _BillAttachmentTopDirId; }
            set { _BillAttachmentTopDirId = value; }
        }

        /// <summary>
        /// 功能模块标识与对应的表单附件目录信息字典
        /// </summary>
        private static Dictionary<string, ProgDirInfo> DicProgDirInfo = new Dictionary<string, ProgDirInfo>();
        private static object _lockObj = new object();
        /// <summary>
        /// 获取功能代码对应的表单附件目录信息
        /// </summary>
        /// <param name="progId"></param>
        /// <returns></returns>
        public static ProgDirInfo GetDirInfo(string progId)
        {
            if (string.IsNullOrEmpty(progId))
                return null;
            lock (_lockObj)
            {
                if (DicProgDirInfo.ContainsKey(progId))
                    return DicProgDirInfo[progId];
                else
                    return null;
            }
        }
        /// <summary>
        /// 添加或更新
        /// </summary>
        /// <param name="progDirInfo"></param>
        public static void AddDirInfo(ProgDirInfo progDirInfo)
        {
            if (progDirInfo == null || string.IsNullOrEmpty(progDirInfo.ProgId))
                return;
            lock (_lockObj)
            {
                if (DicProgDirInfo.ContainsKey(progDirInfo.ProgId))
                    DicProgDirInfo[progDirInfo.ProgId] = progDirInfo;
                else
                    DicProgDirInfo.Add(progDirInfo.ProgId, progDirInfo);
            }
        }


        private string _ProgId = string.Empty;
        private string _ProgDisplayName = string.Empty;
        private string _DirId = string.Empty;
        /// <summary>
        /// 功能模块下的按日期的子目录名称，与文档库中的目录编号的对应关系
        /// </summary>
        private Dictionary<string, string> _DicDayDirIds = new Dictionary<string, string>();
        private object _lockItemObj = new object();

        /// <summary>
        /// 功能模块名称
        /// </summary>
        public string ProgId
        {
            get { return _ProgId; }
            set { _ProgId = value; }
        }
        /// <summary>
        /// 模块显示名称
        /// </summary>
        public string ProgDisplayName
        {
            get { return _ProgDisplayName; }
            set { _ProgDisplayName = value; }
        }
        /// <summary>
        /// 功能模块对应的目录名称。如果为空则说明还不存在或未从数据库读取
        /// </summary>
        public string DirId
        {
            get { return _DirId; }
            set { _DirId = value; }
        }

        public ProgDirInfo(string progId)
        {
            this._ProgId = progId;
        }

        /// <summary>
        /// 获取按日期命名的目录名称与目录编号的对应关系
        /// </summary>
        /// <param name="dayDirName"></param>
        /// <returns></returns>
        public string GetDayDirId(string dayDirName)
        {
            if (string.IsNullOrEmpty(dayDirName))
                return string.Empty;
            lock (_lockItemObj)
            {
                if (_DicDayDirIds.ContainsKey(dayDirName))
                    return _DicDayDirIds[dayDirName];
                else
                    return string.Empty;
            }
        }
        /// <summary>
        /// 添加或更新子目录名称与目录编号的对应关系
        /// </summary>
        /// <param name="dayDirName"></param>
        /// <param name="dayDirId"></param>
        public void AddDayDirId(string dayDirName,string dayDirId)
        {
            if (string.IsNullOrEmpty(dayDirName) || string.IsNullOrEmpty(dayDirId))
                return;
            lock (_lockItemObj)
            {
                if (_DicDayDirIds.ContainsKey(dayDirName))
                    _DicDayDirIds[dayDirName] = dayDirId;
                else
                    _DicDayDirIds.Add(dayDirName, dayDirId);
            }
        }
    }
}
