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
using System.IO;
using System.Reflection;


namespace AxCRL.Parser
{
    enum OffsetType
    {
        ANY= 1,
        STRUCT=2,
        ARRAY=4
    }

    class HostOffset
    {
        public readonly object host;
        public readonly object offset;

        public HostOffset(object host, object offset)
        {
            this.host = host;
            this.offset = offset;
        }
    }


    class HostOperation
    {

        public static VAL Assign(VAL R0, VAL R1)
        {
            bool r = false;
            if (R0.ty != VALTYPE.funccon)
                r = HostOperation.HostTypeAssign(R0, R1);

            if (!r)
            {
                R0.ty = R1.ty;
                R0.Class = R1.Class;
                R0.hty = R1.hty;
                R0.value = R1.value;
                //R0.name = R1.name;  

                if (R1.ty == VALTYPE.funccon
                    || (R1.ty == VALTYPE.hostcon && (R1.value is MethodInfo || R1.value is MethodInfo[]))
                    ) 
                    R0.temp = R1.temp;      //instance of CPU 
            }
            return R0;
        }



        #region HostType Assign
        public static bool HostTypeAssign(VAL R0, VAL R1)
        {
            /***
             * 
             * CASE 2:  
             *      textbox1.Text ="Hello";
             * 
             * */
            if (R0.temp != null && R0.temp is HostOffset)
            {
                HostOffset hosts = (HostOffset)R0.temp;
                object host = hosts.host;
                object offset = hosts.offset;

                if (offset is MethodInfo)   
                    return false;

                return HostTypeAssign(host, offset, R1.HostValue, R1.hty == HandlerActionType.Add); 
            }

         
            
            return false;
        }

        public static bool HostTypeAssign(object host, object offset, object val, bool addHandler)
        {
            Type type = host.GetType();




            if (offset is string)
            {
                PropertyInfo propertyInfo = type.GetProperty((string)offset);
                if (propertyInfo != null)
                {
                    if (propertyInfo.CanWrite)
                    {
                        if (IsCompatibleType(propertyInfo.PropertyType, val, null))
                        {
                            propertyInfo.SetValue(host, val, null);
                            return true;
                        }
                        else
                            throw new HostTypeValueNotMatchedException(string.Format("{0} is not matched to property {1}.{2}", val, type.FullName, offset));
                    }
                    else
                        throw new HostTypeException("property {0}.{1} is read only.", type.FullName, offset);
                }
            }

            if (host.GetType().IsArray)
            {
                Array array = (Array)host;
                if (offset is int)
                {
                    if ((int)offset >= array.Length)
                        throw new HostTypeException("Array {0}[{1}] index is out of range[0..{2}].", host, offset, array.Length);

                    array.SetValue(val, (int)offset);
                    return true;
                }
                else if (offset is int[])
                {
                    array.SetValue(val, (int[])offset);
                    return true;
                }
                else
                    throw new HostTypeException("Array {0}.[{1}] subscript must be integer.", host.GetType().FullName, offset);
            }

            {
                Type offsetType = (offset != null) ? offsetType = offset.GetType() : typeof(object);
                Type valType = (val != null)? val.GetType() : typeof(object);

                Type[] types;
                object[] objectArray;

                if (offset is Array)
                {
                    Array array = (Array)offset;
                    types = new Type[array.Length + 1];
                    objectArray = new object[array.Length + 1];
                    int i = 0;
                    foreach (object obj in array)
                    {
                        types[i] = obj.GetType();
                        objectArray[i] = obj;
                        i++;
                    }
                    types[i] = valType;
                    objectArray[i] = val;
                }
                else  
                {
                    types = new Type[] { offsetType, valType };
                    objectArray = new object[] { offset, val };
                }

                MethodInfo methodInfo = type.GetMethod("set_Item", types);
                if (methodInfo != null)
                {
                    methodInfo.Invoke(host, objectArray);
                    return true;
                }
            }

            FieldInfo fieldInfo = type.GetField((string)offset);
            if (fieldInfo != null)
            {
                if (IsCompatibleType(fieldInfo.FieldType, val, null))
                {
                    fieldInfo.SetValue(host, val);
                    return true;
                }
                else
                    throw new HostTypeValueNotMatchedException(string.Format("{0} is not matched to field {1}.{2}", val, type.FullName, offset));

            }

            EventInfo eventInfo = type.GetEvent((string)offset);
            if (eventInfo != null)
            {
                if (val is Delegate)
                {
                    if (addHandler)
                        eventInfo.AddEventHandler(host, val as Delegate);
                    else
                        eventInfo.RemoveEventHandler(host, val as Delegate);
                    return true;
                }
                else
                    throw new HostTypeException("{0} is not delegate of {1}.{2}.", val, type.FullName, offset);
            }

            //BAD Performance
            //throw new HostTypeMemberNotFoundException(string.Format("Property/Attribute [{0}] is not supported in class {1}", offset, type.FullName));
            return false;
        }

