using AxCRL.Comm.Utils;
using AxCRL.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using AxCRL.Bcf.Sfl;
using System.Text.RegularExpressions;
using AxCRL.Core.Cache;
using AxCRL.Bcf;

namespace MES_Sys.UtilsBcf.Com
{
    public class SysCommon
    {
        public static void DelBcf(string progId, List<string> list, LibDataAccess dataAccess, LibManagerMessage manager = null)
        {
            if (manager == null)
            {
                manager = new LibManagerMessage();
            }
            try
            {
                foreach (var item in list)
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        LibBcfData bcfData = (LibBcfData)LibBcfSystem.Default.GetBcfInstance(progId);
                        bcfData.DataAccess = dataAccess;
                        DataSet ds = bcfData.BrowseTo(new object[] { item });

                        if (ds == null || ds.Tables.Count <= 0 || ds.Tables[0].Rows.Count <= 0)
                        {
                            continue;
                        }
                        ds.Clear();
                        bcfData.Delete(new object[] { item });
                    }
                }
            }
            catch (Exception ex)
            {
                manager.AddMessage(new LibMessage()
                {
                    MessageKind = LibMessageKind.Error,
                    Message = string.Format("系统错误:{0}", ex.Message)
                });
            }
        }
    }
}
