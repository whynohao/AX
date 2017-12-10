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

namespace AxCRL.Parser
{

    /// <summary>
    /// Represent .NET object Type
    /// </summary>
    public class HostType
    {

        //object host;

        //public HostType(string host)
        //{
        //    this.host = host;
        //}


        ////members = fields + properties
        //public VAL Members
        //{
        //    get
        //    {
        //        return HostValization.Host2Val(host);
        //    }
        //    set
        //    {
        //        HostValization.Val2Host(value, host);
        //    }
        //}


        /// <summary>
        /// Register valizer
        /// </summary>
        /// <param name="type"></param>
        /// <param name="valizer"></param>
        /// <returns></returns>
        public static VAL Register(Type type, Valizer valizer)
        {
            return ValizerScript.Register(type, valizer, null);
        }
        
        /// <summary>
        /// Register valizer and devalizer
        /// </summary>
        /// <param name="type"></param>
        /// <param name="valizer"></param>
        /// <param name="devalizer"></param>
        /// <returns></returns>
        public static VAL Register(Type type, Valizer valizer, Devalizer devalizer)
        {
            return ValizerScript.Register(type, valizer, devalizer);
        }


        /// <summary>
        /// Register valizer script 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="valizerScript"></param>
        /// <returns></returns>
        public static VAL Register(Type type, string valizerScript)
        {
            return ValizerScript.Register(type, valizerScript, null);
        }

        /// <summary>
        /// Register Valizer by object interface
        /// </summary>
        /// <param name="type"></param>
        /// <param name="valizer"></param>
        /// <returns></returns>
        public static VAL Register(Type type, IValizer valizer)
        {
            return ValizerScript.Register(type, valizer, null);
        }

        /// <summary>
        /// Register valizer by class's members
        /// </summary>
        /// <param name="type"></param>
        /// <param name="valizerMembers"></param>
        /// <returns></returns>
        public static VAL Register(Type type, string[] valizerMembers)
        {
            return ValizerScript.Register(type, valizerMembers, null);
        }
        
        #region Register Type Functions

        //---------------------------------------------------------------------------------------

        /// <summary>
        /// Register .NET type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool Register(Type type)
        {
            return Register(type, false);
        }

   
        /// <summary>
        /// Register multiple .NET types
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        public static bool Register(Type[] types)
        {
            return Register(types, false);
        }


        /// <summary>
        /// Register all types of assembly
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static bool Register(Assembly assembly)
        {
            return Register(assembly, false);
        }

        /// <summary>
        /// Register .NET type with brief/short name
        /// </summary>
        /// <param name="type"></param>
        /// <param name="briefName"></param>
        /// <returns></returns>
        public static bool Register(Type type, bool briefName)
        {
            return Register(new Type[] { type }, briefName);
        }

        /// <summary>
        /// Register multiple .NET types with brief/short name
        /// </summary>
        /// <param name="types"></param>
        /// <param name="briefName"></param>
        /// <returns></returns>
        public static bool Register(Type[] types, bool briefName)
        {
            string code = "";
            for(int i=0; i< types.Length; i++)
            {
                Type type = types[i];
                 Computer.DS1.Add("$" + i, VAL.NewHostType(type));

                code += string.Format("{0}=${1};", type.FullName,i);

                if (briefName)
                    code += string.Format("{0}=${1};", type.Name, i);

                if (type.IsClass && type.IsAbstract && type.IsSealed)   
                {
                    MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static);
                    foreach (MethodInfo methodInfo in methods)
                    {
                        VAL method;
                        string name = methodInfo.Name;

                        if (Computer.DS1.ContainsKey(name))
                        {
                            List<MethodInfo> L = new List<MethodInfo>();
                            object m = Computer.DS1[name].value;
                            if (m is MethodInfo)
                                L.Add((MethodInfo)m);
                            else if (m is MethodInfo[])
                                L.AddRange((MethodInfo[])m);
                            else                                   
                            {
                                RuntimeException.Warning("{0}.{1}(...) conflict with variable/function {2} during extend method registering", type.FullName, name, Computer.DS1[name]);        
                                continue;   
                            }

                            if (L.IndexOf(methodInfo) == -1)
                            {
                                L.Add(methodInfo);

                                Computer.DS1.Remove(name);
                                method = VAL.NewHostType(L.ToArray());
                            }
                            else
                                continue;
                        }
                        else
                            method = VAL.NewHostType(methodInfo);

                        Computer.DS1.Add(name, method);
                        method.temp = new HostOffset(typeof(object), name);   
                    }
                }
            }

            Script.Execute(code, Computer.DS1);
            
            for (int i = 0; i < types.Length; i++)
                Computer.DS1.Remove("$" + i);

            return true;
        }



        /// <summary>
        /// Register a generic class
        /// </summary>
        /// <param name="typeName">type name in script</param>
        /// <param name="type">generic type</param>
        /// <returns></returns>
        public static bool Register(string typeName, Type type)
        {
            Computer.DS1.Add("$1", VAL.NewHostType(type));
            Script.Execute(typeName + "=$1;", Computer.DS1);
            Computer.DS1.Remove("$1");
            return true;
        }


