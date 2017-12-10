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
    /// Function chain, the late added function will be invoked first
    /// </summary>
    public sealed class FunctionChain
    {
        private static FunctionChain chain;

        internal static FunctionChain Chain
        {
            get 
            {
                if (chain == null)
                    chain = new FunctionChain();

                return chain;
            }
        }
        
        private List<object> functions;

        private FunctionChain()
        {
            functions = new List<object>();
        }

        /// <summary>
        /// Add one function into chain with delegate implmentation
        /// </summary>
        /// <param name="func">function name</param>
        /// <param name="body">function body</param>
        /// <returns></returns>
        public FunctionChain Add(string func, Function1 body)
        {
            Remove(func);
            functions.Insert(0, new KeyValuePair<string, Function1>(func, body));
            return this;
        }

        /// <summary>
        /// Add multiple functions into chain with delegate implementation
        /// </summary>
        /// <param name="func">function</param>
        /// <returns></returns>
        public FunctionChain Add(Functionn func)
        {
            functions.Insert(0, func);
            return this;
        }

        /// <summary>
        /// Add function into chain with interface implementation
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        public FunctionChain Add(IUserDefinedFunction func)
        {
            functions.Insert(0, func);
            return this;
        }


        /// <summary>
        /// Remove last added function from chain, it is equivalent to (dequeue)
        /// </summary>
        /// <returns></returns>
        public FunctionChain Remove()
        {
            functions.RemoveAt(0); 
            return this;
        }

        /// <summary>
        /// Remove function from chain
        /// </summary>
        /// <param name="func">function name</param>
        /// <returns></returns>
        public FunctionChain Remove(string func)
        {
            object found = null;
            foreach (object function in functions)
            {
                if (function is KeyValuePair<string, Function1>)
                {
                    if ((string)((KeyValuePair<string, Function1>)function).Key  == func)
                    {
                        found = function;
                        break;
                    }
                }
            }
             
            if(found !=null)
                functions.Remove(found);

            return this;
        }

        /// <summary>
        /// Remove function from chain
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        public FunctionChain Remove(Functionn func)
        {
            functions.Remove(func);
            return this;
        }

        /// <summary>
        /// Remove function from chain
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        public FunctionChain Remove(IUserDefinedFunction func)
        {
            functions.Remove(func);
            return this;
        }


        internal VAL Invoke(string func, VAL parameters, Memory DS)
        {
            VAL R0 = null;

            foreach(object function in functions)
            {
                if (function is IUserDefinedFunction)
                {
                    R0 = ((IUserDefinedFunction)function).Function(func, parameters, DS);
                }
                else if (function is Functionn)
                {
                    R0 = ((Functionn)function)(func, parameters, DS);
                }
                else if (function is KeyValuePair<string, Function1>)
                {
                    if (((KeyValuePair<string, Function1>)function).Key == func)
                    {
                        R0 = ((KeyValuePair<string, Function1>)function).Value(parameters, DS);
                    }
                }

                if ((object)R0 != null)
                    return R0;
                    
            }

            return null; 
        }

    }
}
