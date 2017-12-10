using AxCRL.Comm.Utils;
using AxCRL.Data;
using AxCRL.Data.SqlBuilder;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCRL.Core.SysNews
{
    public static class LibSysNewsHelper
    {
        public static void SendNews(object paramObj)
        {
            List<LibSysNews> list = (List<LibSysNews>)paramObj;
            SendNews(list);
        }

        public static void SendNews(LibSysNews news, bool needUserInfo = true)
        {
            LibDataAccess dataAccess = new LibDataAccess();
            List<string> userInfo = needUserInfo ? GetUserInfo(dataAccess, news.UserList) : news.UserList;
            string execTaskDataId = SaveTaskResult(dataAccess, news.ProgId, news.Data);
            if (news.UserList.Count > 0)
            {
                SaveSysNews(dataAccess, news.Title, news.Content, news.PersonId, execTaskDataId, userInfo);
            }
        }

        public static void SendNews(List<LibSysNews> newsList, bool needUserInfo = true)
        {
            LibDataAccess dataAccess = new LibDataAccess();
            foreach (LibSysNews news in newsList)
            {
                List<string> userInfo = needUserInfo ? GetUserInfo(dataAccess, news.UserList) : news.UserList;
                string execTaskDataId = SaveTaskResult(dataAccess, news.ProgId, news.Data);
                if (news.UserList.Count > 0)
                {
                    SaveSysNews(dataAccess, news.Title, news.Content, news.PersonId, execTaskDataId, userInfo);
                }
            }
        }

        private static List<string> GetUserInfo(LibDataAccess dataAccess, List<string> personList)
        {
            List<string> userList = new List<string>();
            StringBuilder builder = new StringBuilder();
            foreach (string personId in personList)
            {
                builder.AppendFormat("A.PERSONID={0} OR ", LibStringBuilder.GetQuotString(personId));
            }
            if (builder.Length > 0)
            {
                builder.Remove(builder.Length - 3, 3);
                SqlBuilder sqlBuilder = new SqlBuilder("axp.User");
                string sql = sqlBuilder.GetQuerySql(0, "A.USERID", builder.ToString(), string.Empty, string.Empty, true);
                using (IDataReader reader = dataAccess.ExecuteDataReader(sql))
                {
                    while (reader.Read())
                    {
                        userList.Add(LibSysUtils.ToString(reader["USERID"]));
                    }
                }
            }
            return userList;
        }

        private static void SaveSysNews(LibDataAccess dataAccess, string title, string mainContent, string personId, string execTaskDataId, List<string> userList)
        {
            List<string> sqlList = new List<string>();
            foreach (string userId in userList)
            {
                if (string.IsNullOrEmpty(userId))
                    continue;
                string newsId = Guid.NewGuid().ToString();
                sqlList.Add(string.Format("Insert into AXPUSERNEWS(NEWSID,USERID,TITLE,MAINCONTENT,INFOID,CREATETIME,PERSONID,ISREAD) values({0},{1},{2},{3},{4},{5},{6},0)",
                    LibStringBuilder.GetQuotString(newsId), LibStringBuilder.GetQuotString(userId), LibStringBuilder.GetQuotString(title),
                    LibStringBuilder.GetQuotString(mainContent), LibStringBuilder.GetQuotString(execTaskDataId), LibDateUtils.GetCurrentDateTime(), LibStringBuilder.GetQuotString(personId)));
            }
            dataAccess.ExecuteNonQuery(sqlList);
        }

        private static string SaveTaskResult(LibDataAccess dataAccess, string progId, string data)
        {
            string execTaskDataId = Guid.NewGuid().ToString();
            dataAccess.ExecuteStoredProcedure("axpInsertExecTaskData", execTaskDataId, LibDateUtils.GetCurrentDateTime(), progId, data);
            return execTaskDataId;
        }
    }

    public class LibSysNews
    {
        private string _PersonId;
        private string _Data;
        private List<string> _UserList;
        private string _Title;
        private string _Content;
        private string _ProgId;

        public string PersonId
        {
            get { return _PersonId; }
            set { _PersonId = value; }
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

        public string Title
        {
            get { return _Title; }
            set { _Title = value; }
        }

        public List<string> UserList
        {
            get
            {
                if (_UserList == null)
                    _UserList = new List<string>();
                return _UserList;
            }
            set { _UserList = value; }
        }

        public string Data
        {
            get { return _Data; }
            set { _Data = value; }
        }
    }
}
