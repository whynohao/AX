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
using System.Reflection;

namespace AxCRL.Parser
{
    /// <summary>
    /// Operate TIE script code.
    /// </summary>
    public sealed partial class Script : IDisposable
    {
        private string moduleName;
        private int moduleSize;

        private string scope;
        private Context context;

     /// <summary>
     /// Initializes a new instance of the Tie.Script class, using dynamically created GUID as module name.
     /// CODE segment size = 16K
     /// CODE will be destoryed once the instance is deconstructed
     /// </summary>
        public Script()
            : this(UniqueName, Constant.MAX_INSTRUCTION_NUM, true)
        {

        }

        /// <summary>
        /// Initializes a new instance of the Tie.Script class, using dynamically created GUID as module name.
        /// CODE will be destoryed once the instance is deconstructed
        /// </summary>
        /// <param name="moduleSize">Code segment size </param>
        public Script(int moduleSize)
            : this(UniqueName, moduleSize, true)
        {

        }

        /// <summary>
        /// Initializes a new instance of the Tie.Script class
        /// CODE segment size = 16K
        /// CODE will be destoryed once the instance is deconstructed
        /// </summary>
        /// <param name="moduleName">Script module name</param>
        public Script(string moduleName)
            : this(moduleName, Constant.MAX_INSTRUCTION_NUM, true)
        {

        }

        /// <summary>
        /// Initializes a new instance of the Tie.Script class
        /// CODE will be destoryed once the instance is deconstructed
        /// </summary>
        /// <param name="moduleName">Module name</param>
        /// <param name="moduleSize">Code segment size</param>
        public Script(string moduleName, int moduleSize)
            : this(moduleName, moduleSize, true)
        {

        }

     
        /// <summary>
        /// Initializes a new instance of the Tie.Script class
        /// </summary>
        /// <param name="moduleName">Module name</param>
        /// <param name="moduleSize">Code segment size</param>
        /// <param name="destroyed">indicate CODE is destroyed or not</param>
        public Script(string moduleName, int moduleSize, bool destroyed)
        {
            this.moduleName = moduleName;
            this.moduleSize = moduleSize;
            this.destroyed = destroyed;

            this.scope = "";
            this.context = new Context(new Memory());

            RemoveModule();     //Clear Module CS
        }


        private static string UniqueName
        {
            get { return Guid.NewGuid().ToString(); }
        }


        private void SyncInstance(object instance, bool toHost)
        {
            SyncInstance(DS, instance, toHost);
        }


        /// <summary>
        /// Synchronize class's internal Fields/Properties/Methods between HOST and Data Segment
        /// </summary>
        /// <param name="DS">Data segment(DS)</param>
        /// <param name="instance">Host instance</param>
        /// <param name="toHost">true: synchronize from DS to Host, false: synchrnize from Host to DS</param>
        public static void SyncInstance(Memory DS, object instance, bool toHost)
        {

            Type type = instance.GetType();
            FieldInfo[] fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            foreach (FieldInfo fieldInfo in fields)
            {
                if (toHost)
                {
                    VAL v = DS[fieldInfo.Name];
                    if(v.Defined)
                        fieldInfo.SetValue(instance, v.HostValue);
                }
                else
                {
                    object obj = fieldInfo.GetValue(instance);
                    DS.AddObject(fieldInfo.Name, obj);
                }
            }
      
            PropertyInfo[] properties = type.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            foreach (PropertyInfo propertyInfo in properties)
            {
                if (toHost)
                {
                    VAL v = DS[propertyInfo.Name];
                    if(v.Defined && propertyInfo.CanWrite) 
                        propertyInfo.SetValue(instance, v.HostValue, null);
                }
                else
                {
                    if (propertyInfo.CanRead)
                    {
                        object obj = propertyInfo.GetValue(instance, null);
                        DS.AddObject(propertyInfo.Name, obj);
                    }
                }
            }

            MethodInfo[] methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            foreach (MethodInfo methodInfo in methods)
            {
                if (!toHost)
                {
                    VAL method = VAL.NewHostType(methodInfo);

                    if(methodInfo.IsPublic)
                        method.temp = new HostOffset(instance, methodInfo.Name);
                    else
                        method.temp = new HostOffset(instance, methodInfo);
                    
                    DS.Add(methodInfo.Name, method);
                }
            }

            EventInfo[] events = type.GetEvents(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            foreach (EventInfo eventInfo in events)
            {
                if (!toHost)
                {
                    VAL _event = VAL.NewHostType(eventInfo);
                    DS.Add(eventInfo.Name, _event);
                }
            }
        }
        #region Dispose

        private bool disposed = false;
        private bool destroyed = true;
        
        /// <summary>
        /// Destroy instance
        /// </summary>
        public void Dispose()
        {
            if (!destroyed)
                return;

            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this.disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources (like other .NET components)
                    // ...
                    RemoveModule();
                }

                // Dispose UNMANAGED resources (like P/Invoke functions)

                // Note disposing has been done.
                disposed = true;

            }
        }

