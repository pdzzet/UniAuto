using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;

namespace UniAuto.UniBCS.EntityManager.UI
{
    public partial class FrmObjectEdit : Form
    {
        private object _selectObject;

        private string _objectName;

        public string ObjectName
        {
            get { return _objectName; }
            set { _objectName = value; }
        }
        public object SelectObject
        {
            get { return _selectObject; }
            set { _selectObject = value; }
        }

        public FrmObjectEdit()
        {
            InitializeComponent();
        }

        private void FrmObjectEdit_Load(object sender, EventArgs e)
        {
            if (SelectObject != null)
                this.propertyGrid1.SelectedObject = SelectObject;
        }

        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            Log.NLogManager.Logger.LogWarnWrite("Service", this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("Modify Object [{0}] item [{1}] Vlaue [{2}] OldValue [{3}].", ObjectName, e.ChangedItem.Label, e.ChangedItem.Value.ToString(), e.OldValue.ToString()));
        }


    }
}
