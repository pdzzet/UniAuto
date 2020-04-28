using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniAuto.UniBCS.Core.Generic;
using System.Xml;
using System.Reflection;
using UniAuto.UniBCS.PLCAgent.PLC;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.EntityManager;

namespace UniAuto.UniBCS.CSOT.LineSpecialService
{
    class EventRule
    {
        string _name;
        List<string> _items=new List<string>();

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        public List<string> Items
        {
            get { return _items; }
           internal set { _items = value; }
        }
    }

    class LineRule
    {
        string _lineType;

        public string LineType
        {
            get { return _lineType; }
            set { _lineType = value; }
        }

        Dictionary<string, EventRule> _eventRules = new Dictionary<string, EventRule>();

       public  Dictionary<string, EventRule> EventRules
        {
            get { return _eventRules; }
            internal set { _eventRules = value; }
        }
    }
  
    public class EqToEqService:AbstractService
    {
        XmlDocument doc;
        string _formatFile;

        Dictionary<string, LineRule> _lineRules=new Dictionary<string, LineRule>();
        public string FormatFile
        {
            get { return _formatFile; }
            set { _formatFile = value; }
        }
        public override bool Init()
        {
            LoadFormatFile();
            return true;
        }
        #region Init Method
        private void LoadFormatFile()
        {
            doc = new XmlDocument();
            doc.Load(_formatFile);
            XmlNode root = doc.DocumentElement;
            foreach (XmlNode child in root.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.Element)
                {
                    LineRule rule = null;
                    string lineType = child.Attributes["type"].Value;
                    if (!_lineRules.ContainsKey(lineType))
                    {
                        rule = new LineRule();
                        rule.LineType = lineType;
                        _lineRules.Add(lineType, rule);
                    }
                    rule = _lineRules[lineType];

                    RetrieveEvent(child, rule);
                }
            }
        }

        private void RetrieveEvent(XmlNode parentNode,LineRule rule)
        {
            if (parentNode.HasChildNodes)
            {
                foreach (XmlNode node in parentNode.ChildNodes)
                {
                    if (node.NodeType == XmlNodeType.Element)
                    {
                        EventRule eventRule = null;
                        string name = node.Attributes["name"].Value;
                        if (!rule.EventRules.ContainsKey(name))
                        {
                            eventRule = new EventRule();
                            eventRule.Name = name;
                            rule.EventRules.Add(name,eventRule);
                        }
                        eventRule = rule.EventRules[name];
                        RetrieveItem(node, eventRule);
                    }
                }
            }
        }

