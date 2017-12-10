using System;
using System.Web.Mvc;
using PageOffice;
using System.IO;
using AxCRL.Data;
using AxCRL.Comm.Utils;
using System.Web;
using System.Collections.Generic;
using AxCRL.Core.Comm;
using AxCRL.Core.Cache;
using Jikon.MES_Dm.DMCommon;
using MES_Dm.FullTextRetrieval.Core.Util;
using AxCRL.Data.SqlBuilder;

namespace Ax.Server.Controllers
{
    public class DocumentController : Controller
    {
        //
        // GET: /Document/
        public ActionResult ReadOnly()
        {
            bool isRead = false;
            bool canPrint = false;
            bool canDownload = false;
            //string userName = string.Empty;
            string fileFullPath = string.Empty;

            int modifyVerionId = -1;
            string fileId = Request.QueryString["fileId"];
            string userHandle = Request.QueryString["userHandle"];
            if (!int.TryParse(Request.QueryString["modifyVerionId"], out modifyVerionId))
            {
                return View("ArgumentsError");
                throw new Exception("参数错误");
            }
            if (string.IsNullOrEmpty(fileId) || string.IsNullOrEmpty(userHandle))
            {
                return View("ArgumentsError");
                throw new Exception("参数错误");
            }
            OpenModeType type = OpenModeType.docReadOnly; //默认为word文档

            LibHandle libHandle = LibHandleCache.Default.GetCurrentHandle(userHandle) as LibHandle;
            if (libHandle == null)
            {
                return View("ArgumentsError");
                throw new Exception("参数错误");
            }
            string personName = libHandle.PersonName;

            #region 获取权限
            List<DMFuncPermissionEnum> permissionList = DMPermissionControl.Default.GetPermissionOf(userHandle, fileId);

            if (permissionList.Contains(DMFuncPermissionEnum.Read))
            {
                isRead = true;
            }
            if (permissionList.Contains(DMFuncPermissionEnum.Download))
            {
                canDownload = true;
            }

            if (permissionList.Contains(DMFuncPermissionEnum.Print))
            {
                canPrint = true;
            }
            #endregion

            #region 获取文件路径
            DirLinkAddress dirLink = new DirLinkAddress(fileId);
            fileFullPath = dirLink.GetDocFullPath(modifyVerionId);
            #endregion

            #region 根据扩展名，确定打开方式
            string extName = dirLink.DocType;
            if (extName == ".doc" || extName == ".docx")
            {
                type = OpenModeType.docReadOnly;
            }
            else if (extName == ".pptx" || extName == ".ppt")
            {
                type = OpenModeType.pptReadOnly;
            }
            else if (extName == ".xlsx" || extName == ".xls")
            {
                type = OpenModeType.xlsReadOnly;
            }
            else if (extName == ".txt")
            {
                StreamReader sr = new StreamReader(fileFullPath, FileEncoding.GetType(fileFullPath));
                string content = string.Empty;
                string temp = string.Empty;
                while ((temp = sr.ReadLine()) != null)
                {
                    content += temp;
                    content += "\n";
                }

                sr.Close();
                if (isRead)
                {
                    ViewData["content"] = content;
                    //ViewData["currentPage"] = pageNum;
                    ViewData["canPrint"] = canPrint;
                    return View("ReadTxt");
                }
            }
            else if (extName == ".pdf")
            {
                if (isRead)
                {
                    ViewData["fileId"] = fileId;
                    ViewData["filename"] = dirLink.DocName;
                    ViewData["canPrint"] = canPrint;
                    ViewData["canDownload"] = canDownload;
                    return View("Viewer");
                }
            }
            else if (extName == ".png" || extName == ".gif" || extName == ".jpg" || extName == ".bmp" || extName == ".jpeg")
            {
                if (isRead)
                {
                    ViewData["DocId"] = fileId;
                    ViewData["DocName"] = dirLink.DocName;
                    ViewData["UserHandle"] = userHandle;
                    ViewData["modifyVerId"] = modifyVerionId;
                    return View("ReadImg");
                }
            }
            else
            {
                return View("NoSupportError");
                throw new Exception("不知道如何浏览文件");
            }
            #endregion

            //是否可以浏览
            if (isRead)
            {
                ViewData["docName"] = fileFullPath;
                ViewData["OpenModeType"] = type;
                ViewData["userName"] = personName;
                return View();
            }
            else
            {
                return View("PermissionError");
                throw new Exception("无权限");
            }
        }

