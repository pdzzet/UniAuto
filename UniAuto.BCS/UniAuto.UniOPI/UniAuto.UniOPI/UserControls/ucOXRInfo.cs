using System;
using System.Windows.Forms;

namespace UniOPI
{
    public partial class ucOXRInfo : UserControl
    {
        int Seq = 0;
        int startPoint = 0;
        int endPoing = 0;

        int MaxLen = 0;

        public int StartPoint
        {
            get { return startPoint; }
        }

        public int EndPoing
        {
            get { return endPoing; }
        }

        public string GetOXR()
        {
            if (txtChipData.Text.Length != txtChipData.MaxLength)
            {
                txtChipData.Text = "0".PadLeft(txtChipData.MaxLength, '0');
            }

            return txtChipData.Text.ToString();
        }

        public ucOXRInfo(int seq, int maxLen)
        {
            InitializeComponent();

            Seq = seq;
            MaxLen = maxLen;

            startPoint = ( seq * 10 ) + 1;

            endPoing = startPoint + maxLen - 1;

            txtChipData.MaxLength = MaxLen;

            lblChipName.Text = string.Format("{0} ~ {1}", startPoint.ToString(), endPoing.ToString());
        }       


        public void InitialOXR(string oxrInfo,char flag)
        {
            try
            {
                if (oxrInfo == string.Empty)
                {
                    txtChipData.Text = string.Empty.PadLeft(MaxLen, flag);
                }
                else
                {
                    txtChipData.Text = oxrInfo.Substring(startPoint - 1, MaxLen);
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

    }
}
