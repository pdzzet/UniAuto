using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.ComponentModel;

namespace UniAuto.UniBCS.Entity
{
    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]  
    public class Qtimec:ICloneable
    {
        public string QTIMEID {get;set;}

        public string LINENAME { get; set; }

        public string STARTMACHINE { get; set; }

        public string STARTUNITS { get; set; }

        public string STARTEVENT { get; set; }

        public string RECIPEID { get; set; }

        public string ENDMACHINE { get; set; }

        public string ENDUNITS { get; set; }

        public string ENDEVENT { get; set; }

        public string QTIMEVALUE { get; set; } //

        public string CFRWQTIME { get; set; } //CF專用

        public bool OVERQTIMEFLAG { get; set; }
        public DateTime STARTQTIME { get; set; }
        public DateTime ENDQTIME { get; set; }
        public bool STARTQTIMEFLAG { get; set; }
        public bool CFRWQTIMEFLAG { get; set; } //CF專用
        public bool CFQTIMEFLAG { get; set; }//add by hujunpeng 20190130
        public bool ENABLED { get; set; } //QTIME啟用
        public string REMARK { get; set; }//add by hujunpeng 20190130
        public Qtimec()
        {
            QTIMEID = string.Empty;
            LINENAME = string.Empty;
            STARTMACHINE = string.Empty;
            STARTUNITS = string.Empty;
            STARTEVENT = string.Empty;
            RECIPEID = string.Empty;
            ENDMACHINE = string.Empty;
            ENDUNITS = string.Empty;
            ENDEVENT = string.Empty;
            QTIMEVALUE = string.Empty;
            CFRWQTIME = string.Empty;
            OVERQTIMEFLAG = false;
            STARTQTIMEFLAG = false;
            ENABLED = false;
            REMARK = string.Empty;
            CFQTIMEFLAG = false;
        }

        public object Clone()
        {
            Qtimec qt = (Qtimec)this.MemberwiseClone();
            return qt;
        }
    }
   
}