        private void RetrieveItem(XmlNode parentNode, EventRule rule)
        {
            if (parentNode.HasChildNodes)
            {
                foreach (XmlNode node in parentNode.ChildNodes)
                {
                    if (node.NodeType == XmlNodeType.Element)
                    {
                        foreach(XmlNode childNode in node.ChildNodes)
                        {
                            if(childNode.NodeType==XmlNodeType.Element)
                            {
                                rule.Items.Add(childNode.InnerText);
                            }
                        }
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// Bit + Word EQ to EQ Trx
        /// </summary>
        /// <param name="inputData"></param>
        public void EQtoEQInterlockEvent(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger == true) return;
                string eqpNo = inputData.Metadata.NodeNo;
                if (inputData.EventGroups[0].Events.Count >= 1)
                {
                    if (inputData.EventGroups[0].Events[1].Items[0].Value == "0")
                    {
                        LogInfo(MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] {2} Bit Off).", eqpNo, inputData.EventGroups[0].Events[1].Name, inputData.TrackKey));
                        return;
                    }

                    Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                    if (eqp == null) throw new Exception(string.Format("Can't find Equipment No =[{0}) in EquipmentEntity!", eqpNo));
                    Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                    if (line == null) throw new Exception(string.Format("Can't find Line =[{0}) in LineEntity!", eqp.Data.LINEID));
                    string lineType = line.Data.LINETYPE;
                    if (!_lineRules.ContainsKey(lineType))
                    {
                        LogWarn(MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] line Typ =[{2}) EQ to EQ Log Format Not Found.", eqpNo, inputData.TrackKey, lineType));
                        return;
                    }
                    LineRule lineRule = _lineRules[lineType];

                    string eventName = inputData.EventGroups[0].Events[1].Name;

                    if (!lineRule.EventRules.ContainsKey(eventName))
                    {
                        LogWarn(MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] Not Found Event =[{2}).", eqpNo, inputData.TrackKey, eventName));
                        return;
                    }
                    EventRule rule = lineRule.EventRules[eventName];
                    string log = MakeLogString(rule, inputData);
                    LogInfo(MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] {2} Bit ON Word Data =[{3}).", eqpNo,eventName, inputData.TrackKey, log));
                }
                else
                {
                    LogWarn(MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] Trx Event Count < 2.", eqpNo, inputData.TrackKey));
                    return;
                }
            }
            catch (System.Exception ex)
            {
                LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }

        }

        public void EQtoEQInterlockEventReply(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger == true) return;
                string eqpNo = inputData.Metadata.NodeNo;
                if (inputData.EventGroups[0].Events.Count >= 1)
                {
                    if (inputData.EventGroups[0].Events[1].Items[0].Value == "0")
                    {
                        LogInfo(MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] {2} Bit Off).", eqpNo, inputData.EventGroups[0].Events[1].Name, inputData.TrackKey));
                        return;
                    }
                    Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                    if (eqp == null) throw new Exception(string.Format("Can't find Equipment No =[{0}) in EquipmentEntity!", eqpNo));
                    Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                    if (line == null) throw new Exception(string.Format("Can't find Line =[{0}) in LineEntity!", eqp.Data.LINEID));
                    string lineType = line.Data.LINETYPE;
                    if (!_lineRules.ContainsKey(lineType))
                    {
                        LogWarn(MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] line Typ =[{2}) EQ to EQ Log Format Not Found.", eqpNo, inputData.TrackKey, lineType));
                        return;
                    }
                    LineRule lineRule = _lineRules[lineType];

                    string eventName = inputData.EventGroups[0].Events[0].Name;

                    if (!lineRule.EventRules.ContainsKey(eventName))
                    {
                        LogWarn(MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] Not Found Event =[{2}).", eqpNo, inputData.TrackKey, eventName));
                        return;
                    }
                    EventRule rule = lineRule.EventRules[eventName];
                    string log = MakeLogString(rule, inputData);
                    LogInfo(MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] {2} Bit ON Word Data =[{3}).", eventName, log));
                }
                else
                {
                    LogWarn(MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] Trx Event Count < 2.", eqpNo, inputData.TrackKey));
                    return;
                }
            }
            catch (System.Exception ex)
            {
                LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }

        }

        public void EQtoEQInterlockBit(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger == true)
                {
                    return;
                }
                string eqpNo = inputData.Metadata.NodeNo;
                if (inputData.EventGroups[0].Events.Count >= 1)
                {
                    //if (inputData.EventGroups[0].Events[0].Items[0].Value == "0")
                    //    return;
                    string eventName = inputData.EventGroups[0].Events[0].Name;
                    //string log = MakeLogString(inputData);
                    LogInfo(MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] {2} Bit [{3}] .", eqpNo, eventName, inputData.TrackKey, inputData.EventGroups[0].Events[0].Items[0].Value));
                }
                else
                {
                    LogWarn(MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] Trx Event Count < 2.", eqpNo, inputData.TrackKey));
                    return;
                }
            }
            catch (System.Exception ex)
            {
                LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }

        }

        public void EQtoEQInterlockBitReply(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger == true) return;
                string eqpNo = inputData.Metadata.NodeNo;
                if (inputData.EventGroups[0].Events.Count >= 1)
                {
                //    if (inputData.EventGroups[0].Events[0].Items[0].Value == "0")
                //        return;
                    string eventName = inputData.EventGroups[0].Events[0].Name;
                    LogInfo(MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] {2} Bit [{3}] .", eqpNo, eventName, inputData.TrackKey, inputData.EventGroups[0].Events[0].Items[0].Value));
                }
                else
                {
                    LogWarn(MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] Trx Event Count < 2.", eqpNo, inputData.TrackKey));
                    return;
                }
            }
            catch (System.Exception ex)
            {
                LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void EQtoEQInterlockWord(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger == true) return;
                string eqpNo = inputData.Metadata.NodeNo;
                //Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                //if (eqp == null) throw new Exception(string.Format("Can't find Equipment No =[{0}) in EquipmentEntity!", eqpNo));
                //Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                //if (line == null) throw new Exception(string.Format("Can't find Line =[{0}) in LineEntity!", eqp.Data.LINEID));
                //string lineType = line.Data.LINETYPE;
                //if (!_lineRules.ContainsKey(lineType))
                //{
                //    LogWarn(MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] line Type =[{2}) EQ to EQ Log Format Not Found.", eqpNo, inputData.TrackKey, lineType));
                //    return;
                //}
                //LineRule lineRule = _lineRules[lineType];
                string eventName = inputData.EventGroups[0].Events[0].Name;

                //if (!lineRule.EventRules.ContainsKey(eventName))
                //{
                //    LogWarn(MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] Not Found Event =[{2}).", eqpNo, inputData.TrackKey, eventName));
                //    return;
                //}
                //EventRule rule = lineRule.EventRules[eventName];
                string log = MakeLogString(inputData);
                LogInfo(MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] EventName=[{2}] ,Word Data =[{3}].", eqpNo, inputData.TrackKey,eventName, log));

            }
            catch (System.Exception ex)
            {
                LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }

        }

        private string MakeLogString(EventRule rule, Trx inputData)
        {
            StringBuilder sb = new StringBuilder();

            foreach (string item in rule.Items)
            {
                string value="";
                
                try
                {
                    value =inputData.EventGroups[0].Events[0].Items[item].Value;
                }
                catch
                {
                }
                sb.AppendFormat("{0}=[{1}],", item, value);
            }
            return sb.ToString(0, sb.Length - 1);
        }

        private string MakeLogString(Trx inputData)
        {
            StringBuilder sb = new StringBuilder();

            foreach (Item item in inputData.EventGroups[0].Events[0].Items.AllValues)
            {
                string value = item.Value;
                sb.AppendFormat("{0}=[{1}],", item.Name, value);
            }
            return sb.ToString(0, sb.Length - 1);
        }
    }
}