        #endregion


        





        #region HostType Offset
      
        public static VAL HostTypeOffset(VAL R0, VAL R1, OffsetType offsetType)
        {
            if (R0.ty != VALTYPE.hostcon)
                return VAL.VOID;

            
            object host = R0.value;

            object offset = R1.HostValue;
          
            
            object obj = null;
            Type type = null;

            if (host is Type)
            {
                type = (Type)host;

                if (!(offset is string))
                    throw new HostTypeException("{0} offset {1} must be ident type.", type.FullName, offset);

                if (type.IsEnum)           
                {
                    FieldInfo fieldInfo = type.GetField((string)offset);
                    if (fieldInfo != null && fieldInfo.IsStatic)
                        return VAL.Boxing1(fieldInfo.GetValue(host));
                    else
                        throw new HostTypeException("enum {0} offset {1} is not enum type.", type.FullName, offset);
                }

                if (offsetType == OffsetType.STRUCT || offsetType == OffsetType.ANY)
                {
                    return HostTypeOffsetMemberInfo(type, host, offset);
                }
            }

            type = host.GetType();

            
            if (offsetType == OffsetType.ANY || offsetType == OffsetType.ARRAY)
            {
                //abstract Array: IList, ICollection, IEumerable
                //interface IList : ICollection, IEumerable
                //interface ICollection : IEumerable
                if (type.IsArray)
                {
                    Array array = (Array)host;
                    if (offset is int)
                    {
                        if ((int)offset >= array.Length)
                            throw new HostTypeException("Array {0}[{1}] index is out of range[0..{2}].", host, offset, array.Length);
                        return HostTypeOffsetBoxing(array.GetValue((int)offset), host, offset);
                    }
                    else if (offset is int[])
                    {
                        return HostTypeOffsetBoxing(array.GetValue((int[])offset), host, offset);
                    }
                    else
                        throw new HostTypeException("Array {0}.[{1}] subscript must be integer.", host.GetType().FullName, offset);
                }

                if (host is IEnumerable && offset is int)
                {
                    IEnumerable collection = (IEnumerable)host;
                    int index = (int)offset;

                    IEnumerator enumerator = collection.GetEnumerator();

                    bool end = false;
                    int count = 0;
                    for (int i = 0; i < index + 1; i++)
                        if (!enumerator.MoveNext())
                        {
                            end = true;
                            count = i;
                            break;
                        }

                    if (end && index >= count)
                        throw new HostTypeException("IEnumerable {0}[{1}] index is out of range[{2}].", host, offset, count);

                    if (!end)
                        return HostTypeOffsetBoxing(enumerator.Current, host, offset);
                }

                
                {   
                    Type[] types;
                    object[] objectArray;

                    if (R1.ty == VALTYPE.listcon)
                    {
                        types = new Type[R1.Size];
                        for (int i = 0; i < types.Length; i++)
                        {
                            types[i] = R1[i].Type;
                        }

                        objectArray = R1.ObjectArray;
                    }
                    else
                    {
                        types = new Type[] { R1.Type };
                        objectArray = new object[] { offset };
                    }
                  
                    MethodInfo methodInfo = type.GetMethod("get_Item", types);
                    if (methodInfo != null)
                    {
                        try
                        {
                            obj = methodInfo.Invoke(host, objectArray);
                            if (obj != null)
                                return HostTypeOffsetBoxing(obj, host, offset);
                        }
                        catch (Exception e)
                        {
                            if (offsetType == OffsetType.ARRAY)
                                throw e;
                        }
                    }
                }


                if (offsetType == OffsetType.ARRAY)
                    return VAL.VOID;
            }

            if (offset is string)
               return HostTypeOffsetMemberInfo(type,host, offset);
            else
               return HostTypeOffsetBoxing(null, host, offset);

        }

