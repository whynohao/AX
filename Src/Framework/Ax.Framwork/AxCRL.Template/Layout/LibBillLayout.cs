using AxCRL.Comm.Runtime;
using AxCRL.Comm.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCRL.Template.Layout
{
    public class LibBillLayout : IViewLayout
    {
        private const string _Name = "LibBillLayout";
        private string _SchemeName = null;
        private LibDisplayScheme _DisplayScheme = null;
        private DataSet _DataSet;
        private LibLayoutBlock _HeaderRange;
        private IList<LibLayoutBlock> _TabRange;
        private LibLayoutBlock _GridRange;
        private LibLayoutBlock _ButtonRange;
        private Dictionary<int, LibLayoutBlock> _SubBill;

        public LibDisplayScheme DisplayScheme
        {
            get
            {
                if (!string.IsNullOrEmpty(SchemeName))
                {
                    string path = Path.Combine(EnvProvider.Default.MainPath, "Scheme", "ShowScheme", SchemeName);
                    if (File.Exists(path))
                    {
                        LibBinaryFormatter formatter = new LibBinaryFormatter();
                        using (FileStream fs = new FileStream(path, FileMode.Open))
                        {
                            _DisplayScheme = (LibDisplayScheme)formatter.Deserialize(fs);
                        }
                    }
                    if (_DisplayScheme != null)
                    {
                        LibGridLayoutBlock dest = null;
                        foreach (var item in TabRange)
                        {
                            dest = item as LibGridLayoutBlock;
                            if (dest != null && _DisplayScheme.GridScheme.ContainsKey(dest.TableIndex))
                                dest.GridScheme = _DisplayScheme.GridScheme[dest.TableIndex];
                        }
                        dest = this.GridRange as LibGridLayoutBlock;
                        if (dest != null && _DisplayScheme.GridScheme.ContainsKey(dest.TableIndex))
                            dest.GridScheme = _DisplayScheme.GridScheme[dest.TableIndex];
                        foreach (var item in SubBill)
                        {
                            dest = item.Value as LibGridLayoutBlock;
                            if (dest != null && _DisplayScheme.GridScheme.ContainsKey(dest.TableIndex))
                                dest.GridScheme = _DisplayScheme.GridScheme[dest.TableIndex];
                        }
                    }
                }
                return _DisplayScheme;
            }
        }

        public string Name
        {
            get
            {
                return _Name;
            }
        }

        public string SchemeName
        {
            get
            {
                return _SchemeName;
            }
            set
            {
                _SchemeName = value;
            }
        }

        public DataSet DataSet
        {
            get { return _DataSet; }
            set { _DataSet = value; }
        }

        public LibLayoutBlock HeaderRange
        {
            get
            {
                return _HeaderRange;
            }
            set { _HeaderRange = value; }
        }


        public IList<LibLayoutBlock> TabRange
        {
            get
            {
                if (_TabRange == null)
                    _TabRange = new List<LibLayoutBlock>();
                return _TabRange;
            }
        }


        public LibLayoutBlock GridRange
        {
            get
            {
                return _GridRange;
            }
            set { _GridRange = value; }
        }


        public LibLayoutBlock ButtonRange
        {
            get
            {
                return _ButtonRange;
            }
            set { _ButtonRange = value; }
        }

        public Dictionary<int, LibLayoutBlock> SubBill
        {
            get
            {
                if (_SubBill == null)
                    _SubBill = new Dictionary<int, LibLayoutBlock>();
                return _SubBill;
            }
            set { _SubBill = value; }
        }

        public LibBillLayout(DataSet dataSet)
        {
            this.DataSet = dataSet;
        }

        public LibControlLayoutBlock BuildControlGroup(int tableIndex, string displayName, IList<string> fieldList)
        {
            return LibViewLayoutBuilder.BuildControlGroup(this.DataSet, tableIndex, displayName, fieldList);
        }


        public LibGridLayoutBlock BuildGrid(int tableIndex, string displayName, IList<string> fieldList = null, bool addAutoRowNo = false)
        {
            return LibViewLayoutBuilder.BuildGrid(this.DataSet, tableIndex, displayName, fieldList, addAutoRowNo);
        }

        public LibBandGridLayoutBlock BuildBandGrid(int tableIndex, string displayName, IList<BandColumn> bandColumn, bool addAutoRowNo = false)
        {
            return LibViewLayoutBuilder.BuildBandGrid(this.DataSet, tableIndex, displayName, bandColumn, addAutoRowNo);
        }

        public LibFuncLayoutBlock BuildButton(IList<FunButton> buttons)
        {
            return LibViewLayoutBuilder.BuildButton(buttons);
        }

        Dictionary<string, string> IViewLayout.GetButtonList()
        {
            return LibViewLayoutBuilder.GetButtonList(this.ButtonRange);
        }
    }

    public class FunButton
    {
        private string _DisplayText;
        private string _Name;
        private int _UseCondition;
        private IList<FunButton> _FunButtonList = null;

        public FunButton()
        {

        }

        public FunButton(string name, string displayText)
        {
            this._Name = name;
            this._DisplayText = displayText;
        }

        public IList<FunButton> FunButtonList
        {
            get
            {
                if (_FunButtonList == null)
                    _FunButtonList = new List<FunButton>();
                return _FunButtonList;
            }
            set
            {
                _FunButtonList = value;
            }
        }
        public int UseCondition
        {
            get { return _UseCondition; }
            set { _UseCondition = value; }
        }

        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }

        public string DisplayText
        {
            get { return _DisplayText; }
            set { _DisplayText = value; }
        }
    }

}
