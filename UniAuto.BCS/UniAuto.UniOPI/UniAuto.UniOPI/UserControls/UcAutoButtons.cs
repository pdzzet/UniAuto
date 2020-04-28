using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Forms = System.Windows.Forms;

namespace UniOPI
{
    public partial class UcAutoButtons : UserControl
    {
        private Dictionary<int, List<Forms.Button>> dicFlpButtons = new Dictionary<int, List<Forms.Button>>();
        private int iButtonPage = 1;
        private int BtnWidth = 40;
        private int BtnHeight = 40;
        private bool IsCustom = false;
        public FlowDirection FlowDirectionType = FlowDirection.TopDown;

        public UcAutoButtons()
        {
            InitializeComponent();
        }

        private void btnLeft_Click(object sender, EventArgs e)
        {
            if (dicFlpButtons.Count == 0) return;
            if (!dicFlpButtons.ContainsKey(iButtonPage - 1)) return;

            iButtonPage -= 1;
            SetButton(ref flpButton);
        }

        private void btnRight_Click(object sender, EventArgs e)
        {
            if (dicFlpButtons.Count == 0) return;
            if (!dicFlpButtons.ContainsKey(iButtonPage + 1)) return;

            iButtonPage += 1;
            SetButton(ref flpButton);
        }

        #region Method

        public void CreateButton(List<ButtonInfo> lstButtonInfo)
        {
            dicFlpButtons.Clear();
            iButtonPage = 1;

            flpButton.FlowDirection = FlowDirectionType;
            dicFlpButtons = GetButtons(flpButton, lstButtonInfo);
            SetButton(ref flpButton);
        }
        public void CreateButton(List<ButtonInfo> lstButtonInfo, int iButtonWidth, int iButtonHeight)
        {
            BtnWidth = iButtonWidth;
            BtnHeight = iButtonHeight;
            IsCustom = true;

            CreateButton(lstButtonInfo);
        }

        private void SetButton(ref FlowLayoutPanel flpMain)
        {
            flpMain.Controls.Clear();

            if (dicFlpButtons.Count == 0) return;
            if (!dicFlpButtons.ContainsKey(iButtonPage)) return;
            if (dicFlpButtons.Count == 1)
            {
                this.btnLeft.Visible = false;
                this.btnRight.Visible = false;
            }
            else
            {
                this.btnLeft.Visible = true;
                this.btnRight.Visible = true;
            }
            foreach (Forms.Button btn in dicFlpButtons[iButtonPage])
            {
                flpMain.Controls.Add(btn);
            }
            flpMain.FlowDirection = FlowDirection.TopDown;
        }

        private Dictionary<int, List<Forms.Button>> GetButtons(FlowLayoutPanel flpMain, List<ButtonInfo> lstButtonInfo)
        {
            Dictionary<int, List<Forms.Button>> dicButtons = new Dictionary<int, List<Forms.Button>>();

            List<Forms.Button> lstButton = new List<Forms.Button>();

            //不是自訂的則按鈕的寬度跟父原件寬度一樣
            if (!IsCustom) BtnWidth = flpMain.Width;

            int iRes = 0;
            int iWCount = Math.DivRem(flpMain.Width, BtnWidth, out iRes);

            int iIndex = 1;
            int iHeightNow = 0;
            int iWCountNow = 0;
            foreach (ButtonInfo btnInfo in lstButtonInfo)
            {
                if (iHeightNow + BtnHeight > flpMain.Height)
                {
                    dicButtons.Add(iIndex, lstButton);

                    iIndex += 1;
                    iHeightNow = 0;
                    lstButton = new List<Forms.Button>();
                }

                Forms.Button btn = new Forms.Button();
                if (!string.IsNullOrEmpty(btnInfo.ButtonName))
                    btn.Name = btnInfo.ButtonName;
                btn.Text = btnInfo.ButtonText;
                btn.Width = BtnWidth;
                btn.Height = BtnHeight;
                btn.Margin = new System.Windows.Forms.Padding(0);
                btn.Font = new Font("Calibri", 12);
                if (btnInfo.ButtonClick != null)
                    btn.Click += new EventHandler(btnInfo.ButtonClick);

                lstButton.Add(btn);
                iWCountNow += 1;
                if (iWCountNow >= iWCount)
                {
                    iWCountNow = 0;
                    iHeightNow += BtnHeight;
                }
            }
            dicButtons.Add(iIndex, lstButton);

            return dicButtons;
        }

        #endregion

        public class ButtonInfo
        {
            public ButtonInfo() { }
            public ButtonInfo(string strButtonName, string strButtonText, EventHandler evnButtonClick)
            {
                ButtonName = strButtonName;
                ButtonText = strButtonText;
                ButtonClick = evnButtonClick;
            }

            public string ButtonName { get; set; }
            public string ButtonText { get; set; }
            public EventHandler ButtonClick { get; set; }
        }
    }
}