        private static VAL HostTypeOffsetMemberInfo(Type type, object host, object offset)
        {
            object obj = null;

            if (offset.Equals("FullName"))
                return new VAL(type.FullName);


            FieldInfo fieldInfo = type.GetField((string)offset);
            if (fieldInfo != null)
            {
                obj = fieldInfo.GetValue(host);
                return HostTypeOffsetBoxing(obj, host, offset);
            }

            PropertyInfo propertyInfo = type.GetProperty((string)offset);
            if (propertyInfo != null)
            {
                if (propertyInfo.CanRead)
                    obj = propertyInfo.GetValue(host, null);

                return HostTypeOffsetBoxing(obj, host, offset);
            }

            MethodInfo[] methods = HostFunction.OverloadingMethods(type, (string)offset);
            if (methods.Length > 0)
            {
                if (methods.Length == 1)
                    return HostTypeOffsetBoxing(methods[0], host, offset);

                return HostTypeOffsetBoxing(methods, host, offset);
            }

            EventInfo eventInfo = type.GetEvent((string)offset);
            if (eventInfo != null)
                return HostTypeOffsetBoxing(eventInfo, host, offset);

            return VAL.VOID;
        }

        private static VAL HostTypeOffsetBoxing(object value, object host, object offset)
        {
            VAL v = VAL.Boxing1(value);
            v.temp = new HostOffset(host, offset);
            return v;
        }


      
        #endregion





        #region HostType Function/Method

        public static VAL HostTypeFunction(VAL proc, VALL parameters)
        {
            VAL ret = VAL.VOID;
            if (proc.ty == VALTYPE.hostcon && (proc.value is MethodInfo || proc.value is MethodInfo[]))
            {
                HostOffset temp = (HostOffset)proc.temp;
                object host = temp.host;
                object offset = temp.offset; 

                if (offset is MethodInfo)
                    ret = VAL.Boxing1(((MethodInfo)proc.value).Invoke(host, parameters.ObjectArray)); 
                else
                {
                    HostFunction hFunc = new HostFunction(host, (string)offset, parameters);

                    if (proc.value is MethodInfo[])
                        ret = hFunc.RunFunction((MethodInfo[])proc.value);    
                    else
                    {
                        MethodInfo method = (MethodInfo)proc.value;
                        if (method.IsGenericMethod)
                            ret = hFunc.RunFunction(new MethodInfo[] { method }); 
                        else
                            ret = hFunc.RunFunction();     
                    }
                }
            }
            else if(proc.value is Delegate)      
            {
                Delegate d = (Delegate)proc.value;
                MethodInfo method = d.Method;
                object[] arguments = parameters.ObjectArray;
                return HostFunction.InvokeMethod(method, d.Target, arguments);
            }
            return ret;
        }


      
        #endregion



        #region Compatible Type 

        public static bool IsCompatibleType(Type type, object val, Type valType)
        {

            if (val == null)
            {
                if (valType != null)
                    return type.IsAssignableFrom(valType);

                if (!type.IsValueType)
                    return true;
                else
                {
                    if (Nullable.GetUnderlyingType(type) == null)
                        throw new HostTypeValueNotMatchedException(string.Format("Value type property {0} cannot be assigned by null", type.FullName));
                    else
                        return true;
                }
            }
            else
            {
                valType = val.GetType();

                if (type.IsAssignableFrom(valType))
                    return true;

                else if (type.IsEnum && val is int)
                    return true;

                else if (type.IsGenericType && Nullable.GetUnderlyingType(type) == valType)
                    return true;

                else 
                    return false;
            }

        }



