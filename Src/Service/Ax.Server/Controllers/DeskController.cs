using System;
using System.Web;
using System.Web.Mvc;

namespace Ax.Ui.Controllers
{
    public class DeskController : Controller
    {
        //
        // GET: /Desk/

        //public ActionResult Home()
        //{
        //    return View();
        //}
        public ActionResult Print()
        {
            return View();
        }

        public ActionResult PrintTpl()
        {
            return View();
        }

        public ActionResult ReportBoard()
        {
            return View();
        }

        public ActionResult Home(string progId, string billNo)
        {
            if (!string.IsNullOrEmpty(progId))
                this.ViewBag.ProgId = progId.Replace('_', '.');
            if (!string.IsNullOrEmpty(billNo))
                this.ViewBag.BillNo = billNo;
            return View();
        }
        public ActionResult Login()
        {
            return View();
        }
        /// <summary>
        /// 为实现下载文件时对于浏览器支持的类型（如图片）不是在浏览器直接打开，而是要弹出下载窗口而增加的Action
        /// Zhangkj 20170227
        /// </summary>
        /// <returns></returns>
        public ActionResult Download()
        {
            string fileName = string.Empty;
            fileName = Request.QueryString["filename"];
            if (string.IsNullOrEmpty(fileName))
                return null;
            string downloadName = fileName;
            var contentType = MimeMapping.GetMimeMapping(downloadName);
            string tempFilePath = System.IO.Path.Combine(AxCRL.Comm.Runtime.EnvProvider.Default.RuningPath, "TempData", "Attachment");
            return File(new System.IO.FileStream(System.IO.Path.Combine(tempFilePath, fileName), System.IO.FileMode.Open), contentType, downloadName);
        }


        /// <summary>
        /// 安卓APK下载
        /// </summary>
        /// <returns></returns>
        public ActionResult DownloadApk()
        {
            string fileName = string.Empty;
            fileName = Request.QueryString["filename"];
            if (string.IsNullOrEmpty(fileName))
                return null;
            string downloadName = fileName;
            var contentType = MimeMapping.GetMimeMapping(downloadName);
            string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PDAapk");
            return File(new System.IO.FileStream(System.IO.Path.Combine(path, fileName), System.IO.FileMode.Open), contentType, downloadName);
        }


       
    }
}
