using AxCRL.Template.Layout;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCRL.Template.ViewTemplate
{
    public class LibDataFuncTpl : LibViewTemplate
    {
        public LibDataFuncTpl(DataSet dataSet, IViewLayout layout)
            : base(dataSet, layout)
        {

        }
    }
}
