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
using System.Collections;
using System.Text;
using System.Reflection;


namespace AxCRL.Parser
{

    /// <summary>
    /// delegate for valizer
    /// </summary>
    /// <param name="host"></param>
    /// <returns></returns>
    public delegate VAL Valizer(object host);

    /// <summary>
    /// delegate fro devalizer
    /// </summary>
    /// <param name="val"></param>
    /// <returns></returns>
    public delegate object Devalizer(VAL val);


    /// <summary>
    /// interface of valizer and devalizer
    /// </summary>
    public interface IValizer
    {
        /// <summary>
        /// prototype of valizer
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        VAL Valizer(object host);

        /// <summary>
        /// prototype of devalizer
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        object Devalizer(VAL val);
    }

    class HostValization
    {

        public static object NewInstance(VAL val, object[] args)
        {
            object instance = HostType.NewInstance(val.Class, args);
            Val2Host(val, instance);
            return instance;
        }

        private static void SetValue(object host, Type type, string offset, VAL val)
        {
            object temp = ValizerScript.ToHost(val, type);
            if (temp == null)
                temp = val.HostValue;

            HostOperation.HostTypeAssign(host, offset, temp, true);
        }

      

        //Devalize
        public static object Val2Host(VAL val, object host)
        {

            FieldInfo[] fields = host.GetType().GetFields();
            foreach (FieldInfo fieldInfo in fields)
            {
                try
                {
                    VAL p = val[fieldInfo.Name];

                    if (p.Defined)
                    {
                        if (fieldInfo.FieldType == typeof(VAL))
                            fieldInfo.SetValue(host, p);
                        else
                            SetValue(host, fieldInfo.FieldType, fieldInfo.Name, p);
                    }
                }
                catch (ArgumentException)
                {
                }
                catch (Exception)      
                { 
                }
            }

            PropertyInfo[] properties = host.GetType().GetProperties();
            foreach (PropertyInfo propertyInfo in properties)
            {
                try
                {
                    VAL p = val[propertyInfo.Name];
                    if (p.Defined)
                    {
                        if (propertyInfo.CanWrite)
                        {
                            if (propertyInfo.PropertyType == typeof(VAL))
                                propertyInfo.SetValue(host, p, null);
                            else
                                SetValue(host, propertyInfo.PropertyType, propertyInfo.Name, p);
                        }
                        else   //CanRead
                        {
                            object prop = propertyInfo.GetValue(host, null);
                            if (prop is IList)
                            {
                                IList array = (IList)prop;
                                int count = array.Count;
                                object phost = p.HostValue;
                                if (phost is ICollection)  
                                {
                                    ICollection collection = (ICollection)phost;
                                    int i = 0;
                                    foreach (object x in collection)
                                    {
                                        if (i < count)
                                            array[i] = x;
                                        else
                                            array.Add(x);       

                                        i++;
                                    }
                                }


                            }
                            else 
                                Val2Host(p, propertyInfo.GetValue(host, null));
                        }
                    }
                }
                catch (ArgumentException)
                {
                }
            }

            EventInfo[] events = host.GetType().GetEvents();
            foreach (EventInfo eventInfo in events)
            {
                try
                {
                    VAL p = val[eventInfo.Name];
                    if (p.Defined)
                    {
                        if (p.ty == VALTYPE.funccon)       //event handler
                        {
                            HostEvent hostEvent = new HostEvent(eventInfo, p);
                            VAL d = hostEvent.AddDelegateEventHandler();
                            eventInfo.AddEventHandler(host, d.value as Delegate);
                        }
                    }
                }
                catch (ArgumentException)
                { 
                
                }
            }

            return host;
        }

    
        private static VAL Host2Valor(object host, VAL val)
        {
            if (host == null || host is System.DBNull)
            {
                val = new VAL();
            }
            else if (host is string || host is char 
                || host is byte
                || host is int || host is short || host is long 
                || host is bool 
                || host is double || host is float || host is decimal
                || host is DateTime)
            {
                val = VAL.Boxing1(host);
            }
            else if (host is IValizable)
            {
                val = ((IValizable)host).GetValData();
            }
            else if (host is Type)
            {
                val = VAL.Script(string.Format("typeof({0})", ((Type)host).FullName));
            }
            else if (host.GetType().IsEnum)      
            {
                val = VAL.Script(HostOperation.EnumBitFlags(host));
            }
            else if (host is ICollection)
            {
                val = VAL.Array();
                foreach (object a in (ICollection)host)
                {
                    val.Add(Host2Valor(a, new VAL()));
                }
            }
            else
            {
                VAL temp = ValizerScript.ToValor(host);
                if ((object)temp != null)
                {
                    return temp;
                }

                FieldInfo[] fields = host.GetType().GetFields();
                foreach (FieldInfo fieldInfo in fields)
                {
                    Attribute[] A = (Attribute[])fieldInfo.GetCustomAttributes(typeof(NonValizedAttribute), true);
                    if (A.Length != 0)
                        continue;

                    object fieldValue = fieldInfo.GetValue(host);
                    VAL persistent = ValizerScript.ToValor(fieldInfo, fieldValue);

                    if ((object)persistent == null)
                    {
                        persistent = VAL.Boxing(fieldValue);
                        if (!fieldInfo.FieldType.IsValueType && persistent.IsHostType)
                        {
                            persistent = Host2Valor(fieldValue, new VAL());
                        }
                    }

                    val[fieldInfo.Name] = persistent;
                }

                PropertyInfo[] properties = host.GetType().GetProperties();
                foreach (PropertyInfo propertyInfo in properties)
                {
                    ValizableAttribute[] attributes = (ValizableAttribute[])propertyInfo.GetCustomAttributes(typeof(ValizableAttribute), true);
                    if (attributes.Length == 0 || !propertyInfo.CanRead)
                        continue;


                    object propertyValue = propertyInfo.GetValue(host, null);
                    if (propertyValue == null)
                        continue;

                    VAL persistent = ValizerScript.ToValor(propertyInfo, propertyValue);

                    if ((object)persistent == null)
                    {
                        if (propertyValue is ICollection)
                        {
                            ICollection collection = (ICollection)propertyValue;
                            persistent = VAL.Array();
                            foreach (object obj in collection)
                            {
                                persistent.Add(VAL.Boxing(obj));
                            }
                        }
                        else
                        {
                            persistent = VAL.Boxing(propertyValue);
                            if (!propertyInfo.PropertyType.IsValueType && persistent.IsHostType)
                            {
                                persistent = Host2Valor(propertyValue, new VAL());
                            }
                        }
                    }

                    val[propertyInfo.Name] = persistent;

                }
            }

            val.Class = host.GetType().FullName;
            return val;
        }

        public static VAL Host2Valor(object host)
        {
            VAL val = new VAL();

            return Host2Valor(host, val);
        }

        public static VAL Valor2Val(VAL valor)
        {
            return Script.Evaluate(valor.Valor);
        }

        public static VAL Host2Val(object host)
        {
            VAL valor = Host2Valor(host);
            return Valor2Val(valor);
        }

        public static object Valor2Host(VAL valor, object host)
        {
            VAL val = Valor2Val(valor);
            return Val2Host(val, host);
        }
    
    }
}
