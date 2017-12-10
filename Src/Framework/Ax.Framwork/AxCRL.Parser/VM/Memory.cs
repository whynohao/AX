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
    /// Variable dictionary
    /// </summary>
    public class Memory
    {
        Dictionary<string, VAL> ds = new Dictionary<string, VAL>();

        /// <summary>
        /// Initializes a new instance
        /// </summary>
        public Memory()
        {
        }

        /// <summary>
        /// Initializes a new instance by associative array
        /// </summary>
        /// <param name="dict"></param>
        public Memory(VAL dict)
        { 
            for (int i = 0; i < dict.Size; i++)
            {
                Add(dict[i][0].Str, dict[i][1]);
            }
        }

        /// <summary>
        /// Add host object variable
        /// </summary>
        /// <param name="key"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public VAL AddHostObject(string key, object v)
        {
            return Add(key, VAL.NewHostType(v));
        }

        /// <summary>
        /// Add object variable into data segment
        /// </summary>
        /// <param name="key"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public VAL AddObject(string key, object v)
        {
            return Add(key, VAL.Boxing1(v));
        }

        /// <summary>
        /// Add VAL variable 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public VAL Add(string key, VAL v)
        {
            bool removed = false;
            VAL oldValue = null;

            if (ds.ContainsKey(key))
            {
                oldValue = ds[key];
                removed = ds.Remove(key);
            }

            v.name = key;
            ds.Add(key, v);

            return oldValue;
        }

        /// <summary>
        /// Add memory variable
        /// </summary>
        /// <param name="spacename"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public VAL Add(string spacename, Memory source)
        {
            VAL v = Assemble(source);
            this.Add(spacename, v);
            return v;
        }

        /// <summary>
        /// Dictionary of varible
        /// </summary>
        public Dictionary<string, VAL> DS
        {
            get
            {
                return ds;
            }
        }

        /// <summary>
        /// Get value by variable name
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public VAL this[string key]
        {
            get
            {
                if (ds.ContainsKey(key))
                    return ds[key];
               else
                   return VAL.VOID; 
            }
            set
            {
                Add(key, value);              //ds[key] = value;
            }
        }

        internal bool ContainsKey(string key)
        {
            return ds.ContainsKey(key);
        }

        /// <summary>
        /// Clear varible dictionary
        /// </summary>
        public void Clear()
        {
            ds.Clear();
        }

        /// <summary>
        /// Remove a variable
        /// </summary>
        /// <param name="key">varible name</param>
        /// <returns></returns>
        public bool Remove(string key)
        {
            if (ds.ContainsKey(key))
                return ds.Remove(key);

            return false;
        }
        
        
        /// <summary>
        ///   Converts the value of this instance to a System.String.
        /// </summary>
        /// <returns> A string whose value is the same as this instance.</returns>
        public override string ToString()
        {
            StringBuilder code = new StringBuilder();
            foreach (KeyValuePair<string, VAL> kvp in DS)
            {
                if (kvp.Value.ty != VALTYPE.nullcon)
                    code.Append(string.Format("{0}={1};", kvp.Key, kvp.Value));
            }
            return code.ToString();
        }


   

        internal static VAL Assemble(Memory memory)
        {
            VALL L = new VALL();
            foreach (KeyValuePair<string, VAL> pair in memory.ds)
            {
                //if (pair.Value.ty == VALTYPE.funccon || pair.Value.ty == VALTYPE.classcon)
                //    continue;

                L.Add(pair.Key, pair.Value);
            }
            return new VAL(L);

        }



        /// <summary>
        /// compress varible names, some varibles may belong to one
        /// </summary>
        /// <param name="keyNames"></param>
        /// <returns>varible name list</returns>
        public static List<string> CompressKeyNames(List<string> keyNames)
        {
            StringBuilder code = new StringBuilder("{");
            foreach (string key in keyNames)
                code.Append(key).Append("=0;");
            code.Append("}");

            Memory SS = new Memory();
            Computer.Run("", code.ToString(), CodeType.statements, new Context(SS));

            List<string> compactedKeyNames = new List<string>();
            foreach (KeyValuePair<string, VAL> kvp in SS.DS)
            {
                compactedKeyNames.Add(kvp.Key);
            }

            return compactedKeyNames;
        }

        /// <summary>
        /// Copy some varibles into new varible dictionary
        /// </summary>
        /// <param name="compactedKeyNames"></param>
        /// <returns></returns>
        public Memory CopyBlock(List<string> compactedKeyNames)
        {

            Memory XS = new Memory();
            foreach (string key in compactedKeyNames)
            {
                if (DS.ContainsKey(key))
                {
                    VAL x = DS[key];
                    if (x.ty != VALTYPE.nullcon)
                        XS.Add(key, x);
                }
            }
            return XS;
        }

        /// <summary>
        /// Remove unchanged varibles
        /// </summary>
        /// <param name="referenceMemory">reference varibles</param>
        public void RemoveValueUnchangedBlock(Memory referenceMemory)
        {
            foreach (KeyValuePair<string, VAL> kvp in DS)
            {
                if (referenceMemory[kvp.Key] != kvp.Value)
                    DS.Remove(kvp.Key);
            }
            return;
        }

        /// <summary>
        /// get varible name list
        /// </summary>
        /// <returns>varible name list</returns>
        public List<string> ExtractKeyNames()
        {
            List<string> keyNames = new List<string>();
            foreach (KeyValuePair<string, VAL> kvp in DS)
                keyNames.Add(kvp.Key);

            return keyNames;
        }

        /// <summary>
        /// explicit convert varible dictionary into VAL associative array
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static explicit operator VAL(Memory x)
        {
            return new VAL(x);
        }

    }
}

