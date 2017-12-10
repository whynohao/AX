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

namespace AxCRL.Parser
{

    /// <summary>
    /// Parse CodePiece
    /// </summary>
    class JParser : Expression
    {
        class FWD	//For, While, Do
        {
            public int Break, Continue;
            public FWD()
            {
            }

            public FWD(int b, int c)
            {
                Break = b;
                Continue = c;
            }

            public FWD(FWD i)
            {
                Break = i.Break;
                Continue = i.Continue;
            }
        }

        Stack<FWD> Stk;
        Stack<int> IPstk;

        bool s_func()       //statement
        {
            return s_func(OPRTYPE.none);
        }

        public bool e_func(OPRTYPE type)       //expression 
        {
            return s_func(type);
        }

        bool s_func(OPRTYPE type)
        {
            bool isCFunc = false;    
            int PARA_NUM = 1;

            vtab.NewFunction();
            vtab.NewLevel();

            lex.InSymbol();

            int L0 = 0;
            int L1 = 0;
            int L2 = 0;
            if (type == OPRTYPE.none)
            {
                if (lex.sy == SYMBOL.identsy)	
                {
                    Operand x = Operand.Ident(lex.sym.id);
                    gen.emit(INSTYPE.MOV, x);
                    lex.InSymbol();
                    type = OPRTYPE.funccon;
                    isCFunc = true;
                }
                else
                    error.OnError(46);
            }

            L0 = gen.emit(INSTYPE.MOV, Operand.Delegate(type, gen.IP + 2, module.moduleName));   
            L1 = gen.emit(INSTYPE.JMP);
            L2 = gen.emit(INSTYPE.PROC, Operand.Delegate(type, gen.IP, module.moduleName));   
            

            expect(SYMBOL.LP);


            do
            {
                if (lex.sy == SYMBOL.identsy)
                {
                    int addr = PARA_NUM + 1;             //+1 keep a return address

                    vtab.AddParameter(lex.sym.id, -addr);
                    lex.InSymbol();
                    PARA_NUM++;
                }
                else
                    break;

                if (lex.sy == SYMBOL.COMMA)
                    lex.InSymbol();
            } while (true);

            gen.IV[L2].operand.Addr = PARA_NUM;           


            expect(SYMBOL.RP);
            emit_func1();

            if (type == OPRTYPE.classcon)               
            {
                if (lex.sy == SYMBOL.COLON)
                {
                    gen.emit(INSTYPE.THIS);
                    gen.emit(INSTYPE.MOV, Operand.Ident(Expression.BASE_INSTANCE));
                    gen.emit(INSTYPE.OFS);
                    s_instance();
                    gen.emit(INSTYPE.STO1);
                }
            }

            expect(SYMBOL.LC);		//function body;
            while (s_sent()==1) ;
            expect(SYMBOL.RC);
            //	Var.BackLevel();		// this has been excuted in (case RC of s_sent())

            vtab.BackFunction();

            gen.emit(INSTYPE.ENDP, (int)type);          //used for determining default RETURN statement

            if (type != OPRTYPE.none)
                gen.remit(L1, gen.IP);

            if (isCFunc)
                gen.emit(INSTYPE.STO1);                 


            if (lex.sy == SYMBOL.LP)
            {
                s_funcarg(false, L2);
                gen.remit(L0, INSTYPE.NOP); 
                gen.IV[L0].operand = null;
            }

            return true;
        }




        //Lambdas Body
        public bool e_lambda(string[] args, int argc)
        {
            int PARA_NUM = 1;

            vtab.NewFunction();
            vtab.NewLevel();

            int L0 = 0;
            int L1 = 0;
            int L2 = 0;

            L0 = gen.emit(INSTYPE.MOV, Operand.Func(gen.IP + 2, module.moduleName));    
            L1 = gen.emit(INSTYPE.JMP);
            L2 = gen.emit(INSTYPE.PROC, Operand.Func(gen.IP, module.moduleName));   

            for(int i=0; i< argc; i++)
            {
                int addr = PARA_NUM + 1;             
                vtab.AddParameter(args[i], -addr);
                PARA_NUM++;
            } 

            gen.IV[L2].operand.Addr = PARA_NUM;      

            emit_func1();

            int index = lex.Index();
            int IP = gen.IP;
            lex.InSymbol();

            if (lex.sy == SYMBOL.LC)       
            {                              
                int f = s_sent();          
                if (f == -1)               
                {
                    gen.IP = IP;           
                    lex.InSymbol(index);
                    
                    s_expr1();
                    emit_ret();            
                }
            }
            else
            {
                s_exp1();                   //Expression Lambdas
                emit_ret();                 
            }


 
            vtab.BackFunction();

            gen.emit(INSTYPE.ENDP, (int)OPRTYPE.funccon);          //used for determining default RETURN statement
            gen.remit(L1, gen.IP);

            return true;
        }

