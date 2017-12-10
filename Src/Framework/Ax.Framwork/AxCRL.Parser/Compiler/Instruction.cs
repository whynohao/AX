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
using System.Runtime.Serialization;

namespace AxCRL.Parser
{
    
    enum INSTYPE
    {
		NEG, ADD, SUB, MUL, DIV, MOD,
	    INC, DEC,
	
	    EQL, NEQ, LSS, LEQ, GTR, GEQ,
	
	    NOTNOT, ANDAND, OROR, NOT, AND, OR, XOR, 
        EACH, //foreach(a in A)

	    SHR, SHL,	// >> , <<
	
	    JMP, JNZ,JZ, LJMP, LJZ,
	    CAS,	//case of switch 

	    PUSH, POP, SP,     //SS 
        RMT, RCP,          //remove CPU top register, register copy
        ESI, ESO,          //EX PUSH/POP
	
	    MOV,STO,STO1,	//LOAD
        //REGI,REGO,      // REGI = REG.Push(), REGO = REG.Pop()

	    CALL,RET,	// call function
	    MARK,END,	// List, Parameter,
	    OFS,ARR,	// struct. array,

	    HALT,NOP,
        THIS, BASE, NS,  //this, base class, namespace, module
        ADR, VLU,            //&var 返回变量的地址, *VL, 返回地址的值 

	    PROC,ENDP,	//function
        DIRC,       //directive
	    DDT,	//debug
        GNRC,    //generic

//class	
        NEW,
	    CLSS,	//class
	    PBLC,	//public
	    PRVT,	//private
	    PRTC,	//protected
	    ENDC,	//end of class


        THRW   //throw

	};


    class Instruction  
    {
        public INSTYPE cmd;
        public Operand operand;

        public readonly int line = 0;
        public readonly int col = 0;
        public readonly int cur = 0;
        public readonly byte block = 0;

        public Instruction(INSTYPE cmd, Position pos)
            : this(cmd, null, pos)
        { 
        
        }


        public Instruction(INSTYPE cmd, Operand opr, Position pos) 
        { 
            this.cmd = cmd;
            this.operand = opr;

            this.line = pos.line;
            this.col = pos.col;
            this.cur = pos.cur;
            this.block = pos.block;

        }

        public override String ToString()
        {
            String o;
            switch (cmd)
            {
                case INSTYPE.NEG: o = "NEG "; break;
                case INSTYPE.ADD: o = "ADD "; break;
                case INSTYPE.SUB: o = "SUB "; break;
                case INSTYPE.MUL: o = "MUL "; break;
                case INSTYPE.DIV: o = "DIV "; break;
                case INSTYPE.MOD: o = "MOD "; break;

                case INSTYPE.INC: o = "INC "; break;
                case INSTYPE.DEC: o = "DEC "; break;

                case INSTYPE.EQL: o = "EQL "; break;
                case INSTYPE.NEQ: o = "NEQ "; break;
                case INSTYPE.LSS: o = "LSS "; break;
                case INSTYPE.LEQ: o = "LEQ "; break;
                case INSTYPE.GTR: o = "GTR "; break;
                case INSTYPE.GEQ: o = "GEQ "; break;

                case INSTYPE.NOT: o = "NOT "; break;
                case INSTYPE.AND: o = "AND "; break;
                case INSTYPE.OR:  o = "OR  "; break;

                case INSTYPE.NOTNOT: o = "NNOT "; break;
                case INSTYPE.ANDAND: o = "AAND "; break;
                case INSTYPE.OROR:   o = "OOR  "; break;

                case INSTYPE.EACH:  o = "EACH"; break;

                case INSTYPE.XOR: o = "XOR "; break;
                case INSTYPE.SHR: o = "SHR "; break;
                case INSTYPE.SHL: o = "SHL "; break;

                case INSTYPE.JMP: o = "JMP "; break;
                case INSTYPE.JNZ: o = "JNZ "; break;
                case INSTYPE.JZ:  o = "JZ  "; break;

                case INSTYPE.CAS: o = "CAS "; break;
                case INSTYPE.LJMP: o = "LJMP"; break;
                case INSTYPE.LJZ: o = "LJZ "; break;

                case INSTYPE.PUSH: o = "PUSH"; break;
                case INSTYPE.POP: o = "POP "; break;
                case INSTYPE.SP:  o = "SP  "; break;
                case INSTYPE.RMT: o = "RMT "; break;
                case INSTYPE.RCP: o = "RCP "; break;
                case INSTYPE.ESI: o = "ESI"; break;
                case INSTYPE.ESO: o = "ESO "; break;

                case INSTYPE.MOV: o = "MOV "; break;
                case INSTYPE.STO: o = "STO "; break;
                case INSTYPE.STO1: o = "STO1"; break;

                case INSTYPE.CALL: o = "CALL"; break;
                case INSTYPE.RET: o = "RET "; break;
                case INSTYPE.MARK: o = "MARK"; break;
                case INSTYPE.END: o = "END "; break;

                case INSTYPE.OFS: o = "OFS "; break;
                case INSTYPE.ARR: o = "ARR "; break;

                case INSTYPE.NOP:  o = "NOP "; break;
                case INSTYPE.HALT: o = "HALT"; break;

                case INSTYPE.DDT:  o = "DDT "; break;
                case INSTYPE.PROC: o = "PROC"; break;
                case INSTYPE.ENDP:
                    if ((OPRTYPE)operand.Addr == OPRTYPE.classcon) 
                        o = "ENDC";
                    else
                        o = "ENDP"; 
                    break;

                case INSTYPE.DIRC: o = "DIRC"; break;

                case INSTYPE.ADR: o = "ADR "; break;
                case INSTYPE.VLU:  o = "VLU "; break;

                //class define
                case INSTYPE.NEW:  o = "NEW"; break;
                case INSTYPE.CLSS: o = "CLSS"; break;
                case INSTYPE.NS: o = "NS"; break;

                case INSTYPE.PBLC: o = "PBLC"; break;
                case INSTYPE.PRVT: o = "PRVT"; break;
                case INSTYPE.PRTC: o = "PRTC"; break;
                case INSTYPE.ENDC: o = "ENDC"; break;

                case INSTYPE.THIS: o = "this"; break;
                case INSTYPE.BASE: o = "base"; break;

                case INSTYPE.THRW: o = "THRW"; break;
                case INSTYPE.GNRC: o = "GNRC"; break;

                default: o = "#INS#" + cmd; break;
            }
            
            if(cmd != INSTYPE.ENDP &&  operand != null)
                o = o + " "+'\t' + operand.ToString();
#if DEBUG
            o = string.Format("({0,3}:{1,3}|{2,2})\t{3}", line, col, block, o);
#endif
            return o;
        }	
	 }

   

}
