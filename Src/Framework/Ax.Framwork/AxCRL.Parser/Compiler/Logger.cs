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
    /// <summary>
    /// Log target device
    /// </summary>
    public enum LOGTARGET
    {
        /// <summary>
        /// null device
        /// </summary>
        NullWriter,

        /// <summary>
        /// default console
        /// </summary>
        Console,

        /// <summary>
        /// memory string writer
        /// </summary>
        StringWriter,

        /// <summary>
        /// file device
        /// </summary>
        File
    }

    /// <summary>
    /// System log class
    /// </summary>
    public class Logger
    {
        private static Logger logger = null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public delegate void WriteLineHandler(string message);

        private WriteLineHandler writeLineHandler;

        private StringWriter stringWriter;
        private StreamWriter streamWriter;
        private string logFileName = "tie.log";
        private bool logAppend = true;

        private Logger()
        {
            SetLogTarget(LOGTARGET.NullWriter);
        }

        private void SetLogTarget(LOGTARGET target)
        {
            switch (target)
            {
                case LOGTARGET.NullWriter:
                    this.writeLineHandler = NullWriteLine;
                    break;

                case LOGTARGET.Console:
                    this.writeLineHandler = new WriteLineHandler(Console.WriteLine);
                    break;

                case LOGTARGET.StringWriter:
                    this.stringWriter = new StringWriter();
                    this.writeLineHandler = new WriteLineHandler(stringWriter.WriteLine);
                    break;

                case LOGTARGET.File:
                    this.writeLineHandler = new WriteLineHandler(StreamWriteLine);
                    break;

            }

        }


        private void NullWriteLine(string message)
        { 
        }

        private void StreamWriteLine(string message)
        {
            if (this.streamWriter == null)      //prevent OPEN file many times
                this.streamWriter = new StreamWriter(logFileName, logAppend);

            streamWriter.WriteLine(message);
            this.streamWriter.Flush();
       
        }

        private static Logger Instance
        {
            get
            {
                if (logger == null)
                    logger = new Logger();
                return logger;
            }
        }


        /// <summary>
        /// Open log device
        /// </summary>
        /// <param name="target"></param>
        public static void Open(LOGTARGET target)
        {
            Close();
            Logger logger = Logger.Instance;
            logger.SetLogTarget(target);
        }

        /// <summary>
        /// Open log file
        /// </summary>
        /// <param name="logFileName"></param>
        public static void Open(string logFileName)
        {
            Close();
            Logger logger = Logger.Instance;
            logger.logFileName = logFileName;
            logger.logAppend = false;
            logger.SetLogTarget(LOGTARGET.File);
        }

        internal static WriteLineHandler WriteLine
        {
            get
            {
                return Logger.Instance.writeLineHandler;
            }
        }

        /// <summary>
        /// log string
        /// </summary>
        public static string Buffer
        {
            get
            {
                if (Logger.Instance.stringWriter != null)
                    return Logger.Instance.stringWriter.ToString();

                return null;
            }
        }

        /// <summary>
        /// Close log device
        /// </summary>
        public static void Close()
        {
            if (Logger.Instance.streamWriter != null)
            {
                Logger.Instance.streamWriter.Close();
                Logger.Instance.streamWriter = null;
            }
        }

    }
}