        private void emit_func1()
        {
            gen.emit(INSTYPE.PUSH, Operand.REG(SEGREG.BP));
            gen.emit(INSTYPE.PUSH, Operand.REG(SEGREG.SP)); // MOV BP,SP
            gen.emit(INSTYPE.POP, Operand.REG(SEGREG.BP));
        }


        private void emit_ret()
        {
            gen.emit(INSTYPE.PUSH, Operand.REG(SEGREG.BP));
            gen.emit(INSTYPE.POP, Operand.REG(SEGREG.SP));
            gen.emit(INSTYPE.POP, Operand.REG(SEGREG.BP));
            gen.emit(INSTYPE.RET);
        }


        bool s_class()
        {
            lex.InSymbol();
            if (lex.sy == SYMBOL.identsy)
            {
                lex.InSymbol();
                expect(SYMBOL.LC);
                func();
                expect(SYMBOL.RC);
                expect(SYMBOL.SEMI);
            }
            return true;
        }

        public bool e_decl()
        { 
          return s_decl(true);
        }

        private bool s_decl()
        { 
          return s_decl(false);
        }

        private bool s_decl(bool expr)
        {
            int parameter;
            lex.InSymbol();
            parameter = 0;
            do
            {
                if (expect(SYMBOL.identsy))
                {	//IDENT id;
                    //strcpy(id,lex.sym.id);
                    int addr = vtab.AddLocal(lex.sym.id);
                    gen.emit(INSTYPE.MOV, Operand.REGAddr(SEGREG.BP, addr, lex.sym.id)); //LOAD
                    if (lex.sy == SYMBOL.EQUAL)
                    {
                        lex.InSymbol();
                        s_exp1();          
                    }
                    else
                        gen.emit(INSTYPE.MOV, new Operand(Numeric.NULL));  

                    gen.emit(INSTYPE.STO1);
                    parameter++;
                }
                else
                    break;//return false;

                if (lex.sy == SYMBOL.COMMA)
                {
                    lex.InSymbol();
                    continue;
                }
                else
                    break;

            } while (true);
            gen.emit(INSTYPE.SP, new Operand(parameter));	// ADD SP,parameter
            
            if(!expr)
                expect(SYMBOL.SEMI);
            
            return true;
        }

