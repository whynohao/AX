/**********************************************************************
 * CopyRight 2016 杭州集控科技有限公司 版权所有
 * 功能描述：文档管理模块的用户权限类。
 * 创建标识：Zhangkj 2016/12/14
 * 
************************************************************************/
using AxCRL.Comm.Utils;
using AxCRL.Core.Comm;
using AxCRL.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jikon.MES_Dm.DMCommon
{
    /// <summary>
    /// 文档管理模块用户权限类
    /// </summary>
    public class DMUserPermission
    {
        /// <summary>
        /// 锁定目录权限和文档权限的明细值字典项
        /// </summary>
        private object _LockObjOfDicDirAndDoc = new object();

        private string _PersonId = null;
        private bool _IsUnlimited = false;

        /// <summary>
        /// 目录标识与该目录下用户具有的操作权限集合（按位“与”形成Int32形式）
        /// </summary>
        private Dictionary<string, int> _DicDirectoryPowers = new Dictionary<string, int>();
        /// <summary>
        /// 文档标识与该文档用户具有的操作权限集合（按位“与”形成Int32形式）
        /// </summary>
        private Dictionary<string, int> _DicDocumentPowers = new Dictionary<string, int>();

        /// <summary>
        /// 获取人员ID
        /// </summary>
        public string PersonId
        {
            get { return _PersonId; }
        }
        /// <summary>
        /// 是否是无限制的权限
        /// </summary>
        public bool IsUnlimited
        {
            get { return _IsUnlimited; }
            set { _IsUnlimited = value; }
        }
        /// <summary>
        /// 此公共属性项仅用于RedisCache缓存数据，不可直接访问而修改或读取字典值
        /// Zhangkj 20170208
        /// </summary>
        public Dictionary<string, int> DicDirectoryPowers
        {
            get { return _DicDirectoryPowers; }
            set { _DicDirectoryPowers = value; }
        }
        /// <summary>
        /// 此公共属性项仅用于RedisCache缓存数据，不可直接访问而修改或读取字典值
        /// Zhangkj 20170208
        /// </summary>
        public Dictionary<string, int> DicDocumentPowers
        {
            get { return _DicDocumentPowers; }
            set { _DicDocumentPowers = value; }
        }

        public DMUserPermission(string personId)
        {
            this._PersonId = personId;
        }
        /// <summary>
        /// 为目录或文档添加权限（仅内存，如需保存到数据库需要通过表单的修改进行添加）
        /// </summary>
        /// <param name="isDir"></param>
        /// <param name="itemId"></param>
        /// <param name="permissionValue"></param>
        public void AddPermission(bool isDir, string itemId, int permissionValue)
        {
            if (string.IsNullOrEmpty(itemId))
                return;
            lock (_LockObjOfDicDirAndDoc)
            {
                if(isDir)
                {
                    if (_DicDirectoryPowers.ContainsKey(itemId))
                        _DicDirectoryPowers[itemId] |= permissionValue;
                    else
                        _DicDirectoryPowers.Add(itemId, permissionValue);
                }
                else
                {
                    if (_DicDocumentPowers.ContainsKey(itemId))
                        _DicDocumentPowers[itemId] |= permissionValue;
                    else
                        _DicDocumentPowers.Add(itemId, permissionValue);
                }
            }
        }
        /// <summary>
        /// 根据用户标识，刷新用户具有的目录和文档权限
        /// </summary>
        public void RefreshUserPower()
        {
            LibDataAccess dataAccess = new LibDataAccess();
            //从数据库中找用户或用户所在的部门对目录或文档拥有的权限
            string sql = " select 1 as IsDir, 1 as IsDept,a.DIRID as ItemId, OPERATEMARK from DMDIRPERMISSION a left join COMDEPT b on a.BELONGID=b.DEPTID            " +
                         " 														                                   left join COMPERSON c on c.DEPTID=b.DEPTID         " +
                        " 													                                   where a.BELONGTYPE=0 and c.PERSONID='" + PersonId + "' " +    //所属部门对目录的权限
                         "  union                                                                                                                                     " +
                         "  select 1 as IsDir,0 as IsDept,a.DIRID as ItemId, OPERATEMARK from DMDIRPERMISSION a left join COMPERSON b on a.BELONGID=b.PERSONID        " +
                        " 														                                where a.BELONGTYPE=2 and b.PERSONID='" + PersonId + "'" +      //个人对目录的权限
                         "  union                                                                                                                                     " +
                         "  select 0 as IsDir,1 as IsDept,a.DOCID as ItemId, OPERATEMARK from DMDOCPERMISSION a left join COMDEPT b on a.BELONGID=b.DEPTID            " +
                        " 													                                   left join COMPERSON c on c.DEPTID=b.DEPTID             " +
                        " 													                                   where a.BELONGTYPE=0 and c.PERSONID='" + PersonId + "' " +   //所属部门对文档的权限
                         "  union                                                                                                                                     " +
                         "  select 0 as IsDir,0 as IsDept,a.DOCID as ItemId, OPERATEMARK from DMDOCPERMISSION a left join COMPERSON b on a.BELONGID=b.PERSONID        " +    //个人对目录的权限
                         "                                                                            where a.BELONGTYPE=2 and b.PERSONID='" + PersonId + "' ";
            DataSet ds = dataAccess.ExecuteDataSet(sql);
            string dirId = string.Empty;
            string docId = string.Empty;
            int permission = 0;
            lock (_LockObjOfDicDirAndDoc)
            {
                this._DicDirectoryPowers.Clear();
                this._DicDocumentPowers.Clear();
                if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                    return;
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    permission = LibSysUtils.ToInt32(row["OPERATEMARK"]);
                    if (LibSysUtils.ToInt32(row["IsDir"]) == 1)
                    {
                        //目录权限
                        dirId = LibSysUtils.ToString(row["ItemId"]);
                        if (_DicDirectoryPowers.ContainsKey(dirId) == false)
                            _DicDirectoryPowers.Add(dirId, permission);
                        else
                        {
                            _DicDirectoryPowers[dirId] |= permission;//如果有重复则直接相或，应该是部门和个人对相同的目录都设置了权限。个人可继承部门权限
                        }
                    }
                    else
                    {
                        //文档权限
                        docId = LibSysUtils.ToString(row["ItemId"]);
                        if (_DicDocumentPowers.ContainsKey(docId) == false)
                            _DicDocumentPowers.Add(docId, permission);
                        else
                        {
                            _DicDocumentPowers[docId] |= permission;//如果有重复则直接相或，应该是部门和个人对相同的文档都设置了权限。个人可继承部门权限
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 检查是否可以浏览指定目录
        /// 如果目录自身有管理或浏览权限、或者目录下的某个目录（无论是子目录还是孙目录等）有管理或者浏览权限、或者下面的某个文档有浏览或管理权限，则可以浏览   
        /// </summary>
        /// <param name="dirId"></param>
        /// <returns></returns>
        public bool CheckDirCanBrowse(string dirId, DirLinkAddress dirLink = null)
        {
            try
            {
                if (string.IsNullOrEmpty(dirId))
                    return false;
                if (dirLink == null)
                    dirLink = new DirLinkAddress(dirId, new LibDataAccess());
               
                //合并权限
                int mergePermission = 0;
                List<string> dirIdList = dirLink.SubDirIdList;
                dirIdList.Add(dirId);

                dirLink.GetDocIds();//文档编号列表需要单独获取
                List<string> docIdList = dirLink.DocIdList;

                lock (this._LockObjOfDicDirAndDoc)
                {
                    foreach (string itemId in dirIdList)
                    {
                        if (this._DicDirectoryPowers.ContainsKey(itemId))
                        {
                            mergePermission |= this._DicDirectoryPowers[itemId];  //按位或
                            if ((mergePermission & (int)DMFuncPermissionEnum.Manage) == (int)DMFuncPermissionEnum.Manage)
                                return true;
                            if ((mergePermission & (int)DMFuncPermissionEnum.Browse) == (int)DMFuncPermissionEnum.Browse)
                                return true;
                        }
                    }
                    foreach (string itemId in docIdList)
                    {
                        if (this._DicDocumentPowers.ContainsKey(itemId))
                        {
                            mergePermission |= this._DicDocumentPowers[itemId];  //按位或
                            if ((mergePermission & (int)DMFuncPermissionEnum.Manage) == (int)DMFuncPermissionEnum.Manage)
                                return true;
                            if ((mergePermission & (int)DMFuncPermissionEnum.Browse) == (int)DMFuncPermissionEnum.Browse)
                                return true;
                        }
                    }
                }                
                return false;
            }
            catch (Exception exp)
            {
                DMCommonMethod.WriteLog("DMUserPermission.CheckDirCanBrowse", string.Format("DirId:{0}\r\nError:{1}", dirId, exp.ToString()));
                return false;
            }
        }
        /// <summary>
        /// 检查用户对于指定目录（或及指定文档）是否具有指定的权限
        /// </summary>
        /// <param name="dirId">目录标识,如果文档标识不为空则目录标识可为空</param>
        /// <param name="docId">文档标识，为空则仅判断目录</param>
        /// <param name="func">操作功能项</param>
        /// <returns></returns>
        public bool CheckCan(string dirId,string docId,DMFuncPermissionEnum func)
        {
            if (string.IsNullOrEmpty(dirId) && string.IsNullOrEmpty(docId))
                return false;//不能两者都为空         
            //找到目录或文档的所有上级目录然后再检查权限  
            if (string.IsNullOrEmpty(docId))
            {
                DirLinkAddress dirLink = new DirLinkAddress(dirId, new LibDataAccess());
                if (dirLink.DirType == DirTypeEnum.Private)
                    return true;
                List<string> dirIds = dirLink.ParentDirIdList;
                dirIds.Add(dirId);//将本级目录标识加入
                return CheckCan(dirIds, string.Empty, func);
            }
            else
            {
                DirLinkAddress dirLink = new DirLinkAddress(false, docId, new LibDataAccess(), true);
                if (dirLink.DirType == DirTypeEnum.Private)
                    return true;
                return CheckCan(dirLink.ParentDirIdList, docId, func);
            }
        }
        /// <summary>
        /// 检查用户对于指定目录（或指定文档）是否具有指定的权限
        /// </summary>
        /// <param name="parentDirIds">目录文档所在的目录列表（从顶级到最后目录），如果是为目录进行查询则应包含本级目录</param>
        /// <param name="docId">文档标识，为空则仅判断目录</param>
        /// <param name="func">操作功能项</param>
        /// <returns></returns>
        public bool CheckCan(List<string> parentDirIds, string docId, DMFuncPermissionEnum func)
        {
            if (parentDirIds == null || parentDirIds.Count == 0)
                return false;
            
            lock (_LockObjOfDicDirAndDoc)
            {
                //先检是否有某个所在目录的管理权限
                string manageDir = (from item in _DicDirectoryPowers
                                   where parentDirIds.Contains(item.Key) && ((item.Value & (int)DMFuncPermissionEnum.Manage) == (int)DMFuncPermissionEnum.Manage)
                                 select item.Key).FirstOrDefault();
                if (string.IsNullOrEmpty(manageDir) == false)
                    return true;//有管理权限则直接返回true

                //检查文档是否有管理权限
                if (string.IsNullOrEmpty(docId) == false)
                {
                    string manageDoc = (from item in _DicDocumentPowers
                                        where item.Key.Equals(docId) && ((item.Value & (int)DMFuncPermissionEnum.Manage) == (int)DMFuncPermissionEnum.Manage)
                                        select item.Key).FirstOrDefault();
                    if (string.IsNullOrEmpty(manageDoc) == false)
                        return true;//有管理权限则直接返回true
                }

                //否则将逐级目录权限项相“或”
                List<int> permissionList= (from item in _DicDirectoryPowers
                                       where parentDirIds.Contains(item.Key) 
                                       select item.Value).ToList();
                int mergePermission = 0;
                bool isFirst = true;
                foreach (int dirPer in permissionList)
                {
                    if (isFirst)
                    {
                        mergePermission = dirPer;
                        isFirst = false;
                    }
                    else
                        mergePermission = mergePermission | dirPer;//合并权限，一个权限项无论是父目录设置了true还是子目录设置了true，都可用。
                }
                if (string.IsNullOrEmpty(docId) || _DicDocumentPowers.ContainsKey(docId) == false)
                {
                    //检查目录权限
                    if ((mergePermission & (int)func) == (int)func)
                        return true;
                }
                else
                {
                    if (isFirst == false)
                        mergePermission = mergePermission | _DicDocumentPowers[docId];//再与文档权限相或
                    else
                        mergePermission = _DicDocumentPowers[docId];
                    bool isCan = (mergePermission & (int)func) == (int)func;
                    if (isCan)
                        return true;
                }      
            }
            if (func == DMFuncPermissionEnum.Browse && string.IsNullOrEmpty(docId))
            {
                //对于仅检查目录的浏览权限的，再进行特殊处理
                string dirId = parentDirIds.Last();
                return CheckDirCanBrowse(dirId);
            }
            else
                return false;
        }
    }
}
