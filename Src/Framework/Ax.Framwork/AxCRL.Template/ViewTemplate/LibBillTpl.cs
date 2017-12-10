using AxCRL.Template.Layout;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCRL.Template.ViewTemplate
{
    public class LibBillTpl : LibViewTemplate
    {
        private bool _ShowAuditState = false;

        public bool ShowAuditState
        {
            get { return _ShowAuditState; }
            set { _ShowAuditState = value; }
        }

        public LibBillTpl(DataSet dataSet, IViewLayout layout)
            : base(dataSet, layout)
        {

        }
    }
}
