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
    class Register
    {
        private int CPU_SP;
        private VAL[] REGS;		//registers
        StackSegment<VAL> SS;	    //Stack segment

        public Register(StackSegment<VAL> SS)
        {
            this.SS = SS;
            REGS = new VAL[Constant.MAX_CPU_REG_NUM];
            CPU_SP = -1;			// register pointer
        }

        public bool Push(VAL v)
        {
            if (IsOverflow())
                throw new TieException("CPU Register overflow");
            
            REGS[++CPU_SP] = v;
            return true;
        }

        public VAL pop()
        {
            if (IsEmpty())
                throw new TieException("CPU Register empty");
            
            return REGS[CPU_SP--];
        }

        
        //indirect addressing
        public VAL Pop()
        {
            if (IsEmpty())
                throw new TieException("CPU Register empty");
            
            return IndirectValue( REGS[CPU_SP--]);
        }

        private VAL IndirectValue(VAL var)
        {
           VAL val;
           try
           {
               if (var.ty == VALTYPE.addrcon)
                   return SS[var.Address];
               else
                   return var;
           }
           catch (Exception)
           {
               val = new VAL();
               throw new TieException("R01 varible is not initialized in STACK");
           }
        }

        public VAL Top()
        {
            return IndirectValue(REGS[CPU_SP]);
        }

        //public VAL top()
        //{
        //    return REGS[CPU_SP];
        //}

        public bool IsEmpty()
        {
            return CPU_SP == -1;
        }

        bool IsOverflow()
        {
            return CPU_SP >= Constant.MAX_CPU_REG_NUM;
        }

        //public void Store()
        //{
        //    VAL R0, R1;
        //    R0 = Pop();
        //    R1 = pop();
        //    if (R1.ty == VALTYPE.addrcon) //Simple Variable
        //        SS[R1.Address] = R0;
        //    else  //Array[i]
        //    {
        //        R1.ty = R0.ty;
        //        R1.value = R0.value;
        //    }
        
        //}

        public override string ToString()
        {
            StringWriter o = new StringWriter();
            o.Write("CPU SP={0}", CPU_SP);
            if (CPU_SP != -1)
            {
                o.Write(" REG= ");
                int i = 0;
                for (i = 0; i < CPU_SP; i++)
                    o.Write("{0},", REGS[i]);
                o.Write("{0}", REGS[i]);
                //o.WriteLine();
            }
            else
                o.Write(" REG=[EMPTY]");
            return o.ToString();
        }

        ////address
        //internal static VAL BPAddr(int BP, Operand operand)
        //{
        //    VAL v = new VAL();
        //    v.ty = VALTYPE.addrcon;
        //    v.value = BP + operand.Addr;
        //    v.name = operand.name;
        //    return v;
        //}
    
    }
}
