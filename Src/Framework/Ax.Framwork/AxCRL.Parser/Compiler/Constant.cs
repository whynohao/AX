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
    /// TIE configuration parameters
    /// </summary>
    public static class Constant
    {
       internal const int  NKW = 39;                        // no. of key words 
       internal const int ALNG = 64;                        // no. of significant chars in identifiers 

       internal const int EMAX = 322;                       // max exponent of real numbers 
       internal const int EMIN = -292;                      // min exponent 

       internal const int KMAX = 15;                        // max no. of significant digits 
       internal const int NMAX = int.MaxValue;              // 2^32-1 

       internal const byte MAX_CODEBLOCK_NUM = 16;           // max CODE Block#

        /// <summary>
        /// Default maximum length of string variable
        /// </summary>
       public static int MAX_STRING_SIZE = 16000;           // size of string-table 

        /// <summary>
        /// Default maximum #columns of source code
        /// </summary>
       public static int MAX_SRC_COL = 512;                 // max SOURCE CODE width
        
        /// <summary>
        /// Default maximum #lines of source code
        /// </summary>
       public static int MAX_SRC_LINE = 1024;               // max SOURCE CODE width

        /// <summary>
        /// Default maximum code segment size
        /// </summary>
       public static int MAX_INSTRUCTION_NUM = 1024 * 16;   // max CODE segment
        


       /// <summary>
       /// Maximum symbol table size, used by compiler
       /// </summary>
       public static int MAX_SYMBOL_TABLE_SIZE = 2024;		// max size of sysmbol table

       internal const string VOLATILE_MODULE_NAME = "volatile"; //temp module
       internal const string DEFAULT_MODULE_NAME = "unknown"; // default module in Library

//--------------------- VM --------------------------------------------------------------------
        
        /// <summary>
        /// Maximum stack size in virtual machine, used by local variable, recursive function calls and etc.
        /// </summary>
       public static int MAX_STACK = 1024 * 16;			    // max STACK segment

        /// <summary>
        /// Maximum Extra size in virtual machine, used Tie class
        /// </summary>
       public static int MAX_EXTRA = 1024;        	        // max Extra segment

        /// <summary>
        /// Maximum #nested exceptions in try...catch...
        /// </summary>
       public static byte MAX_EXCEPTION = 16;       	        // max nested Exception

        /// <summary>
        /// Maximum #registers in CPU
        /// </summary>
       public static int MAX_CPU_REG_NUM = 256;		        // max register number of CPU


       
       internal const string FUNC_MAKE_ARRAY_TYPE = "$makearraytype";
       internal const string FUNC_FUNCTION = "$function";
       internal const string FUNC_CLASS = "$class";
       internal const string FUNC_CAST_VALUE_TYPE = "$castvt";
       internal const string FUNC_CAST_TYPE_VALUE = "$casttv";
       internal const string FUNC_IS_TYPE = "$istype";

       internal const string FUNC_CON_INSTANCE_INVOKE = "FuncconInstanceInvoke";
       internal const string SCOPE = "scope";
        
    }
    

}
