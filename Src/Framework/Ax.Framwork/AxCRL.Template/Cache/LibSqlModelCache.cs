using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Caching;
using System.Collections.Specialized;
using System.IO;
using System.Data;
using AxCRL.Comm.Runtime;
using System.Runtime.Serialization.Formatters.Binary;
using AxCRL.Comm.Utils;
using AxCRL.Template.DataSource;
using System.Collections.Concurrent;
using AxCRL.Comm.Redis;

namespace AxCRL.Template
{

    public class LibSqlModelCache : MemoryCacheRedis
    {
        private static LibSqlModelCache _Default = null;
        private static object _LockObj = new object();
        private static ConcurrentDictionary<string, object> lockObjDic = null;
        private static object _dicContainsLock = new object();
        /// <summary>
        /// 功能模块、数据表、数据列是否存在的全局信息存储。
        /// </summary>
        private static Dictionary<string, bool> _DicContainsProg = new Dictionary<string, bool>();
        private static Dictionary<string, bool> _DicContainsProgTable = new Dictionary<string, bool>();
        private static Dictionary<string, bool> _DicContainsProgTableColumn = new Dictionary<string, bool>();

        public LibSqlModelCache(string name, NameValueCollection config = null)
            : base(name)
        {
            
        }

        public static  LibSqlModelCache Default
        {
            get
            {
                if (_Default == null)
                {
                    lock (_LockObj)
                    {
                        if (_Default == null)
                        {
                            _Default = new LibSqlModelCache("LibSqlModelCache");
                            lockObjDic = new ConcurrentDictionary<string, object>();
                        }
                    }
                }
                return _Default;
            }
        }


        public LibSqlModel GetSqlModel(string name)
        {  
            #region 序列化
            byte[] mybyte = Default.StringGetBytes(name);
            LibSqlModel dataSet = null;
            LibBinaryFormatter formatter = new LibBinaryFormatter();
            if (mybyte != null&& mybyte.Length !=0)
            { 
                MemoryStream stream = new MemoryStream(mybyte); 
                stream.Position = 0;
                dataSet = (LibSqlModel)formatter.Deserialize(stream);  
            }
            #endregion


            if (dataSet == null || mybyte == null)
            {
                object lockItem = lockObjDic.GetOrAdd(name, new object());
                lock (lockItem)
                { 
                    string preFix = name.Substring(0, name.IndexOf('.'));
                    string path = Path.Combine(EnvProvider.Default.MainPath, "SqlModel", preFix, string.Format("{0}.bin", name));
                    if (File.Exists(path))
                    {
                        using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                        { 
                            dataSet = (LibSqlModel)formatter.Deserialize(fs);
                            fs.Position = 0;
                            BinaryReader br = new BinaryReader(fs);
                            byte[] bytes = br.ReadBytes((int)br.BaseStream.Length);
                            br.Close();
                            Default.StringSetBytes(name, bytes, new TimeSpan(0, 30, 0)); 
                        }
                    }

                }
            }
            return dataSet;
        }
        /// <summary>
        /// 是否包含指定功能模块、指定数据表、指定数据列
        /// </summary>
        /// <param name="progId"></param>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public bool Contains(string progId, string tableName = "", string columnName = "")
        {
            if (string.IsNullOrEmpty(progId))
                return false;
            if (string.IsNullOrEmpty(tableName) && string.IsNullOrEmpty(columnName) == false)
                return false;//指定了列名未指定表名，直接返回true
            bool stepContains = true;
            LibSqlModel sqlModel = null;
            lock (_dicContainsLock)
            {
                if (_DicContainsProg.ContainsKey(progId) == false)
                {
                    sqlModel = GetSqlModel(progId);
                    if (sqlModel == null)
                    {
                        _DicContainsProg[progId] = false;
                    }
                    else
                        _DicContainsProg[progId] = true;
                }
                stepContains= _DicContainsProg[progId];
                if (string.IsNullOrEmpty(tableName) == false && stepContains)
                {
                    string key = string.Format("{0}_{1}", progId, tableName);
                    if (_DicContainsProgTable.ContainsKey(key) == false)
                    {
                        sqlModel = GetSqlModel(progId);
                        if (sqlModel == null || sqlModel.Tables.Contains(tableName) == false)
                        {
                            _DicContainsProgTable[key] = false;
                        }
                        else
                            _DicContainsProgTable[key] = true;
                    }                   
                    stepContains = _DicContainsProgTable[key];
                    if (string.IsNullOrEmpty(columnName) == false && stepContains)
                    {
                        key = string.Format("{0}_{1}_{2}", progId, tableName,columnName);
                        if (_DicContainsProgTableColumn.ContainsKey(key) == false)
                        {
                            sqlModel = GetSqlModel(progId);
                            if (sqlModel == null || sqlModel.Tables.Contains(tableName) == false || sqlModel.Tables[tableName].Columns.Contains(columnName) == false)
                            {
                                _DicContainsProgTableColumn[key] = false;
                            }
                            else
                                _DicContainsProgTableColumn[key] = true;
                        }
                        stepContains = _DicContainsProgTableColumn[key];
                    }
                }
            }
            return stepContains;
        }
    }
}
