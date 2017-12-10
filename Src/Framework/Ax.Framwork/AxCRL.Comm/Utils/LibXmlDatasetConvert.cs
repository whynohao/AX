﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.IO;
using System.Xml;

namespace AxCRL.Comm.Utils
{
    public class LibXmlDatasetConvert
    {
        //将xml对象内容字符串转换为DataSet  
        public static DataSet ConvertXMLToDataSet(string xmlData)  
        {  
            StringReader stream = null;  
            XmlTextReader reader = null;  
            try  
            {  
                DataSet xmlDS = new DataSet();  
                stream = new StringReader(xmlData);  
                //从stream装载到XmlTextReader  
                reader = new XmlTextReader(stream);  
                xmlDS.ReadXml(reader);  
                return xmlDS;  
            }  
            catch (System.Exception ex)  
            {  
                throw ex;  
            }  
            finally  
            {  
                if (reader != null) reader.Close();  
            }  
        }  
  
        //将xml文件转换为DataSet  
        public static DataSet ConvertXMLFileToDataSet(string xmlFile)  
        {  
            StringReader stream = null;  
            XmlTextReader reader = null;  
            try  
            {  
                XmlDocument xmld = new XmlDocument();  
                xmld.Load(xmlFile);  
  
                DataSet xmlDS = new DataSet();  
                stream = new StringReader(xmld.InnerXml);  
                //从stream装载到XmlTextReader  
                reader = new XmlTextReader(stream);  
                xmlDS.ReadXml(reader);  
                //xmlDS.ReadXml(xmlFile);  
                return xmlDS;  
            }  
            catch (System.Exception ex)  
            {  
                throw ex;  
            }  
            finally  
            {  
                if (reader != null) reader.Close();  
            }  
        }  
  
        //将DataSet转换为xml对象字符串  
        public static string ConvertDataSetToXML(DataSet xmlDS)  
        {  
            MemoryStream stream = null;  
            XmlTextWriter writer = null;  
  
            try  
            {  
                stream = new MemoryStream();  
                //从stream装载到XmlTextReader  
                writer = new XmlTextWriter(stream, Encoding.Unicode);  
  
                //用WriteXml方法写入文件.  
                xmlDS.WriteXml(writer);  
                int count = (int)stream.Length;  
                byte[] arr = new byte[count];  
                stream.Seek(0, SeekOrigin.Begin);  
                stream.Read(arr, 0, count);

                UTF8Encoding utf = new UTF8Encoding();
                return utf.GetString(arr).Trim();
            }  
            catch (System.Exception ex)  
            {  
                throw ex;  
            }  
            finally  
            {  
                if (writer != null) writer.Close();  
            }  
        }  
  
        //将DataSet转换为xml文件  
        public static void ConvertDataSetToXMLFile(DataSet xmlDS,string xmlFile)  
        {  
            MemoryStream stream = null;  
            XmlTextWriter writer = null;  
  
            try  
            {  
                stream = new MemoryStream();  
                //从stream装载到XmlTextReader  
                writer = new XmlTextWriter(stream, Encoding.Unicode);  
  
                //用WriteXml方法写入文件.  
                xmlDS.WriteXml(writer);  
                int count = (int)stream.Length;  
                byte[] arr = new byte[count];  
                stream.Seek(0, SeekOrigin.Begin);  
                stream.Read(arr, 0, count);  
  
                //返回Unicode编码的文本  
                UnicodeEncoding utf = new UnicodeEncoding();  
                StreamWriter sw = new StreamWriter(xmlFile);
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");  
                sw.WriteLine(utf.GetString(arr).Trim());  
                sw.Close();  
            }  
            catch( System.Exception ex )  
            {  
                throw ex;  
            }  
            finally  
            {  
                if (writer != null) writer.Close();  
            }  
        }

        public static void DataSetToXml(DataSet ds, string filename)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<?xml version=\"1.0\" ?>");
            sb.Append("<DataSet>");
            foreach (DataTable dt in ds.Tables)
            {
                sb.AppendFormat("<Table Name=\"{0}\">", dt.TableName);
                foreach (DataRow row in dt.Rows)
                {
                    sb.Append("<Row>");
                    foreach (DataColumn col in dt.Columns)
                    {
                        sb.AppendFormat("<{0}>{1}</{0}>", col.ColumnName, row[col.ColumnName]);
                    }
                    sb.Append("</Row>");
                }
                sb.Append("</Table>");
            }
            sb.Append("</DataSet>");
            File.WriteAllText(filename, sb.ToString());
        }

    }  
}
