using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Windows.Forms;

namespace UniOPI
{
    public class ConfigXML
    {
        public string XmlFilePath { get; set; }
        XDocument XConfigXml;

        public Dictionary<string, string> Dic_PublicParam;

        public ConfigXML(string filePath,out string errMsg)
        {
            errMsg = string.Empty;

            if (File.Exists(filePath))
            {
                XmlFilePath = filePath;
                XConfigXml = XDocument.Load(XmlFilePath);

                GetPublicSectionParam();
            }
            else
            {
                errMsg = "Config.xml not exist.";
            }
        }

        private void GetPublicSectionParam()
        {
            //取得Public區所有的Key參數集合
            //若不知查詢結果型態就用var, 不然可直接使用IEnumerable<T>
            IEnumerable<XElement> result = XConfigXml.Descendants("Public").Elements("Key");

            //將結果載入Dictionary
            Dic_PublicParam = result.ToDictionary(r => r.FirstAttribute.Name.LocalName, r => r.FirstAttribute.Value);
        }

        /// <summary>
        /// 取得Public區段的參數值
        /// </summary>
        /// <param name="key">參數名稱</param>
        /// <returns></returns>
        public string GetPublicParam(string key)
        {
            return Dic_PublicParam.ContainsKey(key) ? Dic_PublicParam[key] : string.Format("Error:Param Key<{0}> not exist.", key);
        }

        public string GetParam(string section, string key)
        {
            //取得section區所有的Key參數集合  
            IEnumerable<XElement> result = XConfigXml.Descendants(section).Elements("Key").Where(v => v.FirstAttribute.Name.LocalName == key);

            string str = result.Count() == 1 ? result.First().FirstAttribute.Value : string.Format("Error:Param Key<{0}> not exist.", key);
            return str;
        }

    }
}
