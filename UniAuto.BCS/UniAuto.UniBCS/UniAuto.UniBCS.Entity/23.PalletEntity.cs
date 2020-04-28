using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UniAuto.UniBCS.Entity
{
	/// <summary>
	/// 對應File, 修改Property後呼叫Save(), 會序列化存檔
	/// </summary>
    [Serializable]
    public class PalletEntityFile : EntityFile
    {
        private ePalletMode _palletMode = ePalletMode.UNKNOWN;
        private string _palletID = string.Empty;
        private string _palletNo = string.Empty;
        private string _lineRecipeName = string.Empty;
        private List<string> _denseBoxList = new List<string>();
        private string _palletDataRequest = "0";
        private string _mes_ValidatePalletReply = string.Empty;
        private string _nodeNo = string.Empty;
        private string _palletName = string.Empty;

        public ePalletMode PalletMode
        {
            get { return _palletMode; }
            set { _palletMode = value; }
        }

        public string PalletID
        {
            get { return _palletID; }
            set { _palletID = value; }
        }

        public string PalletNo
        {
            get { return _palletNo; }
            set { _palletNo = value; }
        }

        public string LineRecipeName
        {
            get { return _lineRecipeName; }
            set { _lineRecipeName = value; }
        }
        public List<string> DenseBoxList
        {
            get { return _denseBoxList; }
            set { _denseBoxList = value; }
        }
        public string PalletDataRequest
        {
            get { return _palletDataRequest; }
            set { _palletDataRequest = value; }
        }
        public string Mes_ValidatePalletReply
        {
            get { return _mes_ValidatePalletReply; }
            set { _mes_ValidatePalletReply = value; }
        }
        public string NodeNo
        {
            get { return _nodeNo; }
            set { _nodeNo = value; }
        }
        public string PalletName
        {
            get { return _palletName; }
            set { _palletName = value; }
        }
    }

    public class Pallet : Entity
	{
		public PalletEntityFile File { get; set; }
		public Pallet(PalletEntityFile file)
		{
            File = file;
		}
	}
}
