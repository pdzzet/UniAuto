using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.MesSpec
{
    //SpecialItem用來讀寫特殊的XML格式, 範例中的ABNORMALCODELIST是繼承自SpecialItem
    //  ** 目前不支援 TESTLIST 的存在 **
    //範例:
    //<ABNORMALCODELIST>
    //  <TESTLIST>                                  <<--- List of Default Format
    //    <TEST>                                    <<--- TEST視為LIST元素
    //      <TESTSEQ>1</TESTSEQ>
    //      <TESTCODE>TESTCODE1</TESTCODE>
    //    </TEST>
    //    <TEST>                                    <<--- TEST視為LIST元素
    //      <TESTSEQ>2</TESTSEQ>
    //      <TESTCODE>TESTCODE2</TESTCODE>
    //    </TEST>
    //  </TESTLIST>
    //  <CODE>                                      <<--- List of Customize Format, CODE視為LIST元素
    //    <ABNORMALSEQ>0</ABNORMALSEQ>
    //    <ABNORMALCODE>ABNORMALCODE</ABNORMALCODE>
    //  </CODE>
    //  <CODE>                                      <<--- List of Customize Format, CODE視為LIST元素
    //    <ABNORMALSEQ>0</ABNORMALSEQ>
    //    <ABNORMALCODE>ABNORMALCODE</ABNORMALCODE>
    //  </CODE>
    //  <NAME>NAME</NAME>                           <<--- NAME視為Item
    //  <DESCRIPTION>DESCRIPTION</DESCRIPTION>      <<--- DESCRIPTION視為Item
    //</ABNORMALCODELIST>

    /// <summary>
    /// 特別項目, 包括多個XmlArray和多個XmlElement
    /// </summary>
    public abstract class SpecialItem : IXmlSerializable
    {
        private class ListNameItemType
        {
            public string _listName = string.Empty;//List變數名
            public Type _itemType = null;//List中的元素型別
            public ListNameItemType(string ListName, Type ItemType)
            {
                _listName = ListName;
                _itemType = ItemType;
            }
        }

        protected class ListNameXmlType
        {
            public string _listName = string.Empty;//List變數名
            public bool _defaultXmlFormat = true;//XML是否使用預設格式
            public ListNameXmlType(string ListName, bool DefaultXmlFormat)
            {
                _listName = ListName;
                _defaultXmlFormat = DefaultXmlFormat;
            }
        }

        /// <summary>
        /// 回傳需要輸出XML的List名稱及輸出XML的方式
        /// </summary>
        /// <returns></returns>
        protected abstract List<ListNameXmlType> GetListNameAndXmlType();

        /// <summary>
        /// 回傳一般字串的Item名稱
        /// </summary>
        /// <returns></returns>
        protected abstract List<string> GetNameOfItems();

        protected void AddToList(string listName, List<object> listValues)
        {
            PropertyInfo[] prop_infos = GetType().GetProperties();
            foreach (PropertyInfo prop_info in prop_infos)
            {
                if (string.Compare(listName, prop_info.Name) == 0)
                {
                    object list = prop_info.GetValue(this, null);
                    MethodInfo add_method = list.GetType().GetMethod("Add");
                    foreach (object obj in listValues)
                    {
                        add_method.Invoke(list, new object[1] { obj });
                    }
                    break;
                }
            }
        }

        protected void SetItemValue(string itemName, string itemValue)
        {
            PropertyInfo[] prop_infos = GetType().GetProperties();
            foreach (PropertyInfo prop_info in prop_infos)
            {
                if (string.Compare(itemName, prop_info.Name) == 0)
                {
                    prop_info.SetValue(this, itemValue, null);
                    break;
                }
            }
        }

        /// <summary>
        /// IXmlSerializable
        /// </summary>
        /// <returns></returns>
        public XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// IXmlSerializable
        /// </summary>
        /// <returns></returns>
        public void ReadXml(XmlReader reader)
        {
            if (reader.IsEmptyElement)
            {
                reader.Read();// 讀取<ABNORMALCODELIST />
                return;
            }

            string name = GetType().Name.Substring(0, GetType().Name.Length - 1);//型別名稱有加綴字'c', 如ABNORMALCODELISTc
            if (reader.LocalName != name)
                return;

            PropertyInfo[] prop_infos = GetType().GetProperties();//全部的Property
            List<ListNameXmlType> list_name_xmltypes = GetListNameAndXmlType();//有多少個List要輸出XML
            List<string> item_names = GetNameOfItems();//有多少個Item要輸出XML
            List<ListNameItemType> list_item_types = new List<ListNameItemType>();//每個List的名稱與元素型別
            Dictionary<string, List<object>> list_items = new Dictionary<string, List<object>>();//存放每個List的元素; key=list name, value=list items
            #region 取出每個List的元素型別
            {
                foreach (PropertyInfo prop_info in prop_infos)
                {
                    foreach (ListNameXmlType list_name_xmltype in list_name_xmltypes)
                    {
                        if (string.Compare(list_name_xmltype._listName, prop_info.Name) == 0)
                        {
                            object list = prop_info.GetValue(this, null);//取得list的instance
                            Type type = list.GetType().GetGenericArguments()[0];//取得list中元素的型別
                            list_item_types.Add(new ListNameItemType(list_name_xmltype._listName, type));
                            break;
                        }
                    }
                }
            }
            #endregion

            reader.Read();// 讀取<ABNORMALCODELIST>
            while (reader.LocalName != name)
            {
                #region 讀取XML, 參考<ABNORMALCODELIST>範例
                {
                    bool find = false;
                    foreach (ListNameItemType list_name_item_type in list_item_types)
                    {
                        string item_type_name = list_name_item_type._itemType.Name;
                        item_type_name = item_type_name.Substring(0, item_type_name.Length - 1);//元素的型別名稱有加綴字'c', 如CODEc
                        if (reader.LocalName == item_type_name)//判斷是否為List元素
                        {
                            XmlSerializer xmlSerializer = new XmlSerializer(list_name_item_type._itemType);
                            object obj = xmlSerializer.Deserialize(reader);//讀取List元素
                            if (!list_items.ContainsKey(list_name_item_type._listName))
                            {
                                list_items.Add(list_name_item_type._listName, new List<object>());
                            }
                            list_items[list_name_item_type._listName].Add(obj);
                            find = true;
                            break;
                        }
                    }

                    if (!find)//不是List元素
                    {
                        foreach (string item_name in item_names)
                        {
                            if (reader.LocalName == item_name)//判斷是否為Item
                            {
                                if (!reader.IsEmptyElement)
                                {
                                    reader.ReadStartElement(item_name);//讀取<NAME>
                                    SetItemValue(item_name, reader.ReadString());//讀取InnerText
                                    reader.ReadEndElement();//讀取</NAME>
                                }
                                else
                                {
                                    reader.Read();//讀取<NAME />
                                    SetItemValue(item_name, string.Empty);
                                }
                                find = true;
                                break;
                            }
                        }
                    }

                    if (!find)
                        throw new Exception(string.Format("Unknown XML element ({0})", reader.LocalName));
                }
                #endregion
            }
            reader.ReadEndElement();// 讀取</ABNORMALCODELIST>

            foreach (string list_name in list_items.Keys)
            {
                AddToList(list_name, list_items[list_name]);
            }
        }

        /// <summary>
        /// IXmlSerializable
        /// </summary>
        /// <returns></returns>
        public void WriteXml(XmlWriter writer)
        {
            XmlSerializerNamespaces names = new XmlSerializerNamespaces();
            names.Add(string.Empty, string.Empty);//移除xmlns:xsi與xmlns:xsd

            PropertyInfo[] prop_infos = GetType().GetProperties();//全部的Property

            List<ListNameXmlType> list_name_xmltypes = GetListNameAndXmlType();//有多少個List要輸出XML且要輸出預設XML格式或特殊格式
            List<string> item_names = GetNameOfItems();//有多少個Item要輸出XML
            foreach (PropertyInfo prop_info in prop_infos)
            {
                bool find = false;
                for (int i = 0; i < list_name_xmltypes.Count; i++)
                {
                    string list_name = list_name_xmltypes[i]._listName;
                    if (string.Compare(list_name, prop_info.Name) == 0)
                    {
                        if (list_name_xmltypes[i]._defaultXmlFormat)
                        {
                            #region 整個List做XML序列化 ---- 暫時, 直接序列化無法輸出需要的格式, 必須自訂
                            {
                                object list = prop_info.GetValue(this, null);//取得list的instance
                                XmlSerializer listSerializer = new XmlSerializer(list.GetType());//list中元素要做XML序列化
                                listSerializer.Serialize(writer, list, names);//整個List做XML序列化
                            }
                            #endregion
                        }
                        else
                        {
                            #region 不是整個List, 而是List中每個元素要做XML序列化
                            {
                                //object list = prop_info.GetValue(this, null);//取得list的instance
                                //Type type = list.GetType().GetGenericArguments()[0];//取得list中元素的型別
                                //int count = (int)(list.GetType().GetProperty("Count").GetValue(list, null));//取得list的count
                                //XmlSerializer listSerializer = new XmlSerializer(type);//list中元素要做XML序列化
                                //for (int k = 0; k < count; k++)
                                //{
                                //    object obj = list.GetType().GetProperty("Item").GetValue(list, new object[1] { k });//取得list中的每個元素
                                //    listSerializer.Serialize(writer, obj, names);//每個元素做XML序列化
                                //}
                                object list = prop_info.GetValue(this, null);//取得list的instance
                                Type type = list.GetType().GetGenericArguments()[0];//取得list中元素的型別
                                int count = (int)(list.GetType().GetProperty("Count").GetValue(list, null));//取得list的count
                                writer.WriteStartElement(type.Name.Substring(0, type.Name.Length - 1));//型別名稱有加綴字'c', 如ABNORMALCODELISTc
                                for (int k = 0; k < count; k++)
                                {
                                    object obj = list.GetType().GetProperty("Item").GetValue(list, new object[1] { k });//取得list中的每個元素
                                    WriteXml(writer, type, obj);
                                }
                                writer.WriteEndElement();
                            }
                            #endregion
                        }
                        list_name_xmltypes.RemoveAt(i);
                        find = true;
                        break;
                    }
                }
                if (!find)
                {
                    for (int i = 0; i < item_names.Count; i++)
                    {
                        string item_name = item_names[i];
                        if (string.Compare(item_name, prop_info.Name) == 0)
                        {
                            #region 輸出簡單的XmlElement
                            {
                                string item_value = prop_info.GetValue(this, null) as string;
                                writer.WriteStartElement(item_name);
                                writer.WriteString(item_value);
                                writer.WriteEndElement();
                            }
                            #endregion
                            item_names.RemoveAt(i);
                            find = true;
                            break;
                        }
                    }
                }
            }
        }

        private void WriteXml(XmlWriter writer, Type type, object instance)
        {
            PropertyInfo[] prop_infos = type.GetProperties();//全部的Property
            foreach (PropertyInfo prop_info in prop_infos)
            {
                string str = prop_info.GetValue(instance, null).ToString();
                writer.WriteStartElement(prop_info.Name);
                writer.WriteString(str);
                writer.WriteEndElement();
            }
        }
    }
}