        /// <summary>
        /// Register all types of assembly with brief/short name
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="briefName"></param>
        /// <returns></returns>
        public static bool Register(Assembly assembly, bool briefName)
        {
            foreach (Type type in assembly.GetExportedTypes())
            {
                try
                {
                    if (!type.IsNestedPublic)
                        HostType.Register(type, briefName);
                }
                catch (Exception)
                {
                    Logger.WriteLine(string.Format("{0} cannot be registed.", type.FullName));
                    return false;
                }
            }

            return true;
        }

    
        #endregion

        
        #region Add Reference


        /**
         * 
         * used on namespace and assembly name(DLL/EXE) inconsistant
         *  如: namespace = Tie
         *      Assembly  = Tie2.Dll
         *   
         *   AddReference("Tie", "Tie2");
         *   AddReference("System.Drawing", Assembly.Load("System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"));
         *   AddReference("System.Windows.Forms", Assembly.Load("System.Windows.Forms, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"));
         *   
         *   
         * */
        public static bool AddReference(string namespaceName, Assembly assembly)
        {
            if (references.ContainsKey(namespaceName))
                references.Remove(namespaceName);

            references.Add(namespaceName, assembly);
            return true;
        }

        //<namespace, assembly>
        private static Dictionary<string, Assembly> references = new Dictionary<string, Assembly>();



        private static Type GetReferenceType(string className)
        {
            string[] nameSpace = className.Split(new char[] { '.' });
            int n = nameSpace.Length-1;

            while (n > 0)
            {
                string ns = "";
                for (int i = 0; i < n-1; i++)
                    ns += nameSpace[i] + ".";
                
                ns += nameSpace[n - 1];

                if (references.ContainsKey(ns))
                {
                    Type type = references[ns].GetType(className);
                    if (type != null)
                        return type;
                }
                
                n--;
            }

            return null;
        }


        #endregion


        #region GetClassType() + NewInstance()
        
        /// <summary>
        /// new instance of class
        /// </summary>
        /// <param name="className">class name</param>
        /// <param name="constructorargs">constructor arguments</param>
        /// <returns></returns>
        public static object NewInstance(string className, object[] constructorargs)
        {
            Type type = GetType(className);
            if (type != null)
                   return Activator.CreateInstance(type, constructorargs);
            else
                return null;
        }


        /// <summary>
        /// Return .NET type
        /// </summary>
        /// <param name="typeName">type name</param>
        /// <returns></returns>
        public static Type GetType(string typeName)
        {
            Type type = null;

            typeName = typeName.Replace(" ","");
            int isArray = 0;    

            while (typeName.EndsWith("[]"))
            {
                isArray++;
                typeName = typeName.Substring(0, typeName.Length - 2); 
            }

            type = typeof(object).Assembly.GetType(typeName);
            if (type != null)
                goto L1;

            type = GetReferenceType(typeName);
            if (type != null)
                 goto L1;

#if !SILVERLIGHT
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = asm.GetType(typeName);
                if (type != null)
                    goto L1;

            }
#endif
            type = GetDefaultAssemblyType(typeName);
            if (type != null)
                goto L1;

            return null;


            L1:
            while (isArray > 0)
            {
                isArray--;
                type = type.MakeArrayType(); 
            }

            return type;
        }


   
        private static Type GetDefaultAssemblyType(string className)
        {
            string[] nameSpace = className.Split(new char[] { '.' });
            int n = nameSpace.Length - 1;

            while (n > 0)
            {
                string ns = "";
                for (int i = 0; i < n - 1; i++)
                    ns += nameSpace[i] + ".";

                ns += nameSpace[n - 1];

                try
                {
                    Assembly assembly = Assembly.Load(ns);
                    Type type = assembly.GetType(className);
                    if (type != null)
                        return type;
                }
                catch (Exception)
                {
                }

                n--;
            }

            return null;
        }

          
        #endregion



        #region Property Extraction


        
        /// <summary>
        /// New instance by persistent data
        /// </summary>
        /// <param name="valor"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static object NewInstance(VAL valor, object[] args)
        {
            return HostValization.NewInstance(valor, args);
        }


        /// <summary>
        /// Get object persistent data
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        public static VAL GetObjectProperties(object host)
        {
            return HostValization.Host2Val(host);
        }


    

        /// <summary>
        /// Set object properties by persistent data
        /// </summary>
        /// <param name="host"></param>
        /// <param name="properties"></param>
        public static void SetObjectProperties(object host, VAL properties)
        {
            HostValization.Val2Host(properties, host);
            return;

        }

        #endregion



        #region Hex <---> String

        /// <summary>
        /// Utility function:
        ///     conver string into byte array
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        public static byte[] HexStringToByteArray(String hexString)
        {
            int numberChars = hexString.Length;
            byte[] bytes = new byte[numberChars / 2];
            for (int i = 0; i < numberChars; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
            }

            return bytes;
        }

        /// <summary>
        /// Utility function:
        ///     convert byte array into string
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string ByteArrayToHexString(byte[] bytes)
        {
            char[] c = new char[bytes.Length * 2];
            byte b;
            for (int i = 0; i < bytes.Length; ++i)
            {
                b = ((byte)(bytes[i] >> 4));
                c[i * 2] = (char)(b > 9 ? b + 0x37 : b + 0x30);

                b = ((byte)(bytes[i] & 0xF));
                c[i * 2 + 1] = (char)(b > 9 ? b + 0x37 : b + 0x30);
            }

            return new string(c);
        }


        #endregion

        internal static Type GetHostType(object host)
        {
            if (host is Type)
                return (Type)host;
            else
                return host.GetType();
        }
       
    }
}
