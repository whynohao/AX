using AxCRL.Comm.Define;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCRL.Bcf
{
    /// <summary>
    /// 
    /// </summary>
    public interface ILibBcfBase
    {

    }

    public interface ILibLiveUpdate
    {
        [LibBusinessTaskAttribute(Name = "LiveUpdate", DisplayText = "实时更新")]
        DataSet LiveUpdate();
    }
}