        /**
         * 1: continue
         * 0: end
         * -1: is not statement
         * 
         * */
        int s_sent()
        {
            int L1, L2, L3, L4, L5, L6;
            //int parameter;
            switch (lex.sy)
            {
                case SYMBOL.DIRECTIVE:
                    lex.InSymbol();
                    if (lex.sy == SYMBOL.identsy)
                    {
                        string directive = lex.sym.id;
                        lex.InSymbol();
                        switch (directive)
                        {
                            case "module":                      //#module Drawing.Shape;
                                module.moduleName = lex.sym.id;
                                lex.InSymbol();
                                break;
                            case "scope":
                                s_scope();
                                break;
                            default:
                                error.OnError(61);
                                break;
                        }
                        expect(SYMBOL.SEMI);
                    }
                    else
                        error.OnError(2);

                    return 1;

                case SYMBOL.SEMI:
                    lex.InSymbol();
                    gen.emit(INSTYPE.NOP);
                    return 1;

                case SYMBOL.RC:
                    vtab.BackLevel();
                    return 0;

                case SYMBOL.LC:
                    vtab.NewLevel();
                    if (lex.InSymbol())
                    {
                        L1:
                        int s = s_sent();
                        if (s == 1) 
                            goto L1;
                        else if (s == -1)
                            return -1;
                    }
                    expect(SYMBOL.RC);
                    return 1;

                case SYMBOL.IF:
                    lex.InSymbol();
                    expect(SYMBOL.LP);
                    s_expr1();
                    expect(SYMBOL.RP);
                    L1 = gen.emit(INSTYPE.JZ);
                    s_sent();
                    if (lex.sy == SYMBOL.ELSE)
                    {
                        lex.InSymbol();
                        gen.remit(L1, gen.IP + 1);
                        L2 = gen.emit(INSTYPE.JMP);
                        s_sent();
                        gen.remit(L2, gen.IP);
                    }
                    else gen.remit(L1, gen.IP);
                    return 1;

                case SYMBOL.WHILE:
                    lex.InSymbol();
                    expect(SYMBOL.LP);
                    L1 = gen.IP;			// #continue;
                    s_expr1();
                    L2 = gen.emit(INSTYPE.JZ);	// #break;
                    Stk.Push(new FWD(L2, L1));
                    expect(SYMBOL.RP);
                    s_sent();
                    gen.emit(INSTYPE.JMP, L1);
                    gen.remit(L2, gen.IP);
                    Stk.Pop();
                    return 1;


                case SYMBOL.FOREACH:
                    lex.InSymbol();
                    expect(SYMBOL.LP);
                    if (lex.sy == SYMBOL.VAR)  
                        lex.InSymbol();

                    if (lex.sy != SYMBOL.identsy)
                        error.OnError(2);

                    vtab.NewLevel();
                    L6 = vtab.AddLocal(lex.sym.id);

                    gen.emit(INSTYPE.MOV, Operand.REGAddr(SEGREG.BP, L6 + 1, "i"));
                    gen.emit(INSTYPE.MOV, new Operand(new Numeric(0)));
                    gen.emit(INSTYPE.STO1);

                    vtab.AddLocal(1);
                    gen.emit(INSTYPE.MOV, Operand.REGAddr(SEGREG.BP, L6, lex.sym.id)); 
                    gen.emit(INSTYPE.MOV, new Operand(Numeric.NULL));
                    gen.emit(INSTYPE.STO1);
                    
                    gen.emit(INSTYPE.SP, new Operand(2));        
                    

                    L1 = gen.IP;			                     // #continue;
                    gen.emit(INSTYPE.MOV, Operand.REGAddr(SEGREG.BP, L6, lex.sym.id));   
                    lex.InSymbol();
                    expect(SYMBOL.IN);
                    s_exp();
                    gen.emit(INSTYPE.EACH);
                    L2 = gen.emit(INSTYPE.JZ);	                    // #break;
                    Stk.Push(new FWD(L2, L1));
                    expect(SYMBOL.RP);
                    s_sent();
                    gen.emit(INSTYPE.JMP, L1);
                    gen.remit(L2, gen.IP);
                    Stk.Pop();
                    vtab.BackLevel();
                    return 1;


                case SYMBOL.DO:
                    lex.InSymbol();
                    L1 = gen.emit(INSTYPE.JMP);		// JMP L3
                    L2 = gen.emit(INSTYPE.JMP);		// JMP L4		L2=#break;
                    L3 = gen.IP;
                    Stk.Push(new FWD(L2, L1));		// #continue;
                    s_sent();
                    expect(SYMBOL.WHILE);
                    expect(SYMBOL.LP);
                    s_expr1();
                    gen.emit(INSTYPE.JNZ, L3);
                    L4 = gen.IP;
                    gen.remit(L1, L3);
                    gen.remit(L2, L4);
                    expect(SYMBOL.RP);
                    expect(SYMBOL.SEMI);
                    Stk.Pop();
                    return 1;


                case SYMBOL.FOR:
                    lex.InSymbol();
                    expect(SYMBOL.LP);
                    s_expr();
                    L6 = gen.emit(INSTYPE.JMP);		    //JMP L3
                    expect(SYMBOL.SEMI);

                    L1 = gen.IP;
                    s_expr1();
                    gen.emit(INSTYPE.JNZ);				//JNZ L3
                    L5 = gen.emit(INSTYPE.JMP);		    //JMP L4   L5=#break;
                    expect(SYMBOL.SEMI);

                    L2 = gen.IP;						// #continue
                    Stk.Push(new FWD(L5, L2));
                    s_expr();
                    gen.emit(INSTYPE.JMP, L1);
                    expect(SYMBOL.RP);

                    L3 = gen.IP;
                    s_sent();
                    gen.emit(INSTYPE.JMP, L2);
                    L4 = gen.IP;						// #break;

                    gen.remit(L5 - 1, L3);
                    gen.remit(L5, L4);
                    gen.remit(L6, L3);
                    Stk.Pop();
                    return 1;

                case SYMBOL.DEBUG:
                    lex.InSymbol();
                    gen.emit(INSTYPE.DDT);
                    expect(SYMBOL.SEMI);
                    return 1;

                case SYMBOL.BREAK:
                    lex.InSymbol();
                    expect(SYMBOL.SEMI);
                    if (Stk.Count != 0)
                        gen.emit(INSTYPE.JMP, (Stk.Peek()).Break);
                    else
                        error.OnError(18);
                    return 1;

                case SYMBOL.CONTINUE:
                    lex.InSymbol();
                    expect(SYMBOL.SEMI);
                    if (Stk.Count != 0)
                    {
                        if ((Stk.Peek()).Continue != 0)
                            gen.emit(INSTYPE.JMP, (Stk.Peek()).Continue);
                        else
                            error.OnError(20);
                    }
                    else
                        error.OnError(19);
                    return 1;


                case SYMBOL.SWITCH:
                    lex.InSymbol();
                    expect(SYMBOL.LP);
                    L4 = gen.emit(INSTYPE.JMP);	//JMP CAS
                    L1 = gen.emit(INSTYPE.JMP);	//JMP END; #break
                    Stk.Push(new FWD(L1, 0));			// no continue point

                    IPstk.Push(IPstk.Peek());
                    gen.IP = Replace(IPstk, gen.IP);
                    s_exp1();
                    gen.IP = Replace(IPstk, gen.IP);

                    expect(SYMBOL.RP);
                    s_case();
                    L3 = gen.emit(INSTYPE.JMP);	// JMP END;

                    //switch judge
                    gen.remit(L4, gen.IP);
                    L2 = IPstk.Pop();
                    gen.Move(gen.IP, IPstk.Peek(), L2 - IPstk.Peek());
                    gen.remit(L1, gen.IP);					// END
                    gen.remit(L3, gen.IP);

                    Stk.Pop();						// used to break, continue;
                    return 1;

                case SYMBOL.RETURN:
                    lex.InSymbol();
                    if (lex.sy == SYMBOL.SEMI)
                        gen.emit(INSTYPE.MOV, new Operand(Numeric.VOID));	// return void
                    else
                    {
                        s_exp1();
                        expect(SYMBOL.SEMI);
                    }
                    emit_ret();
                    return 1;

                case SYMBOL.VAR:
                    s_decl();
                    return 1;

                case SYMBOL.FUNC:
                    return s_func()? 1 : 0;

                case SYMBOL.THROW:
                    lex.InSymbol();
                    s_exp1();
                    gen.emit(INSTYPE.THRW); 
                    return 1;

                case SYMBOL.TRY:
                    //TRY
                    L1 = gen.emit(INSTYPE.PUSH, Operand.REG(SEGREG.EX)); 
                    lex.InSymbol();
                    if(lex.sy==SYMBOL.LC)                
                        s_sent();
                    else
                        error.OnError(lex.sy);

                    gen.emit(INSTYPE.POP, Operand.REG(SEGREG.EX));       
                    L2 = gen.emit(INSTYPE.JMP);         
                
                    //CATCH
                    L3 = gen.IP;
                    while (lex.sy == SYMBOL.CATCH)      
                    {
                        e_func(OPRTYPE.funccon);
                    }

                    if (L3 == gen.IP)                 
                    { 
                    }
                    else
                    {
                        L4 = (int)gen.IV[L3].operand.value; 
                        gen.remitvalue(L1, gen.IP);      
                        gen.emit(INSTYPE.POP, Operand.REG(SEGREG.EX));
                        s_call(L4, 1);                  
                    }

                    //FINALLY
                    gen.remit(L2, gen.IP);
                    if (lex.sy == SYMBOL.FINALLY)
                    {
                        lex.InSymbol();
                        if (lex.sy == SYMBOL.LC)
                            s_sent();
                        else
                            error.OnError(lex.sy);
                    }
                    return 1;

                case SYMBOL.NOP:
                    return 0;

                default:
                    if (s_expr())
                    {
                        if (lex.sy == SYMBOL.SEMI)
                        {
                            lex.InSymbol();

                            return 1;
                        }

                        return -1;
                    }
                    else
                        return -1;

            }
            
        }





