using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace UniOPI
{
    public partial class FormCassetteControl_JobExistenceSlot : FormBase
    {
        Port CurPort=null;

        public FormCassetteControl_JobExistenceSlot(Port _port)
        {
            InitializeComponent();

            CurPort = _port;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void FormCassetteControl_JobExistenceSlot_Load(object sender, EventArgs e)
        {
            try
            {
                string _slotNo = string.Empty;
                string _jobExistSlot = CurPort.JobExistenceSlot.ToString();
                bool _jobExist = false;

                txtPortID.Text = CurPort.PortID;
                txtJobExistSlot.Text  = _jobExistSlot;
                txtSlotGlassCount.Text = CurPort.PortGlassCount.ToString();

                for (int i = 0; i < _jobExistSlot.Length; i++)
                {
                    Label _lbl = new Label();

                    _slotNo = (i + 1).ToString().PadLeft(3, '0');

                    _lbl.Name = _slotNo;
                    _lbl.Text = _slotNo;
                    _lbl.AutoSize=false ;
                    _lbl.Font = new Font("Calibri", 12f, FontStyle.Regular);
                    _lbl.Size = new Size(60, 20);
                    _lbl.TextAlign = ContentAlignment.MiddleCenter;
                    _lbl.BorderStyle = BorderStyle.Fixed3D;
                    _lbl.Margin = new System.Windows.Forms.Padding(3);
                    flpExist.Controls.Add(_lbl);

                    _jobExist = (_jobExistSlot.Substring(i, 1) == "1" ? true : false);

                    if (_jobExist) _lbl.BackColor = Color.Lime;
                    else _lbl.BackColor = Color.Silver;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }
    }
}
