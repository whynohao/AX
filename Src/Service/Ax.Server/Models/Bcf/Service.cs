using Ax.Ui.Models.ModelService;
using AxCRL.Bcf;
using AxCRL.Comm.Runtime;
using AxCRL.Comm.Service;
using AxCRL.Comm.Utils;
using AxCRL.Core.Cache;
using AxCRL.Core.Comm;
using AxCRL.Core.SysNews;
using AxCRL.Data;
using AxCRL.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web;

namespace Ax.Ui.Models.Bcf
{
    public class Service
    {
        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public static Result Login(UserInfo info)
        {
            Result res = new Result();
            try
            {
                LibDataAccess access = new LibDataAccess();
                SystemService server = new SystemService();
                if (APPCache.CacheDic.ContainsKey(info.CodeId))
                {
                    if (string.Equals(APPCache.CacheDic[info.CodeId], info.Code, StringComparison.CurrentCultureIgnoreCase))
                    {
                        LoginInfo loginInfo = server.Login(info.UserId, info.Password, false);
                        LibHandle handle = LibHandleCache.Default.IsExistsHandle(LibHandeleType.PC, info.UserId);
                        if (loginInfo.PersonId == null)
                        {
                            res.ReturnValue = false;
                            res.Message = "登录失败！";
                        }
                        else if (handle == null)
                        {
                            res.ReturnValue = false;
                            res.Message = "请重新登录！";
                        }
                        else
                        {

                            string sql = string.Format("SELECT B.PHONENO,B.MAIL FROM AXPUSER A LEFT JOIN COMPERSON B ON B.PERSONID=A.PERSONID WHERE A.USERID={0}", LibStringBuilder.GetQuotString(info.UserId));
                            using (IDataReader reader = access.ExecuteDataReader(sql))
                            {
                                if (reader.Read())
                                {
                                    //loginInfo.UserPhone = LibSysUtils.ToString(reader["PHONENO"]);
                                    loginInfo.UserEMail = LibSysUtils.ToString(reader["MAIL"]);
                                }
                            }
                            loginInfo.Handle = handle.Handle;
                            res.Info = loginInfo;
                            res.ReturnValue = true;
                            APPCache.RemoveAPPCache(info.CodeId);
                        }
                    }
                    else
                    {
                        res.Message = "验证码错误！";
                        res.ReturnValue = false;
                    }
                }
                else
                {
                    res.Message = "验证码失效！";
                    res.ReturnValue = false;
                }

            }
            catch (Exception ex)
            {
                res.Message = ex.Message;
                res.ReturnValue = false;
            }
            return res;

        }

        /// <summary>
        /// 验证Handle
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="handle"></param>
        /// <param name="Handle"></param>
        /// <returns></returns>
        public static bool VerificationHandle(string userId, string handle, LibHandle Handle, Result res)
        {
            if (string.IsNullOrEmpty(handle))
            {
                res.Message = "请重新登录！";
                res.ReturnValue = false;
                return false;
            }
            //验证用户信息

            if (Handle == null)
            {
                res.Message = "请重新登录！";
                res.ReturnValue = false;
                return false;

            }
            if (!Handle.Handle.Equals(handle))
            {
                res.Message = "请重新登录！";
                res.ReturnValue = false;
                return false;

            }
            return true;
        }

