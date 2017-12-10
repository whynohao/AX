//--------------------------------------------------------------------------------------------------//
//                                                                                                  //
//        Tie                                                                                       //
//                                                                                                  //
//          Copyright(c) Datum Connect Inc.                                                         //
//                                                                                                  //
// This source code is subject to terms and conditions of the Datum Connect Software License. A     //
// copy of the license can be found in the License.html file at the root of this distribution. If   //
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
    ///  Allows an object to control its own valization and devalization.
    /// </summary>
    public interface IValizable
    {
        /// <summary>
        /// Populates a ValizationInfo with the data
        ///     needed to valize the target object.
        /// </summary>
        /// <returns></returns>
        VAL GetValData();
    }


    /// <summary>
    ///  Stores all the data needed to valization or devalization an object. This class
    ///     cannot be inherited.
    /// </summary>
    public class ValizationInfo
    {
        VAL dict = VAL.Array();

        /// <summary>
        /// Creates a new instance 
        /// </summary>
        public ValizationInfo()
        {
        }

        /// <summary>
        /// Creates a new instance 
        /// </summary>
        /// <param name="val"></param>
        public ValizationInfo(VAL val)
        {
            this.dict = val;
        }

        /// <summary>
        /// Adds value into the ValizationInfo store
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void AddValue(string key, VAL value)
        {
            dict.List.Add(key, value);
        }

        /// <summary>
        /// Retrieves a value from the ValizationInfo store.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public VAL GetValue(string key)
        {
            return dict[key];
        }

        /// <summary>
        /// return ValizationInfo store
        /// </summary>
        /// <returns></returns>
        public VAL ToVAL()
        {
            return dict;
        }
    }

}