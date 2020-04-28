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

namespace UniAuto.UniBCS.CSOT.SECSService
{
    public partial class FrmTestSECS : Form, IApplicationContextAware
    {
        private IApplicationContext _applicationContext;
        private string _nikonEqpNo;
        private string _nikonEqpId;
		private string _csotEqpNo;
		private string _csotEqpId;

        public FrmTestSECS()
        {
            InitializeComponent();
            _nikonEqpNo = "L7";
            _nikonEqpId = "TBPNK180";
			_csotEqpNo = "L3";
			_csotEqpId = "TBPDN130";
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
                        NLogManager.Logger.LogErrorWrite("Service", this.GetType().Name, MethodBase.GetCurrentMethod().Name, string.Format("Methed {0} is not exist in class {1}.", methodName, obj.GetType().ToString()));
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
            InvkeMethod(eServiceName.NikonSECSService, "TS1F3_H_SelectedEquipmentStatusRequest", new object[4] { _nikonEqpNo, _nikonEqpId, (uint)1024, string.Empty });
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
            if (textNodeID.Text.Trim() != string.Empty)
                _csotEqpId = textNodeID.Text.Trim();
            if (textNodeNO.Text.Trim() != string.Empty)
                _csotEqpNo = textNodeNO.Text.Trim();
 
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
						new object[] { _csotEqpNo, _csotEqpId, "LOTID01", "GLASSID01", "PPID01", "TEST",string.Empty });
					break;				
				case "S2F105":
					InvkeMethod(eServiceName.CSOTSECSService, "TS2F105_H_LotEndInformSend",
						new object[] { _csotEqpNo, _csotEqpId,"5","1","GLASSID01","PPID01","TEST" , string.Empty });
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
							}, "TEST", string.Empty });
					break;
				case "S2F123":
					InvkeMethod(eServiceName.CSOTSECSService, "TS2F123_H_SetGlassRecipeGroupEndFlagSend",
						new object[] { _csotEqpNo, _csotEqpId,"5","1","GLASSID01","1", "TEST", string.Empty });
					break;
				case "S5F5":
					InvkeMethod(eServiceName.CSOTSECSService, "TS5F5_H_ListAlarmRequest",
						new object[] { _csotEqpNo, _csotEqpId, "TEST",string.Empty });
					break;
				case "S7F19":
					InvkeMethod(eServiceName.CSOTSECSService, "TS7F19_H_CurrentEPPDRequest",
						new object[] { _csotEqpNo, _csotEqpId, "TEST",string.Empty });
					break;
				case "S7F23":
					break;
				case "S7F25":
					InvkeMethod(eServiceName.CSOTSECSService, "TS7F25_H_FormattedProcessProgramRequest",
						new object[] { new RecipeCheckInfo(_csotEqpNo, 1, 1, "PPID01"), "TEST",string.Empty  }
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

        private void button39_Click(object sender, EventArgs e)
        {
            string trxid = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            string OKPortModeStoreQTime = "0";
            string OKPortModeProductTypeCheckMode = "1";
            string NGPortModeStoreQTime = "1";
            string NGPortModeProductTypeCheckMode = "0";
            string NGPortJudge = "0";
            string PDPortModeStoreQTime = "1";
            string PDPortModeProductTypeCheckMode = "1";
            string RPPortModeStoreQTime = "0";
            string RPPortModeProductTypeCheckMode = "1";
            string IRPortModeStoreQTime = "1";
            string IRPortModeProductTypeCheckMode = "0";
            string MIXPortModeStoreQTime = "1";
            string MIXPortModeProductTypeCheckMode = "1";
            string MIXPortJudge = "1";
            string OperatorID = "0";

            InvkeMethod(eServiceName.CFSpecialService, "UnloadingPortSettingCommand", new object[] { trxid, OKPortModeStoreQTime,  OKPortModeProductTypeCheckMode, 
                NGPortModeStoreQTime,  NGPortModeProductTypeCheckMode,  NGPortJudge,  PDPortModeStoreQTime, PDPortModeProductTypeCheckMode,  RPPortModeStoreQTime,  
                RPPortModeProductTypeCheckMode, IRPortModeStoreQTime,  IRPortModeProductTypeCheckMode,  MIXPortModeStoreQTime, MIXPortModeProductTypeCheckMode,  
                MIXPortJudge,  OperatorID });
        }

        private void button40_Click(object sender, EventArgs e)
        {
            string trxid = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            string cvBUF = "1";
            string b01RW = "0";
            string b02RW = "1";
            string operID = "0";
            InvkeMethod(eServiceName.CFSpecialService, "BufferRWJudgeCapacityChangeCommand", new object[] { trxid, cvBUF,b01RW,b02RW,operID });
        }

        private void button41_Click(object sender, EventArgs e)
        {
            string trxid = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            string recipe = "ABCDtest";
            InvkeMethod(eServiceName.CFSpecialService, "ExposureMaskCheckCommand", new object[] { trxid, recipe });
        }

        private void button42_Click(object sender, EventArgs e)
        {
            string trxid= DateTime.Now.ToString("yyyyMMddHHmmssfff");
            string currentEqpNo = "L2";
            Job job = new Job(1001,1);
            InvkeMethod(eServiceName.JobService, "SetLastGlassCommand", new object[] { trxid, currentEqpNo, job});
        }

        private void button43_Click(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }  

	}
}
