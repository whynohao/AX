using AxCRL.Comm.Runtime;
using AxCRL.Comm.Utils;
using AxCRL.Core.Cache;
using AxCRL.Core.Comm;
using AxCRL.Core.Permission;
using AxCRL.Data;
using AxCRL.Services;
using AxCRL.Template;
using AxCRL.Template.DataSource;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace AxCRL.Services
{
    public class FileTransferService : IFileTransferService
    {

        private UpLoadFileResult UpLoad(System.IO.Stream file, string dirPath, string fileName = "")
        {
            UpLoadFileResult upLoadFileResult = new UpLoadFileResult();
            string encodingName = "utf-8";
            using (MemoryStream ms = new MemoryStream())
            {
                file.CopyTo(ms);
                ms.Position = 0;

                var encoding = Encoding.GetEncoding(encodingName);
                var reader = new StreamReader(ms, encoding);
                var headerLength = 0L;

                //读取第一行  
                var firstLine = reader.ReadLine();
                //计算偏移（字符串长度+回车换行2个字符）  
                headerLength += encoding.GetBytes(firstLine).LongLength + 2;

                //读取第二行  
                var secondLine = reader.ReadLine();
                //计算偏移（字符串长度+回车换行2个字符）  
                headerLength += encoding.GetBytes(secondLine).LongLength + 2;
                //解析文件名  
                string orgFileName = new System.Text.RegularExpressions.Regex("filename=\"(?<fn>.*)\"").Match(secondLine).Groups["fn"].Value;
                if (string.IsNullOrEmpty(fileName))
                    fileName = orgFileName;
                else
                    fileName = orgFileName.Replace(orgFileName.Substring(0, orgFileName.LastIndexOf('.')), fileName);
                //一直读到空行为止  
                while (true)
                {
                    //读取一行  
                    var line = reader.ReadLine();
                    //若到头，则直接返回  
                    if (line == null)
                        break;
                    //若未到头，则计算偏移（字符串长度+回车换行2个字符）  
                    headerLength += encoding.GetBytes(line).LongLength + 2;
                    if (line == "")
                        break;
                }

                //设置偏移，以开始读取文件内容  
                ms.Position = headerLength;
                ////减去末尾的字符串：“/r/n--/r/n”  
                ms.SetLength(ms.Length - encoding.GetBytes(firstLine).LongLength - 3 * 2);
                if (!Directory.Exists(dirPath))
                    Directory.CreateDirectory(dirPath);
                string path = Path.Combine(dirPath, fileName);
                using (FileStream fileToupload = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read))
                {
                    ms.CopyTo(fileToupload);
                    fileToupload.Flush();
                }
                upLoadFileResult.FileName = fileName;
                upLoadFileResult.success = true;
            }
            return upLoadFileResult;
        }

        private UpLoadFileResult UpLoadPicture(System.IO.Stream file, string dirPath, string fileName = "")
        {
            UpLoadFileResult upLoadFileResult = new UpLoadFileResult();
            string encodingName = "utf-8";
            string imgType = string.Empty;
            List<string> typeList = new List<string>() { "JPG","BMP","PNG","JPEG"};
            using (MemoryStream ms = new MemoryStream())
            {
                file.CopyTo(ms);
                ms.Position = 0;

                var encoding = Encoding.GetEncoding(encodingName);
                var reader = new StreamReader(ms, encoding);
                var headerLength = 0L;

                //读取第一行  
                var firstLine = reader.ReadLine();
                //计算偏移（字符串长度+回车换行2个字符）  
                headerLength += encoding.GetBytes(firstLine).LongLength + 2;

                //读取第二行  
                var secondLine = reader.ReadLine();
                //计算偏移（字符串长度+回车换行2个字符）  
                headerLength += encoding.GetBytes(secondLine).LongLength + 2;
                //解析文件名  
                string orgFileName = new System.Text.RegularExpressions.Regex("filename=\"(?<fn>.*)\"").Match(secondLine).Groups["fn"].Value;
                if (string.IsNullOrEmpty(fileName))
                    fileName = orgFileName;
                else
                    fileName = orgFileName.Replace(orgFileName.Substring(0, orgFileName.LastIndexOf('.')), fileName);
                //判断图片格式
                imgType=fileName.Substring(fileName.LastIndexOf('.')+1).ToUpper();
                if (!typeList.Contains(imgType))
                {
                    upLoadFileResult.FileName = "errorType";
                    upLoadFileResult.success = false;
                    return upLoadFileResult;
                }  
                //一直读到空行为止  
                while (true)
                {
                    //读取一行  
                    var line = reader.ReadLine();
                    //若到头，则直接返回  
                    if (line == null)
                        break;
                    //若未到头，则计算偏移（字符串长度+回车换行2个字符）  
                    headerLength += encoding.GetBytes(line).LongLength + 2;
                    if (line == "")
                        break;
                }

                //设置偏移，以开始读取文件内容  
                ms.Position = headerLength;
                ////减去末尾的字符串：“/r/n--/r/n”  
                ms.SetLength(ms.Length - encoding.GetBytes(firstLine).LongLength - 3 * 2);
                if (!Directory.Exists(dirPath))
                    Directory.CreateDirectory(dirPath);
                string path = Path.Combine(dirPath, fileName);
                using (FileStream fileToupload = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read))
                {
                    ms.CopyTo(fileToupload);
                    fileToupload.Flush();
                }
                upLoadFileResult.FileName = fileName;
                upLoadFileResult.success = true;
            }
            return upLoadFileResult;
        }
        public UpLoadFileResult UpLoadFile(Stream stream)
        {
            string dirPath = Path.Combine(EnvProvider.Default.RuningPath, "TempData", "ImportData");
            return UpLoad(stream, dirPath);
        }

        public void DeleteExportFile(string fileName)
        {
            string path = Path.Combine(EnvProvider.Default.RuningPath, "TempData", "ExportData", fileName);
            if (File.Exists(path))
                File.Delete(path);
        }

        public UpLoadFileResult UpLoadUserPicture(Stream stream)
        {
            string dirPath = Path.Combine(EnvProvider.Default.RuningPath, "UserPicture");
            return UpLoadPicture(stream, dirPath, LibCommUtils.GetInternalId().ToString());
        }


        public string MoveUserPicture(string progId, string internalId, string fileName)
        {
            string path = Path.Combine(EnvProvider.Default.RuningPath, "UserPicture", progId, internalId);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            string orgFilePath = Path.Combine(EnvProvider.Default.RuningPath, "UserPicture", fileName);
            DirectoryInfo info = new DirectoryInfo(path);
            FileInfo[] files = info.GetFiles();
            foreach (var item in files)
            {
                item.Delete();
            }
            File.Move(orgFilePath, Path.Combine(path, fileName));
            LibDataAccess dataAccess = new LibDataAccess();
            LibSqlModel model = LibSqlModelCache.Default.GetSqlModel(progId);
            dataAccess.ExecuteNonQuery(string.Format("update {0} set IMGSRC={1} where INTERNALID={2}", model.Tables[0].TableName,
                LibStringBuilder.GetQuotString(fileName), LibStringBuilder.GetQuotString(internalId)));
            return fileName;
        }

        public void RemoveUserPicture(string progId, string internalId, string fileName)
        {
            string path = Path.Combine(EnvProvider.Default.RuningPath, "UserPicture", progId, internalId, fileName);
            if (File.Exists(path))
            {
                File.Delete(path);
                LibDataAccess dataAccess = new LibDataAccess();
                LibSqlModel model = LibSqlModelCache.Default.GetSqlModel(progId);
                dataAccess.ExecuteNonQuery(string.Format("update {0} set IMGSRC='' where INTERNALID={1}", model.Tables[0].TableName,
                     LibStringBuilder.GetQuotString(internalId)), false);
            }
        }


        public void SaveMenuSetting(string handle, string menuData)
        {
            LibHandle libHandle = LibHandleCache.Default.GetCurrentHandle(handle) as LibHandle;
            if (libHandle == null)
            {
                throw new Exception("用户句柄无效。");
            }
            string path = Path.Combine(EnvProvider.Default.MainPath, "Scheme", "MenuSetting", libHandle.UserId);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            using (FileStream fs = new FileStream(Path.Combine(path, string.Format("{0}.json", libHandle.UserId)), FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(menuData);
                }
            }
        }

        public string LoadMenuSetting(string handle, bool setting = false)
        {
            string menuData = string.Empty;
            LibHandle libHandle = LibHandleCache.Default.GetCurrentHandle(handle) as LibHandle;
            if (libHandle == null)
            {
                throw new Exception("用户句柄无效。");
            }
            string path = Path.Combine(EnvProvider.Default.MainPath, "Scheme", "MenuSetting", libHandle.UserId, string.Format("{0}.json", libHandle.UserId));
            if (!File.Exists(path))
                path = Path.Combine(EnvProvider.Default.MainPath, "Scheme", "MenuSetting", "admin", "admin.json");
            if (File.Exists(path))
            {
                using (FileStream fs = new FileStream(path, FileMode.Open))
                {
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        menuData = sr.ReadToEnd();
                    }
                }
            }
            if (!setting)
                PowerFliter(libHandle, ref menuData);
            return menuData;
        }

        private void PowerFliter(LibHandle handle, ref string data)
        {
            MenuTree obj = JsonConvert.DeserializeObject(data, typeof(MenuTree)) as MenuTree;
            if (obj != null)
            {
                var childrenList = obj.children;
                if (childrenList != null && childrenList.Count > 0)
                {
                    for (int i = childrenList.Count - 1; i >= 0; i--)
                    {
                        PowerControl(handle, childrenList, childrenList[i]);
                    }
                    data = JsonConvert.SerializeObject(obj);
                }
            }
        }

        private void PowerControl(LibHandle handle, List<MenuTree> list, MenuTree subObj)
        {
            var childrenList = subObj.children;
            if (childrenList != null && childrenList.Count > 0)
            {
                for (int i = childrenList.Count - 1; i >= 0; i--)
                {
                    PowerControl(handle, childrenList, childrenList[i]);
                }
            }
            else if (!string.IsNullOrEmpty(subObj.PROGID))
            {
                if (!LibPermissionControl.Default.CanUse(handle, subObj.PROGID))
                {
                    list.Remove(subObj);
                }
            }
        }



        public UpLoadFileResult UpLoadAttach(Stream stream)
        {
            string fileName = DateTime.Now.Ticks.ToString();
            string path = Path.Combine(EnvProvider.Default.MainPath, "AxFile", "Attachment");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return UpLoad(stream, path, fileName);
        }


        public void UpdateAttachStruct(LibAttachData attachData, List<string> listSql)
        {
            if (string.IsNullOrEmpty(attachData.AttachSrc))
            {
                attachData.AttachSrc = LibCommUtils.GetInternalId().ToString();
                StringBuilder builder = new StringBuilder();
                foreach (var item in attachData.PkList)
                {
                    if (item.Value.GetType() == typeof(string))
                        builder.AppendFormat("{0} = {1} and ", item.Key, LibStringBuilder.GetQuotObject(item.Value));
                    else
                        builder.AppendFormat("{0} = {1} and ", item.Key, item.Value);
                }
                if (builder.Length > 0)
                    builder.Remove(builder.Length - 4, 4);
                //更新单据附件关联字段
                listSql.Add(string.Format("update {0} set ATTACHMENTSRC='{1}' where {2}", attachData.TableName, attachData.AttachSrc, builder.ToString()));
            }
            foreach (var item in attachData.AttachList)
            {
                switch (item.Status)
                {
                    case LibAttachStatus.Add:
                        listSql.Add(string.Format("insert into AXPATTACHMENTRECORD(BELONGTOID,ORDERID,ORDERNUM,ATTACHMENTNAME,CANUSE) values('{0}',{1},{2},'{3}',1)",
                            attachData.AttachSrc, item.OrderId, item.OrderNum, item.AttachmentName));
                        break;
                    case LibAttachStatus.Modif:
                        listSql.Add(string.Format("update AXPATTACHMENTRECORD set ORDERNUM={2},ATTACHMENTNAME='{3}' where BELONGTOID='{0}' and ORDERID={1}",
                            attachData.AttachSrc, item.OrderId, item.OrderNum, item.AttachmentName));
                        break;
                    case LibAttachStatus.Delete:
                        listSql.Add(string.Format("update AXPATTACHMENTRECORD set CANUSE=0 where BELONGTOID='{0}' and ORDERID={1}",
                            attachData.AttachSrc, item.OrderId));
                        break;
                }
            }
        }

        public string MoveAttach(LibAttachData attachData)
        {
            LibDataAccess dataAccess = new LibDataAccess();
            List<string> listSql = new List<string>();
            //更新结构
            UpdateAttachStruct(attachData, listSql);
            //更新附件记录明细
            listSql.Add(string.Format("insert into AXPATTACHMENTRECORDDETAIL(BELONGTOID,ORDERID,FILENAME,PERSONID,CREATETIME) values('{0}',{1},'{2}','{3}',{4})",
                attachData.AttachSrc, attachData.OrderId, attachData.FileName, attachData.PersonId, LibDateUtils.GetCurrentDateTime()));

            LibDBTransaction trans = dataAccess.BeginTransaction();
            try
            {
                dataAccess.ExecuteNonQuery(listSql);
                trans.Commit();
            }
            catch
            {
                trans.Rollback();
                throw;
            }

            string path = Path.Combine(EnvProvider.Default.MainPath, "AxFile", "Attachment", attachData.ProgId, attachData.AttachSrc);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            string orgFilePath = Path.Combine(EnvProvider.Default.MainPath, "AxFile", "Attachment", attachData.FileName);
            File.Move(orgFilePath, Path.Combine(path, attachData.FileName));
            return attachData.AttachSrc;
        }

        public void RemoveAttach(string attachSrc, int orderId, string personId)
        {
            LibDataAccess dataAccess = new LibDataAccess();
            dataAccess.ExecuteNonQuery(string.Format("insert into AXPATTACHMENTRECORDDETAIL(BELONGTOID,ORDERID,FILENAME,PERSONID,CREATETIME) values('{0}',{1},'{2}','{3}',{4})",
                attachSrc, orderId, string.Empty, personId, LibDateUtils.GetCurrentDateTime()));
        }


        public string SaveAttachStruct(LibAttachData attachData)
        {
            if (attachData.AttachList.Count == 0)
                return string.Empty;
            LibDataAccess dataAccess = new LibDataAccess();
            List<string> listSql = new List<string>();
            UpdateAttachStruct(attachData, listSql);
            LibDBTransaction trans = dataAccess.BeginTransaction();
            try
            {
                dataAccess.ExecuteNonQuery(listSql);
                trans.Commit();
            }
            catch
            {
                trans.Rollback();
                throw;
            }
            return attachData.AttachSrc;
        }

        public void DownloadAttach(string progId, string attachSrc, string fileName)
        {
            string orgFilePath = Path.Combine(EnvProvider.Default.MainPath, "AxFile", "Attachment", progId, attachSrc, fileName);
            string path = Path.Combine(EnvProvider.Default.RuningPath, "TempData", "Attachment");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            File.Copy(orgFilePath, Path.Combine(path, fileName), true);
        }


        public UpLoadFileResult UpLoadWallpaper(Stream stream)
        {
            string dirPath = Path.Combine(EnvProvider.Default.RuningPath, "Wallpapers");
            return UpLoad(stream, dirPath);
        }

        public string MoveWallpaper(string handle, string fileName)
        {
            LibHandle libHandle = LibHandleCache.Default.GetCurrentHandle(handle) as LibHandle;
            if (libHandle == null)
            {
                throw new Exception("用户句柄无效。");
            }
            string path = Path.Combine(EnvProvider.Default.RuningPath, "Wallpapers", libHandle.UserId);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            string orgFilePath = Path.Combine(EnvProvider.Default.RuningPath, "Wallpapers", fileName);
            File.Move(orgFilePath, Path.Combine(path, fileName));
            return fileName;
        }


        public void RemoveWallpaper(string handle, string fileName)
        {
            LibHandle libHandle = LibHandleCache.Default.GetCurrentHandle(handle) as LibHandle;
            if (libHandle == null)
            {
                throw new Exception("用户句柄无效。");
            }
            string path = Path.Combine(EnvProvider.Default.RuningPath, "Wallpapers", libHandle.UserId, fileName);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        /// <summary>
        /// 文档管理模块的文档上传
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public UpLoadFileResult UpLoadDoc(Stream stream)
        {
            string fileName = DateTime.Now.Ticks.ToString();
            string path = Path.Combine(EnvProvider.Default.DocumentsPath, "Temp");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return UpLoad(stream, path, fileName);
        }
        /// <summary>
        /// 安卓APK上传
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public UpLoadFileResult UpLoadApk(Stream stream)
        {
            string fileName = DateTime.Now.Ticks.ToString();
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PDAapk");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return UpLoad(stream, path, fileName);
        }
    }

    /// <summary>
    /// Menu树结构
    /// </summary>
    public class MenuTree
    {
        public string parentId { get; set; }
        public string MENUITEM { get; set; }
        public string PROGID { get; set; }
        public string PROGNAME { get; set; }
        public int BILLTYPE { get; set; }
        public string ENTRYPARAM { get; set; }
        public bool ISVISUAL { get; set; }
        public string CONDITION { get; set; }
        public string id { get; set; }
        public bool leaf { get; set; }
        public List<MenuTree> children { get; set; }
    }
}
