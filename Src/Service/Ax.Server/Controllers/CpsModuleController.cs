using AxCRL.Comm.Runtime;
using MES_Sys.UtilsBcf.Cps;
using MES_Sys.UtilsBcf.Cps.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AxCRL.Comm.Entity;
using AxCRL.Bcf;

namespace Ax.Server.Controllers
{
    /// <summary>
    /// CPS建模
    /// </summary>
    public class CpsModuleController : Controller
    {
        /// <summary>
        /// 检查CPS组件图标库中是否与当前上传的图标名称重复，若是返回true，否则返回false
        /// </summary>
        /// <returns>返回值为 true:CPS组件图标库中已存在此图标;false:CPS组件图标库中不存在此图标</returns>
        public string CheckIcon()
        {
            string path = CPSModuleConfig.GetCPSIconPath();
            string filePath = Path.Combine(path, Request.Form["RealFileName"]);
            return System.IO.File.Exists(filePath) ? "true" : "false";
        }

        /// <summary>
        /// 将临时上传的图标转移到CPS组件图标库中
        /// </summary>
        public void MoveIcon()
        {
            string path = CPSModuleConfig.GetCPSIconPath();

            string orgFileName = Request.Form["FileName"];
            string orgFilePath = Path.Combine(EnvProvider.Default.DocumentsPath, "Temp", orgFileName);
            string fileName = Request.Form["RealFileName"];
            string moveFilePath = Path.Combine(path, fileName);
            System.IO.File.Move(orgFilePath, moveFilePath);
        }

        /// <summary>
        /// 获取动态CPS组件
        /// </summary>
        /// <param name="controlConfigId">CPS组件配置标识</param>
        /// <returns>返回JSON格式的数据</returns>
        public JsonResult GetCurrentConfig(string controlConfigId, int factoryModuleType)
        {
            CPSControlConfigModel controlConfigList = CPSModuleConfig.GetCurrentConfig(controlConfigId, factoryModuleType);
            return Json(controlConfigList, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取最新有效的CPS产线配置数据
        /// </summary>
        /// <param name="produceControlLineId">>CPS产线配置标识</param>
        /// <returns>返回JSON格式的数据</returns>
        public JsonResult GetProduceControlLineModel(string produceControlLineId)
        {
            CPSProduceControlLineConfig produceControlLineModel = CPSModuleConfig.GetProduceControlLineModel(produceControlLineId);
            return Json(produceControlLineModel, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        ///验证产线配置规则，并保存为CPS产线模型数据和产线配置数据
        /// </summary>
        /// <param name="info">CPS产线配置模型数据</param>
        /// <returns>返回保存后的CPS产线配置模型数据以及错误信息</returns>
        public JsonResult SaveProduceControlLine(CPSProduceControlLineConfig info)
        {
            ExecuteBcfMethodResult result = new ExecuteBcfMethodResult();
            LibManagerMessage manageMessage = new LibManagerMessage();
            try
            {
                var model = CPSModuleConfig.SaveProduceControlLine(info, manageMessage);
                result.Result = model;
                result.Messages = manageMessage.MessageList;
            }
            catch (Exception ex)
            {
                string message = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                manageMessage.AddMessage(LibMessageKind.SysException, string.Format("异常信息:{0}{1}异常堆栈:{2}", message, Environment.NewLine, ex.StackTrace));
                result.Messages = manageMessage.MessageList;
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获得生产线下所有工作站点的派工单的完成情况
        /// </summary>
        /// <param name="info">CPS产线配置模型数据</param>
        /// <returns>返回生产线下的工作站点-派工单的信息</returns>
        public JsonResult GetProduceLineOfStationInfo(CPSProduceControlLineConfig info)
        {
            ExecuteBcfMethodResult result = new ExecuteBcfMethodResult();
            LibManagerMessage manageMessage = new LibManagerMessage();
            List<ProduceModuleInfo> produceControlLineModel = CPSModuleConfig.GetProduceLineOfStationInfo(info, manageMessage);
            result.Result = produceControlLineModel;
            result.Messages = manageMessage.MessageList;
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 根据当前标准工艺路线的ID返回其本身或复制的数据模型
        /// </summary>
        /// <param name="techRouteId">标准工艺路线的唯一标识</param>
        /// <param name="materialId">物料的唯一标识</param>
        /// <param name="isCopy">是否要复制当前标准工艺路线的数据模型，true表示复制，false表示不复制</param>
        /// <returns>返回当前标准工艺路线本身或复制的组件布局数据模型</returns>
        public JsonResult GetTechRouteInfoById(string techRouteId, string materialId, bool isCopy)
        {
            ExecuteBcfMethodResult result = new ExecuteBcfMethodResult();
            LibManagerMessage manageMessage = new LibManagerMessage();
            TechRouteModel techRouteModel = CPSModuleConfig.GetTechRouteInfoById(techRouteId, materialId, isCopy, manageMessage);
            result.Result = techRouteModel;
            result.Messages = manageMessage.MessageList;
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获得当前标准工艺路线的数据模型
        /// </summary>
        /// <param name="techRouteId">CPS标准工艺路线的关联主数据ID，当其为空时，标准工艺路线进行新增操作，否则进行修改操作</param>
        /// <param name="techRouteControlId">CPS标准工艺路线的组件ID</param>
        /// <param name="info">CPS标准工艺路线的建模模型数据</param>
        /// <returns>返回标准工艺路线的数据模型</returns>
        public JsonResult GetCurrentTechRouteInfo(string techRouteId, string techRouteControlId, CPSProduceControlLineConfig info)
        {
            TechRouteModel techRouteModel = CPSModuleConfig.GetCurrentTechRouteInfo(techRouteId, techRouteControlId, info);
            return Json(techRouteModel, JsonRequestBehavior.AllowGet);
        }
    }
}
