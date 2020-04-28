using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace UniAuto.UniRCS.RobotService
{
    public partial class FrmRobot : Form
    {
        public class DLLInfo
        {
            public bool Exists { get; set; }
            public string Filename { get; set; }
            public string Version { get; set; }
        }

        /// <summary>
        /// RobotCoreService, 由Spring讀Config並給值
        /// </summary>
        public RobotCoreService RobotTestService { get; set; }

        public FrmRobot()
        {
            InitializeComponent();
        }

        public void Init()
        {
        }

        private void _btnRead_Click(object sender, EventArgs e)
        {
            DLLInfo dll_info = RobotTestService.ReadDLLInfo();
            RefreshListView(dll_info, _lstFolderDLLInfo);
        }

        private void _btnConfirm_Click(object sender, EventArgs e)
        {

        }

        private void RefreshListView(DLLInfo DLLInfo, ListView ListView)
        {
            ListView.Items.Clear();
            if (DLLInfo != null)
            {
                PropertyInfo[] property_infos = typeof(DLLInfo).GetProperties();
                foreach (PropertyInfo prop in property_infos)
                {
                    ListViewItem item = ListView.Items.Add(prop.Name);
                    object value = prop.GetValue(DLLInfo, null);
                    if (value != null)
                        item.SubItems.Add(value.ToString());
                }
            }
        }
    }
}
