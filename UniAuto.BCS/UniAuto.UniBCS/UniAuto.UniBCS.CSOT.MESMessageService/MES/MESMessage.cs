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
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Globalization;

namespace UniAuto.UniBCS.CSOT.MESMessageService
{
    public partial class MESService
    {
        #region MES Function
        /* MES Function  Name                    Class  File Name               BC2MES Interface Name                     
         6.1.	AlarmReport                      MESService.cs             AlarmReport
         6.2.	AreYouThereRequest               MESService.cs             AreYouTherePeriodicityRequest 
                                                 MESService.cs             AreYouThereRequest
                                                 MESService.cs             AreYouThereRequest_UI
         6.3.	AreYouThereReply                 MESService.cs             MES_AreYouThereReply
         6.4.	AssembleComplete                 MESService.cs             AssembleComplete   
         6.5.	AutoDecreaseMaterialQuantity     MESService.cs             AutoDecreaseMaterialQuantity
         6.6.	BoxLabelInformationRequest       CellSpecial.cs            BoxLabelInformationRequest
         6.7.	BoxLabelInformationReply         CellSpecial.cs            MES_BoxProcessEndReply
         6.8.	BoxProcessCanceled               CellSpecial.cs            BoxProcessCanceled 
            6.9.	BoxLineOutRequest               CellSpecial.cs            BoxLineOutRequest                 
            6.10.	BoxLineOutReply                 CellSpecial.cs            MES_BoxLineOutReply
         6.11.	BoxProcessEnd                    CellSpecial.cs            BoxProcessEnd
                                                 CellSpecial.cs            BoxProcessEndByDPI
                                                 CellSpecial.cs            BoxProcessEndByDPIRemove
         6.12.	BoxProcessEndReply               CellSpecial.cs            MES_BoxProcessEndReply
         6.13.	BoxProcessLineRequest            CellSpecial.cs            BoxProcessLineRequest
         6.14.	BoxProcessLineReply              CellSpecial.cs            MES_BoxProcessLineReply
         6.15.	BoxProcessStarted                CellSpecial.cs            BoxProcessStarted
                                                 CellSpecial.cs            BoxProcessStarted_PPK
                                                 CellSpecial.cs            BoxProcessStartedDPI
         6.16.	BoxTargetPortChanged             CellSpecial.cs            BoxTargetPortChanged
            6.17.	CassetteCleanEnd                MESService.cs             CassetteCleanEnd
         6.18.	CassetteOperModeChanged          MESService.cs             CassetteOperModeChanged
         6.19.	CFShortCutPermitRequest          CFSpecial.cs              CFShortCutPermitRequest
         6.20.	CFShortCutPermitReply            CFSpecial.cs              MES_CFShortCutPermitReply
         6.21.	CFShortCutGlassProcessEnd        CFSpecial.cs              CFShortCutGlassProcessEnd
         6.22.	CFShortCutGlassProcessEndReply   CFSpecial.cs              MES_CFShortCutGlassProcessEndReply
         6.23.	CFShortCutModeChangeRequest      CFSpecial.cs              CFShortCutModeChangeRequest
         6.24.	CFShortCutModeChangeReply        CFSpecial.cs              MES_CFShortCutModeChangeReply
            6.25.	ChamberRunModeChanged           EquipmentPort.cs          ChamberRunModeChanged
         6.26.	ChangeMaterialLifeReport         MTLKeyParts.cs            ChangeMaterialLife
            6.27.	ChangePlanAborted               ChangePlan.cs             ChangePlanAborted    
            6.28.	ChangePlanCanceled              ChangePlan.cs             ChangePlanCancled
         6.29.	ChangePlanRequest                ChangerPlan.cs            ChangePlanRequest
         6.30.	ChangePlanReply                  ChangerPlan.cs            MES_ChangePlanReply
         6.31.	ChangePlanStarted                ChangerPlan.cs            ChangePlanStarted
         6.32.	ChangePVDMaterialLife            ArraySpecial.cs           ChangePVDMaterialLife
         6.33.	ChangeTargetLife                 MESServiceSpecial.cs      ChangeTargetLife
         6.34.	CheckBoxNameRequest              CellSpecial.cs            CheckBoxNameRequest
         6.35.	CheckBoxNameReply                CellSpecial.cs            MES_CheckBoxNameReply
         6.36.	CheckLocalPPIDRequest            Recipe.cs                 CheckLocalPPIDRequest
         6.37.	CheckLocalPPIDReply              Recipe.cs                 MES_CheckLocalPPIDReply
         6.38.	CheckRecipeParameter             Recipe.cs                 CheckRecipeParameter
         6.39.	CheckRecipeParameterReply        Recipe.cs                 MES_CheckRecipeParameterReply
         6.40.	ChangeTankReport                 MESServiceSpecial.cs      ChangeTankReport
         6.41.	CIMMessageSend                   CIMMessageSend.cs         MES_CIMMessageSend
         6.42.	CurrentDateTime                  MESService.cs             MES_CurrentDateTime
         6.43.	CutComplete                      CellSpecial.cs            CutComplete
         6.44.	DefectCodeReportByGlass          MESServiceSpecial.cs      DefectCodeReportByGlass
         6.45.	FacilityCriteriaSend             Facility.cs               MES_FacilityCriteriaSend
         6.46.	FacilityParameterRequest         Facility.cs               MES_FacilityParameterRequest
         6.47.	FacilityParameterReply           Facility.cs               FacilityParameterReply
         6.48.	FacilityCheckReport              Facility.cs               FacilityCheckReport
         6.49.	FacilityCheckRequest             Facility.cs               MES_FacilityCheckRequest
         6.50.	FacilityCheckReply               Facility.cs               FacilityCheckReply
         6.51.	GlassChangeMACOJudge             MESService02.cs           GlassChangeMACOJudge
         6.52.	GlassProcessStarted              MESService.cs             GlassProcessStarted
         6.53.	GlassProcessLineChanged          MESService.cs             GlassProcessLineChanged
            6.54.	GlassReworkJudgeReport           MESService.cs             GlassReworkJudgeReport
         6.55.	IndexerOperModeChanged           MESService.cs             IndexerOperModeChanged
         6.56.	InspectionModeChanged            MESService.cs             InspectionModeChanged
         6.57.	LineLinkChanged                  CellSpecial.cs            LineLinkChanged
         6.58.	LineStateChanged                 MESService.cs             LineStateChanged
         6.59.	LotProcessAbnormalEnd            LotProcessAbnormalEnd.cs  LotProcessAbnormalEnd
         6.60.	LotProcessAbnormalEndReply       LotProcessAbnoramlEnd.cs  MES_LotProcessAbnormalEndReply
         6.61.	LotProcessAborted                MESService.cs             LotProcessAborted
         6.62.	LotProcessCanceled               MESService.cs             LotProcessCanceled
         6.63.	LotProcessEnd                    LotProcessEnd.cs          LotProcessEnd
         6.64.	LotProcessEndReply               LotprocessEnd.cs          MES_LotProcessEndReply
         6.65.	LotProcessStarted                MESService.cs             LotProcessStarted
            6.66.	LotProcessStartRequest           MESService.cs             LotProcessStartRequest
            6.67.	LotProcessStartReply             MESService.cs             MES_LotProcessStartReply
         6.68.	MachineControlStateChanged       EquipmentPort.cs          MachineControlStateChanged
                                                 EquipmentPort.cs          MachineControlStateChanged_FirstRun
         (delete)6.69.	MachineModeChanged       EquipmentPort.cs          MachineModeChanged
         6.70.	MachineModeChangeRequest         EquipmentPort.cs          MachineModeChangeRequest
         6.71.	MachineModeChangeReply           EquipmentPort.cs          MES_MachineModeChangeReply
         (delete)6.72.	MachinePauseCommandRequest  EquipmentPort.cs       MES_MachinePauseCommandRequest
         (delete)6.73.	MachinePauseCommandReply    Equipmentport.cs          MachinePauseCommandReply
         (delete)6.74.	MachineSampleRateChangeReport   Equipmentport.cs    MachineSampleRateChangeReport
         6.75.	MachineStateChanged              EquipmentPort.cs          MachineStateChanged
         (delete)6.76.	MaskCleanerIn
         (delete)6.77.	MaskCleanerOut
         6.78.	MaskProcessEnd                   ValidateMask.cs           MaskProcessEnd_MCL        
                                                 ValidateMask.cs           MaskProcessEnd_UVA
                                                 ValidateMask.cs           MaskProcessEndAbort
         6.79.	MaskStateChanged                 MESServiceSpecial.cs      MaskStateChanged
                                                 MESServiceSpecial.cs      MaskStateChanged_OnLine
                                                 MESServiceSpecial.cs      MaskStateChanged_OnLine_TBPHL
         6.80.	MaskStateChangedReply            MESServiceSpecial.cs      MES_MaskStateChangedReply
         (t2 not use)6.81.	MachineSetupFlagChanged          
         6.82.	MachineSiteChangeRequest         EquipmentPort.cs          MachineSiteChangeRequest
         6.83.	MachineSiteChangeReply           EquipmentPort.cs          MES_MachineSiteChangeReply
         6.84.	MaskLocationChanged              MESServiceSpecial.cs      MaskLocationChanged
         6.85.	MaskLocationChangedReply         MESServiceSpecial.cs      MES_MaskLocationChangedReply
         6.86.	MaskUsedCountReport              MESServiceSpecial.cs      MaskUsedCountReport
         6.87.	MaterialConsumableReport         MTLKeyParts.cs            MaterialConsumableRequest   
            6.88.	MaterialMount                   MTLKeyParts.cs            MaterialMount    
            6.89.	MaterialMountReply              MTLKeyParts.cs            MES_MaterialMountReply
            6.90.	MaterialDismountReport          MTLKeyParts.cs            MaterialDismountReport
            6.91.	MaterialWeightReport            MTLKeyParts.cs            MaterialWeightReport
         6.92.	MaterialStateChanged             MTLKeyParts.cs            MaterialStateChanged
                                                 MTLKeyParts.cs            MaterialStateChanged
                                                 MTLKeyParts.cs            MaterialStateChanged_OnLine
         6.93.	MaterialStateChangedReply        MTLKeyParts.cs            MES_MaterialStateChangedReply
         6.94.	MachineControlStateChangeRequest EquipmentPort.cs          MES_MachineControlStateChangeRequest
         6.95.	MachineControlStateChangeReply   EquipmentPort.cs          MachineControlStateChangeReply
         6.96.	MachineDataUpdate                EquipmentPort.cs          MachineDataUpdate
         6.97.	MaxCutGlassProcessEnd            CellSpecial.cs            MaxCutGlassProcessEnd
         6.98.	MaxCutGlassProcessEndReply       CellSpecial.cs            MES_MaxCutGlassProcessEndReply
         6.99.	MachineInspectionOverRatio       EquipmentPort.cs          MachineInspectionOverRatio
         6.100.	MachineLoginReport               EquipmentProt.cs          MachineLoginReport
            6.101.	PanelInformationRequest         CellSpecial.cs            PanelInformationRequest    
            6.102.	PanelInformationReply           CellSpecial.cs            MES_PanelInforamtionReply
         6.103.	PalletLabelInformationRequest    CellSpecial.cs            PalletLabelInformationRequest
         6.104.	PalletLabelInformationReply      CellSpecial.cs            MES_PalletLabelInformationReply
         6.105.	PalletProcessCanceled            CellSpecial.cs            PalletProcessCanceled
         6.106.	PalletProcessEnd                 CellSpecial.cs            PalletProcessEnd
                                                 CellSpecial.cs            PalletProcessEnd
         6.107.	PalletProcessEndReply            CellSpecial.cs            MES_PalletProcessEndReply
         6.108.	PalletProcessStarted             CellSpecial.cs            PalletProcessStarted
                                                 CellSpecial.cs            PalletProcessStarted
         6.109.	PanleReportByGlass               CellSpecial.cs            PanelReportByGlass
         (delete)6.110.	POLAttachComplete        CellSpecial.cs            POLAttachComplete
         (delete)6.111.	POLStateChanged          CellSpecial.cs            POLStateChanged
                                                 CellSpecial.cs            POLStateChanged_Online
         (delete)6.112.	POLStateChangedReply     CellSpecial.cs            MES_POLStateChangedReply
         6.113.	PortAccessModeChanged            EquipmentPort.cs          PortAccessModeChanged
                                                 EquipmentPort.cs          PortAccessModeChanged
         6.114.	PortCarrierSetCodeChanged        EquipmentPort.cs          PortCarrierSetCodeChanged
         6.115.	PortDataUpdate                   EquipmentPort.cs          PortDataUpdate
         6.116.	PortEnableChanged                EpuipmentPort.cs          PortEnableChanged
                                                 EpuipmentPort.cs          PortEnableChanged
                                                 EpuipmentPort.cs          PortEnableChangedByPMT
         6.117.	PortOperModeChanged              EpuipmentPort.cs          PortOperModeChanged
         6.118.	PortOperModeChangeRequest        EpuipmentPort.cs          PortOperModeChangeRequest
         6.119.	PortOperModeChangeReply          EpuipmentPort.cs          MES_PortOperModeChangeReply
         6.120.	PortTransferStateChanged         EpuipmentPort.cs          PortTransferStateChanged
                                                 EpuipmentPort.cs          PortTransferStateChanged
         6.121.	PortTypeChanged                  EpuipmentPort.cs          PortTypeChanged
                                                 EpuipmentPort.cs          PortTypeChanged
         6.122.	PortUseTypeChanged               EpuipmentPort.cs          PortUseTypeChanged
                                                 EpuipmentPort.cs          PortUseTypeChanged
         6.123.	PreCheckRecipeParameterRequest   Recipe.cs                 MES_PreCheckRecipeParameterRequest
         6.124.	PreCheckRecipeParameterReply     Recipe.cs                 PreCheckRecipeParameterReplyNG
                                                 Recipe.cs                 PreCheckRecipeParameterReplyNG
            6.125.	ProductLineIn                   MESService.cs             ProductLineIn
            6.126.	ProductLineOut                  MESService.cs             ProductLineOut
         6.127.	ProductIn                        MESService.cs             ProductIn
         6.128.	ProductOut                       MESService.cs             ProductOut
         6.129.	ProductProcessData               MESService02.cs           ProductProcessData
         6.130.	ProductScrapped                  MESService.cs             ProductScrapped
         6.131.	ProductUnscrapped                MESService.cs             ProductUnscrapped
            6.132.	QtimeOverReport                  MESService.cs             QtimeOverReport
         6.133.	QtimeSetChanged                  MESService.cs             QtimeSetChanged
                                                 MESService.cs             QtimeSetChanged_OnLine
         6.134.	RecipeParameterChangeRequest     Recipe.cs                 RecipeParameterChangeRequest
         6.135.	RecipeParameterChangeReply       Recipe.cs                 MES_RecipeParameterChangeReply
         6.136.	RecipeParameterRequest           Recipe.cs                 MES_RecipeParameterRequest
         6.137.	RecipeParameterReply             Recipe.cs                 RecipeParameterReply
                                                 Recipe.cs                 RecipeParameterReplyNG
         6.138.	RecipeRegisterRequest            Recipe.cs                 MES_RecipeRegisterRequest
         (only for T1 not use)6.139.	RecipeRegisterRequestReply       Recipe.cs                 RecipeRegisterRequestReply
                                                 Recipe.cs                 RecipeRegisterRequestReplyNG
         6.140.	RecipeIDRegisterCheckRequest     Recipe.cs                 MES_RecipeIDRegisterCheckRequest
                                                 Recipe.cs                 MES_RecipeIDRegisterCheckRequest2
         6.141.	RecipeIDRegisterCheckReply       Recipe.cs                 RecipeIDRegisterCheckReply
            6.142.	RuncardIdCreateRequest           MESService.cs             RuncardIdCreateRequest
            6.143.	RuncardIdCreateReply             MESService.cs             MES_RuncardIdCreateReply
            6.144.	RuncardLableInformationRequest   MESServicecs              RuncardLableInformationRequest
            6.145.	RuncardLableInformationReply     MESService.cs             MES_RuncardLableInformationReply
         (已mark)6.146.	TerminalMessageSend              MESService.cs             MES_TerminalMessageSend
         6.147.	UnitStateChanged                 MESService.cs             UnitStateChanged
         6.148.	ValidateBoxRequest               ValidateBox.cs            ValidateBoxRequest
                                                 ValidateBox.cs            ValidateBoxRequest
         6.149.	ValidateBoxReply                 ValidateBox.cs            MES_ValidateBoxReply
                                                 ValidateBox.cs            MES_ValidateBoxReply_DenseBoxDataRequest
         6.150.	ValidateBoxWeightRequest         CellSpecial.cs            ValidateBoxWeightRequest
         6.151.	ValidateBoxWeightReply           CellSpecial.cs            MES_ValidateBoxWeightReply
            6.152.	ValidateCleanCassetteRequest     MESService.cs              ValidateCleanCassetteRequest
            6.153.	ValidateCleanCassetteReply       MESServece.cs              MES_ValidateCleanCassetteReply
         6.154.	ValidateCassetteRequest          ValidateCassette.cs       ValidateCassetteRequest
         6.155.	ValidateCassetteReply            ValidateCassette.cs       MES_ValidateCassetteReply
         6.156.	ValidateGlassRequest             MESService.cs             ValidateGlassRequest
         6.157.	ValidateGlassReply               MESService.cs             MES_ValidateGlassReply
         6.158.	ValidateMaskPrepareRequest       MESService.cs             ValidateMaskPrepareRequest
         6.159.	ValidateMaskPrepareReply         MESService02.cs           MES_ValidateMaskPrepareReply
         6.160.	ValidateMaskRequest              ValidateMask.cs           ValidateMaskRequest
         6.161.	ValidateMaskReply                ValidateMask.cs           MES_ValidateMaskReply
         6.162.	ValidateMaskByCarrierRequest     ValidateMask.cs           ValidateMaskByCarrierRequest
                                                 ValidateMask.cs           ValidateMaskByCarrierRequest_HVA2
         6.163.	ValidateMaskByCarrierReply       ValidateMask.cs           MES_ValidateMaskByCarrierReply
                                                 ValidateMask.cs           MES_ValidateMaskByCarrierReply_HVA2
         6.164.	ValidatePalletRequest            CellSpecial.cs            ValidatePalletRequest
         6.165.	ValidatePalletReply              CellSpecial.cs            MES_ValidatePalletReply                                        
         6.166.	VCRReadReport                    MESService.cs             VCRReadReport
         6.167.	VCRStateChanged                  MESServiceSpecial.cs      VCRStateChanged
         6.168.	UVMaskUseCount                   MESService.cs             UVMaskUseCount
         6.180  CIMModeChangeReport              ArraySpecial.cs           CIMModeChangeReport
         * 
         ~~~~~~~~~~~~~~~~~~~~~~~~~
         6.11.	BoxIdCreateRequest               CellSpecial.cs            BoxIdCreateRequest
         6.12.	BoxIdCreateReply                 CellSpecial.cs            BoxIdCreateReply
         6.104. PalletIdCreateRequest            CellSpecial.cs            PalletIdCreateRequest
         6.105. PalletIdCreateReply              CellSpecial.cs            MES_PalletIdCreateRequest
         6.102.	OutBoxProcessEnd                 CellSpecial.cs            OutBoxProcessEnd
        */
        #endregion
    }
}
