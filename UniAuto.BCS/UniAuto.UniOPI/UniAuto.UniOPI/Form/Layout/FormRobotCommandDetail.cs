using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace UniOPI
{
    public partial class FormRobotCommandDetail : FormBase
    {
        public FormRobotCommandDetail(string type,string txt)
        {
            InitializeComponent();

            richTxt.Text = txt;

            if (type.ToUpper() == "ERROR") richTxt.ForeColor = Color.Blue;
            else if (type.ToUpper() == "WARN") richTxt.ForeColor = Color.Black;
        }
    }
}
