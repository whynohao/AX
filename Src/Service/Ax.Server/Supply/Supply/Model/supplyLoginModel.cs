using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Ax.Server.Supply.Model
{
    [Serializable]
    public class supplyLoginModel
    {
        string personId = string.Empty;

        public string PersonId
        {
            get { return personId; }
            set { personId = value; }
        }
        bool loginSuccess = false;

        public bool LoginSuccess
        {
            get { return loginSuccess; }
            set { loginSuccess = value; }
        }
    }
}