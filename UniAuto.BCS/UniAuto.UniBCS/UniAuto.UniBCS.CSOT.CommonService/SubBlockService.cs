using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniAuto.UniBCS.Core.Generic;
using UniAuto.UniBCS.PLCAgent.PLC;
using System.Reflection;
using UniAuto.UniBCS.EntityManager;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.MISC;
using UniAuto.UniBCS.Core.Message;
using UniAuto.UniBCS.Core;
using System.Threading;

namespace UniAuto.UniBCS.CSOT.CommonService
{
    public class SubBlockService : AbstractService
    {
        Thread _mplcBlcokThread;
        bool _isRuning;
        bool threadStart;
                    
        public override bool Init()
        {
            _isRuning = true;
            return true;
        }

        public void Destroy()
        {
            _isRuning = false;
        }

        public void JobCountBlock(Trx inputData)
        {
            try
            {
                //if (inputData.IsInitTrigger) return; //可能一開始就要更新，所以不跳出
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);

                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO =[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));

                IList<Unit> units = ObjectManager.UnitManager.GetUnitsByEQPNo(eqp.Data.NODENO);

                //Job Data Block
                #region [拆出PLCAgent Data]  Word 1
                Line line = ObjectManager.LineManager.GetLine(Workbench.ServerName);

                lock (eqp)
                {
                    if (eqp.File.TotalTFTJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_TFT].Value))
                        eqp.File.TotalTFTJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_TFT].Value);

                    if (eqp.File.TotalCFProductJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_CF].Value))
                        eqp.File.TotalCFProductJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_CF].Value);
                    //20160106 cy:增加判斷,避免錯誤
                    if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_DUMMY] != null)
                          if (eqp.File.TotalDummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_DUMMY].Value))
                                eqp.File.TotalDummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_DUMMY].Value);

                    if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_ThroughDummy] != null)
                          if (eqp.File.ThroughDummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_ThroughDummy].Value))
                                eqp.File.ThroughDummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_ThroughDummy].Value);

                    if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_ThicknessDummy] != null)
                          if (eqp.File.ThicknessDummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_ThicknessDummy].Value))
                                eqp.File.ThicknessDummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_ThicknessDummy].Value);

                    if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_UVMask] != null)
                          if (eqp.File.UVMASKJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_UVMask].Value))
                                eqp.File.UVMASKJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_UVMask].Value);

                    if (line.Data.FABTYPE == eFabType.CELL.ToString())//sy add 20160826
                    {
                        if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_UnassembledTFT] != null)//sy add 20160826
                            if (eqp.File.TotalUnassembledTFTJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_UnassembledTFT].Value))
                                eqp.File.TotalUnassembledTFTJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_UnassembledTFT].Value);

                        if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_ITODUMMY] != null)//sy add 20160826
                            if (eqp.File.TotalITODummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_ITODUMMY].Value))
                                eqp.File.TotalITODummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_ITODUMMY].Value);

                        if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_NIPDUMMY] != null)//sy add 20160826
                            if (eqp.File.TotalNIPDummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_NIPDUMMY].Value))
                                eqp.File.TotalNIPDummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_NIPDUMMY].Value);

                        if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_MatelOneDUMMY] != null)//sy add 20160826
                            if (eqp.File.TotalMetalOneDummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_MatelOneDUMMY].Value))
                                eqp.File.TotalMetalOneDummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_MatelOneDUMMY].Value);
                    }

                }
                ObjectManager.EquipmentManager.EnqueueSave(eqp.File);

                //Jun Modify 20150325 AC廠跟CELL廠的Unit Job Count結構不一樣，所以需要另外取值
                //bruce modify 20150629 Unit# 原1碼改為2碼 , 改用Item name 取值不用再分Shop
                //if (line.Data.FABTYPE != eFabType.CELL.ToString())
                //{
                //    #region AC
                    foreach (Unit unit in units)
                    {
                        lock (unit)
                        {
                            if (!unit.Data.UNITATTRIBUTE.Trim().Equals("ATTACHMENT"))   // add by bruce 20160505 只處理非附屬設備
                            {
                                switch (unit.Data.UNITNO)
                                {
                                    case "1":
                                        if (unit.File.TFTProductCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit01_TFT].Value))
                                            unit.File.TFTProductCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit01_TFT].Value); //Unit#01 TFTProductJobCount
                                        if (unit.File.CFProductCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit01_CF].Value))
                                            unit.File.CFProductCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit01_CF].Value); //Unit#01 CFProductJobCount
                                        #region [CELL]
                                        if (line.Data.FABTYPE == eFabType.CELL.ToString())//sy add 20160826
                                        {
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit01_DUMMY] != null)
                                                if (unit.File.DummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit01_DUMMY].Value))
                                                    unit.File.DummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit01_DUMMY].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit01_ThroughDummy] != null)
                                                if (unit.File.ThroughDummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit01_ThroughDummy].Value))
                                                    unit.File.ThroughDummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit01_ThroughDummy].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit01_ThicknessDummy] != null)
                                                if (unit.File.ThicknessDummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit01_ThicknessDummy].Value))
                                                    unit.File.ThicknessDummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit01_ThicknessDummy].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit01_UVMASK] != null)
                                                if (unit.File.UVMASKJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit01_UVMASK].Value))
                                                    unit.File.UVMASKJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit01_UVMASK].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit01_UnassembledTFT] != null)
                                                if (unit.File.UnassembledTFTJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit01_UnassembledTFT].Value))
                                                    unit.File.UnassembledTFTJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit01_UnassembledTFT].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit01_ITODummy] != null)
                                                if (unit.File.ITODummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit01_ITODummy].Value))
                                                    unit.File.ITODummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit01_ITODummy].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit01_NIPDummy] != null)
                                                if (unit.File.NIPDummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit01_NIPDummy].Value))
                                                    unit.File.NIPDummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit01_NIPDummy].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit01_MetalOneDummy] != null)
                                                if (unit.File.MetalOneDummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit01_MetalOneDummy].Value))
                                                    unit.File.MetalOneDummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit01_MetalOneDummy].Value);
                                        }
                                        #endregion
                                        break;
                                    case "2":
                                        if (unit.File.TFTProductCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit02_TFT].Value))
                                            unit.File.TFTProductCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit02_TFT].Value); //Unit#02 TFTProductJobCount
                                        if (unit.File.CFProductCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit02_CF].Value))
                                            unit.File.CFProductCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit02_CF].Value); //Unit#02 CFProductJobCount
                                        #region [CELL]
                                        if (line.Data.FABTYPE == eFabType.CELL.ToString())//sy add 20160826
                                        {
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit02_DUMMY] != null)
                                                if (unit.File.DummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit02_DUMMY].Value))
                                                    unit.File.DummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit02_DUMMY].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit02_ThroughDummy] != null)
                                                if (unit.File.ThroughDummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit02_ThroughDummy].Value))
                                                    unit.File.ThroughDummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit02_ThroughDummy].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit02_ThicknessDummy] != null)
                                                if (unit.File.ThicknessDummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit02_ThicknessDummy].Value))
                                                    unit.File.ThicknessDummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit02_ThicknessDummy].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit02_UVMASK] != null)
                                                if (unit.File.UVMASKJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit02_UVMASK].Value))
                                                    unit.File.UVMASKJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit02_UVMASK].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit02_UnassembledTFT] != null)
                                                if (unit.File.UnassembledTFTJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit02_UnassembledTFT].Value))
                                                    unit.File.UnassembledTFTJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit02_UnassembledTFT].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit02_ITODummy] != null)
                                                if (unit.File.ITODummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit02_ITODummy].Value))
                                                    unit.File.ITODummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit02_ITODummy].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit02_NIPDummy] != null)
                                                if (unit.File.NIPDummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit02_NIPDummy].Value))
                                                    unit.File.NIPDummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit02_NIPDummy].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit02_MetalOneDummy] != null)
                                                if (unit.File.MetalOneDummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit02_MetalOneDummy].Value))
                                                    unit.File.MetalOneDummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit02_MetalOneDummy].Value);
                                        }
                                        #endregion
                                        break;
                                    case "3":
                                        if (unit.File.TFTProductCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit03_TFT].Value))
                                            unit.File.TFTProductCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit03_TFT].Value); //Unit#03 TFTProductJobCount
                                        if (unit.File.CFProductCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit03_CF].Value))
                                            unit.File.CFProductCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit03_CF].Value); //Unit#03 CFProductJobCount
                                        #region [CELL]
                                        if (line.Data.FABTYPE == eFabType.CELL.ToString())//sy add 20160826
                                        {
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit03_DUMMY] != null)
                                                if (unit.File.DummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit03_DUMMY].Value))
                                                    unit.File.DummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit03_DUMMY].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit03_ThroughDummy] != null)
                                                if (unit.File.ThroughDummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit03_ThroughDummy].Value))
                                                    unit.File.ThroughDummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit03_ThroughDummy].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit03_ThicknessDummy] != null)
                                                if (unit.File.ThicknessDummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit03_ThicknessDummy].Value))
                                                    unit.File.ThicknessDummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit03_ThicknessDummy].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit03_UVMASK] != null)
                                                if (unit.File.UVMASKJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit03_UVMASK].Value))
                                                    unit.File.UVMASKJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit03_UVMASK].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit03_UnassembledTFT] != null)
                                                if (unit.File.UnassembledTFTJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit03_UnassembledTFT].Value))
                                                    unit.File.UnassembledTFTJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit03_UnassembledTFT].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit03_ITODummy] != null)
                                                if (unit.File.ITODummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit03_ITODummy].Value))
                                                    unit.File.ITODummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit03_ITODummy].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit03_NIPDummy] != null)
                                                if (unit.File.NIPDummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit03_NIPDummy].Value))
                                                    unit.File.NIPDummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit03_NIPDummy].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit03_MetalOneDummy] != null)
                                                if (unit.File.MetalOneDummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit03_MetalOneDummy].Value))
                                                    unit.File.MetalOneDummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit03_MetalOneDummy].Value);
                                        }
                                        #endregion
                                        break;
                                    case "4":
                                        if (unit.File.TFTProductCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit04_TFT].Value))
                                            unit.File.TFTProductCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit04_TFT].Value); //Unit#04 TFTProductJobCount
                                        if (unit.File.CFProductCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit04_CF].Value))
                                            unit.File.CFProductCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit04_CF].Value); //Unit#04 CFProductJobCount
                                        #region [CELL]
                                        if (line.Data.FABTYPE == eFabType.CELL.ToString())//sy add 20160826
                                        {
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit04_DUMMY] != null)
                                                if (unit.File.DummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit04_DUMMY].Value))
                                                    unit.File.DummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit04_DUMMY].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit04_ThroughDummy] != null)
                                                if (unit.File.ThroughDummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit04_ThroughDummy].Value))
                                                    unit.File.ThroughDummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit04_ThroughDummy].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit04_ThicknessDummy] != null)
                                                if (unit.File.ThicknessDummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit04_ThicknessDummy].Value))
                                                    unit.File.ThicknessDummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit04_ThicknessDummy].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit04_UVMASK] != null)
                                                if (unit.File.UVMASKJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit04_UVMASK].Value))
                                                    unit.File.UVMASKJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit04_UVMASK].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit04_UnassembledTFT] != null)
                                                if (unit.File.UnassembledTFTJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit04_UnassembledTFT].Value))
                                                    unit.File.UnassembledTFTJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit04_UnassembledTFT].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit04_ITODummy] != null)
                                                if (unit.File.ITODummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit04_ITODummy].Value))
                                                    unit.File.ITODummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit04_ITODummy].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit04_NIPDummy] != null)
                                                if (unit.File.NIPDummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit04_NIPDummy].Value))
                                                    unit.File.NIPDummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit04_NIPDummy].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit04_MetalOneDummy] != null)
                                                if (unit.File.MetalOneDummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit04_MetalOneDummy].Value))
                                                    unit.File.MetalOneDummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit04_MetalOneDummy].Value);
                                        }
                                        #endregion
                                        break;
                                    case "5":
                                        if (unit.File.TFTProductCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit05_TFT].Value))
                                            unit.File.TFTProductCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit05_TFT].Value); //Unit#05 TFTProductJobCount
                                        if (unit.File.CFProductCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit05_CF].Value))
                                            unit.File.CFProductCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit05_CF].Value); //Unit#05 CFProductJobCount
                                        #region [CELL]
                                        if (line.Data.FABTYPE == eFabType.CELL.ToString())//sy add 20160826
                                        {
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit05_DUMMY] != null)
                                                if (unit.File.DummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit05_DUMMY].Value))
                                                    unit.File.DummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit05_DUMMY].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit05_ThroughDummy] != null)
                                                if (unit.File.ThroughDummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit05_ThroughDummy].Value))
                                                    unit.File.ThroughDummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit05_ThroughDummy].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit05_ThicknessDummy] != null)
                                                if (unit.File.ThicknessDummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit05_ThicknessDummy].Value))
                                                    unit.File.ThicknessDummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit05_ThicknessDummy].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit05_UVMASK] != null)
                                                if (unit.File.UVMASKJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit05_UVMASK].Value))
                                                    unit.File.UVMASKJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit05_UVMASK].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit05_UnassembledTFT] != null)
                                                if (unit.File.UnassembledTFTJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit05_UnassembledTFT].Value))
                                                    unit.File.UnassembledTFTJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit05_UnassembledTFT].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit05_ITODummy] != null)
                                                if (unit.File.ITODummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit05_ITODummy].Value))
                                                    unit.File.ITODummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit05_ITODummy].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit05_NIPDummy] != null)
                                                if (unit.File.NIPDummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit05_NIPDummy].Value))
                                                    unit.File.NIPDummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit05_NIPDummy].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit05_MetalOneDummy] != null)
                                                if (unit.File.MetalOneDummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit05_MetalOneDummy].Value))
                                                    unit.File.MetalOneDummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit05_MetalOneDummy].Value);
                                        }
                                        #endregion
                                        break;
                                    case "6":
                                        if (unit.File.TFTProductCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit06_TFT].Value))
                                            unit.File.TFTProductCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit06_TFT].Value); //Unit#06 TFTProductJobCount
                                        if (unit.File.CFProductCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit06_CF].Value))
                                            unit.File.CFProductCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit06_TFT].Value); //Unit#06 CFProductJobCount
                                        #region [CELL]
                                        if (line.Data.FABTYPE == eFabType.CELL.ToString())//sy add 20160826
                                        {
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit06_DUMMY] != null)
                                                if (unit.File.DummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit06_DUMMY].Value))
                                                    unit.File.DummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit06_DUMMY].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit06_ThroughDummy] != null)
                                                if (unit.File.NIPDummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit06_ThroughDummy].Value))
                                                    unit.File.NIPDummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit06_ThroughDummy].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit06_ThicknessDummy] != null)
                                                if (unit.File.ThicknessDummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit06_ThicknessDummy].Value))
                                                    unit.File.ThicknessDummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit06_ThicknessDummy].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit06_UVMASK] != null)
                                                if (unit.File.UVMASKJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit06_UVMASK].Value))
                                                    unit.File.UVMASKJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit06_UVMASK].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit06_UnassembledTFT] != null)
                                                if (unit.File.UnassembledTFTJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit06_UnassembledTFT].Value))
                                                    unit.File.UnassembledTFTJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit06_UnassembledTFT].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit06_ITODummy] != null)
                                                if (unit.File.ITODummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit06_ITODummy].Value))
                                                    unit.File.ITODummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit06_ITODummy].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit06_NIPDummy] != null)
                                                if (unit.File.NIPDummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit06_NIPDummy].Value))
                                                    unit.File.NIPDummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit06_NIPDummy].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit06_MetalOneDummy] != null)
                                                if (unit.File.MetalOneDummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit06_MetalOneDummy].Value))
                                                    unit.File.MetalOneDummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit06_MetalOneDummy].Value);
                                        }
                                        #endregion
                                        break;
                                    case "7":
                                        if (unit.File.TFTProductCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit07_TFT].Value))
                                            unit.File.TFTProductCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit07_TFT].Value); //Unit#07 TFTProductJobCount
                                        if (unit.File.CFProductCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit07_CF].Value))
                                            unit.File.CFProductCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit07_CF].Value); //Unit#07 CFProductJobCount
                                        #region [CELL]
                                        if (line.Data.FABTYPE == eFabType.CELL.ToString())//sy add 20160826
                                        {
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit07_DUMMY] != null)
                                                if (unit.File.DummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit07_DUMMY].Value))
                                                    unit.File.DummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit07_DUMMY].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit07_ThroughDummy] != null)
                                                if (unit.File.ThroughDummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit07_ThroughDummy].Value))
                                                    unit.File.ThroughDummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit07_ThroughDummy].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit07_ThicknessDummy] != null)
                                                if (unit.File.ThicknessDummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit07_ThicknessDummy].Value))
                                                    unit.File.ThicknessDummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit07_ThicknessDummy].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit07_UVMASK] != null)
                                                if (unit.File.UVMASKJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit07_UVMASK].Value))
                                                    unit.File.UVMASKJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit07_UVMASK].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit07_UnassembledTFT] != null)
                                                if (unit.File.UnassembledTFTJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit07_UnassembledTFT].Value))
                                                    unit.File.UnassembledTFTJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit07_UnassembledTFT].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit07_ITODummy] != null)
                                                if (unit.File.ITODummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit07_ITODummy].Value))
                                                    unit.File.ITODummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit07_ITODummy].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit07_NIPDummy] != null)
                                                if (unit.File.NIPDummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit07_NIPDummy].Value))
                                                    unit.File.NIPDummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit07_NIPDummy].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit07_MetalOneDummy] != null)
                                                if (unit.File.MetalOneDummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit07_MetalOneDummy].Value))
                                                    unit.File.MetalOneDummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit07_MetalOneDummy].Value);
                                        }
                                        #endregion
                                        break;
                                    case "8":
                                        if (unit.File.TFTProductCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit08_TFT].Value))
                                            unit.File.TFTProductCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit08_TFT].Value); //Unit#08 TFTProductJobCount
                                        if (unit.File.CFProductCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit08_CF].Value))
                                            unit.File.CFProductCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit08_CF].Value); //Unit#08 CFProductJobCount
                                        #region [CELL]
                                        if (line.Data.FABTYPE == eFabType.CELL.ToString())//sy add 20160826
                                        {
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit08_DUMMY] != null)
                                                if (unit.File.DummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit08_DUMMY].Value))
                                                    unit.File.DummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit08_DUMMY].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit08_ThroughDummy] != null)
                                                if (unit.File.ThroughDummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit08_ThroughDummy].Value))
                                                    unit.File.ThroughDummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit08_ThroughDummy].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit08_ThicknessDummy] != null)
                                                if (unit.File.ThicknessDummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit08_ThicknessDummy].Value))
                                                    unit.File.ThicknessDummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit08_ThicknessDummy].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit08_UVMASK] != null)
                                                if (unit.File.UVMASKJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit08_UVMASK].Value))
                                                    unit.File.UVMASKJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit08_UVMASK].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit08_UnassembledTFT] != null)
                                                if (unit.File.UnassembledTFTJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit08_UnassembledTFT].Value))
                                                    unit.File.UnassembledTFTJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit08_UnassembledTFT].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit08_ITODummy] != null)
                                                if (unit.File.ITODummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit08_ITODummy].Value))
                                                    unit.File.ITODummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit08_ITODummy].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit08_NIPDummy] != null)
                                                if (unit.File.NIPDummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit08_NIPDummy].Value))
                                                    unit.File.NIPDummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit08_NIPDummy].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit08_MetalOneDummy] != null)
                                                if (unit.File.MetalOneDummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit08_MetalOneDummy].Value))
                                                    unit.File.MetalOneDummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit08_MetalOneDummy].Value);
                                        }
                                        #endregion
                                        break;
                                    case "9":
                                        if (unit.File.TFTProductCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit09_TFT].Value))
                                            unit.File.TFTProductCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit09_TFT].Value); //Unit#09 TFTProductJobCount
                                        if (unit.File.CFProductCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit09_CF].Value))
                                            unit.File.CFProductCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit09_CF].Value); //Unit#09 CFProductJobCount
                                        #region [CELL]
                                        if (line.Data.FABTYPE == eFabType.CELL.ToString())//sy add 20160826
                                        {
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit09_DUMMY] != null)
                                                if (unit.File.DummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit09_DUMMY].Value))
                                                    unit.File.DummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit09_DUMMY].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit09_ThroughDummy] != null)
                                                if (unit.File.ThroughDummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit09_ThroughDummy].Value))
                                                    unit.File.ThroughDummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit09_ThroughDummy].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit09_ThicknessDummy] != null)
                                                if (unit.File.ThicknessDummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit09_ThicknessDummy].Value))
                                                    unit.File.ThicknessDummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit09_ThicknessDummy].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit09_UVMASK] != null)
                                                if (unit.File.UVMASKJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit09_UVMASK].Value))
                                                    unit.File.UVMASKJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit09_UVMASK].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit09_UnassembledTFT] != null)
                                                if (unit.File.UnassembledTFTJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit09_UnassembledTFT].Value))
                                                    unit.File.UnassembledTFTJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit09_UnassembledTFT].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit09_ITODummy] != null)
                                                if (unit.File.ITODummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit09_ITODummy].Value))
                                                    unit.File.ITODummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit09_ITODummy].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit09_NIPDummy] != null)
                                                if (unit.File.NIPDummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit09_NIPDummy].Value))
                                                    unit.File.NIPDummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit09_NIPDummy].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit09_MetalOneDummy] != null)
                                                if (unit.File.MetalOneDummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit09_MetalOneDummy].Value))
                                                    unit.File.MetalOneDummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit09_MetalOneDummy].Value);
                                        }
                                        #endregion
                                        break;
                                    case "10":
                                        if (unit.File.TFTProductCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit10_TFT].Value))
                                            unit.File.TFTProductCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit10_TFT].Value); //Unit#10 TFTProductJobCount
                                        if (unit.File.CFProductCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit10_CF].Value))
                                            unit.File.CFProductCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit10_CF].Value); //Unit#10 CFProductJobCount
                                        #region [CELL]
                                        if (line.Data.FABTYPE == eFabType.CELL.ToString())//sy add 20160826
                                        {
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit10_DUMMY] != null)
                                                if (unit.File.DummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit10_DUMMY].Value))
                                                    unit.File.DummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit10_DUMMY].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit10_ThroughDummy] != null)
                                                if (unit.File.ThroughDummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit10_ThroughDummy].Value))
                                                    unit.File.ThroughDummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit10_ThroughDummy].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit10_ThicknessDummy] != null)
                                                if (unit.File.ThicknessDummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit10_ThicknessDummy].Value))
                                                    unit.File.ThicknessDummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit10_ThicknessDummy].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit10_UVMASK] != null)
                                                if (unit.File.UVMASKJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit10_UVMASK].Value))
                                                    unit.File.UVMASKJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit10_UVMASK].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit10_UnassembledTFT] != null)
                                                if (unit.File.UnassembledTFTJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit10_UnassembledTFT].Value))
                                                    unit.File.UnassembledTFTJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit10_UnassembledTFT].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit10_ITODummy] != null)
                                                if (unit.File.ITODummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit10_ITODummy].Value))
                                                    unit.File.ITODummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit10_ITODummy].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit10_NIPDummy] != null)
                                                if (unit.File.NIPDummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit10_NIPDummy].Value))
                                                    unit.File.NIPDummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit10_NIPDummy].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit10_MetalOneDummy] != null)
                                                if (unit.File.MetalOneDummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit10_MetalOneDummy].Value))
                                                    unit.File.MetalOneDummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit10_MetalOneDummy].Value);
                                        }
                                        #endregion
                                        break;
                                    case "11":
                                        if (unit.File.TFTProductCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit11_TFT].Value))
                                            unit.File.TFTProductCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit11_TFT].Value); //Unit#11 TFTProductJobCount
                                        if (unit.File.CFProductCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit11_CF].Value))
                                            unit.File.CFProductCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit11_CF].Value); //Unit#11 CFProductJobCount
                                        #region [CELL]
                                        if (line.Data.FABTYPE == eFabType.CELL.ToString())//sy add 20160826
                                        {
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit11_DUMMY] != null)
                                                if (unit.File.DummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit11_DUMMY].Value))
                                                    unit.File.DummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit11_DUMMY].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit11_ThroughDummy] != null)
                                                if (unit.File.ThroughDummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit11_ThroughDummy].Value))
                                                    unit.File.ThroughDummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit11_ThroughDummy].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit11_ThicknessDummy] != null)
                                                if (unit.File.ThicknessDummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit11_ThicknessDummy].Value))
                                                    unit.File.ThicknessDummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit11_ThicknessDummy].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit11_UVMASK] != null)
                                                if (unit.File.UVMASKJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit11_UVMASK].Value))
                                                    unit.File.UVMASKJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit11_UVMASK].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit11_UnassembledTFT] != null)
                                                if (unit.File.UnassembledTFTJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit11_UnassembledTFT].Value))
                                                    unit.File.UnassembledTFTJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit11_UnassembledTFT].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit11_ITODummy] != null)
                                                if (unit.File.ITODummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit11_ITODummy].Value))
                                                    unit.File.ITODummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit11_ITODummy].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit11_NIPDummy] != null)
                                                if (unit.File.NIPDummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit11_NIPDummy].Value))
                                                    unit.File.NIPDummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit11_NIPDummy].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit11_MetalOneDummy] != null)
                                                if (unit.File.MetalOneDummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit11_MetalOneDummy].Value))
                                                    unit.File.MetalOneDummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit11_MetalOneDummy].Value);
                                        }
                                        #endregion
                                        break;
                                    case "12":
                                        if (unit.File.TFTProductCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit12_TFT].Value))
                                            unit.File.TFTProductCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit12_TFT].Value); //Unit#12 TFTProductJobCount
                                        if (unit.File.CFProductCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit12_CF].Value))
                                            unit.File.CFProductCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit12_CF].Value); //Unit#12 CFProductJobCount
                                        #region [CELL]
                                        if (line.Data.FABTYPE == eFabType.CELL.ToString())//sy add 20160826
                                        {
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit12_DUMMY] != null)
                                                if (unit.File.DummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit12_DUMMY].Value))
                                                    unit.File.DummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit12_DUMMY].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit12_ThroughDummy] != null)
                                                if (unit.File.ThroughDummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit12_ThroughDummy].Value))
                                                    unit.File.ThroughDummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit12_ThroughDummy].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit12_ThicknessDummy] != null)
                                                if (unit.File.ThicknessDummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit12_ThicknessDummy].Value))
                                                    unit.File.ThicknessDummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit12_ThicknessDummy].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit12_UVMASK] != null)
                                                if (unit.File.UVMASKJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit12_UVMASK].Value))
                                                    unit.File.UVMASKJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit12_UVMASK].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit12_UnassembledTFT] != null)
                                                if (unit.File.UnassembledTFTJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit12_UnassembledTFT].Value))
                                                    unit.File.UnassembledTFTJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit12_UnassembledTFT].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit12_ITODummy] != null)
                                                if (unit.File.ITODummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit12_ITODummy].Value))
                                                    unit.File.ITODummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit12_ITODummy].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit12_NIPDummy] != null)
                                                if (unit.File.NIPDummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit12_NIPDummy].Value))
                                                    unit.File.NIPDummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit12_NIPDummy].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit12_MetalOneDummy] != null)
                                                if (unit.File.MetalOneDummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit12_MetalOneDummy].Value))
                                                    unit.File.MetalOneDummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit12_MetalOneDummy].Value);
                                        }
                                        #endregion
                                        break;
                                    case "13":
                                        if (unit.File.TFTProductCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit13_TFT].Value))
                                            unit.File.TFTProductCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit13_TFT].Value); //Unit#13 TFTProductJobCount
                                        if (unit.File.CFProductCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit13_CF].Value))
                                            unit.File.CFProductCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit13_CF].Value); //Unit#13 CFProductJobCount
                                        #region [CELL]
                                        if (line.Data.FABTYPE == eFabType.CELL.ToString())//sy add 20160826
                                        {
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit13_DUMMY] != null)
                                                if (unit.File.DummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit13_DUMMY].Value))
                                                    unit.File.DummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit13_DUMMY].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit13_ThroughDummy] != null)
                                                if (unit.File.ThroughDummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit13_ThroughDummy].Value))
                                                    unit.File.ThroughDummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit13_ThroughDummy].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit13_ThicknessDummy] != null)
                                                if (unit.File.ThicknessDummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit13_ThicknessDummy].Value))
                                                    unit.File.ThicknessDummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit13_ThicknessDummy].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit13_UVMASK] != null)
                                                if (unit.File.UVMASKJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit13_UVMASK].Value))
                                                    unit.File.UVMASKJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit13_UVMASK].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit13_UnassembledTFT] != null)
                                                if (unit.File.UnassembledTFTJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit13_UnassembledTFT].Value))
                                                    unit.File.UnassembledTFTJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit13_UnassembledTFT].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit13_ITODummy] != null)
                                                if (unit.File.ITODummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit13_ITODummy].Value))
                                                    unit.File.ITODummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit13_ITODummy].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit13_NIPDummy] != null)
                                                if (unit.File.NIPDummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit13_NIPDummy].Value))
                                                    unit.File.NIPDummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit13_NIPDummy].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit13_MetalOneDummy] != null)
                                                if (unit.File.MetalOneDummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit13_MetalOneDummy].Value))
                                                    unit.File.MetalOneDummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit13_MetalOneDummy].Value);
                                        }
                                        #endregion
                                        break;
                                    case "14":
                                        if (unit.File.TFTProductCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit14_TFT].Value))
                                            unit.File.TFTProductCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit14_TFT].Value); //Unit#14 TFTProductJobCount
                                        if (unit.File.CFProductCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit14_CF].Value))
                                            unit.File.CFProductCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit14_CF].Value); //Unit#14 CFProductJobCount
                                        #region [CELL]
                                        if (line.Data.FABTYPE == eFabType.CELL.ToString())//sy add 20160826
                                        {
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit14_DUMMY] != null)
                                                if (unit.File.DummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit14_DUMMY].Value))
                                                    unit.File.DummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit14_DUMMY].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit14_ThroughDummy] != null)
                                                if (unit.File.ThroughDummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit14_ThroughDummy].Value))
                                                    unit.File.ThroughDummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit14_ThroughDummy].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit14_ThicknessDummy] != null)
                                                if (unit.File.ThicknessDummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit14_ThicknessDummy].Value))
                                                    unit.File.ThicknessDummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit14_ThicknessDummy].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit14_UVMASK] != null)
                                                if (unit.File.UVMASKJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit14_UVMASK].Value))
                                                    unit.File.UVMASKJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit14_UVMASK].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit14_UnassembledTFT] != null)
                                                if (unit.File.UnassembledTFTJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit14_UnassembledTFT].Value))
                                                    unit.File.UnassembledTFTJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit14_UnassembledTFT].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit14_ITODummy] != null)
                                                if (unit.File.ITODummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit14_ITODummy].Value))
                                                    unit.File.ITODummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit14_ITODummy].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit14_NIPDummy] != null)
                                                if (unit.File.NIPDummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit14_NIPDummy].Value))
                                                    unit.File.NIPDummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit14_NIPDummy].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit14_MetalOneDummy] != null)
                                                if (unit.File.MetalOneDummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit14_MetalOneDummy].Value))
                                                    unit.File.MetalOneDummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit14_MetalOneDummy].Value);
                                        }
                                        #endregion
                                        break;
                                    case "15":
                                        if (unit.File.TFTProductCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit15_TFT].Value))
                                            unit.File.TFTProductCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit15_TFT].Value); //Unit#15 TFTProductJobCount
                                        if (unit.File.CFProductCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit15_CF].Value))
                                            unit.File.CFProductCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit15_CF].Value); //Unit#15 CFProductJobCount
                                        #region [CELL]
                                        if (line.Data.FABTYPE == eFabType.CELL.ToString())//sy add 20160826
                                        {
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit15_DUMMY] != null)
                                                if (unit.File.DummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit15_DUMMY].Value))
                                                    unit.File.DummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit15_DUMMY].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit15_ThroughDummy] != null)
                                                if (unit.File.ThroughDummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit15_ThroughDummy].Value))
                                                    unit.File.ThroughDummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit15_ThroughDummy].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit15_ThicknessDummy] != null)
                                                if (unit.File.ThicknessDummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit15_ThicknessDummy].Value))
                                                    unit.File.ThicknessDummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit15_ThicknessDummy].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit15_UVMASK] != null)
                                                if (unit.File.UVMASKJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit15_UVMASK].Value))
                                                    unit.File.UVMASKJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit15_UVMASK].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit15_UnassembledTFT] != null)
                                                if (unit.File.UnassembledTFTJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit15_UnassembledTFT].Value))
                                                    unit.File.UnassembledTFTJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit15_UnassembledTFT].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit15_ITODummy] != null)
                                                if (unit.File.ITODummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit15_ITODummy].Value))
                                                    unit.File.ITODummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit15_ITODummy].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit15_NIPDummy] != null)
                                                if (unit.File.NIPDummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit15_NIPDummy].Value))
                                                    unit.File.NIPDummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit15_NIPDummy].Value);
                                            if (inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit15_MetalOneDummy] != null)
                                                if (unit.File.MetalOneDummyJobCount != int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit15_MetalOneDummy].Value))
                                                    unit.File.MetalOneDummyJobCount = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.JobCount_Unit15_MetalOneDummy].Value);
                                        }
                                        #endregion
                                        break;
                                    default:
                                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("Node No=[{1}] Unit No =[{0}] > 15", unit.Data.UNITNO, unit.Data.NODENO));
                                        break;
                                }
                                ObjectManager.UnitManager.EnqueueSave(unit.File);
                            }
                        }
                    }
                //    #endregion
                //}
               // else
               // {
               //     #region Cell
                    //foreach (Unit unit in units)
                    //{
                    //    lock (unit)
                    //    {
                    //        switch (unit.Data.UNITNO)
                    //        {
                    //            case "01":
                    //                if (unit.File.TFTProductCount != int.Parse(inputData.EventGroups[0].Events[0].Items[6].Value))
                    //                    unit.File.TFTProductCount = int.Parse(inputData.EventGroups[0].Events[0].Items[6].Value); //Unit#01 TFTProductJobCount
                    //                if (unit.File.CFProductCount != int.Parse(inputData.EventGroups[0].Events[0].Items[7].Value))
                    //                    unit.File.CFProductCount = int.Parse(inputData.EventGroups[0].Events[0].Items[7].Value); //Unit#01 CFProductJobCount
                    //                break;
                    //            case "02":
                    //                if (unit.File.TFTProductCount != int.Parse(inputData.EventGroups[0].Events[0].Items[12].Value))
                    //                    unit.File.TFTProductCount = int.Parse(inputData.EventGroups[0].Events[0].Items[12].Value); //Unit#02 TFTProductJobCount
                    //                if (unit.File.CFProductCount != int.Parse(inputData.EventGroups[0].Events[0].Items[13].Value))
                    //                    unit.File.CFProductCount = int.Parse(inputData.EventGroups[0].Events[0].Items[13].Value); //Unit#02 CFProductJobCount
                    //                break;
                    //            case "03":
                    //                if (unit.File.TFTProductCount != int.Parse(inputData.EventGroups[0].Events[0].Items[18].Value))
                    //                    unit.File.TFTProductCount = int.Parse(inputData.EventGroups[0].Events[0].Items[18].Value); //Unit#03 TFTProductJobCount
                    //                if (unit.File.CFProductCount != int.Parse(inputData.EventGroups[0].Events[0].Items[19].Value))
                    //                    unit.File.CFProductCount = int.Parse(inputData.EventGroups[0].Events[0].Items[19].Value); //Unit#03 CFProductJobCount
                    //                break;
                    //            case "04":
                    //                if (unit.File.TFTProductCount != int.Parse(inputData.EventGroups[0].Events[0].Items[24].Value))
                    //                    unit.File.TFTProductCount = int.Parse(inputData.EventGroups[0].Events[0].Items[24].Value); //Unit#04 TFTProductJobCount
                    //                if (unit.File.CFProductCount != int.Parse(inputData.EventGroups[0].Events[0].Items[25].Value))
                    //                    unit.File.CFProductCount = int.Parse(inputData.EventGroups[0].Events[0].Items[25].Value); //Unit#04 CFProductJobCount
                    //                break;
                    //            case "05":
                    //                if (unit.File.TFTProductCount != int.Parse(inputData.EventGroups[0].Events[0].Items[30].Value))
                    //                    unit.File.TFTProductCount = int.Parse(inputData.EventGroups[0].Events[0].Items[30].Value); //Unit#05 TFTProductJobCount
                    //                if (unit.File.CFProductCount != int.Parse(inputData.EventGroups[0].Events[0].Items[31].Value))
                    //                    unit.File.CFProductCount = int.Parse(inputData.EventGroups[0].Events[0].Items[31].Value); //Unit#05 CFProductJobCount
                    //                break;
                    //            case "06":
                    //                if (unit.File.TFTProductCount != int.Parse(inputData.EventGroups[0].Events[0].Items[36].Value))
                    //                    unit.File.TFTProductCount = int.Parse(inputData.EventGroups[0].Events[0].Items[36].Value); //Unit#06 TFTProductJobCount
                    //                if (unit.File.CFProductCount != int.Parse(inputData.EventGroups[0].Events[0].Items[37].Value))
                    //                    unit.File.CFProductCount = int.Parse(inputData.EventGroups[0].Events[0].Items[37].Value); //Unit#06 CFProductJobCount
                    //                break;
                    //            case "07":
                    //                if (unit.File.TFTProductCount != int.Parse(inputData.EventGroups[0].Events[0].Items[42].Value))
                    //                    unit.File.TFTProductCount = int.Parse(inputData.EventGroups[0].Events[0].Items[42].Value); //Unit#07 TFTProductJobCount
                    //                if (unit.File.CFProductCount != int.Parse(inputData.EventGroups[0].Events[0].Items[43].Value))
                    //                    unit.File.CFProductCount = int.Parse(inputData.EventGroups[0].Events[0].Items[43].Value); //Unit#07 CFProductJobCount
                    //                break;
                    //            case "08":
                    //                if (unit.File.TFTProductCount != int.Parse(inputData.EventGroups[0].Events[0].Items[48].Value))
                    //                    unit.File.TFTProductCount = int.Parse(inputData.EventGroups[0].Events[0].Items[48].Value); //Unit#08 TFTProductJobCount
                    //                if (unit.File.CFProductCount != int.Parse(inputData.EventGroups[0].Events[0].Items[49].Value))
                    //                    unit.File.CFProductCount = int.Parse(inputData.EventGroups[0].Events[0].Items[49].Value); //Unit#08 CFProductJobCount
                    //                break;
                    //            case "09":
                    //                if (unit.File.TFTProductCount != int.Parse(inputData.EventGroups[0].Events[0].Items[54].Value))
                    //                    unit.File.TFTProductCount = int.Parse(inputData.EventGroups[0].Events[0].Items[54].Value); //Unit#09 TFTProductJobCount
                    //                if (unit.File.CFProductCount != int.Parse(inputData.EventGroups[0].Events[0].Items[55].Value))
                    //                    unit.File.CFProductCount = int.Parse(inputData.EventGroups[0].Events[0].Items[55].Value); //Unit#09 CFProductJobCount
                    //                break;
                    //            case "10":
                    //                if (unit.File.TFTProductCount != int.Parse(inputData.EventGroups[0].Events[0].Items[60].Value))
                    //                    unit.File.TFTProductCount = int.Parse(inputData.EventGroups[0].Events[0].Items[60].Value); //Unit#10 TFTProductJobCount
                    //                if (unit.File.CFProductCount != int.Parse(inputData.EventGroups[0].Events[0].Items[61].Value))
                    //                    unit.File.CFProductCount = int.Parse(inputData.EventGroups[0].Events[0].Items[61].Value); //Unit#10 CFProductJobCount
                    //                break;
                    //            case "11":
                    //                if (unit.File.TFTProductCount != int.Parse(inputData.EventGroups[0].Events[0].Items[66].Value))
                    //                    unit.File.TFTProductCount = int.Parse(inputData.EventGroups[0].Events[0].Items[66].Value); //Unit#11 TFTProductJobCount
                    //                if (unit.File.CFProductCount != int.Parse(inputData.EventGroups[0].Events[0].Items[67].Value))
                    //                    unit.File.CFProductCount = int.Parse(inputData.EventGroups[0].Events[0].Items[67].Value); //Unit#11 CFProductJobCount
                    //                break;
                    //            default:
                    //                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("EQUIPMENT_NO =[{1}] UNIT_NO =[{0}] > 11!", unit.Data.UNITNO, unit.Data.NODENO));
                    //                break;
                    //        }
                    //        ObjectManager.UnitManager.EnqueueSave(unit.File);
                    //    }
                    //}
               //     #endregion
               // }
                    
                Invoke(eServiceName.UIService, "EquipmentStatusReport", new object[] { inputData.TrackKey, eqp });
                #endregion
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void ProductTypeBlock(Trx inputData)
        {
            try
            {
                //if (inputData.IsInitTrigger) return; //可能一開始就要更新，所以不跳出
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);

                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO =[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));
                IList<Unit> units = ObjectManager.UnitManager.GetUnitsByEQPNo(eqp.Data.NODENO);

                //Line line = ObjectManager.LineManager.GetLine(Workbench.ServerName);
                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID =[{0}] IN EQUIPMENTENTITY!", eqp.Data.LINEID));
                #region CF Prooduct Type 拆解
                if (line.Data.FABTYPE == eFabType.CF.ToString())
                {
                    if (eqp.File.ProductType != int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value))
                        eqp.File.ProductType = int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);

                    foreach (Unit unit in units)
                    {
                        lock (unit)
                        {
                            switch (unit.Data.UNITNO)
                            {
                                case "1":
                                    if (unit.File.ProductType != int.Parse(inputData.EventGroups[0].Events[0].Items[1].Value))
                                        unit.File.ProductType = int.Parse(inputData.EventGroups[0].Events[0].Items[1].Value); //Unit#01 Product Type
                                    break;
                                case "2":
                                    if (unit.File.ProductType != int.Parse(inputData.EventGroups[0].Events[0].Items[2].Value))
                                        unit.File.ProductType = int.Parse(inputData.EventGroups[0].Events[0].Items[2].Value); //Unit#02 Product Type
                                    break;
                                case "3":
                                    if (unit.File.ProductType != int.Parse(inputData.EventGroups[0].Events[0].Items[3].Value))
                                        unit.File.ProductType = int.Parse(inputData.EventGroups[0].Events[0].Items[3].Value); //Unit#03 Product Type
                                    break;
                                case "4":
                                    if (unit.File.ProductType != int.Parse(inputData.EventGroups[0].Events[0].Items[4].Value))
                                        unit.File.ProductType = int.Parse(inputData.EventGroups[0].Events[0].Items[4].Value); //Unit#04 Product Type
                                    break;
                                case "5":
                                    if (unit.File.ProductType != int.Parse(inputData.EventGroups[0].Events[0].Items[5].Value))
                                        unit.File.ProductType = int.Parse(inputData.EventGroups[0].Events[0].Items[5].Value); //Unit#05 Product Type
                                    break;
                                case "6":
                                    if (unit.File.ProductType != int.Parse(inputData.EventGroups[0].Events[0].Items[6].Value))
                                        unit.File.ProductType = int.Parse(inputData.EventGroups[0].Events[0].Items[6].Value); //Unit#06 Product Type
                                    break;
                                case "7":
                                    if (unit.File.ProductType != int.Parse(inputData.EventGroups[0].Events[0].Items[7].Value))
                                        unit.File.ProductType = int.Parse(inputData.EventGroups[0].Events[0].Items[7].Value); //Unit#07 Product Type
                                    break;
                                case "8":
                                    if (unit.File.ProductType != int.Parse(inputData.EventGroups[0].Events[0].Items[8].Value))
                                        unit.File.ProductType = int.Parse(inputData.EventGroups[0].Events[0].Items[8].Value); //Unit#08 Product Type
                                    break;
                                default:
                                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("EQUIPMENT={1}] UNIT_NO =[{0}] > 8!", unit.Data.UNITNO, unit.Data.NODENO));
                                    break;
                            }
                        }
                        ObjectManager.UnitManager.EnqueueSave(unit.File);
                    }
                }
                #endregion

                #region CELL Prooduct Type 拆解
                if (line.Data.FABTYPE == eFabType.CELL.ToString())
                {
                    if (eqp.File.ProductType != int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value))
                        eqp.File.ProductType = int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);

                    foreach (Unit unit in units)
                    {
                        lock (unit)
                        {
                            switch (unit.Data.UNITNO)
                            {
                                case "1":
                                    if (unit.File.ProductType != int.Parse(inputData.EventGroups[0].Events[0].Items[1].Value))
                                        unit.File.ProductType = int.Parse(inputData.EventGroups[0].Events[0].Items[1].Value); //Unit#01 Product Type
                                    break;
                                case "2":
                                    if (inputData.EventGroups[0].Events[0].Items.Count < 3)  //預防 DB UNIT COUNT > TRX PRODUCT TYPE ITEMS COUNT
                                        return;
                                    if (unit.File.ProductType != int.Parse(inputData.EventGroups[0].Events[0].Items[2].Value))
                                        unit.File.ProductType = int.Parse(inputData.EventGroups[0].Events[0].Items[2].Value); //Unit#02 Product Type
                                    break;
                                case "3":
                                    if (inputData.EventGroups[0].Events[0].Items.Count < 4)  //預防 DB UNIT COUNT > TRX PRODUCT TYPE ITEMS COUNT
                                        return;
                                    if (unit.File.ProductType != int.Parse(inputData.EventGroups[0].Events[0].Items[3].Value))
                                        unit.File.ProductType = int.Parse(inputData.EventGroups[0].Events[0].Items[3].Value); //Unit#03 Product Type
                                    break;
                                case "4":
                                    if (inputData.EventGroups[0].Events[0].Items.Count < 5)  //預防 DB UNIT COUNT > TRX PRODUCT TYPE ITEMS COUNT
                                        return;
                                    if (unit.File.ProductType != int.Parse(inputData.EventGroups[0].Events[0].Items[4].Value))
                                        unit.File.ProductType = int.Parse(inputData.EventGroups[0].Events[0].Items[4].Value); //Unit#04 Product Type
                                    break;
                                case "5":
                                    if (inputData.EventGroups[0].Events[0].Items.Count < 6)  //預防 DB UNIT COUNT > TRX PRODUCT TYPE ITEMS COUNT
                                        return;
                                    if (unit.File.ProductType != int.Parse(inputData.EventGroups[0].Events[0].Items[5].Value))
                                        unit.File.ProductType = int.Parse(inputData.EventGroups[0].Events[0].Items[5].Value); //Unit#05 Product Type
                                    break;
                                case "6":
                                    if (inputData.EventGroups[0].Events[0].Items.Count < 7)  //預防 DB UNIT COUNT > TRX PRODUCT TYPE ITEMS COUNT
                                        return;
                                    if (unit.File.ProductType != int.Parse(inputData.EventGroups[0].Events[0].Items[6].Value))
                                        unit.File.ProductType = int.Parse(inputData.EventGroups[0].Events[0].Items[6].Value); //Unit#06 Product Type
                                    break;
                                case "7":
                                    if (inputData.EventGroups[0].Events[0].Items.Count < 8)  //預防 DB UNIT COUNT > TRX PRODUCT TYPE ITEMS COUNT
                                        return;
                                    if (unit.File.ProductType != int.Parse(inputData.EventGroups[0].Events[0].Items[7].Value))
                                        unit.File.ProductType = int.Parse(inputData.EventGroups[0].Events[0].Items[7].Value); //Unit#07 Product Type
                                    break;
                                case "8":
                                    if (inputData.EventGroups[0].Events[0].Items.Count < 9)  //預防 DB UNIT COUNT > TRX PRODUCT TYPE ITEMS COUNT
                                        return;
                                    if (unit.File.ProductType != int.Parse(inputData.EventGroups[0].Events[0].Items[8].Value))
                                        unit.File.ProductType = int.Parse(inputData.EventGroups[0].Events[0].Items[8].Value); //Unit#08 Product Type
                                    break;
                                case "9":  //ODF has 11 Units
                                    if (inputData.EventGroups[0].Events[0].Items.Count < 10)  //預防 DB UNIT COUNT > TRX PRODUCT TYPE ITEMS COUNT
                                        return;
                                    if (unit.File.ProductType != int.Parse(inputData.EventGroups[0].Events[0].Items[9].Value))
                                        unit.File.ProductType = int.Parse(inputData.EventGroups[0].Events[0].Items[9].Value); //Unit#09 Product Type
                                    break;
                                case "10": //ODF has 11 Units
                                    if (inputData.EventGroups[0].Events[0].Items.Count < 11)  //預防 DB UNIT COUNT > TRX PRODUCT TYPE ITEMS COUNT
                                        return;
                                    if (unit.File.ProductType != int.Parse(inputData.EventGroups[0].Events[0].Items[10].Value))
                                        unit.File.ProductType = int.Parse(inputData.EventGroups[0].Events[0].Items[10].Value); //Unit#10 Product Type
                                    break;
                                case "11": //ODF has 11 Units
                                    if (inputData.EventGroups[0].Events[0].Items.Count < 12)   //預防 DB UNIT COUNT > TRX PRODUCT TYPE ITEMS COUNT
                                        return;
                                    if (unit.File.ProductType != int.Parse(inputData.EventGroups[0].Events[0].Items[11].Value))
                                        unit.File.ProductType = int.Parse(inputData.EventGroups[0].Events[0].Items[11].Value); //Unit#11 Product Type
                                    break;
                                case "12": //CUT has 12 Units
                                    if (inputData.EventGroups[0].Events[0].Items.Count < 13)   //預防 DB UNIT COUNT > TRX PRODUCT TYPE ITEMS COUNT
                                        return;
                                    if (unit.File.ProductType != int.Parse(inputData.EventGroups[0].Events[0].Items[12].Value))
                                        unit.File.ProductType = int.Parse(inputData.EventGroups[0].Events[0].Items[12].Value); //Unit#12 Product Type
                                    break;
                                default: 
                                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("UNIT_NO =[{0}] > 11!", unit.Data.UNITNO));
                                    break;
                            }
                        }
                        ObjectManager.UnitManager.EnqueueSave(unit.File);
                    }
                }
                #endregion

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        #region [MPLC Interlock Command ]
        /// <summary>
        /// PLC Connect时呼叫
        /// </summary>
        object _syncObject = new object();
        public void StartThread()
        {
            lock (_syncObject)
            {
                if (!threadStart && (_mplcBlcokThread == null))
                {
                    _mplcBlcokThread = new Thread(new ThreadStart(MplcBlockProcess));
                    _mplcBlcokThread.IsBackground = true;
                    _mplcBlcokThread.Start();
                    threadStart = true;
                }
            }
        }

        /// <summary>
        /// PLC Disconnect时呼叫
        /// </summary>
        public void StopThread()
        {
            _mplcBlcokThread.Join();
        }
        /// <summary>
        /// 处理MPLC Intrelock OFF的thread方法
        /// </summary>
        private void MplcBlockProcess()
        {
            while (_isRuning)
            {
                Thread.Sleep(500);
                try
                {
                    if (Workbench.State != eWorkbenchState.RUN) continue;
                    if (GetServerAgent(eAgentName.PLCAgent).ConnectedState == eAGENT_STATE.DISCONNECTED) continue;
                    IDictionary<string, IList<SubBlock>> subBlocks = new Dictionary<string, IList<SubBlock>>();
                    if (ObjectManager.SubBlockManager != null)
                    {
                        subBlocks = ObjectManager.SubBlockManager.SubBlocks;
                        if (subBlocks != null)
                        {
                            foreach (var item in subBlocks)
                            {
                                string startEq = item.Key;
                                IList<SubBlock> subBlock = item.Value;
                                int productType = 0;
                                
                                if (startEq.Contains(":"))
                                {
                                    string portId = startEq.Split(new char[] { ':' })[1];
                                    if (portId.Contains("P"))
                                    {
                                        string portNo = portId.Substring(1, portId.Length - 1);
                                        Port port = ObjectManager.PortManager.GetPort(Workbench.ServerName,startEq.Split(new char[] { ':' })[0], portNo);

                                        if (port == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}],PORT_NO=[{1}] IN PORTENTITY!", startEq.Split(new char[] { ':' })[0], portNo));
                                        //update Port的interlockNo
                                        if (port.File.PortIntelockNo=="")
                                        {
                                            lock (port.File) port.File.PortIntelockNo = subBlock[0].Data.INTERLOCKNO;
                                            ObjectManager.PortManager.EnqueueSave(port.File);
                                        }
                                        //检查Port的CassetteStatus
                                        if (port.File.CassetteStatus!=eCassetteStatus.WAITING_FOR_PROCESSING&&port.File.CassetteStatus!=eCassetteStatus.IN_PROCESSING)
                                        {
                                            continue;
                                        }
                                        
                                        string portProductType = GetProductType(startEq.Split(new char[] { ':' })[0],portNo);
                                        if (portProductType == "") continue;
                                        productType = int.Parse(portProductType);
                                    }
                                    else 
                                    {
                                        string unitNo = portId;
                                        Unit unit = ObjectManager.UnitManager.GetUnit(startEq.Split(new char[] { ':' })[0], unitNo);
                                        if (unit == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO =[{0}],UNIT_NO =[{1}] IN UNITENTITY!", startEq.Split(new char[] { ':' })[0], unitNo));
                                        productType = unit.File.ProductType;
                                    }
                                }
                                else
                                {
                                    Equipment eq = ObjectManager.EquipmentManager.GetEQP(startEq);
                                    if (eq == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO =[{0}] IN EQUIPMENTENTITY!", startEq));
                                    productType = eq.File.ProductType;
                                }
                                for (int i = 0; i < subBlock.Count; i++)
                                {
                                    #region [先看ENABLED是否开启]
                                    if (subBlock[i].Data.ENABLED != "Y")
                                    {
                                        string _trxName = string.Format("{0}_MPLCInterlockCommand#{1}", subBlock[i].Data.CONTROLEQP, subBlock[i].Data.INTERLOCKNO.PadLeft(2, '0'));
                                        Trx _trxData = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { _trxName, false }) as Trx;
                                        if (_trxData == null)
                                        {
                                            subBlock[i].Data.ENABLED = "N";
                                            continue;
                                        }
                                        if (_trxData.EventGroups[0].Events[0].Items[0].Value == "1")
                                        {
                                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ,MPLCINTERLOCKNO =[{4}] (OFF):STARTEQUIPMENT =[{2}], PRODUCT TYPE MISMATCH WITH NEXTSUBBLOCKEQUIPMENT =[{3}] AND NEXTSUBBLOCKEQLIST GLASS COUNT IS EQUAL TO 0!",
                                                subBlock[i].Data.CONTROLEQP, UtilityMethod.GetAgentTrackKey(), subBlock[i].Data.STARTEQP, subBlock[i].Data.NEXTSUBBLOCKEQP, subBlock[i].Data.INTERLOCKNO.PadLeft(2, '0')));
                                            MPLCInterlockCommand(subBlock[i].Data.CONTROLEQP, subBlock[i].Data.INTERLOCKNO, eBitResult.OFF);
                                            //同样的MPLCInterlock#NO会来不及读取最新的PLC value,导致重复呼叫MPLCInterlockCommand 
                                            Thread.Sleep(100);
                                        }
                                        continue;
                                    }
                                    #endregion

                                    if (subBlock[i].Data.CONTROLEQP == "" || subBlock[i].Data.STARTEQP == "" || subBlock[i].Data.INTERLOCKNO == "" || subBlock[i].Data.NEXTSUBBLOCKEQP == "" || subBlock[i].Data.NEXTSUBBLOCKEQPLIST == "")
                                    {
                                        continue;
                                    }

                                    #region [检查MPLCIntelock是否OFF]
                                    string controlEq = subBlock[i].Data.CONTROLEQP;
                                    string interlockNo = subBlock[i].Data.INTERLOCKNO;
                                    string trxName = string.Format("{0}_MPLCInterlockCommand#{1}", controlEq, interlockNo.PadLeft(2, '0'));
                                    Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                                    if (outputdata == null)
                                    {
                                        if (subBlock[i].Data.REMARK != "NG")
                                        {
                                            string strError = string.Format("NOT FOUND TRX =[{0}]!", trxName);
                                            Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strError);
                                        }
                                        subBlock[i].Data.REMARK = "NG";
                                        continue;
                                    }
                                    Trx trxData = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxName,false }) as Trx;
                                    if (trxData == null)
                                    {
                                        subBlock[i].Data.ENABLED = "N";
                                        continue;
                                    }
                                    if (trxData.EventGroups[0].Events[0].Items[0].Value == "0")
                                    {
                                        continue;
                                    }
                                    #endregion

                                    Equipment toEq = ObjectManager.EquipmentManager.GetEQP(subBlock[i].Data.NEXTSUBBLOCKEQP);
                                    if (toEq == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO =[{0}] IN EQUIPMENTENTITY!", subBlock[i].Data.NEXTSUBBLOCKEQP));
                                    if (productType == toEq.File.ProductType)
                                    {
                                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                            string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ,MPLCINTERLOCKNO =[{4}] (OFF):STARTEQUIPMENT =[{2}],PRODUCT TYPE MATCH WITH NEXT MATCH WITH NEXTSUBBLOCKEQUIPMENT =[{3}]",
                                            subBlock[i].Data.CONTROLEQP, UtilityMethod.GetAgentTrackKey(), subBlock[i].Data.STARTEQP, subBlock[i].Data.NEXTSUBBLOCKEQP, subBlock[i].Data.INTERLOCKNO.PadLeft(2, '0')));
                                        MPLCInterlockCommand(subBlock[i].Data.CONTROLEQP, subBlock[i].Data.INTERLOCKNO, eBitResult.OFF);
                                        Thread.Sleep(100);
                                        //ObjectManager.SubBlockManager.ReloadByUI();
                                        continue;
                                    }
                                    //检查是否已经有其它port的interlock已经off,防止几个Port同时满足off的情况
                                    if (startEq.Contains("P"))
                                    {
                                        Equipment eq = ObjectManager.EquipmentManager.GetEQP(startEq.Split(new char[] { ':' })[0]);
                                        if (eq == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO =[{0}] IN EQUIPMENTENTITY!", startEq));
                                        List<Port> portList = ObjectManager.PortManager.GetPorts(eq.Data.NODEID);
                                        bool otherPortOff = false;
                                        for (int j = 0; j < portList.Count; j++)
                                        {
                                            if (portList[j].File.PortInterlockStatus == "0" && (portList[j].File.CassetteStatus == eCassetteStatus.WAITING_FOR_PROCESSING || portList[j].File.CassetteStatus == eCassetteStatus.IN_PROCESSING))
                                            {
                                                otherPortOff = true;
                                                break;
                                            }
                                        }
                                        if (otherPortOff)
                                        {
                                            continue;
                                        }
                                    }
                                    string nextEqlist = subBlock[i].Data.NEXTSUBBLOCKEQPLIST;
                                    string[] strEq = nextEqlist.Split(new char[] { ';' });
                                    string nextEq = "";
                                    string nextUnit = "";
                                    bool checkMPLC = false;
                                    int count = 0;
                                    string portId = string.Empty;

                                    if (startEq.Contains("P"))
                                    {
                                        portId = startEq.Split(new char[] { ':' })[1];
                                    }
                                    if (portId.Contains("P"))
                                    {
                                        string portNo = portId.Substring(1, portId.Length - 1);
                                        Port port = ObjectManager.PortManager.GetPort(Workbench.ServerName, startEq.Split(new char[] { ':' })[0], portNo);
                                        if (port == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}],PORT_NO=[{1}] IN PORTENTITY!", startEq.Split(new char[] { ':' })[0], portNo));
                                        string _portProductType = GetProductType(startEq.Split(new char[] { ':' })[0], port.Data.PORTNO);







                                        Equipment eq = ObjectManager.EquipmentManager.GetEQP(startEq.Split(new char[] { ':' })[0]);
                                        if (eq == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO =[{0}] IN EQUIPMENTENTITY!", startEq));
                                        List<Port> portList = ObjectManager.PortManager.GetPorts(eq.Data.NODEID);
                                        bool otherPortOff = false;
                                        for (int j = 0; j < portList.Count; j++)
                                        {
                                            if (portList[j].File.PortInterlockStatus == "0" && (portList[j].File.CassetteStatus == eCassetteStatus.WAITING_FOR_PROCESSING || portList[j].File.CassetteStatus == eCassetteStatus.IN_PROCESSING))
                                            {
                                                string portProductType = GetProductType(startEq.Split(new char[] { ':' })[0], portList[j].Data.PORTNO);
                                                if (_portProductType != portProductType)
                                                {
                                                    otherPortOff = true;
                                                    break;
                                                }
                                            }
                                        }
                                        if (otherPortOff)
                                        {
                                            continue;
                                        }

                                        checkMPLC = false;
                                        for (int x = 0; x < strEq.Length; x++)
                                        {
                                            nextEq = strEq[x];
                                            if (nextEq.Contains(":"))
                                            {
                                                nextEq = strEq[x].Split(new char[] { ':' })[0];
                                                nextUnit = strEq[x].Split(new char[] { ':' })[1];
                                                Unit toUnit = ObjectManager.UnitManager.GetUnit(nextEq, nextUnit);
                                                if (toUnit == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO =[{0}],UNIT_NO =[{1}] IN UNITENTITY!", nextEq, nextUnit));
                                                if (productType != toUnit.File.ProductType && toUnit.File.ProductType != 0)
                                                {
                                                    checkMPLC = true;
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                Equipment toEQ = ObjectManager.EquipmentManager.GetEQP(nextEq);
                                                if (toEQ == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO =[{0}] IN EQUIPMENTENTITY!", nextEq));
                                                if (productType != toEQ.File.ProductType && toEQ.File.ProductType != 0)
                                                {
                                                    checkMPLC = true;
                                                    break;
                                                }
                                            }
                                        }

                                        count = 0;
                                        for (int y = 0; y < strEq.Length; y++)
                                        {
                                            nextEq = strEq[y];
                                            if (nextEq.Contains(":"))
                                            {
                                                nextEq = strEq[y].Split(new char[] { ':' })[0];
                                                nextUnit = strEq[y].Split(new char[] { ':' })[1];
                                                Unit toUnit = ObjectManager.UnitManager.GetUnit(nextEq, nextUnit);
                                                if (toUnit == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO =[{0}],UNIT_NO =[{1}] IN UNITENTITY!", nextEq, nextUnit));
                                                count += toUnit.File.CFProductCount + toUnit.File.TFTProductCount;
                                            }
                                            else
                                            {
                                                Equipment toEQ = ObjectManager.EquipmentManager.GetEQP(nextEq);
                                                if (toEQ == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO =[{0}] IN EQUIPMENTENTITY!", nextEq));
                                                count += toEQ.File.TotalCFProductJobCount + toEQ.File.TotalTFTJobCount;
                                            }
                                        }

                                        if ((!checkMPLC || count == 0) && port.File.CassetteStatus != eCassetteStatus.PROCESS_PAUSED)
                                        {
                                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ,MPLCINTERLOCKNO =[{4}] (OFF):STARTEQUIPMENT =[{2}], PRODUCT TYPE MISMATCH WITH NEXTSUBBLOCKEQUIPMENT =[{3}] AND NEXTSUBBLOCKEQLIST GLASS COUNT IS EQUAL TO 0!",
                                                subBlock[i].Data.CONTROLEQP, UtilityMethod.GetAgentTrackKey(), subBlock[i].Data.STARTEQP, subBlock[i].Data.NEXTSUBBLOCKEQP, subBlock[i].Data.INTERLOCKNO.PadLeft(2, '0')));
                                            MPLCInterlockCommand(subBlock[i].Data.CONTROLEQP, subBlock[i].Data.INTERLOCKNO, eBitResult.OFF);
                                            //同样的MPLCInterlock#NO会来不及读取最新的PLC value,导致重复呼叫MPLCInterlockCommand 
                                            Thread.Sleep(100);
                                        }
                                    }
                                    else
                                    {
                                        checkMPLC = false;
                                        for (int x = 0; x < strEq.Length; x++)
                                        {
                                            nextEq = strEq[x];
                                            if (nextEq.Contains(":"))
                                            {
                                                nextEq = strEq[x].Split(new char[] { ':' })[0];
                                                nextUnit = strEq[x].Split(new char[] { ':' })[1];
                                                Unit toUnit = ObjectManager.UnitManager.GetUnit(nextEq, nextUnit);
                                                if (toUnit == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO =[{0}],UNIT_NO =[{1}] IN UNITENTITY!", nextEq, nextUnit));
                                                if (productType != toUnit.File.ProductType && toUnit.File.ProductType != 0)
                                                {
                                                    checkMPLC = true;
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                Equipment toEQ = ObjectManager.EquipmentManager.GetEQP(nextEq);
                                                if (toEQ == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO =[{0}] IN EQUIPMENTENTITY!", nextEq));
                                                if (productType != toEQ.File.ProductType && toEQ.File.ProductType != 0)
                                                {
                                                    checkMPLC = true;
                                                    break;
                                                }
                                            }
                                        }

                                        count = 0;
                                        for (int y = 0; y < strEq.Length; y++)
                                        {
                                            nextEq = strEq[y];
                                            if (nextEq.Contains(":"))
                                            {
                                                nextEq = strEq[y].Split(new char[] { ':' })[0];
                                                nextUnit = strEq[y].Split(new char[] { ':' })[1];
                                                Unit toUnit = ObjectManager.UnitManager.GetUnit(nextEq, nextUnit);
                                                if (toUnit == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO =[{0}],UNIT_NO =[{1}] IN UNITENTITY!", nextEq, nextUnit));
                                                count += toUnit.File.CFProductCount + toUnit.File.TFTProductCount;
                                            }
                                            else
                                            {
                                                Equipment toEQ = ObjectManager.EquipmentManager.GetEQP(nextEq);
                                                if (toEQ == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO =[{0}] IN EQUIPMENTENTITY!", nextEq));
                                                count += toEQ.File.TotalCFProductJobCount + toEQ.File.TotalTFTJobCount;
                                            }
                                        }

                                        if (!checkMPLC || count == 0)
                                        {
                                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ,MPLCINTERLOCKNO =[{4}] (OFF):STARTEQUIPMENT =[{2}],PRODUCT TYPE MATCH WITH NEXT MATCH WITH NEXTSUBBLOCKEQUIPMENT =[{3}]",
                                                subBlock[i].Data.CONTROLEQP, UtilityMethod.GetAgentTrackKey(), subBlock[i].Data.STARTEQP, subBlock[i].Data.NEXTSUBBLOCKEQP, subBlock[i].Data.INTERLOCKNO.PadLeft(2, '0')));
                                            MPLCInterlockCommand(subBlock[i].Data.CONTROLEQP, subBlock[i].Data.INTERLOCKNO, eBitResult.OFF);
                                            Thread.Sleep(100);
                                            //ObjectManager.SubBlockManager.ReloadByUI();
                                            continue;
                                        }
                                    }
                                }
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex.ToString());

                }

            }

        }

        private string GetProductType(string eqNo, string portId)
        {
            string produtType = "";
            try
            {
                string strName = string.Format("{0}_Port#{1}JobEachCassetteSlotPositionBlock", eqNo, portId);
                Trx positionTrx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { strName,false }) as Trx;

                if (positionTrx == null)
                {
                    string strError = string.Format("NOT FOUND TRX =[{0}]!", strName);
                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strError);
                }
                else
                {
                    int iPos = 1;
                    for (int i = 0; i < positionTrx.EventGroups[0].Events[0].Items.Count; i += 2)
                    {
                        Job job = ObjectManager.JobManager.GetJob(positionTrx.EventGroups[0].Events[0].Items[i].Value, positionTrx.EventGroups[0].Events[0].Items[i + 1].Value);
                        if (job != null)
                        {
                            produtType=job.ProductType.Value.ToString();
                            return produtType;
                        }
                        iPos++;
                    }
                }
                return produtType;
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return "";
            }
        }

        /// <summary>
        /// 提供给外部调用触发MPLC Intrelock ON
        /// </summary>
        /// <param name="nodeno"></param>
        /// <param name="unitno"></param>
        /// <param name="eventName"></param>
        public void CheckMplcBlock(string nodeno, string unitno, string eventName)
        {
            try
            {
                #region [判断有无对应的设定]
                IDictionary<string, IList<SubBlock>> subBlocks = new Dictionary<string, IList<SubBlock>>();
                subBlocks = ObjectManager.SubBlockManager.GetBlock(nodeno, unitno, eventName);
                if (subBlocks == null || subBlocks.Count == 0)
                {
                    return;
                }
                #endregion

                int productType;
                string startEq;
                if (unitno != "" && unitno != "0")
                {
                    Unit unit = ObjectManager.UnitManager.GetUnit(nodeno, unitno);
                    if (unit == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO =[{0}],UNIT_NO =[{1}] IN UNITENTTITY!", nodeno, unitno));
                    productType = unit.File.ProductType;
                    startEq = string.Format("{0}:{1}", nodeno, unitno);
                }
                else
                {
                    Equipment eq = ObjectManager.EquipmentManager.GetEQP(nodeno);
                    if (eq == null)
                    {
                        throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO =[{0}] IN EQUIPMENTENTITY!", nodeno));
                    }
                    productType = eq.File.ProductType;
                    startEq = nodeno;
                }
                IList<SubBlock> list = subBlocks[startEq];

                foreach (SubBlock item in list)
                {
                    #region [先看ENABLED是否开启]
                    if (item.Data.ENABLED != "Y")
                    {
                        continue;
                    }
                    #endregion

                    Equipment toEq = ObjectManager.EquipmentManager.GetEQP(item.Data.NEXTSUBBLOCKEQP);
                    if (toEq == null) throw new Exception(string.Format("CAN'T FIND SUBBLOCK TOEQUIPMENT_NO =[{0}] IN EQUIPMENTENTITTY!", item.Data.NEXTSUBBLOCKEQP));
                    string trxName = string.Format("{0}_MPLCInterlockCommand#{1}", item.Data.CONTROLEQP, item.Data.INTERLOCKNO.PadLeft(2, '0'));

                    Trx trxData = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxName,false }) as Trx;
                    if (trxData == null) continue;
                    if (trxData.EventGroups[0].Events[0].Items[0].Value == "1")
                    {
                        continue;
                    }
                    if (productType == toEq.File.ProductType)
                    {
                        continue;
                    }
                    string nextEqlist = item.Data.NEXTSUBBLOCKEQPLIST;
                    string[] strEq = nextEqlist.Split(new char[] { ';' });
                    string nextEq = "";
                    string nextUnit = "";

                    bool checkMPLC = false;
                    for (int i = 0; i < strEq.Length; i++)
                    {
                        nextEq = strEq[i];
                        if (nextEq.Contains(":"))
                        {
                            nextEq = strEq[i].Split(new char[] { ':' })[0];
                            nextUnit = strEq[i].Split(new char[] { ':' })[1];
                            Unit toUnit = ObjectManager.UnitManager.GetUnit(nextEq, nextUnit);
                            if (toUnit == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO =[{0}],UNIT_NO =[{1}] IN UNITENTITY!", nextEq, nextUnit));
                            if (productType != toUnit.File.ProductType && toUnit.File.ProductType != 0)
                            {
                                checkMPLC = true;
                                break;
                            }
                        }
                        else
                        {
                            Equipment toEQ = ObjectManager.EquipmentManager.GetEQP(nextEq);
                            if (toEQ == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO =[{0}] IN EQUIPMENTENTITY!", nextEq));
                            if (productType != toEQ.File.ProductType && toEQ.File.ProductType != 0)
                            {
                                checkMPLC = true;
                                break;
                            }
                        }
                    }

                    int count = 0;
                    for (int i = 0; i < strEq.Length; i++)
                    {
                        nextEq = strEq[i];
                        if (nextEq.Contains(":"))
                        {
                            nextEq = strEq[i].Split(new char[] { ':' })[0];
                            nextUnit = strEq[i].Split(new char[] { ':' })[1];
                            Unit toUnit = ObjectManager.UnitManager.GetUnit(nextEq, nextUnit);
                            if (toUnit == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO =[{0}],UNIT_NO =[{1}] IN UNITENTITY!", nextEq, nextUnit));
                            count += toUnit.File.CFProductCount + toUnit.File.TFTProductCount;
                        }
                        else
                        {
                            Equipment toEQ = ObjectManager.EquipmentManager.GetEQP(nextEq);
                            if (toEQ == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO =[{0}] IN EQUIPMENTENTITY!", nextEq));
                            count += toEQ.File.TotalCFProductJobCount + toEQ.File.TotalTFTJobCount;
                        }
                    }
                    if (count > 0)
                    {
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ,MPLCINTERLOCKNO =[{4}] (ON):STARTEQUIPMENT =[{2}], PRODUCT TYPE MISMATCH WITH NEXTSUBBLOCKEQUIPMENT =[{3}] AND NEXTSUBBLOCKEQPLIST GLASS COUNT MORE THAN 0",
                            item.Data.CONTROLEQP, UtilityMethod.GetAgentTrackKey(), item.Data.STARTEQP, item.Data.NEXTSUBBLOCKEQP, item.Data.INTERLOCKNO.PadLeft(2, '0')));
                        MPLCInterlockCommand(item.Data.CONTROLEQP, item.Data.INTERLOCKNO, eBitResult.ON);
                    }
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void CheckMplcBlockByPort(string nodeno, string portno,string productType)
        {
            try
            {
                #region [判断有无对应的设定]
                IDictionary<string, IList<SubBlock>> subBlocks = new Dictionary<string, IList<SubBlock>>();
                subBlocks = ObjectManager.SubBlockManager.GetBlock(nodeno, portno);
                if (subBlocks == null || subBlocks.Count == 0)
                {
                    return;
                }
                #endregion

                string startEq = string.Format("{0}:{1}", nodeno, portno);

                IList<SubBlock> list = subBlocks[startEq];
                Port port = ObjectManager.PortManager.GetPort(Workbench.ServerName, startEq.Split(new char[] { ':' })[0], portno.Substring(1, portno.Length - 1));

                if (port == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}],PORT_NO=[{1}] IN PORTENTITY!", startEq.Split(new char[] { ':' })[0], portno.Substring(1, portno.Length - 1)));

                //update Port的interlockNo
                if (port.File.PortIntelockNo == "")
                {
                    lock (port.File) port.File.PortIntelockNo = list[0].Data.INTERLOCKNO;
                    ObjectManager.PortManager.EnqueueSave(port.File);
                }
                foreach (SubBlock item in list)
                {
                    #region [先看ENABLED是否开启]
                    if (item.Data.ENABLED != "Y")
                    {
                        continue;
                    }
                    #endregion

                    Equipment toEq = ObjectManager.EquipmentManager.GetEQP(item.Data.NEXTSUBBLOCKEQP);
                    if (toEq == null) throw new Exception(string.Format("CAN'T FIND SUBBLOCK TOEQUIPMENT_NO =[{0}] IN EQUIPMENTENTITTY!", item.Data.NEXTSUBBLOCKEQP));
                    string trxName = string.Format("{0}_MPLCInterlockCommand#{1}", item.Data.CONTROLEQP, item.Data.INTERLOCKNO.PadLeft(2, '0'));

                    Trx trxData = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxName, false }) as Trx;
                    if (trxData == null) continue;
                    if (trxData.EventGroups[0].Events[0].Items[0].Value == "1")
                    {
                        continue;
                    }

                    string nextEqlist = item.Data.NEXTSUBBLOCKEQPLIST;
                    string[] strEq = nextEqlist.Split(new char[] { ';' });
                    string nextEq = "";
                    string nextUnit = "";

                    bool checkMPLC = false;
                    for (int i = 0; i < strEq.Length; i++)
                    {
                        nextEq = strEq[i];
                        if (nextEq.Contains(":"))
                        {
                            nextEq = strEq[i].Split(new char[] { ':' })[0];
                            nextUnit = strEq[i].Split(new char[] { ':' })[1];
                            Unit toUnit = ObjectManager.UnitManager.GetUnit(nextEq, nextUnit);
                            if (toUnit == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO =[{0}],UNIT_NO =[{1}] IN UNITENTITY!", nextEq, nextUnit));
                            if (int.Parse(productType) != toUnit.File.ProductType && toUnit.File.ProductType != 0)
                            {
                                checkMPLC = true;
                                break;
                            }
                        }
                        else
                        {
                            Equipment toEQ = ObjectManager.EquipmentManager.GetEQP(nextEq);
                            if (toEQ == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO =[{0}] IN EQUIPMENTENTITY!", nextEq));
                            if (int.Parse(productType) != toEQ.File.ProductType && toEQ.File.ProductType != 0)
                            {
                                checkMPLC = true;
                                break;
                            }
                        }
                    }

                    int count = 0;
                    for (int i = 0; i < strEq.Length; i++)
                    {
                        nextEq = strEq[i];
                        if (nextEq.Contains(":"))
                        {
                            nextEq = strEq[i].Split(new char[] { ':' })[0];
                            nextUnit = strEq[i].Split(new char[] { ':' })[1];
                            Unit toUnit = ObjectManager.UnitManager.GetUnit(nextEq, nextUnit);
                            if (toUnit == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO =[{0}],UNIT_NO =[{1}] IN UNITENTITY!", nextEq, nextUnit));
                            count += toUnit.File.CFProductCount + toUnit.File.TFTProductCount;
                        }
                        else
                        {
                            Equipment toEQ = ObjectManager.EquipmentManager.GetEQP(nextEq);
                            if (toEQ == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO =[{0}] IN EQUIPMENTENTITY!", nextEq));
                            count += toEQ.File.TotalCFProductJobCount + toEQ.File.TotalTFTJobCount;
                        }
                    }

                    if (checkMPLC == true && count > 0)
                    {
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ,MPLCINTERLOCKNO =[{4}] (ON):STARTEQUIPMENT =[{2}], PRODUCT TYPE MISMATCH WITH NEXTSUBBLOCKEQUIPMENT =[{3}]",
                                item.Data.CONTROLEQP, UtilityMethod.GetAgentTrackKey(), item.Data.STARTEQP, item.Data.NEXTSUBBLOCKEQP, item.Data.INTERLOCKNO.PadLeft(2, '0')));
                        MPLCInterlockCommand(item.Data.CONTROLEQP, item.Data.INTERLOCKNO, eBitResult.ON);
                    }
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        /// <summary>
        ///供外部调用下MPLCInterlockCommand
        /// </summary>
        /// <param name="eqpNo"></param>
        /// <param name="commandNo"></param>
        /// <param name="result"></param>
        public void MPLCInterlockCommand(string eqpNo, string commandNo, eBitResult result)
        {
            try
            {
                Equipment eqp;
                string err = string.Empty;
                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null)
                {
                    //TODO:打訊息到發送Command的OPI
                    throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO =[{0}] IN EQUIPMENTENTITY!", eqpNo));
                }
                // CIM MODE OFF 不能改
                if (eqp.File.CIMMode == eBitResult.OFF)
                {
                    err = string.Format("[EQUIPMENT={0}] CIM MODE (OFF), CAN NOT SEND MPLCINTERLOCKCOMMAND!", eqpNo);
                    //TODO: 打訊息到OPI
                    LogWarn(MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }
                string trxName = string.Format("{0}_MPLCInterlockCommand#{1}", eqpNo, commandNo.PadLeft(2, '0'));

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                if (outputdata == null)
                {
                    string strError = string.Format("NOT FOUND TRX =[{0}]!", trxName);
                    LogError(MethodBase.GetCurrentMethod().Name + "()", strError);
                    return;
                }
                Trx trxData = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxName,false }) as Trx;

                if (trxData[0][0][0].Value == "0" && result == eBitResult.ON)
                {
                    outputdata[0][0][0].Value = ((int)eBitResult.ON).ToString();
                    outputdata.TrackKey = UtilityMethod.GetAgentTrackKey();
                    SendPLCData(outputdata);
                    Repository.Add(outputdata.Name, outputdata);
                    LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] , SET BIT (ON)", eqp.Data.NODENO,
                            outputdata.TrackKey));
                }
                else if (trxData[0][0][0].Value == "1" && result == eBitResult.OFF)
                {
                    outputdata[0][0][0].Value = ((int)eBitResult.OFF).ToString();
                    outputdata.TrackKey = UtilityMethod.GetAgentTrackKey();
                    SendPLCData(outputdata);
                    Repository.Add(outputdata.Name, outputdata);
                    LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] , SET BIT (OFF)", eqp.Data.NODENO,
                            outputdata.TrackKey));
                }
                else
                {
                    Repository.Add(outputdata.Name, trxData);
                }
                List<Port> portList = ObjectManager.PortManager.GetPorts(eqp.Data.NODEID);
                if (portList == null || portList.Count == 0)
                {
                    return;
                }
                for (int i = 0; i < portList.Count; i++)
                {
                    if (portList[i].File.PortIntelockNo==commandNo.PadLeft(2, '0'))
                    {
                        lock (portList[i].File) portList[i].File.PortInterlockStatus = ((int)result).ToString();
                        ObjectManager.PortManager.EnqueueSave(portList[i].File);
                        LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] , SET PORTNO[{2}] PORTINTERLOCKSTATUS[{3}]", eqp.Data.NODENO,
                            outputdata.TrackKey, portList[i].Data.PORTNO,result));
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                this.LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void MPLCInterlockCommandReply(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                eBitResult triggerBit = (eBitResult)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);
                Repository.Add(inputData.Name, inputData);
                //OPI畫面自動更新需BCS回報EQ狀態 by chia-chi 2015/1/28
                string[] trxName = inputData.Name.Split(new char[] { '_' }, StringSplitOptions.None);
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(trxName[0]);
                Invoke(eServiceName.UIService, "EquipmentStatusReport", new object[] { inputData.TrackKey, eqp });
                if (triggerBit == eBitResult.OFF) return;                
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        private void SendPLCData(Trx outputData)
        {
            xMessage msg = new xMessage();
            msg.Data = outputData;
            msg.ToAgent = eAgentName.PLCAgent;
            PutMessage(msg);
        }
    }
}
