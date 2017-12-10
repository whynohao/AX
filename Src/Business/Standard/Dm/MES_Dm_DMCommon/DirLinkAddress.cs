/**********************************************************************
 * CopyRight 2016 杭州集控科技有限公司 版权所有
 * 功能描述：文档管理模块的目录链接处理类，完成关于目录链接、子目录列表、子目录下的文档等处理
 * 创建标识：Zhangkj 2016/12/12
 * 
************************************************************************/
using AxCRL.Comm.Runtime;
using AxCRL.Comm.Utils;
using AxCRL.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jikon.MES_Dm.DMCommon
{
    /// <summary>
    /// 目录链接类
    /// 用于前后台交换数据、根据指定目录标识构造目录链接，形成类似“\公共文档\财务部文档"”的链接。
    /// 还用于生成目录在磁盘上的实际文件夹相对路径信息等
    /// </summary>
    public class DirLinkAddress
    {
        /// <summary>
        /// 是否是为目录生成的目录链接
        /// </summary>
        private bool isForDir = true;

        private LibDataAccess _DataAccess = null;
        private string _DirID = string.Empty;
        private DirTypeEnum _DirType = DirTypeEnum.Public;
        private string _DocID = string.Empty;
        private string _DocDirName = string.Empty;
        private int _DocMaxModifyVerId = 1;
        private string _DocType = string.Empty;
        /// <summary>
        /// 文件名，不包含后缀名
        /// </summary>
        private string _DocName = string.Empty;
        /// <summary>
        /// 文档是否被锁定了
        /// </summary>
        private bool _IsDocLocked = false;

        private DataTable _DirLinkTable = new DataTable();
        private string _DirSavePath = string.Empty;
        private string _DirNameLink = string.Empty;
        private string _DirIdLink = string.Empty;
        private List<string> _SubDirIdList = new List<string>();
        private List<string> _ParentDirIdList = new List<string>();
        private List<string> _DocIdList = new List<string>();


        /// <summary>
        /// 获取或设置目录ID
        /// </summary>
        public string DirID
        {
            get { return this._DirID; }
            set { this._DirID = value; }
        }
        /// <summary>
        /// 获取或设置文档ID
        /// </summary>
        public string DocID
        {
            get { return this._DocID; }
            set { this._DocID = value; }
        }
        /// <summary>
        /// 获取目录类型
        /// </summary>
        public DirTypeEnum DirType
        {
            get { return this._DirType; }
        }
        /// <summary>
        /// 获取文档所在的文件夹名称
        /// </summary>
        public string DocDirName
        {
            get { return this._DocDirName; }
        }
        /// <summary>
        /// 获取文档类型，类似".doc"
        /// </summary>
        public string DocType
        {
            get { return this._DocType; }
        }
        /// <summary>
        /// 获取文档名称，包含后缀名，类似“示例.docx”
        /// </summary>
        public string DocName
        {
            get { return this._DocName + this._DocType; }
        }
        /// <summary>
        /// 文档是否已经被锁定
        /// </summary>
        public bool IsDocLocked
        {
            get { return this._IsDocLocked; }
        }
        /// <summary>
        /// 文档的最大(最新)修订版的修订号
        /// </summary>
        public int DocMaxModifyVerId
        {
            get { return _DocMaxModifyVerId; }
        }
        /// <summary>
        /// 获取文档所在的文件夹在磁盘上的绝对路径
        /// </summary>
        public string DocDirFullPath
        {
            get
            {
                return Path.Combine(DMCommonMethod.GetDMRootPath(this._DirType), DirSavePath, DocDirName);
            }
        }
        /// <summary>
        /// 获取数据库访问器
        /// </summary>
        public LibDataAccess DataAccess
        {
            get { return this._DataAccess; }
        }
        /// <summary>
        /// 获取目录链接数据表，从本级目录开始到顶级目录。第一行为本级目录（DirID标识的目录），最后一行为顶级目录(父目录为空)
        /// </summary>
        public DataTable DirLinkTable
        {
            get { return this._DirLinkTable; }
        }
        /// <summary>
        /// 获取目录在磁盘上的实际文件夹相对路径信息等
        /// </summary>
        public string DirSavePath
        {
            get { return this._DirSavePath; }
        }
        /// <summary>
        /// 获取目录在磁盘上的实际文件夹的绝对路径
        /// </summary>
        public string DirFullSavePath
        {
            get
            {
                return Path.Combine(DMCommonMethod.GetDMRootPath(this._DirType), DirSavePath);
            }
        }
        /// <summary>
        /// 获取目录链接信息，类似“\公共文档\财务部文档”的字符串。
        /// </summary>
        public string DirNameLink
        {
            get { return this._DirNameLink; }
        }
        /// <summary>
        /// 获取目录的标识号链接，从根目录开始，以/分割。
        /// </summary>
        public string DirIdLink
        {
            get { return this._DirIdLink; }
        }
        /// <summary>
        /// 获取本目录Id下的子目录Id列表
        /// 返回时会重新构造一个List列表
        /// </summary>
        public List<string> SubDirIdList
        {
            get { return this._SubDirIdList.ToList(); }
        }
        /// <summary>
        /// 获取本目录或本文档的上层目录Id列表
        /// </summary>
        public List<string> ParentDirIdList
        {
            get { return this._ParentDirIdList; }
        }
        /// <summary>
        /// 获取本目录及各级子目录下的文档Id列表
        /// 需要调用获取文档GetDocIds才能获取
        /// 返回时会重新构造一个List列表
        /// </summary>
        public List<string> DocIdList
        {
            get { return this._DocIdList.ToList(); }
        }
        /// <summary>
        /// 目录链接构造函数
        /// </summary>
        /// <param name="forDir">是为目录创建还是为文档创建</param>
        /// <param name="itemId">目录或文档标识号</param>
        /// <param name="dbAccess">数据库访问器</param>
        /// <param name="isAutoCreateDirLink">是否自动创建目录链接</param>
        public DirLinkAddress(bool forDir, string itemId, LibDataAccess dbAccess, bool isAutoCreateDirLink)
        {
            if (dbAccess == null)
                throw new Exception("数据库访问器为null!");
            this._DataAccess = dbAccess;

            if (string.IsNullOrEmpty(itemId))
                throw new Exception("标识号为空!");
            if (forDir)
                this._DirID = itemId;
            else
            {
                _DocID = itemId;
                //获得文档所在的目录Id，文档所在的文件夹名称，文档对应的最大修订号
                _DirID = GetDirId(_DocID, out _DocDirName, out _DocMaxModifyVerId, out _DocType, out _DocName, out _IsDocLocked);
            }

            isForDir = forDir;

            if (isAutoCreateDirLink)
                CreateDirLink();
        }
        /// <summary>
        /// 默认为目录创建目录链接对象并自动执行CreateDirLink方法
        /// </summary>
        /// <param name="dirId"></param>
        /// <param name="dbAccess"></param>
        public DirLinkAddress(string dirId, LibDataAccess dbAccess)
            : this(true, dirId, dbAccess, true)
        {
        }
        /// <summary>
        /// 默认为文档创建目录链接对象并自动执行CreateDirLink方法
        /// 自动创建新的数据库访问器
        /// </summary>
        /// <param name="dirId"></param>
        /// <param name="dbAccess"></param>
        public DirLinkAddress(string docId)
            : this(false, docId, new LibDataAccess(), true)
        {
        }
        /// <summary>
        /// 获取文档所在的目录Id和文档的文件夹名称（仅文档自身的文件夹名称）
        /// </summary>
        /// <param name="docId"></param>
        /// <param name="docDirName">文档的文件夹名称,仅文档自身的文件夹名称</param>
        /// <param name="maxModifyVerId">最大修订号</param>
        /// <returns></returns>
        protected string GetDirId(string docId, out string docDirName, out int maxModifyVerId, out string docType, out string docName, out bool isDocLocked)
        {
            string dirId = string.Empty;
            docDirName = string.Empty;
            maxModifyVerId = 1;
            docType = string.Empty;
            docName = string.Empty;
            isDocLocked = true;

            string sql = string.Format("select A.DIRID,A.SAVEPATH,A.DOCTYPE,A.DOCNAME,A.LOCKSTATE,(select max(DOCMODIFYID) from DMDOCMODIFYHISTORY where DOCID='{0}') as MaxDOCMODIFYID from DMDOCUMENT A where A.DOCID = '{0}'", docId);
            DataSet ds = this.DataAccess.ExecuteDataSet(sql);
            if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                return string.Empty;
            DataRow row = ds.Tables[0].Rows[0];
            dirId = LibSysUtils.ToString(row["DIRID"]);
            docDirName = LibSysUtils.ToString(row["SAVEPATH"]);
            maxModifyVerId = LibSysUtils.ToInt32(row["MaxDOCMODIFYID"]);
            docType = LibSysUtils.ToString(row["DOCTYPE"]);
            docName = LibSysUtils.ToString(row["DOCNAME"]);
            docName = docName.EndsWith(docType) ? docName.Substring(0, docName.Length - docType.Length) : docName;

            int lockState = LibSysUtils.ToInt32(row["LOCKSTATE"]);
            isDocLocked = lockState == 1;

            return dirId;
        }
        /// <summary>
        /// 根据目录标识，构造目录链接
        /// </summary>
        public void CreateDirLink()
        {
            //构造递归查询语句。
            //从本级目录开始到顶级目录。第一行为本级目录（DirID标识的目录），最后一行为顶级目录(父目录为空)
            string sqlFindParent = "";
            string tempTableName = string.Format("{0}_{1}", "temp", DateTime.Now.Ticks);
            if (this.DataAccess.DatabaseType == LibDatabaseType.SqlServer)
                sqlFindParent = " with " + tempTableName + " as  " +
                      "   ( " +
                      "   select a.DIRID,a.DIRNAME,a.PARENTDIRID,a.DIRPATH,A.DIRTYPE  from DMDIRECTORY a where DIRID = '" + DirID + "' " +
                      "   union all " +
                      "   select k.DIRID,k.DIRNAME,k.PARENTDIRID,k.DIRPATH,k.DIRTYPE from DMDIRECTORY k inner " +
                      "                               join " + tempTableName + " t on t.PARENTDIRID = k.DIRID " +
                      "   ) select * from " + tempTableName;
            else
            {
                //Oracle的递归查询待测试
                sqlFindParent = "select DIRID,DIRNAME,PARENTDIRID,DIRPATH,DIRTYPE " +
                      " from DMDIRECTORY " +
                      " START WITH DIRID='" + DirID + "' " +
                      " CONNECT BY DIRID = PRIOR PARENTDIRID ";
            }

            _DirSavePath = string.Empty;
            _DirNameLink = string.Empty;
            _DirIdLink = string.Empty;
            _DirLinkTable = new DataTable();
            _ParentDirIdList.Clear();

            DataSet ds = this.DataAccess.ExecuteDataSet(sqlFindParent);
            if (ds != null && ds.Tables.Count > 0)
                _DirLinkTable = ds.Tables[0];
            if (_DirLinkTable != null && _DirLinkTable.Rows.Count > 0)
            {
                DataRow row = null;
                //倒序，从根目录开始
                for (int i = _DirLinkTable.Rows.Count - 1; i >= 0; i--)
                {
                    row = _DirLinkTable.Rows[i];
                    _DirSavePath = Path.Combine(_DirSavePath, LibSysUtils.ToString(row["DIRPATH"]));
                    _DirNameLink = string.Format("{0}/{1}", _DirNameLink, LibSysUtils.ToString(row["DIRNAME"]));
                    _DirIdLink = string.Format("{0}/{1}", _DirIdLink, LibSysUtils.ToString(row["DIRID"]));
                    if (isForDir == false || i != 0)
                    {
                        //对于目录来说 只添加父目录（i==0),对于文档来说目录都要添加
                        _ParentDirIdList.Add(LibSysUtils.ToString(row["DIRID"]));
                    }
                    if (i == 0)
                    {
                        //最底层的目录,设置目录类型
                        _DirType = (DirTypeEnum)LibSysUtils.ToInt32(row["DIRTYPE"]);
                        _DirNameLink = string.Format("/{0}{1}", (_DirType == DirTypeEnum.Public) ? "公共文档" : "个人文档", _DirNameLink);
                    }
                }
            }

            GetSubDirIds();
        }
        /// <summary>
        /// 获取子目录标识号列表
        /// </summary>
        public void GetSubDirIds()
        {
            //从本级目录开始到最底级目录。第一行为本级目录（DirID标识的目录），下面的是按层级的子级目录
            string sqlFindSub = "";
            string tempTableName = string.Format("{0}_{1}", "temp", DateTime.Now.Ticks);
            if (this.DataAccess.DatabaseType == LibDatabaseType.SqlServer)
                sqlFindSub = " with " + tempTableName + " as  " +
                      "   ( " +
                      "   select a.DIRID,a.DIRNAME,a.PARENTDIRID,a.DIRPATH  from DMDIRECTORY a where DIRID = '" + DirID + "' " +
                      "   union all " +
                      "   select k.DIRID,k.DIRNAME,k.PARENTDIRID,k.DIRPATH from DMDIRECTORY k inner " +
                      "                               join " + tempTableName + " t on t.DIRID = k.PARENTDIRID " +
                      "   ) select * from  " + tempTableName;
            else
            {
                //Oracle的递归查询待测试
                sqlFindSub = "select DIRID,DIRNAME,PARENTDIRID,DIRPATH " +
                      " from DMDIRECTORY " +
                      " START WITH DIRID='" + DirID + "' " +
                      " CONNECT BY PRIOR DIRID =  PARENTDIRID ";
            }
            DataTable subDirDt = null;
            _SubDirIdList.Clear();
            DataSet ds2 = this.DataAccess.ExecuteDataSet(sqlFindSub);
            if (ds2 != null && ds2.Tables.Count > 0)
                subDirDt = ds2.Tables[0];
            if (subDirDt != null && subDirDt.Rows.Count > 1)
            {
                DataRow row = null;
                //正序，从当前目录的第一个子级目录开始
                for (int i = 1; i < subDirDt.Rows.Count; i++)
                {
                    row = subDirDt.Rows[i];
                    _SubDirIdList.Add(LibSysUtils.ToString(row["DIRID"]));
                }
            }
        }
        /// <summary>
        /// 获取目录下的文档及子目录下的文档Id列表。
        /// </summary>
        public void GetDocIds()
        {
            List<string> dirIds = this._SubDirIdList.ToList();
            dirIds.Add(this.DirID);
            if (dirIds.Count == 0)
                return;
            this._DocIdList.Clear();
            List<string> list = GetDocIdsOfDirList(dirIds);
            if (list != null)
                this._DocIdList = list;
        }
        /// <summary>
        /// 获取指定修订版的文档绝对路径
        /// </summary>
        /// <param name="modifyVerionId">小于等于0则表示取最新（最大）的修订版</param>
        /// <returns></returns>
        public string GetDocFullPath(int modifyVerionId)
        {
            if (modifyVerionId <= 0)
                modifyVerionId = DocMaxModifyVerId;
            //不同修订版的文档在磁盘上是直接以修订号作为名称的
            return Path.Combine(DocDirFullPath, modifyVerionId.ToString() + _DocType);
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
        /// 为文档编号列表，获取各个文档对应的所属目录列表（从顶级目录到最终目录）
        /// </summary>
        /// <param name="docIds">文档编号列表</param>
        /// <returns></returns>
        public static Dictionary<string, List<DocInfo>> GetParentDirIdsForDocs(List<string> docIds)
        {
            if (docIds == null || docIds.Count == 0)
                return null;

            List<string> newList = (from item in docIds
                                    select string.Format("'{0}'", item)).ToList();
            string docIdsQueryStr = string.Join(",", newList);

            Dictionary<string, List<DocInfo>> dicDocId_DirIds = new Dictionary<string, List<DocInfo>>();
            //构造递归查询语句。
            //从本级目录开始到顶级目录。第一行为本级目录（DirID标识的目录），最后一行为顶级目录(父目录为空)
            string sqlFindParent = "";
            string tempTableName = string.Format("{0}_{1}", "temp", DateTime.Now.Ticks);
            LibDataAccess dataAccess = new LibDataAccess();
            if (dataAccess.DatabaseType == LibDatabaseType.SqlServer)
                sqlFindParent = " with " + tempTableName + " as  " +
                      "   ( " +
                      "   select b.DOCID,a.DIRID,a.DIRNAME,a.PARENTDIRID,a.DIRPATH, a.DIRTYPE, b.CREATORID ,0 as DORDER  from DMDIRECTORY a  " +
                      "   left join DMDOCUMENT b on a.DIRID = b.DIRID where b.DOCID in  (" + docIdsQueryStr + ") " +
                      "   union all " +
                      "   select t.DOCID,k.DIRID,k.DIRNAME,k.PARENTDIRID,k.DIRPATH, k.DIRTYPE, t.CREATORID ,(t.DORDER+1 ) as DORDER from DMDIRECTORY k inner " +
                      "                               join " + tempTableName + " t on t.PARENTDIRID = k.DIRID " +
                      "   ) select * from " + tempTableName +
                      "   order by DOCID,DORDER asc";
            else
            {
                //Oracle的递归查询待测试
                //To DO
                sqlFindParent = "select b.DOCID,a.DIRID,a.DIRNAME,a.PARENTDIRID,a.DIRPATH, a.DIRTYPE, b.CREATORID from (select CONNECT_BY_ROOT(DIRID) as ROOTDIRID,DIRID,DIRNAME,PARENTDIRID,DIRPATH,DIRTYPE " +
                      " from DMDIRECTORY START WITH DIRID in (select distinct DIRID from DMDOCUMENT where DOCID in (" + docIdsQueryStr + ")) " +
                      " CONNECT BY PRIOR PARENTDIRID = DIRID) a left join DMDOCUMENT b on a.ROOTDIRID = b.DIRID order by b.DOCID";
            }
            if (string.IsNullOrEmpty(sqlFindParent))
                return null;
            DataSet ds = dataAccess.ExecuteDataSet(sqlFindParent);
            DataTable dt = new DataTable();
            if (ds != null && ds.Tables.Count > 0)
                dt = ds.Tables[0];
            if (dt != null && dt.Rows.Count > 0)
            {
                DataRow row = null;
                //倒序，从根目录开始
                string docId = string.Empty;
                string dirId = string.Empty;
                DirTypeEnum dirType = DirTypeEnum.Public;
                string creatorId = string.Empty;
                for (int i = dt.Rows.Count - 1; i >= 0; i--)
                {
                    row = dt.Rows[i];
                    docId = LibSysUtils.ToString(row["DOCID"]);
                    dirId = LibSysUtils.ToString(row["DIRID"]);
                    dirType = (DirTypeEnum)LibSysUtils.ToInt32(row["DIRTYPE"]);
                    creatorId = LibSysUtils.ToString(row["CREATORID"]);
                    DocInfo temp = new DocInfo()
                    {
                        DocId = docId,
                        DirType = dirType,
                        CreatorId = creatorId,
                        DirId = dirId
                    };
                    if (dicDocId_DirIds.ContainsKey(docId) == false)
                        dicDocId_DirIds.Add(docId, new List<DocInfo>() { temp });
                    else
                        dicDocId_DirIds[docId].Add(temp);
                }
            }
            return dicDocId_DirIds;
        }
        /// <summary>
        /// 为目录编号列表，获取各个目录对应的所属目录列表（从顶级目录到自身目录）
        /// </summary>
        /// <param name="dirIds">目录编号列表</param>
        /// <returns></returns>
        public static Dictionary<string, List<string>> GetDirIdsForDirs(List<string> dirIds)
        {
            if (dirIds == null || dirIds.Count == 0)
                return null;

            List<string> newList = (from item in dirIds
                                    select string.Format("'{0}'", item)).ToList();
            string dirIdsQueryStr = string.Join(",", newList);

            Dictionary<string, List<string>> dicDirId_DirIds = new Dictionary<string, List<string>>();
            //构造递归查询语句。
            //从本级目录开始到顶级目录。第一行为本级目录（DirID标识的目录），最后一行为顶级目录(父目录为空)
            string sqlFindParent = "";
            string tempTableName = string.Format("{0}_{1}", "temp", DateTime.Now.Ticks);
            LibDataAccess dataAccess = new LibDataAccess();
            if (dataAccess.DatabaseType == LibDatabaseType.SqlServer)
                sqlFindParent = " with " + tempTableName + " as  " +
                      "   ( " +
                      "   select a.DIRID as SEARCHDIRID,a.DIRID,a.DIRNAME,a.PARENTDIRID,a.DIRPATH ,0 as DORDER from DMDIRECTORY a  " +
                      "   where a.DIRID in  (" + dirIdsQueryStr + ") " +
                      "   union all " +
                      "   select t.SEARCHDIRID,k.DIRID,k.DIRNAME,k.PARENTDIRID,k.DIRPATH,(t.DORDER+1 ) as DORDER from DMDIRECTORY k inner " +
                      "                               join " + tempTableName + " t on t.PARENTDIRID = k.DIRID " +
                      "   ) select * from " + tempTableName +
                      "   order by SEARCHDIRID,DORDER asc";
            else
            {
                //Oracle的递归查询待测试
                //To DO
                sqlFindParent = "select CONNECT_BY_ROOT(DIRID) as SEARCHDIRID,DIRID,DIRNAME,PARENTDIRID,DIRPATH " +
                      " from DMDIRECTORY START WITH DIRID in ("+ dirIdsQueryStr + ") " +
                      " CONNECT BY PRIOR PARENTDIRID = DIRID order by SEARCHDIRID";
            }
            if (string.IsNullOrEmpty(sqlFindParent))
                return null;
            DataSet ds = dataAccess.ExecuteDataSet(sqlFindParent);
            DataTable dt = new DataTable();
            if (ds != null && ds.Tables.Count > 0)
                dt = ds.Tables[0];
            if (dt != null && dt.Rows.Count > 0)
            {
                DataRow row = null;
                //倒序，从根目录开始
                string searchDirId = string.Empty;
                for (int i = dt.Rows.Count - 1; i >= 0; i--)
                {
                    row = dt.Rows[i];
                    searchDirId = LibSysUtils.ToString(row["SEARCHDIRID"]);
                    if (dicDirId_DirIds.ContainsKey(searchDirId) == false)
                        dicDirId_DirIds.Add(searchDirId, new List<string>() { LibSysUtils.ToString(row["DIRID"]) });
                    else
                        dicDirId_DirIds[searchDirId].Add(LibSysUtils.ToString(row["DIRID"]));
                }
            }
            return dicDirId_DirIds;
        }
    }

    public class DocInfo
    {
        public string DirId
        {
            set;
            get;
        }

        public string DocId
        {
            set;
            get;
        }

        public DirTypeEnum DirType
        {
            set;
            get;
        }

        public string CreatorId
        {
            set;
            get;
        }
    }
}
