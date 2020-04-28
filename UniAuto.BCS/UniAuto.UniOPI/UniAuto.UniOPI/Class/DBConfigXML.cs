using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Windows.Forms;
namespace UniOPI
{
    public class csDBConfigXML
    {
        public Dictionary<string, csXmlShop> dic_Setting = new Dictionary<string, csXmlShop>();

        public csXmlLine getLineData(string ServerName)
        {
            csXmlLine retureData = new csXmlLine();
            try
            {
                foreach (csXmlShop shop in dic_Setting.Values)
                {
                    foreach (csXmlLineType lineType in shop.dic_LineType.Values)
                    {
                        foreach (csXmlLine line in lineType.dic_Line.Values)
                        {
                            if (line.LineID == ServerName)
                                return line;
                        }
                    }
                }
                return retureData;
            }
            catch 
            {                
                return null;
            }
        }

        public csDBConfigXML(string filePath, out string errMsg)
        {
            errMsg = string.Empty;

            try
            {
                if (!File.Exists(filePath))
                {
                    errMsg = string.Format("File is not exists. Path [{0}]", filePath);
                    return;
                }


                XmlDocument xmDoc = new XmlDocument();
                xmDoc.Load(filePath);
                XmlNode xnSeting = xmDoc.SelectSingleNode("LineSetting");
                foreach (XmlNode xnShop in xnSeting.ChildNodes)
                {
                    csXmlShop Shop = new csXmlShop();

                    foreach (XmlNode xnLineType in xnShop.ChildNodes)
                    {
                        csXmlLineType lineType = new csXmlLineType();
                        if (xnLineType.Attributes == null)
                        {
                            NLogManager.Logger.LogErrorWrite(this.GetType().Name, xnLineType.OuterXml.ToString(), "");
                            continue;
                        }
                        else
                        lineType.Name = xnLineType.Attributes["Name"].Value.ToString();
                        //lineType.Type = xnLineType.Attributes["LineType"].Value.ToString();

                        foreach (XmlNode xnLine in xnLineType.ChildNodes)
                        {
                            if (xnLine.Attributes == null)
                            {
                                NLogManager.Logger.LogErrorWrite(this.GetType().Name, xnLine.OuterXml.ToString(), "");
                            }
                            else
                            {
                                csXmlLine mLine = new csXmlLine();
                                mLine.LineID = xnLine.Attributes["LineID"].Value.ToString();
                                mLine.DBConn = xnLine.Attributes["DBConn"].Value.ToString();
                                mLine.SocketIP = xnLine.Attributes["SocketIP"].Value.ToString();
                                mLine.SocketPort = xnLine.Attributes["SocketPort"].Value.ToString();
                            lineType.dic_Line.Add(mLine.LineID, mLine);
                            }
                         }

                        Shop.dic_LineType.Add(lineType.Name, lineType);
                    }

                    dic_Setting.Add(xnShop.Attributes["type"].Value.ToString(), Shop);
                }
            }
            catch (Exception ex)
            {
                errMsg = ex.ToString();
            }
        }
    }
    public class csXmlShop
    {
        public Dictionary<string, csXmlLineType> dic_LineType = new Dictionary<string, csXmlLineType>();
    }

    public class csXmlLineType
    {
        public string Name = "";
        public string Type = "";
        public Dictionary<string, csXmlLine> dic_Line = new Dictionary<string, csXmlLine>();
    }
    public class csXmlLine
    {
        public string LineID = "";
        public string SocketIP = "";
        public string SocketPort = "";
        public string DBConn = "";
    }
}
