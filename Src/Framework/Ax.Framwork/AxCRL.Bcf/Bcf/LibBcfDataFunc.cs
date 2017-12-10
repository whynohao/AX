using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace AxCRL.Bcf
{
    public class LibBcfDataFunc : LibBcfDataBase
    {
        public virtual bool NeedAutoMasterRow
        {
            get { return true; }
        }


        public DataSet OpenFunc()
        {
            if (NeedAutoMasterRow)
            {
                DataRow masterRow = this.DataSet.Tables[0].NewRow();
                this.DataSet.Tables[0].Rows.Add(masterRow);
                AfterAddMasterRow(masterRow);
            }
            return this.DataSet;
        }

        protected virtual void AfterAddMasterRow(DataRow masterRow)
        {

        }
    }
}
