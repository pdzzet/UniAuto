using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UniOPI
{
    public class BCS_StagePositionInfoReply
    {
        public bool IsReply { get; set; }  //判斷SlotPositionReply是否已回復
        //public bool IsDisplay { get; set; } //是否為開啟狀態

        public bool SendReady { get; set; }
        public bool ReceiveReady { get; set; }
        public bool ExchangePossible { get; set; }
        public bool DoubleGlassExist { get; set; }

        public string NodeNo { get; set; }

        public DateTime LastReceiveMsgDateTime { get; set; } //最後發送request的時間  

        public BCS_StagePositionInfoReply()
        {
            //IsDisplay = true;
            IsReply = true;

            SendReady = false;
            ReceiveReady = false;
            ExchangePossible = false;
            DoubleGlassExist = false;

            NodeNo = string.Empty;

            LastReceiveMsgDateTime = Convert.ToDateTime("2010-01-01 00:00:00");
        }
    }
}
