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
using System.Text;
using System.IO;

namespace AxCRL.Parser
{
    /// <summary>
    /// Code generator
    /// </summary>
    class CodeGeneration
    {
        int maxSize;

        public Instruction[] IV;
        public int IP;

        private Module module;

        public CodeGeneration(Module module)
        {
            maxSize = module.maxSize;
            IP = module.IP1;        //current codeblock start address
            this.IV = module.CS;
            this.module = module;
        }

        public static Instruction[] NewCS(int size)
        {
            Instruction[] CS = new Instruction[size];
            if (CS == null)
                Error.OnFatal(0);

            return CS;
        }

      
        public int emit(INSTYPE c, Operand n)
        {
            IV[IP] = new Instruction(c, n, module.Position);
            fatal();
            return IP++;
        }

        public int emit(INSTYPE c)
        {
            IV[IP] = new Instruction(c, module.Position);
            fatal();
            return IP++;
        }

        public int emit(INSTYPE c, int n)
        {
            IV[IP] = new Instruction(c, new Operand(n), module.Position);
            fatal();
            return IP++;
        }

        Instruction Code(int i)
        {
            return IV[i];
        }

        public void remit(int IP1, int IP2)
        {
            IV[IP1].operand = new Operand(IP2);
        }

        public void remitvalue(int IP1, object value)
        {
            IV[IP1].operand.value = value;
        }

        public void remit(int IP, INSTYPE cmd)
        {
            IV[IP].cmd = cmd;
        }


        public void Move(int d, int s, int n)
        {
            long c = s - d;
            for (int i = 0; i < n; i++)
            {
                //		switch(IV[s+i].cmd)
                //		{
                //		case Instruction::JMP	:
                //		case Instruction::JZ	:
                //		case Instruction::JNZ	: 
                //					IV[s+i].v.i -= c; 
                //					break;
                //		}
                IV[d + i] = IV[s + i];
                IP++;
            }
        }


        public int Size()
        {
            return maxSize - 1024;
        }

        void fatal()
        {
            if (IP >= maxSize)
                Error.OnFatal(3);
#if DEBUG_PARSER
            Logger.WriteLine(string.Format("{0} \t {1}", IP, IV[IP].ToString()));
#endif

        }


#if DEBUG
        public override string ToString()
        {
            StringWriter o = new StringWriter();
            int line = 1;

            //output current CodeBlock
            for (int i = module.IP1; i < IP; i++)
            {
                while (line <= IV[i].line)
                    o.WriteLine(module.Position.LineCode(line++));

                o.WriteLine("{0:0000} \t {1}", i, IV[i]);
            }

            return o.ToString();
        }
#else

        public override string ToString()
        {
            StringWriter o = new StringWriter();
            for (int i = module.IP1; i < IP; i++)
            {
                o.WriteLine("{0} \t {1}", i, IV[i]);
            }

            return o.ToString();
        }	
#endif
    };	
}