        //---------------------------------------------------------------------------------------------


        bool func()
        {
            switch (lex.sy)
            {
                case SYMBOL.FUNC: return s_func();
                case SYMBOL.CLASS: return s_class();
                case SYMBOL.VAR: return s_decl();
            }
            return false;
        }

        //-----------------------------------------------------------------------------
        bool s_case()
        {
            bool i = false;
            expect(SYMBOL.LC);

            do
            {
                if (lex.sy == SYMBOL.CASE)
                {
                    i = true;
                    lex.InSymbol();

                    gen.IP = Replace(IPstk, gen.IP);
                    s_exp1();
                    gen.emit(INSTYPE.CAS, IPstk.Peek());
                    gen.IP = Replace(IPstk, gen.IP);

                    expect(SYMBOL.COLON);
                    while (!(
                        lex.sy == SYMBOL.CASE
                        || lex.sy == SYMBOL.DEFAULT
                        || lex.sy == SYMBOL.RC)
                        ) s_sent();
                }
                else break;
            } while (true);

            if (!i)
            {
                error.OnWarning(0);
                gen.emit(INSTYPE.RMT);
                return false;
            }

            if (lex.sy == SYMBOL.DEFAULT)
            {
                lex.InSymbol();
                expect(SYMBOL.COLON);
                gen.IP = Replace(IPstk, gen.IP);
                gen.emit(INSTYPE.JMP, IPstk.Peek());
                gen.IP = Replace(IPstk, gen.IP);
                while (lex.sy != SYMBOL.RC) s_sent();

            }

            expect(SYMBOL.RC);
            return true;
        }



