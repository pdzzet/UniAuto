using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UniOPI
{
    public class CustButton
    {
        /// <summary>
        /// 按鈕類型:Main/Sub/Function
        /// </summary>
        public buttonType ButtonType { get; set; }
        /// <summary>
        /// 按鈕識別碼
        /// </summary>
        public string  ButtonKey{ get; set; }  
        /// <summary>
        /// 按鈕代碼
        /// </summary>
        public string  ButtonID{ get; set; }  
        /// <summary>
        /// 按鈕標題
        /// </summary>
        public string  ButtonCaption{ get; set; }
        /// <summary>
        /// 按鈕順序
        /// </summary>
        public int ButtonSequence { get; set; }
        /// <summary>
        /// 上一層的button Key
        /// </summary>
        public string ButtonParentButtonKey { get; set; }
        /// <summary>
        /// 按鈕是否要顯示
        /// </summary>
        public bool ButtonVisible { get; set; }
        /// <summary>
        /// 按鈕是否可使用
        /// </summary>
        public bool ButtonEnable { get; set; }
        /// <summary>
        /// 按鈕的圖示
        /// </summary>
        public string ButtonImage { get; set; }
        /// <summary>
        /// 按鈕對應控制項的名稱---用於function button name
        /// </summary>
        public string ButtonName { get; set; }
        /// <summary>
        /// 按鈕對應控制項的描述
        /// </summary>
        public string ButtonDesc { get; set; }
    }
    public enum buttonType
    {
        Main=1,
        Sub = 2,
        Function=3
    }    
}
