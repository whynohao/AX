using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCRL.Comm.Bill
{
    /// <summary>
    /// 入口参数
    /// </summary>
    public class LibEntryParam
    {
        private Dictionary<string, object> _ParamStore;
        /// <summary>
        /// 参数存储字典
        /// </summary>
        public Dictionary<string, object> ParamStore
        {
            get
            {
                if (_ParamStore == null)
                    _ParamStore = new Dictionary<string, object>();
                return _ParamStore;
            }
            set { _ParamStore = value; }
        }
    }
}
