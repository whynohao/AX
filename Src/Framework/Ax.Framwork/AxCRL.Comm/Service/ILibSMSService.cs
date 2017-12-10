
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCRL.Comm.Service
{
    public interface ILibSMSService
    {
        void SendMsg(SendSMSParam param);
    }

    public class SendSMSParam
    {
        private List<string> _PhoneList;
        private string _Message;

        public string Message
        {
            get { return _Message; }
            set { _Message = value; }
        }

        public List<string> PhoneList
        {
            get
            {
                if (_PhoneList == null)
                    _PhoneList = new List<string>();
                return _PhoneList;
            }
            set
            {
                _PhoneList = value;
            }
        }
    }
}
