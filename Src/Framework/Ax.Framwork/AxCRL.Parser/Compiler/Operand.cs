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

    enum OPRTYPE
    {
        none,
        numcon,

        funccon,
        classcon,
        
        intcon,
        addrcon,
        
        identcon,
        regcon
    }

    enum SEGREG
    {
        NS,         //No Segment Register
        DS,
        BP,
        SI,
        IP,
        SP,

        ES,     //used for:  instance of class
        EX      //used for:  try..catch..finally
    };



    class Operand
    {
        public object value;
        public OPRTYPE ty;

        internal string mod;
        internal string name;

        internal SEGREG SEG;


        public Operand()
        {
            ty = OPRTYPE.none;
            value = null;
        }


        public Operand(Numeric opr)
        {
            ty = OPRTYPE.numcon;
            value = opr;
        }

        public Operand(int opr)
        {
            ty = OPRTYPE.intcon;
            value = opr;
        }

        public string Str
        {
            get
            {
                return (string)value;
            }
        }

        internal int Addr
        {
            get
            {
                return (int)value;
            }
            set
            {
                this.value = value;
            }

        }


        internal int Intcon
        {
            get
            {
                return (int)value;
            }
        }

    
        internal static Operand Ident(string id)
        {
            Operand v = new Operand();
            v.ty = OPRTYPE.identcon;
            v.name = id;
            v.value = id;
            v.SEG = SEGREG.DS;
            return v;
        }

       
        internal static Operand Scope(string id)
        {
            Operand v = new Operand();
            v.ty = OPRTYPE.identcon;
            v.mod = Constant.SCOPE;
            v.name = id;
            v.value = id;
            return v;
        }

     

        internal static Operand REG(SEGREG REG)
        {
            Operand v = new Operand();
            v.ty = OPRTYPE.regcon;
            v.SEG = REG;
            v.value = -1;
            return v;
        }

        internal static Operand REGAddr(SEGREG REG, int offset, string name)
        {
            Operand v = new Operand();
            v.ty = OPRTYPE.addrcon;
            v.SEG = REG;
            v.value = offset;
            v.name = name;

            return v;
        }

        


        internal static Operand Delegate(OPRTYPE ty, int addr, string moduleName)
        {
            Operand v = new Operand();
            v.ty = ty;
            v.value = addr;
            v.mod = moduleName;
            return v;
        }

        internal static Operand Func(int addr, string moduleName)
        {
            Operand v = new Operand();
            v.ty = OPRTYPE.funccon;
            v.value = addr;
            v.mod = moduleName;
            return v;
        }

       

        internal static Operand Func(string func, string moduleName)
        {
            Operand v = new Operand();
            v.ty = OPRTYPE.funccon;
            v.value = func;
            v.mod = moduleName;
            return v;
        }

        internal static Operand Clss(int addr, string moduleName)
        {
            Operand v = new Operand();
            v.ty = OPRTYPE.classcon;
            v.value = addr;
            v.mod = moduleName;
            return v;
        }

        public override string ToString()
        {
            StringWriter o = new StringWriter();
            
            switch (ty)
            {
                case OPRTYPE.none:
                    break;

                case OPRTYPE.numcon:
                    o.Write(((Numeric)value).ToString());
                    break;

                case OPRTYPE.funccon:
                    o.Write("{0}(\"{1}\",{2})", Constant.FUNC_FUNCTION, mod, value);
                    break;
                case OPRTYPE.classcon:
                    o.Write("{0}(\"{1}\",{2})", Constant.FUNC_CLASS, mod, value);
                    break;

                case OPRTYPE.intcon:
                    o.Write("{0}",value);
                    break;

                case OPRTYPE.addrcon:
                    if (SEG == SEGREG.NS)
                    {
                        o.Write("[{0}]", Addr);
                        break;
                    }
                    o.Write('[');
                    switch (SEG)
                    {
                        case SEGREG.DS: o.Write("DS"); break;
                        case SEGREG.BP: o.Write("BP"); break;
                        case SEGREG.SI: o.Write("SI"); break;
                        case SEGREG.ES: o.Write("ES"); break;
                        case SEGREG.EX: o.Write("EX"); break;
                    }
                    o.Write("{0}{1}]", (int)value >= 0 ? "+" : "", (int)value);
                    break;

                case OPRTYPE.identcon:
                    if (mod != null && mod != "")
                    {
                        if (SEG == SEGREG.DS)
                            o.Write("[DS+{0}]", mod + ":" + value);
                        else
                            o.Write("[{0}]", mod + ":" + value);
                    }
                    else
                    {
                        if (SEG == SEGREG.DS)
                            o.Write("[DS+{0}]", value);
                        else
                            o.Write("[{0}]", value);
                    }
                    break;

                case OPRTYPE.regcon:
                    switch (SEG)
                    {
                        case SEGREG.BP: o.Write("BP"); break;
                        case SEGREG.IP: o.Write("IP"); break;
                        case SEGREG.SI: o.Write("SI"); break;
                        case SEGREG.SP: o.Write("SP"); break;
                        case SEGREG.ES: o.Write("ES"); break;
                        case SEGREG.EX: o.Write("EX+"); o.Write(value); break;
                    }
                    break;
            }
            return o.ToString();
        }

    }
}
