//--------------------------------------------------------------------------------------------------//
//                                                                                                  //
//        Tie                                                                                       //
//                                                                                                  //
//          Copyright(c) Datum Connect Inc.                                                         //
//                                                                                                  //
// This source code is subject to terms and conditions of the Datum Connect Software License. A     //
// copy of the license can be found in the License.txt file at the root of this distribution. If    //
// you cannot locate the  Datum Connect Software License, please send an email to                   //
// support@datconn.com. By using this source code in any fashion, you are agreeing to be bound      //
// by the terms of the Datum Connect Software License.                                              //
//                                                                                                  //
// You must not remove this notice, or any other, from this software.                               //
//                                                                                                  //
//                                                                                                  //
//--------------------------------------------------------------------------------------------------//

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection.Emit;
using System.Reflection;

namespace AxCRL.Parser
{
    class ContextInstance
    {
        public readonly Context context;
        public readonly VAL instance;

        public ContextInstance(Context context, VAL instance)
        {
            this.context = context;
            this.instance = instance;
        }
    }

    class HostEvent
    {
        public const string EVENT_HANDLER_NAME = "$EventHandlers";
        public static bool RemoveEventHandlerSupported = false;
        
        EventInfo eventInfo;
        VAL func;
        VAL ret;
        Context context;
        VAL instance;

        Memory DS2;
       

        public HostEvent(EventInfo eventInfo, VAL func)
        {
            this.eventInfo = eventInfo;
            this.func = func;
            ContextInstance temp = (ContextInstance)func.temp;
            this.context = temp.context;
            this.instance = temp.instance;

            this.DS2 = this.context.DataSegment;
        }

        public VAL AddDelegateEventHandler()
        {
            MethodInfo methodInfo = this.GetType().GetMethod("Callback", BindingFlags.NonPublic | BindingFlags.Instance);
            Delegate dEmitted = Delegate.CreateDelegate(eventInfo.EventHandlerType, this, methodInfo);

            SaveEventHandler(dEmitted);

            VAL dVAL = VAL.NewHostType(dEmitted);
            dVAL.temp = this;
            dVAL.hty = HandlerActionType.Add;   //using SEGREG.DS as flag of ADD HANDLER 
            return dVAL;
        }


        public VAL RemoveDelegateEventHandler()
        {
            Delegate dEmitted = LoadEventHandler();

            VAL dVAL = VAL.NewHostType(dEmitted);
            dVAL.temp = this;
            dVAL.hty = HandlerActionType.Remove;   //using SEGREG.NS as flag of REMOVE HANDLER 
            return dVAL;
        }



        private void Callback(object sender, EventArgs e)
        {
            VALL L = new VALL();
            L.Add(VAL.NewHostType(sender));
            L.Add(VAL.NewHostType(e));
            VAL arguments = new VAL(L);
            ret = CPU.ExternalUserFuncCall(func, instance, arguments, context);
        }


        private Dictionary<EventInfo, Delegate> delegates
        {
            get
            {
                if (DS2 == null)
                    return null;

                if (!DS2.ContainsKey(EVENT_HANDLER_NAME))
                    DS2.AddHostObject(EVENT_HANDLER_NAME, new Dictionary<EventInfo, Delegate>());

                return (Dictionary<EventInfo, Delegate>)DS2[EVENT_HANDLER_NAME].value;
            }
        }

        private void SaveEventHandler(Delegate dEmitted)
        {
            if (!RemoveEventHandlerSupported)
                return;

            if (delegates != null)
            {
                if (delegates.ContainsKey(eventInfo))
                    delegates.Remove(eventInfo);

                delegates.Add(eventInfo, dEmitted);
            }
        }

        private Delegate LoadEventHandler()
        {
            if (!RemoveEventHandlerSupported)
                new HostTypeException("Remove EventHandler is not supported.");

            if (delegates != null && delegates.ContainsKey(eventInfo))
            {
                Delegate dEmitted = delegates[eventInfo]; ;

                if (delegates.ContainsKey(eventInfo))
                    delegates.Remove(eventInfo);
                
                return dEmitted;
            }

            throw new HostTypeException("Can't remove not existed EventHandler");
        }

        public void ClearEventHandler()
        {
            ClearEventHandler(DS2);
        }

        public static void ClearEventHandler(Memory DS2)
        {
            if (DS2.ContainsKey(HostEvent.EVENT_HANDLER_NAME))
                DS2.Remove(HostEvent.EVENT_HANDLER_NAME);
        }

    }
}
