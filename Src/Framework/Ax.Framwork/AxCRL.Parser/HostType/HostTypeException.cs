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
    ///  Represents errors that occur during host object operation.
    /// </summary>
    public class HostTypeException : TieException
    {
        /// <summary>
        /// Initializes a new instance of the Exception class.
        /// </summary>
        /// <param name="message"></param>
        internal HostTypeException(string message)
            : base(message)
        {

        }

        internal HostTypeException(string format, params object[] args)
            : base(string.Format(format, args))
        {

        }

    }

    
    /// <summary>
    /// Represents errors that Host value is not matached.
    /// </summary>
    public class HostTypeValueNotMatchedException : HostTypeException
    {
        /// <summary>
        /// Initializes a new instance of the Exception class.
        /// </summary>
        /// <param name="message"></param>
        internal HostTypeValueNotMatchedException(string message)
            : base(message)
        {

        }
    }

   

}
