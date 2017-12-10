using AxCRL.Bcf;
using AxCRL.Comm.Define;
using AxCRL.Comm.Enums;
using AxCRL.Comm.Runtime;
using AxCRL.Comm.Utils;
using AxCRL.Data;
using AxCRL.Data.SqlBuilder;
using AxCRL.Template;
using AxCRL.Template.DataSource;
using AxCRL.Template.Layout;
using AxCRL.Template.ViewTemplate;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace MES_Com.MasterDataBcf
{
    [ProgId(ProgId = "com.Person", ProgIdType = ProgIdType.Bcf)]
    public class ComPersonBcf : LibBcfData
    {
        protected override LibTemplate RegisterTemplate()
        {
            return new ComPersonBcfTemplate("com.Person");
        }

        protected override void BeforeUpdate()
        {
            //base.BeforeUpdate();
            //DataRow row = this.DataSet.Tables[0].Rows[0];
            //if (!string.IsNullOrEmpty(LibSysUtils.ToString(row["WECHAT"])))
            //{
            //    SendMessage(row);
            //}

        }
        protected override void BeforeDelete()
        {
            //base.BeforeDelete();
            //DataRow row = this.DataSet.Tables[0].Rows[0];
            //if (!string.IsNullOrEmpty(LibSysUtils.ToString(row["WECHAT"])))
            //{

            //    string paramData = "{";
            //    paramData += "\"useridlist\":[\"" + LibSysUtils.ToString(row["PERSONID"]) + "\"]";
            //    paramData += "}";

            //    WeiXinProvider provider = EnvProvider.Default.WeiXinProvider;
            //    string accessToken = GetAccessToken(provider.CorpId, provider.Secret);
            //    string postUrl = string.Format("https://qyapi.weixin.qq.com/cgi-bin/user/batchdelete?access_token={0}", accessToken);
            //    Encoding dataEncode = Encoding.UTF8;
            //    PostWebRequest(postUrl, paramData, dataEncode);
            //}

        }

        public string SendMessage(DataRow row)
        {
            string paramData = "{";
            paramData += "\"userid\": \"" + LibSysUtils.ToString(row["PERSONID"]) + "\",";
            paramData += "\"name\": \"" + LibSysUtils.ToString(row["PERSONNAME"]) + "\",";
            paramData += "\"department\": " + "[36]" + ",";
            paramData += "\"position\": \"" + LibSysUtils.ToString(row["POSITION"]) + "\",";
            paramData += "\"mobile\": \"" + LibSysUtils.ToString(row["PHONENO"]) + "\",";
            paramData += "\"gender\": \"" + LibSysUtils.ToInt32(row["GENDER"]) + "\",";
            paramData += "\"email\": \"" + LibSysUtils.ToString(row["MAIL"]) + "\",";
            paramData += "\"weixinid\": \"" + LibSysUtils.ToString(row["WECHAT"]) + "\",";
            paramData += "}";

            Encoding dataEncode = Encoding.UTF8;

            WeiXinProvider provider = EnvProvider.Default.WeiXinProvider;
            string accessToken = GetAccessToken(provider.CorpId, provider.Secret);
            string postUrl = string.Format("https://qyapi.weixin.qq.com/cgi-bin/user/create?access_token={0}", accessToken);

            return PostWebRequest(postUrl, paramData, dataEncode);
        }
        /// <summary>
        /// 获取企业号的accessToken
        /// </summary>
        /// <param name="corpid">企业号ID</param>
        /// <param name="corpsecret">管理组密钥</param>
        /// <returns></returns>
        private string GetAccessToken(string corpid, string corpsecret)
        {
            string accessToken = "";
            string respText = "";
            string url = string.Format("https://qyapi.weixin.qq.com/cgi-bin/gettoken?corpid={0}&corpsecret={1}", corpid, corpsecret);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            using (Stream resStream = response.GetResponseStream())
            {
                StreamReader reader = new StreamReader(resStream, Encoding.Default);
                respText = reader.ReadToEnd();
                resStream.Close();
            }
            JavaScriptSerializer Jss = new JavaScriptSerializer();
            Dictionary<string, object> respDic = (Dictionary<string, object>)Jss.DeserializeObject(respText);
            accessToken = respDic["access_token"].ToString();//通过键access_token获取值
            return accessToken;
        }

        /// <summary>
        /// Post数据接口
        /// </summary>
        /// <param name="postUrl">接口地址</param>
        /// <param name="paramData">提交json数据</param> 
        /// <param name="dataEncode">编码方式</param>
        /// <returns></returns>
        private string PostWebRequest(string postUrl, string paramData, Encoding dataEncode)
        {
            string ret = string.Empty;
            try
            {
                byte[] byteArray = dataEncode.GetBytes(paramData);
                HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(new Uri(postUrl));
                webReq.Method = "POST";
                //webReq.ContentType = "application/x-www-form-urlencoded";
                webReq.ContentType = "application/json; charset=utf-8";
                webReq.ContentLength = byteArray.Length;
                Stream newStream = webReq.GetRequestStream();
                newStream.Write(byteArray, 0, byteArray.Length);
                newStream.Close();
                HttpWebResponse response = (HttpWebResponse)webReq.GetResponse();
                StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.Default);
                ret = sr.ReadToEnd();
                sr.Close();
                response.Close();
                newStream.Close();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return ret;
        }

    }

    public class ComPersonBcfTemplate : LibTemplate
    {
        private const string comPersonName = "COMPERSON";
        /// <summary>
        /// 人员单据消息订阅
        /// </summary>
        private const string bodyTableName = "COMPERSONNOTICESUBSCRIBE";
        /// <summary>
        /// 人员单据消息订阅明细表
        /// </summary>
        private const string subDetailTableName = "COMPERSONNOTICESUBSCRIBEDETAIL";

        private const string primaryName = "PERSONID";

        public ComPersonBcfTemplate(string progId)
            : base(progId, BillType.Master, "人员")
        {

        }
        /// <summary>
        /// 生成通知渠道类型NoticeChanelType枚举对应的KeyValue选项
        /// </summary>
        /// <returns></returns>
        private LibTextOptionCollection GetNoticeTypeKeyValue()
        {
            LibTextOptionCollection options = new LibTextOptionCollection();           
            Array arrays = Enum.GetValues(typeof(NoticeChanelType));        
            for (int i = 0; i < arrays.LongLength; i++)
            {
                object test = arrays.GetValue(i);
                FieldInfo fieldInfo = test.GetType().GetField(test.ToString());
                int key = (int)arrays.GetValue(i);
                object[] attribArray = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), true);
                if (attribArray == null || attribArray.Length == 0)
                {
                    continue;
                }
                else
                {
                    DescriptionAttribute attr = attribArray[0] as DescriptionAttribute;
                    if (attr == null)
                        continue;
                    options.Add(new LibTextOption()
                    {
                         Key= Convert.ToString((int)arrays.GetValue(i)),
                         Value= attr.Description
                    });                  
                }
            }
            return options;
        }
        protected override void BuildDataSet()
        {
            this.DataSet = new DataSet();
            DataTable comPerson = new DataTable(comPersonName);
            DataSourceHelper.AddColumn(new DefineField(comPerson, primaryName, "人员代码", FieldSize.Size20) { AllowEmpty = false, AllowCopy = false });
            DataSourceHelper.AddColumn(new DefineField(comPerson, "PERSONNAME", "人员名称", FieldSize.Size50) { DataType = LibDataType.NText, AllowEmpty = false });
            DataSourceHelper.AddColumn(new DefineField(comPerson, "POSITION", "职位", FieldSize.Size50) { DataType = LibDataType.NText });
            DataSourceHelper.AddColumn(new DefineField(comPerson, "NOWPERSON", "员工号", FieldSize.Size20) { AllowEmpty = false, AllowCopy = false });
            DataSourceHelper.AddColumn(new DefineField(comPerson, "GENDER", "性别") { DataType = LibDataType.Int32, ControlType = LibControlType.TextOption, TextOption = new string[] { "男", "女" } });
            Dictionary<string, bool> orderBys = new Dictionary<string, bool>();
            orderBys.Add("SORTORDER", true);
            DataSourceHelper.AddColumn(new DefineField(comPerson, "DEPTID", "部门", FieldSize.Size20)
            {
                #region 部门   
                ControlType = LibControlType.IdName,
                RelativeSource = new RelativeSourceCollection()
                {
                    new RelativeSource("com.Dept")
                    {
                         RelFields = new RelFieldCollection()
                         {
                             new RelField("DEPTNAME", LibDataType.NText,FieldSize.Size50,"部门名称")
                         } ,
                         //ContainsSub = true,
                         //ExpandAll=true,//默认全部展开
                         //SearchFilterCount = 200,//筛选200条
                         //ParentColumnName = "SUPERDEPTID", //在关联的表中表示父数据的列
                         //OrderbyColumns = orderBys
                    }
                },
                //ControlType = LibControlType.IdNameTree //以树形结构展示,需要在RelativeSource属性后设置，否则会重置为IdName
                #endregion
            });
            DataSourceHelper.AddColumn(new DefineField(comPerson, "WECHAT", "微信", FieldSize.Size50) { DataType = LibDataType.NText });

            DataSourceHelper.AddColumn(new DefineField(comPerson, "MAIL", "邮箱", FieldSize.Size50) { AllowCopy = false, InputType = InputType.Email });
            DataSourceHelper.AddColumn(new DefineField(comPerson, "PHONENO", "手机", FieldSize.Size20) { DataType = LibDataType.Text, ControlType = LibControlType.Text });
            //施卢威 20170214 用于集团内部通讯
            DataSourceHelper.AddColumn(new DefineField(comPerson, "CORNET", "短号", FieldSize.Size20) { DataType = LibDataType.Text, ControlType = LibControlType.Text });
            //施卢威 20170214  用于APP用户头像展示
            DataSourceHelper.AddColumn(new DefineField(comPerson, "HEADPORTRAIT", "头像", FieldSize.Size500) { DataType = LibDataType.NText });
            DataSourceHelper.AddColumn(new DefineField(comPerson, "ROLETYPE", "人员类型") { AllowCopy = false, DataType = LibDataType.Int32, ControlType = LibControlType.TextOption, TextOption = new string[] { "普通工人","班组长"  }, DefaultValue = 0 });

            DataSourceHelper.AddFixColumn(comPerson, this.BillType);
            comPerson.PrimaryKey = new DataColumn[] { comPerson.Columns["PERSONID"] };    
            this.DataSet.Tables.Add(comPerson);

            //#region 人员消息订阅
            //DataTable bodyTable = new DataTable(bodyTableName);
            //DataSourceHelper.AddColumn(new DefineField(bodyTable, primaryName, "人员代码", FieldSize.Size20) { AllowEmpty = false, ReadOnly = true });
            //DataSourceHelper.AddRowId(bodyTable);
            //DataSourceHelper.AddRowNo(bodyTable);           
            //DataSourceHelper.AddColumn(new DefineField(bodyTable, "PROGID", "功能代码", FieldSize.Size50)
            //{
            //    AllowEmpty = false,
            //    ControlType = LibControlType.IdName,
            //    RelativeSource = new RelativeSourceCollection(){
            //        new RelativeSource("axp.FuncList"){
            //               RelFields = new RelFieldCollection(){
            //               new RelField("PROGNAME", LibDataType.NText,FieldSize.Size50,"功能名称")
            //          }
            //        }
            //    }
            //});
            //DataSourceHelper.AddColumn(new DefineField(bodyTable, "SUBSCRIBETYPE", "订阅类型")
            //{
            //    DataType = LibDataType.Int32,
            //    AllowEmpty = false,
            //    ControlType = LibControlType.TextOption,
            //    TextOption = new string[] { "订阅", "拒收" },
            //    DefaultValue = 0                
            //});
            //DataSourceHelper.AddColumn(new DefineField(bodyTable, "NOTICETYPE", "渠道类型")
            //{
            //    DataType = LibDataType.Int32,
            //    AllowEmpty = false,
            //    ControlType = LibControlType.KeyValueOption,
            //    KeyValueOption = GetNoticeTypeKeyValue(),//根据渠道类型的枚举生成键值对
            //    DefaultValue = 0
            //});
            //DataSourceHelper.AddColumn(new DefineField(bodyTable, "CONDITION", "生效条件") { DataType = LibDataType.Binary, ControlType = LibControlType.Text, ReadOnly = true });
            //DataSourceHelper.AddColumn(new DefineField(bodyTable, "HASCONDITION", "存在生效条件") { DataType = LibDataType.Boolean, ControlType = LibControlType.YesNo, FieldType = FieldType.Virtual, ReadOnly = true });
            //DataSourceHelper.AddColumn(new DefineField(bodyTable, "ISBILLACTION", "操作种类") { ReadOnly = true, DataType = LibDataType.Boolean, ControlType = LibControlType.YesNo, SubTableIndex = 2 });
            //DataSourceHelper.AddRemark(bodyTable);
            //bodyTable.PrimaryKey = new DataColumn[] { bodyTable.Columns["PERSONID"], bodyTable.Columns["ROW_ID"] };
            //this.DataSet.Tables.Add(bodyTable);
            //this.DataSet.Relations.Add(string.Format("{0}_{1}", comPerson, bodyTable), comPerson.Columns["PERSONID"], bodyTable.Columns["PERSONID"]);
            
            ////DataTable subTable = new DataTable(subDetailTableName);
            ////DataSourceHelper.AddColumn(new DefineField(subTable, primaryName, "人员代码", FieldSize.Size20) { AllowEmpty = false, AllowCopy = false });
            ////DataSourceHelper.AddRowId(subTable, "PARENTROWID", "父行标识");
            ////DataSourceHelper.AddRowId(subTable);
            ////DataSourceHelper.AddRowNo(subTable);
            ////DataSourceHelper.AddColumn(new DefineField(subTable, "OPERATE", "表单操作") { DataType = LibDataType.Int32, ControlType = LibControlType.Number, ReadOnly = true });
            ////DataSourceHelper.AddColumn(new DefineField(subTable, "OPERATEPOWERNAME", "操作", FieldSize.Size50)
            ////{
            ////    DataType = LibDataType.NText, ControlType = LibControlType.NText, ReadOnly = true
            ////});
            ////DataSourceHelper.AddColumn(new DefineField(subTable, "CHECK", "选择") { DataType = LibDataType.Boolean, ControlType = LibControlType.YesNo });
            ////DataSourceHelper.AddRemark(subTable);
            ////subTable.PrimaryKey = new DataColumn[] { subTable.Columns[primaryName], subTable.Columns["PARENTROWID"], subTable.Columns["ROW_ID"] };
            ////this.DataSet.Tables.Add(subTable);
            ////this.DataSet.Relations.Add(string.Format("{0}_{1}", bodyTableName, subTableName), new DataColumn[] { bodyTable.Columns[primaryName], bodyTable.Columns["ROW_ID"] }, new DataColumn[] { subTable.Columns[primaryName], subTable.Columns["PARENTROWID"] });
            //#endregion
        }


        protected override void DefineViewTemplate(DataSet dataSet)
        {
            LibBillLayout layout = new LibBillLayout(this.DataSet);
            layout.HeaderRange = layout.BuildControlGroup(0, string.Empty, new List<string>() { "PERSONID", "PERSONNAME", "NOWPERSON", "POSITION", "GENDER", "DEPTID", "WECHAT", "MAIL", "PHONENO", "ROLETYPE" });
            this.ViewTemplate = new LibBillTpl(this.DataSet, layout);
        }
        protected override void DefineFuncPermission()
        {
            base.DefineFuncPermission();
            this.FuncPermission = new LibFuncPermission("", this.BillType);
            //清单页上的分类树配置
            this.FuncPermission.TreeListing = new TreeListingConfig()
            {
                ColumnName = "DEPTID",
                RelativeIdColumn = "DEPTID",
                RelativeNameColumn = "DEPTNAME",
                RelativeParentColumn = "SUPERDEPTID",//关联的数据表中表示上级对象的列
                OrderBy = "DEPTLEVEL Asc",
                NodeShowId = true,
                NodeShowJoinChar = false,
                NodeJoinChar = "_",
                NodeShowName = true
            };
            this.FuncPermission.UseSynchroData = false;//是否启用数据同步
        }
    }
}
