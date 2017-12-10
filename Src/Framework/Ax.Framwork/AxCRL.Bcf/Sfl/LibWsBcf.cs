using AxCRL.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCRL.Bcf.Sfl
{
    public class LibWsBcf : ILibWsBcf
    {
        private LibDataAccess _DataAccess = null;
        private LibManagerMessage _ManagerMessage = null;

        public LibManagerMessage ManagerMessage
        {
            get
            {
                if (_ManagerMessage == null)
                    _ManagerMessage = new LibManagerMessage();
                return _ManagerMessage;
            }
        }
        public LibDataAccess DataAccess
        {
            get
            {
                if (_DataAccess == null)
                    _DataAccess = new LibDataAccess();
                return _DataAccess;
            }
        }
    }
}
