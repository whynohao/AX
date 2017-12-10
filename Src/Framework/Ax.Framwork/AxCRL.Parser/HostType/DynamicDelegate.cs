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
using System.Reflection.Emit;

namespace AxCRL.Parser
{
    class DynamicDelegate
    {
        private VAL func = null;   

        private DynamicDelegate(VAL func)
        {
            this.func = func;
        }


        public static object CallFunc(VAL funccon, object[] arguments)
        {
            if (funccon.ty == VALTYPE.funccon)
            {
                ContextInstance temp = (ContextInstance)funccon.temp;
                Context context = temp.context;
                VAL instance = temp.instance;
                VAL ret = CPU.ExternalUserFuncCall(funccon, instance, VAL.Boxing(arguments), context);
                return ret.HostValue;
            }

            throw new HostTypeException("VAL {0} is not funccon type.", funccon);
        }

        public static int FuncArgc(VAL func)
        {
            string moduleName = func.Class;
            Module module = Library.GetModule(moduleName);
            if (module == null)
                return -1;;

            return module.CS[func.Address].operand.Addr -1;
        }



        public static Delegate InstanceDelegate(Type dType, VAL func)
        {
            DynamicDelegate instance = new DynamicDelegate(func);
            FieldInfo funcconField = typeof(DynamicDelegate).GetField("func", BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo methodAdapter = typeof(DynamicDelegate).GetMethod("CallFunc", BindingFlags.Public | BindingFlags.Static);
            return InstanceDelegate(dType, instance, funcconField, methodAdapter);
        }

        public static Delegate InstanceDelegate(Type dType, object target, FieldInfo funcField, MethodInfo methodAdapter)
        {

            MethodInfo dMethod = dType.GetMethod("Invoke");
            ParameterInfo[] dParemeters = dMethod.GetParameters();

            int len = dParemeters.Length;
            Type[] dParameterTypes = new Type[len + 1];
            dParameterTypes[0] = target.GetType();
            for (int i = 0; i < len; i++)
                dParameterTypes[i+1] = dParemeters[i].ParameterType;


            DynamicMethod dynamicMethod = new DynamicMethod(
                Constant.FUNC_CON_INSTANCE_INVOKE,
                dMethod.ReturnType,
                dParameterTypes,
                target.GetType());  

            ILGenerator il = dynamicMethod.GetILGenerator(256);

            il.DeclareLocal(typeof(object[]));      //object[] L0;
            il.DeclareLocal(typeof(object));        //object ret; 
            il.DeclareLocal(dMethod.ReturnType);    

            il.Emit(OpCodes.Ldc_I4, len);
            il.Emit(OpCodes.Newarr, typeof(object));
            il.Emit(OpCodes.Stloc, 0);

            for (int i = 0; i < len; i++)
            {
                il.Emit(OpCodes.Ldloc, 0);    //LOAD L0
                il.Emit(OpCodes.Ldc_I4, i);   //LOAD i
                il.Emit(OpCodes.Ldarg, i+1);  //LOAD arg[i+1]
                if (dParameterTypes[i].IsValueType)
                    il.Emit(OpCodes.Box, dParameterTypes[i]);
                il.Emit(OpCodes.Stelem_Ref);
            }

            il.Emit(OpCodes.Ldarg, 0);              //LOAD target
            il.Emit(OpCodes.Ldfld, funcField);      //LOAD field
            il.Emit(OpCodes.Ldloc, 0);              //LOAD L0
            il.EmitCall(OpCodes.Call, methodAdapter, null);   //CallFunc(VAL, L0);
            il.Emit(OpCodes.Stloc, 1);
            il.Emit(OpCodes.Ldloc, 1);

            if (dMethod.ReturnType.IsValueType)
            {
                il.Emit(OpCodes.Unbox_Any, dMethod.ReturnType);
                il.Emit(OpCodes.Stloc, 2);
                il.Emit(OpCodes.Ldloc, 2);
            }

            il.Emit(OpCodes.Ret);

            for (int i = 0; i < len+1; i++)
                dynamicMethod.DefineParameter(i, ParameterAttributes.In, "arg" + i);

            return dynamicMethod.CreateDelegate(dType, target);
        }


        private static bool CompareMethodSignature(MethodInfo method1, MethodInfo method2)
        {
            ParameterInfo[] parameters1 = method1.GetParameters();
            ParameterInfo[] parameters2 = method2.GetParameters();
            if (parameters1.Length != parameters2.Length)
                return false;

            if (method1.ReturnType != method2.ReturnType)
                return false;

            for (int i = 0; i < parameters1.Length; i++)
                if (parameters1[i].ParameterType != parameters2[i].ParameterType)
                    return false;

            return true;
        }

        public static Type[] GetDelegateParameterTypes(Type d)
        {
            if (d.BaseType != typeof(MulticastDelegate))
                return null;

            MethodInfo invoke = d.GetMethod("Invoke");
            if (invoke == null)
                return null;

            ParameterInfo[] parameters = invoke.GetParameters();
            Type[] typeParameters = new Type[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                typeParameters[i] = parameters[i].ParameterType;
            }
            return typeParameters;
        }

        public static Type GetDelegateReturnType(Type d)
        {
            if (d.BaseType != typeof(MulticastDelegate))
                return null;

            MethodInfo invoke = d.GetMethod("Invoke");
            if (invoke == null)
                return null;

            return invoke.ReturnType;
        }


        public static object ToDelegate(Type type, object val)
        {
            MethodInfo method1 = type.GetMethod("Invoke");
            if (val is MethodInfo)
            {
                MethodInfo method2 = (MethodInfo)val;
                if (CompareMethodSignature(method1, method2))
                {
                    val = Delegate.CreateDelegate(type, null, method2);
                    return val;
                }
            }
            else if (val is MulticastDelegate)
            {
                MethodInfo method2 = ((MulticastDelegate)val).GetType().GetMethod("Invoke");    
                if (CompareMethodSignature(method1, method2))
                    return val;
            }
            else if (val is VAL)
            {
                VAL func = (VAL)val;
                if (func.ty == VALTYPE.funccon) 
                {
#if DEBUG_TIE_DELEGATE
                    DynamicDelegate.funccon = func;
                    return Delegate.CreateDelegate(type, null, typeof(DynamicDelegate).GetMethod("test102"));
#else
                    int argc = DynamicDelegate.FuncArgc(func);                   
                    Type[] pTypes = DynamicDelegate.GetDelegateParameterTypes(type);
                    if (argc == pTypes.Length)                                     
                        return DynamicDelegate.InstanceDelegate(type, func);
                    else
                        return null;
#endif
                }
            }

            return null;
        }



#if DEBUG_TIE_DELEGATE
        public static VAL funccon = null;

        public static int test101(int[] A)
        {
            object[] args = new object[1];
            args[0] = A;
            return (int)DynamicDelegate.CallFunc(funccon, args);
        }

        public static bool test102(string A)
        {
            return A.Length > 6;
        }

#endif

    }
}
