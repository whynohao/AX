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
    class Export
    {
        public static string ToXml(VAL val, string tag)
        {
            return ToXML(val, tag, 0);
        }


        private static string ToXML(VAL val, string tag, int tab)
        {
            StringWriter o = new StringWriter();
            if (val.IsAssociativeArray())
            {
                o.Write(Indent(tab)); o.WriteLine("<" + tag + ">");
                for (int i = 0; i < val.Size; i++)
                {
                    VAL v = val[i];
                    o.Write(ToXML(v[1], v[0].Str, tab + 1));
                }
                o.Write(Indent(tab)); o.WriteLine("</" + tag + ">");
            }
            else if (val.ty == VALTYPE.listcon)
            {
                for (int j = 0; j < val.Size; j++)
                {
                    VAL v = val[j];
                    o.Write(ToXML(v, tag, tab + 1));
                }
            }
            else
            {
                o.Write(Indent(tab)); o.Write("<" + tag + ">"); 
                o.Write(XmlString(val.ToString2())); 
                o.WriteLine("</" + tag + ">");
            }
            return o.ToString();

        }





        public static string ToJson(VAL val, string tag, bool quotationMark)
        {
            if(tag==null || tag=="")
                return ToJson(val, "", 0, quotationMark);
            else
                return "{" + ToJson(val, tag, 0, quotationMark) + "}";
        }


        private static string ToJson(VAL val,string tag, int tab, bool quotationMark)
        {
        
            StringWriter o = new StringWriter();
            
            o.Write(Indent(tab));
            if (tag != "")
            {
                if(quotationMark)
                    o.Write("\"" + tag + "\""); 
                else
                    o.Write(tag); 
                o.Write(" : ");
            }
            
            if (val.IsAssociativeArray())
            {
                o.WriteLine("{");
                for (int i = 0; i < val.Size; i++)
                {
                    VAL v = val[i];
                    o.Write(ToJson(v[1], v[0].Str, tab + 1, quotationMark));

                    if (i < val.Size - 1)
                         o.WriteLine(",");
                    else
                         o.WriteLine();
                }
                o.Write(Indent(tab)); o.Write("}");
                if (!quotationMark && val.Class != null)
                    o.Write(".typeof(\"{0}\")", val.Class);
            }
            else if (val.ty == VALTYPE.listcon)
            {
                o.WriteLine("[");
                for (int j = 0; j < val.Size; j++)
                {
                    VAL a = val[j];
                    o.Write(ToJson(a, "", tab + 1, quotationMark));
                    
                    if (j < val.Size - 1)
                        o.WriteLine(",");
                    else
                        o.WriteLine();
                }
                o.Write(Indent(tab)); o.Write("]");
                if (!quotationMark && val.Class != null)
                    o.Write(".typeof(\"{0}\")", val.Class);
            }
            else if (val.ty == VALTYPE.hostcon)
            {
                val = HostValization.Host2Valor(val.value);
                if (val.ty == VALTYPE.listcon)
                    o.Write(ToJson(val, "", tab, quotationMark));
                else
                    o.Write(val.Valor);
            }
            else
            {
                o.Write(val.Valor);
            }
            
            return o.ToString();

        }
        
        private static string Indent(int n)
        {
            string tab = "";
            for (int k = 0; k < n; k++)
                tab += "  ";

            return tab;
        }


        private static string XmlString(string s)
        {
            StringWriter o = new StringWriter();
            for (int i = 0; i < s.Length; i++)
            {
                switch (s[i])
                {
                    case '"':
                        o.Write("&quot;");
                        break;

                    case '\'':
                        o.Write("&apos;");
                        break;

                    case '\\':
                        o.Write("\\\\");
                        break;

                    case ' ':
                        o.Write("&nbsp;");
                        break;

                    case '\t':
                        o.Write("\\t");
                        break;

                    case '&':
                        o.Write("&amp;");
                        break;

                    case '<':
                        o.Write("&lt;");
                        break;

                    case '>':
                        o.Write("&gt;");
                        break;

                    default:
                        o.Write(s[i]);
                        break;
                }

            }
            
            return o.ToString();
        }
    }
}
