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
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
#if !SILVERLIGHT
//using System.Runtime.Serialization.Formatters.Soap;
using System.Runtime.Serialization.Formatters.Binary;
#endif

namespace AxCRL.Parser
{
    class HostCoding
    {
        #region 扩展Decode/Encode 支持.NET Object  in Computer.DS

        /*
         * 
         * .net object format/protocol:  
         * 
         *      (1) new className(args,...) 
         *          如 new System.Windows.Form.TextBox()
         *             new Tie.VAL(20)
         *             
         * 
         *      (2) new SerializedObject(string className, string value.ToString(), string SerializedValue);       
         *          请看Encode()生成的格式
         *      
         * 例如:
         *      前提:
         *          System.Windows.Form.Label的值是Type, 已经用Register函数登记在数据字典Computer.DS中.
         *     
         *      求:
         *          new System.Windows.Form.Label();   转化为  new Label(System.Window.Form);
         * 
         * */
        public static VAL Decode(string className, VAL args, VAL scope, Context context)
        {
            VAL clss = new VAL();
            /*
                * 如果是注册过的class, 那么它的namespace是定义在Computer.DS中
                * 譬如 
                *      A= new NS.CLSS();
                *      scope[className] --> clss;
                *  
                * 如果没有定义namespace,那么直接到Computer.DS中去取值
                * 譬如
                *      A= CLSS();
                *      Computer.DS[className] --> clss
                *   
                * */
            clss = scope[className];
            if (clss.Undefined)
                clss = context.GetVAL(className, true);         //如果没有找到val.Class, context.GetVAL(...)返回new VAL()

            //返回注册过的class名字
            if (clss.Defined && clss.value is Type)
            {
                Type type = (Type)clss.value;
                object instance = Activator.CreateInstance(type, ConstructorArguments(args));
                return VAL.Boxing1(instance); //返回实例
            }

            //throw new RuntimeException(string.Format("class [{0}]has not registered yet.", scope.name + "." + val.Class)); 
            //如果没有注册过
            if (scope.name != null)
            {
                object instance = HostType.NewInstance(scope.name + "." + className, ConstructorArguments(args));

                if (instance != null)
                {
                    //把HostType类型注册到CPU.DS2中去
                    VAL hostType = VAL.NewHostType(instance.GetType());
                    scope[className] = hostType;

                    return VAL.Boxing1(instance);  //返回实例
                }
            }
            
          
            if (clss.IsNull)
                throw new HostTypeException("class {0} is not defined.", className);


            if (clss.value != null)
            {
                Type type;
                if (clss.value is Type)
                    type = (Type)clss.value;
                else
                    type = clss.value.GetType();

                object instance = Activator.CreateInstance(type, ConstructorArguments(args));
                return VAL.NewHostType(instance);
            }


            return args;
        }

        internal static object[] ConstructorArguments(VAL args)
        {
            if (args.Size > 0)
                return args.List.ObjectArray;
            else
                return null;
        }



        public static string Encode(object host, bool persistent)
        {
            Type type;
            if (host is MethodInfo)
            {
                MethodInfo methodInfo = (MethodInfo)host;
                if(methodInfo.IsStatic)
                    return methodInfo.ReflectedType.FullName + "." + methodInfo.Name;
                else
                    return methodInfo.Name;
            }
            else if (host is Type)
            {
                type = (Type)host;
                return string.Format("typeof({0})", type.FullName);
            }
            else
                type = host.GetType();

            if (type.IsEnum)            //处理enum常量
                return HostOperation.EnumBitFlags(host);

            if (host is DateTime)
                return string.Format("new {0}({1})", typeof(DateTime).FullName, ((DateTime)host).Ticks);
                

            VAL val = HostValization.Host2Valor(host);
            if (persistent)
                return val.Valor;
            else
            {
                //default contructor      
                if (HostCoding.HasContructor(type, new Type[]{}))
                     return string.Format("new {0}()", type.FullName);   //有缺省的constructor

                if (type.FullName == host.ToString())
                    return string.Format("new {0}(...)", type.FullName);
                else
                    return string.Format("new {0}({1})", type.FullName, host);
            }
    }


