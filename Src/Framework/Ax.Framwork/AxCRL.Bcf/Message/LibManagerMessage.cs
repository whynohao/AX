using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace AxCRL.Bcf
{
    public class LibManagerMessage
    {
        private LibMessageList _MessageList = null;
        private static readonly object listLock = new object();

        public LibMessageList MessageList
        {
            get
            {
                if (_MessageList == null)
                {
                    lock (listLock)
                    {
                        if (_MessageList == null)
                            _MessageList = new LibMessageList();
                    }
                }
                return _MessageList;

            }
        }

        public int Count
        {
            get
            {
                return this.MessageList.Count;
            }
        }

        public bool IsThrow
        {
            get
            {
                bool ret = false;
                foreach (var item in this.MessageList)
                {
                    if (item.MessageKind == LibMessageKind.Error)
                    {
                        ret = true;
                        break;
                    }
                }
                return ret;
            }
        }

        public void ThrowException(string message)
        {
            throw new Exception(message);
        }


        public void AddMessage(LibMessage message)
        {
            this.MessageList.Add(message);
        }

        public void AddMessage(LibMessageKind messageKind, string message)
        {
            this.MessageList.Add(new LibMessage() { MessageKind = messageKind, Message = message });
        }

        public void AddMessage(int tableIndex, int rowId, string field, LibMessageKind messageKind, string message)
        {
            this.MessageList.Add(new LibMessage() { TableIndex = tableIndex, RowId = rowId, Field = field, MessageKind = messageKind, Message = message });
        }
    }

    /// <summary>
    /// 信息列表
    /// </summary>
    public class LibMessageList : List<LibMessage>
    {
        public bool HasError()
        {
            return this.Any(msg =>
            {
                return msg.MessageKind == LibMessageKind.Error || msg.MessageKind == LibMessageKind.SysException;
            });
        }
    }
    /// <summary>
    /// 信息
    /// </summary>
    [DataContract]
    public class LibMessage
    {
        private string _Message;
        private int _TableIndex = 0;
        private int _RowId;
        private string _Field;
        private LibMessageKind _MessageKind = LibMessageKind.Error;
        [DataMember]
        public LibMessageKind MessageKind
        {
            get { return _MessageKind; }
            set { _MessageKind = value; }
        }
        [DataMember]
        public string Field
        {
            get { return _Field; }
            set { _Field = value; }
        }

        [DataMember]
        public int RowId
        {
            get { return _RowId; }
            set { _RowId = value; }
        }
        [DataMember]
        public int TableIndex
        {
            get { return _TableIndex; }
            set { _TableIndex = value; }
        }
        [DataMember]
        public string Message
        {
            get { return _Message; }
            set { _Message = value; }
        }
    }

    /// <summary>
    /// 信息类别
    /// </summary>
    public enum LibMessageKind
    {
        /// <summary>
        ///  提示
        /// </summary>
        Info = 0,
        /// <summary>
        ///  警告
        /// </summary>
        Warn = 1,
        /// <summary>
        /// 错误
        /// </summary>
        Error = 2,
        /// <summary>
        /// 系统异常
        /// </summary>
        SysException = 3
    }
}
