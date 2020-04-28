using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniAuto.UniBCS.OpiSpec;

namespace UniOPI
{
    public class Unit
    {
        public string ServerName { get; set; }
        public string LineID{ get; set; }
        public string NodeID{ get; set; }
        public string UnitNo{ get; set; }
        public string UnitID{ get; set; }
        public string NodeNo{ get; set; }
        public string UnitType{ get; set; }
        public string UseRunMode { get; set; }  // N : 沒有run mode , Y: 可設定 & 須顯示 , R : 只顯示 不可設定run mode

        public string HSMSStatus { get; set; }

        #region For CELL Buffer
        public int BufferWarningCount { get; set; }  //Buffer Warning Glass Setting Count
        public int BufferCurrentCount { get; set; }  //Buffer Current Glass Count
        public int BufferTotalSlotCount { get; set; }  //Buffer Total Slot Count
        public int BufferWarningStatus { get; set; }  //1：Set ; 2：Reset
        public int BufferStoreOverAlive { get; set; }  //1：Set ; 2：Reset
        #endregion
        
        public eEQPStatus UnitStatus { get; set; } 

        public int TFTJobCount { get; set; }
        public int CFJobCount { get; set; }

        public string UnitRunMode { get; set; }

        public Unit()
        {
            UnitStatus= eEQPStatus.UnKnown;
            TFTJobCount = 0;
            CFJobCount = 0;
            UnitRunMode=string.Empty ;
        }

        public void SetUnitInfo(EquipmentStatusReply.UNITc unitData)
        {
            int _num = 0;
            CFJobCount = (int.TryParse(unitData.CFJOBCNT, out _num) == true) ? int.Parse(unitData.CFJOBCNT) : 0;
            UnitStatus = (eEQPStatus)int.Parse(unitData.CURRENTSTATUS);
            TFTJobCount = (int.TryParse(unitData.TFTJOBCNT, out _num) == true) ? int.Parse(unitData.TFTJOBCNT) : 0;
            UnitRunMode = unitData.UNITRUNMODE;
            HSMSStatus = unitData.HSMSSTATUS.ToString();

            BufferWarningCount= (int.TryParse(unitData.BF_WARNING_GLASS_COUNT, out _num) == true) ? int.Parse(unitData.BF_WARNING_GLASS_COUNT) : 0;
            BufferCurrentCount= (int.TryParse(unitData.BF_CURRENT_COUNT, out _num) == true) ? int.Parse(unitData.BF_CURRENT_COUNT) : 0;
            BufferTotalSlotCount = (int.TryParse(unitData.BF_TOTAL_SLOT_COUNT, out _num) == true) ? int.Parse(unitData.BF_TOTAL_SLOT_COUNT) : 0;
            BufferWarningStatus= (int.TryParse(unitData.BF_WARNING_STATUS, out _num) == true) ? int.Parse(unitData.BF_WARNING_STATUS) : 0;  //1：Set,2：Reset
            BufferStoreOverAlive= (int.TryParse(unitData.BF_STORE_OVER_ALIVE, out _num) == true) ? int.Parse(unitData.BF_STORE_OVER_ALIVE) : 0; //1：Set,2：Reset
        }

        public void SetUnitInfo(EquipmentStatusReport.UNITc unitData)
        {
            int _num = 0;
            this.CFJobCount = (int.TryParse(unitData.CFJOBCNT, out _num) == true) ? int.Parse(unitData.CFJOBCNT) : 0;
            this.UnitStatus = (eEQPStatus)int.Parse(unitData.CURRENTSTATUS);
            this.TFTJobCount = (int.TryParse(unitData.TFTJOBCNT, out _num) == true) ? int.Parse(unitData.TFTJOBCNT) : 0;
            UnitRunMode = unitData.UNITRUNMODE;
            HSMSStatus = unitData.HSMSSTATUS.ToString();

            BufferWarningCount = (int.TryParse(unitData.BF_WARNING_GLASS_COUNT, out _num) == true) ? int.Parse(unitData.BF_WARNING_GLASS_COUNT) : 0;
            BufferCurrentCount = (int.TryParse(unitData.BF_CURRENT_COUNT, out _num) == true) ? int.Parse(unitData.BF_CURRENT_COUNT) : 0;
            BufferTotalSlotCount = (int.TryParse(unitData.BF_TOTAL_SLOT_COUNT, out _num) == true) ? int.Parse(unitData.BF_TOTAL_SLOT_COUNT) : 0;
            BufferWarningStatus = (int.TryParse(unitData.BF_WARNING_STATUS, out _num) == true) ? int.Parse(unitData.BF_WARNING_STATUS) : 0;
            BufferStoreOverAlive = (int.TryParse(unitData.BF_STORE_OVER_ALIVE, out _num) == true) ? int.Parse(unitData.BF_STORE_OVER_ALIVE) : 0;
        }

        public void SetUnitInfo(AllDataUpdateReply.UNITc unitData)
        {
            int _num = 0;
            this.CFJobCount = (int.TryParse(unitData.CFJOBCNT, out _num) == true) ? int.Parse(unitData.CFJOBCNT) : 0;
            this.UnitStatus = (eEQPStatus)int.Parse(unitData.CURRENTSTATUS);
            this.TFTJobCount = (int.TryParse(unitData.TFTJOBCNT, out _num) == true) ? int.Parse(unitData.TFTJOBCNT) : 0;
            UnitRunMode = unitData.UNITRUNMODE;
            HSMSStatus = unitData.HSMSSTATUS.ToString();

            BufferWarningCount = (int.TryParse(unitData.BF_WARNING_GLASS_COUNT, out _num) == true) ? int.Parse(unitData.BF_WARNING_GLASS_COUNT) : 0;
            BufferCurrentCount = (int.TryParse(unitData.BF_CURRENT_COUNT, out _num) == true) ? int.Parse(unitData.BF_CURRENT_COUNT) : 0;
            BufferTotalSlotCount = (int.TryParse(unitData.BF_TOTAL_SLOT_COUNT, out _num) == true) ? int.Parse(unitData.BF_TOTAL_SLOT_COUNT) : 0;
            BufferWarningStatus = (int.TryParse(unitData.BF_WARNING_STATUS, out _num) == true) ? int.Parse(unitData.BF_WARNING_STATUS) : 0;
            BufferStoreOverAlive = (int.TryParse(unitData.BF_STORE_OVER_ALIVE, out _num) == true) ? int.Parse(unitData.BF_STORE_OVER_ALIVE) : 0;
        }
    }
}
