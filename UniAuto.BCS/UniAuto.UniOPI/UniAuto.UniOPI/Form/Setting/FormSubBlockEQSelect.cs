using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;


namespace UniOPI
{
    public partial class FormSubBlockEQSelect : FormBase
    {
        private bool isShowUnit = true;
        private bool isShowPort = false;

        #region Property
        public string SelectedValue { get; set; }
        public string ReturnValue { get; set; }
        public bool MultipleSelect { get; set; }

        //是否顯示Unit
        public bool ShowUnit
        {
            get { return isShowUnit; }
            set { isShowUnit = value; }
        }

        public bool ShowPort
        {
            get { return isShowPort; }
            set { isShowPort = value; }
        }
        #endregion

        public FormSubBlockEQSelect()
        {
            InitializeComponent();
            MultipleSelect = false;
        }

        #region Events
        private void FormSubBlockEQSelect_Load(object sender, EventArgs e)
        {
            List<string> lstChecked = SelectedValue.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            this.InitialCheckListBox(lstChecked);
        }

        private void chklstEqpNo_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            try
            {
                CheckedListBox clb = (CheckedListBox)sender;

                if (MultipleSelect == false)
                {
                    clb.ItemCheck -= chklstEqpNo_ItemCheck;
                    if (e.NewValue == CheckState.Checked && clb.CheckedItems.Count > 0)
                        clb.SetItemChecked(clb.CheckedIndices[0], false);
                    clb.SetItemCheckState(e.Index, e.NewValue);
                    clb.ItemCheck += chklstEqpNo_ItemCheck;
                }
                else
                {
                    clb.ItemCheck -= chklstEqpNo_ItemCheck;
                    clb.SetItemCheckState(e.Index, e.NewValue);
                    clb.ItemCheck += chklstEqpNo_ItemCheck;
                }

                string[] items = clb.CheckedItems.Cast<string>().ToArray();
                string[] parse = null;
                List<string> lstSelEQ = new List<string>();
                int unitNo = 0;
                string portNo = string.Empty;

                foreach (string item in items)
                {
                    parse = item.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parse[1].StartsWith("UNIT"))
                    {
                        unitNo = Convert.ToInt16(parse[1].Substring(5, 2));
                        lstSelEQ.Add(string.Format("{0}:{1}", parse[0], unitNo));
                    }
                    else if (parse[1].StartsWith("PORT"))
                    {
                        //unitNo = Convert.ToInt16(parse[1].Substring(5, 2));
                        portNo = parse[1].Substring(5, 2);
                        lstSelEQ.Add(string.Format("{0}:P{1}", parse[0], portNo));
                    }
                    else lstSelEQ.Add(parse[0]);
                }

                ReturnValue = string.Join(";", lstSelEQ.ToArray());
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        #endregion

        #region Private Methods
        private void InitialCheckListBox(List<string> lstChecked)
        {
            try
            {
                chklstEqpNo.Items.Clear();
                int unitNo = 0;
                string _key = string.Empty;

                if (lstChecked.Count == 0)
                {
                    #region chklstEqpNo
                    foreach (Node n in FormMainMDI.G_OPIAp.Dic_Node.Values)
                    {
                        _key = string.Format("{0}-{1}", n.NodeNo, n.NodeID);
                        chklstEqpNo.Items.Add(_key);

                        //chklstEqpNo.Items.Add(string.Format("{0}-{1}-{2}", n.NODENO, n.NODEID, n.NODENAME));

                        if (isShowUnit)
                        {
                            var unit = (from u in FormMainMDI.G_OPIAp.Dic_Unit.Values
                                        where u.NodeNo == n.NodeNo
                                        select u);

                            foreach (Unit u in unit)
                            {
                                int.TryParse(u.UnitNo, out unitNo);
                                chklstEqpNo.Items.Add(string.Format("{0}-UNIT[{1}]", n.NodeNo, unitNo.ToString("00")));
                            }
                        }

                        if (isShowPort)
                        {
                            var port = (from p in FormMainMDI.G_OPIAp.Dic_Port.Values
                                        where p.NodeNo == n.NodeNo
                                        select p);

                            foreach (Port p in port)
                            {
                                //int.TryParse(p.PORTNO, out portNo);
                                chklstEqpNo.Items.Add(string.Format("{0}-PORT[{1}]", n.NodeNo, p.PortNo.PadLeft(2, '0')));
                            }
                        }
                    }
                    #endregion
                }
                else
                {
                    #region chklstEqpNo
                    string itemName = string.Empty;
                    foreach (Node n in FormMainMDI.G_OPIAp.Dic_Node.Values)
                    {
                        itemName = string.Format("{0}-{1}", n.NodeNo, n.NodeID);

                        if (lstChecked.Contains(n.NodeNo))
                            chklstEqpNo.Items.Add(itemName, true);
                        else
                            chklstEqpNo.Items.Add(itemName);

                        if (isShowUnit)
                        {
                            var unit = (from u in FormMainMDI.G_OPIAp.Dic_Unit.Values
                                        where u.NodeNo == n.NodeNo
                                        select u);
                            foreach (Unit u in unit)
                            {
                                int.TryParse(u.UnitNo, out unitNo);
                                itemName = string.Format("{0}-UNIT[{1}]", n.NodeNo, unitNo.ToString("00"));

                                if (lstChecked.Contains(string.Format("{0}:{1}", u.NodeNo, u.UnitNo)))
                                    chklstEqpNo.Items.Add(itemName, true);
                                else
                                    chklstEqpNo.Items.Add(itemName);
                            }
                        }

                        if (isShowPort)
                        {
                            var port = (from p in FormMainMDI.G_OPIAp.Dic_Port.Values
                                        where p.NodeNo == n.NodeNo
                                        select p);

                            foreach (Port p in port)
                            {
                                itemName = string.Format("{0}-PORT[{1}]", p.NodeNo, p.PortNo.PadLeft(2, '0'));

                                if (lstChecked.Contains(string.Format("{0}:{1}", p.NodeNo, p.PortNo)))
                                    chklstEqpNo.Items.Add(itemName, true);
                                else
                                    chklstEqpNo.Items.Add(itemName);
                            }
                        }
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }
        #endregion
    }
}
