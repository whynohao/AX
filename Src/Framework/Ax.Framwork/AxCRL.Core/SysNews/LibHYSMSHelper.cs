using AxCRL.Comm.Service;
using AxSRL.SMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCRL.Core.SysNews
{
    public static class LibHYSMSHelper
    {
        public static void SendMsg(object destObj)
        {
            ILibSMSService svc = new LibHYSMSService();
            SendSMSParam param = destObj as SendSMSParam;
            if (param != null)
                svc.SendMsg(param);
        }
    }
}
