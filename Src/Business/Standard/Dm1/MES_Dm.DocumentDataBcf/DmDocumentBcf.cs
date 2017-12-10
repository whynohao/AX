/**********************************************************************
 * CopyRight 2016 杭州集控科技有限公司 版权所有
 * 功能描述：文档管理模块的文档表单数据处理类
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
using System.Threading;
using System.Threading.Tasks;

namespace Jikon.MES_Dm.DocumentDataBcf
{
    /// <summary>
    /// 文档目录表单处理类
    /// </summary>
    [ProgId(ProgId = "dm.Document", ProgIdType = ProgIdType.Bcf, VclPath = @"/Scripts/module/mes/dm/dmDocumentVcl.js", ViewPath = @"/Scripts/module/mes/dm/dmDocumentView.js")]
    public class DmDocumentBcf : LibBcfData, ILibLiveUpdate
    {
        
        
        private const string FileNameStringKey = "FileName";
        private const string DocOpTypeStringKey = "DocOpType";

        /// <summary>
        /// 获取扩展参数：上传、新增、修改的临时文件名
        /// </summary>
        public string FileNameParamValue
        {
            get
            {
                if (ExtendBcfParam.ContainsKey(FileNameStringKey))
                    return ExtendBcfParam[FileNameStringKey].ToString();
                else
                    return string.Empty;
            }
        }
        /// <summary>
        /// 获取扩展参数：文档文件的操作类型（如上传、新建、编辑、替换等类型）
        /// </summary>
        public DocOpTypeENum DocOpTypeValue
        {
            get
            {
                int opType = -1;
                if (ExtendBcfParam.ContainsKey(DocOpTypeStringKey))
                {
                    int.TryParse(ExtendBcfParam[DocOpTypeStringKey].ToString(), out opType);
                    return (DocOpTypeENum)opType;
                }
                else
                    return DocOpTypeENum.UnknownOrUnset;
            }
        }
        public DmDocumentBcf():base()
        {
            if (this.RegisterBcfParamType.ContainsKey(FileNameStringKey) == false)
                this.RegisterBcfParamType.Add(FileNameStringKey, typeof(string));//注册扩展参数的类型    
            if (this.RegisterBcfParamType.ContainsKey(DocOpTypeStringKey) == false)
                this.RegisterBcfParamType.Add(DocOpTypeStringKey, typeof(int));//注册扩展参数的类型    
        }
        /// <summary>
        /// 获取第一个Error消息
        /// </summary>
        /// <returns></returns>
        private string GetFirstErrorMessage()
        {
            if (this.ManagerMessage.IsThrow == false)
                return string.Empty;
            else
            {
                foreach (LibMessage msg in this.ManagerMessage.MessageList)
                {
                    if (msg.MessageKind == LibMessageKind.Error)
                        return msg.Message;
                }
                return string.Empty;
            }
        }
        /// <summary>
        /// 保存新增或修改的文件。仅针对对文件本身的修改（及涉及到的部分实体文件相关的字段，如文件大小等）
        /// 主要由前台js文件通过InvokeBcf方式调用
        /// </summary>
        /// <param name="isAddNew">是否新增</param>
        /// <param name="tempFileName">临时文件名称，含扩展名，是存放在文档库的Temp目录的</param>
        /// <param name="docId">文档编号，对于新增的可以为string.empty</param>
        /// <param name="dirId">文档所属的目录编号</param>
        /// <param name="dirType">目录类型</param>
        /// <param name="realFileName">真实的文件名称，对于编辑的可以为string.empty</param>
        /// <returns></returns>
        public string SaveDoc(int isAddNew, string tempFileName, string docId, string dirId, int dirType,string realFileName)
        {
            Dictionary<string, LibChangeRecord> dicChanges = new Dictionary<string, LibChangeRecord>();
            Dictionary<string, object> dicChangeColumns = new Dictionary<string, object>();

            Dictionary<string, string> extendParams = new Dictionary<string, string>();
            extendParams.Add(FileNameStringKey, "\"" + tempFileName + "\"");
            extendParams.Add(DocOpTypeStringKey, ((isAddNew == 1) ? (int)DocOpTypeENum.AddNew : (int)DocOpTypeENum.Edit).ToString());

            if (isAddNew==1)
            {
                //新增                
                LibEntryParam entry = new LibEntryParam();
                entry.ParamStore.Add("DirId", dirId);
                entry.ParamStore.Add("DirType", dirType);
                this.AddNew(entry);
                if (this.ManagerMessage.IsThrow)
                    return GetFirstErrorMessage();
                object[] pks = new object[] { LibSysUtils.ToString(this.DataSet.Tables[0].Rows[0]["DOCID"]) };

                //因对于Add的对象Save方法中会检查Add的第一条记录数据并做相关处理，因此需要模拟生成前端传递来的change数据
                LibChangeRecord record = new LibChangeRecord();
                foreach (DataColumn col in this.DataSet.Tables[0].Columns)
                {
                    dicChangeColumns.Add(col.ColumnName, this.DataSet.Tables[0].Rows[0][col.ColumnName]);//将文档主表的第一行数据变成change数据
                }
                dicChangeColumns["DIRID"] = dirId;
                dicChangeColumns["DOCNAME"] = realFileName;
                record.Add.Add(dicChangeColumns);
                dicChanges.Add("DMDOCUMENT", record);

                this.DataSet.Clear();//将通过addNew添加的数据全清空,以免和通过change数据添加的重复了。

                this.Save(BillAction.AddNew, pks, dicChanges, extendParams);
                if (this.ManagerMessage.IsThrow)
                    return GetFirstErrorMessage();
            }
            else
            {
                object[] pks = new object[] { docId };
                this.Edit(pks);
                if (this.ManagerMessage.IsThrow)
                    return GetFirstErrorMessage();

                this.Save(BillAction.Modif, pks, dicChanges, extendParams);
                if (this.ManagerMessage.IsThrow)
                    return GetFirstErrorMessage();
            }
            return "ok";
        }
        /// <summary>
        /// 设定文档的版本号
        /// 成功返回true
        /// </summary>
        /// <param name="docId"></param>
        /// <param name="oldVersion"></param>
        /// <param name="newVersion"></param>
        /// <param name="desc"></param>
        /// <returns>成功返回true,失败返回false</returns>
        public bool SetVersion(string docId,string oldVersion, string newVersion,string desc)
        {
            if(string.IsNullOrEmpty(docId)||string.IsNullOrEmpty(newVersion))
            {
                this.ManagerMessage.AddMessage(LibMessageKind.Error, "参数有误。");
                return false;
            }
            if (this.Handle.UserId != "admin" && CheckPermission(string.Empty, docId, DMFuncPermissionEnum.SetVersion) == false)
            {
                this.ManagerMessage.AddMessage(LibMessageKind.Error, "当前人员不具备操作权限。");
                return false;
            }
            try
            {
                int curMaxModifyId = -1;
                string sql = string.Format("select max(DOCMODIFYID) from DMDOCMODIFYHISTORY where DOCID = '{0}'", docId);
                object obj= this.DataAccess.ExecuteScalar(sql);
                if (obj != DBNull.Value)
                    curMaxModifyId = LibSysUtils.ToInt32(obj);

                Dictionary<string, object> dicChangeColumns = new Dictionary<string, object>();
                Dictionary<string, LibChangeRecord> dicChanges = new Dictionary<string, LibChangeRecord>();

                LibChangeRecord lcr = new LibChangeRecord();
                dicChangeColumns = new Dictionary<string, object>();
                dicChangeColumns.Add("_DOCID", docId);              //修改的行必须添加一个主键标识
                dicChangeColumns.Add("DOCVERSION", newVersion);
                lcr.Modif.Add(dicChangeColumns);//修改
                
                dicChanges.Add("DMDOCUMENT", lcr);  //文档的版本号更改为新的值


                int curRowId = 1;
                sql = string.Format("select max(ROW_ID) from DMDOCVERHISTORY where DOCID = '{0}'", docId);
                obj = this.DataAccess.ExecuteScalar(sql);
                if (obj != DBNull.Value)
                    curRowId = LibSysUtils.ToInt32(obj) + 1;

                lcr = new LibChangeRecord();
                dicChangeColumns = new Dictionary<string, object>();
                dicChangeColumns.Add("DOCID", docId);
                dicChangeColumns.Add("ROW_ID", curRowId);
                dicChangeColumns.Add("ROWNO", curRowId);
                dicChangeColumns.Add("OPPERSONID", string.IsNullOrEmpty(this.Handle.PersonId) ? "(NoSet)" : this.Handle.PersonId);
                dicChangeColumns.Add("IP", this.Handle.LogIp);

                string opLog = string.Format("将版本号从{0}修改为{1}，修改说明:{2}", oldVersion, newVersion, desc);
                dicChangeColumns.Add("Log", opLog);
                dicChangeColumns.Add("DOCMODIFYID", curMaxModifyId);
                dicChangeColumns.Add("DATETIME", LibDateUtils.GetCurrentDateTime());
                lcr.Add.Add(dicChangeColumns);//添加新行
                dicChanges.Add("DMDOCVERHISTORY", lcr);  //文档的版本历史添加新行

                this.Edit(new object[] { docId });
                this.Save(BillAction.Modif, new object[] { docId }, dicChanges);
                if (this.ManagerMessage.IsThrow)
                    return false;

                this.AddNewDocOpLog(docId, string.Format("设定了文档版本:{0}", opLog), true);
                return true;
            }
            catch (Exception exp)
            {
                DMCommonMethod.WriteLog("DmDocumentBcf.SetVersion", string.Format("DocId:{0}\r\nNewVersion:{1}\r\nDesc:{2}\r\nError:{3}", docId, newVersion, desc, exp.ToString()));
                this.ManagerMessage.AddMessage(LibMessageKind.Error, "服务器出现异常，请查看日志。");
                return false;
            }
            
          
        } 
        /// <summary>
        /// 回退指定的修订版为当前版本
        /// </summary>
        /// <param name="docId"></param>
        /// <param name="toFallbackModifyVerId"></param>
        /// <param name="desc"></param>
        /// <returns></returns>
        public bool FallBackVersion(string docId,int toFallbackModifyVerId,string desc)
        {
            if (string.IsNullOrEmpty(docId) || toFallbackModifyVerId < -1)
            {
                this.ManagerMessage.AddMessage(LibMessageKind.Error, "参数有误。");
                return false;
            }
            if (this.Handle.UserId != "admin" && CheckPermission(string.Empty, docId, DMFuncPermissionEnum.Fallback) == false)
            {
                this.ManagerMessage.AddMessage(LibMessageKind.Error, "当前人员不具备操作权限。");
                return false;
            }
            try
            {
                int curMaxModifyId = 1;
                string sql = string.Format("select max(DOCMODIFYID) from DMDOCMODIFYHISTORY where DOCID = '{0}'", docId);
                object obj = this.DataAccess.ExecuteScalar(sql);
                if (obj != DBNull.Value)
                    curMaxModifyId = LibSysUtils.ToInt32(obj) + 1;

                //复制新文件
                DirLinkAddress dirLink = new DirLinkAddress(false, docId, this.DataAccess, true);
                string oldVerPath = dirLink.GetDocFullPath(toFallbackModifyVerId);      //指定修订版作为回退的文件
                string newVerPath = dirLink.GetDocFullPath(curMaxModifyId);
                if (File.Exists(oldVerPath) == false)
                {
                    this.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("修订版:{0} 文件在文档库中不存在。",toFallbackModifyVerId));
                    return false;
                }
                File.Copy(oldVerPath, newVerPath);
                FileInfo fileInfo = new FileInfo(newVerPath);
                
                Dictionary<string, object> dicChangeColumns = new Dictionary<string, object>();
                Dictionary<string, LibChangeRecord> dicChanges = new Dictionary<string, LibChangeRecord>();

                LibChangeRecord lcr = new LibChangeRecord();
                dicChangeColumns = new Dictionary<string, object>();
                dicChangeColumns.Add("_DOCID", docId);              //修改的行必须添加一个主键标识
                dicChangeColumns.Add("DOCSIZE", Math.Round(fileInfo.Length / 1024.0 / 1024.0, 3));//转换为以M为单位的大小,设置新的文档大小
                lcr.Modif.Add(dicChangeColumns);//修改

                dicChanges.Add("DMDOCUMENT", lcr);  //文档的版本号更改为新的值

                lcr = new LibChangeRecord();
                dicChangeColumns = new Dictionary<string, object>();
                dicChangeColumns.Add("DOCID", docId);
                dicChangeColumns.Add("DOCMODIFYID", curMaxModifyId);
                dicChangeColumns.Add("ROWNO", curMaxModifyId);
                dicChangeColumns.Add("MODIFYPERSONID", string.IsNullOrEmpty(this.Handle.PersonId) ? "(NoSet)" : this.Handle.PersonId);
                dicChangeColumns.Add("IP", this.Handle.LogIp);

                string opLog = string.Format("回退至修订版{0}，修改说明:{1}", toFallbackModifyVerId, desc);
                dicChangeColumns.Add("MODIFYDESC", opLog);
                dicChangeColumns.Add("DATETIME", LibDateUtils.GetCurrentDateTime());
                lcr.Add.Add(dicChangeColumns);//添加新行
                dicChanges.Add("DMDOCMODIFYHISTORY", lcr);  //文档的修订版添加新行

                this.Edit(new object[] { docId });
                this.Save(BillAction.Modif, new object[] { docId }, dicChanges);
                if (this.ManagerMessage.IsThrow)
                    return false;

                this.AddNewDocOpLog(docId, string.Format("回退文档:{0}", opLog), true);
                return true;
            }
            catch (Exception exp)
            {
                DMCommonMethod.WriteLog("DmDocumentBcf.FallBackVersion", string.Format("DocId:{0}\r\nToFallbackModifyVerId:{1}\r\nDesc:{2}\r\nError:{3}", 
                    docId, toFallbackModifyVerId, desc, exp.ToString()));
                this.ManagerMessage.AddMessage(LibMessageKind.Error, "服务器出现异常，请查看日志。");
                return false;
            }
            
        }

        #region 表单附件处理相关 
        [DataContract]
        public class MoveAttachResult
        {
            [DataMember]
            public bool success { get; set; }
            [DataMember]
            public string DocId { get; set; }
            [DataMember]
            public string DirId { get; set; }
            [DataMember]
            public string AttachSrc { get; set; }
        }
        /// <summary>
        /// 根据LibAttachData生成需要执行的sql语句
        /// </summary>
        /// <param name="attachData"></param>
        /// <param name="listSql"></param>
        protected void UpdateAttachStruct(LibAttachData attachData, List<string> listSql)
        {
            if (string.IsNullOrEmpty(attachData.AttachSrc))
            {
                attachData.AttachSrc = LibCommUtils.GetInternalId().ToString();
                StringBuilder builder = new StringBuilder();
                foreach (var item in attachData.PkList)
                {
                    if (item.Value.GetType() == typeof(string))
                        builder.AppendFormat("{0} = {1} and ", item.Key, LibStringBuilder.GetQuotObject(item.Value));
                    else
                        builder.AppendFormat("{0} = {1} and ", item.Key, item.Value);
                }
                if (builder.Length > 0)
                    builder.Remove(builder.Length - 4, 4);
                //更新单据附件关联字段
                listSql.Add(string.Format("update {0} set ATTACHMENTSRC='{1}' where {2}", attachData.TableName, attachData.AttachSrc, builder.ToString()));
            }
            foreach (var item in attachData.AttachList)
            {
                switch (item.Status)
                {
                    case LibAttachStatus.Add:
                        listSql.Add(string.Format("insert into AXPATTACHMENTRECORD(BELONGTOID,ORDERID,ORDERNUM,ATTACHMENTNAME,CANUSE,DOCID) values('{0}',{1},{2},'{3}',1,'{4}')",
                            attachData.AttachSrc, item.OrderId, item.OrderNum, item.AttachmentName, item.DocId));
                        break;
                    case LibAttachStatus.Modif:
                        listSql.Add(string.Format("update AXPATTACHMENTRECORD set ORDERNUM={2},ATTACHMENTNAME='{3}',DOCID='{4}' where BELONGTOID='{0}' and ORDERID={1}",
                            attachData.AttachSrc, item.OrderId, item.OrderNum, item.AttachmentName, item.DocId));
                        break;
                    case LibAttachStatus.Delete:
                        listSql.Add(string.Format("update AXPATTACHMENTRECORD set CANUSE=0 where BELONGTOID='{0}' and ORDERID={1}",
                            attachData.AttachSrc, item.OrderId));
                        break;
                }
            }
        }
        /// <summary>
        /// 移动表单附件
        /// 替代原先文档服务FileTransferService中的方法
        /// </summary>
        /// <param name="attachData"></param>
        /// <returns></returns>
        public MoveAttachResult MoveAttach(LibAttachData attachData)
        {
            MoveAttachResult result = new MoveAttachResult();
            result.success = false;

            this.DataAccess = new LibDataAccess();
            LibDBTransaction trans = this.DataAccess.BeginTransaction();

            string docId = string.Empty;
            string dirId = string.Empty;
            //添加表单附件文件到文档库
            if (AddAttachment(attachData.FileName, attachData.ProgId, attachData.RealFileName, out docId, out dirId) == false
                || string.IsNullOrEmpty(docId)
                || string.IsNullOrEmpty(dirId))
            {
                this.ManagerMessage.AddMessage(LibMessageKind.Error, "添加表单附件到文档库发生错误或生成的文档编号为空，请检查日志!");
                return null;
            }
            //将文档编号更新到对象中，以便下面生成sql语句时
            LibAttachStruct attachStruct = (from item in attachData.AttachList
                                            where item.Status == LibAttachStatus.Add
                                            select item).FirstOrDefault();
            if (attachStruct != null)
                attachStruct.DocId = docId;

            List<string> listSql = new List<string>();
            //更新结构
            UpdateAttachStruct(attachData, listSql);
            //更新附件记录明细
            listSql.Add(string.Format("insert into AXPATTACHMENTRECORDDETAIL(BELONGTOID,ORDERID,FILENAME,PERSONID,CREATETIME) values('{0}',{1},'{2}','{3}',{4})",
                attachData.AttachSrc, attachData.OrderId, attachData.FileName, attachData.PersonId, LibDateUtils.GetCurrentDateTime()));
            
            try
            {
                DataAccess.ExecuteNonQuery(listSql);
                trans.Commit();
            }
            catch
            {
                trans.Rollback();
                throw;
            }

            //添加成功后将用户对该文档的管理权限立即添加到内存权限缓存中
            if (OrignUserHandle != null)
            {
                DMUserPermission permission = (DMUserPermission)DMUserPermissionCache.Default.Get(OrignUserHandle.PersonId);
                if (permission != null)
                {
                    permission.AddPermission(false, docId, (int)DMFuncPermissionEnum.Manage);
                }
            }
            result.AttachSrc = attachData.AttachSrc;
            result.success = true;
            result.DocId = docId;
            result.DirId = dirId;

            return result;
        }
        /// <summary>
        /// 保存真实的原始用户
        /// 进行某些特殊操作（如表单附件的文档保存）需要使用系统用户(System)进行权限检查，但操作信息还需要使用原始用户信息
        /// </summary>
        protected LibHandle OrignUserHandle = null;
        /// <summary>
        /// 添加表单附件文档，成功返回true并设置输出参数文档编号
        /// </summary>
        /// <param name="tempFileName"></param>
        /// <param name="progId"></param>
        /// <param name="realFileName"></param>
        /// <param name="userHandle"></param>
        /// <param name="docId"></param>
        /// <returns></returns>
        public bool AddAttachment(string tempFileName,string progId,string realFileName,out string docId,out string dirId)
        {
            docId = string.Empty;
            dirId = string.Empty;

            string fileFullPath = Path.Combine(DMCommonMethod.GetDMRootTempPath(), tempFileName);
            if (string.IsNullOrEmpty(tempFileName) || File.Exists(fileFullPath) == false ||
                string.IsNullOrEmpty(progId))
                return false;
            OrignUserHandle = this.Handle;
            if (OrignUserHandle == null)
                return false;

            this.Handle = LibHandleCache.Default.GetSystemHandle();//设置为系统用户，以便权限检查通过
            try
            {
                DmDirectoryBcf bcf = new DmDirectoryBcf();
                bcf.DataAccess = this.DataAccess;
                //检查表单附件目录，如果需要的目录不存在则创建。返回检查得到的目录编号
                if (bcf.CheckAndAddBillAttachmentDir(progId, out dirId) == false)
                    return false;

                Dictionary<string, LibChangeRecord> dicChanges = new Dictionary<string, LibChangeRecord>();
                Dictionary<string, object> dicChangeColumns = new Dictionary<string, object>();

                Dictionary<string, string> extendParams = new Dictionary<string, string>();
                extendParams.Add(FileNameStringKey, "\"" + tempFileName + "\"");
                extendParams.Add(DocOpTypeStringKey, ((int)DocOpTypeENum.UploadBillAttachment).ToString());

                LibEntryParam entry = new LibEntryParam();
                entry.ParamStore.Add("DirId", dirId);
                entry.ParamStore.Add("DirType", (int)DirTypeEnum.Public);
                this.AddNew(entry);
                if (this.ManagerMessage.IsThrow)
                    return false;
                docId = LibSysUtils.ToString(this.DataSet.Tables[0].Rows[0]["DOCID"]);
                object[] pks = new object[] { docId };

                //因对于Add的对象Save方法中会检查Add的第一条记录数据并做相关处理，因此需要模拟生成前端传递来的change数据
                LibChangeRecord record = new LibChangeRecord();
                foreach (DataColumn col in this.DataSet.Tables[0].Columns)
                {
                    dicChangeColumns.Add(col.ColumnName, this.DataSet.Tables[0].Rows[0][col.ColumnName]);//将文档主表的第一行数据变成change数据
                }
                dicChangeColumns["DIRID"] = dirId;
                dicChangeColumns["DOCNAME"] = realFileName;
                dicChangeColumns["CREATORID"] = OrignUserHandle.PersonId;                
                dicChangeColumns["CREATORNAME"] = OrignUserHandle.PersonName;
                record.Add.Add(dicChangeColumns);
                dicChanges.Add("DMDOCUMENT", record);

                if (OrignUserHandle.UserId.Equals("admin") == false &&
                    string.IsNullOrEmpty(OrignUserHandle.PersonId) == false)
                {
                    //使上传用户对文档具有管理权限
                    record = new LibChangeRecord();
                    dicChangeColumns = new Dictionary<string, object>();
                    dicChangeColumns["DOCID"] = docId;
                    dicChangeColumns["ROW_ID"] = 1;
                    dicChangeColumns["ROWNO"] = 1;
                    dicChangeColumns["BELONGTYPE"] = 2;//个人
                    dicChangeColumns["BELONGID"] = OrignUserHandle.PersonId;                   
                    dicChangeColumns["ISOPERATEPOWER"] = true;
                    dicChangeColumns["OPERATEMARK"] = (int)DMFuncPermissionEnum.Manage;
                    record.Add.Add(dicChangeColumns);
                    dicChanges.Add("DMDOCPERMISSION", record);

                    record = new LibChangeRecord();
                    dicChangeColumns = new Dictionary<string, object>();
                    dicChangeColumns["DOCID"] = docId;
                    dicChangeColumns["PARENTROWID"] = 1;
                    dicChangeColumns["ROW_ID"] = 17;//管理权限的顺序号
                    dicChangeColumns["ROWNO"] = 17;
                    dicChangeColumns["OPERATEPOWERID"] = (int)DMFuncPermissionEnum.Manage;
                    dicChangeColumns["OPERATEPOWERNAME"] = "管理";
                    dicChangeColumns["CANUSE"] = true;
                    dicChangeColumns["REMARK"] = "上传表单附件自动拥有管理权限";
                    record.Add.Add(dicChangeColumns);
                    dicChanges.Add("DMDOCOPERATEPOWER", record);

                }

                this.DataSet.Clear();//将通过addNew添加的数据全清空,以免和通过change数据添加的重复了。
                                
                this.Save(BillAction.AddNew, pks, dicChanges, extendParams);
                if (this.ManagerMessage.IsThrow)
                    return false;
                return true;
            }
            catch(Exception exp)
            {
                DMCommonMethod.WriteLog("DmDocumentBcf.AddAttachment",
                    string.Format("TempFileFullPathName:{0}\r\nProgId:{1}\r\nRealFileName:{2}\r\nAccount:{3}\r\nError:{4}",
                    tempFileName, progId, realFileName, OrignUserHandle.UserId, exp.ToString()));
                return false;
            }
        }
        #endregion

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
        /// <summary>
        /// 根据目录编号，获取该目录下的文档信息
        /// </summary>
        /// <param name="libHandle">用户信息</param>
        /// <param name="dirID"></param>
        /// <returns></returns>
        public DataTable GetDocOfDir(LibHandle libHandle,string dirID,DirTypeEnum dirType)
        {
            DataTable dtDoc = this.DataSet.Tables[0];

            string sqlDetail = string.Format("select A.*,B.PERSONNAME as CREATORNAME from {0} A left join COMPERSON B on A.CREATORID=B.PERSONID where A.DIRID = '{1}'",
                "DMDOCUMENT", dirID);
            if (dirType == DirTypeEnum.Private)
            {
                //个人文档根据创建人
                sqlDetail = string.Format("{0} And A.CREATORID = '{1}'", sqlDetail, libHandle.PersonId);
            }         
            this.DataAccess.ExecuteDataTable(sqlDetail, dtDoc);
            if (libHandle.UserId != "admin" && dirType == DirTypeEnum.Public)
            {
                //公共文档再从查询出来的数据行中进行筛选
                List<string> docIdList = new List<string>();
                foreach (DataRow row in dtDoc.Rows)
                {
                    docIdList.Add(LibSysUtils.ToString(row["DOCID"]));
                }
                docIdList = DMPermissionControl.Default.FilterDocIds(libHandle, DMFuncPermissionEnum.Browse, docIdList);
                List<DataRow> toDeleteList = new List<DataRow>();
                foreach (DataRow row in dtDoc.Rows)
                {
                    if (docIdList.Contains(LibSysUtils.ToString(row["DOCID"])) == false)
                        toDeleteList.Add(row);
                }
                foreach (DataRow row in toDeleteList)
                {
                    dtDoc.Rows.Remove(row);
                }
                dtDoc.AcceptChanges();//删除没有权限的记录行
            }            
            return dtDoc;
        }
        /// <summary>
        /// 获取目录Id列表下的文档Id列表
        /// </summary>
        /// <param name="dirIdList"></param>
        /// <returns></returns>
        public List<string> GetDocIdsOfDirList(List<string> dirIdList)
        {
            for (int i = 0; i < dirIdList.Count; i++)
            {
                dirIdList[i] = LibStringBuilder.GetQuotObject(dirIdList[i]);//添加‘’
            }
            string dirIdsStr = string.Join(",", dirIdList);
            string sql = " select DOCID from DMDOCUMENT where DIRID in ( " + dirIdsStr + " )";
            DataSet ds = this.DataAccess.ExecuteDataSet(sql);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                List<string> docIdList = new List<string>();
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    docIdList.Add(LibSysUtils.ToString(row["DOCID"]));
                }
                return docIdList;
            }
            else
                return null;
        }
      

        /// <summary>
        /// 修改前的文档文件夹绝对路径地址
        /// </summary>
        private string _OldDocPath = string.Empty;
        /// <summary>
        /// 文档的当前修订号
        /// </summary>
        private int _CurModifyVerId = 1;
        /// <summary>
        /// 处理文件的详情信息、增加修订版等
        /// </summary>
        /// <param name="masterRow"></param>
        private void DealFileInfo(DataRow masterRow,bool isAddNew)
        {
            string fileName = FileNameParamValue;
            if (string.IsNullOrEmpty(fileName) == false)
            {
                //文件在Temp目录的路径
                string tempPath = Path.Combine(DMCommonMethod.GetDMRootTempPath(), fileName);
                if (File.Exists(tempPath) == false) {
                    this.ManagerMessage.AddMessage(LibMessageKind.Error, "指定的临时文件不存在！");
                    return;
                }
                FileInfo fileInfo = new FileInfo(tempPath);
                masterRow["DOCSIZE"] = Math.Round(fileInfo.Length / 1024.0 / 1024.0, 3);//转换为以M为单位的大小
                masterRow["DOCTYPE"] = fileInfo.Extension;

                #region  添加修订版
                DataTable modifyTable = this.DataSet.Tables[4];//修订版数据表
                DataRow modifyRow = modifyTable.NewRow();
                modifyRow["DOCID"] = masterRow["DOCID"];
                if (isAddNew)
                {
                    _CurModifyVerId = 1;
                    modifyRow["DOCMODIFYID"] = 1;//第一次添加的文件，修订号为1
                    modifyRow["ROWNO"] = 1;
                }                    
                else
                {
                    int maxId = 1;
                    foreach (DataRow row in modifyTable.Rows)
                    {
                        if (LibSysUtils.ToInt32(row["DOCMODIFYID"]) > maxId)
                            maxId = LibSysUtils.ToInt32(row["DOCMODIFYID"]);
                    }
                    maxId++;
                    modifyRow["DOCMODIFYID"] = maxId;
                    modifyRow["ROWNO"] = maxId;
                    _CurModifyVerId = maxId;
                }                
                string creatorId = LibSysUtils.ToString(masterRow["CREATORID"]);
                if (this.OrignUserHandle != null && string.IsNullOrEmpty(this.OrignUserHandle.PersonId) == false)
                    creatorId = this.OrignUserHandle.PersonId;

                if (string.IsNullOrEmpty(creatorId))
                    modifyRow["MODIFYPERSONID"] = "(NotSet)";
                else
                    modifyRow["MODIFYPERSONID"] = creatorId;

                string loginIp = "(NotSet)";
                if (OrignUserHandle != null && string.IsNullOrEmpty(OrignUserHandle.LogIp) == false)
                    loginIp = OrignUserHandle.LogIp;
                else if(this.Handle!=null&& string.IsNullOrEmpty(Handle.LogIp) == false)
                    loginIp = this.Handle.LogIp;
                modifyRow["IP"] = loginIp;

                modifyRow["DATETIME"] = LibDateUtils.GetCurrentDateTime();
                modifyTable.Rows.Add(modifyRow);
                #endregion

            }
            else
            {
                
                this.ManagerMessage.AddMessage(LibMessageKind.Error, "没有需要的文件名参数!");
                return;
            }
        }
        protected override void BeforeUpdate()
        {
            base.BeforeUpdate();

            #region 操作权限标识设置
            HashSet<string> hasSet = new HashSet<string>();
            foreach (DataRow curRow in this.DataSet.Tables[1].Rows)
            {
                if (curRow.RowState == DataRowState.Deleted)
                    continue;
                DataRow[] subRows = curRow.GetChildRows(DmDocumentBcfTemplate.PermissionDetailSubRelationName, DataRowVersion.Current);
                int mark = 0;
                foreach (DataRow subRow in subRows)
                {
                    if (LibSysUtils.ToBoolean(subRow["CANUSE"]))
                        mark += LibSysUtils.ToInt32(subRow["OPERATEPOWERID"]);
                }
                curRow["OPERATEMARK"] = mark;
                string type_BelongID = string.Format("{0}_{1}", LibSysUtils.ToInt32(curRow["BELONGTYPE"]), LibSysUtils.ToString(curRow["BELONGID"]));
                if (hasSet.Contains(type_BelongID))
                    this.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("权限行{0}的拥有者重复。", curRow["ROWNO"]));
                else
                    hasSet.Add(type_BelongID);
            }
            #endregion

            DataRow masterRow = this.DataSet.Tables[0].Rows[0];
            string docId= LibSysUtils.ToString(masterRow["DOCID"]);
            try
            {
               
                if (masterRow.RowState == DataRowState.Added)
                {
                    #region 检查管理权限
                    //根据权限设置，确定是否采用前台传递的数据。虽然根据权限设置前端控制不显示相关页面，但需要防止前端伪造数据。
                    DirTypeEnum dirType = (DirTypeEnum)LibSysUtils.ToInt32(masterRow["DIRTYPE"]);
                    string dirId = LibSysUtils.ToString(masterRow["DIRID"]);
                    if (dirType == DirTypeEnum.Public)
                    {
                        //公共目录下的文档才进行检查。
                        if (DMPermissionControl.Default.HasPermission(this.Handle, dirId, docId, DMFuncPermissionEnum.Manage) == false)
                        {
                            //新增时如果没有管理权限，则设置了文档权限也没用。
                            this.DataSet.Tables[2].RejectChanges();//文档权限细项
                            this.DataSet.Tables[1].RejectChanges();//文档权限
                            
                        }
                    }
                    this.DataSet.Tables[3].RejectChanges();//版本历史,不可通过前台编辑
                    this.DataSet.Tables[4].RejectChanges();//修订版管理,不可通过前台编辑
                    this.DataSet.Tables[5].RejectChanges();//文档审计,不可通过前台编辑

                    #endregion

                    //添加前设置文档文件夹对应的文件夹名称。文档的文件夹以D_开头
                    string docDirName = string.Format("D_{0}",Guid.NewGuid().ToString().ToUpper());//新的文件夹名称，用于对应新创建的文件夹
                    masterRow["SAVEPATH"] = docDirName;//保存对应的文件夹路径

                    //处理文件的详情信息、增加修订版等
                    DealFileInfo(masterRow, true);

                    string opLogInfo = string.Empty;
                    if (DocOpTypeValue != DocOpTypeENum.UnknownOrUnset)
                    {
                        switch (DocOpTypeValue)
                        {
                            case DocOpTypeENum.AddNew:
                                opLogInfo = "保存新建的文档";
                                break;
                            case DocOpTypeENum.Upload:
                                opLogInfo = "保存上传的文档";
                                break;
                            case DocOpTypeENum.UploadBillAttachment:
                                opLogInfo = "保存通过表单附件上传的文档";
                                break;
                        }
                    }
                    else
                    {
                        opLogInfo = "添加文档";
                    }
                    this.AddNewDocOpLog(docId, opLogInfo, false);
                }
                else
                {
                    string oldDirID = LibSysUtils.ToString(masterRow["DIRID", DataRowVersion.Original]);
                    string currentDirID = LibSysUtils.ToString(masterRow["DIRID"]);
                    if (oldDirID.Equals(currentDirID) == false)
                    {
                        int oldDirType = LibSysUtils.ToInt32(masterRow["DIRTYPE", DataRowVersion.Original]);
                        int newDirType = LibSysUtils.ToInt32(masterRow["DIRTYPE"]);
                        if (newDirType != oldDirType) {
                            this.ManagerMessage.AddMessage(LibMessageKind.Error, "不能将目录更改为与原目录类型不一致的目录!");
                            return;
                        }

                        #region 检查管理权限及前台数据
                        //根据权限设置，确定是否采用前台传递的数据。虽然根据权限设置前端控制不显示相关页面，但需要防止前端伪造数据。
                        DirTypeEnum dirType = (DirTypeEnum)oldDirType;//仍采用原先目录类型检查
                        string dirId = oldDirID;
                        if (dirType == DirTypeEnum.Public)
                        {
                            //公共目录下的文档才进行检查。
                            if (DMPermissionControl.Default.HasPermission(this.Handle, dirId, docId, DMFuncPermissionEnum.Manage) == false)
                            {
                                //新增时如果没有管理权限，则设置了文档权限也没用。
                                this.DataSet.Tables[2].RejectChanges();//文档权限细项
                                this.DataSet.Tables[1].RejectChanges();//文档权限
                            }
                        }
                        this.DataSet.Tables[3].RejectChanges();//版本历史,不可通过前台编辑
                        this.DataSet.Tables[4].RejectChanges();//修订版管理,不可通过前台编辑
                        this.DataSet.Tables[5].RejectChanges();//文档审计,不可通过前台编辑
                        #endregion
                        
                        //文档所在的目录发生了变化
                        DirLinkAddress dirLink = new DirLinkAddress(oldDirID, this.DataAccess);
                        _OldDocPath = string.Empty;
                        //如果是修改的，则记录修改前的SavePath
                        string path = DMCommonMethod.GetDMRootPath((DirTypeEnum)LibSysUtils.ToInt32(masterRow["DIRTYPE"]));
                        _OldDocPath = Path.Combine(path, dirLink.DirSavePath, LibSysUtils.ToString(masterRow["SAVEPATH"]));//保存原先的文档文件夹的绝对路径,需要加上文档文件夹的名称
                    }
                    if (string.IsNullOrEmpty(FileNameParamValue) == false)
                    {
                        //处理文件的详情信息、增加修订版等
                        DealFileInfo(masterRow, false);
                    }
                    #region 检查并添加文档操作审计记录
                    string opLogInfo = string.Empty;
                    if (DocOpTypeValue != DocOpTypeENum.UnknownOrUnset)
                    {
                        switch (DocOpTypeValue)
                        {
                            case DocOpTypeENum.Edit:
                                opLogInfo = "保存编辑的文档。";
                                break;
                            case DocOpTypeENum.Replace:
                                opLogInfo = "替换文档";
                                break;
                        }
                    }
                    else
                    {
                        opLogInfo = "保存修改的文档信息。";
                        //检查是否修改了文档权限
                        foreach(DataRow row in this.DataSet.Tables[2].Rows)
                        {
                            if(row.RowState!=DataRowState.Unchanged)
                            {
                                opLogInfo += "修改了文档权限。";
                                break;
                            }
                        }
                        string oldDocName = LibSysUtils.ToString(masterRow["DOCNAME", DataRowVersion.Original]);
                        string newDocName = LibSysUtils.ToString(masterRow["DOCNAME", DataRowVersion.Current]);
                        if (newDocName.Equals(oldDocName) == false)
                            opLogInfo += string.Format("重命名了文档，从{0}修改为{1}。", oldDocName, newDocName);                        
                    }
                    this.AddNewDocOpLog(docId, opLogInfo, false);
                    #endregion
                }
            }
            catch (Exception exp)
            {
                this.ManagerMessage.AddMessage(LibMessageKind.Error, "保存文档失败，原因:" + exp.Message);
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
                string docId = LibSysUtils.ToString(masterRow["DOCID"]);
                string docType= LibSysUtils.ToString(masterRow["DOCTYPE"]);
                if (masterRow.RowState == DataRowState.Added)
                {
                    //添加时创建目录文件夹  
                    DirLinkAddress dirLink = new DirLinkAddress(LibSysUtils.ToString(masterRow["DIRID"]), this.DataAccess);                   
                    path = Path.Combine(path, dirLink.DirSavePath, LibSysUtils.ToString(masterRow["SAVEPATH"]));//新文档文件夹路径
                    Directory.CreateDirectory(path);

                    path = Path.Combine(path, "1" + LibSysUtils.ToString(masterRow["DOCTYPE"]));//将初始的修订号作为文件名

                    string fileName = FileNameParamValue;
                    if (string.IsNullOrEmpty(fileName) == false)
                    {
                        //将文件从Temp目录移动到实际目录                       
                        string tempPath = Path.Combine(DMCommonMethod.GetDMRootTempPath(), fileName);
                        File.Move(tempPath, path);//将文件从Temp目录移动到文档文件夹位置

                        //触发新文档需要建立全文索引
                        FullIndexHelper.RaiseNewDocArrivedToFullIndex(docId, docType, 1, true, path);//全新的文档
                    }
                    else
                    {
                        this.ManagerMessage.AddMessage(LibMessageKind.Error, "没有需要的文件名参数!");
                        return;
                    }
                    //更新目录下的文档数
                    DmDirectoryBcf.UpdateDocCountOfDir(LibSysUtils.ToString(masterRow["DIRID"]),this.DataAccess);                    
                }
                else
                {
                    //update时检查所属目录是否变化，如有变化根据将目录对应的文件夹移动到所属目录文件夹下
                    string oldDirID = LibSysUtils.ToString(masterRow["DIRID", DataRowVersion.Original]);
                    string currentDirID = LibSysUtils.ToString(masterRow["DIRID"]);
                    if (oldDirID.Equals(currentDirID) == false)
                    {
                        DirLinkAddress dirLink = new DirLinkAddress(LibSysUtils.ToString(masterRow["DIRID"]), this.DataAccess);
                        string newDirPath = Path.Combine(path, dirLink.DirSavePath, LibSysUtils.ToString(masterRow["SAVEPATH"]));
                        Directory.Move(_OldDocPath, newDirPath);//移动文件夹
                    }
                    else
                    {
                        DirLinkAddress dirLink = new DirLinkAddress(false, LibSysUtils.ToString(masterRow["DOCID"]), this.DataAccess, true);
                        path = Path.Combine(path, dirLink.DirSavePath, LibSysUtils.ToString(masterRow["SAVEPATH"]));//新文档文件夹路径       
                        path = Path.Combine(path, _CurModifyVerId.ToString() + LibSysUtils.ToString(masterRow["DOCTYPE"]));//将初始的修订号作为文件名
                        string fileName = FileNameParamValue;
                        if (string.IsNullOrEmpty(fileName) == false)
                        {
                            //将文件从Temp目录移动到实际目录                       
                            string tempPath = Path.Combine(DMCommonMethod.GetDMRootTempPath(), fileName);
                            File.Move(tempPath, path);//将文件从Temp目录移动到文档文件夹位置

                            //触发新文档需要建立全文索引
                            FullIndexHelper.RaiseNewDocArrivedToFullIndex(docId, docType, 1, true, path);//全新的文档
                        }
                    }
                }
            }
            catch (Exception exp)
            {
                this.ManagerMessage.AddMessage(LibMessageKind.Error, "保存文档失败，原因:" + exp.Message);
                throw exp;
            }
        }

        private bool _IsDeleteDiskDoc = true;
        /// <summary>
        /// 在删除时是否删除磁盘上的文档文件。默认为true，如果是通过目录等连带删除时，需要提前将此值设置为false
        /// </summary>
        public bool IsDeleteDiskDoc
        {
            get { return _IsDeleteDiskDoc; }
            set { _IsDeleteDiskDoc = value; }
        }
        protected override void BeforeDelete()
        {
            base.BeforeDelete();
            
        }
        protected override void AfterDelete()
        {
            base.AfterDelete();
            DataRow masterRow = this.DataSet.Tables[0].Rows[0];
            if (masterRow == null)
                return;
            string dirId = LibSysUtils.ToString(masterRow["DIRID"]);
            string docId = LibSysUtils.ToString(masterRow["DOCID"]);

            FullIndexHelper.RaiseNeedDeleteDocFullIndex(docId);
            //更新目录下的文档数
            DmDirectoryBcf.UpdateDocCountOfDir(dirId, this.DataAccess);
            this.AddNewDocOpLog(docId, "删除了文档。", false);

            if (IsDeleteDiskDoc)
            {
                //删除文档文件夹目录。sql中的表单记录操作可以回滚，磁盘上的文件目录和文档操作统一处理，失败了也不用回滚。
                DirLinkAddress dirLink = new DirLinkAddress(dirId, this.DataAccess);
                if (dirLink.DirSavePath == string.Empty)
                    return;
                //先定位到文档库根路径             
                string path = DMCommonMethod.GetDMRootPath((DirTypeEnum)LibSysUtils.ToInt32(masterRow["DIRTYPE"]));
                path = Path.Combine(path, dirLink.DirSavePath,LibSysUtils.ToString(masterRow["SAVEPATH"]));
                try
                {
                    if (Directory.Exists(path))
                        Directory.Delete(path, true);//删除文档文件夹及其下的所有修订版文件等                        
                    
                }
                catch (Exception exp)
                {
                    //对于删除文档文件夹在磁盘上的文件夹的动作，因为文件系统的删除无法回滚，不再向上抛出异常（以免触发本级事务回滚），仅记录异常信息，以便后续手动清理
                    DMCommonMethod.WriteLog("DeleteDocument", string.Format("DirID:{0}\r\nDocID:{1}\r\nPath:{2}\r\nError:{3}", 
                        dirId, LibSysUtils.ToInt32(masterRow["DOCID"]), path, exp.ToString()));
                }
            }
        }

        #region 操作权限明细项及控制相关
        protected override void CheckFieldReturn(int tableIndex, string fieldName, object[] curPk, Dictionary<string, object> fieldKeyAndValue, Dictionary<string, object> returnValue)
        {
            base.CheckFieldReturn(tableIndex, fieldName, curPk, fieldKeyAndValue, returnValue);
            if (tableIndex == 1 && fieldName == "BELONGID" && curPk != null && curPk.Length == 1)
            {
                this.Template.GetViewTemplate(this.DataSet);
                returnValue.Add("OperatePowerData", DMPermissionControl.Default.BuildPowerInfo(false,this.Template.FuncPermission.Permission));//构建权限项列表
            }
        }
        protected override bool CheckBrowseTo(object[] pks)
        {
            string sql = string.Format("select B.DIRID,B.DIRTYPE from DMDOCUMENT A left join DMDIRECTORY B on A.DIRID = B.DIRID where A.DOCID = '{0}'", pks[0]);
            DataSet ds = this.DataAccess.ExecuteDataSet(sql);           
            DataRow masterRow = ds.Tables[0].Rows[0];
            string dirId = LibSysUtils.ToString(masterRow["DIRID"]);
            int dirType = LibSysUtils.ToInt32(masterRow["DIRTYPE"]);          
            if (dirType == (int)DirTypeEnum.Private)
                return true;//个人目录不做控制            

            return CheckPermission(dirId, pks[0].ToString(), DMFuncPermissionEnum.Browse);
        }
        protected override bool CheckAddNew(LibEntryParam entryParam)
        {
            //使用入口参数传递检查权限需要的参数
            string dirId = "";
            int dirType = -1;
            if (entryParam != null && entryParam.ParamStore.Count >= 2 &&
                entryParam.ParamStore.ContainsKey("DirId") &&
                entryParam.ParamStore.ContainsKey("DirType"))
            {
                dirId = entryParam.ParamStore["DirId"].ToString();
                int.TryParse(entryParam.ParamStore["DirType"].ToString(), out dirType);
                //用完后置空
                entryParam.ParamStore.Remove("DirId");
                entryParam.ParamStore.Remove("DirType");
            }
            else
            {
                this.ManagerMessage.AddMessage(LibMessageKind.Error, "缺少检查权限需要的参数。");
                return false;
            }
            if (dirType == (int)DirTypeEnum.Private)
                return true;//个人目录不做控制
            //文档的新增对应的是所在目录的“上传”权限
            return CheckPermission(dirId,string.Empty, DMFuncPermissionEnum.Upload);
        }
        protected override bool CheckModif(object[] pks)
        {
            DataRow masterRow = this.DataSet.Tables[0].Rows[0];
            string dirId = LibSysUtils.ToString(masterRow["DIRID"]);
            int dirType= LibSysUtils.ToInt32(masterRow["DIRTYPE"]);
            if (dirType == (int)DirTypeEnum.Private)
                return true;//个人目录不做控制            
            return CheckPermission(dirId, pks[0].ToString(), DMFuncPermissionEnum.Edit);
        }
        protected override bool CheckDelete()
        {
            DataRow masterRow = this.DataSet.Tables[0].Rows[0];
            string dirId = LibSysUtils.ToString(masterRow["DIRID"]);
            int dirType = LibSysUtils.ToInt32(masterRow["DIRTYPE"]);
            if (dirType == (int)DirTypeEnum.Private)
                return true;//个人目录不做控制
            string docId = LibSysUtils.ToString(masterRow["DOCID"]);
            return CheckPermission(dirId, docId, DMFuncPermissionEnum.Delete);
        }
        //导入、导出、打印等可能需要的方法需要编写覆盖基类的方法，应在相关方法中检查权限


        /// <summary>
        /// 权限检查
        /// </summary>
        /// <param name="dirId"></param>
        /// <param name="docId"></param>
        /// <param name="permission"></param>
        /// <returns></returns>
        public bool CheckPermission(string dirId,string docId, DMFuncPermissionEnum permission)
        {           
            bool ret = true;
            if (LibHandleCache.Default.GetSystemHandle() != this.Handle&&this.Handle.UserId!="admin")
            {
                ret = DMPermissionControl.Default.HasPermission(this.Handle, dirId, docId, permission);
                if (!ret)
                {
                    this.ManagerMessage.AddMessage(LibMessageKind.Error, "当前人员不具备操作权限。");
                }
            }
            return ret;
        }
        #endregion

        #region 文档审计（操作记录日志）       
        /// <summary>
        /// 添加文档操作日志说明。
        /// 同时可以配置是否增加文档的点击数
        /// </summary>
        /// <param name="docId"></param>
        /// <param name="opDesc"></param>
        public void AddNewDocOpLog(string docId,string opDesc,bool isAddClickCount)
        {
            if (string.IsNullOrEmpty(docId) || string.IsNullOrEmpty(opDesc))
                return;
            try
            {
                int curMaxRowId = 1;
                string sql = string.Format("select max(ROW_ID) from DMDOCOPLOG where DOCID= '{0}'",docId);
                object obj = this.DataAccess.ExecuteScalar(sql);
                if (obj != DBNull.Value)
                    curMaxRowId = LibSysUtils.ToInt32(obj) + 1;

                string loginIp = "(NotSet)";
                if (OrignUserHandle != null && string.IsNullOrEmpty(OrignUserHandle.LogIp) == false)
                    loginIp = OrignUserHandle.LogIp;
                else if (this.Handle != null && string.IsNullOrEmpty(Handle.LogIp) == false)
                    loginIp = this.Handle.LogIp;

                string personId = "(NotSet)";
                if (OrignUserHandle != null && string.IsNullOrEmpty(OrignUserHandle.PersonId) == false)
                    personId = OrignUserHandle.PersonId;
                else if (this.Handle != null && string.IsNullOrEmpty(Handle.PersonId) == false)
                    personId = this.Handle.PersonId;

                sql = string.Format("insert into DMDOCOPLOG(DOCID,ROW_ID,ROWNO,OPPERSONID,IP,OPDESC,DATETIME) values('{0}',{1},{2},'{3}','{4}','{5}',{6})",
                                                                            docId, curMaxRowId, curMaxRowId,
                                                                            personId,
                                                                            loginIp,
                                                                            opDesc,
                                                                            LibDateUtils.GetCurrentDateTime()
                                                                            );
                this.DataAccess.ExecuteNonQuery(sql);

                //增加文档的点击数
                if (isAddClickCount)
                {
                    sql = string.Format("update DMDOCUMENT set CLICKCOUNT=CLICKCOUNT+1 where DOCID = '{0}'", docId);
                    this.DataAccess.ExecuteNonQuery(sql);
                }
            }
            catch(Exception exp)
            {
                DMCommonMethod.WriteLog("DmDocumentBcf.AddNewDocOpLog", string.Format("DocId:{0}\r\nOpDesc:{1}\r\nError:{2}",docId,opDesc,exp.ToString()));
            }
        }
        #endregion
        /// <summary>
        /// 根据权限设置返回不同的视图模板
        /// </summary>
        /// <param name="checkDirType"></param>
        /// <param name="checkDirId"></param>
        /// <param name="docId"></param>
        /// <param name="entryParam"></param>
        /// <returns></returns>
        public LibViewTemplate GetViewTemplateOfPermission(int checkDirType, string checkDirId, string docId,LibEntryParam entryParam = null)
        {
            LibBillTpl tpl = (LibBillTpl)base.GetViewTemplate(entryParam);
            bool isRemoveManageView = true;
            if (checkDirType == (int)DirTypeEnum.Private || checkDirType == (int)DirTypeEnum.PrivateRoot)
                isRemoveManageView = false;
            else
            {
                if (DMPermissionControl.Default.HasPermission(this.Handle, checkDirId, docId, DMFuncPermissionEnum.Manage))
                    isRemoveManageView = false;
            }
            if (isRemoveManageView)
            {
                //移除文档权限等管理面板
                LibBillLayout layout = tpl.Layout as LibBillLayout;
                layout.TabRange.RemoveAt(0);//文档权限
                layout.SubBill.Remove(2);//文档操作细项
                layout.TabRange.RemoveAt(2);//文档审计,因为前面“文档权限”已被移除，因此序号由3变为2
            }
            return tpl;
        }
        protected override LibTemplate RegisterTemplate()
        {
            return new DmDocumentBcfTemplate("dm.Document");
        }

        [LibBusinessTask(Name = "LiveUpdate", DisplayText = "删除临时文件")]
        public DataSet LiveUpdate()
        {
            DataSet ds = new DataSet();
            //删除临时文件夹中的超过1小时的文件
            DeleteTempFile();
            return ds;
        }
        /// <summary>
        /// 
        /// </summary>
        private void DeleteTempFile()
        {
            DirectoryInfo dirInfo = new DirectoryInfo(DMCommonMethod.GetDMRootTempPath());
            FileInfo[] files = dirInfo.GetFiles();
            foreach (FileInfo file in files)
            {
                DateTime start = file.LastWriteTime;
                DateTime end = DateTime.Now;
                if (start.AddHours(1) < end)
                {
                    file.Delete();
                }
            }
        }
    }

    public class DmDocumentBcfTemplate : LibTemplate
    {   
        /// <summary>
        /// 文档目录的文档表名称
        /// </summary>
        private const string dmDocTableName = "DMDOCUMENT";
        /// <summary>
        /// 文档的权限表名称
        /// </summary>
        private const string dmDocPermissionTableName = "DMDOCPERMISSION";

        /// <summary>
        /// 权限明细表名称
        /// </summary>
        private const string dmSubTableName = "DMDOCOPERATEPOWER";
        /// <summary>
        /// 权限标识与详细操作权限项的关系名称
        /// </summary>
        public static string PermissionDetailSubRelationName
        {
            get { return string.Format("{0}_{1}", dmDocPermissionTableName, dmSubTableName); }
        }

        /// <summary>
        /// 文档的版本历史表名称   由人工设置确认的版本号
        /// </summary>
        private const string dmDocVerHistoryTableName = "DMDOCVERHISTORY";
        /// <summary>
        /// 文档的修订版历史表名称  编辑一次新增一次的修订记录
        /// </summary>
        private const string dmDocModifyHistoryTableName = "DMDOCMODIFYHISTORY";
        /// <summary>
        /// 文档的操作记录表名称 保存上传、修改版本号，编辑文档内容等针对文档的操作日志
        /// </summary>
        private const string dmDocOpLogTableName = "DMDOCOPLOG";


        /// <summary>
        /// 构造函数。注意BillType类型为Document
        /// </summary>
        /// <param name="progId"></param>
        public DmDocumentBcfTemplate(string progId)
            : base(progId, BillType.Master, "文档")
        {

        }
        /// <summary>
        /// 创建数据集合
        /// </summary>
        protected override void BuildDataSet()
        {
            this.DataSet = new DataSet();

            #region 文档
            //文档
            DataTable dmDocument = new DataTable(dmDocTableName);
            //文档编号设置ReadOnly为true是为了前台通过功能按钮来操作修改
            DataSourceHelper.AddColumn(new DefineField(dmDocument, "DOCID", "文档代码", FieldSize.Size50) { DataType = LibDataType.Text, AllowEmpty = false, AllowCopy = false, ReadOnly = true });
            DataSourceHelper.AddColumn(new DefineField(dmDocument, "DIRID", "目录", FieldSize.Size20)
            {
                ControlType = LibControlType.IdName,
                AllowEmpty = false,
                RelativeSource = new RelativeSourceCollection()
                {
                    new RelativeSource("dm.Directory")
                    {
                         RelFields = new RelFieldCollection()
                         {
                             new RelField("DIRNAME", LibDataType.NText,FieldSize.Size100,"目录名称"),
                             new RelField("DIRTYPE", LibDataType.Int32,FieldSize.Size100,"目录类型")
                         }
                    }
                }
            });            
            DataSourceHelper.AddColumn(new DefineField(dmDocument, "DOCNAME", "文档名称", FieldSize.Size200) { DataType = LibDataType.NText, AllowEmpty = false });         
            //如txt,doc等，如果用于展示目录信息则此项固定为“目录”。后台自动根据文件的扩展名获取类型
            DataSourceHelper.AddColumn(new DefineField(dmDocument, "DOCTYPE", "类型", FieldSize.Size20) { DataType = LibDataType.NText,ReadOnly = true });
            DataSourceHelper.AddColumn(new DefineField(dmDocument, "LOCKSTATE", "锁定状态") { DataType = LibDataType.Int32, ReadOnly = true, ControlType = LibControlType.TextOption, TextOption = new string[] { "未锁定", "已锁定" } });
            //是否是通过其他模块的表单附件的形式上传的
            DataSourceHelper.AddColumn(new DefineField(dmDocument, "ISATTACHMENT", "是否附件") { DataType = LibDataType.Boolean, ReadOnly = true, ControlType = LibControlType.YesNo, DefaultValue = false });
            //此项仅后台设置的文档的大小，以M为单位，保留2位小数
            DataSourceHelper.AddColumn(new DefineField(dmDocument, "DOCSIZE", "大小(M)") { DataType = LibDataType.Numeric, ControlType = LibControlType.Double, Precision = 3, ReadOnly = true });
            //虚字段 目录链接，根据目录（此处指文档所在的目录）的父目录关系，构造形如"\公共文档\财务部文档"的目录链接地址样式
            DataSourceHelper.AddColumn(new DefineField(dmDocument, "DIRLINKADDRESS", "目录链接", FieldSize.Size2000) { ControlType = LibControlType.Id, FieldType = FieldType.Virtual, ReadOnly = true });
            //版本号ReadOnly为true是为了前台通过功能按钮来操作修改
            DataSourceHelper.AddColumn(new DefineField(dmDocument, "DOCVERSION", "版本", FieldSize.Size20) { DataType = LibDataType.NText, DefaultValue = "0.1", ReadOnly = true });
            //此项用于后台统计本文档的点击(阅读)次数
            DataSourceHelper.AddColumn(new DefineField(dmDocument, "CLICKCOUNT", "点击数") { DataType = LibDataType.Int32, ControlType = LibControlType.Number, ReadOnly = true });
            //此项用于后台设置是否已建立全文索引
            DataSourceHelper.AddColumn(new DefineField(dmDocument, "ISFULLINDEX", "已全文索引") { DataType = LibDataType.Boolean, ControlType = LibControlType.YesNo, ReadOnly = true, DefaultValue = false });
            //此路径为文档的多个修订版和设定版文件所在的目录名称（不包含其他路径）
            DataSourceHelper.AddColumn(new DefineField(dmDocument, "SAVEPATH", "存储路径",FieldSize.Size2000) { DataType = LibDataType.NText, ControlType = LibControlType.NText, ReadOnly = true });
            //文档删除状态，如果为1表示已删除
            DataSourceHelper.AddColumn(new DefineField(dmDocument, "ISDELETE", "已删除") { DataType = LibDataType.Boolean, ReadOnly = true, ControlType = LibControlType.YesNo, DefaultValue = false });
            //用于同一级目录下的排序
            DataSourceHelper.AddColumn(new DefineField(dmDocument, "SORTORDER", "文档排序") { DataType = LibDataType.Int32, ControlType = LibControlType.Number });
            DataSourceHelper.AddFixColumn(dmDocument, BillType); 

            dmDocument.PrimaryKey = new DataColumn[] { dmDocument.Columns["DOCID"] };
            this.DataSet.Tables.Add(dmDocument);            
            #endregion

            #region 文档的权限
            //文档的权限。如果每个所有者有了针对具体文档的权限设置，该所有者必须还要有目录的浏览权限。如果不能浏览目录，即使设置了目录下文档的权限也看不到。
            DataTable dmDocPermission = new DataTable(dmDocPermissionTableName);           
            DataSourceHelper.AddColumn(new DefineField(dmDocPermission, "DOCID", "文档编号", FieldSize.Size20) { ControlType = LibControlType.Id, AllowEmpty = false, ReadOnly = true });
            DataSourceHelper.AddRowId(dmDocPermission);
            DataSourceHelper.AddRowNo(dmDocPermission);
            //权限拥有者类型，如部门、用户组、个人，如果此权限是属于部门的，则部门下的所有人都有相应的权限
            DataSourceHelper.AddColumn(new DefineField(dmDocPermission, "BELONGTYPE", "拥有者类型") { DataType = LibDataType.Int32, AllowEmpty = false, ControlType = LibControlType.TextOption, TextOption = new string[] { "部门", "用户组", "个人" } });
            //此项根据拥有者类型确定是部门代码，还是个人或用户组代码(后续扩展)
            DataSourceHelper.AddColumn(new DefineField(dmDocPermission, "BELONGID", "拥有者代码", FieldSize.Size20)
            {
                AllowEmpty = false,
                RelativeSource = new RelativeSourceCollection()
                {
                    new RelativeSource("com.Dept")
                    {
                         GroupCondation = "B.BELONGTYPE=0",
                         GroupIndex = 0,
                         RelFields = new RelFieldCollection()
                         {
                             new RelField("DEPTNAME", LibDataType.NText,FieldSize.Size50,"部门名称") //如果拥有者类型是部门的则代码是部门代码，名称显示部门名称
                         },
                    },
                    new RelativeSource("com.Person")
                    {
                         GroupCondation = "B.BELONGTYPE=2",
                         GroupIndex = 1,
                         RelFields = new RelFieldCollection()
                         {
                             new RelField("PERSONNAME", LibDataType.NText,FieldSize.Size50,"人员名称")//如果拥有者类型是个人的则代码是个人代码，名称显示人员名称
                         }
                    }
                }

            });
            //是否设置了操作权限
            DataSourceHelper.AddColumn(new DefineField(dmDocPermission, "ISOPERATEPOWER", "操作权限") { ReadOnly = true, DataType = LibDataType.Boolean, ControlType = LibControlType.YesNo, SubTableIndex = 2 });
            //权限值，是权限项组合的按位与 得出的Int32值。通过SubBill的页面获得
            DataSourceHelper.AddColumn(new DefineField(dmDocPermission, "OPERATEMARK", "操作权限标识") { DataType = LibDataType.Int32, ControlType = LibControlType.Number, ReadOnly = true });
            
            dmDocPermission.PrimaryKey = new DataColumn[] { dmDocPermission.Columns["DOCID"], dmDocPermission.Columns["ROW_ID"] };
            this.DataSet.Tables.Add(dmDocPermission);
            this.DataSet.Relations.Add(string.Format("{0}_{1}", dmDocTableName, dmDocPermissionTableName), new DataColumn[] { dmDocument.Columns["DOCID"] }, new DataColumn[] { dmDocPermission.Columns["DOCID"] });

            #endregion

            #region 操作权限SubBill
            DataTable subTable = new DataTable(dmSubTableName);
            DataSourceHelper.AddColumn(new DefineField(subTable, "DOCID", "文档代码", FieldSize.Size20) { AllowEmpty = false, AllowCopy = false });
            DataSourceHelper.AddRowId(subTable, "PARENTROWID", "父行标识");
            DataSourceHelper.AddRowId(subTable);
            DataSourceHelper.AddRowNo(subTable);
            DataSourceHelper.AddColumn(new DefineField(subTable, "OPERATEPOWERID", "操作代码") { DataType = LibDataType.Int32, ControlType = LibControlType.Number, ReadOnly = true });
            DataSourceHelper.AddColumn(new DefineField(subTable, "OPERATEPOWERNAME", "操作", FieldSize.Size50) { DataType = LibDataType.NText, ControlType = LibControlType.NText, ReadOnly = true });
            DataSourceHelper.AddColumn(new DefineField(subTable, "CANUSE", "具备权限") { DataType = LibDataType.Boolean, ControlType = LibControlType.YesNo });
            DataSourceHelper.AddRemark(subTable);
            subTable.PrimaryKey = new DataColumn[] { subTable.Columns["DOCID"], subTable.Columns["PARENTROWID"], subTable.Columns["ROW_ID"] };
            this.DataSet.Tables.Add(subTable);
            this.DataSet.Relations.Add(PermissionDetailSubRelationName,
                new DataColumn[] { dmDocPermission.Columns["DOCID"], dmDocPermission.Columns["ROW_ID"] },
                new DataColumn[] { subTable.Columns["DOCID"], subTable.Columns["PARENTROWID"] });
            #endregion

            #region 文档的版本历史
            //文档的版本历史
            DataTable dmDocVerHistory = new DataTable(dmDocVerHistoryTableName);           
            //关联到文档
            DataSourceHelper.AddColumn(new DefineField(dmDocVerHistory, "DOCID", "文档编号", FieldSize.Size50) { DataType = LibDataType.Text, AllowEmpty = false });
            DataSourceHelper.AddRowId(dmDocVerHistory);
            DataSourceHelper.AddRowNo(dmDocVerHistory);
            DataSourceHelper.AddColumn(new DefineField(dmDocVerHistory, "OPPERSONID", "操作人", FieldSize.Size20)
            {
                AllowEmpty = false,
                ReadOnly = true,
                RelativeSource = new RelativeSourceCollection()
                {
                    new  RelativeSource("com.PERSON")
                    {
                        RelFields = new RelFieldCollection()
                        {
                            new RelField("PERSONNAME", LibDataType.NText,FieldSize.Size50, "操作人"),
                        }
                    }
                }
            });
            //操作人操作时的IP地址,由后台设置
            DataSourceHelper.AddColumn(new DefineField(dmDocVerHistory, "IP", "IP", FieldSize.Size50) { DataType = LibDataType.NText,  ReadOnly = true });
            //修改版本的详情。记录从某版本修改为某版本，及修改说明。
            DataSourceHelper.AddColumn(new DefineField(dmDocVerHistory, "LOG", "日志", FieldSize.Size500) { DataType = LibDataType.NText });
            //设定版本时对应的修订版的修订号
            DataSourceHelper.AddColumn(new DefineField(dmDocVerHistory, "DOCMODIFYID", "修订号")
            {
                AllowEmpty = false,
                ReadOnly = true,
                DataType = LibDataType.Int32,
                ControlType = LibControlType.Number,
                DefaultValue=1,             
            });
            DataSourceHelper.AddColumn(new DefineField(dmDocVerHistory, "DATETIME", "时间") { DataType = LibDataType.Int64, ReadOnly = true, ControlType = LibControlType.DateTime });
            dmDocVerHistory.PrimaryKey = new DataColumn[] {  dmDocVerHistory.Columns["DOCID"], dmDocVerHistory.Columns["ROW_ID"] };
            this.DataSet.Tables.Add(dmDocVerHistory);
            this.DataSet.Relations.Add(string.Format("{0}_{1}", dmDocTableName, dmDocVerHistoryTableName), new DataColumn[] { dmDocument.Columns["DOCID"] }, new DataColumn[] { dmDocVerHistory.Columns["DOCID"] });
            #endregion

            #region 文档的修订版历史
            //文档的修订版历史
            DataTable dmDocModifyHistory = new DataTable(dmDocModifyHistoryTableName);            
            //关联到文档
            DataSourceHelper.AddColumn(new DefineField(dmDocModifyHistory, "DOCID", "文档编号", FieldSize.Size50) { DataType = LibDataType.Text, AllowEmpty = false, ReadOnly = true });
            //每编辑保存一次形成一个新的修订号。修订号就是修订版文件的名称（后台文件设置为没有后缀名）
            DataSourceHelper.AddRowId(dmDocModifyHistory, "DOCMODIFYID", "修订号");
            DataSourceHelper.AddRowNo(dmDocModifyHistory);
            DataSourceHelper.AddColumn(new DefineField(dmDocModifyHistory, "MODIFYPERSONID", "修订者编号", FieldSize.Size20)
            {
                AllowEmpty = false,
                ReadOnly = true,               
                RelativeSource = new RelativeSourceCollection()
                {
                    new  RelativeSource("com.PERSON")
                    {
                        RelFields = new RelFieldCollection()
                        {
                            new RelField("PERSONNAME", LibDataType.NText,FieldSize.Size50, "修改人"),
                        }
                    }
                }
            });
            //修改人操作时的IP地址,由后台设置
            DataSourceHelper.AddColumn(new DefineField(dmDocModifyHistory, "IP", "IP", FieldSize.Size50) { DataType = LibDataType.NText, ReadOnly = true });
            //修改说明
            DataSourceHelper.AddColumn(new DefineField(dmDocModifyHistory, "MODIFYDESC", "修改说明", FieldSize.Size500) { DataType = LibDataType.NText });
            DataSourceHelper.AddColumn(new DefineField(dmDocModifyHistory, "DATETIME", "修改时间") { DataType = LibDataType.Int64, ReadOnly = true, ControlType = LibControlType.DateTime });
            dmDocModifyHistory.PrimaryKey = new DataColumn[] { dmDocModifyHistory.Columns["DOCID"], dmDocModifyHistory.Columns["DOCMODIFYID"] };
            this.DataSet.Tables.Add(dmDocModifyHistory);
            this.DataSet.Relations.Add(string.Format("{0}_{1}", dmDocTableName, dmDocModifyHistoryTableName), new DataColumn[] { dmDocument.Columns["DOCID"] }, new DataColumn[] { dmDocModifyHistory.Columns["DOCID"] });
            #endregion

            #region 文档的操作历史(文档审计)
            //文档的操作历史(文档审计)
            DataTable dmDocOpLog = new DataTable(dmDocOpLogTableName);
            //关联到文档
            DataSourceHelper.AddColumn(new DefineField(dmDocOpLog, "DOCID", "文档编号", FieldSize.Size50) { DataType = LibDataType.Text, AllowEmpty = false, ReadOnly = true });
            DataSourceHelper.AddRowId(dmDocOpLog);
            DataSourceHelper.AddRowNo(dmDocOpLog);
            DataSourceHelper.AddColumn(new DefineField(dmDocOpLog, "OPPERSONID", "操作者编号", FieldSize.Size20)
            {
                AllowEmpty = false,
                ReadOnly = true,
                RelativeSource = new RelativeSourceCollection()
                {
                    new  RelativeSource("com.PERSON")
                    {
                        RelFields = new RelFieldCollection()
                        {
                            new RelField("PERSONNAME", LibDataType.NText,FieldSize.Size50, "操作人"),
                        }
                    }
                }
            });
            //操作人操作时的IP地址,由后台设置
            DataSourceHelper.AddColumn(new DefineField(dmDocOpLog, "IP", "IP", FieldSize.Size50) { DataType = LibDataType.NText,ReadOnly = true });
            //操作说明
            DataSourceHelper.AddColumn(new DefineField(dmDocOpLog, "OPDESC", "操作说明", FieldSize.Size500) { DataType = LibDataType.NText });
            DataSourceHelper.AddColumn(new DefineField(dmDocOpLog, "DATETIME", "操作时间") { DataType = LibDataType.Int64, ReadOnly = true, ControlType = LibControlType.DateTime });
            dmDocOpLog.PrimaryKey = new DataColumn[] { dmDocOpLog.Columns["DOCID"], dmDocOpLog.Columns["ROW_ID"] };
            this.DataSet.Tables.Add(dmDocOpLog);
            this.DataSet.Relations.Add(string.Format("{0}_{1}", dmDocTableName, dmDocOpLogTableName), new DataColumn[] { dmDocument.Columns["DOCID"] }, new DataColumn[] { dmDocOpLog.Columns["DOCID"] });
            #endregion

        }

        /// <summary>
        /// 定义前台表单样式
        /// </summary>
        /// <param name="dataSet"></param>
        protected override void DefineViewTemplate(DataSet dataSet)
        {
            LibBillLayout layout = new LibBillLayout(this.DataSet);

            layout.HeaderRange = layout.BuildControlGroup(0, "文档信息", new List<string> { "DOCID", "DOCNAME", "DOCTYPE", "DIRID", "DIRLINKADDRESS", "DOCVERSION", "DOCSIZE", "CLICKCOUNT", "LOCKSTATE", "ISFULLINDEX", "SORTORDER" });
           
            layout.TabRange.Add(layout.BuildGrid(1, "文档权限"));
            layout.SubBill.Add(2, layout.BuildGrid(2, "操作权限明细"));

            layout.TabRange.Add(layout.BuildGrid(3, "版本历史"));
            layout.TabRange.Add(layout.BuildGrid(4, "修订版管理"));
            layout.TabRange.Add(layout.BuildGrid(5, "文档审计"));
            
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
