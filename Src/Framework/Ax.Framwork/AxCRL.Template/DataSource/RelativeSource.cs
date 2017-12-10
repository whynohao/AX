using AxCRL.Comm.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AxCRL.Template.DataSource
{
    /// <summary>
    /// 关联字段
    /// </summary>
    [Serializable]
    public class RelField : ILibSerializable
    {
        private string _name;
        private string _asName;
        private string _visibleCondition;
        private string _DisplayText;
        private LibDataType _DataType;
        private int _Size;
        private LibControlType _ControlType = LibControlType.NText;
        private string[] _TextOption = null;
        private LibTextOptionCollection _KeyValueOption;
        private string _Format = null;
        private int _ColumnSpan = 1;
        private int _RowSpan = 1;
        private bool _AllowCondition = true;
        private LibSummary _Summary = LibSummary.None;
        private InputType _InputType = InputType.Text;
        private string _SummaryRenderer;
        private string _AttributeField;
        private int _Precision = 2;

        public int Precision
        {
            get { return _Precision; }
            set { _Precision = value; }
        }

        public string AttributeField
        {
            get { return _AttributeField; }
            set { _AttributeField = value; }
        }

        public string SummaryRenderer
        {
            get { return _SummaryRenderer; }
            set { _SummaryRenderer = value; }
        }

        public int RowSpan
        {
            get { return _RowSpan; }
            set { _RowSpan = value; }
        }

        public InputType InputType
        {
            get { return _InputType; }
            set { _InputType = value; }
        }
        public LibSummary Summary
        {
            get { return _Summary; }
            set { _Summary = value; }
        }

        public bool AllowCondition
        {
            get { return _AllowCondition; }
            set { _AllowCondition = value; }
        }

        public string[] TextOption
        {
            get { return _TextOption; }
            set
            {
                _TextOption = value;
                if (_TextOption != null)
                    this._ControlType = LibControlType.TextOption;
            }
        }

        public LibTextOptionCollection KeyValueOption
        {
            get { return _KeyValueOption; }
            set
            {
                _KeyValueOption = value;
                if (_TextOption != null)
                    this._ControlType = LibControlType.TextOption;
            }
        }

        public string Format
        {
            get { return _Format; }
            set { _Format = value; }
        }


        public int ColumnSpan
        {
            get { return _ColumnSpan; }
            set { _ColumnSpan = value; }
        }

        public LibControlType ControlType
        {
            get { return _ControlType; }
            set { _ControlType = value; }
        }

        public int Size
        {
            get { return _Size; }
            set { _Size = value; }
        }

        public LibDataType DataType
        {
            get { return _DataType; }
            set { _DataType = value; }
        }

        public string DisplayText
        {
            get { return _DisplayText; }
            set { _DisplayText = value; }
        }

        public RelField()
        {

        }

        public RelField(string name, LibDataType dataType, int size, string displayText)
        {
            _name = name;
            this._DataType = dataType;
            this._Size = size;
            this._DisplayText = displayText;
        }

        public RelField(string name, LibDataType dataType, int size, string displayText, string asName)
        {
            _name = name;
            this._DataType = dataType;
            this._Size = size;
            this._DisplayText = displayText;
            this.AsName = asName;
        }


        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }

        public string AsName
        {
            get
            {
                return _asName;
            }
            set
            {
                _asName = value;
            }
        }

        public string VisibleCondition
        {
            get
            {
                return _visibleCondition;
            }
            set
            {
                _visibleCondition = value;
            }
        }


        public void ReadObjectData(LibSerializationInfo info)
        {
            this.Name = info.ReadString();
            this.VisibleCondition = info.ReadString();
            this.AsName = info.ReadString();
            this.DisplayText = info.ReadString();
        }

        public void WriteObjectData(LibSerializationInfo info)
        {
            info.WriteString(this.Name);
            info.WriteString(this.VisibleCondition);
            info.WriteString(this.AsName);
            info.WriteString(this.DisplayText);
        }
    }
    /// <summary>
    /// 初值来源字段
    /// </summary>
    [Serializable]
    public class SetValueField : ILibSerializable
    {
        private string _name;
        private string _asName;

        public SetValueField()
        {

        }

        public SetValueField(string name)
        {
            _name = name;
        }
        public SetValueField(string name, string asName)
        {
            _name = name;
            _asName = asName;
        }

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }

        public string AsName
        {
            get
            {
                return _asName;
            }
            set
            {
                _asName = value;
            }
        }


        public void ReadObjectData(LibSerializationInfo info)
        {
            this.Name = info.ReadString();
            this.AsName = info.ReadString();
        }

        public void WriteObjectData(LibSerializationInfo info)
        {
            info.WriteString(this.Name);
            info.WriteString(this.AsName);
        }
    }
    /// <summary>
    /// 关联过滤条件
    /// </summary>
    [Serializable]
    public class SelCondition : ILibSerializable
    {
        private string _condition;
        private string _msgCode;
        private string _msgParam;
        private string _DisplayText;

        public string DisplayText
        {
            get { return _DisplayText; }
            set { _DisplayText = value; }
        }

        public SelCondition()
        {

        }

        public string Condition
        {
            get
            {
                return _condition;
            }
            set
            {
                _condition = value;
            }
        }

        public string MsgCode
        {
            get
            {
                return _msgCode;
            }
            set
            {
                _msgCode = value;
            }
        }

        public string MsgParam
        {
            get
            {
                return _msgParam;
            }
            set
            {
                _msgParam = value;
            }
        }

        public void ReadObjectData(LibSerializationInfo info)
        {
            this.Condition = info.ReadString();
            this.MsgCode = info.ReadString();
            this.MsgParam = info.ReadString();
            this.DisplayText = info.ReadString();
        }

        public void WriteObjectData(LibSerializationInfo info)
        {
            info.WriteString(this.Condition);
            info.WriteString(this.MsgCode);
            info.WriteString(this.MsgParam);
            info.WriteString(this.DisplayText);
        }
    }

    /// <summary>
    /// 关联来源
    /// </summary>
    [Serializable]
    public class RelativeSource : ILibSerializable
    {
        private string _relSource;
        private int _tableIndex = 0;
        private string _relPK;
        private bool _isCheckSource = false;
        private int _groupIndex;
        private string _groupCondation;
        private RelFieldCollection _relFields = null;
        private SelConditionCollection _selConditions = null;
        private SetValueFieldCollection _setValueFields = null;

        /// <summary>
        /// 关联字段的控件类型为IdName时，更多的筛选列集合（不仅仅从Id和Name两个列中筛选）
        /// </summary>
        private RelFieldCollection _idNameFilterFields = null;
        /// <summary>
        /// 指定具有父子结构的关联数据源时，如果要以树形结构展示，需要设置关联数据源的父子引用关系列的列名
        /// </summary>
        private string _parentColumnName = null;
        /// <summary>
        /// 对于具有父子结构的关联数据源，查找时是否包含子数据，以免形成循环结构
        /// </summary>
        private bool _containsSub = false;

        /// <summary>
        /// 查询筛选的数据行数，默认为30
        /// </summary>
        private int _searchFilterCount = 30;
        /// <summary>
        /// 树形数据是否默认全部展开
        /// </summary>
        private bool _expandAll = false;

        public RelativeSource()
        {

        }

        public RelativeSource(string relSource)
        {
            _relSource = relSource;
        }

        public string RelSource
        {
            get
            {
                return _relSource;
            }
            set
            {
                _relSource = value;
            }
        }

        public int TableIndex
        {
            get
            {
                return _tableIndex;
            }
            set
            {
                _tableIndex = value;
            }
        }

        public string RelPK
        {
            get
            {
                return _relPK;
            }
            set
            {
                _relPK = value;
            }
        }

        public bool IsCheckSource
        {
            get
            {
                return _isCheckSource;
            }
            set
            {
                _isCheckSource = value;
            }
        }
        public int GroupIndex
        {
            get
            {
                return _groupIndex;
            }
            set
            {
                _groupIndex = value;
            }
        }

        public string GroupCondation
        {
            get
            {
                return _groupCondation;
            }
            set
            {
                _groupCondation = value;
            }
        }

        public RelFieldCollection RelFields
        {
            get
            {
                if (_relFields == null)
                    _relFields = new RelFieldCollection();
                return _relFields;
            }
            set
            {
                _relFields = value;
            }
        }

        public SetValueFieldCollection SetValueFields
        {
            get
            {
                if (_setValueFields == null)
                    _setValueFields = new SetValueFieldCollection();
                return _setValueFields;
            }
            set
            {
                _setValueFields = value;
            }
        }

        public SelConditionCollection SelConditions
        {
            get
            {
                if (_selConditions == null)
                    _selConditions = new SelConditionCollection();
                return _selConditions;
            }
            set
            {
                _selConditions = value;
            }
        }

        /// <summary>
        /// 关联字段的控件类型为IdName时，更多的筛选列集合（不仅仅从Id和Name两个列中筛选）
        /// </summary>
        public RelFieldCollection IdNameFilterFields
        {
            get
            {
                if (_idNameFilterFields == null)
                    _idNameFilterFields = new RelFieldCollection();
                return _idNameFilterFields;
            }
            set
            {
                _idNameFilterFields = value;
            }
        }
        /// <summary>
        /// 指定具有父子结构的关联数据源时，如果要以树形结构展示，需要设置关联数据源的父子引用关系列的列名
        /// </summary>
        public string ParentColumnName
        {
            get { return _parentColumnName; }
            set { _parentColumnName = value; }
        }
        /// <summary>
        /// 对于具有父子结构的关联数据源，查找时是否包含子数据，以免形成循环结构。默认为false
        /// 注意：不适合数据量较大的数据筛选
        /// </summary>
        public bool ContainsSub
        {
            get { return _containsSub; }
            set { _containsSub = value; }
        }
        /// <summary>
        /// 树形数据是否默认全部展开。默认为false
        /// </summary>
        public bool ExpandAll
        {
            get { return _expandAll; }
            set { _expandAll = value; }
        }
        /// <summary>
        /// 查询筛选的数据行数，默认为30
        /// 有效范围为5到500
        /// </summary>
        public int SearchFilterCount
        {
            get { return _searchFilterCount; }
            set { _searchFilterCount = (value >= 5 && value <= 500)?value: 30; }
        }

        private Dictionary<string, bool> _OrderbyColumns = new Dictionary<string, bool>();
        /// <summary>
        /// 查询时需要排序的数据列与是否升序的对应关系
        /// 注意：对于具有动态的IdNameFilterFields列无法完全排序
        /// </summary>
        public Dictionary<string, bool> OrderbyColumns
        {
            get { return _OrderbyColumns; }
            set { _OrderbyColumns = value; }
        }

        public void ReadObjectData(LibSerializationInfo info)
        {
            this.RelSource = info.ReadString();
            this.TableIndex = info.ReadInt32();
            this.RelPK = info.ReadString();
            this.IsCheckSource = info.ReadBoolean();
            this.GroupIndex = info.ReadInt32();
            this.GroupCondation = info.ReadString();
            int count = info.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                this.RelFields.Add((RelField)info.ReadObject());
            }
            count = info.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                this.SetValueFields.Add((SetValueField)info.ReadObject());
            }
            count = info.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                this.SelConditions.Add((SelCondition)info.ReadObject());
            }            
        }

        public void WriteObjectData(LibSerializationInfo info)
        {
            info.WriteString(this.RelSource);
            info.WriteInt32(this.TableIndex);
            info.WriteString(this.RelPK);
            info.WriteBoolean(this.IsCheckSource);
            info.WriteInt32(this.GroupIndex);
            info.WriteString(this.GroupCondation);
            int count = this.RelFields.Count;
            info.WriteInt32(count);
            for (int i = 0; i < count; i++)
            {
                info.WriteObject(this.RelFields[i]);
            }
            count = this.SetValueFields.Count;
            info.WriteInt32(count);
            for (int i = 0; i < count; i++)
            {
                info.WriteObject(this.SetValueFields[i]);
            }
            count = this.SelConditions.Count;
            info.WriteInt32(count);
            for (int i = 0; i < count; i++)
            {
                info.WriteObject(this.SelConditions[i]);
            }            
        }
    }
    [Serializable]
    public class RelativeSourceCollection : ObservableCollection<RelativeSource>
    {

    }
    [Serializable]
    public class RelFieldCollection : ObservableCollection<RelField>
    {

    }
    [Serializable]
    public class SetValueFieldCollection : ObservableCollection<SetValueField>
    {

    }
    [Serializable]
    public class SelConditionCollection : ObservableCollection<SelCondition>
    {

    }
}