        public ActionResult Edit()
        {
            bool isAllow = false;
            string fileFullPath = string.Empty;
            string fileId = Request.QueryString["fileId"];
            string userHandle = Request.QueryString["userHandle"];
            string dirId = Request.QueryString["dirId"];
            string dirType = Request.QueryString["dirType"];
            string isChrome = Request.QueryString["isChrome"];
            int modifyVerionId = -1;
            //if (!int.TryParse(Request.Form["modifyVerionId"], out modifyVerionId))
            //{
            //    throw new Exception("参数错误");
            //}
            if (string.IsNullOrEmpty(fileId) || string.IsNullOrEmpty(userHandle) || string.IsNullOrEmpty(dirId) || string.IsNullOrEmpty(dirType) || string.IsNullOrEmpty(isChrome))
            {
                return View("ArgumentsError");
                throw new Exception("参数错误");
            }

            OpenModeType type = OpenModeType.docNormalEdit; //默认为word文档

            #region 用户判断
            LibHandle libHandle = LibHandleCache.Default.GetCurrentHandle(userHandle) as LibHandle;
            if (libHandle == null)
            {
                return View("ArgumentsError");
                throw new Exception("参数错误");
            }
            string personName = libHandle.PersonName;
            #endregion

            #region 权限判断
            List<DMFuncPermissionEnum> permissionList = DMPermissionControl.Default.GetPermissionOf(userHandle, fileId);
            if (permissionList.Contains(DMFuncPermissionEnum.Edit))
            {
                isAllow = true;
            }
            #endregion

            #region 获取文件路径
            DirLinkAddress dirLink = new DirLinkAddress(fileId);
            fileFullPath = dirLink.GetDocFullPath(modifyVerionId);
            #endregion

            #region 判断是否锁定文档，未锁定可以编辑
            #endregion

            #region 根据扩展名，确定打开方式
            string extName = dirLink.DocType;
            if (extName == ".doc" || extName == ".docx")
            {
                type = OpenModeType.docNormalEdit;
            }
            else if (extName == ".pptx" || extName == ".ppt")
            {
                type = OpenModeType.pptNormalEdit;
            }
            else if (extName == ".xlsx" || extName == ".xls")
            {
                type = OpenModeType.xlsNormalEdit;
            }
            else if (extName == ".txt")
            {
                ViewBag.Contents = System.IO.File.ReadAllText(fileFullPath, FileEncoding.GetType(fileFullPath));
                ViewBag.FileId = fileId;
                ViewBag.UserHandle = userHandle;
                ViewBag.NewFileName = Guid.NewGuid().ToString() + extName;
                ViewBag.DirId = dirId;
                ViewBag.DirType = dirType;
                return View("EditTxt");
            }
            else
            {
                return View("NoSupportError");
                throw new Exception("无法打开该文件");
            }
            #endregion

            if (isAllow)
            {
                ViewBag.FileFullPath = fileFullPath;
                ViewBag.UserName = personName;
                ViewBag.FileId = fileId;
                ViewBag.DirId = dirId;
                ViewBag.DirType = dirType;

                ViewBag.OpenModeType = type;
                ViewBag.UserHandle = userHandle;
                ViewBag.NewFileName = Guid.NewGuid().ToString() + extName;
                ViewBag.IsChrome = isChrome;
                return View();
            }
            else
            {
                return View("PermissionError");
                throw new Exception("无编辑权限");
            }
        }

