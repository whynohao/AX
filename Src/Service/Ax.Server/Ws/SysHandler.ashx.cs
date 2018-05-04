using AxCRL.Comm.Utils;
using AxCRL.Core.Cache;
using AxCRL.Core.Comm;
using AxCRL.Data;
using AxCRL.Data.SqlBuilder;
using AxCRL.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;

namespace Ax.Ui.Ws
{
    /// <summary>
    /// SysHandler 的摘要说明
    /// </summary>
    public class SysHandler : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            string action = context.Request["action"].ToLower();
            object ret = null;
            switch (action)
            {
                case "login":
                    ret = Login(context);
                    break;
                case "weixin":
                    ret = GetDiary(context);
                    break;
                case "adddiary":
                    ret = AddDiary(context);
                    break;
                default:
                    break;
            }

            string json = JsonConvert.SerializeObject(ret);
            context.Response.Write(json);
            context.Response.End();
        }

        #region 微信日记
        #region 日记对象
        public class Diary
        {
            public DiaryMeta meta { get; set; }
            public List<DiaryContent> list { get; set; }
        }

        public class DiaryMeta
        {
            public string cover { get; set; }
            public string avatar { get; set; }
            public string title { get; set; }
            public string meta { get; set; }
            public string create_time { get; set; }
            public string nickName { get; set; }
        }

        public class DiaryContent
        {
            public string type { get; set; }
            public string content { get; set; }
            public DiaryPoi poi { get; set; }
            public string description { get; set; }
            public int id { get; set; }
            public int commentNum { get; set; }
            public int likeNum { get; set; }
        }

        public class DiaryPoi
        {
            public string longitude { get; set; }
            public string latitude { get; set; }
            public string name { get; set; }
        }
        #endregion

        public List<Diary> GetDiary(HttpContext context)
        {
            List<Diary> list = new List<Diary>();

            string sql = string.Format(" SELECT * FROM Diary ");
            LibDataAccess dataAccess = new LibDataAccess();
            DataSet dataSet = dataAccess.ExecuteDataSet(sql);

            foreach (DataRow dr in dataSet.Tables[0].Rows)
            {
                #region 本地案例
                Diary info = new Diary()
                {
                    meta = new DiaryMeta()
                    {
                        cover = "https://thumbnail0.baidupcs.com/thumbnail/0b95855351dc3e3da0ab41d2b71e00aa?fid=537037902-250528-293683274725876&time=1524326400&rt=sh&sign=FDTAER-DCb740ccc5511e5e8fedcff06b081203-hWFVzqB0U910msWMBvvDYwar3e4%3D&expires=8h&chkv=0&chkbd=0&chkpc=&dp-logid=2574598147729486112&dp-callid=0&size=c710_u400&quality=100&vuk=-&ft=video",
                        avatar = "http://i4.bvimg.com/639864/d7ec6328ddb04555.jpg",
                        title = "愿你不是至尊宝",
                        meta = "2018.4.21",
                        create_time = "2018.4.21 23:30:00",
                        nickName = "时光"
                    },
                    list = new List<DiaryContent>()
                                                            {
                                                                new DiaryContent()
                                                                {
                                                                    type="TEXT",
                                                                    content=@"相遇相离，人间至憾，莫过于此。
                                                                            “我的意中人是个盖世英雄，我知道有一天他会在一个万众瞩目的情况下出现，身披金甲圣衣，脚踏七色云彩来娶我，我猜中了前头，可是我猜不着这结局。”一行泪下，紫霞仙子看着戴上金箍变成齐天大圣的至尊宝说着。那是她的意中人，那也不是她的意中人。五百年前，他是那个花言巧语，苟且无为，拔出紫青宝剑，心里却想着晶晶姑娘的至尊宝。戴上金箍，他是那个遁入空门，神通广大，决心西去取经，心里却留着仙子泪滴的孙悟空。他注定是紫霞的意中人，却又注定和紫霞擦肩而过。
                                                                            曾经看《大话西游》不觉其中深意，看过几篇精彩的影评后，初觉真味。可后来几次重温，每次都有不同的体会。无论是再精彩的妙语点评，多么深刻的挖掘揣摩，都觉得白璧微瑕。影评终归是他人的感受，只有自己心中的震撼才是最标准的答案。
                                                                            其实，至尊宝和紫霞的剧本，是我们每个人的爱情故事。我们曾经可能都做过谎话连篇时的至尊宝，或者一往情深时的紫霞仙子。
                                                                            时常会想，如果我是至尊宝，我会怎么选，我遇到的哪一位姑娘，是我的紫霞。然而残忍的是，至尊宝他，没得选。不戴金箍，他救不了紫霞，戴上金箍，他爱不了她。这金箍，他不得不带，这西天路，他不得不走。
                                                                            有得选的，是紫霞。单纯，专情，敢爱敢恨，她是这一切美好的化身，可又因这残忍的美好，深深伤了自己的心。如果她不曾抢走月光宝盒，如果她不曾给至尊宝点三颗痣，如果她不曾有过紫青宝剑。就算这些都是命运不可避免，那么请她，不要轻易爱上一个人，即使他拔出了紫青宝剑。这把剑看似锋利，挡了刀枪斩了水火，遇他之后，再难伤人，竟挡不住几句谎言。
                                                                            如果你是女子，愿你是紫霞，也愿你不是紫霞。愿你保留纯真，懂得爱人，没有软肋。
                                                                            如果你是男生，愿你不是至尊宝。
                                                                            愿你不是那个不知道想要什么的至尊宝，愿你不是那个不知道最爱谁的至尊宝，愿你不是那个后知后觉至尊宝。
                                                                            有人说，爱过就好。真的不是爱过就好，要知道，失去以后，佳人难再得!",
                                                                    poi=new DiaryPoi()
                                                                    {
                                                                        longitude="117.2",
                                                                        latitude="37.5",
                                                                        name="广州",
                                                                    },
                                                                    description="",
                                                                    id=1,
                                                                    commentNum=0,
                                                                    likeNum= 0,
                                                                }
                                                            }
                };
                #endregion

                #region 数据库案例
                //Diary info = new Diary()
                //{
                //    meta = new DiaryMeta()
                //    {
                //        cover = "https://thumbnail0.baidupcs.com/thumbnail/0b95855351dc3e3da0ab41d2b71e00aa?fid=537037902-250528-293683274725876&time=1524326400&rt=sh&sign=FDTAER-DCb740ccc5511e5e8fedcff06b081203-hWFVzqB0U910msWMBvvDYwar3e4%3D&expires=8h&chkv=0&chkbd=0&chkpc=&dp-logid=2574598147729486112&dp-callid=0&size=c710_u400&quality=100&vuk=-&ft=video",
                //        avatar = "http://i4.bvimg.com/639864/d7ec6328ddb04555.jpg",
                //        title = LibSysUtils.ToString(dr["Title"]),
                //        meta = "2018.4.21",
                //        create_time = "2018.4.21 23:30:00",
                //        nickName = "时光"
                //    },
                //    list = new List<DiaryContent>()
                //    {
                //        new DiaryContent()
                //        {
                //            type="TEXT",
                //            content=HttpUtility.HtmlDecode(LibSysUtils.ToString(dr["Content"])),
                //            poi=new DiaryPoi()
                //            {
                //                longitude="117.2",
                //                latitude="37.5",
                //                name="广州",
                //            },
                //            description="",
                //            id=1,
                //            commentNum=0,
                //            likeNum= 0,
                //        }
                //    }
                //};

                //info.list[0].content = htmlencode(info.list[0].content);
                #endregion
                list.Add(info);
            }

            return list;
        }

        //写入日记
        public bool AddDiary(HttpContext context)
        {
            string data = context.Request["data"];
            Diary info = JsonConvert.DeserializeObject<Diary>(data);
            info.list[0].content = HttpUtility.HtmlEncode(info.list[0].content);

            #region sql
            string sql = string.Format(@"INSERT  Diary( ID, Title, Content ) VALUES  ({0}, N'{1}',N'{2}' ) ", 2, info.meta.title, info.list[0].content);
            #endregion
            LibDataAccess dataAccess = new LibDataAccess();
            dataAccess.ExecuteNonQuery(sql);
            return false;
        }

        public string htmlencode(string str)
        {
            if (str == null || str == "")
                return "";
            string[] arr = new string[] { "<p>", "</p>", "<br />", "&nbsp;", "&amp;", "&quot;", "&lt;", "&gt;", "&#039;", "&ldquo;", "&rdquo;" };
            foreach (var item in arr)
            {
                str = str.Replace(item, "");
            }

            return str;
        }

        #endregion

        //登录
        public LoginInfo Login(HttpContext context)
        {
            string userId = context.Request["userId"];
            string password = context.Request["password"];
            bool quitOther = false;

            LoginInfo loginInfo = new LoginInfo();
            SqlBuilder builder = new SqlBuilder("axp.User");
            string sql = builder.GetQuerySql(0, "A.PERSONID,A.PERSONNAME,A.ROLEID,A.WALLPAPER,A.WALLPAPERSTRETCH", string.Format("A.USERID={0} And A.USERPASSWORD={1} And A.ISUSE=1", LibStringBuilder.GetQuotString(userId), LibStringBuilder.GetQuotString(password)));
            LibDataAccess dataAccess = new LibDataAccess();
            string roleId = string.Empty;
            bool exists = false;
            using (IDataReader reader = dataAccess.ExecuteDataReader(sql))
            {
                if (reader.Read())
                {
                    loginInfo.PersonId = LibSysUtils.ToString(reader[0]);
                    loginInfo.PersonName = LibSysUtils.ToString(reader[1]);
                    roleId = LibSysUtils.ToString(reader[2]);
                    loginInfo.Wallpaper = LibSysUtils.ToString(reader[3]);
                    loginInfo.Stretch = LibSysUtils.ToBoolean(reader[4]);
                    exists = true;
                }
            }
            if (exists)
            {
                LibHandle handle = LibHandleCache.Default.IsExistsHandle(LibHandeleType.PC, userId);
                if (handle != null)
                {
                    if (quitOther)
                        LibHandleCache.Default.RemoveHandle(handle.Handle);
                    else
                        loginInfo.IsUsed = true;
                }
                if (!loginInfo.IsUsed)
                {
                    long currentCount = LibHandleCache.Default.GetCount();
                    long maxUserCount = (long)LibHandleCache.Default.MaxUserCount;
                    if (maxUserCount != -1 && maxUserCount < currentCount)
                    {
                        loginInfo.IsOverUser = true;
                    }
                    else
                    {
                        //string loginIp = string.Empty;
                        ////Zhangkj20161219 增加LoginIp
                        //System.ServiceModel.OperationContext context = System.ServiceModel.OperationContext.Current;
                        ////对于非WCF的访问context为null
                        //if (context != null)
                        //{
                        //    System.ServiceModel.Channels.MessageProperties properties = context.IncomingMessageProperties;
                        //    System.ServiceModel.Channels.RemoteEndpointMessageProperty endpoint = properties[System.ServiceModel.Channels.RemoteEndpointMessageProperty.Name] as System.ServiceModel.Channels.RemoteEndpointMessageProperty;
                        //    loginIp = endpoint.Address + ":" + endpoint.Port.ToString();
                        //}
                        ////创建新的Handle                        
                        //handle = LibHandleCache.Default.GetHandle(string.Empty, LibHandeleType.PC, userId, loginInfo.PersonId, loginInfo.PersonName, roleId, loginIp);
                        //if (handle != null)
                        //{
                        //    loginInfo.Handle = handle.Handle;
                        //}
                    }
                }

            }
            return loginInfo;
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}