using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCRL.Comm.Define
{
    /// <summary>
    /// 功能标识
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ProgIdAttribute : Attribute
    {
        private string _ProgId;
        private ProgIdType _ProgIdType;
        private string _VclPath;
        private string _ViewPath;
        private string _ExtendVclPath;

        public string ViewPath
        {
            get { return _ViewPath; }
            set { _ViewPath = value; }
        }

        public string ExtendVclPath
        {
            get { return _ExtendVclPath; }
            set { _ExtendVclPath = value; }
        }

        public string VclPath
        {
            get { return _VclPath; }
            set { _VclPath = value; }
        }

        public string VclClass
        {
            get { return string.Format("{0}Vcl", this.ProgId.Replace(".", string.Empty)); }
        }

        public string ViewClass
        {
            get { return string.Format("{0}View", this.ProgId.Replace(".", string.Empty)); }
        }

        public string ProgId
        {
            get { return _ProgId; }
            set { _ProgId = value; }
        }

        public ProgIdType ProgIdType
        {
            get { return _ProgIdType; }
            set { _ProgIdType = value; }
        }

        public ProgIdAttribute()
        {

        }

        public ProgIdAttribute(string progId, ProgIdType progIdType, string vclPath, string viewPath)
        {
            this._ProgId = progId;
            this._ProgIdType = progIdType;
            this._VclPath = vclPath;
            this._ViewPath = viewPath;
        }

    }

    /// <summary>
    /// 功能标识类别
    /// </summary>
    public enum ProgIdType
    {
        /// <summary>
        /// 中间层
        /// </summary>
        Bcf = 0,
    }
}
