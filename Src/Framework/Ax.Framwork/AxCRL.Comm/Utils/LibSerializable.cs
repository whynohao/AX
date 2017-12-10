using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCRL.Comm.Utils
{
    /// <summary>
    /// 自定义序列化接口，相关对象需实现此接口
    /// </summary>
    public interface ILibSerializable
    {
        void ReadObjectData(LibSerializationInfo info);
        void WriteObjectData(LibSerializationInfo info);
    }
    /// <summary>
    ///  提供对象序列化信息
    /// </summary>
    public sealed class LibSerializationInfo
    {
        private BinaryWriter m_Writer;
        private BinaryReader m_Reader;
        private long canReadLength = 0L;

        /// <summary>
        /// 序列化时，使用此构造器
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="graph"></param>
        public LibSerializationInfo(Stream stream, object graph)
        {
            m_Writer = new BinaryWriter(stream);
            WriteType(graph);
        }
        /// <summary>
        /// 反序列化时，使用此构造器
        /// </summary>
        /// <param name="stream"></param>
        public LibSerializationInfo(Stream stream)
        {
            m_Reader = new BinaryReader(stream);
            canReadLength = stream.Length;
        }
        /// <summary>
        /// 反序列化时，获取对象实例
        /// </summary>
        /// <returns></returns>
        public ILibSerializable GetObject()
        {
            Type t = ReadType();
            return (ILibSerializable)Activator.CreateInstance(t);
        }
        /// <summary>
        /// 序列化时,写入对象类型
        /// </summary>
        /// <param name="graph"></param>
        private void WriteType(object graph)
        {
            Type t = graph.GetType();
            m_Writer.Write(t.AssemblyQualifiedName);
        }

        private Type ReadType()
        {
            string aqName = m_Reader.ReadString();
            Type t = Type.GetType(aqName);
            return t;
        }

        public void WriteBoolean(bool value)
        {
            this.m_Writer.Write(value);
        }

        public void WriteByte(byte value)
        {
            this.m_Writer.Write(value);
        }

        public void WriteChar(char value)
        {
            this.m_Writer.Write(value);
        }

        public void WriteDecimal(decimal value)
        {
            int[] bits = decimal.GetBits(value);
            for (int i = 0; i < 4; i++)
            {
                this.m_Writer.Write(bits[i]);
            }
        }

        public void WriteDouble(double value)
        {
            this.m_Writer.Write(value);
        }

        public void WriteInt16(short value)
        {
            this.m_Writer.Write(value);
        }

        public void WriteInt32(int value)
        {
            this.m_Writer.Write(value);
        }

        public void WriteInt64(long value)
        {
            this.m_Writer.Write(value);
        }

        public void WriteString(string value)
        {
            this.m_Writer.Write(LibSysUtils.ToString(value));
        }

        public void WriteSByte(sbyte value)
        {
            this.m_Writer.Write(value);
        }

        public void WriteSingle(float value)
        {
            this.m_Writer.Write(value);
        }

        public void WriteUInt16(ushort value)
        {
            this.m_Writer.Write(value);
        }

        public void WriteUInt32(uint value)
        {
            this.m_Writer.Write(value);
        }

        public void WriteUInt64(ulong value)
        {
            this.m_Writer.Write(value);
        }


        public void WriteObject(object value)
        {
            WriteType(value);
            ILibSerializable serializer = (ILibSerializable)value;
            serializer.WriteObjectData(this);
        }

        public object ReadObject()
        {
            Type t = ReadType();
            object obj = Activator.CreateInstance(t);
            ILibSerializable deserialize = (ILibSerializable)obj;
            deserialize.ReadObjectData(this);
            return deserialize;
        }

        public bool ReadBoolean()
        {
            if (this.m_Reader.BaseStream.Position < canReadLength)
                return this.m_Reader.ReadBoolean();
            else
                return false;
        }

        public byte ReadByte()
        {
            if (this.m_Reader.BaseStream.Position < canReadLength)
                return this.m_Reader.ReadByte();
            else
                return 0;
        }

        public char ReadChar()
        {
            if (this.m_Reader.BaseStream.Position < canReadLength)
                return this.m_Reader.ReadChar();
            else
                return char.MinValue;
        }


        public decimal ReadDecimal()
        {
            if (this.m_Reader.BaseStream.Position < canReadLength)
            {
                int[] bits = new int[] { m_Reader.ReadInt32(), m_Reader.ReadInt32(), 
                                    m_Reader.ReadInt32(), m_Reader.ReadInt32() };
                decimal ret = new decimal(bits);
                return ret;
            }
            else
                return decimal.Zero;
        }

        public double ReadDouble()
        {
            if (this.m_Reader.BaseStream.Position < canReadLength)
                return this.m_Reader.ReadDouble();
            else
                return 0;
        }

        public short ReadInt16()
        {
            if (this.m_Reader.BaseStream.Position < canReadLength)
                return this.m_Reader.ReadInt16();
            else
                return 0;
        }

        public int ReadInt32()
        {
            if (this.m_Reader.BaseStream.Position < canReadLength)
                return this.m_Reader.ReadInt32();
            else
                return 0;
        }

        public long ReadInt64()
        {
            if (this.m_Reader.BaseStream.Position < canReadLength)
                return this.m_Reader.ReadInt64();
            else
                return 0L;
        }


        public sbyte ReadSByte()
        {
            if (this.m_Reader.BaseStream.Position < canReadLength)
                return this.m_Reader.ReadSByte();
            else
                return 0;
        }

        public float ReadSingle()
        {
            if (this.m_Reader.BaseStream.Position < canReadLength)
                return this.m_Reader.ReadSingle();
            else
                return 0;
        }

        public string ReadString()
        {
            if (this.m_Reader.BaseStream.Position < canReadLength)
                return this.m_Reader.ReadString();
            else
                return string.Empty;
        }


        public ushort ReadUInt16()
        {
            if (this.m_Reader.BaseStream.Position < canReadLength)
                return this.m_Reader.ReadUInt16();
            else
                return 0;
        }


        public uint ReadUInt32()
        {
            if (this.m_Reader.BaseStream.Position < canReadLength)
                return this.m_Reader.ReadUInt32();
            else
                return 0;
        }

        public ulong ReadUInt64()
        {
            if (this.m_Reader.BaseStream.Position < canReadLength)
                return this.m_Reader.ReadUInt64();
            else
                return 0;
        }
    }

    /// <summary>
    /// 自定义二进制序列化
    /// </summary>
    public sealed class LibBinaryFormatter
    {
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="serializationStream"></param>
        /// <returns></returns>
        public object Deserialize(Stream serializationStream)
        {
            LibSerializationInfo sInfo = new LibSerializationInfo(serializationStream);
            ILibSerializable deserialize = sInfo.GetObject();
            deserialize.ReadObjectData(sInfo);
            return deserialize;
        }
        /// <summary>
        /// 序列化对象
        /// 序列化顺序
        /// 1、类型名
        /// 2、对象的各个属性
        /// </summary>
        /// <param name="serializationStream">流</param>
        /// <param name="graph">对象</param>
        public void Serialize(Stream serializationStream, object graph)
        {
            ILibSerializable libSerializable = (ILibSerializable)graph;
            LibSerializationInfo sInfo = new LibSerializationInfo(serializationStream, graph);
            libSerializable.WriteObjectData(sInfo);
        }
    }
}