        public string Save()
        {
            //string userName = string.Empty;
            string fileFullPath = string.Empty;
            string fileId = Request.QueryString["fileId"];
            string userHandle = Request.QueryString["userHandle"];
            string fileName = Request.QueryString["fileName"];
            bool isAllow = false;

            if (string.IsNullOrEmpty(userHandle) || string.IsNullOrEmpty(fileId) || string.IsNullOrEmpty(fileName))
            {
                return "参数错误";
                throw new Exception("参数错误");
            }

            #region 权限判断
            List<DMFuncPermissionEnum> permissionList = DMPermissionControl.Default.GetPermissionOf(userHandle, fileId);
            if (permissionList.Contains(DMFuncPermissionEnum.Edit))
            {
                isAllow = true;
            }
            #endregion

            //获取文件保存路径
            fileFullPath = Path.Combine(DMCommonMethod.GetDMRootTempPath(), fileName);

            if (isAllow)
            {
                FileSaver fs = new FileSaver();
                fs.SaveToFile(fileFullPath);
                //fs.CustomSaveResult = "ok";
                fs.Close();
                return "ok";
            }
            else
            {
                //return View("PermissionError");
                throw new Exception("无编辑权限,无法保存");
            }
        }

        public string SaveTxt()
        {
            string fileFullPath = string.Empty;
            string fileId = Request.Form["fileId"];
            string userHandle = Request.Form["userHandle"];
            string fileName = Request.Form["fileName"];
            string contents = Request.Form["contents"];
            bool isAllow = false;

            if (string.IsNullOrEmpty(contents) || string.IsNullOrEmpty(fileId) || string.IsNullOrEmpty(userHandle))
            {
                return "参数错误";
                throw new Exception("参数错误");
            }

            #region 权限判断
            List<DMFuncPermissionEnum> permissionList = DMPermissionControl.Default.GetPermissionOf(userHandle, fileId);
            if (permissionList.Contains(DMFuncPermissionEnum.Edit))
            {
                isAllow = true;
            }
            #endregion

            //获取文件保存路径
            fileFullPath = Path.Combine(DMCommonMethod.GetDMRootTempPath(), fileName);

            if (isAllow)
            {
                StreamWriter sw = null;

                try
                {
                    sw = new StreamWriter(fileFullPath);
                    foreach (char c in contents)
                    {
                        if (c == '\n')
                        {
                            sw.Write('\r');
                        }
                        sw.Write(c);
                    }
                    sw.Flush();
                }
                catch (Exception)
                {
                    return "保存失败";
                    throw;
                }
                finally
                {
                    if (sw != null)
                    {
                        sw.Close();
                    }
                }

                return "ok";
            }
            else
            {
                throw new Exception("无编辑权限,无法保存");
            }
        }

