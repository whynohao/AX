using AxCRL.Comm.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCRL.Template
{
    /// <summary>
    /// 功能显示方案
    /// </summary>
    public class LibDisplayScheme : ILibSerializable
    {
        private string _ProgId;
        private Dictionary<int, LibGridScheme> _GridScheme;

        public LibDisplayScheme()
        {

        }

        public LibDisplayScheme(string progId)
        {
            this._ProgId = progId;
        }

        public string ProgId
        {
            get { return _ProgId; }
            set { _ProgId = value; }
        }

        public Dictionary<int, LibGridScheme> GridScheme
        {
            get
            {
                if (_GridScheme == null)
                    _GridScheme = new Dictionary<int, LibGridScheme>();
                return _GridScheme;
            }
            set { _GridScheme = value; }
        }

        public void ReadObjectData(LibSerializationInfo info)
        {
            this._ProgId = info.ReadString();
            int count = info.ReadInt32();
            if (count > 0)
            {
                _GridScheme = new Dictionary<int, LibGridScheme>(count);
                for (int i = 0; i < count; i++)
                {
                    _GridScheme.Add(info.ReadInt32(), (LibGridScheme)info.ReadObject());
                }
            }
        }

        public void WriteObjectData(LibSerializationInfo info)
        {
            info.WriteString(this._ProgId);
            int count = this._GridScheme.Count;
            info.WriteInt32(count);
            foreach (KeyValuePair<int, LibGridScheme> item in this._GridScheme)
            {
                info.WriteInt32(item.Key);
                info.WriteObject(item.Value);
            }
        }
    }

    public class LibGridScheme : ILibSerializable
    {
        private IList<LibGridBandFieldScheme> _GridFields;
        public IList<LibGridBandFieldScheme> GridFields
        {
            get
            {
                if (_GridFields == null)
                    _GridFields = new List<LibGridBandFieldScheme>();
                return _GridFields;
            }
            set { _GridFields = value; }
        }

        public void ReadObjectData(LibSerializationInfo info)
        {
            int count = info.ReadInt32();
            if (count > 0)
            {
                _GridFields = new List<LibGridBandFieldScheme>(count);
                for (int i = 0; i < count; i++)
                {
                    _GridFields.Add((LibGridBandFieldScheme)info.ReadObject());
                }
            }
        }

        public void WriteObjectData(LibSerializationInfo info)
        {
            int count = this._GridFields.Count;
            info.WriteInt32(count);
            for (int i = 0; i < count; i++)
            {
                info.WriteObject(this._GridFields[i]);
            }
        }
    }


    public class LibGridFieldScheme : ILibSerializable
    {

        public LibGridFieldScheme()
        {

        }
        public LibGridFieldScheme(string name, int width)
        {
            this._Name = name;
            this._Width = width;
        }
        private string _Name;
        private int _Width;

        public int Width
        {
            get { return _Width; }
            set { _Width = value; }
        }

        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }

        public void ReadObjectData(LibSerializationInfo info)
        {
            this._Name = info.ReadString();
            this._Width = info.ReadInt32();
        }

        public void WriteObjectData(LibSerializationInfo info)
        {
            info.WriteString(this._Name);
            info.WriteInt32(this._Width);
        }
    }

    public class LibGridBandFieldScheme : ILibSerializable
    {
        private string _Header;
        private LibGridFieldScheme _Field;
        private IList<LibGridBandFieldScheme> _BandFields;

        public string Header
        {
            get { return _Header; }
            set { _Header = value; }
        }

        public IList<LibGridBandFieldScheme> BandFields
        {
            get
            {
                if (_BandFields == null)
                    _BandFields = new List<LibGridBandFieldScheme>();
                return _BandFields;
            }
            set { _BandFields = value; }
        }

        public LibGridFieldScheme Field
        {
            get { return _Field; }
            set { _Field = value; }
        }

        public void ReadObjectData(LibSerializationInfo info)
        {
            this._Header = info.ReadString();
            bool hasValue = info.ReadBoolean();
            if (hasValue)
                this._Field = info.ReadObject() as LibGridFieldScheme;
            int count = info.ReadInt32();
            if (count > 0)
            {
                _BandFields = new List<LibGridBandFieldScheme>(count);
                for (int i = 0; i < count; i++)
                {
                    _BandFields.Add((LibGridBandFieldScheme)info.ReadObject());
                }
            }
        }

        public void WriteObjectData(LibSerializationInfo info)
        {
            info.WriteString(this._Header);
            if (this._Field == null)
                info.WriteBoolean(false);
            else
            {
                info.WriteBoolean(true);
                info.WriteObject(this._Field);
            }
            int count = this._BandFields == null ? 0 : this._BandFields.Count;
            info.WriteInt32(count);
            for (int i = 0; i < count; i++)
            {
                info.WriteObject(this._BandFields[i]);
            }
        }
    }
}
