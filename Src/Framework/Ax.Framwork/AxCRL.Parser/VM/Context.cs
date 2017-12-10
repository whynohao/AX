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
 
    class Context
    {
        private Memory DS0;      //System Level variables
        private Memory DS1;      //User Global variables
        private Memory DS2;      //User Temp variables

        protected IUserDefinedFunction userFunc;
        public Context()
            :this(null, null, null)
        {
            this.DS2 = Computer.DS2;
            this.DS1 = Computer.DS1;
        }

        public Context(Memory DS2)
            : this(DS2, null, null)
        {
            this.DS1 = Computer.DS1;
        }

        public Context(Memory DS2, IUserDefinedFunction userFunc)
            : this(DS2, null , userFunc)
        {
            this.DS1 = Computer.DS1;
        }

        public Context(Memory DS2, Memory DS1, IUserDefinedFunction userFunc)
        {
            this.userFunc = userFunc;
            this.DS2 = DS2;
            this.DS1 = DS1;
            this.DS0 = new Memory();
            Init(DS0);
        }

        public IUserDefinedFunction UserFunction
        {
            set
            {
                userFunc = value;
            }

            get
            {
                return userFunc;
            }
        }


       
        public Memory DataSegment
        {
            get
            {
                return DS2;
            }
            set
            {
                DS2 = value;
            }
        }


        public VAL InvokeFunction(string func, VAL parameters, Position position)
        {
            VAL ret = SystemFunction.Function(func, parameters, DS2, position);

            if ((object)ret == null)
            {
                if (userFunc != null)
                    ret = userFunc.Function(func, parameters, DS2);
            }

            if ((object)ret == null)
                ret = FunctionChain.Chain.Invoke(func, parameters, DS2); 


            if ((object)ret == null)
            {
#if !EASYWAY
                throw new FunctionNotFoundException(position, string.Format("function {0}({1}) is not defined, or arguments are not matched.", func, parameters.List.ToString2()));
#else
            VAL ret = new VAL(func, L);
            return ret;
#endif
            }
            
            return ret;
        }

        public  VAL GetVAL(string ident, bool readOnly)
        {
            if (DS2 != null && DS2.ContainsKey(ident))         //user temp variable
                return DS2[ident];

            if (DS1 != null && DS1.ContainsKey(ident))         //user global variable
                return DS1[ident];

            if (DS0.ContainsKey(ident))                        //CPU system level memory
                return DS0[ident];

            if (readOnly)
                return new VAL();

            VAL v = new VAL();
            v.name = ident;

            if (DS2 != null)
            {
                DS2.Add(ident, v);
                return DS2[ident];
            }
            else if (DS1 != null)
            {
                DS1.Add(ident, v);
                return DS1[ident];
            }
            else
            {
                DS0.Add(ident, v);
                return DS0[ident];
            }
        }


        //system level variable
        static void Init(Memory ds)
        {
            // ds.Add("Count", new VAL(true));    // List.Count, refer: File(VAL.cs) 
            // class VALL,  public VAL this[VAL arr]
        }
    }

}