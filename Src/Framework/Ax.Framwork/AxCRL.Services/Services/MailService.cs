using AxCRL.Comm.Runtime;
using AxCRL.Comm.Utils;
using AxCRL.Core.Cache;
using AxCRL.Core.Comm;
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

namespace AxCRL.Services.Services
{
    public class MailService : IMailService
    {
        public void SendMail(LibMailParam param)
        {
            LibHandle libHandle = LibHandleCache.Default.GetCurrentHandle(param.Handle) as LibHandle;
            if (libHandle == null)
            {
                throw new Exception("用户句柄无效。");
            }
            try
            {
                //确定smtp服务器地址。实例化一个Smtp客户端
                System.Net.Mail.SmtpClient client = new System.Net.Mail.SmtpClient(EnvProvider.Default.MailProvider.Host);
                Dictionary<string, string[]> addressDic = GetMailAddress(param.UserId, param.To, param.CC);
                string[] sendInfo = addressDic[param.UserId];
                //构造一个Email的Message对象
                MailMessage message = new MailMessage();
                message.From = new MailAddress(sendInfo[1], sendInfo[0], Encoding.UTF8);
                foreach (var item in param.To)
                {
                    string[] info = addressDic[item];
                    message.To.Add(new MailAddress(info[1], info[0], Encoding.UTF8));
                }
                foreach (var item in param.CC)
                {
                    string[] info = addressDic[item];
                    message.CC.Add(new MailAddress(info[1], info[0], Encoding.UTF8));
                }
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
                throw;
            }
        }

        public Dictionary<string, string[]> GetMailAddress(string send, IList<string> to, IList<string> cc)
        {
            Dictionary<string, string[]> dic = new Dictionary<string, string[]>();
            StringBuilder builder = new StringBuilder();
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

        private string GetMailBody(LibMailParam param, Dictionary<string, string[]> addressDic)
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
            html.Replace("@BILL", param.ProgName);
            html.Replace("@USER", addressDic[param.UserId][0]);
            html.Replace("@DATE", DateTime.Now.ToShortDateString());
            html.Replace("@TEXT", param.Content);
            if (param.ExpandData.ContainsKey("@IMG"))
            {
                string base64 = param.ExpandData["@IMG"];
                string imageName = string.Format("{0}-{1}.png", LibDateUtils.GetCurrentDateTime(), LibCommUtils.GetInternalId());
                string imgUrl = Path.Combine(".", "PublicData", "Mail", "Image", imageName);
                using (FileStream fs = new FileStream(imgUrl, FileMode.Create))
                {
                    byte[] imageBytes = Convert.FromBase64String(base64);
                    fs.Write(imageBytes, 0, imageBytes.Length);
                }
                html.Replace("@IMG", imgUrl);
            }
            html.Replace("@LINK", "");
            return html;
        }
    }

    public class LibInfoMailTpl : LibMailTpl
    {
        public override string GetMailBody(LibMailParam param, Dictionary<string, string[]> addressDic)
        {
            throw new NotImplementedException();
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
