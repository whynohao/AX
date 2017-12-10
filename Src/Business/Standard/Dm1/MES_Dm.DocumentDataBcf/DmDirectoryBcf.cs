/**********************************************************************
 * CopyRight 2016 杭州集控科技有限公司 版权所有
 * 功能描述：文档管理模块的文档目录表单数据处理类
 * 创建标识：Zhangkj 2016/11/28
 * 
 * 修改标识：Zhangkj 2016/11/30
 * 修改描述：调试实现一个Dataset中多个数据表;
************************************************************************/

using AxCRL.Bcf;
using AxCRL.Comm.Bill;
using AxCRL.Comm.Define;
using AxCRL.Comm.Runtime;
using AxCRL.Comm.Utils;
using AxCRL.Core.Cache;
using AxCRL.Core.Comm;
using AxCRL.Core.Server;
using AxCRL.Data;
using AxCRL.Services;
using AxCRL.Template;
using AxCRL.Template.DataSource;
using AxCRL.Template.Layout;
using AxCRL.Template.ViewTemplate;
using Jikon.MES_Dm.DMCommon;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Jikon.MES_Dm.DocumentDataBcf
{
    /// <summary>
    /// 文档目录表单处理类
    /// </summary>
    [ProgId(ProgId = "dm.Directory", ProgIdType = ProgIdType.Bcf,VclPath = @"/Scripts/module/mes/dm/dmDirectoryVcl.js",ViewPath = @"/Scripts/module/mes/dm/dmDirectoryView.js")]
    public class DmDirectoryBcf : LibBcfData
    {
        protected override void AfterChangeData(DataSet tables)
        {
            base.AfterChangeData(tables);
            DataRow masterRow = this.DataSet.Tables[0].Rows[0];
            string dirId = LibSysUtils.ToString(masterRow["DIRID"]);
            if (string.IsNullOrEmpty(dirId) == false)
            {             
                DirLinkAddress dirLink = new DirLinkAddress(dirId, this.DataAccess);
                masterRow["DIRLINKADDRESS"] = dirLink.DirNameLink;//设置目录链接
            }
            this.DataSet.AcceptChanges();
        }
        public override void BeforeFillList(LibHandle libHandle, DataTable table, object listingQuery, LibEntryParam entryParam)
        {
            base.BeforeFillList(libHandle, table, listingQuery, entryParam);
            if (libHandle == null || table == null || listingQuery == null)
                return;
            BillListingQuery queryinfo = listingQuery as BillListingQuery;
            if (queryinfo == null)
                return;
            int dirType = 0;
            if (int.TryParse((from item in queryinfo.Condition.QueryFields
                              where item.Name.Equals("DIRTYPE") && item.QueryChar.Equals(LibQueryChar.Equal) &&
                              item.Value.Count == 1
                              select item.Value[0]).FirstOrDefault().ToString(), out dirType) == false)
                return;
            if (dirType == (int)DirTypeEnum.Private)
            {
                //私有目录,根据创建人进行标识
                AxCRL.Core.Comm.LibQueryField libQueryField = new AxCRL.Core.Comm.LibQueryField();
                libQueryField.Name = "CREATORID";
                libQueryField.QueryChar = LibQueryChar.Equal;
                libQueryField.Value.Add(libHandle.PersonId);
                queryinfo.Condition.QueryFields.Add(libQueryField);
            }
            //公共目录，在Fill后在根据目录权限设置

        }
        /// <summary>
        /// 对主表的列表数据增加处理
        /// 设置获取目录下的文档简要信息、目录/文档的的“目录链接”虚字段等
        /// </summary>
        /// <param name="dataTable"></param>
        public override void AfterFillList(LibHandle libHandle, DataTable table, object listingQuery, LibEntryParam entryParam)
        {
            base.AfterFillList(libHandle, table, listingQuery, entryParam);
            if (libHandle == null || table == null || listingQuery == null)
                return;

            BillListingQuery queryinfo = listingQuery as BillListingQuery;
            if (queryinfo == null|| queryinfo.Condition==null||
                queryinfo.Condition.QueryFields==null||
                queryinfo.Condition.QueryFields.Count==0)
                return;
            string belongDirID = "";
            //查询获得父目录编码
            belongDirID = (from item in queryinfo.Condition.QueryFields
                     where item.Name.Equals("PARENTDIRID") && item.QueryChar.Equals(LibQueryChar.Equal) &&
                     item.Value.Count == 1
                     select item.Value[0]).FirstOrDefault().ToString();

            int dirType = 0;
            if (int.TryParse((from item in queryinfo.Condition.QueryFields
                              where item.Name.Equals("DIRTYPE") && item.QueryChar.Equals(LibQueryChar.Equal) &&
                              item.Value.Count == 1
                              select item.Value[0]).FirstOrDefault().ToString(), out dirType) == false)
                return;

            DataTable dtDir = table;

            #region 公共目录则还需要再根据权限筛选一次
            if (libHandle.UserId != "admin" && dirType == (int)DirTypeEnum.Public)
            {
                //公共目录则还需要再根据权限筛选一次
                List<string> dirIdList = new List<string>();
                foreach (DataRow row in dtDir.Rows)
                {
                    dirIdList.Add(LibSysUtils.ToString(row["DIRID"]));
                }
                dirIdList = DMPermissionControl.Default.FilterDirIds(libHandle, DMFuncPermissionEnum.Browse, dirIdList);//筛选具有浏览权限的目录标识号
                List<DataRow> toDeleteList = new List<DataRow>();
                foreach (DataRow row in dtDir.Rows)
                {
                    if (dirIdList.Contains(LibSysUtils.ToString(row["DIRID"])) == false)
                        toDeleteList.Add(row);
                }
                foreach (DataRow row in toDeleteList)
                {
                    dtDir.Rows.Remove(row);
                }                
                dtDir.AcceptChanges();
            }
            #endregion


            string dirLinkAddress = "";
            if (belongDirID.Equals(""))
                dirLinkAddress = "";//如果父目录编码为空则目录链接的路径也为空
            else
            {
                DirLinkAddress dirLink = new DirLinkAddress(belongDirID, this.DataAccess);
                dirLinkAddress = dirLink.DirNameLink;//获取目录链接虚字段
            }

            foreach (DataRow row in dtDir.Rows)
            {
                //设置目录数据行的信息
                row["ISDIR"] = true;
                row["DOCTYPE"] = "目录";
                row["DIRLINKADDRESS"] = dirLinkAddress;//设置目录的目录链接虚字段

                row["DOCNAME"] = row["DIRNAME"];
            }

            //获取目录下的文档数据行并设置简要信息虚字段
            #region 获取目录下的文档数据行并设置简要信息虚字段
            DmDocumentBcf docBcf = new DmDocumentBcf();
            DataTable dtDoc = docBcf.GetDocOfDir(libHandle, belongDirID, (DirTypeEnum)dirType);
            if (dtDoc != null && dtDoc.Rows.Count > 0)
            {
                DataRow newRow = null;
                foreach (DataRow rowDoc in dtDoc.Rows)
                {
                    newRow = dtDir.NewRow();
                    newRow["DIRID"] = Guid.NewGuid().ToString().Substring(0,20);//设置一个不重复的值作为编号
                    newRow["DIRNAME"] = "无意义";
                    newRow["DOCCOUNT"] = 0;
                    newRow["PARENTDIRID"] = rowDoc["DIRID"];
                    newRow["DIRTYPE"] = 0;
                    rowDoc["DIRLINKADDRESS"] = dirLinkAddress;//设置目录的目录链接虚字段
                    newRow["SORTORDER"] = int.MaxValue;//实现文档在目录的下面

                    //文档虚字段
                    newRow["DOCID"] = rowDoc["DOCID"];
                    newRow["DOCNAME"] = rowDoc["DOCNAME"];
                    newRow["ISDIR"] = false;
                    newRow["DOCTYPE"] = rowDoc["DOCTYPE"];
                    newRow["LOCKSTATE"] = rowDoc["LOCKSTATE"];                   
                    newRow["DOCSIZE"] = rowDoc["DOCSIZE"];
                    newRow["SAVEPATH"] = rowDoc["SAVEPATH"];

                    newRow["LASTUPDATETIME"] = rowDoc["LASTUPDATETIME"];
                    newRow["CREATORNAME"] = rowDoc["CREATORNAME"];

                    newRow["SORTORDER"] = rowDoc["SORTORDER"];

                    dtDir.Rows.Add(newRow);
                }
            }
            #endregion

            //对目录按照SortOrder从小到大排序 ,文档目录在上    
            #region 对目录按照SortOrder从小到大排序 ,文档目录在上
            table.DefaultView.Sort = "ISDIR desc,SORTORDER asc";
            DataTable copyTable = table.DefaultView.ToTable();
            table.Rows.Clear();
            for (int i = 0; i < copyTable.Rows.Count; i++)
            {
                table.Rows.Add(copyTable.Rows[i].ItemArray);
            }
            #endregion

            if (table.DataSet != null)
                table.DataSet.AcceptChanges();

        }
        /// <summary>
        /// 获取功能模块确定的GridScheme
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="entryParam"></param>
        /// <returns></returns>
        public override LibGridScheme GetDefinedGridScheme(LibHandle handle, LibEntryParam entryParam)
        {
            LibGridScheme gridScheme = null;
            StringBuilder builder = new StringBuilder();
            if (entryParam != null)
            {
                foreach (var item in entryParam.ParamStore)
                {
                    builder.AppendFormat("{0}", item.Value);
                }
            }
            string schemeName = string.Format("{0}{1}List.bin", "dm.Directory", builder.ToString());
            LibDisplayScheme displayScheme = null;
            string path = Path.Combine(EnvProvider.Default.MainPath, "Scheme", "ShowScheme", schemeName);
            if (File.Exists(path))
            {
                LibBinaryFormatter formatter = new LibBinaryFormatter();
                using (FileStream fs = new FileStream(path, FileMode.Open))
                {
                    displayScheme = (LibDisplayScheme)formatter.Deserialize(fs);
                }
            }
            if (displayScheme != null)
            {
                gridScheme = displayScheme.GridScheme[0];
            }
            return gridScheme;
        }

        /// <summary>
        /// 修改前的目录绝对路径地址
        /// </summary>
        private string _OldDirPath = string.Empty;
        protected override void BeforeUpdate()
        {
            base.BeforeUpdate();
            DataRow masterRow = this.DataSet.Tables[0].Rows[0];
            #region 操作权限标识设置
            HashSet<string> hasSet = new HashSet<string>();
            foreach (DataRow curRow in this.DataSet.Tables[1].Rows)
            {
                if (curRow.RowState == DataRowState.Deleted)
                    continue;
                DataRow[] subRows = curRow.GetChildRows(DmDirectoryBcfTemplate.PermissionDetailSubRelationName, DataRowVersion.Current);
                int mark = 0;
                foreach (DataRow subRow in subRows)
                {
                    if (LibSysUtils.ToBoolean(subRow["CANUSE"]))
                        mark += LibSysUtils.ToInt32(subRow["OPERATEPOWERID"]);
                }
                curRow["OPERATEMARK"] = mark;
                string type_BelongID = string.Format("{0}_{1}", LibSysUtils.ToInt32(curRow["BELONGTYPE"]),LibSysUtils.ToString(curRow["BELONGID"]));
                if (hasSet.Contains(type_BelongID))
                    this.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("权限行{0}的拥有者重复。", curRow["ROWNO"]));
                else
                    hasSet.Add(type_BelongID);
            }
            #endregion

           

            try
            {               
                if (masterRow.RowState == DataRowState.Added)
                {
                    //添加前设置目录文件夹对应的文件夹名称   
                    string dirName = Guid.NewGuid().ToString().ToUpper();//新的文件夹名称，用于对应新创建的文件夹
                    masterRow["DIRPATH"] = dirName;//保存对应的文件夹路径

                    #region 检查管理权限
                    //根据权限设置，确定是否采用前台传递的目录数据。虽然根据权限设置前端控制不显示相关页面，但需要防止前端伪造数据。
                    DirTypeEnum dirType = (DirTypeEnum)LibSysUtils.ToInt32(masterRow["DIRTYPE"]);
                    string parentDirId = LibSysUtils.ToString(masterRow["PARENTDIRID"]);
                    if (dirType == DirTypeEnum.Public&&string.IsNullOrEmpty(parentDirId)==false)
                    {
                        //公共目录才进行检查。如果父目录为空，则应为超级管理员创建也不用检查。
                        if (DMPermissionControl.Default.HasPermission(this.Handle, parentDirId, string.Empty, DMFuncPermissionEnum.Manage) == false)
                        {
                            //新增时如果对父目录没有管理权限，则设置了目录权限也没用。
                            this.DataSet.Tables[2].RejectChanges();
                            this.DataSet.Tables[1].RejectChanges();
                        }
                    }
                    #endregion
                }
                else
                {
                    string dirId = LibSysUtils.ToString(masterRow["DIRID"]);
                    #region 检查管理权限
                    //根据权限设置，确定是否采用前台传递的目录数据。虽然根据权限设置前端控制不显示相关页面，但需要防止前端伪造数据。
                    DirTypeEnum dirType = (DirTypeEnum)LibSysUtils.ToInt32(masterRow["DIRTYPE"]);
                    if (dirType == DirTypeEnum.Public)
                    {
                        //公共目录才进行检查。
                        if (DMPermissionControl.Default.HasPermission(this.Handle, dirId, string.Empty, DMFuncPermissionEnum.Manage) == false)
                        {
                            //修改时如果对目录没有管理权限，则设置了目录权限也没用。
                            this.DataSet.Tables[2].RejectChanges();
                            this.DataSet.Tables[1].RejectChanges();
                        }
                    }
                    #endregion

                    string oldParentID = LibSysUtils.ToString(masterRow["PARENTDIRID", DataRowVersion.Original]);
                    string currentParentID = LibSysUtils.ToString(masterRow["PARENTDIRID"]);
                    if (oldParentID.Equals(currentParentID) == false)
                    {
                        DirLinkAddress dirLink = new DirLinkAddress(LibSysUtils.ToString(masterRow["DIRID", DataRowVersion.Original]), this.DataAccess);
                        if (dirLink.SubDirIdList.Contains(currentParentID))
                        {
                            this.ManagerMessage.AddMessage(LibMessageKind.Error, "不能将子目录设置为父目录!");
                            return;
                        }
                        _OldDirPath = string.Empty;
                        //如果是修改的，则记录修改前的SavePath
                        string path = DMCommonMethod.GetDMRootPath((DirTypeEnum)LibSysUtils.ToInt32(masterRow["DIRTYPE"]));
                        //string path = Path.Combine(EnvProvider.Default.DocumentsPath, (LibSysUtils.ToInt32(masterRow["DIRTYPE"]) == 0 ? "" : "my"));//根据是否为私有类型在路径下增加my    
                        string relativePath = dirLink.DirSavePath;
                        _OldDirPath = Path.Combine(path, relativePath);
                    }
                }
            }
            catch (Exception exp)
            {
                this.ManagerMessage.AddMessage(LibMessageKind.Error, "保存目录失败，原因:"+exp.Message);
                throw exp;
            }
        }
        protected override void AfterUpdate()
        {
            base.AfterUpdate();
            if (this.ManagerMessage.IsThrow)
                return;//如果已经有错误发生则直接返回
            try
            {
                DataRow masterRow = this.DataSet.Tables[0].Rows[0];               
                //先定位到文档库根路径
                string path = DMCommonMethod.GetDMRootPath((DirTypeEnum)LibSysUtils.ToInt32(masterRow["DIRTYPE"]));
                //string path = Path.Combine(EnvProvider.Default.DocumentsPath, (LibSysUtils.ToInt32(masterRow["DIRTYPE"]) == 0 ? "" : "my"));//根据是否为私有类型在路径下增加my
               
                string relativePath = string.Empty;
                if (masterRow.RowState == DataRowState.Added)
                {
                    //添加时创建目录文件夹  
                    DirLinkAddress dirLink = new DirLinkAddress(LibSysUtils.ToString(masterRow["DIRID"]), this.DataAccess);
                    relativePath = dirLink.DirSavePath;
                    path=Path.Combine(path, relativePath);
                    Directory.CreateDirectory(path);                    
                }
                else
                {
                    //update时检查父目录是否变化，如有变化根据将目录对应的文件夹移动到父目录文件夹下
                    string oldParentID = LibSysUtils.ToString(masterRow["PARENTDIRID", DataRowVersion.Original]);
                    string currentParentID = LibSysUtils.ToString(masterRow["PARENTDIRID"]);
                    if (oldParentID.Equals(currentParentID) == false)
                    {
                        DirLinkAddress dirLink = new DirLinkAddress(LibSysUtils.ToString(masterRow["DIRID"]), this.DataAccess);
                        relativePath = dirLink.DirSavePath;
                        string newDirPath = Path.Combine(path, relativePath);                                       
                        Directory.Move(_OldDirPath, newDirPath);//移动文件夹
                    }
                }
            }
            catch (Exception exp)
            {
                this.ManagerMessage.AddMessage(LibMessageKind.Error, "保存目录失败，原因:" + exp.Message);
                throw exp;
            }
        }
        /// <summary>
        /// 是否级联删除子目录及文档等，默认为真。如果是通过父目录级联调用删除时会提前将此值设置为false
        /// </summary>
        protected bool isDeleteSubs = true;
        /// <summary>
        /// 目录链接类
        /// </summary>
        protected DirLinkAddress dirLink = null;
        protected override void BeforeDelete()
        {
            base.BeforeDelete();
            //删除前先删除文件夹目录
            DataRow masterRow = this.DataSet.Tables[0].Rows[0];
            if (masterRow == null)
                return;
            string dirId = LibSysUtils.ToString(masterRow["DIRID"]);
            //如果需要删除所有子目录及文档
            if (isDeleteSubs)
            {
                //删除目录权限数据、子目录数据（及子目录的权限、文件）、文件（及文件数据）
                dirLink = new DirLinkAddress(dirId, this.DataAccess);
                dirLink.GetDocIds();

                //先删除文档
                DmDocumentBcf docBcf = null;       
                foreach (string docId in dirLink.DocIdList)
                {
                    //每个都要重新构造Bcf再删除
                    docBcf = LibBcfSystem.Default.GetBcfInstance("dm.Document", this.DataAccess) as DmDocumentBcf; //通过共用同一个DataAccess共用同一个事务  
                    docBcf.IsDeleteDiskDoc = false;//连带删除时，不删除磁盘文档文件的操作
                    docBcf.Delete(new object[] { docId });
                }

                //再删除目录
                DmDirectoryBcf dirBcf = null;
                foreach (string subDirId in dirLink.SubDirIdList)
                {
                    //每个都要重新构造Bcf再删除
                    dirBcf = LibBcfSystem.Default.GetBcfInstance("dm.Directory", this.DataAccess) as DmDirectoryBcf;//通过共用同一个DataAccess共用同一个事务         
                    dirBcf.isDeleteSubs = false;
                    dirBcf.Delete(new object[] { subDirId });                   
                }
            }
        }
        protected override void AfterDelete()
        {
            base.AfterDelete();
            if(this.isDeleteSubs)
            {
                //删除文件夹目录。sql中的表单记录操作可以回滚，磁盘上的文件目录和文档操作统一处理，失败了也不用回滚。
                DataRow masterRow = this.DataSet.Tables[0].Rows[0];
                if (masterRow == null)
                    return;
                string dirId = LibSysUtils.ToString(masterRow["DIRID"]);
                //DirLinkAddress dirLink = new DirLinkAddress(dirId, this.DataAccess);
                if (dirLink.DirSavePath == string.Empty)
                    return;
                //先定位到文档库根路径
                //string path = Path.Combine(EnvProvider.Default.DocumentsPath, (LibSysUtils.ToInt32(masterRow["DIRTYPE"]) == 0 ? "" : "my"));//根据是否为私有类型在路径下增加my
                string path = DMCommonMethod.GetDMRootPath((DirTypeEnum)LibSysUtils.ToInt32(masterRow["DIRTYPE"]));
                path = Path.Combine(path, dirLink.DirSavePath);
                try
                {
                    if (Directory.Exists(path))
                        Directory.Delete(path, true);//删除目录文件夹及其子目录文件夹、文件等。            
                }
                catch (Exception exp)
                {
                    //对于删除文档目录在磁盘上的文件夹的动作，因为文件系统的删除无法回滚，不再向上抛出异常（以免触发本级事务回滚），仅记录异常信息，以便后续手动清理
                    DMCommonMethod.WriteLog("DeleteDirectory", string.Format("DirID:{0}\r\nPath:{1}\r\nError:{2}", dirId, path, exp.ToString()));
                }
            }
        }

        /// <summary>
        /// 覆盖基类的导出方法
        /// </summary>
        /// <param name="pks"></param>
        /// <returns></returns>
        public new string ExportData(object[] pks)
        {
            string fileName = string.Empty;
            //To Do
            return fileName;
        }
        /// <summary>
        /// 更新目录下的文档数量
        /// </summary>
        /// <param name="dirId"></param>
        public static void UpdateDocCountOfDir(string dirId,LibDataAccess dataAccess)
        {
            if (string.IsNullOrEmpty(dirId) || dataAccess == null)
                return;
            string sql = "Update DMDIRECTORY set DOCCOUNT = (select count(*) from DMDOCUMENT B where B.DIRID = '" + dirId + "') where DIRID= '" + dirId + "' ";
            try
            {
                dataAccess.ExecuteNonQuery(sql);
            }
            catch(Exception exp)
            {
                DMCommonMethod.WriteLog("DmDirectoryBcf.UpdateDocCountOfDir", string.Format("DirId:{0},\r\nError:{1}", dirId, exp.ToString()));
            }
        }

        #region 表单附件文档目录处理
        /// <summary>
        /// 检查表单附件目录，如果需要的目录不存在则创建。返回检查得到的目录编号
        /// 目录编号为表单的功能代码progid,名称为表单的功能显示名称
        /// 添加到公共目录下的“表单附件”目录下，如该父目录不存在则创建
        /// 子目录按照日期构建
        /// </summary>
        /// <param name="progId"></param>
        /// <param name="displayName"></param>
        /// <returns></returns>
        public bool CheckAndAddBillAttachmentDir(string progId,out string retDirId)
        {
            retDirId = string.Empty;
            if (string.IsNullOrEmpty(progId))
                return false;
           string displayName = string.Empty;
           string sql = string.Empty;
           object obj = DBNull.Value;

            Dictionary<string, LibChangeRecord> dicChanges = new Dictionary<string, LibChangeRecord>();
            Dictionary<string, object> dicChangeColumns = new Dictionary<string, object>();
            try
            {
                LibHandle systemHandle = LibHandleCache.Default.GetSystemHandle();
                this.Handle = systemHandle;//设置为系统用户

                LibDataAccess dataAccess = this.DataAccess;
                //检查“表单附件”一级公共目录
                if (string.IsNullOrEmpty(ProgDirInfo.BillAttachmentTopDirId))
                {
                    sql = string.Format("select DIRID from DMDIRECTORY where DIRNAME = '{0}' and DIRTYPE = 0 and PARENTDIRID = '' order by CREATETIME asc", ProgDirInfo.BillAttachmentTopDirName);
                    obj = dataAccess.ExecuteScalar(sql);
                    if (string.IsNullOrEmpty(LibSysUtils.ToString(obj)))
                    {
                        //表单附件一级目录还不存在
                        string topDirId = string.Empty;
                        if (AddDirDirectly(ref topDirId, ProgDirInfo.BillAttachmentTopDirName, DirTypeEnum.Public, "") == false)
                            return false;
                        else
                            ProgDirInfo.BillAttachmentTopDirId = topDirId;
                    }
                    else
                        ProgDirInfo.BillAttachmentTopDirId = (string)obj;
                }

                ProgDirInfo progDirInfo = ProgDirInfo.GetDirInfo(progId);
                if(progDirInfo==null||string.IsNullOrEmpty(progDirInfo.DirId)||
                    string.IsNullOrEmpty(progDirInfo.ProgDisplayName))
                {
                    progDirInfo = new ProgDirInfo(progId);
                    //查找ProgId对应的功能名称
                    sql = string.Format("select PROGNAME from AXPFUNCLIST where PROGID = '{0}'", progId);
                    displayName = LibSysUtils.ToString(dataAccess.ExecuteScalar(sql));
                    if (string.IsNullOrEmpty(displayName))
                        return false;
                    progDirInfo.ProgDisplayName = displayName;
                }
                if(string.IsNullOrEmpty( progDirInfo.DirId))
                {
                    //检查功能模块二级目录
                    sql = string.Format("select DIRID from DMDIRECTORY where DIRID = '{0}'", progId);
                    obj = dataAccess.ExecuteScalar(sql);
                    if (string.IsNullOrEmpty(LibSysUtils.ToString(obj)))
                    {
                        //表单附件二级目录（功能模块）还不存在
                        if (AddDirDirectly(ref progId, displayName, DirTypeEnum.Public, ProgDirInfo.BillAttachmentTopDirId) == false)
                            return false;
                        progDirInfo.DirId = progId;
                    }
                    progDirInfo.DirId = progId;
                }                
                //检查三级目录，日期名
                string threeDirName = DateTime.Now.ToString("yyyyMMdd");
                retDirId = progDirInfo.GetDayDirId(threeDirName);
                if(string.IsNullOrEmpty(retDirId))
                {
                    sql = string.Format("select DIRID from DMDIRECTORY where DIRNAME = '{0}' and DIRTYPE = 0 and PARENTDIRID = '{1}' order by CREATETIME asc", threeDirName, progId);
                    obj = dataAccess.ExecuteScalar(sql);
                    if (string.IsNullOrEmpty(LibSysUtils.ToString(obj)))
                    {
                        //表单附件三级目录还不存在
                        bool ret = AddDirDirectly(ref retDirId, threeDirName, DirTypeEnum.Public, progDirInfo.DirId);
                        if (ret)
                        {
                            progDirInfo.AddDayDirId(threeDirName, retDirId);
                            ProgDirInfo.AddDirInfo(progDirInfo);
                        }
                        return ret;
                    }
                    else
                    {
                        retDirId = (string)obj;
                        if (string.IsNullOrEmpty(retDirId))
                            return false;
                        else
                        {
                            progDirInfo.AddDayDirId(threeDirName, retDirId);
                            ProgDirInfo.AddDirInfo(progDirInfo);
                        }
                    }
                }               
                return true;
            }
            catch (Exception exp)
            {
                DMCommonMethod.WriteLog("DmDirectoryBcf.AddBillAttachmentDir", string.Format("ProgId:{0}\r\nDisplayName:{1}\r\nError:{2}",progId,displayName,exp.ToString()));
                return false;
            }
           
        }
        /// <summary>
        /// 直接创建目录
        /// </summary>
        /// <param name="dirId"></param>
        /// <param name="dirName"></param>
        /// <returns></returns>
        protected bool AddDirDirectly(ref string dirId, string dirName,DirTypeEnum dirType,string parentDirId )
        {
            if (string.IsNullOrEmpty(dirName))
                return false;
            if (parentDirId == null)
                parentDirId = string.Empty;

            Dictionary<string, LibChangeRecord> dicChanges = new Dictionary<string, LibChangeRecord>();
            Dictionary<string, object> dicChangeColumns = new Dictionary<string, object>();

            LibEntryParam entryParam = new LibEntryParam();
            entryParam.ParamStore.Add("ParentDirId", parentDirId);
            entryParam.ParamStore.Add("ParentDirType", (int)dirType);

            this.DataSet.Clear();

            this.AddNew(entryParam);
            if (this.ManagerMessage.IsThrow)
                return false;
            if (string.IsNullOrEmpty(dirId))
                dirId = LibSysUtils.ToString(this.DataSet.Tables[0].Rows[0]["DIRID"]);

            object[] pks = new object[] { dirId };
            //因对于Add的对象Save方法中会检查Add的第一条记录数据并做相关处理，因此需要模拟生成前端传递来的change数据
            LibChangeRecord record = new LibChangeRecord();
            foreach (DataColumn col in this.DataSet.Tables[0].Columns)
            {
                dicChangeColumns.Add(col.ColumnName, this.DataSet.Tables[0].Rows[0][col.ColumnName]);//将文档主表的第一行数据变成change数据
            }
            dicChangeColumns["DIRID"] = dirId;
            dicChangeColumns["DIRNAME"] = dirName;
            dicChangeColumns["DIRTYPE"] = (dirType == DirTypeEnum.Public) ? 0 : 1;
            dicChangeColumns["PARENTDIRID"] = parentDirId;

            record.Add.Add(dicChangeColumns);
            dicChanges.Add("DMDIRECTORY", record);

            this.DataSet.Clear();//将通过addNew添加的数据全清空,以免和通过change数据添加的重复了。

            this.Save(BillAction.AddNew, pks, dicChanges, null);
            if (this.ManagerMessage.IsThrow)
                return false;
            else
                return true;
        }
        #endregion

        #region 操作权限明细项及控制相关
        protected override void CheckFieldReturn(int tableIndex, string fieldName, object[] curPk, Dictionary<string, object> fieldKeyAndValue, Dictionary<string, object> returnValue)
        {
            base.CheckFieldReturn(tableIndex, fieldName, curPk, fieldKeyAndValue, returnValue);
            if (tableIndex == 1 && fieldName == "BELONGID" && curPk != null && curPk.Length == 1)
            {
                this.Template.GetViewTemplate(this.DataSet);
                returnValue.Add("OperatePowerData", DMPermissionControl.Default.BuildPowerInfo(true,this.Template.FuncPermission.Permission));//构建权限项列表
            }
        }
        protected override bool CheckBrowseTo(object[] pks)
        {
            string sql = string.Format("select DIRTYPE from DMDIRECTORY where DIRID = '{0}'",pks[0]);
            object obj = this.DataAccess.ExecuteScalar(sql);            
            int dirType = LibSysUtils.ToInt32(obj);
            if (dirType == (int)DirTypeEnum.Private)
                return true;//个人目录不做控制

            return CheckPermission(pks[0].ToString(), DMFuncPermissionEnum.Browse);
        }
        protected override bool CheckAddNew(LibEntryParam entryParam)
        {
            //使用入口参数传递检查权限需要的参数
            string parentDirId = "";
            int parentDirType = -1;
            if (entryParam != null && entryParam.ParamStore.Count >= 2&&
                entryParam.ParamStore.ContainsKey("ParentDirId") &&
                entryParam.ParamStore.ContainsKey("ParentDirType"))
            {
                parentDirId = entryParam.ParamStore["ParentDirId"].ToString();
                int.TryParse(entryParam.ParamStore["ParentDirType"].ToString(),out parentDirType);
                //用完后置空
                entryParam.ParamStore.Remove("ParentDirId");
                entryParam.ParamStore.Remove("ParentDirType");
            }
            else
            {
                this.ManagerMessage.AddMessage(LibMessageKind.Error, "缺少检查权限需要的参数。");
                return false;
            }

            if (parentDirType == (int)DirTypeEnum.Private|| parentDirType == (int)DirTypeEnum.PrivateRoot)
                return true;//个人目录不做控制
            if (parentDirType == (int)DirTypeEnum.PublicRoot)
            {
                if (this.Handle.UserId.Equals("admin") == false)
                {
                    this.ManagerMessage.AddMessage(LibMessageKind.Error, "公共文档根目录下只能由超级管理员才能新增目录。");
                    return false;
                }
                else
                    return true;
            }

            return CheckPermission(parentDirId, DMFuncPermissionEnum.Add);            
        }
        protected override bool CheckModif(object[] pks)
        {
            DataRow masterRow = this.DataSet.Tables[0].Rows[0];          
            int dirType = LibSysUtils.ToInt32(masterRow["DIRTYPE"]);
            if (dirType == (int)DirTypeEnum.Private)
                return true;//个人目录不做控制

            return CheckPermission(pks[0].ToString(), DMFuncPermissionEnum.Edit);
        }
        protected override bool CheckDelete()
        {
            DataRow masterRow = this.DataSet.Tables[0].Rows[0];
            string dirId = LibSysUtils.ToString(masterRow["DIRID"]);
            int dirType = LibSysUtils.ToInt32(masterRow["DIRTYPE"]);
            if (dirType == (int)DirTypeEnum.Private)
                return true;//个人目录不做控制
            return CheckPermission(dirId, DMFuncPermissionEnum.Delete);

        }
        //导入、导出、打印等可能需要的方法需要编写覆盖基类的方法，应在相关方法中检查权限
       

        /// <summary>
        /// 权限检查
        /// </summary>
        /// <param name="permission"></param>
        /// <returns></returns>
        protected bool CheckPermission(string dirId, DMFuncPermissionEnum permission)
        {
            bool ret = true;//因DMFuncPermissionEnum权限项的值能覆盖FuncPermissionEnum的值，可以直接转换使用
            if (LibHandleCache.Default.GetSystemHandle() != this.Handle && this.Handle.UserId != "admin")
            {
                ret = DMPermissionControl.Default.HasPermission(this.Handle, dirId, string.Empty, permission);
                if (!ret)
                {
                    this.ManagerMessage.AddMessage(LibMessageKind.Error, "当前人员不具备操作权限。");
                }
            }
            return ret;
        }
        #endregion
        /// <summary>
        /// 根据权限设置返回不同的视图模板
        /// </summary>
        /// <param name="checkDirType"></param>
        /// <param name="checkDirId"></param>
        /// <param name="entryParam"></param>
        /// <returns></returns>
        public  LibViewTemplate GetViewTemplateOfPermission(int checkDirType, string checkDirId, LibEntryParam entryParam = null)
        {
            LibBillTpl tpl = (LibBillTpl)base.GetViewTemplate(entryParam);
            bool isRemoveManageView = true;
            if (checkDirType == (int)DirTypeEnum.Private || checkDirType == (int)DirTypeEnum.PrivateRoot)
                isRemoveManageView = false;
            else
            {
                if (DMPermissionControl.Default.HasPermission(this.Handle, checkDirId, string.Empty, DMFuncPermissionEnum.Manage))
                    isRemoveManageView = false;
            }
            if (isRemoveManageView)
            {
                //移除目录权限面板
                LibBillLayout layout = tpl.Layout as LibBillLayout;
                layout.TabRange.RemoveAt(0);//目录权限
                layout.SubBill.Remove(2);//目录操作细项
            }
            return tpl;
        }       
        protected override LibTemplate RegisterTemplate()
        {
            DmDirectoryBcfTemplate template= new DmDirectoryBcfTemplate("dm.Directory");            
            return template;
        }
    }
   
    public class DmDirectoryBcfTemplate : LibTemplate
    {
        private const string dmDirTableName = "DMDIRECTORY";
        private const string dmDirPKID = "DIRID";

        /// <summary>
        /// 目录的权限表名称
        /// </summary>
        private const string dmDirPermissionTableName = "DMDIRPERMISSION";
        /// <summary>
        /// 权限明细表名称
        /// </summary>
        private const string dmSubTableName = "DMDIROPERATEPOWER";
        /// <summary>
        /// 权限标识与详细操作权限项的关系名称
        /// </summary>
        public static string PermissionDetailSubRelationName
        {
            get { return string.Format("{0}_{1}", dmDirPermissionTableName, dmSubTableName); }
        }

        /// <summary>
        /// 构造函数。
        /// </summary>
        /// <param name="progId"></param>
        public DmDirectoryBcfTemplate(string progId)
            : base(progId, BillType.Master, "文档目录")
        {

        }
        /// <summary>
        /// 创建数据集合
        /// </summary>
        protected override void BuildDataSet()
        {
            this.DataSet = new DataSet();

            #region 文档目录
            //文档目录
            DataTable dmDir = new DataTable(dmDirTableName);
            //目录代码ReadOnly为true是为了通过前台功能按钮来设置
            DataSourceHelper.AddColumn(new DefineField(dmDir, dmDirPKID, "目录代码", FieldSize.Size20) { AllowEmpty = false, AllowCopy = false, ReadOnly = true });
            DataSourceHelper.AddColumn(new DefineField(dmDir, "DIRNAME", "目录名称", FieldSize.Size100) { DataType = LibDataType.NText, AllowEmpty = false, });
            //此项仅后台统计本级目录下的文档数，不包含子目录下的文档数
            DataSourceHelper.AddColumn(new DefineField(dmDir, "DOCCOUNT", "文档数") { DataType = LibDataType.Int32, ControlType = LibControlType.Number, ReadOnly = true });
            DataSourceHelper.AddColumn(new DefineField(dmDir, "PARENTDIRID", "父目录代码", FieldSize.Size20)
            {
                ControlType=LibControlType.IdName,
                RelativeSource = new RelativeSourceCollection()
                {
                    new RelativeSource("dm.Directory")
                    {
                         RelFields = new RelFieldCollection()
                         {
                             new RelField("DIRNAME", LibDataType.NText,FieldSize.Size50,"父目录名称","PARENTDIRNAME"),
                             new RelField("DIRPATH", LibDataType.NText,FieldSize.Size2000,"父目录存储路径","PARENTDIRPATH")
                         },                         
                         SelConditions=new SelConditionCollection()
                         {
                             //必须是相同目录类型的才能作为父目录,不能引用自身或自身的子目录(BeforeUpdate时再判断)
                              new SelCondition(){Condition="A.DIRTYPE = @A.DIRTYPE AND A.DIRID<> @A.DIRID  "}
                         }
                    }
                }

            });
            //目录类型只能在创建时根据其对应的根目录类型确定，不可直接修改
            DataSourceHelper.AddColumn(new DefineField(dmDir, "DIRTYPE", "目录类型")
            {
                DataType = LibDataType.Int32,
                AllowEmpty = false,
                ReadOnly = true,
                ControlType = LibControlType.TextOption,
                TextOption = new string[] { "公共", "个人" },
                DefaultValue=DirTypeEnum.Public,
            });
            //目录在文档库磁盘上的文件夹名称（不包含路径）
            DataSourceHelper.AddColumn(new DefineField(dmDir, "DIRPATH", "存储路径", FieldSize.Size2000) { DataType = LibDataType.NText, ControlType = LibControlType.NText, ReadOnly = true });
            //虚字段 目录链接，根据目录的父目录关系，构造形如"\公共文档\财务部文档"的目录链接地址样式。前端不可修改
            DataSourceHelper.AddColumn(new DefineField(dmDir, "DIRLINKADDRESS", "目录链接", FieldSize.Size2000) { ControlType = LibControlType.Id, ReadOnly = true, FieldType = FieldType.Virtual });
            //用于同一级目录下的子目录排序
            DataSourceHelper.AddColumn(new DefineField(dmDir, "SORTORDER", "目录排序") { DataType = LibDataType.Int32, ControlType = LibControlType.Number });          
            DataSourceHelper.AddFixColumn(dmDir, BillType);

            #region 为显示目录下的文档信息添加的虚字段
            DataSourceHelper.AddColumn(new DefineField(dmDir, "DOCID", "文档编号", FieldSize.Size50) { DataType = LibDataType.Text,  AllowCopy = false, FieldType = FieldType.Virtual });
            DataSourceHelper.AddColumn(new DefineField(dmDir, "DOCNAME", "文档名称", FieldSize.Size200) { DataType = LibDataType.NText, ReadOnly = true, FieldType = FieldType.Virtual });//如果类型是目录，则为目录名
            //是否是目录，此字段为虚字段，后台设置，用于向前端传递信息
            DataSourceHelper.AddColumn(new DefineField(dmDir, "ISDIR", "是否目录") { DataType = LibDataType.Boolean, ControlType = LibControlType.YesNo, ReadOnly = true, FieldType = FieldType.Virtual });
            //如txt,doc等，如果用于展示目录信息则此项固定为“目录”
            DataSourceHelper.AddColumn(new DefineField(dmDir, "DOCTYPE", "类型", FieldSize.Size20) { DataType = LibDataType.NText, ReadOnly = true, FieldType = FieldType.Virtual });
            DataSourceHelper.AddColumn(new DefineField(dmDir, "LOCKSTATE", "锁定状态") { DataType = LibDataType.Int32, ReadOnly = true, ControlType = LibControlType.TextOption, TextOption = new string[] { "未锁定", "已锁定" }, FieldType = FieldType.Virtual });
            //此项仅后台设置的文档的大小，以M为单位，保留2位小数
            DataSourceHelper.AddColumn(new DefineField(dmDir, "DOCSIZE", "大小(M)") { DataType = LibDataType.Numeric, ControlType = LibControlType.Double, Precision = 2, ReadOnly = true, FieldType = FieldType.Virtual });
            //此路径为文档的多个修订版和设定版文件所在的目录名称（不包含其他路径）
            DataSourceHelper.AddColumn(new DefineField(dmDir, "SAVEPATH", "文档存储路径", FieldSize.Size2000) { DataType = LibDataType.NText, ControlType = LibControlType.NText, ReadOnly = true, FieldType = FieldType.Virtual });
            #endregion

            dmDir.PrimaryKey = new DataColumn[] { dmDir.Columns[dmDirPKID] };
            this.DataSet.Tables.Add(dmDir);
            #endregion

            #region 文档目录的权限
            //文档目录的权限
            DataTable dmDirPermission = new DataTable(dmDirPermissionTableName);            
            DataSourceHelper.AddColumn(new DefineField(dmDirPermission, "DIRID", "目录代码", FieldSize.Size20) { ControlType = LibControlType.Id, AllowEmpty = false, ReadOnly = true });
            DataSourceHelper.AddRowId(dmDirPermission);
            DataSourceHelper.AddRowNo(dmDirPermission);
            //权限拥有者类型，如部门、用户组、个人，如果此权限是属于部门的，则部门下的所有人都有相应的权限
            DataSourceHelper.AddColumn(new DefineField(dmDirPermission, "BELONGTYPE", "拥有者类型") { DataType = LibDataType.Int32, AllowEmpty = false, ControlType = LibControlType.TextOption, TextOption = new string[] { "部门", "用户组", "个人" } });
            //此项根据拥有者类型确定是部门代码，还是个人或用户组代码(后续扩展)
            DataSourceHelper.AddColumn(new DefineField(dmDirPermission, "BELONGID", "拥有者代码", FieldSize.Size20)
            {
                AllowEmpty = false,
                ControlType=LibControlType.IdName,
                RelativeSource = new RelativeSourceCollection()
                {
                    new RelativeSource("com.Dept")
                    {
                         GroupCondation = "B.BELONGTYPE=0",
                         GroupIndex = 0,
                         //RelPK="A.DEPTID",
                         RelFields = new RelFieldCollection()
                         {
                             new RelField("DEPTNAME", LibDataType.NText,FieldSize.Size50,"部门名称") //如果拥有者类型是部门的则代码是部门代码，名称显示部门名称
                         },                         
                    },
                    new RelativeSource("com.Person")
                    {
                         GroupCondation = "B.BELONGTYPE=2",
                         GroupIndex = 1,
                         //RelPK="A.PERSONID",
                         RelFields = new RelFieldCollection()
                         {
                             new RelField("PERSONNAME", LibDataType.NText,FieldSize.Size50,"人员名称")//如果拥有者类型是个人的则代码是个人代码，名称显示人员名称
                         }
                    }
                }

            });
            //是否设置了操作权限
            DataSourceHelper.AddColumn(new DefineField(dmDirPermission, "ISOPERATEPOWER", "操作权限") { ReadOnly = true, DataType = LibDataType.Boolean, ControlType = LibControlType.YesNo, SubTableIndex = 2 });
            //权限值，是权限项组合的按位与 得出的Int32值。通过SubBill的页面获得
            DataSourceHelper.AddColumn(new DefineField(dmDirPermission, "OPERATEMARK", "操作权限标识") { DataType = LibDataType.Int32, ControlType = LibControlType.Number, ReadOnly = true });
            
            dmDirPermission.PrimaryKey = new DataColumn[] { dmDirPermission.Columns["DIRID"], dmDirPermission.Columns["ROW_ID"] };
            this.DataSet.Tables.Add(dmDirPermission);
            this.DataSet.Relations.Add(string.Format("{0}_{1}", dmDirTableName, dmDirPermissionTableName), new DataColumn[] { dmDir.Columns[dmDirPKID] }, new DataColumn[] { dmDirPermission.Columns[dmDirPKID] });

            #endregion

            #region 操作权限SubBill
            DataTable subTable = new DataTable(dmSubTableName);
            DataSourceHelper.AddColumn(new DefineField(subTable, dmDirPKID, "目录代码", FieldSize.Size20) { AllowEmpty = false, AllowCopy = false });
            DataSourceHelper.AddRowId(subTable, "PARENTROWID", "父行标识");
            DataSourceHelper.AddRowId(subTable);
            DataSourceHelper.AddRowNo(subTable);
            DataSourceHelper.AddColumn(new DefineField(subTable, "OPERATEPOWERID", "操作代码") { DataType = LibDataType.Int32, ControlType = LibControlType.Number, ReadOnly = true });
            DataSourceHelper.AddColumn(new DefineField(subTable, "OPERATEPOWERNAME", "操作", FieldSize.Size50) { DataType = LibDataType.NText, ControlType = LibControlType.NText, ReadOnly = true });
            DataSourceHelper.AddColumn(new DefineField(subTable, "CANUSE", "具备权限") { DataType = LibDataType.Boolean, ControlType = LibControlType.YesNo });
            DataSourceHelper.AddRemark(subTable);
            subTable.PrimaryKey = new DataColumn[] { subTable.Columns[dmDirPKID], subTable.Columns["PARENTROWID"], subTable.Columns["ROW_ID"] };
            this.DataSet.Tables.Add(subTable);
            this.DataSet.Relations.Add(PermissionDetailSubRelationName,
                new DataColumn[] { dmDirPermission.Columns[dmDirPKID], dmDirPermission.Columns["ROW_ID"] }, 
                new DataColumn[] { subTable.Columns[dmDirPKID], subTable.Columns["PARENTROWID"] });
            #endregion
        }

        /// <summary>
        /// 定义前台表单样式
        /// </summary>
        /// <param name="dataSet"></param>
        protected override void DefineViewTemplate(DataSet dataSet)
        {
            LibBillLayout layout = new LibBillLayout(this.DataSet);
            layout.HeaderRange = layout.BuildControlGroup(0, "目录信息", new List<string>() { dmDirPKID, "DIRNAME", "PARENTDIRID", "DIRLINKADDRESS", "DOCCOUNT", "DIRTYPE", "SORTORDER" });
            layout.TabRange.Add(layout.BuildGrid(1, "目录权限"));
            layout.SubBill.Add(2, layout.BuildGrid(2, "操作权限明细"));

            this.ViewTemplate = new LibBillTpl(this.DataSet, layout);
            
        }
        protected override void DefineFuncPermission()
        {
            base.DefineFuncPermission();
            //设置默认的权限值 因枚举值相同可以强制类型转换
            this.FuncPermission = new LibFuncPermission("", new FuncPermissionEnum[] { (FuncPermissionEnum)DMFuncPermissionEnum.Browse });
        }
    }
}
