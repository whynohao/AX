/**********************************************************************
 * CopyRight 2017 杭州集控科技有限公司 版权所有
 * 功能描述：微服务配置信息类
 * 创建标识：Zhangkj 2017/06/06
 * 
 *
************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCRL.Comm.Configs
{
    /// <summary>
    /// 微服务配置信息类
    /// </summary>
    public class MicroServicesConfig:ConfigBase
    {
        protected static MicroServicesConfig _Instance = null;
        public static MicroServicesConfig Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = ReadConfig<MicroServicesConfig>();
                    if (_Instance == null)
                    {
                        _Instance = new MicroServicesConfig();
                        _Instance.SaveConfig();
                    }
                }
                return _Instance;
            }
        }
        /// <summary>
        /// 移动端推送服务配置信息
        /// </summary>
        public ServiceConfig AppPush { get; set; }

        private MicroServicesConfig()
        {
            AppPush = new ServiceConfig()
            {
                Enabled = false,
                Name = "移动端推送",
                BaseUrl = "http://localhost:10001",
                Desc = "向移动端App推送消息"
            };
        }
    }
    /// <summary>
    /// 服务配置信息类
    /// </summary>
    public class ServiceConfig
    {
        /// <summary>
        /// 服务是否已启用
        /// </summary>
        public bool Enabled { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 服务基地址
        /// </summary>
        public string BaseUrl { get; set; }
        /// <summary>
        /// 服务描述
        /// </summary>
        public string Desc { get; set; }       
    }
}
