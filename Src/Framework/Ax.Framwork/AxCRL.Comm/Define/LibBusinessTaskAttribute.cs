using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCRL.Comm.Define
{
    /// <summary>
    /// 业务任务
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class LibBusinessTaskAttribute : Attribute
    {
        private string _Name;
        private string _DisplayText;


        public string DisplayText
        {
            get { return _DisplayText; }
            set { _DisplayText = value; }
        }

        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }

        public LibBusinessTaskAttribute()
        {

        }

        public LibBusinessTaskAttribute(string name, string displayText)
        {
            this.Name = name;
            this.DisplayText = displayText;
        }

    }
}