        /// <summary>
        /// Dispose object
        /// </summary>
        ~Script()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        /// <summary>
        /// explicit dispose instance
        /// </summary>
        public void Close()
        {
            // Calls the Dispose method without parameters.
            Dispose();
        }
        
        #endregion


        /// <summary>
        /// Get module name
        /// </summary>
        public string ModuleName
        {
            get { return moduleName; }
        }

        /// <summary>
        /// Get data segment of this instance
        /// </summary>
        public Memory DS
        {
            get { return context.DataSegment; }
            set { context.DataSegment = value; }
        }

        /// <summary>
        /// Set/Get scope of script.
        /// code piece may be a part of class code.
        /// e.g.
        ///     code piece: 
        ///         this.x = 20;
        ///     the value of (this)above is defined by property (Scope)
        /// </summary>
        public string Scope
        {
            get { return scope; }
            set { scope = value; }
        }

        /// <summary>
        /// Tie script functions extension. Tie will call functions defined in the (UserFunction)
        /// </summary>
        public IUserDefinedFunction UserFunction
        {
            get { return context.UserFunction; }
            set { context.UserFunction = value; }
        }

        /// <summary>
        /// System level memory (Data Segment), this is restricted to use 
        /// </summary>
        public static Memory SystemMemory
        {
            get { return Computer.DS1; }
        }

        /// <summary>
        /// Shared memory (Data Segment) used by all scripts
        /// </summary>
        public static Memory CommonMemory
        {
            get { return Computer.DS2; }
        }

        /// <summary>
        /// Fuction chain used to implement tie script functions by .NET code
        /// </summary>
        public static FunctionChain FunctionChain
        {
            get
            {
                return FunctionChain.Chain;
            }
        }
        


        #region Function Invoke


        //产生一个$class的实例, 等价于InvokeFunction(..)
        /// <summary>
        /// Create Tie class instance
        /// </summary>
        /// <param name="className">class name</param>
        /// <param name="parameters">parameters of constructor</param>
        /// <returns></returns>
        public VAL CreateInstance(string className, object[] parameters)
        {
            return InvokeFunction(className, parameters);
        }

  
        //调用全局$function
        /// <summary>
        /// Invoke Tie function
        /// </summary>
        /// <param name="funcName">function name</param>
        /// <param name="parameters">parameters of function</param>
        /// <returns></returns>
        public VAL InvokeFunction(string funcName,  object[] parameters)
        {
            VAL f = DS[funcName];
            if(f.Undefined)
                return VAL.VOID;

            VAL instance = new VAL();     
            return InvokeFunction(instance, f, parameters);
        }

