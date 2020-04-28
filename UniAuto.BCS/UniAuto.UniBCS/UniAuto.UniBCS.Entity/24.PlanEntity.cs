using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UniAuto.UniBCS.Entity
{
    public class PlanConstant : EntityFile
    {
        private List<Plan> _planList = new List<Plan>();

        public List<Plan> PlanList
        {
            get { return _planList; }
            set { _planList = value; }
        }
    }

    [Serializable]
    public class Plan : EntityFile
    {
        private string _pLANNAME = string.Empty;

        private List<string> _planCompleteCst = new List<string>();

        private List<SLOTPLAN> _planCollection = new List<SLOTPLAN>();

        public string PLAN_NAME
        {
            get { return _pLANNAME; }
            set { _pLANNAME = value; }
        }

        public List<string> PlanCompleteCst
        {
            get { return _planCompleteCst; }
            set { _planCompleteCst = value; }
        }

        public List<SLOTPLAN> PlanCollection
        {
            get { return _planCollection; }
            set { _planCollection = value; }
        }
    }

    [Serializable]
    public class SLOTPLAN 
    {
        private string _pRODUCTNAME = string.Empty;
        private int _sLOTNO = 0;
        private bool _hAVEBEENUSED = false;
        private string _sOURCECASID = string.Empty;
        private string _tARGETCSTID = string.Empty;
        private string _tARGETSLOTNO = string.Empty;

        public string PRODUCT_NAME
        {
            get { return _pRODUCTNAME; }
            set { _pRODUCTNAME = value; }
        }

        public int SLOTNO
        {
            get { return _sLOTNO; }
            set { _sLOTNO = value; }
        }

        public bool HAVE_BEEN_USED
        {
            get { return _hAVEBEENUSED; }
            set { _hAVEBEENUSED = value; }
        }

        public string SOURCE_CASSETTE_ID
        {
            get { return _sOURCECASID; }
            set { _sOURCECASID = value; }
        }

        public string TARGET_CASSETTE_ID
        {
            get { return _tARGETCSTID; }
            set { _tARGETCSTID = value; }
        }

        /// <summary>
        /// 若0則表示可放入任意空Slot, 若非0則表示指定放入的Slot
        /// </summary>
        public string TARGET_SLOTNO
        {
            get { return _tARGETSLOTNO; }
            set { _tARGETSLOTNO = value; }
        }
    }
}
