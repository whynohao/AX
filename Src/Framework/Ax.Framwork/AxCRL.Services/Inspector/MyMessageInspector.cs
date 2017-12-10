using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

namespace AxCRL.Services.Inspector
{
    public class CrossDomainInspector : IDispatchMessageInspector
    {
        #region IDispatchMessageInspector
        /// <summary>
        ///     token验证
        /// </summary>
        /// <param name="request"></param>
        /// <param name="channel"></param>
        /// <param name="instanceContext"></param>
        /// <returns></returns>
        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            if (CrossDomainInspector.DealOptions(ref request))
            {
                return "3";
            }
            return string.Empty;
        }

        /// <summary>
        ///     回复内容
        /// </summary>
        /// <param name="reply"></param>
        /// <param name="correlationState"></param>
        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            if ((string)correlationState == "3")
            {
                reply = Message.CreateMessage(OperationContext.Current.IncomingMessageVersion, "OPTIONS");
                CrossDomainInspector.DealNewMessage(ref reply);
            }
            else
                CrossDomainInspector.DealtMessage(ref reply);
        }
        #endregion


        /// <summary>
        /// 对已处理的消息进行cross加工
        /// </summary>
        /// <param name="msg"></param>
        public static void DealtMessage(ref Message msg)
        {
            try
            {
                var ct = ((HttpResponseMessageProperty)msg.Properties["httpResponse"]).Headers["Content-Type"];

                if (MimeTypes.Contains(ct))
                {
                    if (ct == MimeTypes[0])
                    {
                        if (!msg.Properties.ContainsKey("WebBodyFormatMessageProperty"))
                        {
                            msg.Properties.Add("WebBodyFormatMessageProperty", new WebBodyFormatMessageProperty(WebContentFormat.Json));
                        }
                        else if (msg.Properties["WebBodyFormatMessageProperty"] == new WebBodyFormatMessageProperty(WebContentFormat.Xml)) //强制将xml返回值改为json
                        {
                            msg.Properties.Remove("WebBodyFormatMessageProperty");
                            msg.Properties.Add("WebBodyFormatMessageProperty", new WebBodyFormatMessageProperty(WebContentFormat.Json));
                        }
                    }
                    var property = new HttpResponseMessageProperty();
                    property.StatusCode = HttpStatusCode.OK;
                    property.Headers.Add("Content-Type", ct);
                    property.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
                    property.Headers.Add("Access-Control-Allow-Origin", "*");
                    property.Headers.Add("Access-Control-Allow-Headers", "Content-Type,X-Requested-With,Accept");
                    property.Headers.Add("Access-Control-Max-Age", "1728000");
                    property.SuppressEntityBody = false;
                    property.SuppressPreamble = false;
                    if (msg.Properties.ContainsKey("httpResponse"))
                        msg.Properties.Remove("httpResponse");
                    msg.Properties.Add("httpResponse", property);
                }
            }
            catch (Exception ex)
            {
                // Log4NetUtil.WriteErrLog("CrossDomain.DealtMessage", ex);
            }
        }

        /// <summary>
        /// 处理新的消息
        /// </summary>
        /// <param name="msg"></param>
        public static void DealNewMessage(ref Message msg)
        {
            try
            {
                msg.Properties.Add("WebBodyFormatMessageProperty", new WebBodyFormatMessageProperty(WebContentFormat.Json));
                var property = new HttpResponseMessageProperty();
                property.StatusCode = HttpStatusCode.Accepted;
                property.Headers.Add("Content-Type", MimeTypes[0]);
                property.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
                property.Headers.Add("Access-Control-Allow-Origin", "*");
                property.Headers.Add("Access-Control-Allow-Headers", "Content-Type,X-Requested-With,Accept");
                property.Headers.Add("Access-Control-Max-Age", "1728000");
                property.SuppressEntityBody = false;
                property.SuppressPreamble = false;
                if (msg.Properties.ContainsKey("httpResponse"))
                    msg.Properties.Remove("httpResponse");
                msg.Properties.Add("httpResponse", property);
            }
            catch { }

        }

        /// <summary>
        /// 对当前请求是OPTIONS进行处理
        /// </summary>
        /// <param name="request"></param>
        /// <returns>已处理为true，未处理为false</returns>
        public static bool DealOptions(ref Message request)
        {
            try
            {
                WebOperationContext.Current.OutgoingResponse.Headers.Add("Access-Control-Allow-Origin", "*");
                if (((System.ServiceModel.Channels.HttpRequestMessageProperty)request.Properties["httpRequest"]).Method == "OPTIONS")
                {
                    WebOperationContext.Current.OutgoingResponse.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
                    WebOperationContext.Current.OutgoingResponse.Headers.Add("Access-Control-Allow-Headers", "Content-Type,X-Requested-With,Accept");
                    WebOperationContext.Current.OutgoingResponse.Headers.Add("Access-Control-Max-Age", "1728000");
                    WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.Accepted;
                    request.Close();
                    return true;
                }
            }
            catch { }
            return false;
        }



        private static string[] _mimeTypes = null;

        /// <summary>
        /// html格式
        /// </summary>
        public static string[] MimeTypes
        {
            get
            {
                if (_mimeTypes == null)
                {
                    _mimeTypes = new string[] {
                        "application/json; charset=utf-8",
                        "image/png"
                    };
                }
                return _mimeTypes;
            }
        }
    }
    public class CrossDomainInspectorBehaviorAttribute : Attribute, IServiceBehavior
    {
        #region implement IServiceBehavior

        public void AddBindingParameters(ServiceDescription serviceDescription, System.ServiceModel.ServiceHostBase serviceHostBase, System.Collections.ObjectModel.Collection<ServiceEndpoint> endpoints, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
        {

        }

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, System.ServiceModel.ServiceHostBase serviceHostBase)
        {
            foreach (ChannelDispatcher channelDispather in serviceHostBase.ChannelDispatchers)
            {
                foreach (var endpoint in channelDispather.Endpoints)
                {
                    // holyshit DispatchRuntime 
                    endpoint.DispatchRuntime.MessageInspectors.Add(new CrossDomainInspector());
                }
            }
        }

        public void Validate(ServiceDescription serviceDescription, System.ServiceModel.ServiceHostBase serviceHostBase)
        {

        }
        #endregion
    }

}
