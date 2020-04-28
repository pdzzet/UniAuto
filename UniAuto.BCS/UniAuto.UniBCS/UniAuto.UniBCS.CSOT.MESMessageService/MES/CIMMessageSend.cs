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
using UniAuto.UniBCS.MesSpec;
using System.Collections;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.EntityManager;
using UniAuto.UniBCS.MISC;
using UniAuto.UniBCS.CSOT.MESMessageService;

namespace UniAuto.UniBCS.CSOT.MESMessageService
{
    public partial class MESService
    {
        /// <summary>
        /// 6.41.	CIMMessageSend
        /// </summary>
        /// <param name="doc"></param>
        public void MES_CIMMessageSend(XmlDocument doc)
        {
            try
            {
                Line line = null; 
                string trxID = GetTransactionID(doc);
                string lineName = GetLineName(doc);
                string err = string.Empty;
                string machineName = string.Empty;

                XmlNode body = GetMESBodyNode(doc);
                machineName = body[keyHost.MACHINENAME].InnerText;

                string cimMessage = body[keyHost.CIMMESSAGE].InnerText;
                string userName = body[keyHost.USERNAME].InnerText;
                #region Check Line
                line = ObjectManager.LineManager.GetLine(lineName);
                if (line == null)
                {
                    err = string.Format("CIM Message Transfer Error: Cannot found Line Object, Line Name =[{0}].", lineName);
                    return;
                }
                #endregion

                #region [Just For Unit Test]
                //if (userName == "Clear")
                //{
                //    Equipment eqp = ObjectManager.EquipmentManager.GetEQPByID(machineName);
                //    Invoke(eServiceName.CIMMessageService, "CIMMessageClearCommand", new object[] { eqp.Data.NODENO, "21", userName });
                //    return;
                //}
                #endregion

                #region Check MachineName is null or not and send
                if (machineName == string.Empty)
                {
                    //Send to all local machine
                    IList<Equipment> equipments = ObjectManager.EquipmentManager.GetEQPsByLine(line.Data.LINEID); 
                    if(equipments!=null)
                    {
                        foreach(Equipment eqp in equipments)
                        {
                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                 string.Format("[Equipment=[{0}]] [BCS <- MES] Invoke CIMMessageService CIMMessageSetCommand.", eqp.Data.NODENO));
                            if (line.Data.FABTYPE == eFabType.CELL.ToString())//20161101 sy add By CELL 是不同IO 
                                Invoke(eServiceName.CIMMessageService, "CIMMessageSetCommandForCELL", new object[] { trxID, eqp.Data.NODENO, cimMessage, userName, "0" });
                            else
                                Invoke(eServiceName.CIMMessageService, "CIMMessageSetCommand", new object[] { trxID, eqp.Data.NODENO, cimMessage, userName });
                            Thread.Sleep(5);
                        }
                    }
                }
                else
                {
                    Equipment eqp = ObjectManager.EquipmentManager.GetEQPByID(machineName);

                    #region Watson Add 20150313 For PMT Line For MES Send Two Type MachineName
                    if ((line.Data.LINETYPE == eLineType.CELL.CBPMT) && (line.Data.LINEID.Contains(keyCELLPMTLINE.CBPTI)))
                    {
                        if (machineName == ParameterManager[keyCELLPTIParameter.CELL_PTI_NODEID].GetString())
                        {
                            eqp = ObjectManager.EquipmentManager.GetEQP("L2");
                            if (eqp != null)
                            {
                                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                    string.Format("[Equipment=[{0}]] [BCS <- MES] Invoke CIMMessageService CIMMessageSetCommand.", eqp.Data.NODENO));
                                Invoke(eServiceName.CIMMessageService, "CIMMessageSetCommandForCELL", new object[] { trxID, eqp.Data.NODENO, cimMessage, userName, "0" });
                            }

                        }
                    }
                    #endregion
                    else
                    {
                        if (eqp != null)
                        {
                            if (line.Data.FABTYPE == eFabType.CELL.ToString())
                            {
                                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                string.Format("[Equipment=[{0}]] [BCS <- MES] Invoke CIMMessageService CIMMessageSetCommand.", eqp.Data.NODENO));
                                Invoke(eServiceName.CIMMessageService, "CIMMessageSetCommandForCELL", new object[] { trxID, eqp.Data.NODENO, cimMessage, userName,"0" });


                            }
                            else
                            {
                                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                    string.Format("[Equipment=[{0}]] [BCS <- MES] Invoke CIMMessageService CIMMessageSetCommand.", eqp.Data.NODENO));
                                Invoke(eServiceName.CIMMessageService, "CIMMessageSetCommand", new object[] { trxID, eqp.Data.NODENO, cimMessage, userName });
                            }
                        }
                        else
                        {
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("MES SEND CIMMessageSetCommand ERROR. NO MACHINENAME=[{0}]", machineName));
                        }
                    }
                }
                #endregion
            }
            catch (System.Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

    }
}
