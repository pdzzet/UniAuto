using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace UniOPI
{
    public partial class FormTimingChart : FormBase
    {
        string CharName = string.Empty;
        string FileName = string.Empty;
        Bitmap _image = null;

        public FormTimingChart(string charName)
        {
            InitializeComponent();
            lblCaption.Text = "Timing Chart";

            CharName = charName;

            FileName = OPIConst.TimingChartFolder + string.Format(charName + ".png");
        }
 

        private void btnClose_Click(object sender, EventArgs e)
        {
            if (_image != null)
                _image.Dispose();//家成偷改:釋放GDI資源
            this.Close();
        }

        private void FormTimingChart_Load(object sender, EventArgs e)
        {
            try
            {
                if (CharName == string.Empty) return;

                if (File.Exists(FileName))
                {
                    _image = new Bitmap(FileName);
                    picChart.BackgroundImage = _image;
                }
                else picChart.BackgroundImage = null;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }
    }
}