        int Replace(Stack<int> stack, int New)
        {
            int old = stack.Pop();
            stack.Push(New);
            New = old;
            return old;
        }

        public override String ToString()
        {
            return gen.ToString();
        }




        public bool Program(string functionEntry)
        {
            int cas = gen.Size();		// used to switch statement
            IPstk.Push(cas);

            gen.emit(INSTYPE.PUSH, Operand.REG(SEGREG.IP)); //func return address[BP-1]
            gen.emit(INSTYPE.CALL, Operand.Func(functionEntry, module.moduleName));

            gen.emit(INSTYPE.SP, new Operand(-1));
            gen.emit(INSTYPE.HALT);
            gen.IP = 10;

            while (func()) ;
            int addr = vtab.FuncAddr(functionEntry);
            if (addr == -1)
                error.OnError(53);
            else
                gen.remit(1, addr);
            return true;
        }


        public void Close()
        {
            lex.Close();
        }



        bool nonblank = true;
        public JParser(string scope, string sourceCode, CodeSource format, Module module)
            : base(sourceCode, format, module)
        {

#if DEBUG_PARSER
            Logger.WriteLine(System.DateTime.Now.ToString() + "------------------------------Parsing--------------------------------------");
#endif

            Stk = new Stack<FWD>();
            IPstk = new Stack<int>();
            nonblank = lex.InSymbol();

            if (!nonblank)          //source code is empty string or all are comment
            {
                gen.emit(INSTYPE.HALT);
                return;
            }

            if (scope != "")
                gen.emit(INSTYPE.DIRC, Operand.Scope(scope));
        }

        public bool IsBlank
        {
            get { return !nonblank; }
        }


        private bool s_statements()
        {
            emit_func1();
            //gen.emit(INSTYPE.PUSH, NewVAL.REG(SEGREG.BP));
            //gen.emit(INSTYPE.PUSH, NewVAL.REG(SEGREG.SP)); // MOV BP,SP
            //gen.emit(INSTYPE.POP, NewVAL.REG(SEGREG.BP));

            L1:
            int f = s_sent();
            if (f == 1) goto L1;
            
            return f != -1;
        }
        
        
    
        public int Compile(CodeType ty)
        {
            IPstk.Push(gen.Size()); // used to switch statement

            switch (ty)
            {
                case CodeType.expression:
                    s_expr1();
                    break;
                
                case CodeType.statements:
                    if (!s_statements())
                        error.OnError(51);
                    break;
                
                case CodeType.auto:
                    int IP = gen.IP;
                    if (!s_statements())
                    {
                        gen.IP = IP;
                        lex.InSymbol(1);
                        s_expr1();
                    }
                    
                    break;
            }

            int halt = gen.emit(INSTYPE.HALT);

#if DEBUG
            Logger.WriteLine(System.DateTime.Now.ToString() + "------------------------------Instruction--------------------------------------");
            Logger.WriteLine("Module=" + module.moduleName);
            Logger.WriteLine(this.ToString());
#endif
            return halt;
        }

    




    }
}

