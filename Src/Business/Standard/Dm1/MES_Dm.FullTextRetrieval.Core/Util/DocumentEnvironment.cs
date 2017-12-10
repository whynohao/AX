/**********************************************************************
 * CopyRight 2016 杭州集控科技有限公司 版权所有
 * 功能描述：一些常量的定义
 * 创建标识：CHENQI 2016/12/19
 * 修改标识：
 * 修改描述：
************************************************************************/
using AxCRL.Comm.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MES_Dm.FullTextRetrieval.Core.Util
{
    public class DocumentEnvironment
    {
        public static string IndexDir
        {
            get
            {
                return  System.IO.Path.Combine(EnvProvider.Default.DocumentsPath, "Index");
            }
        }

        public static string DocumentDir
        {
            get
            {
                return EnvProvider.Default.DocumentsPath;
            }
        }

        public static string PanGuXml
        {
            get
            {
                return System.IO.Path.Combine(IndexDir, "PanGu.xml"); 
            }
        }
    }
}
