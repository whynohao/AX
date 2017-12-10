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

namespace AxCRL.Parser
{
    /// <summary>
    /// User defined function must implement IUserDefinedFunction
    /// </summary>
    public interface IUserDefinedFunction
    {
        /// <summary>
        ///  return null if function is defined which give a chance to call SystemFunction
        /// </summary>
        /// <param name="func">function name</param>
        /// <param name="parameters">function arguments</param>
        /// <param name="DS">data segment memory</param>
        /// <returns></returns>
        VAL Function(string func, VAL parameters, Memory DS);
    }

    /// <summary>
    /// define a function body
    /// </summary>
    /// <param name="Parameters"></param>
    /// <param name="DS"></param>
    /// <returns></returns>
    public delegate VAL Function1(VAL Parameters, Memory DS);       //single function

    /// <summary>
    /// define multiple functions 
    /// </summary>
    /// <param name="func"></param>
    /// <param name="Parameters"></param>
    /// <param name="DS"></param>
    /// <returns></returns>
    public delegate VAL Functionn(string func, VAL Parameters, Memory DS);  //function set
   
}