        //调用$class中的一个方法method
        /// <summary>
        /// Invoke method of Tie script class
        /// </summary>
        /// <param name="instance">instance of class</param>
        /// <param name="methodName">method name</param>
        /// <param name="parameters">method parameters</param>
        /// <returns></returns>
        public VAL InvokeMethod(VAL instance, string methodName,  object[] parameters)
        {
            VAL f = instance[methodName];
            if (!f.Defined)
                return VAL.VOID;

            return InvokeFunction(instance, f, parameters);
        }

        /**
         * 
         * 这个函数,
         *  1. CreateInstance 为$class
         *  2. 调用gobal 或者 static 函数$function
         *  3. 调用$class的method
         *  
         * 上面的3个函数都是调用这个函数的.
         **/
        /// <summary>
        /// Create class instance,invoke global function or method of class  
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="funcEntry"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public VAL InvokeFunction(VAL instance, VAL funcEntry, object[] parameters)
        {
            if (funcEntry.ty != VALTYPE.funccon
                && funcEntry.ty != VALTYPE.classcon)
                throw new TieException("{0} is not function or class entry.", funcEntry);

            VAL arguments = new VAL(parameters);
            return CPU.ExternalUserFuncCall(funcEntry, instance, arguments, context);
        }

        
        //调用TIE的系统函数以及用户添加的Function Chain
        /// <summary>
        /// Invoke function defined in the function chains
        /// </summary>
        /// <param name="funcName">function name</param>
        /// <param name="parameters">parameters of function</param>
        /// <returns></returns>
        public VAL InvokeChainedFunction(string funcName, object[] parameters)
        {
            return Script.InvokeChainedFunction(context.DataSegment, funcName, parameters);
        }
 

        /***
         * 
         * 1.func是TIE script $function, 含有函数入口地址$function(...)
         *    如:
         *      a.class的成员函数
         *         ret = InvokeFunction(DS, instance, functionAddress, ...);
         *      b.全局函数
         *         ret = InvokeFunction(DS, new VAL(), functionAddress, ...);
         *         
         * 2.也可以是Tie script $class, 
         *     如: instance = InvokeFunction(DS, new VAL(), classAddress,...);
         *       等价于 instance = CreateInstance(DS, classAddress, ...);
         * 
         * */
        /// <summary>
        /// Invoke method in the data segment
        /// </summary>
        /// <param name="memory">data segment</param>
        /// <param name="instance">instance of Tie class</param>
        /// <param name="funcEntry">method entry</param>
        /// <param name="parameters">parameters of method</param>
        /// <returns></returns>
        public static VAL InvokeFunction(Memory memory, VAL instance, VAL funcEntry, object[] parameters)
        {
            VAL arguments = new VAL(parameters);
            return CPU.ExternalUserFuncCall(funcEntry, instance, arguments, new Context(memory));
        }

        /// <summary>
        /// Invoke Tie function in the data segment
        /// </summary>
        /// <param name="memory">data segment</param>
        /// <param name="func">function entry</param>
        /// <param name="parameters">parameters of function</param>
        /// <returns></returns>
        public static VAL CreateInstance(Memory memory, VAL func, object[] parameters)
        {
            return InvokeFunction(memory, new VAL(), func, parameters);
        }


        /***
         * 
         * 调用TIE的FunctionChain中函数
         * 
         * */
        /// <summary>
        ///  Invoke function defined in the function chains
        /// </summary>
        /// <param name="memory"></param>
        /// <param name="funcName">function name</param>
        /// <param name="parameters">function signatrue</param>
        /// <returns></returns>
        public static VAL InvokeChainedFunction(Memory memory, string funcName, object[] parameters)
        {
            Context context = new Context(memory);
            VAL arguments = new VAL(parameters);
            return context.InvokeFunction(funcName, arguments, Position.UNKNOWN);
        }



