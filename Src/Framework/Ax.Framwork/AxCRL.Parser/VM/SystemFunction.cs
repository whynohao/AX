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
using System.IO;
using System.Reflection;

namespace AxCRL.Parser
{


    /// <summary>
    /// 
    /// </summary>
    class SystemFunction  
    {

        public static VAL Function(string func, VAL parameters, Memory DS, Position position)
        {
            VALL L = (VALL)parameters.value;
            VAL R0;

            int size = L.Size;
            VAL L0 = size > 0 ? L[0] : null;
            VAL L1 = size > 1 ? L[1] : null;

            switch (func)
            {

                /*
                 *  register(Type type)
                 *  register(Assembly assemby)
                 * */
                case "register":
                    if (size == 1)
                    {
                        if (L0.ty == VALTYPE.hostcon)
                        {
                            object host = L0.HostValue;
                            if (host is Type)
                                return new VAL(HostType.Register((Type)host));
                            if (host is Type[])
                                return new VAL(HostType.Register((Type[])host));
                            else if (host is Assembly)
                                return new VAL(HostType.Register((Assembly)host));
                        }
                    }
                    break;

                case "addreference":
                    if (size == 2 && L0.ty == VALTYPE.stringcon && L1.ty == VALTYPE.hostcon) 
                    {
                        object host = L1.HostValue;
                        if (host is Assembly)
                        {
                            HostType.AddReference(L0.Str, (Assembly)host);
                            return VAL.NewHostType(host);
                        }
                    }
                    break;
         
                //return VAL type
                case "type":
                    if(size==1)
                        return new VAL((int)L0.ty);
                    break;
                
                case "GetType":
                    if (size == 1)
                    {
                        if (L0.value == null)
                            return new VAL();
                        else
                            return VAL.NewHostType(L0.value.GetType());
                    }
                    break;

                case "typeof":
                    if (size == 2)
                    {
                        if (L0.ty == VALTYPE.listcon && L1.ty == VALTYPE.stringcon)            //1.
                        {
                            L0.Class = L1.Str;
                            return L0;
                        }
                    }
                    else if (size == 1)
                    {
                        if (L0.value == null)     
                        {
                            Type ty = HostType.GetType(L0.name);
                            if (ty != null) 
                                return VAL.NewHostType(ty);
                            else
                                return new VAL();
                        }
                        else if (L0.ty == VALTYPE.listcon)
                        {
                            if (L0.Class == null)
                                return VAL.VOID;     
                            return new VAL(L0.Class);
                        }
                        else if (L0.ty == VALTYPE.hostcon)
                        {
                            if (L0.value is Type)
                                return L0;               
                            else
                                return VAL.NewHostType(L0.value.GetType()); 
                        }
                        else if (L0.ty == VALTYPE.stringcon)
                        {
                            return VAL.NewHostType(HostType.GetType(L0.Str));    //6.
                        }
                    }
                    break;

                case "classof":
                    if (size == 1)
                    {
                        if (L0.ty == VALTYPE.hostcon)
                            return HostValization.Host2Val(L0.value);
                    }
                    else if (size == 2)
                    {
                        if (L0.ty == VALTYPE.hostcon && L1.ty == VALTYPE.listcon)            //1.
                        {
                            HostValization.Val2Host(L1, L0.value);
                            return L0;
                        }
                    }
                    break;

                    
                case "valize":
                    if(size==1)
                    {
                        return VAL.Script(L0.Valor);
                    }
                    break;
 
                case "isnull":
                    if (size == 2)
                    {
                        if (L0.ty == VALTYPE.nullcon)
                            return L1;
                        else
                            return L0;
                    }
                    break;

                case "VAL":
                    if (size == 1)
                    {
                        R0 = VAL.Clone(L0);
                        R0.Class = "VAL";              //force to CAST VAL, don't do HostValue unboxing
                        return R0;
                    }
                    break;

                case "HOST":                       //cast to hostcon
                    if (size == 1)
                    {
                        R0 = VAL.Clone(L0);
                        R0.ty = VALTYPE.hostcon;
                        return R0;
                    }
                    break;

 
                case "ctype":
                    if (size == 2)
                    {
                        if (L1.value is Type)
                        {
                            return VAL.cast(VAL.Clone(L0), (Type)L1.value);
                        }
                        else if (L[1].value is string)
                        {
                            Type ty = HostType.GetType(L1.Str); 
                            if(ty!=null)
                                return VAL.cast(VAL.Clone(L0), ty);
                        }
                    } 
                    break;    

     

                case "DateTime":
                    if (size == 6)
                        return VAL.NewHostType(new DateTime(L0.Intcon, L1.Intcon, L[2].Intcon, L[3].Intcon, L[4].Intcon, L[5].Intcon));
                    else if (size == 3)
                        return VAL.NewHostType(new DateTime(L0.Intcon, L1.Intcon, L[2].Intcon));
                    break;
         
                //STRING
                case "format":
                    if(size>=1 && L0.ty == VALTYPE.stringcon)
                        return format(L);
                    break;
        


                #region LIST function

                case "size":
                    if (size == 1)
                       return new VAL(L0.Size);
                    break;

                case "array":           //array(2,3,4)
                    int[] A = new int[size];
                    for(int i=0; i < size; i++)
                    {
                        if (L[1].ty != VALTYPE.intcon)
                            return null;

                        A[i] = L[i].Intcon;
                    }
                    return VAL.Array(A);
     
                case "slice":
                    return Slice(L);

                case "append":
                case "push":  
                    if (size == 2 && L0.ty == VALTYPE.listcon)      
                    {
                        R0 = L1;
                        L0.List.Add(VAL.Clone(R0));
                        return L0;
                    }
                    break;
                case "pop":
                    if (size == 1 && L0.ty== VALTYPE.listcon) 
                    {
                        int index = L0.List.Size - 1;
                        R0 = L0.List[index];
                        L0.List.Remove(index);
                        return R0;
                    }
                    else if (size == 2 && L0.ty == VALTYPE.listcon && L1.ty == VALTYPE.intcon)
                    {
                        int index = L1.Intcon;
                        R0 = L0.List[index];
                        L0.List.Remove(index);
                        return R0;
                    }
                    break;


                case "insert":
                    if (size == 3 && L0.ty == VALTYPE.listcon && L1.ty == VALTYPE.intcon)
                    {
                        L0.List.Insert(L1.Intcon, VAL.Clone(L[2]));
                        return L0;
                    }
                    break;
                case "remove":
                    if (size == 2 && L0.ty == VALTYPE.listcon && L1.ty == VALTYPE.intcon)
                    {
                        L0.List.Remove(L1.Intcon);
                        return L0;
                    }
                    break;
#endregion


                //DEBUG
                case "echo":
                    return new VAL(L);
                case "write":
                    return WriteLine(L);
                case "loginfo":
                    return LogInfo(L);

                
                    
               #region internal functions used by parser

                case Constant.FUNC_CAST_TYPE_VALUE:   
                    if (size == 2)
                        return cast(L1, L0);
                    break;

                case Constant.FUNC_CAST_VALUE_TYPE:   
                    if (size == 2)
                        return cast(L0, L1);
                    break;

                case Constant.FUNC_IS_TYPE:
                    if (size == 2)
                    {
                        Type type = SystemFunction.GetValDefinitionType(L1);
                        if (type != null)
                        {
                            if (L0.value == null)
                                return new VAL(false);
                            else
                                return new VAL(type.IsAssignableFrom(L0.value.GetType()));
                        }
                        else
                            throw new RuntimeException(position, "{0} is not type or not registered.", L1.value);
                    }
                    break;
                
                case Constant.FUNC_MAKE_ARRAY_TYPE:
                    if (size == 1 || size ==2)
                    {
                        Type ty = SystemFunction.GetValDefinitionType(L0); 
                        if (ty != null)
                        {
                            if (size == 1)
                                return VAL.Boxing1(ty.MakeArrayType());
                            else if (L1.value is int)
                                return VAL.Boxing1(ty.MakeArrayType(L1.Intcon));
                        }
                        else
                            throw new RuntimeException(position, "declare array failed, {0} is not type.", L0.value);
                    }
                    break;


                case Constant.FUNC_FUNCTION:
                    if (L[1].ty == VALTYPE.intcon)
                        return new VAL(Operand.Func(L[1].Intcon, L[0].Str));
                    else
                        return new VAL(Operand.Func(L[1].Str, L[0].Str));

                case Constant.FUNC_CLASS:
                    return new VAL(Operand.Clss(L[1].Intcon, L[0].Str));

  
               #endregion


                #region propertyof, methodof, fieldof

   
                case "propertyof":
                    if(size>=2 && size <=4)
                    {
                        object host = L0.value;
                        if (host == null)
                            break;

                        if (L0.ty == VALTYPE.hostcon && L1.ty == VALTYPE.stringcon) 
                        {
                            if (size == 2 || size == 3)
                                return HostFunction.propertyof(size == 2, null, (string)L1.value, host, size == 2 ? null: L[2].HostValue);
                        }
                        else if (L0.ty == VALTYPE.hostcon                      
                            && L1.ty == VALTYPE.hostcon && L1.value is Type
                            && L[2].ty == VALTYPE.stringcon)
                        {
                            if (size == 3 || size == 4)
                                return HostFunction.propertyof(size == 3, (Type)L1.value, (string)L[2].value, host, size == 3 ? null : L[3].HostValue);
                        }
                    }
                    break;


              case "fieldof":
                    if (size == 2 || size == 3)
                    {
                        object host = L0.value;
                        if (host == null)
                            break;

                        if (L0.ty == VALTYPE.hostcon && L1.ty == VALTYPE.stringcon)
                        {
                            Type ty = HostType.GetHostType(host);
                            FieldInfo fieldInfo = ty.GetField((string)L1.value, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                            if(fieldInfo == null)
                                throw new RuntimeException(position, string.Format("Invalid field name: {0}.{1}", ty.FullName, L1.value));

                            if (size == 2)
                                return VAL.Boxing1(fieldInfo.GetValue(host));
                            else
                            {
                                fieldInfo.SetValue(host, L[2].HostValue);
                                return VAL.VOID;
                            }
                        }
                    }
                    break;




               case "methodof":
                    if (size == 4)
                    {
                        object host = L0.value;
                        if (host == null)
                            break;
                        
                        VAL L2 = L[2];
                        object args = L[3].HostValue;

                        if (L0.ty == VALTYPE.hostcon                       
                            && L1.ty == VALTYPE.hostcon && L1.value is Type
                            && L2.ty == VALTYPE.stringcon
                            && args is Type[])
                        {
                            MethodInfo methodInfo = HostFunction.methodof(host, (Type)L1.value, (string)L2.value, (Type[])args);
                            if (methodInfo != null)
                            {
                                VAL method = VAL.Boxing1(methodInfo);
                                method.temp = new HostOffset(host, methodInfo); 
                                return method;
                            }
                            else
                                throw new RuntimeException(position, "method {0} is not existed", L2.value);
                        }
                    }
                    break;
                
                #endregion   

            }

            return null;   
        }

        

        #region System function implementation

        private static VAL WriteLine(VALL L)
        {
            StringWriter o = new StringWriter();
            for (int i = 0; i < L.Size; i++)
                o.Write(L[i].ToString2());

            Logger.WriteLine(o.ToString());
            return new VAL(L);   //return void
        }

        private static VAL LogInfo(VALL L)
        {
            StringWriter o = new StringWriter();
            o.Write(System.DateTime.Now);
            o.Write(" ");
            for (int i = 0; i < L.Size; i++)
                o.Write(L[i].ToString2());

            Logger.WriteLine(o.ToString());
            return new VAL(L);   //return void
        }



      
        private static VAL format(VALL L)
        {
            string fmt = L[0].Str;
            object[] args = new object[L.Size - 1];
            for (int i = 1; i < L.Size; i++)
                if (L[i].ty != VALTYPE.listcon)
                    args[i - 1] = L[i].value;  //use C# string.format string control
                else
                    args[i - 1] = L[i].ToString2();

            string s = string.Format(fmt, args);
            return new VAL(s);
        }



        public static VAL Slice(VALL arr)
        {
            VAL V = arr[0];

            int start = 0;
            int stop = -1;
            int step = 1;

            switch (arr.Size)
            {
                case 1:
                    break;
                case 2:
                    start = arr[1].Intcon;
                    break;
                case 3:
                    start = arr[1].Intcon;
                    stop = arr[2].Intcon;
                    break;
                case 4:
                    start = arr[1].Intcon;
                    stop = arr[2].Intcon;
                    step = arr[3].Intcon;
                    break;
            }

            return new VAL(V.List.Slice(start, stop, step));

        }



        #endregion


        #region CAST/Convert

        public static VAL cast(VAL val, VAL type)
        {
            if (type.ty == VALTYPE.voidcon) 
                    return val;

            if (type.value is Type)
            {
                return VAL.cast(VAL.Clone(val), (Type)type.value);
            }
            else
                throw new TieException("cast failed, {0} is not type.", type.value);
        }

    

        #endregion


        private static Type GetValDefinitionType(VAL L0)
        {
            if (L0.value is Type)
                return (Type)L0.value;

            Type ty = HostType.GetType(L0.name); 
            if (ty != null)
                return ty;
            else
                return null;
        }


    }


}

