using AxCRL.Template.DataSource;
using AxCRL.Template.Layout;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCRL.Template
{
    public class LibTemplate
    {
        /// <summary>
        /// 是否包含外接站点的相关数据表
        /// </summary>
        public static readonly bool HasAxpLinkSite = LibSqlModelCache.Default.Contains("axp.LinkSite");
        /// <summary>
        /// 是否包含同步数据配置及历史的数据表
        /// </summary>
        public static readonly bool HasSyncDataTable = LibSqlModelCache.Default.Contains("axp.SyncDataHistory") && LibSqlModelCache.Default.Contains("axp.SyncDataSetting");

        private BillType _BillType;
        private string _ProgId;
        private DataSet _DataSet = null;
        private LibViewTemplate _ViewTemplate = null;
        private LibFuncPermission _FuncPermission = null;
        private string _DisplayText;


        /// <summary>
        /// 功能显示名称
        /// </summary>
        public string DisplayText
        {
            get { return _DisplayText; }
            set { _DisplayText = value; }
        }

        public LibFuncPermission FuncPermission
        {
            get
            {
                if (_FuncPermission == null)
                    _FuncPermission = new LibFuncPermission(string.Empty, this.BillType);
                return _FuncPermission;
            }
            set { _FuncPermission = value; }
        }

        public string ProgId
        {
            get { return _ProgId; }
            set { _ProgId = value; }
        }

        public BillType BillType
        {
            get { return _BillType; }
            set { _BillType = value; }
        }

        public DataSet DataSet
        {
            get
            {
                return _DataSet;
            }
            protected set
            {
                _DataSet = value;
            }
        }

        public LibViewTemplate ViewTemplate
        {
            get
            {
                return _ViewTemplate;
            }
            protected set
            {
                _ViewTemplate = value;
            }
        }

        public LibTemplate(string progId, BillType billType, string displayText)
        {
            _BillType = billType;
            _ProgId = progId;
            _DisplayText = displayText;
            //先定义功能权限
            DefineFuncPermission();
            //再构建数据结构
            BuildDataSetStructure();
            foreach (DataTable table in this.DataSet.Tables)
            {
                table.ExtendedProperties[TableProperty.FieldAddrDic] = DataSourceHelper.FillFieldAddr(table);
            }
        }

        public LibViewTemplate GetViewTemplate(DataSet dataSet)
        {
            DefineViewTemplate(dataSet);
            if (BillType == Template.BillType.Bill || BillType == Template.BillType.Master)
            {
                //如果启用数据同步功能，则添加同步数据的子表视图
                if (dataSet != null && dataSet.Tables.Contains(LibFuncPermission.SynchroDataSettingTableName) && this.FuncPermission.UseSynchroData && LibTemplate.HasAxpLinkSite)
                {
                    LibBillLayout layout = this.ViewTemplate.Layout as LibBillLayout;
                    if (layout != null)
                    {
                        layout.DataSet = dataSet;//设置新的数据集模型
                        int index = 0;
                        for (index = 0; index < dataSet.Tables.Count; index++)
                        {
                            if (dataSet.Tables[index].TableName.Equals(LibFuncPermission.SynchroDataSettingTableName))
                                break;
                        }
                        LibGridLayoutBlock grid = layout.BuildGrid(index, "同步配置", new string[] {"ISSYNCTO", "SITEID", "SHORTNAME"});
                        grid.IsCanNotEditRow = true;//用户不可新增或删除行
                        layout.TabRange.Add(grid);
                        grid = layout.BuildGrid(index + 1, "同步历史", new string[] { "USERID", "PERSONNAME", "SITEID", "SHORTNAME", "SYNCTIME", "SYNCOP", "SYNCSTATE", "SYNCINFO" });
                        grid.IsCanNotEditRow = true;//用户不可新增或删除行
                        layout.TabRange.Add(grid);
                    }
                }
            }
            return this.ViewTemplate;
        }

        protected virtual void DefineViewTemplate(DataSet dataSet)
        {

        }

        protected void BuildDataSetStructure()
        {
            BuildDataSet();
            if (BillType == Template.BillType.Bill || BillType == Template.BillType.Master)
            {
                DataTable masterTable = this.DataSet.Tables[0];
                DataSourceHelper.AddAttachmentSrcColumn(masterTable);
                if (!masterTable.ExtendedProperties.ContainsKey(TableProperty.DBIndex))
                    masterTable.ExtendedProperties.Add(TableProperty.DBIndex, new DBIndexCollection());
                DBIndexCollection dbIndexs = (DBIndexCollection)masterTable.ExtendedProperties[TableProperty.DBIndex];
                dbIndexs.Add(new DBIndex(string.Format("{0}_ID_IDX", masterTable.TableName.ToUpper()), new DBIndexFieldCollection() { new DBIndexField("INTERNALID") 
                }, true));
                for (int i = 1; i < this.DataSet.Tables.Count; i++)
                {
                    DataTable table = this.DataSet.Tables[i];
                    if (table.ExtendedProperties.ContainsKey(TableProperty.UsingAttachment))
                    {
                        if (Convert.ToBoolean(table.ExtendedProperties[TableProperty.UsingAttachment]))
                            DataSourceHelper.AddAttachmentSrcColumn(table);
                    }
                    // 自动构建行项审核需要的数据列
                    if (table.ExtendedProperties.ContainsKey(TableProperty.UsingApproveRow))
                    {
                        if (Convert.ToBoolean(table.ExtendedProperties[TableProperty.UsingApproveRow]))
                            DataSourceHelper.AddApproveRowFixColumn(table);
                    }
                }
                try
                {
                    if (this.FuncPermission.UseSynchroData)
                    {
                        //如果启用数据同步功能，则添加同步到的目标站点的虚拟子表
                        if (this.DataSet.Tables.Contains(LibFuncPermission.SynchroDataSettingTableName) == false && LibTemplate.HasAxpLinkSite && LibTemplate.HasSyncDataTable)
                        {
                            DataTable dt = DataSourceHelper.AddSyncDataSettingTable(this.DataSet, LibFuncPermission.SynchroDataSettingTableName);
                            dt.ExtendedProperties.Add(TableProperty.IsVirtual, true);//设定同步配置数据表为虚表
                            dt.ExtendedProperties.Add(TableProperty.AllowCopy, false);//设定同步配置数据表不可复制
                            dt = DataSourceHelper.AddSyncDataHistoryTable(this.DataSet, LibFuncPermission.SynchroDataHisTableName);
                            dt.ExtendedProperties.Add(TableProperty.IsVirtual, true);//设定同步历史数据表为虚表
                            dt.ExtendedProperties.Add(TableProperty.AllowCopy, false);//设定同步历史数据表不可复制
                        }                            
                    }
                }
                catch { }

            }
        }

        protected virtual void BuildDataSet()
        {

        }

        protected virtual void DefineFuncPermission()
        {

        }
    }

    public enum BillType
    {
        Master = 0,
        Bill = 1,
        Grid = 2,
        DataFunc = 3,
        Rpt = 4,
        DailyRpt = 5
    }
    /// <summary>
    /// 模块数据同步配置
    /// </summary>
    public class BcfSyncConfig
    {
        private List<string> _NonSyncSubTables = new List<string>();
        /// <summary>
        /// 不需要同步的子表或子子表的表名称。
        /// </summary>
        public List<string> NonSyncSubTables
        {
            get
            {
                if (_NonSyncSubTables == null)
                    _NonSyncSubTables = new List<string>();
                return _NonSyncSubTables;
            }
            set
            {
                _NonSyncSubTables = value;
            }
        }
        private Dictionary<string, List<string>> _NonSyncFields = new Dictionary<string, List<string>>();
        /// <summary>
        /// 不需要同步的字段字典。
        /// Key是数据表名称,Value是数据表下不需要同步的字段列表。
        /// 主键字段和非空字段必须同步，如果设置了不同步主键字段则忽略该设置。
        /// </summary>
        public Dictionary<string, List<string>> NonSyncFields
        {
            get {
                if (_NonSyncFields == null)
                    _NonSyncFields = new Dictionary<string, List<string>>();
                return _NonSyncFields;
            }
            set
            {
                _NonSyncFields = value;
            }
        }
    }
    /// <summary>
    /// 功能权限
    /// </summary>
    public class LibFuncPermission
    {
        private string _ConfigPack;
        private bool _CanMenu = true;
        private int _Permission = 0;
        private string _KeyCode;
        private bool _UsingCache = false;
        private string _BillTypeName;
        private bool _UsingDynamicColumn = false;
        private bool _UsingApproveRow = false;
        private IList<string> _EntryParam = null;
        private string _ProgTag;
        
        /// <summary>
        /// 清单页的分类树配置
        /// </summary>
        public TreeListingConfig TreeListing { get; set; }
        /// <summary>
        /// 同步数据配置的虚拟子表的表名称
        /// </summary>
        public const string SynchroDataSettingTableName = "SynchroDataSetting";
        /// <summary>
        /// 同步数据历史的虚拟子表的表名称
        /// </summary>
        public const string SynchroDataHisTableName = "SynchroDataHistory";
        /// <summary>
        /// 是否启用同步数据到其他站点
        /// </summary>
        public bool UseSynchroData { get; set; }
        /// <summary>
        /// 功能模块的数据同步配置。未启用同步时此属性无意义。
        /// </summary>
        public BcfSyncConfig SyncConfig { get; set; }


        public const string KPIChartTag = "KPIChart";

        public LibFuncPermission()
        {
            //默认为主数据的权限
            this._Permission = this._Permission = GetPermission(new FuncPermissionEnum[] {   
                        FuncPermissionEnum.Use, 
                        FuncPermissionEnum.Browse,
                        FuncPermissionEnum.Add,
                        FuncPermissionEnum.Edit, 
                        FuncPermissionEnum.Delete, 
                        FuncPermissionEnum.Audit, 
                        FuncPermissionEnum.CancelAudit,
                        FuncPermissionEnum.Import, 
                        FuncPermissionEnum.Export, 
                        FuncPermissionEnum.Print});
        }

        private int GetPermission(FuncPermissionEnum[] usePermission)
        {
            int ret = 0;
            foreach (FuncPermissionEnum item in usePermission)
            {
                ret += (int)item;
            }
            return ret;
        }

        public LibFuncPermission(string keyCode, FuncPermissionEnum[] usePermission, bool canMenu = true, string configPack = "", bool usingCache = false)
        {
            this._KeyCode = keyCode;
            this._CanMenu = canMenu;
            this._ConfigPack = configPack;
            this._UsingCache = usingCache;
            this._Permission = GetPermission(usePermission);
        }

        public LibFuncPermission(string keyCode, BillType billType, bool canMenu = true, string configPack = "", bool usingCache = false)
        {
            this._KeyCode = keyCode;
            this._CanMenu = canMenu;
            this._ConfigPack = configPack;
            this._UsingCache = usingCache;
            //Bill 65219 Master 57759 dataFunc 57345 Grid 57375 Rpt 49155
            switch (billType)
            {
                case BillType.Master:
                    this._Permission = GetPermission(new FuncPermissionEnum[] {   
                        FuncPermissionEnum.Use, 
                        FuncPermissionEnum.Browse,
                        FuncPermissionEnum.Add,
                        FuncPermissionEnum.Edit, 
                        FuncPermissionEnum.Delete, 
                        FuncPermissionEnum.Audit, 
                        FuncPermissionEnum.CancelAudit,
                        FuncPermissionEnum.Import, 
                        FuncPermissionEnum.Export, 
                        FuncPermissionEnum.Print});
                    break;
                case BillType.Bill:
                    this._Permission = GetPermission(new FuncPermissionEnum[] {   
                        FuncPermissionEnum.Use, 
                        FuncPermissionEnum.Browse,
                        FuncPermissionEnum.Add,
                        FuncPermissionEnum.Edit, 
                        FuncPermissionEnum.Delete, 
                        FuncPermissionEnum.Release, 
                        FuncPermissionEnum.CancelRelease, 
                        FuncPermissionEnum.Audit, 
                        FuncPermissionEnum.CancelAudit,
                        FuncPermissionEnum.EndCase, 
                        FuncPermissionEnum.CancelEndCase,
                        FuncPermissionEnum.Invalid,
                        FuncPermissionEnum.CancelInvalid,
                        FuncPermissionEnum.Import, 
                        FuncPermissionEnum.Export, 
                        FuncPermissionEnum.Print});
                    break;
                case BillType.Grid:
                    this._Permission = GetPermission(new FuncPermissionEnum[] {   
                        FuncPermissionEnum.Use, 
                        FuncPermissionEnum.Browse,
                        FuncPermissionEnum.Add,
                        FuncPermissionEnum.Edit, 
                        FuncPermissionEnum.Delete, 
                        FuncPermissionEnum.Import, 
                        FuncPermissionEnum.Export, 
                        FuncPermissionEnum.Print});
                    break;
                case BillType.DataFunc:
                    this._Permission = GetPermission(new FuncPermissionEnum[] {   
                        FuncPermissionEnum.Use, 
                        FuncPermissionEnum.Import, 
                        FuncPermissionEnum.Export, 
                        FuncPermissionEnum.Print});
                    break;
                case BillType.Rpt:
                case BillType.DailyRpt:
                    this._Permission = GetPermission(new FuncPermissionEnum[] {
                        FuncPermissionEnum.Use, 
                        FuncPermissionEnum.Browse,
                        FuncPermissionEnum.Export, 
                        FuncPermissionEnum.Print});
                    break;
            }
        }
        /// <summary>
        /// 功能标签
        /// </summary>
        public string ProgTag
        {
            get { return _ProgTag; }
            set { _ProgTag = value; }
        }
        /// <summary>
        /// 是否启用行项审核
        /// </summary>
        public bool UsingApproveRow
        {
            get { return _UsingApproveRow; }
            set { _UsingApproveRow = value; }
        }
        /// <summary>
        /// 入口参数
        /// </summary>
        public IList<string> EntryParam
        {
            get
            {
                if (_EntryParam == null)
                    _EntryParam = new List<string>();
                return _EntryParam;
            }
            set { _EntryParam = value; }
        }
        /// <summary>
        /// 单据对应的单据类型功能名
        /// </summary>
        public string BillTypeName
        {
            get { return _BillTypeName; }
            set { _BillTypeName = value; }
        }
        /// <summary>
        /// 权限值
        /// </summary>
        public int Permission
        {
            get { return _Permission; }
            set { _Permission = value; }
        }
        /// <summary>
        /// 使用缓存
        /// </summary>
        public bool UsingCache
        {
            get { return _UsingCache; }
            set { _UsingCache = value; }
        }
        /// <summary>
        /// 是否能挂菜单
        /// </summary>
        public bool CanMenu
        {
            get { return _CanMenu; }
            set { _CanMenu = value; }
        }

        //配置包
        public string ConfigPack
        {
            get { return _ConfigPack; }
            set { _ConfigPack = value; }
        }

        /// <summary>
        /// 快捷代码
        /// </summary>
        public string KeyCode
        {
            get { return _KeyCode; }
            set { _KeyCode = value; }
        }
        /// <summary>
        /// 使用动态列
        /// </summary>
        public bool UsingDynamicColumn
        {
            get { return _UsingDynamicColumn; }
            set { _UsingDynamicColumn = value; }
        }

    }

    /// <summary>
    /// 权限
    /// </summary>
    public enum FuncPermissionEnum
    {
        Use = 1,
        Browse = 2,
        Add = 4,
        Edit = 8,
        Delete = 16,
        Release = 32,
        CancelRelease = 64,
        Audit = 128,
        CancelAudit = 256,
        EndCase = 512,
        CancelEndCase = 1024,
        Invalid = 2048,
        CancelInvalid = 4096,
        Import = 8192,
        Export = 16384,
        Print = 32768
    }

}