        #endregion


        
        #region HostType Compare
        
        public static int HostCompareTo(Operator opr, VAL v1, VAL v2)
        {
            if (v1.ty != VALTYPE.hostcon || v2.ty != VALTYPE.hostcon)
                throw new HostTypeException("cannot compare different type value {0} and {1}.", v1, v2);

            object x1 = v1.value;
            object x2 = v2.value;

            if (x1 == null && x2 == null)
                return 0;

            if (x1 != null && x2 == null)
            {
                if (x1.GetType().IsValueType)   
                    throw new HostTypeException("cannot compare value type {0} to null.", x1);
                else
                    return 1;                   
            }
            else if (x1 == null && x2 != null)
            {
                if (x2.GetType().IsValueType)   
                    throw new HostTypeException("cannot compare value type {0} to null.", x2);
                else
                    return -1;                      
            }


            if (x1 is Type && x2 is Type)
            {
                if ((Type)x1 == (Type)x2)
                    return 0;
                else
                    return -1;
            }
            else if (!(x1 is Type) && (x2 is Type) || (x1 is Type) && !(x2 is Type))
            {
                throw new HostTypeException("cannot compare type to non-type: {0} and {1}.", x1, x2);
            }

            Type type1 = x1.GetType();
            Type type2 = x2.GetType();
            
            if (type1.IsValueType && type2.IsValueType)
            {
                if (System.ValueType.Equals(x1, x2))
                    return 0;
            }


            Type[] I = type1.GetInterfaces();
            if (I.Length != 0)
            {
                foreach (Type i in I)
                {
                    if (i == typeof(IComparable))
                        return (x1 as IComparable).CompareTo(x2);
                }
            }

            I = type2.GetInterfaces();
            if (I.Length != 0)
            {
                foreach (Type i in I)
                {
                    if (i == typeof(IComparable))
                        return (x2 as IComparable).CompareTo(x1);
                }
            }


            //operator overloading >, >=, <, <=, !=, ==
            VAL comp = HostFunction.OperatorOverloading(opr, v1, v2, true);
            if ((object)comp != null)
            {
                switch (opr)
                {
                    case Operator.op_LessThan:
                        return comp.Boolcon? - 1: 10;
                    case Operator.op_Equality:
                        return comp.Boolcon ? 0 : 10;
                    case Operator.op_GreaterThan:
                        return comp.Boolcon ? 1 : -10;
                }
            }


            Type type = HostCoding.CommonBaseClass(new object[] { x1, x2});
            if (type != null)
            {
                if (x1 == x2)
                     return 0;
            }


            throw new HostTypeException("cannot compare value {0} and {1} without implement IComparable.", x1, x2);

        }

        #endregion


        #region HostType Enum

        public static VAL EnumOperation(VAL R0, VAL R1, object value)
        {
            Type type0 = R0.value.GetType();
            Type type1 = R1.value.GetType();

            Type type = null;
            if (type1.IsEnum)       
                type = type1;
            else if (type0.IsEnum)
                type = type0;

            if (type != null)
                return VAL.Boxing1(Enum.ToObject(type, value)); 
            else
                return VAL.Boxing1(value);                      

        }


        public static string EnumBitFlags(object host)
        {
            Type type = host.GetType();

            if(Enum.IsDefined(type,host))
                return string.Format("{0}.{1}", type.FullName, host);

            string s = "";

            foreach (FieldInfo fieldInfo in type.GetFields())
            {
                if (!fieldInfo.IsLiteral)
                    continue;

                int offset = (int)fieldInfo.GetValue(type);
                if (offset != 0 && ((int)host & offset) == offset)
                {
                    if (s != "")
                        s += "|";
                    s += string.Format("{0}.{1}", type.FullName, Enum.ToObject(type, offset).ToString());
                }
            }

            return s;

        }
        
        #endregion

    }
}
