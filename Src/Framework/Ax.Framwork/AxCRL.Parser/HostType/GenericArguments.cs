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
    class GenericArguments
    {
        Dictionary<string, Type> dict = new Dictionary<string, Type>(); 

        public GenericArguments()
        {
        }

        public object CheckCompatibleType(ParameterInfo parameter, object val, Type valType)
        {
            Type parameterType = parameter.ParameterType;

            if (parameterType.IsGenericType)
            {
                GenericArgument ga = new GenericArgument(this, parameterType, valType);
                return ga.PrepareArgument(val);
            }

            if (parameterType.IsSubclassOf(typeof(MulticastDelegate)))
                return DynamicDelegate.ToDelegate(parameterType, val);

            return null;
        }




        public bool MatchGenericMethod(MethodInfo method1, MethodInfo method2)
        {

            ParameterInfo[] parameters1 = method1.GetParameters();
            ParameterInfo[] parameters2 = method2.GetParameters();
            if (parameters1.Length != parameters2.Length)
                return false;

            if (MatchGenericParameter(method1.ReturnType, method2.ReturnType) == null)
                return false;

            for (int k = 0; k < parameters1.Length; k++)
            {
                if (MatchGenericParameter(parameters1[k].ParameterType, parameters2[k].ParameterType) == null)
                    return false;
            }

            return true;
        }


        public bool MatchGenericParameters(Type ty1, Type ty2)
        {
            Type gty1 = ty1.GetGenericTypeDefinition();
            Type[] gpty1 = ty1.GetGenericArguments();

            if (ty2.IsGenericType)
            {
                Type gty2 = ty2.GetGenericTypeDefinition();
                Type[] gaty2 = ty2.GetGenericArguments();

                if (gty1 == gty2 && gpty1.Length == gaty2.Length)
                {
                    for (int k = 0; k < gpty1.Length; k++)
                        if (MatchGenericParameter(gpty1[k], gaty2[k]) == null)
                            return false;

                    return true;
                }
            }
            return false;
        }


        public Type[] ConstructGenericArguments(Type[] gaty1)
        {
            Type[] gaty2 = new Type[gaty1.Length];
            for (int k = 0; k < gaty1.Length; k++)
            {
                gaty2[k] = MatchGenericParameter(gaty1[k], gaty1[k]);
            }
            return gaty2;

        }



        private Type MatchGenericParameter(Type gpty1, Type gpty2)
        {
            if (gpty1.IsGenericParameter)       
            {
                if (dict.ContainsKey(gpty1.Name))
                    return dict[gpty1.Name];
                else
                {
                    if (gpty2.IsGenericParameter)   
                        gpty2 = typeof(object);     

                    dict.Add(gpty1.Name, gpty2);
                    return gpty2;
                }
            }
            else
            {
                if (gpty2.IsGenericParameter)      
                {
                    return gpty1;
                }
                else
                {
                    if (gpty1 == gpty2 || gpty2.IsSubclassOf(gpty1)) 
                        return gpty2;
                    else
                        return null;
                }
            }



        }
    }




}