        #endregion



/****
        #region SOAPFormatter Encode/Decode

#if !SILVERLIGHT 

        public static object DecodeSOAP(string SOAP)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(SOAP);
            using (MemoryStream stream = new MemoryStream(buffer))
            {
                SoapFormatter formatter = new SoapFormatter();
                try
                {
                    return formatter.Deserialize(stream);
                }
                catch (Exception)
                {
                    throw RuntimeException.Exception(".NET object Deserialization failed in Tie. " + SOAP);
                }
                finally
                {
                    stream.Close();
                    stream.Dispose();
                }
            }
        }


        public static string EncodeSOAP(object value)
        {
            byte[] buffer = new byte[16 * 1024];

            using (MemoryStream stream = new MemoryStream(buffer))
            {
                SoapFormatter formatter = new SoapFormatter();
                try
                {
                    formatter.Serialize(stream, value);
                    return Encoding.UTF8.GetString(buffer, 0, (int)stream.Position);
                }
                catch (Exception)
                {
                    throw RuntimeException.Exception(".NET object Serialization failed in Tie. " + value.ToString());
                }
                finally
                {
                    stream.Close();
                    stream.Dispose();
                }
            }

        }



#endif
 
        #endregion
*/



        #region BinaryFormatter Encode/Decode

#if !SILVERLIGHT

        public static object DecodeBinary(string hexString)
        {
            byte[] buffer = HostType.HexStringToByteArray(hexString);
            using (MemoryStream stream = new MemoryStream(buffer))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                try
                {
                    return formatter.Deserialize(stream);
                }
                catch (Exception)
                {
                    throw new HostTypeException(".NET object Deserialization failed in Tie. " + hexString);
                }
                finally
                {
                    stream.Close();
                    stream.Dispose();
                }
            }
        }


        public static string EncodeBinary(object value)
        {
            byte[] buffer = new byte[16 * 1024];

            using (MemoryStream stream = new MemoryStream(buffer))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                try
                {
                    formatter.Serialize(stream, value);
                    StringWriter sw = new StringWriter();
                    for (int i = 0; i < stream.Position; i++)
                        sw.Write("{0:x2}", buffer[i]);

                    return sw.ToString();
                }
                catch (Exception)
                {
                    throw new HostTypeException(".NET object Serialization failed in Tie. " + value.ToString()); ;
                }
                finally
                {
                    stream.Close();
                    stream.Dispose();
                }
            }

        }

#endif
        #endregion


        
        
        #region Find HostType array common Type( interface[] /base class)

        public static object ToHostArray(object[] values)
        {

            if (values.Length == 0)
                return values;

            Type type = CommonBaseClass(values);
            if (type == null)
            {
                
                Type[] I = CommonInterface(values);
                if (I.Length == 0)
                    return values;
                type = I[0];
            }


            Type arrayType = type.MakeArrayType();
            Array array = (Array)Activator.CreateInstance(arrayType, new object[] { values.Length });

            for(int i=0; i< values.Length; i++)
            {
                array.SetValue(values[i], i);
            }

            return array;

        }


        public static Type CommonBaseClass(object[] values)
        {
            Type type = null;
            foreach (object obj in values)
            {
                if (obj == null)
                    continue;

                type = obj.GetType();
                break;
            }

            if (type == null)
                return null;

            foreach (object obj in values)
            {
                if (obj == null)
                    continue;

                Type t = obj.GetType();

                if (t == type || t.IsSubclassOf(type))
                    continue;
                else if (type.IsSubclassOf(t))
                    type = t;
                else
                {
                    return null;
                }
            }

            return type;

        }



        public static Type[] CommonInterface(object[] values)
        {

            Type[] I = new Type[0];
            foreach (object obj in values)
            {
                if (obj == null)
                    continue;

                I = obj.GetType().GetInterfaces();
                if (I.Length != 0)
                     break;
                
            }

            if (I.Length == 0)
                return I;

            foreach (object obj in values)
            {
                if (obj == null)
                    continue;

                Type t = obj.GetType();
                I = CommonInterface(I, t.GetInterfaces());

                if (I.Length == 0)
                    return I;
            }

            return I;
        
        }

        private static Type[] CommonInterface(Type[] I1, Type[] I2)
        {
            List<Type> I = new List<Type>();
            foreach(Type i1 in I1)
            {
                foreach(Type i2 in I2)
                {
                    if(i1==i2)
                        I.Add(i1);
                }
            }
            
            return I.ToArray() ;
        }

    
        #endregion


        public static bool HasInterface(Type clss, Type interfce)
        {
            Type[] I = clss.GetInterfaces();
            foreach (Type i in I)
            {
                if (i == interfce)
                    return true;
            }

            return false;

        }


        public static bool HasContructor(Type clss, Type[] arguments)
        {
            ConstructorInfo[] constructors = clss.GetConstructors();
            foreach (ConstructorInfo constructorInfo in constructors)
            {
                ParameterInfo[] parameters = constructorInfo.GetParameters();
                if (parameters.Length == arguments.Length)
                {
                    int count = 0;
                    for (int i = 0; i < parameters.Length; i++ )
                    {
                        if (parameters[i].ParameterType == arguments[i])
                            count++;
                    }
                    
                    if (count == arguments.Length)
                        return true;
                }
            }

            return false;
        }

     
    }
}
