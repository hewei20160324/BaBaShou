using UnityEngine;
using System.Collections;
using System.Xml.Serialization;
using System;
using System.IO;
using System.Text;

namespace CustomUtil
{
    public class CommonUtil
    {
        #region xml serial
        /// <summary>
        /// 读取xml内容到数据结构;
        /// </summary>

        public static T ReadFromXmlString<T>(string xmlString) where T : class
        {
            XmlSerializer x = new XmlSerializer(typeof(T));
            MemoryStream ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(xmlString));
            T ret = x.Deserialize(ms) as T;
            return ret;
        }

        public static T ReadFromXmlString<T>(byte[] bytes) where T : class
        {
            XmlSerializer x = new XmlSerializer(typeof(T));
            MemoryStream ms = new MemoryStream(bytes);
            T ret = x.Deserialize(ms) as T;
            return ret;
        }

        /// <summary>
        /// 从文件读取xml内容到数据结构;
        /// </summary>

        public static T LoadFromXmlFile<T>(string fileName) where T : class
        {
            XmlSerializer x = new XmlSerializer(typeof(T));
            FileStream f = new FileStream(fileName, FileMode.Open);
            T ret = x.Deserialize(f) as T;
            f.Close();
            return ret;
        }

        /// <summary>
        /// 将xml结构写入文件;
        /// </summary>

        public static void SaveToXml(System.Object obj, string fileName)
        {
            try
            {
                XmlSerializer x = new XmlSerializer(obj.GetType());
                FileStream f = new FileStream(fileName, FileMode.Create);
                StreamWriter sw = new StreamWriter(f, Encoding.UTF8);
                XmlSerializerNamespaces xsn = new XmlSerializerNamespaces();
                xsn.Add(string.Empty, string.Empty);
                x.Serialize(sw, obj, xsn);
                f.Close();
                sw.Close();
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError(ex.ToString());
            }
        }
        #endregion

        /// <summary>
        /// format string valid check;
        /// </summary>

        public static bool StringFormatValidCheck(string formatStr, int paramStrLength)
        {
            if (paramStrLength == 0 || string.IsNullOrEmpty(formatStr))
            {
                return false;
            }

            int count = System.Text.RegularExpressions.Regex.Matches(formatStr, @"{\d*?}").Count;
            if (paramStrLength == count)
            {
                return true;
            }
            return false;
        }
    }
}


