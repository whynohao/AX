using AxCRL.Comm.Runtime;
using AxCRL.Comm.Utils;
using AxCRL.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace AxCRL.Core.Mail
{
    public static class LibMailHelper
    {
        public static void SendMail(object paramObj)
        {
            List<LibMailParam> list = (List<LibMailParam>)paramObj;
            foreach (var item in list)
            {
                SendMailCore(item);
            }
        }

        private static void SendMailCore(object paramObj)
        {
            LibMailParam param = (LibMailParam)paramObj as LibMailParam;
            try
            {
                string host = EnvProvider.Default.MailProvider.Host;
                if (string.IsNullOrEmpty(host))
                    return;
                //确定smtp服务器地址。实例化一个Smtp客户端
                System.Net.Mail.SmtpClient client = new System.Net.Mail.SmtpClient(host);
                Dictionary<string, string[]> addressDic = GetMailAddress(param.PersonId, param.To, param.CC);
                //string[] sendInfo = addressDic[param.PersonId];
                //构造一个Email的Message对象
                MailMessage message = new MailMessage();
                //message.From = new MailAddress(sendInfo[1], sendInfo[0], Encoding.UTF8);
                message.From = new MailAddress(EnvProvider.Default.MailProvider.MailSys, "", Encoding.UTF8);
                foreach (var item in param.To)
                {
                    if (string.IsNullOrEmpty(item))
                        continue;
                    if (item.CompareTo("System") == 0)
                        continue;
                    string[] info = addressDic[item];
                    if (string.IsNullOrEmpty(info[1]))
                        continue;
                    message.To.Add(new MailAddress(info[1], info[0], Encoding.UTF8));
                }
                foreach (var item in param.CC)
                {
                    if (item.CompareTo("System") == 0)
                        continue;
                    string[] info = addressDic[item];
                    if (string.IsNullOrEmpty(info[1]))
                        continue;
                    message.CC.Add(new MailAddress(info[1], info[0], Encoding.UTF8));
                }
                if (message.To.Count == 0)
                    return;
                //为 message 添加附件
                foreach (string fileName in param.AttachmentList)
                {
                    //判断文件是否存在
                    string fileFullPath = Path.Combine(EnvProvider.Default.MainPath, "Resource", "Public", "Mail", "Attachment", fileName);
                    if (File.Exists(fileFullPath))
                    {
                        //构造一个附件对象
                        Attachment attach = new Attachment(fileFullPath);
                        //得到文件的信息
                        ContentDisposition disposition = attach.ContentDisposition;
                        disposition.CreationDate = System.IO.File.GetCreationTime(fileFullPath);
                        disposition.ModificationDate = System.IO.File.GetLastWriteTime(fileFullPath);
                        disposition.ReadDate = System.IO.File.GetLastAccessTime(fileFullPath);
                        //向邮件添加附件
                        message.Attachments.Add(attach);
                    }
                }
                //添加邮件主题和内容
                message.Subject = param.Subject;
                message.SubjectEncoding = Encoding.UTF8;
                //设置邮件的信息
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                message.BodyEncoding = System.Text.Encoding.UTF8;
                message.IsBodyHtml = true;
                message.Body = GetMailBody(param, addressDic);
                //如果服务器支持安全连接，则将安全连接设为true。
                //gmail支持，163不支持，如果是gmail则一定要将其设为true
                client.EnableSsl = false;
                //if (cmbBoxSMTP.SelectedText == "smpt.163.com")
                //    client.EnableSsl = false;
                //else
                //    client.EnableSsl = true;
                //设置用户名和密码。
                //string userState = message.Subject;
                client.UseDefaultCredentials = false;
                string username = EnvProvider.Default.MailProvider.MailSys;
                string passwd = EnvProvider.Default.MailProvider.MailPwd;
                //用户登陆信息
                NetworkCredential myCredentials = new NetworkCredential(username, passwd);
                client.Credentials = myCredentials;
                //发送邮件
                client.Send(message);
            }
            catch
            {
                //throw;
            }
        }

        public static Dictionary<string, string[]> GetMailAddress(string send, IList<string> to, IList<string> cc)
        {
            Dictionary<string, string[]> dic = new Dictionary<string, string[]>();
            StringBuilder builder = new StringBuilder();
            if (!string.IsNullOrEmpty(send))
                builder.AppendFormat("PERSONID={0} OR ", LibStringBuilder.GetQuotString(send));
            foreach (string item in to)
            {
                builder.AppendFormat("PERSONID={0} OR ", LibStringBuilder.GetQuotString(item));
            }
            foreach (string item in cc)
            {
                builder.AppendFormat("PERSONID={0} OR ", LibStringBuilder.GetQuotString(item));
            }
            if (builder.Length > 0)
            {
                builder.Remove(builder.Length - 3, 3);
                string sql = string.Format("select PERSONID,PERSONNAME,MAIL from COMPERSON where {0}", builder.ToString());
                LibDataAccess dataAccess = new LibDataAccess();
                using (IDataReader reader = dataAccess.ExecuteDataReader(sql))
                {
                    while (reader.Read())
                    {
                        string personId = LibSysUtils.ToString(reader[0]);
                        string personName = LibSysUtils.ToString(reader[1]);
                        string mail = LibSysUtils.ToString(reader[2]);
                        if (!dic.ContainsKey(personId))
                            dic[personId] = new string[] { personName, mail };
                    }
                }
            }
            return dic;
        }

        private static string GetMailBody(LibMailParam param, Dictionary<string, string[]> addressDic)
        {
            string html = string.Empty;
            LibMailTpl mailTpl = null;
            switch (param.MailKind)
            {
                case LibMailKind.Info:
                    mailTpl = new LibInfoMailTpl();
                    break;
                case LibMailKind.Approve:
                    mailTpl = new LibApproveMailTpl();
                    break;
                case LibMailKind.Warning:
                    mailTpl = new LibWarningMailTpl();
                    break;
                case LibMailKind.Problem:
                    mailTpl = new LibProblemMailTpl();
                    break;
            }
            html = mailTpl.GetMailBody(param, addressDic);
            return html;
        }
    }

    public class LibMailParam
    {
        private LibMailKind _MailKind = LibMailKind.Info;
        private List<string> _To;
        private List<string> _CC;
        private string _Subject;
        private string _Content;
        private string _PersonId;
        private string _ProgId;
        private string _BillNo;
        private IList<string> _AttachmentList;
        private IDictionary<string, string> _ExpandData;


        public string BillNo
        {
            get { return _BillNo; }
            set { _BillNo = value; }
        }

        public IList<string> AttachmentList
        {
            get
            {
                if (_AttachmentList == null)
                    _AttachmentList = new List<string>();
                return _AttachmentList;
            }
        }

        public IDictionary<string, string> ExpandData
        {
            get
            {
                if (_ExpandData == null)
                    _ExpandData = new Dictionary<string, string>();
                return _ExpandData;
            }
        }

        public string ProgId
        {
            get { return _ProgId; }
            set { _ProgId = value; }
        }

        public string Content
        {
            get { return _Content; }
            set { _Content = value; }
        }

        public string Subject
        {
            get { return _Subject; }
            set { _Subject = value; }
        }

        public LibMailKind MailKind
        {
            get { return _MailKind; }
            set { _MailKind = value; }
        }

        public List<string> To
        {
            get
            {
                if (_To == null)
                    _To = new List<string>();
                return _To;
            }
            set { _To = value; }
        }

        public List<string> CC
        {
            get
            {
                if (_CC == null)
                    _CC = new List<string>();
                return _CC;
            }
        }

        public string PersonId
        {
            get { return _PersonId; }
            set { _PersonId = value; }
        }
    }

    public enum LibMailKind
    {
        Info = 0,
        Approve = 1,
        Warning = 2,
        Problem = 3
    }

    public abstract class LibMailTpl
    {
        public abstract string GetMailBody(LibMailParam param, Dictionary<string, string[]> addressDic);
    }

    public class LibApproveMailTpl : LibMailTpl
    {
        public override string GetMailBody(LibMailParam param, Dictionary<string, string[]> addressDic)
        {
            string html = string.Empty;
            string path = Path.Combine(EnvProvider.Default.MainPath, "Resource", "MailTpl", "ApproveMailTpl.html");
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                using (StreamReader reader = new StreamReader(fs, Encoding.Default))
                {
                    html = reader.ReadToEnd();
                }
            }
            html = html.Replace("@USER", addressDic[param.PersonId][0]);
            html = html.Replace("@DATE", DateTime.Now.ToShortDateString());
            html = html.Replace("@TEXT", param.Content);
            //if (param.ExpandData.ContainsKey("@IMG"))
            //{
            //    string base64 = param.ExpandData["@IMG"];
            //    string imageName = string.Format("{0}-{1}.png", LibDateUtils.GetCurrentDateTime(), LibCommUtils.GetInternalId());
            //    string imgUrl = Path.Combine(".", "PublicData", "Mail", "Image", imageName);
            //    using (FileStream fs = new FileStream(imgUrl, FileMode.Create))
            //    {
            //        byte[] imageBytes = Convert.FromBase64String(base64);
            //        fs.Write(imageBytes, 0, imageBytes.Length);
            //    }
            //    html.Replace("@IMG", imgUrl);
            //}
            string progId = param.ProgId.Replace('.', '_');
            html = html.Replace("@LINK", string.Format("http://{0}:{1}/desk/{2}/{3}", EnvProvider.Default.LocalHostName, EnvProvider.Default.CurrentPort, progId, param.BillNo));
            return html;
        }
    }

    public class LibInfoMailTpl : LibMailTpl
    {
        public override string GetMailBody(LibMailParam param, Dictionary<string, string[]> addressDic)
        {
            string html = string.Empty;
            string path = Path.Combine(EnvProvider.Default.MainPath, "Resource", "MailTpl", "InfoMailTpl.html");
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                using (StreamReader reader = new StreamReader(fs, Encoding.Default))
                {
                    html = reader.ReadToEnd();
                }
            }
            html = html.Replace("@USER", string.IsNullOrEmpty(param.PersonId) ? string.Empty : addressDic[param.PersonId][0]);
            html = html.Replace("@DATE", DateTime.Now.ToShortDateString());
            html = html.Replace("@TEXT", param.Content);
            html = html.Replace("@LINK", string.Format("http://{0}:{1}", EnvProvider.Default.LocalHostName, EnvProvider.Default.CurrentPort));
            return html;
        }
    }

    public class LibWarningMailTpl : LibMailTpl
    {
        public override string GetMailBody(LibMailParam param, Dictionary<string, string[]> addressDic)
        {
            throw new NotImplementedException();
        }
    }

    public class LibProblemMailTpl : LibMailTpl
    {
        public override string GetMailBody(LibMailParam param, Dictionary<string, string[]> addressDic)
        {
            throw new NotImplementedException();
        }
    }
}
