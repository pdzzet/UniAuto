using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Spring.Context;
using Spring.Objects.Factory;
using System.Diagnostics;
using System.Reflection;
using UniAuto.UniBCS.Log;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.EntityManager;
using UniAuto.UniBCS.Core.Generic;
using UniAuto.UniBCS.MesSpec;
using UniAuto.UniBCS.CSOT.MESMessageService;
using System.Collections;

namespace UniAuto.UniBCS.CSOT.MESMessageService
{
    public partial class FrmTestMES : Form, IApplicationContextAware
    {
        private IApplicationContext _applicationContext;
        private string _nikonEqpNo;
        private string _nikonEqpId;
		private string _csotEqpNo;
		private string _csotEqpId;
        private string trxID;
        private string lineName;

        public FrmTestMES()
        {
            InitializeComponent();
            _nikonEqpNo = "L7";
            _nikonEqpId = "TBPNK180";
			_csotEqpNo = "L3";
			_csotEqpId = "TBPDN130";
            trxID = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            lineName = "FCBPH100";
        }

        public IApplicationContext ApplicationContext
        {
            set { _applicationContext = value; }
        }

        private void InvkeMethod(string className, string methodName, object[] @param)
        {
            IObjectFactory objFactory = (IObjectFactory)_applicationContext;

            try
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                object obj = objFactory.GetObject(className);
                if (obj != null)
                {
                    Type[] typeList = GetParameterType(@param);
                    MethodInfo mi = obj.GetType().GetMethod(methodName, typeList);
                    if (mi == null)
                    {
                        NLogManager.Logger.LogErrorWrite("Service", this.GetType().Name, MethodBase.GetCurrentMethod().Name, string.Format("Methed {0} is not exist in class {1}.", "RecipeRegisterValidationCommand", obj.GetType().ToString()));
                        return;
                    }

                    mi.Invoke(obj, @param);
                }
                sw.Stop();
                System.Console.WriteLine(string.Format("Run time={0}.", sw.ElapsedMilliseconds));


            }
            catch (System.Exception ex)
            {
                NLogManager.Logger.LogErrorWrite("Service", this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        private Type[] GetParameterType(object[] @param)
        {
            Type[] typeList = new Type[@param.Length];
            for (int i = 0; i < @param.Length; i++)
            {
                typeList[i] = @param[i].GetType();
            }
            return typeList;
        }

        #region Simulator Sample Data
        private Job NewJob()
        {
            Job job = new Job();
            job.CassetteSequenceNo = "0123456789";
            job.MesProduct.PRODUCTNAME = "1100560080";
            job.LineRecipeName = "310015";
            job.ToCstID = "1";
            job.ToSlotNo = "2";
            job.MesCstBody.LOTLIST.Add(new LOTc());
            if (job.MesCstBody.LOTLIST.Count > 0)
            {
                job.MesCstBody.LOTLIST[0].LOTNAME = "2";
            }
            job.GlassChipMaskBlockID = "TC570184AA";
            //job.MesProduct.PRODUCTNAME = "1";
            job.LineRecipeName = "682361479698";
            if (job.MesCstBody.LOTLIST.Count > 0)
                job.MesCstBody.LOTLIST[0].LINERECIPENAME = "263946596768";
            job.MES_PPID = "4";
            job.MesProduct.PPID = "T53345666723";
            job.FromCstID = "FromCstID";
            job.FromSlotNo = "FromSlotNo";
            job.JobJudge = "1";

            job.MesProduct.ABNORMALCODELIST.Add(new CODEc() { ABNORMALCODE = "abnormal123", ABNORMALVALUE = "abnormal456"});

            Random rand = new Random();
            job.ChipCount = rand.Next(0, 255);
            job.JobGrade = "1";
            if (job.MesCstBody.LOTLIST.Count != 0)
            {
                job.MesCstBody.LOTLIST[0].PRODUCTSPECNAME = "prodspecname";
                job.MesCstBody.LOTLIST[0].PROCESSOPERATIONNAME = "processopername";
                job.MesCstBody.LOTLIST[0].PRODUCTOWNER = "prodower";
                job.MesCstBody.LOTLIST[0].CFREWORKCOUNT = "CFcount";
            }
            job.HoldInforList.Add(new HoldInfo());
            job.MesProduct.DUMUSEDCOUNT = "1";
            job.ChamberName = "chamname";
            job.ArraySpecial.ExposureMaskID = "arraymskID";
            job.SourcePortID = "01";

            job.EQPJobID = "02";

            job.VCRJobID = "03";

            job.PPID = "PPID3143141341341";
            job.FromSlotNo = "28";
            job.MesProduct.DENSEBOXID = "02";
            job.OXRInformation = "O";
            job.VCR_Result = eVCR_EVENT_RESULT.NOUSE;
            List<HoldInfo> HoldInforList = new List<HoldInfo>();
            HoldInforList.Add(new HoldInfo());
            job.MesProduct.CFTYPE1REPAIRCOUNT = "1";
            job.MesProduct.CFTYPE2REPAIRCOUNT = "2";
            job.MesProduct.CARBONREPAIRCOUNT = "3";
            job.CfSpecial.AbnormalCode.ALNSIDE = "abc";
            job.CfSpecial.MaskID = "5723634";
            job.MesProduct.ALIGNERNAME = "ALN1";
            job.MesProduct.MACROFLAG = "MCO1";

            job.DefectCodes = new List<DefectCode>();
            DefectCode def = new DefectCode();
            def.ChipPostion = "1";
            def.DefectCodes = ",,,,,,,,,";
            job.DefectCodes.Add(def);

            job.SubProductName = "FC57018400";
            job.MesProduct.SUBPRODUCTSPECNAEM = "8785701841";
            job.MesProduct.SUBPRODUCTPOSITION = "01";
            job.MesProduct.SUBPRODUCTSIZE = "3";

            job.OXRInformation = "OOXX";

            job.MesProduct.REVPROCESSOPERATIONNAME = "CELL";
            job.MesCstBody.LOTLIST[0].PRODUCTOWNER = "CCPPK";

            job.MesProduct.ABNORMALCODELIST = new List<CODEc>();
            CODEc l = new CODEc();
            l.ABNORMALCODE = ",,,,,";
            l.ABNORMALSEQ = "11";
            l.ABNORMALVALUE = "abn";
            job.MesProduct.ABNORMALCODELIST.Add(l);

            return job;

        }

        private Cassette NewCassette()
        {
            Cassette cst = new Cassette();
            //Port port = NewPort();
            cst.MES_CstData.LOTLIST.Add(new LOTc());
            cst.CassetteID = "CAS001";
            cst.CassetteSequenceNo = "1";
            //MES_CstBody MES_CstData = new MES_CstBody();
            //List<LOTc> LOTLIST = new List<LOTc>();
            //LOTLIST.Add(new LOTc());
            for (int i = 0; i < cst.MES_CstData.LOTLIST.Count; i++)
            {
                cst.MES_CstData.LOTLIST[i].LOTNAME = i.ToString();
            }

            cst.LineRecipeName = "879723673489";
            cst.MES_CstData.LINERECIPENAME = "345628435562";
            

            return cst;
        }
        private List<MaterialEntity> listMaterial()
        {
            List<MaterialEntity> lstMTL = new List<MaterialEntity>();
            lstMTL.Add(new MaterialEntity());
            foreach (MaterialEntity material in lstMTL)
            {
                material.eMaterialMode = eMaterialMode.NORMAL;
                material.MaterialID = "mk46873";
                material.MaterialStatus = eMaterialStatus.MOUNT;
                material.MaterialType = "PR";
                material.MaterialValue = "0.1";
                material.GroupId = "4323";
                material.MaterialWeight = "20";
                material.MaterialPosition = "qi";
                material.MaterialCount = "1";
                material.MaterialAbnormalCode = ",,,,,,,,,";
                material.UsedTime = "10";
            }
            //List<uint> alid = new List<uint>();
            //alid.Add((uint)1000);
            //alid.Add((uint)2000);
            //alid.Add((uint)3000);

            return lstMTL;
        }
        private Port NewPort()
        {
            Port port = new Port(new PortEntityData(), new PortEntityFile());
            port.Data.LINEID = lineName;
            port.Data.NODENO = "L2";
            port.Data.PORTID = "01";
            port.File.CassetteID = "TBA810";
            port.Data.NODEID = "TCIMP10B";
            port.Data.PORTATTRIBUTE = "4";
            port.File.Type = ePortType.UnloadingPort;
            port.File.CassetteSetCode = "FULL";
            port.File.EnableMode = ePortEnableMode.Unknown;
            port.File.Status = ePortStatus.LR;
            port.File.OperMode = ePortOperMode.PACK;
            port.File.Mode = ePortMode.ByGrade;
            port.File.TransferMode = ePortTransferMode.AGV;
            port.File.CassetteSequenceNo = "1";

            return port;
            //ObjectManager.CassetteManager.
        }
        private Equipment NewEqp()
        {
            Equipment eqp = new Equipment(new EquipmentEntityData(), new EquipmentEntityFile());
            eqp.Data.LINEID = lineName;
            eqp.Data.NODENO = "L2";
            eqp.File.CurrentRecipeID = "1";
            eqp.Data.NODEID = "TCIMP10B";

            return eqp;
        }
        private Unit NewUnit()
        {
            Unit unit = new Unit(new UnitEntityData(), new UnitEntityFile());
            unit.Data.NODENO = "L2";
            unit.Data.UNITID = "01";

            return unit;
        }
        private Line NewLine()
        {
            Line line = new Line(new LineEntityData(), new LineEntityFile());
            line.Data.LINEID = lineName;
            //line.Data.LINETYPE == eLineType.ARRAY.CHN_SEEC;
            //line.File.IndexOperMode != eINDEXER_OPERATION_MODE.CHANGER_MODE;
            line.File.CFShortCutMode = eShortCutMode.Enable;

            return line;
        }

        #endregion

        #region [ NIKON ]
        private void button1_Click(object sender, EventArgs e)
        {
            InvkeMethod(eServiceName.NikonSECSService, "TS1F1_H_AreYouThereRequest", new object[] { _nikonEqpNo, _nikonEqpId, string.Empty, string.Empty });
        }

        private void button2_Click(object sender, EventArgs e)
        {
            InvkeMethod(eServiceName.NikonSECSService, "TS1F3_H_SelectedEquipmentStatusRequest", new object[] { _nikonEqpNo, _nikonEqpId, (uint)1001, string.Empty, string.Empty });
        }

        private void button3_Click(object sender, EventArgs e)
        {
            InvkeMethod(eServiceName.NikonSECSService, "TS1F3_H_SelectedEquipmentStatusRequest", new object[] { _nikonEqpNo, _nikonEqpId, (uint)1002, string.Empty, string.Empty });
        }

        private void button4_Click(object sender, EventArgs e)
        {
            InvkeMethod(eServiceName.NikonSECSService, "TS1F3_H_SelectedEquipmentStatusRequest", new object[] { _nikonEqpNo, _nikonEqpId, (uint)1003, string.Empty, string.Empty });
        }

        private void button5_Click(object sender, EventArgs e)
        {
            InvkeMethod(eServiceName.NikonSECSService, "TS1F3_H_SelectedEquipmentStatusRequest", new object[] { _nikonEqpNo, _nikonEqpId, (uint)1004, string.Empty, string.Empty });
        }

        private void button6_Click(object sender, EventArgs e)
        {
            InvkeMethod(eServiceName.NikonSECSService, "TS1F3_H_SelectedEquipmentStatusRequest", new object[] { _nikonEqpNo, _nikonEqpId, (uint)1005, string.Empty, string.Empty });
        }

        private void button7_Click(object sender, EventArgs e)
        {
            InvkeMethod(eServiceName.NikonSECSService, "TS1F3_H_SelectedEquipmentStatusRequest", new object[] { _nikonEqpNo, _nikonEqpId, (uint)1006, string.Empty, string.Empty });
        }

        private void button8_Click(object sender, EventArgs e)
        {
            InvkeMethod(eServiceName.NikonSECSService, "TS1F3_H_SelectedEquipmentStatusRequest", new object[] { _nikonEqpNo, _nikonEqpId, (uint)1007, string.Empty, string.Empty });
        }

        private void button9_Click(object sender, EventArgs e)
        {
            InvkeMethod(eServiceName.NikonSECSService, "TS1F3_H_SelectedEquipmentStatusRequest", new object[] { _nikonEqpNo, _nikonEqpId, (uint)1008, string.Empty, string.Empty });
        }

        private void button10_Click(object sender, EventArgs e)
        {
            InvkeMethod(eServiceName.NikonSECSService, "TS1F3_H_SelectedEquipmentStatusRequest", new object[] { _nikonEqpNo, _nikonEqpId, (uint)1009, string.Empty, string.Empty });
        }

        private void button11_Click(object sender, EventArgs e)
        {
            InvkeMethod(eServiceName.NikonSECSService, "TS1F3_H_SelectedEquipmentStatusRequest", new object[] { _nikonEqpNo, _nikonEqpId, (uint)1010, string.Empty, string.Empty });
        }

        private void button12_Click(object sender, EventArgs e)
        {
            InvkeMethod(eServiceName.NikonSECSService, "TS1F3_H_SelectedEquipmentStatusRequest", new object[] { _nikonEqpNo, _nikonEqpId, (uint)1018, string.Empty, string.Empty });
        }

        private void button13_Click(object sender, EventArgs e)
        {
            InvkeMethod(eServiceName.NikonSECSService, "TS1F3_H_SelectedEquipmentStatusRequest", new object[] { _nikonEqpNo, _nikonEqpId, (uint)1019, string.Empty, string.Empty });
        }

        private void button14_Click(object sender, EventArgs e)
        {
            InvkeMethod(eServiceName.NikonSECSService, "TS1F3_H_SelectedEquipmentStatusRequest", new object[] { _nikonEqpNo, _nikonEqpId, (uint)1020, string.Empty, string.Empty });
        }

        private void button15_Click(object sender, EventArgs e)
        {
            InvkeMethod(eServiceName.NikonSECSService, "TS1F3_H_SelectedEquipmentStatusRequest", new object[] { _nikonEqpNo, _nikonEqpId, (uint)1021, string.Empty, string.Empty });
        }

        private void button16_Click(object sender, EventArgs e)
        {
            InvkeMethod(eServiceName.NikonSECSService, "TS1F3_H_SelectedEquipmentStatusRequest", new object[] { _nikonEqpNo, _nikonEqpId, (uint)1022, string.Empty, string.Empty });
        }

        private void button17_Click(object sender, EventArgs e)
        {
            InvkeMethod(eServiceName.NikonSECSService, "TS1F13_H_EstablishCommunicationsRequest", new object[] { _nikonEqpNo, _nikonEqpId, string.Empty, string.Empty });
        }

        private void button18_Click(object sender, EventArgs e)
        {
            InvkeMethod(eServiceName.NikonSECSService, "TS1F15_H_RequestOffLine", new object[] { _nikonEqpNo, _nikonEqpId, string.Empty, string.Empty });
        }

        private void button19_Click(object sender, EventArgs e)
        {
            InvkeMethod(eServiceName.NikonSECSService, "TS1F17_H_RequestOnLine", new object[] { _nikonEqpNo, _nikonEqpId, string.Empty, string.Empty });
        }

        private void button20_Click(object sender, EventArgs e)
        {
            InvkeMethod(eServiceName.NikonSECSService, "TS2F17_H_DateandTimeRequest", new object[] { _nikonEqpNo, _nikonEqpId, string.Empty, string.Empty });
        }

        private void button21_Click(object sender, EventArgs e)
        {
            InvkeMethod(eServiceName.NikonSECSService, "TS2F25_H_LoopbackDiagnosticRequest", new object[] { _nikonEqpNo, _nikonEqpId, string.Empty, string.Empty });
        }

        private void button22_Click(object sender, EventArgs e)
        {
            InvkeMethod(eServiceName.NikonSECSService, "TS2F31_H_DateandTimeSetRequest", new object[] { _nikonEqpNo, _nikonEqpId, string.Empty, string.Empty });
        }

        private void button23_Click(object sender, EventArgs e)
        {
            InvkeMethod(eServiceName.NikonSECSService, "TS10F3_H_TerminalDisplaySingle", new object[5] { _nikonEqpNo, _nikonEqpId, "Message Test", string.Empty, string.Empty });
        }

        private void button24_Click(object sender, EventArgs e)
        {
            InvkeMethod(eServiceName.NikonSECSService, "TS1F3_H_SelectedEquipmentStatusRequest", new object[4] { _nikonEqpNo, _nikonEqpId, (uint)1023, string.Empty });
        }

        private void button25_Click(object sender, EventArgs e)
        {
            InvkeMethod(eServiceName.MESMessageService, "AreYouThereRequest", new object[2] { _nikonEqpNo, _nikonEqpId });
        }

        private void button26_Click(object sender, EventArgs e)
        {
            InvkeMethod(eServiceName.NikonSECSService, "TS2F41_STOP_H_STOPHostCommandSend", new object[3] { _nikonEqpNo, _nikonEqpId, string.Empty });
        }

        private void button27_Click(object sender, EventArgs e)
        {
            InvkeMethod(eServiceName.NikonSECSService, "TS2F41_ABORT_H_ABORTHostCommandSend", new object[3] { _nikonEqpNo, _nikonEqpId, string.Empty });
        }

        private void button28_Click(object sender, EventArgs e)
        {
            InvkeMethod(eServiceName.NikonSECSService, "TS5F3_H_EnableDisableAlarmSend", new object[] { _nikonEqpNo, _nikonEqpId, true, null, string.Empty, string.Empty });
        }

        private void button29_Click(object sender, EventArgs e)
        {
            List<uint> alid = new List<uint>();
            alid.Add((uint)1000);
            alid.Add((uint)2000);
            alid.Add((uint)3000);
            InvkeMethod(eServiceName.NikonSECSService, "TS5F3_H_EnableDisableAlarmSend", new object[] { _nikonEqpNo, _nikonEqpId, true, alid, string.Empty, string.Empty });
        }

        private void button30_Click(object sender, EventArgs e)
        {
            InvkeMethod(eServiceName.NikonSECSService, "TS2F29_H_EquipmentConstantNamelistRequest", new object[] { _nikonEqpNo, _nikonEqpId, (uint)501, string.Empty, string.Empty });
        }

        private void button31_Click(object sender, EventArgs e)
        {
            InvkeMethod(eServiceName.NikonSECSService, "TS2F29_H_EquipmentConstantNamelistRequest", new object[] { _nikonEqpNo, _nikonEqpId, (uint)503, string.Empty, string.Empty });
        }

        private void button32_Click(object sender, EventArgs e)
        {
            InvkeMethod(eServiceName.NikonSECSService, "TS2F13_H_EquipmentConstantRequest", new object[] { _nikonEqpNo, _nikonEqpId, (uint)501, string.Empty, string.Empty });
        }

        private void button33_Click(object sender, EventArgs e)
        {
            InvkeMethod(eServiceName.NikonSECSService, "TS2F13_H_EquipmentConstantRequest", new object[] { _nikonEqpNo, _nikonEqpId, (uint)503, string.Empty, string.Empty });
        }

        private void button34_Click(object sender, EventArgs e)
        {
            InvkeMethod(eServiceName.NikonSECSService, "TS6F15_H_EventReportRequest", new object[] { _nikonEqpNo, _nikonEqpId, (uint)1, string.Empty, string.Empty });
        }

        private void button35_Click(object sender, EventArgs e)
        {
            InvkeMethod(eServiceName.NikonSECSService, "TS6F15_H_EventReportRequest", new object[] { _nikonEqpNo, _nikonEqpId, (uint)101, string.Empty, string.Empty });
        }

        private void button36_Click(object sender, EventArgs e)
        {
            InvkeMethod(eServiceName.NikonSECSService, "TS6F19_H_IndividualReportRequest", new object[] { _nikonEqpNo, _nikonEqpId, (uint)1001, string.Empty, string.Empty });
        }
		#endregion

		#region [ CSOT ]
		private void btnCSOTSECS_Click(object sender, EventArgs e) 
		{
			switch (txtSxFy.Text.Trim().ToUpper()) 
			{
				case "S1F1":
					InvkeMethod(eServiceName.CSOTSECSService, "TS1F1_H_AreYouThereRequest", 
						new object[] {_csotEqpNo,_csotEqpId,string.Empty,string.Empty});
					break;
				case "S1F5_01":
					InvkeMethod(eServiceName.CSOTSECSService, "TS1F5_H_FormattedStatusRequest", 
						new object[] {_csotEqpNo,_csotEqpId,"01",string.Empty, string.Empty});
					break;
				case "S1F5_02":
					InvkeMethod(eServiceName.CSOTSECSService, "TS1F5_H_FormattedStatusRequest",
						new object[] { _csotEqpNo, _csotEqpId, "02", string.Empty, string.Empty });
					break;
				case "S1F5_03":
					InvkeMethod(eServiceName.CSOTSECSService, "TS1F5_H_FormattedStatusRequest",
						new object[] { _csotEqpNo, _csotEqpId, "03", string.Empty, string.Empty });
					break;
				case "S1F5_04":
					InvkeMethod(eServiceName.CSOTSECSService, "TS1F5_H_FormattedStatusRequest",
						new object[] { _csotEqpNo, _csotEqpId, "04", string.Empty, string.Empty });
					break;
				case "S1F5_05":
					InvkeMethod(eServiceName.CSOTSECSService, "TS1F5_H_FormattedStatusRequest",
						new object[] { _csotEqpNo, _csotEqpId, "05", string.Empty, string.Empty });
					break;
				case "S1F5_06":
					InvkeMethod(eServiceName.CSOTSECSService, "TS1F5_H_FormattedStatusRequest",
						new object[] { _csotEqpNo, _csotEqpId, "06", string.Empty, string.Empty });
					break;
				case "S1F5_07":
					InvkeMethod(eServiceName.CSOTSECSService, "TS1F5_H_FormattedStatusRequest",
						new object[] { _csotEqpNo, _csotEqpId, "07", string.Empty, string.Empty });
					break;
				case "S1F5_08":
					InvkeMethod(eServiceName.CSOTSECSService, "TS1F5_H_FormattedStatusRequest",
						new object[] { _csotEqpNo, _csotEqpId, "08", string.Empty, string.Empty });
					break;
				case "S1F5_09":
					InvkeMethod(eServiceName.CSOTSECSService, "TS1F5_H_FormattedStatusRequest",
						new object[] { _csotEqpNo, _csotEqpId, "09", string.Empty, string.Empty });
					break;
				case "S1F5_10":
					InvkeMethod(eServiceName.CSOTSECSService, "TS1F5_H_FormattedStatusRequest",
						new object[] { _csotEqpNo, _csotEqpId, "10", string.Empty, string.Empty });
					break;				
				case "S1F13":
					InvkeMethod(eServiceName.CSOTSECSService, "TS1F13_H_EstablishCommunicationsRequest",
						new object[] { _csotEqpNo, _csotEqpId, string.Empty, string.Empty });
					break;				
				case "S1F15":
					InvkeMethod(eServiceName.CSOTSECSService, "TS1F15_H_OfflineRequest",
						new object[] { _csotEqpNo, _csotEqpId, string.Empty, string.Empty });
					break;				
				case "S1F17":
					InvkeMethod(eServiceName.CSOTSECSService, "TS1F17_H_OnlineRequest",
						new object[] { _csotEqpNo, _csotEqpId, string.Empty, string.Empty });
					break;				
				case "S2F15":
					InvkeMethod(eServiceName.CSOTSECSService, "TS2F15_H_NewEquipmentConstantSend",
					    new object[] { _csotEqpNo, _csotEqpId,
							new List<Tuple<string,string>>() {Tuple.Create("1","EV01"),Tuple.Create("2","EV02")}, string.Empty, string.Empty });
					break;
				case "S2F19":
					InvkeMethod(eServiceName.CSOTSECSService, "TS2F19_H_DataSetCommand",
					    new object[] { _csotEqpNo, _csotEqpId,
							new List<Tuple<string,string,string,string,List<string>>>() 
							{
								Tuple.Create("S1F5",
								             "02",
											 "0",
											 "4000",
								             new List<string>() {"DATANAME1","DATANAME2"}
								),
								Tuple.Create("S1F5",
								             "04",
											 "0",
											 "4000",
								             new List<string>() {"DATANAME3","DATANAME4"}
								),
							},
							string.Empty, string.Empty });
					break;
				case "S2F21":
					InvkeMethod(eServiceName.CSOTSECSService, "TS2F21_H_DataSetCommandforID",
					    new object[] { _csotEqpNo, _csotEqpId,
							new List<Tuple<string,string,string,string,List<string>>>() 
							{
								Tuple.Create("S6F3",
								             "09",
											 "0",
											 "4000",
								             new List<string>() {"DATID1","DATID2"}
								),
								Tuple.Create("S6F3",
								             "10",
											 "0",
											 "4000",
								             new List<string>() {"DATID3","DATID4"}
								),
							},
							string.Empty, string.Empty });
					break;
				case "S2F23":
					InvkeMethod(eServiceName.CSOTSECSService, "TS2F23_H_DataItemMappingTableRequest",
						new object[] { _csotEqpNo, _csotEqpId, string.Empty, string.Empty });
					break;				
				case "S2F25":
					InvkeMethod(eServiceName.CSOTSECSService, "TS2F25_H_LoopbackDiagnosticRequest",
						new object[] { _csotEqpNo, _csotEqpId, string.Empty, string.Empty });
					break;				
				case "S2F29":
					InvkeMethod(eServiceName.CSOTSECSService, "TS2F29_H_EquipmentConstantNamlistRequest",
						new object[] { _csotEqpNo, _csotEqpId,new List<string>() {"1","2"}, string.Empty, string.Empty });
					break;				
				case "S2F31":
					InvkeMethod(eServiceName.CSOTSECSService, "TS2F31_H_DateandTimeSetRequest",
						new object[] { _csotEqpNo, _csotEqpId, string.Empty, string.Empty });
					break;				
				case "S2F41":
					InvkeMethod(eServiceName.CSOTSECSService, "TS2F41_H_HostCommandSend",
						new object[] { _csotEqpNo, _csotEqpId, "PAUSE", 
							new List<Tuple<string,string>>() { 
								Tuple.Create("CPNAME1", "CPVAL1"),
								Tuple.Create("CPNAME2","CPVAL2") 
							}, string.Empty });
					break;														
				case "S2F103":
					InvkeMethod(eServiceName.CSOTSECSService, "TS2F103_H_LotStartInformSend",
						new object[] { _csotEqpNo, _csotEqpId, "LOTID01", "GLASSID01", "PPID01", string.Empty });
					break;				
				case "S2F105":
					InvkeMethod(eServiceName.CSOTSECSService, "TS2F105_H_LotEndInformSend",
						new object[] { _csotEqpNo, _csotEqpId,"5","1","GLASSID01","PPID01", string.Empty });
					break;				
				case "S2F111":
					InvkeMethod(eServiceName.CSOTSECSService, "TS2F111_H_ForcedCleanOutCommandSend",
						new object[] { _csotEqpNo, _csotEqpId, _csotEqpId, "1", string.Empty });
					break;
				case "S2F117":
					InvkeMethod(eServiceName.CSOTSECSService, "TS2F117_H_GlassEraseRecoveryInformationSend",
						new object[] { _csotEqpNo, _csotEqpId,"5","1","GLASSID01","0","1", string.Empty });
					break;
				case "S2F119":
					InvkeMethod(eServiceName.CSOTSECSService, "TS2F119_H_EquipmentModeChangeCommandSend",
						new object[] { _csotEqpNo, _csotEqpId,_csotEqpId,"NORN", string.Empty, string.Empty });
					break;
				case "S2F121":
					InvkeMethod(eServiceName.CSOTSECSService, "TS2F121_H_APCDataDownloadCommandSend",
						new object[] { _csotEqpNo, _csotEqpId,new List<Tuple<string,string>>() { 
								Tuple.Create("APCNAME1", "APCVAL1"),
								Tuple.Create("APCNAME2","APCVAL2") 
							}, string.Empty });
					break;
				case "S2F123":
					InvkeMethod(eServiceName.CSOTSECSService, "TS2F123_H_SetGlassRecipeGroupEndFlagSend",
						new object[] { _csotEqpNo, _csotEqpId,"5","1","GLASSID01","1", string.Empty });
					break;
				case "S5F5":
					InvkeMethod(eServiceName.CSOTSECSService, "TS5F5_H_ListAlarmRequest",
						new object[] { _csotEqpNo, _csotEqpId, string.Empty });
					break;
				case "S7F19":
					InvkeMethod(eServiceName.CSOTSECSService, "TS7F19_H_CurrentEPPDRequest",
						new object[] { _csotEqpNo, _csotEqpId, string.Empty });
					break;
				case "S7F23":
					break;
				case "S7F25":
					InvkeMethod(eServiceName.CSOTSECSService, "TS7F25_H_FormattedProcessProgramRequest",
						new object[] { new RecipeCheckInfo(_csotEqpNo, 1, 1, "PPID01"),string.Empty  }
						);
					break;
				case "S7F27":
					break;
				case "S7F73":
					InvkeMethod(eServiceName.CSOTSECSService, "TS7F73_H_RecipeIDCheck",
						new object[] { new RecipeCheckInfo(_csotEqpNo, 1, 1, "PPID01"),string.Empty  }
						);
					break;
				case "S10F3":
                    InvkeMethod(eServiceName.CSOTSECSService, "TS10F3_H_TerminalDisplaySingle",
                        new object[] { _csotEqpNo, _csotEqpId, "123", string.Empty, string.Empty }
                        );
					break;
				case "S10F5":
					InvkeMethod(eServiceName.CSOTSECSService, "TS10F5_H_TerminalDisplaySingleforDNSLC",
						new object[] { _csotEqpNo, _csotEqpId,"01 02 03 04 0A 0B 0C 0D 0E 0F", string.Empty });
					break;
				case "S64F1":
					InvkeMethod(eServiceName.CSOTSECSService, "TS64F1_H_MeasurementResultSend",
					    new object[] { _csotEqpNo, _csotEqpId,"PPID01",
							new List<Tuple<string,string,List<Tuple<string,string,string,string,string>>>>() 
							{
								Tuple.Create("GLASSID01",
								             "20141002010201",
								             new List<Tuple<string,string,string,string,string>>() {
									                Tuple.Create("1","11","12","13","14"),
									                Tuple.Create("2","21","22","23","24")
											 }
								),
								Tuple.Create("GLASSID02",
								             "20141002010202",
											 new List<Tuple<string,string,string,string,string>>() {
									                Tuple.Create("3","31","32","33","34"),
									                Tuple.Create("4","41","42","43","44")
											 }
								)
							},
							string.Empty });
					break;
			}
		}
		#endregion

        private void button37_Click(object sender, EventArgs e)
        {
            TEST_CREATE_JOBS();
        }

        private void TEST_CREATE_JOBS()
        {
            try
            {

                Random rand = new Random();


                int j = ObjectManager.JobManager.GetJobCount();

                for (int i = 0; i <= 10; i++)
                {
                    Job job = new Job(5, i + 1);
                    //ObjectManager.JobManager.NewJob(job.CassetteSequenceNo, job.JobSequenceNo);
                    //

                    if (job == null)
                        continue;
                    job.GroupIndex = rand.Next(1, 65535).ToString();
                    job.ProductType.Value = rand.Next(1, 65535);
                    job.CSTOperationMode = (eCSTOperationMode)int.Parse(rand.Next(0, 1).ToString());
                    job.SubstrateType = (eSubstrateType)int.Parse(rand.Next(0, 3).ToString());
                    job.CIMMode = (eBitResult)int.Parse(rand.Next(0, 1).ToString());
                    job.JobType = (eJobType)int.Parse(rand.Next(1, 6).ToString());
                    job.JobJudge = rand.Next(0, 8).ToString();
                    job.SamplingSlotFlag = rand.Next(0, 1).ToString();
                    job.OXRInformationRequestFlag = rand.Next(0, 1).ToString();
                    //job. = eVent.Items[11].Value.ToString(); //Reserve
                    job.FirstRunFlag = rand.Next(0, 1).ToString();
                    job.JobGrade = rand.Next(0, 1).ToString();
                    job.GlassChipMaskBlockID = "GLASS" + (i + 1).ToString("00000000");//rand.Next(11111111, 99999999).ToString();
                    job.PPID = "PPID3143141341341";
                    //job.INSPReservations = rand.Next(0, 768).ToString();
                    //job.EQPReservations = rand.Next(0, 235).ToString();
                    job.LastGlassFlag = rand.Next(0, 1).ToString();
                    //job. = eVent.Items[19].Value.ToString();//Reserve
                    if (job.CfSpecial == null)
                        job.CfSpecial = new JobCfSpecial();
                    job.InspJudgedData = rand.Next(0, 1).ToString();
                    job.CFSpecialReserved = rand.Next(0, 1).ToString();
                    job.TrackingData = rand.Next(0, 1).ToString();
                    job.CFSpecialReserved = rand.Next(0, 1).ToString();
                    //job.EQPFlag = rand.Next(0, 1).ToString();
                    job.OXRInformation = rand.Next(0, 4).ToString();
                    job.ChipCount = rand.Next(0, 255);
                    //job.File = eVent.Items[27].Value.ToString();//Reserve
                    job.CfSpecial.COAversion = "AB";
                    job.CfSpecial.DummyUsedCount = rand.Next(0, 5).ToString();
                    job.MesCstBody.CARRIERNAME = "Carrier515151";
                    job.MesCstBody.LOTLIST.Add(new LOTc() { LOTNAME = "LOT123" });
                    job.FromCstID = job.MesCstBody.CARRIERNAME;

                    ObjectManager.JobManager.AddJob(job);
                    ObjectManager.JobManager.EnqueueSave(job);
                    //System.Threading.Thread.Sleep(300);
                }
                int w = ObjectManager.JobManager.GetJobs().Count;
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
            }
        }

        //Add by marine start 2015/7/6
        #region  MES TEST

        private void AreYouThereRequest(object sender, EventArgs e)
        {
            InvkeMethod(eServiceName.MESService, "AreYouThereRequest", new object[2] { trxID, lineName });
        }

        private void LotProcessStartRequest(object sender, EventArgs e)
        {
            Port port = NewPort();
            Cassette cst = NewCassette();
            Job job = NewJob();
            InvkeMethod(eServiceName.MESService, "LotProcessStartRequest", new object[4] { trxID, port, cst,job });
        }

        private void LotProcessStarted(object sender, EventArgs e)
        {
            Port port = NewPort();
            Cassette cst = NewCassette();
            InvkeMethod(eServiceName.MESService, "LotProcessStarted", new object[3] { trxID, port, cst });
        }

        private void MaterialMount(object sender, EventArgs e)
        {
            Equipment eqp = NewEqp();
            //string materialmode = "2";
            string glassID = "3";
            string materialdurablename = "4";
            string poltype = "5";
            List<MaterialEntity> materialList = listMaterial();

            InvkeMethod(eServiceName.MESService, "MaterialMount", new object[6] { trxID, eqp, glassID, materialdurablename, poltype, materialList });
        }

        private void MaterialDismountReport(object sender, EventArgs e)
        {
            Equipment eqp = NewEqp();
            string glassID = "3";
            string materialdurablename = "4";
            List<MaterialEntity> materialList = listMaterial();

            InvkeMethod(eServiceName.MESService, "MaterialDismountReport", new object[5] { trxID, eqp, glassID, materialdurablename, materialList });
        }

        private void MachineDataUpdate(object sender, EventArgs e)
        {
            //LineEntityData l = new LineEntityData();
            //l.LINENAME = lineName;
            Line line = new Line(new LineEntityData(), new LineEntityFile());
            line.Data.LINEID = lineName;
            line.File.CurrentPlanID = "199";
            //line.File.LineOperMode.ToUpper() = "";
            //line.File.Status.ToString() = "";

            InvkeMethod(eServiceName.MESService, "MachineDataUpdate", new object[2] { trxID, line });
        }

        private void ProductLineIn(object sender, EventArgs e)
        {
            Job job = NewJob();
            string currentEQPID = "TCIMP10B";
            string portID = "3";
            string unitID = "4";

            InvkeMethod(eServiceName.MESService, "ProductLineIn", new object[6] { trxID, lineName, job, currentEQPID, portID, unitID });
        }

        private void ProductLineOut(object sender, EventArgs e)
        {
            Job job = NewJob();
            job.ChamberName = "";
            string currentEQPID = "TCIMP10B";
            string portID = "3";
            string unitID = "4";

            InvkeMethod(eServiceName.MESService, "ProductLineOut", new object[6] { trxID, lineName, job, currentEQPID, portID, unitID });
        }

        private void ProductIn(object sender, EventArgs e)
        {
            Job job = NewJob();
            string processtime = "7";
            string currentEQPID = "TCIMP10B";
            string portID = "3";
            string unitID = "4";
            eMESTraceLevel traceLvl = new eMESTraceLevel();

            InvkeMethod(eServiceName.MESService, "ProductIn", new object[8] { trxID, lineName, job, currentEQPID, portID, unitID, traceLvl, processtime });
        }

        private void ChangePlanAborted(object sender, EventArgs e)
        {
            string curPlanName = "changer1";
            string reasonCode = ",,,,,,,";
            string description = "abort";
            string carrierName = "CAS001";

            InvkeMethod(eServiceName.MESService, "ChangePlanAborted", new object[6] { trxID, lineName, curPlanName, reasonCode, description, carrierName });
        }

        private void ChangePlanCanceled(object sender, EventArgs e)
        {
            string curPlanName = "changer2";
            string reasonCode = ",,,,,,,";
            string description = "cancel";
            string carrierName = "CAS001";

            InvkeMethod(eServiceName.MESService, "ChangePlanCanceled", new object[6] { trxID, lineName, curPlanName, reasonCode, description, carrierName });
        }

        private void CassetteCleanEnd(object sender, EventArgs e)
        {
            string portID = "daiding";
            string cSTID = "daiding";
            string reasonCode = "daiding";
            InvkeMethod(eServiceName.MESService, "CassetteCleanEnd", new object[5] { trxID, lineName, portID, cSTID, reasonCode });
        }

        private void ChamberRunModeChanged(object sender, EventArgs e)
        {
            string lineOperMode = "CHANGER";
            string machineName = "TCIMP10B";
            List<ChamberRunModeChanged.CHAMBERc> chamberList = new List<ChamberRunModeChanged.CHAMBERc>();
            chamberList.Add(new ChamberRunModeChanged.CHAMBERc());
            foreach (ChamberRunModeChanged.CHAMBERc chamber in chamberList)
            {
                chamber.CHAMBERNAME = "unit";
                chamber.CHAMBERRUNMODE = "chambermode";
            }

            InvkeMethod(eServiceName.MESService, "ChamberRunModeChanged", new object[5] { trxID, lineName, lineOperMode, machineName, chamberList });
        }

        private void ValidateCleanCassetteRequest(object sender, EventArgs e)
        {
            Port port = NewPort();
            InvkeMethod(eServiceName.MESService, "ValidateCleanCassetteRequest", new object[2] { trxID, port });
        }

        private void ProductScrapped(object sender, EventArgs e)
        {
            Equipment eqp = NewEqp();
            Job job = NewJob();
            string removeReasonCode = "1";
            InvkeMethod(eServiceName.MESService, "ProductScrapped", new object[5] { trxID, lineName, eqp, job, removeReasonCode });
        }

        private void MaterialConsumableReport(object sender, EventArgs e)
        {
            Equipment eqp = NewEqp();
            string glassID = "1";
            string materialDurableName = "2";
            IList<MaterialEntity> materilst = listMaterial();
            InvkeMethod(eServiceName.MESService, "MaterialConsumableRequest", new object[6] { trxID, lineName, eqp, glassID, materialDurableName, materilst });
        }

        private void LotProcessEnd(object sender, EventArgs e)
        {
            Port port = NewPort();
            Cassette cst = NewCassette();
            IList<Job> jobs = new List<Job>();
            jobs.Add(NewJob());
            InvkeMethod(eServiceName.MESService, "LotProcessEnd", new object[4] { trxID, port, cst, jobs });
        }

        private void LotProcessAbnormalEnd(object sender, EventArgs e)
        {
            Port port = NewPort();
            Cassette cst = NewCassette();
            IList<Job> jobs = new List<Job>();
            jobs.Add(NewJob());
            InvkeMethod(eServiceName.MESService, "LotProcessAbnormalEnd", new object[4] { trxID, port, cst, jobs });
        }

        private void AlarmReport(object sender, EventArgs e)
        {
            InvkeMethod(eServiceName.MESService, "AlarmReport", new object[8] { trxID, lineName, "TCIMP10B", "TCIMP10B", "1", "2", "3", "4" });
        }

        private void CassetteOperModeChanged(object sender, EventArgs e)
        {
            string CstOperMode = "---CstOperMode---";
            InvkeMethod(eServiceName.MESService, "CassetteOperModeChanged", new object[3] { trxID, lineName, CstOperMode });

        }

        private void ChangeMaterialLifeReport(object sender, EventArgs e)
        {
            string machineName = "TCIMP100---MachineName";
            string productName = "TCIMP100---ProductName";
            IList<ChangeMaterialLifeReport.MATERIALc> materialList = new List<ChangeMaterialLifeReport.MATERIALc>();

            materialList.Add(new ChangeMaterialLifeReport.MATERIALc());
            foreach (ChangeMaterialLifeReport.MATERIALc chamber in materialList)
            {
                chamber.CHAMBERID = "ChangeMaterialLifeReport ID";
                chamber.MATERIALNAME = "Materialname";
                chamber.MATERIALTYPE = "TYPE-123";
                chamber.QUANTITY = "9,999";
            }
            InvkeMethod(eServiceName.MESService, "ChangeMaterialLife", new object[5] { trxID, lineName, machineName, productName, materialList });

        }

        private void ChangePlanRequest(object sender, EventArgs e)
        {
            string cSTID = "CassetteID---999";
            string curPlanName = "curPlanName - Plannnn";
            InvkeMethod(eServiceName.MESService, "ChangePlanRequest", new object[4] { trxID, lineName, cSTID, curPlanName });

        }

        private void ChangePlanStarted(object sender, EventArgs e)
        {
            string planName = "TFTCHANGER";
            InvkeMethod(eServiceName.MESService, "ChangePlanStarted", new object[3] { trxID, lineName, planName });
        }

        private void ChangePVDMaterialLife(object sender, EventArgs e)
        {
            string machineName = "MachineName-TCIMP100";
            string productName = "ProductName-----";
            IList<ChangePVDMaterialLife.CHAMBERc> chamberList = new List<ChangePVDMaterialLife.CHAMBERc>();
            chamberList.Add(new ChangePVDMaterialLife.CHAMBERc());
            foreach (ChangePVDMaterialLife.CHAMBERc chamber in chamberList)
            {
                chamber.CHAMBERID = "101";
                chamber.MATERIALTYPE = "TYPE-chambermode";
                chamber.QUANTITY = "99,999";
            }
            InvkeMethod(eServiceName.MESService, "ChangePVDMaterialLife", new object[5] { trxID, lineName, machineName, productName, chamberList });
        }

        private void CheckLocalPPIDRequest(object sender, EventArgs e)
        {
            string localrecipe = "LocalRecipe100";
            IList<Equipment> nodelist = new List<Equipment>();
            nodelist.Add(NewEqp());
            InvkeMethod(eServiceName.MESService, "CheckLocalPPIDRequest", new object[4] { trxID, lineName, localrecipe, nodelist });
        }

        private void FacilityCheckReply(object sender, EventArgs e)
        {
            IList<FacilityCheckReply.MACHINEc> machineList = new List<FacilityCheckReply.MACHINEc>();
            machineList.Add(new FacilityCheckReply.MACHINEc());
            foreach (FacilityCheckReply.MACHINEc chamber in machineList)
            {
                //chamber.FACILITYPARALIST = ;
                chamber.MACHINENAME = "TCIMP10B";
                chamber.MACHINESTATENAME = "RUN";
                List<FacilityCheckReply.PARAc> l = new List<FacilityCheckReply.PARAc>();
                l.Add(new FacilityCheckReply.PARAc());
                chamber.FACILITYPARALIST = l;
            }
            InvkeMethod(eServiceName.MESService, "FacilityCheckReply", new object[3] { trxID, lineName, machineList });
        }

        private void GlassProcessStarted(object sender, EventArgs e)
        {
            Job job = NewJob();
            InvkeMethod(eServiceName.MESService, "GlassProcessStarted", new object[3] { trxID, lineName, job });
        }

        private void GlassProcessLineChanged(object sender, EventArgs e)
        {
            string crosslineName = "---CrossLineName---";
            Job job = NewJob();
            InvkeMethod(eServiceName.MESService, "GlassProcessLineChanged", new object[4] { trxID, lineName, crosslineName, job });
        }

        private void IndexerOperModeChanged(object sender, EventArgs e)
        {
            string indexOPerMode = "OperModeChange~~yoyo";
            InvkeMethod(eServiceName.MESService, "IndexerOperModeChanged", new object[3] { trxID, lineName, indexOPerMode });
        }

        private void InspectionModeChanged(object sender, EventArgs e)
        {
            string inspMode = "---InspectionMode---";
            string eqpID = "eqpID=TICMP100";
            string pullmodeGrade = "PULLLLLLLLLLLLLLLLLL";
            string waittime = "99,999s";
            string samplerate = "Sample~~~";
            string reasoncode = "there's no reason.";
            InvkeMethod(eServiceName.MESService, "InspectionModeChanged", new object[8] { trxID, lineName, inspMode, eqpID, pullmodeGrade, waittime, samplerate, reasoncode });
        }

        private void MachineControlStateChanged(object sender, EventArgs e)
        {
            string controlStatus = "ControlStatus - OFF";
            InvkeMethod(eServiceName.MESService, "MachineControlStateChanged", new object[3] { trxID, lineName, controlStatus });
        }

        private void LotProcessAborted(object sender, EventArgs e)
        {
            Port port = NewPort();
            string reasonCode = "ReasonCode RRRR";
            string reasonTxt = "Reason,because.123456";
            InvkeMethod(eServiceName.MESService, "LotProcessAborted", new object[4] { trxID, port, reasonCode, reasonTxt });
        }

        private void LotProcessCanceled(object sender, EventArgs e)
        {
            Port port = NewPort();
            string reasonCode = "RRRRRRR, ReasonCOde";
            string reasonTxt = "reasonTXTTTTTTTTTTTT";
            InvkeMethod(eServiceName.MESService, "LotProcessCanceled", new object[4] { trxID, port, reasonCode, reasonTxt });
        }

        private void MachineStateChanged(object sender, EventArgs e)
        {
            Equipment eqp = NewEqp();
            string alarmID = "ALARMID-111";
            string alarmtext = "AlARMTEXT---";
            string alarmTime = "10,000s";
            InvkeMethod(eServiceName.MESService, "MachineStateChanged", new object[5] { trxID, eqp, alarmID, alarmtext, alarmTime });
        }

        private void MaskProcessEnd(object sender, EventArgs e)
        {
            Port port = NewPort();
            IList<Job> jobList = new List<Job>();
            jobList.Add(NewJob());
            string reasoncode = "ReasonCode";
            string reasontxt = "ReasonTEXT";
            InvkeMethod(eServiceName.MESService, "MaskProcessEndAbort", new object[5] { trxID, port, jobList, reasoncode, reasontxt });
        }

        private void MaskStateChanged(object sender, EventArgs e)
        {
            string machineName = "TCIMP10B";
            string machineRecipeName = "Receipe Name";
            string eventUse = "Event Trigger";
            IList<MaskStateChanged.MASKc> maskList = new List<MaskStateChanged.MASKc>();
            maskList.Add(new MaskStateChanged.MASKc());
            foreach (MaskStateChanged.MASKc chamber in maskList)
            {
                chamber.CLEANRESULT = "CleanResult - Idle";
                chamber.HEADID = "HEADID~~~123";
                chamber.MASKNAME = "MASK MASK";
                chamber.MASKPOSITION = "top";
                chamber.MASKSTATE = "DISMOUNT";
                chamber.MASKUSECOUNT = "count 9";
                chamber.MASKUSECOUNTint = 999;
                chamber.REASONCODE = "No Reason";
                chamber.UNITNAME = "TCIMP";
            }
            string requestKey = "KEYYYYYYYYY";
            InvkeMethod(eServiceName.MESService, "MaskStateChanged", new object[7] { trxID, lineName, machineName, machineRecipeName, eventUse, maskList, requestKey });
        }

        private void MaskLocationChanged(object sender, EventArgs e)
        {
            string eQPID = "TCIMP10B";
            string maskState = "WAIT";
            string maskid = "T3151B01-11A0006";
            string maskAction = "IN";
            string maskPosition = "101";
            string user = "marine";
            InvkeMethod(eServiceName.MESService, "MaskLocationChanged", new object[8] { trxID, lineName, eQPID, maskState, maskid, maskAction, maskPosition, user });
        }

        private void MaskUsedCountReport(object sender, EventArgs e)
        {
            string machineName = "TCIMP";
            IList<MaskUsedCountReport.MASKc> maskList = new List<MaskUsedCountReport.MASKc>();
            maskList.Add(new MaskUsedCountReport.MASKc());
            foreach (MaskUsedCountReport.MASKc chamber in maskList)
            {
                chamber.MASKNAME = "MASK";
                chamber.MASKUSECOUNT = "count 999+";
                chamber.MASKUSECOUNTint = 3;
            }
            InvkeMethod(eServiceName.MESService, "MaskUsedCountReport", new object[4] { trxID, lineName, machineName, maskList });
        }

        private void MachineControlStateChangeReply(object sender, EventArgs e)
        {
            string ctlStatus = "ControlStatus = ON";
            string ackResult = "Result  ttt";
            string inboxname = "inbox bobobobo";
            InvkeMethod(eServiceName.MESService, "MachineControlStateChangeReply", new object[5] { trxID, lineName, ctlStatus, ackResult, inboxname });
        }

        private void MachineInspectionOverRatio(object sender, EventArgs e)
        {
            string machineName = "TCIMP10B";
            string overResult = "A";
            InvkeMethod(eServiceName.MESService, "MachineInspectionOverRatio", new object[4] { trxID, lineName, machineName, overResult });
        }

        private void MachineLoginReport(object sender, EventArgs e)
        {
            string machineName = "TCIMP10B";
            string loginUserID = "Uni-999";
            InvkeMethod(eServiceName.MESService, "MachineLoginReport", new object[4] { trxID, lineName, machineName, loginUserID });
        }

        private void PortAccessModeChanged(object sender, EventArgs e)
        {
            IList<Port> portlist = new List<Port>();
            portlist.Add(NewPort());
            InvkeMethod(eServiceName.MESService, "PortAccessModeChanged", new object[3] { trxID, lineName, portlist });
        }

        private void PortCarrierSetCodeChanged(object sender, EventArgs e)
        {
            IList<Port> portList = new List<Port>();
            portList.Add(NewPort());
            InvkeMethod(eServiceName.MESService, "PortCarrierSetCodeChanged", new object[3] { trxID, lineName, portList });
        }

        private void PortOperModeChanged(object sender, EventArgs e)
        {
            IList<Port> portlist = new List<Port>();
            portlist.Add(NewPort());
            InvkeMethod(eServiceName.MESService, "PortOperModeChanged", new object[3] { trxID, lineName, portlist });
        }

        private void PortOperModeChangeRequest(object sender, EventArgs e)
        {
            IList<Port> portlist = new List<Port>();
            portlist.Add(NewPort());
            InvkeMethod(eServiceName.MESService, "PortOperModeChangeRequest", new object[3] { trxID, lineName, portlist });
        }

        private void PortTransferStateChanged(object sender, EventArgs e)
        {
            //IList<Port> portlist = new List<Port>();
            //portlist.Add(NewPort());
            Port port = NewPort();
            InvkeMethod(eServiceName.MESService, "PortTransferStateChanged", new object[3] { trxID, lineName, port });
        }

        private void PortTypeChanged(object sender, EventArgs e)
        {
            IList<Port> portlist = new List<Port>();
            portlist.Add(NewPort());
            //Port port = NewPort();
            InvkeMethod(eServiceName.MESService, "PortTypeChanged", new object[3] { trxID, lineName, portlist });
        }

        private void ProductOut(object sender, EventArgs e)
        {
            Job job = NewJob();
            string currentEQPID = "TCIMP10B";
            string portid = "3";
            string unitID = "4";
            eMESTraceLevel traceLvl = new eMESTraceLevel();
            InvkeMethod(eServiceName.MESService, "ProductOut", new object[7] { trxID, lineName, job, currentEQPID, portid, unitID, traceLvl });
        }

        private void ProductProcessData(object sender, EventArgs e)
        {
            string machineName = "TCIMP10B";
            string unitName = "TCIMP-Unit";
            string lotName = "LotLotLot";
            string carrierName = "01";
            string productName = "TFT32";
            string productSpecName = "SpecName~";
            string productSpecVer = "Version";
            string processOperationName = "OperationName";
            string lineRecipeName = "123454656532";
            IList<ProductProcessData.ITEMc> itemList = new List<ProductProcessData.ITEMc>();
            itemList.Add(new ProductProcessData.ITEMc());
            foreach (ProductProcessData.ITEMc chamber in itemList)
            {
                chamber.ITEMNAME = "ItemNameNCAA";
                //chamber.SITELIST = null;
            }
            InvkeMethod(eServiceName.MESService, "ProductProcessData", new object[12] { trxID, lineName, machineName, unitName, lotName, carrierName, productName, productSpecName, productSpecVer, processOperationName, lineRecipeName, itemList });
        }

        private void ProductUnscrapped(object sender, EventArgs e)
        {
            Equipment eqp = NewEqp();
            Job job = NewJob();
            string recoveReasonCode = "RecoverReason";
            InvkeMethod(eServiceName.MESService, "ProductUnscrapped", new object[5] { trxID, lineName, eqp, job, recoveReasonCode });
        }

        private void RecipeParameterChangeRequest(object sender, EventArgs e)
        {
            string machienName = "TCIMP10B";
            string recipeID = "23";
            IDictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("1", "2");
            InvkeMethod(eServiceName.MESService, "RecipeParameterChangeRequest", new object[5] { trxID, lineName, machienName, recipeID, parameters });
        }

        private void ValidateCassetteRequest(object sender, EventArgs e)
        {
            Port port = NewPort();
            Cassette cst = NewCassette();
            ObjectManager.CassetteManager.EnqueueSave(cst);

            //Dictionary<string, Cassette> _entities = new Dictionary<string, Cassette>();
            //_entities.Add(port.File.CassetteSequenceNo,cst);
            InvkeMethod(eServiceName.MESService, "ValidateCassetteRequest", new object[2] { trxID, port });
        }

        private void VCRReadReport(object sender, EventArgs e)
        {
            string eqpID = "TCIMP310";
            Job job = NewJob();
            eVCR_EVENT_RESULT vcrreadflag = eVCR_EVENT_RESULT.READING_OK_MATCH_JOB;
            InvkeMethod(eServiceName.MESService, "VCRReadReport", new object[5] { trxID, lineName, eqpID, job, vcrreadflag });
        }

        private void VCRStateChanged(object sender, EventArgs e)
        {
            string machineName = "TCIMP310";
            string vcrName = "01V";
            string vcrStateName = "OK";
            InvkeMethod(eServiceName.MESService, "VCRStateChanged", new object[5] { trxID, lineName, machineName, vcrName, vcrStateName });
        }

        private void ValidateMaskPrepareRequest(object sender, EventArgs e)
        {
            string eqpID = "TCIMP100";
            string maskid = "MaskID100";
            string jobid = "JobID100";
            InvkeMethod(eServiceName.MESService, "ValidateMaskPrepareRequest", new object[5] { trxID, lineName, eqpID, maskid, jobid });
        }

        private void ValidateMaskRequest(object sender, EventArgs e)
        {
            string eqpID = "TCIMP10B";
            IList mlist = new ArrayList();
            mlist.Add("a");
            //IList<ValidateMaskRequest> masklist = new List<ValidateMaskRequest>();
            //masklist.Add(new ValidateMaskRequest());
            //foreach (ValidateMaskRequest chamber in masklist)
            //{
            //    chamber.BODY.LINENAME = "TCIMP100";
            //    chamber.BODY.MACHINENAME = "TCIMP";
            //    //chamber.BODY.MASKLIST = 
            //    chamber.Direction = Spec.DirType.BC_TO_MES;
            //    chamber.WaitReply = "WaitReply";
            //    chamber.HEADER.INBOXNAME = "55688";
            //    chamber.HEADER.MESSAGENAME = "Message55688";
            //}

            InvkeMethod(eServiceName.MESService, "ValidateMaskRequest", new object[4] { trxID, lineName, eqpID, mlist });

        }

        private void GlassReworkJudgeReport(object sender, EventArgs e)
        {
            Unit unit = NewUnit();
            Job job = NewJob();
            InvkeMethod(eServiceName.MESService, "GlassReworkJudgeReport", new object[4] { trxID, lineName, unit, job });
        }

        private void CFShortCutPermitRequest(object sender, EventArgs e)
        {
            string productName = "FC570184AA";
            string hostProductName = "387219749";
            string processLineName = "TCIMP100";
            InvkeMethod(eServiceName.MESService, "CFShortCutPermitRequest", new object[5] { trxID, lineName, productName, hostProductName, processLineName });

        }

        private void CFShortCutGlassProcessEnd(object sender, EventArgs e)
        {
            Line line = NewLine();
            Job job = NewJob();
            InvkeMethod(eServiceName.MESService, "CFShortCutGlassProcessEnd", new object[3] { trxID, line, job });

        }

        private void MaterialStateChanged(object sender, EventArgs e)
        {
            string eQPID = "TCIMP10B";
            string pPID = "A124";
            string materialmode = "Normal";
            string panelID = "234234123";
            string materialID = "a1234";
            eMaterialStatus materialStatus = new eMaterialStatus();
            string materialWeiaht = "1";
            string type = "A";
            string useCount = "2";
            string lifeQtime = "12";
            string groupID = "12423556534";
            string unitID = "01";
            string headID = "aa";
            string requestKe = "MaterialStatusChangeReport";
            InvkeMethod(eServiceName.MESService, "MaterialStateChanged", new object[16] { trxID, lineName, eQPID, pPID, materialmode, panelID, materialID, 
                materialStatus, materialWeiaht, type, useCount, lifeQtime, groupID, unitID, headID, requestKe });
        }

        private void AutoDecreaseMaterialQuantity(object sender, EventArgs e)
        {
            string eqpid = "TCIMP310";
            string materialid = "43445";
            string materialtype = "PR";
            string pnlID = "13213982037";
            string materialqty = "28";
            InvkeMethod(eServiceName.MESService, "AutoDecreaseMaterialQuantity", new object[7] { trxID, lineName, eqpid, materialid, materialtype, pnlID, materialqty });
        }

        private void ChangeTargetLife(object sender, EventArgs e)
        {
            string eqpName = "TCIMP10B";
            IList<ChangeTargetLife.CHAMBERc> chamberData = new List<ChangeTargetLife.CHAMBERc>();
            chamberData.Add(new ChangeTargetLife.CHAMBERc());
            foreach (ChangeTargetLife.CHAMBERc chamber in chamberData)
            {
                //chamber.AVERAGE = DateTime.Now;
                chamber.CHAMBERID = "TCIMP10B";
                chamber.QUANTITY = "2";
                chamber.AVERAGE = "1";
            }
            InvkeMethod(eServiceName.MESService, "ChangeTargetLife", new object[4] { trxID, lineName, eqpName, chamberData });
        }

        private void ChangeTankReport(object sender, EventArgs e)
        {
            string eqpId = "TCIMP10B";
            string tankNo = "01";
            InvkeMethod(eServiceName.MESService, "ChangeTankReport", new object[4] { trxID, lineName, eqpId, tankNo });
        }

        private void CFShortCutModeChangeRequest(object sender, EventArgs e)
        {
            string cfshortcutmode = "Y";
            InvkeMethod(eServiceName.MESService, "CFShortCutModeChangeRequest", new object[3] { trxID, lineName, cfshortcutmode });
        }

        private void BoxLineOutRequest(object sender, EventArgs e)
        {
            string portID = "01";
            Cassette cst = NewCassette();
            IList<Job> jobList = new List<Job>();
            jobList.Add(NewJob());
            InvkeMethod(eServiceName.MESService, "BoxLineOutRequest", new object[5] { trxID, lineName, portID, cst, jobList });
        }

        private void BoxIdCreateRequest(object sender, EventArgs e)
        {
            string productName = "TC570184BA";
            string boxType = "B100";
            InvkeMethod(eServiceName.MESService, "BoxIdCreateRequest", new object[4] { trxID, lineName, productName, boxType });
        }

        private void AssembleComplete(object sender, EventArgs e)
        {
            string eQPID = "TCIMP10B";
            string tFTPanelID = "TC570184AA";
            string tFTCSTID = "CAS001";
            string cFPanelID = "FC570184AA";
            string cFCSTID = "CAS002";
            InvkeMethod(eServiceName.MESService, "AssembleComplete", new object[7] { trxID, lineName, eQPID, tFTPanelID, tFTCSTID, cFPanelID, cFCSTID });
        }

        private void CheckRecipeParameter(object sender, EventArgs e)
        {
            Port port = NewPort();
            IDictionary<string, IList<RecipeCheckInfo>> recipeCheckInfos = new Dictionary<string, IList<RecipeCheckInfo>>();
            IList<RecipeCheckInfo> recipeCheckInfoss = new List<RecipeCheckInfo>();
            recipeCheckInfoss.Add(new RecipeCheckInfo());
            recipeCheckInfos.Add("1", recipeCheckInfoss);
            //recipeCheckInfos["1"][0].Result = eRecipeCheckResult.OK;   放开下面的也要放开这里
            //okRecipeInfo.Add("2", recipeCheckInfoss);     //Hardcode在public void CheckRecipeParameter(...中,再拿掉，做test
            InvkeMethod(eServiceName.MESService, "CheckRecipeParameter", new object[3] { trxID, port, recipeCheckInfos });
        }

        private void RecipeParameterReply(object sender, EventArgs e)
        {
            string lineRecipeName = "LineRecipeName100";
            string ppid = "PPID=3321";
            IList<RecipeCheckInfo> recipeCheckInfos = new List<RecipeCheckInfo>();
            RecipeCheckInfo ric = new RecipeCheckInfo();
            ric.Parameters.Add("1", "2");
            recipeCheckInfos.Add(ric);
            foreach (RecipeCheckInfo chamber in recipeCheckInfos)
            {
                chamber.EqpName = "chambermode";
                chamber.CreateTime = DateTime.Now;
                chamber.EQPNo = "123456";
                chamber.Result = 0;
            }
            string inboxname = "55688";
            InvkeMethod(eServiceName.MESService, "RecipeParameterReply", new object[6] { trxID, lineName, lineRecipeName, ppid, recipeCheckInfos, inboxname });
        }

        private void RecipeIDRegisterCheckReply(object sender, EventArgs e)
        {
            string lineRecipeName = "LineRecipeName---";
            string mesActionType = "MESActionType~~~";
            string inBoxName = "inbox55688";
            IList<RecipeCheckInfo> recipeCheckInfos = new List<RecipeCheckInfo>();
            recipeCheckInfos.Add(new RecipeCheckInfo());
            foreach (RecipeCheckInfo chamber in recipeCheckInfos)
            {
                chamber.CreateTime = DateTime.Now;
                chamber.EqpName = "recipeCheckInfosEQP";
            }

            IList<RecipeCheckInfo> NoCheckInfos = new List<RecipeCheckInfo>();
            NoCheckInfos.Add(new RecipeCheckInfo());
            foreach (RecipeCheckInfo chamber in NoCheckInfos)
            {
                chamber.CreateTime = DateTime.Now;
                chamber.EqpName = "NoCheckInfosEQP";
            }

            InvkeMethod(eServiceName.MESService, "RecipeIDRegisterCheckReply", new object[7] { trxID, lineName, lineRecipeName, mesActionType, inBoxName, recipeCheckInfos, NoCheckInfos });
        }

        private void PreCheckRecipeParameterReply(object sender, EventArgs e)
        {
            string linerecipe = "LineRecipe";
            string pPID = "PPPPPID";
            IList<RecipeCheckInfo> recipeCheckInfos = new List<RecipeCheckInfo>();
            //public RecipeCheckInfo NewRecipeCheckInfo(){ RecipeCheckInfo rec = new RecipeCheckInfo(); rec.Result = eRecipeCheckResult.OK; return rec; }
            RecipeCheckInfo ric = new RecipeCheckInfo();
            ric.Parameters.Add("1", "2");
            recipeCheckInfos.Add(ric);
            foreach (RecipeCheckInfo chamber in recipeCheckInfos)
            {
                chamber.CreateTime = DateTime.Now;
                chamber.EqpName = "chambermode";
                chamber.LineRecipeName = "RRRR"; ;
            }
            string inboxname = "459992";
            InvkeMethod(eServiceName.MESService, "PreCheckRecipeParameterReply", new object[6] { trxID, lineName, linerecipe, pPID, recipeCheckInfos, inboxname });
        }

        private void BoxLabelInformationRequest(object sender, EventArgs e)
        {
            string eqpNo = "L2";
            string eqpID = "TCIMP10B";
            string boxID = "BOX001";
            string boxType = "InBox";
            List<Cassette> subBoxList = new List<Cassette>();
            Cassette cst = new Cassette();
            cst.SubBoxID = "SUBO01";
            cst.eBoxType = eBoxType.InBox;
            subBoxList.Add(cst);

            InvkeMethod(eServiceName.MESService, "BoxLabelInformationRequest", new object[7] { trxID, lineName, eqpNo, eqpID, boxID, boxType, subBoxList });
        }

        private void BoxProcessEnd(object sender, EventArgs e)
        {
            string portID = "01";
            Cassette cst = NewCassette();
            IList<Job> jobList = new List<Job>();
            jobList.Add(NewJob());
            InvkeMethod(eServiceName.MESService, "BoxProcessEnd", new object[5] { trxID, lineName, portID, cst, jobList });
        }

        private void FacilityCheckReport(object sender, EventArgs e)
        {
            IList<FacilityCheckReport.MACHINEc> machineList = new List<FacilityCheckReport.MACHINEc>();
            machineList.Add(new FacilityCheckReport.MACHINEc());
            foreach (FacilityCheckReport.MACHINEc chamber in machineList)
            {
                //chamber.FACILITYPARALIST = ;
                chamber.MACHINENAME = "TCIMP10B";
                chamber.MACHINESTATENAME = "RUN";
                List<FacilityCheckReport.PARAc> l = new List<FacilityCheckReport.PARAc>();
                l.Add(new FacilityCheckReport.PARAc());
                chamber.FACILITYPARALIST = l;
            }
            InvkeMethod(eServiceName.MESService, "FacilityCheckReport", new object[3] { trxID, lineName, machineList });
        }

        private void FacilityParameterReply(object sender, EventArgs e)
        {
            IList<FacilityParameterReply.MACHINEc> machineList = new List<FacilityParameterReply.MACHINEc>();
            machineList.Add(new FacilityParameterReply.MACHINEc());
            foreach (FacilityParameterReply.MACHINEc chamber in machineList)
            {
                chamber.MACHINENAME = "TCIMP10B";
                chamber.MACHINESTATENAME = "RUN";
                List<FacilityParameterReply.PARAc> l = new List<FacilityParameterReply.PARAc>();
                l.Add(new FacilityParameterReply.PARAc());
                chamber.FACILITYPARALIST = l;
            }
            string mesINBOXName = "MesINBOXName=Name~~~";
            InvkeMethod(eServiceName.MESService, "FacilityParameterReply", new object[4] { trxID, lineName, machineList, mesINBOXName });
        }

        private void BoxProcessLineRequest(object sender, EventArgs e)
        {
            string ppklinename = "CCPPK";
            string boxid = "BOX001";
            string portID = "01";
            InvkeMethod(eServiceName.MESService, "BoxProcessLineRequest", new object[5] { trxID, lineName, ppklinename, boxid, portID });
        }

        private void BoxTargetPortChanged(object sender, EventArgs e)
        {
            string boxid = "BOX001";
            string portID = "01";
            InvkeMethod(eServiceName.MESService, "BoxTargetPortChanged", new object[4] { trxID, lineName, boxid, portID });
        }

        private void CutComplete(object sender, EventArgs e)
        {
            string cSTID = "CAS001";
            Job job = NewJob();
            InvkeMethod(eServiceName.MESService, "CutComplete", new object[4] { trxID, lineName, cSTID, job });
        }


        private void GlassChangeMACOJudge(object sender, EventArgs e)
        {
            Job job = NewJob();
            InvkeMethod(eServiceName.MESService, "GlassChangeMACOJudge", new object[3] { trxID, lineName, job });
        }


        private void LineLinkChanged(object sender, EventArgs e)
        {
            string eqpid = "TCIMP10B";
            eCELL_TCVDispatchRule proresult = eCELL_TCVDispatchRule.POL;
            InvkeMethod(eServiceName.MESService, "LineLinkChanged", new object[4] { trxID, lineName, eqpid, proresult });
        }


        private void MaterialWeightReport(object sender, EventArgs e)
        {
            Equipment eqp = NewEqp();
            string materialMode = "Normal";
            string panelID = "TC570184AA";
            List<MaterialEntity> l = listMaterial();
            InvkeMethod(eServiceName.MESService, "MaterialWeightReport", new object[5] { trxID, eqp, materialMode, panelID, l });
        }

        private void MaxCutGlassProcessEnd(object sender, EventArgs e)
        {
            Equipment eqp = NewEqp();
            Job job = NewJob();
            InvkeMethod(eServiceName.MESService, "MaxCutGlassProcessEnd", new object[4] { trxID, lineName, eqp, job });
        }


        private void PortDataUpdate(object sender, EventArgs e)
        {
            Line line = NewLine();
            foreach (Equipment eqp in ObjectManager.EquipmentManager.GetEQPsByLine(line.Data.LINEID))  //Jun Modify 20141222 By Line 取得EQP List
            {
                foreach (Port _port in ObjectManager.PortManager.GetPorts(eqp.Data.NODEID))
                {
                    _port.File.CassetteID = "CAS001";
                    _port.File.TransferMode = ePortTransferMode.AGV;
                    _port.File.UseGrade = "OK";
                    _port.File.Mode = ePortMode.OK;
                    _port.File.Type = ePortType.BothPort;
                    _port.File.EnableMode = ePortEnableMode.Enabled;
                    _port.File.Status = ePortStatus.LC;
                    _port.File.OperMode = ePortOperMode.PACK;
                    _port.File.CassetteSetCode = "cas1111111";

                }
            }
            InvkeMethod(eServiceName.MESService, "PortDataUpdate", new object[2] { trxID, line });
        }

        private void PortEnableChanged(object sender, EventArgs e)
        {
            IList<Port> portlist = new List<Port>();
            portlist.Add(NewPort());
            foreach (Port _port in portlist)
            {
                _port.File.EnableMode = ePortEnableMode.Disabled;
            }
            InvkeMethod(eServiceName.MESService, "PortEnableChanged", new object[3] { trxID, lineName, portlist });
        }

        private void PortUseTypeChanged(object sender, EventArgs e)
        {
            IList<Port> portlist = new List<Port>();
            portlist.Add(NewPort());
            foreach (Port _port in portlist)
            {
                _port.File.Mode = ePortMode.MIX; ;
            }
            InvkeMethod(eServiceName.MESService, "PortUseTypeChanged", new object[3] { trxID, lineName, portlist });
        }


        private void MachineSiteChangeRequest(object sender, EventArgs e)
        {
            string newSite = "T1";
            string machineEnable = "ENABLE";
            string user = "marine";
            InvkeMethod(eServiceName.MESService, "MachineSiteChangeRequest", new object[5] { trxID, lineName, newSite, machineEnable, user });
        }

        private void DefectCodeReportByGlass(object sender, EventArgs e)
        {
            string newSite = "T1";
            string machineEnable = "ENABLE";
            string user = "marine";
            InvkeMethod(eServiceName.MESService, "DefectCodeReportByGlass", new object[5] { trxID, lineName, newSite, machineEnable, user });
        }

        private void CheckBoxNameRequest(object sender, EventArgs e)
        {
            string bOXID = "BOX001";
            string PalletteID = "PAL001";
            InvkeMethod(eServiceName.MESService, "CheckBoxNameRequest", new object[4] { trxID, lineName, bOXID, PalletteID });
        }

        private void BoxProcessStarted(object sender, EventArgs e)
        {
            Port port = NewPort();
            Cassette cst = NewCassette();
            InvkeMethod(eServiceName.MESService, "BoxProcessStarted", new object[3] { trxID, port, cst });
        }

        private void BoxProcessCanceled(object sender, EventArgs e)
        {
            string portID = "01";
            string boxID = "CAS001";
            string reasonCode = "LABCC";
            string reasonText = "..........";
            InvkeMethod(eServiceName.MESService, "BoxProcessCanceled", new object[6] { trxID, lineName, portID, boxID, reasonCode, reasonText });
        }

        private void MachineModeChangeRequest(object sender, EventArgs e)
        {
            InvkeMethod(eServiceName.MESService, "MachineModeChangeRequest", new object[2] { trxID, lineName });
        }

        private void LineStateChanged(object sender, EventArgs e)
        {
            string linestatus = "LineStatus = Idle";
            InvkeMethod(eServiceName.MESService, "LineStateChanged", new object[3] { trxID, lineName, linestatus });
        }

        private void PanelInformationRequest(object sender, EventArgs e)
        {
            string eQPID = "TCIMP10B";
            string panelID = "CC570184AA";
            InvkeMethod(eServiceName.MESService, "PanelInformationRequest", new object[4] { trxID, lineName, eQPID, panelID });
        }

        private void PalletLabelInformationRequest(object sender, EventArgs e)
        {
            string palletteID = "PAl001";
            string eqpNoforTimeout = "EQTimeOut";
            InvkeMethod(eServiceName.MESService, "PalletLabelInformationRequest", new object[4] { trxID, lineName, palletteID, eqpNoforTimeout });
        }

        private void PalletProcessCanceled(object sender, EventArgs e)
        {
            string palletteID = "PAL001";
            string portID = "01";
            InvkeMethod(eServiceName.MESService, "PalletProcessCanceled", new object[4] { trxID, lineName, palletteID, portID });
        }

        private void PalletProcessEnd(object sender, EventArgs e)
        {
            string palletteID = "PAL001";
            string carrierName = "CAS001";
            string pPID = "L2:AA";
            List<string> boxList = new List<string>();
            boxList.Add("BOX001");
            InvkeMethod(eServiceName.MESService, "PalletProcessEnd", new object[6] { trxID, lineName, palletteID, carrierName, pPID, boxList });
        }

        private void PalletProcessStarted(object sender, EventArgs e)
        {
            string palletteID = "PAL001";
            string pPID = "L2:AA";
            List<string> boxList = new List<string>();
            boxList.Add("BOX001");
            InvkeMethod(eServiceName.MESService, "PalletProcessStarted", new object[5] { trxID, lineName, palletteID, pPID, boxList });
        }

        private void PanelReportByGlass(object sender, EventArgs e)
        {
            IList<PanelReportByGlass.ORIGINALPRODUCTc> originalProductList = new List<PanelReportByGlass.ORIGINALPRODUCTc>();
            PanelReportByGlass.ORIGINALPRODUCTc or = new PanelReportByGlass.ORIGINALPRODUCTc();
            or.ORIGINALPRODUCTNAME = "TC570184AA";
            or.PRODUCTLIST = new List<PanelReportByGlass.PRODUCTc>();
            PanelReportByGlass.PRODUCTc pro = new PanelReportByGlass.PRODUCTc();
            pro.PRODUCTNAME = "TC570184AB";
            pro.MACOJUDGE = "B";
            pro.MURACODES = "";
            pro.EVENTCOMMENT = ",,,,,";
            or.PRODUCTLIST.Add(pro);
            originalProductList.Add(or);
            InvkeMethod(eServiceName.MESService, "PanelReportByGlass", new object[3] { trxID, lineName, originalProductList });
        }

        private void QtimeOverReport(object sender, EventArgs e)
        {
            string productName = "TC570184AB";
            string qtime = "8";
            string fromEQ = "AA";
            string fromTracelvl = "OK";
            string startTime = "01";
            string toEQ = "02";
            string toTracelvl = "....";
            string endTime = "08";
            InvkeMethod(eServiceName.MESService, "QtimeOverReport", new object[10] { trxID, lineName, productName, qtime, fromEQ, fromTracelvl, startTime, toEQ, toTracelvl, endTime });
        }

        private void QtimeSetChanged(object sender, EventArgs e)
        {
            string user = "marine";
            string qtime = "8";
            string fromEQ = "BB";
            string fromTracelvl = "NG";
            string toEQ = "TCIMP10B";
            string toTracelvl = ",,,,";
            InvkeMethod(eServiceName.MESService, "QtimeSetChanged", new object[8] { trxID, lineName, user, qtime, fromEQ, fromTracelvl, toEQ, toTracelvl });
        }

        private void UnitStateChanged(object sender, EventArgs e)
        {
            Unit unit = new Unit(new UnitEntityData(), new UnitEntityFile());
            unit.Data.LINEID = "TCIMP100";
            unit.Data.SERVERNAME = "ServerName~~";
            unit.Data.NODENO = "L2";
            unit.Data.NODEID = "TCIMP10B";
            unit.File.Status = eEQPStatus.IDLE;
            unit.File.ProductType = 1;
            unit.File.MESStatus = "DOWN";
            string alarmID = "01";
            string alarmText = "warning";
            string alarmTime = "10";
            InvkeMethod(eServiceName.MESService, "UnitStateChanged", new object[5] { trxID, unit, alarmID, alarmText, alarmTime });
        }

        private void RuncardIdCreateRequest(object sender, EventArgs e)
        {
            string productName = "TC570184AB";
            InvkeMethod(eServiceName.MESService, "RuncardIdCreateRequest", new object[3] { trxID, lineName, productName });
        }

        private void RuncardLableInformationRequest(object sender, EventArgs e)
        {
            string lotID = "TC570184N00";
            InvkeMethod(eServiceName.MESService, "RuncardLableInformationRequest", new object[3] { trxID, lineName, lotID });
        }

        private void ValidateBoxWeightRequest(object sender, EventArgs e)
        {
            string boxtotalweight = "100";
            IList<string> boxlist = new List<string>();
            boxlist.Add("BOX001");
            string eqpNoforTimeout = "L3";
            InvkeMethod(eServiceName.MESService, "ValidateBoxWeightRequest", new object[5] { trxID, lineName, boxtotalweight, boxlist, eqpNoforTimeout });
        }

        private void UVMaskUseCount(object sender, EventArgs e)
        {
            string eqpID = "TCIMP100";
            string maskID = "MSK001";
            string useQty = "1";
            InvkeMethod(eServiceName.MESService, "UVMaskUseCount", new object[5] { trxID, lineName, eqpID, maskID, useQty });
        }

        private void ValidateMaskByCarrierRequest(object sender, EventArgs e)
        {
            Port port = NewPort();
            InvkeMethod(eServiceName.MESService, "ValidateMaskByCarrierRequest", new object[2] { trxID, port });
        }

        private void ValidatePalletRequest(object sender, EventArgs e)
        {
            string portID = "01";
            string portmode = "TFT";
            string palletID = "PAL001";
            IList<string> boxlist = new List<string>();
            boxlist.Add("BOX001");
            string eqpNoforTimeout = "TimeOut";
            InvkeMethod(eServiceName.MESService, "ValidatePalletRequest", new object[7] { trxID, lineName, portID, portmode, palletID, boxlist, eqpNoforTimeout });
        }

        private void ValidateBoxRequest(object sender, EventArgs e)
        {
            Port port = NewPort();
            IList<string> boxlist = new List<string>();
            boxlist.Add("BOX001");
            InvkeMethod(eServiceName.MESService, "ValidateBoxRequest", new object[3] { trxID, port, boxlist });
        }

        private void ValidateGlassRequest(object sender, EventArgs e)
        {
            Equipment eqp = NewEqp();
            IList<Job> joblist = new List<Job>();
            joblist.Add(NewJob());
            InvkeMethod(eServiceName.MESService, "ValidateGlassRequest", new object[4] { trxID, lineName, eqp, joblist });
        }

        private void PalletIdCreateRequest(object sender, EventArgs e)
        {
            string boxName = "BOX001";
            InvkeMethod(eServiceName.MESService, "PalletIdCreateRequest", new object[3] { trxID, lineName, boxName });
        }

        private void OutBoxProcessEnd(object sender, EventArgs e)
        {
            string portID = "01";
            Cassette cst = NewCassette();
            IList<Job> jobList = new List<Job>();
            Job job = NewJob();
            jobList.Add(job);
            InvkeMethod(eServiceName.MESService, "OutBoxProcessEnd", new object[5] { trxID, lineName, portID, cst, jobList });
        }

        # endregion

	}
}
