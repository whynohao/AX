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
using System.Runtime.Serialization;

namespace AxCRL.Parser
{
    enum CodeMode
    { 
        Overwritten,    //instruction is overwritten
        Append
    }

    class CodeBlock
    {
        public readonly int IP; //start IP address in code block
        public readonly string CodePiece;

        public CodeBlock(int IP, string codePiece)
        {
            this.IP = IP;
            this.CodePiece = codePiece;
        }
    }

    class Module 
    {
        List<CodeBlock> blocks;       //keep all code blocks
        
        public string moduleName;
        public readonly int maxSize;
        public Instruction[] CS;

        public int IP1;      //start address
        private int IP2;     //stop address

        private Position pos;
        private Error error;

        public Module()
            : this(Constant.VOLATILE_MODULE_NAME, Constant.MAX_INSTRUCTION_NUM)
        {
        }



        public Module(string moduleName, int moduleSize)
        {
         
            maxSize = moduleSize + 1024;
            blocks = new List<CodeBlock>();
            

            this.CS = CodeGeneration.NewCS(maxSize);

            this.IP1 = 0;
            this.IP2 = 0;

            this.moduleName = moduleName;
        }

        public int FreeSpace
        {
            get { return maxSize - IP2 - 1024; }
        }

        public Position Position
        {
            get
            {
                return pos;
            }
        }

        public Error Error
        {
            get
            {
                return this.error;
            }
        }

       

        public string GetCodePiece(int index)
        {
            if (index < 0 || index > blocks.Count)
                throw new CompilingException("invalid CodeBlock index", Position.UNKNOWN); 

            return blocks[index].CodePiece;
        }

        public string SourceCode
        {
            get
            {
                StringBuilder builder = new StringBuilder();
                foreach(CodeBlock block in blocks)
                    builder.Append(block.CodePiece);

                return builder.ToString();
            }
        }


        public bool CompileCodeBlock(string scope, string codePiece, CodeType ty, CodeMode codeMode)
        {

            //SourceCode line# may be messy if !overwritten, becuase codePiece merged, 
            this.pos = new Position(moduleName, codePiece);
            this.error = new Error(pos);

            if (blocks.Count == 0)    
            {
                codeMode = CodeMode.Overwritten;
            }
            

            if (codeMode == CodeMode.Append)
            {
                if (blocks.Count + 1 > Constant.MAX_CODEBLOCK_NUM)
                    throw new CompilingException("CodeBlock number reaches maximum limitation.", Position.UNKNOWN); 

                IP1 = IP2;
            }

            this.pos.block = (byte)blocks.Count;
            blocks.Add(new CodeBlock(IP1, codePiece));

            JParser parser = new JParser(scope, codePiece, CodeSource.STRING, this);
            if (!parser.IsBlank)
            {
                IP2 = parser.Compile(ty) + 1;
            }

            pos.ModuleName = moduleName;      
            

            return !parser.IsBlank;
        }

      
        public static Module decode(VAL val)
        {
            string moduleName = val["name"].Str;
            string codePiece = val["code"].Str;
            int size = val["size"].Intcon;
            
            Module module = new Module(moduleName, size);
            module.CompileCodeBlock("", codePiece, CodeType.statements, CodeMode.Overwritten);
            return module;
        }

        public static VAL encode(Module module)
        {
            VAL val = new VAL();
            val["name"] = new VAL(module.moduleName);
            val["code"] = new VAL(module.SourceCode);
            val["size"] = new VAL(module.IP2);
            return val;
        }

        public override string ToString()
        {
            return moduleName;
        }


        public static string[] ParseScope(string scope)
        {
            Error error = new Error(new Position("$scope", scope));
            List<string> L = new List<string>();
            JLex lex = new StringLex(scope, error);
            lex.InSymbol();
            if (lex.sy == SYMBOL.identsy)
            {
                L.Add(lex.sym.id);
                lex.InSymbol();
                while (lex.sy == SYMBOL.STRUCTOP)
                {
                    SYMBOL2 Opr = lex.opr;
                    if (!lex.InSymbol())    
                    {
                        error.OnError(7);
                        break;
                    }
                    L.Add(lex.sym.id);
                    lex.InSymbol();
                }
            }

            return L.ToArray();
        }

    }
}
