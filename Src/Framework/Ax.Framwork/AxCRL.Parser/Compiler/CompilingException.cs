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
    public class TieException : Exception
    {
        public TieException(string message)
            : base(message)
        { 
        
        }

        internal TieException(string format, params object[] args)
            : base(string.Format(format, args))
        {

        }
    }

    /// <summary>
    /// Exception occurs on the position of source code.
    /// </summary>
    public abstract class PositionException : TieException
    {
       

        protected Position position;
        
        /// <summary>
        /// Initializes a new instance of the Exception class.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="position"></param>
        public PositionException(string message, Position position)
            : base(string.Format("{0} {1}", message, position.ToString()))
        {
            this.position = position;
            Logger.Close();
        }

        /// <summary>
        /// returns position error occured
        /// </summary>
        public Position Position
        {
            get
            {
                return this.position;
            }
        }

    }

    /// <summary>
    ///   Represents errors that occur during compiling.
    /// </summary>
    public sealed class CompilingException : PositionException
    {
        /// <summary>
        /// Initializes a new instance of the Exception class.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="position"></param>
        internal CompilingException(string message, Position position)
            : base("SYNTAX " + message, position)
        {
        }

    }

}
