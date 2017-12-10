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

    class Error
    {
        #region Message Defintion

        private static readonly string [] msgFatal = new string[]
	            {
	            "F00 memory overflow",
	            "F01 ",
	            "F02 ",
	            "F03 Code segment overfolow",
	            "F04 ",
	            "F05 ",
	            "F06 ",
	            "F07 ",
	            "F08 source file don't exist"
                };

        private static readonly string[] msgWarning = new string[]
	        {
	            "W00 switch statement contains 'default' but no 'case' labels",
	            "W01 undeclared identifier",
	            "W02 duplicated name"
	        };

        private static readonly string[] msgError = new string[]
            {
            "C00 undefine identifier",			
            "C01 identifer multiple define",
            "C02 identifier expected",
            "C03 program expected",
            "C04 ) expected",							
            "C05 : expected",							
            "C06 syntax error",		
            "C07 ident,var expected",
            "C08 { expected",							
            "C09 ( expected",							

            "C10 id,array,struct expected",
            "C11 [ expected",							
            "C12 ] expected",							
            "C13 } expected",							
            "C14 ; expected",							
            "C15 case expected",							
            "C16 = expected",
            "C17 while expected",							
            "C18 break isn't in cycle or switch body",	
            "C19 continue isn't in cycle body",			
  
            "C20 illeagl continue",						
            "C21 number too big",						
            "C22 . expected",
            "C23 type (case)",
            "C24 'illeagl character",					
            "C25 const id=3.14",
            "C26 index type AT array",
            "C27 index bound",
            "C28 no array",
            "C29 type id",
            "C30 undefine type",
            "C31 no record",
            "C32 boolean type",
            "C33 arithmatic type",
            "C34 integer AT div,mod",
            "C35 type must be same AT compare",
            "C36 parameter type must be same in call function,procedure",
            "C37 variable expected",
            "C38 string be made of a char at least",
            "C39 real unequal unreal parameters",
            "C40 float number, .0 expected",					
            "C41 type error AT read,write",
            "C42 expression must be real type ",
            "C43 field width must be integer AT write",
            "C44 expression can''t have proc,type identifer",
            "C45 var,proc,func IDENT expected",
            "C46 undefine function name",	
            "C47 ",
            "C48 standard function paramter''s type is error",
            "C49 store overflow",
            "C50 constant expected",
            "C51 syntax error in statement",
            "C52 return expected",
            "C53 function entry does not defined",
            "C54 > expected",
            "C55 file is not existed",
            "C56 => expected",
            "C57 ",
            "C58 factor BEFORE ident,const,not or (",
            "C59 string is too long",		
            "C60 comment lack of */",
            "C61 directive is not defined",
            "C62 keyword 'in' expected",
        };


        #endregion

        private Position pos;

        public Error(Position pos)
        {
            this.pos = pos;
        }


        public Position Position
        {
            get
            {
                return this.pos;
            }
        }

        #region Compiling 

        public static void OnFatal(int i)
        {
            throw new TieException("Fatal: " + msgFatal[i]);
        }

        public void OnWarning(int i)
        {
            Logger.WriteLine(
                string.Format("Warning :{0} {1}", msgWarning[i], pos.ToString()));

        }

        public void OnError(int i)
        {
            throw new CompilingException(msgError[i], pos);
            //C:\Jiang\tie\test.cpp(14) : error C2039: 'In' : is not a member of 'ScanToken'

        }

        public void OnError(SYMBOL sy)
        {
            switch (sy)
            {
                case SYMBOL.CASE: OnError(15); break;
                case SYMBOL.WHILE: OnError(17); break;
                case SYMBOL.RETURN: OnError(52); break;

                //---------------------------------------------------------------------
                case SYMBOL.SEMI: OnError(14); break;
                case SYMBOL.COLON: OnError(5); break;
                case SYMBOL.LP: OnError(9); break;
                case SYMBOL.RP: OnError(4); break;
                case SYMBOL.RB: OnError(12); break;
                case SYMBOL.LC: OnError(8); break;
                case SYMBOL.RC: OnError(13); break;
                case SYMBOL.IN: OnError(62); break;
                case SYMBOL.identsy: OnError(2); break;
                case SYMBOL.GOESTO: OnError(56); break;

                default:
                    throw new CompilingException(string.Format("Error: unknown symbol:{0} expected", sy), pos);
            }
        }

        #endregion

        public CompilingException CompilingException(string message)
        {
            return new CompilingException("Symbol Table overflow.", pos);
        }

    
    }
}
