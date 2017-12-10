using AxCRL.Comm.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCRL.Comm.Define
{
    /// <summary>
    /// ProgId清单
    /// </summary>
    public class ProgIdConfigListing : ILibSerializable
    {
        private long _Version;
        private Dictionary<string, long> _DllVersions;
        private Dictionary<string, ProgIdRelationDll> _RelationDlls;
        private Dictionary<string, string> _VclMap;
        private Dictionary<string, string> _ViewMap;

        public Dictionary<string, string> ViewMap
        {
            get
            {
                if (_ViewMap == null)
                    _ViewMap = new Dictionary<string, string>();
                return _ViewMap;
            }
            set { _ViewMap = value; }
        }

        public Dictionary<string, string> VclMap
        {
            get
            {
                if (_VclMap == null)
                    _VclMap = new Dictionary<string, string>();
                return _VclMap;
            }
            set { _VclMap = value; }
        }

        public string GetMap()
        {
            string mapStr = string.Empty;
            StringBuilder builder = new StringBuilder();
            foreach (var item in this.VclMap)
            {
                builder.Append(string.Format("{0}:'{1}',", item.Key, item.Value));
            }
            foreach (var item in this.ViewMap)
            {
                builder.Append(string.Format("{0}:'{1}',", item.Key, item.Value));
            }
            if (builder.Length > 0)
            {
                builder.Remove(builder.Length - 1, 1);
                mapStr = "{" + builder.ToString() + "}";
            }
            return mapStr;
        }

        /// <summary>
        /// ProgId关联Dll信息
        /// </summary>
        public Dictionary<string, ProgIdRelationDll> RelationDlls
        {
            get
            {
                if (_RelationDlls == null)
                    _RelationDlls = new Dictionary<string, ProgIdRelationDll>();
                return _RelationDlls;
            }
            set { _RelationDlls = value; }
        }

        /// <summary>
        /// Dll最新的版本
        /// </summary>
        public Dictionary<string, long> DllVersions
        {
            get
            {
                if (_DllVersions == null)
                    _DllVersions = new Dictionary<string, long>();
                return _DllVersions;
            }
            set { _DllVersions = value; }
        }

        /// <summary>
        /// 版本
        /// </summary>
        public long Version
        {
            get { return _Version; }
            set { _Version = value; }
        }


        public void ReadObjectData(LibSerializationInfo info)
        {
            this.Version = info.ReadInt64();
            int count = info.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                this.RelationDlls.Add(info.ReadString(), (ProgIdRelationDll)info.ReadObject());
            }
            count = info.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                this.DllVersions.Add(info.ReadString(), info.ReadInt64());
            }
            count = info.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                this.VclMap.Add(info.ReadString(), info.ReadString());
            }
            count = info.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                this.ViewMap.Add(info.ReadString(), info.ReadString());
            }
        }

        public void WriteObjectData(LibSerializationInfo info)
        {
            info.WriteInt64(this.Version);
            int count = this.RelationDlls.Count;
            info.WriteInt32(count);
            foreach (KeyValuePair<string, ProgIdRelationDll> item in this.RelationDlls)
            {
                info.WriteString(item.Key);
                info.WriteObject(item.Value);
            }
            count = this.DllVersions.Count;
            info.WriteInt32(count);
            foreach (KeyValuePair<string, long> item in this.DllVersions)
            {
                info.WriteString(item.Key);
                info.WriteInt64(item.Value);
            }
            count = this.VclMap.Count;
            info.WriteInt32(count);
            foreach (KeyValuePair<string, string> item in this.VclMap)
            {
                info.WriteString(item.Key);
                info.WriteString(item.Value);
            }
            count = this.ViewMap.Count;
            info.WriteInt32(count);
            foreach (KeyValuePair<string, string> item in this.ViewMap)
            {
                info.WriteString(item.Key);
                info.WriteString(item.Value);
            }
        }
    }

    public class ProgIdRelationDll : ILibSerializable
    {
        private string _DllName;
        private string _ClassName;

        public ProgIdRelationDll()
        {

        }
        public ProgIdRelationDll(string dllName, string className)
        {
            this.DllName = dllName;
            this.ClassName = className;
        }
        /// <summary>
        /// 类名
        /// </summary>
        public string ClassName
        {
            get { return _ClassName; }
            set { _ClassName = value; }
        }
        /// <summary>
        /// Dll名
        /// </summary>
        public string DllName
        {
            get { return _DllName; }
            set { _DllName = value; }
        }

        public void ReadObjectData(LibSerializationInfo info)
        {
            this.DllName = info.ReadString();
            this.ClassName = info.ReadString();
        }

        public void WriteObjectData(LibSerializationInfo info)
        {
            info.WriteString(this.DllName);
            info.WriteString(this.ClassName);
        }
    }

}
