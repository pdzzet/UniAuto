using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UniAuto.UniBCS.Entity
{
    [Serializable]
    public class EnergyMeter
    {
        public EnergyMeter()
        {

        }

        private string _unitID = string.Empty;
        private string _meterNo = string.Empty;
        private string _energyType = string.Empty;
        private string _refresh = string.Empty;
        private Dictionary<string, string> _energyParam = new Dictionary<string, string>();

        public string UnitID
        {
            get { return _unitID; }
            set { _unitID = value; }
        }

        public string MeterNo
        {
            get { return _meterNo; }
            set { _meterNo = value; }
        }

        public string EnergyType
        {
            get { return _energyType; }
            set { _energyType = value; }
        }

        public string Refresh
        {
            get { return _refresh; }
            set { _refresh = value; }
        }

        public Dictionary<string, string> EnergyParam
        {
            get { return _energyParam; }
            set { _energyParam = value; }
        }

    }
}
