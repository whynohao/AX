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
    ///  Exception occurs on script executed
    /// </summary>
    public class RuntimeException : PositionException
    {
        /// <summary>
        /// Initializes a new instance of the Exception class.
        /// </summary>
        /// <param name="message"></param>
        protected RuntimeException(Position position, string message)
            : base("RUNTIME " + message, position)
        {
        }

        internal RuntimeException(Position position, string format, params object[] args)
            :this(position, string.Format(format, args))
        {
        }

        internal static void Warning(string format, params object[] args)
        {
            Logger.WriteLine(
               string.Format("Warning :{0}", string.Format(format, args)));
        }
    }

    /// <summary>
    /// Exception occurs when function is not found
    /// </summary>
    public class FunctionNotFoundException : RuntimeException
    {
        /// <summary>
        /// Initializes a new instance of the Exception class.
        /// </summary>
        /// <param name="message"></param>
        internal FunctionNotFoundException(Position position, string message)
            : base(position, message)
        {

        }


    }
}
