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
    enum NUMTYPE
    {
        voidcon,
        nullcon,
        boolcon,
        intcon,
        doublecon,
        // decimalcon,
        stringcon
    }

    /// <summary>
    /// Numeric, logical, lexical operands
    /// </summary>
    class Numeric
    {
        public NUMTYPE ty;
        public object value;

        private Numeric()
        {
            ty = NUMTYPE.voidcon;
            value = null;
        }

        public static Numeric VOID
        {
            get
            {
                Numeric c = new Numeric();
                c.ty = NUMTYPE.voidcon;
                c.value = null; 
                return c;
            }
        }


        public static Numeric NULL
        {
            get
            {
                Numeric c = new Numeric();
                c.ty = NUMTYPE.nullcon;
                c.value = null; 
                return c;
            }
        }

        public Numeric(int i)
        {
            ty = NUMTYPE.intcon;
            value = i;
        }

        public Numeric(string str)
        {
            ty = NUMTYPE.stringcon;
            value = str;
        }

        public Numeric(SYMBOL sy, Sym sym)
        {
            switch (sy)
            {
                case SYMBOL.intcon:
                    ty = NUMTYPE.intcon;
                    value = sym.inum;  
                    break;

                case SYMBOL.floatcon:
                    ty = NUMTYPE.doublecon;
                    value = sym.fnum; 
                    break;

                case SYMBOL.stringcon:
                    ty = NUMTYPE.stringcon;
                    value = sym.stab;  
                    break;
                
                case SYMBOL.nullsy:
                    ty = NUMTYPE.nullcon;
                    value = null;
                    break;

                case SYMBOL.VOID:
                    ty = NUMTYPE.voidcon;
                    value = null;
                    break;

                case SYMBOL.truesy:
                    ty = NUMTYPE.boolcon;
                    value = true; 
                    break;
                
                case SYMBOL.falsesy:
                    ty = NUMTYPE.boolcon;
                    value = false; 
                    break;
            }
        }

        public override string ToString()
        {
            StringWriter o = new StringWriter();
            string s;

            switch (ty)
            {
                case NUMTYPE.voidcon:
                    break;

                case NUMTYPE.nullcon:
                    o.Write("null");
                    break;

                case NUMTYPE.intcon:
                    o.Write(value);
                    break;

                case NUMTYPE.doublecon:
                    o.Write(value);
                    break;

                case NUMTYPE.boolcon:
                    o.Write("{0}", (bool)value ? "true" : "false");
                    break;

                case NUMTYPE.stringcon:
                    if (value is char)
                    {
                        o.Write("'{0}'", value);
                        break;
                    }
                    if (ty == NUMTYPE.stringcon)
                        o.Write("\"");

                    s = (string)value;
                    for (int i = 0; i < s.Length; i++)
                    {
                        switch (s[i])
                        {
                            case '"':
                                o.Write("\\\"");
                                break;

                            case '\\':
                                o.Write("\\\\");
                                break;

                            case '\n':
                                o.Write("\\n");
                                break;

                            case '\t':
                                o.Write("\\t");
                                break;

                            default:
                                o.Write(s[i]);
                                break;
                        }

                    }

                    if (ty == NUMTYPE.stringcon)
                        o.Write("\"");
                    break;

            }

            return o.ToString();
        }
    }
}
