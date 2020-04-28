using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using UniAuto.UniBCS.Entity;

namespace UniAuto.UniBCS.CSOT.SocketService
{
    public class SocketServiceUtility
    {
        public class Recipe
        {
            private static readonly string TRXID = "TrxID";
            private static readonly string LINENAME = "LineName";
            private static readonly string RECIPECHECKINFOLIST = "RecipeCheckInfoList";
            private static readonly string RECIPECHECKINFO = "RecipeCheckInfo";
            private static readonly string RECIPECHECKINFO_PARAMS = "Parameters";
            private static readonly string NOCHECK = "NoCheck";

            public static string ToXml(string trxid, string lineName, IList<RecipeCheckInfo> recipeCheckInfos, IList<string> noCheckList)
            {
                XmlDocument xml_doc = new XmlDocument();
                XmlNode body_node = xml_doc.CreateElement("BODY");
                XmlNode trxid_node = xml_doc.CreateElement(TRXID);
                trxid_node.InnerText = trxid;
                body_node.AppendChild(trxid_node);

                XmlNode linename_node = xml_doc.CreateElement(LINENAME);
                linename_node.InnerText = lineName;
                body_node.AppendChild(linename_node);

                RecipeCheckInfoList_ToXml(xml_doc, body_node, recipeCheckInfos);
                NoCheckList_ToXml(xml_doc, body_node, noCheckList);

                return body_node.InnerXml;
            }

            private static void RecipeCheckInfoList_ToXml(XmlDocument xmlDoc, XmlNode bodyNode, IList<RecipeCheckInfo> recipeCheckInfos)
            {
                XmlNode list_node = xmlDoc.CreateElement(RECIPECHECKINFOLIST);
                bodyNode.AppendChild(list_node);
                PropertyInfo[] properties = typeof(RecipeCheckInfo).GetProperties();
                foreach (RecipeCheckInfo rci in recipeCheckInfos)
                {
                    XmlNode rci_node = xmlDoc.CreateElement(RECIPECHECKINFO);
                    list_node.AppendChild(rci_node);

                    foreach (PropertyInfo prop in properties)
                    {
                        object prop_value = prop.GetValue(rci, null);
                        if (prop_value == null)
                            continue;
                        XmlNode prop_node = xmlDoc.CreateElement(prop.Name);
                        rci_node.AppendChild(prop_node);
                        if (prop.Name == RECIPECHECKINFO_PARAMS)
                        {
                            //IDictionary<string, string> Parameters
                            IDictionary<string, string> parameters = prop_value as IDictionary<string, string>;
                            foreach (string key in parameters.Keys)
                            {
                                XmlNode param = xmlDoc.CreateElement("param");
                                #region key
                                {
                                    XmlElement key_node = xmlDoc.CreateElement("key");
                                    key_node.InnerText = key;
                                    param.AppendChild(key_node);
                                }
                                #endregion
                                #region value
                                {
                                    XmlElement value_node = xmlDoc.CreateElement("value");
                                    value_node.InnerText = parameters[key];
                                    param.AppendChild(value_node);
                                }
                                #endregion
                                prop_node.AppendChild(param);
                            }
                        }
                        else if(prop.PropertyType == typeof(DateTime))
                        {
                            DateTime dt = (DateTime)(prop_value);
                            prop_node.InnerText = dt.ToString("yyyy-MM-dd HH:mm:ss.fff", System.Globalization.DateTimeFormatInfo.InvariantInfo);
                        }
                        else
                        {
                            prop_node.InnerText = prop_value.ToString();
                        }
                    }
                }
            }

            private static void NoCheckList_ToXml(XmlDocument xmlDoc, XmlNode bodyNode, IList<string> noCheckList)
            {
                XmlNode list_node = xmlDoc.CreateElement(NOCHECK);
                bodyNode.AppendChild(list_node);
                StringBuilder sb = new StringBuilder();
                if (noCheckList.Count > 0)
                {
                    foreach (string no_check in noCheckList)
                    {
                        sb.AppendFormat("{0},", no_check);
                    }
                    sb.Remove(sb.Length - 1, 1);
                }
                list_node.InnerText = sb.ToString();
            }

            public static void FromXml(XmlNode bodyNode, out string trxid, out string lineName, out List<RecipeCheckInfo> recipeCheckInfos, out List<string> noCheckList)
            {
                trxid = string.Empty;
                lineName = string.Empty;
                recipeCheckInfos = null;
                noCheckList = null;

                trxid = bodyNode.SelectSingleNode(TRXID).InnerText;
                lineName = bodyNode.SelectSingleNode(LINENAME).InnerText;
                RecipeCheckInfoList_FromXml(bodyNode, out recipeCheckInfos);
                NoCheckList_FromXml(bodyNode, out noCheckList);
            }

            private static void RecipeCheckInfoList_FromXml(XmlNode bodyNode, out List<RecipeCheckInfo> recipeCheckInfos)
            {
                recipeCheckInfos = new List<RecipeCheckInfo>();
                XmlNodeList list_node = bodyNode.SelectNodes(string.Format("{0}/{1}", RECIPECHECKINFOLIST, RECIPECHECKINFO));
                PropertyInfo[] properties = typeof(RecipeCheckInfo).GetProperties();
                Dictionary<string, PropertyInfo> dic_properties = new Dictionary<string, PropertyInfo>();
                foreach (PropertyInfo prop in properties)
                {
                    dic_properties.Add(prop.Name, prop);
                }
                foreach (XmlNode rci_node in list_node)
                {
                    RecipeCheckInfo rci = new RecipeCheckInfo();
                    recipeCheckInfos.Add(rci);
                    foreach (XmlNode prop_node in rci_node.ChildNodes)
                    {
                        if (prop_node.Name == RECIPECHECKINFO_PARAMS)
                        {
                            foreach (XmlNode param_node in prop_node.ChildNodes)
                            {
                                string key = param_node["key"].InnerText;
                                string value = param_node["value"].InnerText;
                                rci.Parameters.Add(key, value);
                            }
                        }
                        else
                        {
                            PropertyInfo prop = dic_properties[prop_node.Name];
                            if(prop.PropertyType == typeof(eRecipeCheckResult))
                            {
                                eRecipeCheckResult res = eRecipeCheckResult.NG;
                                Enum.TryParse<eRecipeCheckResult>(prop_node.InnerText, out res);
                                prop.SetValue(rci, res, null);
                            }
                            else if (prop.PropertyType == typeof(int))
                            {
                                int i = 0;
                                int.TryParse(prop_node.InnerText, out i);
                                prop.SetValue(rci, i, null);
                            }
                            else if (prop.PropertyType == typeof(DateTime))
                            {
                                DateTime dt = DateTime.MinValue;
                                DateTime.TryParse(prop_node.InnerText, out dt);
                                prop.SetValue(rci, dt, null);
                            }
                            else if (prop.PropertyType == typeof(bool))
                            {
                                bool b = (string.Compare(prop_node.InnerText, bool.TrueString, true) == 0);
                                prop.SetValue(rci, b, null);
                            }
                            else if(prop.PropertyType == typeof(string))
                            {
                                prop.SetValue(rci, prop_node.InnerText, null);
                            }
                        }
                    }
                }
            }

            private static void NoCheckList_FromXml(XmlNode bodyNode, out List<string> noCheckList)
            {
                noCheckList = new List<string>();
                XmlNode list_node = bodyNode.SelectSingleNode(NOCHECK);
                string[] strs = list_node.InnerText.Split(',');
                foreach (string str in strs)
                {
                    noCheckList.Add(str);
                }
            }
        }
    }
}
