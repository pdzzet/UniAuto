using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UniOPI
{
    public class BCS_ProductTypeInfoRequestReply
    {
        public bool IsReply { get; set; }  //判斷SlotPositionReply是否已回復

        public DateTime LastRequestDate { get; set; } //最後發送request的時間             

        public Dictionary<string, ProductTypeInfo> Dic_ProductTypeInfo { get; set; }  //// Key: NODENO

        public BCS_ProductTypeInfoRequestReply()
        {
            IsReply = true;
 
            LastRequestDate = Convert.ToDateTime("2010-01-01 00:00:00");

            Dic_ProductTypeInfo = new Dictionary<string, ProductTypeInfo>();
        }
    }

    public class ProductTypeInfo
    {
        public string NodeNo { get; set; }
        public string ProductType { get; set; }
        public string ProductType_Unit01 { get; set; }
        public string ProductType_Unit02 { get; set; }
        public string ProductType_Unit03 { get; set; }
        public string ProductType_Unit04 { get; set; }
        public string ProductType_Unit05 { get; set; }
        public string ProductType_Unit06 { get; set; }
        public string ProductType_Unit07 { get; set; }
        public string ProductType_Unit08 { get; set; }

        public ProductTypeInfo()
        {
            NodeNo = string.Empty;
            ProductType = string.Empty;
            ProductType_Unit01 = string.Empty;
            ProductType_Unit02 = string.Empty;
            ProductType_Unit03 = string.Empty;
            ProductType_Unit04 = string.Empty;
            ProductType_Unit05 = string.Empty;
            ProductType_Unit06 = string.Empty;
            ProductType_Unit07 = string.Empty;
            ProductType_Unit08 = string.Empty;
        }
    }
}
