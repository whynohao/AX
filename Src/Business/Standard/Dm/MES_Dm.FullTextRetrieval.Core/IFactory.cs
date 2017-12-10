/**********************************************************************
 * CopyRight 2016 杭州集控科技有限公司 版权所有
 * 功能描述：抽象工厂接口
 * 创建标识：CHENQI 2016/12/06
 * 修改标识：
 * 修改描述：
************************************************************************/
using MES_Dm.FullTextRetrieval.Core.Index;
using MES_Dm.FullTextRetrieval.Core.HightLight;

namespace MES_Dm.FullTextRetrieval.Core
{
    public interface IIndexManagerFactory
    {
        IIndexManager Create();
    }

    public interface IHighLightFactory
    {
        IHightLighter Create();
    }

}
