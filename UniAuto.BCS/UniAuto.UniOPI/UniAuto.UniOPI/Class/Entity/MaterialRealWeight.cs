//Add By Yangzhenteng For OPI Display 20180904
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniAuto.UniBCS.OpiSpec;
namespace UniOPI
{
    public class MaterialRealWeight
    {
        public string MaterialForCF01ID { get; set; }
        public string MaterialForCF01Weight { get; set; }
        public string MaterialForCF01Status { get; set; }
        public string MaterialForCF02ID { get; set; }
        public string MaterialForCF02Weight { get; set; }
        public string MaterialForCF02Status { get; set; }
        public string MaterialTK01ID { get; set; }
        public int MaterialTK01Weight { get; set; }
        public string MaterialTK02ID { get; set; }
        public int MaterialTK02Weight { get; set; }
        public string MaterialTK03ID { get; set; }
        public int MaterialTK03Weight { get; set; }
        public string MaterialTK04ID { get; set; }
        public int MaterialTK04Weight { get; set; }
        public string MaterialTK05ID { get; set; }
        public int MaterialTK05Weight { get; set; }
        public string MaterialTK06ID { get; set; }
        public int MaterialTK06Weight { get; set; }
        public string MaterialTK07ID { get; set; }
        public int MaterialTK07Weight { get; set; }
        public string MaterialTK08ID { get; set; }
        public int MaterialTK08Weight { get; set; }
        public MaterialRealWeight()
        {
            MaterialForCF01ID = string.Empty;
            MaterialForCF01Weight = string.Empty;
            MaterialForCF01Status = string.Empty;
            MaterialForCF02ID = string.Empty;
            MaterialForCF02Weight = string.Empty;
            MaterialForCF02Status = string.Empty;
            MaterialTK01ID = string.Empty;
            MaterialTK02ID = string.Empty;
            MaterialTK03ID = string.Empty;
            MaterialTK04ID = string.Empty;
            MaterialTK05ID = string.Empty;
            MaterialTK06ID = string.Empty;
            MaterialTK07ID = string.Empty;
            MaterialTK08ID = string.Empty;
        }
        public void SetMaterialRealWeightInfo(MaterialRealWeightReport MaterialData)
        {
            int _num = 0;
            this.MaterialForCF01ID = MaterialData.BODY.MaterialForCF01ID;
            this.MaterialForCF01Weight = MaterialData.BODY.MaterialForCF01Weight;
            this.MaterialForCF01Status = MaterialData.BODY.MaterialForCF01Status;
            this.MaterialForCF02ID = MaterialData.BODY.MaterialForCF02ID;
            this.MaterialForCF02Weight = MaterialData.BODY.MaterialForCF02Weight;
            this.MaterialForCF02Status = MaterialData.BODY.MaterialForCF02Status;
            this.MaterialTK01ID = MaterialData.BODY.MaterialTK01ID;
            this.MaterialTK01Weight = (int.TryParse(MaterialData.BODY.MaterialTK01Weight, out _num) == true) ? int.Parse(MaterialData.BODY.MaterialTK01Weight) : 0;
            this.MaterialTK02ID = MaterialData.BODY.MaterialTK02ID;
            this.MaterialTK02Weight = (int.TryParse(MaterialData.BODY.MaterialTK02Weight, out _num) == true) ? int.Parse(MaterialData.BODY.MaterialTK02Weight) : 0;
            this.MaterialTK03ID = MaterialData.BODY.MaterialTK03ID;
            this.MaterialTK03Weight = (int.TryParse(MaterialData.BODY.MaterialTK03Weight, out _num) == true) ? int.Parse(MaterialData.BODY.MaterialTK03Weight) : 0;
            this.MaterialTK04ID = MaterialData.BODY.MaterialTK04ID;
            this.MaterialTK04Weight = (int.TryParse(MaterialData.BODY.MaterialTK04Weight, out _num) == true) ? int.Parse(MaterialData.BODY.MaterialTK04Weight) : 0;
            this.MaterialTK05ID = MaterialData.BODY.MaterialTK05ID;
            this.MaterialTK05Weight = (int.TryParse(MaterialData.BODY.MaterialTK05Weight, out _num) == true) ? int.Parse(MaterialData.BODY.MaterialTK05Weight) : 0;
            this.MaterialTK06ID = MaterialData.BODY.MaterialTK06ID;
            this.MaterialTK06Weight = (int.TryParse(MaterialData.BODY.MaterialTK06Weight, out _num) == true) ? int.Parse(MaterialData.BODY.MaterialTK06Weight) : 0;
            this.MaterialTK07ID = MaterialData.BODY.MaterialTK07ID;
            this.MaterialTK07Weight = (int.TryParse(MaterialData.BODY.MaterialTK07Weight, out _num) == true) ? int.Parse(MaterialData.BODY.MaterialTK07Weight) : 0;
            this.MaterialTK08ID = MaterialData.BODY.MaterialTK08ID;
            this.MaterialTK08Weight = (int.TryParse(MaterialData.BODY.MaterialTK08Weight, out _num) == true) ? int.Parse(MaterialData.BODY.MaterialTK08Weight) : 0;
        }
        public void SetMaterialRealWeightInfo(AllDataUpdateReply.MaterialRealWeight MaterialData)
        {
            int _num = 0;
            this.MaterialForCF01ID = MaterialData.MaterialForCF01ID;
            this.MaterialForCF01Weight = MaterialData.MaterialForCF01Weight;
            this.MaterialForCF01Status = MaterialData.MaterialForCF01Status;
            this.MaterialForCF02ID = MaterialData.MaterialForCF02ID;
            this.MaterialForCF02Weight = MaterialData.MaterialForCF02Weight;
            this.MaterialForCF02Status = MaterialData.MaterialForCF02Status;
            this.MaterialTK01ID = MaterialData.MaterialTK01ID;
            this.MaterialTK01Weight = (int.TryParse(MaterialData.MaterialTK01Weight, out _num) == true) ? int.Parse(MaterialData.MaterialTK01Weight) : 0;
            this.MaterialTK02ID = MaterialData.MaterialTK02ID;
            this.MaterialTK02Weight = (int.TryParse(MaterialData.MaterialTK02Weight, out _num) == true) ? int.Parse(MaterialData.MaterialTK02Weight) : 0;
            this.MaterialTK03ID = MaterialData.MaterialTK03ID;
            this.MaterialTK03Weight = (int.TryParse(MaterialData.MaterialTK03Weight, out _num) == true) ? int.Parse(MaterialData.MaterialTK03Weight) : 0;
            this.MaterialTK04ID = MaterialData.MaterialTK04ID;
            this.MaterialTK04Weight = (int.TryParse(MaterialData.MaterialTK04Weight, out _num) == true) ? int.Parse(MaterialData.MaterialTK04Weight) : 0;
            this.MaterialTK05ID = MaterialData.MaterialTK05ID;
            this.MaterialTK05Weight = (int.TryParse(MaterialData.MaterialTK05Weight, out _num) == true) ? int.Parse(MaterialData.MaterialTK05Weight) : 0;
            this.MaterialTK06ID = MaterialData.MaterialTK06ID;
            this.MaterialTK06Weight = (int.TryParse(MaterialData.MaterialTK06Weight, out _num) == true) ? int.Parse(MaterialData.MaterialTK06Weight) : 0;
            this.MaterialTK07ID = MaterialData.MaterialTK07ID;
            this.MaterialTK07Weight = (int.TryParse(MaterialData.MaterialTK07Weight, out _num) == true) ? int.Parse(MaterialData.MaterialTK07Weight) : 0;
            this.MaterialTK08ID = MaterialData.MaterialTK08ID;
            this.MaterialTK08Weight = (int.TryParse(MaterialData.MaterialTK08Weight, out _num) == true) ? int.Parse(MaterialData.MaterialTK08Weight) : 0;
        }
    }
}