        /**
        * 不支持函数重载:
        * 
        * 调用.net host的函数,不管是private还是public, 
        *  
        * 
        * 容许调用静态函数
        *   这个时候,instance为Type, 如: InvokeHostFunction(typeof(System.Convert), "ToInt32", ...); 
        *   
        * 
        * */
        /// <summary>
        /// Invoke .NET private or public method(static method)
        /// </summary>
        /// <param name="instance">either instance or class Type</param>
        /// <param name="methodName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static object InvokeHostMethod(object instance, string methodName, object[] parameters)
        {
            Type type;
            
            if(instance is Type)
                type = (Type)instance;
            else
                type = instance.GetType();
            
            MethodInfo methodInfo = type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            if(methodInfo!=null)
                return methodInfo.Invoke(instance, parameters);
            
            return null;
        }

        #endregion


        #region Evalute/Execute

        /*
         * 加载新的Module
         * 
         * module name 由directive语句
         *      #module moduleName; 决定
         * 
         * 
         * 或者继续把CodeBlock加载现有的Module
         * 
         * 
         * */
        /// <summary>
        /// Execute tie script statements and CodeBlock is resident
        /// </summary>
        /// <param name="src">source code</param>
        public void Execute(string src)
        {
            Execute(src, CodeType.statements, CodeMode.Append);       //缺省: 保留statements的代码在module上面
        }

        /// <summary>
        /// Execute tie script statements with Host instance synchronized,and CodeBlock is resident
        /// keyword this in the script pointer to host instance
        /// </summary>
        /// <param name="src">source code</param>
        /// <param name="instance">Host instance</param>
        public void Execute(string src, object instance)
        {
            //支持this操作符,但是this.xxx只能是public成员
            this.scope = "THIS";
            DS.AddObject(this.scope, instance);

            SyncInstance(instance, false);
            Execute(src, CodeType.statements, CodeMode.Append);       //缺省: 保留statements的代码在module上面
            SyncInstance(instance, true);
        }

        /// <summary>
        /// Execute tie script statements 
        /// </summary>
        /// <param name="src">source code</param>
        /// <param name="overwritten">true: existed CodeBlock is overwritten</param>
        public void Execute(string src, bool overwritten)
        {
            Execute(src, CodeType.statements, overwritten? CodeMode.Overwritten: CodeMode.Append);    
        }


        

        /// <summary>
        /// Evaluate an expression, CodeBlock is resident
        /// </summary>
        /// <param name="src">source code expression</param>
        /// <returns>value of expression</returns>
        public VAL ResidentEvaluate(string src)
        {
            return Execute(src, CodeType.expression, CodeMode.Append);     //缺省:保留expression的代码在module上面
        }

        /// <summary>
        /// Execute source code from a file
        /// </summary>
        /// <param name="fileName"></param>
        public void ExecuteFromFile(string fileName) //读入文件并运行,moduleName 为文件名
        {
            StreamReader streamReader = new StreamReader(fileName);
            string src = streamReader.ReadToEnd();
            streamReader.Close();
            
            this.moduleName = Path.GetFileNameWithoutExtension(fileName);
            Execute(src, CodeType.statements, CodeMode.Append); 
        }

        //驻留在内存中,直到RemoveModule
        private VAL Execute(string src, CodeType ty, CodeMode overwritten)
        {
            Module module = Library.CompileModule(ref moduleName, moduleSize, scope, src, ty, overwritten);
            if (module != null)
                return Computer.Run(module, context);

            return VAL.VOID;

        }



        //----------------------------------------------------------------
        /// <summary>
        /// Execute code and then CodeBlock is thrown away
        /// </summary>
        /// <param name="src">Source code statements</param>
        public void VolatileExecute(string src)
        {
            //不驻留code在内存中, 在另外一个module中运行,每次都产生一个module
            Computer.Run(scope, src, CodeType.statements, context);
        }

        /// <summary>
        /// Execute code and then CodeBlock is thrown away
        /// keyword (this) in the script pointer to host instance
        /// </summary>
        /// <param name="src">Source code statements</param>
        /// <param name="instance">Host instance</param>
        public void VolatileExecute(string src, object instance)
        {
            SyncInstance(instance, false);
            Computer.Run(scope, src, CodeType.statements, context);
            SyncInstance(instance, true);
        }

