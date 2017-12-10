using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace AxSRL.SMS.SignalR
{
    public  class ChatConnection : PersistentConnection
    {
        protected override Task OnConnected(IRequest request, string connectionId)
        {
            this.Groups.Add(connectionId,"Ax");
            return Connection.Send(connectionId,string.Empty);
        }

        protected override Task OnReceived(IRequest request, string connectionId, string data)
        {
            return Connection.Broadcast(data);
        }

        public void SendMessage(MessageInfo message)
        {
            var content = GlobalHost.ConnectionManager.GetConnectionContext<ChatConnection>();
            content.Groups.Send("Ax", message);
        }

        
    }
    public class MessageInfo
    {
        public MessageInfo(string personId, string message)
        {
            PersonId = personId;
            Message = message;
        }
        private string _message = string.Empty;

        public string Message
        {
            get { return _message; }
            set { _message = value; }
        }
        private string personId = string.Empty;

        public string PersonId
        {
            get { return personId; }
            set { personId = value; }
        }
    }
}