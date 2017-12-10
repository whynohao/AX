using AxCRL.Bcf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Ax.Server.MES.Model
{
    /// <summary>
    /// 方法返回结果
    /// </summary>
    [Serializable]
    public class MethodResult
    {
        private object _Result = new object();
        private bool _IsSuccess = false;
        private string _Message = string.Empty;

        public string Message
        {
            get { return _Message; }
            set { _Message = value; }
        }

        public bool IsSuccess
        {
            get { return _IsSuccess; }
            set { _IsSuccess = value; }
        }

        public object ResultValue
        {
            get { return _Result; }
            set { _Result = value; }
        }
    }
}