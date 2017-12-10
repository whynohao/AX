/**********************************************************************
 * CopyRight 2016 杭州集控科技有限公司 版权所有
 * 功能描述：文件索引的基对象
 * 创建标识：CHENQI 2016/12/01
 * 修改标识：
 * 修改描述：
************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MES_Dm.FullTextRetrieval.Core.Model
{
    /// <summary>
    /// 文件的基本信息
    /// </summary>
    public abstract class AbstractFileBase
    {
        private String fileId;

        private String content;

        /// <summary>
        /// 文件代码
        /// </summary>
        public string FileId
        {
            get
            {
                return fileId;
            }

            set
            {
                fileId = value;
            }
        }

        /// <summary>
        /// 文件的内容或概要
        /// </summary>
        public string Content
        {
            get
            {
                return content;
            }

            set
            {
                content = value;
            }
        }
    }
}
