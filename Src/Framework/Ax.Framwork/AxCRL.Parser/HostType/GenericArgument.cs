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

#if TIE4
using System.Linq;
using System.Linq.Expressions;
#endif

namespace AxCRL.Parser
{

    class GenericArgument
    {
        private GenericArguments gas;

        private Type parameterType;
        private Type valType;

        public GenericArgument(GenericArguments genericArguments, Type parameterType,  Type valType)
        {
            this.gas = genericArguments;
            this.parameterType = parameterType;
            this.valType = valType;

        }

        public object PrepareArgument(object val)
        {
            if (parameterType.IsInterface)                  
            {
                Type[] I = valType.GetInterfaces();
                foreach (Type type in I)
                {
                    if (gas.MatchGenericParameters(parameterType, type))
                        return val;
                }
            }
            else if (parameterType.IsClass)
            {

                if (parameterType.IsSubclassOf(typeof(MulticastDelegate))) 
                {
                    return PrepareDelegate(parameterType, val);
                }
#if TIE4
                //Linq SQL, Expression Tree
                else if (parameterType.IsSubclassOf(typeof(LambdaExpression)))
                {
                    /*
                    Type funcGenericType = parameterType.GetMethod("Compile", new Type[] { }).ReturnType; 
                    object d = Convert2Delegate(funcGenericType, val, funcGenericType.GetGenericTypeDefinition(), funcGenericType.GetGenericArguments());
                    Type[] funcGenericParameterTypes = ConstructGenericArguments(funcGenericType.GetGenericArguments());
                    
                    Type exprGenericType = funcGenericType.GetGenericTypeDefinition().MakeGenericType(funcGenericParameterTypes);  
                    Type exprType = parameterType.GetGenericTypeDefinition().MakeGenericType(new Type[] { exprGenericType });
                    
                    //convert delegate into expression tree, don't know how to implment DelegateConverter.ToExpression(d); 
                    object exprTree = null;   
                    return Activator.CreateInstance(exprType, new object[] { exprTree });
                    */

                    throw new NotImplementedException("Linq to SQL not implemented yet in TIE");
                }
#endif
                else if (gas.MatchGenericParameters(parameterType, valType))   
                    return val;
            }

            return null;

        }

        
        private object PrepareDelegate(Type parameterType, object val)
        {
            Type gty1 = parameterType.GetGenericTypeDefinition();
            Type[] gpty1 = parameterType.GetGenericArguments();

        
            MethodInfo method1 = parameterType.GetMethod("Invoke");
            if (val is MethodInfo)
            {
                MethodInfo method2 = (MethodInfo)val;
                if (gas.MatchGenericMethod(method1, method2))
                {
                    Type[] gpty2 = gas.ConstructGenericArguments(gpty1);
                    parameterType = gty1.MakeGenericType(gpty2);
                    val = Delegate.CreateDelegate(parameterType, null, method2);
                    return val;
                }
                else
                    return null;
            }
            else if (val is MulticastDelegate)
            {
                MethodInfo method2 = ((MulticastDelegate)val).GetType().GetMethod("Invoke");
                if (gas.MatchGenericMethod(method1, method2))
                    return val;
                else
                    return null;
            }
            else if (val is VAL)
            {
                VAL func = (VAL)val;
                if (func.ty == VALTYPE.funccon)
                {
                    int argc = DynamicDelegate.FuncArgc(func);
                    Type[] pTypes = DynamicDelegate.GetDelegateParameterTypes(parameterType);
                    if (argc == pTypes.Length)         
                    {
                        Type[] gpty2 = gas.ConstructGenericArguments(gpty1); 
                        if (gpty2 == null)
                            throw new HostTypeException("Generic Type is not matched on {0}", parameterType);

                        parameterType = gty1.MakeGenericType(gpty2);
                        return DynamicDelegate.ToDelegate(parameterType, val);
                    }
                    return null;
                }
            }

            return null;
        }

    }
}
