using AxCRL.Comm.Utils;
using AxCRL.Core.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCRL.Core.Comm
{
    public static class LibFormater
    {
        public static decimal GetFormater(decimal value, string unitId, LibFormatType formatType = LibFormatType.Quantity)
        {
            //目前只实现数量的格式化
            int retainDigits = LibSysUtils.ToInt32(LibFormatUnitCache.Default.GetFormatData(unitId));
            return decimal.Round(value, retainDigits);
        }
    }

    public enum LibFormatType
    {
        Quantity = 0,
    }
}
