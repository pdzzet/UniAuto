using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace UniOPI
{
    public partial class ucFlowPriority : UserControl
    {
        public int Priority { get; set; }

        public string CurrentLocalNo { get; set; }

        Dictionary<string, Node> Dic_Node;

        public ucFlowPriority()
        {
            InitializeComponent();
        }

        public ucFlowPriority(int priority, Dictionary<string, Node> dicNode)
        {
            InitializeComponent();

            Priority = priority;
            Dic_Node = dicNode;

            lblFlowPriority.Text = string.Format("{0}'st Priority of Local No [ 00 ]",priority.ToString());

            if (Dic_Node != null) InitialCombox();

            CurrentLocalNo = "00";
        }

        private void InitialCombox()
        {
            try
            {
                List<comboxInfo> _lstLocalNo = new List<comboxInfo>();

                _lstLocalNo.Add(new comboxInfo { ITEM_ID = "00", ITEM_NAME = "" });

                foreach (Node _node in Dic_Node.Values.Where(r => r.NodeAttribute.Equals("IN")))
                {
                    _lstLocalNo.Add(new comboxInfo { ITEM_ID = _node.NodeNo, ITEM_NAME = _node.NodeNo + " : " + _node.NodeID });
                }

                cboFlowPriority.DataSource = _lstLocalNo.ToList();
                cboFlowPriority.DisplayMember = "ITEM_NAME";
                cboFlowPriority.ValueMember = "ITEM_ID";
                cboFlowPriority.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public void SetDefalutLocalNo(string defaultLocalNo)
        {
            try 
            {
                CurrentLocalNo = defaultLocalNo;

                string _localNo = "L" + (int.Parse(defaultLocalNo)).ToString();

                if (defaultLocalNo != "00")
                {
                    if (Dic_Node.ContainsKey(_localNo))
                    {
                        lblFlowPriority.Text = string.Format("{0}'st Priority of Local No [ {1} ]", Priority.ToString(), _localNo);
                    }
                }
                else
                {
                    lblFlowPriority.Text = string.Format("{0}'st Priority of Local No [ 00 ]", Priority.ToString());
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public string GetChooseLocalNo()
        {
            if (cboFlowPriority.SelectedValue == null) return "00";
            else
            {
                string _no = cboFlowPriority.SelectedValue.ToString();

                if (_no == "00") return "00";

                return _no.Substring(1).PadLeft(2, '0');
            }
        }

        public void ResetCombobox()
        {
            cboFlowPriority.SelectedIndex = -1;
        }
    }
}
