/**********************************************************************
 * CopyRight 2016 杭州集控科技有限公司 版权所有
 * 功能描述：工厂类，创建对象
 * 创建标识：CHENQI 2016/12/01
 * 修改标识：
 * 修改描述：
************************************************************************/
using MES_Dm.FullTextRetrieval.Core.HightLight;
using MES_Dm.FullTextRetrieval.Core.Index;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MES_Dm.FullTextRetrieval.Core
{
    public class IndexManagerFactory : IIndexManagerFactory
    {
        /// <summary>
        /// 产生一个索引管理者
        /// </summary>
        /// <returns></returns>
        public IIndexManager Create()
        {
            return new IndexManagerImp();
        }
    }

    public class HightLighterFactory : IHighLightFactory
    {
        /// <summary>
        /// 产生一个关键字高亮的处理者
        /// </summary>
        /// <returns></returns>
        public IHightLighter Create()
        {
            return new HightLighterImp();
        }
    }
}
