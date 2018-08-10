using AxCRL.Comm.Bill;
using AxCRL.Comm.Entity;
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
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;

namespace Ax.Ui.Ws
{
    public class SysHandler : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            string action = context.Request["action"].ToLower();
            object ret = null;

            try
            {
                switch (action)
                {
                    case "checkfieldvalue":
                        ret = checkFieldValue(context);
                        break;
                    case "fuzzysearchfield":
                        ret = fuzzySearchField(context);
                        break;
                    case "savemenusetting":
                        ret = SaveMenuSetting(context);
                        break;
                    case "loadmenusetting":
                        ret = LoadMenuSetting(context);
                        break;
                    case "getrptfields":
                        ret = GetRptFields(context);
                        break;
                    case "getjspath":
                        ret = GetJsPath(context);
                        break;
                    case "savebilllistingscheme":
                        ret = SaveBillListingScheme(context);
                        break;
                    case "print":
                        ret = Print(context);
                        break;
                    case "selectqueryfield":
                        ret = SelectQueryField(context);
                        break;
                    case "selectfuncfield":
                        ret = SelectFuncField(context);
                        break;
                    case "uploadfile":
                        ret = UpLoadFile(context);
                        break;
                    case "batchexportalldata":
                        ret = BatchExportAllData(context);
                        break;
                    case "batchexport":
                        ret = BatchExportData(context);
                        break;
                    case "batchimport":
                        ret = BatchImportData(context);
                        break;
                    case "getlist":
                        ret = getList(context);
                        break;
                    case "batchmethod":
                        ret = BatchMethod(context);
                        break;
                    case "method":
                        ret = processRequsetExecuteMethod(context);
                        break;
                    case "login":
                        ret = Login(context);
                        break;
                    case "weixin":
                        ret = GetDiary(context);
                        break;
                    case "update":
                        SystemManager info = new SystemManager();
                        info.SystemUpgrade1();
                        break;
                    default:
                        throw new Exception("没有对应的方法");
                        break;
                }
            }
            catch (Exception ex)
            {
                LibLog.WriteLog(ex);
                ret = new ExecuteBcfMethodResult(ex.Message);
            }

