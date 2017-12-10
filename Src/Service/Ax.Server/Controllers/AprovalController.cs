using AxCRL.Comm.Utils;
using AxCRL.Core.Cache;
using AxCRL.Core.Comm;
using AxCRL.Data;
using AxCRL.Services;
using AxCRL.Services.ServiceMethods;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using System.IO;
using AxCRL.Comm.Runtime;
using Ax.Server.Models.ModelService;
using Ax.Server.Models.Bcf;
using Newtonsoft.Json;
namespace Ax.Server.Controllers
{
    public class AprovalController : ApiController
    {
        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [Route("api/approval/list")]
        public IHttpActionResult GetList(HttpRequestMessage request, PageModel info)
        {
            string validateCode = GetHeader(request, "x-session-token");
            string userId = GetHeader(request, "x-session-userid");
            var result = Service.GetMyNews(userId, validateCode, info);
            if (!string.IsNullOrEmpty(result.Message))
                return BadRequest(result.Message);
            return Ok(result);
        }


        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [Route("api/message/List")]
        public IHttpActionResult GetAllList(HttpRequestMessage request, PageModel info)
        {
            string validateCode = GetHeader(request, "x-session-token");
            string userId = GetHeader(request, "x-session-userid");
            //var result = server.GetAbnormalReport(userId, validateCode, info);
            var result = Message.GetAbnormalReport(userId, validateCode, info);
            return Ok(result);
        }

        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [Route("api/message/Select")]
        public IHttpActionResult GetSelectList(HttpRequestMessage request, PageModel info)
        {
            string validateCode = GetHeader(request, "x-session-token");
            string userId = GetHeader(request, "x-session-userid");
            var result = Message.GetSelectList(validateCode, userId, info);
            return Ok();
        }


        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [Route("api/login")]
        public IHttpActionResult Login(HttpRequestMessage request, UserInfo info)
        {
            if (info == null)
                return BadRequest("请求错误");
            Result res = new Result();
            res = Service.Login(info);
            if (!string.IsNullOrEmpty(res.Message))
            {
                return BadRequest(res.Message);
            }
            return Ok(res);
        }


        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [Route("api/Register")]
        public IHttpActionResult Register(HttpRequestMessage request, RegisterInfo info)
        {
            if (info == null)
                return BadRequest("请求错误");
            var result = Service.AppRegister(info);
            if (!result.ReturnValue)
                return BadRequest(result.Message);
            return Ok(result);
        }

        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [Route("api/approval/audit")]
        public IHttpActionResult Audit(HttpRequestMessage request, AuditModel model)
        {
            if (model == null)
                return BadRequest("请求错误");
            string validateCode = GetHeader(request, "x-session-token");
            var result = Service.Audit(model.ProgId, model.BillNo, model.RowId, model.UserId, validateCode, model.IsPass, model.Message);
            if (!result.ReturnValue)
            {
                return BadRequest(result.Message);
            }
            return Ok(result);
        }


        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [Route("api/approval/billinfo")]
        public IHttpActionResult GetBillInfo(HttpRequestMessage request, AuditModel model)
        {
            if (model == null)
                return BadRequest("请求错误");
            string validateCode = GetHeader(request, "x-session-token");
            string userId = GetHeader(request, "x-session-userid");
            var result = Service.GetBillInfo(userId, validateCode, model.ProgId, model.BillNo, model.RowId);
            return Ok(result);
        }


        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [Route("api/getReportData")]
        public IHttpActionResult GetReportData(HttpRequestMessage request, ModelReport model)
        {
            string validateCode = GetHeader(request, "x-session-token");
            string userId = GetHeader(request, "x-session-userid");
            var result = Report.GetReportData(userId, validateCode, model.ProgId, model.queryFieldList);
            return Ok(result);
        }

        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [Route("api/message/exceptionTrack")]
        public IHttpActionResult exceptionTrack(HttpRequestMessage request, ExceptionTrack model)
        {
            string userId = GetHeader(request, "x-session-userid");
            var result = Message.ExceptionTrack(model.PersonId, model.BillNo, model.PlanEndTime, model.Solution, model.DealwithState, userId);
            if (result.ReturnValue)
                return Ok(result);
            else
                return BadRequest(result.Message);
        }

        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [Route("api/message/exceptionTrack/{billNo}")]
        [HttpGet]
        public IHttpActionResult GetExceptionTrack(HttpRequestMessage request, string billNo)
        {
            string validateCode = GetHeader(request, "x-session-token");
            string userId = GetHeader(request, "x-session-userid");
            var result = Message.ExceptionTrackBillNo(billNo, userId, validateCode);
            if (!result.ReturnValue)
                return BadRequest(result.Message);
            return Ok(result);
        }
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [Route("api/message/exceptionBill/{billNo}")]
        [HttpGet]
        public IHttpActionResult GetExceptionBill(HttpRequestMessage request, string billNo)
        {
            string validateCode = GetHeader(request, "x-session-token");
            string userId = GetHeader(request, "x-session-userid");
            var result = Message.ExceptionBill(billNo, userId, validateCode);
            if (!result.ReturnValue)
                return BadRequest(result.Message);
            return Ok(result);
        }


        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [Route("api/getDeptInfo")]
        public IHttpActionResult GetDeptInfo(HttpRequestMessage request)
        {
            var result = Service.GetDept();
            if (!result.ReturnValue)
                return BadRequest(result.Message);
            return Ok(result);
        }

        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [Route("api/appGenerateCode/{phoneNo}/{userId}")]
        [HttpGet]
        public IHttpActionResult AppGenerateCode(HttpRequestMessage request, string phoneNo, string userId)
        {
            var result = Service.GenerateCode(userId, phoneNo);
            return Ok(result);
        }

        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [Route("api/allColumnInfo/{progId}")]
        [HttpGet]
        public IHttpActionResult AllColumnInfo(HttpRequestMessage request, string progId)
        {
            string errorMsg = string.Empty;
            var result = BcfTemplateMethods.GetBcfDefineFields(progId, out errorMsg);
            if (!string.IsNullOrEmpty(errorMsg))
            {
                return BadRequest(errorMsg);
            }
            return Ok(result);
        }

        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [Route("api/displayColumnInfo/{progId}")]
        [HttpGet]
        public IHttpActionResult DisplayColumnInfo(HttpRequestMessage request, string progId)
        {
            string errorMsg = string.Empty;
            var result = BcfTemplateMethods.GetBcfMobileShowScheme(progId, out errorMsg);
            if (!string.IsNullOrEmpty(errorMsg))
            {
                return BadRequest(errorMsg);
            }
            return Ok(JsonConvert.DeserializeObject(result));
        }


        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [Route("api/pictureUpload")]
        public IHttpActionResult PictureUpload(HttpRequestMessage request, pictureUploadModel model)
        {
            var result = Service.PictureUpload(model);
            if (!result.ReturnValue)
                return BadRequest(result.Message);
            return Ok(result);
        }

        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [Route("api/editPersonInfo")]
        public IHttpActionResult EditPersonInfo(HttpRequestMessage request, Models.ModelService.PersonInfo model)
        {
            var result = Service.SetPersonInfo(model);
            if (!result.ReturnValue)
                return BadRequest(result.Message);
            return Ok(result);
        }

        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [Route("api/feedbackMsg")]
        public IHttpActionResult FeedbackMsg(HttpRequestMessage request, FeedbackModel info)
        {
            string validateCode = GetHeader(request, "x-session-token");
            string userId = GetHeader(request, "x-session-userid");
            var result = Service.FeedbackMsg(userId, validateCode, info);
            if (!result.ReturnValue)
            {
                return BadRequest(result.Message);
            }
            return Ok(result);
        }

        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [Route("api/getCount")]
        public IHttpActionResult GetCount(HttpRequestMessage request)
        {
            string validateCode = GetHeader(request, "x-session-token");
            string userId = GetHeader(request, "x-session-userid");
            var result = Service.GetCount(userId, validateCode);
            if (!result.ReturnValue)
            {
                return BadRequest(result.Message);
            }
            return Ok(result);
        }

        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [Route("api/test")]
        [HttpGet]
        public IHttpActionResult test(HttpRequestMessage request)
        {
            List<string> list = new List<string>();
            list.Add("f70befc00249c7337c15ba253e7cc391");
            AxCRL.Core.SysNews.LibSysNewsHelper.SendNews(list);
            //Service.PushMessageListToListByTransmissionTemplate();
            return Ok();
        }

        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [Route("api/login/pictureValidateCode")]
        [HttpGet]
        public IHttpActionResult GetPictureValidateCode(HttpRequestMessage request)
        {
            Result result = new Result();

            string userId = GetHeader(request, "x-session-userid");
            ValidateCode v = new ValidateCode();
            string code = v.CreateVerifyCode(); //取随机码
            string codeId = APPCache.SetAPPCache(code);
            //service.savepicturecalidatecode(userid, code);
            string validatecode = v.CreateImageToString(code);
            //var context = HttpContext.Current;
            //SessionHelper.SetSession(SessionEnum.VALIDATE_CODE, code);
            string pictureCode = string.Format("data:image/png;base64,{0}", validatecode);
            PictureCodeResult pictureCodeResult = new PictureCodeResult() { PictureCode = pictureCode, CodeID = codeId };
            result.Info = pictureCodeResult;
            result.ReturnValue = true;
            return Ok(result);
        }


        /// <summary>
        /// 获取前端传入的Handel或UserId
        /// </summary>
        /// <param name="request"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private static string GetHeader(HttpRequestMessage request, string key)
        {
            IEnumerable<string> headers = new List<string>();
            if (request.Headers.TryGetValues(key, out headers) == false)
                return null;
            foreach (var header in headers)
                return header;
            return null;
        }
        public enum StateEnum
        {
            [DescriptionAttribute("已审核")]
            Finished,
            [DescriptionAttribute("未审核")]
            UnFinished
        }
    }
}
