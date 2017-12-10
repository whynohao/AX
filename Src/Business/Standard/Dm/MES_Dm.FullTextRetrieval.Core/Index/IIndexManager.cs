/**********************************************************************
 * CopyRight 2016 杭州集控科技有限公司 版权所有
 * 功能描述：管理索引的接口
 * 创建标识：CHENQI 2016/12/01
 * 修改标识：
 * 修改描述：
************************************************************************/
using MES_Dm.FullTextRetrieval.Core.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AxCRL.Core.Comm;

namespace MES_Dm.FullTextRetrieval.Core.Index
{
    /// <summary>
    /// 索引管理者的接口
    /// </summary>
    public interface IIndexManager
    {
        bool CreateIndex(AbstractFileBase fileInfo);

        bool DeleteIndex(AbstractFileBase fileInfo);

        Object SearchNextIndex(Dictionary<string, string> dic, int pageIndex, int pageSize, LibHandle handle, string lastFileId);

        Object SearchIndex(Dictionary<string, string> dic, int pageIndex, int pageSize, LibHandle handle);

        Object SearchPrevIndex(Dictionary<string, string> dic, int pageIndex, int pageSize, LibHandle handle, string lastFileId);
    }
}
