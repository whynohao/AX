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
    class Library
    {
        private static Dictionary<string, Module> Modules = new Dictionary<string, Module>();

        public Library()
        { }

        public static void AddModule(Module module)
        {
            lock (Modules)
            {
                if (Modules.ContainsKey(module.moduleName))
                    Modules.Remove(module.moduleName);
                Modules.Add(module.moduleName, module);
            }
        }

        public static Module GetModule(string moduleName)
        {
            if (Modules.ContainsKey(moduleName))
                return Modules[moduleName];

            else
                return null;
        }

        public static bool RemoveModule(string moduleName)
        {
            if (Modules.ContainsKey(moduleName))
                return Modules.Remove(moduleName);

            return false;
        }

        public static void ClearLibrary()
        {
            Modules.Clear();
        }


        /// <summary>
        /// Compile module
        /// </summary>
        /// <param name="moduleName"></param>
        /// <param name="moduleSize"></param>
        /// <param name="scope"></param>
        /// <param name="codePiece"></param>
        /// <param name="codeType"></param>
        /// <param name="overwritten"></param>
        /// <returns></returns>
        public static Module CompileModule(ref string moduleName, int moduleSize, string scope, string codePiece, CodeType codeType, CodeMode overwritten)
        {
            Module module = Library.GetModule(moduleName);
            if (module != null)
            {
                if (module.CompileCodeBlock(scope, codePiece, codeType, overwritten))
                {
                    moduleName = module.moduleName;
                    return module;
                }
                else
                    return null;
            }

            module = new Module(moduleName, moduleSize);
            if (module.CompileCodeBlock(scope, codePiece, codeType, overwritten))
            {
                Library.AddModule(module);
                moduleName = module.moduleName;
                return module;
            }

            return null;
        }


        public static void decode(VAL val)
        {
            for (int i = 0; i < val.Size; i++)
            {
                Module module = Module.decode(val[i]);
                Library.AddModule(module);
            }
        }

        public static VAL encode()
        {
            VAL val = VAL.Array();
            foreach (KeyValuePair<string, Module> kvp in Library.Modules)
            {
                val.List.Add(Module.encode(kvp.Value));
            }
            return val;
        }

        public override string ToString()
        {
            return string.Format("Library#{0}", Modules.Count);
        }
    }
}