        /// <summary>
        /// Evaluate expression and then CodeBlock is thrown away
        /// </summary>
        /// <param name="src">Souce code expression</param>
        /// <returns>Value of expression</returns>
        public VAL VolatileEvaluate(string src)
        {
            return Computer.Run(scope, src, CodeType.expression, context); 
        }

        #endregion


        #region compile once, execute multiple times
        /// <summary>
        /// Compile source code statement, one module may include many code blocks
        /// </summary>
        /// <param name="src">Source code statements</param>
        /// <param name="overwritten">true: overwrite existed code blocks in the module, false: append to existed code blocks</param>
        /// <returns>Entry of code segment</returns>
        public int Compile(string src, bool overwritten)
        {
            //返回entry的IP
            Module module= Library.CompileModule(ref moduleName, moduleSize, scope, src, CodeType.statements, overwritten? CodeMode.Overwritten: CodeMode.Append);
            if (module != null)
                return module.IP1;
            else
                return -1;
        }

        /// <summary>
        /// Run Codeblock from address entry 
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        public VAL Run(int entry)
        {
            Module module = Library.GetModule(moduleName);
            module.IP1 = entry;
            return Computer.Run(module, context);
        }

        #endregion


        
        
        #region Clear/Remove: Libray/Module/EventHandler

        /// <summary>
        /// Remove current module from library
        /// </summary>
        public void RemoveModule()
        {
            Library.RemoveModule(moduleName);
        }

        /// <summary>
        /// Clear all modules in the library
        /// </summary>
        public static void ClearLibrary()
        {
            Library.ClearLibrary();
        }


        /// <summary>
        /// Clear event handlers
        /// </summary>
        public void ClearEventHandler()
        {
            HostEvent.ClearEventHandler(context.DataSegment);
        }

        /// <summary>
        ///  Remove event handler is supported or not. System default is not supported.
        /// </summary>
        public bool RemoveEventHandlerSupported
        {
            get
            {
                return HostEvent.RemoveEventHandlerSupported;
            }
            set
            {
                HostEvent.RemoveEventHandlerSupported = value;
            }
        }

        #endregion




        #region DEBUG feature, support breakpoint

        /**
         * 用例: 每一行都停顿
         * 
         *  int line = 1;
         *  if(script.DebugStart(src))
         *      while(script.DebugContinue(line++, handler));
         * 
         * */
        private CPU cpu = null;
        
        /// <summary>
        /// Debug handler, when breakpoint is reached, debug handler is invoked
        /// </summary>
        /// <param name="breakpoint">Address of break point</param>
        /// <param name="info">CPU registers/stacks infomation is passed in</param>
        /// <param name="DS2">Shared data segment</param>
        public delegate void DebugHandler(int breakpoint, int cursor, string info, Memory DS2);
        
        /// <summary>
        /// Debug Tie script
        /// </summary>
        /// <param name="src"></param>
        /// <returns>source code statements</returns>
        public bool DebugStart(string src)
        {
            Module module = Library.CompileModule(ref moduleName, moduleSize, scope, src, CodeType.statements, CodeMode.Overwritten);
            if (module!=null)
                cpu = new CPU(module, context);

            return module != null;
        }

        /// <summary>
        /// Run code, stop at breakpoint indicated, and debug handler is invoked
        /// </summary>
        /// <param name="breakpoint">address of break point</param>
        /// <param name="debugHandler">debug handler</param>
        /// <returns></returns>
        public bool DebugContinue(int breakpoint, DebugHandler debugHandler)
        {
            if ((object)Computer.Run(cpu, breakpoint) == null)
            {
                debugHandler(breakpoint, cpu.Position.cur, cpu.DebugInfo(), context.DataSegment);
                return true;
            }

            cpu = null;
            return false;
        }
        
        #endregion

    }
}
