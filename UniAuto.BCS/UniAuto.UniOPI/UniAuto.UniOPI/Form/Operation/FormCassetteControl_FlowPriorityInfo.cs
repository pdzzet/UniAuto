using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace UniOPI
{
    public partial class FormCassetteeControl_FlowPriorityInfo : FormBase
    {

        public string FlowPriorityInfo = string.Empty;

        public FormCassetteeControl_FlowPriorityInfo(string defaultInfo)
        {
            InitializeComponent();

            if (defaultInfo.Trim() != string.Empty) FlowPriorityInfo = defaultInfo.Trim();
        }

        private void FormCassetteeControl_FlowPriorityInfo_Load(object sender, EventArgs e)
        {
            try
            {
                string _nodeNo = string.Empty;

                _nodeNo = "00";
                if (FlowPriorityInfo.Length >= 2) _nodeNo = GetLocalName(FlowPriorityInfo.Substring(0, 2));
                InitialCombox(cboFlowPriority1, _nodeNo);

                _nodeNo = "00";
                if (FlowPriorityInfo.Length >= 4) _nodeNo = GetLocalName(FlowPriorityInfo.Substring(2, 2));
                InitialCombox(cboFlowPriority2, _nodeNo);

                _nodeNo = "00";
                if (FlowPriorityInfo.Length >= 6) _nodeNo = GetLocalName(FlowPriorityInfo.Substring(4, 2));
                InitialCombox(cboFlowPriority3, _nodeNo);

                if (cboFlowPriority1.Items.Count < 4)
                {
                    lblFlowPriority3.Visible = false;
                    cboFlowPriority3.Visible = false;
                }

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void Button_Click(object sender, EventArgs e)
        {           
            try
            {
                Button _btn = (Button)sender;

                switch (_btn.Name)
                {
                    case "btnOK":

                        string _flowPriority1 = "00";
                        string _flowPriority2 = "00";
                        string _flowPriority3 = "00";

                        if (cboFlowPriority1.SelectedValue != null) _flowPriority1 = cboFlowPriority1.SelectedValue.ToString();
                        if (cboFlowPriority2.SelectedValue != null) _flowPriority2 = cboFlowPriority2.SelectedValue.ToString();
                        if (cboFlowPriority3.SelectedValue != null) _flowPriority3 = cboFlowPriority3.SelectedValue.ToString();

                        if (_flowPriority1 == _flowPriority2 && _flowPriority1 != "00")
                        {
                            ShowMessage(this, lblCaption.Text, "", "1'st Priority of Local No and 2'st Priority of Local No must be different", MessageBoxIcon.Warning);
                            return;
                        }

                        if (_flowPriority1 == _flowPriority3 && _flowPriority1 != "00")
                        {
                            ShowMessage(this, lblCaption.Text, "", "1'st Priority of Local No and 3'st Priority of Local No must be different", MessageBoxIcon.Warning);
                            return;
                        }

                        if (_flowPriority2 == _flowPriority3 && _flowPriority2 != "00")
                        {
                            ShowMessage(this, lblCaption.Text , "", "2'st Priority of Local No and 3'st Priority of Local No must be different", MessageBoxIcon.Warning);
                            return;
                        }

                        FlowPriorityInfo = GetLocalNo(_flowPriority1) + GetLocalNo(_flowPriority2) + GetLocalNo(_flowPriority3);

                        this.DialogResult = System.Windows.Forms.DialogResult.OK;

                        break;

                    case "btnCancel":

                        FlowPriorityInfo=string.Empty ;

                        this.DialogResult = System.Windows.Forms.DialogResult.Cancel;

                        break;

                    default :
                        break;
                }

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }

        private string GetLocalName(string Data)
        {
            try
            {
                if (Data == "00") return "00";

                int _num = 0;

                int.TryParse(Data, out _num);

                if (_num == 0) return "00";

                return string.Format("L{0}", _num.ToString());
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
                
                return "00";
            }
        }

        private string GetLocalNo(string LocalNo)
        {
            try
            {
                if (LocalNo == "00") return "00";

                return LocalNo.Substring(1).PadLeft(2, '0');
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
                
                return "00";
            }
        }

        private void InitialCombox(ComboBox cbo,string defaultNodeNo)
        {
            try
            {
                List<comboxInfo> _lstLocalNo = new List<comboxInfo>();

                _lstLocalNo.Add(new comboxInfo { ITEM_ID = "00", ITEM_NAME = "" });

                foreach (Node _node in FormMainMDI.G_OPIAp.Dic_Node.Values.Where(r=>r.NodeAttribute.Equals("IN")))
                {
                    _lstLocalNo.Add(new comboxInfo { ITEM_ID = _node.NodeNo, ITEM_NAME = _node.NodeNo + " : " + _node.NodeID });
                }

                cbo.DataSource = _lstLocalNo.ToList();
                cbo.DisplayMember = "ITEM_NAME";
                cbo.ValueMember = "ITEM_ID";
                cbo.SelectedIndex = -1;

                if (defaultNodeNo != "00")
                {
                    if (FormMainMDI.G_OPIAp.Dic_Node.ContainsKey(defaultNodeNo))
                    {
                        cbo.SelectedValue = defaultNodeNo;
                    }
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
