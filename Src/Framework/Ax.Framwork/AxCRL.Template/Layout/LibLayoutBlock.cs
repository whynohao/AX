using AxCRL.Template.DataSource;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AxCRL.Template.Layout
{
    public abstract class LibLayoutBlock
    {
        private LibLayoutBlockType _BlockType = LibLayoutBlockType.ControlGroup;
        private string _Renderer;
        private string _DisplayName;
        

        public string DisplayName
        {
            get { return _DisplayName; }
            set { _DisplayName = value; }
        }

        public string Renderer
        {
            get
            {
                _Renderer = CreateRenderer();
                //_Renderer = Regex.Replace(_Renderer, @"\s", string.Empty);  //空格被替换完了会导致html等解析错误
                return _Renderer;
            }
        }

        public LibLayoutBlockType BlockType
        {
            get { return _BlockType; }
            set { _BlockType = value; }
        }
        

        public abstract string CreateRenderer();
    }

    public class LibControlLayoutBlock : LibLayoutBlock
    {
        private DataSet _DataSet;
        private int _TableIndex;
        private IList<string> _FieldList;

        public IList<string> FieldList
        {
            get
            {
                if (_FieldList == null)
                    _FieldList = new List<string>();
                return _FieldList;
            }
            set { _FieldList = value; }
        }

        public int TableIndex
        {
            get { return _TableIndex; }
            set { _TableIndex = value; }
        }

        public DataSet DataSet
        {
            get { return _DataSet; }
            set { _DataSet = value; }
        }
        public string Store
        {
            get { return this.DataSet.Tables[TableIndex].TableName; }
        }

        public LibControlLayoutBlock(DataSet dataSet, int tableIndex, string displayName, IList<string> _fieldList)
        {
            this.DataSet = dataSet;
            this.TableIndex = tableIndex;
            this.DisplayName = displayName;
            this.BlockType = LibLayoutBlockType.ControlGroup;
            this._FieldList = _fieldList;
        }

        public override string CreateRenderer()
        {
            DataTable table = this.DataSet.Tables[TableIndex];
            List<LayoutField> fields = new List<LayoutField>(this.FieldList.Count);
            foreach (string item in this.FieldList)
            {
                if (string.IsNullOrEmpty(item) == false && table.Columns.Contains(item))
                    fields.Add(new LayoutField(table.Columns[item], TableIndex));
            }
            return JsBuilder.BuildControlGroup(fields);
        }
    }


    public class LibGridLayoutBlock : LibLayoutBlock
    {
        private DataSet _DataSet;
        private int _TableIndex;
        private IList<string> _FieldList;
        private bool _AddAutoRowNo = false;
        private LibGridScheme _GridScheme;
        private bool _IsCanNotEditRow = false;

        public LibGridScheme GridScheme
        {
            get { return _GridScheme; }
            set { _GridScheme = value; }
        }

        public bool AddAutoRowNo
        {
            get { return _AddAutoRowNo; }
            set { _AddAutoRowNo = value; }
        }

        public IList<string> FieldList
        {
            get
            {
                if (_FieldList == null)
                    _FieldList = new List<string>();
                return _FieldList;
            }
            set { _FieldList = value; }
        }

        public int TableIndex
        {
            get { return _TableIndex; }
            set { _TableIndex = value; }
        }

        public DataSet DataSet
        {
            get { return _DataSet; }
            set { _DataSet = value; }
        }
        public string Store
        {
            get { return this.DataSet.Tables[TableIndex].TableName; }
        }
        /// <summary>
        /// 是否不可以新增或删除行
        /// </summary>
        public bool IsCanNotEditRow
        {
            get { return _IsCanNotEditRow; }
            set { _IsCanNotEditRow = value; }
        }
        public LibGridLayoutBlock(DataSet dataSet, int tableIndex, string displayName, bool addAutoRowNo = false)
        {
            this.DataSet = dataSet;
            this.TableIndex = tableIndex;
            this.DisplayName = displayName;
            this.BlockType = LibLayoutBlockType.Grid;
            this.AddAutoRowNo = addAutoRowNo;
        }

        public override string CreateRenderer()
        {
            DataTable table = this.DataSet.Tables[TableIndex];
            List<LayoutField> layoutFields = new List<LayoutField>();
            if (GridScheme != null)
            {
                HashSet<string> exist = new HashSet<string>();
                foreach (var item in GridScheme.GridFields)
                {
                    LibGridFieldScheme realItem = item.Field;
                    if (!exist.Contains(realItem.Name))
                        exist.Add(realItem.Name);
                    if (!table.Columns.Contains(realItem.Name))
                        continue;
                    LayoutField field = new LayoutField(table.Columns[realItem.Name], TableIndex);
                    if (realItem.Width != field.Width)
                        field.Width = realItem.Width;
                    layoutFields.Add(field);
                }
                foreach (var item in this.FieldList)
                {
                    if (exist.Contains(item))
                        continue;
                    if (string.IsNullOrEmpty(item) || table.Columns.Contains(item) == false)
                        continue;
                    LayoutField field = new LayoutField(table.Columns[item], TableIndex);
                    field.Hidden = true;
                    layoutFields.Add(field);
                }
            }
            else
            {
                foreach (var item in this.FieldList)
                {
                    if (string.IsNullOrEmpty(item) || table.Columns.Contains(item) == false)
                        continue;
                    layoutFields.Add(new LayoutField(table.Columns[item], TableIndex));
                }
            }
            return JsBuilder.BuildGrid(layoutFields, AddAutoRowNo);
        }
    }

    public class LibBandGridLayoutBlock : LibGridLayoutBlock
    {
        private IList<BandColumn> _BandColumn;

        public LibBandGridLayoutBlock(DataSet dataSet, int tableIndex, string displayName, bool addAutoRowNo = false)
            : base(dataSet, tableIndex, displayName, addAutoRowNo)
        {

        }

        public IList<BandColumn> BandColumn
        {
            get
            {
                if (_BandColumn == null)
                    _BandColumn = new List<BandColumn>();
                return _BandColumn;
            }
            set { _BandColumn = value; }
        }


        public override string CreateRenderer()
        {
            DataTable table = this.DataSet.Tables[TableIndex];
            List<BandLayoutField> bandLayoutField = new List<BandLayoutField>();
            if (GridScheme != null)
            {
                HashSet<string> exist = new HashSet<string>();
                foreach (LibGridBandFieldScheme item in GridScheme.GridFields)
                {
                    BuildBandColForScheme(item, bandLayoutField, table, exist);
                }
                foreach (BandColumn item in this.BandColumn)
                {
                    BuildBandHideCol(item, bandLayoutField, table, exist);
                }
            }
            else
            {
                foreach (BandColumn item in this.BandColumn)
                {
                    BuildBandCol(item, bandLayoutField, table);
                }
            }
            return JsBuilder.BuildBandGrid(bandLayoutField, AddAutoRowNo);
        }

        private void BuildBandCol(BandColumn bandColumn, List<BandLayoutField> list, DataTable table)
        {
            if (bandColumn.Columns != null && bandColumn.Columns.Count > 0)
            {
                List<BandLayoutField> subList = new List<BandLayoutField>();
                list.Add(new BandLayoutField(bandColumn.Name, subList));
                foreach (var item in bandColumn.Columns)
                {
                    BuildBandCol(item, subList, table);
                }
            }
            else
            {
                list.Add(new BandLayoutField(bandColumn.Name) { Field = new LayoutField(table.Columns[bandColumn.Name], TableIndex) });
            }
        }

        private void BuildBandHideCol(BandColumn bandColumn, List<BandLayoutField> list, DataTable table, HashSet<string> exist)
        {
            if (bandColumn.Columns != null && bandColumn.Columns.Count > 0)
            {
                foreach (var item in bandColumn.Columns)
                {
                    BuildBandHideCol(item, list, table, exist);
                }
            }
            else
            {
                if (!exist.Contains(bandColumn.Name))
                    list.Add(new BandLayoutField(bandColumn.Name) { Field = new LayoutField(table.Columns[bandColumn.Name], TableIndex) { Hidden = true } });
            }
        }

        private void BuildBandColForScheme(LibGridBandFieldScheme bandColumn, List<BandLayoutField> list, DataTable table, HashSet<string> exist)
        {
            if (bandColumn.BandFields != null && bandColumn.BandFields.Count > 0)
            {
                List<BandLayoutField> subList = new List<BandLayoutField>();
                list.Add(new BandLayoutField(bandColumn.Header, subList));
                foreach (var item in bandColumn.BandFields)
                {
                    BuildBandColForScheme(item, subList, table, exist);
                }
            }
            else
            {
                string fieldName = bandColumn.Field.Name;
                if (table.Columns.Contains(fieldName))
                {
                    list.Add(new BandLayoutField(fieldName) { Field = new LayoutField(table.Columns[fieldName], TableIndex) { Width = bandColumn.Field.Width } });
                    if (!exist.Contains(fieldName))
                        exist.Add(fieldName);
                }
            }
        }

    }

    public class LibFuncLayoutBlock : LibLayoutBlock
    {
        private IList<FunButton> _Buttons;

        public IList<FunButton> Buttons
        {
            get
            {
                if (_Buttons == null)
                    _Buttons = new List<FunButton>();
                return _Buttons;
            }
            set { _Buttons = value; }
        }

        public LibFuncLayoutBlock()
        {
            this.BlockType = LibLayoutBlockType.Func;
        }

        public override string CreateRenderer()
        {
            return JsBuilder.BuildButton(this.Buttons);
        }
    }

    public enum LibLayoutBlockType
    {
        ControlGroup = 0,
        Grid = 1,
        Func = 2,
        TreeView = 3,
        TreeGrid = 4
    }
}
