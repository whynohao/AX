using AxCRL.Comm.Define;
using AxCRL.Comm.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCRL.Core
{
    public class ProgIdHost
    {
        private static ProgIdHost _Instance = null;
        private static readonly object lockObj = new object();
        private ProgIdHost()
        {
            _ProgIdRef = new Dictionary<string, BcfServerInfo>();
        }

        public static ProgIdHost Instance
        {
            get
            {
                if (_Instance == null)
                {
                    lock (lockObj)
                    {
                        if (_Instance == null)
                            _Instance = new ProgIdHost();
                    }
                }
                return _Instance;
            }
        }

        private Dictionary<string, BcfServerInfo> _ProgIdRef = null;

        /// <summary>
        /// ProgId对应的Part索引
        /// </summary>
        public Dictionary<string, BcfServerInfo> ProgIdRef
        {
            get
            {
                return _ProgIdRef;
            }
            private set { _ProgIdRef = value; }
        }

        public void Run()
        {
            ProgIdConfigListing progIdConfigListing = ProgIdConfigListingManager.GetProgIdListing(EnvProvider.Default.MainPath);
            if (progIdConfigListing != null)
            {
                foreach (var item in progIdConfigListing.RelationDlls)
                {
                    BcfServerInfo info = new BcfServerInfo(item.Value.DllName, item.Value.ClassName);
                    if (ProgIdRef.ContainsKey(item.Key))
                        ProgIdRef[item.Key] = info;
                    else
                        ProgIdRef.Add(item.Key, info);
                }
            }
        }
    }

    public class BcfServerInfo
    {
        private string _DllName;
        private string _ClassName;
        public BcfServerInfo(string dllName, string className)
        {
            DllName = dllName;
            ClassName = className;
        }
        public string ClassName
        {
            get { return _ClassName; }
            set { _ClassName = value; }
        }

        public string DllName
        {
            get { return _DllName; }
            set { _DllName = value; }
        }
    }
}
