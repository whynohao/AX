using AxCRL.Core.Comm;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCRL.Bcf
{
    public class LibBcfDailyRpt : LibBcfRptBase
    {
        public virtual DataSet GetDailyData(int currentDate, LibQueryCondition condition)
        {
            return this.DataSet;
        }
    }
}
