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
    /// Represents non valized attributes.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Field)]
    public class NonValizedAttribute : Attribute
    {
    }


    /// <summary>
    /// Represents valizable attributes.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class)]
    public class ValizableAttribute : Attribute
    {
        internal object valizer;    //TIE Script or Properties List
        internal object devalizer;  //TIE Script

        /// <summary>
        /// initialize instance
        /// </summary>
        public ValizableAttribute()
            :this(null,null)
        {
        }

        /// <summary>
        /// initialize instance by valizer
        /// </summary>
        /// <param name="valizer"></param>
        public ValizableAttribute(string valizer)
            :this(valizer, null)
        {
        }

        /// <summary>
        /// initialize instance by valizer and devalizer
        /// </summary>
        /// <param name="valizer"></param>
        /// <param name="devalizer"></param>
        public ValizableAttribute(string valizer, string devalizer)
        {
            this.valizer = valizer;
            this.devalizer = devalizer;
        }

        /*
         * 
         * members: any Fields or Properties
         * return "{ field1 : this.field1,  property1 : this.property1, ....}"
         * 
         */

         /// <summary>
        ///  initialize instance by members of class
         /// </summary>
         /// <param name="members"></param>
        public ValizableAttribute(string[] members)
        {
            this.valizer = members;
        }

    }

 
}
