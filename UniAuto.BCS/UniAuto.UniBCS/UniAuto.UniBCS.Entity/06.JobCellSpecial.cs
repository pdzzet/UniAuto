using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace UniAuto.UniBCS.Entity
{
      [Serializable]
      [TypeConverter(typeof(ExpandableObjectConverter))]
      public class JobCellSpecial : ICloneable
      {
            //BCS Memo Flag
            private eCUTTING_FLAG _cuttingFlag = eCUTTING_FLAG.NOT_CUTTING;
            private eBitResult _engModeFlag = eBitResult.OFF;
            private eBitResult _cqltModeFlag = eBitResult.OFF;
            private eBitResult _assemblycompleteFlag = eBitResult.OFF;
            private eBitResult _cutCompleteFlag = eBitResult.OFF;
            private eBitResult _crosslineRepotFlag = eBitResult.OFF;
            private eBitResult _crossLineBackReportFlag = eBitResult.OFF;
            private eBitResult _maxcutrepotFlag = eBitResult.OFF;
            private string _uvMaskUseCount = "0";
            private string _uvMaskNames = string.Empty;
            private string _crossLineFlag = "N";
            private string _rptflag = string.Empty; //Watson Add 20141220 FOR MES New SPEC 3.89
            private string _gapNGFlag = string.Empty;
            private string _maskid = string.Empty; //Watson Add 2015122 For MaskCleaner
            private DateTime _unitProcessStartTime = DateTime.Now;//add by hujunpeng 20181119

            public eBitResult ENGModeFlag
            {
                  get { return _engModeFlag; }
                  set { _engModeFlag = value; }
            }

            public eBitResult CQLTModeFlag
            {
                  get { return _cqltModeFlag; }
                  set { _cqltModeFlag = value; }
            }


            public eCUTTING_FLAG CuttingFlag
            {
                  get { return _cuttingFlag; }
                  set { _cuttingFlag = value; }
            }

            public eBitResult AssemblyCompleteFlag
            {
                  get { return _assemblycompleteFlag; }
                  set { _assemblycompleteFlag = value; }
            }

            public eBitResult CutCompleteFlag
            {
                  get { return _cutCompleteFlag; }
                  set { _cutCompleteFlag = value; }
            }

            public eBitResult CrossLineReportFlag
            {
                  get { return _crosslineRepotFlag; }
                  set { _crosslineRepotFlag = value; }
            }

            public eBitResult CrossLineBackReportFlag
            {
                get { return _crossLineBackReportFlag; }
                set { _crossLineBackReportFlag = value; }
            }

            public eBitResult MaxCutReportFlag
            {
                  get { return _maxcutrepotFlag; }
                  set { _maxcutrepotFlag = value; }
            }

            public string UVMaskUseCount 
            {
                  get { return _uvMaskUseCount; }
                  set { _uvMaskUseCount = value; }
            }

            public string UVMaskNames 
            {
                  get { return _uvMaskNames; }
                  set { _uvMaskNames = value; }
            }

            public string CrossLineFlag
            {
                  get { return _crossLineFlag; }
                  set { _crossLineFlag = value; }
            }

            //Watson Add 20141220 FOR MES New SPEC 3.89
            public string RTPFlag
            {
                  get { return _rptflag; }
                  set { _rptflag = value; }
            }

            public string GAPNGFlag
            {
                  get { return _gapNGFlag; }
                  set { _gapNGFlag = value; }
            }
            //Watson Add 2015122 For MaskCleaner
            public string MASKID
            {
                  get { return _maskid; }
                  set { _maskid = value; }
            }
            public DateTime UnitProcessStartTime
            {
                get { return _unitProcessStartTime; }
                set { _unitProcessStartTime = value; }
            }
            //Glass Job Data Item
            private eHostMode _controlMode = eHostMode.OFFLINE;    //INT 0 : Off Line ,1 : Local ,2 : Remote
            private string _productID = "0"; //INT
            private string _chipCount = "0"; //INT
            private string _cassetteSettingCode = string.Empty; //ASCII
            private string _ownerID = string.Empty; //ASCII
            private string _groupID = string.Empty; //ASCII

            private string _ppoSlotNo = "0"; //INT
            private string _reworkCount = "0"; //INT

            private string _cfCassetteSeqNo = "0"; //INT
            private string _cfJobSeqNo = "0"; //INT
            private string _odfBoxChamberOpenTime01 = string.Empty;
            private string _odfBoxChamberOpenTime02 = string.Empty;
            private string _odfBoxChamberOpenTime03 = string.Empty;
            private string _networkNo = "0"; //INT
            private string _repairCount = "0"; //INT
            private string _uvMaskAlreadyUseCount = "0"; //INT
            private string _arrayTTPEQVer = "0";

            private string _turnAngle = "1"; //INT
            private string _returnModeTurnAngle = "0"; //INT

            private string _abnormalCode = string.Empty; //ASCII
            private string _panelSize = "0"; //INT
            private string _crossLineCassetteSettingCode = string.Empty; //ASCII
            private string _panelSizeFlag = "0";
            private string _mmgFlag = "0";
            private string _crossLinePanelSize = "0";
            private string _cutProductID = "0";
            private string _cutProductID2 = "0";
            private string _cutCrossProductID = "0";
            private string _cutProductType = "0";
            private string _cutProductType2 = "0";
            private string _cutCrossProductType = "0";
            private string _cutLayout = "0";
            private string _cutPoint = "0";
            private string _cutSubProductSpecs = "0";
            private string _polProductType = "0";
            private string _polProductID = "0";
            private string _crossLinePPID = string.Empty; //ASCII

            private string _currentRunMode = "0"; //INT
            private string _nodeStack = "0"; //INT

            private string _repairResult = "0"; //INT
            private string _runMode = "0"; //INT
            private string _virtualPortEnableMode = "0"; //INT

            private string _boxuldflag = string.Empty;  //MES
            private bool _cgmoFlag = false;
            private string _glassThickness = "1"; //INT
            private string _productOwner = "1"; //INT
            private string _operationID = "0"; //INT
            private string _maxRwkCount = "0"; //INT//T3 shihyang add
            private string _oQCBank = "0"; //INT//T3 shihyang add
            private string _currentRwkCount = "0";//INT
            private string _pILiquidTypet = "0";//INT
            private string _panelOXInformation  = "0";//INT
            private string _panelLWH = string.Empty;
            private string _blockOXInformation = string.Empty;
            private string _blockLWH = string.Empty;
            private string _defectCode = string.Empty; //ASCII
            private string _vendorName = string.Empty; //ASCII
            private string _assembleSeqNo = "0"; //INT
            private string _blockSize = "0"; //INT
            private string _blockCount = "0"; //INT
            private string _pcsProductID = "0"; //INT
            private string _pcsProductID2 = "0"; //INT
            private string _pcsProductType = "0"; //INT
            private string _pcsProductType2 = "0"; //INT
            private string _rejudgeCount = "0"; //INT
            private string _panelGroup = string.Empty; //1碼 ASCII
            private string _dotRepairCount = "0"; //INT
            private string _lineRepairCount = "0"; //INT
            private string _cfPanelOXInfoUnassembled = string.Empty;
            private string _tftPanelOXInfoUnassembled = string.Empty;
            private string _bURCheckCount = "0"; //INT
            private string _cUTCassetteSettingCode = string.Empty;
            private string _cUTCassetteSettingCode2 = string.Empty;
            private string _disCardJudges = string.Empty;
            private string _sortFlagNo = string.Empty;
            private ProductType _productType1 = new ProductType();
            private ProductType _productType2 = new ProductType();
            private ProductID _productID1 = new ProductID();
            private ProductID _productID2 = new ProductID();
            private string _rwLifeTime = string.Empty;  //add ReWorkLifetime
            private string _pcsCassetteSettingCodeList = string.Empty;
            private string _pcsBlockSizeList = string.Empty;
            private string _blockSize1 = "0";//INT
            private string _blockSize2 = "0";//INT
            private string _hvaChippingFlagForJps = "N";//huangjiayin add for HVACHIPPINGFLAG TO HVA over Q time flag
            private string _tftIdLastChar = string.Empty;//20171120 huangjiayin add for EDA


            [Category("PLC")]
            public ProductType ProductType1
            {
                get { return _productType1; }
                set { _productType1 = value; }
            }
            [Category("PLC")]
            public ProductType ProductType2
            {
                get { return _productType2; }
                set { _productType2 = value; }
            }
            [Category("PLC")]
            public ProductID ProductID1
            {
                get { return _productID1; }
                set { _productID1 = value; }
            }
            [Category("PLC")]
            public ProductID ProductID2
            {
                get { return _productID2; }
                set { _productID2 = value; }
            }
            /// <summary>
            /// T3. After assembly event, TFT glass keep self panel OX information befor assemble. (20151028 cy.tsai)
            /// </summary>
            public string TFTPanelOXInfoUnassembled
            {
                  get { return _tftPanelOXInfoUnassembled; }
                  set { _tftPanelOXInfoUnassembled = value; }
            }
            /// <summary>
            /// T3. After assembly event, TFT glass keep CF glass panel OX information befor assemble. (20151028 cy.tsai)
            /// </summary>
            public string CFPanelOXInfoUnassembled
            {
                  get { return _cfPanelOXInfoUnassembled; }
                  set { _cfPanelOXInfoUnassembled = value; }
            }

            public eHostMode ControlMode
            {
                  get { return _controlMode; }
                  set { _controlMode = value; }
            }
            public string PanelOXInformation
            {
                get { return _panelOXInformation; }
                set { _panelOXInformation = value; }
            }
            public string PanelLWH
            {
                get { return _panelLWH; }
                set { _panelLWH = value; }
            }
            public string BlockOXInformation
            {
                  get { return _blockOXInformation; }
                  set { _blockOXInformation = value; }
            }
            public string BlockLWH
            {
                get { return _blockLWH; }
                set { _blockLWH = value; }
            }
            public string HVAChippingFlagForJps
            {
                get { return _hvaChippingFlagForJps; }
                set { _hvaChippingFlagForJps = value; }
            }
            public string TFTIdLastChar
            {
                get { return _tftIdLastChar; }
                set { _tftIdLastChar = value; }
            }
            public string ChipCount
            {
                  get { return _chipCount; }
                  set { _chipCount = value; }
            }
            public string ProductID
            {
                  get { return _productID; }
                  set { _productID = value; }
            }
            public string CassetteSettingCode
            {
                  get { return _cassetteSettingCode; }
                  set { _cassetteSettingCode = value; }
            }
            public string OwnerID
            {
                  get { return _ownerID; }
                  set { _ownerID = value; }
            }
            public string GroupID
            {
                get { return _groupID; }
                set { _groupID = value; }
            }
            public string PPOSlotNo
            {
                  get { return _ppoSlotNo; }
                  set { _ppoSlotNo = value; }
            }
            public string ReworkCount
            {
                  get { return _reworkCount; }
                  set { _reworkCount = value; }
            }


            public string CFCassetteSeqNo
            {
                  get { return _cfCassetteSeqNo; }
                  set { _cfCassetteSeqNo = value; }
            }
            public string CFJobSeqNo
            {
                  get { return _cfJobSeqNo; }
                  set { _cfJobSeqNo = value; }
            }
            public string ODFBoxChamberOpenTime01
            {
                  get { return _odfBoxChamberOpenTime01; }
                  set { _odfBoxChamberOpenTime01 = value; }
            }
            public string ODFBoxChamberOpenTime02
            {
                  get { return _odfBoxChamberOpenTime02; }
                  set { _odfBoxChamberOpenTime02 = value; }
            }
            public string ODFBoxChamberOpenTime03
            {
                  get { return _odfBoxChamberOpenTime03; }
                  set { _odfBoxChamberOpenTime03 = value; }
            }
            public string NetworkNo
            {
                  get { return _networkNo; }
                  set { _networkNo = value; }
            }
            public string RepairCount
            {
                  get { return _repairCount; }
                  set { _repairCount = value; }
            }
            public string UVMaskAlreadyUseCount
            {
                  get { return _uvMaskAlreadyUseCount; }
                  set { _uvMaskAlreadyUseCount = value; }
            }
            public string ArrayTTPEQVer
            {
                  get { return _arrayTTPEQVer; }
                  set { _arrayTTPEQVer = value; }
            }

            public string TurnAngle
            {
                  get { return _turnAngle; }
                  set { _turnAngle = value; }
            }
            public string ReturnModeTurnAngle
            {
                  get { return _returnModeTurnAngle; }
                  set { _returnModeTurnAngle = value; }
            }
            public string AbnormalCode
            {
                  get { return _abnormalCode; }
                  set { _abnormalCode = value; }
            }
            public string PanelSize
            {
                  get { return _panelSize; }
                  set { _panelSize = value; }
            }
            public string CrossLineCassetteSettingCode
            {
                  get { return _crossLineCassetteSettingCode; }
                  set { _crossLineCassetteSettingCode = value; }
            }
            public string PanelSizeFlag
            {
                  get { return _panelSizeFlag; }
                  set { _panelSizeFlag = value; }
            }
            public string MMGFlag
            {
                  get { return _mmgFlag; }
                  set { _mmgFlag = value; }
            }
            public string CrossLinePanelSize
            {
                  get { return _crossLinePanelSize; }
                  set { _crossLinePanelSize = value; }
            }
            public string CUTProductID
            {
                  get { return _cutProductID; }
                  set { _cutProductID = value; }
            }
            public string CUTProductID2
            {
                get { return _cutProductID2; }
                set { _cutProductID2 = value; }
            }
            public string CUTCrossProductID
            {
                  get { return _cutCrossProductID; }
                  set { _cutCrossProductID = value; }
            }
            public string CUTProductType
            {
                  get { return _cutProductType; }
                  set { _cutProductType = value; }
            }
            public string CUTProductType2
            {
                get { return _cutProductType2; }
                set { _cutProductType2 = value; }
            }
            public string CUTCrossProductType
            {
                  get { return _cutCrossProductType; }
                  set { _cutCrossProductType = value; }
            }
            public string CutLayout
            {
                get { return _cutLayout; }
                set { _cutLayout = value; }
            }
            public string CutPoint
            {
                get { return _cutPoint; }
                set { _cutPoint = value; }
            }
            public string CutSubProductSpecs
            {
                get { return _cutSubProductSpecs; }
                set { _cutSubProductSpecs = value; }
            }
            public string POLProductType
            {
                  get { return _polProductType; }
                  set { _polProductType = value; }
            }
            public string POLProductID
            {
                  get { return _polProductID; }
                  set { _polProductID = value; }
            }
            public string CrossLinePPID
            {
                  get { return _crossLinePPID; }
                  set { _crossLinePPID = value; }
            }
            public string CurrentRunMode
            {
                  get { return _currentRunMode; }
                  set { _currentRunMode = value; }
            }
            public string NodeStack
            {
                  get { return _nodeStack; }
                  set { _nodeStack = value; }
            }
            public string RepairResult
            {
                  get { return _repairResult; }
                  set { _repairResult = value; }
            }
            public string RunMode
            {
                  get { return _runMode; }
                  set { _runMode = value; }
            }
            public string VirtualPortEnableMode
            {
                  get { return _virtualPortEnableMode; }
                  set { _virtualPortEnableMode = value; }
            }
            public string BOXULDFLAG
            {
                  get { return _boxuldflag; }
                  set { _boxuldflag = value; }
            }
            public bool CGMOFlag
            {
                  get { return _cgmoFlag; }
                  set { _cgmoFlag = value; }
            }
            //T3 cs.chou 20150824(ODF)
            public string AssembleSeqNo
            {
                  get { return _assembleSeqNo; }
                  set { _assembleSeqNo = value; }
            }
            //T3 cs.chou 20150824(PCS)
            public string BlockSize
            {
                  get { return _blockSize; }
                  set { _blockSize = value; }
            }
            public string BlockCount
            {
                get { return _blockCount; }
                set { _blockCount = value; }
            }
            public string PCSProductID//不用了 全用 CUTProductID & CUTProductID2 
            {
                get { return _pcsProductID; }
                set { _pcsProductID = value; }
            }
            public string PCSProductID2//不用了 全用 CUTProductID & CUTProductID2
            {
                get { return _pcsProductID2; }
                set { _pcsProductID2 = value; }
            }
            public string PCSProductType//不用了 全用 CUTProductID & CUTProductID2
            {
                get { return _pcsProductType; }
                set { _pcsProductType = value; }
            }
            public string PCSProductType2//不用了 全用 CUTProductID & CUTProductID2
            {
                get { return _pcsProductType2; }
                set { _pcsProductType2 = value; }
            }
            //T3 cs.chou 20150824(CUT)
            public string RejudgeCount
            {
                  get { return _rejudgeCount; }
                  set { _rejudgeCount = value; }
            }
            //T3 cs.chou 20150824(PCK)
            public string PanelGroup
            {
                  get { return _panelGroup; }
                  set { _panelGroup = value; }
            }
            //T3 cs.chou 20150824(RWT)
            public string DotRepairCount
            {
                  get { return _dotRepairCount; }
                  set { _dotRepairCount = value; }
            }
            public string LineRepairCount
            {
                  get { return _lineRepairCount; }
                  set { _lineRepairCount = value; }
            }

            //T3 cs.chou 20150818 add (TAM PTH CRP)
            public string GlassThickness
            {
                  get { return _glassThickness; }
                  set { _glassThickness = value; }
            }
            public string OperationID
            {
                  get { return _operationID; }
                  set { _operationID = value; }
            }
            public string ProductOwner
            {
                  get { return _productOwner; }
                  set { _productOwner = value; }
            }
            //T3 shihyang 20150820 add (PDR)
            public string MaxRwkCount
            {
                get { return _maxRwkCount; }
                set { _maxRwkCount = value; }
            }
            public string OQCBank
            {
                get { return _oQCBank; }
                set { _oQCBank = value; }
            }
            public string CurrentRwkCount
            {
                  get { return _currentRwkCount; }
                  set { _currentRwkCount = value; }
            }
            public string PILiquidType
            {
                  get { return _pILiquidTypet; }
                  set { _pILiquidTypet = value; }
            }
            public string DefectCode
            {
                  get { return _defectCode; }
                  set { _defectCode = value; }
            }
            public string VendorName
            {
                get { return _vendorName; }
                set { _vendorName = value; }
            }
            //T3. CUT
            public string BURCheckCount
            {
                  get { return _bURCheckCount; }
                  set { _bURCheckCount = value; }
            }
            //T3. CUT/PCS
            public string CUTCassetteSettingCode
            {
                  get { return _cUTCassetteSettingCode; }
                  set { _cUTCassetteSettingCode = value; }
            }
            //T3. PCS
            public string CUTCassetteSettingCode2
            {
                get { return _cUTCassetteSettingCode2; }
                set { _cUTCassetteSettingCode2 = value; }
            }
            //T3. CUT
            public string DisCardJudges
            {
                get { return _disCardJudges; }
                set { _disCardJudges = value; }
            }
            public string SortFlagNo
            {
                get { return _sortFlagNo; }
                set { _sortFlagNo = value; }
            }
          //Modify by zhuxingxing RwLiftTime  20160829 
            public string RwLiftTime
            {
                get { return _rwLifeTime; }
                set { _rwLifeTime = value; }
            }

          //huangjiayin 20170714 add for pcs new cutting rule..
            public string PCSCassetteSettingCodeList
            {
                get { return _pcsCassetteSettingCodeList; }
                set { _pcsCassetteSettingCodeList = value; }
            }

            //huangjiayin 20170725 add for pcs new cutting rule..
            public string PCSBlockSizeList
            {
                get { return _pcsBlockSizeList; }
                set { _pcsBlockSizeList = value; }
            }

            //huangjiayin 20170724 add for pcs new cutting rule..
            public string BlockSize1
            {
                get { return _blockSize1; }
                set { _blockSize1 = value; }
            }

            //huangjiayin 20170724 add for pcs new cutting rule..
            public string BlockSize2
            {
                get { return _blockSize2; }
                set { _blockSize2 = value; }
            }


            //For Create File Service Use
            private string _abnormalTFT = string.Empty;
            private string _abnormalCF = string.Empty;
            private string _abnormalLCD = string.Empty;
            private string _decreateFlag = string.Empty;
            private string _subProductJPSCode = string.Empty;
            private string _lcdQtapLotGroupID = string.Empty;
            private string _fGradeFlag = string.Empty;
            private string[] _defectList = new string[30] { "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" };
            private string _glassType = string.Empty;

            public string AbnormalTFT
            {
                  get { return _abnormalTFT; }
                  set { _abnormalTFT = value; }
            }
            public string AbnormalCF
            {
                  get { return _abnormalCF; }
                  set { _abnormalCF = value; }
            }
            public string AbnormalLCD
            {
                  get { return _abnormalLCD; }
                  set { _abnormalLCD = value; }
            }
            public string DeCreateFlag
            {
                  get { return _decreateFlag; }
                  set { _decreateFlag = value; }
            }
            public string SubProductJPSCode
            {
                  get { return _subProductJPSCode; }
                  set { _subProductJPSCode = value; }
            }
            public string LcdQtapLotGroupID
            {
                  get { return _lcdQtapLotGroupID; }
                  set { _lcdQtapLotGroupID = value; }
            }
            public string FGradeFlag
            {
                  get { return _fGradeFlag; }
                  set { _fGradeFlag = value; }
            }
            public string[] DefectList
            {
                  get { return _defectList; }
                  set { _defectList = value; }
            }
            public string GlassType
            {
                  get { return _glassType; }
                  set { _glassType = value; }
            }

            public string PanelSpare1
            { get { return DefectList[0] + DefectList[1]; } }

            public string PanelSpare2
            { get { return DefectList[2] + DefectList[3]; } }

            public string PanelSpare3
            { get { return DefectList[4] + DefectList[5]; } }

            public string PanelSpare4
            { get { return DefectList[6] + DefectList[7]; } }

            public string PanelSpare5
            { get { return DefectList[8] + DefectList[9]; } }

            public string PanelSpare6
            { get { return DefectList[10] + DefectList[11]; } }

            public string PanelSpare7
            { get { return DefectList[12] + DefectList[13]; } }

            public string PanelSpare8
            { get { return DefectList[14] + DefectList[15]; } }

            public string PanelSpare9
            { get { return DefectList[16] + DefectList[17]; } }

            public string PanelSpare10
            { get { return DefectList[18] + DefectList[19]; } }

            public string PanelSpare11
            { get { return DefectList[20] + DefectList[21]; } }

            public string PanelSpare12
            { get { return DefectList[22] + DefectList[23]; } }

            public string PanelSpare13
            { get { return DefectList[24] + DefectList[25]; } }

            public string PanelSpare14
            { get { return DefectList[26] + DefectList[27]; } }

            public string PanelSpare15
            { get { return DefectList[28] + DefectList[29]; } }

            public object Clone()
            {
                  JobCellSpecial cell = (JobCellSpecial)this.MemberwiseClone();
                  cell.DefectList = new string[30];
                  for (int i = 0; i < 30; i++)
                  {
                        cell.DefectList[i] = this.DefectList[i].Clone() as string;
                  }
                  return cell;
            }
      }

      //[T2 Use]
      [Serializable]
      [TypeConverter(typeof(ExpandableObjectConverter))]
      public class RealGlassCount
      {
            private string _eqpNo;
            private string _assemblyTFTGlassCnt;
            private string _notAssemblyTFTGlassCnt;
            private string _cfGlassCnt;
            private string _throughGlassCnt;
            private string _piDummyGlassCnt;
            private string _uvMaskGlassCnt;
            private bool _isReply = false;

            public string EqpNo
            {
                  get { return _eqpNo; }
                  set { _eqpNo = value; }
            }

            public string AssemblyTFTGlassCnt
            {
                  get { return _assemblyTFTGlassCnt; }
                  set { _assemblyTFTGlassCnt = value; }
            }

            public string NotAssemblyTFTGlassCnt
            {
                  get { return _notAssemblyTFTGlassCnt; }
                  set { _notAssemblyTFTGlassCnt = value; }
            }

            public string CFGlassCnt
            {
                  get { return _cfGlassCnt; }
                  set { _cfGlassCnt = value; }
            }

            public string ThroughGlassCnt
            {
                  get { return _throughGlassCnt; }
                  set { _throughGlassCnt = value; }
            }

            public string PIDummyGlassCnt
            {
                  get { return _piDummyGlassCnt; }
                  set { _piDummyGlassCnt = value; }
            }

            public string UVMaskGlassCnt
            {
                  get { return _uvMaskGlassCnt; }
                  set { _uvMaskGlassCnt = value; }
            }

            public bool IsReply
            {
                  get { return _isReply; }
                  set { _isReply = value; }
            }
      }

      //CELL Unload Dispacth Rule 與job無關，但需宣告全域
      [Serializable]
      [TypeConverter(typeof(ExpandableObjectConverter))]
      //Add By Yangzhenteng20180420  
      #region[For BUR Check] 
      public class RemoteRejudgePanel
      {
          public string Eqpid;
          public string Glassid;
          public string Portno;
          public string Slotno;
          public string Sideno;
          public string Glassjudgeresult;
          public string Productspecname;
          public string Inboxname;
          public string ReasonCode;
          public DateTime OccurDateTime { get; set; }
          public bool IsSend { get; set; }
          public bool IsFinish { get; set; }
          public string TrackKey { get; set; }
          public RemoteRejudgePanel(string eqpid, string glassid, string portno, string slotno, string sideno, string glassjudgeresult, string trackKey, string productspecname, string inboxname, string reasoncode, bool IsSend = false, bool isFinish = false)
          {
              Eqpid = eqpid;
              Glassid = glassid;
              Portno = portno;
              Slotno = slotno;
              Sideno = sideno;
              Glassjudgeresult = glassjudgeresult;
              Productspecname = productspecname;
              Inboxname = inboxname;
              ReasonCode = reasoncode;
              this.IsFinish = isFinish;
              this.IsSend = IsSend;
              OccurDateTime = DateTime.Now;
              TrackKey = trackKey;
          }
      }
      [Serializable]
      [TypeConverter(typeof(ExpandableObjectConverter))]
      #endregion
      public class clsDispatchRule
      {
            public string Grade1 = string.Empty;
            public string AbnormalCode1 = string.Empty;
            public string Grade2 = string.Empty;
            public string AbnormalCode2 = string.Empty;
            public string Grade3 = string.Empty;
            public string AbnormalCode3 = string.Empty;
            public string Grade4 = string.Empty;
            public string AbnormalCode4 = string.Empty;
            public string AbnormalFlag = string.Empty;
            public string OperatorID = string.Empty;
      }

      //CELL GroupIdex
      [Serializable]
      [TypeConverter(typeof(ExpandableObjectConverter))]
      public class clsGroupIndex
      {
            public string GroupID = string.Empty;
            public DateTime DateTime = DateTime.Now;
      }

      //CELL Box Key MES Reply not key write PLC,so BC Add Repository
      //then MES Reply Message BC Will get repository key
      [Serializable]
      [TypeConverter(typeof(ExpandableObjectConverter))]
      public class keyBoxReplyPLCKey
      {
            public const string BoxLabelInformationReply = "BoxLabelInformationReply";
            public const string PalletDataRequestReportReply = "PalletDataRequestReportReply";
            public const string PalletLabelInformationRequestReply = "PalletLabelInformationRequestReply";
            public const string DenseBoxWeightCheckReportReply = "DenseBoxWeightCheckReportReply";
            public const string DenseBoxDataRequestReply = "DenseBoxDataRequestReply";
            public const string DenseBoxIDCheckReportReply = "DenseBoxIDCheckReportReply";
            public const string ValidateMaskByCarrierReply = "ValidateMaskByCarrierReply";
            public const string POLStateChangedReply = "POLStateChangedReply";
            public const string PaperBoxDataRequestReply = "PaperBoxDataRequestReply";
            public const string BoxProcessEndReply = "BoxProcessEndReply";
            public const string PaperBoxReply = "PaperBoxReply";
            public const string LotIDCreateRequestReportReply = "LotIDCreateRequestReportReply";
            public const string LotIDCreateRequestReportReplyForPort = "LotIDCreateRequestReportReplyForPort";
            public const string BoxIDCheckRequestReply = "BoxIDCheckRequestReply";
            public const string GlassChangeLineRequestReply = "GlassChangeLineRequestReply";
            public const string PanelRequestReplyCommandNo = "PanelRequestReplyCommandNo";  //add ComandNo keyup JobDatarequest by zhuxingxing 20160906
      }

}