        public ActionResult Create()
        {
            bool isAllow = false;
            string dirId = Request.QueryString["dirId"];
            string dirType = Request.QueryString["dirType"];
            string userHandle = Request.QueryString["userHandle"];
            string docType = Request.QueryString["docType"];
            string isChrome = Request.QueryString["isChrome"];
            string realFileName = Request.QueryString["realfileName"];
            if (string.IsNullOrEmpty(userHandle) || string.IsNullOrEmpty(dirId) || string.IsNullOrEmpty(docType) || string.IsNullOrEmpty(isChrome) || string.IsNullOrEmpty(realFileName))
            {
                return View("ArgumentsError");
                throw new Exception("参数错误");
            }
            DocumentVersion type = DocumentVersion.Word2003; //默认为word文档

            #region 用户判断
            LibHandle libHandle = LibHandleCache.Default.GetCurrentHandle(userHandle) as LibHandle;
            if (libHandle == null)
            {
                return View("ArgumentsError");
                throw new Exception("参数错误");
            }
            string personName = libHandle.PersonName;
            #endregion

            #region 新增权限判断
            //List<DMFuncPermissionEnum> permissionList = DMPermissionControl.Default.GetPermissionOf(userHandle, dirId);
            //if (permissionList.Contains(DMFuncPermissionEnum.Add))
            //{
            //    isAllow = true;
            //}
            isAllow = true;
            #endregion

            #region 判断是否锁定文档，未锁定可以编辑
            #endregion

            #region 根据扩展名，确定打开方式
            string extName = docType;
            if (extName == ".doc")
            {
                type = DocumentVersion.Word2003;
            }
            else if (extName == ".docx")
            {
                type = DocumentVersion.Word2007;
            }
            else if (extName == ".ppt")
            {
                type = DocumentVersion.PowerPoint2003;
            }
            else if (extName == ".pptx")
            {
                type = DocumentVersion.PowerPoint2007;
            }
            else if (extName == ".xls")
            {
                type = DocumentVersion.Excel2003;
            }
            else if (extName == ".xlsx")
            {
                type = DocumentVersion.Excel2007;
            }
            else if (extName == ".txt")
            {
                ViewBag.UserHandle = userHandle;
                ViewBag.NewFileName = Guid.NewGuid().ToString() + extName;
                ViewBag.DirId = dirId;
                ViewBag.DirType = dirType;
                ViewBag.RealFileName = realFileName;
                return View("CreateTxt");
            }
            else
            {
                return View("NoSupportError");
                throw new Exception("无法新建该文件");
            }
            #endregion

            if (isAllow)
            {
                ViewBag.UserName = personName;
                ViewBag.DocumentVersion = type;
                ViewBag.DirId = dirId;
                ViewBag.NewFileName = Guid.NewGuid().ToString() + docType;
                ViewBag.UserHandle = userHandle;
                ViewBag.DirType = dirType;
                ViewBag.IsChrome = isChrome;
                ViewBag.RealFileName = realFileName;
                return View();
            }
            else
            {
                return View("PermissionError");
                throw new Exception("无新建权限");
            }
        }

        public string SaveNew()
        {
            //string userName = string.Empty;
            string fileFullPath = string.Empty;
            string userHandle = Request.QueryString["userHandle"];
            string fileName = Request.QueryString["fileName"];
            bool isAllow = false;

            if (string.IsNullOrEmpty(userHandle) || string.IsNullOrEmpty(fileName))
            {
                return "参数错误";
                throw new Exception("参数错误");
            }

            #region 新增权限判断
            isAllow = true;
            #endregion

            //获取文件保存路径
            fileFullPath = Path.Combine(DMCommonMethod.GetDMRootTempPath(), fileName);

            if (isAllow)
            {
                FileSaver fs = new FileSaver();
                fs.SaveToFile(fileFullPath);
                //fs.CustomSaveResult = "ok";
                fs.Close();
                return "ok";
            }
            else
            {
                throw new Exception("无编辑权限,无法保存");
            }
        }

        public string SaveNewTxt()
        {
            string fileFullPath = string.Empty;
            string userHandle = Request.Form["userHandle"];
            string fileName = Request.Form["fileName"];
            string contents = Request.Form["contents"];
            bool isAllow = false;

            if (string.IsNullOrEmpty(userHandle) || string.IsNullOrEmpty(fileName))
            {
                return "参数错误";
                throw new Exception("参数错误");
            }

            #region 新增权限判断
            isAllow = true;
            #endregion

            //获取文件保存路径
            fileFullPath = Path.Combine(DMCommonMethod.GetDMRootTempPath(), fileName);

            if (isAllow)
            {
                StreamWriter sw = new StreamWriter(fileFullPath);
                try
                {
                    foreach (char c in contents)
                    {
                        if (c == '\n')
                        {
                            sw.Write('\r');
                        }
                        sw.Write(c);
                    }
                    sw.Flush();
                }
                catch (Exception)
                {
                    return "保存失败";
                    throw;
                }
                finally
                {
                    if (sw != null)
                    {
                        sw.Close();
                    }
                }
                return "ok";
            }
            else
            {
                throw new Exception("无编辑权限,无法保存");
            }
        }