            string json = JsonConvert.SerializeObject(ret);
            context.Response.Write(json);
            context.Response.End();
        }

        #region checkFieldValue
        public object checkFieldValue(HttpContext context)
        {
            ExecuteBcfMethodResult result = new ExecuteBcfMethodResult();

            string data = context.Request["data"];
            MDictionary<string, string> dic = new MDictionary<string, string>();
            if (!string.IsNullOrEmpty(data))
            {
                dic = JsonConvert.DeserializeObject<MDictionary<string, string>>(data);
            }

            BillService service = new BillService();
            string handle = dic["Handle"];
            string fields = dic["Fields"];
            string relSource = dic["RelSource"];
            string curPk = dic["CurPk"];
            string condition = dic["Condition"];
            Int16 tableIndex = LibSysUtils.ToInt16(dic["TableIndex"]);
            result.Result = service.CheckFieldValue(handle, fields, relSource, curPk, condition, tableIndex);
            return result;
        }
        #endregion

        #region fuzzySearchField
        public object fuzzySearchField(HttpContext context)
        {
            ExecuteBcfMethodResult result = new ExecuteBcfMethodResult();

            string data = context.Request["data"];
            MDictionary<string, string> dic = new MDictionary<string, string>();
            if (!string.IsNullOrEmpty(data))
            {
                dic = JsonConvert.DeserializeObject<MDictionary<string, string>>(data);
            }

            BillService service = new BillService();
            string handle = dic["Handle"];
            string relSource = dic["RelSource"];
            string query = dic["Query"];
            string condition = dic["Condition"];
            Int16 tableIndex = LibSysUtils.ToInt16(dic["TableIndex"]);
            string selectSql = dic["SelectSql"];
            string selectFields = dic["SelectFields"];
            result.Result = service.FuzzySearchField1(handle, relSource, selectFields, query, condition, tableIndex, selectSql);
            //result.Result = service.FuzzySearchField(handle, relSource, query, condition, tableIndex);
            return result;
        }
        #endregion

        #region 菜单
        //保存菜单
        public object SaveMenuSetting(HttpContext context)
        {
            ExecuteBcfMethodResult result = new ExecuteBcfMethodResult();

            string data = context.Request["data"];
            MDictionary<string, string> dic = new MDictionary<string, string>();
            if (!string.IsNullOrEmpty(data))
            {
                dic = JsonConvert.DeserializeObject<MDictionary<string, string>>(data);
            }

            FileTransferService service = new FileTransferService();
            string handle = dic["Handle"];
            string menuData = dic["MenuData"];
            service.SaveMenuSetting(handle, menuData);
            return result;
        }

        //加载菜单
        public object LoadMenuSetting(HttpContext context)
        {
            ExecuteBcfMethodResult result = new ExecuteBcfMethodResult();

            string data = context.Request["data"];
            MDictionary<string, string> dic = new MDictionary<string, string>();
            if (!string.IsNullOrEmpty(data))
            {
                dic = JsonConvert.DeserializeObject<MDictionary<string, string>>(data);
            }

            FileTransferService service = new FileTransferService();
            string handle = dic["Handle"];
            result.Result = service.LoadMenuSetting(handle);
            return result;
        }
        #endregion

        #region GetRptFields
        public object GetRptFields(HttpContext context)
        {
            ExecuteBcfMethodResult result = new ExecuteBcfMethodResult();

            string data = context.Request["data"];
            MDictionary<string, string> dic = new MDictionary<string, string>();
            if (!string.IsNullOrEmpty(data))
            {
                dic = JsonConvert.DeserializeObject<MDictionary<string, string>>(data);
            }

            BillService service = new BillService();
            string progId = dic["Progid"];
            result.Result = service.GetRptFields(progId);
            return result;
        }
        #endregion

        #region 加载Js
        public object GetJsPath(HttpContext context)
        {
            ExecuteBcfMethodResult result = new ExecuteBcfMethodResult();
            SystemService service = new SystemService();
            result.Result = service.GetJsPath();
            return result;
        }
        #endregion

        #region 显示方案
        public object SaveBillListingScheme(HttpContext context)
        {
            string data = context.Request["data"];
            MDictionary<string, string> dic = new MDictionary<string, string>();
            if (!string.IsNullOrEmpty(data))
            {
                dic = JsonConvert.DeserializeObject<MDictionary<string, string>>(data);
            }

            BillService service = new BillService();
            return service.SaveDisplayScheme1(dic["Handle"], dic["ProgId"], dic["EntryParam"], dic["DisplayScheme"]);
        }
        #endregion

        #region 打印
        public object Print(HttpContext context)
        {
            string data = context.Request["data"];
            ExecuteBcfMethodParam param = JsonConvert.DeserializeObject<ExecuteBcfMethodParam>(data);

            BillService service = new BillService();
            return service.Print(param);
        }
        #endregion

        #region 实体清单查询
        public object SelectQueryField(HttpContext context)
        {
            BillService service = new BillService();

            string data = context.Request["data"];

            BillListingQuery listingQuery = JsonConvert.DeserializeObject<BillListingQuery>(data);
            return service.SelectQueryField1(listingQuery.Handle, listingQuery.ProgId);
        }
        #endregion

        #region 关联实体清单
        public object SelectFuncField(HttpContext context)
        {
            string data = context.Request["data"];
            MDictionary<string, string> dic = new MDictionary<string, string>();
            if (!string.IsNullOrEmpty(data))
            {
                dic = JsonConvert.DeserializeObject<MDictionary<string, string>>(data);
            }

            string handle = dic["handle"];
            string progId = dic["progId"];
            int tableIndex = LibSysUtils.ToInt16(dic["tableIndex"]);

            BillService service = new BillService();
            return service.SelectFuncField1(handle, progId, tableIndex);
        }
        #endregion

        #region 上传
        public object UpLoadFile(HttpContext context)
        {
            string data = context.Request["data"];
            MDictionary<string, string> dic = new MDictionary<string, string>();
            if (!string.IsNullOrEmpty(data))
            {
                dic = JsonConvert.DeserializeObject<MDictionary<string, string>>(data);
            }
            string progId = dic["ProgId"];

            HttpFileCollection postedFile = context.Request.Files;

            FileTransferService service = new FileTransferService();
            for (int i = 0; i < postedFile.Count; i++)
            {
                UpLoadFileResult ret = service.UpLoadFile1(postedFile[i], progId);
                return new { uploaded = 1, fileName = ret.FileName, url = ret.FileName, };
            }

            return null;
        }
        #endregion

        #region 导入
        public object BatchImportData(HttpContext context)
        {
            string data = context.Request["data"];
            MDictionary<string, string> dic = new MDictionary<string, string>();
            if (!string.IsNullOrEmpty(data))
            {
                dic = JsonConvert.DeserializeObject<MDictionary<string, string>>(data);
            }

            string handle = dic["handle"];
            string progId = dic["progId"];
            string fileName = dic["fileName"];

            BillService service = new BillService();
            return service.BatchImportData1(handle, progId, fileName);
        }
        #endregion

        #region 导出
        public object BatchExportData(HttpContext context)
        {
            string data = context.Request["data"];
            //MDictionary<string, string> dic = new MDictionary<string, string>();
            //if (!string.IsNullOrEmpty(data))
            //{
            //    dic = JsonConvert.DeserializeObject<MDictionary<string, string>>(data);
            //}

            //string handle = dic["handle"];
            //string progId = dic["progId"];
            //List<object[]> batchParams = new List<object[]>() { new object[] { "RJ180527011" } };

            ExecuteBcfMethodParam param = JsonConvert.DeserializeObject<ExecuteBcfMethodParam>(data);

            BillService service = new BillService();
            return service.BatchExportData1(param.Handle, param.ProgId, param.MethodParam);
        }

        public object BatchExportAllData(HttpContext context)
        {
            string data = context.Request["data"];
            //MDictionary<string, string> dic = new MDictionary<string, string>();
            //if (!string.IsNullOrEmpty(data))
            //{
            //    dic = JsonConvert.DeserializeObject<MDictionary<string, string>>(data);
            //}

            //string handle = dic["handle"];
            //string progId = dic["progId"];
            //List<object[]> batchParams = new List<object[]>() { new object[] { "RJ180527011" } };

            ExecuteBcfMethodParam param = JsonConvert.DeserializeObject<ExecuteBcfMethodParam>(data);

            BillService service = new BillService();
            return service.ExportAllData1(param.Handle, param.ProgId, "");
        }
        #endregion

        #region 获取实体列表
        private object getList(HttpContext context)
        {
            BillService service = new BillService();

            string data = context.Request["data"];

            BillListingQuery listingQuery = JsonConvert.DeserializeObject<BillListingQuery>(data);
            return service.GetBillListing1(listingQuery);
        }
        #endregion

        #region 执行批量方法
        private object BatchMethod(HttpContext context)
        {
            string data = context.Request["data"];

            BillService service = new BillService();

            ExecuteBcfMethodParam param = JsonConvert.DeserializeObject<ExecuteBcfMethodParam>(data);
            return service.BatchExecBcfMethod1(param);
        }
        #endregion

        #region 实体方法执行
        private object processRequsetExecuteMethod(HttpContext context)
        {
            string data = context.Request["data"];

            //MDictionary<string, string> meta = new MDictionary<string, string>();
            //if (!string.IsNullOrEmpty(data))
            //{
            //    meta = JsonConvert.DeserializeObject<MDictionary<string, string>>(data);
            //}
            BillService service = new BillService();

            ExecuteBcfMethodParam param = JsonConvert.DeserializeObject<ExecuteBcfMethodParam>(data);
            //return service.ExecuteMethod(ns, cn, ay, method, meta);

            //ExecuteBcfMethodParam param = new ExecuteBcfMethodParam()
            //{
            //    ProgId = context.Request.Form["ProgId"],
            //    MethodName = context.Request.Form["MethodName"],
            //    MethodParam = JsonConvert.DeserializeObject<string[]>(context.Request.Form["MethodParam"])
            //};
            return service.ExecuteBcfMethod1(param);
        }
        #endregion

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

            string sql = string.Format(" SELECT * FROM Diary ORDER BY CREATETIME DESC ");
            LibDataAccess dataAccess = new LibDataAccess();
            DataSet dataSet = dataAccess.ExecuteDataSet(sql);

            int index = 1;
            foreach (DataRow dr in dataSet.Tables[0].Rows)
            {
                #region 本地案例
                //                Diary info = new Diary()
                //                {
                //                    meta = new DiaryMeta()
                //                    {
                //                        cover = "https://thumbnail0.baidupcs.com/thumbnail/0b95855351dc3e3da0ab41d2b71e00aa?fid=537037902-250528-293683274725876&time=1524326400&rt=sh&sign=FDTAER-DCb740ccc5511e5e8fedcff06b081203-hWFVzqB0U910msWMBvvDYwar3e4%3D&expires=8h&chkv=0&chkbd=0&chkpc=&dp-logid=2574598147729486112&dp-callid=0&size=c710_u400&quality=100&vuk=-&ft=video",
                //                        avatar = "http://i4.bvimg.com/639864/d7ec6328ddb04555.jpg",
                //                        title = "愿你不是至尊宝",
                //                        meta = "2018.4.21",
                //                        create_time = "2018.4.21 23:30:00",
                //                        nickName = "时光"
                //                    },
                //                    list = new List<DiaryContent>()
                //                                                            {
                //                                                                new DiaryContent()
                //                                                                {
                //                                                                    type="TEXT",
                //                                                                    content=@"相遇相离，人间至憾，莫过于此。
                //                                                                            “我的意中人是个盖世英雄，我知道有一天他会在一个万众瞩目的情况下出现，身披金甲圣衣，脚踏七色云彩来娶我，我猜中了前头，可是我猜不着这结局。”一行泪下，紫霞仙子看着戴上金箍变成齐天大圣的至尊宝说着。那是她的意中人，那也不是她的意中人。五百年前，他是那个花言巧语，苟且无为，拔出紫青宝剑，心里却想着晶晶姑娘的至尊宝。戴上金箍，他是那个遁入空门，神通广大，决心西去取经，心里却留着仙子泪滴的孙悟空。他注定是紫霞的意中人，却又注定和紫霞擦肩而过。
                //                                                                            曾经看《大话西游》不觉其中深意，看过几篇精彩的影评后，初觉真味。可后来几次重温，每次都有不同的体会。无论是再精彩的妙语点评，多么深刻的挖掘揣摩，都觉得白璧微瑕。影评终归是他人的感受，只有自己心中的震撼才是最标准的答案。
                //                                                                            其实，至尊宝和紫霞的剧本，是我们每个人的爱情故事。我们曾经可能都做过谎话连篇时的至尊宝，或者一往情深时的紫霞仙子。
                //                                                                            时常会想，如果我是至尊宝，我会怎么选，我遇到的哪一位姑娘，是我的紫霞。然而残忍的是，至尊宝他，没得选。不戴金箍，他救不了紫霞，戴上金箍，他爱不了她。这金箍，他不得不带，这西天路，他不得不走。
                //                                                                            有得选的，是紫霞。单纯，专情，敢爱敢恨，她是这一切美好的化身，可又因这残忍的美好，深深伤了自己的心。如果她不曾抢走月光宝盒，如果她不曾给至尊宝点三颗痣，如果她不曾有过紫青宝剑。就算这些都是命运不可避免，那么请她，不要轻易爱上一个人，即使他拔出了紫青宝剑。这把剑看似锋利，挡了刀枪斩了水火，遇他之后，再难伤人，竟挡不住几句谎言。
                //                                                                            如果你是女子，愿你是紫霞，也愿你不是紫霞。愿你保留纯真，懂得爱人，没有软肋。
                //                                                                            如果你是男生，愿你不是至尊宝。
                //                                                                            愿你不是那个不知道想要什么的至尊宝，愿你不是那个不知道最爱谁的至尊宝，愿你不是那个后知后觉至尊宝。
                //                                                                            有人说，爱过就好。真的不是爱过就好，要知道，失去以后，佳人难再得!",
                //                                                                    poi=new DiaryPoi()
                //                                                                    {
                //                                                                        longitude="117.2",
                //                                                                        latitude="37.5",
                //                                                                        name="广州",
                //                                                                    },
                //                                                                    description="",
                //                                                                    id=1,
                //                                                                    commentNum=0,
                //                                                                    likeNum= 0,
                //                                                                }
                //                                                            }
                //                };
                #endregion

                #region 数据库案例
                Diary info = new Diary()
                {
                    meta = new DiaryMeta()
                    {
                        cover = string.Format("http://kantime.cn/UserFiles/SysUser/image/{0}.jpg", index++),
                        avatar = "http://kantime.cn/UserFiles/SysUser/image/0.jpg",
                        title = LibSysUtils.ToString(dr["Title"]),
                        meta = "2018.4.21",
                        create_time = "2018.4.21 23:30:00",
                        nickName = "时光"
                    },
                    list = new List<DiaryContent>()
                    {
                        new DiaryContent()
                        {
                            type="TEXT",
                            content=HttpUtility.HtmlDecode(LibSysUtils.ToString(dr["Content"])),
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

                info.list[0].content = CleanHtml(info.list[0].content);
                #endregion
                list.Add(info);
            }

            return list;
        }

        public static string CleanHtml(string strHtml)
        {
            if (string.IsNullOrEmpty(strHtml)) return strHtml;
            //删除脚本
            //Regex.Replace(strHtml, @"<script[^>]*?>.*?</script>", "", RegexOptions.IgnoreCase)
            strHtml = Regex.Replace(strHtml, @"(\<script(.+?)\</script\>)|(\<style(.+?)\</style\>)", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            //删除标签
            var r = new Regex(@"</?[^>]*>", RegexOptions.IgnoreCase);
            Match m;
            for (m = r.Match(strHtml); m.Success; m = m.NextMatch())
            {
                strHtml = strHtml.Replace(m.Groups[0].ToString(), "");
            }
            return strHtml.Trim();
        }

        #endregion

        #region 登录
        public object Login(HttpContext context)
        {
            ExecuteBcfMethodResult result = new ExecuteBcfMethodResult();

            string data = context.Request["data"];
            MDictionary<string, string> dic = new MDictionary<string, string>();
            if (!string.IsNullOrEmpty(data))
            {
                dic = JsonConvert.DeserializeObject<MDictionary<string, string>>(data);
            }

            string userId = dic["userId"];
            string password = dic["password"];
            bool quitOther = false;

            SystemService service = new SystemService();
            result.Result = service.Login1(userId, password, quitOther);
            return result;
        }

        #endregion

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}