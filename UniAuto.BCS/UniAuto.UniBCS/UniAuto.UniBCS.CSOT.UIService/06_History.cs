using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml;
using UniAuto.UniBCS.Core;
using UniAuto.UniBCS.Core.Generic;
using UniAuto.UniBCS.Core.Message;
using UniAuto.UniBCS.Log;
using UniAuto.UniBCS.OpiSpec;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.EntityManager;
using UniAuto.UniBCS.PLCAgent.PLC;
using System.IO;
using System.Collections;

namespace UniAuto.UniBCS.CSOT.UIService
{
    public partial class UIService
    {
        #region Status
        /// <summary>
        /// OPI MessageSet: Process Data Report Request
        /// </summary>
        /// <param name="xmlDoc"></param>
        public void OPI_ProcessDataHistoryRequest(XmlDocument xmlDoc)
        {
            IServerAgent agent = GetServerAgent();
            ProcessDataHistoryRequest command = Spec.XMLtoMessage(xmlDoc) as ProcessDataHistoryRequest;
            XmlDocument xml_doc = agent.GetTransactionFormat("ProcessDataHistoryReply") as XmlDocument;
            ProcessDataHistoryReply reply = Spec.XMLtoMessage(xml_doc) as ProcessDataHistoryReply;

            try
            {
                reply.BODY.LINENAME = command.BODY.LINENAME;
                reply.BODY.EQUIPMENTNO = command.BODY.EQUIPMENTNO;
                reply.BODY.JOBSEQNO = command.BODY.JOBSEQNO;
                reply.BODY.CSTSEQNO = command.BODY.CSTSEQNO;
                reply.BODY.JOBID = command.BODY.JOBID;
                reply.BODY.TRXID = command.BODY.TRXID;
                reply.BODY.FILENAME = command.BODY.FILENAME;
                //TODO: DATALIST

                Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={4}] [BCS <- OPI][{0}] {1} {2}, FILENAME[{3}]", command.HEADER.TRANSACTIONID, command.GetType().Name, "OK", command.BODY.FILENAME, command.BODY.EQUIPMENTNO));

                reply.BODY.DATALIST.Clear();

                IList<string> lsData = ObjectManager.ProcessDataManager.ProcessDataValues(command.BODY.FILENAME);
                bool execute = false;

                if (lsData == null)
                {
                    reply.RETURN.RETURNCODE = "0010540";
                    reply.RETURN.RETURNMESSAGE = "Can't get this FILENAME";
                }
                else if (lsData.Count == 0)
                {
                    reply.RETURN.RETURNCODE = "0010541";
                    reply.RETURN.RETURNMESSAGE = "Get NoData";
                    execute = true;
                }
                else
                {
                    execute = true;
                    ProcessDataHistoryReply.DATAc c;
                    string[] splitc;

                    foreach (var d in lsData)
                    {
                        if (!string.IsNullOrEmpty(d) && d.Contains("="))
                        {
                            c = new ProcessDataHistoryReply.DATAc();
                            splitc = d.Trim().Split(new string[] { "=" }, StringSplitOptions.None);
                            c.NAME = splitc[0].Trim();
                            c.VALUE = splitc[1].Trim();
                            reply.BODY.DATALIST.Add(c);
                        }
                    }
                }

                xMessage msg = SendReplyToOPI(command, reply);

                Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={3}] [BCS -> OPI][{0}] {1} {2}", reply.HEADER.TRANSACTIONID, reply.GetType().Name, execute ? "OK" : "NG", command.BODY.EQUIPMENTNO));

            }
            catch (Exception ex)
            {
                //NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService catch message({2}) exception : {3} ",
                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME, ex.ToString()));
            }
        }
        #endregion

        #region Command

        #endregion
    }
}
