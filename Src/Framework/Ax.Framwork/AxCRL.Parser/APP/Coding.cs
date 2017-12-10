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

namespace AxCRL.Parser
{


    //  Examples:
    //
    //    object o = Script.Evaluate("a={40,30},30<a", Script.memory);
    //    Logger.WriteLine(o);
    //
    //     Script.Execute("{var a=3; if(a>10) b=3; else b=5;}",  Script.memory);
    //     Logger.WriteLine("a={0}", Coding.memory["a"][0]);
    //    
    //
    public partial class Script 
    {

        /// <summary>
        /// Evaluate expression 
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static VAL Evaluate(string expression)
        {
            return Computer.Run("", expression, CodeType.expression, new Context());
        }

        /// <summary>
        /// Evaluate expression in the memory indicated
        /// </summary>
        /// <param name="code"></param>
        /// <param name="memory"></param>
        /// <returns></returns>
        public static VAL Evaluate(string code, Memory memory)
        {
            return Computer.Run("", code, CodeType.expression, new Context(memory));
        }

        /// <summary>
        /// Evaluate expression in the memory. the functions in expression are defined in .NET  
        /// </summary>
        /// <param name="code"></param>
        /// <param name="memory"></param>
        /// <param name="userFunc"></param>
        /// <returns></returns>
        public static VAL Evaluate(string code, Memory memory, IUserDefinedFunction userFunc)
        {
            return Computer.Run("", code, CodeType.expression, new Context(memory, userFunc));
        }

        /// <summary>
        /// Evaluate expression, keyword "this" can be used in the expression.
        /// </summary>
        /// <param name="scope">the value of "this" </param>
        /// <param name="code"></param>
        /// <param name="memory"></param>
        /// <param name="userFunc"></param>
        /// <returns></returns>
        public static VAL Evaluate(string scope, string code, Memory memory, IUserDefinedFunction userFunc)
        {
            return Computer.Run(scope, code, CodeType.expression, new Context(memory, userFunc));
        }


        //here tring function is complete Tie function defintion,such as: "function(a,b) { return a+b;}" 
        /// <summary>
        /// invoke a Tie script global function or method
        /// </summary>
        /// <param name="memory"></param>
        /// <param name="instance"></param>
        /// <param name="function">function definiton, e.g. "function(a,b) { return a+b;}" </param>
        /// <param name="parameters"></param>
        /// <param name="userFunc"></param>
        /// <returns></returns>
        public static VAL InvokeFunction(Memory memory, VAL instance, string function, object[] parameters, IUserDefinedFunction userFunc)
        {
            Module module = new Module();
            if (module.CompileCodeBlock("", function, CodeType.expression, CodeMode.Overwritten))
            {
                Context context = new Context(memory, userFunc);
                VAL funcEntry = Computer.Run(module, context);
                if (funcEntry.ty != VALTYPE.funccon && funcEntry.ty != VALTYPE.classcon)
                    throw new TieException("invalid function/class: " + function);

                VAL arguments = new VAL(parameters);
                CPU cpu = new CPU(module, context);
                return cpu.InternalUserFuncCall((int)funcEntry.value, instance, arguments);
            }

            return new VAL();
        }

        /// <summary>
        /// Execute statements
        /// </summary>
        /// <param name="code"></param>
        /// <param name="memory"></param>
        /// <returns></returns>
        public static VAL Execute(string code,  Memory memory)
        {
            return Computer.Run("", code, CodeType.statements, new Context(memory));
        }

        /// <summary>
        /// Execute statements
        /// </summary>
        /// <param name="scope">value of keyword "this"</param>
        /// <param name="code"></param>
        /// <param name="memory"></param>
        /// <returns></returns>
        public static VAL Execute(string scope, string code, Memory memory)
        {
            return Computer.Run(scope, code, CodeType.statements, new Context(memory));
        }

        /// <summary>
        /// Execute statements by using user defined .NET function
        /// </summary>
        /// <param name="code"></param>
        /// <param name="memory"></param>
        /// <param name="userFunc"></param>
        /// <returns></returns>
        public static VAL Execute(string code, Memory memory, IUserDefinedFunction userFunc)
        {
            return Computer.Run("", code, CodeType.statements, new Context(memory, userFunc));
        }

        /// <summary>
        /// Execute statements by using user defined .NET function and scope "this"
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="code">value of keyword "this"</param>
        /// <param name="memory"></param>
        /// <param name="userFunc"></param>
        /// <returns></returns>
        public static VAL Execute(string scope, string code, Memory memory, IUserDefinedFunction userFunc)
        {
            return Computer.Run(scope, code, CodeType.statements, new Context(memory, userFunc));
        }

        /// <summary>
        /// Evaluate expression or Execute statements 
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="code"></param>
        /// <param name="memory"></param>
        /// <param name="userFunc"></param>
        /// <returns></returns>
        public static VAL Run(string scope, string code, Memory memory, IUserDefinedFunction userFunc)
        {
            return Computer.Run(scope, code, CodeType.auto, new Context(memory, userFunc));
        }

        internal static VAL Run(object instance, string code, Memory memory)
        {
            memory.Add("$THIS", VAL.Boxing1(instance));

            if (code.IndexOf("return") == -1)
                return Script.Evaluate("$THIS", code, memory, null);
            else
                return Script.Execute("$THIS", code, memory, null);
        }


    }


}
