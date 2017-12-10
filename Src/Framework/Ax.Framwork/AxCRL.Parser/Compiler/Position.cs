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
    /// Represents location in the source code
    /// </summary>
    public sealed class Position
    {
        internal int cur;       // current position
        internal int line;      //current line
        internal int col;       // current column
        internal byte block;     // current code block

        private string moduleName;
        private string codePiece;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="moduleName"></param>
        /// <param name="codePiece">search CodePiece on the Libray if codePiece == null </param>
        internal Position(string moduleName, string codePiece)
        {
            this.cur = 0;
            this.line = 1;
            this.col = 1;
            this.block = 0;

            this.moduleName = moduleName;
            this.codePiece = codePiece;
        }

        private Position()
        {
            this.cur = 0;
            this.line = 0;
            this.col = 0;

            this.moduleName = null;
            this.codePiece = null;
        }

        internal static Position UNKNOWN = new Position();



        /// <summary>
        /// location string
        /// </summary>
        public override string ToString()
        {
            if (this.line != 0 )
                return string.Format("at line:{0} col:{1} mod:{2}", line, col, moduleName);
            else
                return string.Empty;
        }

        /// <summary>
        /// Module name
        /// </summary>
        public string ModuleName
        {
            get
            {
                return this.moduleName;
            }
            internal set
            {
                this.moduleName = value;
            }
        }

        /// <summary>
        /// Source code script
        /// </summary>
        public string CodePiece
        {
            get
            {
                if (codePiece != null)
                    return codePiece;

                Module module = Library.GetModule(moduleName);
                if (module != null)
                {
                    return module.GetCodePiece(block);
                }
                else
                    return string.Empty;
            }
        }

        /// <summary>
        /// current cursor in source code
        /// </summary>
        public int Cursor
        {
            get
            {
                return this.cur;
            }
        }
  
        

#if DEBUG
        private char[] linebuffer = new char[Constant.MAX_SRC_COL];
        private List<string> lines = new List<string>();
#endif


        internal void Move(char ch)
        {
#if DEBUG
            if (ch == '\n')
            {
                string str = string.Format("   {0}\t{1}",line, new string(linebuffer, 0, col-1));
                lines.Add(str);

            #if DEBUG_PARSER
                Logger.WriteLine(str+"\n");
            #endif
                line++;
                col = 1;
            }

            if (col < Constant.MAX_SRC_COL)
            {
                if (ch == '\r' || ch == '\n')
                    linebuffer[col - 1] = ' ';
                else
                    linebuffer[col - 1] = ch;
            }
#else
            if (ch == '\n')
            {
                line++;
                col = 1;
            }
#endif

            col++;
            cur++;

        }

#if DEBUG
        internal string LineCode(int line)
        {
            if (lines.Count == 0)
            {
                int len = col;
                if (len > Constant.MAX_SRC_COL)
                    len = Constant.MAX_SRC_COL;

                return string.Format("   {0}\t{1}", line, new string(linebuffer, 0, len - 1));
            }
            else if (line <= lines.Count)
                return lines[line - 1];
    
            return "";

        }
#endif


    }
}