        public ActionResult Print()
        {
            bool isPrint = false;
            bool canDownload = false;
            //string userName = string.Empty;
            string fileFullPath = string.Empty;

            int modifyVerionId = -1;
            string fileId = Request.QueryString["fileId"];
            string userHandle = Request.QueryString["userHandle"];
            if (!int.TryParse(Request.QueryString["modifyVerionId"], out modifyVerionId))
            {
                return View("ArgumentsError");
                throw new Exception("参数错误");
            }
            if (string.IsNullOrEmpty(fileId) || string.IsNullOrEmpty(userHandle))
            {
                return View("ArgumentsError");
                throw new Exception("参数错误");
            }
            OpenModeType type = OpenModeType.docReadOnly; //默认为word文档

            LibHandle libHandle = LibHandleCache.Default.GetCurrentHandle(userHandle) as LibHandle;
            if (libHandle == null)
            {
                return View("ArgumentsError");
                throw new Exception("参数错误");
            }
            string personName = libHandle.PersonName;

            #region 获取权限
            List<DMFuncPermissionEnum> permissionList = DMPermissionControl.Default.GetPermissionOf(userHandle, fileId);

            if (permissionList.Contains(DMFuncPermissionEnum.Print))
            {
                isPrint = true;
            }

            if (permissionList.Contains(DMFuncPermissionEnum.Download))
            {
                canDownload = true;
            }
            #endregion

            #region 获取文件路径
            DirLinkAddress dirLink = new DirLinkAddress(fileId);
            fileFullPath = dirLink.GetDocFullPath(modifyVerionId);
            #endregion

            #region 根据扩展名，确定打开方式
            string extName = dirLink.DocType;
            if (extName == ".doc" || extName == ".docx")
            {
                type = OpenModeType.docReadOnly;
            }
            else if (extName == ".pptx" || extName == ".ppt")
            {
                type = OpenModeType.pptReadOnly;
            }
            else if (extName == ".xlsx" || extName == ".xls")
            {
                type = OpenModeType.xlsReadOnly;
            }
            else if (extName == ".txt")
            {
                if (isPrint)
                {
                    StreamReader sr = new StreamReader(fileFullPath, FileEncoding.GetType(fileFullPath));
                    string content = string.Empty;
                    string temp = string.Empty;
                    while ((temp = sr.ReadLine()) != null)
                    {
                        content += temp;
                        content += "\n";
                    }

                    sr.Close();
                    if (isPrint)
                    {
                        ViewData["content"] = content;
                        ViewData["canPrint"] = true;
                        return View("ReadTxt");
                    }
                }
            }
            else if (extName == ".pdf")
            {
                if (isPrint)
                {
                    ViewData["fileId"] = fileId;
                    ViewData["filename"] = dirLink.DocName;
                    ViewData["canPrint"] = true;
                    ViewData["canDownload"] = canDownload;
                    return View("Viewer");
                }
            }
            else
            {
                return View("NoSupportError");
                throw new Exception("不能打印该文件类型");
            }
            #endregion

            //是否可以打印
            if (isPrint)
            {
                ViewData["docName"] = fileFullPath;
                ViewData["OpenModeType"] = type;
                ViewData["userName"] = personName;
                ViewData["canDownload"] = canDownload;
                return View();
            }
            else
            {
                return View("PermissionError");
                throw new Exception("无打印权限");
            }
        }

        /// <summary>
        /// 浏览pdf文件
        /// </summary>
        /// <returns></returns>
        public FileStreamResult DownloadPdf()
        {
            string fileId = Request.QueryString["fileId"];
            if (fileId == null)
            {
                throw new Exception("参数错误");
            }
            // to do 权限判断
            string pdfFile = new DirLinkAddress(fileId).GetDocFullPath(0);
            //string pdfFile = "C:\\inetpub\\wwwroot\\doc\\10.pdf";
            return File(new FileStream(pdfFile, FileMode.Open), "application/pdf", Server.UrlEncode(Path.GetFileName(pdfFile)));
        }
        /// <summary>
        /// 获得文件的url
        /// </summary>
        /// <param name="url"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public string GetFileUrl()
        {
            string url = Request.Form["url"];
            string options = Request.Form["options"];
            if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(options))
            {
                throw new Exception("无效参数");
            }
            return PageOfficeLink.OpenWindow(url, options);
        }

