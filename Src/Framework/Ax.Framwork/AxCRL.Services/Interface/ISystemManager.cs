using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

namespace AxCRL.Services
{
    [ServiceContract]
    public interface ISystemManager
    {
        [OperationContract]
        [WebInvoke(UriTemplate = "upgrade")]
        void SystemUpgrade();
        [OperationContract]
        [WebInvoke(UriTemplate = "openScheduleTask")]
        void OpenScheduleTask();
    }
}
