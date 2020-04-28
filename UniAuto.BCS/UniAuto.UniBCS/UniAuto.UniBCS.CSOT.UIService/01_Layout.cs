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
            /// OPI MessageSet: OPI send to BC All Data Update
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_AllDataUpdateRequest(XmlDocument xmlDoc)
            {
                  IServerAgent agent = GetServerAgent();
                  AllDataUpdateRequest command = Spec.XMLtoMessage(xmlDoc) as AllDataUpdateRequest;
                  XmlDocument xml_doc = agent.GetTransactionFormat("AllDataUpdateReply") as XmlDocument;
                  AllDataUpdateReply reply = Spec.XMLtoMessage(xml_doc) as AllDataUpdateReply;

                  try
                  {
                        Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI.",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME));

                        reply.BODY.LINENAME = command.BODY.LINENAME;

                        //Line line = ObjectManager.LineManager.GetLine(command.BODY.LINENAME);
                        IList<Line> lines = ObjectManager.LineManager.GetLines();

                        if (lines == null)
                        {
                            reply.RETURN.RETURNCODE = "0010010";
                            reply.RETURN.RETURNMESSAGE = string.Format("Can't find Line[{0}] in LineEntity", command.BODY.LINENAME);

                            Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't find Line({0}) in Line Entity.",
                                command.BODY.LINENAME, command.HEADER.TRANSACTIONID));
                        }
                        else
                        {
                            //某些特殊線會有同ServerName，但是LineID不同的狀況，ServerName相同的不同Line，OPI視為同一條線
                            //不同LineID的線應該大部分的狀態會相同。所以取其中一條來上報即可
                            eFabType fabType;
                            Enum.TryParse<eFabType>(lines[0].Data.FABTYPE, out fabType);

                            reply.BODY.LINETYPE = lines[0].Data.LINETYPE;
                            reply.BODY.FACTORYTYPE = lines[0].Data.FABTYPE;
                            //reply.BODY.MESCONTROLSTATENAM = line.File.HostMode.ToString();
                            //MESCONTROLSTATENAM，不同LineID可能有不同的狀態
                            foreach (Line line in lines)
                            {
                                reply.BODY.LINEIDLIST.Add(new AllDataUpdateReply.LINEc() { LINEID = line.Data.LINEID, MESCONTROLSTATENAM = line.File.HostMode.ToString() });
                            }
                            reply.BODY.INDEXEROPERATIONMODE = ((int)lines[0].File.IndexOperMode).ToString() == "" ? "0" : ((int)lines[0].File.IndexOperMode).ToString();
                            reply.BODY.PLCSTATUS = lines[0].File.PLCStatus;
                            reply.BODY.LINEOPERMODE = lines[0].File.LineOperMode;
                            reply.BODY.SHORTCUTMODE = lines[0].File.CFShortCutMode.ToString();
                            reply.BODY.LINESTATUSNAME = lines[0].File.Status.ToString();
                            reply.BODY.COOLRUNSETCOUNT = lines[0].File.CoolRunSetCount.ToString();
                            reply.BODY.COOLRUNREMAINCOUNT = lines[0].File.CoolRunRemainCount.ToString();
                            reply.BODY.ROBOT_FETCH_SEQ_MODE = lines[0].File.RobotFetchSeqMode == null ? "0" : lines[0].File.RobotFetchSeqMode.ToString();   //add by bruce 2015/7/17 opi add new transation Item
                            Trx statusTrx = Invoke(eAgentName.PLCAgent, "GetTransactionFormat", new object[] { "L1_BCStatus" }) as Trx;
                            if (statusTrx != null) statusTrx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { "L1_BCStatus", true }) as Trx;
                            //reply.BODY.BCSTATUS = statusTrx == null ? "0" : statusTrx.EventGroups[0].Events[0].Items[0].Value;
                            //reply.BODY.EQUIPMENTRUNMODE = lines[0].File.UPKEquipmentRunMode.ToString();

                            foreach (Equipment node in ObjectManager.EquipmentManager.GetEQPs())
                            {
                                //if (node.Data.LINEID == line.Data.LINEID)
                                //{
                                AllDataUpdateReply.EQUIPMENTc eqp = new AllDataUpdateReply.EQUIPMENTc();
                                eqp.EQUIPMENTNO = node.Data.NODENO;

                                ////CBPMT Line的Loader(L2)有兩個EquipmentRunMode，需要特殊處理
                                //if ((node.Data.LINEID.Contains(keyCELLPMTLINE.CBPMI) || node.Data.LINEID.Contains(keyCELLPMTLINE.CBPTI))
                                //    && node.Data.NODENO == "L2")
                                //{
                                //      Line line1 = ObjectManager.LineManager.GetLine("CBPMI500");
                                //      Line line2 = ObjectManager.LineManager.GetLine("CBPTI100");

                                //      eqp.EQUIPMENTRUNMODE = line1.File.CellLineOperMode;
                                //      eqp.EQUIPMENTRUNMODE2 = line2.File.CellLineOperMode;
                                //}
                                //else
                                //{
                                //      eqp.EQUIPMENTRUNMODE = node.File.EquipmentRunMode;
                                //}

                                eqp.EQUIPMENTRUNMODE = node.File.EquipmentRunMode;

                                //string aliveName = string.Format("{0}_EquipmentAlive", node.Data.NODENO);
                                //Trx aliveTrx = Invoke(eAgentName.PLCAgent, "GetTransactionFormat", new object[] { aliveName }) as Trx;
                                //if (aliveTrx != null) aliveTrx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { aliveName,true }) as Trx;
                                //eqp.EQUIPMENTALIVE = aliveTrx == null ? "0" : aliveTrx.EventGroups[0].Events[0].Items[0].Value;
                                //20150130 modify by edison:昌爷要求，取EQENTITY的AliveTimeout报给OPI
                                eqp.EQUIPMENTALIVE = node.File.AliveTimeout == true ? "0" : "1";
                                eqp.CURRENTSTATUS = ((int)node.File.Status).ToString() == "" ? "0" : ((int)node.File.Status).ToString();
                                eqp.CIMMODE = ((int)node.File.CIMMode).ToString() == "" ? "0" : ((int)node.File.CIMMode).ToString();
                                eqp.HSMSSTATUS = node.HsmsConnStatus;
                                eqp.HSMSCONTROLMODE = node.File.HSMSControlMode;
                                eqp.CSTOPERMODE = node.File.CSTOperationMode.ToString();
                                eqp.UPSTREAMINLINEMODE = node.File.UpstreamInlineMode.ToString();
                                eqp.DOWNSTREAMINLINEMODE = node.File.DownstreamInlineMode.ToString();
                                eqp.AUTORECIPECHANGEMODE = node.File.AutoRecipeChangeMode.ToString();
                                eqp.PARTIALFULLMODE = ((int)node.File.PartialFullMode).ToString();
                                eqp.BYPASSMODE = node.File.BypassMode.ToString();
                                eqp.CURRENTRECIPEID = node.File.CurrentRecipeID.Trim();
                                eqp.TFTJOBCNT = node.File.TotalTFTJobCount.ToString();
                                eqp.CFJOBCNT = node.File.TotalCFProductJobCount.ToString();
                                eqp.DMYJOBCNT = node.File.TotalDummyJobCount.ToString();
                                eqp.THROUGHDMYJOBCNT = node.File.ThroughDummyJobCount.ToString();
                                eqp.THICKNESSDMYJOBCNT = node.File.ThicknessDummyJobCount.ToString();
                                eqp.UNASSEMBLEDTFTDMYJOBCNT = node.File.TotalUnassembledTFTJobCount.ToString();//sy add 20160826
                                eqp.ITODMYJOBCNT = node.File.TotalITODummyJobCount.ToString();//sy add 20160826
                                eqp.NIPDMYJOBCNT = node.File.TotalNIPDummyJobCount.ToString();//sy add 20160826
                                eqp.METALONEDMYJOBCNT = node.File.TotalMetalOneDummyJobCount.ToString();//sy add 20160826
                                eqp.UVMASKJOBCNT = node.File.UVMASKJobCount.ToString();
                                eqp.TURNTABLEMODE = ((int)node.File.TurnTableMode).ToString() == "" ? "0" : ((int)node.File.TurnTableMode).ToString();
                                eqp.LOCALALARMSTATUS = node.File.LocalAlarmStatus.ToString();
                                eqp.CASSETTEQTIME = node.File.CassetteonPortQTime.ToString();
                                eqp.INSPECTIONIDLETIME = node.File.InspectionIdleTime.ToString();
                                eqp.EQPOPERATIONMODE = ((int)node.File.EquipmentOperationMode).ToString() == "" ? "0" : ((int)node.File.EquipmentOperationMode).ToString();
                                eqp.BYPASSINSP01MODE = ((int)node.File.BypassInspectionEquipment01Mode).ToString() == "" ? "0" : ((int)node.File.BypassInspectionEquipment01Mode).ToString();
                                eqp.BYPASSINSP02MODE = ((int)node.File.BypassInspectionEquipment02Mode).ToString() == "" ? "0" : ((int)node.File.BypassInspectionEquipment02Mode).ToString();
                                eqp.HIGHCVMODE = ((int)node.File.HighCVMode).ToString() == "" ? "0" : ((int)node.File.HighCVMode).ToString();
                                eqp.NEXTLINEBCSTATUS = ((int)node.File.NextLineBCStatus).ToString() == "" ? "0" : ((int)node.File.NextLineBCStatus).ToString();
                                eqp.JOBDATACHECKMODE = ((int)node.File.JobDataCheckMode).ToString() == "" ? "0" : ((int)node.File.JobDataCheckMode).ToString();
                                eqp.COAVERSIONCHECKMODE = ((int)node.File.COAVersionCheckMode).ToString() == "" ? "0" : ((int)node.File.COAVersionCheckMode).ToString();
                                eqp.JOBDUPLICATECHECKMODE = ((int)node.File.JobDuplicateCheckMode).ToString() == "" ? "0" : ((int)node.File.JobDuplicateCheckMode).ToString();
                                eqp.PRODUCTIDCHECKMODE = ((int)node.File.ProductIDCheckMode).ToString() == "" ? "0" : ((int)node.File.ProductIDCheckMode).ToString();
                                eqp.GROUPINDEXCHECKMODE = ((int)node.File.GroupIndexCheckMode).ToString() == "" ? "0" : ((int)node.File.GroupIndexCheckMode).ToString();
                                eqp.RECIPEIDCHECKMODE = ((int)node.File.RecipeIDCheckMode).ToString() == "" ? "0" : ((int)node.File.RecipeIDCheckMode).ToString();
                                eqp.PRODUCTTYPECHECKMODE = ((int)node.File.ProductTypeCheckMode).ToString() == "" ? "0" : ((int)node.File.ProductTypeCheckMode).ToString();
                                //eqp.LOADEROPERATIONMODE_ATS = ((int)node.File.ATSLoaderOperMode).ToString() == "" ? "0" : ((int)node.File.ATSLoaderOperMode).ToString();
                                eqp.MATERIALSTATUS = "0";
                                eqp.LASTGLASSID = node.File.FinalReceiveGlassID;
                                eqp.LASTRECIVETIME = node.File.FinalReceiveGlassTime;
                                eqp.TIMEOUTFLAG = node.File.TactTimeOut ? "1" : "0";

                                if (node.Data.NODENO == "L3")
                                {
                                    switch (lines[0].Data.LINETYPE)
                                    {
                                        case eLineType.ARRAY.WEI_DMS:
                                        case eLineType.ARRAY.WET_DMS:
                                        case eLineType.ARRAY.STR_DMS:
                                        case eLineType.ARRAY.CLN_DMS:  //add by qiumin 20171222
                                            eqp.MATERIALSTATUS = lines[0].File.Array_Material_Change ? "1" : "0";
                                            break;
                                    }
                                }

                                #region Sampling Side

                                AllDataUpdateReply.SAMPLINGSIDEc sampling1 = new AllDataUpdateReply.SAMPLINGSIDEc();
                                sampling1.ITEMNAME = "VCD01";
                                sampling1.SIDESTATUS = node.File.VCD01.ToString();
                                eqp.SAMPLINGSIDELIST.Add(sampling1);
                                AllDataUpdateReply.SAMPLINGSIDEc sampling2 = new AllDataUpdateReply.SAMPLINGSIDEc();
                                sampling2.ITEMNAME = "VCD02";
                                sampling2.SIDESTATUS = node.File.VCD02.ToString();
                                eqp.SAMPLINGSIDELIST.Add(sampling2);
                                AllDataUpdateReply.SAMPLINGSIDEc sampling4 = new AllDataUpdateReply.SAMPLINGSIDEc();
                                sampling4.ITEMNAME = "CP01";
                                sampling4.SIDESTATUS = node.File.CP01.ToString();
                                eqp.SAMPLINGSIDELIST.Add(sampling4);
                                AllDataUpdateReply.SAMPLINGSIDEc sampling5 = new AllDataUpdateReply.SAMPLINGSIDEc();
                                sampling5.ITEMNAME = "CP02";
                                sampling5.SIDESTATUS = node.File.CP02.ToString();
                                eqp.SAMPLINGSIDELIST.Add(sampling5);
                                AllDataUpdateReply.SAMPLINGSIDEc sampling6 = new AllDataUpdateReply.SAMPLINGSIDEc();
                                sampling6.ITEMNAME = "HP01";
                                sampling6.SIDESTATUS = node.File.HP01.ToString();
                                eqp.SAMPLINGSIDELIST.Add(sampling6);
                                AllDataUpdateReply.SAMPLINGSIDEc sampling7 = new AllDataUpdateReply.SAMPLINGSIDEc();
                                sampling7.ITEMNAME = "HP02";
                                sampling7.SIDESTATUS = node.File.HP02.ToString();
                                eqp.SAMPLINGSIDELIST.Add(sampling7);

                                #endregion

                                #region VCR List

                                //if (node.Data.VCRCOUNT == 1)
                                //{
                                //    AllDataUpdateReply.VCRc vcr = new AllDataUpdateReply.VCRc();
                                //    vcr.VCRNO = "01";
                                //    vcr.VCRENABLEMODE = ((int)node.File.VcrMode[0]).ToString();
                                //    eqp.VCRLIST.Add(vcr);
                                //}
                                //else if (node.Data.VCRCOUNT > 1)
                                //{
                                //    if (fabType == eFabType.CELL)
                                //    {
                                //        string strVCR = node.File.UnitVCREnableStatus;
                                //        for (int index = 0; index < strVCR.Length; index++)
                                //        {
                                //            if (index < node.Data.VCRCOUNT)
                                //            {
                                //                AllDataUpdateReply.VCRc vcr = new AllDataUpdateReply.VCRc();
                                //                vcr.VCRNO = index.ToString().PadLeft(2, '0');
                                //                vcr.VCRENABLEMODE = strVCR[index].ToString() == "1" ? "1" : "0";
                                //                eqp.VCRLIST.Add(vcr);
                                //            }
                                //        }
                                //    }
                                //    else
                                //    {
                                //        for (int j = 0; j < node.File.VcrMode.Count(); j++)
                                //        {
                                //            AllDataUpdateReply.VCRc vcr = new AllDataUpdateReply.VCRc();
                                //            vcr.VCRNO = (j + 1).ToString().PadLeft(2, '0');
                                //            vcr.VCRENABLEMODE = node.File.VcrMode[j].ToString() == "1" ? "1" : "0";
                                //            eqp.VCRLIST.Add(vcr);
                                //        }
                                //    }
                                //}
                                for (int j = 0; j < node.File.VcrMode.Count(); j++)
                                {
                                    AllDataUpdateReply.VCRc vcr = new AllDataUpdateReply.VCRc();
                                    vcr.VCRNO = (j + 1).ToString().PadLeft(2, '0');
                                    vcr.VCRENABLEMODE = node.File.VcrMode[j].ToString() == "ON" ? "1" : "0";
                                    eqp.VCRLIST.Add(vcr);
                                }


                                #endregion

                                #region Interlock List

                                for (int j = 1; j <= node.Data.MPLCINTERLOCKCOUNT; j++)
                                {
                                    #region 從倉庫取MPLCInterlockCommand
                                    //string interlockNo = j.ToString("00");
                                    //Trx interlockTrx = Repository.Get(string.Format("{0}_MPLCInterlockCommand#{1}", node.Data.NODENO, interlockNo)) as Trx;
                                    //AllDataUpdateReply.INTERLOCKc interlock = new AllDataUpdateReply.INTERLOCKc();
                                    //interlock.INTERLOCKNO = interlockNo;
                                    //if (interlockTrx == null) continue;
                                    //interlock.INTERLOCKSTATUS = interlockTrx.EventGroups[0].Events[0].Items[0].Value;
                                    //eqp.INTERLOCKLIST.Add(interlock);
                                    #endregion
                                    #region 從PLC讀MPLCInterlockCommand
                                    string interlockNo = j.ToString("00");
                                    string trx_name = string.Format("{0}_MPLCInterlockCommand#{1}", node.Data.NODENO, interlockNo);
                                    Trx interlockTrx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trx_name, false }) as Trx;
                                    AllDataUpdateReply.INTERLOCKc interlock = new AllDataUpdateReply.INTERLOCKc();
                                    interlock.INTERLOCKNO = interlockNo;
                                    if (interlockTrx == null) continue;
                                    interlock.INTERLOCKSTATUS = interlockTrx.EventGroups[0].Events[0].Items[0].Value;
                                    eqp.INTERLOCKLIST.Add(interlock);
                                    #endregion
                                }

                                #endregion

                                foreach (Unit unit in ObjectManager.UnitManager.GetUnitsByEQPNo(node.Data.NODENO))
                                {
                                    AllDataUpdateReply.UNITc _unit = new AllDataUpdateReply.UNITc();
                                    _unit.UNITNO = unit.Data.UNITNO;
                                    _unit.UNITID = unit.Data.UNITID;
                                    _unit.CURRENTSTATUS = ((int)unit.File.Status).ToString() == "" ? "0" : ((int)unit.File.Status).ToString();
                                    _unit.TFTJOBCNT = unit.File.TFTProductCount.ToString();
                                    _unit.CFJOBCNT = unit.File.CFProductCount.ToString();
                                    _unit.UNITRUNMODE = unit.File.RunMode;
                                    eqp.UNITLIST.Add(_unit);
                                }

                                //CELL PPK Line以DenseBox上報OPI，而非Port
                                if (lines[0].Data.LINETYPE == eLineType.CELL.CBPPK)
                                {
                                    foreach (Port port in ObjectManager.PortManager.GetPorts())
                                    {
                                        if (port.Data.NODEID == node.Data.NODEID)
                                        {
                                            AllDataUpdateReply.DENSEBOXc dense = new AllDataUpdateReply.DENSEBOXc();
                                            dense.PORTID = port.Data.PORTID;
                                            dense.PORTNO = port.Data.PORTNO;
                                            dense.PORTENABLEMODE = ((int)port.File.EnableMode).ToString() == "" ? "0" : ((int)port.File.EnableMode).ToString();
                                            dense.PORTPACKINGMODE = ((int)port.File.PortPackMode).ToString() == "" ? "0" : ((int)port.File.PortPackMode).ToString();
                                            dense.BOXID01 = port.File.PortBoxID1;
                                            dense.BOXID02 = port.File.PortBoxID2;
                                            dense.UNPACKINGSOURCE = port.File.PortUnPackSource;
                                            dense.DENSEBOXDATAREQUEST = port.File.PortDBDataRequest;

                                            eqp.DENSEBOXLIST.Add(dense);
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (Port port in ObjectManager.PortManager.GetPorts())
                                    {
                                        if (port.Data.NODEID == node.Data.NODEID)
                                        {
                                            AllDataUpdateReply.PORTc _port = new AllDataUpdateReply.PORTc();
                                            _port.PORTNO = port.Data.PORTNO;
                                            _port.PORTID = port.Data.PORTID;
                                            _port.LINEID = port.Data.LINEID;
                                            _port.CASSETTESEQNO = port.File.CassetteSequenceNo;
                                            _port.CASSETTEID = port.File.CassetteID.Trim();
                                            _port.PORTSTATUS = ((int)port.File.Status).ToString() == "" ? "0" : ((int)port.File.Status).ToString();
                                            _port.CASSETTESTATUS = ((int)port.File.CassetteStatus).ToString() == "" ? "0" : ((int)port.File.CassetteStatus).ToString();
                                            _port.PORTTYPE = ((int)port.File.Type).ToString() == "" ? "0" : ((int)port.File.Type).ToString();
                                            _port.PORTMODE = ((int)port.File.Mode).ToString() == "" ? "0" : ((int)port.File.Mode).ToString();
                                            _port.PORTENABLEMODE = ((int)port.File.EnableMode).ToString() == "" ? "0" : ((int)port.File.EnableMode).ToString();
                                            _port.PORTTRANSFERMODE = ((int)port.File.TransferMode).ToString() == "" ? "0" : ((int)port.File.TransferMode).ToString();
                                            //_port.PORTOPERMODE = port.File.OperMode.ToString();
                                            _port.PORTGRADE = port.File.MappingGrade;
                                            _port.PRODUCTTYPE = port.File.ProductType;
                                            _port.PORTCNT = port.File.JobCountInCassette;
                                            _port.SUBCSTSTATE = port.File.OPI_SubCstState.ToString();
                                            _port.JOBEXISTSLOT = port.File.JobExistenceSlot;
                                            _port.PORTDOWN = ((int)port.File.DownStatus).ToString() == "" ? "0" : ((int)port.File.DownStatus).ToString();
                                            //_port.PARTIALFULLFLAG = ((int)port.File.PartialFullFlag).ToString() == "" ? "0" : ((int)port.File.PartialFullFlag).ToString();
                                            _port.PARTIALFULLMODE = ((int)node.File.PartialFullMode).ToString() == "" ? "0" : ((int)node.File.PartialFullMode).ToString();
                                            _port.LOADINGCASSETTETYPE = ((int)port.File.LoadingCassetteType).ToString() == "" ? "0" : ((int)port.File.LoadingCassetteType).ToString();
                                            _port.PROCESSTYPE_ARRAY = port.File.ProcessType == "" ? "0" : port.File.ProcessType;    //add by bruce 2015/7/22 by port report
                                            //20160128 add for GAP Port Assignment
                                            _port.ASSIGNMENT_GAP = ((int)port.File.PortAssignment).ToString();
                                            eqp.PORTLIST.Add(_port);
                                        }
                                    }
                                }

                                reply.BODY.EQUIPMENTLIST.Add(eqp);
                                //}
                            }

                            IList<Pallet> pallets = ObjectManager.PalletManager.GetPallets();
                            reply.BODY.PALLETLIST.Clear();

                            if (pallets != null)
                            {
                                foreach (Pallet pallet in ObjectManager.PalletManager.GetPallets())
                                {
                                    AllDataUpdateReply.PALLETc _pallet = new AllDataUpdateReply.PALLETc();
                                    _pallet.PALLETNO = pallet.File.PalletNo;
                                    _pallet.PALLETID = pallet.File.PalletID;
                                    _pallet.PALLETMODE = ((int)pallet.File.PalletMode).ToString() == "" ? "0" : ((int)pallet.File.PalletMode).ToString();
                                    _pallet.PALLETDATAREQUEST = pallet.File.PalletDataRequest;

                                    reply.BODY.PALLETLIST.Add(_pallet);
                                }
                            }
                            #region [new pallet]
                            IList<Port> ports = ObjectManager.PortManager.GetPorts();
                            foreach (Port port in ports)
                            {
                                if (port.Data.PORTATTRIBUTE == keyCELLPORTAtt.PALLET)
                                {
                                    if (ObjectManager.PalletManager.GetPalletByNo(port.Data.PORTNO)==null)
                                    {
                                        Pallet pallet = new Pallet(new PalletEntityFile());
                                        pallet.File.PalletNo = port.Data.PORTNO;
                                        pallet.File.PalletMode = ePalletMode.UNKNOWN;
                                        ObjectManager.PalletManager.AddPallet(pallet);

                                        AllDataUpdateReply.PALLETc _pallet = new AllDataUpdateReply.PALLETc();
                                        _pallet.PALLETNO = pallet.File.PalletNo;
                                        _pallet.PALLETID = pallet.File.PalletID;
                                        _pallet.PALLETMODE = ((int)pallet.File.PalletMode).ToString() == "" ? "0" : ((int)pallet.File.PalletMode).ToString();
                                        _pallet.PALLETDATAREQUEST = pallet.File.PalletDataRequest;

                                        reply.BODY.PALLETLIST.Add(_pallet);
                                    }                                    
                                }
                            }
                            #endregion


                            if (lines[0].File.IndexOperMode == eINDEXER_OPERATION_MODE.CHANGER_MODE)
                            {
                                CurrentChangerPlanReport(lines[0].Data.LINEID);
                            }
                        }

                        //移除多餘的項目
                        if (reply.BODY.EQUIPMENTLIST.Count > 0)
                              reply.BODY.EQUIPMENTLIST.RemoveAt(0);

                        xMessage msg = SendReplyToOPI(command, reply);

                        Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService reply message({2}) to OPI.",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME));
                  }
                  catch (Exception ex)
                  {
                        //NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService catch message({2}) exception : {3} ",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME, ex.Message));
                  }
            }

            /// <summary>
            /// OPI MessageSet: Equipment Status Report to OPI
            /// </summary>
            /// <param name="trxID"></param>
            /// <param name="eqp"></param>
            public void EquipmentStatusReport(string trxID, Equipment eqp)
            {
                  try
                  {
                        IServerAgent agent = GetServerAgent();
                        XmlDocument xml_doc = agent.GetTransactionFormat("EquipmentStatusReport") as XmlDocument;
                        EquipmentStatusReport trx = Spec.XMLtoMessage(xml_doc) as EquipmentStatusReport;
                        //trx.BODY.LINENAME = eqp.Data.LINEID;
                        trx.BODY.LINENAME = eqp.Data.SERVERNAME;
                        trx.BODY.EQUIPMENTNO = eqp.Data.NODENO;

                        //Trx aliveTrx = MISC.Repository.Get(string.Format("{0}_EquipmentAlive", eqp.Data.NODENO)) as Trx;//Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { string.Format("{0}_EquipmentAlive", eqp.Data.NODENO) }) as Trx;
                        //trx.BODY.EQUIPMENTALIVE = aliveTrx == null ? "0" : aliveTrx.EventGroups[0].Events[0].Items[0].Value;
                        //20150131 modify by edison:昌爷要求，取EQENTITY的AliveTimeout报给OPI
                        trx.BODY.EQUIPMENTALIVE = eqp.File.AliveTimeout == true ? "0" : "1";

                        trx.BODY.CURRENTSTATUS = ((int)eqp.File.Status).ToString() == "" ? "0" : ((int)eqp.File.Status).ToString();
                        trx.BODY.CIMMODE = ((int)eqp.File.CIMMode).ToString() == "" ? "0" : ((int)eqp.File.CIMMode).ToString();
                        trx.BODY.HSMSSTATUS = eqp.HsmsConnStatus;
                        trx.BODY.HSMSCONTROLMODE = eqp.File.HSMSControlMode;
                        trx.BODY.CSTOPERMODE = eqp.File.CSTOperationMode.ToString();
                        trx.BODY.UPSTREAMINLINEMODE = eqp.File.UpstreamInlineMode.ToString();
                        trx.BODY.DOWNSTREAMINLINEMODE = eqp.File.DownstreamInlineMode.ToString();
                        trx.BODY.AUTORECIPECHANGEMODE = eqp.File.AutoRecipeChangeMode.ToString();
                        trx.BODY.PARTIALFULLMODE = ((int)eqp.File.PartialFullMode).ToString();
                        trx.BODY.BYPASSMODE = eqp.File.BypassMode.ToString();
                        trx.BODY.CURRENTRECIPEID = eqp.File.CurrentRecipeID;

                        Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                        if (line != null)
                              trx.BODY.MATERIALSTATUS = (line.File.Array_Material_Change ? "1" : "0");

                        ////CBPMT Line的Loader(L2)有兩個EquipmentRunMode，需要特殊處理
                        //if ((eqp.Data.LINEID.Contains(keyCELLPMTLINE.CBPMI) || eqp.Data.LINEID.Contains(keyCELLPMTLINE.CBPTI))
                        //    && eqp.Data.NODENO == "L2")
                        //{
                        //      Line line1 = ObjectManager.LineManager.GetLine("CBPMI500");
                        //      Line line2 = ObjectManager.LineManager.GetLine("CBPTI100");

                        //      trx.BODY.EQUIPMENTRUNMODE = line1.File.CellLineOperMode;
                        //      trx.BODY.EQUIPMENTRUNMODE2 = line2.File.CellLineOperMode;
                        //}
                        //else
                        //      trx.BODY.EQUIPMENTRUNMODE = eqp.File.EquipmentRunMode;

                        trx.BODY.EQUIPMENTRUNMODE = eqp.File.EquipmentRunMode;
                        trx.BODY.TFTJOBCNT = eqp.File.TotalTFTJobCount.ToString();
                        trx.BODY.CFJOBCNT = eqp.File.TotalCFProductJobCount.ToString();
                        trx.BODY.DMYJOBCNT = eqp.File.TotalDummyJobCount.ToString();
                        trx.BODY.THROUGHDMYJOBCNT = eqp.File.ThroughDummyJobCount.ToString();
                        trx.BODY.THICKNESSDMYJOBCNT = eqp.File.ThicknessDummyJobCount.ToString();
                        trx.BODY.UNASSEMBLEDTFTDMYJOBCNT = eqp.File.TotalUnassembledTFTJobCount.ToString();//sy add 20160826
                        trx.BODY.ITODMYJOBCNT = eqp.File.TotalITODummyJobCount.ToString();//sy add 20160826
                        trx.BODY.NIPDMYJOBCNT = eqp.File.TotalNIPDummyJobCount.ToString();//sy add 20160826
                        trx.BODY.METALONEDMYJOBCNT = eqp.File.TotalMetalOneDummyJobCount.ToString();//sy add 20160826
                        trx.BODY.UVMASKJOBCNT = eqp.File.UVMASKJobCount.ToString();
                        trx.BODY.TURNTABLEMODE = ((int)eqp.File.TurnTableMode).ToString() == "" ? "0" : ((int)eqp.File.TurnTableMode).ToString();
                        trx.BODY.LOCALALARMSTATUS = eqp.File.LocalAlarmStatus.ToString();
                        trx.BODY.CASSETTEQTIME = eqp.File.CassetteonPortQTime.ToString();
                        trx.BODY.INSPECTIONIDLETIME = eqp.File.InspectionIdleTime.ToString();
                        trx.BODY.EQPOPERATIONMODE = ((int)eqp.File.EquipmentOperationMode).ToString() == "" ? "0" : ((int)eqp.File.EquipmentOperationMode).ToString();
                        trx.BODY.LASTGLASSID = eqp.File.FinalReceiveGlassID;
                        trx.BODY.LASTRECIVETIME = eqp.File.FinalReceiveGlassTime;
                        trx.BODY.TIMEOUTFLAG = eqp.File.TactTimeOut ? "1" : "0";
                        
                        // 20151125新增
                        trx.BODY.HIGHCVMODE = ((int)eqp.File.HighCVMode).ToString();
                        trx.BODY.NEXTLINEBCSTATUS = ((int)eqp.File.NextLineBCStatus).ToString() == "" ? "0" : ((int)eqp.File.NextLineBCStatus).ToString();
                        trx.BODY.JOBDATACHECKMODE = ((int)eqp.File.JobDataCheckMode).ToString() == "" ? "0" : ((int)eqp.File.JobDataCheckMode).ToString();
                        trx.BODY.COAVERSIONCHECKMODE = ((int)eqp.File.COAVersionCheckMode).ToString() == "" ? "0" : ((int)eqp.File.COAVersionCheckMode).ToString();
                        trx.BODY.JOBDUPLICATECHECKMODE = ((int)eqp.File.JobDuplicateCheckMode).ToString() == "" ? "0" : ((int)eqp.File.JobDuplicateCheckMode).ToString();
                        trx.BODY.PRODUCTIDCHECKMODE = ((int)eqp.File.ProductIDCheckMode).ToString() == "" ? "0" : ((int)eqp.File.ProductIDCheckMode).ToString();
                        trx.BODY.PRODUCTTYPECHECKMODE = ((int)eqp.File.ProductTypeCheckMode).ToString() == "" ? "0" : ((int)eqp.File.ProductTypeCheckMode).ToString();
                        trx.BODY.GROUPINDEXCHECKMODE = ((int)eqp.File.GroupIndexCheckMode).ToString() == "" ? "0" : ((int)eqp.File.GroupIndexCheckMode).ToString();
                        trx.BODY.RECIPEIDCHECKMODE = ((int)eqp.File.RecipeIDCheckMode).ToString() == "" ? "0" : ((int)eqp.File.RecipeIDCheckMode).ToString();
                        trx.BODY.BYPASSINSP01MODE = ((int)eqp.File.BypassInspectionEquipment01Mode).ToString() == "" ? "0" : ((int)eqp.File.BypassInspectionEquipment01Mode).ToString();
                        trx.BODY.BYPASSINSP02MODE = ((int)eqp.File.BypassInspectionEquipment02Mode).ToString() == "" ? "0" : ((int)eqp.File.BypassInspectionEquipment02Mode).ToString();
                        trx.BODY.CV07_STATUS = ((int)eqp.File.CV07Status).ToString() == "" ? "0" : ((int)eqp.File.CV07Status).ToString().ToString(); ;

                        #region Sampling Side
                        EquipmentStatusReport.SAMPLINGSIDEc sampling1 = new EquipmentStatusReport.SAMPLINGSIDEc();
                        sampling1.ITEMNAME = "VCD01";
                        sampling1.SIDESTATUS = eqp.File.VCD01.ToString();
                        trx.BODY.SAMPLINGSIDELIST.Add(sampling1);
                        EquipmentStatusReport.SAMPLINGSIDEc sampling2 = new EquipmentStatusReport.SAMPLINGSIDEc();
                        sampling2.ITEMNAME = "VCD02";
                        sampling2.SIDESTATUS = eqp.File.VCD02.ToString();
                        trx.BODY.SAMPLINGSIDELIST.Add(sampling2);
                        EquipmentStatusReport.SAMPLINGSIDEc sampling4 = new EquipmentStatusReport.SAMPLINGSIDEc();
                        sampling4.ITEMNAME = "CP01";
                        sampling4.SIDESTATUS = eqp.File.CP01.ToString();
                        trx.BODY.SAMPLINGSIDELIST.Add(sampling4);
                        EquipmentStatusReport.SAMPLINGSIDEc sampling5 = new EquipmentStatusReport.SAMPLINGSIDEc();
                        sampling5.ITEMNAME = "CP02";
                        sampling5.SIDESTATUS = eqp.File.CP02.ToString();
                        trx.BODY.SAMPLINGSIDELIST.Add(sampling5);
                        EquipmentStatusReport.SAMPLINGSIDEc sampling6 = new EquipmentStatusReport.SAMPLINGSIDEc();
                        sampling6.ITEMNAME = "HP01";
                        sampling6.SIDESTATUS = eqp.File.HP01.ToString();
                        trx.BODY.SAMPLINGSIDELIST.Add(sampling6);
                        EquipmentStatusReport.SAMPLINGSIDEc sampling7 = new EquipmentStatusReport.SAMPLINGSIDEc();
                        sampling7.ITEMNAME = "HP02";
                        sampling7.SIDESTATUS = eqp.File.HP02.ToString();
                        trx.BODY.SAMPLINGSIDELIST.Add(sampling7);

                        #endregion

                        #region VCR List
                        //20160104 cy:T3 CELL不會有UnitVCR, 全以VCRMode上報, 與保印確認OK
                        for (int j = 0; j < eqp.File.VcrMode.Count(); j++)
                        {
                              EquipmentStatusReport.VCRc vcr = new EquipmentStatusReport.VCRc();
                              vcr.VCRNO = (j + 1).ToString().PadLeft(2, '0');
                              vcr.VCRENABLEMODE = ((int)eqp.File.VcrMode[j]).ToString();
                              trx.BODY.VCRLIST.Add(vcr);
                        }
                        //if (eqp.Data.VCRCOUNT == 1)
                        //{
                        //      EquipmentStatusReport.VCRc vcr = new EquipmentStatusReport.VCRc();
                        //      vcr.VCRNO = "01";
                        //      vcr.VCRENABLEMODE = ((int)eqp.File.VcrMode[0]).ToString();
                        //      trx.BODY.VCRLIST.Add(vcr);
                        //}
                        //else if (eqp.Data.VCRCOUNT > 1)
                        //{
                        //      if (line != null)
                        //      {
                        //            if (line.Data.FABTYPE == eFabType.CELL.ToString())
                        //            {
                        //                  string strVCR = eqp.File.UnitVCREnableStatus;
                        //                  for (int index = 0; index < strVCR.Length; index++)
                        //                  {
                        //                        if (index < eqp.Data.VCRCOUNT)
                        //                        {
                        //                              EquipmentStatusReport.VCRc vcr = new EquipmentStatusReport.VCRc();
                        //                              vcr.VCRNO = (index + 1).ToString().PadLeft(2, '0');
                        //                              vcr.VCRENABLEMODE = strVCR[index].ToString() == "1" ? "1" : "0";
                        //                              trx.BODY.VCRLIST.Add(vcr);
                        //                        }
                        //                  }
                        //            }
                        //            else
                        //            {
                        //                  for (int j = 0; j < eqp.File.VcrMode.Count(); j++)
                        //                  {
                        //                        EquipmentStatusReport.VCRc vcr = new EquipmentStatusReport.VCRc();
                        //                        vcr.VCRNO = (j + 1).ToString().PadLeft(2, '0');
                        //                        vcr.VCRENABLEMODE = ((int)eqp.File.VcrMode[j]).ToString();
                        //                        trx.BODY.VCRLIST.Add(vcr);
                        //                  }
                        //            }
                        //      }
                        //}

                        #endregion

                        #region Interlock List

                        for (int j = 1; j <= eqp.Data.MPLCINTERLOCKCOUNT; j++)
                        {
                              #region 從倉庫取MPLCInterlockCommand
                              //string interlockNo = j.ToString("00");
                              //Trx interlockTrx = Repository.Get(string.Format("{0}_MPLCInterlockCommand#{1}", eqp.Data.NODENO, interlockNo)) as Trx;
                              //EquipmentStatusReport.INTERLOCKc interlock = new EquipmentStatusReport.INTERLOCKc();
                              //interlock.INTERLOCKNO = interlockNo;
                              //if (interlockTrx == null) continue;
                              //interlock.INTERLOCKSTATUS = interlockTrx.EventGroups[0].Events[0].Items[0].Value;
                              //trx.BODY.INTERLOCKLIST.Add(interlock);
                              #endregion
                              #region 從PLC讀MPLCInterlockCommand
                              string interlockNo = j.ToString("00");
                              string trx_name = string.Format("{0}_MPLCInterlockCommand#{1}", eqp.Data.NODENO, interlockNo);
                              Trx interlockTrx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trx_name, false }) as Trx;
                              EquipmentStatusReport.INTERLOCKc interlock = new EquipmentStatusReport.INTERLOCKc();
                              interlock.INTERLOCKNO = interlockNo;
                              if (interlockTrx == null) continue;
                              interlock.INTERLOCKSTATUS = interlockTrx.EventGroups[0].Events[0].Items[0].Value;
                              trx.BODY.INTERLOCKLIST.Add(interlock);
                              #endregion
                        }

                        #endregion

                        foreach (Unit unit in ObjectManager.UnitManager.GetUnits())
                        {
                              if (unit.Data.NODENO == eqp.Data.NODENO && unit.Data.LINEID == eqp.Data.LINEID)
                              {
                                    EquipmentStatusReport.UNITc _unit = new EquipmentStatusReport.UNITc();
                                    _unit.UNITNO = unit.Data.UNITNO;
                                    _unit.UNITID = unit.Data.UNITID;
                                    _unit.CURRENTSTATUS = ((int)unit.File.Status).ToString() == "" ? "0" : ((int)unit.File.Status).ToString();
                                    _unit.TFTJOBCNT = unit.File.TFTProductCount.ToString();
                                    _unit.CFJOBCNT = unit.File.CFProductCount.ToString();
                                    _unit.UNITRUNMODE = unit.File.RunMode;
                                    trx.BODY.UNITLIST.Add(_unit);
                              }
                        }

                        if (trx.BODY.UNITLIST.Count > 0)
                              trx.BODY.UNITLIST.RemoveAt(0);
                        if (trx.BODY.INTERLOCKLIST.Count > 0)
                              trx.BODY.INTERLOCKLIST.RemoveAt(0);
                        if (trx.BODY.SAMPLINGSIDELIST.Count > 0)
                              trx.BODY.SAMPLINGSIDELIST.RemoveAt(0);
                        if (trx.BODY.VCRLIST.Count > 1)
                              trx.BODY.VCRLIST.RemoveAt(0);

                        xMessage msg = SendReportToAllOPI(trxID, trx);

                        if (ParameterManager[eREPORT_SWITCH.RECORD_OPI_STATUS_LOG].GetBoolean())
                        {
                              //如果沒有OPI Client連線，OPIAgent將不會report message
                              if (dicClient.Count == 0)
                              {
                                    Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                        string.Format("[EQUIPMENT={0}] [BCS -> OPI][{1}] OPI client number is zero, UIService will not report message to OPI.",
                                        trx.BODY.EQUIPMENTNO, msg.TransactionID));
                              }
                              else
                              {
                                    Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                        string.Format("[EQUIPMENT={0}] [BCS -> OPI][{1}] UIService report message({2}) to OPI.",
                                        trx.BODY.EQUIPMENTNO, trx.HEADER.TRANSACTIONID, trx.HEADER.MESSAGENAME));
                              }
                        }
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }

            ///// <summary>
            ///// OPI MessageSet: Equipment Status Report Reply to BC
            ///// </summary>
            ///// <param name="xmlDoc"></param>
            //public void OPI_EquipmentStatusReportReply(XmlDocument xmlDoc)
            //{
            //    try
            //    {
            //        //什么都没有做就不 直接跳出去。 20150328 Tom
            //        //IServerAgent agent = GetServerAgent();
            //        //{
            //        //    Message equipmentStatusReportReply = Spec.CheckXMLFormat(xmlDoc);
            //        //}
            //    }
            //    catch (Exception ex)
            //    {
            //        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            //    }
            //}

            /// <summary>
            /// OPI MessageSt: EquipmentSatusRequest to BC
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_EquipmentStatusRequest(XmlDocument xmlDoc)
            {
                  IServerAgent agent = GetServerAgent();
                  EquipmentStatusRequest command = Spec.XMLtoMessage(xmlDoc) as EquipmentStatusRequest;
                  XmlDocument xml_doc = agent.GetTransactionFormat("EquipmentStatusReply") as XmlDocument;
                  EquipmentStatusReply reply = Spec.XMLtoMessage(xml_doc) as EquipmentStatusReply;

                  try
                  {
                        Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI. EQUIPMENTNO={3}",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME, command.BODY.EQUIPMENTNO));

                        reply.BODY.LINENAME = command.BODY.LINENAME;
                        reply.BODY.EQUIPMENTNO = command.BODY.EQUIPMENTNO;

                        //Line line = ObjectManager.LineManager.GetLine(command.BODY.LINENAME);
                        IList<Line> lines = ObjectManager.LineManager.GetLines();
                        Equipment node = ObjectManager.EquipmentManager.GetEQP(command.BODY.EQUIPMENTNO);
                        if (lines == null)
                        {
                              reply.RETURN.RETURNCODE = "0010020";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Line[{0}] in LineEntity", command.BODY.EQUIPMENTNO);

                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't find Line({0}) in LineEntity.",
                                  command.BODY.LINENAME, command.HEADER.TRANSACTIONID));
                        }
                        else if (node == null)
                        {
                              reply.RETURN.RETURNCODE = "0010021";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Equipment[{0}] in EquipmentEntity", command.BODY.EQUIPMENTNO);

                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Can't find Equipment({0}) in EquipmentEntity.",
                                  command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                        }
                        else
                        {
                              //string aliveName = string.Format("{0}_EquipmentAlive", node.Data.NODENO);
                              //Trx aliveTrx = Invoke(eAgentName.PLCAgent, "GetTransactionFormat", new object[] { aliveName }) as Trx;
                              //if (aliveTrx != null) aliveTrx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { aliveName,true }) as Trx;
                              //reply.BODY.EQUIPMENTALIVE = aliveTrx == null ? "0" : aliveTrx.EventGroups[0].Events[0].Items[0].Value;
                              //20150130 modify by edison:昌爷要求，取EQENTITY的AliveTimeout报给OPI
                              reply.BODY.EQUIPMENTALIVE = node.File.AliveTimeout == true ? "0" : "1";

                              reply.BODY.CURRENTSTATUS = ((int)node.File.Status).ToString() == "" ? "0" : ((int)node.File.Status).ToString();
                              reply.BODY.CIMMODE = ((int)node.File.CIMMode).ToString() == "" ? "0" : ((int)node.File.CIMMode).ToString();
                              reply.BODY.HSMSSTATUS = node.HsmsConnStatus;
                              reply.BODY.HSMSCONTROLMODE = node.File.HSMSControlMode;
                              reply.BODY.CSTOPERMODE = node.File.CSTOperationMode.ToString();
                              reply.BODY.UPSTREAMINLINEMODE = node.File.UpstreamInlineMode.ToString();
                              reply.BODY.DOWNSTREAMINLINEMODE = node.File.DownstreamInlineMode.ToString();
                              reply.BODY.AUTORECIPECHANGEMODE = node.File.AutoRecipeChangeMode.ToString();
                              reply.BODY.PARTIALFULLMODE = ((int)node.File.PartialFullMode).ToString();
                              reply.BODY.BYPASSMODE = node.File.BypassMode.ToString();
                              reply.BODY.CURRENTRECIPEID = node.File.CurrentRecipeID;
                              reply.BODY.LASTGLASSID = node.File.FinalReceiveGlassID;
                              reply.BODY.LASTRECIVETIME = node.File.FinalReceiveGlassTime;
                              reply.BODY.TIMEOUTFLAG = node.File.TactTimeOut ? "1" : "0";

                              ////CBPMT Line的Loader(L2)有兩個EquipmentRunMode，需要特殊處理
                              //if ((node.Data.LINEID.Contains(keyCELLPMTLINE.CBPMI) || node.Data.LINEID.Contains(keyCELLPMTLINE.CBPTI))
                              //    && node.Data.NODENO == "L2")
                              //{
                              //      Line line1 = ObjectManager.LineManager.GetLine("CBPMI500");
                              //      Line line2 = ObjectManager.LineManager.GetLine("CBPTI100");

                              //      reply.BODY.EQUIPMENTRUNMODE = line1.File.CellLineOperMode;
                              //      reply.BODY.EQUIPMENTRUNMODE2 = line2.File.CellLineOperMode;
                              //}
                              //else
                                    reply.BODY.EQUIPMENTRUNMODE = node.File.EquipmentRunMode;

                              reply.BODY.TFTJOBCNT = node.File.TotalTFTJobCount.ToString();
                              reply.BODY.CFJOBCNT = node.File.TotalCFProductJobCount.ToString();
                              reply.BODY.DMYJOBCNT = node.File.TotalDummyJobCount.ToString();
                              reply.BODY.THROUGHDMYJOBCNT = node.File.ThroughDummyJobCount.ToString();
                              reply.BODY.THICKNESSDMYJOBCNT = node.File.ThicknessDummyJobCount.ToString();
                              reply.BODY.UNASSEMBLEDTFTDMYJOBCNT = node.File.TotalUnassembledTFTJobCount.ToString();//sy add 20160826
                              reply.BODY.ITODMYJOBCNT = node.File.TotalITODummyJobCount.ToString();//sy add 20160826
                              reply.BODY.NIPDMYJOBCNT = node.File.TotalNIPDummyJobCount.ToString();//sy add 20160826
                              reply.BODY.METALONEDMYJOBCNT = node.File.TotalMetalOneDummyJobCount.ToString();//sy add 20160826
                              reply.BODY.UVMASKJOBCNT = node.File.UVMASKJobCount.ToString();
                              reply.BODY.TURNTABLEMODE = ((int)node.File.TurnTableMode).ToString() == "" ? "0" : ((int)node.File.TurnTableMode).ToString();
                              reply.BODY.LOCALALARMSTATUS = node.File.LocalAlarmStatus.ToString();
                              reply.BODY.CASSETTEQTIME = node.File.CassetteonPortQTime.ToString();
                              reply.BODY.INSPECTIONIDLETIME = node.File.InspectionIdleTime.ToString();
                              reply.BODY.EQPOPERATIONMODE = ((int)node.File.EquipmentOperationMode).ToString() == "" ? "0" : ((int)node.File.EquipmentOperationMode).ToString();
                              reply.BODY.BYPASSINSP01MODE = ((int)node.File.BypassInspectionEquipment01Mode).ToString() == "" ? "0" : ((int)node.File.BypassInspectionEquipment01Mode).ToString();
                              reply.BODY.BYPASSINSP02MODE = ((int)node.File.BypassInspectionEquipment02Mode).ToString() == "" ? "0" : ((int)node.File.BypassInspectionEquipment02Mode).ToString();
                              reply.BODY.HIGHCVMODE = ((int)node.File.HighCVMode).ToString() == "" ? "0" : ((int)node.File.HighCVMode).ToString();
                              reply.BODY.NEXTLINEBCSTATUS = ((int)node.File.NextLineBCStatus).ToString() == "" ? "0" : ((int)node.File.NextLineBCStatus).ToString();
                              reply.BODY.JOBDATACHECKMODE = ((int)node.File.JobDataCheckMode).ToString() == "" ? "0" : ((int)node.File.JobDataCheckMode).ToString();
                              reply.BODY.COAVERSIONCHECKMODE = ((int)node.File.COAVersionCheckMode).ToString() == "" ? "0" : ((int)node.File.COAVersionCheckMode).ToString();
                              reply.BODY.JOBDUPLICATECHECKMODE = ((int)node.File.JobDuplicateCheckMode).ToString() == "" ? "0" : ((int)node.File.JobDuplicateCheckMode).ToString();
                              reply.BODY.PRODUCTIDCHECKMODE = ((int)node.File.ProductIDCheckMode).ToString() == "" ? "0" : ((int)node.File.ProductIDCheckMode).ToString();
                              reply.BODY.GROUPINDEXCHECKMODE = ((int)node.File.GroupIndexCheckMode).ToString() == "" ? "0" : ((int)node.File.GroupIndexCheckMode).ToString();
                              reply.BODY.RECIPEIDCHECKMODE = ((int)node.File.RecipeIDCheckMode).ToString() == "" ? "0" : ((int)node.File.RecipeIDCheckMode).ToString();
                              reply.BODY.PRODUCTTYPECHECKMODE = ((int)node.File.ProductTypeCheckMode).ToString() == "" ? "0" : ((int)node.File.ProductTypeCheckMode).ToString();
                              //reply.BODY.LOADEROPERATIONMODE_ATS = ((int)node.File.ATSLoaderOperMode).ToString() == "" ? "0" : ((int)node.File.ATSLoaderOperMode).ToString();
                              reply.BODY.MATERIALSTATUS = "0";
                              if (node.Data.NODENO == "L3")
                              {
                                    switch (lines[0].Data.LINETYPE)
                                    {
                                          case eLineType.ARRAY.WET_DMS:
                                          case eLineType.ARRAY.WEI_DMS:
                                          case eLineType.ARRAY.STR_DMS:
                                          case eLineType.ARRAY.CLN_DMS:  //add by qiumin 20171222
                                                reply.BODY.MATERIALSTATUS = lines[0].File.Array_Material_Change ? "1" : "0";
                                                break;
                                    }
                              }

                              #region Sampling Side

                              EquipmentStatusReply.SAMPLINGSIDEc sampling1 = new EquipmentStatusReply.SAMPLINGSIDEc();
                              sampling1.ITEMNAME = "VCD01";
                              sampling1.SIDESTATUS = node.File.VCD01.ToString();
                              reply.BODY.SAMPLINGSIDELIST.Add(sampling1);
                              EquipmentStatusReply.SAMPLINGSIDEc sampling2 = new EquipmentStatusReply.SAMPLINGSIDEc();
                              sampling2.ITEMNAME = "VCD02";
                              sampling2.SIDESTATUS = node.File.VCD02.ToString();
                              reply.BODY.SAMPLINGSIDELIST.Add(sampling2);
                              EquipmentStatusReply.SAMPLINGSIDEc sampling4 = new EquipmentStatusReply.SAMPLINGSIDEc();
                              sampling4.ITEMNAME = "CP01";
                              sampling4.SIDESTATUS = node.File.CP01.ToString();
                              reply.BODY.SAMPLINGSIDELIST.Add(sampling4);
                              EquipmentStatusReply.SAMPLINGSIDEc sampling5 = new EquipmentStatusReply.SAMPLINGSIDEc();
                              sampling5.ITEMNAME = "CP02";
                              sampling5.SIDESTATUS = node.File.CP02.ToString();
                              reply.BODY.SAMPLINGSIDELIST.Add(sampling5);
                              EquipmentStatusReply.SAMPLINGSIDEc sampling6 = new EquipmentStatusReply.SAMPLINGSIDEc();
                              sampling6.ITEMNAME = "HP01";
                              sampling6.SIDESTATUS = node.File.HP01.ToString();
                              reply.BODY.SAMPLINGSIDELIST.Add(sampling6);
                              EquipmentStatusReply.SAMPLINGSIDEc sampling7 = new EquipmentStatusReply.SAMPLINGSIDEc();
                              sampling7.ITEMNAME = "HP02";
                              sampling7.SIDESTATUS = node.File.HP02.ToString();
                              reply.BODY.SAMPLINGSIDELIST.Add(sampling7);

                              #endregion

                              #region VCR List
                              //if (reply.BODY.VCRLIST.Count > 0)
                              //{
                              //      reply.BODY.VCRLIST.Clear();
                              //}
                              //if (node.Data.VCRCOUNT == 1)
                              //{
                              //      EquipmentStatusReply.VCRc vcr = new EquipmentStatusReply.VCRc();
                              //      vcr.VCRNO = "01";
                              //      vcr.VCRENABLEMODE = ((int)node.File.VcrMode[0]).ToString();
                              //      reply.BODY.VCRLIST.Add(vcr);
                              //}
                              //else if (node.Data.VCRCOUNT > 1)
                              //{
                              //      Line line = ObjectManager.LineManager.GetLine(node.Data.LINEID);
                              //      if (line != null)
                              //      {
                              //            if (line.Data.FABTYPE == eFabType.CELL.ToString())
                              //            {
                              //                  string strVCR = node.File.UnitVCREnableStatus;
                              //                  for (int index = 0; index < strVCR.Length; index++)
                              //                  {
                              //                        if (index < node.Data.VCRCOUNT)
                              //                        {
                              //                              EquipmentStatusReply.VCRc vcr = new EquipmentStatusReply.VCRc();
                              //                              vcr.VCRNO = (index + 1).ToString().PadLeft(2, '0');
                              //                              vcr.VCRENABLEMODE = strVCR[index].ToString() == "1" ? "1" : "0";
                              //                              reply.BODY.VCRLIST.Add(vcr);
                              //                        }
                              //                  }
                              //            }
                              //            else
                              //            {
                              //                  for (int j = 0; j < node.File.VcrMode.Count(); j++)
                              //                  {
                              //                        EquipmentStatusReply.VCRc vcr = new EquipmentStatusReply.VCRc();
                              //                        vcr.VCRNO = (j + 1).ToString().PadLeft(2, '0');
                              //                        vcr.VCRENABLEMODE = node.File.VcrMode[j].ToString() == "ON" ? "1" : "0";
                              //                        reply.BODY.VCRLIST.Add(vcr);
                              //                  }
                              //            }
                              //      }
                              //}
                              if (reply.BODY.VCRLIST.Count > 0)                     //Modify by Tom.su 20160819 T3 Vcr Use VcrMode;
                              {
                                  reply.BODY.VCRLIST.Clear();
                              }
                              for (int j = 0; j < node.File.VcrMode.Count(); j++)
                              {
                                  EquipmentStatusReply.VCRc vcr = new EquipmentStatusReply.VCRc();
                                  vcr.VCRNO = (j + 1).ToString().PadLeft(2, '0');
                                  vcr.VCRENABLEMODE = ((int)node.File.VcrMode[j]).ToString();
                                  reply.BODY.VCRLIST.Add(vcr);
                              }


                              #endregion

                              #region Interlock

                              for (int j = 1; j <= node.Data.MPLCINTERLOCKCOUNT; j++)
                              {
                                    #region 從倉庫取MPLCInterlockCommand
                                    //string interlockNo = j.ToString("00");
                                    //Trx interlockTrx = Repository.Get(string.Format("{0}_MPLCInterlockCommand#{1}", node.Data.NODENO, interlockNo)) as Trx;
                                    //EquipmentStatusReply.INTERLOCKc interlock = new EquipmentStatusReply.INTERLOCKc();
                                    //interlock.INTERLOCKNO = interlockNo;
                                    //if (interlockTrx == null) continue;
                                    //interlock.INTERLOCKSTATUS = interlockTrx.EventGroups[0].Events[0].Items[0].Value;
                                    //reply.BODY.INTERLOCKLIST.Add(interlock);
                                    #endregion
                                    #region 從PLC讀MPLCInterlockCommand
                                    string interlockNo = j.ToString("00");
                                    string trx_name = string.Format("{0}_MPLCInterlockCommand#{1}", node.Data.NODENO, interlockNo);
                                    Trx interlockTrx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trx_name, false }) as Trx;
                                    EquipmentStatusReply.INTERLOCKc interlock = new EquipmentStatusReply.INTERLOCKc();
                                    interlock.INTERLOCKNO = interlockNo;
                                    if (interlockTrx == null) continue;
                                    interlock.INTERLOCKSTATUS = interlockTrx.EventGroups[0].Events[0].Items[0].Value;
                                    reply.BODY.INTERLOCKLIST.Add(interlock);
                                    #endregion
                              }

                              #endregion

                              #region RobotOperationMode
                              //TODO:
                              #endregion

                              foreach (Unit unit in ObjectManager.UnitManager.GetUnits())
                              {
                                    if (unit.Data.LINEID == node.Data.LINEID && unit.Data.NODENO == node.Data.NODENO)
                                    {
                                          EquipmentStatusReply.UNITc _unit = new EquipmentStatusReply.UNITc();
                                          _unit.UNITNO = unit.Data.UNITNO;
                                          _unit.UNITID = unit.Data.UNITID;
                                          _unit.CURRENTSTATUS = ((int)unit.File.Status).ToString() == "" ? "0" : ((int)unit.File.Status).ToString();
                                          _unit.TFTJOBCNT = unit.File.TFTProductCount.ToString();
                                          _unit.CFJOBCNT = unit.File.CFProductCount.ToString();
                                          _unit.UNITRUNMODE = unit.File.RunMode;
                                          reply.BODY.UNITLIST.Add(_unit);
                                    }
                              }
                        }

                        if (reply.BODY.UNITLIST.Count > 0)
                              reply.BODY.UNITLIST.RemoveAt(0);
                        if (reply.BODY.INTERLOCKLIST.Count > 0)
                              reply.BODY.INTERLOCKLIST.RemoveAt(0);
                        if (reply.BODY.SAMPLINGSIDELIST.Count > 0)
                              reply.BODY.SAMPLINGSIDELIST.RemoveAt(0);
                        //if (reply.BODY.VCRLIST.Count > 0)
                        //    reply.BODY.VCRLIST.RemoveAt(0);

                        xMessage msg = SendReplyToOPI(command, reply);

                        if (ParameterManager[eREPORT_SWITCH.RECORD_OPI_STATUS_LOG].GetBoolean())
                        {
                              Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService reply message({2}) to OPI.",
                                  command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME));
                        }
                  }
                  catch (Exception ex)
                  {
                        //NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService catch message({2}) exception : {3} ",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME, ex.Message));
                  }
            }

            /// <summary>
            /// OPI MessageSet: Port CST Status Report to OPI
            /// </summary>
            /// <param name="trxID"></param>
            /// <param name="port"></param>
            public void PortCSTStatusReport(string trxID, Port port)
            {
                  try
                  {
                        IServerAgent agent = GetServerAgent();
                        XmlDocument xml_doc = agent.GetTransactionFormat("PortCSTStatusReport") as XmlDocument;
                        PortCSTStatusReport trx = Spec.XMLtoMessage(xml_doc) as PortCSTStatusReport;
                        //trx.BODY.LINENAME = port.Data.LINEID;
                        trx.BODY.LINENAME = port.Data.SERVERNAME;
                        trx.BODY.LINEID = port.Data.LINEID;
                        trx.BODY.MESCONTROLSTATENAM = ObjectManager.LineManager.GetLine(port.Data.LINEID).File.HostMode.ToString();
                        trx.BODY.EQUIPMENTNO = port.Data.NODENO;
                        trx.BODY.PORTNO = port.Data.PORTNO;
                        trx.BODY.PORTID = port.Data.PORTID;
                        trx.BODY.CASSETTESEQNO = port.File.CassetteSequenceNo;
                        trx.BODY.CASSETTEID = port.File.CassetteID.Trim();
                        trx.BODY.PORTSTATUS = ((int)port.File.Status).ToString() == "" ? "0" : ((int)port.File.Status).ToString();
                        trx.BODY.CASSETTESTATUS = ((int)port.File.CassetteStatus).ToString() == "" ? "0" : ((int)port.File.CassetteStatus).ToString();
                        trx.BODY.PORTTYPE = ((int)port.File.Type).ToString() == "" ? "0" : ((int)port.File.Type).ToString();
                        trx.BODY.PORTMODE = ((int)port.File.Mode).ToString() == "" ? "0" : ((int)port.File.Mode).ToString();
                        trx.BODY.PORTENABLEMODE = ((int)port.File.EnableMode).ToString() == "" ? "0" : ((int)port.File.EnableMode).ToString();
                        trx.BODY.PORTTRANSFERMODE = ((int)port.File.TransferMode).ToString() == "" ? "0" : ((int)port.File.TransferMode).ToString();
                        //trx.BODY.PORTOPERMODE = port.File.OperMode.ToString();
                        trx.BODY.PORTGRADE = port.File.MappingGrade;
                        trx.BODY.PORTCNT = port.File.JobCountInCassette;
                        trx.BODY.SUBCSTSTATE = port.File.OPI_SubCstState.ToString();
                        trx.BODY.JOBEXISTSLOT = port.File.JobExistenceSlot;
                        trx.BODY.PORTDOWN = ((int)port.File.DownStatus).ToString() == "" ? "0" : ((int)port.File.DownStatus).ToString();
                        //trx.BODY.PARTIALFULLFLAG = ((int)port.File.PartialFullFlag).ToString() == "" ? "0" : ((int)port.File.PartialFullFlag).ToString();
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(port.Data.NODENO);
                        trx.BODY.LOADINGCASSETTETYPE = ((int)port.File.LoadingCassetteType).ToString() == "" ? "0" : ((int)port.File.LoadingCassetteType).ToString();
                        trx.BODY.PRODUCTTYPE = port.File.ProductType;
                        trx.BODY.PROCESSTYPE_ARRAY = port.File.ProcessType; // add by bruce 2015/7/22 by port report

                        //20160128 add For GAP Assignment
                        trx.BODY.ASSIGNMENT_GAP = ((int)port.File.PortAssignment).ToString();

                        xMessage msg = SendReportToAllOPI(trxID, trx);

                        if (ParameterManager[eREPORT_SWITCH.RECORD_OPI_STATUS_LOG].GetBoolean())
                        {
                              if (dicClient.Count == 0)
                              {
                                    Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                    string.Format("[PORTID={0}] [BCS -> OPI][{1}] OPI client number is zero, UIService will not report message to OPI.",
                                    port.Data.PORTID, msg.TransactionID));
                              }
                              else
                              {
                                    Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                    string.Format("[PORTID={0}] [BCS -> OPI][{1}] UIService report equipment({2}) message({3}) to OPI.",
                                    port.Data.PORTID, msg.TransactionID, trx.BODY.EQUIPMENTNO, trx.HEADER.MESSAGENAME));
                              }
                        }
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }

            ///// <summary>
            ///// OPI MessageSet: Port CST Status Report Reply to BC
            ///// </summary>
            ///// <param name="xmlDoc"></param>
            //public void OPI_PortCSTStatusReportReply(XmlDocument xmlDoc)
            //{
            //    try
            //    {
            //        //don't do something return 20150328 tom
            //        //IServerAgent agent = GetServerAgent();
            //        //{
            //        //    Message portCSTStatusReportReply = Spec.CheckXMLFormat(xmlDoc);
            //        //}
            //    }
            //    catch (Exception ex)
            //    {
            //        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            //    }
            //}

            /// <summary>
            /// OPI MessageSet: Port Status Request to BC
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_PortStatusRequest(XmlDocument xmlDoc)
            {
                  IServerAgent agent = GetServerAgent();
                  PortStatusRequest command = Spec.XMLtoMessage(xmlDoc) as PortStatusRequest;
                  XmlDocument xml_doc = agent.GetTransactionFormat("PortStatusReply") as XmlDocument;
                  PortStatusReply reply = Spec.XMLtoMessage(xml_doc) as PortStatusReply;

                  try
                  {
                        Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI. EQUIPMENTNO={3}, PORTNO={4}",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME, command.BODY.EQUIPMENTNO, command.BODY.PORTNO));

                        reply.BODY.LINENAME = command.BODY.LINENAME;
                        reply.BODY.EQUIPMENTNO = command.BODY.EQUIPMENTNO;
                        reply.BODY.PORTNO = command.BODY.PORTNO;

                        List<Line> lines = ObjectManager.LineManager.GetLines();
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(command.BODY.EQUIPMENTNO);

                        if (lines == null || lines.Count == 0)
                        {
                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[LINE={0}] [BCS <- OPI][{1}] Can't find Line({0}) in LineEntity.",
                                  command.BODY.LINENAME, command.HEADER.TRANSACTIONID));

                              reply.RETURN.RETURNCODE = "0010031";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Line[{0}] in LineEntity", command.BODY.LINENAME);
                        }
                        else if (eqp == null)
                        {
                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Can't find Equipment({0}) in EquipmentEntity.",
                                  command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));

                              reply.RETURN.RETURNCODE = "0010032";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Equipment[{0}] in EquipmentEntity.", command.BODY.EQUIPMENTNO);
                        }
                        else
                        {
                              Line line = null;
                              Port port = null;
                              foreach (Line tmp in lines)
                              {
                                    port = ObjectManager.PortManager.GetPort(tmp.Data.LINEID, command.BODY.EQUIPMENTNO, command.BODY.PORTNO);
                                    if (port != null)
                                    {
                                          line = tmp;
                                          break;
                                    }
                              }
                              if (port == null)
                              {
                                    reply.RETURN.RETURNCODE = "0010030";
                                    reply.RETURN.RETURNMESSAGE = string.Format("Can't find Port[{0}] in PortEntity", command.BODY.PORTNO);

                                    Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                        string.Format("[PORTNO={0}] [BCS <- OPI][{1}] Can't find Port({0}) of Equipment({2}) in PortEntity.",
                                        command.BODY.PORTNO, command.HEADER.TRANSACTIONID, command.BODY.EQUIPMENTNO));
                              }
                              else
                              {
                                    reply.BODY.LINEID = port.Data.LINEID;
                                    reply.BODY.MESCONTROLSTATENAM = line.File.HostMode.ToString();
                                    reply.BODY.PORTID = port.Data.PORTID;
                                    reply.BODY.CASSETTESEQNO = port.File.CassetteSequenceNo;
                                    reply.BODY.CASSETTEID = port.File.CassetteID.Trim();
                                    reply.BODY.PORTSTATUS = ((int)port.File.Status).ToString() == "" ? "0" : ((int)port.File.Status).ToString();
                                    reply.BODY.CASSETTESTATUS = ((int)port.File.CassetteStatus).ToString() == "" ? "0" : ((int)port.File.CassetteStatus).ToString();
                                    reply.BODY.PORTTYPE = ((int)port.File.Type).ToString() == "" ? "0" : ((int)port.File.Type).ToString();
                                    reply.BODY.PORTMODE = ((int)port.File.Mode).ToString() == "" ? "0" : ((int)port.File.Mode).ToString();
                                    reply.BODY.PORTENABLEMODE = ((int)port.File.EnableMode).ToString() == "" ? "0" : ((int)port.File.EnableMode).ToString();
                                    reply.BODY.PORTTRANSFERMODE = ((int)port.File.TransferMode).ToString() == "" ? "0" : ((int)port.File.TransferMode).ToString();
                                    //reply.BODY.PORTOPERMODE = port.File.OperationID;
                                    reply.BODY.PORTGRADE = port.File.MappingGrade;
                                    reply.BODY.PORTCNT = port.File.JobCountInCassette;
                                    reply.BODY.SUBCSTSTATE = port.File.OPI_SubCstState.ToString();
                                    //reply.BODY.SUBCSTSTATE = "WACSTEDIT"; test
                                    reply.BODY.JOBEXISTSLOT = port.File.JobExistenceSlot;
                                    reply.BODY.PORTDOWN = ((int)port.File.DownStatus).ToString() == "" ? "0" : ((int)port.File.DownStatus).ToString();
                                    //reply.BODY.PARTIALFULLFLAG = ((int)port.File.PartialFullFlag).ToString() == "" ? "0" : ((int)port.File.PartialFullFlag).ToString();
                                    reply.BODY.PARTIALFULLMODE = ((int)eqp.File.PartialFullMode).ToString() == "" ? "0" : ((int)eqp.File.PartialFullMode).ToString();
                                    reply.BODY.LOADINGCASSETTETYPE = ((int)port.File.LoadingCassetteType).ToString() == "" ? "0" : ((int)port.File.LoadingCassetteType).ToString();
                                    reply.BODY.PRODUCTTYPE = port.File.ProductType;
                                    reply.BODY.PROCESSTYPE_ARRAY = port.File.ProcessType;   //add by bruce 2015/7/22 by port report
                                    //20160128 add for GAP Port Assignment Info
                                    reply.BODY.ASSIGNMENT_GAP = ((int)port.File.PortAssignment).ToString();

                              }
                        }

                        xMessage msg = SendReplyToOPI(command, reply);

                        if (ParameterManager[eREPORT_SWITCH.RECORD_OPI_STATUS_LOG].GetBoolean())
                        {
                              Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService reply message({2}) to OPI.",
                                  command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME));
                        }
                  }
                  catch (Exception ex)
                  {
                        //NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService catch message({2}) exception : {3} ",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME, ex.Message));
                  }
            }

            /// <summary>
            /// OPI MessageSet: Line Status Report to OPI
            /// </summary>
            /// <param name="trxID"></param>
            /// <param name="line"></param>
            //Add By Yangzhenteng&Hujunpeng For OPI Display 20180904
            public void MaterialRealWeightReport(string trxID, List<MaterialEntity> MaterialList)
            {
                try
                {
                    IServerAgent agent = GetServerAgent();
                    XmlDocument xml_doc = agent.GetTransactionFormat("MaterialRealWeightReport") as XmlDocument;
                    MaterialRealWeightReport trx = Spec.XMLtoMessage(xml_doc) as MaterialRealWeightReport;
                    Equipment eqp = ObjectManager.EquipmentManager.GetEQP("L6");
                    if(MaterialList.Count==1)
                    #region[Add By HUjunpeng For CF OPI Display20180904]
                    {
                       switch (MaterialList[0].MaterialSlotNo)
                            {
                                case "0":
                                    if (!string.IsNullOrEmpty(eqp.File.PrID1))
                                    {
                                        trx.BODY.MaterialForCF01ID = eqp.File.PrID1.Substring(0, 24);
                                        trx.BODY.MaterialForCF01Weight = eqp.File.PrID1.Substring(25, 5);
                                        switch (eqp.File.PrID1.Substring(24, 1))
                                        {
                                            case "I":
                                                trx.BODY.MaterialForCF01Status = "INUSE";
                                                break;
                                            case "D":
                                                trx.BODY.MaterialForCF01Status = "DISMOUNT";
                                                break;
                                            case "M":
                                                trx.BODY.MaterialForCF01Status = "MOUNT";
                                                break;
                                            default:
                                                break;
                                        }
                                    }
                                    if (!string.IsNullOrEmpty(eqp.File.PrID2))
                                    {
                                        trx.BODY.MaterialForCF02ID = eqp.File.PrID2.Substring(0, 24);
                                        trx.BODY.MaterialForCF02Weight = eqp.File.PrID2.Substring(25, 5);
                                        switch (eqp.File.PrID2.Substring(24, 1))
                                        {
                                            case "I":
                                                trx.BODY.MaterialForCF02Status = "INUSE";
                                                break;
                                            case "D":
                                                trx.BODY.MaterialForCF02Status = "DISMOUNT";
                                                break;
                                            case "M":
                                                trx.BODY.MaterialForCF02Status = "MOUNT";
                                                break;
                                            default:
                                                break;
                                        }
                                    }
                                    break;
                               //if (!string.IsNullOrEmpty(eqp.File.PrID3))
                               //     {
                               //         trx.BODY.MaterialForCF01ID = eqp.File.PrID3.Substring(0, 24);
                               //         trx.BODY.MaterialForCF01Weight = eqp.File.PrID3.Substring(25, 5);
                               //         switch (eqp.File.PrID3.Substring(24, 1))
                               //         {
                               //             case "I":
                               //                 trx.BODY.MaterialForCF01Status = "INUSE";
                               //                 break;
                               //             case "D":
                               //                 trx.BODY.MaterialForCF01Status = "DISMOUNT";
                               //                 break;
                               //             case "M":
                               //                 trx.BODY.MaterialForCF01Status = "MOUNT";
                               //                 break;
                               //             default:
                               //                 break;
                               //         }
                               //     }
                               //     if (!string.IsNullOrEmpty(eqp.File.PrID4))
                               //     {
                               //         trx.BODY.MaterialForCF02ID = eqp.File.PrID4.Substring(0, 24);
                               //         trx.BODY.MaterialForCF02Weight = eqp.File.PrID4.Substring(25, 5);
                               //         switch (eqp.File.PrID4.Substring(24, 1))
                               //         {
                               //             case "I":
                               //                 trx.BODY.MaterialForCF02Status = "INUSE";
                               //                 break;
                               //             case "D":
                               //                 trx.BODY.MaterialForCF02Status = "DISMOUNT";
                               //                 break;
                               //             case "M":
                               //                 trx.BODY.MaterialForCF02Status = "MOUNT";
                               //                 break;
                               //             default:
                               //                 break;
                               //         }
                               //     }
                               //     break;
                                case "1":
                                    trx.BODY.MaterialForCF01ID = MaterialList[0].MaterialID;
                                    trx.BODY.MaterialForCF01Weight = MaterialList[0].MaterialWeight;
                                    trx.BODY.MaterialForCF01Status = MaterialList[0].MaterialStatus.ToString();
                                    if (!string.IsNullOrEmpty(eqp.File.PrID2))
                                    {
                                        trx.BODY.MaterialForCF02ID = eqp.File.PrID2.Substring(0, 24);
                                        trx.BODY.MaterialForCF02Weight = eqp.File.PrID2.Substring(25, 5);
                                        switch (eqp.File.PrID2.Substring(24, 1))
                                        {
                                            case "I":
                                                trx.BODY.MaterialForCF02Status = "INUSE";
                                                break;
                                            case "D":
                                                trx.BODY.MaterialForCF02Status = "DISMOUNT";
                                                break;
                                            case "M":
                                                trx.BODY.MaterialForCF02Status = "MOUNT";
                                                break;
                                            default:
                                                break;
                                        }
                                    }
                                    break;
                                case "2":
                                    if (!string.IsNullOrEmpty(eqp.File.PrID1))
                                    {
                                        trx.BODY.MaterialForCF01ID = eqp.File.PrID1.Substring(0, 24);
                                        trx.BODY.MaterialForCF01Weight = eqp.File.PrID1.Substring(25, 5);
                                        switch (eqp.File.PrID1.Substring(24, 1))
                                        {
                                            case "I":
                                                trx.BODY.MaterialForCF01Status = "INUSE";
                                                break;
                                            case "D":
                                                trx.BODY.MaterialForCF01Status = "DISMOUNT";
                                                break;
                                            case "M":
                                                trx.BODY.MaterialForCF01Status = "MOUNT";
                                                break;
                                            default:
                                                break;
                                        }
                                    }
                                    trx.BODY.MaterialForCF02ID = MaterialList[0].MaterialID;
                                    trx.BODY.MaterialForCF02Weight = MaterialList[0].MaterialWeight;
                                    trx.BODY.MaterialForCF02Status = MaterialList[0].MaterialStatus.ToString();
                                    break;
                                default:
                                    break;

                            }
                    }
                         
                    #endregion                    
                    else
                    #region[For PIL OPI Display 20180904]
                    {
                    trx.BODY.MaterialTK01ID = MaterialList[0].MaterialID;
                    trx.BODY.MaterialTK01Weight = MaterialList[0].MaterialWeight;
                    trx.BODY.MaterialTK02ID = MaterialList[1].MaterialID;
                    trx.BODY.MaterialTK02Weight = MaterialList[1].MaterialWeight;                          
                    trx.BODY.MaterialTK03ID = MaterialList[2].MaterialID;
                    trx.BODY.MaterialTK03Weight = MaterialList[2].MaterialWeight;                       
                    trx.BODY.MaterialTK04ID = MaterialList[3].MaterialID;
                    trx.BODY.MaterialTK04Weight = MaterialList[3].MaterialWeight;
                    trx.BODY.MaterialTK05ID = MaterialList[4].MaterialID;
                    trx.BODY.MaterialTK05Weight = MaterialList[4].MaterialWeight;
                    trx.BODY.MaterialTK06ID = MaterialList[5].MaterialID;
                    trx.BODY.MaterialTK06Weight = MaterialList[5].MaterialWeight;
                    trx.BODY.MaterialTK07ID = MaterialList[6].MaterialID;
                    trx.BODY.MaterialTK07Weight = MaterialList[6].MaterialWeight;
                    trx.BODY.MaterialTK08ID = MaterialList[7].MaterialID;
                    trx.BODY.MaterialTK08Weight = MaterialList[7].MaterialWeight;  
                    }                        
                    #endregion
                    xMessage msg = SendReportToAllOPI(trxID, trx);
                }
                catch (Exception ex)
                {
                    NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                }
            }
            //add by hujunpeng 20190723
            public void PIJobCountReport(string trxID, Dictionary<string, Tuple<int, int>> PIJobCount)
            {
                try
                {
                    IServerAgent agent = GetServerAgent();
                    XmlDocument xml_doc = agent.GetTransactionFormat("PIJobCountReport") as XmlDocument;
                    PIJobCountReport trx = Spec.XMLtoMessage(xml_doc) as PIJobCountReport;
                    List<string> keys = PIJobCount.Keys.ToList<string>();

                    if (keys.Count == 1)
                    {
                        trx.BODY.ProductGroupOwner01 = keys[0];
                        trx.BODY.TFTCount01 = PIJobCount[keys[0]].Item1.ToString();
                        trx.BODY.CFCount01 = PIJobCount[keys[0]].Item2.ToString();
                    }

                    if (keys.Count == 2)
                    {
                        trx.BODY.ProductGroupOwner01 = keys[0];
                        trx.BODY.TFTCount01 = PIJobCount[keys[0]].Item1.ToString();
                        trx.BODY.CFCount01 = PIJobCount[keys[0]].Item2.ToString();
                        trx.BODY.ProductGroupOwner02 = keys[1];
                        trx.BODY.TFTCount02 = PIJobCount[keys[1]].Item1.ToString();
                        trx.BODY.CFCount02 = PIJobCount[keys[1]].Item2.ToString();
                    }

                    if (keys.Count == 3)
                    {
                        trx.BODY.ProductGroupOwner01 = keys[0];
                        trx.BODY.TFTCount01 = PIJobCount[keys[0]].Item1.ToString();
                        trx.BODY.CFCount01 = PIJobCount[keys[0]].Item2.ToString();
                        trx.BODY.ProductGroupOwner02 = keys[1];
                        trx.BODY.TFTCount02 = PIJobCount[keys[1]].Item1.ToString();
                        trx.BODY.CFCount02 = PIJobCount[keys[1]].Item2.ToString();
                        trx.BODY.ProductGroupOwner03 = keys[2];
                        trx.BODY.TFTCount03 = PIJobCount[keys[2]].Item1.ToString();
                        trx.BODY.CFCount03 = PIJobCount[keys[2]].Item2.ToString();
                    }
                    xMessage msg = SendReportToAllOPI(trxID, trx);
                }
                catch (Exception ex)
                {
                    NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                }
            }
            public void LineStatusReport(string trxID, Line line)
            {
                  try
                  {
                        IServerAgent agent = GetServerAgent();
                        XmlDocument xml_doc = agent.GetTransactionFormat("LineStatusReport") as XmlDocument;
                        LineStatusReport trx = Spec.XMLtoMessage(xml_doc) as LineStatusReport;
                        //trx.BODY.LINENAME = line.Data.LINEID;
                        trx.BODY.LINENAME = line.Data.SERVERNAME;
                        trx.BODY.LINETYPE = line.Data.LINETYPE;
                        trx.BODY.FACTORYTYPE = line.Data.FABTYPE;
                        //for部分是由兩條Line組成的特殊Line
                        trx.BODY.LINEID = line.Data.LINEID;
                        trx.BODY.MESCONTROLSTATENAM = line.File.HostMode.ToString();
                        trx.BODY.INDEXEROPERATIONMODE = ((int)line.File.IndexOperMode).ToString() == "" ? "0" : ((int)line.File.IndexOperMode).ToString();
                        trx.BODY.PLCSTATUS = line.File.PLCStatus;
                        trx.BODY.LINESTATUSNAME = line.File.Status.ToString();
                        trx.BODY.COOLRUNSETCOUNT = line.File.CoolRunSetCount.ToString();
                        trx.BODY.COOLRUNREMAINCOUNT = line.File.CoolRunRemainCount.ToString();
                        trx.BODY.LINEOPERMODE = line.File.LineOperMode;
                        trx.BODY.SHORTCUTMODE = line.File.CFShortCutMode.ToString();
                        trx.BODY.ROBOT_FETCH_SEQ_MODE = line.File.RobotFetchSeqMode == null ? "0" : line.File.RobotFetchSeqMode;    //add by bruce 2015/7/17 opi add new transation item
                        Trx statusTrx = Invoke(eAgentName.PLCAgent, "GetTransactionFormat", new object[] { "L1_BCStatus" }) as Trx;
                        if (statusTrx != null) statusTrx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { "L1_BCStatus", true }) as Trx;
                        //trx.BODY.BCSTATUS = statusTrx == null ? "" : statusTrx.EventGroups[0].Events[0].Items[0].Value;
                        //trx.BODY.EQUIPMENTRUNMODE = line.File.UPKEquipmentRunMode.ToString();

                        xMessage msg = SendReportToAllOPI(trxID, trx);

                        if (ParameterManager[eREPORT_SWITCH.RECORD_OPI_STATUS_LOG].GetBoolean())
                        {
                              if (dicClient.Count == 0)
                              {
                                    Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                    string.Format("[LINENAME={0}] [BCS -> OPI][{1}] OPI client number is zero, UIService will not report message to OPI.s",
                                    trx.BODY.LINENAME, msg.TransactionID));
                              }
                              else
                              {
                                    Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                    string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService report message({2}) to OPI.",
                                    ObjectManager.LineManager.GetLineID(trx.BODY.LINENAME), msg.TransactionID, trx.HEADER.MESSAGENAME));
                              }
                        }
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }

            ///// <summary>
            ///// OPI MessageSet: Line Status Report Reply to BC
            ///// </summary>
            ///// <param name="xmlDoc"></param>
            //public void OPI_LineStatusReportReply(XmlDocument xmlDoc)
            //{
            //    try
            //    {
            //        //don't do something 20150328 tom
            //        //IServerAgent agent = GetServerAgent();
            //        //{
            //        //    Message lineStatusReportReply = Spec.CheckXMLFormat(xmlDoc);
            //        //}
            //    }
            //    catch (Exception ex)
            //    {
            //        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            //    }
            //}

            /// <summary>
            /// OPI MessageSet: Line Status Request to BC
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_LineStatusRequest(XmlDocument xmlDoc)
            {
                  IServerAgent agent = GetServerAgent();
                  LineStatusRequest command = Spec.XMLtoMessage(xmlDoc) as LineStatusRequest;
                  XmlDocument xml_doc = agent.GetTransactionFormat("LineStatusReply") as XmlDocument;
                  LineStatusReply reply = Spec.XMLtoMessage(xml_doc) as LineStatusReply;

                  try
                  {
                        Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI.",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME));

                        reply.BODY.LINENAME = command.BODY.LINENAME;

                        //Line line = ObjectManager.LineManager.GetLine(command.BODY.LINENAME);
                        IList<Line> lines = ObjectManager.LineManager.GetLines();
                        if (lines == null)
                        {
                              reply.RETURN.RETURNCODE = "0010040";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Line[{0}] in LineEntity", command.BODY.LINENAME);

                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't find Line({0}) in LineEntity.",
                                  command.BODY.LINENAME, command.HEADER.TRANSACTIONID));
                        }
                        else
                        {
                              reply.BODY.LINETYPE = lines[0].Data.LINETYPE;
                              reply.BODY.FACTORYTYPE = lines[0].Data.FABTYPE;
                              //reply.BODY.MESCONTROLSTATENAM = line.File.HostMode.ToString();
                              foreach (Line line in lines)
                              {
                                    reply.BODY.LINEIDLIST.Add(new LineStatusReply.LINEc() { LINEID = line.Data.LINEID, MESCONTROLSTATENAM = line.File.HostMode.ToString() });
                              }
                              reply.BODY.INDEXEROPERATIONMODE = ((int)lines[0].File.IndexOperMode).ToString() == "" ? "0" : ((int)lines[0].File.IndexOperMode).ToString();
                              reply.BODY.PLCSTATUS = lines[0].File.PLCStatus;
                              reply.BODY.LINEOPERMODE = lines[0].File.LineOperMode;
                              reply.BODY.SHORTCUTMODE = lines[0].File.CFShortCutMode.ToString();
                              reply.BODY.LINESTATUS = lines[0].File.Status.ToString();
                              reply.BODY.COOLRUNSETCOUNT = lines[0].File.CoolRunSetCount.ToString();
                              reply.BODY.COOLRUNREMAINCOUNT = lines[0].File.CoolRunRemainCount.ToString();
                              reply.BODY.ROBOT_FETCH_SEQ_MODE = lines[0].File.RobotFetchSeqMode == null ? "0" : lines[0].File.RobotFetchSeqMode.ToString();   // add by bruce 2015/7/17 opi add transation new item
                              reply.BODY.DAILYCHECKREPORTTIME = lines[0].File.DailyCheckIntervalS;
                              Trx statusTrx = Invoke(eAgentName.PLCAgent, "GetTransactionFormat", new object[] { "L1_BCStatus" }) as Trx;
                              //if(statusTrx != null) statusTrx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { "L1_BCStatus",true }) as Trx;
                              if (statusTrx != null) statusTrx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { "L1_BCStatus", false }) as Trx;//此处不用PLCAgent 记录Log 20150211 Tom
                              //reply.BODY.BCSTATUS = statusTrx == null ? "" : statusTrx.EventGroups[0].Events[0].Items[0].Value;
                              //reply.BODY.EQUIPMENTRUNMODE = lines[0].File.UPKEquipmentRunMode.ToString();
                        }

                        xMessage msg = SendReplyToOPI(command, reply);
                        if (ParameterManager[eREPORT_SWITCH.RECORD_OPI_STATUS_LOG].GetBoolean())
                        {
                              Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService reply message({2}) to OPI.",
                                  command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME));
                        }
                  }
                  catch (Exception ex)
                  {
                        //NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService catch message({2}) exception : {3} ",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME, ex.Message));
                  }
            }

            /// <summary>
            /// OPI MessageSet: Link Signal Data Request to BC
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_LinkSignalDataRequest(XmlDocument xmlDoc)
            {
                  IServerAgent agent = GetServerAgent();
                  LinkSignalDataRequest command = Spec.XMLtoMessage(xmlDoc) as LinkSignalDataRequest;
                  XmlDocument xml_doc = agent.GetTransactionFormat("LinkSignalDataReply") as XmlDocument;
                  LinkSignalDataReply reply = Spec.XMLtoMessage(xml_doc) as LinkSignalDataReply;

                  try
                  {
                        Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI. UPSTREAMEQUIPMENTNO={3}, DOWNSTREAMEQUIPMENTNO={4}",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME, command.BODY.UPSTREAMEQUIPMENTNO, command.BODY.DOWNSTREAMEQUIPMENTNO));

                        reply.BODY.LINENAME = command.BODY.LINENAME;

                        //Line line = ObjectManager.LineManager.GetLine(command.BODY.LINENAME);
                        IList<Line> lines = ObjectManager.LineManager.GetLines();
                        if (lines == null)
                        {
                              reply.RETURN.RETURNCODE = "0010050";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Line[{0}] in LineEntity", command.BODY.LINENAME);

                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't find Line({0}) in LineEntity.",
                                  command.BODY.LINENAME, command.HEADER.TRANSACTIONID));
                        }

                        reply.BODY.UPSTREAMEQUIPMENTNO = command.BODY.UPSTREAMEQUIPMENTNO;
                        reply.BODY.UPSTREAMEQUIPMENTUNITNO = command.BODY.UPSTREAMEQUIPMENTUNITNO;
                        reply.BODY.DOWNSTREAMEQUIPMENTNO = command.BODY.DOWNSTREAMEQUIPMENTNO;
                        reply.BODY.DOWNSTREAMEQUIPMENTUNITNO = command.BODY.DOWNSTREAMEQUIPMENTUNITNO;
                        //reply.BODY.SEQUENCENO = command.BODY.SEQUENCENO;
                        reply.BODY.UPSTREAMSEQUENCENO = command.BODY.UPSTREAMSEQUENCENO_BIT;
                        reply.BODY.DOWNSTREAMSEQUENCENO = command.BODY.DOWNSTREAMSEQUENCENO_BIT;

                        //Equipment upEqp = ObjectManager.EquipmentManager.GetEQP(command.BODY.UPSTREAMEQUIPMENTNO);
                        Equipment downEqp = ObjectManager.EquipmentManager.GetEQP(command.BODY.DOWNSTREAMEQUIPMENTNO);

                        string upLocalNo = command.BODY.UPSTREAMEQUIPMENTNO;
                        string downLocalNo = command.BODY.DOWNSTREAMEQUIPMENTNO;
                        string upLinkSignal = "00000000000000000000000000000000";
                        string downLinkSignal = "00000000000000000000000000000000";

                        //由PLCAgent取回link signal資料
                        string upTrxName = string.Format("{0}_UpstreamPath#{1}", upLocalNo, command.BODY.UPSTREAMSEQUENCENO_BIT);
                        Trx upTrx = Invoke(eAgentName.PLCAgent, "GetTransactionFormat", new object[] { upTrxName }) as Trx;
                        if (upTrx != null) upTrx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { upTrxName, false }) as Trx;
                        if (upTrx == null)
                        {
                              reply.RETURN.RETURNCODE = "0010051";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't get PLC value from Trx[{0}].", upTrxName);

                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't get PLC value from Trx({2}).",
                                  command.BODY.LINENAME, command.HEADER.TRANSACTIONID, upTrxName));
                        }
                        else
                        {
                              for (int i = 0; i < upTrx.EventGroups[0].Events[0].Items.Count; i++)
                              {
                                    int iOffset = upTrx.EventGroups[0].Events[0].Items[i].Metadata.BitOffset;
                                    string strValue = upTrx.EventGroups[0].Events[0].Items[i].Value.ToString();
                                    upLinkSignal = upLinkSignal.Remove(iOffset, 1);
                                    upLinkSignal = upLinkSignal.Insert(iOffset, strValue);
                              }
                              reply.BODY.UPSTREAMSIGNAL = upLinkSignal;//.PadRight(32,'0');
                              reply.BODY.UPSTREAMBITADDRESS = upTrx.EventGroups[0].Events[0].Metadata.Address;
                        }

                        string downTrxName = string.Format("{0}_DownstreamPath#{1}", downLocalNo, command.BODY.DOWNSTREAMSEQUENCENO_BIT);
                        Trx downTrx = Invoke(eAgentName.PLCAgent, "GetTransactionFormat", new object[] { downTrxName }) as Trx;
                        //if(downTrx != null) downTrx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { downTrxName,true }) as Trx;
                        if (downTrx != null) downTrx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { downTrxName, false }) as Trx; //此处不用PLCAgent 记录Log 20150211 Tom
                        if (downTrx == null)
                        {
                              reply.RETURN.RETURNCODE = "0010051";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't get PLC value from Trx[{0}].", downTrxName);

                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't get PLC value from Trx({2}).",
                                  command.BODY.LINENAME, command.HEADER.TRANSACTIONID, downTrxName));
                        }
                        else
                        {
                              for (int i = 0; i < downTrx.EventGroups[0].Events[0].Items.Count; i++)
                              {
                                    int iOffset = downTrx.EventGroups[0].Events[0].Items[i].Metadata.BitOffset;
                                    string strValue = downTrx.EventGroups[0].Events[0].Items[i].Value.ToString();
                                    downLinkSignal = downLinkSignal.Remove(iOffset, 1);
                                    downLinkSignal = downLinkSignal.Insert(iOffset, strValue);
                              }
                              reply.BODY.DOWNSTREAMSIGNAL = downLinkSignal;//.PadRight(32, '0');
                              reply.BODY.DOWNSTREAMBITADDRESS = downTrx.EventGroups[0].Events[0].Metadata.Address;
                        }
                        Line line = ObjectManager.LineManager.GetLine(reply.BODY.LINENAME);

                        //20151215 cy:Change get job data rule.
                        #region Get SendOutJobData
                        //由PLCAgent取回JobData資料
                        reply.BODY.UPSTREAMJOBDATALIST.RemoveAt(0);
                        foreach (UniAuto.UniBCS.OpiSpec.LinkSignalDataRequest.SEQUENCENO_WORDc seq in command.BODY.UPSTREAMSEQUENCENOLIST)
                        {
                              string upJobTrxName = string.Format("{0}_SendOutJobDataReport#{1}", upLocalNo, seq.SEQUENCENO_WORD); // command.BODY.UPSTREAMSEQUENCENO_BIT);
                              Trx upJobTrx = Invoke(eAgentName.PLCAgent, "GetTransactionFormat", new object[] { upJobTrxName }) as Trx;
                              //if(upJobTrx != null) upJobTrx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { upJobTrxName,true }) as Trx;
                              if (upJobTrx != null) upJobTrx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { upJobTrxName, false }) as Trx;//此处不用PLCAgent 记录Log  20150211  Tom
                              if (upJobTrx == null)
                              {
                                    reply.RETURN.RETURNCODE = "0010051";
                                    reply.RETURN.RETURNMESSAGE = string.Format("Can't get PLC value from Trx[{0}].", upJobTrxName);

                                    Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                        string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't get PLC value from Trx({2}).",
                                        command.BODY.LINENAME, command.HEADER.TRANSACTIONID, upJobTrxName));
                              }
                              else
                              {
                                    UniAuto.UniBCS.OpiSpec.LinkSignalDataReply.JOBDATAc upJobData = new LinkSignalDataReply.JOBDATAc();
                                    upJobData.CASSETTESEQNO = upJobTrx.EventGroups[0].Events[0].Items[eJOBDATA.CassetteSequenceNo].Value;
                                    upJobData.JOBSEQNO = upJobTrx.EventGroups[0].Events[0].Items[eJOBDATA.JobSequenceNo].Value;
                                    upJobData.PRODUCTTYPE = upJobTrx.EventGroups[0].Events[0].Items[eJOBDATA.ProductType].Value;
                                    upJobData.SUBSTRATETYPE = upJobTrx.EventGroups[0].Events[0].Items[eJOBDATA.SubstrateType].Value;
                                    upJobData.JOBTYPE = upJobTrx.EventGroups[0].Events[0].Items[eJOBDATA.JobType].Value;
                                    upJobData.JOBJUDGE = upJobTrx.EventGroups[0].Events[0].Items[eJOBDATA.JobJudge].Value;
                                    upJobData.JOBGRADE = upJobTrx.EventGroups[0].Events[0].Items[eJOBDATA.JobGrade].Value;
                                    upJobData.GLASSID = upJobTrx.EventGroups[0].Events[0].Items[eJOBDATA.Glass_Chip_Mask_BlockID].Value.Trim();
                                    upJobData.PPID = upJobTrx.EventGroups[0].Events[0].Items[eJOBDATA.PPID].Value;
                                    upJobData.TRACKINGDATA = upJobTrx.EventGroups[0].Events[0].Items[eJOBDATA.TrackingData].Value;
                                    if (line.Data.JOBDATALINETYPE == "FCPHO_BMPS" || line.Data.JOBDATALINETYPE == "FCPHO_GRB")
                                        upJobData.EQPFLAG = upJobTrx.EventGroups[0].Events[0].Items[eJOBDATA.EQPFlag1].Value +
                                                            upJobTrx.EventGroups[0].Events[0].Items[eJOBDATA.EQPFlag2].Value;
                                    else
                                        upJobData.EQPFLAG = upJobTrx.EventGroups[0].Events[0].Items[eJOBDATA.EQPFlag].Value;
                                    upJobData.JOBADDRESS = upJobTrx.EventGroups[0].Events[0].Metadata.Address;
                                    reply.BODY.UPSTREAMJOBDATALIST.Add(upJobData);
                              }
                        }
                        #endregion

                        //20151215 cy:Change get job data rule.
                        #region Get ReceiveJobData
                        //SECS機台不會有ReceiveJobData
                        reply.BODY.DOWNSTREAMJOBDATALIST.RemoveAt(0);
                        if (downEqp.Data.REPORTMODE != "HSMS_CSOT" && downEqp.Data.REPORTMODE != "HSMS_NIKON" && downEqp.Data.REPORTMODE != "HSMS_PLC")
                        {
                              foreach (UniAuto.UniBCS.OpiSpec.LinkSignalDataRequest.SEQUENCENO_WORDc seq in command.BODY.DOWNSTREAMSEQUENCENOLIST)
                              {
                                    string downJobTrxName = string.Format("{0}_ReceiveJobDataReport#{1}", downLocalNo, seq.SEQUENCENO_WORD);// command.BODY.DOWNSTREAMSEQUENCENO_BIT);
                                    Trx downJobTrx = Invoke(eAgentName.PLCAgent, "GetTransactionFormat", new object[] { downJobTrxName }) as Trx;
                                    //if(downJobTrx != null) downJobTrx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { downJobTrxName,true }) as Trx;
                                    if (downJobTrx != null) downJobTrx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { downJobTrxName, false }) as Trx; //此处不用PLCAgent 记录Log  20150211 Tom
                                    if (downJobTrx == null)
                                    {
                                          reply.RETURN.RETURNCODE = "0010051";
                                          reply.RETURN.RETURNMESSAGE = string.Format("Can't get PLC value from Trx[{0}].", downJobTrxName);

                                          Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                              string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't get PLC value from Trx({2}).",
                                              command.BODY.LINENAME, command.HEADER.TRANSACTIONID, downJobTrxName));
                                    }
                                    else
                                    {
                                          UniAuto.UniBCS.OpiSpec.LinkSignalDataReply.JOBDATAc downJobData = new LinkSignalDataReply.JOBDATAc();
                                          downJobData.CASSETTESEQNO = downJobTrx.EventGroups[0].Events[0].Items[eJOBDATA.CassetteSequenceNo].Value;
                                          downJobData.JOBSEQNO = downJobTrx.EventGroups[0].Events[0].Items[eJOBDATA.JobSequenceNo].Value;
                                          downJobData.PRODUCTTYPE = downJobTrx.EventGroups[0].Events[0].Items[eJOBDATA.ProductType].Value;
                                          downJobData.SUBSTRATETYPE = downJobTrx.EventGroups[0].Events[0].Items[eJOBDATA.SubstrateType].Value;
                                          downJobData.JOBTYPE = downJobTrx.EventGroups[0].Events[0].Items[eJOBDATA.JobType].Value;
                                          downJobData.JOBJUDGE = downJobTrx.EventGroups[0].Events[0].Items[eJOBDATA.JobJudge].Value;
                                          downJobData.JOBGRADE = downJobTrx.EventGroups[0].Events[0].Items[eJOBDATA.JobGrade].Value;
                                          downJobData.GLASSID = downJobTrx.EventGroups[0].Events[0].Items[eJOBDATA.Glass_Chip_Mask_BlockID].Value.Trim();
                                          downJobData.PPID = downJobTrx.EventGroups[0].Events[0].Items[eJOBDATA.PPID].Value;
                                          downJobData.TRACKINGDATA = downJobTrx.EventGroups[0].Events[0].Items[eJOBDATA.TrackingData].Value;
                                          if (line.Data.JOBDATALINETYPE == "FCPHO_BMPS" || line.Data.JOBDATALINETYPE == "FCPHO_GRB")
                                              downJobData.EQPFLAG = downJobTrx.EventGroups[0].Events[0].Items[eJOBDATA.EQPFlag1].Value +
                                                                  downJobTrx.EventGroups[0].Events[0].Items[eJOBDATA.EQPFlag2].Value;
                                          else
                                              downJobData.EQPFLAG = downJobTrx.EventGroups[0].Events[0].Items[eJOBDATA.EQPFlag].Value;
                                          downJobData.JOBADDRESS = downJobTrx.EventGroups[0].Events[0].Metadata.Address;
                                          reply.BODY.DOWNSTREAMJOBDATALIST.Add(downJobData);
                                    }
                              }
                        } 
                        #endregion

                        xMessage msg = SendReplyToOPI(command, reply);
                        if (ParameterManager[eREPORT_SWITCH.RECORD_OPI_STATUS_LOG].GetBoolean())
                        {
                              Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService reply message({2}) to OPI.",
                                  command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME));
                        }
                  }
                  catch (Exception ex)
                  {
                        //NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService catch message({2}) exception : {3} ",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME, ex.Message));
                  }
            }

            /// <summary>
            /// OPI MessageSet: Slot Position Request
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_SlotPositionRequest(XmlDocument xmlDoc)
            {
                  IServerAgent agent = GetServerAgent();
                  SlotPositionRequest command = Spec.XMLtoMessage(xmlDoc) as SlotPositionRequest;
                  XmlDocument xml_doc = agent.GetTransactionFormat("SlotPositionReply") as XmlDocument;
                  SlotPositionReply reply = Spec.XMLtoMessage(xml_doc) as SlotPositionReply;

                  try
                  {
                        Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI. EQUIPMENTNO={3}, PORTID={4}",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME, command.BODY.EQUIPMENTNO, command.BODY.PORTID));

                        reply.BODY.LINENAME = command.BODY.LINENAME;
                        reply.BODY.EQUIPMENTNO = command.BODY.EQUIPMENTNO;
                        reply.BODY.PORTNO = command.BODY.PORTNO;
                        reply.BODY.PORTID = command.BODY.PORTID;

                        //Line line = ObjectManager.LineManager.GetLine(command.BODY.LINENAME);
                        IList<Line> lines = ObjectManager.LineManager.GetLines();
                        Equipment node = ObjectManager.EquipmentManager.GetEQP(command.BODY.EQUIPMENTNO);
                        Port port = ObjectManager.PortManager.GetPort(command.BODY.PORTID);
                        if (lines == null)
                        {
                              reply.RETURN.RETURNCODE = "0010672";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Line[{0}] in LineEntity", command.BODY.EQUIPMENTNO);

                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't find Line({0} in LineEntity.)",
                                  command.BODY.LINENAME, command.HEADER.TRANSACTIONID));
                        }
                        else if (node == null)
                        {
                              reply.RETURN.RETURNCODE = "0010670";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Equipment[{0}] in EquipmentEntity", command.BODY.EQUIPMENTNO);

                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Can't find Equipment({0} in EquipmentEntity.)",
                                  command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                        }
                        else if (port == null)
                        {
                              reply.RETURN.RETURNCODE = "0010673";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Port[{0}] in PortEntity", command.BODY.PORTID);

                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[PORT={0}] [BCS <- OPI][{1}] Can't find Port({0} in PortEntity.)",
                                  command.BODY.PORTID, command.HEADER.TRANSACTIONID));
                        }
                        else
                        {
                              //從PLCAgent取得CassetteSlotPosition資訊
                              string strName = string.Empty;
                              //CELL DenseBox特殊情況
                              Line line = ObjectManager.LineManager.GetLine(node.Data.LINEID);
                              //sy modify By T3 IO同CST 所以都用Port#{1}JobEachCassetteSlotPositionBlock  so Mark
                              //if (line.Data.FABTYPE == eFabType.CELL.ToString() && port.Data.PORTATTRIBUTE == keyCELLPORTAtt.DENSE)
                              //      strName = string.Format("{0}_DP#{1}JobEachCassetteSlotPositionBlock", command.BODY.EQUIPMENTNO, command.BODY.PORTNO);
                              //else
                            strName = string.Format("{0}_Port#{1}JobEachCassetteSlotPositionBlock", command.BODY.EQUIPMENTNO, command.BODY.PORTNO);

                              Trx positionTrx = Invoke(eAgentName.PLCAgent, "GetTransactionFormat", new object[] { strName }) as Trx;
                              if (positionTrx != null) positionTrx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { strName, true }) as Trx;

                              if (positionTrx == null)
                              {
                                    reply.RETURN.RETURNCODE = "0010671";
                                    reply.RETURN.RETURNMESSAGE = string.Format("Can't get value from Trx[{0}]", strName);

                                    Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                   string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Trx Name is wrong, can't get Trx({2}) value,",
                                   command.BODY.LINENAME, command.HEADER.TRANSACTIONID, strName));
                              }
                              else
                              {
                                    int iPos = 1;
                                    for (int i = 0; i < positionTrx.EventGroups[0].Events[0].Items.Count; i += 2)
                                    {
                                          SlotPositionReply.POSITIONc pos = new SlotPositionReply.POSITIONc();
                                          pos.POSITIONNO = iPos.ToString();
                                          pos.CASSETTESEQNO = positionTrx.EventGroups[0].Events[0].Items[i].Value;
                                          pos.JOBSEQNO = positionTrx.EventGroups[0].Events[0].Items[i + 1].Value;

                                          Job job = ObjectManager.JobManager.GetJob(pos.CASSETTESEQNO, pos.JOBSEQNO);
                                          if (job != null)
                                          {
                                                if (line != null && line.Data.LINETYPE == eLineType.CELL.CBMCL)
                                                      pos.JOBID = ObjectManager.JobManager.GetJob(pos.CASSETTESEQNO, pos.JOBSEQNO).CellSpecial.MASKID;
                                                else
                                                      pos.JOBID = ObjectManager.JobManager.GetJob(pos.CASSETTESEQNO, pos.JOBSEQNO).GlassChipMaskBlockID;
                                                //防止Decode失敗
                                                if (!string.IsNullOrEmpty(job.TrackingData))
                                                {
                                                      IDictionary<string, string> subItems = ObjectManager.SubJobDataManager.Decode(job.TrackingData, "TrackingData");
                                                      string data = string.Empty;
                                                      //subItem可能為Null 2015/2/5
                                                      if (subItems != null)
                                                      {
                                                            foreach (KeyValuePair<string, string> item in subItems)
                                                            {
                                                                  data += item.Value;
                                                            }
                                                            pos.TRACKINGVALUE = data;
                                                      }
                                                }

                                                pos.SAMPLINGSLOTFLAG = job.SamplingSlotFlag;
                                                pos.RECIPENAME = job.LineRecipeName;
                                                pos.PPID = job.PPID;
                                                pos.EQPRTCFLAG = job.RobotWIP.EQPRTCFlag.ToString();  //add by yang 
                                          }

                                          reply.BODY.POSITIONLIST.Add(pos);
                                          iPos++;
                                    }
                              }
                        }

                        if (reply.BODY.POSITIONLIST.Count > 0)
                              reply.BODY.POSITIONLIST.RemoveAt(0);

                        xMessage msg = SendReplyToOPI(command, reply);

                        if (ParameterManager[eREPORT_SWITCH.RECORD_OPI_STATUS_LOG].GetBoolean())
                        {
                              Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService reply message({2}) to OPI.",
                                  command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME));
                        }
                  }
                  catch (Exception ex)
                  {
                        //NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService catch message({2}) exception : {3} ",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME, ex.Message));
                  }
            }

            /// <summary>
            /// OPI MessageSet: Job Data Request to BC
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_JobDataRequest(XmlDocument xmlDoc)
            {
                  IServerAgent agent = GetServerAgent();
                  JobDataRequest command = Spec.XMLtoMessage(xmlDoc) as JobDataRequest;
                  XmlDocument xml_doc = agent.GetTransactionFormat("JobDataReply") as XmlDocument;
                  JobDataReply reply = Spec.XMLtoMessage(xml_doc) as JobDataReply;

                  try
                  {
                        Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI. CASSETTESEQNO={3}, JOBSEQNO={4}, GLASSID={5}",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME,
                            command.BODY.CASSETTESEQNO, command.BODY.JOBSEQNO, command.BODY.JOBID));

                        reply.BODY.LINENAME = command.BODY.LINENAME;
                        reply.BODY.CASSETTESEQUENCENO = command.BODY.CASSETTESEQNO;
                        reply.BODY.JOBSEQUENCENO = command.BODY.JOBSEQNO;

                        Line line = null;//sy 移到此處 ConvertJobJudge 需要by fabtype
                        if (command.BODY.LINENAME.Contains(eLineType.CELL.CBPMT))
                            line = GetLineByLines(keyCELLPMTLINE.CBPMI);
                        else
                            line = ObjectManager.LineManager.GetLine(command.BODY.LINENAME);
                        Job job = ObjectManager.JobManager.GetJob(command.BODY.CASSETTESEQNO, command.BODY.JOBSEQNO);

                        if (job == null)
                        {
                              reply.RETURN.RETURNCODE = "0010080";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Job[{0}] in JobEntity! CSTSeqNo[{1}]/JobSeqNo[{2}]",
                                  command.BODY.JOBID, command.BODY.CASSETTESEQNO, command.BODY.JOBSEQNO);

                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[JOB={0}] [BCS <- OPI][{1}] Can't find Job({0} in JobEntity, CassetteSeqNo({2}) / JobSeqNo({3}).",
                                  command.BODY.JOBID, command.HEADER.TRANSACTIONID, command.BODY.CASSETTESEQNO, command.BODY.JOBSEQNO));
                        }
                        else
                        {
                              reply.BODY.GLASSID = job.GlassChipMaskBlockID;
                              reply.BODY.GROUPINDEX = job.GroupIndex;
                              reply.BODY.PRODUCTTYPE = job.ProductType.Value.ToString();
                              reply.BODY.CSTOPERATIONMODE = string.Format("{0}: {1}", ((int)job.CSTOperationMode).ToString(), job.CSTOperationMode.ToString());
                              reply.BODY.SUBSTRATETYPE = string.Format("{0}: {1}", ((int)job.SubstrateType).ToString(), job.SubstrateType.ToString());
                              reply.BODY.CIMMODE = string.Format("{0}: {1}", ((int)job.CIMMode).ToString(), job.CIMMode.ToString());
                              reply.BODY.JOBTYPE = string.Format("{0}: {1}", ((int)job.JobType).ToString(), job.JobType.ToString());
                              reply.BODY.JOBJUDGE = ConvertJobJudge(job.JobJudge, line);
                              reply.BODY.SAMPLINGSLOTFLAG = ConvertSamplingSlotFlag(job.SamplingSlotFlag);
                              reply.BODY.FIRSTRUN = ConvertFirstRun(job.FirstRunFlag);
                              reply.BODY.JOBGRADE = job.JobGrade;
                              reply.BODY.PPID = job.PPID;

                              #region CSTCONTROL

                              reply.BODY.CSTCONTROL.SOURCECST = job.FromCstID;
                              reply.BODY.CSTCONTROL.TRAGETCST = job.TargetCSTID;
                              reply.BODY.CSTCONTROL.PRODUCTTYPE = job.ProductType.Value.ToString();
                              reply.BODY.CSTCONTROL.OWERTYPE = job.OWNERTYPE;
                              reply.BODY.CSTCONTROL.OWERID = job.MesProduct.OWNERID;
                              reply.BODY.CSTCONTROL.GROUPINDEX = job.GroupIndex;
                              if (job.MesCstBody.LOTLIST.Count != 0)
                              {
                                    reply.BODY.CSTCONTROL.PRODUCTSPECNAME = job.MesCstBody.LOTLIST[0].PRODUCTSPECNAME;
                                    reply.BODY.CSTCONTROL.PROCESSOPERATORNAME = job.MesCstBody.LOTLIST[0].PROCESSOPERATIONNAME;
                                    reply.BODY.CSTCONTROL.PRODUCTOWER = job.MesCstBody.LOTLIST[0].PRODUCTOWNER;
                              }

                              #endregion


                              string fabType = line.Data.FABTYPE;
                              string lineType = line.Data.LINETYPE;

                              #region Abnormal Flag
                              switch (fabType)
                              {
                                    case "ARRAY":
                                          #region Array Abnormal Flag

                                          reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = eJOBDATA.GlassFlowType, VVALUE = job.ArraySpecial.GlassFlowType });
                                          reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = eJOBDATA.ProcessType, VVALUE = ConvertProcessType(line.Data.LINETYPE, job.ArraySpecial.ProcessType) });//20150428 tom  ARRAY ProcessType有很多种所以需要从ConstantManager 中取得
                                          reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = eJOBDATA.LastGlassFlag, VVALUE = ConvertLastGlassFlag(job.LastGlassFlag) });
                                          reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = eJOBDATA.RTCFlag, VVALUE = ConvertRTC(job.ArraySpecial.RtcFlag) });
                                          reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = eJOBDATA.EQPRTCFlag, VVALUE = ConvertEQPRTC(job.RobotWIP.EQPRTCFlag.ToString()) });
                                          reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = eJOBDATA.MainEQInFlag, VVALUE = ConvertMainEQInFlag(job.ArraySpecial.MainEQInFlag) });
                                          //將TrackingData/EQPFlag/InspJudgedData/INSPReservations拆解細項後再報給OPI    
                                          //reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "InspJudgedData", VVALUE = job.InspJudgedData });
                                          if (!string.IsNullOrEmpty(job.InspJudgedData))
                                          {
                                                IDictionary<string, string> subItems = ObjectManager.SubJobDataManager.Decode(job.InspJudgedData, eJOBDATA.InspJudgedData);
                                                foreach (KeyValuePair<string, string> item in subItems)
                                                {
                                                      reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = string.Format("InspJudgedData_{0}", item.Key), VVALUE = item.Value });
                                                }
                                          }
                                          else
                                                reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = eJOBDATA.InspJudgedData, VVALUE = job.InspJudgedData });

                                          //reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "TrackingData", VVALUE = job.TrackingData });
                                          if (!string.IsNullOrEmpty(job.TrackingData))
                                          {
                                                IDictionary<string, string> subItems = ObjectManager.SubJobDataManager.Decode(job.TrackingData, "TrackingData");
                                                foreach (KeyValuePair<string, string> item in subItems)
                                                {
                                                      reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = string.Format("TrackingData_{0}", item.Key), VVALUE = item.Value });
                                                }
                                          }
                                          else
                                                reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = eJOBDATA.TrackingData, VVALUE = job.TrackingData });

                                          //reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "EQPFlag", VVALUE = job.EQPFlag });
                                          if (!string.IsNullOrEmpty(job.EQPFlag))
                                          {
                                                IDictionary<string, string> subItems = ObjectManager.SubJobDataManager.Decode(job.EQPFlag, "EQPFlag");
                                                foreach (KeyValuePair<string, string> item in subItems)
                                                {
                                                      reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = string.Format("EQPFlag_{0}", item.Key), VVALUE = item.Value });
                                                }
                                          }
                                          else
                                                reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = eJOBDATA.EQPFlag, VVALUE = job.EQPFlag });

                                          // reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "OXRInformation", VVALUE = job.OXRInformation }); // T3 不使用OXR Info
                                          reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = eJOBDATA.ChipCount, VVALUE = job.ChipCount.ToString() });
                                          reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = eJOBDATA.RecipeGroupNumber, VVALUE = job.ArraySpecial.RecipeGroupNumber });
                                          reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = eJOBDATA.SourcePortNo, VVALUE = job.ArraySpecial.SourcePortNo });
                                          reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = eJOBDATA.TargetPortNo, VVALUE = job.ArraySpecial.TargetPortNo });
                                          //    break;
                                          //case eJobDataLineType.ARRAY.TEST_1:
                                          //case eJobDataLineType.ARRAY.SORT:
                                          //    reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "SourcePortNo", VVALUE = job.ArraySpecial.SourcePortNo });
                                          //    reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "TargetPortNo", VVALUE = job.ArraySpecial.TargetPortNo });
                                          //    reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "TargetCSTID", VVALUE = job.TargetCSTID });
                                          //    break;
                                          //case eJobDataLineType.ARRAY.TEST_2:
                                          //    reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "SourcePortNo", VVALUE = job.ArraySpecial.SourcePortNo });
                                          //    reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "TargetPortNo", VVALUE = job.ArraySpecial.TargetPortNo });
                                          //    break;

                                          #endregion
                                          break;
                                    case "CF":
                                          #region CF Abnormal Flag

                                          #region AllLineCommon

                                          #region INSPReservations
                                          if (!string.IsNullOrEmpty(job.INSPReservations))
                                          {
                                                IDictionary<string, string> subItems = ObjectManager.SubJobDataManager.Decode(job.INSPReservations, "INSPReservations");
                                                foreach (KeyValuePair<string, string> item in subItems)
                                                {
                                                      reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = string.Format("INSPReservations_{0}", item.Key), VVALUE = item.Value });
                                                }
                                          }
                                          else
                                                reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "INSPReservations", VVALUE = job.INSPReservations });
                                          #endregion

                                          #region EQPReservations
                                          if (!string.IsNullOrEmpty(job.EQPReservations))
                                          {
                                                IDictionary<string, string> subItems = ObjectManager.SubJobDataManager.Decode(job.EQPReservations, "EQPReservations");
                                                foreach (KeyValuePair<string, string> item in subItems)
                                                {
                                                      reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = string.Format("EQPReservations_{0}", item.Key), VVALUE = item.Value });
                                                }
                                          }
                                          else
                                                reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "EQPReservations", VVALUE = job.EQPReservations });
                                          #endregion

                                          #region Insp.JudgeData
                                          if (!string.IsNullOrEmpty(job.InspJudgedData))
                                          {
                                                IDictionary<string, string> subItems = ObjectManager.SubJobDataManager.Decode(job.InspJudgedData, "Insp.JudgedData");
                                                foreach (KeyValuePair<string, string> item in subItems)
                                                {
                                                      reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = string.Format("Insp.JudgedData_{0}", item.Key), VVALUE = item.Value });
                                                }
                                          }
                                          else
                                                reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "Insp.JudgedData", VVALUE = job.InspJudgedData });
                                          #endregion

                                          #region TrackingData
                                          if (!string.IsNullOrEmpty(job.TrackingData))
                                          {
                                                IDictionary<string, string> subItems = ObjectManager.SubJobDataManager.Decode(job.TrackingData, "TrackingData");
                                                foreach (KeyValuePair<string, string> item in subItems)
                                                {
                                                      reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = string.Format("TrackingData_{0}", item.Key), VVALUE = item.Value });
                                                }
                                          }
                                          else
                                                reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "TrackingData", VVALUE = job.TrackingData });
                                          #endregion

                                          #region EQPFlag
                                          if (!string.IsNullOrEmpty(job.EQPFlag))
                                          {
                                                IDictionary<string, string> subItems = ObjectManager.SubJobDataManager.Decode(job.EQPFlag, "EQPFlag");
                                                foreach (KeyValuePair<string, string> item in subItems)
                                                {
                                                      reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = string.Format("EQPFlag_{0}", item.Key), VVALUE = item.Value });
                                                }
                                          }
                                          else
                                                reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "EQPFlag", VVALUE = job.EQPFlag });
                                          #endregion

                                          reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "LastGlassFlag", VVALUE = ConvertLastGlassFlag(job.LastGlassFlag) });
                                          reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "ChipCount", VVALUE = job.ChipCount.ToString() });
                                          reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "COAVersion", VVALUE = job.CfSpecial.COAversion });
                                          #endregion

                                          switch (lineType)
                                          {
                                                case "FCMPH_TYPE1":
                                                case "FCSPH_TYPE1":
                                                case "FCRPH_TYPE1":
                                                case "FCGPH_TYPE1":
                                                case "FCBPH_TYPE1":
                                                case "FCOPH_TYPE1":

                                                      #region Insp.JudgeData2
                                                      if (!string.IsNullOrEmpty(job.InspJudgedData2))
                                                      {
                                                            IDictionary<string, string> subItems = ObjectManager.SubJobDataManager.Decode(job.InspJudgedData2, "Insp.JudgedData2");
                                                            foreach (KeyValuePair<string, string> item in subItems)
                                                            {
                                                                  reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = string.Format("Insp.JudgedData2_{0}", item.Key), VVALUE = item.Value });
                                                            }
                                                      }
                                                      else
                                                            reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "Insp.JudgedData2", VVALUE = job.InspJudgedData2 });
                                                      #endregion

                                                      #region EQPFlag2
                                                      if (!string.IsNullOrEmpty(job.EQPFlag2))
                                                      {
                                                            IDictionary<string, string> subItems = ObjectManager.SubJobDataManager.Decode(job.EQPFlag2, "EQPFlag2");
                                                            foreach (KeyValuePair<string, string> item in subItems)
                                                            {
                                                                  reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = string.Format("EQPFlag2_{0}", item.Key), VVALUE = item.Value });
                                                            }
                                                      }
                                                      else
                                                            reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "EQPFlag2", VVALUE = job.EQPFlag2 });
                                                      #endregion

                                                      #region CFSpecialReserved
                                                      if (!string.IsNullOrEmpty(job.CFSpecialReserved))
                                                      {
                                                            IDictionary<string, string> subItems = ObjectManager.SubJobDataManager.Decode(job.CFSpecialReserved, "CFSpecialReserved");
                                                            foreach (KeyValuePair<string, string> item in subItems)
                                                            {
                                                                  reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = string.Format("CFSpecialReserved_{0}", item.Key), VVALUE = item.Value });
                                                            }
                                                      }
                                                      else
                                                            reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "CFSpecialReserved", VVALUE = job.CFSpecialReserved });
                                                      #endregion

                                                      #region MarcoReserveFlag
                                                      if (!string.IsNullOrEmpty(job.CFMarcoReserveFlag))
                                                      {
                                                            IDictionary<string, string> subItems = ObjectManager.SubJobDataManager.Decode(job.CFMarcoReserveFlag, "MarcoReserveFlag");
                                                            foreach (KeyValuePair<string, string> item in subItems)
                                                            {
                                                                  reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = string.Format("MarcoReserveFlag_{0}", item.Key), VVALUE = item.Value });
                                                            }
                                                      }
                                                      else
                                                            reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "MarcoReserveFlag", VVALUE = job.CFMarcoReserveFlag });
                                                      #endregion

                                                      reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "CoaterCSPNo", VVALUE = job.CfSpecial.CoaterCSPNo });
                                                      reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "TargetCassetteID", VVALUE = job.CfSpecial.TargetCSTID });
                                                      reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "TargetPortNo", VVALUE = job.CfSpecial.TargetPortNo });
                                                      reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "TargetSlotNo", VVALUE = job.CfSpecial.TargetSlotNo });
                                                      reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "ArrayPhotoPreInlineID", VVALUE = job.CfSpecial.AbnormalCode.ArrayPhotoPre_InlineID });
                                                      reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "OvenHPSlotNumber", VVALUE = job.CfSpecial.OvenHPSlotNumber });
                                                      reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "InLineReworkMaxCount", VVALUE = job.CfSpecial.InlineReworkMaxCount });
                                                      reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "InLineReworkRealCount", VVALUE = job.CfSpecial.InlineReworkRealCount });
                                                      reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "SamplingValue", VVALUE = job.CfSpecial.SamplingValue });

                                                      #region ProcessBackUp
                                                      if (!string.IsNullOrEmpty(job.CFProcessBackUp))
                                                      {
                                                            IDictionary<string, string> subItems = ObjectManager.SubJobDataManager.Decode(job.CFProcessBackUp, "ProcessBackUp");
                                                            foreach (KeyValuePair<string, string> item in subItems)
                                                            {
                                                                  reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = string.Format("ProcessBackUp_{0}", item.Key), VVALUE = item.Value });
                                                            }
                                                      }
                                                      else
                                                            reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "ProcessBackUp", VVALUE = job.CFProcessBackUp });
                                                      #endregion

                                                      break;

                                                case "FCMSK_TYPE1":
                                                      if (!string.IsNullOrEmpty(job.CFSpecialReserved))
                                                      {
                                                            IDictionary<string, string> subItems = ObjectManager.SubJobDataManager.Decode(job.CFSpecialReserved, "CFSpecialReserved");
                                                            foreach (KeyValuePair<string, string> item in subItems)
                                                            {
                                                                  reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = string.Format("CFSpecialReserved_{0}", item.Key), VVALUE = item.Value });
                                                            }
                                                      }
                                                      else
                                                            reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "CFSpecialReserved", VVALUE = job.CFSpecialReserved });
                                                      break;

                                                case "FCREW_TYPE1":
                                                      reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "TargetCassetteID", VVALUE = job.CfSpecial.TargetCSTID });
                                                      reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "TargetPortNo", VVALUE = job.CfSpecial.TargetPortNo });
                                                      reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "TargetSlotNo", VVALUE = job.CfSpecial.TargetSlotNo });
                                                      reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "SourcePortNo", VVALUE = job.CfSpecial.SourcePortNo });
                                                      reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "ReworkMaxCount", VVALUE = job.CfSpecial.ReworkMaxCount });
                                                      reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "ReworkRealCount", VVALUE = job.CfSpecial.ReworkRealCount });
                                                      reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "VCRMismatchFlag", VVALUE = ConvertVCRMismatchFlag(job.CfSpecial.VCRMismatchFlag) });
                                                      break;

                                                case "FCREP_TYPE1":
                                                case "FCREP_TYPE2":
                                                case "FCREP_TYPE3":
                                                      reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "TargetCassetteID", VVALUE = job.CfSpecial.TargetCSTID });
                                                      reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "TargetPortNo", VVALUE = job.CfSpecial.TargetPortNo });
                                                      reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "TargetSlotNo", VVALUE = job.CfSpecial.TargetSlotNo });
                                                      reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "SourcePortNo", VVALUE = job.CfSpecial.SourcePortNo });
                                                      reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "RTCFlag", VVALUE = ConvertRTC(job.CfSpecial.RTCFlag) });
                                                      reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "LoaderBufferingFlag", VVALUE = ConvertLoaderBufferingFlag(job.CfSpecial.LoaderBufferingFlag) });
                                                      reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "VCRMismatchFlag", VVALUE = ConvertVCRMismatchFlag(job.CfSpecial.VCRMismatchFlag) });
                                                      break;

                                                case "FCMAC_TYPE1":
                                                      reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "TargetCassetteID", VVALUE = job.CfSpecial.TargetCSTID });
                                                      reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "TargetPortNo", VVALUE = job.CfSpecial.TargetPortNo });
                                                      reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "TargetSlotNo", VVALUE = job.CfSpecial.TargetSlotNo });
                                                      reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "SourcePortNo", VVALUE = job.CfSpecial.SourcePortNo });
                                                      reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "RTCFlag", VVALUE = ConvertRTC(job.CfSpecial.RTCFlag) });
                                                      reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "LoaderBufferingFlag", VVALUE = ConvertLoaderBufferingFlag(job.CfSpecial.LoaderBufferingFlag) });
                                                      break;

                                                case "FCMQC_TYPE1":
                                                case "FCMQC_TYPE2":
                                                      reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "TargetCassetteID", VVALUE = job.CfSpecial.TargetCSTID });
                                                      reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "TargetPortNo", VVALUE = job.CfSpecial.TargetPortNo });
                                                      reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "TargetSlotNo", VVALUE = job.CfSpecial.TargetSlotNo });
                                                      reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "SourcePortNo", VVALUE = job.CfSpecial.SourcePortNo });
                                                      reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "RTCFlag", VVALUE = ConvertRTC(job.CfSpecial.RTCFlag) });
                                                      reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "LoaderBufferingFlag", VVALUE = ConvertLoaderBufferingFlag(job.CfSpecial.LoaderBufferingFlag) });
                                                      reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "FlowPriorityInfo", VVALUE = job.CfSpecial.FlowPriorityInfo });
                                                      break;

                                                case "FCSRT_TYPE1":
                                                      reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "TargetCassetteID", VVALUE = job.CfSpecial.TargetCSTID });
                                                      reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "TargetPortNo", VVALUE = job.CfSpecial.TargetPortNo });
                                                      reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "TargetSlotNo", VVALUE = job.CfSpecial.TargetSlotNo });
                                                      reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "SourcePortNo", VVALUE = job.CfSpecial.SourcePortNo });
                                                      reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "VCRMismatchFlag", VVALUE = ConvertVCRMismatchFlag(job.CfSpecial.VCRMismatchFlag) });
                                                      break;

                                                case "FCPSH_TYPE1":
                                                      reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "TargetCassetteID", VVALUE = job.CfSpecial.TargetCSTID });
                                                      reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "TargetPortNo", VVALUE = job.CfSpecial.TargetPortNo });
                                                      reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "TargetSlotNo", VVALUE = job.CfSpecial.TargetSlotNo });
                                                      reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "SourcePortNo", VVALUE = job.CfSpecial.SourcePortNo });
                                                      reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "RTCFlag", VVALUE = job.CfSpecial.RTCFlag });
                                                      reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "LoaderBufferingFlag", VVALUE = job.CfSpecial.LoaderBufferingFlag });

                                                      if (!string.IsNullOrEmpty(job.CFSpecialReserved))
                                                      {
                                                            IDictionary<string, string> subItems = ObjectManager.SubJobDataManager.Decode(job.CFSpecialReserved, "CFSpecialReserved");
                                                            foreach (KeyValuePair<string, string> item in subItems)
                                                            {
                                                                  reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = string.Format("CFSpecialReserved_{0}", item.Key), VVALUE = item.Value });
                                                            }
                                                      }
                                                      else
                                                            reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "CFSpecialReserved", VVALUE = job.CFSpecialReserved });
                                                      break;

                                                case "FCAOI_TYPE1":
                                                      reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "RTCFlag", VVALUE = job.CfSpecial.RTCFlag });
                                                      reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "LoaderBufferingFlag", VVALUE = job.CfSpecial.LoaderBufferingFlag });
                                                      break;
                                          }
                                          #endregion
                                          break;
                                    case "CELL":
                                          #region CELL Abnormal Flag

                                          #region INSPReservations
                                          if (!string.IsNullOrEmpty(job.INSPReservations))
                                          {
                                                IDictionary<string, string> subItems = ObjectManager.SubJobDataManager.Decode(job.INSPReservations, eJOBDATA.INSPReservations);
                                                if (subItems != null && subItems.Count > 0)
                                                {
                                                      foreach (KeyValuePair<string, string> item in subItems)
                                                      {
                                                            reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = string.Format("{0}_{1}", eJOBDATA.INSPReservations, item.Key), VVALUE = item.Value });
                                                      }
                                                }
                                                else
                                                      reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = eJOBDATA.INSPReservations, VVALUE = job.INSPReservations });
                                          }
                                          else
                                                reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = eJOBDATA.INSPReservations, VVALUE = job.INSPReservations });

                                          #endregion
                                          #region EQPReservations
                                          if (!string.IsNullOrEmpty(job.EQPReservations))
                                          {

                                                IDictionary<string, string> subItems = ObjectManager.SubJobDataManager.Decode(job.EQPReservations, eJOBDATA.EQPReservations);
                                                if (subItems != null && subItems.Count > 0)
                                                {
                                                      foreach (KeyValuePair<string, string> item in subItems)
                                                      {
                                                            reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = string.Format("{0}_{1}", eJOBDATA.EQPReservations, item.Key), VVALUE = item.Value });
                                                      }
                                                }
                                                else
                                                      reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = eJOBDATA.EQPReservations, VVALUE = job.EQPReservations });
                                          }
                                          else
                                                reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = eJOBDATA.EQPReservations, VVALUE = job.EQPReservations });

                                          #endregion
                                          reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = eJOBDATA.LastGlassFlag, VVALUE = ConvertLastGlassFlag(job.LastGlassFlag) });
                                          #region InspJudgedData
                                          if (!string.IsNullOrEmpty(job.InspJudgedData))
                                          {
                                                IDictionary<string, string> subItems = ObjectManager.SubJobDataManager.Decode(job.InspJudgedData, eJOBDATA.InspJudgedData);
                                                if (subItems != null && subItems.Count > 0)
                                                {
                                                      foreach (KeyValuePair<string, string> item in subItems)
                                                      {
                                                            reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = string.Format("{0}_{1}", eJOBDATA.InspJudgedData, item.Key), VVALUE = item.Value });
                                                      }
                                                }
                                                else
                                                      reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = eJOBDATA.InspJudgedData, VVALUE = job.InspJudgedData });
                                          }
                                          else
                                                reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = eJOBDATA.InspJudgedData, VVALUE = job.InspJudgedData });

                                          #endregion
                                          #region TrackingData
                                          if (!string.IsNullOrEmpty(job.TrackingData))
                                          {
                                                IDictionary<string, string> subItems = ObjectManager.SubJobDataManager.Decode(job.TrackingData, eJOBDATA.TrackingData);
                                                if (subItems != null && subItems.Count > 0)
                                                {
                                                      foreach (KeyValuePair<string, string> item in subItems)
                                                      {
                                                            reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = string.Format("{0}_{1}", eJOBDATA.TrackingData, item.Key), VVALUE = item.Value });
                                                      }
                                                }
                                                else
                                                      reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = eJOBDATA.TrackingData, VVALUE = job.TrackingData });
                                          }
                                          else
                                                reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = eJOBDATA.TrackingData, VVALUE = job.TrackingData });

                                          #endregion
                                          #region EQPFlag
                                          if (!string.IsNullOrEmpty(job.EQPFlag))
                                          {
                                                IDictionary<string, string> subItems = ObjectManager.SubJobDataManager.Decode(job.EQPFlag, eJOBDATA.EQPFlag);
                                                if (subItems != null && subItems.Count > 0)
                                                {
                                                      foreach (KeyValuePair<string, string> item in subItems)
                                                      {
                                                            reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = string.Format("{0}_{1}", eJOBDATA.EQPFlag, item.Key), VVALUE = item.Value });
                                                      }
                                                }
                                                else
                                                      reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = eJOBDATA.EQPFlag, VVALUE = job.EQPFlag });
                                          }
                                          else
                                                reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = eJOBDATA.EQPFlag, VVALUE = job.EQPFlag });

                                          #endregion
                                          reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = eJOBDATA.ChipCount, VVALUE = job.ChipCount.ToString() });
                                          reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = eJOBDATA.ProductID, VVALUE = job.CellSpecial.ProductID });
                                          reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = eJOBDATA.CassetteSettingCode, VVALUE = job.CellSpecial.CassetteSettingCode });

                                          switch (lineType)
                                          {
                                              #region [T3 USE]
                                              case eLineType.CELL.CCPIL://shihyang add for T3 20150929
                                              case eLineType.CELL.CCPIL_2:
                                                  reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "BlockOXInformation", VVALUE = job.CellSpecial.BlockOXInformation.ToString() });
                                                  reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "TurnAngle", VVALUE = ConvertTurnAngle(job.CellSpecial.TurnAngle) });
                                                  reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "GlassThickness", VVALUE = job.CellSpecial.GlassThickness.ToString() });
                                                  reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "OperationID", VVALUE = job.CellSpecial.OperationID.ToString() });
                                                  reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "OwnerID", VVALUE = job.CellSpecial.OwnerID.ToString() });
                                                  reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "ProductOwner", VVALUE = ConvertProductOwner(job.CellSpecial.ProductOwner.ToString()) });
                                                  reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "PILiquidType", VVALUE = job.CellSpecial.PILiquidType.ToString() });
                                                  break;
                                              case eLineType.CELL.CCODF://shihyang add for T3 20151008
                                              case eLineType.CELL.CCODF_2://sy add 20160907
                                                  //huangjiayin modify 20170120 BlockOxInfor 不用拆
                                                  reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = eJOBDATA.BlockOXInformation, VVALUE = job.CellSpecial.BlockOXInformation });
                                                  reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = eJOBDATA.TurnAngle, VVALUE = ConvertTurnAngle(job.CellSpecial.TurnAngle) });
                                                  reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = eJOBDATA.GlassThickness, VVALUE = job.CellSpecial.GlassThickness.ToString() });
                                                  reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = eJOBDATA.OperationID, VVALUE = job.CellSpecial.OperationID.ToString() });
                                                  reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = eJOBDATA.ProductOwner, VVALUE = ConvertProductOwner(job.CellSpecial.ProductOwner.ToString()) });
                                                  reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = eJOBDATA.OwnerID, VVALUE = job.CellSpecial.OwnerID.ToString() });
                                                  reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = eJOBDATA.AssembleSeqNo, VVALUE = job.CellSpecial.AssembleSeqNo.ToString() });
                                                  reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = eJOBDATA.UVMaskAlreadyUseCount, VVALUE = job.CellSpecial.UVMaskAlreadyUseCount.ToString() });
                                                  break;
                                              case eLineType.CELL.CCPCS://shihyang add for T3 20151008
                                                  reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "BlockOXInformation", VVALUE = job.CellSpecial.BlockOXInformation.ToString() });
                                                  reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "TurnAngle", VVALUE = ConvertTurnAngle(job.CellSpecial.TurnAngle) });
                                                  reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "GlassThickness", VVALUE = job.CellSpecial.GlassThickness.ToString() });
                                                  reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "OperationID", VVALUE = job.CellSpecial.OperationID.ToString() });
                                                  reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "OwnerID", VVALUE = job.CellSpecial.OwnerID.ToString() });
                                                  reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "ProductOwner", VVALUE = ConvertProductOwner(job.CellSpecial.ProductOwner.ToString()) });
                                                  reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "BlockSize", VVALUE = job.CellSpecial.BlockSize.ToString() });
                                                  //20170717 huangjiayin modify: PCS new Rule
                                                  reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "PCSCassetteSettingCodeList", VVALUE = job.CellSpecial.PCSCassetteSettingCodeList.ToString() });
                                                  reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "PCSCassetteSettingCode", VVALUE = job.CellSpecial.CUTCassetteSettingCode.ToString() });
                                                  reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "PCSCassetteSettingCode2", VVALUE = job.CellSpecial.CUTCassetteSettingCode2.ToString() });
                                                  //20170725 huangjiayin add BlockSizeList, BlockSize1, BlockSize2
                                                  reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "PCSBlockSizeList", VVALUE = job.CellSpecial.PCSBlockSizeList.ToString() });
                                                  reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "BlockSize1", VVALUE = job.CellSpecial.BlockSize1.ToString() });
                                                  reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "BlockSize2", VVALUE = job.CellSpecial.BlockSize2.ToString() });
                                                 // reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "PCSProductID", VVALUE = job.CellSpecial.CUTProductID.ToString() });
                                                 // reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "PCSProductType", VVALUE = job.CellSpecial.CUTProductType.ToString() });
                                                //  reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "PCSProductID2", VVALUE = job.CellSpecial.CUTProductID2.ToString() });
                                                //  reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "PCSProductType2", VVALUE = job.CellSpecial.CUTProductType2.ToString() });
                                                  break;
                                              case eLineType.CELL.CCPCK://shihyang add for T3 20151008
                                                  reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "PanelGroup", VVALUE = job.CellSpecial.PanelGroup.ToString() });
                                                  reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "OQCBank", VVALUE = job.CellSpecial.OQCBank.ToString() });
                                                  break;
                                              case eLineType.CELL.CCPDR:
                                                  reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "TurnAngle", VVALUE = ConvertTurnAngle(job.CellSpecial.TurnAngle) });
                                                  reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "GlassThickness", VVALUE = job.CellSpecial.GlassThickness.ToString() });
                                                  reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "OperationID", VVALUE = job.CellSpecial.OperationID.ToString() });
                                                  reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "OwnerID", VVALUE = job.CellSpecial.OwnerID.ToString() });
                                                  reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "ProductOwner", VVALUE = ConvertProductOwner(job.CellSpecial.ProductOwner.ToString()) });
                                                  reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "MaxRwkCount", VVALUE = job.CellSpecial.MaxRwkCount.ToString() });
                                                  reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "CurrentRwkCount", VVALUE = job.CellSpecial.CurrentRwkCount.ToString() });
                                                  break;
                                              case eLineType.CELL.CCTAM:
                                              case eLineType.CELL.CCPTH:
                                              case eLineType.CELL.CCGAP:
                                                  reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "TurnAngle", VVALUE = ConvertTurnAngle(job.CellSpecial.TurnAngle) });
                                                  reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "GlassThickness", VVALUE = job.CellSpecial.GlassThickness.ToString() });
                                                  reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "OperationID", VVALUE = job.CellSpecial.OperationID.ToString() });
                                                  reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "OwnerID", VVALUE = job.CellSpecial.OwnerID.ToString() });
                                                  reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "ProductOwner", VVALUE = ConvertProductOwner(job.CellSpecial.ProductOwner.ToString()) });
                                                  break;
                                              case eLineType.CELL.CCRWT:
                                              case eLineType.CELL.CCCRP:
                                              case eLineType.CELL.CCCRP_2:
                                                  reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "DotRepairCount", VVALUE = job.CellSpecial.DotRepairCount.ToString() });
                                                  reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "LineRepairCount", VVALUE = job.CellSpecial.LineRepairCount.ToString() });
                                                  reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "DefectCode", VVALUE = job.CellSpecial.DefectCode.ToString() });
                                                  break;
                                              case eLineType.CELL.CCSOR:
                                              case eLineType.CELL.CCRWK:
                                              case eLineType.CELL.CCQUP:
                                              case eLineType.CELL.CCCHN:
                                              case eLineType.CELL.CCQSR:
                                                  break;
                                              case eLineType.CELL.CCNLS:
                                              case eLineType.CELL.CCNRD:
                                                  reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "PanelOXInformation", VVALUE = job.CellSpecial.PanelOXInformation.ToString() });
                                                  reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "TurnAngle", VVALUE = ConvertTurnAngle(job.CellSpecial.TurnAngle) });
                                                  reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "PanelSize", VVALUE = job.CellSpecial.PanelSize.ToString() });
                                                  break;
                                              #endregion
                                              default:
                                                  if (line.Data.LINETYPE.Contains(keyCellLineType.CUT))//sy modify  20160705 放在deault
                                                      #region [CUT]
                                                  {
                                                      reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "PanelOXInformation", VVALUE = job.CellSpecial.PanelOXInformation.ToString() });
                                                      reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "TurnAngle", VVALUE = ConvertTurnAngle(job.CellSpecial.TurnAngle) });
                                                      reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "GlassThickness", VVALUE = job.CellSpecial.GlassThickness.ToString() });
                                                      reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "OperationID", VVALUE = job.CellSpecial.OperationID.ToString() });
                                                      reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "OwnerID", VVALUE = job.CellSpecial.OwnerID.ToString() });
                                                      reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "ProductOwner", VVALUE = ConvertProductOwner(job.CellSpecial.ProductOwner.ToString()) });
                                                      reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "PanelSize", VVALUE = job.CellSpecial.PanelSize.ToString() });
                                                      reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "CUTProductID", VVALUE = job.CellSpecial.CUTProductID.ToString() });
                                                      reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "CUTProductType", VVALUE = job.CellSpecial.CUTProductType.ToString() });
                                                      reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "DefectCode", VVALUE = job.CellSpecial.DefectCode.ToString() });
                                                      reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "RejudgeCount", VVALUE = job.CellSpecial.RejudgeCount.ToString() });
                                                      reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "VendorName", VVALUE = job.CellSpecial.VendorName.ToString() });
                                                      reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "BURCheckCount", VVALUE = job.CellSpecial.BURCheckCount.ToString() });
                                                      reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "CUTCassetteSettingCode", VVALUE = job.CellSpecial.CUTCassetteSettingCode.ToString() });
                                                  }
                                                  #endregion
                                                  else if (line.Data.LINETYPE.Contains(keyCellLineType.POL))//sy modify  20160705 放在deault
                                                      #region [POL]
                                                  {
                                                      reply.BODY.ABNORMALFLAGLIST.Add(new JobDataReply.ABNORMALFLAGc() { VNAME = "DefectCode", VVALUE = job.CellSpecial.DefectCode.ToString() });
                                                  }                                                  
                                                  #endregion
                                                  break;
                                          }
                                          #endregion
                                          break;
                              }
                              #endregion
                        }

                        if (reply.BODY.ABNORMALFLAGLIST.Count > 0)
                              reply.BODY.ABNORMALFLAGLIST.RemoveAt(0);

                        xMessage msg = SendReplyToOPI(command, reply);

                        if (ParameterManager[eREPORT_SWITCH.RECORD_OPI_STATUS_LOG].GetBoolean())
                        {
                              Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService reply message({2}) to OPI.",
                                  command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME));
                        }
                  }
                  catch (Exception ex)
                  {
                        //NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService catch message({2}) exception : {3} ",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME, ex.Message));
                  }
            }

            /// <summary>
            /// OPI MessageSet: Job Data Category Request to BC
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_JobDataCategoryRequest(XmlDocument xmlDoc)
            {
                  IServerAgent agent = GetServerAgent();
                  JobDataCategoryRequest command = Spec.XMLtoMessage(xmlDoc) as JobDataCategoryRequest;
                  XmlDocument xml_doc = agent.GetTransactionFormat("JobDataCategoryReply") as XmlDocument;
                  JobDataCategoryReply reply = Spec.XMLtoMessage(xml_doc) as JobDataCategoryReply;

                  try
                  {
                        Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI. EQUIPMENTNO={3}",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME, command.BODY.EQUIPMENTNO));

                        reply.BODY.LINENAME = command.BODY.LINENAME;

                        //Line line = ObjectManager.LineManager.GetLine(command.BODY.LINENAME);
                        IList<Line> lines = ObjectManager.LineManager.GetLines();
                        if (lines == null)
                        {
                              reply.RETURN.RETURNCODE = "0010090";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Line[{0}] in LineEntity!", command.BODY.LINENAME);

                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't find Line({0}) in LineEntity.",
                                  command.BODY.LINENAME, command.HEADER.TRANSACTIONID));
                        }
                        else
                        {
                              //Jun Add 20150515
                              Line line = ObjectManager.LineManager.GetLine(command.BODY.LINENAME);

                              //先將JobData依照Equipment分類
                              IList<Job> jobs = ObjectManager.JobManager.GetJobs();
                              List<Job> query = jobs.ToList<Job>();

                              if (!string.IsNullOrEmpty(command.BODY.EQUIPMENTNO) && command.BODY.EQUIPMENTNO != "00")
                                    query = query.Where(j => j.CurrentEQPNo == command.BODY.EQUIPMENTNO).ToList<Job>();
                              if (!string.IsNullOrEmpty(command.BODY.UNITNO) && command.BODY.UNITNO != "00")
                                  query = query.Where(j => j.CurrentUNITNo == (int.Parse(command.BODY.UNITNO)).ToString()).ToList<Job>();   //modify by bruce 20160203 修正UnitNo 長度問題
                              if (!string.IsNullOrEmpty(command.BODY.CASSETTESEQNO))
                                    query = query.Where(j => j.CassetteSequenceNo == command.BODY.CASSETTESEQNO).ToList<Job>();
                              if (!string.IsNullOrEmpty(command.BODY.JOBSEQNO))
                                    query = query.Where(j => j.JobSequenceNo == command.BODY.JOBSEQNO).ToList<Job>();
                              if (!string.IsNullOrEmpty(command.BODY.GLASSID))
                              {
                                    //Jun Add 20150515 如果是CBUAM要拿MaskID跟來OPI傳的ID比
                                    if (line != null && line.Data.LINETYPE == eLineType.CELL.CBMCL)
                                          query = query.Where(j => j.CellSpecial.MASKID == command.BODY.GLASSID).ToList<Job>();
                                    else
                                          query = query.Where(j => j.GlassChipMaskBlockID == command.BODY.GLASSID).ToList<Job>();
                              }
                              if (command.BODY.REMOVEFLAG == "Y")
                                    query = query.Where(j => j.RemoveFlag == true).ToList<Job>();
                              else
                                    query = query.Where(j => j.RemoveFlag == false).ToList<Job>();

                              Dictionary<string, string> port_slot_position = null;//key=CstSeq_JobSeq, value=string.Empty
                              if (!string.IsNullOrEmpty(command.BODY.EQUIPMENTNO) && command.BODY.EQUIPMENTNO != "00" &&
                                  !string.IsNullOrEmpty(command.BODY.PORTNO) && command.BODY.PORTNO != "00")
                              {
                                    port_slot_position = new Dictionary<string, string>();
                                    //L2_Port#01JobEachCassetteSlotPositionBlock
                                    string trx_name = string.Format("{0}_Port#{1}JobEachCassetteSlotPositionBlock", command.BODY.EQUIPMENTNO, command.BODY.PORTNO);
                                    Trx trx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trx_name, true }) as Trx;
                                    if (trx != null)
                                    {
                                          for (int i = 0; i < trx.EventGroups[0].Events[0].Items.Count; i += 2)
                                          {
                                                string cst_seq = trx.EventGroups[0].Events[0].Items[i].Value;
                                                string job_seq = trx.EventGroups[0].Events[0].Items[i + 1].Value;
                                                string key = string.Format("{0}_{1}", cst_seq, job_seq);
                                                if (!port_slot_position.ContainsKey(key))
                                                {
                                                      port_slot_position.Add(key, string.Empty);
                                                }
                                          }
                                    }
                                    else
                                    {
                                          reply.RETURN.RETURNCODE = "0010090";
                                          reply.RETURN.RETURNMESSAGE = string.Format("PLCAgent SyncReadTrx({0}) return null", trx_name);
                                    }
                              }

                              Dictionary<string, List<JobDataCategoryReply.JOBc>> dicJob = new Dictionary<string, List<JobDataCategoryReply.JOBc>>();

                              foreach (Job job in query)
                              {
                                    JobDataCategoryReply.JOBc _job = new JobDataCategoryReply.JOBc();
                                    _job.CASSETTESEQNO = job.CassetteSequenceNo;
                                    _job.SLOTNO = job.CurrentSlotNo == "" ? "0" : job.CurrentSlotNo;    // add by bruce 20160412 for T2 Issue
                                    //_job.SLOTNO = job.ToSlotNo == "0" ? job.JobSequenceNo : job.ToSlotNo;
                                    _job.JOBSEQNO = job.JobSequenceNo;
                                    _job.PRODUCTTYPE = job.ProductType.Value.ToString();
                                    _job.JOBTYPE = string.Format("{0}:{1}", (int)job.JobType, job.JobType.ToString());
                                    _job.JOBJUDGE = ConvertJobJudge(job.JobJudge,line);
                                    _job.JOBGRADE = job.JobGrade;
                                    if (line != null && line.Data.LINETYPE == eLineType.CELL.CBMCL)
                                          _job.GLASSID = job.CellSpecial.MASKID;
                                    else
                                          _job.GLASSID = job.GlassChipMaskBlockID;
                                    _job.PPID = job.PPID;
                                    _job.TRACKINGDATA = job.TrackingData;
                                    _job.LINERECIPENAME = job.LineRecipeName;
                                    //_job.OXR = job.OXRInformation;

                                    _job.SAMPLINGFLAG = job.SamplingSlotFlag;
                                    if (job.MesProduct != null) _job.OWNERID = job.MesProduct.OWNERID;

                                    if (job.MesCstBody != null)
                                    {
                                        if (job.MesCstBody.LOTLIST.Count > 0)
                                        {
                                            _job.PRODUCTSPECVER = job.MesCstBody.LOTLIST[0].PRODUCTSPECVER;
                                            _job.PROCESSOPERATIONNAME = job.MesCstBody.LOTLIST[0].PROCESSOPERATIONNAME;
                                        }
                                    }

                                    if (port_slot_position != null)
                                    {
                                          if (port_slot_position.ContainsKey(string.Format("{0}_{1}", _job.CASSETTESEQNO, _job.JOBSEQNO)))
                                          {
                                                if (!dicJob.ContainsKey(job.CurrentEQPNo))
                                                      dicJob.Add(job.CurrentEQPNo, new List<JobDataCategoryReply.JOBc>());

                                                dicJob[job.CurrentEQPNo].Add(_job);
                                          }
                                    }
                                    else
                                    {
                                          if (!dicJob.ContainsKey(job.CurrentEQPNo))
                                                dicJob.Add(job.CurrentEQPNo, new List<JobDataCategoryReply.JOBc>());

                                          dicJob[job.CurrentEQPNo].Add(_job);
                                    }
                              }

                              //for (int i = 0; i < jobs.Count; i++)
                              //{
                              //    if (command.BODY.EQUIPMENTNO != "00" && command.BODY.EQUIPMENTNO == jobs[i].CurrentEQPNo)
                              //    {
                              //        //if EQ remove job, OPI can't show this job
                              //        if (!jobs[i].RemoveFlag)
                              //        {
                              //            JobDataCategoryReply.JOBc _job = new JobDataCategoryReply.JOBc();
                              //            _job.CASSETTESEQNO = jobs[i].CassetteSequenceNo;
                              //            _job.SLOTNO = jobs[i].ToSlotNo == "0" ? jobs[i].JobSequenceNo : jobs[i].ToSlotNo;
                              //            _job.JOBSEQNO = jobs[i].JobSequenceNo;
                              //            _job.PRODUCTTYPE = jobs[i].ProductType.Value.ToString();
                              //            _job.JOBTYPE = jobs[i].JobType.ToString();
                              //            _job.JOBJUDGE = jobs[i].JobJudge;
                              //            _job.JOBGRADE = jobs[i].JobGrade;
                              //            _job.GLASSID = jobs[i].GlassChipMaskBlockID;
                              //            _job.PPID = jobs[i].PPID;
                              //            _job.TRACKINGDATA = jobs[i].TrackingData;
                              //            _job.OXR = jobs[i].OXRInformation;

                              //            if (!dicJob.ContainsKey(jobs[i].CurrentEQPNo))
                              //                dicJob.Add(jobs[i].CurrentEQPNo, new List<JobDataCategoryReply.JOBc>());

                              //            dicJob[jobs[i].CurrentEQPNo].Add(_job);
                              //        }
                              //    }
                              //    else if (command.BODY.EQUIPMENTNO == "00" && jobs[i].CurrentEQPNo != "")
                              //    {
                              //        if (!jobs[i].RemoveFlag)
                              //        {
                              //            JobDataCategoryReply.JOBc _job = new JobDataCategoryReply.JOBc();
                              //            _job.CASSETTESEQNO = jobs[i].CassetteSequenceNo;
                              //            _job.SLOTNO = jobs[i].ToSlotNo == "0" ? jobs[i].JobSequenceNo : jobs[i].ToSlotNo;
                              //            _job.JOBSEQNO = jobs[i].JobSequenceNo;
                              //            _job.PRODUCTTYPE = jobs[i].ProductType.Value.ToString();
                              //            _job.JOBTYPE = jobs[i].JobType.ToString();
                              //            _job.JOBJUDGE = jobs[i].JobJudge;
                              //            _job.JOBGRADE = jobs[i].JobGrade;
                              //            _job.GLASSID = jobs[i].GlassChipMaskBlockID;
                              //            _job.PPID = jobs[i].PPID;
                              //            _job.TRACKINGDATA = jobs[i].TrackingData;
                              //            _job.OXR = jobs[i].OXRInformation;

                              //            if (!dicJob.ContainsKey(jobs[i].CurrentEQPNo))
                              //                dicJob.Add(jobs[i].CurrentEQPNo, new List<JobDataCategoryReply.JOBc>());

                              //            dicJob[jobs[i].CurrentEQPNo].Add(_job);
                              //        }
                              //    }
                              //}

                              //依照OPI format填入資料
                              foreach (KeyValuePair<string, List<JobDataCategoryReply.JOBc>> eqpItem in dicJob)
                              {
                                    JobDataCategoryReply.EQUIPMENTITEMc item = new JobDataCategoryReply.EQUIPMENTITEMc();
                                    item.EQUIPMENTNO = eqpItem.Key;
                                    item.JOBDATALIST = eqpItem.Value;
                                    item.PORTNO = command.BODY.PORTNO;

                                    reply.BODY.EQUIPMENTLIST.Add(item);
                              }
                        }

                        if (reply.BODY.EQUIPMENTLIST.Count > 0)
                              reply.BODY.EQUIPMENTLIST.RemoveAt(0);

                        xMessage msg = SendReplyToOPI(command, reply);

                        if (ParameterManager[eREPORT_SWITCH.RECORD_OPI_STATUS_LOG].GetBoolean())
                        {
                              Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService reply message({2}) to OPI.",
                                  command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME));
                        }
                  }
                  catch (Exception ex)
                  {
                        //NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService catch message({2}) exception : {3} ",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME, ex.Message));
                  }
            }

            /// <summary>
            /// OPI MessageSet: Cassette Map Download Result Report
            /// </summary>
            /// <param name="port"></param>
            /// <param name="result"></param>
            public void CassetteMapDownloadResultReport(Port port, int result)
            {
                  try
                  {
                        IServerAgent agent = GetServerAgent();
                        XmlDocument xml_doc = agent.GetTransactionFormat("CassetteMapDownloadResultReport") as XmlDocument;
                        CassetteMapDownloadResultReport trx = Spec.XMLtoMessage(xml_doc) as CassetteMapDownloadResultReport;
                        //trx.BODY.LINENAME = port.Data.LINEID;
                        trx.BODY.LINENAME = port.Data.SERVERNAME;
                        trx.BODY.EQUIPMENTNO = port.Data.NODENO;
                        trx.BODY.PORTNO = port.Data.PORTNO;
                        trx.BODY.PORTID = port.Data.PORTID;
                        trx.BODY.CASSETTEID = port.File.CassetteID.Trim();
                        //以數值傳給OPI，OPI會自己轉換
                        trx.BODY.RESULT = result.ToString();

                        xMessage msg = SendReportToAllOPI(string.Empty, trx);

                        if (dicClient.Count == 0)
                        {
                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                              string.Format("[LINENAME={0}] [BCS -> OPI][{1}] OPI client number is zero, UIService will not report message to OPI.s",
                              trx.BODY.LINENAME, msg.TransactionID));
                        }
                        else
                        {
                              Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                              string.Format("[LINENAME={0}] [BCS -> OPI][{1}] Cassette Mapping Download Result cassette[{2}] result [{2}].",
                              trx.BODY.LINENAME, msg.TransactionID, trx.BODY.CASSETTEID, trx.BODY.RESULT));
                        }
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }

            ///// <summary>
            ///// OPI MessageSet: Cassette Map Download Result Reply
            ///// </summary>
            ///// <param name="xml"></param>
            //public void OPI_CassetteMapDownloadResultReply(XmlDocument xmlDoc)
            //{
            //    try
            //    {
            //        //IServerAgent agent = GetServerAgent();
            //        //{
            //        //    Message lineStatusReportReply = Spec.CheckXMLFormat(xmlDoc);
            //        //}
            //    }
            //    catch (Exception ex)
            //    {
            //        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            //    }
            //}

            ///// <summary>
            ///// OPI MessageSet: Cassette Map Download Result Report for DPI
            ///// </summary>
            ///// <param name="ports"></param>
            ///// <param name="results"></param>
            //public void CassetteMapDownloadResultReport_DPI(Dictionary<Port, int> ports)
            //{
            //    try
            //    {
            //        IServerAgent agent = GetServerAgent();
            //        XmlDocument xml_doc = agent.GetTransactionFormat("CassetteMapDownloadResultReport_DPI") as XmlDocument;
            //        CassetteMapDownloadResultReport_DPI trx = Spec.XMLtoMessage(xml_doc) as CassetteMapDownloadResultReport_DPI;
            //        trx.BODY.LINENAME = Workbench.ServerName;

            //        //string eqpNo = trx.BODY.EQUIPMENTNO; // string.Empty; Watson Modify 20150415 For Rebeca

            //        trx.BODY.PORTLIST.Clear();
            //        foreach (KeyValuePair<Port, int> port in ports)
            //        {
            //            trx.BODY.EQUIPMENTNO = port.Key.Data.NODENO; // Watson Modify 20150415 For Rebeca
            //            trx.BODY.PORTLIST.Add(new CassetteMapDownloadResultReport_DPI.PORTc()
            //            {
            //                PORTNO = port.Key.Data.PORTNO,
            //                PORTID = port.Key.Data.PORTID,
            //                CASSETTEID = port.Key.File.CassetteID,
            //                RESULT = port.Value.ToString()
            //            });

            //            // if (string.IsNullOrEmpty(eqpNo)) eqpNo = port.Key.Data.NODENO;
            //        }

            //        xMessage msg = SendReportToAllOPI(string.Empty, trx);

            //        if (dicClient.Count == 0)
            //        {
            //            Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] OPI client number is zero, UIService will not report message to OPI.s",
            //            trx.BODY.LINENAME, msg.TransactionID));
            //        }
            //        else
            //        {
            //            Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
            //            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] Cassette Mapping Download Result.",
            //            trx.BODY.LINENAME, msg.TransactionID));
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            //    }
            //}

            /// <summary>
            /// OPI MessageSet: Disconnect User Request
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_DisconnectUsersRequest(XmlDocument xmlDoc)
            {
                  IServerAgent agent = GetServerAgent();
                  DisconnectUsersRequest command = Spec.XMLtoMessage(xmlDoc) as DisconnectUsersRequest;
                  XmlDocument xml_doc = agent.GetTransactionFormat("DisconnectUsersReply") as XmlDocument;
                  DisconnectUsersReply reply = Spec.XMLtoMessage(xml_doc) as DisconnectUsersReply;

                  try
                  {
                        Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI. OPERATORID={3}, REASON={4}",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME, command.BODY.OPERATORID, command.BODY.REASON));

                        reply.BODY.LINENAME = command.BODY.LINENAME;

                        //Line line = ObjectManager.LineManager.GetLine(command.BODY.LINENAME);
                        IList<Line> lines = ObjectManager.LineManager.GetLines();
                        if (lines == null)
                        {
                              reply.RETURN.RETURNCODE = "0010690";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Line[{0}] in LineEntity", command.BODY.LINENAME);

                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't find Line({0}) in LineEntity.",
                                  command.BODY.LINENAME, command.HEADER.TRANSACTIONID));
                        }

                        xMessage msg = SendReplyToOPI(command, reply);

                        Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService reply message({2}) to OPI.",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME));

                        if (reply.RETURN.RETURNCODE == "0000000")
                        {
                            lock (dicClient)//2016/08/01
                            {
                                foreach (DisconnectUsersRequest.USERc user in command.BODY.USERLIST)
                                {
                                    string targetIP = string.Empty;
                                    foreach (KeyValuePair<string, ClientRecord> client in dicClient)
                                    {
                                        if (user.USERID == client.Value.userID && user.LOGINSERVERIP == client.Value.loginServerIP)
                                            targetIP = client.Key;
                                    }
                                    //下達斷線通知
                                    if (targetIP != string.Empty)
                                        ClientDisconnectRequest(command.HEADER.TRANSACTIONID, command.BODY.LINENAME, user.USERID, targetIP,
                                            user.USERGROUP, user.LOGINTIME, command.BODY.OPERATORID, command.BODY.REASON);
                                }
                            }
                        }
                  }
                  catch (Exception ex)
                  {
                        //NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService catch message({2}) exception : {3} ",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME, ex.ToString()));
                  }
            }

            /// <summary>
            /// OPI MessageSet : Client Information Reply
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_ClientInformationRequest(XmlDocument xmlDoc)
            {
                  IServerAgent agent = GetServerAgent();
                  ClientInformationRequest command = Spec.XMLtoMessage(xmlDoc) as ClientInformationRequest;
                  XmlDocument xml_doc = agent.GetTransactionFormat("ClientInformationReply") as XmlDocument;
                  ClientInformationReply reply = Spec.XMLtoMessage(xml_doc) as ClientInformationReply;

                  try
                  {
                        Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI.",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME));

                        reply.BODY.LINENAME = command.BODY.LINENAME;

                        //Line line = ObjectManager.LineManager.GetLine(command.BODY.LINENAME);
                        IList<Line> lines = ObjectManager.LineManager.GetLines();
                        if (lines == null)
                        {
                              reply.RETURN.RETURNCODE = "0010600";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Line[{0}] in LineEntity", command.BODY.LINENAME);

                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't find Line({0}) in LineEntity.",
                                  command.BODY.LINENAME, command.HEADER.TRANSACTIONID));
                        }
                        else
                        {
                            lock (dicClient)//2016/08/01
                            {
                                RemoveDisconnectClientRecord();//2016/08/01
                                foreach (KeyValuePair<string, ClientRecord> _client in dicClient)
                                {
                                    ClientInformationReply.CLIENTc client = new ClientInformationReply.CLIENTc();
                                    client.USERID = _client.Value.userID;
                                    client.USERGROUP = _client.Value.userGroup;
                                    client.LOGINSERVERIP = _client.Value.loginServerIP;
                                    client.LOGINSERVERNAME = _client.Value.loginServerName;
                                    client.LOGINTIME = _client.Value.loginTime;

                                    reply.BODY.CLIENTLIST.Add(client);
                                }
                            }
                        }

                        if (reply.BODY.CLIENTLIST.Count > 0)
                              reply.BODY.CLIENTLIST.RemoveAt(0);

                        xMessage msg = SendReplyToOPI(command, reply);

                        Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService reply message({2}) to OPI.",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME));
                  }
                  catch (Exception ex)
                  {
                        //NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService catch message({2}) exception : {3} ",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME, ex.ToString()));
                  }
            }

            /// <summary>
            /// OPI MessageSet: Client Disconnect Reply
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_ClientDisconnectReply(XmlDocument xmlDoc)
            {
                  try
                  {
                        //IServerAgent agent = GetServerAgent();
                        //{
                        //    Message clientDisconnectReply = Spec.CheckXMLFormat(xmlDoc);
                        //}
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }

            ///// <summary>
            ///// OPI MessageSet: Equipment Data Link Status Report Reply
            ///// </summary>
            ///// <param name="xmlDoc"></param>
            //public void OPI_EquipmentDataLinkStatusReportReply(XmlDocument xmlDoc)
            //{
            //    try
            //    {
            //        //IServerAgent agent = GetServerAgent();
            //        //{
            //        //    Message clientDisconnectReply = Spec.CheckXMLFormat(xmlDoc);
            //        //}
            //    }
            //    catch (Exception ex)
            //    {
            //        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            //    }
            //}

            /// <summary>
            /// OPI MessageSet: Current Changer Plan Request
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_CurrentChangerPlanRequest(XmlDocument xmlDoc)
            {
                  IServerAgent agent = GetServerAgent();
                  CurrentChangerPlanRequest command = Spec.XMLtoMessage(xmlDoc) as CurrentChangerPlanRequest;
                  XmlDocument xml_doc = agent.GetTransactionFormat("CurrentChangerPlanRequestReply") as XmlDocument;
                  CurrentChangerPlanRequestReply reply = Spec.XMLtoMessage(xml_doc) as CurrentChangerPlanRequestReply;

                  try
                  {
                        Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI.",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME));

                        reply.BODY.LINENAME = command.BODY.LINENAME;

                        //Line line = ObjectManager.LineManager.GetLine(command.BODY.LINENAME);
                        IList<Line> lines = ObjectManager.LineManager.GetLines();
                        if (lines == null)
                        {
                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't find Line({0}) in LineEntity.",
                                  command.BODY.LINENAME, command.HEADER.TRANSACTIONID));

                              reply.RETURN.RETURNCODE = "0010750";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Line[{0}] in LineEntity", command.BODY.LINENAME);
                        }
                        else
                        {
                              //由兩條Line合組成一條的特殊Line，IndexOperMode應一致
                              if (lines[0].File.IndexOperMode == eINDEXER_OPERATION_MODE.CHANGER_MODE)
                              {
                                    reply.BODY.CHANGERPLAN.CURRENTPLANID = lines[0].File.CurrentPlanID;
                                    string standByPlanID;
                                    IList<SLOTPLAN> standByProductList = ObjectManager.PlanManager.GetProductPlansStandby(out standByPlanID);
                                    reply.BODY.CHANGERPLAN.STANDBYPLANID = standByPlanID;
                                    reply.BODY.CHANGERPLAN.PLANSTATUS = ((int)lines[0].File.PlanStatus).ToString();
                                    IList<SLOTPLAN> productList = ObjectManager.PlanManager.GetProductPlans(lines[0].File.CurrentPlanID);

                                    foreach (SLOTPLAN product in productList)
                                    {
                                          reply.BODY.CHANGERPLAN.PRODUCTLIST.Add(new CurrentChangerPlanRequestReply.PRODUCTc
                                          {
                                                SLOTNO = product.SLOTNO.ToString(),
                                                PRODUCTNAME = product.PRODUCT_NAME,
                                                SOURCECSTID = product.SOURCE_CASSETTE_ID,
                                                TARGETCSTID = product.TARGET_CASSETTE_ID,
                                                HAVEBEENUSE = product.HAVE_BEEN_USED.ToString()
                                          });
                                    }

                                    foreach (SLOTPLAN product in standByProductList)
                                    {
                                        reply.BODY.CHANGERPLAN.STANDBYPRODUCTLIST.Add(new CurrentChangerPlanRequestReply.PRODUCTc
                                        {
                                            SLOTNO = product.SLOTNO.ToString(),
                                            PRODUCTNAME = product.PRODUCT_NAME,
                                            SOURCECSTID = product.SOURCE_CASSETTE_ID,
                                            TARGETCSTID = product.TARGET_CASSETTE_ID,
                                            HAVEBEENUSE = product.HAVE_BEEN_USED.ToString()
                                        });
                                    }
                              }
                              else
                              {
                                  reply.BODY.CHANGERPLAN.CURRENTPLANID = string.Empty;
                                  reply.BODY.CHANGERPLAN.STANDBYPLANID = string.Empty;
                                  reply.BODY.CHANGERPLAN.PLANSTATUS = "0";

                                    //Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                    //string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Line Index Operation Mode isn't ChangerMode",
                                    //command.BODY.LINENAME, command.HEADER.TRANSACTIONID));

                                    //reply.RETURN.RETURNCODE = "0010751";
                                    //reply.RETURN.RETURNMESSAGE = string.Format("Line Index Operation Mode isn't ChangerMode", command.BODY.LINENAME);
                              }
                        }

                        if (reply.BODY.CHANGERPLAN.PRODUCTLIST.Count > 0)
                              reply.BODY.CHANGERPLAN.PRODUCTLIST.RemoveAt(0);
                        xMessage msg = SendReplyToOPI(command, reply);

                        Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService reply message({2}) to OPI.",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME));

                  }
                  catch (Exception ex)
                  {
                        //NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService catch message({2}) exception : {3} ",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME, ex.ToString()));
                  }
            }

            /// <summary>
            /// OPI MessageSet: Current Changer Plan Report
            /// </summary>
            /// <param name="line"></param>
            public void CurrentChangerPlanReport(string lineID)
            {
                  try
                  {
                        IServerAgent agent = GetServerAgent();
                        XmlDocument xml_doc = agent.GetTransactionFormat("CurrentChangerPlanReport") as XmlDocument;
                        CurrentChangerPlanReport trx = Spec.XMLtoMessage(xml_doc) as CurrentChangerPlanReport;

                        //trx.BODY.LINENAME = lineID;
                        Line line = ObjectManager.LineManager.GetLine(lineID);
                        string standByPlanID;
                        IList<SLOTPLAN> standByProductList = ObjectManager.PlanManager.GetProductPlansStandby(out standByPlanID);
                        trx.BODY.LINENAME = line == null ? lineID : line.Data.SERVERNAME;
                        trx.BODY.CHANGERPLAN.CURRENTPLANID = line.File.CurrentPlanID;
                        trx.BODY.CHANGERPLAN.STANDBYPLANID = standByPlanID;
                        trx.BODY.CHANGERPLAN.PLANSTATUS = ((int)line.File.PlanStatus).ToString();
                        IList<SLOTPLAN> productList = ObjectManager.PlanManager.GetProductPlans(line.File.CurrentPlanID);

                        trx.BODY.CHANGERPLAN.PRODUCTLIST.Clear();
                        trx.BODY.CHANGERPLAN.STANDBYPRODUCTLIST.Clear();

                        foreach (SLOTPLAN product in productList)
                        {
                              trx.BODY.CHANGERPLAN.PRODUCTLIST.Add(new CurrentChangerPlanReport.PRODUCTc()
                              {
                                    SLOTNO = product.SLOTNO.ToString(),
                                    PRODUCTNAME = product.PRODUCT_NAME,
                                    SOURCECSTID = product.SOURCE_CASSETTE_ID,
                                    TARGETCSTID = product.TARGET_CASSETTE_ID,
                                    HAVEBEENUSE = product.HAVE_BEEN_USED.ToString()
                              });
                        }

                        foreach (SLOTPLAN product in standByProductList)
                        {
                            trx.BODY.CHANGERPLAN.STANDBYPRODUCTLIST.Add(new CurrentChangerPlanReport.PRODUCTc()
                            {
                                SLOTNO = product.SLOTNO.ToString(),
                                PRODUCTNAME = product.PRODUCT_NAME,
                                SOURCECSTID = product.SOURCE_CASSETTE_ID,
                                TARGETCSTID = product.TARGET_CASSETTE_ID,
                                HAVEBEENUSE = product.HAVE_BEEN_USED.ToString()
                            });
                        }

                        xMessage msg = SendReportToAllOPI(string.Empty, trx);

                        if (dicClient.Count == 0)
                        {
                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                              string.Format("[LINENAME={0}] [BCS -> OPI][{1}] OPI client number is zero, UIService will not report message to OPI.s",
                              trx.BODY.LINENAME, msg.TransactionID));
                        }
                        else
                        {
                              Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                              string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService report message({2}) to OPI.",
                              trx.BODY.LINENAME, msg.TransactionID, trx.HEADER.MESSAGENAME));
                        }
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }

            ///// <summary>
            ///// OPI MessageSet: Currnet Changer Plan Report Reply
            ///// </summary>
            ///// <param name="xmlDoc"></param>
            //public void OPI_CurrentChangerPlanReportReply(XmlDocument xmlDoc)
            //{
            //    try
            //    {
            //        //IServerAgent agent = GetServerAgent();
            //        //{
            //        //    Message clientDisconnectReply = Spec.CheckXMLFormat(xmlDoc);
            //        //}
            //    }
            //    catch (Exception ex)
            //    {
            //        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            //    }
            //}

            /// <summary>
            /// OPI MessageSet: Dense Status Report
            /// </summary>
            /// <param name="port"></param>
            public void DenseStatusReport(Port port)
            {
                  try
                  {
                        IServerAgent agent = GetServerAgent();
                        XmlDocument xml_doc = agent.GetTransactionFormat("DenseStatusReport") as XmlDocument;
                        DenseStatusReport trx = Spec.XMLtoMessage(xml_doc) as DenseStatusReport;

                        Line line = ObjectManager.LineManager.GetLine(port.Data.LINEID);
                        //trx.BODY.LINENAME = line.Data.LINENAME;
                        trx.BODY.LINENAME = port.Data.SERVERNAME;
                        trx.BODY.MESCONTROLSTATENAM = line.File.HostMode.ToString();
                        trx.BODY.EQUIPMENTNO = port.Data.NODENO;

                        //確認Port為Dense才回
                        //20160106 cy:不用確認, 確認了又不return, 只會放空值給OPI, 外部Invoke時就應該要確認是要做Dense Rule才會進來.
                        //if (port.Data.PORTATTRIBUTE == keyCELLPORTAtt.DENSE)
                        //{
                              trx.BODY.PORTNO = port.Data.PORTNO;
                              trx.BODY.PORTID = port.Data.PORTID;
                              trx.BODY.PORTENABLEMODE = ((int)port.File.EnableMode).ToString() == "" ? "0" : ((int)port.File.EnableMode).ToString();
                              trx.BODY.PORTPACKINGMODE = ((int)port.File.PortPackMode).ToString() == "" ? "0" : ((int)port.File.PortPackMode).ToString();
                              trx.BODY.BOXID01 = port.File.PortBoxID1;
                              trx.BODY.BOXID02 = port.File.PortBoxID2;
                              trx.BODY.PAPER_BOXID = port.File.PortBoxID2;
                              trx.BODY.BOXTYPE = ((int)port.File.BoxType).ToString() == "" ? "0" : ((int)port.File.BoxType).ToString(); 
                              trx.BODY.UNPACKINGSOURCE = port.File.PortUnPackSource;
                              trx.BODY.DENSEBOXDATAREQUEST = port.File.PortDBDataRequest;
                        //}

                        xMessage msg = SendReportToAllOPI(string.Empty, trx);

                        //如果沒有OPI Client連線，OPIAgent將不會report message
                        if (dicClient.Count == 0)
                        {
                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[EQUIPMENT={0}] [BCS -> OPI][{1}] OPI client number is zero, UIService will not report message to OPI.",
                                  trx.BODY.EQUIPMENTNO, msg.TransactionID));
                        }
                        else
                        {
                              Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[EQUIPMENT={0}] [BCS -> OPI][{1}] UIService report message({2}) to OPI.",
                                  trx.BODY.EQUIPMENTNO, trx.HEADER.TRANSACTIONID, trx.HEADER.MESSAGENAME));
                        }
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }

            ///// <summary>
            ///// OPI MessageSet: Dense Status Report Reply
            ///// </summary>
            ///// <param name="xmlDoc"></param>
            //public void OPI_DenseStatusReportReply(XmlDocument xmlDoc)
            //{
            //    try
            //    {
            //        //IServerAgent agent = GetServerAgent();
            //        //{
            //        //    Message denseStatusReportReply = Spec.CheckXMLFormat(xmlDoc);
            //        //}
            //    }
            //    catch (Exception ex)
            //    {
            //        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            //    }
            //}

            /// <summary>
            /// OPI MessageSet: Dense Status Request
            /// </summary>
            /// <param name="?"></param>
            public void OPI_DenseStatusRequest(XmlDocument xmlDoc)
            {
                  IServerAgent agent = GetServerAgent();
                  DenseStatusRequest command = Spec.XMLtoMessage(xmlDoc) as DenseStatusRequest;
                  XmlDocument xml_doc = agent.GetTransactionFormat("DenseStatusReply") as XmlDocument;
                  DenseStatusReply reply = Spec.XMLtoMessage(xml_doc) as DenseStatusReply;

                  try
                  {
                        Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI. EQUIPMENTNO={3}, PORTNO={4}",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME, command.BODY.EQUIPMENTNO, command.BODY.PORTNO));

                        reply.BODY.LINENAME = command.BODY.LINENAME;
                        reply.BODY.EQUIPMENTNO = command.BODY.EQUIPMENTNO;

                        //Line line = ObjectManager.LineManager.GetLine(command.BODY.LINENAME);
                        IList<Line> lines = ObjectManager.LineManager.GetLines();
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(command.BODY.EQUIPMENTNO);
                        Port port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, command.BODY.PORTNO);
                        if (lines == null)
                        {
                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't find Line({0}) in LineEntity.",
                                  command.BODY.LINENAME, command.HEADER.TRANSACTIONID));

                              reply.RETURN.RETURNCODE = "0010770";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Line[{0}] in LineEntity", command.BODY.LINENAME);
                        }
                        else if (eqp == null)
                        {
                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Can't find Equipment({0}) in EquipmentEntity.",
                                  command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));

                              reply.RETURN.RETURNCODE = "0010771";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Equipment[{0}] in EquipmentEntity", command.BODY.EQUIPMENTNO);
                        }
                        else if (port == null)
                        {
                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[PORT={0}] [BCS <- OPI][{1}] Can't find Port({0}) in PortEntity.",
                                  command.BODY.PORTNO, command.HEADER.TRANSACTIONID));

                              reply.RETURN.RETURNCODE = "0010772";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Port[{0}] in PortEntity", command.BODY.PORTNO);
                        }
                        //20160105 cy:CELL的Dense,BOX都會來問, 所以要排除所有可能
                        else if (port.Data.PORTATTRIBUTE != keyCELLPORTAtt.DENSE & port.Data.PORTATTRIBUTE != keyCELLPORTAtt.BOX & port.Data.PORTATTRIBUTE != keyCELLPORTAtt.BOX_MANUAL)
                        {
                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[PORT={0}] [BCS <- OPI][{1}] Port[{0}] isn't Dense.",
                                  command.BODY.PORTNO, command.HEADER.TRANSACTIONID));

                              reply.RETURN.RETURNCODE = "0010773";
                              reply.RETURN.RETURNMESSAGE = string.Format("Port[{0}] isn't Dense.", command.BODY.PORTNO);
                        }
                        else
                        {
                              Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                              reply.BODY.MESCONTROLSTATENAM = line.File.HostMode.ToString();
                              reply.BODY.PORTNO = port.Data.PORTNO;
                              reply.BODY.PORTID = port.Data.PORTID;
                              reply.BODY.PORTENABLEMODE = ((int)port.File.EnableMode).ToString() == "" ? "0" : ((int)port.File.EnableMode).ToString();
                              reply.BODY.PORTPACKINGMODE = ((int)port.File.PortPackMode).ToString() == "" ? "0" : ((int)port.File.PortPackMode).ToString();
                              reply.BODY.BOXID01 = port.File.PortBoxID1;
                              reply.BODY.BOXID02 = port.File.PortBoxID2;
                              reply.BODY.PAPER_BOXID = port.File.PortBoxID2;
                              reply.BODY.BOXTYPE = ((int)port.File.BoxType).ToString() == "" ? "0" : ((int)port.File.BoxType).ToString();
                              reply.BODY.UNPACKINGSOURCE = port.File.PortUnPackSource;
                              reply.BODY.DENSEBOXDATAREQUEST = port.File.PortDBDataRequest;
                        }

                        xMessage msg = SendReplyToOPI(command, reply);

                        Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService reply message({2}) to OPI.",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME));
                  }
                  catch (Exception ex)
                  {
                        //NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService catch message({2}) exception : {3} ",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME, ex.ToString()));
                  }
            }

            /// <summary>
            /// OPI MessageSet: Pallet Status Report
            /// </summary>
            /// <param name="lineName"></param>
            /// <param name="pallet"></param>
            public void PalletStatusReport(string lineName, Pallet pallet)
            {
                  try
                  {
                        IServerAgent agent = GetServerAgent();
                        XmlDocument xml_doc = agent.GetTransactionFormat("PalletStatusReport") as XmlDocument;
                        PalletStatusReport trx = Spec.XMLtoMessage(xml_doc) as PalletStatusReport;

                        Line line = ObjectManager.LineManager.GetLine(lineName);
                        //trx.BODY.LINENAME = lineName;
                        trx.BODY.LINENAME = line == null ? lineName : line.Data.SERVERNAME;
                        //trx.BODY.MESCONTROLSTATENAM = line.File.HostMode.ToString();

                        trx.BODY.PALLETNO = pallet.File.PalletNo;
                        trx.BODY.PALLETID = pallet.File.PalletID;
                        trx.BODY.PALLETMODE = ((int)pallet.File.PalletMode).ToString() == "" ? "0" : ((int)pallet.File.PalletMode).ToString();
                        trx.BODY.PALLETDATAREQUEST = pallet.File.PalletDataRequest;

                        xMessage msg = SendReportToAllOPI(string.Empty, trx);

                        //如果沒有OPI Client連線，OPIAgent將不會report message
                        if (dicClient.Count == 0)
                        {
                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[LINENAEM={0}] [BCS -> OPI][{1}] OPI client number is zero, UIService will not report message to OPI.",
                                  trx.BODY.LINENAME, msg.TransactionID));
                        }
                        else
                        {
                              Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService report message({2}) to OPI.",
                                  trx.BODY.LINENAME, trx.HEADER.TRANSACTIONID, trx.HEADER.MESSAGENAME));
                        }
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }

            ///// <summary>
            ///// OPI MessageSet: Pallet Status Report Reply
            ///// </summary>
            ///// <param name="xmlDoc"></param>
            //public void OPI_PalletStatusReportReply(XmlDocument xmlDoc)
            //{
            //    try
            //    {
            //        //IServerAgent agent = GetServerAgent();
            //        //{
            //        //    Message palletStatusReportReply = Spec.CheckXMLFormat(xmlDoc);
            //        //}
            //    }
            //    catch (Exception ex)
            //    {
            //        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            //    }
            //}

            /// <summary>
            /// OPI MessageSet: Pallet Status Request
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_PalletStatusRequest(XmlDocument xmlDoc)
            {
                  IServerAgent agent = GetServerAgent();
                  PalletStatusRequest command = Spec.XMLtoMessage(xmlDoc) as PalletStatusRequest;
                  XmlDocument xml_doc = agent.GetTransactionFormat("PalletStatusReply") as XmlDocument;
                  PalletStatusReply reply = Spec.XMLtoMessage(xml_doc) as PalletStatusReply;

                  try
                  {
                        Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI. PALLETNO={3}",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME, command.BODY.PALLETNO));

                        reply.BODY.LINENAME = command.BODY.LINENAME;

                        Line line = ObjectManager.LineManager.GetLine(command.BODY.LINENAME);
                        //IList<Line> lines = ObjectManager.LineManager.GetLines();
                        Pallet pallet = ObjectManager.PalletManager.GetPalletByNo(command.BODY.PALLETNO);
                        if (line == null)
                        {
                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't find Line({0}) in LineEntity.",
                                  command.BODY.LINENAME, command.HEADER.TRANSACTIONID));

                              reply.RETURN.RETURNCODE = "0010780";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Line[{0}] in LineEntity", command.BODY.LINENAME);
                        }
                        else if (pallet == null)
                        {
                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[PALLET={0}] [BCS <- OPI][{1}] Can't find Pallet({0}) in PalletEntity.",
                                  command.BODY.PALLETNO, command.HEADER.TRANSACTIONID));

                              reply.RETURN.RETURNCODE = "0010781";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Pallet[{0}] in PalletEntity", command.BODY.PALLETNO);
                        }
                        else
                        {
                              //reply.BODY.MESCONTROLSTATENAM = line.File.HostMode.ToString();
                              reply.BODY.PALLETNO = pallet.File.PalletNo;
                              reply.BODY.PALLETID = pallet.File.PalletID;
                              reply.BODY.PALLETMODE = ((int)pallet.File.PalletMode).ToString() == "" ? "0" : ((int)pallet.File.PalletMode).ToString();
                              reply.BODY.PALLETDATAREQUEST = pallet.File.PalletDataRequest;
                        }

                        xMessage msg = SendReplyToOPI(command, reply);

                        Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService reply message({2}) to OPI.",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME));
                  }
                  catch (Exception ex)
                  {
                        //NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService catch message({2}) exception : {3} ",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME, ex.ToString()));
                  }
            }

            /// <summary>
            /// OPI MessageSet: Local Mode Dense Data Send
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_LocalModeDenseDataSend(XmlDocument xmlDoc)
            {
                  IServerAgent agent = GetServerAgent();
                  LocalModeDenseDataSend command = Spec.XMLtoMessage(xmlDoc) as LocalModeDenseDataSend;
                  XmlDocument xml_doc = agent.GetTransactionFormat("LocalModeDenseDataSendReply") as XmlDocument;
                  LocalModeDenseDataSendReply reply = Spec.XMLtoMessage(xml_doc) as LocalModeDenseDataSendReply;

                  try
                  {
                        Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI. EQUIPMENTNO={3}, PORTNO={4}",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME, command.BODY.EQUIPMENTNO, command.BODY.PORTNO));

                        reply.BODY.LINENAME = command.BODY.LINENAME;

                        //Line line = ObjectManager.LineManager.GetLine(command.BODY.LINENAME);
                        IList<Line> lines = ObjectManager.LineManager.GetLines();
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(command.BODY.EQUIPMENTNO);
                        Port port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, command.BODY.PORTNO);
                        if (lines == null)
                        {
                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't find Line({0}) in LineEntity.",
                                  command.BODY.LINENAME, command.HEADER.TRANSACTIONID));

                              reply.RETURN.RETURNCODE = "0010800";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Line[{0}] in LineEntity", command.BODY.LINENAME);
                        }
                        else if (eqp == null)
                        {
                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Can't find Equipment({0}) in EquipmentEntity.",
                                  command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));

                              reply.RETURN.RETURNCODE = "0010801";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Equipment[{0}] in EquipmentEntity", command.BODY.EQUIPMENTNO);
                        }
                        else if (port == null)
                        {
                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[PORT={0}] [BCS <- OPI][{1}] Can't find Port({0}) in PortEntity.",
                                  command.BODY.PORTNO, command.HEADER.TRANSACTIONID));

                              reply.RETURN.RETURNCODE = "0010802";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Port[{0}] in PortEntity", command.BODY.EQUIPMENTNO);
                        }
                        else if (port.File.PortDBDataRequest == "0")
                        {
                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[PORT={0}] [BCS <- OPI][{1}] Port({0}) DenseBoxDataRequest is off.",
                                  command.BODY.PORTNO, command.HEADER.TRANSACTIONID));

                              reply.RETURN.RETURNCODE = "0010803";
                              reply.RETURN.RETURNMESSAGE = string.Format("Port[{0}] DenseBoxDataRequest is off", command.BODY.EQUIPMENTNO);
                        }
                        else
                        {
                              string strKey = string.Format("{0}_{1}", command.BODY.EQUIPMENTNO, command.BODY.PORTNO);
                              object[] _data = new object[4]
                    { 
                        eqp.Data.NODENO,                //0 eqpno
                        eBitResult.ON,                  //1 ON
                        command.HEADER.TRANSACTIONID,   //2 trx id
                        dicDenseBoxData[strKey]
                    };

                              Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[PORT={0}] [BCS <- OPI][{1}] Invoke DenseBoxService DenseBoxDataRequestReply",
                                  command.BODY.PORTNO, command.HEADER.TRANSACTIONID));

                              Invoke(eServiceName.DenseBoxService, "DenseBoxDataRequestReply", _data);
                        }

                        xMessage msg = SendReplyToOPI(command, reply);

                        Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService reply message({2}) to OPI.",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME));
                  }
                  catch (Exception ex)
                  {
                        //NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService catch message({2}) exception : {3} ",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME, ex.ToString()));
                  }
            }

            /// <summary>
            /// OPI MessageSet: Offline Dense Data Send
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_OfflineDenseDataSend(XmlDocument xmlDoc)
            {
                  IServerAgent agent = GetServerAgent();
                  OfflineDenseDataSend command = Spec.XMLtoMessage(xmlDoc) as OfflineDenseDataSend;
                  XmlDocument xml_doc = agent.GetTransactionFormat("OfflineDenseDataSendReply") as XmlDocument;
                  OfflineDenseDataSendReply reply = Spec.XMLtoMessage(xml_doc) as OfflineDenseDataSendReply;

                  try
                  {
                        Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI. EQUIPMENTNO={3}, PORTNO={4}",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME, command.BODY.EQUIPMENTNO, command.BODY.PORTNO));

                        reply.BODY.LINENAME = command.BODY.LINENAME;

                        //Line line = ObjectManager.LineManager.GetLine(command.BODY.LINENAME);
                        IList<Line> lines = ObjectManager.LineManager.GetLines();
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(command.BODY.EQUIPMENTNO);
                        Port port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, command.BODY.PORTNO);
                        if (lines == null)
                        {
                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't find Line({0}) in LineEntity.",
                                  command.BODY.LINENAME, command.HEADER.TRANSACTIONID));

                              reply.RETURN.RETURNCODE = "0010810";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Line[{0}] in LineEntity", command.BODY.LINENAME);
                        }
                        else if (eqp == null)
                        {
                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Can't find Equipment({0}) in EquipmentEntity.",
                                  command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));

                              reply.RETURN.RETURNCODE = "0010811";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Equipment[{0}] in EquipmentEntity", command.BODY.EQUIPMENTNO);
                        }
                        else if (port == null)
                        {
                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[PORT={0}] [BCS <- OPI][{1}] Can't find Port({0}) in PortEntity.",
                                  command.BODY.PORTNO, command.HEADER.TRANSACTIONID));

                              reply.RETURN.RETURNCODE = "0010812";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Port[{0}] in PortEntity", command.BODY.EQUIPMENTNO);
                        }
                        else if (port.File.PortDBDataRequest == "0")
                        {
                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[PORT={0}] [BCS <- OPI][{1}] Port({0}) DenseBoxDataRequest is off.",
                                  command.BODY.PORTNO, command.HEADER.TRANSACTIONID));

                              reply.RETURN.RETURNCODE = "0010813";
                              reply.RETURN.RETURNMESSAGE = string.Format("Port[{0}] DenseBoxDataRequest is off", command.BODY.EQUIPMENTNO);
                        }
                        else
                        {
                            reply.BODY.EQUIPMENTNO = command.BODY.EQUIPMENTNO;
                            reply.BODY.PORTNO = command.BODY.PORTNO;

                            //建立DenseBox物件
                            Cassette cst1 = new Cassette();
                            Cassette cst2 = new Cassette();
                            cst1.CassetteID = command.BODY.BOXID01;
                            cst1.PortID = port.Data.PORTID;
                            cst1.Empty = port.File.Empty;//sy add for QPP Empty Box
                            ObjectManager.CassetteManager.CreateBoxforPacking(cst1);//sy add for QPP Empty Box
                            cst2.CassetteID = command.BODY.BOXID02;
                            cst2.PortID = port.Data.PORTID;
                            cst2.Empty = port.File.Empty;//sy add for QPP Empty Box
                            ObjectManager.CassetteManager.CreateBoxforPacking(cst2);//sy add for QPP Empty Box

                            List<string> replylist = new List<string>();
                            replylist.AddRange(new string[] 
                    { 
                        "0", 
                        port.Data.PORTID,
                        command.BODY.BOXID01,
                        command.BODY.BOXID02,
                        command.BODY.PRODUCTTYPE,
                        command.BODY.JOBGRADE01,
                        command.BODY.JOBGRADE02,
                        command.BODY.CASSETTESETTINGCODE01,
                        command.BODY.CASSETTESETTINGCODE02,
                        string.IsNullOrEmpty(command.BODY.BOXGLASSCOUNT01) ? "0" : command.BODY.BOXGLASSCOUNT01,
                        string.IsNullOrEmpty(command.BODY.BOXGLASSCOUNT02) ? "0" : command.BODY.BOXGLASSCOUNT02 });

                            object[] _data = new object[4]
                    { 
                        command.BODY.EQUIPMENTNO,   // eqp.Data.NODENO, /*0 eqpno*/
                        eBitResult.ON,                                  /*1 ON*/
                        command.HEADER.TRANSACTIONID,                   /*2 trx id */
                        replylist
                    };

                              Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[PORT={0}] [BCS <- OPI][{1}] Invoke DenseBoxService DenseBoxDataRequestReply.",
                                  command.BODY.PORTNO, command.HEADER.TRANSACTIONID));

                              Invoke(eServiceName.DenseBoxService, "DenseBoxDataRequestReply", _data);
                        }

                        xMessage msg = SendReplyToOPI(command, reply);

                        Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService reply message({2}) to OPI.",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME));
                  }
                  catch (Exception ex)
                  {
                        //NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService catch message({2}) exception : {3} ",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME, ex.ToString()));
                  }
            }

            public void OPI_PPKLocalModeDenseDataSend(XmlDocument xmlDoc)
            {
                //no need
            }

            public void OPI_PPKOfflineDenseDataSend(XmlDocument xmlDoc)
            {
                IServerAgent agent = GetServerAgent();
                PPKOfflineDenseDataSend command = Spec.XMLtoMessage(xmlDoc) as PPKOfflineDenseDataSend;
                XmlDocument xml_doc = agent.GetTransactionFormat("PPKOfflineDenseDataSendReply") as XmlDocument;
                PPKOfflineDenseDataSendReply reply = Spec.XMLtoMessage(xml_doc) as PPKOfflineDenseDataSendReply;
                try
                {
                    Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI. EQUIPMENTNO={3}, PORTNO={4}",
                        command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME, command.BODY.EQUIPMENTNO, command.BODY.PORTNO));

                    reply.BODY.LINENAME = command.BODY.LINENAME;

                    //Line line = ObjectManager.LineManager.GetLine(command.BODY.LINENAME);
                    IList<Line> lines = ObjectManager.LineManager.GetLines();
                    Equipment eqp = ObjectManager.EquipmentManager.GetEQP(command.BODY.EQUIPMENTNO);
                    Port port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, command.BODY.PORTNO);
                    if (lines == null)
                    {
                        Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't find Line({0}) in LineEntity.",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID));

                        reply.RETURN.RETURNCODE = "0010810";
                        reply.RETURN.RETURNMESSAGE = string.Format("Can't find Line[{0}] in LineEntity", command.BODY.LINENAME);
                    }
                    else if (eqp == null)
                    {
                        Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Can't find Equipment({0}) in EquipmentEntity.",
                            command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));

                        reply.RETURN.RETURNCODE = "0010811";
                        reply.RETURN.RETURNMESSAGE = string.Format("Can't find Equipment[{0}] in EquipmentEntity", command.BODY.EQUIPMENTNO);
                    }
                    else if (port == null)
                    {
                        Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[PORT={0}] [BCS <- OPI][{1}] Can't find Port({0}) in PortEntity.",
                            command.BODY.PORTNO, command.HEADER.TRANSACTIONID));

                        reply.RETURN.RETURNCODE = "0010812";
                        reply.RETURN.RETURNMESSAGE = string.Format("Can't find Port[{0}] in PortEntity", command.BODY.EQUIPMENTNO);
                    }
                    else if (port.File.PortDBDataRequest == "0")
                    {
                        Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[PORT={0}] [BCS <- OPI][{1}] Port({0}) DenseBoxDataRequest is off.",
                            command.BODY.PORTNO, command.HEADER.TRANSACTIONID));

                        reply.RETURN.RETURNCODE = "0010813";
                        reply.RETURN.RETURNMESSAGE = string.Format("Port[{0}] PaperBoxDataRequest is off", command.BODY.EQUIPMENTNO);
                    }
                    else
                    {
                        reply.BODY.EQUIPMENTNO = command.BODY.EQUIPMENTNO;
                        reply.BODY.PORTNO = command.BODY.PORTNO;

                        //建立DenseBox物件                        
                        Cassette cst = new Cassette();
                        eBoxType boxType = command.BODY.BOXTYPE == "InBox" ? eBoxType.InBox : eBoxType.OutBox;
                        if (boxType == eBoxType.InBox)
                            cst.CassetteID = command.BODY.BOXID;
                        else
                        {
                            cst.CassetteID = command.BODY.PAPER_BOXID;
                            cst.SubBoxID = command.BODY.BOXID;
                        }
                        cst.PortID = port.Data.PORTID;
                        cst.eBoxType = boxType;
                        cst.Grade = command.BODY.JOBGRADE;
                        cst.ProductType = command.BODY.PRODUCTTYPE;
                        ObjectManager.CassetteManager.CreateBoxforPacking(cst);

                        object[] _data = new object[10]
                    { 
                        command.BODY.EQUIPMENTNO,   // eqp.Data.NODENO, /*0 eqpno*/
                        eBitResult.ON,                                  /*1 ON*/
                        command.HEADER.TRANSACTIONID,                   /*2 trx id */
                        eReturnCode1.OK,
                        port.Data.PORTNO,
                        command.BODY.BOXID,                        
                        command.BODY.PAPER_BOXID,                        
                        boxType,                        
                        command.BODY.PRODUCTTYPE,                        
                        command.BODY.JOBGRADE
                    };
            //            PaperBoxDataRequestReply(string eqpNo, eBitResult value, string trackKey, eReturnCode1 returnCode, string portNO,
            //string boxID, string paperBoxID,eBoxType boxType, string productType, string grade)
                        Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[PORT={0}] [BCS <- OPI][{1}] Invoke PaperBoxService PaperBoxDataRequestReply.",
                            command.BODY.PORTNO, command.HEADER.TRANSACTIONID));

                        Invoke(eServiceName.PaperBoxService, "PaperBoxDataRequestReply", _data);
                    }

                    xMessage msg = SendReplyToOPI(command, reply);

                    Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService reply message({2}) to OPI.",
                        command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME));
                }
                catch (Exception ex)
                {
                    //NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                    NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService catch message({2}) exception : {3} ",
                        command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME, ex.ToString()));
                }
            }

            /// <summary>
            /// OPI MessageSet: Local Mode Pallet Data Request
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_LocalModePalletDataRequest(XmlDocument xmlDoc)
            {
                  IServerAgent agent = GetServerAgent();
                  LocalModePalletDataRequest command = Spec.XMLtoMessage(xmlDoc) as LocalModePalletDataRequest;
                  XmlDocument xml_doc = agent.GetTransactionFormat("LocalModePalletDataReply") as XmlDocument;
                  LocalModePalletDataReply reply = Spec.XMLtoMessage(xml_doc) as LocalModePalletDataReply;

                  try
                  {
                        Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI. PALLETNO={3}",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME, command.BODY.PALLETNO));

                        reply.BODY.LINENAME = command.BODY.LINENAME;

                        Line line = ObjectManager.LineManager.GetLine(command.BODY.LINENAME);
                        Pallet pallet = ObjectManager.PalletManager.GetPalletByNo(command.BODY.PALLETNO);

                        if (line == null)
                        {
                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't find Line({0}) in LineEntity.",
                                  command.BODY.LINENAME, command.HEADER.TRANSACTIONID));

                              reply.RETURN.RETURNCODE = "0010820";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Line[{0}] in LineEntity", command.BODY.LINENAME);
                        }
                        else if (pallet == null)
                        {
                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[PALLET={0}] [BCS <- OPI][{1}] Can't find Pallet({0}) in PalletEntity.",
                                  command.BODY.PALLETNO, command.HEADER.TRANSACTIONID));

                              reply.RETURN.RETURNCODE = "0010821";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Pallet[{0}] in PalletEntity", command.BODY.PALLETNO);
                        }
                        else if (string.IsNullOrEmpty(pallet.File.Mes_ValidatePalletReply))
                        {
                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[PALLET={0}] [BCS <- OPI][{1}] Pallet({0}) doesn't has MES information",
                                  command.BODY.PALLETNO, command.HEADER.TRANSACTIONID));

                              reply.RETURN.RETURNCODE = "0010822";
                              reply.RETURN.RETURNMESSAGE = string.Format("Pallet[{0}] doesn't has MES information", command.BODY.PALLETNO);
                        }
                        else
                        {
                              XmlDocument mesXml = new XmlDocument();
                              mesXml.LoadXml(pallet.File.Mes_ValidatePalletReply);

                              string lineRecipeName = mesXml[keyHost.MESSAGE][keyHost.BODY][keyHost.LINERECIPENAME].InnerText;
                              string palletid = mesXml[keyHost.MESSAGE][keyHost.BODY][keyHost.PALLETNAME].InnerText;
                              string boxqty = mesXml[keyHost.MESSAGE][keyHost.BODY][keyHost.BOXQUANTITY].InnerText;
                              string portid = mesXml[keyHost.MESSAGE][keyHost.BODY][keyHost.PORTNAME].InnerText;

                              reply.BODY.PALLETNO = command.BODY.PALLETNO;
                              reply.BODY.PALLETID = palletid;
                              reply.BODY.DENSEBOXCOUNT = boxqty;
                              reply.BODY.DENSEBOXLIST.Clear();

                              //Jun Modify 20150304
                              //pallet.File.PalletID = palletid;
                              //pallet.File.PalletNo = portid;
                              pallet.File.DenseBoxList = new List<string>();

                              XmlNode boxlistnode = mesXml[keyHost.MESSAGE][keyHost.BODY][keyHost.BOXLIST];
                              List<string> boxlist = new List<string>();
                              int pos = 1;
                              int palletNo = 0;
                              int.TryParse(portid, out palletNo);
                              foreach (XmlNode boxnode in boxlistnode)
                              {
                                    if (boxnode[keyHost.BOXNAME] != null)
                                    {
                                          string boxname = boxnode[keyHost.BOXNAME].InnerText;
                                          boxlist.Add(boxname);
                                          pallet.File.DenseBoxList.Add(boxname);

                                          reply.BODY.DENSEBOXLIST.Add(new LocalModePalletDataReply.DENSEBOXc() { DNESEBOXIDNO = pos.ToString(), DNESEBOXID = boxname });

                                          Cassette cst = new Cassette();
                                          cst.PortNo = palletid;
                                          cst.PortID = palletid;
                                          cst.CassetteSequenceNo = (palletNo * 100 + pos).ToString();
                                          cst.CassetteID = boxname;
                                          cst.LineRecipeName = lineRecipeName;
                                          ObjectManager.CassetteManager.CreateBoxforPacking(cst); //20161102 sy modify 只會下空PALLET
                                    }
                                    pos++;
                              }
                        }

                        xMessage msg = SendReplyToOPI(command, reply);

                        Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService reply message({2}) to OPI.",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME));
                  }
                  catch (Exception ex)
                  {
                        //NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService catch message({2}) exception : {3} ",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME, ex.ToString()));
                  }
            }

            /// <summary>
            /// OPI MessageSet: Local Mode Pallet Data Send
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_LocalModePalletDataSend(XmlDocument xmlDoc)
            {
                  IServerAgent agent = GetServerAgent();
                  LocalModePalletDataSend command = Spec.XMLtoMessage(xmlDoc) as LocalModePalletDataSend;
                  XmlDocument xml_doc = agent.GetTransactionFormat("LocalModePalletDataSendReply") as XmlDocument;
                  LocalModePalletDataSendReply reply = Spec.XMLtoMessage(xml_doc) as LocalModePalletDataSendReply;

                  try
                  {
                        Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI. PALLETNO={3}, PALLETID={4}",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME, command.BODY.PALLETNO, command.BODY.PALLETID));

                        reply.BODY.LINENAME = command.BODY.LINENAME;

                        Line line = ObjectManager.LineManager.GetLine(command.BODY.LINENAME);
                        Pallet pallet = ObjectManager.PalletManager.GetPalletByNo(command.BODY.PALLETNO);
                        if (line == null)
                        {
                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't find Line({0}) in LineEntity.",
                                  command.BODY.LINENAME, command.HEADER.TRANSACTIONID));

                              reply.RETURN.RETURNCODE = "0010880";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Line[{0}] in LineEntity", command.BODY.LINENAME);
                        }
                        else if (pallet == null)
                        {
                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't find Pallet({2}) in PalletEntity.",
                                  command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.BODY.PALLETID));

                              reply.RETURN.RETURNCODE = "0010881";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Pallet[{0}] in PalletEntity", command.BODY.PALLETID);
                        }
                        else if (pallet.File.PalletDataRequest == "0")
                        {
                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[PALLETID={0}] [BCS <- OPI][{1}] Pallet({0}) PalletDataRequest is 0.",
                                  command.BODY.PALLETID, command.HEADER.TRANSACTIONID));

                              reply.RETURN.RETURNCODE = "0010882";
                              reply.RETURN.RETURNMESSAGE = string.Format("Pallet({0}) PalletDataRequest is 0", command.BODY.PALLETID);
                        }
                        else
                        {
                              object[] _data = new object[5]
                    { 
                        pallet.File.NodeNo,   // eqp.Data.NODENO,   /*0 eqpno*/
                        eBitResult.ON,                              /*1 ON*/
                        command.HEADER.TRANSACTIONID,               /*trx id */
                        "1",
                        pallet
                    };

                              Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[PALLET={0}] [BCS <- OPI][{1}] Invoke PalletService PalletDataRequestReportReply.",
                                  command.BODY.PALLETID, command.HEADER.TRANSACTIONID));

                              Invoke(eServiceName.PalletService, "PalletDataRequestReportReply", _data);
                        }

                        xMessage msg = SendReplyToOPI(command, reply);

                        Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService reply message({2}) to OPI.",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME));
                  }
                  catch (Exception ex)
                  {
                        //NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService catch message({2}) exception : {3} ",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME, ex.ToString()));
                  }
            }

            /// <summary>
            /// OPI MessageSet: Offline Pallet Data Send
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_OfflinePalletDataSend(XmlDocument xmlDoc)
            {
                  IServerAgent agent = GetServerAgent();
                  OfflinePalletDataSend command = Spec.XMLtoMessage(xmlDoc) as OfflinePalletDataSend;
                  XmlDocument xml_doc = agent.GetTransactionFormat("OfflinePalletDataSendReply") as XmlDocument;
                  OfflinePalletDataSendReply reply = Spec.XMLtoMessage(xml_doc) as OfflinePalletDataSendReply;

                  try
                  {
                        Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI. PALLETNO={3}, PALLETID={4}",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME, command.BODY.PALLETNO, command.BODY.PALLETID));

                        reply.BODY.LINENAME = command.BODY.LINENAME;

                        Line line = ObjectManager.LineManager.GetLine(command.BODY.LINENAME);
                        Pallet pallet = ObjectManager.PalletManager.GetPalletByNo(command.BODY.PALLETNO);
                        if (line == null)
                        {
                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't find Line({0}) in LineEntity.",
                                  command.BODY.LINENAME, command.HEADER.TRANSACTIONID));

                              reply.RETURN.RETURNCODE = "0010830";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Line[{0}] in LineEntity", command.BODY.LINENAME);
                        }
                        else if (pallet == null)
                        {
                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't find Pallet({2}) in PalletEntity.",
                                  command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.BODY.PALLETID));

                              reply.RETURN.RETURNCODE = "0010831";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Pallet({0}) in PalletEntity", command.BODY.PALLETID);
                        }
                        else if (pallet.File.PalletDataRequest == "0")
                        {
                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Pallet({2}) PalletDataRequest is 0.",
                                  command.BODY.LINENAME, command.HEADER.TRANSACTIONID));

                              reply.RETURN.RETURNCODE = "0010832";
                              reply.RETURN.RETURNMESSAGE = string.Format("Pallet({2}) PalletDataRequest is 0.", command.BODY.LINENAME);
                        }
                        else
                        {
                              reply.BODY.PALLETNO = command.BODY.PALLETNO;

                              pallet.File.PalletID = command.BODY.PALLETID;
                              pallet.File.DenseBoxList = new List<string>();

                              int pos = 1;
                              int palletNo = 0;
                              int.TryParse(pallet.File.PalletNo, out palletNo);
                              foreach (OfflinePalletDataSend.DENSEBOXc box in command.BODY.DENSEBOXLIST)
                              {
                                    pallet.File.DenseBoxList.Add(box.DNESEBOXID);

                                    Cassette cst = new Cassette();
                                    cst.PortNo = command.BODY.PALLETID;
                                    cst.PortID = command.BODY.PALLETID;
                                    cst.CassetteSequenceNo = (palletNo * 100 + pos).ToString();
                                    cst.CassetteID = box.DNESEBOXID;
                                    ObjectManager.CassetteManager.CreateBoxforPacking(cst);//20161102 sy modify 只會下空PALLET
                                    pos++;
                              }

                              object[] _data = new object[5]
                    { 
                        pallet.File.NodeNo,     // eqp.Data.NODENO, /*0 eqpno*/
                        eBitResult.ON,                              /*1 ON*/
                        command.HEADER.TRANSACTIONID,               /*trx id */
                        "1",
                        pallet
                    };

                              Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[PALLET={0}] [BCS <- OPI][{1}] Invoke PalletService PalletDataRequestReportReply.",
                                  command.BODY.PALLETID, command.HEADER.TRANSACTIONID));

                              Invoke(eServiceName.PalletService, "PalletDataRequestReportReply", _data);
                        }

                        xMessage msg = SendReplyToOPI(command, reply);

                        Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService reply message({2}) to OPI.",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME));
                  }
                  catch (Exception ex)
                  {
                        //NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService catch message({2}) exception : {3} ",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME, ex.ToString()));
                  }
            }

            /// <summary>
            /// OPI MessageSet: BC Control Command Info Request
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_BCControlCommandInfoRequest(XmlDocument xmlDoc)
            {
                  IServerAgent agent = GetServerAgent();
                  BCControlCommandInfoRequest command = Spec.XMLtoMessage(xmlDoc) as BCControlCommandInfoRequest;
                  XmlDocument xml_doc = agent.GetTransactionFormat("BCControlCommandInfoReply") as XmlDocument;
                  BCControlCommandInfoReply reply = Spec.XMLtoMessage(xml_doc) as BCControlCommandInfoReply;

                  try
                  {
                        Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI. COMMANDTYPE={3}",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME, command.BODY.COMMANDTYPE));

                        reply.BODY.LINENAME = command.BODY.LINENAME;
                        reply.BODY.COMMANDTYPE = command.BODY.COMMANDTYPE;

                        IList<Equipment> eqps = ObjectManager.EquipmentManager.GetEQPs();

                        if (string.IsNullOrEmpty(command.BODY.COMMANDTYPE))
                        {
                              reply.RETURN.RETURNCODE = "0010940";
                              reply.RETURN.RETURNMESSAGE = string.Format("BC Control CommandType Invalid.",
                                  command.BODY.COMMANDTYPE);

                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[LINENAME={0}] [BCS <- OPI][{1}] BC Control CommandType Invalid. CommandType({2})",
                                  command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.BODY.COMMANDTYPE));
                        }
                        else
                        {
                              string trxName = string.Empty;

                              reply.BODY.ITEMLIST.Clear();
                              foreach (Equipment eqp in eqps)
                              {
                                    BCControlCommandInfoReply.ITEMc _item = new BCControlCommandInfoReply.ITEMc();
                                    //SECS機台不用上報
                                    switch (command.BODY.COMMANDTYPE)
                                    {
                                          case "PROCESSPAUSE": trxName = string.Format("{0}_ProcessPauseCommand", eqp.Data.NODENO); break;
                                          case "TRANSFERSTOP": trxName = string.Format("{0}_TransferStopCommand", eqp.Data.NODENO); break;
                                          case "PROCESSSTOP": trxName = string.Format("{0}_ProcessStopCommand", eqp.Data.NODENO); break;
                                    }
                                    Trx cmdTrx = Invoke(eAgentName.PLCAgent, "GetTransactionFormat", new object[] { trxName }) as Trx;
                                    if (cmdTrx != null)
                                    {
                                          cmdTrx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxName, false }) as Trx;

                                          _item.EQUIPMENTNO = eqp.Data.NODENO;
                                          //TransferStop沒有UnitNo
                                          if (command.BODY.COMMANDTYPE != "TRANSFERSTOP")
                                                _item.UNITNO = cmdTrx.EventGroups[0].Events[0].Items[1].Value;
                                          _item.COMMAND = cmdTrx.EventGroups[0].Events[0].Items[0].Value;

                                          reply.BODY.ITEMLIST.Add(_item);
                                    }
                              }
                        }

                        xMessage msg = SendReplyToOPI(command, reply);

                        Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                               string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService reply message({2}) to OPI.",
                               command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME));
                  }
                  catch (Exception ex)
                  {
                        //NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService catch message({2}) exception : {3} ",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME, ex.ToString()));
                  }
            }

            /// <summary>
            /// OPI MessageSet: OPI Flash Message Reply
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_OPIFlashMessageReply(XmlDocument xmlDoc)
            {
                  try
                  {
                        //IServerAgent agent = GetServerAgent();
                        //{
                        //    Message OPIFlashMessageReply = Spec.CheckXMLFormat(xmlDoc);
                        //}
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }

            }
            #endregion

            #region Command
            /// <summary>
            /// OPI MessageSet: Local Mode Cassette Data Send to BC
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_LocalModeCassetteDataSend(XmlDocument xmlDoc)
            {
                  IServerAgent agent = GetServerAgent();
                  LocalModeCassetteDataSend command = Spec.XMLtoMessage(xmlDoc) as LocalModeCassetteDataSend;
                  XmlDocument xml_doc = agent.GetTransactionFormat("LocalModeCassetteDataSendReply") as XmlDocument;
                  LocalModeCassetteDataSendReply reply = Spec.XMLtoMessage(xml_doc) as LocalModeCassetteDataSendReply;

                  try
                  {
                        Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI. EQUIPMENTNO={3}, PORTNO={4}, CASSETTEID={5}",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME, command.BODY.EQUIPMENTNO, command.BODY.PORTNO, command.BODY.CASSETTEID));

                        //Line line = ObjectManager.LineManager.GetLine(command.BODY.LINENAME);
                        IList<Line> lines = ObjectManager.LineManager.GetLines();
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(command.BODY.EQUIPMENTNO);
                        Port port = ObjectManager.PortManager.GetPort(command.BODY.PORTID);
                        Cassette cst = ObjectManager.CassetteManager.GetCassette(command.BODY.CASSETTEID);

                        if (lines == null)
                        {
                              reply.RETURN.RETURNCODE = "0010120";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Line[{0}] in LineEntity.", command.BODY.LINENAME);

                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't find Line({0}) in LineEntity.",
                                  command.BODY.LINENAME, command.HEADER.TRANSACTIONID));
                        }
                        else if (eqp == null)
                        {
                              reply.RETURN.RETURNCODE = "0010121";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Equipment[{0}] in EquipmentEntity.", command.BODY.EQUIPMENTNO);

                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Can't find Equipment({0}) in EquipmentEntity.",
                                  command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));
                        }
                        else if (port == null)
                        {
                              reply.RETURN.RETURNCODE = "0010122";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Port[{0}] in PortEntity.", command.BODY.PORTID);

                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[PORT={0}] [BCS <- OPI][{1}] Can't find Port({0}) in PortEntity.",
                                  command.BODY.PORTID, command.HEADER.TRANSACTIONID));
                        }
                        else if (cst == null)
                        {
                              reply.RETURN.RETURNCODE = "0010123";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Cassette[{0}] in CassetteEntity.", command.BODY.CASSETTEID);

                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[CASSETTE={0}] [BCS <- OPI][{1}] Can't find Cassette({0}) in CassetteEntity.",
                                  command.BODY.CASSETTEID, command.HEADER.TRANSACTIONID));
                        }
                            //add by yang 20161021
                        else if (port.File.CassetteStatus!=eCassetteStatus.WAITING_FOR_CASSETTE_DATA&&port.File.CassetteStatus!=eCassetteStatus.CASSETTE_REMAP)
                        {
                            reply.RETURN.RETURNCODE = "0010124";
                            reply.RETURN.RETURNMESSAGE = string.Format("cur Cassette[{0}] CassetteStatus not permit edit Cassette Data.", command.BODY.CASSETTEID);

                            Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                string.Format("[CASSETTE={0}] [BCS <- OPI][{1}] cur Cassette[{0}] CassetteStatus not permit edit Cassette Data.",
                                command.BODY.CASSETTEID, command.HEADER.TRANSACTIONID));

                        }
                        else
                        {
                              Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                              XmlDocument mesXml = new XmlDocument();
                              //IServerAgent mesAgent = GetServerAgent("MESAgent");
                              //XmlDocument mesXml = mesAgent.GetTransactionFormat("ValidateCassetteReply") as XmlDocument;
                              mesXml.LoadXml(cst.Mes_ValidateCassetteReply);

                              bool bRemap = command.BODY.REMAPFLAG == "Y" ? true : false;

                              if ((line.Data.FABTYPE == eFabType.CELL.ToString()) && (port.Data.PORTATTRIBUTE == keyCELLPORTAtt.DENSE))
                              {
                                    mesXml.LoadXml(port.File.Mes_ValidateBoxReply);
                                    #region BOX Decode
                                    XmlNode body = mesXml.SelectSingleNode("//MESSAGE/BODY");
                                    body["OPI_LINERECIPENAME"].InnerText = command.BODY.LOTLIST[0].LINERECIPENAME;
                                    body["OPI_CARRIERSETCODE"].InnerText = command.BODY.CSTSETTINGCODE;
                                    XmlNodeList mesBoxList = mesXml.SelectSingleNode("//MESSAGE/BODY/BOXLIST").ChildNodes;
                                    foreach (XmlNode mesBox in mesBoxList)
                                    {
                                          foreach (LocalModeCassetteDataSend.LOTDATAc opi_lot in command.BODY.LOTLIST)
                                          {
                                                if (mesBox["BOXNAME"].InnerText == opi_lot.LOTNAME)
                                                {
                                                      mesBox["OPI_PRDCARRIERSETCODE"].InnerText = opi_lot.CSTSETTINGCODE;
                                                      XmlNodeList mesProductList = mesBox.SelectSingleNode("PRODUCTLIST").ChildNodes;
                                                      foreach (XmlNode mesProduct in mesProductList)
                                                      {
                                                            foreach (LocalModeCassetteDataSend.PRODUCTDATAc opi_product in opi_lot.PRODUCTLIST)
                                                            {
                                                                  if (mesProduct["POSITION"].InnerText == opi_product.SLOTNO)
                                                                  {
                                                                        mesProduct["PROCESSFLAG"].InnerText = opi_product.PROCESSFLAG;

                                                                        //OPI是直接塞入各slot PRODUCT RECIPE NAME, 但CELL BOX只需要給一個LOT RECIPE NAME即可
                                                                        body["OPI_LINERECIPENAME"].InnerText = opi_product.PRODUCTRECIPENAME;
                                                                        mesProduct["OPI_PRODUCTRECIPENAME"].InnerText = opi_product.PRODUCTRECIPENAME;
                                                                        mesProduct["OPI_PPID"].InnerText = opi_product.PPID;
                                                                        break;
                                                                  }
                                                            }
                                                      }
                                                      break;
                                                }
                                          }
                                    }
                                    #endregion
                              }
                              else
                              {
                                    #region Cassette Decode
                                    mesXml["MESSAGE"]["BODY"]["OPI_CARRIERSETCODE"].InnerText = command.BODY.CSTSETTINGCODE;
                                    XmlNodeList mesLotList = mesXml.SelectSingleNode("//MESSAGE/BODY/LOTLIST").ChildNodes;
                                    foreach (XmlNode mesLot in mesLotList)
                                    {
                                          XmlNode mesProductListNew = mesXml.CreateElement("PRODUCTLIST");
                                          XmlNode mesProductList = null;
                                          foreach (LocalModeCassetteDataSend.LOTDATAc opi_lot in command.BODY.LOTLIST)
                                          {
                                                if (mesLot["LOTNAME"].InnerText == opi_lot.LOTNAME)
                                                {
                                                      mesLot["OPI_LINERECIPENAME"].InnerText = opi_lot.LINERECIPENAME;
                                                      mesLot["OPI_PPID"].InnerText = opi_lot.PPID;
                                                      mesLot["OPI_CURRENTLINEPPID"].InnerText = opi_lot.CURRENTLINEPPID;
                                                      mesLot["OPI_CROSSLINEPPID"].InnerText = opi_lot.CROSSLINEPPID;
                                                      mesLot["OPI_PRDCARRIERSETCODE"].InnerText = opi_lot.CSTSETTINGCODE;
                                                      #region 只有CUT Line OPI才會給ProcessLineList/STBProductSpecList
                                                      if (Workbench.LineType == eLineType.CELL.CBCUT_1 || Workbench.LineType == eLineType.CELL.CBCUT_2 || Workbench.LineType == eLineType.CELL.CBCUT_3)
                                                      {
                                                            XmlNode mesProcessLineList = mesLot["PROCESSLINELIST"];
                                                            if (mesProcessLineList == null && opi_lot.PROCESSLINELIST.Count > 0)
                                                            {
                                                                  mesProcessLineList = mesXml.CreateElement("PROCESSLINELIST");
                                                                  mesLot.AppendChild(mesProcessLineList);
                                                            }
                                                            if (mesProcessLineList != null)
                                                            {
                                                                  #region 若OPI沒有PROCESSLINE, 但MES有, 將MES ValidateCassetteReply的PROCESSLINE移除
                                                                  {
                                                                        for (int i = mesProcessLineList.ChildNodes.Count - 1; i >= 0; i--)
                                                                        {
                                                                              XmlNode mesProcessLine = mesProcessLineList.ChildNodes.Item(i);
                                                                              bool find = false;
                                                                              foreach (LocalModeCassetteDataSend.PROCESSLINEc opi_process_line in opi_lot.PROCESSLINELIST)
                                                                              {
                                                                                    if (mesProcessLine["LINENAME"].InnerText == opi_process_line.LINENAME)
                                                                                    {
                                                                                          find = true;
                                                                                          break;
                                                                                    }
                                                                              }
                                                                              if (!find)
                                                                              {
                                                                                    mesProcessLineList.RemoveChild(mesProcessLine);
                                                                              }
                                                                        }
                                                                  }
                                                                  #endregion
                                                                  #region 若MES ValidateCassetteReply已經有PROCESSLINE, 且OPI有相同PROCESSLINE, 將OPI的值填入MES XML裡
                                                                  {
                                                                        foreach (XmlNode mesProcessLine in mesProcessLineList.ChildNodes)
                                                                        {
                                                                              foreach (LocalModeCassetteDataSend.PROCESSLINEc opi_process_line in opi_lot.PROCESSLINELIST)
                                                                              {
                                                                                    if (mesProcessLine["LINENAME"].InnerText == opi_process_line.LINENAME)
                                                                                    {
                                                                                          //將OPI編輯後的資料寫入對應的位置
                                                                                          mesProcessLine["OPI_LINERECIPENAME"].InnerText = opi_process_line.LINERECIPENAME;
                                                                                          mesProcessLine["OPI_PPID"].InnerText = opi_process_line.PPID;
                                                                                          mesProcessLine["OPI_CARRIERSETCODE"].InnerText = opi_process_line.CSTSETTINGCODE;
                                                                                          break;
                                                                                    }
                                                                              }
                                                                        }
                                                                  }
                                                                  #endregion
                                                                  #region 若MES ValidateCassetteReply沒有PROCESSLINE, 但OPI有, 在MES新增PROCESSLINE
                                                                  {
                                                                        foreach (LocalModeCassetteDataSend.PROCESSLINEc opi_process_line in opi_lot.PROCESSLINELIST)
                                                                        {
                                                                              bool find = false;
                                                                              foreach (XmlNode mesProcessLine in mesProcessLineList.ChildNodes)
                                                                              {
                                                                                    if (mesProcessLine["LINENAME"].InnerText == opi_process_line.LINENAME)
                                                                                    {
                                                                                          find = true;
                                                                                          break;
                                                                                    }
                                                                              }
                                                                              if (!find)
                                                                              {
                                                                                    XmlElement mesProcessLine = mesXml.CreateElement("PROCESSLINE");
                                                                                    mesProcessLineList.AppendChild(mesProcessLine);
                                                                                    mesProcessLine.AppendChild(mesXml.CreateElement("LINENAME"));
                                                                                    mesProcessLine.AppendChild(mesXml.CreateElement("PRODUCTSPECNAME"));
                                                                                    mesProcessLine.AppendChild(mesXml.CreateElement("PRODUCTSPECVER"));
                                                                                    mesProcessLine.AppendChild(mesXml.CreateElement("BCPRODUCTTYPE"));
                                                                                    mesProcessLine.AppendChild(mesXml.CreateElement("BCPRODUCTID"));
                                                                                    mesProcessLine.AppendChild(mesXml.CreateElement("CARRIERSETCODE"));
                                                                                    mesProcessLine.AppendChild(mesXml.CreateElement("OPI_CARRIERSETCODE"));
                                                                                    mesProcessLine.AppendChild(mesXml.CreateElement("LINERECIPENAME"));
                                                                                    mesProcessLine.AppendChild(mesXml.CreateElement("OPI_LINERECIPENAME"));
                                                                                    mesProcessLine.AppendChild(mesXml.CreateElement("PPID"));
                                                                                    mesProcessLine.AppendChild(mesXml.CreateElement("OPI_PPID"));
                                                                                    mesProcessLine.AppendChild(mesXml.CreateElement("RECIPEPARAVALIDATIONFLAG"));
                                                                                    mesProcessLine["LINENAME"].InnerText = opi_process_line.LINENAME;
                                                                                    mesProcessLine["OPI_CARRIERSETCODE"].InnerText = opi_process_line.CSTSETTINGCODE;
                                                                                    mesProcessLine["OPI_LINERECIPENAME"].InnerText = opi_process_line.LINERECIPENAME;
                                                                                    mesProcessLine["OPI_PPID"].InnerText = opi_process_line.PPID;
                                                                                    mesProcessLine["BCPRODUCTTYPE"].InnerText = opi_process_line.BCPRODUCTTYPE;
                                                                                    mesProcessLine["BCPRODUCTID"].InnerText = opi_process_line.PRODUCTID;
                                                                              }
                                                                        }
                                                                  }
                                                                  #endregion
                                                                  #region 將本LINE的PROCESSLINE移到第一筆
                                                                  {
                                                                        foreach (XmlNode mesProcessLine in mesProcessLineList.ChildNodes)
                                                                        {
                                                                              if (mesProcessLine["LINENAME"].InnerText == Workbench.ServerName)
                                                                              {
                                                                                    mesProcessLineList.RemoveChild(mesProcessLine);
                                                                                    mesProcessLineList.InsertBefore(mesProcessLine, mesProcessLineList.FirstChild);
                                                                                    break;
                                                                              }
                                                                        }
                                                                  }
                                                                  #endregion
                                                            }
                                                      }
                                                      if (Workbench.LineType == eLineType.CELL.CBCUT_2 || Workbench.LineType == eLineType.CELL.CBCUT_3)
                                                      {
                                                            XmlNode mesStbProductSpecList = mesLot["STBPRODUCTSPECLIST"];
                                                            if (mesStbProductSpecList == null && opi_lot.STBPRODUCTSPECLIST.Count > 0)
                                                            {
                                                                  mesStbProductSpecList = mesXml.CreateElement("STBPRODUCTSPECLIST");
                                                                  mesLot.AppendChild(mesStbProductSpecList);
                                                            }
                                                            if (mesStbProductSpecList != null)
                                                            {
                                                                  #region 若OPI沒有STBPRODUCTSPEC, 但MES有, 將MES ValidateCassetteReply的STBPRODUCTSPEC移除
                                                                  {
                                                                        for (int i = mesStbProductSpecList.ChildNodes.Count - 1; i >= 0; i--)
                                                                        {
                                                                              XmlNode mesStb = mesStbProductSpecList.ChildNodes.Item(i);
                                                                              bool find = false;
                                                                              foreach (LocalModeCassetteDataSend.STBPRODUCTSPECc opi_stb in opi_lot.STBPRODUCTSPECLIST)
                                                                              {
                                                                                    if (mesStb["LINENAME"].InnerText == opi_stb.LINENAME)
                                                                                    {
                                                                                          find = true;
                                                                                          break;
                                                                                    }
                                                                              }
                                                                              if (!find)
                                                                              {
                                                                                    mesStbProductSpecList.RemoveChild(mesStb);
                                                                              }
                                                                        }
                                                                  }
                                                                  #endregion
                                                                  #region 若MES ValidateCassetteReply已經有STBPRODUCTSPEC, 且OPI有相同STBPRODUCTSPEC, 將OPI的值填入MES XML裡
                                                                  {
                                                                        foreach (XmlNode mesStb in mesStbProductSpecList.ChildNodes)
                                                                        {
                                                                              foreach (LocalModeCassetteDataSend.STBPRODUCTSPECc opi_stb in opi_lot.STBPRODUCTSPECLIST)
                                                                              {
                                                                                    if (mesStb["LINENAME"].InnerText == opi_stb.LINENAME)
                                                                                    {
                                                                                          //將OPI編輯後的資料寫入對應的位置
                                                                                          mesStb["OPI_LINERECIPENAME"].InnerText = opi_stb.LINERECIPENAME;
                                                                                          mesStb["OPI_PPID"].InnerText = opi_stb.PPID;
                                                                                          mesStb["OPI_CARRIERSETCODE"].InnerText = opi_stb.CSTSETTINGCODE;
                                                                                          break;
                                                                                    }
                                                                              }
                                                                        }
                                                                  }
                                                                  #endregion
                                                                  #region 若MES ValidateCassetteReply沒有STBPRODUCTSPEC, 但OPI有, 在MES新增STBPRODUCTSPEC
                                                                  {
                                                                        foreach (LocalModeCassetteDataSend.STBPRODUCTSPECc opi_stb in opi_lot.STBPRODUCTSPECLIST)
                                                                        {
                                                                              bool find = false;
                                                                              foreach (XmlNode mesStb in mesStbProductSpecList.ChildNodes)
                                                                              {
                                                                                    if (mesStb["LINENAME"].InnerText == opi_stb.LINENAME)
                                                                                    {
                                                                                          find = true;
                                                                                          break;
                                                                                    }
                                                                              }
                                                                              if (!find)
                                                                              {
                                                                                    XmlElement mesStb = mesXml.CreateElement("STBPRODUCTSPEC");
                                                                                    mesStbProductSpecList.AppendChild(mesStb);
                                                                                    mesStb.AppendChild(mesXml.CreateElement("LINENAME"));
                                                                                    mesStb.AppendChild(mesXml.CreateElement("PRODUCTSPECNAME"));
                                                                                    mesStb.AppendChild(mesXml.CreateElement("PRODUCTSPECVER"));
                                                                                    mesStb.AppendChild(mesXml.CreateElement("PRODUCTOWNER"));
                                                                                    mesStb.AppendChild(mesXml.CreateElement("OWNERID"));
                                                                                    mesStb.AppendChild(mesXml.CreateElement("BCPRODUCTTYPE"));
                                                                                    mesStb.AppendChild(mesXml.CreateElement("BCPRODUCTID"));
                                                                                    mesStb.AppendChild(mesXml.CreateElement("CARRIERSETCODE"));
                                                                                    mesStb.AppendChild(mesXml.CreateElement("OPI_CARRIERSETCODE"));
                                                                                    mesStb.AppendChild(mesXml.CreateElement("LINERECIPENAME"));
                                                                                    mesStb.AppendChild(mesXml.CreateElement("OPI_LINERECIPENAME"));
                                                                                    mesStb.AppendChild(mesXml.CreateElement("PPID"));
                                                                                    mesStb.AppendChild(mesXml.CreateElement("OPI_PPID"));
                                                                                    mesStb["LINENAME"].InnerText = opi_stb.LINENAME;
                                                                                    mesStb["OPI_CARRIERSETCODE"].InnerText = opi_stb.CSTSETTINGCODE;
                                                                                    mesStb["OPI_LINERECIPENAME"].InnerText = opi_stb.LINERECIPENAME;
                                                                                    mesStb["OPI_PPID"].InnerText = opi_stb.PPID;
                                                                                    mesStb["BCPRODUCTTYPE"].InnerText = opi_stb.BCPRODUCTTYPE;
                                                                                    mesStb["BCPRODUCTID"].InnerText = opi_stb.PRODUCTID;
                                                                              }
                                                                        }
                                                                  }
                                                                  #endregion
                                                                  #region 將本LINE的STBPRODUCTSPEC移到第一筆
                                                                  {
                                                                        foreach (XmlNode mesStb in mesStbProductSpecList.ChildNodes)
                                                                        {
                                                                              if (mesStb["LINENAME"].InnerText == Workbench.ServerName)
                                                                              {
                                                                                    mesStbProductSpecList.RemoveChild(mesStb);
                                                                                    mesStbProductSpecList.InsertBefore(mesStb, mesStbProductSpecList.FirstChild);
                                                                                    break;
                                                                              }
                                                                        }
                                                                  }
                                                                  #endregion
                                                            }
                                                      }
                                                      #endregion
                                                      #region PRODUCTLIST
                                                      {
                                                            //XmlNode mesProductList = mesLot["PRODUCTLIST"];
                                                            mesProductList = mesLot["PRODUCTLIST"];
                                                            if (mesProductList == null && opi_lot.PRODUCTLIST.Count > 0)
                                                            {
                                                                  mesProductList = mesXml.CreateElement("PRODUCTLIST");
                                                                  mesLot.AppendChild(mesProductList);
                                                            }
                                                            if (mesProductList != null)
                                                            {
                                                                  #region 若MES ValidateCassetteReply已經有PRODUCT, 且OPI有相同PRODUCT, 將OPI的值填入MES XML裡
                                                                  {
                                                                        foreach (XmlNode mesProduct in mesProductList.ChildNodes)
                                                                        {
                                                                              bool bPositionSet = false;
                                                                              foreach (LocalModeCassetteDataSend.PRODUCTDATAc opi_product in opi_lot.PRODUCTLIST)
                                                                              {
                                                                                    if (mesProduct["POSITION"].InnerText == opi_product.SLOTNO)
                                                                                    {                                                                                          
                                                                                          //將OPI編輯後的資料寫入對應的位置
                                                                                          mesProduct["OPI_PROCESSFLAG"].InnerText = opi_product.PROCESSFLAG;  //yang 20161112 keep  original Process Flag
                                                                                          mesProduct["OPI_PRODUCTRECIPENAME"].InnerText = opi_product.PRODUCTRECIPENAME;
                                                                                          mesProduct["OPI_PPID"].InnerText = opi_product.PPID;

                                                                                          //Reset MES Position
                                                                                          bPositionSet = true;
                                                                                          mesProductListNew.AppendChild(mesProduct.Clone());

                                                                                          break;
                                                                                    }
                                                                              }

                                                                              //Reset MES Position 2016/06/14 cc.kuang
                                                                              if (line.Data.FABTYPE == eFabType.ARRAY.ToString() && bRemap)
                                                                              {
                                                                                  if (!bPositionSet)
                                                                                  {
                                                                                      mesProduct["POSITION"].InnerText = string.Empty;
                                                                                  }
                                                                              }
                                                                        }
                                                                  }
                                                                  #endregion
                                                            }
                                                      }
                                                      #endregion
                                                      break;
                                                }
                                          }
                                          if (bRemap)
                                          {
                                              mesLot.RemoveChild(mesProductList);
                                              mesLot.AppendChild(mesProductListNew);
                                          }
                                    }
                                    #endregion
                              }

                              //modify for array re-map slot info
                              XmlDocument opiXml = mesXml;

                              Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[CASSETTE={0}] [BCS <- OPI][{1}] Invoke MESService Decode_ValidateCassetteData",
                                  command.BODY.CASSETTEID, command.HEADER.TRANSACTIONID));

                              //bool bRemap = command.BODY.REMAPFLAG == "Y" ? true : false;
                              //轉拋回給MESService處理後續動作
                              if ((line.Data.FABTYPE == eFabType.CELL.ToString()) && (port.Data.PORTATTRIBUTE == keyCELLPORTAtt.DENSE))
                                    Invoke(eServiceName.MESService, "Decode_ValidateBoxData", new object[] { opiXml, line, eqp, port, cst });
                              else
                                    Invoke(eServiceName.MESService, "Decode_ValidateCassetteData", new object[] { opiXml, line, eqp, port, cst, bRemap });
                        }

                        reply.BODY.LINENAME = command.BODY.LINENAME;
                        reply.BODY.EQUIPMENTNO = command.BODY.EQUIPMENTNO;
                        reply.BODY.PORTNO = command.BODY.PORTNO;
                        reply.BODY.PORTID = command.BODY.PORTID;
                        reply.BODY.CASSETTEID = command.BODY.CASSETTEID;

                        xMessage msg = SendReplyToOPI(command, reply);

                        Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService reply message({2}) to OPI.",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME));
                  }
                  catch (Exception ex)
                  {
                        //NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService catch message({2}) exception : {3} ",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME, ex.Message));
                  }
            }

            /// <summary>
            /// OPI MessageSet: Cassette Command Request to BC
            /// </summary>
            /// <param name="xmlDoc"></param>
            public void OPI_CassetteCommandRequest(XmlDocument xmlDoc)
            {
                  IServerAgent agent = GetServerAgent();
                  CassetteCommandRequest command = Spec.XMLtoMessage(xmlDoc) as CassetteCommandRequest;
                  XmlDocument xml_doc = agent.GetTransactionFormat("CassetteCommandReply") as XmlDocument;
                  CassetteCommandReply reply = Spec.XMLtoMessage(xml_doc) as CassetteCommandReply;

                  try
                  {
                        Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI. OPERATORID={3}, EQUIPMENTNO={4}, PORTNO={5}, CASSETTEID={6}, CASSETTECOMMAND={7}, PROCESSCOUNT={8}",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME,
                            command.BODY.OPERATORID, command.BODY.EQUIPMENTNO, command.BODY.PORTNO, command.BODY.CASSETTEID, command.BODY.CASSETTECOMMAND, command.BODY.PROCESSCOUNT));

                        //Line line = ObjectManager.LineManager.GetLine(command.BODY.LINENAME);
                        IList<Line> lines = ObjectManager.LineManager.GetLines();
                        if (lines == null)
                        {
                              reply.RETURN.RETURNCODE = "0010131";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Line[{0}] in LineEntity", command.BODY.LINENAME);

                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[PORT={0}] [BCS <- OPI][{1}] Can't find Line[{0}] in LineEntity.",
                                  command.BODY.LINENAME, command.HEADER.TRANSACTIONID));
                        }
                        Port port = ObjectManager.PortManager.GetPort(command.BODY.PORTID);
                        Cassette cst = ObjectManager.CassetteManager.GetCassette(command.BODY.CASSETTEID.Trim());
                        if (port == null)
                        {
                              reply.RETURN.RETURNCODE = "0010131";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Port[{0}] in PortEntity", command.BODY.PORTID);

                              Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[PORT={0}] [BCS <- OPI][{1}] Can't find Port[{0}] in PortEntity.",
                                  command.BODY.PORTID, command.HEADER.TRANSACTIONID));
                        }
                        //需排除Reload/Load的特殊情況
                        else if (cst == null && !(command.BODY.CASSETTECOMMAND == "8" || command.BODY.CASSETTECOMMAND == "7"))
                        {
                              reply.RETURN.RETURNCODE = "0010130";
                              reply.RETURN.RETURNMESSAGE = string.Format("Can't find Cassette[{0}] in CassetteEntity!", command.BODY.CASSETTEID);

                              Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("[CASSETTE={0}] [BCS <- OPI][{1}] Can't find Cassette({0}) in CassetteEntity.",
                                  command.BODY.CASSETTEID, command.HEADER.TRANSACTIONID));
                        }
                        else
                        {
                              Line line = ObjectManager.LineManager.GetLine(port.Data.LINEID);

                              //if (line.Data.LINETYPE == eLineType.CELL.CBDPI)
                              //{
                              //    OPI_CassetteCommandRequest_DPI(xmlDoc);
                              //    return;
                              //}
                              //1:Process Start, 2:Process Start By Count, 3:Process Pause, 4:Process Resume
                              //5:Process Abort, 6:Process Cancel, 7:Reload, 8:Load, 9:Re-Map, 11:Map Download
                              switch (command.BODY.CASSETTECOMMAND)
                              {
                                    case "1":
                                      //sy modify By T3 IO同CST  so Mark
                                          //if ((line.Data.FABTYPE == eFabType.CELL.ToString()) && (port.Data.PORTATTRIBUTE == keyCELLPORTAtt.DENSE))
                                          //{
                                          //      NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                          //      string.Format("[CASSETTE={0}] [BCS <- OPI][{1}] Invoke DenseBoxCassetteService DPCassetteProcessStart_UI.",
                                          //      cst.CassetteID, command.HEADER.TRANSACTIONID));
                                          //      Invoke("DenseBoxCassetteService", "DPCassetteProcessStart_UI", new object[] { command });
                                          //}
                                          //else
                                          //{
                                                NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                    string.Format("[CASSETTE={0}] [BCS <- OPI][{1}] Invoke CassetteService CassetteProcessStart_UI.",
                                                    cst.CassetteID, command.HEADER.TRANSACTIONID));
                                                Invoke("CassetteService", "CassetteProcessStart_UI", new object[] { command });
                                          //}
                                          break;
                                    case "2":
                                          //sy modify By T3 IO同CST  so Mark
                                          //if ((line.Data.FABTYPE == eFabType.CELL.ToString()) && (port.Data.PORTATTRIBUTE == keyCELLPORTAtt.DENSE))
                                          //{
                                          //      NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                          //      string.Format("[CASSETTE={0}] [BCS <- OPI][{1}] Invoke DenseBoxCassetteService DPCassetteProcessStartByCount_UI.",
                                          //      cst.CassetteID, command.HEADER.TRANSACTIONID));
                                          //      Invoke("DenseBoxCassetteService", "DPCassetteProcessStartByCount_UI", new object[] { command });
                                          //}
                                          //else
                                          //{
                                                NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                    string.Format("[CASSETTE = {0}] [BCS <- OPI][{1}] Invoke CassetteService CassetteProcessStartByCount_UI.",
                                                    cst.CassetteID, command.HEADER.TRANSACTIONID));
                                                Invoke("CassetteService", "CassetteProcessStartByCount_UI", new object[] { command });
                                          //}
                                          break;
                                    case "3":
                                          //sy modify By T3 IO同CST  so Mark
                                          //if ((line.Data.FABTYPE == eFabType.CELL.ToString()) && (port.Data.PORTATTRIBUTE == keyCELLPORTAtt.DENSE))
                                          //{
                                          //      NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                          //      string.Format("[CASSETTE={0}] [BCS <- OPI][{1}] Invoke DenseBoxCassetteService DPCassetteProcessPause_UI.",
                                          //      cst.CassetteID, command.HEADER.TRANSACTIONID));
                                          //      Invoke("DenseBoxCassetteService", "DPCassetteProcessPause_UI", new object[] { command });
                                          //}
                                          //else
                                          //{
                                                NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                    string.Format("[CASSETTE={0}] [BCS <- OPI][{1}] Invoke CassetteService CassetteProcessPause_UI.",
                                                    cst.CassetteID, command.HEADER.TRANSACTIONID));
                                                Invoke("CassetteService", "CassetteProcessPause_UI", new object[] { command });
                                          //}
                                          break;
                                    case "4":
                                          //sy modify By T3 IO同CST  so Mark
                                          //if ((line.Data.FABTYPE == eFabType.CELL.ToString()) && (port.Data.PORTATTRIBUTE == keyCELLPORTAtt.DENSE))
                                          //{
                                          //      NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                          //      string.Format("[CASSETTE={0}] [BCS <- OPI][{1}] Invoke DenseBoxCassetteService DPCassetteProcessPause_UI.",
                                          //      cst.CassetteID, command.HEADER.TRANSACTIONID));
                                          //      Invoke("DenseBoxCassetteService", "DPCassetteProcessPause_UI", new object[] { command });
                                          //}
                                          //else
                                          //{
                                                NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                    string.Format("[CASSETTE={0}] [BCS <- OPI][{1}] Invoke CassetteService CassetteProcessResume_UI.",
                                                    cst.CassetteID, command.HEADER.TRANSACTIONID));
                                                Invoke("CassetteService", "CassetteProcessResume_UI", new object[] { command });
                                          //}
                                          break;
                                    case "5":
                                          //sy modify By T3 IO同CST  so Mark
                                          //if ((line.Data.FABTYPE == eFabType.CELL.ToString()) && (port.Data.PORTATTRIBUTE == keyCELLPORTAtt.DENSE))
                                          //{
                                          //      NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                          //      string.Format("[CASSETTE={0}] [BCS <- OPI][{1}] Invoke DenseBoxCassetteService DPCassetteProcessAbort_UI.",
                                          //      cst.CassetteID, command.HEADER.TRANSACTIONID));
                                          //      Invoke("DenseBoxCassetteService", "DPCassetteProcessAbort_UI", new object[] { command });
                                          //}
                                          //else
                                          //{
                                                NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                    string.Format("[CASSETTE={0}] [BCS <- OPI][{1}] Invoke CassetteService CassetteProcessAbort_UI.",
                                                    cst.CassetteID, command.HEADER.TRANSACTIONID));
                                                Invoke("CassetteService", "CassetteProcessAbort_UI", new object[] { command });
                                          //}
                                          break;
                                    case "6":
                                          //sy modify By T3 IO同CST  so Mark
                                          //if ((line.Data.FABTYPE == eFabType.CELL.ToString()) && (port.Data.PORTATTRIBUTE == keyCELLPORTAtt.DENSE))
                                          //{
                                          //      NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                          //      string.Format("[CASSETTE={0}] [BCS <- OPI][{1}] Invoke DenseBoxCassetteService DPCassetteProcessCancel_UI.",
                                          //      cst.CassetteID, command.HEADER.TRANSACTIONID));
                                          //      Invoke("DenseBoxCassetteService", "DPCassetteProcessCancel_UI", new object[] { command });
                                          //}
                                          //else
                                          //{
                                                NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                    string.Format("[CASSETTE={0}] [BCS <- OPI][{1}] Invoke CassetteService CassetteProcessCancel_UI.",
                                                    cst.CassetteID, command.HEADER.TRANSACTIONID));
                                                Invoke("CassetteService", "CassetteProcessCancel_UI", new object[] { command });
                                          //}
                                          break;
                                    case "7":
                                          //sy modify By T3 IO同CST  so Mark
                                          //if ((line.Data.FABTYPE == eFabType.CELL.ToString()) && (port.Data.PORTATTRIBUTE == keyCELLPORTAtt.DENSE))
                                          //{
                                          //      NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                          //      string.Format("[CASSETTE={0}] [BCS <- OPI][{1}] Invoke DenseBoxCassetteService DPCassetteReload_UI.",
                                          //      command.BODY.CASSETTEID, command.HEADER.TRANSACTIONID));
                                          //      Invoke("DenseBoxCassetteService", "DPCassetteReload_UI", new object[] { command });
                                          //}
                                          //else
                                          //{
                                                NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                    string.Format("[CASSETTE={0}] [BCS <- OPI][{1}] Invoke CassetteService CassetteReload_UI.",
                                                    command.BODY.CASSETTEID, command.HEADER.TRANSACTIONID));
                                                Invoke("CassetteService", "CassetteReload_UI", new object[] { command });
                                          //}
                                          break;
                                    case "8":
                                          //Load此時沒有CST資訊
                                          //sy modify By T3 IO同CST  so Mark
                                          //if ((line.Data.FABTYPE == eFabType.CELL.ToString()) && (port.Data.PORTATTRIBUTE == keyCELLPORTAtt.DENSE))
                                          //{
                                          //      NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                          //      string.Format("[CASSETTE={0}] [BCS <- OPI][{1}] Invoke DenseBoxCassetteService DPCassetteLoad_UI.",
                                          //      command.BODY.CASSETTEID, command.HEADER.TRANSACTIONID));
                                          //      Invoke("DenseBoxCassetteService", "DPCassetteLoad_UI", new object[] { command });
                                          //}
                                          //else
                                          //{
                                                NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                    string.Format("[PORT={0}] [BCS <- OPI][{1}] Invoke CassetteService CassetteLoad_UI.",
                                                    command.BODY.PORTID, command.HEADER.TRANSACTIONID));
                                                Invoke("CassetteService", "CassetteLoad_UI", new object[] { command });
                                          //}
                                          break;
                                    case "9":
                                          NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                              string.Format("[CASSETTE={0}] [BCS <- OPI][{1}] Invoke CassetteService CassetteReMap_UI.",
                                              cst.CassetteID, command.HEADER.TRANSACTIONID));
                                          Invoke("CassetteService", "CassetteReMap_UI", new object[] { command });
                                          break;
                                    case "11":
                                          //sy modify By T3 IO同CST  so Mark
                                          //if ((line.Data.FABTYPE == eFabType.CELL.ToString()) && (port.Data.PORTATTRIBUTE == keyCELLPORTAtt.DENSE))
                                          //{
                                          //      NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                          //      string.Format("[CASSETTE={0}] [BCS <- OPI][{1}] Invoke DenseBoxCassetteService DPCassetteMapDownload_UI.",
                                          //      cst.CassetteID, command.HEADER.TRANSACTIONID));
                                          //      Invoke("DenseBoxCassetteService", "DPCassetteMapDownload_UI", new object[] { command });
                                          //}
                                          //else
                                          //{
                                                NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                    string.Format("[CASSETTE={0}] [BCS <- OPI][{1}] Invoke CassetteService CassetteMapDownload_UI.",
                                                    cst.CassetteID, command.HEADER.TRANSACTIONID));
                                                Invoke("CassetteService", "CassetteMapDownload_UI", new object[] { command });
                                          //}
                                          break;
                                    case "12":
                                          NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                              string.Format("[CASSETTE={0}] [BCS <- OPI][{1}] Invoke CassetteService CassetteProcessAborting_UI.",
                                              cst.CassetteID, command.HEADER.TRANSACTIONID));
                                          Invoke("CassetteService", "CassetteProcessAborting_UI", new object[] { command });
                                          break;
                              }
                        }

                        reply.BODY.LINENAME = command.BODY.LINENAME;
                        reply.BODY.EQUIPMENTNO = command.BODY.EQUIPMENTNO;
                        reply.BODY.CASSETTEID = command.BODY.CASSETTEID;
                        reply.BODY.CASSETTECOMMAND = command.BODY.CASSETTECOMMAND;
                        reply.BODY.PROCESSCOUNT = command.BODY.PROCESSCOUNT;

                        xMessage msg = SendReplyToOPI(command, reply);

                        Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                               string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService reply message({2}) to OPI.",
                               command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME));
                  }
                  catch (Exception ex)
                  {
                        //NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService catch message({2}) exception : {3} ",
                            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME, ex.ToString()));
                  }
            }

            /// <summary>
            /// OPI MessageSet: BCSTerminalMessageInform to OPI
            /// </summary>
            /// <param name="trxID"></param>
            /// <param name="lineName"></param>
            /// <param name="terminalText"></param>
            public void BCSTerminalMessageInform(string trxID, string lineName, string terminalText)
            {
                  try
                  {
                        IServerAgent agent = GetServerAgent();
                        XmlDocument xml_doc = agent.GetTransactionFormat("BCSTerminalMessageInform") as XmlDocument;
                        BCSTerminalMessageInform trx = Spec.XMLtoMessage(xml_doc) as BCSTerminalMessageInform;
                        //Watson Modify 20150319 因為OPI Receive是以ServerName 
                        //避免傳入值為其他lineid 而傳不到opi上顯示，所以直接在method上修改
                        //trx.BODY.LINENAME = string.IsNullOrEmpty(lineName) ? Workbench.ServerName : lineName;
                        trx.BODY.LINENAME = Workbench.ServerName;
                        //trx.BODY.LINENAME = lineName;
                        trx.BODY.DATETIME = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        trx.BODY.TERMINALTEXT = terminalText;

                        //cy:增加記錄到DB table . 20150904
                        ObjectManager.CIMMessageManager.SaveTerminalMessage(terminalText, trxID, lineName, string.Empty);

                        xMessage msg = SendReportToAllOPI(trxID, trx);

                        Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService report message to OPI.",
                            trx.BODY.LINENAME, msg.TransactionID));

                        //yang 2017/3/13
                        #region[for BCS Error Report]
                        if ((terminalText.ToUpper().Contains("ERROR") || terminalText.ToUpper().Contains("NG") || terminalText.ToUpper().Contains("CANCEL")
                            )&&!terminalText.ToUpper().Contains("OK"))
                        {
                            if (terminalText.ToUpper().Contains("CASSETTE"))
                                Invoke(eServiceName.EvisorService, "AppErrorReport", new object[] { lineName, "C001", terminalText, "1", "CASSETTE", "CASSETTE" });
                            else if (terminalText.ToUpper().Contains("RECIPE"))
                                Invoke(eServiceName.EvisorService, "AppErrorReport", new object[] { lineName, "R001", terminalText, "1", "RECIPE", "RECIPE" });
                            else if (terminalText.ToUpper().Contains("PORT"))
                                Invoke(eServiceName.EvisorService, "AppErrorReport", new object[] { lineName, "P001", terminalText, "1", "PORT", "PORT" });
                            else
                                Invoke(eServiceName.EvisorService, "AppErrorReport", new object[] { lineName, "L001", terminalText, "1", "LINE", "LINE" });
                        }
                        #endregion

                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }

            /// <summary>
            /// OPI MessageSet: BCSTerminalMessageInform to OPI
            /// </summary>
            /// <param name="trxID"></param>
            /// <param name="lineName"></param>
            /// <param name="caption"></param>
            /// <param name="terminalText"></param>
            public void BCSTerminalMessageInform(string trxID, string lineName, string caption, string terminalText)
            {
                  try
                  {
                        IServerAgent agent = GetServerAgent();
                        XmlDocument xml_doc = agent.GetTransactionFormat("BCSTerminalMessageInform") as XmlDocument;
                        BCSTerminalMessageInform trx = Spec.XMLtoMessage(xml_doc) as BCSTerminalMessageInform;
                        //Watson Modify 20150319 因為OPI Receive是以ServerName 
                        //避免傳入值為其他lineid 而傳不到opi上顯示，所以直接在method上修改
                        //trx.BODY.LINENAME = string.IsNullOrEmpty(lineName) ? Workbench.ServerName : lineName;
                        trx.BODY.LINENAME = Workbench.ServerName;
                        //trx.BODY.LINENAME = lineName;
                        trx.BODY.DATETIME = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        trx.BODY.TERMINALTEXT = "[" + caption + "]" + terminalText;

                        //cy:增加記錄到DB table . 20150904
                        ObjectManager.CIMMessageManager.SaveTerminalMessage(terminalText, trxID, lineName, caption);

                        xMessage msg = SendReportToAllOPI(trxID, trx);

                        Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService report message to OPI.",
                            trx.BODY.LINENAME, msg.TransactionID));

                      //yang 2017/3/13
                  #region[for BCS Error Report]
                  if(terminalText.ToUpper().Contains("ERROR")||terminalText.ToUpper().Contains("NG")||terminalText.ToUpper().Contains("CANCEL"))
                  {
                      if (terminalText.ToUpper().Contains("CASSETTE"))
                          Invoke(eServiceName.EvisorService, "AppErrorReport", new object[] { lineName, "C001", terminalText, "1", "CASSETTE", "CASSETTE" });
                      else if(terminalText.ToUpper().Contains("RECIPE"))
                          Invoke(eServiceName.EvisorService, "AppErrorReport", new object[] { lineName, "R001", terminalText, "1", "RECIPE", "RECIPE" });
                      else if(terminalText.ToUpper().Contains("PORT"))
                          Invoke(eServiceName.EvisorService, "AppErrorReport", new object[] { lineName, "P001", terminalText, "1", "PORT", "PORT" });
                      else
                          Invoke(eServiceName.EvisorService, "AppErrorReport", new object[] { lineName, "L001", terminalText, "1", "LINE", "LINE" });
                  }
                  #endregion

                  }                  
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }

            ///// <summary>
            ///// OPI MessageSet: BCS Terminal Message Inform Reply to BC
            ///// </summary>
            ///// <param name="xmlDoc"></param>
            //public void OPI_BCSTerminalMessageInformReply(XmlDocument xmlDoc)
            //{
            //    try
            //    {
            //        //IServerAgent agent = GetServerAgent();
            //        //{
            //        //    Message bcsTerminalMessageInformReply = Spec.CheckXMLFormat(xmlDoc);
            //        //}
            //    }
            //    catch (Exception ex)
            //    {
            //        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            //    }
            //}

            /// <summary>
            /// OPI MessageSet: Client Disconnect Request，通知OPI Client斷線
            /// </summary>
            /// <param name="trxID"></param>
            /// <param name="lineName"></param>
            /// <param name="id"></param>
            /// <param name="ip"></param>
            /// <param name="group"></param>
            /// <param name="time"></param>
            public void ClientDisconnectRequest(string trxID, string lineName, string id, string ip, string group, string time, string operatorID, string reason)
            {
                  try
                  {
                        IServerAgent agent = GetServerAgent();
                        XmlDocument xml_doc = agent.GetTransactionFormat("ClientDisconnectRequest") as XmlDocument;
                        ClientDisconnectRequest trx = Spec.XMLtoMessage(xml_doc) as ClientDisconnectRequest;

                        trx.BODY.LINENAME = lineName;
                        trx.BODY.USERID = id;
                        trx.BODY.LOGINSERVERIP = ip;
                        trx.BODY.USERGROUP = group;
                        trx.BODY.LOGINTIME = time;
                        trx.BODY.OPERATORID = operatorID;
                        trx.BODY.REASON = reason;

                        xMessage msg = SendToOPI(trxID, trx, new List<string>(1) { ip });

                        Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService report message to OPI.",
                            trx.BODY.LINENAME, msg.TransactionID));
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }

            /// <summary>
            /// OPI MessageSet: Equipment Data Link Status Report
            /// </summary>
            /// <param name="trxID"></param>
            /// <param name="batonPassStatus"></param>
            /// <param name="batonPassInterrupton"></param>
            /// <param name="dataLinkStop"></param>
            /// <param name="stationLoopStatus"></param>
            /// <param name="batonPassEachStation"></param>
            /// <param name="cycleTransmissionStatus"></param>
            public void EquipmentDataLinkStatusReport(string trxID, string batonPassStatus, string batonPassInterrupton, string dataLinkStop,
                string stationLoopStatus, string batonPassEachStation, string cycleTransmissionStatus)
            {
                  try
                  {
                        IServerAgent agent = GetServerAgent();
                        XmlDocument xml_doc = agent.GetTransactionFormat("EquipmentDataLinkStatusReport") as XmlDocument;
                        EquipmentDataLinkStatusReport trx = Spec.XMLtoMessage(xml_doc) as EquipmentDataLinkStatusReport;
                        trx.BODY.LINENAME = Workbench.ServerName;
                        trx.BODY.BATONPASSSTATUS = batonPassStatus;
                        trx.BODY.BATONPASSINTERRUPTION = batonPassInterrupton;
                        trx.BODY.DATALINKSTOP = dataLinkStop;
                        trx.BODY.STATIONLOOPSTATUS = stationLoopStatus;
                        trx.BODY.BATONPASSEACHSTATION = batonPassEachStation;
                        trx.BODY.CYCLETRANSMISSIONSTATUS = cycleTransmissionStatus;

                        xMessage msg = SendReportToAllOPI(trxID, trx);

                        Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                               string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService report message to OPI.",
                               Workbench.ServerName, msg.TransactionID));
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }

            public void OPIFlashMessage(string trxID, string fromEQP, string toEQP)
            {
                  try
                  {
                        IServerAgent agent = GetServerAgent();
                        XmlDocument xml_doc = agent.GetTransactionFormat("OPIFlashMessage") as XmlDocument;
                        OPIFlashMessage trx = Spec.XMLtoMessage(xml_doc) as OPIFlashMessage;

                        trx.BODY.LINENAME = Workbench.ServerName;
                        trx.BODY.MESSAGE = string.Format("EQUIPMENT=[{0}] notification \"Send Wait Time Out\" to the BCS , DOWNSTREAM EQUIPMENT=[{1}]!",
                            fromEQP, toEQP);
                        xMessage msg = SendReportToAllOPI(trxID, trx);
                        Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService report message to OPI.",
                            trx.BODY.LINENAME, msg.TransactionID));
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            #endregion
      }
}