        /// <summary>
        /// 下载文件
        /// 检查用户对指定文档的下载权限，如果有则将文件复制到临时目录然后通过文件流返回
        /// </summary>
        /// <returns></returns>
        public FileStreamResult Download()
        {
            string docId = string.Empty;
            int modifyVerId = -1;
            try
            {
                docId = Request.QueryString["DocId"];
                string userHandle = Request.QueryString["UserHandle"];
                string modifyVerIdStr = Request.QueryString["modifyVerId"];
                if (string.IsNullOrEmpty(docId) || string.IsNullOrEmpty(userHandle) || string.IsNullOrEmpty(modifyVerIdStr))
                    return null;

                if (int.TryParse(modifyVerIdStr, out modifyVerId) == false)
                    return null;

                string downloadName = string.Empty;
                string tempFilePath = DMCommonMethod.CheckAndGetDownloadTempFile(userHandle, docId, modifyVerId, out downloadName);
                var contentType = MimeMapping.GetMimeMapping(downloadName);
                return File(new FileStream(tempFilePath, FileMode.Open), contentType, downloadName);
            }
            catch (Exception exp)
            {
                DMCommonMethod.WriteLog("DocumentController.Download", string.Format("DocId:{0}\r\nModifyVerId:{1}\r\nError:{2}", docId, modifyVerId, exp.ToString()));
                return null;
            }
        }

        /// <summary>
        /// 使用用户名密码  下载文件
        /// 检查用户对指定文档的下载权限，如果有则将文件复制到临时目录然后通过文件流返回
        /// </summary>
        /// <returns></returns>
        public FileStreamResult DownloadForPassword()
        {
            string docId = string.Empty;
            int modifyVerId = -1;
            try
            {
                docId = Request.Form["docId"];
                string userId = Request.Form["userId"];
                string password = Request.Form["password"];
                string modifyVerIdStr = Request.Form["modifyVerId"];
                if (string.IsNullOrEmpty(docId) || string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(modifyVerIdStr) || string.IsNullOrEmpty(password))
                    return null;

                if (int.TryParse(modifyVerIdStr, out modifyVerId) == false)
                    return null;

                string downloadName = string.Empty;

                SqlBuilder builder = new SqlBuilder("axp.User");
                string sql = builder.GetQuerySql(0, "A.PERSONID", string.Format("A.USERID={0} And A.USERPASSWORD={1} And A.ISUSE=1", LibStringBuilder.GetQuotString(userId), LibStringBuilder.GetQuotString(password)));
                LibDataAccess dataAccess = new LibDataAccess();
                string personId = LibSysUtils.ToString(dataAccess.ExecuteScalar(sql));
                if (personId == null)
                {
                    throw new Exception("用户账户或密码错误");
                }

                //权限判断
                if (userId.ToLower() != "admin")
                {
                    DMUserPermission userPermission = DMUserPermissionCache.Default.GetCacheItem(personId);
                    if (!userPermission.CheckCan(string.Empty, docId, DMFuncPermissionEnum.Download))
                    {
                        throw new Exception("没有权限下载该文件");
                    }
                }


                DirLinkAddress dirlink = new DirLinkAddress(docId);
                downloadName = dirlink.DocName;
                //复制一份到临时目录
                string tempPath = Path.Combine(DMCommonMethod.GetDMRootTempPath(), string.Format("{0}_{1}", DateTime.Now.Ticks.ToString(), downloadName));
                string docFullPath = dirlink.GetDocFullPath(modifyVerId);
                if (System.IO.File.Exists(docFullPath))
                {
                    System.IO.File.Copy(docFullPath, tempPath);
                    var contentType = MimeMapping.GetMimeMapping(downloadName);
                    return File(new FileStream(tempPath, FileMode.Open), contentType, downloadName);
                }
                throw new Exception("文件不存在");
            }
            catch (Exception exp)
            {
                DMCommonMethod.WriteLog("DocumentController.Download", string.Format("DocId:{0}\r\nModifyVerId:{1}\r\nError:{2}", docId, modifyVerId, exp.ToString()));
                return null;
            }
        }
    }
}