        /// <summary>
        /// 获取消息
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="handle"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        public static Result GetMyNews(string userId, string handle, PageModel info)
        {
            Result res = new Result();
            res.ReturnValue = true;
            LibHandle Handle = LibHandleCache.Default.IsExistsHandle(LibHandeleType.PC, userId);
            VerificationHandle(userId, handle, Handle, res);
            if (res.ReturnValue)
            {
                LibDataAccess access = new LibDataAccess();
                try
                {
                    string sql = string.Empty;
                    if (!string.IsNullOrEmpty(Handle.PersonId))//待优化
                    {
                        string selectCondition = string.Empty;
                        if (info.SelectCondition == 0)
                        {
                            selectCondition = string.Format("AND ISPASS = {0}", 0);
                        }
                        else
                        {
                            selectCondition = string.Format("AND ISPASS <> {0}", 0);
                        }
                        Dictionary<string, object> dic = new Dictionary<string, object>();
                        int beginNum = 0, endNum = 0;
                        sql = string.Format("SELECT COUNT(*) AS TOTALNUM FROM AXPAPPROVETASK WHERE PERSONID = {0} {1}", LibStringBuilder.GetQuotString(Handle.PersonId), selectCondition);
                        int totalCount = LibSysUtils.ToInt32(access.ExecuteScalar(sql));
                        int pageCount = 0;
                        if (totalCount / info.PageSize == 0)
                            pageCount = totalCount / info.PageSize;
                        else
                            pageCount = totalCount / info.PageSize + 1;
                        endNum = info.PageNo * info.PageSize;
                        beginNum = (info.PageNo - 1) * info.PageSize + 1;
                        sql = string.Empty;
                        sql = string.Format(@" SELECT A.PROGID,
                                                      B.PROGNAME,
                                                      A.BILLNO,
                                                      A.FROMROWID,
                                                      A.SUBMITPERSONID,
                                                      D.PERSONNAME AS SUBMITPERSONNAME,
                                                      A.PERSONID,
                                                      C.PERSONNAME,
                                                      A.CREATETIME 
                                                      FROM 
                                                      (
                                                      SELECT E.*,ROWNUM RN 
                                                      FROM (SELECT * FROM AXPAPPROVETASK) E 
                                                      WHERE ROWNUM <= {1} AND PERSONID={2} {3}) A 
                                                      LEFT JOIN AXPFUNCLIST B ON A.PROGID = B.PROGID 
                                                      LEFT JOIN COMPERSON C ON A.PERSONID = C.PERSONID 
                                                      LEFT JOIN COMPERSON D ON A.SUBMITPERSONID = D.PERSONID
                                                      WHERE RN>={0}", beginNum, endNum, LibStringBuilder.GetQuotString(Handle.PersonId), selectCondition);
                        res.Info = access.ExecuteDataSet(sql);
                        res.pageModel.PageNo = info.PageNo;
                        res.pageModel.PageSize = info.PageSize;
                        res.pageModel.PageCount = pageCount;
                        res.pageModel.TotalCount = totalCount;
                        res.ReturnValue = true;
                    }
                    else
                    {
                        res.Message = "请重新登录！";
                    }
                }
                catch (Exception ex)
                {
                    res.ReturnValue = false;
                    res.Message = "查询失败！" + ex.Message;
                }
            }
            return res;
        }

        public static Result AppRegister(RegisterInfo info)
        {
            Result result = new Result();
            AxCRL.Services.SystemService SysSvc = new SystemService();
            Result CodeResult = VerifyCode(info.inputId, info.VerificationCode);
            if (CodeResult.ReturnValue)
            {
                if (string.IsNullOrEmpty(SysSvc.Register(info)))
                {
                    DeleteCode(info.inputId);
                    result.ReturnValue = true;
                }
                else
                {
                    result.Message = SysSvc.Register(info);
                    result.ReturnValue = false;
                }
            }
            else
            {
                result.Message = CodeResult.Message;
                result.ReturnValue = false;
            }
            return result;
        }

        /// <summary>
        /// 验证验证码是否正确
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        private static Result VerifyCode(string userId, string code)
        {
            Result res = new Result();
            try
            {
                string sql = string.Format("SELECT VERIFYCODE FROM AXPVERIFYCODE WHERE USERID={0}", LibStringBuilder.GetQuotString(userId));
                LibDataAccess access = new LibDataAccess();
                string realCode = LibSysUtils.ToString(access.ExecuteScalar(sql));
                if (realCode == string.Empty)
                {
                    res.ReturnValue = false;
                    res.Message = "该帐号没有对应的验证码";
                    return res;
                }

                if (code != null && code.Equals(realCode))
                {
                    res.ReturnValue = true;
                    return res;
                }
                else
                {
                    res.ReturnValue = false;
                    res.Message = "验证码错误";
                    return res;
                }
            }
            catch (Exception ex)
            {
                res.ReturnValue = false;
                res.Message = "注册失败！" + ex.Message;
                return res;
            }
        }

        /// <summary>
        /// 登录成功后删除验证码
        /// </summary>
        /// <param name="userId"></param>
        private static void DeleteCode(string userId)
        {
            string sql = string.Format("DELETE FROM AXPVERIFYCODE WHERE USERID={0}", LibStringBuilder.GetQuotString(userId));
            LibDataAccess access = new LibDataAccess();
            access.ExecuteNonQuery(sql);
        }

        /// <summary>
        /// 审核操作
        /// </summary>
        /// <param name="progId"></param>
        /// <param name="billNo"></param>
        /// <param name="rowId"></param>
        /// <param name="userId"></param>
        /// <param name="handle"></param>
        /// <param name="isPass"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static Result Audit(string progId, string billNo, int rowId, string userId, string handle, bool isPass, string message)
        {

            Result res = new Result();
            res.ReturnValue = true;
            LibHandle Handle = LibHandleCache.Default.IsExistsHandle(LibHandeleType.PC, userId);
            VerificationHandle(userId, handle, Handle, res);
            if (res.ReturnValue)
            {
                try
                {
                    //根据progid构建bcf
                    LibBcfData bcf = (LibBcfData)LibBcfSystem.Default.GetBcfInstance(progId);
                    if (bcf == null)
                    {
                        res.ReturnValue = false;
                        res.Message = "请输入正确progId!";
                        return res;
                    }
                    bcf.Handle = Handle;

                    //根据rowid判断是行审核还是单据审核
                    if (rowId > 0)
                    {
                        Dictionary<int, SortedList<int, List<LibApproveFlowInfo>>> dic =
                            new Dictionary<int, SortedList<int, List<LibApproveFlowInfo>>>();
                        dic.Add(rowId, new SortedList<int, List<LibApproveFlowInfo>>());
                        bcf.AuditRow(new object[] { billNo }, isPass, dic, new Dictionary<int, int>());
                    }
                    else
                    {
                        //string.Empty 为审核意见，后期加上
                        bcf.Audit(new object[] { billNo }, isPass, string.Empty, new Dictionary<string, LibChangeRecord>(), -1, null);
                    }

                    StringBuilder sb = new StringBuilder();

                    //根据messagelist判断是否操作成功
                    if (bcf.ManagerMessage.IsThrow)
                    {
                        foreach (LibMessage item in bcf.ManagerMessage.MessageList)
                        {
                            sb.Append(item.Message);
                        }
                        res.Message = sb.ToString();
                        res.ReturnValue = false;
                    }
                    else
                    {
                        res.ReturnValue = true;
                        res.Message = "审核成功！";
                        PushMessage(userId, PushType.Approval);
                    }
                }
                catch (Exception ex)
                {
                    res.ReturnValue = false;
                    res.Message = "审核失败！" + ex.Message;
                }
            }
            return res;


        }
        public static void PushMessage(string userId, PushType pushType)
        {
            LibDataAccess dataAccess = new LibDataAccess();
            try
            {
                string sql = string.Format("SELECT UUID FROM AXPUSER WHERE USERID={0}");
                string uuid = LibSysUtils.ToString(dataAccess.ExecuteScalar(sql));
                if (!string.IsNullOrEmpty(uuid))
                {
                    List<string> uuidList = new List<string>();
                    uuidList.Add(uuid);
                    //PushMessageListToListByTransmissionTemplate(uuidList, pushType);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 获取审核单据数据
        /// </summary>
        /// <param name="progId"></param>
        /// <param name="billNo"></param>
        /// <param name="RowId"></param>
        /// <returns></returns>
        public static Result GetBillInfo(string userId, string handle, string progId, string billNo, int RowId)
        {
            Result res = new Result();
            res.ReturnValue = true;
            LibHandle Handle = LibHandleCache.Default.IsExistsHandle(LibHandeleType.PC, userId);
            VerificationHandle(userId, handle, Handle, res);
            if (res.ReturnValue)
            {
                try
                {
                    LibBcfData bcfData = (LibBcfData)LibBcfSystem.Default.GetBcfInstance(progId);
                    DataSet ds = bcfData.BrowseTo(new object[] { billNo });
                    if (RowId > 0)
                    {
                        DataSet newDs = new DataSet();
                        DataTable dt = ds.Tables[1].Clone();
                        newDs.Tables.Add(ds.Tables[0].Copy());
                        DataRow selrow = ds.Tables[1].Select("ROW_ID='" + RowId + "'")[0];
                        dt.ImportRow(selrow);
                        newDs.Tables.Add(dt);
                        res.Info = newDs;
                    }
                    else
                    {
                        res.Info = ds;
                    }
                    res.ReturnValue = true;
                }
                catch (Exception ex)
                {

                    res.ReturnValue = false;
                    res.Message = "审核失败！" + ex.Message;
                }
            }
            return res;
        }

        public static Result GetDept()
        {
            Result res = new Result();
            LibDataAccess access = new LibDataAccess();
            res.ReturnValue = true;
            res.Message = "OK";
            try
            {
                string sql = string.Format("SELECT DEPTID,DEPTNAME FROM COMDEPT");
                res.Info = access.ExecuteDataSet(sql);
            }
            catch (Exception ex)
            {
                res.ReturnValue = false;
                res.Message = "查询失败！" + ex.Message;
            }
            return res;
        }

        public static Result GenerateCode(string userId, string phoneNo)
        {
            Result res = new Result();
            try
            {
                // 生成四位数的验证码
                Random r = new Random();
                int i = (int)(r.NextDouble() * 10000);
                string code = i.ToString().PadLeft(4, '0');


                // 查看是否存在该帐号对应的验证码
                LibDataAccess access = new LibDataAccess();
                string sql = string.Format("SELECT count(*) FROM AXPVERIFYCODE WHERE USERID = {0}", LibStringBuilder.GetQuotString(userId));
                int count = LibSysUtils.ToInt32(access.ExecuteScalar(sql));


                // 生成或者更新验证码
                if (count == 0)
                {
                    sql = string.Format("INSERT INTO AXPVERIFYCODE(USERID, VERIFYCODE, TIME) VALUES({0}, {1}, {2})", LibStringBuilder.GetQuotString(userId), LibStringBuilder.GetQuotString(code), LibDateUtils.GetCurrentDateTime());
                }
                else if (count == 1)
                {
                    sql = string.Format("UPDATE AXPVERIFYCODE SET VERIFYCODE={0}, TIME={1} WHERE USERID={2}", LibStringBuilder.GetQuotString(code), LibDateUtils.GetCurrentDateTime(), LibStringBuilder.GetQuotString(userId));
                }
                access.ExecuteNonQuery(sql);


                // 发送短信
                SendSMSParam sendSMSParam = new SendSMSParam();
                sendSMSParam.Message = "验证码为：" + code;
                sendSMSParam.PhoneList.Add(phoneNo);
                LibSMSHelper.SendMsg(sendSMSParam);


                res.ReturnValue = true;
                res.Message = "成功！";
            }
            catch (Exception ex)
            {
                res.ReturnValue = false;
                res.Message = "失败！" + ex.Message;
            }
            return res;
        }

        public static Result PictureUpload(pictureUploadModel model)
        {
            Result res = new Result();
            try
            {
                //var result = "";
                //将byte数组转为base64String
                //string base64String = "/9j/4AAQSkZJRgABAQEAkACQAAD/4QCMRXhpZgAATU0AKgAAAAgABQESAAMAAAABAAEAAAEaAAUAAAABAAAASgEbAAUAAAABAAAAUgEoAAMAAAABAAIAAIdpAAQAAAABAAAAWgAAAAAAAACQAAAAAQAAAJAAAAABAAOgAQADAAAAAQABAACgAgAEAAAAAQAAAHKgAwAEAAAAAQAAAHIAAAAA/9sAQwAfFRcbFxMfGxkbIyEfJS9OMi8rKy9fREg4TnBjdnRuY21rfIyyl3yEqYZrbZvTnam4vsjKyHiV2+rZwumyxMjA/9sAQwEhIyMvKS9bMjJbwIBtgMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDA/8AAEQgAcgByAwEiAAIRAQMRAf/EAB8AAAEFAQEBAQEBAAAAAAAAAAABAgMEBQYHCAkKC//EALUQAAIBAwMCBAMFBQQEAAABfQECAwAEEQUSITFBBhNRYQcicRQygZGhCCNCscEVUtHwJDNicoIJChYXGBkaJSYnKCkqNDU2Nzg5OkNERUZHSElKU1RVVldYWVpjZGVmZ2hpanN0dXZ3eHl6g4SFhoeIiYqSk5SVlpeYmZqio6Slpqeoqaqys7S1tre4ubrCw8TFxsfIycrS09TV1tfY2drh4uPk5ebn6Onq8fLz9PX29/j5+v/EAB8BAAMBAQEBAQEBAQEAAAAAAAABAgMEBQYHCAkKC//EALURAAIBAgQEAwQHBQQEAAECdwABAgMRBAUhMQYSQVEHYXETIjKBCBRCkaGxwQkjM1LwFWJy0QoWJDThJfEXGBkaJicoKSo1Njc4OTpDREVGR0hJSlNUVVZXWFlaY2RlZmdoaWpzdHV2d3h5eoKDhIWGh4iJipKTlJWWl5iZmqKjpKWmp6ipqrKztLW2t7i5usLDxMXGx8jJytLT1NXW19jZ2uLj5OXm5+jp6vLz9PX29/j5+v/aAAwDAQACEQMRAD8A6GiiigAooqF5scL+dAEpIHU4phmQepquSWOSc0UwJvPH939aPP8A9n9ahooAsCZT1yKeCG6HNVKASDkHFAFyioEm7N+dTAgjIpALRRRQAUUUUAFFFQzv/CPxoAbLJuOB0/nUdFFMAoopyIXPH50ANoqTES9SWPtR+6b1WgCOinPGU56j1ptABTo5Ch9vSm0UAWwQRkdKWq8L4O09DVikAUUUUAIx2qT6VUJycmp5zhQPWoKYBRRRQAdalkOxQi/jUaffX606b/WGgBlFFFAEkTZ+RuhpjDaxHpQn31+tOm/1hoAZRRRQAVajbcgPeqtS255I/GgCeiiikBBcfeA9qiqS4++PpUdMAooooAKlceYoZeo6ioqkjRh82do96AI6KmZoieRk+1N3xr91Mn3oAI12je3QdKjJyST3pXcueaSgAooooAKfD/rBTKfD/rBQBZooopAQ3A6GoasyruQiq1MAoopVG5gPWgB6KFXe34CmO5c5NOmPzbR0FMoAKKKKACpNoePKjkdRUdOiba49+KAG0U6RdrkU2gAqSAfOT6Co6sQLhM+tAElFFFIAqtKm1vY1ZprqHXBoAq06MhXBPSkZSpwaSmArHLE+ppKKKACiiigAooooAfKwZsj0plFABJwOtADkXe2KtdKZGmxffvT6QBRRRQAUUUUANdA4waruhQ89PWrVFAFOip2hU9OKYYXHTBpgR0U7Y/8AdNJsb+6fyoASiniFz2xUiwAfeOaAIVUscAVYjjCD1PrTgABgDFLSAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKAP/2Q==";
                string base64String = model.PersonPicture;

                //将base64String 转为 byte数组
                byte[] byteArray = Convert.FromBase64String(base64String);
                string saveFileName = DateTime.Now.ToFileTime().ToString() + '.' + model.FileExtension;
                //string saveFileName = DateTime.Now.ToFileTime().ToString() + ".jpg";
                string dirPath = Path.Combine(EnvProvider.Default.RuningPath, "PersonPicture");
                string path = Path.Combine(dirPath, saveFileName);
                //使用文件流读取byte数组中的数据
                Stream saveStream = new FileStream(path, FileMode.Append);
                saveStream.Write(byteArray, 0, byteArray.Length);
                saveStream.Close();
                string httpPath = "http://" + EnvProvider.Default.VisualHostName + ':' + EnvProvider.Default.CurrentPort + '/' + "PersonPicture" + '/' + saveFileName;
                string sql = string.Format("UPDATE COMPERSON SET HEADPORTRAIT = {0} WHERE PERSONID = {1}", LibStringBuilder.GetQuotString(httpPath), LibStringBuilder.GetQuotString(model.PrisonId));
                LibDataAccess dataAccess = new LibDataAccess();
                int count = -1;
                count = LibSysUtils.ToInt32(dataAccess.ExecuteNonQuery(sql));
                if (count < 0)
                {
                    res.ReturnValue = false;
                    res.Message = "请求错误";
                }
                else
                {
                    res.ReturnValue = true;
                    res.Info = httpPath;
                }
            }
            catch (Exception ex)
            {
                res.ReturnValue = false;
                res.Message = ex.Message;
            }

            return res;
        }

        public static Result SetPersonInfo(ModelService.PersonInfo model)
        {
            Result res = new Result();
            switch (model.SetPersonInfoState)
            {
                case SetPersonInfoState.PersonPicture:
                    res = PictureUpload(model.Setpicture);
                    break;
                case SetPersonInfoState.phoneNo:
                    res = SetComPersonInfo(model.PersonId, model.PhoneNo, SetPersonInfoState.phoneNo);
                    break;
                case SetPersonInfoState.UserId:
                    break;
                case SetPersonInfoState.Cornet:
                    res = SetComPersonInfo(model.PersonId, model.Cornet, SetPersonInfoState.Cornet);
                    break;
                case SetPersonInfoState.PersonName:
                    res = SetComPersonInfo(model.PersonId, model.PersonName, SetPersonInfoState.PersonName);
                    break;
                case SetPersonInfoState.Email:
                    res = SetComPersonInfo(model.PersonId, model.Email, SetPersonInfoState.Email);
                    break;
                case SetPersonInfoState.Password:
                    break;
            }
            return res;
        }

        private static Result SetComPersonInfo(string personId, string value, SetPersonInfoState state)
        {
            Result res = new Result();
            string columnName = string.Empty;
            switch (state)
            {
                case SetPersonInfoState.phoneNo:
                    columnName = "PHONENO";
                    break;
                case SetPersonInfoState.Cornet:
                    columnName = "CORNET";
                    break;
                case SetPersonInfoState.PersonName:
                    columnName = "PERSONNAME";
                    break;
                case SetPersonInfoState.Email:
                    columnName = "MAIL";
                    break;
            }
            try
            {
                string sql = string.Format("UPDATE COMPERSON SET {0}={1} WHERE PERSONID={2}", columnName, LibStringBuilder.GetQuotString(value), LibStringBuilder.GetQuotString(personId));
                LibDataAccess dataAccess = new LibDataAccess();
                int count = -1;
                count = dataAccess.ExecuteNonQuery(sql);
                if (count > 0)
                {
                    res.ReturnValue = true;
                }
                else
                {
                    res.ReturnValue = false;
                    res.Message = "设置失败!";
                }
            }
            catch (Exception ex)
            {
                res.ReturnValue = false;
                res.Message = ex.Message;
            }

            return res;
        }

        private static Result SetAxpUserInfo()
        {
            Result res = new Result();
            return res;
        }

        public static Result FeedbackMsg(string userId, string handle, FeedbackModel info)
        {
            Result res = new Result();
            res.ReturnValue = true;
            LibHandle Handle = LibHandleCache.Default.IsExistsHandle(LibHandeleType.PC, userId);
            VerificationHandle(userId, handle, Handle, res);
            if (res.ReturnValue)
            {
                LibDataAccess access = new LibDataAccess();
                try
                {
                    string sql = string.Empty;
                    if (!string.IsNullOrEmpty(Handle.PersonId))//待优化
                    {
                        sql = string.Format("Insert into AXPFEEDBACKMSG(GUID, USERID, MESSAGETYPE, MESSAGE) VALUES({0}, {1}, {2}, {3})", LibStringBuilder.GetQuotString(Guid.NewGuid().ToString()), LibStringBuilder.GetQuotString(userId), info.MessageType, LibStringBuilder.GetQuotString(info.Message));
                        access.ExecuteNonQuery(sql);
                        res.ReturnValue = true;
                    }
                    else
                    {
                        res.Message = "请重新登录！";
                        res.ReturnValue = false;
                    }
                }
                catch (Exception ex)
                {
                    res.ReturnValue = false;
                    res.Message = "失败！" + ex.Message;
                }
            }
            return res;
        }
        public static Result GetCount(string userId, string handle)
        {
            Result res = new Result();
            res.ReturnValue = true;
            string personId = "018427";
            //LibHandle Handle = LibHandleCache.Default.IsExistsHandle(LibHandeleType.None, userId);
            //VerificationHandle(userId, handle, Handle, res);
            if (res.ReturnValue)
            {
                LibDataAccess access = new LibDataAccess();
                try
                {
                    string sql = string.Empty;
                    //if (!string.IsNullOrEmpty(Handle.PersonId))//待优化
                    //{
                    CountModel model = new CountModel();
                    sql = string.Format("SELECT COUNT(*) AS TOTALNUM FROM AXPAPPROVETASK WHERE PERSONID = {0} AND AUDITSTATE = 0 and FLOWLEVEL=CURRENTLEVEL+1", LibStringBuilder.GetQuotString(personId));
                    model.ApprovelCount = LibSysUtils.ToInt32(access.ExecuteScalar(sql));
                    sql = string.Format("SELECT COUNT(*) FROM COMABNORMALREPORT A INNER JOIN COMABNORMALREPORTTYPEFLOW B ON B.TYPEID = A.TYPEID  WHERE A.TRANSMITLEVEL = B.TRANSMITLEVEL AND B.PERSONID = {0} AND A.BILLNO NOT IN (select FROMBILLNO from COMABNORMALTRACE)", LibStringBuilder.GetQuotString(personId));
                    model.AbnormalCount = LibSysUtils.ToInt32(access.ExecuteScalar(sql));

                    res.Info = model;
                    res.ReturnValue = true;
                    //}
                    //else
                    //{
                    //    res.Message = "请重新登录！";
                    //    res.ReturnValue = false;
                    //}
                }
                catch (Exception ex)
                {
                    res.ReturnValue = false;
                    res.Message = "失败！" + ex.Message;
                }
            }
            return res;
        }        
        public static Result SavePictureCalidateCode(string userId, string code)
        {
            Result result = new Result();
            LibDataAccess dataAccess = new LibDataAccess();
            string sql = string.Format("SELECT count(*) FROM AXPVERIFYCODE WHERE USERID = {0}", LibStringBuilder.GetQuotString(userId));
            int count = LibSysUtils.ToInt32(dataAccess.ExecuteScalar(sql));


            // 生成或者更新验证码
            if (count == 0)
            {
                sql = string.Format("INSERT INTO AXPVERIFYCODE(USERID, VERIFYCODE, TIME) VALUES({0}, {1}, {2})", LibStringBuilder.GetQuotString(userId), LibStringBuilder.GetQuotString(code), LibDateUtils.GetCurrentDateTime());
            }
            else if (count == 1)
            {
                sql = string.Format("UPDATE AXPVERIFYCODE SET VERIFYCODE={0}, TIME={1} WHERE USERID={2}", LibStringBuilder.GetQuotString(code), LibDateUtils.GetCurrentDateTime(), LibStringBuilder.GetQuotString(userId));
            }
            dataAccess.ExecuteNonQuery(sql);
            return result;
        }

    }
}