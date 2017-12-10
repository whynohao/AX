using AxCRL.Comm.Runtime;
using AxCRL.Core;
using AxCRL.Core.Cache;
using AxCRL.Core.Comm;
using AxCRL.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AxCRL.Bcf
{
    public class LibBcfSystem
    {
        private static LibBcfSystem _Default = null;
        private static object _LockObj = new object();

        private LibBcfSystem()
        {

        }
        public static LibBcfSystem Default
        {
            get
            {
                if (_Default == null)
                {
                    lock (_LockObj)
                    {
                        if (_Default == null)
                            _Default = new LibBcfSystem();
                    }
                }
                return _Default;
            }
        }

        public LibBcfBase GetBcfInstance(string progId, LibDataAccess dataAccess = null)
        {
            LibBcfBase destObj = null;
            if (ProgIdHost.Instance.ProgIdRef.ContainsKey(progId))
            {
                BcfServerInfo info = ProgIdHost.Instance.ProgIdRef[progId];
                string path = Path.Combine(EnvProvider.Default.MainPath, "Bcf", info.DllName);
                Assembly assembly = Assembly.LoadFrom(path);
                Type t = assembly.GetType(info.ClassName);
                destObj = (LibBcfBase)t.InvokeMember(null, BindingFlags.CreateInstance, null, null, null);
                destObj.Handle = LibHandleCache.Default.GetSystemHandle();
                if (dataAccess != null)
                    destObj.DataAccess = dataAccess;
                return destObj;
            }
            return destObj;
        }
    }
}
