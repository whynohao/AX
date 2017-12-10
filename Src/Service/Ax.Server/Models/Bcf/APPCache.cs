using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AxCRL.Bcf;
using AxCRL.Comm.Runtime;
using AxCRL.Comm.Service;
using AxCRL.Comm.Utils;
using AxCRL.Core.Cache;
using AxCRL.Core.Comm;
using AxCRL.Core.SysNews;
using AxCRL.Data;
using AxCRL.Services;
namespace Ax.Server.Models.Bcf
{
    public class APPCache
    {
        public static Dictionary<string, string> CacheDic = new Dictionary<string, string>();
        public static string SetAPPCache(string appCode)
        {
            string codeId = LibSysUtils.ToString(Guid.NewGuid());
            if (!string.IsNullOrEmpty(appCode))
            {
                CacheDic.Add(codeId, appCode);
            }
            return codeId;
        }

        public static void RemoveAPPCache(string codeId)
        {
            if (!string.IsNullOrEmpty(codeId))
            {
                if (CacheDic.ContainsKey(codeId))
                {
                    CacheDic.Remove(codeId);
                }
            }
        }
    }
}