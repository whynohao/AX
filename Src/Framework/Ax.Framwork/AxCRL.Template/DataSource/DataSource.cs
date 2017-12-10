using AxCRL.Comm.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCRL.Template.DataSource
{

    public class TableProperty
    {
        public const string IsVirtual = "IsVirtual";
        public const string DBIndex = "DBIndex";
        public const string FieldAddrDic = "FieldAddrDic";
        public const string AllowCopy = "AllowCopy";
        public const string AllowEmpt = "AllowEmpt";
        public const string NotRepeat = "NotRepeat";
        public const string DisplayText = "DisplayText";
        public const string BandColumnList = "BandColumnList";
        public const string DynamicFieldRelaion = "DynamicFieldRelaion";
        public const string UsingApproveRow = "UsingApproveRow";
        public const string FilterSetting = "FilterSetting";
        /// <summary>
        /// 启用附件
        /// </summary>
        public const string UsingAttachment = "UsingAttachment";
    }
    
    public class FieldProperty
    {
        public const string DataType = "DataType";
        public const string ControlType = "ControlType";
        public const string Option = "Option";
        public const string KeyValueOption = "KeyValueOption";
        public const string AllowEmpty = "AllowEmpty";
        public const string Format = "Format";
        public const string RelativeSource = "RelativeSource";
        public const string FieldType = "FieldType";
        public const string ReadOnly = "ReadOnly";
        public const string AllowCopy = "AllowCopy";
        public const string SubTableIndex = "SubTableIndex";
        public const string ColumnSpan = "ColumnSpan";
        public const string RowSpan = "RowSpan";
        public const string RelProgId = "RelProgId";
        public const string RelTableIndex = "RelTableIndex";
        public const string QtyLimit = "QtyLimit";
        public const string AllowCondition = "AllowCondition";
        public const string IsDynamic = "IsDynamic";
        public const string Summary = "Summary";
        public const string InputType = "InputType";
        public const string SummaryRenderer = "SummaryRenderer";
        public const string AttributeField = "AttributeField";
        public const string Precision = "Precision";
        /// <summary>
        /// 步进值属性字符串
        /// </summary>
        public const string StepValue = "StepValue";
        /// <summary>
        /// 字体设置
        /// </summary>
        public const string FontName = "FontName";
        
        /// <summary>
        /// 下拉grid
        /// yangj 20170629 增加
        /// </summary>
        public const string GridAttribute = "GridAttribute";
        
    }
    /// <summary>
    /// 动态字段关系
    /// </summary>
    [Serializable]
    public class LibDynamicFildRelation
    {
        private Dictionary<int, Dictionary<string, LibDynamicFildInfo>> _DynamicFildRelation = null;
        /// <summary>
        /// key:动态字段对应的实际表索引,value动态字段的必要信息
        /// </summary>
        public Dictionary<int, Dictionary<string, LibDynamicFildInfo>> DynamicFildRelation
        {
            get {
                if (_DynamicFildRelation == null)
                { _DynamicFildRelation = new Dictionary<int, Dictionary<string, LibDynamicFildInfo>>(); }
                
                return _DynamicFildRelation;
            }
            
            set { _DynamicFildRelation = value; }
        }
    }
    [Serializable]
    public class LibDynamicFildInfo
    {
        private string _ParentMapToField;
        private string _ParentForValue;
        private string _MapToField;
        private string _FieldForValue;
        private object _Data;
        
        public LibDynamicFildInfo (string mapToFild, string fieldForValue)
        {
            this._MapToField = mapToFild;
            this._FieldForValue = fieldForValue;
        }
        
        public LibDynamicFildInfo (string mapToFild, string fieldForValue, object data)
        {
            this._MapToField = mapToFild;
            this._FieldForValue = fieldForValue;
            this._Data = data;
        }
        /// <summary>
        /// 父节点值
        /// </summary>
        public string ParentForValue
        {
            get { return _ParentForValue; }
            
            set { _ParentForValue = value; }
        }
        /// <summary>
        /// 父节点
        /// </summary>
        public string ParentMapToField
        {
            get { return _ParentMapToField; }
            
            set { _ParentMapToField = value; }
        }
        /// <summary>
        /// 动态字段对应的实际字段
        /// </summary>
        public string MapToField
        {
            get { return _MapToField; }
            
            set { _MapToField = value; }
        }
        /// <summary>
        /// 字段值对应的实际字段
        /// </summary>
        public string FieldForValue
        {
            get { return _FieldForValue; }
            
            set { _FieldForValue = value; }
        }
        /// <summary>
        /// 动态字段的必要信息扩展
        /// </summary>
        public object Data
        {
            get { return _Data; }
            
            set { _Data = value; }
        }
    }
    
    public enum LibSummary {
        None = 0,
        Count = 1,
        Sum = 2,
        Min = 3,
        Max = 4,
        Average = 5
    }
    
    public enum LibQtyLimit {
        None = 0,
        GreaterThanZero = 1,
        LessThanZero = 2,
        GreaterOrEqualThanZero = 3,
        LessOrEqualThanZero = 4,
        ZeroBetweenHundred = 5,
        UnequalToZero = 6,
        ZeroBetweenOne = 7,
    }
    
    public enum FieldType {
        None = 0,
        Virtual = 1,
        Relative = 2,
        SetValue = 3
    }
    
    public class FieldSize
    {
        public const int Size10 = 10;
        public const int Size20 = 20;
        public const int Size40 = 40;
        public const int Size50 = 50;
        public const int Size100 = 100;
        public const int Size200 = 200;
        public const int Size400 = 400;
        public const int Size500 = 500;
        public const int Size1000 = 1000;
        public const int Size2000 = 2000;
        public const int Size4000 = 4000;
        public const int Size5000 = 5000;
    }
    
    public enum InputType {
        Text = 0,
        Password = 1,
        File = 3,
        Url = 4,
        Email = 5
    }
    
    public class FilterSetting
    {
        private int _Day = 0;
        private bool _IsRange = false;
        private string _Name;
        
        public string Name
        {
            get { return _Name; }
            
            set { _Name = value; }
        }
        
        public FilterSetting()
        {
        }
        
        public FilterSetting (string name, int day)
        {
            this._Name = name;
            this._Day = day;
        }
        
        /// <summary>
        /// 是否是一个区间
        /// </summary>
        public bool IsRange
        {
            get { return _IsRange; }
            
            set { _IsRange = value; }
        }
        
        /// <summary>
        /// 间隔天数
        /// </summary>
        public int Day
        {
            get { return _Day; }
            
            set { _Day = value; }
        }
    }
    
    
    
    #region [数据表索引]
    
    /// <summary>
    /// 索引排序规则
    /// </summary>
    public enum IndexOrderWay {
        ASC = 0,
        DESC = 1,
    }
    /// <summary>
    /// 数据库索引
    /// </summary>
    [Serializable]    
    public class DBIndex
    {
        private string _name;
        private DBIndexFieldCollection _dbIndexFields = null;
        private bool _isUnique = false;
        
        public DBIndex (string name, DBIndexFieldCollection dbIndexFields)
        {
            _name = name;
            _dbIndexFields = dbIndexFields;
        }
        
        public DBIndex (string name, DBIndexFieldCollection dbIndexFields, bool isUnique)
        {
            _name = name;
            _dbIndexFields = dbIndexFields;
            _isUnique = isUnique;
        }
        
        /// <summary>
        /// 索引字段集合
        /// </summary>
        public DBIndexFieldCollection DbIndexFields
        {
            get {
                if (_dbIndexFields == null)
                { _dbIndexFields = new DBIndexFieldCollection(); }
                
                return _dbIndexFields;
            }
        }
        /// <summary>
        /// 索引名
        /// </summary>
        public string Name
        {
            get { return _name; }
            
            set { _name = value; }
        }
        
        /// <summary>
        /// 是否是唯一索引
        /// </summary>
        public bool IsUnique
        {
            get { return _isUnique; }
            
            set { _isUnique = value; }
        }
    }
    /// <summary>
    /// 索引字段
    /// </summary>
    [Serializable]
    public class DBIndexField
    {
        private string _name;
        private IndexOrderWay _indexOrderWay = IndexOrderWay.ASC;
        
        public DBIndexField (string name)
        {
            this._name = name;
        }
        
        public DBIndexField (string name, IndexOrderWay indexOrderWay)
        {
            this._name = name;
            this._indexOrderWay = indexOrderWay;
        }
        /// <summary>
        /// 排序规则
        /// </summary>
        public IndexOrderWay IndexOrderWay
        {
            get { return _indexOrderWay; }
            
            set { _indexOrderWay = value; }
        }
        /// <summary>
        /// 索引字段名
        /// </summary>
        public string Name
        {
            get { return _name; }
            
            set { _name = value; }
        }
    }
    [Serializable]
    public class DBIndexFieldCollection : Collection<DBIndexField>
    {
    
    }
    [Serializable]
    public class DBIndexCollection : Collection<DBIndex>
    {
    
    }
    #endregion
    /// <summary>
    /// 存储字段在表里的具体位置信息
    /// </summary>
    [Serializable]
    public class FieldAddr : ILibSerializable
    {
        /// <summary>
        /// 字段在Table的索引
        /// </summary>
        public int FieldIndex;
        /// <summary>
        /// 字段在RelativeSourceCollection的索引
        /// </summary>
        public int RelSourceIndex;
        /// <summary>
        /// 字段在RelFieldCollection的索引
        /// </summary>
        public int RelFieldIndex;
        /// <summary>
        /// 分组时，GroupIndex != 0 时的关联源int[0]和字段索引int[1]
        /// </summary>
        public List<int[]> GroupRelIndexs { get; set; }
        
        public FieldAddr()
        {
        }
        
        public FieldAddr (int fieldIndex, int relSourceIndex, int relFieldIndex)
        {
            this.FieldIndex = fieldIndex;
            this.RelSourceIndex = relSourceIndex;
            this.RelFieldIndex = relFieldIndex;
        }
        
        public void ReadObjectData (LibSerializationInfo info)
        {
            this.FieldIndex = info.ReadInt32();
            this.RelSourceIndex = info.ReadInt32();
            this.RelFieldIndex = info.ReadInt32();
            int count = info.ReadInt32();
            
            if (count > 0) {
                this.GroupRelIndexs = new List<int[]>();
                
                for (int i = 0; i < count; i++) {
                    int length = info.ReadInt32();
                    //this.GroupRelIndexs[i] = new int[length];
                    this.GroupRelIndexs.Add (new int[length]);
                    
                    for (int r = 0; r < length; r++) {
                        this.GroupRelIndexs[i][r] = info.ReadInt32();
                    }
                }
            }
        }
        
        public void WriteObjectData (LibSerializationInfo info)
        {
            info.WriteInt32 (this.FieldIndex);
            info.WriteInt32 (this.RelSourceIndex);
            info.WriteInt32 (this.RelFieldIndex);
            int count = this.GroupRelIndexs == null ? 0 : this.GroupRelIndexs.Count;
            info.WriteInt32 (count);
            
            for (int i = 0; i < count; i++) {
                int length = this.GroupRelIndexs[i].Length;
                info.WriteInt32 (length);
                
                for (int r = 0; r < length; r++) {
                    info.WriteInt32 (this.GroupRelIndexs[i][r]);
                }
            }
        }
        
    }
    
    
    [Serializable]
    public class GridAttribute
    {
        private string _ProgId; //ProgId
        public string ProgId
        {
            get { return _ProgId; }
            
            set { _ProgId = value; }
        }
        private string _FuncName; //方法名
        public string FuncName
        {
            get { return _FuncName; }
            
            set { _FuncName = value; }
        }
        private string _ValueField; //填充字段
        public string ValueField
        {
            get { return _ValueField; }
            
            set { _ValueField = value; }
        }
        private List<GridField> _ShowField; //显示字段(包含填充字段)
        public List<GridField> ShowField
        {
            get { return _ShowField; }
            
            set { _ShowField = value; }
        }
        private string _ParamField; //参数字段
        public string ParamField
        {
            get { return _ParamField; }
            
            set { _ParamField = value; }
        }
    }
    
    [Serializable]
    public class GridField
    {
        public GridField (string f, string t)
        {
            this.FieldName = f;
            this.TextName = t;
        }
        
        private string _FieldName; //字段名称
        private string _TextName; //显示名称
        
        public string FieldName
        {
            get {
                return _FieldName;
            }
            
            set {
                _FieldName = value;
            }
        }
        
        public string TextName
        {
            get {
                return _TextName;
            }
            
            set {
                _TextName = value;
            }
        }
    }
}
