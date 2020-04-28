using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniAuto.UniBCS.OpiSpec;

namespace UniOPI
{
    public class Dense
    {
        public string NodeNo { get; set; }
        public string NodeID { get; set; }
        public string PortID { get; set; }
        public string PortNo { get; set; }
        public ePortEnable PortEnable { get; set; }
        public string BoxID01 { get; set; }
        public string BoxID02 { get; set; }
        public ePackingMode PackingMode { get; set; }
        public eUnpackSource UnpackSource { get; set; }
        //public string BoxID { get; set; }
        public string PaperBoxID { get; set; }
        public eBoxType BoxType { get; set; }
        public bool DenseDataRequest { get; set; }  //是否可下貨

        public Dense()
        {
            BoxID01 = string.Empty;
            BoxID02 = string.Empty;
            DenseDataRequest = false;
            PortEnable = ePortEnable.Unknown;
            PackingMode = ePackingMode.UnKnown;
            UnpackSource = eUnpackSource.UnKnown;
            //BoxID = string.Empty;
            PaperBoxID = string.Empty;
            BoxType = eBoxType.Unknown;
        }

        public void SetDenseInfo(AllDataUpdateReply.DENSEBOXc DenseData)
        {
            this.BoxID01 = DenseData.BOXID01;
            this.BoxID02 = DenseData.BOXID02;
            this.PaperBoxID = DenseData.PAPER_BOXID;

            this.DenseDataRequest = DenseData.DENSEBOXDATAREQUEST == "1" ? true : false;

            this.PortEnable =DenseData.PORTENABLEMODE==string.Empty ? ePortEnable.Unknown:(ePortEnable)int.Parse(DenseData.PORTENABLEMODE);            
            this.PackingMode = DenseData.PORTPACKINGMODE == string.Empty ? ePackingMode.UnKnown : (ePackingMode)int.Parse(DenseData.PORTPACKINGMODE);
            this.UnpackSource = DenseData.UNPACKINGSOURCE == string.Empty ? eUnpackSource.UnKnown : (eUnpackSource)int.Parse(DenseData.UNPACKINGSOURCE);
            this.BoxType = DenseData.BOXTYPE == string.Empty ? eBoxType.Unknown : (eBoxType)int.Parse(DenseData.BOXTYPE);
        }

        public void SetDenseInfo(DenseStatusReport DenseData)
        {
            this.BoxID01 = DenseData.BODY.BOXID01;
            this.BoxID02 = DenseData.BODY.BOXID02;
            this.PaperBoxID = DenseData.BODY.PAPER_BOXID;

            this.DenseDataRequest = DenseData.BODY.DENSEBOXDATAREQUEST == "1" ? true : false;

            this.PortEnable = DenseData.BODY.PORTENABLEMODE == string.Empty ? ePortEnable.Unknown : (ePortEnable)int.Parse(DenseData.BODY.PORTENABLEMODE);
            this.PackingMode = DenseData.BODY.PORTPACKINGMODE == string.Empty ? ePackingMode.UnKnown : (ePackingMode)int.Parse(DenseData.BODY.PORTPACKINGMODE);
            this.UnpackSource = DenseData.BODY.UNPACKINGSOURCE == string.Empty ? eUnpackSource.UnKnown : (eUnpackSource)int.Parse(DenseData.BODY.UNPACKINGSOURCE);
            this.BoxType = DenseData.BODY.BOXTYPE == string.Empty ? eBoxType.Unknown : (eBoxType)int.Parse(DenseData.BODY.BOXTYPE);
        }

        public void SetDenseInfo(DenseStatusReply DenseData)
        {
            this.BoxID01 = DenseData.BODY.BOXID01;
            this.BoxID02 = DenseData.BODY.BOXID02;
            this.PaperBoxID = DenseData.BODY.PAPER_BOXID;

            this.DenseDataRequest = DenseData.BODY.DENSEBOXDATAREQUEST == "1" ? true : false;

            this.PortEnable = DenseData.BODY.PORTENABLEMODE == string.Empty ? ePortEnable.Unknown : (ePortEnable)int.Parse(DenseData.BODY.PORTENABLEMODE);
            this.PackingMode = DenseData.BODY.PORTPACKINGMODE == string.Empty ? ePackingMode.UnKnown : (ePackingMode)int.Parse(DenseData.BODY.PORTPACKINGMODE);
            this.UnpackSource = DenseData.BODY.UNPACKINGSOURCE == string.Empty ? eUnpackSource.UnKnown : (eUnpackSource)int.Parse(DenseData.BODY.UNPACKINGSOURCE);
            this.BoxType = DenseData.BODY.BOXTYPE == string.Empty ? eBoxType.Unknown : (eBoxType)int.Parse(DenseData.BODY.BOXTYPE);
        }
    }



}
