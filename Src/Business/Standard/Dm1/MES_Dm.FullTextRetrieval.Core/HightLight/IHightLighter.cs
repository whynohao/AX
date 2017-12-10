/**********************************************************************
 * CopyRight 2016 杭州集控科技有限公司 版权所有
 * 功能描述：关键字高亮接口
 * 创建标识：CHENQI 2016/12/01
 * 修改标识：
 * 修改描述：
************************************************************************/
using MES_Dm.FullTextRetrieval.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MES_Dm.FullTextRetrieval.Core.HightLight
{
    /// <summary>
    /// 对关键字进行高亮处理的接口
    /// </summary>
    public interface IHightLighter
    {
        AbstractFileBase InitHightLight(Dictionary<string, string> keywords, AbstractFileBase t);

        //HashSet<HightLightField> GetHightLightFields();

        void SetHightLightFields(List<HightLightField> hightLightFields);
    }

    /// <summary>
    /// 可以高亮显示的字段
    /// </summary>
    public enum HightLightField
    {
        FileName,
        FilePath,
        CreateTime,
        Content,
        UpLoadPersonId
    }
}
