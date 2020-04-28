using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniAuto.UniBCS.OpiSpec;

namespace UniOPI
{
    public class Pallet
    {
        public string PalletNo { get; set; } 
        public string PalletID { get; set; }
        public ePalletMode PalletMode { get; set; } 
        public bool PalletDataRequest { get; set; }  //是否可下貨

        public Pallet()
        {
            PalletID = string.Empty;
            PalletMode = ePalletMode.UnKnown;
            PalletDataRequest = false;
        }

        public void SetPalletInfo(AllDataUpdateReply.PALLETc PalletData)
        {
            this.PalletID = PalletData.PALLETID;
            this.PalletMode = PalletData.PALLETMODE==string.Empty ? ePalletMode.UnKnown:(ePalletMode)int.Parse(PalletData.PALLETMODE); 
            this.PalletDataRequest = PalletData.PALLETDATAREQUEST == "1" ? true : false;
        }

        public void SetPalletInfo(PalletStatusReport PalletData)
        {
            this.PalletID = PalletData.BODY.PALLETID;
            this.PalletMode = PalletData.BODY.PALLETMODE == string.Empty ? ePalletMode.UnKnown : (ePalletMode)int.Parse(PalletData.BODY.PALLETMODE);
            this.PalletDataRequest = PalletData.BODY.PALLETDATAREQUEST == "1" ? true : false;
        }

        public void SetPalletInfo(PalletStatusReply PalletData)
        {
            this.PalletID = PalletData.BODY.PALLETID;
            this.PalletMode = PalletData.BODY.PALLETMODE == string.Empty ? ePalletMode.UnKnown : (ePalletMode)int.Parse(PalletData.BODY.PALLETMODE);
            this.PalletDataRequest = PalletData.BODY.PALLETDATAREQUEST == "1" ? true : false;
        }
        
    }
}
