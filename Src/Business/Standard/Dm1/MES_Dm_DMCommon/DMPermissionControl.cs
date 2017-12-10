/**********************************************************************
 * CopyRight 2016 杭州集控科技有限公司 版权所有
 * 功能描述：文档管理模块的权限处理控制类。
 * 创建标识：Zhangkj 2016/12/14
 * 
************************************************************************/
using AxCRL.Core.Cache;
using AxCRL.Core.Comm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Jikon.MES_Dm.DMCommon
{
    /// <summary>
    /// 文档管理的权限处理控制类
    /// 单例模式
    /// </summary>
    public class DMPermissionControl
    {
        private static DMPermissionControl _Default = null;
        private static object _LockObj = new object();

        private DMPermissionControl()
        {
            
        }

        public static DMPermissionControl Default
        {
            get
            {
                if (_Default == null)
                {
                    lock (_LockObj)
                    {
                        if (_Default == null)
                        {
                            _Default = new DMPermissionControl();
                        }
                    }
                }
                return _Default;
            }
        }
        /// <summary>
        /// 检查用户是否具有对于指定目录（或及指定文档）的指定权限
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="dirId"></param>
        /// <param name="docId">如果参数为空则指针对目录的检查</param>
        /// <param name="funcPermission"></param>
        /// <returns></returns>
        public bool HasPermission(LibHandle handle, string dirId, string docId, DMFuncPermissionEnum funcPermission)
        {
            bool ret = false;
            if (handle == LibHandleCache.Default.GetSystemHandle()||string.IsNullOrEmpty(handle.UserId)||handle.UserId=="admin")
            {
                ret = true;
            }
            else
            {
                string personId = handle.PersonId;
                DMUserPermission userPermission = DMUserPermissionCache.Default.GetCacheItem(personId);
                if (userPermission.IsUnlimited)
                {
                    ret = true;
                }
                else
                    return userPermission.CheckCan(dirId, docId, funcPermission);
            }
            return ret;
        }
        /// <summary>
        /// 获取用户对于指定文档的权限集合
        /// </summary>
        /// <param name="userHandle">用户会话标识</param>
        /// <param name="docId"></param>
        /// <returns></returns>
        public List<DMFuncPermissionEnum> GetPermissionOf(string userHandle,string docId)
        {
            if (string.IsNullOrEmpty(userHandle) || string.IsNullOrEmpty(docId))
                return new List<DMFuncPermissionEnum>() ;//参数非法，返回空
            LibHandle libHandle = LibHandleCache.Default.GetCurrentHandle(userHandle) as LibHandle;
            if (libHandle == null)
            {
                return new List<DMFuncPermissionEnum>();//用户句柄无效
            }
            List<DMFuncPermissionEnum> listPer = new List<DMFuncPermissionEnum>();
            //添加所有权限
            listPer.AddRange(new DMFuncPermissionEnum[] {
                    DMFuncPermissionEnum.Browse,
                    DMFuncPermissionEnum.Add,
                    DMFuncPermissionEnum.Edit,
                    DMFuncPermissionEnum.Delete,
                    DMFuncPermissionEnum.Read,
                    DMFuncPermissionEnum.Print,
                    DMFuncPermissionEnum.Download,
                    DMFuncPermissionEnum.Move,
                    DMFuncPermissionEnum.SetVersion,
                    DMFuncPermissionEnum.Subscribe,
                    DMFuncPermissionEnum.Lend,
                    DMFuncPermissionEnum.Link,
                    DMFuncPermissionEnum.Associate,
                    DMFuncPermissionEnum.Comment,
                    DMFuncPermissionEnum.Rename,
                    DMFuncPermissionEnum.Replace,
                    DMFuncPermissionEnum.Fallback,
                });
            DirLinkAddress dirLink = new DirLinkAddress(docId);
            if (libHandle.UserId.Equals("admin") || (DirTypeEnum)dirLink.DirType == DirTypeEnum.Private||libHandle==LibHandleCache.Default.GetSystemHandle())
                return listPer;//具有所有权限           
            DMUserPermission userPer = DMUserPermissionCache.Default.GetCacheItem(libHandle.PersonId);
            listPer = (from item in listPer
                       where userPer.CheckCan(dirLink.ParentDirIdList, docId, item)             //筛选
                       select item).ToList();
            return listPer;
        }
        /// <summary>
        /// 检查用户对于一批文档是否具有指定权限，返回具有权限的文档Id列表
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="permission"></param>
        /// <param name="toFilterDocIdList"></param>
        /// <returns></returns>
        public List<string> FilterDocIds(LibHandle handle, DMFuncPermissionEnum permission, List<string> toFilterDocIdList)
        {
            if (handle == LibHandleCache.Default.GetSystemHandle() || string.IsNullOrEmpty(handle.UserId) || handle.UserId == "admin")
            {
                return toFilterDocIdList;//系统用户则直接原样返回
            }
            string personId = handle.PersonId;
            DMUserPermission userPermission = DMUserPermissionCache.Default.GetCacheItem(personId);
            if(userPermission.IsUnlimited)
                return toFilterDocIdList;//用户权限不受限制则直接原样返回

            Dictionary<string, List<DocInfo>> dicDocId_DirIds = DirLinkAddress.GetParentDirIdsForDocs(toFilterDocIdList);
            if (dicDocId_DirIds == null)
                return null;

            List<string> resultList = new List<string>();
            Dictionary<string, List<string>> tempDic = new Dictionary<string, List<string>>();
            foreach (KeyValuePair<string, List<DocInfo>> item in dicDocId_DirIds)
            {
                foreach (DocInfo temp in item.Value)
                {
                    if(temp.DirType == DirTypeEnum.Private && temp.CreatorId == personId)
                    {
                        if (!resultList.Contains(item.Key))
                        {
                            resultList.Add(item.Key);
                            break;
                        }
                    }
                    else
                    {
                        if (tempDic.ContainsKey(item.Key))
                            tempDic[item.Key].Add(temp.DirId);
                        else
                            tempDic.Add(item.Key, new List<string>() { temp.DirId });
                    }
                }
            }
            try {
                //使用每个文档的标识和父目录标识列表检查是否可用权限
                resultList.AddRange((from item in toFilterDocIdList
                              where tempDic.ContainsKey(item) &&  userPermission.CheckCan(tempDic[item], item, permission)
                              select item).ToList());
            }
            catch(Exception exp)
            {
                DMCommonMethod.WriteLog("FilterDocIds", exp.Message);
            }
            return resultList;
        }
        /// <summary>
        /// 检查用户对于一批目录是否具有指定权限，返回具有权限的目录Id列表
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="permission"></param>
        /// <param name="toFilterDirIdList"></param>
        /// <returns></returns>
        public List<string> FilterDirIds(LibHandle handle, DMFuncPermissionEnum permission, List<string> toFilterDirIdList)
        {
            if (handle == LibHandleCache.Default.GetSystemHandle() || string.IsNullOrEmpty(handle.UserId) || handle.UserId == "admin")
            {
                return toFilterDirIdList;//系统用户则直接原样返回
            }
            string personId = handle.PersonId;
            DMUserPermission userPermission = DMUserPermissionCache.Default.GetCacheItem(personId);
            if (userPermission.IsUnlimited)
                return toFilterDirIdList;//用户权限不受限制则直接原样返回

            Dictionary<string, List<string>> dicDirId_DirIds = DirLinkAddress.GetDirIdsForDirs(toFilterDirIdList);
            if (dicDirId_DirIds == null )
                return new List<string>();

            List<string> resultList = new List<string>();
            try
            {
                //使用每个目录的目录标识列表（含自身和所有父级目录）检查是否可用权限
                resultList = (from item in toFilterDirIdList
                              where dicDirId_DirIds.ContainsKey(item) && userPermission.CheckCan(dicDirId_DirIds[item], string.Empty, permission)
                              select item).ToList();
            }
            catch (Exception exp)
            {
                DMCommonMethod.WriteLog("FilterDocIds", exp.Message);
            }
            return resultList;
        }
        /// <summary>
        /// 根据权限值构建权限子项
        /// </summary>
        /// <param name="permission"></param>
        /// <returns></returns>
        public Dictionary<int, PowerInfo> BuildPowerInfo(bool isDir,int permission)
        {
            Dictionary<int, PowerInfo> ret = new Dictionary<int, PowerInfo>();
            if(isDir)
            {
                ret.Add((int)DMFuncPermissionEnum.Manage, new PowerInfo("管理", (permission & (int)DMFuncPermissionEnum.Manage) == (int)DMFuncPermissionEnum.Manage));
                ret.Add((int)DMFuncPermissionEnum.Browse, new PowerInfo("浏览", (permission & (int)DMFuncPermissionEnum.Browse) == (int)DMFuncPermissionEnum.Browse));
                ret.Add((int)DMFuncPermissionEnum.Export, new PowerInfo("导出", (permission & (int)DMFuncPermissionEnum.Export) == (int)DMFuncPermissionEnum.Export));
                ret.Add((int)DMFuncPermissionEnum.Add, new PowerInfo("新增", (permission & (int)DMFuncPermissionEnum.Add) == (int)DMFuncPermissionEnum.Add));
                ret.Add((int)DMFuncPermissionEnum.Edit, new PowerInfo("修改", (permission & (int)DMFuncPermissionEnum.Edit) == (int)DMFuncPermissionEnum.Edit));
                ret.Add((int)DMFuncPermissionEnum.Delete, new PowerInfo("删除", (permission & (int)DMFuncPermissionEnum.Delete) == (int)DMFuncPermissionEnum.Delete));
                ret.Add((int)DMFuncPermissionEnum.Read, new PowerInfo("阅读", (permission & (int)DMFuncPermissionEnum.Read) == (int)DMFuncPermissionEnum.Read));
                ret.Add((int)DMFuncPermissionEnum.Print, new PowerInfo("打印", (permission & (int)DMFuncPermissionEnum.Print) == (int)DMFuncPermissionEnum.Print));
                ret.Add((int)DMFuncPermissionEnum.Upload, new PowerInfo("上传", (permission & (int)DMFuncPermissionEnum.Upload) == (int)DMFuncPermissionEnum.Upload));
                ret.Add((int)DMFuncPermissionEnum.Download, new PowerInfo("下载", (permission & (int)DMFuncPermissionEnum.Download) == (int)DMFuncPermissionEnum.Download));
                ret.Add((int)DMFuncPermissionEnum.Move, new PowerInfo("移动", (permission & (int)DMFuncPermissionEnum.Move) == (int)DMFuncPermissionEnum.Move));
                ret.Add((int)DMFuncPermissionEnum.SetVersion, new PowerInfo("设定版本", (permission & (int)DMFuncPermissionEnum.SetVersion) == (int)DMFuncPermissionEnum.SetVersion));
                ret.Add((int)DMFuncPermissionEnum.Subscribe, new PowerInfo("订阅", (permission & (int)DMFuncPermissionEnum.Subscribe) == (int)DMFuncPermissionEnum.Subscribe));
                ret.Add((int)DMFuncPermissionEnum.Lend, new PowerInfo("借出", (permission & (int)DMFuncPermissionEnum.Lend) == (int)DMFuncPermissionEnum.Lend));
                ret.Add((int)DMFuncPermissionEnum.Link, new PowerInfo("发送链接", (permission & (int)DMFuncPermissionEnum.Link) == (int)DMFuncPermissionEnum.Link));
                ret.Add((int)DMFuncPermissionEnum.Associate, new PowerInfo("关联", (permission & (int)DMFuncPermissionEnum.Link) == (int)DMFuncPermissionEnum.Associate));
                ret.Add((int)DMFuncPermissionEnum.Comment, new PowerInfo("评论", (permission & (int)DMFuncPermissionEnum.Comment) == (int)DMFuncPermissionEnum.Comment));
                ret.Add((int)DMFuncPermissionEnum.Rename, new PowerInfo("重命名", (permission & (int)DMFuncPermissionEnum.Rename) == (int)DMFuncPermissionEnum.Rename));
                ret.Add((int)DMFuncPermissionEnum.Replace, new PowerInfo("替换", (permission & (int)DMFuncPermissionEnum.Replace) == (int)DMFuncPermissionEnum.Replace));
                ret.Add((int)DMFuncPermissionEnum.Fallback, new PowerInfo("回退", (permission & (int)DMFuncPermissionEnum.Fallback) == (int)DMFuncPermissionEnum.Fallback));
            }
            else
            {
                ret.Add((int)DMFuncPermissionEnum.Manage, new PowerInfo("管理", (permission & (int)DMFuncPermissionEnum.Manage) == (int)DMFuncPermissionEnum.Manage));//文档也可单独设置管理权限
                ret.Add((int)DMFuncPermissionEnum.Browse, new PowerInfo("浏览", (permission & (int)DMFuncPermissionEnum.Browse) == (int)DMFuncPermissionEnum.Browse));
                ret.Add((int)DMFuncPermissionEnum.Add, new PowerInfo("新增", (permission & (int)DMFuncPermissionEnum.Add) == (int)DMFuncPermissionEnum.Add));
                ret.Add((int)DMFuncPermissionEnum.Edit, new PowerInfo("修改", (permission & (int)DMFuncPermissionEnum.Edit) == (int)DMFuncPermissionEnum.Edit));
                ret.Add((int)DMFuncPermissionEnum.Delete, new PowerInfo("删除", (permission & (int)DMFuncPermissionEnum.Delete) == (int)DMFuncPermissionEnum.Delete));
                ret.Add((int)DMFuncPermissionEnum.Read, new PowerInfo("阅读", (permission & (int)DMFuncPermissionEnum.Read) == (int)DMFuncPermissionEnum.Read));
                ret.Add((int)DMFuncPermissionEnum.Print, new PowerInfo("打印", (permission & (int)DMFuncPermissionEnum.Print) == (int)DMFuncPermissionEnum.Print));                
                ret.Add((int)DMFuncPermissionEnum.Download, new PowerInfo("下载", (permission & (int)DMFuncPermissionEnum.Download) == (int)DMFuncPermissionEnum.Download));
                ret.Add((int)DMFuncPermissionEnum.Move, new PowerInfo("移动", (permission & (int)DMFuncPermissionEnum.Move) == (int)DMFuncPermissionEnum.Move));
                ret.Add((int)DMFuncPermissionEnum.SetVersion, new PowerInfo("设定版本", (permission & (int)DMFuncPermissionEnum.SetVersion) == (int)DMFuncPermissionEnum.SetVersion));
                ret.Add((int)DMFuncPermissionEnum.Subscribe, new PowerInfo("订阅", (permission & (int)DMFuncPermissionEnum.Subscribe) == (int)DMFuncPermissionEnum.Subscribe));
                ret.Add((int)DMFuncPermissionEnum.Lend, new PowerInfo("借出", (permission & (int)DMFuncPermissionEnum.Lend) == (int)DMFuncPermissionEnum.Lend));
                ret.Add((int)DMFuncPermissionEnum.Link, new PowerInfo("发送链接", (permission & (int)DMFuncPermissionEnum.Link) == (int)DMFuncPermissionEnum.Link));
                ret.Add((int)DMFuncPermissionEnum.Associate, new PowerInfo("关联", (permission & (int)DMFuncPermissionEnum.Link) == (int)DMFuncPermissionEnum.Associate));
                ret.Add((int)DMFuncPermissionEnum.Comment, new PowerInfo("评论", (permission & (int)DMFuncPermissionEnum.Comment) == (int)DMFuncPermissionEnum.Comment));
                ret.Add((int)DMFuncPermissionEnum.Rename, new PowerInfo("重命名", (permission & (int)DMFuncPermissionEnum.Rename) == (int)DMFuncPermissionEnum.Rename));
                ret.Add((int)DMFuncPermissionEnum.Replace, new PowerInfo("替换", (permission & (int)DMFuncPermissionEnum.Replace) == (int)DMFuncPermissionEnum.Replace));
                ret.Add((int)DMFuncPermissionEnum.Fallback, new PowerInfo("回退", (permission & (int)DMFuncPermissionEnum.Fallback) == (int)DMFuncPermissionEnum.Fallback));
            }            
            return ret;
        }
        
    }
    /// <summary>
    /// 权限项信息
    /// 与前端交互的数据契约
    /// </summary>
    [DataContract]
    public class PowerInfo
    {
        private string _DisplayText;
        private bool _CanUse;

        public PowerInfo(string displayText, bool canUse)
        {
            this.DisplayText = displayText;
            this.CanUse = canUse;
        }

        [DataMember]
        public bool CanUse
        {
            get { return _CanUse; }
            set { _CanUse = value; }
        }

        [DataMember]
        public string DisplayText
        {
            get { return _DisplayText; }
            set { _DisplayText = value; }
        }
    }
}
